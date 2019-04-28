using System;
using System.ComponentModel;

namespace II.Server {
    public class Mirrors {
        private bool ThreadLock = false;
        public Timer timerUpdate = new Timer ();

        public enum Statuses { INACTIVE, HOST, CLIENT };

        private int RefreshSeconds = 5;
        private string _Accession = "";

        public Statuses Status = Statuses.INACTIVE;
        public string PasswordAccess = "",
                        PasswordEdit = "";

        public DateTime PatientUpdated, ServerQueried;

        public string Accession {
            get { return _Accession.ToUpper (); }
            set { _Accession = value.ToUpper (); }
        }

        public void TimerProcess (object sender, EventArgs e) {
            timerUpdate.Process ();
        }

        public void TimerTick (Patient p, Servers s) {
            timerUpdate.Reset (5000);
            GetPatient (p, s);
        }

        public void GetPatient (Patient p, Servers s) {
            // Mirroring not active; neither client or host
            if (Status != Statuses.CLIENT)
                return;

            // Mirroring as client, check server q RefreshSeconds
            if (DateTime.Compare (ServerQueried, DateTime.UtcNow.Subtract (new TimeSpan (0, 0, RefreshSeconds))) < 0) {
                // Must use intermediary Patient(), if App.Patient is thread-locked, Waveforms stop populating!!
                Patient pBuffer = new Patient ();
                BackgroundWorker bgw = new BackgroundWorker ();

                bgw.DoWork += delegate {
                    pBuffer = s.Get_PatientMirror (this);
                };
                bgw.RunWorkerCompleted += delegate {
                    ThreadLock = false;
                    if (pBuffer != null)
                        p.Load_Process (pBuffer.Save ());
                };
                if (!ThreadLock) {
                    ThreadLock = true;
                    bgw.RunWorkerAsync ();
                }
            }
        }

        public void PostPatient (Patient p, Servers s) {
            if (Status != Statuses.HOST)
                return;

            // Must use intermediary objects, if App.Patient is thread-locked, Waveforms stop populating!!
            string pStr = p.Save ();
            DateTime pUp = p.Updated;
            BackgroundWorker bgw = new BackgroundWorker ();

            if (Accession == "")
                Accession = Utility.RandomString (8);

            bgw.DoWork += delegate { s.Post_PatientMirror (this, pStr, pUp); };
            bgw.RunWorkerCompleted += delegate { ThreadLock = false; };
            if (!ThreadLock) {
                ThreadLock = true;
                bgw.RunWorkerAsync ();
            }
        }
    }
}