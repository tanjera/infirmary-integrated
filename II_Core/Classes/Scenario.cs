/* Scenario.cs
 * Infirmary Integrated
 * By Ibi Keller (Tanjera), (c) 2017
 *
 * Management of scenario takes place here; loading and saving patient states.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace II {
    public class Scenario {
        private int _Index = 0;
        private List<Patient> _Stages = new List<Patient> ();

        public Scenario () {
            _Stages.Add (new Patient ());
        }

        public Patient Patient {
            get { return _Stages [_Index]; }
            set { _Stages [_Index] = value; }
        }

        public Patient NextStage () {
            _Index = Math.Min (_Index + 1, _Stages.Count);

            if (_Index == _Stages.Count) {
                // If we've reached the threshhold for the collection, extend with a serialized clone
                _Stages.Add (new Patient ());
                _Stages [_Index].Load_Process (_Stages [_Index].Save ());
            }

            return _Stages [_Index];
        }

        public Patient LastStage () {
            _Index = Math.Max (_Index - 1, 0);
            return _Stages [_Index];
        }

        public Patient SetStage (int incIndex) {
            _Index = Utility.Clamp (incIndex, 0, _Stages.Count - 1);
            return _Stages [_Index];
        }
    }
}