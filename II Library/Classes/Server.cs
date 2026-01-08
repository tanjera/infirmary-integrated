/* Server.cs
 * Infirmary Integrated
 * By Ibi Keller (Tanjera) (c) 2023
 *
 * Connection to MySQL database functioning as a data server for Infirmary Integrated.
 * Establishes database connection, includes functions for queries/commands.
 *
 * Note: Information for database connection (host, database, user ID and password) are
 * stored in Access.cs, to be ignored from git to prevent exposing login information to
 * public git server. On changes to Access.cs, to be archived in Access.rar with password
 * protection for version control.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using II.Localization;

namespace II.Server {
    public partial class Server {
        /* Variables for checking for updates, running bootstrapper */
        public string UpgradeVersion = String.Empty;
        public string UpgradeWebpage = String.Empty;

        private static string FormatForPHP (string inc)
            => inc.Replace ("#", "_").Replace ("$", "_");

        public async Task Get_LatestVersion () {
            HttpClient hc = new ();

            try {
                string resp = await hc.GetStringAsync ("http://server.infirmary-integrated.com/version.php");

                using (StringReader sr = new (resp)) {
                    UpgradeVersion = (await sr.ReadLineAsync ())?.Trim () ?? "0.0";
                    UpgradeWebpage = (await sr.ReadLineAsync ())?.Trim () ?? "";
                }

                hc.Dispose ();
            } catch {
                hc.Dispose ();
            }
        }

        public static async Task<Scenario.Step?> Get_StepMirror (Mirror m) {
            HttpClient hc = new ();

            try {
                HttpResponseMessage resp = await hc.GetAsync (FormatForPHP (
                    $"{m.ServerAddress}{(m.ServerAddress.EndsWith('/') ? String.Empty : '/')}" 
                    + $"mirror_get.php?accession={m.Accession}&accesshash={Encryption.HashSHA256 (m.PasswordAccess)}"));

                // We want this exception thrown in case of web disconnect- it will finish the task and end the faux ThreadLock
                resp.EnsureSuccessStatusCode ();

                string updated, step;
                using (StringReader sr = new (await resp.Content.ReadAsStringAsync ())) {
                    updated = (await sr.ReadLineAsync ())?.Trim () ?? "";
                    step = (await sr.ReadLineAsync ())?.Trim () ?? "";
                }

                if (String.IsNullOrEmpty (updated) || String.IsNullOrEmpty (step))
                    return null;

                DateTime serverUpdated = Utility.DateTime_FromString (updated);
                if (DateTime.Compare (serverUpdated, m.PatientUpdated) <= 0)
                    return null;

                m.ServerQueried = DateTime.UtcNow;
                m.PatientUpdated = serverUpdated;

                Scenario.Step s = new ();
                await s.Load (Encryption.DecryptAES (step.Replace (' ', '+')));

                hc.Dispose ();
                return s;
            } catch {
                hc.Dispose ();
                return null;
            }
        }

        public static async Task Post_StepMirror (Mirror m, string pStr, DateTime pUp) {
            HttpClient hc = new ();

            try {
                HttpResponseMessage resp = await hc.GetAsync (FormatForPHP (
                    $"{m.ServerAddress}{(m.ServerAddress.EndsWith('/') ? String.Empty : '/')}" 
                    + $"mirror_post.php?accession={m.Accession}"
                    + $"&key_access={Encryption.HashSHA256 (m.PasswordAccess)}&key_edit={Encryption.HashSHA256 (m.PasswordEdit)}"
                    + $"&patient={Encryption.EncryptAES (pStr)}&updated={Utility.DateTime_ToString (pUp)}"));

                hc.Dispose ();
            } catch {
                hc.Dispose ();
            }
        }
    }
}