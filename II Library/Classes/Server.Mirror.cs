using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace II.Server {

    public class Mirror {
        private bool ThreadLock = false;
        public Timer timerUpdate = new();

        public enum Statuses { INACTIVE, HOST, CLIENT };

        private int RefreshSeconds = 5;
        private string _Accession = "";
        private BackgroundWorker _BackgroundWorker = new();

        public Statuses Status = Statuses.INACTIVE;

        public string PasswordAccess = "",
                        PasswordEdit = "";

        public DateTime PatientUpdated, ServerQueried;

        public string Accession {
            get { return _Accession.ToUpper (); }
            set { _Accession = value.ToUpper (); }
        }

        public Mirror () {
            ResetBackgroundWorker ();
        }

        private void ResetBackgroundWorker () {
            _BackgroundWorker = new BackgroundWorker ();
            _BackgroundWorker.WorkerSupportsCancellation = true;
        }

        public void ProcessTimer (object? sender, EventArgs e) {
            timerUpdate.Process ();
        }

        public void TimerTick (Patient? p, Server s) {
            _ = timerUpdate.ResetAuto (5000);
            _ = GetPatient (p, s);
        }

        public void CancelOperation () {
            try {
                _BackgroundWorker.CancelAsync ();
            } catch {
            } finally {
                ThreadLock = false;
                ResetBackgroundWorker ();
            }
        }

        public async Task GetPatient (Patient? p, Server s) {
            /* Mirroring not active; neither client or host */
            if (Status != Statuses.CLIENT)
                return;

            /* Mirroring as client, check server q RefreshSeconds */
            if (DateTime.Compare (ServerQueried, DateTime.UtcNow.Subtract (new TimeSpan (0, 0, RefreshSeconds))) < 0) {
                // Using a thread lock to prevent multiple web calls from generating race conditions against each other
                if (!ThreadLock) {
                    ThreadLock = true;
                    Patient? pBuffer = await Server.Get_PatientMirror (this);
                    ThreadLock = false;

                    if (pBuffer != null) {
                        if (p == null)
                            p = new ();

                        await p.Load_Process (pBuffer.Save ());
                    }
                }
            }
        }

        public async Task PostPatient (Patient? p, Server s) {
            if (Status != Statuses.HOST)
                return;

            // Must use intermediary objects, if App.Patient is thread-locked, Waveforms stop populating!!
            string pStr = p.Save ();
            DateTime pUp = p.Updated;

            Regex regex = new("^[a-zA-Z0-9]*$");
            if (Accession.Length <= 0 || !regex.IsMatch (Accession))
                return;

            await Server.Post_PatientMirror (this, pStr, pUp);
        }
    }
}