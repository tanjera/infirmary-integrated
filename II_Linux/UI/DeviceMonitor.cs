using System;
namespace II_Linux.UI
{
    public partial class DeviceMonitor : Gtk.Window
    {
        public DeviceMonitor() :
                base(Gtk.WindowType.Toplevel)
        {
            this.Build();
        }
    }
}
