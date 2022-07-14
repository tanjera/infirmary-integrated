using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

using II;
using II.Localization;

namespace IISIM {

    public partial class DialogLanguage : Window {
        public App? Instance;

        public DialogLanguage () {
            InitializeComponent ();
        }

        public DialogLanguage (App? app) {
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
            // Populate UI strings per language selection
            if (Instance is not null) {
                this.FindControl<Window> ("dlgLanguage").Title = Instance.Language.Localize ("LANGUAGE:Title");
                this.FindControl<Label> ("lblChooseLanguage").Content = Instance.Language.Localize ("LANGUAGE:Select");
                this.FindControl<Button> ("btnContinue").Content = Instance.Language.Localize ("BUTTON:Continue");

                this.FindControl<ComboBox> ("cmbLanguages").Items = II.Localization.Language.Descriptions;
                this.FindControl<ComboBox> ("cmbLanguages").SelectedIndex = (int)II.Localization.Language.Values.ENG;
            }
        }

        private void OnClick_Continue (object sender, RoutedEventArgs e) {
            if (Instance is null)
                return;

            Instance.Language.Value = Enum.GetValues<Language.Values> () [this.FindControl<ComboBox> ("cmbLanguages").SelectedIndex];

            Instance.Settings.Language = Instance.Language.Value.ToString ();
            Instance.Settings.Save ();

            this.Close ();
        }
    }
}