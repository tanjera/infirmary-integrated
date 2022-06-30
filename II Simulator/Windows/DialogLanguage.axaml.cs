using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

using II;
using II.Localization;

namespace IISIM {

    public partial class DialogLanguage : Window {

        public DialogLanguage () {
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
            this.FindControl<Window> ("dlgLanguage").Title = App.Language.Localize ("LANGUAGE:Title");
            this.FindControl<Label> ("lblChooseLanguage").Content = App.Language.Localize ("LANGUAGE:Select");
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
    }
}