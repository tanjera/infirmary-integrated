using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waveform_Generator {

    public class Lead {
        public Values Value;

        public Lead (Values v) {
            Value = v;
        }

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
    }
}