using System;
namespace II_Linux.UI
{
    public partial class DeviceECG : Gtk.Window
    {
        public DeviceECG() :
                base(Gtk.WindowType.Toplevel)
        {
            this.Build();
        }
    }
}
