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
        public class Phase {
            public Patient Patient;
            public string Name, Description;

            private Timer timerProgress = new Timer ();
            private List<Condition> Conditions = new List<Condition> ();

            /* Conditions to advance to the next phase of the scenario */
            public class Condition {
                public Interventions Intervention;
                public int Counter;

                public enum Interventions {
                    Defibrillation,
                    Cardioversion,
                    Timer
                }

                public Condition (Interventions i, int count) {
                    Intervention = i;
                    Counter = count;
                }
            }

            public Phase () {
                Patient = new Patient ();
            }
        }

        private int Index = 0;
        private List<Phase> Phases = new List<Phase> ();

        public Scenario () {
            Phases.Add (new Phase ());
        }

        public Patient Patient {
            get { return Phases [Index].Patient; }
            set { Phases [Index].Patient = value; }
        }

        public Patient NextStage () {
            Index = Math.Min (Index + 1, Phases.Count);

            if (Index == Phases.Count) {
                // If we've reached the threshhold for the collection, extend with a serialized clone
                Phases.Add (new Phase ());
                Phases [Index].Patient.Load_Process (Phases [Index].Patient.Save ());
            }

            return Phases [Index].Patient;
        }

        public Patient LastStage () {
            Index = Math.Max (Index - 1, 0);
            return Phases [Index].Patient;
        }

        public Patient SetStage (int incIndex) {
            Index = Utility.Clamp (incIndex, 0, Phases.Count - 1);
            return Phases [Index].Patient;
        }
    }
}