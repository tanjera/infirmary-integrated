/* Infirmary Integrated Simulator
 * By Ibi Keller (Tanjera), (c) 2023
 */

using System;
using System.Collections.Generic;
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
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Models;

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
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (Init)}");
                return;
            }

            // Populate UI strings per language selection
            this.GetControl<Window> ("dlgMirrorReceive").Title = Instance.Language.Localize ("MIRROR:ReceiveTitle");
            this.GetControl<TextBlock> ("txtMessage").Text = Instance.Language.Localize ("MIRROR:EnterSettings");

            this.GetControl<TextBlock> ("txtAccessionKey").Text = Instance.Language.Localize ("MIRROR:AccessionKey");
            this.GetControl<TextBlock> ("txtAccessPassword").Text = Instance.Language.Localize ("MIRROR:AccessPassword");

            this.GetControl<Button> ("btnContinue").Content = Instance.Language.Localize ("BUTTON:Continue");
        }

        private void OnClick_Continue (object sender, RoutedEventArgs e) {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (OnClick_Continue)}");
                return;
            }

            Regex regex = new ("^[a-zA-Z0-9]*$");
            if ((this.GetControl<TextBox> ("tbAccessionKey").Text ?? "").Length > 0
                    && regex.IsMatch (this.GetControl<TextBox> ("tbAccessionKey").Text)) {
                Instance.Mirror.Status = II.Server.Mirror.Statuses.CLIENT;
                Instance.Mirror.Accession = this.GetControl<TextBox> ("tbAccessionKey").Text ?? "";
                Instance.Mirror.PasswordAccess = this.GetControl<TextBox> ("tbAccessPassword").Text ?? "";

                this.Close ();
            } else {
                var msBoxStandardWindow = MsBox.Avalonia.MessageBoxManager
                    .GetMessageBoxCustom (new MessageBoxCustomParams {
                        ButtonDefinitions = new List<ButtonDefinition> {
                            new ButtonDefinition {
                                Name = "OK",
                                IsCancel=true}
                        },
                        ContentTitle = Instance.Language.Localize ("MIRROR:ReceiveTitle"),
                        ContentMessage = Instance.Language.Localize ("MIRROR:SettingsInvalid"),
                        Icon = MsBox.Avalonia.Enums.Icon.Info,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        WindowIcon = this.Icon,
                        Topmost = true,
                        CanResize = false,
                    });

                msBoxStandardWindow.ShowWindowAsync ();
            }
        }
    }
}