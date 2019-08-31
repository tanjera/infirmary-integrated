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

        public Scenario () {
            Steps.Add (new Step ());
            ProgressTimer.Tick += ProgressTimer_Tick; ;
        }

        public Step Current {
            get { return Steps [CurrentIndex]; }
        }

        public Patient Patient {
            get { return Steps [CurrentIndex].Patient; }
            set { Steps [CurrentIndex].Patient = value; }
        }

        public void Load_Process (string inc) {
            StringReader sRead = new StringReader (inc);
            string line, pline;
            StringBuilder pbuffer;

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
            } catch (Exception e) {
                new Server.Servers ().Post_Exception (e);
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

            foreach (Step s in Steps) {
                sWrite.AppendLine ("> Begin: Step");
                sWrite.Append (s.Save ());
                sWrite.AppendLine ("> End: Step");
            }

            return sWrite.ToString ();
        }

        public Patient NextStep () {
            int pFrom = CurrentIndex;

            CurrentIndex = Current.ProgressTo >= 0
                ? Current.ProgressTo
                : Math.Min (CurrentIndex + 1, Steps.Count - 1);

            if (pFrom != CurrentIndex)
                Current.ProgressFrom = pFrom;

            StartTimer ();
            return Current.Patient;
        }

        public Patient LastStep () {
            CurrentIndex = Current.ProgressFrom;
            StartTimer ();
            return Current.Patient;
        }

        public Patient InsertStep () {
            int pFrom = CurrentIndex;
            CurrentIndex = Math.Min (CurrentIndex + 1, Steps.Count);

            Step s = new Step ();
            s.Load_Process (Steps [CurrentIndex - 1].Save ());
            Steps.Insert (CurrentIndex, s);

            Steps [pFrom].ProgressTo = CurrentIndex;
            s.ProgressFrom = pFrom;

            StartTimer ();
            return Current.Patient;
        }

        public Patient SetStep (int incIndex) {
            int pFrom = CurrentIndex;
            CurrentIndex = Utility.Clamp (incIndex, 0, Steps.Count - 1);

            if (pFrom != CurrentIndex)
                Current.ProgressFrom = pFrom;

            StartTimer ();
            return Current.Patient;
        }

        public void PauseStep () => ProgressTimer.Stop ();

        public void PlayStep () => ProgressTimer.Start ();

        public void StartTimer () {
            if (Current.ProgressTimer > 0)
                ProgressTimer.ResetAuto (Current.ProgressTimer * 1000);
        }

        public void ProcessTimer (object sender, EventArgs e) {
            ProgressTimer.Process ();
        }

        private void ProgressTimer_Tick (object sender, EventArgs e)
            => NextStep ();

        public class Step {
            public Patient Patient;
            public string Name, Description;

            public List<Progression> Progressions = new List<Progression> ();
            public int ProgressTo = -1;
            public int ProgressFrom = -1;
            public int ProgressTimer = -1;

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
                            }
                        }
                    }
                } catch (Exception e) {
                    new Server.Servers ().Post_Exception (e);
                    // If the load fails... just bail on the actual value parsing and continue the load process
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
                public string Description;
                public int DestinationIndex;

                public Progression () { }
                public Progression (string desc, int destIndex) {
                    Description = desc;
                    DestinationIndex = destIndex;
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
                    } catch (Exception e) {
                        new Server.Servers ().Post_Exception (e);
                        // If the load fails... just bail on the actual value parsing and continue the load process
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