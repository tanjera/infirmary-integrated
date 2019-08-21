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
        public class Stage {
            public Patient Patient;
            public string Name, Description;

            public Timer ProgressTimer = new Timer ();
            public List<Progression> Progressions = new List<Progression> ();

            /* Possible progressions/routes to the next stage of the scenario */
            public class Progression {
                public string Description;
                public int DestinationIndex;

                public Progression (string desc, int destIndex) {
                    Description = desc;
                    DestinationIndex = destIndex;
                }
            }

            public Stage () {
                Patient = new Patient ();
            }
        }

        public int Current = 0;
        public List<Stage> Stages = new List<Stage> ();

        public Scenario () {
            Stages.Add (new Stage ());
        }

        public Patient Patient {
            get { return Stages [Current].Patient; }
            set { Stages [Current].Patient = value; }
        }

        public Patient NextStage () {
            Current = Math.Min (Current + 1, Stages.Count - 1);
            return Stages [Current].Patient;
        }

        public Patient LastStage () {
            Current = Math.Max (Current - 1, 0);
            return Stages [Current].Patient;
        }

        public Patient SetStage (int incIndex) {
            Current = Utility.Clamp (incIndex, 0, Stages.Count - 1);
            return Stages [Current].Patient;
        }
    }
}