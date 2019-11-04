﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace II {
    public static class File {
        public static string GetTempFilePath (string extension) {
            return Path.Combine (Path.GetTempPath (),
                String.Format ("Infirmary Integrated\\{0}.{1}",
                Guid.NewGuid (), extension));
        }

        public static string GetTempDirPath () {
            return Path.Combine (Path.GetTempPath (), "Infirmary Integrated\\");
        }

        public static void InitTempDir () {
            if (!Directory.Exists (GetTempDirPath ()))
                Directory.CreateDirectory (GetTempDirPath ());

            ClearTempDir ();
        }

        public static void ClearTempDir () {
            string [] files = Directory.GetFiles (GetTempDirPath ());

            foreach (string f in files)
                System.IO.File.Delete (f);
        }
    }
}