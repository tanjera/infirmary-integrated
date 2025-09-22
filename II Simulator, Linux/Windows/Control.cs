using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace IISIM
{
    class Control : Window
    {
        private App Instance;
        public Control(App inst) : this(inst, new Builder("Control.glade")) { }

        private Control(App inst, Builder builder) : base(builder.GetRawOwnedObject("wdwControl")) {
            Instance = inst;
            
            builder.Autoconnect(this);

            DeleteEvent += Window_DeleteEvent;
            this.Shown += Init;

        }

        private void Init (object sender, EventArgs args) {



        }
        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }
    }
}