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
        public int Current = 0;
        public List<Stage> Stages = new List<Stage> ();
        public Timer ProgressTimer = new Timer ();

        public Scenario () {
            Stages.Add (new Stage ());
            ProgressTimer.Tick += ProgressTimer_Tick; ;
        }

        public Patient Patient {
            get { return Stages [Current].Patient; }
            set { Stages [Current].Patient = value; }
        }

        public void Load_Process (string inc) {
            StringReader sRead = new StringReader (inc);
            string line, pline;
            StringBuilder pbuffer;

            try {
                while ((line = sRead.ReadLine ()) != null) {
                    if (line == "> Begin: Stage") {
                        pbuffer = new StringBuilder ();

                        while ((pline = sRead.ReadLine ()) != null && pline != "> End: Stage")
                            pbuffer.AppendLine (pline);

                        Stage s = new Stage ();
                        s.Load_Process (pbuffer.ToString ());
                        Stages.Add (s);
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

            SetStage (0);
            sRead.Close ();
        }

        public string Save () {
            StringBuilder sWrite = new StringBuilder ();

            sWrite.AppendLine (String.Format ("{0}:{1}", "Updated", Utility.DateTime_ToString (Updated)));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Name", Name));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Description", Description));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Author", Author));

            foreach (Stage s in Stages) {
                sWrite.AppendLine ("> Begin: Stage");
                sWrite.Append (s.Save ());
                sWrite.AppendLine ("> End: Stage");
            }

            return sWrite.ToString ();
        }

        public Patient NextStage () {
            Current = Math.Min (Current + 1, Stages.Count - 1);
            StartTimer ();
            return Stages [Current].Patient;
        }

        public Patient LastStage () {
            Current = Math.Max (Current - 1, 0);
            StartTimer ();
            return Stages [Current].Patient;
        }

        public Patient SetStage (int incIndex) {
            Current = Utility.Clamp (incIndex, 0, Stages.Count - 1);
            StartTimer ();
            return Stages [Current].Patient;
        }

        public void PauseStage () => ProgressTimer.Stop ();

        public void PlayStage () => ProgressTimer.Start ();

        public void StartTimer () {
            if (Stages [Current].ProgressionTime > 0)
                ProgressTimer.ResetAuto (Stages [Current].ProgressionTime * 1000);
        }

        public void ProcessTimer (object sender, EventArgs e) {
            ProgressTimer.Process ();
        }

        private void ProgressTimer_Tick (object sender, EventArgs e)
            => NextStage ();

        public class Stage {
            public Patient Patient;
            public string Name, Description;

            public List<Progression> Progressions = new List<Progression> ();
            public int ProgressionTime = 0;

            /* Possible progressions/routes to the next stage of the scenario */
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

            public Stage () {
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
                                case "ProgressionTime": ProgressionTime = int.Parse (pValue); break;
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
                sWrite.AppendLine (String.Format ("{0}:{1}", "ProgressionTime", ProgressionTime));

                foreach (Progression p in Progressions) {
                    sWrite.AppendLine ("> Begin: Progression");
                    sWrite.Append (p.Save ());
                    sWrite.AppendLine ("> End: Progression");
                }

                return sWrite.ToString ();
            }
        }
    }
}