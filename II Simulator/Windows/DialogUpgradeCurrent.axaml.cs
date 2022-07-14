using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace IISIM {

    public partial class DialogUpgradeCurrent : Window {
        public App? Instance;

        public DialogUpgradeCurrent () {
            InitializeComponent ();
        }

        public DialogUpgradeCurrent (App? app) {
            InitializeComponent ();
#if DEBUG
            this.AttachDevTools ();
#endif

            DataContext = this;
            Instance = app;

            Init ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        private void Init () {
            if (Instance is not null) {
                this.FindControl<Window> ("dlgUpgradeCurrent").Title = Instance.Language.Localize ("UPGRADE:Upgrade");
                this.FindControl<TextBlock> ("txtMessage").Text = Instance.Language.Localize ("UPGRADE:NoUpdateAvailable");
                this.FindControl<Button> ("btnContinue").Content = Instance.Language.Localize ("BUTTON:Continue");
            }
        }

        private void ButtonOK_Click (object s, RoutedEventArgs e) => this.Close ();
    }
}