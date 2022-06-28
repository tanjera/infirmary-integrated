using System;
using System.Collections.Generic;
using System.Text;

namespace II {
    public class Scales {
        public class Intensity {
            public Values Value;
            public enum Values { Absent, Mild, Moderate, Severe }

            public Intensity (Values v) { Value = v; }
            public Intensity () { Value = Values.Absent; }

            public string LookupString () => LookupString (Value);
            public static string LookupString (Values v) {
                return String.Format ("INTENSITY:{0}", Enum.GetValues (typeof (Values)).GetValue ((int)v).ToString ());
            }
        }
    }
}