using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;

using II;
using II_Scenario_Editor.Controls;
using II_Scenario_Editor.Windows;

namespace II_Scenario_Editor {

    public partial class WindowMain : Window {
        /* Main data structure to build our scene into */
        private Scenario Scenario;

        /* Pointers for interface items */
        private TabControl ITabControl;
        private PanelOverview IPanelOverview;
        private PanelDevices IPanelDevices;
        private PanelChart IPanelChart;

        // Switch for processing elements in a loading sequence
        private bool IsLoading = false;

        public WindowMain () {
            InitializeComponent ();

            DataContext = this;

            ITabControl = this.FindControl<TabControl> ("tabControl");
            IPanelOverview = this.FindControl<PanelOverview> ("panelOverview");
            IPanelDevices = this.FindControl<PanelDevices> ("panelDevices");
            IPanelChart = this.FindControl<PanelChart> ("panelChart");

            _ = IPanelOverview.InitReferences (this);
            _ = IPanelDevices.InitReferences (this);
            _ = IPanelChart.InitReferences (this);

            _ = InitScenario ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        private async Task DialogAbout () {
            DialogAbout dlg = new ();
            await dlg.ShowDialog (this);
        }

        private static async Task Exit () {
            await App.Exit ();
        }

        private async Task<bool> PromptUnsavedWork () {
            if (Scenario.Steps.Count > 0) {
                DialogMessage dlg = new () {
                    Title = "Lose Unsaved Work?",
                    Message = "Are you sure you want to continue? All unsaved work will be lost!",
                    Option = DialogMessage.Options.YesNo,
                    Indicator = DialogMessage.Indicators.InfirmaryIntegratedScenarioEditor
                };
                DialogMessage.Responses? response = await dlg.AsyncShow (this);

                return (response != null && response == DialogMessage.Responses.Yes);
            } else
                return true;
        }

        private async Task InitScenario () {
            Scenario = new (false);
            await IPanelOverview.SetScenario (Scenario);

            ITabControl.SelectedIndex = 0;
        }

        private async Task NewScenario () {
            if (await PromptUnsavedWork () == false)
                return;

            await InitScenario ();
        }

        private async Task LoadScenario () {
            if (await PromptUnsavedWork () == false)
                return;

            string filepath = await LoadDialog ();
            if (String.IsNullOrEmpty (filepath))
                return;

            await LoadFile (filepath);
        }

        private async Task SaveScenario () {
            string filepath = await SaveDialog ();
            if (String.IsNullOrEmpty (filepath))
                return;

            await SaveFile (filepath);
        }

        private async Task<string> LoadDialog () {
            OpenFileDialog dlg = new ();
            dlg.Title = "Save Simulation";
            dlg.Filters.Add (new FileDialogFilter () { Name = "Infirmary Integrated Simulation", Extensions = { "ii" } });
            dlg.Filters.Add (new FileDialogFilter () { Name = "All Files", Extensions = { "*" } });
            dlg.AllowMultiple = false;

            string []? res = await dlg.ShowAsync (this);
            return (res == null || res.Length == 0 || String.IsNullOrEmpty (res [0])) ? "" : res [0];
        }

        private async Task<string> SaveDialog () {
            SaveFileDialog dlg = new ();
            dlg.Title = "Save Simulation";
            dlg.Filters.Add (new FileDialogFilter () { Name = "Infirmary Integrated Simulation", Extensions = { "ii" } });
            dlg.Filters.Add (new FileDialogFilter () { Name = "All Files", Extensions = { "*" } });
            dlg.DefaultExtension = "ii";

            string? res = await dlg.ShowAsync (this);
            return String.IsNullOrEmpty (res) ? "" : res;
        }

        private async Task LoadFile (string filepath) {
            StreamReader sr = new StreamReader (filepath);

            // Read savefile metadata indicating data formatting
            // Supports II:T1 file structure
            string metadata = sr.ReadLine ();
            if (!metadata.StartsWith (".ii:t1")) {
                await LoadFail ();
                return;
            }

            // Savefile type 1: validated and encrypted
            // Line 1 is metadata (.ii:t1)
            // Line 2 is hash for validation (hash taken of raw string data, unobfuscated)
            // Line 3 is savefile data encrypted by AES encoding
            string hash = sr.ReadLine ().Trim ();
            string file = Encryption.DecryptAES (sr.ReadToEnd ().Trim ());

            // Original save files used MD5, later changed to SHA256
            if (hash != Encryption.HashSHA256 (file) && hash != Encryption.HashMD5 (file)) {
                await LoadFail ();
                return;
            }

            StringReader sRead = new StringReader (file);
            string? line, pline;
            StringBuilder pbuffer;

            try {
                while (!String.IsNullOrEmpty (line = sRead.ReadLine ())) {
                    line = line.Trim ();

                    if (line == "> Begin: Scenario") {
                        pbuffer = new StringBuilder ();
                        while ((pline = sRead.ReadLine ()) != null && pline != "> End: Scenario")
                            pbuffer.AppendLine (pline);
                        Scenario.Load_Process (pbuffer.ToString ());
                    }
                }
            } catch {
                await LoadFail ();
            } finally {
                sRead.Close ();
            }

            for (int i = 0; i < Scenario.Steps.Count; i++) {
                /* TODO: IMPLEMENT

                // Add to the main Steps stack
                ItemStep ist = new ItemStep ();
                ist.Init ();
                ist.Step = sc.Steps [i];
                ist.SetNumber (i);
                ist.SetName (ist.Step.Name);

                // After all UIElements are initialized, will need to "refresh" the line positions via loadIProgressions
                isLoading = true;
                ist.LayoutUpdated += loadIProgressions;

                ist.IStep.MouseLeftButtonDown += IStep_MouseLeftButtonDown;
                ist.IStep.MouseLeftButtonUp += IStep_MouseLeftButtonUp;
                ist.IStep.MouseMove += IStep_MouseMove;

                ist.IStepEnd.MouseLeftButtonDown += IStepEnd_MouseLeftButtonDown;

                // Add to lists and display elements
                Steps.Add (ist);
                canvasDesigner.Children.Add (ist);

                Canvas.SetZIndex (ist, 1);
                Canvas.SetLeft (ist, ist.Step.IPositionX);
                Canvas.SetTop (ist, ist.Step.IPositionY);

                */
            }

            // Refresh the Properties View with the newly selected step

            /* TODO: IMPLEMENT
            selectStep (Steps.Count > 0 ? Steps [0] : null);
            updatePropertyView ();
            UpdateScenarioProperty ();
            drawIProgressions ();
            expStepProperty.IsExpanded = true;
            expProgressionProperty.IsExpanded = true;
            */
        }

        private async Task LoadFail () {
            DialogMessage dlg = new () {
                Title = "Unable to Load File",
                Message = "The selected file was unable to be loaded. Perhaps the file was damaged or edited outside of Infirmary Integrated.",
                Option = DialogMessage.Options.OK,
                Indicator = DialogMessage.Indicators.InfirmaryIntegratedScenarioEditor
            };
            await dlg.AsyncShow (this);
        }

        private Task SaveFile (string filepath, int indent = 1) {
            Scenario.Updated = DateTime.UtcNow;

            string dent = Utility.Indent (indent);
            StringBuilder sb = new ();

            sb.AppendLine ($"{dent}> Begin: Scenario");
            sb.Append (Scenario.Save (indent + 1));
            sb.AppendLine ($"{dent}> End: Scenario");

            // Save in II:T1 format
            StreamWriter sw = new StreamWriter (filepath);
            sw.WriteLine (".ii:t1");                                            // Metadata (type 1 savefile)

            sw.WriteLine (Encryption.HashSHA256 (sb.ToString ()));              // Hash for validation
            sw.Write (Encryption.EncryptAES (sb.ToString ()));                  // Savefile data encrypted with AES

#if DEBUG
            sw.WriteLine ();
            sw.WriteLine ();
            sw.WriteLine (sb.ToString ());                                          // FOR DEBUGGING: An unencrypted write call
#endif

            sw.Close ();

            return Task.CompletedTask;
        }

        public void MenuFileNew_Click (object sender, RoutedEventArgs e)
            => _ = NewScenario ();

        public void MenuFileLoad_Click (object sender, RoutedEventArgs e)
            => _ = LoadScenario ();

        public void MenuFileSave_Click (object sender, RoutedEventArgs e)
            => _ = SaveScenario ();

        public void MenuFileExit_Click (object sender, RoutedEventArgs e)
            => _ = Exit ();

        public void MenuHelpAbout_Click (object sender, RoutedEventArgs e)
            => _ = DialogAbout ();
    }
}