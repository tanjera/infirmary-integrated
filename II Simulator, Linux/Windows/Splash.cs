using System;
using System.Diagnostics;
using System.Threading;
using Gdk;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;
using Window = Gtk.Window;

namespace IISIM
{
    class Splash : Window {
        private App Instance;
        
        [UI] private Image imgSplash = null;

        public Splash(App inst) : this(inst, new Builder("Splash.glade")) { }

        private Splash(App inst, Builder builder) : base(builder.GetRawOwnedObject("wdwSplash")) {
            Instance = inst;
            
            builder.Autoconnect(this);

            imgSplash.Pixbuf = new Pixbuf("Resources/SplashScreen_Infirmary_800x480.png");
        }
    }
}