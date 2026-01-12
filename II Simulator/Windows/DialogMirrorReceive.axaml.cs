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
using Avalonia.Threading;
using II;
using II.Localization;
using II.Server;
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

            this.GetControl<TextBlock> ("txtServerAddress").Text = Instance.Language.Localize ("MIRROR:ServerAddress");
            this.GetControl<TextBlock> ("txtAccessionKey").Text = Instance.Language.Localize ("MIRROR:AccessionKey");
            this.GetControl<TextBlock> ("txtAccessPassword").Text = Instance.Language.Localize ("MIRROR:AccessPassword");

            this.GetControl<TextBox> ("tbServerAddress").Text = Instance.Mirror.ServerAddress;
            this.GetControl<TextBox> ("tbServerAddress").Watermark = II.Server.Mirror.DefaultServer;
            
            this.GetControl<TextBox> ("tbAccessionKey").Text = !String.IsNullOrEmpty(Instance.Mirror.Accession) 
                ? Instance.Mirror.Accession : "";
            this.GetControl<TextBox> ("tbAccessPassword").Text = !String.IsNullOrEmpty(Instance.Mirror.PasswordAccess)
                ? Instance.Mirror.PasswordAccess : "";
            
            this.GetControl<Button> ("btnContinue").Content = Instance.Language.Localize ("BUTTON:Continue");
        }

        private async Task SetMirrorReceive () {
            if (Instance is null) {
                    Debug.WriteLine ($"Null return at {this.Name}.{nameof (OnClick_Continue)}");
                    return;
            }

            Regex regex = new ("^[a-zA-Z0-9]*$");
            if ((this.GetControl<TextBox> ("tbAccessionKey").Text ?? "").Length > 0
                && regex.IsMatch (this.GetControl<TextBox> ("tbAccessionKey")?.Text ?? "")) {
                Instance.Mirror.Status = II.Server.Mirror.Statuses.CLIENT;
                Instance.Mirror.ServerAddress =
                    this.GetControl<TextBox> ("tbServerAddress").Text ?? II.Server.Mirror.DefaultServer;
                Instance.Mirror.Accession = this.GetControl<TextBox> ("tbAccessionKey").Text ?? "";
                Instance.Mirror.PasswordAccess = this.GetControl<TextBox> ("tbAccessPassword").Text ?? "";

                // Attempt to connect with the server to validate connection and credentials; present error if problem
                if (Instance?.Server is not null) {
                    II.Scenario.Step test = new Scenario.Step (Instance.Timer_Simulation); 
                    Server.ServerResponse resp = await Instance.Mirror.GetStep (test, Instance.Server);
                    
                    switch (resp) {
                        default: 
                            this.Close ();
                            break;
                        
                        case Server.ServerResponse.ErrorCredentials:
                            Instance.Mirror.Status = II.Server.Mirror.Statuses.INACTIVE;
                            
                            await Dispatcher.UIThread.InvokeAsync (async () => {
                                DialogMessage dlg = new (Instance) {
                                    Message = Instance.Language.Localize ("MIRROR:ErrorServerEmptyResponse"),
                                    Title = Instance.Language.Localize ("MIRROR:Error"),
                                    Indicator = DialogMessage.Indicators.Error,
                                    Option = DialogMessage.Options.OK,
                                };

                                if (!this.IsVisible)                    // Avalonia's parent must be visible to attach a window
                                    this.Show ();

                                await dlg.AsyncShow (this);
                            });
                            break;
                        
                        case Server.ServerResponse.ErrorNameResolution: 
                            Instance.Mirror.Status = II.Server.Mirror.Statuses.INACTIVE;
                            
                            await Dispatcher.UIThread.InvokeAsync (async () => {
                                DialogMessage dlg = new (Instance) {
                                    Message = Instance.Language.Localize ("MIRROR:ErrorServerInaccessible"),
                                    Title = Instance.Language.Localize ("MIRROR:Error"),
                                    Indicator = DialogMessage.Indicators.Error,
                                    Option = DialogMessage.Options.OK,
                                };

                                if (!this.IsVisible)                    // Avalonia's parent must be visible to attach a window
                                    this.Show ();

                                await dlg.AsyncShow (this);
                            });
                            break;
                    }
                }
            } else {
                await Dispatcher.UIThread.InvokeAsync (async () => {
                    DialogMessage dlg = new (Instance) {
                        Message = Instance.Language.Localize ("MIRROR:SettingsInvalid"),
                        Title = Instance.Language.Localize ("MIRROR:ReceiveTitle"),
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
            => _ = SetMirrorReceive ();

    }
}