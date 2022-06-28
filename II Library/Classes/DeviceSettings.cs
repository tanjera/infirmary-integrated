/* Scenario.cs
 * Infirmary Integrated
 * By Ibi Keller (Tanjera), (c) 2022
 *
 * Handling and storage of Devices' Settings takes place here
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace II {
    public class DeviceSettings {
        public bool IsEnabled { get; set; }

        public string Save (int indent = 1) {
            string dent = Utility.Indent (indent);
            StringBuilder sw = new StringBuilder ();

            sw.AppendLine (String.Format ("{0}{1}:{2}", dent, "IsEnabled", IsEnabled));

            return sw.ToString ();
        }

        public void Load_Process (string inc) {
            StringReader sRead = new StringReader (inc);
            string? line;

            try {
                while (!String.IsNullOrEmpty (line = sRead.ReadLine ())) {
                    line = line.Trim ();

                    if (line.Contains (":")) {
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
    }
}