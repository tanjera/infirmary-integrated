using System;
using System.Diagnostics;
using System.Reflection;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

using II;
using II.Localization;

namespace IISIM {

    public partial class DialogAbout : Window {
        public App? Instance;

        public DialogAbout () {
            InitializeComponent ();
        }

        public DialogAbout (App? app) {
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

        public void Init () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (Init)}");
                return;
            }

            this.FindControl<Window> ("dlgAbout").Title = Instance.Language.Localize ("ABOUT:AboutProgram");
            this.FindControl<Label> ("lblInfirmaryIntegrated").Content = Instance.Language.Localize ("II:InfirmaryIntegrated");
            this.FindControl<Label> ("lblVersion").Content = String.Format (Instance.Language.Localize ("ABOUT:Version"),
                Assembly.GetExecutingAssembly ()?.GetName ()?.Version?.ToString (3) ?? "0.0.0");
            this.FindControl<TextBlock> ("tblDescription").Text = Instance.Language.Localize ("ABOUT:Description");
        }

        private void Hyperlink_Website (object sender, RoutedEventArgs e)
            => II.InterOp.OpenBrowser ("http://www.infirmary-integrated.com/");

        private void Hyperlink_GitRepo (object sender, RoutedEventArgs e)
            => II.InterOp.OpenBrowser ("https://github.com/tanjera/infirmary-integrated");
    }
}