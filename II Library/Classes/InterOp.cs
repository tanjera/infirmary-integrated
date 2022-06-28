/* InterOp.cs
 * Infirmary Integrated
 * By Ibi Keller (Tanjera), (c) 2021
 *
 * Code for cross-platform functionality
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace II {

    public class InterOp {

        public enum Platforms {
            Null,
            Windows,
            Linux,
            OSX
        }

        public static Platforms? GetPlatform () {
            if (RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
                return Platforms.Windows;
            } else if (RuntimeInformation.IsOSPlatform (OSPlatform.Linux)) {
                return Platforms.Linux;
            } else if (RuntimeInformation.IsOSPlatform (OSPlatform.OSX)) {
                return Platforms.OSX;
            } else {
                return null;
            }
        }

        public static void OpenBrowser (string url) {
            try {
                Process.Start (url);
            } catch {
                switch (GetPlatform ()) {
                    default: throw;

                    case Platforms.Windows:
                        url = url.Replace ("&", "^&");
                        Process.Start (new ProcessStartInfo ("cmd", $"/c start {url}") { CreateNoWindow = true });
                        break;

                    case Platforms.Linux:
                        Process.Start ("xdg-open", url);
                        break;

                    case Platforms.OSX:
                        Process.Start ("open", url);
                        break;
                }
            }
        }
    }
}