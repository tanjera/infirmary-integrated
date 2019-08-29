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
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace II.Server {
    public partial class Servers {
        private List<MySqlConnection> listConnections;
        private string connectionString;

        public Servers () {
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
            }
        }

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
            MySqlConnection c;
            if ((c = Open ()) == null)
                return version;
            MySqlCommand com = c?.CreateCommand ();

            try {
                com.CommandText = "SELECT version FROM versioning ORDER BY accession DESC LIMIT 1";
                MySqlDataReader dr = com.ExecuteReader ();

                if (dr.Read ())
                    version = dr.GetValue (0).ToString ();

                Dispose (c, com, dr);
                return version;
            } catch {
                Close (c);
                return version;
            }
        }

        public Patient Get_PatientMirror (Mirrors m) {
            MySqlConnection c;
            if ((c = Open ()) == null)
                return null;
            MySqlCommand com = c?.CreateCommand ();

            try {
                string s = Utility.HashSHA256 (m.PasswordAccess);
                com.CommandText = String.Format ("SELECT updated, patient FROM mirrors WHERE accession = '{0}' AND key_access = '{1}'",
                    m.Accession, Utility.HashSHA256 (m.PasswordAccess));
                MySqlDataReader dr = com.ExecuteReader ();

                if (!dr.Read () || dr.FieldCount < 2) {
                    Dispose (c, com, dr);
                    return null;
                }

                DateTime serverUpdated = Utility.DateTime_FromString (dr.GetValue (0).ToString ());
                if (DateTime.Compare (serverUpdated, m.PatientUpdated) <= 0) {
                    Dispose (c, com, dr);
                    return null;
                }

                m.ServerQueried = DateTime.UtcNow;
                m.PatientUpdated = serverUpdated;
                Patient p = new Patient ();
                p.Load_Process (Utility.DecryptAES (dr.GetValue (1).ToString ()));

                Dispose (c, com, dr);
                return p;
            } catch {
                Close (c);
                return null;
            }
        }

        public void Post_PatientMirror (Mirrors m, string pStr, DateTime pUp) {
            MySqlConnection c;
            if ((c = Open ()) == null)
                return;
            MySqlCommand com = c?.CreateCommand ();

            try {
                bool rowExists = false;
                com.CommandText = String.Format ("SELECT key_edit FROM mirrors WHERE accession = '{0}'",
                    m.Accession);
                MySqlDataReader dr = com.ExecuteReader ();
                rowExists = dr.Read ();
                if (rowExists && dr.GetValue (0).ToString () != Utility.HashSHA256 (m.PasswordEdit)) {
                    Dispose (c, com, dr);
                    return;
                }
                dr.Close ();

                com = c?.CreateCommand ();
                if (rowExists)
                    com.CommandText =
                        "UPDATE mirrors SET " +
                        "accession = ?accession, key_access = ?key_access, key_edit = ?key_edit, " +
                        "patient = ?patient, updated = ?updated, client_ip = ?client_ip, client_user = ?client_user " +
                        String.Format ("WHERE accession = '{0}'", m.Accession);
                else
                    com.CommandText =
                        "INSERT INTO mirrors " +
                        "(accession, key_access, key_edit, patient, updated, client_ip, client_user) " +
                        "VALUES " +
                        "(?accession, ?key_access, ?key_edit, ?patient, ?updated, ?client_ip, ?client_user)";

                string ipAddress = new WebClient ().DownloadString ("http://icanhazip.com").Trim ();
                com.Parameters.Add ("?accession", MySqlDbType.VarChar).Value = m.Accession;
                com.Parameters.Add ("?key_access", MySqlDbType.VarChar).Value = Utility.HashSHA256 (m.PasswordAccess);
                com.Parameters.Add ("?key_edit", MySqlDbType.VarChar).Value = Utility.HashSHA256 (m.PasswordEdit);
                com.Parameters.Add ("?patient", MySqlDbType.LongText).Value = Utility.EncryptAES (pStr);
                com.Parameters.Add ("?updated", MySqlDbType.VarChar).Value = Utility.DateTime_ToString (pUp);
                com.Parameters.Add ("?client_ip", MySqlDbType.VarChar).Value = Utility.HashSHA256 (ipAddress);
                com.Parameters.Add ("?client_user", MySqlDbType.VarChar).Value = Utility.HashSHA256 (Environment.UserName);

                com.ExecuteNonQuery ();

                Dispose (c, com, dr);
                return;
            } catch {
                Close (c);
                return;
            }
        }
    }
}