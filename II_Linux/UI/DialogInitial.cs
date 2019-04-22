using System;
namespace II_Linux.UI
{
    public partial class DialogInitial : Gtk.Window
    {
        public DialogInitial() :
                base(Gtk.WindowType.Toplevel)
        {
            this.Build();
        }
    }
}
