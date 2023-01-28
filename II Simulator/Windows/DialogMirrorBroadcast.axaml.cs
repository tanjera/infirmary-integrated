using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;

using II;
using II.Localization;

namespace IISIM {

    public partial class DialogMirrorBroadcast : Window {
        public App? Instance;

        public DialogMirrorBroadcast () {
            InitializeComponent ();
        }

        public DialogMirrorBroadcast (App? app) {
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

            this.FindControl<Window> ("dlgMirrorBroadcast").Title = Instance.Language.Localize ("MIRROR:BroadcastTitle");
            this.FindControl<TextBlock> ("txtMessage").Text = Instance.Language.Localize ("MIRROR:EnterSettings");

            this.FindControl<TextBlock> ("txtAccessionKey").Text = Instance.Language.Localize ("MIRROR:AccessionKey");
            this.FindControl<TextBlock> ("txtAccessPassword").Text = Instance.Language.Localize ("MIRROR:AccessPassword");
            this.FindControl<TextBlock> ("txtAdminPassword").Text = Instance.Language.Localize ("MIRROR:AdminPassword");

            this.FindControl<Button> ("btnContinue").Content = Instance.Language.Localize ("BUTTON:Continue");
        }

        private void ButtonGenerateAccessionKey_Click (object sender, RoutedEventArgs e)
            => this.FindControl<TextBox> ("tbAccessionKey").Text = Utility.RandomString (8);

        private void ButtonGenerateAccessPassword_Click (object sender, RoutedEventArgs e)
            => this.FindControl<TextBox> ("tbAccessPassword").Text = Utility.RandomString (8);

        private void OnClick_Continue (object sender, RoutedEventArgs e) {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (OnClick_Continue)}");
                return;
            }

            Regex regex = new ("^[a-zA-Z0-9]*$");
            if ((this.FindControl<TextBox> ("tbAccessionKey").Text ?? "").Length > 0
                    && regex.IsMatch (this.FindControl<TextBox> ("tbAccessionKey").Text)) {
                Instance.Mirror.Status = II.Server.Mirror.Statuses.HOST;
                Instance.Mirror.Accession = this.FindControl<TextBox> ("tbAccessionKey").Text ?? "";
                Instance.Mirror.PasswordAccess = this.FindControl<TextBox> ("tbAccessPassword").Text ?? "";
                Instance.Mirror.PasswordEdit = this.FindControl<TextBox> ("tbAdminPassword").Text ?? "";

                this.Close ();
            } else {
                var assets = AvaloniaLocator.Current.GetService<Avalonia.Platform.IAssetLoader> ();
                var icon = new Bitmap (assets.Open (new Uri ("avares://Infirmary Integrated/Third_Party/Icon_DeviceMonitor_128.png")));

                var msBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager
                    .GetMessageBoxCustomWindow (new MessageBox.Avalonia.DTO.MessageBoxCustomParamsWithImage {
                        ButtonDefinitions = new [] {
                            new MessageBox.Avalonia.Models.ButtonDefinition {
                                Name = "OK",
                                IsCancel=true}
                        },
                        ContentTitle = Instance?.Language.Localize ("MIRROR:BroadcastTitle"),
                        ContentMessage = Instance?.Language.Localize ("MIRROR:SettingsInvalid"),
                        Icon = icon,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        WindowIcon = this.Icon,
                        Topmost = true,
                        CanResize = false,
                    });

                msBoxStandardWindow.Show ();
            }
        }
    }
}