using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Gtk;

using II;
using II.Localization;
using II.Server;

namespace IISIM
{
    class App
    {
        public Gtk.Application Application;

        public string [] StartArgs;
        
        public II.Settings.Simulator Settings = new ();
        public Server Server = new ();
        public Mirror Mirror = new ();
        public Language Language = new ();

        public Scenario? Scenario;
        public Physiology? Physiology { get => Scenario?.Physiology; }
        
        /* TODO? Implementation comparison against avlts branch:
         * Need to declare UI elements for referencing from child functions later?
         * Including: Window_Splash, Window_Main, Device_Monitor, Device_ECG, Device_Defib,
         * Device_IABP, and Device_EFM?
        */
        
        public System.Timers.Timer Timer_Main = new ();
        
        public static void Main (string [] args) {
            App app = new App ();

            app.Run (args);
        }
        private void Run(string[] args)
        {
            Application.Init();

            Application = new Application("org.tanjera.infirmary-integrated", GLib.ApplicationFlags.None);
            Application.Register(GLib.Cancellable.Current);
            
            StartArgs = args;
            
            II.File.Init ();                                            // Init file structure (for config file, temp files)
            Settings.Load ();                                           // Load config file
            Language = new (Settings.Language);                 // Load localization dictionary based on settings
            
            var wdwSplash = new Splash (this);
            var wdwControl = new Control(this, wdwSplash);
            
            Application.AddWindow(wdwControl);
            wdwControl.Show();
            
            wdwSplash.TransientFor = wdwControl;
            Application.AddWindow(wdwSplash);
            wdwSplash.Show();
            
            int splashTimeout = 2000;                     // Splash screen for 2 seconds for Release version
#if DEBUG
            splashTimeout = 500;                          // Shorten splash screen for debug builds; same logic flow though
#endif
            
            Thread thr = new Thread (() => {
                Thread.Sleep (splashTimeout);
                wdwSplash.Close();
                Application.RemoveWindow (wdwSplash);
            });
            
            Timer_Main.Interval = 10; // q 10 milliseconds
            Timer_Main.Start ();

            thr.Start();
            
            Application.Run();
        }
    }
}
