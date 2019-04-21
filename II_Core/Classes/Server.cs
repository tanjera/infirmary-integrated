using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;

using MySql.Data.MySqlClient;

namespace II.Server {

    public partial class Connection {

        private List<MySqlConnection> listConnections;
        private string connectionString;

        public Connection() {
            listConnections = new List<MySqlConnection>();

            connectionString = String.Format ("SERVER={0};DATABASE={1};UID={2};PASSWORD={3};",
                accessServer, accessDatabase, accessUid, accessPassword);
        }

        private MySqlConnection Open () {
            try {
                MySqlConnection c = new MySqlConnection(connectionString);
                listConnections.Add(c);
                c.Open();
                return c;
            }
            catch (Exception e) {
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
                listConnections.Remove(c);
                c.Dispose ();
                return true;
            } catch (Exception e) {
                return false;
            }
        }

        public void Send_UsageStatistics () {
            MySqlConnection conn;
            if ((conn = Open ()) == null)
                return;
            MySqlCommand comm = conn?.CreateCommand();

            try {
                string macAddress = "",
                        ipAddress = "";

                NetworkInterface nInterface = NetworkInterface.GetAllNetworkInterfaces ().Where (
                    (o) => (o.NetworkInterfaceType == NetworkInterfaceType.Ethernet || o.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                        && o.OperationalStatus == OperationalStatus.Up)
                    .First ();
                if (nInterface != null) {
                    macAddress = nInterface.GetPhysicalAddress ().ToString ();
                    ipAddress = nInterface.GetIPProperties ().UnicastAddresses.Where (
                        (a) => a.Address.AddressFamily == AddressFamily.InterNetwork)
                        .Select (a => a.Address.ToString ()).First ();
                }

                comm.CommandText = "INSERT INTO usage_statistics" +
                    "(timestamp, ii_version, client_os, client_ip, client_mac, client_user) " +
                    "VALUES" +
                    "(?timestamp, ?ii_version, ?client_os, ?client_ip, ?client_mac, ?client_user)";
                comm.Parameters.Add ("?timestamp", MySqlDbType.VarChar).Value = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                comm.Parameters.Add ("?ii_version", MySqlDbType.VarChar).Value = Utility.Version;
                comm.Parameters.Add ("?client_os", MySqlDbType.VarChar).Value = Environment.OSVersion.VersionString;
                comm.Parameters.Add ("?client_ip", MySqlDbType.VarChar).Value = ipAddress;
                comm.Parameters.Add ("?client_mac", MySqlDbType.VarChar).Value = macAddress;
                comm.Parameters.Add ("?client_user", MySqlDbType.VarChar).Value = Environment.UserName;

                comm.ExecuteNonQuery ();
                Close (conn);
            } catch (Exception e) {
                Close (conn);
            }
        }

        public void Send_Exception(Exception exception) {
            MySqlConnection conn;
            if ((conn = Open ()) == null)
                return;
            MySqlCommand comm = conn?.CreateCommand();

            try {
                StringBuilder excData = new StringBuilder();
                foreach (DictionaryEntry entry in exception.Data)
                    excData.AppendLine(String.Format("{0,-20} '{1}'", entry.Key.ToString(), entry.Value.ToString()));

                comm.CommandText = "INSERT INTO exceptions" +
                    "(timestamp, ii_version, client_os, exception_message, exception_method, exception_stacktrace, exception_hresult, exception_data) " +
                    "VALUES" +
                    "(?timestamp, ?ii_version, ?client_os, ?exception_message, ?exception_method, ?exception_stacktrace, ?exception_hresult, ?exception_data)";
                comm.Parameters.Add("?timestamp", MySqlDbType.VarChar).Value = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                comm.Parameters.Add("?ii_version", MySqlDbType.VarChar).Value = Utility.Version;
                comm.Parameters.Add("?client_os", MySqlDbType.VarChar).Value = Environment.OSVersion.VersionString;
                comm.Parameters.Add("?exception_message", MySqlDbType.VarChar).Value = exception.Message ?? "null";
                comm.Parameters.Add("?exception_method", MySqlDbType.VarChar).Value = exception.TargetSite?.Name ?? "null";
                comm.Parameters.Add("?exception_stacktrace", MySqlDbType.VarChar).Value = exception.StackTrace ?? "null";
                comm.Parameters.Add("?exception_hresult", MySqlDbType.VarChar).Value = exception.HResult.ToString() ?? "null";
                comm.Parameters.Add("?exception_data", MySqlDbType.VarChar).Value = excData.ToString() ?? "null";

                comm.ExecuteNonQuery();
                Close (conn);
            } catch (Exception e) {
                Close (conn);
            }
        }

        public string Get_LatestVersion() {
            string version = "0.0";
            MySqlConnection conn;
            if ((conn = Open()) == null)
                return version;
            MySqlCommand comm = conn?.CreateCommand();

            try {
                comm.CommandText = "SELECT version FROM versioning ORDER BY accession DESC LIMIT 1";
                MySqlDataReader dr = comm.ExecuteReader();

                if (dr.Read())
                    version = dr.GetValue(0).ToString();

                Close(conn);
                return version;
            } catch (Exception e) {
                Close(conn);
                return version;
            }
        }
    }
}
