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
    public partial class Server (Settings.Simulator sim) {
        /* Parameters for timekeeping re: simulation */
        public II.Settings.Simulator Simulation = sim;

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
                HttpResponseMessage resp = await hc.GetAsync (FormatForPHP (String.Format (
                    "http://server.infirmary-integrated.com/mirror_get.php?accession={0}&accesshash={1}",
                    m.Accession,
                    Encryption.HashSHA256 (m.PasswordAccess))));

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

                Scenario.Step s = new (m.Simulation);
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
                string ipAddress = (await hc.GetStringAsync ("http://icanhazip.com")).Trim ().Trim ('\n');

                HttpResponseMessage resp = await hc.GetAsync (FormatForPHP (String.Format (
                    "http://server.infirmary-integrated.com/mirror_post.php" +
                    "?accession={0}&key_access={1}&key_edit={2}&patient={3}&updated={4}&client_ip={5}&client_user={6}",
                    m.Accession,
                    Encryption.HashSHA256 (m.PasswordAccess),
                    Encryption.HashSHA256 (m.PasswordEdit),
                    Encryption.EncryptAES (pStr),
                    Utility.DateTime_ToString (pUp),
                    Encryption.HashSHA256 (ipAddress),
                    Encryption.HashSHA256 (Environment.UserName))));

                hc.Dispose ();
            } catch {
                hc.Dispose ();
            }
        }
    }
}