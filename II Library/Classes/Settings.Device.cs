/* Settings.Device.cs
 * Infirmary Integrated
 * By Ibi Keller (Tanjera)
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

        public Alarms? Alarms;
        public bool IsEnabled { get; set; }

        public enum Devices {
            Monitor,
            Defib,
            ECG,
            IABP
        }

        public Device (Devices d) {
            DeviceType = d;

            if (DeviceType == Devices.Monitor) {
                Alarms = new ();
                _ = Alarms.Import (Alarms.DefaultListing_Adult);
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

                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null && pline != "> End: Alarms")
                            pbuffer.AppendLine (pline);

                        Alarms = new ();
                        await Alarms.Load (pbuffer.ToString ());
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

            /* Save() the Alarms */
            if (Alarms is not null) {
                sw.AppendLine ($"{dent}> Begin: Alarms");
                sw.Append (await Alarms.Save (indent + 1));
                sw.AppendLine ($"{dent}> End: Alarms");
            }

            return sw.ToString ();
        }
    }
}