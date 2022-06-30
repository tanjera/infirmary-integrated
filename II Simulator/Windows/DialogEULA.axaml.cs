using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

using II;
using II.Localization;

namespace IISIM {

    public partial class DialogEULA : Window {

        public DialogEULA () {
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

            // Populate UI strings per language selection
            this.FindControl<Window> ("dlgEULA").Title = App.Language.Localize ("EULA:Title");
            this.FindControl<TextBlock> ("txtAgreeTerms").Text = App.Language.Localize ("EULA:AgreeToTerms");
            this.FindControl<Button> ("btnContinue").Content = App.Language.Localize ("BUTTON:Continue");
        }

        private void OnClick_Continue (object sender, RoutedEventArgs e) {
            App.Settings.Save ();

            this.Close ();
        }

        private void Hyperlink_Terms (object sender, RoutedEventArgs e)
            => II.InterOp.OpenBrowser ("http://www.infirmary-integrated.com/license-and-data-collection/");
    }
}