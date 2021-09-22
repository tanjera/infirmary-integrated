using System;
using System.Diagnostics;
using System.IO;

namespace Publishing {

    public class Utility {

        public static void CopyDirectory (string dirFrom, string dirTo) {
            Directory.CreateDirectory (dirTo);

            foreach (string eachFile in Directory.GetFiles (dirFrom)) {
                File.Copy (eachFile, Path.Combine (dirTo, Path.GetFileName (eachFile)));
            }

            foreach (string eachDir in Directory.GetDirectories (dirFrom)) {
                string subDir = eachDir.Substring (dirFrom.Length).Trim ('\\', '/');
                CopyDirectory (eachDir, Path.Combine (dirTo, subDir));
            }
        }
    }
}