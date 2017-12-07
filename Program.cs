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
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Dialog_Main = new Forms.Dialog_Main ();
            Application.Run(Dialog_Main);
        }
    }
}
