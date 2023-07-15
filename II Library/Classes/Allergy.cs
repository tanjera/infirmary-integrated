/* Allergy.cs
 * Infirmary Integrated
 * By Ibi Keller (Tanjera), (c) 2023
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace II {
    public class Allergy {
        public string? UUID;

        public string? Allergen;
        public string? Reaction;
        public Scales.Intensity.Values Intensity;

        public Allergy () {
            UUID = Guid.NewGuid ().ToString ();
        }

        public Task Load (string inc) {
            using StringReader sRead = new (inc);

            try {
                string? line;
                while (!String.IsNullOrEmpty (line = sRead.ReadLine ())) {
                    line = line.Trim ();

                    if (line.Contains (':')) {
                        string pName = line.Substring (0, line.IndexOf (':')),
                                pValue = line.Substring (line.IndexOf (':') + 1).Trim ();
                        switch (pName) {
                            default: break;

                            case "UUID": UUID = pValue; break;

                            case "Allergen": Allergen = pValue; break;
                            case "Reaction": Reaction = pValue; break;
                            case "Intensity": Intensity = (Scales.Intensity.Values)Enum.Parse (typeof (Scales.Intensity.Values), pValue); break;
                        }
                    }
                }
            } catch {
                /* If the load fails... just bail on the actual value parsing and continue the load process */
            }

            sRead.Close ();

            return Task.CompletedTask;
        }

        public string Save (int indent = 1) {
            string dent = Utility.Indent (indent);
            StringBuilder sWrite = new ();

            // File/scenario information
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "UUID", UUID));

            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Allergen", Allergen));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Reaction", Reaction));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Intensity", Intensity));

            return sWrite.ToString ();
        }
    }
}