using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace II_Simulator {

    public partial class WindowSplash : Window {

        public WindowSplash () {
            InitializeComponent ();
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