/* Infirmary Integrated Simulator
 * By Ibi Keller (Tanjera), (c) 2023
 */

using System;
using System.Diagnostics;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

using II;
using II.Localization;

namespace IISIM {

    public partial class DialogEULA : Window {
        public App? Instance;

        public DialogEULA () {
            InitializeComponent ();
        }

        public DialogEULA (App? app) {
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
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (Init)}");
                return;
            }

            // Populate UI strings per language selection

            this.GetControl<Window> ("dlgEULA").Title = Instance.Language.Localize ("EULA:Title");
            this.GetControl<TextBlock> ("txtAgreeTerms").Text = Instance.Language.Localize ("EULA:AgreeToTerms");
            this.GetControl<Button> ("btnContinue").Content = Instance.Language.Localize ("BUTTON:Continue");
        }

        private void OnClick_Continue (object sender, RoutedEventArgs e) {
            Instance?.Settings.Save ();

            this.Close ();
        }

        private void Hyperlink_Terms (object sender, RoutedEventArgs e)
            => II.InterOp.OpenBrowser ("http://www.infirmary-integrated.com/license-and-data-collection/");
    }
}