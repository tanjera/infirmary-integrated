/* Infirmary Integrated Scenario Editor
 * By Ibi Keller (Tanjera), (c) 2023
 */

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

using IISE.Controls;
using IISE.Windows;

namespace IISE {

    public partial class WindowMain : Window {
        /* Main data structure to build our scene into */
        private Scenario Scenario;

        /* Pointers for interface items */
        private TabControl ITabControl;
        private PanelSimulation IPanelSimulation;
        private PanelStepEditor IPanelStepEditor;
        private PanelParameters IPanelParameters;
        private PanelRecords IPanelRecords;

        /* Data structures for use */
        private string? SaveFilePath;

        public WindowMain () {
            InitializeComponent ();

            DataContext = this;

            ITabControl = this.FindControl<TabControl> ("tabControl");
            IPanelSimulation = this.FindControl<PanelSimulation> ("panelSimulation");
            IPanelStepEditor = this.FindControl<PanelStepEditor> ("panelStepEditor");
            IPanelParameters = this.FindControl<PanelParameters> ("panelParameters");
            IPanelRecords = this.FindControl<PanelRecords> ("panelRecords");

            ITabControl.SelectionChanged += ITabControl_SelectionChanged;

            _ = IPanelSimulation.InitReferences (this);
            _ = IPanelStepEditor.InitReferences (this);
            _ = IPanelParameters.InitReferences (this);
            _ = IPanelRecords.InitReferences (this);

            _ = InitScenario ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public async Task DialogAbout () {
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

        private async Task InitScenario () {
            SaveFilePath = null;
            await SetScenario (new Scenario (false));
        }

        private async Task SetScenario (Scenario scenario) {
            Scenario = scenario;

            await IPanelSimulation.SetScenario (Scenario);
            await IPanelStepEditor.SetScenario (Scenario);
            await IPanelParameters.SetStep (null);
            await IPanelRecords.SetStep (null);

            ITabControl.SelectedIndex = 0;
        }

        public async Task SetStep (Scenario.Step? step) {
            await IPanelParameters.SetStep (step);
            await IPanelRecords.SetStep (step);
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
            dlg.Title = "Load Simulation";
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
                string hash = (await sr.ReadLineAsync ())?.Trim () ?? "";
                string file = Encryption.DecryptAES ((await sr.ReadToEndAsync ()).Trim ());

                // Original save files used MD5, later changed to SHA256
                if (hash != Encryption.HashSHA256 (file) && hash != Encryption.HashMD5 (file)) {
                    return null;
                }

                string? line, pline;
                StringBuilder pbuffer;
                Scenario loadScene = new ();

                using (StringReader sRead = new (file)) {
                    while (!String.IsNullOrEmpty (line = await sRead.ReadLineAsync ())) {
                        line = line.Trim ();

                        if (line == "> Begin: Scenario") {
                            pbuffer = new StringBuilder ();
                            while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null
                                && pline != "> End: Scenario")
                                pbuffer.AppendLine (pline);

                            await loadScene.Load (pbuffer.ToString ());
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

        private async Task SaveFile (string filepath, int indent = 1) {
            Scenario.Updated = DateTime.UtcNow;

            string dent = Utility.Indent (indent);
            StringBuilder sb = new ();

            sb.AppendLine ($"{dent}> Begin: Scenario");
            sb.Append (await Scenario.Save (indent + 1));
            sb.AppendLine ($"{dent}> End: Scenario");

            // Save in II:T1 format
            StreamWriter sw = new StreamWriter (filepath);
            await sw.WriteLineAsync (".ii:t1");                                            // Metadata (type 1 savefile)

            await sw.WriteLineAsync (Encryption.HashSHA256 (sb.ToString ()));              // Hash for validation
            await sw.WriteAsync (Encryption.EncryptAES (sb.ToString ()));                  // Savefile data encrypted with AES

#if DEBUG
            /* Note: the following debugging code CRASHES the Load() process */
            //sw.WriteLine ($"{Environment.NewLine}{Environment.NewLine}");
            //sw.WriteLine (sb.ToString ());                                      // FOR DEBUGGING: An unencrypted write call; human-readable output
#endif

            sw.Close ();
        }

        public void ToggleFullscreen () {
            if (WindowState == WindowState.FullScreen)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.FullScreen;
        }

        private void ITabControl_SelectionChanged (object? sender, SelectionChangedEventArgs e) {
            MenuItem miPanelStepEditor = this.FindControl<MenuItem> ("menuEdit_PanelStepEditor");

            if (e.AddedItems.Count > 0 && e.AddedItems [0] is TabItem ta && ta.Name == "PanelStepEditor")
                miPanelStepEditor.IsVisible = true;

            if (e.RemovedItems.Count > 0 && e.RemovedItems [0] is TabItem tr && tr.Name == "PanelStepEditor")
                miPanelStepEditor.IsVisible = false;
        }

        public void MenuFileNew_Click (object sender, RoutedEventArgs e)
            => _ = NewScenario ();

        public void MenuFileLoad_Click (object sender, RoutedEventArgs e)
            => _ = LoadScenario ();

        public void MenuFileSave_Click (object sender, RoutedEventArgs e)
            => _ = SaveScenario (true);

        public void MenuFileSaveAs_Click (object sender, RoutedEventArgs e)
            => _ = SaveScenario (false);

        private void MenuToggleFullscreen_Click (object s, RoutedEventArgs e)
            => ToggleFullscreen ();

        public void MenuFileExit_Click (object sender, RoutedEventArgs e)
            => _ = Exit (true);

        public void MenuHelpAbout_Click (object sender, RoutedEventArgs e)
            => _ = DialogAbout ();

        /* Menu Items specific to IPanelStepEditor */

        public void MenuEditAddStep_Click (object sender, RoutedEventArgs e)
            => IPanelStepEditor.Action_AddStep ();

        public void MenuEditDuplicateStep_Click (object sender, RoutedEventArgs e)
            => IPanelStepEditor.Action_DuplicateStep ();

        public void MenuEditDeleteStep_Click (object sender, RoutedEventArgs e)
            => IPanelStepEditor.Action_DeleteStep ();

        public void MenuEditRepositionSteps_Click (object sender, RoutedEventArgs e)
            => _ = IPanelStepEditor.Action_RepositionSteps ();

        public void MenuEditCopyPhysiology_Click (object sender, RoutedEventArgs e)
            => _ = IPanelStepEditor.Action_CopyPhysiology ();

        public void MenuEditPastePhysiology_Click (object sender, RoutedEventArgs e) {
            _ = IPanelStepEditor.Action_PastePhysiology ();
            _ = IPanelParameters.UpdateView ();
        }

        public void MenuEditCopyRecords_Click (object sender, RoutedEventArgs e)
            => _ = IPanelStepEditor.Action_CopyRecords ();

        public void MenuEditPasteRecords_Click (object sender, RoutedEventArgs e) {
            _ = IPanelStepEditor.Action_PasteRecords ();
            _ = IPanelParameters.UpdateView ();
        }

        /* Menu Commands: For HotKey support!! */

        private void MenuFileNew_Command ()
        => MenuFileNew_Click (this, new RoutedEventArgs ());

        private void MenuFileLoad_Command ()
            => MenuFileLoad_Click (this, new RoutedEventArgs ());

        private void MenuFileSave_Command ()
            => MenuFileSave_Click (this, new RoutedEventArgs ());

        private void Menu_Command ()
            => MenuFileSaveAs_Click (this, new RoutedEventArgs ());

        private void MenuEditCopyPhysiology_Command () {
            if (ITabControl.SelectedItem is TabItem ti && ti.Name == "PanelStepEditor")
                MenuEditCopyPhysiology_Click (this, new RoutedEventArgs ());
        }

        private void MenuEditPastePhysiology_Command () {
            if (ITabControl.SelectedItem is TabItem ti && ti.Name == "PanelStepEditor")
                MenuEditPastePhysiology_Click (this, new RoutedEventArgs ());

            _ = IPanelParameters.UpdateView ();
        }

        private void MenuEditCopyRecords_Command () {
            if (ITabControl.SelectedItem is TabItem ti && ti.Name == "PanelStepEditor")
                MenuEditCopyRecords_Click (this, new RoutedEventArgs ());
        }

        private void MenuEditPasteRecords_Command () {
            if (ITabControl.SelectedItem is TabItem ti && ti.Name == "PanelStepEditor")
                MenuEditPasteRecords_Click (this, new RoutedEventArgs ());

            _ = IPanelParameters.UpdateView ();
        }

        private void MenuEditDeleteStep_Command () {
            if (ITabControl.SelectedItem is TabItem ti && ti.Name == "PanelStepEditor")
                MenuEditDeleteStep_Click (this, new RoutedEventArgs ());
        }

        private void MenuEditAddStep_Command () {
            if (ITabControl.SelectedItem is TabItem ti && ti.Name == "PanelStepEditor")
                MenuEditAddStep_Click (this, new RoutedEventArgs ());
        }

        private void MenuEditDuplicateStep_Command () {
            if (ITabControl.SelectedItem is TabItem ti && ti.Name == "PanelStepEditor")
                MenuEditDuplicateStep_Click (this, new RoutedEventArgs ());
        }
    }
}