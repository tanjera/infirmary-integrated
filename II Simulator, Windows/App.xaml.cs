
using System;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Navigation;
using II;
using II.Localization;
using II.Server;


namespace IISIM {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>

    public partial class App : Application {
        public string []? StartArgs;

        public II.Settings.Simulator Settings = new ();
        public Server Server = new ();
        public Mirror Mirror = new ();
        public Language Language = new ();

        public Scenario? Scenario;
        public Physiology? Physiology { get => Scenario?.Physiology; }


        public IISIM.Windows.Splash? Window_Splash;
        public IISIM.Windows.Control? Window_Control;

        /* TODO         
        public DeviceMonitor? Device_Monitor;
        public DeviceECG? Device_ECG;
        public DeviceDefib? Device_Defib;
        public DeviceIABP? Device_IABP;
        public DeviceEFM? Device_EFM;
        */

        public System.Timers.Timer Timer_Main = new ();

        void Init (object sender, StartupEventArgs e) {
            Timer_Main.Interval = 10; // q 10 milliseconds
            Timer_Main.Start ();

            II.File.Init ();                                            // Init file structure (for config file, temp files)
            Settings.Load ();                                           // Load config file
            Language = new (Settings.Language);                         // Load localization dictionary based on settings

            StartArgs = e.Args;


            int splashTimeout= 2000;                     // Splash screen for 2 seconds for Release version
#if DEBUG                                       
            splashTimeout = 200;                         // Shorten splash screen for debug builds; same logic flow though
#endif

            Window_Splash = new Windows.Splash ();
            App.Current.Dispatcher.InvokeAsync (new Action (() => {
                MainWindow = Window_Splash;
                Window_Splash.Show ();
            }));
            
            App.Current.Dispatcher.InvokeAsync (new Action (() => {
                Thread.Sleep (splashTimeout);

                Window_Control = new Windows.Control ();
                Window_Splash.Close ();
                MainWindow = Window_Control;
                Window_Control.Show ();
            }));            
        }
    }
}
