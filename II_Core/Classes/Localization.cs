using System;
using System.Collections.Generic;
using System.Text;

namespace II.Localization {

    public class Languages {
        public Values Value;
        public Languages (Values v) { Value = v; }

        public enum Values {
            AR,
            EN,
            ES,
            FR,
            HE,
            IT,
            JA,
            PT,
            RU,
            ZH_CN
        }

        public string Description { get { return Descriptions [(int)Value]; } }

        public static List<string> MenuItem_Formats {
            get { return Descriptions; }
        }
        public static Values Parse_MenuItem (string inc) {
            try {
                int i = Descriptions.FindIndex (o => { return o == inc; });
                if (i >= 0)
                    return (Values)Enum.GetValues (typeof (Values)).GetValue (i);
                else
                    return Values.EN;
            } catch {
                return Values.EN;
            }
        }

        public static List<string> Descriptions = new List<string> {
            "Arabic: ",
            "English",
            "Spanish: ",
            "French: ",
            "Hebrew: ",
            "Italian: ",
            "Japanese: ",
            "Portugese: ",
            "Russian: ",
            "Chinese, Simplified: "
        };
    }


    public static class Strings {
        public static string Lookup(Languages.Values lang, string key) {
            Pair pair;
            switch (lang) {
                case Languages.Values.AR: pair = AR.Find (o => { return o.Index == key; }); break;
                default:
                case Languages.Values.EN: pair = EN.Find (o => { return o.Index == key; }); break;
                case Languages.Values.ES: pair = ES.Find (o => { return o.Index == key; }); break;
                case Languages.Values.FR: pair = FR.Find (o => { return o.Index == key; }); break;
                case Languages.Values.HE: pair = HE.Find (o => { return o.Index == key; }); break;
                case Languages.Values.IT: pair = IT.Find (o => { return o.Index == key; }); break;
                case Languages.Values.JA: pair = JA.Find (o => { return o.Index == key; }); break;
                case Languages.Values.PT: pair = PT.Find (o => { return o.Index == key; }); break;
                case Languages.Values.RU: pair = RU.Find (o => { return o.Index == key; }); break;
                case Languages.Values.ZH_CN: pair = ZH_CN.Find (o => { return o.Index == key; }); break;
            }
            if (pair != null)
                return pair.Value;
            else
                return "?!ERROR?!";
        }


        public class Pair {
            public string Index { get; set; }
            public string Value { get; set; }

            public Pair () {
                Index = ""; Value = "";
            }
            public Pair (string index, string value) {
                Index = index; Value = value;
            }
        }

        static List<Pair> AR = new List<Pair> {
        };

        static List<Pair> EN = new List<Pair> {
            new Pair("File",                                    "File"),
            new Pair("LoadSimulation",                          "Load Simulation"),
            new Pair("SaveSimulation",                          "Save Simulation"),
            new Pair("ExitProgram",                             "Exit Infirmary Integrated"),
            new Pair("Help",                                    "Help"),
            new Pair("AboutProgram",                            "About Infirmary Integrated"),
            new Pair("HeartRate",                               "Heart Rate"),
            new Pair("BloodPressure",                           "Blood Pressure"),
            new Pair("RespiratoryRate",                         "Respiratory Rate"),
            new Pair("PulseOximetry",                           "Pulse Oximetry"),
            new Pair("Temperature",                             "Temperature"),
            new Pair("EndTidalCO2",                             "End Tidal CO2"),
            new Pair("ArterialBloodPressure",                   "Arterial Blood Pressure"),
            new Pair("CentralVenousPressure",                   "Central Venous Pressure"),
            new Pair("PulmonaryArteryPressure",                 "Pulmonary Artery Pressure"),
            new Pair("RespiratoryRhythm",                       "Respiratory Rhythm"),
            new Pair("InspiratoryExpiratoryRatio",              "Inspiratory-Expiratory Ratio"),
            new Pair("CardiacRhythm",                           "Cardiac Rhythm"),
            new Pair("VitalSigns",                              "Vital Signs"),
            new Pair("AdvancedHemodynamics",                    "Advanced Hemodynamics"),
            new Pair("RespiratoryProfile",                      "Respiratory Profile"),
            new Pair("CardiacProfile",                          "Cardiac Profile"),
            new Pair("UseDefaultVitalSignRanges",               "Use default vital sign ranges for rhythm selections?"),
            new Pair("STSegmentElevation",                      "ST Segment Elevation"),
            new Pair("TWaveElevation",                          "T Wave Elevation"),
            new Pair("ApplyChanges",                            "Apply Changes"),
            new Pair("ResetParameters",                         "Reset Parameters"),

            new Pair("Devices",                                 "Devices"),
            new Pair("CardiacMonitor",                          "Cardiac Monitor"),
            new Pair("12LeadECG",                               "12 Lead ECG"),
            new Pair("Defibrillator",                           "Defibrillator"),
            new Pair("Ventilator",                              "Ventilator"),
            new Pair("IABP",                                    "Intra-Aortic Balloon Pump"),
            new Pair("Cardiotocograph",                         "Cardiotocograph"),
            new Pair("IVPump",                                  "IV Pump"),
            new Pair("LabResults",                              "Laboratory Results"),

            new Pair("DeviceOptions",                           "Device Options"),
            new Pair("PauseDevice",                             "Pause Device"),
            new Pair("NumericRowAmounts",                       "Numeric Row Amounts"),
            new Pair("TracingRowAmounts",                       "Tracing Row Amounts"),
            new Pair("FontSize",                                "Font Size"),
            new Pair("ColorScheme",                             "Color Scheme"),
            new Pair("ToggleFullscreen",                        "Toggle Fullscreen"),
            new Pair("CloseDevice",                             "Close Device"),
            new Pair("PatientOptions",                          "Patient Options"),
            new Pair("NewPatient",                              "New Patient"),
            new Pair("EditPatient",                             "Edit Patient"),
        };

        static List<Pair> ES = new List<Pair> {
        };

        static List<Pair> FR = new List<Pair> {
        };

        static List<Pair> HE = new List<Pair> {
        };

        static List<Pair> IT = new List<Pair> {
        };

        static List<Pair> JA = new List<Pair> {
        };

        static List<Pair> PT = new List<Pair> {
        };

        static List<Pair> RU = new List<Pair> {
        };

        static List<Pair> ZH_CN = new List<Pair> {
        };
    }
}
