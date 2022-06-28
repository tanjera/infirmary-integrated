using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private PanelSimulation IPanelSimulation;
        private PanelStepEditor IPanelStepEditor;
        private PanelPatientParameters IPanelPatientParameters;
        private PanelPatientChart IPanelPatientChart;

        /* Data structures for use */
        private string? SaveFilePath;

        public WindowMain () {
            InitializeComponent ();

            DataContext = this;

            ITabControl = this.FindControl<TabControl> ("tabControl");
            IPanelSimulation = this.FindControl<PanelSimulation> ("panelSimulation");
            IPanelStepEditor = this.FindControl<PanelStepEditor> ("panelStepEditor");
            IPanelPatientParameters = this.FindControl<PanelPatientParameters> ("panelPatientParameters");
            IPanelPatientChart = this.FindControl<PanelPatientChart> ("panelPatientChart");

            _ = IPanelSimulation.InitReferences (this);
            _ = IPanelStepEditor.InitReferences (this);
            _ = IPanelPatientParameters.InitReferences (this);
            _ = IPanelPatientChart.InitReferences (this);

            _ = InitScenario ();
            _ = InitHotkeys ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        private async Task DialogAbout () {
            DialogAbout dlg = new ();
            await dlg.ShowDialog (this);
        }

        private async Task Exit (bool toConfirm = true) {
            if (toConfirm && Scenario.Steps.Count > 0) {
                DialogMessage dlg = new () {
                    Title = "Lose Unsaved Work?",
                    Message = "Are you sure you want to continue? All unsaved work will be lost!",
                    Option = DialogMessage.Options.YesNo,
                    Indicator = DialogMessage.Indicators.InfirmaryIntegratedScenarioEditor
                };
                DialogMessage.Responses? response = await dlg.AsyncShow (this);

                if (response != null && response == DialogMessage.Responses.Yes)
                    await App.Exit ();
                else
                    return;
            } else
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

        private async Task InitHotkeys () {
            var menuNew = IPanelSimulation.FindControl<MenuItem> ("menuNew");
            var menuLoad = IPanelSimulation.FindControl<MenuItem> ("menuLoad");
            var menuSave = IPanelSimulation.FindControl<MenuItem> ("menuSave");

            HotKeyManager.SetHotKey (menuNew, new KeyGesture (Key.N, KeyModifiers.Control));
            HotKeyManager.SetHotKey (menuLoad, new KeyGesture (Key.O, KeyModifiers.Control));
            HotKeyManager.SetHotKey (menuSave, new KeyGesture (Key.S, KeyModifiers.Control));
        }

        private async Task InitScenario () {
            SaveFilePath = null;
            await SetScenario (new Scenario (false));
        }

        private async Task SetScenario (Scenario scenario) {
            Scenario = scenario;

            await IPanelSimulation.SetScenario (Scenario);
            await IPanelStepEditor.SetScenario (Scenario);
            await IPanelPatientParameters.SetPatient (null);

            ITabControl.SelectedIndex = 0;
        }

        public async Task SetPatient (Patient? patient)
            => await IPanelPatientParameters.SetPatient (patient);

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

            Scenario? res = await LoadFile (filepath);
            if (res == null)
                await LoadFail ();
            else {
                SaveFilePath = filepath;
                await SetScenario (res);
            }
        }

        private async Task SaveScenario (bool useCurrentFile = false) {
            string filepath;

            if (useCurrentFile && !String.IsNullOrEmpty (SaveFilePath))
                filepath = SaveFilePath;
            else
                filepath = await SaveDialog ();

            if (String.IsNullOrEmpty (filepath))
                return;

            SaveFilePath = filepath;
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

        private async Task<Scenario?> LoadFile (string filepath) {
            StreamReader sr = new StreamReader (filepath);

            try {
                // Read savefile metadata indicating data formatting
                // Supports II:T1 file structure
                string metadata = sr.ReadLine ();
                if (!metadata.StartsWith (".ii:t1")) {
                    return null;
                }

                // Savefile type 1: validated and encrypted
                // Line 1 is metadata (.ii:t1)
                // Line 2 is hash for validation (hash taken of raw string data, unobfuscated)
                // Line 3 is savefile data encrypted by AES encoding
                string hash = sr.ReadLine ().Trim ();
                string file = Encryption.DecryptAES (sr.ReadToEnd ().Trim ());

                // Original save files used MD5, later changed to SHA256
                if (hash != Encryption.HashSHA256 (file) && hash != Encryption.HashMD5 (file)) {
                    return null;
                }

                string? line, pline;
                StringBuilder pbuffer;
                Scenario loadScene = new ();

                using (StringReader sRead = new StringReader (file)) {
                    while (!String.IsNullOrEmpty (line = sRead.ReadLine ())) {
                        line = line.Trim ();

                        if (line == "> Begin: Scenario") {
                            pbuffer = new StringBuilder ();
                            while ((pline = sRead.ReadLine ()) != null && pline != "> End: Scenario")
                                pbuffer.AppendLine (pline);

                            loadScene.Load_Process (pbuffer.ToString ());
                        }
                    }
                }

                return loadScene;
            } catch {
                return null;
            } finally {
                sr.Close ();
            }
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
            /* Note: the following debugging code CRASHES the Load() process */
            //sw.WriteLine ($"{Environment.NewLine}{Environment.NewLine}");
            //sw.WriteLine (sb.ToString ());                                      // FOR DEBUGGING: An unencrypted write call; human-readable output
#endif

            sw.Close ();

            return Task.CompletedTask;
        }

        public void MenuFileNew_Click (object sender, RoutedEventArgs e)
            => _ = NewScenario ();

        public void MenuFileLoad_Click (object sender, RoutedEventArgs e)
            => _ = LoadScenario ();

        public void MenuFileSave_Click (object sender, RoutedEventArgs e)
            => _ = SaveScenario (true);

        public void MenuFileSaveAs_Click (object sender, RoutedEventArgs e)
            => _ = SaveScenario (false);

        public void MenuFileExit_Click (object sender, RoutedEventArgs e)
            => _ = Exit (true);

        public void MenuHelpAbout_Click (object sender, RoutedEventArgs e)
            => _ = DialogAbout ();
    }
}