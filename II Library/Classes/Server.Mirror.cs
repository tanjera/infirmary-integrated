/* Sever.Mirror.cs
 * Infirmary Integrated
 * By Ibi Keller (Tanjera), (c) 2023
 */

using System;
using System.ComponentModel;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using II.Settings;

namespace II.Server {
    public class Mirror {
        private bool ThreadLock = false;
        public Timer? TimerSimulation;
        public Timer TimerUpdate = new ();

        public enum Statuses { INACTIVE, HOST, CLIENT };

        private int RefreshSeconds = 5;
        private string _Accession = "";
        private BackgroundWorker _BackgroundWorker = new ();

        public Statuses Status = Statuses.INACTIVE;

        public static string DefaultServer = "http://server.infirmary-integrated.com/";
        
        public string ServerAddress = DefaultServer,
            PasswordAccess = "",
            PasswordEdit = "";

        public DateTime PatientUpdated, ServerQueried;

        public string Accession {
            get { return _Accession.ToUpper (); }
            set { _Accession = value.ToUpper (); }
        }

        
        public Mirror (Timer? timerSimulation) {
            TimerSimulation = timerSimulation;
            ResetBackgroundWorker ();
        }

        private void ResetBackgroundWorker () {
            _BackgroundWorker = new BackgroundWorker ();
            _BackgroundWorker.WorkerSupportsCancellation = true;
        }

        public void ProcessTimer (object? sender, EventArgs e) {
            TimerUpdate.Process ();
        }

        public void TimerTick (Scenario.Step? step, Server s) {
            _ = TimerUpdate.ResetStart (5000);
            _ = GetStep (step, s);
        }

        public void CancelOperation () {
            try {
                _BackgroundWorker.CancelAsync ();
            } finally {
                ThreadLock = false;
                ResetBackgroundWorker ();
            }
        }
        
        public async Task<Server.ServerResponse> GetStep (Scenario.Step? step, Server s) {
            /* Mirroring not active; neither client or host */
            if (Status != Statuses.CLIENT)
                return Server.ServerResponse.NA;

            /* Mirroring as client, check server q RefreshSeconds */
            if (DateTime.Compare (ServerQueried, DateTime.UtcNow.Subtract (new TimeSpan (0, 0, RefreshSeconds))) < 0) {
                // Using a thread lock to prevent multiple web calls from generating race conditions against each other
                if (!ThreadLock) {
                    ThreadLock = true;
                    (Server.ServerResponse resp, Scenario.Step? pBuffer) = await s.Get_StepMirror (this);
                    ThreadLock = false;

                    if (pBuffer != null) {
                        step ??= new (TimerSimulation);

                        await step.Load (pBuffer.Save ());
                    }
                    
                    return resp;
                }
            } 

            return Server.ServerResponse.NA;
        }

        public async Task<Server.ServerResponse> PostStep (Scenario.Step? step, Server s) {
            if (Status != Statuses.HOST)
                return Server.ServerResponse.NA;

            // Must use intermediary objects, if App.Patient is thread-locked, Waveforms stop populating!!
            string? pStr = step?.Save ();
            DateTime? pUp = step?.Physiology?.Updated;

            Regex regex = new ("^[a-zA-Z0-9]*$");
            if (Accession.Length <= 0 || !regex.IsMatch (Accession))
                return Server.ServerResponse.ErrorCredentials;

            Server.ServerResponse resp = await Server.Post_StepMirror (this, pStr, pUp);

            return resp;
        }
    }
}