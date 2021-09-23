using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

using II;
using II.Localization;

namespace II_Avalonia {

    public partial class DialogInitial : Window {

        public DialogInitial () {
            InitializeComponent ();
#if DEBUG
            this.AttachDevTools ();
#endif

            Init ();
        }

        /* Properties for applying DPI scaling options */
        public double UIScale { get { return App.Settings.UIScale; } }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        private void Init () {
            DataContext = this;

            this.Width *= UIScale;
            this.Height *= UIScale;

            // Populate UI strings per language selection
            this.FindControl<Window> ("dlgLanguage").Title = App.Language.Localize ("INITIAL:LanguageAndTerms");
            this.FindControl<Label> ("lblChooseLanguage").Content = App.Language.Localize ("INITIAL:ChooseLanguage");
            this.FindControl<TextBlock> ("txtAgreeTerms").Text = App.Language.Localize ("INITIAL:AgreeToTerms");
            this.FindControl<Button> ("btnContinue").Content = App.Language.Localize ("BUTTON:Continue");

            this.FindControl<ComboBox> ("cmbLanguages").Items = II.Localization.Language.Descriptions;
            this.FindControl<ComboBox> ("cmbLanguages").SelectedIndex = (int)II.Localization.Language.Values.ENG;
        }

        private void OnClick_Continue (object sender, RoutedEventArgs e) {
            App.Language.Value = Enum.GetValues<Language.Values> () [this.FindControl<ComboBox> ("cmbLanguages").SelectedIndex];

            App.Settings.Language = App.Language.Value.ToString ();
            App.Settings.Save ();
            this.Close ();
        }

        private void Hyperlink_Terms (object sender, RoutedEventArgs e)
            => II.InterOp.OpenBrowser ("http://www.infirmary-integrated.com/license-and-data-collection/");
    }
}