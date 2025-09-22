using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Gtk;

namespace IISIM
{
    class App
    {
        public Gtk.Application Application;
        public string [] StartArgs;

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
            
            var wdwSplash = new Splash(this);
            Application.AddWindow(wdwSplash);
            wdwSplash.Show();
            
            var wdwControl = new Control(this);
            Application.AddWindow(wdwControl);
            
            int splashTimeout = 2000;                     // Splash screen for 2 seconds for Release version
#if DEBUG
            splashTimeout = 200;                          // Shorten splash screen for debug builds; same logic flow though
#endif
            
            Thread thr = new Thread (() => {
                Thread.Sleep (splashTimeout);
                wdwSplash.Close();
                
                wdwControl.Show();
            });
            
            thr.Start();
            Application.Run();
        }
    }
}
