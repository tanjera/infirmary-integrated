using System;
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

    public partial class DialogMirrorReceive : Window {
        public App? Instance;

        public DialogMirrorReceive () {
            InitializeComponent ();
        }

        public DialogMirrorReceive (App? app) {
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
            this.FindControl<Window> ("dlgMirrorReceive").Title = Instance.Language.Localize ("MIRROR:ReceiveTitle");
            this.FindControl<TextBlock> ("txtMessage").Text = Instance.Language.Localize ("MIRROR:EnterSettings");

            this.FindControl<TextBlock> ("txtAccessionKey").Text = Instance.Language.Localize ("MIRROR:AccessionKey");
            this.FindControl<TextBlock> ("txtAccessPassword").Text = Instance.Language.Localize ("MIRROR:AccessPassword");

            this.FindControl<Button> ("btnContinue").Content = Instance.Language.Localize ("BUTTON:Continue");
        }

        private void OnClick_Continue (object sender, RoutedEventArgs e) {
            Regex regex = new ("^[a-zA-Z0-9]*$");
            if ((this.FindControl<TextBox> ("tbAccessionKey").Text ?? "").Length > 0
                    && regex.IsMatch (this.FindControl<TextBox> ("tbAccessionKey").Text)) {
                Instance.Mirror.Status = II.Server.Mirror.Statuses.CLIENT;
                Instance.Mirror.Accession = this.FindControl<TextBox> ("tbAccessionKey").Text ?? "";
                Instance.Mirror.PasswordAccess = this.FindControl<TextBox> ("tbAccessPassword").Text ?? "";

                this.Close ();
            } else {
                var assets = AvaloniaLocator.Current.GetService<Avalonia.Platform.IAssetLoader> ();
                var icon = new Bitmap (assets.Open (new Uri ("avares://Infirmary Integrated/Third_Party/Icon_DeviceMonitor_48.png")));

                var msBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager
                    .GetMessageBoxCustomWindow (new MessageBox.Avalonia.DTO.MessageBoxCustomParamsWithImage {
                        ButtonDefinitions = new [] {
                            new MessageBox.Avalonia.Models.ButtonDefinition {
                                Name = "OK",
                                Type = MessageBox.Avalonia.Enums.ButtonType.Default,
                                IsCancel=true}
                        },
                        ContentTitle = Instance.Language.Localize ("MIRROR:ReceiveTitle"),
                        ContentMessage = Instance.Language.Localize ("MIRROR:SettingsInvalid"),
                        Icon = icon,
                        Style = MessageBox.Avalonia.Enums.Style.None,
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