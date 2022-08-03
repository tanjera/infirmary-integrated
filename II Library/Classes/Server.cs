/* Server.cs
 * Infirmary Integrated
 * By Ibi Keller (Tanjera)
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

        public class UsageStat {
            public DateTime? Timestamp;
            public string? Version;
            public string? Environment_OS;
            public string? Environment_Language;
            public string? Client_Language;
            public string? Client_Country;
            public string? Hash_IPAddress;
            public string? Hash_MACAddress;
            public string? Hash_Username;

            public UsageStat () {
            }

            public UsageStat (UsageStat copy) {
                Timestamp = copy.Timestamp;
                Version = copy.Version;
                Environment_OS = copy.Environment_OS;
                Environment_Language = copy.Environment_Language;
                Client_Language = copy.Client_Language;
                Client_Country = copy.Client_Country;
                Hash_IPAddress = copy.Hash_IPAddress;
                Hash_MACAddress = copy.Hash_MACAddress;
                Hash_Username = copy.Hash_Username;
            }

            public async Task Init (Language appLanguage) {
                Timestamp = DateTime.UtcNow;
                Client_Language = appLanguage.Value.ToString ();

                await GatherInfo_Offline ();
                await GatherInfo_Online ();
            }

            private Task GatherInfo_Offline () {
                Version = Assembly.GetExecutingAssembly ()?.GetName ()?.Version?.ToString (3) ?? "0.0.0";

                string os = "";

                if (OperatingSystem.IsWindows ())
                    os = "Windows";
                else if (OperatingSystem.IsLinux ())
                    os = "Linux";
                else if (OperatingSystem.IsMacOS ())
                    os = "MacOS";
                else
                    os = "Other";

                Environment_OS = $"{os} {Environment.OSVersion.Version}";

                CultureInfo ci = CultureInfo.CurrentUICulture;
                Environment_Language = ci.ThreeLetterISOLanguageName.ToUpper ();

                Hash_Username = Encryption.HashSHA256 (Environment.UserName);

                NetworkInterface? nInterface = NetworkInterface.GetAllNetworkInterfaces ().Where (
                    (o) => (o.NetworkInterfaceType == NetworkInterfaceType.Ethernet || o.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                        && o.OperationalStatus == OperationalStatus.Up)
                    .First ();
                if (nInterface != null) {
                    string macAddress = nInterface.GetPhysicalAddress ().ToString ();
                    Hash_MACAddress = Encryption.HashSHA256 (macAddress);
                } else {
                    Hash_MACAddress = "No Network Interface";
                }

                return Task.CompletedTask;
            }

            private async Task GatherInfo_Online () {
                HttpClient hc = new ();

                try {
                    // Get the user's IP address using icanhazip.com; only store a hashed version for end-user security
                    string ipAddress = (await hc.GetStringAsync ("http://ipv4.icanhazip.com/")).Trim ();
                    Hash_IPAddress = Encryption.HashSHA256 (ipAddress);

                    // Get the user's country code via geolocation service via PHP script
                    string resp = await hc.GetStringAsync (FormatForPHP (String.Format (
                        "http://server.infirmary-integrated.com/ipdata_provider.php?ip_address={0}",
                        ipAddress)));

                    string? body;
                    using (StringReader sr = new (resp)) {
                        while ((body = await sr.ReadLineAsync ()) != null) {
                            if (body.Contains ("country_code")) {
                                int start = body.IndexOf (": \"") + 3;
                                int length = body.IndexOf ("\",") - start;
                                Client_Country = body.Substring (start, length);
                                break;
                            }
                        }
                    }

                    hc.Dispose ();
                    return;
                } catch {
                    Hash_IPAddress = "OFFLINE";
                    Client_Country = "OFFLINE";

                    hc.Dispose ();
                    return;
                }
            }
        }

        public async Task Run_UsageStats (UsageStat stat) {
            List<UsageStat> stats = new () { stat };

            stats.AddRange (await Retrieve_UsageStats ());                      // Retrieve stored offline usage statistics
            System.IO.File.Delete (File.GetUsageStatsPath ());                  // Delete offline cached usage statistics

            for (int i = stats.Count - 1; i > -1; i--) {
                if (await Post_UsageStats (stats [i])) {                        // If stats post to server successfully
                    stats.RemoveAt (i);                                         // Remove posted stat from running list
                }
            }

            if (stats.Count > 0)                                                // If any stats are not successfully posted
                await Store_UsageStats (stats);                                 // Store them to offline usage statistics
        }

        private async Task<List<UsageStat>> Retrieve_UsageStats () {
            List<UsageStat> listStats = new ();

            if (!System.IO.File.Exists (File.GetUsageStatsPath ()))
                return listStats;

            using StreamReader sr = new (File.GetUsageStatsPath ());

            string? line;
            UsageStat bufferStat = new ();

            while (!String.IsNullOrEmpty (line = await sr.ReadLineAsync ())) {
                if (line == "# USAGESTAT BEGIN") {
                    bufferStat = new UsageStat ();
                } else if (line == "# USAGESTAT END") {
                    listStats.Add (new UsageStat (bufferStat));
                } else if (line.Contains (':')) {
                    string pName = line.Substring (0, line.IndexOf (':')),
                            pValue = line.Substring (line.IndexOf (':') + 1).Trim ();
                    switch (pName) {
                        default: break;
                        case "Timestamp": bufferStat.Timestamp = Utility.DateTime_FromString (pValue); break;
                        case "Version": bufferStat.Version = pValue; break;
                        case "Environment_OS": bufferStat.Environment_OS = pValue; break;
                        case "Environment_Language": bufferStat.Environment_Language = pValue; break;
                        case "Client_Language": bufferStat.Client_Language = pValue; break;
                        case "Client_Country": bufferStat.Client_Country = pValue; break;
                        case "Hash_IPAddress": bufferStat.Hash_IPAddress = pValue; break;
                        case "Hash_MACAddress": bufferStat.Hash_MACAddress = pValue; break;
                        case "Hash_Username": bufferStat.Hash_Username = pValue; break;
                    }
                }
            }

            sr.Close ();
            return listStats;
        }

        private static async Task Store_UsageStats (List<UsageStat> listStats) {
            using StreamWriter sw = new (File.GetUsageStatsPath (), false);

            foreach (UsageStat stat in listStats) {
                await sw.WriteLineAsync ("# USAGESTAT BEGIN");
                await sw.WriteLineAsync ($"Timestamp:{Utility.DateTime_ToString (stat.Timestamp)}");
                await sw.WriteLineAsync ($"Version:{stat.Version}");
                await sw.WriteLineAsync ($"Environment_OS:{stat.Environment_OS}");
                await sw.WriteLineAsync ($"Environment_Language:{stat.Environment_Language}");
                await sw.WriteLineAsync ($"Client_Language:{stat.Client_Language}");
                await sw.WriteLineAsync ($"Client_Country:{stat.Client_Country}");
                await sw.WriteLineAsync ($"Hash_IPAddress:{stat.Hash_IPAddress}");
                await sw.WriteLineAsync ($"Hash_MACAddress:{stat.Hash_MACAddress}");
                await sw.WriteLineAsync ($"Hash_Username:{stat.Hash_Username}");
                await sw.WriteLineAsync ("# USAGESTAT END");
                await sw.WriteLineAsync ("");
                await sw.FlushAsync ();
            }

            sw.Close ();
        }

        private static async Task<bool> Post_UsageStats (UsageStat stat) {
            HttpClient hc = new ();

            try {
                HttpResponseMessage resp = await hc.GetAsync (FormatForPHP (String.Format (
                    "http://server.infirmary-integrated.com/usage_post.php" +
                        "?timestamp={0}&ii_version={1}&env_os={2}&env_lang={3}&client_lang={4}&client_country={5}"
                        + "&client_ip={6}&client_mac={7}&client_user={8}",
                    Utility.DateTime_ToString (stat.Timestamp),
                    stat.Version,
                    stat.Environment_OS,
                    stat.Environment_Language,
                    stat.Client_Language,
                    stat.Client_Country,
                    stat.Hash_IPAddress,
                    stat.Hash_MACAddress,
                    stat.Hash_Username
                    )));

                // We want this exception thrown in case of web disconnect- it will finish the task and end the faux ThreadLock
                resp.EnsureSuccessStatusCode ();

                hc.Dispose ();
                return true;
            } catch {
                hc.Dispose ();
                return false;
            }
        }

        public static async Task Post_Exception (Exception e) {
            HttpClient hc = new ();

            try {
                StringBuilder excData = new ();
                foreach (DictionaryEntry entry in e.Data)
                    excData.AppendLine (String.Format ("{0,-20} '{1}'", entry.Key.ToString (), entry.Value?.ToString ()));

                HttpResponseMessage resp = await hc.GetAsync (FormatForPHP (String.Format (
                    "http://server.infirmary-integrated.com/exception_post.php" +
                        "?timestamp={0}&ii_version={1}&client_os={2}&exception_message={3}&exception_method={4}&exception_stacktrace={5}&exception_hresult={6}&exception_data={7}",

                    Utility.DateTime_ToString (DateTime.UtcNow),
                    Assembly.GetExecutingAssembly ()?.GetName ()?.Version?.ToString (3) ?? "0.0.0",
                    Environment.OSVersion.VersionString,
                    e.Message ?? "null",
                    e.TargetSite?.Name ?? "null",
                    e.StackTrace ?? "null",
                    e.HResult.ToString () ?? "null",
                    excData.ToString () ?? "null")));

                hc.Dispose ();
            } catch {
                hc.Dispose ();
            }
        }

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