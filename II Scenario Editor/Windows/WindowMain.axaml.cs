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
            if (!this.IsVisible)                    // Avalonia's parent must be visible to attach a window
                this.Show ();

            DialogAbout dlg = new DialogAbout ();
            dlg.Activate ();
            await dlg.ShowDialog (this);
        }

        private static async Task Exit () {
            await App.Exit ();
        }

        private bool PromptUnsavedWork () {
            /* TODO: IMPLEMENT
            if (Steps.Count > 0)
                return MessageBox.Show (
                        "Are you sure you want to continue? All unsaved work will be lost!",
                        "Lose Unsaved Work?",
                        MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            else
                return MessageBoxResult.OK;
            */

            return true;
        }

        private async Task InitScenario () {
            Scenario = new (false);
            await IPanelOverview.InitScenario (Scenario);

            ITabControl.SelectedIndex = 0;
        }

        private async Task NewScenario () {
            if (PromptUnsavedWork () == false)
                return;

            await InitScenario ();
        }

        private async Task LoadScenario () {
            if (PromptUnsavedWork () == false)
                return;

            string filepath = LoadDialog ();
            if (String.IsNullOrEmpty (filepath))
                return;

            await LoadFile (filepath);
        }

        private string LoadDialog () {
            /* TODO: IMPLEMENT
            Microsoft.Win32.OpenFileDialog dlgLoad = new Microsoft.Win32.OpenFileDialog ();

            dlgLoad.Filter = "Infirmary Integrated simulation files (*.ii)|*.ii|All files (*.*)|*.*";
            dlgLoad.FilterIndex = 1;
            dlgLoad.RestoreDirectory = true;

            if (dlgLoad.ShowDialog () == true)
                return dlgLoad.FileName;
            else
                return null;
            */

            return "";
        }

        private async Task LoadFile (string filepath) {
            StreamReader sr = new StreamReader (filepath);

            // Read savefile metadata indicating data formatting
            // Supports II:T1 file structure
            string metadata = sr.ReadLine ();
            if (!metadata.StartsWith (".ii:t1")) {
                LoadFail ();
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
                LoadFail ();
                return;
            }

            StringReader sRead = new StringReader (file);
            string line, pline;
            StringBuilder pbuffer;

            try {
                while ((line = sRead.ReadLine ()) != null) {
                    if (line == "> Begin: Scenario") {
                        pbuffer = new StringBuilder ();
                        while ((pline = sRead.ReadLine ()) != null && pline != "> End: Scenario")
                            pbuffer.AppendLine (pline);
                        Scenario.Load_Process (pbuffer.ToString ());
                    }
                }
            } catch {
                LoadFail ();
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

        private void LoadFail () {
            /* TODO: IMPLEMENT
            MessageBox.Show (
                    "The selected file was unable to be loaded. Perhaps the file was damaged or edited outside of Infirmary Integrated.",
                    "Unable to Load File", MessageBoxButton.OK, MessageBoxImage.Error);
            */
        }

        private async Task SaveScenario () {
            /* TODO: IMPLEMENT

            // SAVE METADATA SECTION FOR ITEMSTEP POSITIONING!!!!!!

            // Set metadata for saving
            Steps [i].Step.IPositionX = Steps [i].Left;
            Steps [i].Step.IPositionY = Steps [i].Top;

            // And add to the main Scenario stack
            sc.Steps.Add (Steps [i].Step);

            */

            // Initiate IO stream, show Save File dialog to select file destination
            Stream s;

            /* TODO: IMPLEMENT
            Microsoft.Win32.SaveFileDialog dlgSave = new Microsoft.Win32.SaveFileDialog ();

            dlgSave.Filter = "Infirmary Integrated simulation files (*.ii)|*.ii|All files (*.*)|*.*";
            dlgSave.FilterIndex = 1;
            dlgSave.RestoreDirectory = true;

            if (dlgSave.ShowDialog () == true) {
                if ((s = dlgSave.OpenFile ()) != null) {
                    // Save in II:T1 format
                    StringBuilder sb = new StringBuilder ();

                    sb.AppendLine ("> Begin: Scenario");
                    sb.Append (sc.Save ());
                    sb.AppendLine ("> End: Scenario");

                    StreamWriter sw = new StreamWriter (s);
                    sw.WriteLine (".ii:t1");                                        // Metadata (type 1 savefile)
                    sw.WriteLine (Encryption.HashSHA256 (sb.ToString ().Trim ()));     // Hash for validation
                    sw.Write (Encryption.EncryptAES (sb.ToString ().Trim ()));         // Savefile data encrypted with AES
                    sw.Close ();
                    s.Close ();
                }
            }

            */
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