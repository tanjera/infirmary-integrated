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

using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace II.Server {
    public partial class Server {
        private List<MySqlConnection> listConnections;
        private string connectionString;

        public Server () {
            listConnections = new List<MySqlConnection> ();

            connectionString = String.Format ("SERVER={0};DATABASE={1};UID={2};PASSWORD={3};",
                Access.Server.Host,
                Access.Server.Database,
                Access.Server.Uid,
                Access.Server.Password);
        }

        private MySqlConnection Open () {
            try {
                MySqlConnection c = new MySqlConnection (connectionString);
                listConnections.Add (c);
                c.Open ();
                return c;
            } catch {

                // When handling errors, you can your application's response based on the error number.
                // The two most common error numbers when connecting are as follows:
                // 0: Cannot connect to server.
                // 1045: Invalid user name and/or password.
                return null;
            }
        }

        private bool Close (MySqlConnection c) {
            try {
                c.Close ();
                listConnections.Remove (c);
                c.Dispose ();
                return true;
            } catch {
                return false;
            }
        }

        private void Dispose (MySqlConnection c, MySqlCommand com, MySqlDataReader dr) {
            try {
                com?.Dispose ();
                dr?.Close ();
                dr?.Dispose ();
                c.Close ();
                listConnections.Remove (c);
                c.Dispose ();
            } catch {
                return;
            }
        }
        private string PHPArgument (string inc)
            => inc.Replace ("#", "_").Replace ("$", "_");

        public void Post_UsageStatistics () {
            MySqlConnection c;
            if ((c = Open ()) == null)
                return;
            MySqlCommand com = c?.CreateCommand ();

            try {
                string macAddress = "",
                        ipAddress = "";

                NetworkInterface nInterface = NetworkInterface.GetAllNetworkInterfaces ().Where (
                    (o) => (o.NetworkInterfaceType == NetworkInterfaceType.Ethernet || o.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                        && o.OperationalStatus == OperationalStatus.Up)
                    .First ();
                if (nInterface != null) {
                    macAddress = nInterface.GetPhysicalAddress ().ToString ();
                    ipAddress = new WebClient ().DownloadString ("http://icanhazip.com").Trim ();
                }

                com.CommandText = "INSERT INTO usage_statistics" +
                    "(timestamp, ii_version, client_os, client_ip, client_mac, client_user) " +
                    "VALUES" +
                    "(?timestamp, ?ii_version, ?client_os, ?client_ip, ?client_mac, ?client_user)";
                com.Parameters.Add ("?timestamp", MySqlDbType.VarChar).Value = Utility.DateTime_ToString (DateTime.UtcNow);
                com.Parameters.Add ("?ii_version", MySqlDbType.VarChar).Value = Utility.Version;
                com.Parameters.Add ("?client_os", MySqlDbType.VarChar).Value = Environment.OSVersion.VersionString;
                com.Parameters.Add ("?client_ip", MySqlDbType.VarChar).Value = Utility.HashSHA256 (ipAddress);
                com.Parameters.Add ("?client_mac", MySqlDbType.VarChar).Value = Utility.HashSHA256 (macAddress);
                com.Parameters.Add ("?client_user", MySqlDbType.VarChar).Value = Utility.HashSHA256 (Environment.UserName);

                com.ExecuteNonQuery ();
                Dispose (c, com, null);
            } catch {
                Close (c);
            }
        }

        public void Post_Exception (Exception e) {
            MySqlConnection c;
            if ((c = Open ()) == null)
                return;
            MySqlCommand com = c?.CreateCommand ();

            try {
                StringBuilder excData = new StringBuilder ();
                foreach (DictionaryEntry entry in e.Data)
                    excData.AppendLine (String.Format ("{0,-20} '{1}'", entry.Key.ToString (), entry.Value.ToString ()));

                com.CommandText = "INSERT INTO exceptions" +
                    "(timestamp, ii_version, client_os, exception_message, exception_method, exception_stacktrace, exception_hresult, exception_data) " +
                    "VALUES" +
                    "(?timestamp, ?ii_version, ?client_os, ?exception_message, ?exception_method, ?exception_stacktrace, ?exception_hresult, ?exception_data)";
                com.Parameters.Add ("?timestamp", MySqlDbType.VarChar).Value = Utility.DateTime_ToString (DateTime.UtcNow);
                com.Parameters.Add ("?ii_version", MySqlDbType.VarChar).Value = Utility.Version;
                com.Parameters.Add ("?client_os", MySqlDbType.VarChar).Value = Environment.OSVersion.VersionString;
                com.Parameters.Add ("?exception_message", MySqlDbType.VarChar).Value = e.Message ?? "null";
                com.Parameters.Add ("?exception_method", MySqlDbType.VarChar).Value = e.TargetSite?.Name ?? "null";
                com.Parameters.Add ("?exception_stacktrace", MySqlDbType.VarChar).Value = e.StackTrace ?? "null";
                com.Parameters.Add ("?exception_hresult", MySqlDbType.VarChar).Value = e.HResult.ToString () ?? "null";
                com.Parameters.Add ("?exception_data", MySqlDbType.VarChar).Value = excData.ToString () ?? "null";

                com.ExecuteNonQuery ();
                Dispose (c, com, null);
                return;
            } catch {
                Close (c);
            }
        }

        public string Get_LatestVersion () {
            string version = "0.0";

            try {
                WebRequest req = WebRequest.Create ("http://server.infirmary-integrated.com/version.php");
                WebResponse resp = req.GetResponse ();
                Stream str = resp.GetResponseStream ();
                string body = String.Empty;

                using (StreamReader sr = new StreamReader (str))
                    body = sr.ReadLine ().Trim ();

                resp.Close ();
                str.Close ();

                return String.IsNullOrEmpty (body) ? version : body;
            } catch {
                return version;
            }
        }

        public Patient Get_PatientMirror (Mirror m) {
            try {
                WebRequest req = WebRequest.Create (String.Format (
                    "http://server.infirmary-integrated.com/mirror_get.php?accession={0}&accesshash={1}",
                    PHPArgument (m.Accession), Utility.HashSHA256 (m.PasswordAccess)));
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

                DateTime serverUpdated = Utility.DateTime_FromString (updated);
                if (DateTime.Compare (serverUpdated, m.PatientUpdated) <= 0)
                    return null;

                m.ServerQueried = DateTime.UtcNow;
                m.PatientUpdated = serverUpdated;
                Patient p = new Patient ();
                p.Load_Process (Utility.DecryptAES (patient));

                return p;
            } catch (Exception e) {
                return null;
            }
        }

        public void Post_PatientMirror (Mirror m, string pStr, DateTime pUp) {
            try {
                string ipAddress = new WebClient ().DownloadString ("http://icanhazip.com").Trim ();

                WebRequest req = WebRequest.Create (String.Format (
                    "http://server.infirmary-integrated.com/mirror_post.php" +
                    "?accession={0}&key_access={1}&key_edit={2}&patient={3}&updated={4}&client_ip={5}&client_user={6}",
                    PHPArgument (m.Accession),
                    Utility.HashSHA256 (m.PasswordAccess),
                    Utility.HashSHA256 (m.PasswordEdit),
                    Utility.EncryptAES (pStr),
                    Utility.DateTime_ToString (pUp),
                    Utility.HashSHA256 (ipAddress),
                    Utility.HashSHA256 (Environment.UserName)));

                req.GetResponse ();
                return;
            } catch (Exception e) {
                return;
            }
        }
    }
}