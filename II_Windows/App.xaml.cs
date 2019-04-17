using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

using II;

namespace II_Windows {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {

        public static string [] Start_Args;

        public static II.Server.Connection Server_Connection = new II.Server.Connection ();
        public static II.Localization.Languages Language = new II.Localization.Languages();

        public static Patient Patient;

        public static PatientEditor Patient_Editor;

        public static DeviceMonitor Device_Monitor;
        public static DeviceECG Device_ECG;
        public static DeviceDefib Device_Defib;
        public static DeviceIABP Device_IABP;

        public static DialogAbout Dialog_About;
        public static DialogInitial Dialog_Language;

        public static DispatcherTimer Timer_Main = new DispatcherTimer();

        private void App_Startup (object sender, StartupEventArgs e) {
            Start_Args = e.Args;

            Timer_Main.Interval = new TimeSpan (100000); // q 10 milliseconds
            Timer_Main.Start ();

            // Send usage statistics to server in background
            BackgroundWorker bgw = new BackgroundWorker ();
            bgw.DoWork += delegate { Server_Connection.UsageStatistics_Send (); };
            bgw.RunWorkerAsync ();
        }
    }
}
