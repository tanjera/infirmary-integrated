/* Infirmary Integrated Simulator
 * By Ibi Keller (Tanjera), (c) 2023
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;

using II;
using II.Localization;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Models;

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

            this.GetControl<Window> ("dlgLanguage").Title = Instance.Language.Localize ("LANGUAGE:Title");
            this.GetControl<Label> ("lblChooseLanguage").Content = Instance.Language.Localize ("LANGUAGE:Select");
            this.GetControl<Button> ("btnContinue").Content = Instance.Language.Localize ("BUTTON:Continue");

            ComboBox cmbLanguages = this.GetControl<ComboBox> ("cmbLanguages");
                foreach (string each in II.Localization.Language.Descriptions)
                cmbLanguages.Items.Add(new MenuItem() {
                    Header =  each,
                });
            this.GetControl<ComboBox> ("cmbLanguages").SelectedIndex = (int)II.Localization.Language.Values.ENG;
        }

        private void OnClick_Continue (object sender, RoutedEventArgs e) {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (OnClick_Continue)}");
                return;
            }

            Instance.Language.Value = Enum.GetValues<Language.Values> () [this.GetControl<ComboBox> ("cmbLanguages").SelectedIndex];

            Instance.Settings.Language = Instance.Language.Value.ToString ();
            Instance.Settings.Save ();

            // Show messagebox prompting user to restart the program for changes to take effect

            var msBoxStandardWindow = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxCustom (new MessageBoxCustomParams {
                    ButtonDefinitions = new List<ButtonDefinition> () {
                            new ButtonDefinition {
                                Name = "OK",
                                IsCancel=true}
                    },
                    ContentTitle = Instance.Language.Localize ("MESSAGE:Restart"),
                    ContentMessage = Instance.Language.Localize ("MESSAGE:RestartForChanges"),
                    Icon = MsBox.Avalonia.Enums.Icon.Info,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    WindowIcon = this.Icon,
                    ShowInCenter = true,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    Topmost = true,
                    CanResize = false,
                });

            msBoxStandardWindow.ShowWindowAsync ();

            this.Close ();
        }
    }
}