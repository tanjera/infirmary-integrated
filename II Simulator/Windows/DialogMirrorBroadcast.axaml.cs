/* Infirmary Integrated Simulator
 * By Ibi Keller (Tanjera), (c) 2023
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using II;
using II.Localization;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Models;

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

            this.GetControl<Window> ("dlgMirrorBroadcast").Title = Instance.Language.Localize ("MIRROR:BroadcastTitle");
            this.GetControl<TextBlock> ("txtMessage").Text = Instance.Language.Localize ("MIRROR:EnterSettings");

            this.GetControl<TextBlock> ("txtServerAddress").Text = Instance.Language.Localize ("MIRROR:ServerAddress");
            this.GetControl<TextBlock> ("txtAccessionKey").Text = Instance.Language.Localize ("MIRROR:AccessionKey");
            this.GetControl<TextBlock> ("txtAccessPassword").Text = Instance.Language.Localize ("MIRROR:AccessPassword");
            this.GetControl<TextBlock> ("txtAdminPassword").Text = Instance.Language.Localize ("MIRROR:AdminPassword");

            this.GetControl<TextBox> ("tbServerAddress").Text = Instance.Mirror.ServerAddress;
            this.GetControl<TextBox> ("tbServerAddress").Watermark = II.Server.Mirror.DefaultServer;

            this.GetControl<Button> ("btnContinue").Content = Instance.Language.Localize ("BUTTON:Continue");
        }

        private void ButtonGenerateAccessionKey_Click (object sender, RoutedEventArgs e)
            => this.GetControl<TextBox> ("tbAccessionKey").Text = Utility.RandomString (8);

        private void ButtonGenerateAccessPassword_Click (object sender, RoutedEventArgs e)
            => this.GetControl<TextBox> ("tbAccessPassword").Text = Utility.RandomString (8);
        
        private async Task SetMirrorBroadcast () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (OnClick_Continue)}");
                return;
            }

            Regex regex = new ("^[a-zA-Z0-9]*$");
            if (this.GetControl<TextBox> ("tbServerAddress").Text?.Length > 0
                && this.GetControl<TextBox> ("tbAccessionKey").Text?.Length > 0
                    && regex.IsMatch (this.GetControl<TextBox> ("tbAccessionKey")?.Text ?? "")) {
                Instance.Mirror.Status = II.Server.Mirror.Statuses.HOST;
                Instance.Mirror.ServerAddress =
                    this.GetControl<TextBox> ("tbServerAddress").Text ?? II.Server.Mirror.DefaultServer;
                Instance.Mirror.Accession = this.GetControl<TextBox> ("tbAccessionKey").Text ?? "";
                Instance.Mirror.PasswordAccess = this.GetControl<TextBox> ("tbAccessPassword").Text ?? "";
                Instance.Mirror.PasswordEdit = this.GetControl<TextBox> ("tbAdminPassword").Text ?? "";

                this.Close ();
            } else {
                await Dispatcher.UIThread.InvokeAsync (async () => {
                    DialogMessage dlg = new (Instance) {
                        Message = Instance.Language.Localize ("MIRROR:SettingsInvalid"),
                        Title = Instance.Language.Localize ("MIRROR:BroadcastTitle"),
                        Indicator = DialogMessage.Indicators.Error,
                        Option = DialogMessage.Options.OK,
                    };

                    if (!this.IsVisible)                    // Avalonia's parent must be visible to attach a window
                        this.Show ();

                    await dlg.AsyncShow (this);
                });
            }
        }
        
        private void OnClick_Continue (object sender, RoutedEventArgs e) 
            => _ = SetMirrorBroadcast ();
    }
}