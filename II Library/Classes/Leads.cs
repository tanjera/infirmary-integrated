/* Leads.cs
 * Infirmary Integrated
 * By Ibi Keller (Tanjera), (c) 2023
 */

using System;

namespace II {
    public class Lead {
        public Values Value;
        public Lead (Values v) { Value = v; }

        public enum Values {
            ECGI, ECGII, ECGIII,
            ECGAVR, ECGAVL, ECGAVF,
            ECGV1, ECGV2, ECGV3,
            ECGV4, ECGV5, ECGV6,

            SPO2,
            RR,
            ETCO2,

            CVP,
            ABP,
            PA,
            ICP,
            IAP,

            IABP,

            FHR,
            Toco
        }

        public static string LookupString (Values value, bool shortName = false) {
            return String.Format ("LEAD:{0}{1}",
                Enum.GetValues (typeof (Values)).GetValue ((int)value)?.ToString (),
                shortName ? "__SHORT" : "");
        }

        public bool IsTransduced () => IsTransduced (Value);
        public static bool IsTransduced (Values value) {
            switch (value) {
                default: return false;

                case Values.ABP:
                case Values.CVP:
                case Values.PA:
                case Values.ICP:
                case Values.IAP:
                    return true;
            }
        }
    }
}