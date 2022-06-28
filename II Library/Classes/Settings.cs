/* Settings.cs
 * Infirmary Integrated
 * By Ibi Keller (Tanjera)
 *
 * Stores program settings for persistence between sessions.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace II {
    public class Settings {
        public string Language;
        public bool AutoApplyChanges;
        public Point WindowSize;
        public Point WindowPosition;
        public bool MuteUpgrade;
        public DateTime MuteUpgradeDate;

        public Settings () {
            Language = "ENG";
            AutoApplyChanges = false;
            WindowSize = new Point (700, 560);
            MuteUpgrade = false;
            MuteUpgradeDate = new DateTime (2000, 1, 1);
        }

        public static bool Exists () {
            return System.IO.File.Exists (File.GetConfigPath ());
        }

        public void Load () {
            if (!System.IO.File.Exists (File.GetConfigPath ()))
                return;

            StreamReader sr = new StreamReader (File.GetConfigPath ());

            string? line;
            bool parseBool;
            int parseInt;

            while ((line = sr.ReadLine ()) != null) {
                if (line.Contains (":")) {
                    string pName = line.Substring (0, line.IndexOf (':')),
                            pValue = line.Substring (line.IndexOf (':') + 1).Trim ();
                    switch (pName) {
                        default: break;

                        case "Language": Language = pValue; break;

                        case "AutoApplyChanges":
                            if (bool.TryParse (pValue, out parseBool))
                                AutoApplyChanges = parseBool;
                            break;

                        // Settings for the size of the Patient Editor
                        case "WindowSizeX":
                            if (int.TryParse (pValue, out parseInt))
                                WindowSize.X = parseInt;
                            break;

                        case "WindowSizeY":
                            if (int.TryParse (pValue, out parseInt))
                                WindowSize.Y = parseInt;
                            break;

                        // Settings for muting whether new program upgrades are available for download
                        case "MuteUpgrade":
                            if (bool.TryParse (pValue, out parseBool))
                                MuteUpgrade = parseBool;
                            break;

                        case "MuteUpgradeDate":
                            MuteUpgradeDate = Utility.DateTime_FromString (pValue);
                            break;
                    }
                }
            }

            sr.Close ();
            sr.Dispose ();
        }

        public void Save () {
            StreamWriter sw = new StreamWriter (File.GetConfigPath (), false);

            sw.WriteLine ($"Language:{Language}");
            sw.WriteLine ($"AutoApplyChanges:{AutoApplyChanges}");
            sw.WriteLine ($"WindowSizeX:{WindowSize.X}");
            sw.WriteLine ($"WindowSizeY:{WindowSize.Y}");
            sw.WriteLine ($"WindowPositionX:{WindowPosition.X}");
            sw.WriteLine ($"WindowPositionY:{WindowPosition.Y}");
            sw.WriteLine ($"MuteUpgrade:{MuteUpgrade}");
            sw.WriteLine ($"MuteUpgradeDate:{Utility.DateTime_ToString (MuteUpgradeDate)}");

            sw.Close ();
            sw.Dispose ();
        }
    }
}