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
        
        public List<Numeric> Numerics = new() {
            Device.Numeric.ECG,
            Device.Numeric.NIBP,
            Device.Numeric.SPO2
        };
        
        public List<Tracing> Tracings =  new() {
                Device.Tracing.ECG_II,
                Device.Tracing.ECG_III,
                Device.Tracing.SPO2
            };
        
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
                    } else if (line == "> Begin: Numerics") {
                        pbuffer = new StringBuilder ();

                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null
                               && pline != "> End: Numerics")
                            pbuffer.AppendLine (pline);

                        await LoadNumerics (pbuffer.ToString ());
                    } else if (line == "> Begin: Tracings") {
                        pbuffer = new StringBuilder ();

                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null
                               && pline != "> End: Tracings")
                            pbuffer.AppendLine (pline);

                        await LoadTracings (pbuffer.ToString ());
                    } else if (line.Contains (":")) {
                        string pName = line.Substring (0, line.IndexOf (':')),
                                pValue = line.Substring (line.IndexOf (':') + 1).Trim ();

                        switch (pName) {
                            default: break;
                            case "IsEnabled": IsEnabled = bool.Parse (pValue); break;
                        }
                    }
                }
            } catch {
                /* If the load fails... just bail on the actual value parsing and continue the load process */
            }

            sRead.Close ();
        }

        public async Task<string> Save (int indent = 1) {
            string dent = Utility.Indent (indent);
            StringBuilder sw = new ();

            sw.AppendLine (String.Format ("{0}{1}:{2}", dent, "IsEnabled", IsEnabled));

            /* Save() the Numerics */
            if (Numerics is not null) {
                sw.AppendLine ($"{dent}> Begin: Numerics");
                sw.Append (await SaveNumerics (indent + 1));
                sw.AppendLine ($"{dent}> End: Numerics");
            }
            
            /* Save() the Tracings */
            if (Tracings is not null) {
                sw.AppendLine ($"{dent}> Begin: Tracings");
                sw.Append (await SaveTracings (indent + 1));
                sw.AppendLine ($"{dent}> End: Tracings");
            }
            
            /* Save() the Alarms */
            if (Alarms is not null) {
                sw.AppendLine ($"{dent}> Begin: Alarms");
                sw.Append (await SaveAlarms (indent + 1));
                sw.AppendLine ($"{dent}> End: Alarms");
            }

            return sw.ToString ();
        }
        
        public async Task LoadNumerics (string inc) {
            using StringReader sRead = new (inc);
            string? line;

            Numerics = new ();

            try {
                while (!String.IsNullOrEmpty (line = await sRead.ReadLineAsync ())) {
                    foreach (string s in line.Split (',')) {
                        Numerics.Add (Enum.Parse<Numeric> (s));
                    }
                }
            } catch {
                /* If the load fails... just bail on the actual value parsing and continue the load process */
            }

            sRead.Close ();
        }
        
        public async Task LoadTracings (string inc) {
            using StringReader sRead = new (inc);
            string? line;

            Tracings = new ();

            try {
                while (!String.IsNullOrEmpty (line = await sRead.ReadLineAsync ())) {
                    foreach (string s in line.Split (',')) {
                        Tracings.Add (Enum.Parse<Tracing> (s));
                    }
                }
            } catch {
                /* If the load fails... just bail on the actual value parsing and continue the load process */
            }

            sRead.Close ();
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
                if (l is not null && l.IsSet && !String.IsNullOrEmpty (line = await l.Save (indent)))
                    sb.AppendLine (line);
            }

            return sb.ToString ();
        }
        
        public async Task<string> SaveNumerics (int indent = 1) {
            StringBuilder sb = new ();
            string? line;

            sb.AppendLine(String.Format ("{0}{1}", 
                Utility.Indent (indent), 
                string.Join (',', Numerics)));

            return sb.ToString ();
        }
        
        public async Task<string> SaveTracings (int indent = 1) {
            StringBuilder sb = new ();
            string? line;

            sb.AppendLine(String.Format ("{0}{1}", 
                Utility.Indent (indent), 
                string.Join (',', Tracings)));

            return sb.ToString ();
        }
    }
}