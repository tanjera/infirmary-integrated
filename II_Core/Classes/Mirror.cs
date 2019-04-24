using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;

using MySql.Data.MySqlClient;

namespace II.Server {
    public class Mirroring {
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

        public void Timers_Process (object sender, EventArgs e) {
            timerUpdate.Process ();
        }

        public void Timer_GetPatient (Patient p, Connection connection) {
            timerUpdate.Reset (5000);
            GetPatient (p, connection);
        }

        public void GetPatient (Patient p, Connection connection) {
            // Mirroring not active; neither client or host
            if (Status != Statuses.CLIENT)
                return;

            // Mirroring as client, check server q RefreshSeconds
            if (DateTime.Compare (ServerQueried, DateTime.UtcNow.Subtract (new TimeSpan (0, 0, RefreshSeconds))) < 0) {
                BackgroundWorker bgw = new BackgroundWorker ();
                bgw.DoWork += delegate { connection.Mirror_GetPatient (this, p); };
                bgw.RunWorkerCompleted += delegate { p.UnlockThread (); };
                if (!p.ThreadLock) {
                    p.ThreadLock = true;
                    bgw.RunWorkerAsync ();
                }
            }
        }

        public void PostPatient (Patient p, Connection connection) {
            if (Status != Statuses.HOST)
                return;

            BackgroundWorker bgw = new BackgroundWorker ();
            bgw.DoWork += delegate { connection.Mirror_PostPatient (this, p); };
            bgw.RunWorkerCompleted += delegate { p.ThreadLock = false; };
            if (!p.ThreadLock) {
                p.ThreadLock = true;
                bgw.RunWorkerAsync ();
            }
        }
    }
}
