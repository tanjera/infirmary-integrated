/* Scenario.cs
 * Infirmary Integrated
 * By Ibi Keller (Tanjera), (c) 2017-2023
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

        public List<Step> Steps = new ();
        public Timer ProgressTimer = new ();

        public Settings.Device DeviceMonitor = new (Settings.Device.Devices.Monitor);
        public Settings.Device DeviceDefib = new (Settings.Device.Devices.Defib);
        public Settings.Device DeviceECG = new (Settings.Device.Devices.ECG);
        public Settings.Device DeviceIABP = new (Settings.Device.Devices.IABP);

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
                await (s.Physiology?.Dispose () ?? Task.CompletedTask);

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

        public Physiology? Physiology {
            get { return Current?.Physiology; }
            set { if (Current != null) Current.Physiology = value; }
        }
        
        public async Task Load (string inc) {
            using StringReader sRead = new (inc);
            string? line, pline;
            StringBuilder pbuffer;

            this.Reset ();

            try {
                while (!String.IsNullOrEmpty (line = sRead.ReadLine ())) {
                    line = line.Trim ();
                    if (line == "> Begin: Step") {
                        pbuffer = new StringBuilder ();

                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null
                                && pline != "> End: Step")
                            pbuffer.AppendLine (pline);

                        IsLoaded = true;
                        Step s = new ();
                        await s.Load (pbuffer.ToString ());
                        Steps.Add (s);
                    } else if (line == "> Begin: DeviceMonitor") {
                        pbuffer = new StringBuilder ();

                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null
                                && pline != "> End: DeviceMonitor")
                            pbuffer.AppendLine (pline);

                        await DeviceMonitor.Load (pbuffer.ToString ());
                    } else if (line == "> Begin: DeviceDefib") {
                        pbuffer = new StringBuilder ();

                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null
                                && pline != "> End: DeviceDefib")
                            pbuffer.AppendLine (pline);

                        await DeviceDefib.Load (pbuffer.ToString ());
                    } else if (line == "> Begin: DeviceECG") {
                        pbuffer = new StringBuilder ();

                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null
                                && pline != "> End: DeviceECG")
                            pbuffer.AppendLine (pline);

                        await DeviceECG.Load (pbuffer.ToString ());
                    } else if (line == "> Begin: DeviceIABP") {
                        pbuffer = new StringBuilder ();

                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null
                                && pline != "> End: DeviceIABP")
                            pbuffer.AppendLine (pline);

                        await DeviceIABP.Load (pbuffer.ToString ());
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

            sRead.Close ();

            _ = SetStep (BeginStep);
        }

        public async Task<string> Save (int indent = 1) {
            string dent = Utility.Indent (indent);
            StringBuilder sWrite = new ();

            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Updated", Utility.DateTime_ToString (Updated)));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Name", Name));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Description", Description));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Author", Author));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Beginning", BeginStep));

            /* Save() each Device's Settings */
            sWrite.AppendLine ($"{dent}> Begin: DeviceMonitor");
            sWrite.Append (await DeviceMonitor.Save (indent + 1));
            sWrite.AppendLine ($"{dent}> End: DeviceMonitor");

            sWrite.AppendLine ($"{dent}> Begin: DeviceDefib");
            sWrite.Append (await DeviceDefib.Save (indent + 1));
            sWrite.AppendLine ($"{dent}> End: DeviceDefib");

            sWrite.AppendLine ($"{dent}> Begin: DeviceECG");
            sWrite.Append (await DeviceECG.Save (indent + 1));
            sWrite.AppendLine ($"{dent}> End: DeviceECG");

            sWrite.AppendLine ($"{dent}> Begin: DeviceIABP");
            sWrite.Append (await DeviceIABP.Save (indent + 1));
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
            } else {                                                         // Optional Progression
                AtStep = optProg;
            }

            if (progFrom != AtStep) {                                       // If the actual step Index changed
                if (Current is not null)
                    Current.DefaultSource = progFrom;

                Step? stepFrom = Steps.Find (s => s.UUID == progFrom);

                if (stepFrom != null) {
                    // Carry device status and physiology event list (for numerics e.g. HR calculation) from previous step
                    CopyDeviceStatus (stepFrom.Physiology, Current?.Physiology);
                    Current?.Physiology?.ListPhysiologyEvents.AddRange (stepFrom?.Physiology?.ListPhysiologyEvents ?? new List<Physiology.PhysiologyEventArgs> ());
                    await (stepFrom?.Physiology?.Deactivate () ?? Task.CompletedTask);                   // Additional unlinking of events and timers!
                }
            }

            // Init step regardless of whether step Index changed; step may have been deactivated by StepChangeRequest()
            await SetTimer ();
            await (Current?.Physiology?.Activate () ?? Task.CompletedTask);

            // Trigger events for loading current Patient, and trigger propagation to devices
            StepChanged?.Invoke (this, new EventArgs ());
            await (Current?.Physiology?.OnPhysiologyEvent (Physiology.PhysiologyEventTypes.Vitals_Change) ?? Task.CompletedTask);
        }

        public async Task LastStep () {
            if (Current == null || String.IsNullOrEmpty (Current.DefaultSource))
                return;

            StepChangeRequest?.Invoke (this, new EventArgs ());

            string? progFrom = AtStep;
            AtStep = Current.DefaultSource;

            if (progFrom != AtStep) {                                       // If the actual step Index changed
                //Current.DefaultSource = progFrom; <-- Don't enable this- it will turn "Last Step" into an endless short loop!
                Step? stepFrom = Steps.Find (s => s.UUID == progFrom);

                if (stepFrom != null) {
                    // Carry device status and physiology event list (for numerics e.g. HR calculation) from previous step
                    CopyDeviceStatus (stepFrom.Physiology, Current.Physiology);
                    Current?.Physiology?.ListPhysiologyEvents.AddRange (stepFrom?.Physiology?.ListPhysiologyEvents ?? new List<Physiology.PhysiologyEventArgs> ());
                    await (stepFrom?.Physiology?.Deactivate () ?? Task.CompletedTask);                   // Additional unlinking of events and timers!
                }
            }

            // Init step regardless of whether step Index changed; step may have been deactivated by StepChangeRequest()
            await SetTimer ();
            await (Current?.Physiology?.Activate () ?? Task.CompletedTask);

            // Trigger events for loading current Patient, and trigger propagation to devices
            StepChanged?.Invoke (this, new EventArgs ());
            await (Current?.Physiology?.OnPhysiologyEvent (Physiology.PhysiologyEventTypes.Vitals_Change) ?? Task.CompletedTask);
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
                    // Carry device status and physiology event list (for numerics e.g. HR calculation) from previous step
                    CopyDeviceStatus (stepFrom.Physiology, Current.Physiology);
                    Current?.Physiology?.ListPhysiologyEvents.AddRange (stepFrom?.Physiology?.ListPhysiologyEvents ?? new List<Physiology.PhysiologyEventArgs> ());
                    await (stepFrom?.Physiology?.Deactivate () ?? Task.CompletedTask);                   // Additional unlinking of events and timers!
                }
            }

            await SetTimer ();
            await (Current?.Physiology?.Activate () ?? Task.CompletedTask);

            // Trigger events for loading current Patient, and trigger propagation to devices
            StepChanged?.Invoke (this, new EventArgs ());
            await (Current?.Physiology?.OnPhysiologyEvent (Physiology.PhysiologyEventTypes.Vitals_Change) ?? Task.CompletedTask);
        }

        public void CopyDeviceStatus (Physiology? lastP, Physiology? thisP) {
            if (lastP is null || thisP is null)
                return;

            thisP.TransducerZeroed_CVP = lastP.TransducerZeroed_CVP;
            thisP.TransducerZeroed_ABP = lastP.TransducerZeroed_ABP;
            thisP.TransducerZeroed_PA = lastP.TransducerZeroed_PA;
            thisP.TransducerZeroed_ICP = lastP.TransducerZeroed_ICP;
            thisP.TransducerZeroed_IAP = lastP.TransducerZeroed_IAP;
        }

        public async Task PauseStep () => await ProgressTimer.Stop ();

        public async Task PlayStep () => await ProgressTimer.Start ();

        public async Task SetTimer () {
            if (Current == null)
                return;

            if (Current.ProgressTimer > 0)
                await ProgressTimer.ResetStart (Current.ProgressTimer * 1000);
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
            public Physiology? Physiology;
            public string? Name, Description;

            public List<Progression> Progressions = new ();
            public Progression? DefaultProgression = null;
            public string? DefaultSource = null;
            public int ProgressTimer = -1;

            /* Positioning metadata for II Scenario Editor */
            public int IISEPositionX = 0;
            public int IISEPositionY = 0;

            public Step () {
                UUID = Guid.NewGuid ().ToString ();

                Physiology = new Physiology ();
            }

            public async Task Load (string inc) {
                using StringReader sRead = new (inc);
                string? line, pline;
                StringBuilder pbuffer;

                try {
                    while (!String.IsNullOrEmpty (line = await sRead.ReadLineAsync ())) {
                        line = line.Trim ();
                        if (line == "> Begin: Physiology") {
                            pbuffer = new ();

                            while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null
                                    && pline.Trim () != "> End: Physiology")
                                pbuffer.AppendLine (pline);

                            await (Physiology?.Load (pbuffer.ToString ()) ?? Task.CompletedTask);
                        } else if (line == "> Begin: Progression") {
                            pbuffer = new ();

                            while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null
                                    && pline.Trim () != "> End: Progression")
                                pbuffer.AppendLine (pline);

                            Progression p = new ();
                            await p.Load (pbuffer.ToString ());
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
                StringBuilder sWrite = new ();

                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "UUID", UUID));
                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Name", Name));
                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Description", Description));
                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "DefaultProgression", DefaultProgression?.UUID));
                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "DefaultSource", DefaultSource));
                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "ProgressTime", ProgressTimer));
                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "IISEPositionX", IISEPositionX));
                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "IISEPositionY", IISEPositionY));

                sWrite.AppendLine ($"{dent}> Begin: Physiology");
                sWrite.Append (Physiology?.Save (indent + 1));
                sWrite.AppendLine ($"{dent}> End: Physiology");

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

                public async Task Load (string inc) {
                    using StringReader sRead = new (inc);
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