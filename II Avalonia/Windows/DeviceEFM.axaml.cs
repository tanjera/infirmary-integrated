using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace II_Avalonia {
    public partial class DeviceEFM : Window {
        public DeviceEFM () {
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
