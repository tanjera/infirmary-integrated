using II;
using II.Localization;
using II.Server;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace II_Windows {

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        public static string [] Start_Args;

        public static Servers Server = new Servers ();
        public static Mirrors Mirror = new Mirrors ();
        public static Languages Language = new Languages ();

        public static Patient Patient;

        public static PatientEditor Patient_Editor;

        public static DeviceMonitor Device_Monitor;
        public static DeviceECG Device_ECG;
        public static DeviceDefib Device_Defib;
        public static DeviceIABP Device_IABP;

        public static DialogAbout Dialog_About;
        public static DialogInitial Dialog_Language;

        // Windows' thread-safe timer; if ported, will need OS equivalent
        public static DispatcherTimer Timer_Main = new DispatcherTimer ();

        private void App_Startup (object sender, StartupEventArgs e) {
            Start_Args = e.Args;

            Timer_Main.Interval = new TimeSpan (100000); // q 10 milliseconds
            Timer_Main.Start ();

            // Send usage statistics to server in background
            BackgroundWorker bgw = new BackgroundWorker ();
            bgw.DoWork += delegate { Server.Post_UsageStatistics (); };
            bgw.RunWorkerAsync ();
        }
    }
}