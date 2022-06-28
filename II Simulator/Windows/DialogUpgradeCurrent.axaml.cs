using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace II_Avalonia {

    public partial class DialogUpgradeCurrent : Window {

        public DialogUpgradeCurrent () {
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
            this.FindControl<Window> ("dlgUpgradeCurrent").Title = App.Language.Localize ("UPGRADE:Upgrade");
            this.FindControl<TextBlock> ("txtMessage").Text = App.Language.Localize ("UPGRADE:NoUpdateAvailable");
            this.FindControl<Button> ("btnContinue").Content = App.Language.Localize ("BUTTON:Continue");
        }

        private void ButtonOK_Click (object s, RoutedEventArgs e) => this.Close ();
    }
}