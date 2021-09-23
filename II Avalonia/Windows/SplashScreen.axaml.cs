using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace II_Avalonia {

    public partial class SplashScreen : Window {

        public SplashScreen () {
            InitializeComponent ();
#if DEBUG
            this.AttachDevTools ();
#endif

            Init ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public double UIScale { get { return App.Settings.UIScale; } }

        private void Init () {
            DataContext = this;

            this.Width *= UIScale;
            this.Height *= UIScale;
        }
    }
}