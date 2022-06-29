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

namespace II_Simulator {

    public partial class DialogMirrorReceive : Window {

        public DialogMirrorReceive () {
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
            this.FindControl<Window> ("dlgMirrorReceive").Title = App.Language.Localize ("MIRROR:ReceiveTitle");
            this.FindControl<TextBlock> ("txtMessage").Text = App.Language.Localize ("MIRROR:EnterSettings");

            this.FindControl<TextBlock> ("txtAccessionKey").Text = App.Language.Localize ("MIRROR:AccessionKey");
            this.FindControl<TextBlock> ("txtAccessPassword").Text = App.Language.Localize ("MIRROR:AccessPassword");

            this.FindControl<Button> ("btnContinue").Content = App.Language.Localize ("BUTTON:Continue");
        }

        private void OnClick_Continue (object sender, RoutedEventArgs e) {
            Regex regex = new Regex ("^[a-zA-Z0-9]*$");
            if ((this.FindControl<TextBox> ("tbAccessionKey").Text ?? "").Length > 0
                    && regex.IsMatch (this.FindControl<TextBox> ("tbAccessionKey").Text)) {
                App.Mirror.Status = II.Server.Mirror.Statuses.CLIENT;
                App.Mirror.Accession = this.FindControl<TextBox> ("tbAccessionKey").Text ?? "";
                App.Mirror.PasswordAccess = this.FindControl<TextBox> ("tbAccessPassword").Text ?? "";

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
                        ContentTitle = App.Language.Localize ("MIRROR:ReceiveTitle"),
                        ContentMessage = App.Language.Localize ("MIRROR:SettingsInvalid"),
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