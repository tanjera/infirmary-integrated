using System;
using System.IO;
using System.Linq;
using System.Text;

namespace II {
    public static class Utility {
        public const string Version = "1.3.0";

        public static bool IsNewerVersion (string current, string comparison) {
            string [] curSplit = current.Split ('.'),
                    compSplit = comparison.Split ('.');
            int buffer;

            for (int i = 0; i < compSplit.Length; i++) {
                if (!int.TryParse (curSplit [i], out buffer))           // Error in parsing current version?
                    return true;                                            // Then send for newer version!
                else if (!int.TryParse (compSplit [i], out buffer))     // Error in parsing comparison version?
                    return false;                                           // Then dodge the newer version!
                else if ((i < curSplit.Length ? int.Parse (curSplit [i]) : 0) < int.Parse (compSplit [i]))
                    return true;
                else if ((i < curSplit.Length ? int.Parse (curSplit [i]) : 0) > int.Parse (compSplit [i]))
                    return false;
            }

            return false;
        }

        public static double UtcStartTime {
            get { return (double)(DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess ().StartTime.ToUniversalTime ()).TotalSeconds; }
        }

        public static string DateTime_ToString (DateTime dt)
            => dt.ToString ("yyyy/MM/dd HH:mm:ss");

        public static string DateTime_ToString_FilePath (DateTime dt)
            => dt.ToString ("yyyy.MM.dd.HH.mm.ss");

        public static DateTime DateTime_FromString (string str) {
            return new DateTime (
                int.Parse (str.Substring (0, 4)),
                int.Parse (str.Substring (5, 2)),
                int.Parse (str.Substring (8, 2)),
                int.Parse (str.Substring (11, 2)),
                int.Parse (str.Substring (14, 2)),
                int.Parse (str.Substring (17, 2)));
        }

        public static string RandomString (int length) {
            Random r = new Random ();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string (Enumerable.Repeat (chars, length)
              .Select (s => s [r.Next (s.Length)]).ToArray ());
        }

        public static string WrapString (string input) {
            return input.Replace (" ", Environment.NewLine);
        }

        public static string UnderscoreToSpace (string str) {
            return str.Replace ('_', ' ');
        }

        public static string SpaceToUnderscore (string str) {
            return str.Replace (' ', '_');
        }
    }
}