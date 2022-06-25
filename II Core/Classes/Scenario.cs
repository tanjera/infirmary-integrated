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
using System.Threading.Tasks;

namespace II {

    public class Scenario {
        public string Name, Description, Author;
        public DateTime Updated;

        public string? BeginStep = null;
        public string? AtStep = null;

        public List<Step> Steps = new List<Step> ();
        public Timer ProgressTimer = new Timer ();

        public event EventHandler<EventArgs> StepChangeRequest;

        public event EventHandler<EventArgs> StepChanged;

        public Scenario (bool toInit = false) {
            if (toInit) {
                Step s = new ();

                BeginStep = s.UUID;
                AtStep = s.UUID;

                Steps.Add (s);
            }

            ProgressTimer.Tick += ProgressTimer_Tick;
        }

        ~Scenario () => _ = Dispose ();

        public async Task Dispose () {
            await UnsubscribeEvents ();

            foreach (Step s in Steps)
                await s.Patient.Dispose ();

            ProgressTimer.Dispose ();
        }

        public async Task UnsubscribeEvents () {
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
            get { return Steps.Find (s => s.UUID == AtStep); }
        }

        public bool IsScenario {    // If there's only one Step- it's a regular Patient parameter
            get { return Steps.Count != 1; }
        }

        public void Reset () {
            Steps.Clear ();
            BeginStep = null;
            AtStep = null;

            Name = "";
            Description = "";
            Author = "";
            Updated = DateTime.UtcNow;
        }

        public Patient Patient {
            get { return Current?.Patient; }
            set { if (Current != null) Current.Patient = value; }
        }

        public void Load_Process (string inc) {
            StringReader sRead = new (inc);
            string? line, pline;
            StringBuilder pbuffer;

            this.Reset ();

            try {
                while (!String.IsNullOrEmpty (line = sRead.ReadLine ())) {
                    line = line.Trim ();
                    if (line == "> Begin: Step") {
                        pbuffer = new StringBuilder ();

                        while ((pline = sRead.ReadLine ().Trim ()) != null && pline != "> End: Step")
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
                            case "Beginning": BeginStep = pValue; break;
                        }
                    }
                }
            } catch {
                // If the load fails... just bail on the actual value parsing and continue the load process
            }

            _ = SetStep (BeginStep);

            sRead.Close ();
        }

        public string Save (int indent = 1) {
            string dent = Utility.Indent (indent);
            StringBuilder sWrite = new StringBuilder ();

            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Updated", Utility.DateTime_ToString (Updated)));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Name", Name));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Description", Description));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Author", Author));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Beginning", BeginStep));

            for (int i = 0; i < Steps.Count; i++) {
                sWrite.AppendLine ($"{dent}> Begin: Step");
                sWrite.Append (Steps [i].Save (indent + 1));
                sWrite.AppendLine ($"{dent}> End: Step");
            }

            return sWrite.ToString ();
        }

        public async Task NextStep (string? optProg = null) {
            StepChangeRequest?.Invoke (this, new EventArgs ());

            string? progFrom = AtStep;

            if (String.IsNullOrEmpty (optProg)) {                            // Default Progression
                AtStep = Current?.DefaultProgression?.DestinationUUID;
            } else {                                                                // Optional Progression
                AtStep = optProg;
            }

            if (progFrom != AtStep) {                                       // If the actual step Index changed
                Current.DefaultSource = progFrom;
                Step? stepFrom = Steps.Find (s => s.UUID == progFrom);

                if (stepFrom != null) {
                    CopyDeviceStatus (stepFrom.Patient, Current.Patient);
                    await stepFrom.Patient.Deactivate ();                   // Additional unlinking of events and timers!
                }
            }

            // Init step regardless of whether step Index changed; step may have been deactivated by StepChangeRequest()
            SetTimer ();
            await Current.Patient.Activate ();

            // Trigger events for loading current Patient, and trigger propagation to devices
            StepChanged?.Invoke (this, new EventArgs ());
            await Current.Patient.OnPatientEvent (Patient.PatientEventTypes.Vitals_Change);
        }

        public async Task LastStep () {
            StepChangeRequest?.Invoke (this, new EventArgs ());

            string? progFrom = AtStep;
            AtStep = Current.DefaultSource;

            if (progFrom != AtStep) {                                       // If the actual step Index changed
                Current.DefaultSource = progFrom;
                Step? stepFrom = Steps.Find (s => s.UUID == progFrom);

                if (stepFrom != null) {
                    CopyDeviceStatus (stepFrom.Patient, Current.Patient);
                    await stepFrom.Patient.Deactivate ();                   // Additional unlinking of events and timers!
                }
            }

            // Init step regardless of whether step Index changed; step may have been deactivated by StepChangeRequest()
            SetTimer ();
            await Current.Patient.Activate ();

            // Trigger events for loading current Patient, and trigger propagation to devices
            StepChanged?.Invoke (this, new EventArgs ());
            await Current.Patient.OnPatientEvent (Patient.PatientEventTypes.Vitals_Change);
        }

        public async Task SetStep (string? incUUID) {
            if (incUUID == null)
                return;

            StepChangeRequest?.Invoke (this, new EventArgs ());

            string? progFrom = AtStep;
            AtStep = incUUID;

            if (progFrom != AtStep) {                                       // If the actual step Index changed
                Current.DefaultSource = progFrom;
                Step? stepFrom = Steps.Find (s => s.UUID == (progFrom ?? ""));

                if (stepFrom != null) {
                    CopyDeviceStatus (stepFrom.Patient, Current.Patient);
                    await stepFrom.Patient.Deactivate ();                   // Additional unlinking of events and timers!
                }
            }

            SetTimer ();
            await Current.Patient.Activate ();

            // Trigger events for loading current Patient, and trigger propagation to devices
            StepChanged?.Invoke (this, new EventArgs ());
            await Current.Patient.OnPatientEvent (Patient.PatientEventTypes.Vitals_Change);
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
            => _ = NextStep ();

        public class Step {
            public string? UUID = null;
            public Patient Patient;
            public string Name, Description;

            public List<Progression> Progressions = new List<Progression> ();
            public Progression? DefaultProgression = null;
            public string? DefaultSource = null;
            public int ProgressTimer = -1;

            public Step () {
                UUID = Guid.NewGuid ().ToString ();
                Patient = new Patient ();
            }

            public void Load_Process (string inc) {
                StringReader sRead = new StringReader (inc);
                string? line, pline;
                StringBuilder pbuffer;

                try {
                    while (!String.IsNullOrEmpty (line = sRead.ReadLine ())) {
                        line = line.Trim ();
                        if (line == "> Begin: Patient") {
                            pbuffer = new StringBuilder ();

                            while ((pline = sRead.ReadLine ()) != null && pline != "> End: Patient")
                                pbuffer.AppendLine (pline);

                            _ = Patient.Load_Process (pbuffer.ToString ());
                        } else if (line == "> Begin: Progression") {
                            pbuffer = new StringBuilder ();

                            while ((pline = sRead.ReadLine ()) != null && pline != "> End: Progression")
                                pbuffer.AppendLine (pline);

                            Progression p = new ();
                            p.Load_Process (pbuffer.ToString ());
                            Progressions.Add (p);

                            if (p.UUID == DefaultProgression?.UUID)
                                DefaultProgression = p;
                        } else if (line.Contains (":")) {
                            string pName = line.Substring (0, line.IndexOf (':')),
                                    pValue = line.Substring (line.IndexOf (':') + 1).Trim ();

                            switch (pName) {
                                default: break;
                                case "UUID": UUID = pValue; break;
                                case "Name": Name = pValue; break;
                                case "Description": Description = pValue; break;
                                case "DefaultProgression": DefaultProgression = new Progression (pValue, null); break;
                                case "DefaultSource": DefaultSource = pValue; break;
                                case "ProgressTime": ProgressTimer = int.Parse (pValue); break;
                            }
                        }
                    }
                } catch {
                    /* If the load fails... just bail on the actual value parsing and continue the load process */
                }

                sRead.Close ();
            }

            public string Save (int indent = 1) {
                string dent = Utility.Indent (indent);
                StringBuilder sWrite = new StringBuilder ();

                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "UUID", UUID));
                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Name", Name));
                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Description", Description));
                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "DefaultProgression", DefaultProgression?.UUID));
                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "DefaultSource", DefaultSource));
                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "ProgressTime", ProgressTimer));

                sWrite.AppendLine ($"{dent}> Begin: Patient");
                sWrite.Append (Patient.Save (indent + 1));
                sWrite.AppendLine ($"{dent}> End: Patient");

                foreach (Progression p in Progressions) {
                    sWrite.AppendLine ($"{dent}> Begin: Progression");
                    sWrite.Append (p.Save (indent + 1));
                    sWrite.AppendLine ($"{dent}> End: Progression");
                }

                return sWrite.ToString ();
            }

            /* Possible progressions/routes to the next step of the scenario */

            public class Progression {
                public string? UUID;
                public string? DestinationUUID;
                public string? Description;

                public Progression () {
                    UUID = Guid.NewGuid ().ToString ();
                }

                public Progression (string? uuid, string? dest, string? desc = "") {
                    UUID = uuid;
                    DestinationUUID = dest;
                    Description = desc;
                }

                public void Load_Process (string inc) {
                    StringReader sRead = new StringReader (inc);
                    string? line;

                    try {
                        while (!String.IsNullOrEmpty (line = sRead.ReadLine ())) {
                            line = line.Trim ();

                            if (line.Contains (":")) {
                                string pName = line.Substring (0, line.IndexOf (':')),
                                        pValue = line.Substring (line.IndexOf (':') + 1).Trim ();

                                switch (pName) {
                                    default: break;
                                    case "UUID": UUID = pValue; break;
                                    case "Description": Description = pValue; break;
                                    case "DestinationUUID": DestinationUUID = pValue; break;
                                }
                            }
                        }
                    } catch {
                        /* If the load fails... just bail on the actual value parsing and continue the load process */
                    }

                    sRead.Close ();
                }

                public string Save (int indent = 1) {
                    string dent = Utility.Indent (indent);
                    StringBuilder sWrite = new ();

                    sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "UUID", UUID));
                    sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Description", Description));
                    sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "DestinationUUID", DestinationUUID));

                    return sWrite.ToString ();
                }
            }
        }
    }
}