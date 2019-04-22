using System;
namespace II_Linux.UI
{
    public partial class DialogAbout : Gtk.Window
    {
        public DialogAbout() :
                base(Gtk.WindowType.Toplevel)
        {
            this.Build();
        }
    }
}
