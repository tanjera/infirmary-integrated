using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace II_Avalonia {
    public partial class DeviceECG : Window {
        public DeviceECG () {
            InitializeComponent ();
#if DEBUG
            this.AttachDevTools ();
#endif
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }
    }
}
