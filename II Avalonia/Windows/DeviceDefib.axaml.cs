using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace II_Avalonia {
    public partial class DeviceDefib : Window {
        public DeviceDefib () {
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
