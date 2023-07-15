/* Infirmary Integrated Simulator
 * By Ibi Keller (Tanjera), (c) 2023
 */

using System;
using System.Diagnostics;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;

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
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (Init)}");
                return;
            }

            // Populate UI strings per language selection

            this.FindControl<Window> ("dlgLanguage").Title = Instance.Language.Localize ("LANGUAGE:Title");
            this.FindControl<Label> ("lblChooseLanguage").Content = Instance.Language.Localize ("LANGUAGE:Select");
            this.FindControl<Button> ("btnContinue").Content = Instance.Language.Localize ("BUTTON:Continue");

            this.FindControl<ComboBox> ("cmbLanguages").Items = II.Localization.Language.Descriptions;
            this.FindControl<ComboBox> ("cmbLanguages").SelectedIndex = (int)II.Localization.Language.Values.ENG;
        }

        private void OnClick_Continue (object sender, RoutedEventArgs e) {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (OnClick_Continue)}");
                return;
            }

            Instance.Language.Value = Enum.GetValues<Language.Values> () [this.FindControl<ComboBox> ("cmbLanguages").SelectedIndex];

            Instance.Settings.Language = Instance.Language.Value.ToString ();
            Instance.Settings.Save ();

            // Show messagebox prompting user to restart the program for changes to take effect

            var assets = AvaloniaLocator.Current.GetService<Avalonia.Platform.IAssetLoader> ();
            var icon = new Bitmap (assets.Open (new Uri ("avares://Infirmary Integrated/Third_Party/Icon_DeviceMonitor_128.png")));

            var msBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager
                .GetMessageBoxCustomWindow (new MessageBox.Avalonia.DTO.MessageBoxCustomParamsWithImage {
                    ButtonDefinitions = new [] {
                            new MessageBox.Avalonia.Models.ButtonDefinition {
                                Name = "OK",
                                IsCancel=true}
                    },
                    ContentTitle = Instance.Language.Localize ("MESSAGE:Restart"),
                    ContentMessage = Instance.Language.Localize ("MESSAGE:RestartForChanges"),
                    Icon = icon,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    WindowIcon = this.Icon,
                    ShowInCenter = true,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    Topmost = true,
                    CanResize = false,
                });

            msBoxStandardWindow.Show ();

            this.Close ();
        }
    }
}