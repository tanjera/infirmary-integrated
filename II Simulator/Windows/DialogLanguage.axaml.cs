/* Infirmary Integrated Simulator
 * By Ibi Keller (Tanjera), (c) 2023
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

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

            this.GetControl<Window> ("dlgLanguage").Title = Instance.Language.Localize ("LANGUAGE:Title");
            this.GetControl<Label> ("lblChooseLanguage").Content = Instance.Language.Localize ("LANGUAGE:Select");
            this.GetControl<Button> ("btnContinue").Content = Instance.Language.Localize ("BUTTON:Continue");

            ComboBox cmbLanguages = this.GetControl<ComboBox> ("cmbLanguages");
                foreach (string each in II.Localization.Language.Descriptions)
                cmbLanguages.Items.Add(new MenuItem() {
                    Header =  each,
                });
                
            this.GetControl<ComboBox> ("cmbLanguages").SelectedIndex = 
                Enum.Parse (typeof (Language.Languages), Instance?.Settings.Language ?? "ENG").GetHashCode();
        }

        private async Task SetLanguage () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (OnClick_Continue)}");
                return;
            }

            Instance.Language.Selection = Enum.GetValues<Language.Languages> () [this.GetControl<ComboBox> ("cmbLanguages").SelectedIndex];

            Instance.Settings.Language = Instance.Language.Selection.ToString ();
            Instance.Settings.Save ();

            // Show messagebox prompting user to restart the program for changes to take effect
            await Dispatcher.UIThread.InvokeAsync (async () => {
                DialogMessage dlg = new (Instance) {
                    Message = Instance.Language.Localize ("MESSAGE:RestartForChanges"),
                    Title = Instance.Language.Localize ("MESSAGE:Restart"),
                    Indicator = DialogMessage.Indicators.InfirmaryIntegrated,
                    Option = DialogMessage.Options.OK,
                };

                if (!this.IsVisible)                    // Avalonia's parent must be visible to attach a window
                    this.Show ();

                await dlg.AsyncShow (this);
            });
            
            this.Close ();
        }

        private void OnClick_Continue (object sender, RoutedEventArgs e)
            => _ = SetLanguage ();

    }
}