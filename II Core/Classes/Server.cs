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
using System.Text;

using II.Localization;

namespace II.Server {
    public partial class Server {
        /* Variables for checking for updates, running bootstrapper */
        public string UpgradeVersion = String.Empty;
        public string UpgradeWebpage = String.Empty;
        public string BootstrapExeUri = String.Empty;
        public string BootstrapHashMd5 = String.Empty;

        private string FormatForPHP (string inc)
            => inc.Replace ("#", "_").Replace ("$", "_");

        private string GetCountryCode (string ipAddress) {
            try {
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

                if (!body.Contains ("country_code"))
                    return "--";

                resp.Close ();
                str.Close ();

                resp.Dispose ();
                str.Dispose ();

                int start = body.IndexOf (": \"") + 3;
                int length = body.IndexOf ("\",") - start;

                return body.Substring (start, length);
            } catch {
                return "--";
            }
        }

        public void Post_UsageStatistics (Language appLanguage) {
            try {
                string macAddress = "",
                        ipAddress = "";

                NetworkInterface nInterface = NetworkInterface.GetAllNetworkInterfaces ().Where (
                    (o) => (o.NetworkInterfaceType == NetworkInterfaceType.Ethernet || o.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                        && o.OperationalStatus == OperationalStatus.Up)
                    .First ();
                if (nInterface != null) {
                    macAddress = nInterface.GetPhysicalAddress ().ToString ();
                    ipAddress = new WebClient ().DownloadString ("http://ipv4.icanhazip.com/").Trim ();
                }

                CultureInfo ci = CultureInfo.CurrentUICulture;

                WebRequest req = WebRequest.Create (FormatForPHP (String.Format (
                    "http://server.infirmary-integrated.com/usage_post.php" +
                        "?timestamp={0}&ii_version={1}&env_os={2}&env_lang={3}&client_lang={4}&client_country={5}"
                        + "&client_ip={6}&client_mac={7}&client_user={8}",

                    Utility.DateTime_ToString (DateTime.UtcNow),
                    Utility.Version,
                    Environment.OSVersion.VersionString,
                    ci.ThreeLetterWindowsLanguageName,
                    appLanguage.Value.ToString (),
                    GetCountryCode (ipAddress),
                    Encryption.HashSHA256 (ipAddress),
                    Encryption.HashSHA256 (macAddress),
                    Encryption.HashSHA256 (Environment.UserName)
                    )));

                req.GetResponse ();
            } catch {
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
                    Utility.Version,
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