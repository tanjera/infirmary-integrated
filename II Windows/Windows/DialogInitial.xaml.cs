using II.Localization;
using System;
using System.Windows;

namespace II_Windows {

    /// <summary>
    /// Interaction logic for DialogLanguage.xaml
    /// </summary>
    public partial class DialogInitial : Window {

        public DialogInitial () {
            InitializeComponent ();

            // Populate UI strings per language selection
            dlgLanguage.Title = App.Language.Dictionary ["INITIAL:LanguageAndTerms"];
            lblChooseLanguage.Content = App.Language.Dictionary ["INITIAL:ChooseLanguage"];
            txtAgreeTerms.Text = App.Language.Dictionary ["INITIAL:AgreeToTerms"];
            btnContinue.Content = App.Language.Dictionary ["BUTTON:Continue"];

            cmbLanguages.ItemsSource = II.Localization.Language.Descriptions;
            cmbLanguages.SelectedIndex = (int)II.Localization.Language.Values.ENU;
        }

        private void OnClick_Continue (object sender, RoutedEventArgs e) {
            App.Language.Value = (Language.Values)Enum.GetValues (typeof (Language.Values)).GetValue (cmbLanguages.SelectedIndex);

            Properties.Settings.Default.Language = App.Language.Value.ToString ();
            Properties.Settings.Default.Save ();
            this.Close ();
        }

        private void Hyperlink_RequestNavigate (object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
            => System.Diagnostics.Process.Start (e.Uri.ToString ());
    }
}