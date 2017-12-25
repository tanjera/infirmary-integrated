using System;
using System.Drawing;
using System.Collections.Generic;

namespace II {

    public class Leads {
        public Values Value;
        public Leads (Values v) { Value = v; }

        public enum Values {
            ECG_I, ECG_II, ECG_III,
            ECG_AVR, ECG_AVL, ECG_AVF,
            ECG_V1, ECG_V2, ECG_V3,
            ECG_V4, ECG_V5, ECG_V6,

            SPO2,
            CVP,
            ABP,
            PA,

            RR,
            ETCO2
        }

        public string Description { get { return Descriptions[(int)Value]; } }
        public Color Color { get { return Colors[(int)Value]; } }

        public static List<string> MenuItem_Formats {
            get {
                List<string> o = new List<string> ();
                foreach (Values v in Enum.GetValues (typeof (Values)))
                    o.Add (String.Format ("{0}: {1}", _.UnderscoreToSpace (v.ToString ()), Descriptions[(int)v]));
                return o;
            }
        }
        public static Values Parse_MenuItem (string inc) {
            string portion = _.SpaceToUnderscore (inc.Substring (0, inc.IndexOf (':')));
            try {
                return (Values)Enum.Parse (typeof (Values), portion);
            } catch {
                return Values.ECG_I;
            }
        }

        public static string[] Descriptions = new string[] {
            "Electrocardiograph Lead I", "Electrocardiograph Lead II", "Electrocardiograph Lead III",
            "Electrocardiograph Lead aVR", "Electrocardiograph Lead aVL", "Electrocardiograph Lead aVF",
            "Electrocardiograph Lead V1", "Electrocardiograph Lead V2", "Electrocardiograph Lead V3",
            "Electrocardiograph Lead V4", "Electrocardiograph Lead V5", "Electrocardiograph Lead V6",

            "Pulse Oximetry",
            "Central Venous Pressure",
            "Arterial Blood Pressure",
            "Pulmonary Artery Pressure",

            "Respiratory Rate",
            "End-tidal Capnography"
        };

        public static Color[] Colors = new Color[] {
            Color.Green, Color.Green, Color.Green,
            Color.Green, Color.Green, Color.Green,
            Color.Green, Color.Green, Color.Green,
            Color.Green, Color.Green, Color.Green,

            Color.Orange,
            Color.Blue,
            Color.Red,
            Color.Yellow,

            Color.Salmon,
            Color.Aqua
        };
    }
}