/* Scenario.cs
 * Infirmary Integrated
 * By Ibi Keller (Tanjera), (c) 2017
 *
 * Management of scenario takes place here; iterating steps.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace II {

    public class Scenario {
        public string Name, Description, Author;
        public DateTime Updated;
        public int CurrentIndex = 0;
        public List<Step> Steps = new List<Step> ();
        public Timer ProgressTimer = new Timer ();

        public event EventHandler<EventArgs> StepChangeRequest;

        public event EventHandler<EventArgs> StepChanged;

        public Scenario (bool toInit) {
            if (toInit)
                Steps.Add (new Step ());

            ProgressTimer.Tick += ProgressTimer_Tick;
        }

        ~Scenario () => Dispose ();

        public void Dispose () {
            UnsubscribeEvents ();

            foreach (Step s in Steps)
                s.Patient.Dispose ();

            ProgressTimer.Dispose ();
        }

        public void UnsubscribeEvents () {
            if (StepChangeRequest != null) {
                foreach (Delegate d in StepChangeRequest?.GetInvocationList ())
                    StepChangeRequest -= (EventHandler<EventArgs>)d;
            }

            if (StepChanged != null) {
                foreach (Delegate d in StepChanged?.GetInvocationList ())
                    StepChanged -= (EventHandler<EventArgs>)d;
            }
        }

        public Step Current {
            get { return Steps [CurrentIndex]; }
        }

        public bool IsScenario {    // If there's only one Step- it's a regular Patient parameter
            get { return Steps.Count != 1; }
        }

        public void Reset () {
            Steps.Clear ();
            CurrentIndex = 0;

            Name = "";
            Description = "";
            Author = "";
            Updated = DateTime.Now;
        }

        public Patient Patient {
            get { return Steps [CurrentIndex].Patient; }
            set { Steps [CurrentIndex].Patient = value; }
        }

        public void Load_Process (string inc) {
            StringReader sRead = new StringReader (inc);
            string line, pline;
            StringBuilder pbuffer;

            this.Reset ();

            try {
                while ((line = sRead.ReadLine ()) != null) {
                    if (line == "> Begin: Step") {
                        pbuffer = new StringBuilder ();

                        while ((pline = sRead.ReadLine ()) != null && pline != "> End: Step")
                            pbuffer.AppendLine (pline);

                        Step s = new Step ();
                        s.Load_Process (pbuffer.ToString ());
                        Steps.Add (s);
                    } else if (line.Contains (":")) {
                        string pName = line.Substring (0, line.IndexOf (':')),
                                pValue = line.Substring (line.IndexOf (':') + 1).Trim ();

                        switch (pName) {
                            default: break;
                            case "Updated": Updated = Utility.DateTime_FromString (pValue); break;
                            case "Name": Name = pValue; break;
                            case "Description": Description = pValue; break;
                            case "Author": Author = pValue; break;
                        }
                    }
                }
            } catch {

                // If the load fails... just bail on the actual value parsing and continue the load process
            }

            SetStep (0);
            sRead.Close ();
        }

        public string Save () {
            StringBuilder sWrite = new StringBuilder ();

            sWrite.AppendLine (String.Format ("{0}:{1}", "Updated", Utility.DateTime_ToString (Updated)));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Name", Name));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Description", Description));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Author", Author));

            for (int i = 0; i < Steps.Count; i++) {
                sWrite.AppendLine ("> Begin: Step");
                sWrite.Append (Steps [i].Save ());
                sWrite.AppendLine ("> End: Step");
            }

            return sWrite.ToString ();
        }

        public void NextStep (int optProg = -1) {
            StepChangeRequest?.Invoke (this, new EventArgs ());

            int pFrom = CurrentIndex;

            if (optProg < 0 || optProg >= Current.Progressions.Count)       // Default Progression
                CurrentIndex = Current.ProgressTo >= 0
                    ? Current.ProgressTo
                    : System.Math.Min (CurrentIndex + 1, Steps.Count - 1);
            else                                                            // Optional Progression
                CurrentIndex = Current.Progressions [optProg].DestinationIndex;

            if (pFrom != CurrentIndex) {                                    // If the actual step Index changed
                Current.ProgressFrom = pFrom;
                CopyDeviceStatus (Steps [pFrom].Patient, Current.Patient);
                Steps [pFrom].Patient.Deactivate ();                        // Additional unlinking of events and timers!
            }

            // Init step regardless of whether step Index changed; step may have been deactivated by StepChangeRequest()
            SetTimer ();
            Current.Patient.Activate ();

            // Trigger events for loading current Patient, and trigger propagation to devices
            StepChanged?.Invoke (this, new EventArgs ());
            Current.Patient.OnPatientEvent (Patient.PatientEventTypes.Vitals_Change);
        }

        public void LastStep () {
            StepChangeRequest?.Invoke (this, new EventArgs ());

            int pFrom = CurrentIndex;
            CurrentIndex = Current.ProgressFrom;

            if (pFrom != CurrentIndex) {                                    // If the actual step Index changed
                CopyDeviceStatus (Steps [pFrom].Patient, Current.Patient);
                Steps [pFrom].Patient.Deactivate ();                        // Additional unlinking of events and timers!
            }

            // Init step regardless of whether step Index changed; step may have been deactivated by StepChangeRequest()
            SetTimer ();
            Current.Patient.Activate ();

            // Trigger events for loading current Patient, and trigger propagation to devices
            StepChanged?.Invoke (this, new EventArgs ());
            Current.Patient.OnPatientEvent (Patient.PatientEventTypes.Vitals_Change);
        }

        public void SetStep (int incIndex) {
            StepChangeRequest?.Invoke (this, new EventArgs ());

            int pFrom = CurrentIndex;
            CurrentIndex = II.Math.Clamp (incIndex, 0, Steps.Count - 1);

            if (pFrom != CurrentIndex) {                                    // If the actual step Index changed
                CopyDeviceStatus (Steps [pFrom].Patient, Current.Patient);
                Steps [pFrom].Patient.Deactivate ();                        // Additional unlinking of events and timers!
            }

            // Init step regardless of whether step Index changed; step may have been deactivated by StepChangeRequest()
            SetTimer ();
            Current.Patient.Activate ();

            // Trigger events for loading current Patient, and trigger propagation to devices
            StepChanged?.Invoke (this, new EventArgs ());
            Current.Patient.OnPatientEvent (Patient.PatientEventTypes.Vitals_Change);
        }

        public void CopyDeviceStatus (Patient lastPatient, Patient thisPatient) {
            thisPatient.TransducerZeroed_CVP = lastPatient.TransducerZeroed_CVP;
            thisPatient.TransducerZeroed_ABP = lastPatient.TransducerZeroed_ABP;
            thisPatient.TransducerZeroed_PA = lastPatient.TransducerZeroed_PA;
            thisPatient.TransducerZeroed_ICP = lastPatient.TransducerZeroed_ICP;
            thisPatient.TransducerZeroed_IAP = lastPatient.TransducerZeroed_IAP;
        }

        public void PauseStep () => ProgressTimer.Stop ();

        public void PlayStep () => ProgressTimer.Start ();

        public void SetTimer () {
            if (Current.ProgressTimer > 0)
                ProgressTimer.ResetAuto (Current.ProgressTimer * 1000);
            else
                ProgressTimer.Stop ();
        }

        public void StopTimer () => ProgressTimer.Stop ();

        public void ResumeTimer () => ProgressTimer.Start ();

        public void ProcessTimer (object sender, EventArgs e)
            => ProgressTimer.Process ();

        private void ProgressTimer_Tick (object sender, EventArgs e)
            => NextStep ();

        public class Step {
            public Patient Patient;
            public string Name, Description;

            public List<Progression> Progressions = new List<Progression> ();
            public int ProgressTo = -1;
            public int ProgressFrom = -1;
            public int ProgressTimer = -1;

            // Metadata: for drawing interface items in Scenario Editor
            public double IPositionX, IPositionY;

            public Step () {
                Patient = new Patient ();
            }

            public void Load_Process (string inc) {
                StringReader sRead = new StringReader (inc);
                string line, pline;
                StringBuilder pbuffer;

                try {
                    while ((line = sRead.ReadLine ()) != null) {
                        if (line == "> Begin: Patient") {
                            pbuffer = new StringBuilder ();

                            while ((pline = sRead.ReadLine ()) != null && pline != "> End: Patient")
                                pbuffer.AppendLine (pline);

                            Patient.Load_Process (pbuffer.ToString ());
                        } else if (line == "> Begin: Progression") {
                            pbuffer = new StringBuilder ();

                            while ((pline = sRead.ReadLine ()) != null && pline != "> End: Progression")
                                pbuffer.AppendLine (pline);

                            Progression p = new Progression ();
                            p.Load_Process (pbuffer.ToString ());
                            Progressions.Add (p);
                        } else if (line.Contains (":")) {
                            string pName = line.Substring (0, line.IndexOf (':')),
                                    pValue = line.Substring (line.IndexOf (':') + 1).Trim ();

                            switch (pName) {
                                default: break;
                                case "Name": Name = pValue; break;
                                case "Description": Description = pValue; break;
                                case "ProgressTo": ProgressTo = int.Parse (pValue); break;
                                case "ProgressFrom": ProgressFrom = int.Parse (pValue); break;
                                case "ProgressTime": ProgressTimer = int.Parse (pValue); break;
                                case "IPositionX": IPositionX = double.Parse (pValue); break;
                                case "IPositionY": IPositionY = double.Parse (pValue); break;
                            }
                        }
                    }
                } catch {
                    /* If the load fails... just bail on the actual value parsing and continue the load process */
                }

                sRead.Close ();
            }

            public string Save () {
                StringBuilder sWrite = new StringBuilder ();

                sWrite.AppendLine (String.Format ("{0}:{1}", "Name", Name));
                sWrite.AppendLine (String.Format ("{0}:{1}", "Description", Description));
                sWrite.AppendLine (String.Format ("{0}:{1}", "ProgressTo", ProgressTo));
                sWrite.AppendLine (String.Format ("{0}:{1}", "ProgressFrom", ProgressFrom));
                sWrite.AppendLine (String.Format ("{0}:{1}", "ProgressTime", ProgressTimer));
                sWrite.AppendLine (String.Format ("{0}:{1}", "IPositionX", System.Math.Round (IPositionX, 2)));
                sWrite.AppendLine (String.Format ("{0}:{1}", "IPositionY", System.Math.Round (IPositionY, 2)));

                sWrite.AppendLine ("> Begin: Patient");
                sWrite.Append (Patient.Save ());
                sWrite.AppendLine ("> End: Patient");

                foreach (Progression p in Progressions) {
                    sWrite.AppendLine ("> Begin: Progression");
                    sWrite.Append (p.Save ());
                    sWrite.AppendLine ("> End: Progression");
                }

                return sWrite.ToString ();
            }

            /* Possible progressions/routes to the next step of the scenario */

            public class Progression {
                public int DestinationIndex;
                public string Description;

                public Progression () {
                }

                public Progression (int dest, string desc = "") {
                    DestinationIndex = dest;
                    Description = desc;
                }

                public void Load_Process (string inc) {
                    StringReader sRead = new StringReader (inc);
                    string line;

                    try {
                        while ((line = sRead.ReadLine ()) != null) {
                            if (line.Contains (":")) {
                                string pName = line.Substring (0, line.IndexOf (':')),
                                        pValue = line.Substring (line.IndexOf (':') + 1).Trim ();

                                switch (pName) {
                                    default: break;
                                    case "Description": Description = pValue; break;
                                    case "DestinationIndex": DestinationIndex = int.Parse (pValue); break;
                                }
                            }
                        }
                    } catch {
                        /* If the load fails... just bail on the actual value parsing and continue the load process */
                    }

                    sRead.Close ();
                }

                public string Save () {
                    StringBuilder sWrite = new StringBuilder ();

                    sWrite.AppendLine (String.Format ("{0}:{1}", "Description", Description));
                    sWrite.AppendLine (String.Format ("{0}:{1}", "DestinationIndex", DestinationIndex));

                    return sWrite.ToString ();
                }
            }
        }
    }
}