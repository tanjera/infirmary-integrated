using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace IISE {

    public partial class WindowSplash : Window {

        public WindowSplash () {
            InitializeComponent ();
#if DEBUG
            this.AttachDevTools ();
#endif

            Init ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        private void Init () {
            DataContext = this;
        }
    }
}