/* Settings.Device.cs
 * Infirmary Integrated
 * By Ibi Keller (Tanjera) (c) 2023
 *
 * Stores settings for persistence between sessions and for loading/saving simulations
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace II.Settings {
    public class Device {
        /* Scenario-specific Settings:
         * Specific to a running Scenario- may be unloaded and loaded from different Scenario (.ii) files!
         *
         * Do *NOT* place Simulation-wide (e.g. device) settings here!
         * (e.g. the user has turned off the audio for the device)
         */
        
        private Devices DeviceType;

        public List<Alarm> Alarms = new List<Alarm> ();
        public List<Numeric> Numerics = new List<Numeric> ();
        public List<Numeric> Transducers_Zeroed = new List<Numeric> ();
        public List<Tracing> Tracings = new List<Tracing> ();
        
        public bool IsEnabled { get; set; }

        public enum Devices {
            Monitor,
            Defib,
            ECG,
            IABP
        }
        
        public enum Numeric {
            ECG, T, RR, ETCO2, 
            SPO2, NIBP, ABP, CVP,
            CO, PA, ICP, IAP, DEFIB
        }
        
        public enum Tracing {
            ECG_I, ECG_II, ECG_III,
            ECG_AVR, ECG_AVL, ECG_AVF,
            ECG_V1, ECG_V2, ECG_V3,
            ECG_V4, ECG_V5, ECG_V6,

            SPO2, RR, ETCO2,

            CVP, ABP, PA, ICP, IAP
        }

        public static Dictionary<Numeric, string> NumericLookup = new () {
                { Numeric.ECG, "Electrocardiograph (ECG)" },
                { Numeric.T, "Temperature (T)"},
                { Numeric.RR, "Respiratory Rate (RR)"},
                { Numeric.ETCO2, "End-tidal Carbon Dioxide (ETCO2)" },
                { Numeric.SPO2, "Pulse Oximetry (SpO2)" },
                { Numeric.NIBP, "Non-invasive Blood Pressure (NiBP)" },
                { Numeric.ABP, "Arterial Blood Pressure (ABP)"},
                { Numeric.CVP, "Central Venous Pressure (CVP)"},
                { Numeric.CO, "Cardiac Output (CO)"},
                { Numeric.PA, "Pulmonary Arterial Pressures (PA)"},
                { Numeric.ICP, "Intra-cranial Pressure (ICP)"},
                { Numeric.IAP, "Intra-abdominal Pressure (IAP)"},
                { Numeric.DEFIB, "Defibrillator Settings" }
            };
        
        
        public static Dictionary<Tracing, string> TracingLookup = new () {
            { Tracing.ECG_I, "Electrocardiograph (ECG): Lead I"},
            { Tracing.ECG_II, "Electrocardiograph (ECG): Lead II"},
            { Tracing.ECG_III, "Electrocardiograph (ECG): Lead III"},
            { Tracing.ECG_AVR, "Electrocardiograph (ECG): Lead aVR"},
            { Tracing.ECG_AVL, "Electrocardiograph (ECG): Lead aVL"},
            { Tracing.ECG_AVF, "Electrocardiograph (ECG): Lead aVF"},
            { Tracing.ECG_V1, "Electrocardiograph (ECG): Lead V1"},
            { Tracing.ECG_V2, "Electrocardiograph (ECG): Lead V2"},
            { Tracing.ECG_V3, "Electrocardiograph (ECG): Lead V3"},
            { Tracing.ECG_V4, "Electrocardiograph (ECG): Lead V4"},
            { Tracing.ECG_V5, "Electrocardiograph (ECG): Lead V5"},
            { Tracing.ECG_V6, "Electrocardiograph (ECG): Lead V6"},
            { Tracing.RR, "Respiratory Rate (RR)"},
            { Tracing.ETCO2, "End-tidal Carbon Dioxide (ETCO2)" },
            { Tracing.SPO2, "Pulse Oximetry (SpO2)" },
            { Tracing.ABP, "Arterial Blood Pressure (ABP)"},
            { Tracing.CVP, "Central Venous Pressure (CVP)"},
            { Tracing.PA, "Pulmonary Arterial Pressures (PA)"},
            { Tracing.ICP, "Intra-cranial Pressure (ICP)"},
            { Tracing.IAP, "Intra-abdominal Pressure (IAP)"},
        };

        public Device (Devices d) {
            DeviceType = d;

            switch (DeviceType) {
                default: break;
                
                case Devices.Monitor:
                    Numerics = new List<Numeric> () {
                        Numeric.ECG,
                        Numeric.NIBP,
                        Numeric.SPO2
                    };
                    
                    Tracings = new List<Tracing> () {
                        Tracing.ECG_II,
                        Tracing.ECG_III,
                        Tracing.SPO2
                    };
                    
                    Alarms = new (Alarm.DefaultListing_Adult);
                    break;
            
                case Devices.Defib:
                    Numerics = new List<Numeric> () {
                        Numeric.DEFIB,
                        Numeric.ECG,
                        Numeric.NIBP,
                        Numeric.SPO2
                    };
                    
                    Tracings = new List<Tracing> () {
                        Tracing.ECG_II,
                        Tracing.SPO2
                    };
                    break;
            }
        }

        // Not all Devices can show all Numerics! This is the hard check on whether a Device supports a Numeric
        public static bool CanUse (Devices d, Numeric n) {
            switch (d) {
                default: return false;
                
                case Devices.Monitor:
                    return n is Numeric.ECG 
                        or Numeric.T 
                        or Numeric.RR 
                        or Numeric.ETCO2 
                        or Numeric.SPO2 
                        or Numeric.NIBP 
                        or Numeric.ABP 
                        or Numeric.CVP 
                        or Numeric.CO 
                        or Numeric.PA 
                        or Numeric.ICP 
                        or Numeric.IAP;
                    
                case Devices.Defib:
                    return n is Numeric.ECG
                        or Numeric.T
                        or Numeric.RR
                        or Numeric.ETCO2
                        or Numeric.SPO2
                        or Numeric.NIBP
                        or Numeric.ABP
                        or Numeric.CVP
                        or Numeric.PA
                        or Numeric.DEFIB;
            }
        }
        
        // Not all Devices can show all Tracing! This is the hard check on whether a Device supports a Tracing
        public static bool CanUse (Devices d, Tracing t) {
            switch (d) {
                default: return false;
                    
                    case Devices.Monitor:
                    return t is Tracing.ECG_I or Tracing.ECG_II or Tracing.ECG_III
                        or Tracing.ECG_AVF or Tracing.ECG_AVL or Tracing.ECG_AVR
                        or Tracing.ECG_V1 or Tracing.ECG_V2 or Tracing.ECG_V3
                        or Tracing.ECG_V4 or Tracing.ECG_V5 or Tracing.ECG_V6
                        or Tracing.SPO2
                        or Tracing.CVP
                        or Tracing.ABP
                        or Tracing.PA
                        or Tracing.RR
                        or Tracing.ETCO2
                        or Tracing.ICP or Tracing.IAP;
                
                case Devices.Defib:
                    return t is Tracing.ECG_I or Tracing.ECG_II or Tracing.ECG_III
                        or Tracing.ECG_AVF or Tracing.ECG_AVL or Tracing.ECG_AVR
                        or Tracing.ECG_V1 or Tracing.ECG_V2 or Tracing.ECG_V3
                        or Tracing.ECG_V4 or Tracing.ECG_V5 or Tracing.ECG_V6
                        or Tracing.SPO2
                        or Tracing.CVP
                        or Tracing.ABP
                        or Tracing.PA
                        or Tracing.RR
                        or Tracing.ETCO2;
            }
        }
        
        public async Task Load (string inc) {
            using StringReader sRead = new (inc);
            string? line, pline;
            StringBuilder pbuffer;

            try {
                while (!String.IsNullOrEmpty (line = await sRead.ReadLineAsync ())) {
                    line = line.Trim ();

                    if (line == "> Begin: Alarms") {
                        pbuffer = new StringBuilder ();

                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null
                               && pline != "> End: Alarms")
                            pbuffer.AppendLine (pline);

                        await LoadAlarms (pbuffer.ToString ());
                    } else if (line.Contains (":")) {
                        string pName = line.Substring (0, line.IndexOf (':')),
                            pValue = line.Substring (line.IndexOf (':') + 1).Trim ();

                        switch (pName) {
                            default: break;
                            case "IsEnabled": IsEnabled = bool.Parse (pValue); break;

                            case "Numerics":
                                Numerics = new();
                                foreach (string s in pValue.Split (',')) {
                                    if (Enum.TryParse<Numeric> (s, true, out Numeric res)) {
                                        Numerics.Add (res);
                                    }
                                }

                                break;
                            
                            case "Numerics_Zeroed":
                                Transducers_Zeroed = new();
                                foreach (string s in pValue.Split (',')) {
                                    if (Enum.TryParse<Numeric> (s, true, out Numeric res)) {
                                        Transducers_Zeroed.Add (res);
                                    }
                                }

                                break;
                            
                            case "Tracings":
                                Tracings = new();
                                foreach (string s in pValue.Split (',')) {
                                    if (Enum.TryParse<Tracing> (s, true, out Tracing res)) {
                                        Tracings.Add (res);
                                    }
                                }

                                break;
                        }
                    }
                }
            } catch {
                /* If the load fails... just bail on the actual value parsing and continue the load process */
            } finally {
                sRead.Close ();    
            }
        }

        public async Task<string> Save (int indent = 1) {
            string dent = Utility.Indent (indent);
            StringBuilder sw = new ();

            sw.AppendLine (String.Format ("{0}{1}:{2}", dent, "IsEnabled", IsEnabled));

            /* Save() the Numerics */
            if (Numerics.Count > 0) {
                sw.AppendLine (String.Format ("{0}{1}:{2}", 
                    dent,
                    "Numerics", 
                    string.Join (',', Numerics)));
            }
            
            if (Transducers_Zeroed.Count > 0) {
                sw.AppendLine (String.Format ("{0}{1}:{2}", 
                    dent,
                    "Numerics_Zeroed", 
                    string.Join (',', Transducers_Zeroed)));
            }
            
            /* Save() the Tracings */
            if (Tracings.Count > 0) {
                sw.AppendLine (String.Format ("{0}{1}:{2}", 
                    dent,
                    "Tracings", 
                    string.Join (',', Tracings)));
            }
            
            /* Save() the Alarms */
            if (Alarms.Count > 0) {
                sw.AppendLine ($"{dent}> Begin: Alarms");
                sw.Append (await SaveAlarms (indent + 1));
                sw.AppendLine ($"{dent}> End: Alarms");
            }

            return sw.ToString ();
        }

        public async Task LoadAlarms (string inc) {
            using StringReader sRead = new (inc);
            string? line;

            Alarms = new ();

            try {
                while (!String.IsNullOrEmpty (line = await sRead.ReadLineAsync ())) {
                    Alarm? alarm = new ();
                    await alarm.Load (line);
                    if (alarm.Parameter is not null)
                        Alarms.Add (alarm);
                }
            } catch {
                /* If the load fails... just bail on the actual value parsing and continue the load process */
            }

            sRead.Close ();
        }

        public async Task<string> SaveAlarms (int indent = 1) {
            StringBuilder sb = new ();
            string? line;

            foreach (Alarm l in Alarms) {
                if (l.IsSet && !String.IsNullOrEmpty (line = await l.Save (indent)))
                    sb.AppendLine (line);
            }

            return sb.ToString ();
        }
    }
}