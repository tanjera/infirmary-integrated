using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;

using MySql.Data.MySqlClient;

namespace II.Server {

    public partial class Connection {

        private MySqlConnection connection;

        public Connection() {
            connection = new MySqlConnection (
                String.Format ("SERVER={0};DATABASE={1};UID={2};PASSWORD={3};",
                accessServer, accessDatabase, accessUid, accessPassword));
        }

        private bool Open () {
            try {
                connection.Open();
                return true;
            }
            catch (MySqlException e) {
                // When handling errors, you can your application's response based on the error number.
                // The two most common error numbers when connecting are as follows:
                // 0: Cannot connect to server.
                // 1045: Invalid user name and/or password.
                return false;
            }
        }

        private bool Close () {
            try {
                connection.Close ();
                return true;
            } catch (MySqlException e) {
                return false;
            }
        }

        public void UsageStatistics_Send () {
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

                MySqlCommand c = connection.CreateCommand ();
                c.CommandText = "INSERT INTO usage_statistics(timestamp, ii_version, client_os, client_ip, client_mac, client_user) " +
                    "VALUES(?timestamp, ?ii_version, ?client_os, ?client_ip, ?client_mac, ?client_user)";
                c.Parameters.Add ("?timestamp", MySqlDbType.VarChar).Value = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                c.Parameters.Add ("?ii_version", MySqlDbType.VarChar).Value = Utility.Version;
                c.Parameters.Add ("?client_os", MySqlDbType.VarChar).Value = Environment.OSVersion.VersionString;
                c.Parameters.Add ("?client_ip", MySqlDbType.VarChar).Value = ipAddress;
                c.Parameters.Add ("?client_mac", MySqlDbType.VarChar).Value = macAddress;
                c.Parameters.Add ("?client_user", MySqlDbType.VarChar).Value = Environment.UserName;

                connection.Open ();
                c.ExecuteNonQuery ();
                connection.Close ();
            } catch (MySqlException e) {
                connection.Close ();
            }
        }
    }
}
