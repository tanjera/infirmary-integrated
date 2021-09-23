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
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;

using II.Localization;

namespace II.Server {

    public partial class Server {
        /* Variables for checking for updates, running bootstrapper */
        public string UpgradeVersion = String.Empty;
        public string UpgradeWebpage = String.Empty;
        public string BootstrapExeUri = String.Empty;
        public string BootstrapHashMd5 = String.Empty;

        private static string FormatForPHP (string inc)
            => inc.Replace ("#", "_").Replace ("$", "_");

        public class UsageStat {
            public DateTime Timestamp;
            public string Version;
            public string Environment_OS;
            public string Environment_Language;
            public string Client_Language;
            public string Client_Country;
            public string Hash_IPAddress;
            public string Hash_MACAddress;
            public string Hash_Username;

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

            public UsageStat (Language appLanguage) {
                Timestamp = DateTime.UtcNow;
                Client_Language = appLanguage.Value.ToString ();

                GatherInfo_Offline ();
                GatherInfo_Online ();
            }

            private void GatherInfo_Offline () {
                string version = Assembly.GetExecutingAssembly ()?.GetName ()?.Version?.ToString (3) ?? "0.0.0";

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

                NetworkInterface nInterface = NetworkInterface.GetAllNetworkInterfaces ().Where (
                    (o) => (o.NetworkInterfaceType == NetworkInterfaceType.Ethernet || o.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                        && o.OperationalStatus == OperationalStatus.Up)
                    .First ();
                if (nInterface != null) {
                    string macAddress = nInterface.GetPhysicalAddress ().ToString ();
                    Hash_MACAddress = Encryption.HashSHA256 (macAddress);
                } else {
                    Hash_MACAddress = "No Network Interface";
                }
            }

            private void GatherInfo_Online () {
                try {
                    // Get the user's IP address using icanhazip.com; only store a hashed version for end-user security
                    string ipAddress = new WebClient ().DownloadString ("http://ipv4.icanhazip.com/").Trim ();
                    Hash_IPAddress = Encryption.HashSHA256 (ipAddress);

                    // Get the user's country code via geolocation service via PHP script
                    WebRequest req = WebRequest.Create (FormatForPHP (String.Format (
                        "http://server.infirmary-integrated.com/ipdata_provider.php?ip_address={0}",
                        ipAddress)));
                    WebResponse resp = req.GetResponse ();
                    Stream str = resp.GetResponseStream ();
                    string body = String.Empty;

                    using (StreamReader sr = new StreamReader (str)) {
                        while (!sr.EndOfStream) {
                            body = sr.ReadLine ();

                            if (body.Contains ("country_code"))
                                break;
                        }
                    }

                    if (!body.Contains ("country_code")) {
                        return;
                    }

                    resp.Close ();
                    str.Close ();

                    resp.Dispose ();
                    str.Dispose ();

                    int start = body.IndexOf (": \"") + 3;
                    int length = body.IndexOf ("\",") - start;

                    Client_Country = body.Substring (start, length);
                } catch {
                    Hash_IPAddress = "Offline";
                    Client_Country = "Offline";
                    return;
                }
            }
        }

        public void Run_UsageStats (UsageStat stat) {
            List<UsageStat> stats = new List<UsageStat> () { stat };

            stats.AddRange (Retrieve_UsageStats ());                    // Retrieve stored offline usage statistics
            System.IO.File.Delete (File.GetUsageStatsPath ());          // Delete offline cached usage statistics

            for (int i = stats.Count - 1; i > -1; i--) {
                if (Post_UsageStats (stats [i])) {                      // If stats post to server successfully
                    stats.RemoveAt (i);                                 // Remove posted stat from running list
                }
            }

            if (stats.Count > 0)                                        // If any stats are not successfully posted
                Store_UsageStats (stats);                               // Store them to offline usage statistics
        }

        private List<UsageStat> Retrieve_UsageStats () {
            List<UsageStat> listStats = new List<UsageStat> ();

            if (!System.IO.File.Exists (File.GetUsageStatsPath ()))
                return listStats;

            StreamReader sr = new StreamReader (File.GetUsageStatsPath ());

            string line;
            UsageStat bufferStat = new UsageStat ();

            while ((line = sr.ReadLine ()) != null) {
                if (line == "# USAGESTAT BEGIN") {
                    bufferStat = new UsageStat ();
                } else if (line == "# USAGESTAT END") {
                    listStats.Add (new UsageStat (bufferStat));
                } else if (line.Contains (":")) {
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
            sr.Dispose ();

            return listStats;
        }

        private void Store_UsageStats (List<UsageStat> listStats) {
            StreamWriter sw = new StreamWriter (File.GetUsageStatsPath (), false);

            foreach (UsageStat stat in listStats) {
                sw.WriteLine ("# USAGESTAT BEGIN");

                sw.WriteLine ($"Timestamp:{Utility.DateTime_ToString (stat.Timestamp)}");
                sw.WriteLine ($"Version:{stat.Version}");
                sw.WriteLine ($"Environment_OS:{stat.Environment_OS}");
                sw.WriteLine ($"Environment_Language:{stat.Environment_Language}");
                sw.WriteLine ($"Client_Language:{stat.Client_Language}");
                sw.WriteLine ($"Client_Country:{stat.Client_Country}");
                sw.WriteLine ($"Hash_IPAddress:{stat.Hash_IPAddress}");
                sw.WriteLine ($"Hash_MACAddress:{stat.Hash_MACAddress}");
                sw.WriteLine ($"Hash_Username:{stat.Hash_Username}");

                sw.WriteLine ("# USAGESTAT END");
                sw.WriteLine ("");
                sw.Flush ();
            }

            sw.Close ();
            sw.Dispose ();
        }

        private bool Post_UsageStats (UsageStat stat) {
            try {
                WebRequest req = WebRequest.Create (FormatForPHP (String.Format (
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

                req.GetResponse ();

                return true;
            } catch {
                return false;
            }
        }

        public void Post_Exception (Exception e) {
            try {
                StringBuilder excData = new StringBuilder ();
                foreach (DictionaryEntry entry in e.Data)
                    excData.AppendLine (String.Format ("{0,-20} '{1}'", entry.Key.ToString (), entry.Value.ToString ()));

                WebRequest req = WebRequest.Create (FormatForPHP (String.Format (
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

                req.GetResponse ();
            } catch {
            }
        }

        public void Get_LatestVersion_Windows () {
            try {
                WebRequest req = WebRequest.Create ("http://server.infirmary-integrated.com/version.php");
                WebResponse resp = req.GetResponse ();
                Stream str = resp.GetResponseStream ();

                using (StreamReader sr = new StreamReader (str)) {
                    UpgradeVersion = sr.ReadLine ().Trim ();
                    UpgradeWebpage = sr.ReadLine ().Trim ();
                    BootstrapExeUri = sr.ReadLine ().Trim ();
                    BootstrapHashMd5 = sr.ReadLine ().Trim ();
                }

                resp.Close ();
                str.Close ();

                resp.Dispose ();
                str.Dispose ();

                UpgradeVersion = String.IsNullOrEmpty (UpgradeVersion) ? "0.0" : UpgradeVersion;
                UpgradeWebpage = String.IsNullOrEmpty (UpgradeWebpage) ? "" : UpgradeWebpage;
                BootstrapExeUri = String.IsNullOrEmpty (BootstrapExeUri) ? "" : BootstrapExeUri;
                BootstrapHashMd5 = String.IsNullOrEmpty (BootstrapHashMd5) ? "" : BootstrapHashMd5;
            } catch {
            }
        }

        public Patient Get_PatientMirror (Mirror m) {
            try {
                WebRequest req = WebRequest.Create (FormatForPHP (String.Format (
                    "http://server.infirmary-integrated.com/mirror_get.php?accession={0}&accesshash={1}",
                    m.Accession,
                    Encryption.HashSHA256 (m.PasswordAccess))));

                WebResponse resp = req.GetResponse ();
                Stream str = resp.GetResponseStream ();

                string updated = String.Empty;
                string patient = String.Empty;
                using (StreamReader sr = new StreamReader (str)) {
                    updated = sr.ReadLine ().Trim ();
                    patient = sr.ReadLine ().Trim ();
                }

                resp.Close ();
                str.Close ();

                resp.Dispose ();
                str.Dispose ();

                DateTime serverUpdated = Utility.DateTime_FromString (updated);
                if (DateTime.Compare (serverUpdated, m.PatientUpdated) <= 0)
                    return null;

                m.ServerQueried = DateTime.UtcNow;
                m.PatientUpdated = serverUpdated;
                Patient p = new Patient ();
                p.Load_Process (Encryption.DecryptAES (patient));

                return p;
            } catch {
                return null;
            }
        }

        public void Post_PatientMirror (Mirror m, string pStr, DateTime pUp) {
            try {
                string ipAddress = new WebClient ().DownloadString ("http://icanhazip.com").Trim ();

                WebRequest req = WebRequest.Create (FormatForPHP (String.Format (
                    "http://server.infirmary-integrated.com/mirror_post.php" +
                    "?accession={0}&key_access={1}&key_edit={2}&patient={3}&updated={4}&client_ip={5}&client_user={6}",
                    m.Accession,
                    Encryption.HashSHA256 (m.PasswordAccess),
                    Encryption.HashSHA256 (m.PasswordEdit),
                    Encryption.EncryptAES (pStr),
                    Utility.DateTime_ToString (pUp),
                    Encryption.HashSHA256 (ipAddress),
                    Encryption.HashSHA256 (Environment.UserName))));

                req.GetResponse ();
            } catch {
            }
        }
    }
}