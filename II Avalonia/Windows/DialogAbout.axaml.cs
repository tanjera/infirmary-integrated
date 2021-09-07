using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

using II;
using II.Localization;

namespace II_Avalonia {

    public partial class DialogAbout : Window {

        public DialogAbout () {
            InitializeComponent ();
#if DEBUG
            this.AttachDevTools ();
#endif

            Init ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public void Init () {
            // Populate UI strings per language selection
            Language.Values l = App.Language.Value;

            this.FindControl<Window> ("dlgAbout").Title = App.Language.Localize ("ABOUT:AboutProgram");
            this.FindControl<Label> ("lblInfirmaryIntegrated").Content = App.Language.Localize ("II:InfirmaryIntegrated");
            this.FindControl<Label> ("lblVersion").Content = String.Format (App.Language.Localize ("ABOUT:Version"), II.Utility.Version);
            this.FindControl<TextBlock> ("tblDescription").Text = App.Language.Localize ("ABOUT:Description");
        }

        private void Hyperlink_Website (object sender, RoutedEventArgs e)
            => II.InterOp.OpenBrowser ("http://www.infirmary-integrated.com/");

        private void Hyperlink_GitRepo (object sender, RoutedEventArgs e)
            => II.InterOp.OpenBrowser ("https://github.com/tanjera/infirmary-integrated");
    }
}