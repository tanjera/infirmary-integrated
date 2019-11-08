using System;
using System.ComponentModel;

namespace II.Server {
    public class Mirror {
        private bool ThreadLock = false;
        public Timer timerUpdate = new Timer ();

        public enum Statuses { INACTIVE, HOST, CLIENT };

        private int RefreshSeconds = 5;
        private string _Accession = "";
        private BackgroundWorker _BackgroundWorker = new BackgroundWorker ();

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

        public void ProcessTimer (object sender, EventArgs e) {
            timerUpdate.Process ();
        }

        public void TimerTick (Patient p, Server s) {
            timerUpdate.ResetAuto (5000);
            GetPatient (p, s);
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

        public void GetPatient (Patient p, Server s) {
            /* Mirroring not active; neither client or host */
            if (Status != Statuses.CLIENT)
                return;

            /* Mirroring as client, check server q RefreshSeconds */
            if (DateTime.Compare (ServerQueried, DateTime.UtcNow.Subtract (new TimeSpan (0, 0, RefreshSeconds))) < 0) {

                // Must use intermediary Patient(), if App.Patient is thread-locked, Waveforms stop populating!!
                Patient pBuffer = new Patient ();

                _BackgroundWorker.DoWork += delegate {
                    pBuffer = s.Get_PatientMirror (this);
                };
                _BackgroundWorker.RunWorkerCompleted += delegate {
                    ThreadLock = false;
                    ResetBackgroundWorker ();

                    if (pBuffer != null)
                        p.Load_Process (pBuffer.Save ());
                };
                if (!ThreadLock) {
                    ThreadLock = true;
                    _BackgroundWorker.RunWorkerAsync ();
                }
            }
        }

        public void PostPatient (Patient p, Server s) {
            if (Status != Statuses.HOST)
                return;

            // Must use intermediary objects, if App.Patient is thread-locked, Waveforms stop populating!!
            string pStr = p.Save ();
            DateTime pUp = p.Updated;

            if (Accession == "")
                Accession = Utility.RandomString (8);

            _BackgroundWorker.DoWork += delegate { s.Post_PatientMirror (this, pStr, pUp); };
            _BackgroundWorker.RunWorkerCompleted += delegate {
                ThreadLock = false;
                ResetBackgroundWorker ();
            };

            if (!ThreadLock) {
                ThreadLock = true;
                _BackgroundWorker.RunWorkerAsync ();
            }
        }
    }
}