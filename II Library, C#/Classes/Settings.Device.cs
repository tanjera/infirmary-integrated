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
        private Devices DeviceType;

        public List<Alarm> Alarms = new List<Alarm> ();
        public List<Numeric> Numerics = new List<Numeric> ();
        public List<Numeric> Numerics_Zeroed = new List<Numeric> ();
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
            CO, PA, ICP, IAP
        }
        
        public enum Tracing {
            ECG_I, ECG_II, ECG_III,
            ECG_AVR, ECG_AVL, ECG_AVF,
            ECG_V1, ECG_V2, ECG_V3,
            ECG_V4, ECG_V5, ECG_V6,

            SPO2, RR, ETCO2,

            CVP, ABP, PA, ICP, IAP,

            IABP,

            FHR, TOCO
        }

        public Device (Devices d) {
            DeviceType = d;

            if (DeviceType == Devices.Monitor) {
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
                                Numerics_Zeroed = new();
                                foreach (string s in pValue.Split (',')) {
                                    if (Enum.TryParse<Numeric> (s, true, out Numeric res)) {
                                        Numerics_Zeroed.Add (res);
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
            
            if (Numerics_Zeroed.Count > 0) {
                sw.AppendLine (String.Format ("{0}{1}:{2}", 
                    dent,
                    "Numerics_Zeroed", 
                    string.Join (',', Numerics_Zeroed)));
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