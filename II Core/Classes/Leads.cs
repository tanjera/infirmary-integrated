using System;

namespace II {
    public class Lead {
        public Values Value;
        public Lead (Values v) { Value = v; }

        public enum Values {
            ECG_I, ECG_II, ECG_III,
            ECG_AVR, ECG_AVL, ECG_AVF,
            ECG_V1, ECG_V2, ECG_V3,
            ECG_V4, ECG_V5, ECG_V6,

            SPO2,
            RR,
            ETCO2,

            CVP,
            ABP,
            PA,
            ICP,
            IAP,

            IABP
        }

        public static string LookupString (Values value, bool shortName = false) {
            return String.Format ("LEAD:{0}{1}",
                Enum.GetValues (typeof (Values)).GetValue ((int)value).ToString (),
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

        public static bool IsZeroed (Values value, Patient patient) {
            switch (value) {
                default: return false;
                case Values.ABP: return patient.TransducerZeroed_ABP;
                case Values.CVP: return patient.TransducerZeroed_CVP;
                case Values.PA: return patient.TransducerZeroed_PA;
                case Values.ICP: return patient.TransducerZeroed_ICP;
                case Values.IAP: return patient.TransducerZeroed_IAP;
            }
        }
    }
}