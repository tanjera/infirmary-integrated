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

using IISIM.Windows;

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

        public Splash? Window_Splash;
        public Control? Window_Control;

        public DeviceMonitor? Device_Monitor;

        public DeviceECG? Device_ECG;
        public DeviceDefib? Device_Defib;
        public DeviceIABP? Device_IABP;
        public DeviceEFM? Device_EFM;

        public System.Timers.Timer Timer_Main = new ();

        private void Init (object sender, StartupEventArgs e) {
            Timer_Main.Interval = 10; // q 10 milliseconds
            Timer_Main.Start ();

            II.File.Init ();                                            // Init file structure (for config file, temp files)
            Settings.Load ();                                           // Load config file
            Language = new (Settings.Language);                         // Load localization dictionary based on settings

            StartArgs = e.Args;

            int splashTimeout = 2000;                     // Splash screen for 2 seconds for Release version
#if DEBUG
            splashTimeout = 200;                         // Shorten splash screen for debug builds; same logic flow though
#endif

            App.Current.Dispatcher.InvokeAsync (async () => {
                Window_Splash = new Windows.Splash ();
                MainWindow = Window_Splash;
                Window_Splash.Show ();

                await Task.Delay (splashTimeout);

                Window_Control = new Windows.Control ();
                Window_Splash.Close ();
                MainWindow = Window_Control;
                Window_Control.Show ();
            });
        }
    }
}