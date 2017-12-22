using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace II
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>


        public static Forms.Dialog_Main Dialog_Main;
        public static Forms.Device_Monitor Device_Monitor;

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Ensure EULA has been accepted!
            if (Properties.Settings.Default.LicenseAccepted != true) {
                Forms.Dialog_License dLicense = new Forms.Dialog_License ();
                DialogResult dr = dLicense.ShowDialog ();
                if (dr == DialogResult.OK) {
                    Properties.Settings.Default.LicenseAccepted = true;
                    Properties.Settings.Default.Save ();
                } else {
                    Application.Exit ();
                    return;
                }
            }

            Dialog_Main = new Forms.Dialog_Main (args);
            Application.Run(Dialog_Main);
        }
    }
}
