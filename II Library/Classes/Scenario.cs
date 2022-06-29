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
        public string? Name, Description, Author;
        public DateTime? Updated;

        public string? BeginStep = null;
        public string? AtStep = null;

        // A holdover from IsScenario... indicates if this Scenario is from a loaded file
        public bool IsLoaded = false;

        public List<Step> Steps = new List<Step> ();
        public Timer ProgressTimer = new Timer ();

        public DeviceSettings DeviceMonitor = new ();
        public DeviceSettings DeviceDefib = new ();
        public DeviceSettings DeviceECG = new ();
        public DeviceSettings DeviceIABP = new ();

        public event EventHandler<EventArgs>? StepChangeRequest;

        public event EventHandler<EventArgs>? StepChanged;

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

        public Task UnsubscribeEvents () {
            if (StepChangeRequest != null) {
                foreach (Delegate d in StepChangeRequest.GetInvocationList ())
                    StepChangeRequest -= (EventHandler<EventArgs>)d;
            }

            if (StepChanged != null) {
                foreach (Delegate d in StepChanged.GetInvocationList ())
                    StepChanged -= (EventHandler<EventArgs>)d;
            }

            return Task.CompletedTask;
        }

        public Step? Current {
            get { return Steps.Find (s => s.UUID == AtStep); }
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

        public Patient? Patient {
            get { return Current?.Patient; }
            set { if (Current != null) Current.Patient = value; }
        }

        public async Task Load_Process (string inc) {
            StringReader sRead = new (inc);
            string? line, pline;
            StringBuilder pbuffer;

            this.Reset ();

            try {
                while (!String.IsNullOrEmpty (line = sRead.ReadLine ())) {
                    line = line.Trim ();
                    if (line == "> Begin: Step") {
                        pbuffer = new StringBuilder ();

                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null && pline != "> End: Step")
                            pbuffer.AppendLine (pline);

                        IsLoaded = true;
                        Step s = new Step ();
                        await s.Load_Process (pbuffer.ToString ());
                        Steps.Add (s);
                    } else if (line == "> Begin: DeviceMonitor") {
                        pbuffer = new StringBuilder ();

                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null && pline != "> End: DeviceMonitor")
                            pbuffer.AppendLine (pline);

                        await DeviceMonitor.Load_Process (pbuffer.ToString ());
                    } else if (line == "> Begin: DeviceDefib") {
                        pbuffer = new StringBuilder ();

                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null && pline != "> End: DeviceDefib")
                            pbuffer.AppendLine (pline);

                        await DeviceDefib.Load_Process (pbuffer.ToString ());
                    } else if (line == "> Begin: DeviceECG") {
                        pbuffer = new StringBuilder ();

                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null && pline != "> End: DeviceECG")
                            pbuffer.AppendLine (pline);

                        await DeviceECG.Load_Process (pbuffer.ToString ());
                    } else if (line == "> Begin: DeviceIABP") {
                        pbuffer = new StringBuilder ();

                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null && pline != "> End: DeviceIABP")
                            pbuffer.AppendLine (pline);

                        await DeviceIABP.Load_Process (pbuffer.ToString ());
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

            /* Save() each Device's Settings */
            sWrite.AppendLine ($"{dent}> Begin: DeviceMonitor");
            sWrite.Append (DeviceMonitor.Save (indent + 1));
            sWrite.AppendLine ($"{dent}> End: DeviceMonitor");

            sWrite.AppendLine ($"{dent}> Begin: DeviceDefib");
            sWrite.Append (DeviceDefib.Save (indent + 1));
            sWrite.AppendLine ($"{dent}> End: DeviceDefib");

            sWrite.AppendLine ($"{dent}> Begin: DeviceECG");
            sWrite.Append (DeviceECG.Save (indent + 1));
            sWrite.AppendLine ($"{dent}> End: DeviceECG");

            sWrite.AppendLine ($"{dent}> Begin: DeviceIABP");
            sWrite.Append (DeviceIABP.Save (indent + 1));
            sWrite.AppendLine ($"{dent}> End: DeviceIABP");

            /* Iterate and Save() each Step in Steps[] */
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
            await SetTimer ();
            await Current.Patient.Activate ();

            // Trigger events for loading current Patient, and trigger propagation to devices
            StepChanged?.Invoke (this, new EventArgs ());
            await Current.Patient.OnPatientEvent (Patient.PatientEventTypes.Vitals_Change);
        }

        public async Task LastStep () {
            if (Current == null || String.IsNullOrEmpty (Current.DefaultSource))
                return;

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
            await SetTimer ();
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

            if (Current == null) {
                AtStep = progFrom;
                return;
            }

            if (progFrom != AtStep) {                                       // If the actual step Index changed
                Current.DefaultSource = progFrom;
                Step? stepFrom = Steps.Find (s => s.UUID == (progFrom ?? ""));

                if (stepFrom != null) {
                    CopyDeviceStatus (stepFrom.Patient, Current.Patient);
                    await stepFrom.Patient.Deactivate ();                   // Additional unlinking of events and timers!
                }
            }

            await SetTimer ();
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

        public async Task PauseStep () => await ProgressTimer.Stop ();

        public async Task PlayStep () => await ProgressTimer.Start ();

        public async Task SetTimer () {
            if (Current == null)
                return;

            if (Current.ProgressTimer > 0)
                await ProgressTimer.ResetAuto (Current.ProgressTimer * 1000);
            else
                await ProgressTimer.Stop ();
        }

        public async Task StopTimer () => await ProgressTimer.Stop ();

        public async Task ResumeTimer () => await ProgressTimer.Start ();

        public void ProcessTimer (object? sender, EventArgs e)
            => ProgressTimer.Process ();

        private void ProgressTimer_Tick (object? sender, EventArgs e)
            => _ = NextStep ();

        public class Step {
            public string? UUID = null;
            public Patient Patient;
            public string? Name, Description;

            public List<Progression> Progressions = new List<Progression> ();
            public Progression? DefaultProgression = null;
            public string? DefaultSource = null;
            public int ProgressTimer = -1;

            /* Positioning metadata for II Scenario Editor */
            public int IISEPositionX = 0;
            public int IISEPositionY = 0;

            public Step () {
                UUID = Guid.NewGuid ().ToString ();
                Patient = new Patient ();
            }

            public async Task Load_Process (string inc) {
                StringReader sRead = new (inc);
                string? line, pline;
                StringBuilder pbuffer;

                try {
                    while (!String.IsNullOrEmpty (line = await sRead.ReadLineAsync ())) {
                        line = line.Trim ();
                        if (line == "> Begin: Patient") {
                            pbuffer = new ();

                            while ((pline = await sRead.ReadLineAsync ()) != null && pline != "> End: Patient")
                                pbuffer.AppendLine (pline);

                            await Patient.Load_Process (pbuffer.ToString ());
                        } else if (line == "> Begin: Progression") {
                            pbuffer = new ();

                            while ((pline = await sRead.ReadLineAsync ()) != null && pline != "> End: Progression")
                                pbuffer.AppendLine (pline);

                            Progression p = new ();
                            await p.Load_Process (pbuffer.ToString ());
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
                                case "IISEPositionX": IISEPositionX = int.Parse (pValue); break;
                                case "IISEPositionY": IISEPositionY = int.Parse (pValue); break;
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
                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "IISEPositionX", IISEPositionX));
                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "IISEPositionY", IISEPositionY));

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

                public async Task Load_Process (string inc) {
                    StringReader sRead = new StringReader (inc);
                    string? line;

                    try {
                        while (!String.IsNullOrEmpty (line = await sRead.ReadLineAsync ())) {
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