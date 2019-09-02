using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace II.Access {
    public static class Server {
        public static string                    // Data for compiling the MySQL connection string
            Host = "mysql.hostname.com",        // MySQL hostname
            Database = "mysql_database_name",   // MySQL database name
            Uid = "userid",                     // MySQL username
            Password = "password";              // MySQL password
    }

    public static class Encryption {
        private static string keyString = "01234567890123456789012345678901";       // A 32 character encryption key
        public static byte [] Key { get { return Encoding.UTF8.GetBytes (keyString); } }
        public static byte [] IV { get { return Encoding.UTF8.GetBytes (keyString).Take(16).ToArray(); } }
    }
}
