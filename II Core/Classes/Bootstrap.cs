using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;

namespace II {
    public static class Bootstrap {
        public enum UpgradeRoute {
            NULL,
            INSTALL,
            WEBSITE,
            DELAY,
            MUTE
        }

        public static async Task BootstrapInstall_Windows (II.Server.Server server) {
            string installer = II.File.GetTempFilePath ("msi");

            using (HttpClient client = new HttpClient ()) {
                using (HttpResponseMessage httpResponse = await client.GetAsync (
                        server.BootstrapExeUri, HttpCompletionOption.ResponseHeadersRead)) {
                    using (Stream httpStream = await httpResponse.Content.ReadAsStreamAsync ()) {
                        using (Stream outStream = System.IO.File.Open (installer, FileMode.Create)) {
                            await httpStream.CopyToAsync (outStream);
                        }
                    }
                }
            }

            if (II.File.MD5Hash (installer) != server.BootstrapHashMd5)
                return;

            Process.Start (installer);
        }
    }
}