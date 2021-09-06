using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace II {
    public static class File {
        public static void Init () {
            if (!Directory.Exists (GetCacheDir ()))
                Directory.CreateDirectory (GetCacheDir ());

            if (!Directory.Exists (GetAppDataDir ()))
                Directory.CreateDirectory (GetAppDataDir ());

            ClearCache ();
        }

        public static string GetAppDataDir () {
            return Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), GetOSStyling ("Infirmary Integrated"));
        }

        public static string GetConfigPath () {
            return Path.Combine (GetAppDataDir (), GetOSStyling ("config.cfg"));
        }

        public static string GetUsageStatsPath () {
            return Path.Combine (GetAppDataDir (), GetOSStyling ("usage.stats"));
        }

        public static string GetCacheDir () {
            return Path.Combine (Path.GetTempPath (), GetOSStyling ("Infirmary Integrated"));
        }

        public static string GetCachePath (string extension) {
            return Path.Combine (GetCacheDir (), String.Format ("{0}.{1}", Guid.NewGuid (), extension));
        }

        public static void ClearCache () {
            string [] files = Directory.GetFiles (GetCacheDir ());

            foreach (string f in files)
                System.IO.File.Delete (f);
        }

        public static string GetOSStyling (string input) {
            if (String.IsNullOrWhiteSpace (input))
                return input;

            OperatingSystem os = Environment.OSVersion;
            if (os.Platform == PlatformID.Win32NT) {
                StringBuilder sb = new StringBuilder ();

                for (int i = 0; i < input.Length; i++) {
                    if (i == 0 || input [i - 1] == ' ')
                        sb.Append (input [i].ToString ().ToUpper ());
                    else
                        sb.Append (input [i].ToString ().ToLower ());
                }
                return sb.ToString ();
            } else {
                return input.ToLower ();                // e.g. "marana"
            }
        }

        public static string MD5Hash (string filepath) {
            using (MD5 md5 = MD5.Create ()) {
                using (FileStream stream = System.IO.File.OpenRead (filepath)) {
                    byte [] hash = md5.ComputeHash (stream);
                    return BitConverter.ToString (hash).Replace ("-", "").ToLowerInvariant ();
                }
            }
        }
    }
}