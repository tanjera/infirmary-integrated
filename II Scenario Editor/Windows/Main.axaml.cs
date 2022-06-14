using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;

using II;

using II_Scenario_Editor.Controls;

namespace II_Scenario_Editor {

    public partial class Main : Window {

        // Master List of all Steps in the Scenario and other Scenario variables
        private List<ItemStep> Steps = new List<ItemStep> ();

        private string ScenarioAuthor, ScenarioName, ScenarioDescription;

        // Variables and pointers for using UI Elements
        private Canvas canvasDesigner;

        private ItemStep selStep;
        /* TODO: IMPLEMENT
        private Controls.ItemStep.UEIStepEnd selEnd;
        */

        // For copy/pasting Patient parameters
        private Patient copiedPatient;

        // Variables for capturing mouse and dragging UI elements
        private bool mouseCaptured = false;

        private double xShape, yShape,
            xCanvas, yCanvas;

        // Switch for processing elements ina  loading sequence
        private bool isLoading = false;

        public Main () {
            InitializeComponent ();

            DataContext = this;

            canvasDesigner = this.FindControl<Canvas> ("cnvsDesigner");

            InitScenarioProperty ();
            InitPropertyView ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        private bool promptUnsavedWork () {
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

        private async Task NewScenario () {
            if (promptUnsavedWork () == false)
                return;

            /* TODO: IMPLEMENT

            // Reset buffer parameters
            selStep = null;
            selEnd = null;
            mouseCaptured = false;

            // Clear master lists and UI elements
            canvasDesigner.Children.Clear ();
            Steps.Clear ();

            // Clear scenario data
            ScenarioAuthor = "";
            ScenarioName = "";
            ScenarioDescription = "";

            UpdateScenarioProperty ();
            */
        }

        private async Task LoadScenario () {
            if (promptUnsavedWork () == false)
                return;

            string filepath = LoadDialog ();
            if (String.IsNullOrEmpty (filepath))
                return;

            LoadFile (filepath);
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

            return null;
        }

        private void LoadFile (string filepath) {
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

            Scenario sc = new Scenario (false);

            try {
                while ((line = sRead.ReadLine ()) != null) {
                    if (line == "> Begin: Scenario") {
                        pbuffer = new StringBuilder ();
                        while ((pline = sRead.ReadLine ()) != null && pline != "> End: Scenario")
                            pbuffer.AppendLine (pline);
                        sc.Load_Process (pbuffer.ToString ());
                    }
                }
            } catch {
                LoadFail ();
            } finally {
                sRead.Close ();
            }

            // Convert loaded scenario to Scenario Editor data structures
            ScenarioAuthor = sc.Author;
            ScenarioName = sc.Name;
            ScenarioDescription = sc.Description;

            Steps.Clear ();
            canvasDesigner.Children.Clear ();

            for (int i = 0; i < sc.Steps.Count; i++) {
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
            // Prepare Scenario for saving
            Scenario sc = new Scenario (false);

            sc.Updated = DateTime.Now;
            sc.Author = ScenarioAuthor;
            sc.Name = ScenarioName;
            sc.Description = ScenarioDescription;

            for (int i = 0; i < Steps.Count; i++) {
                /* TODO: IMPLEMENT

                // Set metadata for saving
                Steps [i].Step.IPositionX = Steps [i].Left;
                Steps [i].Step.IPositionY = Steps [i].Top;

                // And add to the main Scenario stack
                sc.Steps.Add (Steps [i].Step);

                */
            }

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

        private void InitScenarioProperty () {
            PropertyString pstrScenarioAuthor = this.FindControl<PropertyString> ("pstrScenarioAuthor");
            PropertyString pstrScenarioName = this.FindControl<PropertyString> ("pstrScenarioName");
            PropertyString pstrScenarioDescription = this.FindControl<PropertyString> ("pstrScenarioDescription");

            // Initiate controls for editing Scenario properties
            pstrScenarioAuthor.Init (PropertyString.Keys.ScenarioAuthor);
            pstrScenarioName.Init (PropertyString.Keys.ScenarioName);
            pstrScenarioDescription.Init (PropertyString.Keys.ScenarioDescription);

            pstrScenarioAuthor.PropertyChanged += UpdateProperty;
            pstrScenarioName.PropertyChanged += UpdateProperty;
            pstrScenarioDescription.PropertyChanged += UpdateProperty;
        }

        private void UpdateScenarioProperty () {
            this.FindControl<PropertyString> ("pstrScenarioAuthor").Set (ScenarioAuthor ?? "");
            this.FindControl<PropertyString> ("pstrScenarioName").Set (ScenarioName ?? "");
            this.FindControl<PropertyString> ("pstrScenarioDescription").Set (ScenarioDescription ?? "");
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

        private void MenuItemNew_Click (object sender, RoutedEventArgs e)
            => _ = NewScenario ();

        private void MenuItemLoad_Click (object sender, RoutedEventArgs e)
            => _ = LoadScenario ();

        private void MenuSave_Click (object sender, RoutedEventArgs e)
            => _ = SaveScenario ();

        private void MenuItemExit_Click (object sender, RoutedEventArgs e)
            => _ = Exit ();

        private void MenuItemAbout_Click (object sender, RoutedEventArgs e)
            => _ = DialogAbout ();

        private void ButtonAddStep_Click (object sender, RoutedEventArgs e) {
            addStep (null);
        }

        private void ButtonDuplicateStep_Click (object sender, RoutedEventArgs e) {
            if (selStep == null)
                return;
            /* TODO: IMPLEMENT
            addStep (selStep.Duplicate ());
            */
        }

        private void BtnDeleteStep_Click (object sender, RoutedEventArgs e) {
            if (selStep == null)
                return;

            /* TODO: IMPLEMENT
            deleteStep (selStep);
            */
        }

        private async void BtnCopyPatient_Click (object sender, RoutedEventArgs e) {
            if (selStep == null)
                return;

            copiedPatient = new Patient ();
            await copiedPatient.Load_Process (selStep.Patient.Save ());
        }

        private async void BtnPastePatient_Click (object sender, RoutedEventArgs e) {
            if (selStep == null)
                return;

            if (copiedPatient != null) {
                await selStep.Patient.Load_Process (copiedPatient.Save ());
            }

            UpdatePropertyView ();
        }

        private void BtnDeleteDefaultProgression_Click (object sender, RoutedEventArgs e) {
            /* TODO: IMPLEMENT
             deleteDefaultProgression ();
            */
        }

        private void InitPropertyView () {
            // Populate enum string lists for readable display
            List<string> cardiacRhythms = new List<string> (),
                respiratoryRhythms = new List<string> (),
                pulmonaryRhythms = new List<string> (),
                cardiacAxes = new List<string> (),
                intensityScale = new List<string> (),
                fetalHeartRhythms = new List<string> ();

            if (App.Language != null) {
                foreach (Cardiac_Rhythms.Values v in Enum.GetValues (typeof (Cardiac_Rhythms.Values)))
                    cardiacRhythms.Add (App.Language.Dictionary [Cardiac_Rhythms.LookupString (v)]);

                foreach (Respiratory_Rhythms.Values v in Enum.GetValues (typeof (Respiratory_Rhythms.Values)))
                    respiratoryRhythms.Add (App.Language.Dictionary [Respiratory_Rhythms.LookupString (v)]);

                foreach (PulmonaryArtery_Rhythms.Values v in Enum.GetValues (typeof (PulmonaryArtery_Rhythms.Values)))
                    pulmonaryRhythms.Add (App.Language.Dictionary [PulmonaryArtery_Rhythms.LookupString (v)]);

                foreach (Cardiac_Axes.Values v in Enum.GetValues (typeof (Cardiac_Axes.Values)))
                    cardiacAxes.Add (App.Language.Dictionary [Cardiac_Axes.LookupString (v)]);
            }

            // Find all controls and attach to reference
            PropertyBP pbpNBP = this.FindControl<PropertyBP> ("pbpNBP");
            PropertyBP pbpABP = this.FindControl<PropertyBP> ("pbpABP");
            PropertyBP pbpPBP = this.FindControl<PropertyBP> ("pbpPBP");

            PropertyCheck pchkMechanicallyVentilated = this.FindControl<PropertyCheck> ("pchkMechanicallyVentilated");
            PropertyCheck pchkPulsusParadoxus = this.FindControl<PropertyCheck> ("pchkPulsusParadoxus");
            PropertyCheck pchkPulsusAlternans = this.FindControl<PropertyCheck> ("pchkPulsusAlternans");

            PropertyDouble pdblT = this.FindControl<PropertyDouble> ("pdblT");
            PropertyDouble pdblCO = this.FindControl<PropertyDouble> ("pdblCO");
            PropertyDouble pdblInspiratoryRatio = this.FindControl<PropertyDouble> ("pdblInspiratoryRatio");
            PropertyDouble pdblExpiratoryRatio = this.FindControl<PropertyDouble> ("pdblExpiratoryRatio");

            PropertyECGSegment pecgSTSegment = this.FindControl<PropertyECGSegment> ("pecgSTSegment");
            PropertyECGSegment pecgTWave = this.FindControl<PropertyECGSegment> ("pecgTWave");

            PropertyEnum penmCardiacRhythms = this.FindControl<PropertyEnum> ("penmCardiacRhythms");
            PropertyEnum penmRespiratoryRhythms = this.FindControl<PropertyEnum> ("penmRespiratoryRhythms");
            PropertyEnum penmCardiacAxis = this.FindControl<PropertyEnum> ("penmCardiacAxis");
            PropertyEnum penmPACatheterRhythm = this.FindControl<PropertyEnum> ("penmPACatheterRhythm");

            PropertyInt pintHR = this.FindControl<PropertyInt> ("pintHR");
            PropertyInt pintRR = this.FindControl<PropertyInt> ("pintRR");
            PropertyInt pintSPO2 = this.FindControl<PropertyInt> ("pintSPO2");
            PropertyInt pintETCO2 = this.FindControl<PropertyInt> ("pintETCO2");
            PropertyInt pintCVP = this.FindControl<PropertyInt> ("pintCVP");
            PropertyInt pintICP = this.FindControl<PropertyInt> ("pintICP");
            PropertyInt pintIAP = this.FindControl<PropertyInt> ("pintIAP");
            PropertyInt pintPacemakerThreshold = this.FindControl<PropertyInt> ("pintPacemakerThreshold");
            PropertyInt pintProgressFrom = this.FindControl<PropertyInt> ("pintProgressFrom");
            PropertyInt pintProgressTo = this.FindControl<PropertyInt> ("pintProgressTo");
            PropertyInt pintProgressTimer = this.FindControl<PropertyInt> ("pintProgressTimer");

            PropertyString pstrStepName = this.FindControl<PropertyString> ("pstrStepName");
            PropertyString pstrStepDescription = this.FindControl<PropertyString> ("pstrStepDescription");

            // Initiate controls for editing Patient values
            pstrStepName.Init (PropertyString.Keys.StepName);
            pstrStepDescription.Init (PropertyString.Keys.StepDescription);

            pbpNBP.Init (PropertyBP.Keys.NSBP, 5, 0, 300, 5, 0, 200);
            pbpABP.Init (PropertyBP.Keys.ASBP, 5, 0, 300, 5, 0, 200);
            pbpPBP.Init (PropertyBP.Keys.PSP, 5, 0, 200, 5, 0, 200);

            pchkMechanicallyVentilated.Init (PropertyCheck.Keys.MechanicallyVentilated);
            pchkPulsusParadoxus.Init (PropertyCheck.Keys.PulsusParadoxus);
            pchkPulsusAlternans.Init (PropertyCheck.Keys.PulsusAlternans);

            pdblT.Init (PropertyDouble.Keys.T, 0.2, 0, 100);
            pdblCO.Init (PropertyDouble.Keys.CO, 0.1, 0, 20);
            pdblInspiratoryRatio.Init (PropertyDouble.Keys.RRInspiratoryRatio, 0.1, 0.1, 10);
            pdblExpiratoryRatio.Init (PropertyDouble.Keys.RRExpiratoryRatio, 0.1, 0.1, 10);

            pecgSTSegment.Init (PropertyECGSegment.Keys.STElevation);
            pecgTWave.Init (PropertyECGSegment.Keys.TWave);

            penmCardiacRhythms.Init (PropertyEnum.Keys.Cardiac_Rhythms,
                Enum.GetNames (typeof (Cardiac_Rhythms.Values)), cardiacRhythms);
            penmRespiratoryRhythms.Init (PropertyEnum.Keys.Respiratory_Rhythms,
                Enum.GetNames (typeof (Respiratory_Rhythms.Values)), respiratoryRhythms);
            penmPACatheterRhythm.Init (PropertyEnum.Keys.PACatheter_Rhythms,
                Enum.GetNames (typeof (PulmonaryArtery_Rhythms.Values)), pulmonaryRhythms);
            penmCardiacAxis.Init (PropertyEnum.Keys.Cardiac_Axis,
                Enum.GetNames (typeof (Cardiac_Axes.Values)), cardiacAxes);

            pintHR.Init (PropertyInt.Keys.HR, 5, 0, 500);
            pintRR.Init (PropertyInt.Keys.RR, 2, 0, 100);
            pintSPO2.Init (PropertyInt.Keys.SPO2, 2, 0, 100);
            pintICP.Init (PropertyInt.Keys.ICP, 1, -100, 100);
            pintIAP.Init (PropertyInt.Keys.IAP, 1, -100, 100);
            pintPacemakerThreshold.Init (PropertyInt.Keys.PacemakerThreshold, 5, 0, 200);
            pintProgressFrom.Init (PropertyInt.Keys.ProgressFrom, 1, -1, 1000);
            pintProgressTo.Init (PropertyInt.Keys.ProgressTo, 1, -1, 1000);
            pintProgressTimer.Init (PropertyInt.Keys.ProgressTimer, 1, -1, 1000);

            pintETCO2.Init (PropertyInt.Keys.ETCO2, 2, 0, 100);
            pintCVP.Init (PropertyInt.Keys.CVP, 1, -100, 100);

            pbpNBP.PropertyChanged += UpdateProperty;
            pbpABP.PropertyChanged += UpdateProperty;
            pbpPBP.PropertyChanged += UpdateProperty;

            pchkMechanicallyVentilated.PropertyChanged += UpdateProperty;
            pchkPulsusParadoxus.PropertyChanged += UpdateProperty;
            pchkPulsusAlternans.PropertyChanged += UpdateProperty;

            pdblT.PropertyChanged += UpdateProperty;
            pdblCO.PropertyChanged += UpdateProperty;
            pdblInspiratoryRatio.PropertyChanged += UpdateProperty;
            pdblExpiratoryRatio.PropertyChanged += UpdateProperty;

            pecgSTSegment.PropertyChanged += UpdateProperty;
            pecgTWave.PropertyChanged += UpdateProperty;

            penmCardiacRhythms.PropertyChanged += UpdateProperty;
            penmRespiratoryRhythms.PropertyChanged += UpdateProperty;
            penmPACatheterRhythm.PropertyChanged += UpdateProperty;
            penmCardiacAxis.PropertyChanged += UpdateProperty;

            penmCardiacRhythms.PropertyChanged += UpdateCardiacRhythm;
            penmRespiratoryRhythms.PropertyChanged += UpdateRespiratoryRhythm;
            penmPACatheterRhythm.PropertyChanged += UpdatePACatheterRhythm;

            pintICP.PropertyChanged += UpdateProperty;
            pintIAP.PropertyChanged += UpdateProperty;
            pintHR.PropertyChanged += UpdateProperty;
            pintRR.PropertyChanged += UpdateProperty;
            pintSPO2.PropertyChanged += UpdateProperty;
            pintETCO2.PropertyChanged += UpdateProperty;
            pintCVP.PropertyChanged += UpdateProperty;
            pintPacemakerThreshold.PropertyChanged += UpdateProperty;
            pintProgressFrom.PropertyChanged += UpdateProperty;
            pintProgressTo.PropertyChanged += UpdateProperty;
            pintProgressTimer.PropertyChanged += UpdateProperty;

            pstrStepName.PropertyChanged += UpdateProperty;
            pstrStepDescription.PropertyChanged += UpdateProperty;
        }

        private void UpdatePropertyView () {
            if (selStep == null)
                return;

            // Find all controls and attach to reference
            PropertyBP pbpNBP = this.FindControl<PropertyBP> ("pbpNBP");
            PropertyBP pbpABP = this.FindControl<PropertyBP> ("pbpABP");
            PropertyBP pbpPBP = this.FindControl<PropertyBP> ("pbpPBP");

            PropertyCheck pchkMechanicallyVentilated = this.FindControl<PropertyCheck> ("pchkMechanicallyVentilated");
            PropertyCheck pchkPulsusParadoxus = this.FindControl<PropertyCheck> ("pchkPulsusParadoxus");
            PropertyCheck pchkPulsusAlternans = this.FindControl<PropertyCheck> ("pchkPulsusAlternans");

            PropertyDouble pdblT = this.FindControl<PropertyDouble> ("pdblT");
            PropertyDouble pdblCO = this.FindControl<PropertyDouble> ("pdblCO");
            PropertyDouble pdblInspiratoryRatio = this.FindControl<PropertyDouble> ("pdblInspiratoryRatio");
            PropertyDouble pdblExpiratoryRatio = this.FindControl<PropertyDouble> ("pdblExpiratoryRatio");

            PropertyECGSegment pecgSTSegment = this.FindControl<PropertyECGSegment> ("pecgSTSegment");
            PropertyECGSegment pecgTWave = this.FindControl<PropertyECGSegment> ("pecgTWave");

            PropertyEnum penmCardiacRhythms = this.FindControl<PropertyEnum> ("penmCardiacRhythms");
            PropertyEnum penmRespiratoryRhythms = this.FindControl<PropertyEnum> ("penmRespiratoryRhythms");
            PropertyEnum penmCardiacAxis = this.FindControl<PropertyEnum> ("penmCardiacAxis");
            PropertyEnum penmPACatheterRhythm = this.FindControl<PropertyEnum> ("penmPACatheterRhythm");

            PropertyInt pintHR = this.FindControl<PropertyInt> ("pintHR");
            PropertyInt pintRR = this.FindControl<PropertyInt> ("pintRR");
            PropertyInt pintSPO2 = this.FindControl<PropertyInt> ("pintSPO2");
            PropertyInt pintETCO2 = this.FindControl<PropertyInt> ("pintETCO2");
            PropertyInt pintCVP = this.FindControl<PropertyInt> ("pintCVP");
            PropertyInt pintICP = this.FindControl<PropertyInt> ("pintICP");
            PropertyInt pintIAP = this.FindControl<PropertyInt> ("pintIAP");
            PropertyInt pintPacemakerThreshold = this.FindControl<PropertyInt> ("pintPacemakerThreshold");
            PropertyInt pintProgressFrom = this.FindControl<PropertyInt> ("pintProgressFrom");
            PropertyInt pintProgressTo = this.FindControl<PropertyInt> ("pintProgressTo");
            PropertyInt pintProgressTimer = this.FindControl<PropertyInt> ("pintProgressTimer");

            PropertyString pstrStepName = this.FindControl<PropertyString> ("pstrStepName");
            PropertyString pstrStepDescription = this.FindControl<PropertyString> ("pstrStepDescription");

            // Update all controls with Patient values
            pbpNBP.Set (selStep.Patient.VS_Settings.NSBP, selStep.Patient.VS_Settings.NDBP);
            pbpABP.Set (selStep.Patient.VS_Settings.ASBP, selStep.Patient.VS_Settings.ADBP);
            pbpPBP.Set (selStep.Patient.VS_Settings.PSP, selStep.Patient.VS_Settings.PDP);

            pchkMechanicallyVentilated.Set (selStep.Patient.Mechanically_Ventilated);
            pchkPulsusParadoxus.Set (selStep.Patient.Pulsus_Paradoxus);
            pchkPulsusAlternans.Set (selStep.Patient.Pulsus_Alternans);

            pdblT.Set (selStep.Patient.VS_Settings.T);
            pdblCO.Set (selStep.Patient.VS_Settings.CO);
            pdblInspiratoryRatio.Set (selStep.Patient.VS_Settings.RR_IE_I);
            pdblExpiratoryRatio.Set (selStep.Patient.VS_Settings.RR_IE_E);

            pecgSTSegment.Set (selStep.Patient.ST_Elevation);
            pecgTWave.Set (selStep.Patient.T_Elevation);

            penmCardiacRhythms.Set ((int)selStep.Patient.Cardiac_Rhythm.Value);
            penmRespiratoryRhythms.Set ((int)selStep.Patient.Respiratory_Rhythm.Value);
            penmPACatheterRhythm.Set ((int)selStep.Patient.PulmonaryArtery_Placement.Value);
            penmCardiacAxis.Set ((int)selStep.Patient.Cardiac_Axis.Value);

            pintHR.Set (selStep.Patient.VS_Settings.HR);
            pintRR.Set (selStep.Patient.VS_Settings.RR);
            pintSPO2.Set (selStep.Patient.VS_Settings.SPO2);
            pintETCO2.Set (selStep.Patient.VS_Settings.ETCO2);
            pintCVP.Set (selStep.Patient.VS_Settings.CVP);
            pintICP.Set (selStep.Patient.VS_Settings.ICP);
            pintIAP.Set (selStep.Patient.VS_Settings.IAP);
            pintPacemakerThreshold.Set (selStep.Patient.Pacemaker_Threshold);
            pintProgressFrom.Set (selStep.Step.ProgressFrom);
            pintProgressTo.Set (selStep.Step.ProgressTo);
            pintProgressTimer.Set (selStep.Step.ProgressTimer);

            pstrStepName.Set (selStep.Step.Name ?? "");
            pstrStepDescription.Set (selStep.Step.Description ?? "");

            UpdateOptionalProgressionView ();
        }

        private void UpdateOptionalProgressionView () {
            StackPanel stackOptionalProgressions = this.FindControl<StackPanel> ("stackOptionalProgressions");

            stackOptionalProgressions.Children.Clear ();

            for (int i = 0; i < selStep.Step.Progressions.Count; i++) {
                Scenario.Step.Progression p = selStep.Step.Progressions [i];
                PropertyOptProgression pp = new PropertyOptProgression ();
                pp.Init (i, p.DestinationIndex, p.Description);
                pp.PropertyChanged += updateProperty;
                stackOptionalProgressions.Children.Add (pp);
            }
        }

        private void updateProperty (object? sender, PropertyOptProgression.PropertyOptProgressionEventArgs e) {
            if (e.Index >= selStep.Step.Progressions.Count)
                return;

            Scenario.Step.Progression p = selStep.Step.Progressions [e.Index];
            p.DestinationIndex = e.IndexStepTo;
            p.Description = e.Description ?? "";

            // Deletes an optional progression via this route
            if (e.ToDelete) {
                selStep.Step.Progressions.RemoveAt (e.Index);
                UpdateOptionalProgressionView ();
                /* TODO: IMPLEMENT
                drawIProgressions ();
                */
            }
        }

        private void UpdateProperty (object? sender, PropertyString.PropertyStringEventArgs e) {
            switch (e.Key) {
                default: break;
                case PropertyString.Keys.ScenarioAuthor: ScenarioAuthor = e.Value ?? ""; break;
                case PropertyString.Keys.ScenarioName: ScenarioName = e.Value ?? ""; break;
                case PropertyString.Keys.ScenarioDescription: ScenarioDescription = e.Value ?? ""; break;
            }

            if (selStep != null) {
                switch (e.Key) {
                    default: break;
                    case PropertyString.Keys.StepName: selStep.SetName (e.Value ?? ""); break;
                    case PropertyString.Keys.StepDescription: selStep.Step.Description = e.Value ?? ""; break;
                }
            }
        }

        private void UpdateProperty (object? sender, PropertyInt.PropertyIntEventArgs e) {
            if (selStep != null) {
                switch (e.Key) {
                    default: break;
                    case PropertyInt.Keys.HR: selStep.Patient.HR = e.Value; break;
                    case PropertyInt.Keys.RR: selStep.Patient.RR = e.Value; break;
                    case PropertyInt.Keys.ETCO2: selStep.Patient.ETCO2 = e.Value; break;
                    case PropertyInt.Keys.SPO2: selStep.Patient.SPO2 = e.Value; break;
                    case PropertyInt.Keys.CVP: selStep.Patient.CVP = e.Value; break;
                    case PropertyInt.Keys.ICP: selStep.Patient.ICP = e.Value; break;
                    case PropertyInt.Keys.IAP: selStep.Patient.IAP = e.Value; break;
                    case PropertyInt.Keys.PacemakerThreshold: selStep.Patient.Pacemaker_Threshold = e.Value; break;

                    case PropertyInt.Keys.ProgressFrom: selStep.Step.ProgressFrom = e.Value; break;
                    case PropertyInt.Keys.ProgressTo: selStep.Step.ProgressTo = e.Value; break;
                    case PropertyInt.Keys.ProgressTimer: selStep.Step.ProgressTimer = e.Value; break;
                }
            }
        }

        private void UpdateProperty (object? sender, PropertyDouble.PropertyDoubleEventArgs e) {
            if (selStep != null) {
                switch (e.Key) {
                    default: break;
                    case PropertyDouble.Keys.T: selStep.Patient.T = e.Value ?? 0d; break;
                    case PropertyDouble.Keys.CO: selStep.Patient.CO = e.Value ?? 0d; break;
                    case PropertyDouble.Keys.RRInspiratoryRatio: selStep.Patient.RR_IE_I = e.Value ?? 0d; break;
                    case PropertyDouble.Keys.RRExpiratoryRatio: selStep.Patient.RR_IE_E = e.Value ?? 0d; break;
                }
            }
        }

        private void UpdateProperty (object? sender, PropertyBP.PropertyIntEventArgs e) {
            if (selStep != null) {
                switch (e.Key) {
                    default: break;
                    case PropertyBP.Keys.NSBP: selStep.Patient.NSBP = e.Value ?? 0; break;
                    case PropertyBP.Keys.NDBP: selStep.Patient.NDBP = e.Value ?? 0; break;
                    case PropertyBP.Keys.NMAP: selStep.Patient.NMAP = e.Value ?? 0; break;
                    case PropertyBP.Keys.ASBP: selStep.Patient.ASBP = e.Value ?? 0; break;
                    case PropertyBP.Keys.ADBP: selStep.Patient.ADBP = e.Value ?? 0; break;
                    case PropertyBP.Keys.AMAP: selStep.Patient.AMAP = e.Value ?? 0; break;
                    case PropertyBP.Keys.PSP: selStep.Patient.PSP = e.Value ?? 0; break;
                    case PropertyBP.Keys.PDP: selStep.Patient.PDP = e.Value ?? 0; break;
                    case PropertyBP.Keys.PMP: selStep.Patient.PMP = e.Value ?? 0; break;
                }
            }
        }

        private void UpdateProperty (object? sender, PropertyEnum.PropertyEnumEventArgs e) {
            if (e.Value != null && selStep != null) {
                switch (e.Key) {
                    default: break;

                    case PropertyEnum.Keys.Cardiac_Axis:
                        selStep.Patient.Cardiac_Axis.Value = (Cardiac_Axes.Values)Enum.Parse (typeof (Cardiac_Axes.Values), e.Value);
                        break;

                    case PropertyEnum.Keys.Cardiac_Rhythms:
                        selStep.Patient.Cardiac_Rhythm.Value = (Cardiac_Rhythms.Values)Enum.Parse (typeof (Cardiac_Rhythms.Values), e.Value);
                        break;

                    case PropertyEnum.Keys.Respiratory_Rhythms:
                        selStep.Patient.Respiratory_Rhythm.Value = (Respiratory_Rhythms.Values)Enum.Parse (typeof (Respiratory_Rhythms.Values), e.Value);
                        break;

                    case PropertyEnum.Keys.PACatheter_Rhythms:
                        selStep.Patient.PulmonaryArtery_Placement.Value = (PulmonaryArtery_Rhythms.Values)Enum.Parse (typeof (PulmonaryArtery_Rhythms.Values), e.Value);
                        break;
                }
            }
        }

        private void UpdateProperty (object? sender, PropertyCheck.PropertyCheckEventArgs e) {
            if (selStep != null) {
                switch (e.Key) {
                    default: break;
                    case PropertyCheck.Keys.PulsusParadoxus: selStep.Patient.Pulsus_Paradoxus = e.Value; break;
                    case PropertyCheck.Keys.PulsusAlternans: selStep.Patient.Pulsus_Alternans = e.Value; break;
                    case PropertyCheck.Keys.MechanicallyVentilated: selStep.Patient.Mechanically_Ventilated = e.Value; break;
                }
            }
        }

        private void UpdateProperty (object? sender, PropertyECGSegment.PropertyECGEventArgs e) {
            if (selStep != null) {
                switch (e.Key) {
                    default: break;
                    case PropertyECGSegment.Keys.STElevation: selStep.Patient.ST_Elevation = e.Values ?? new double [] { 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d }; break;
                    case PropertyECGSegment.Keys.TWave: selStep.Patient.T_Elevation = e.Values ?? new double [] { 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d }; break;
                }
            }
        }

        private void UpdateCardiacRhythm (object? sender, PropertyEnum.PropertyEnumEventArgs e) {
            CheckBox chkClampVitals = this.FindControl<CheckBox> ("chkClampVitals");
            if ((!chkClampVitals.IsChecked ?? false) || e.Value == null || selStep == null)
                return;

            Patient p = ((ItemStep)selStep).Patient;

            Cardiac_Rhythms.Default_Vitals v = Cardiac_Rhythms.DefaultVitals (
                (Cardiac_Rhythms.Values)Enum.Parse (typeof (Cardiac_Rhythms.Values), e.Value));

            p.HR = (int)II.Math.Clamp ((double)p.VS_Settings.HR, v.HRMin, v.HRMax);
            p.RR = (int)II.Math.Clamp ((double)p.VS_Settings.RR, v.RRMin, v.RRMax);
            p.SPO2 = (int)II.Math.Clamp ((double)p.VS_Settings.SPO2, v.SPO2Min, v.SPO2Max);
            p.ETCO2 = (int)II.Math.Clamp ((double)p.VS_Settings.ETCO2, v.ETCO2Min, v.ETCO2Max);
            p.NSBP = (int)II.Math.Clamp ((double)p.VS_Settings.NSBP, v.SBPMin, v.SBPMax);
            p.NDBP = (int)II.Math.Clamp ((double)p.VS_Settings.NDBP, v.DBPMin, v.DBPMax);
            p.ASBP = (int)II.Math.Clamp ((double)p.VS_Settings.ASBP, v.SBPMin, v.SBPMax);
            p.ADBP = (int)II.Math.Clamp ((double)p.VS_Settings.ADBP, v.DBPMin, v.DBPMax);
            p.PSP = (int)II.Math.Clamp ((double)p.VS_Settings.PSP, v.PSPMin, v.PSPMax);
            p.PDP = (int)II.Math.Clamp ((double)p.VS_Settings.PDP, v.PDPMin, v.PDPMax);

            UpdatePropertyView ();
        }

        private void UpdateRespiratoryRhythm (object? sender, PropertyEnum.PropertyEnumEventArgs e) {
            CheckBox chkClampVitals = this.FindControl<CheckBox> ("chkClampVitals");

            if ((!chkClampVitals.IsChecked ?? false) || e.Value == null || selStep == null)
                return;

            Patient p = ((ItemStep)selStep).Patient;

            Respiratory_Rhythms.Default_Vitals v = Respiratory_Rhythms.DefaultVitals (
                (Respiratory_Rhythms.Values)Enum.Parse (typeof (Respiratory_Rhythms.Values), e.Value));

            p.RR = (int)II.Math.Clamp ((double)p.RR, v.RRMin, v.RRMax);
            p.RR_IE_I = (int)II.Math.Clamp ((double)p.RR_IE_I, v.RR_IE_I_Min, v.RR_IE_I_Max);
            p.RR_IE_E = (int)II.Math.Clamp ((double)p.RR_IE_E, v.RR_IE_E_Min, v.RR_IE_E_Max);

            UpdatePropertyView ();
        }

        private void UpdatePACatheterRhythm (object? sender, PropertyEnum.PropertyEnumEventArgs e) {
            if (e.Value == null || selStep == null)
                return;

            Patient p = ((ItemStep)selStep).Patient;

            PulmonaryArtery_Rhythms.Default_Vitals v = PulmonaryArtery_Rhythms.DefaultVitals (
                (PulmonaryArtery_Rhythms.Values)Enum.Parse (typeof (PulmonaryArtery_Rhythms.Values), e.Value));

            p.PSP = (int)II.Math.Clamp ((double)p.PSP, v.PSPMin, v.PSPMax);
            p.PDP = (int)II.Math.Clamp ((double)p.PDP, v.PDPMin, v.PDPMax);

            UpdatePropertyView ();
        }

        /* TODO: IMPLEMENT
            private void selectStep (ItemStep ist) {
                selStep = ist;

                foreach (ItemStep i in Steps)
                    i.IStep.StrokeThickness = (i == selStep) ? i.StrokeThickness_Selected : i.StrokeThickness_Default;
            }

            private void selectStepEnd (ItemStep.UEIStepEnd iste) {
                selEnd = iste;

                foreach (ItemStep i in Steps)
                    i.IStepEnd.StrokeThickness = (i.IStepEnd == selEnd) ? i.StrokeThickness_Selected : i.StrokeThickness_Default;
            }

            private void addStep (ItemStep ist) {
                if (ist == null)
                    ist = new ItemStep ();

                // Init ItemStep
                ist.Init ();

                ist.IStep.MouseLeftButtonDown += IStep_MouseLeftButtonDown;
                ist.IStep.MouseLeftButtonUp += IStep_MouseLeftButtonUp;
                ist.IStep.MouseMove += IStep_MouseMove;

                ist.IStepEnd.MouseLeftButtonDown += IStepEnd_MouseLeftButtonDown;

                // Add to lists and display elements
                Steps.Add (ist);
                canvasDesigner.Children.Add (ist);
                Canvas.SetZIndex (ist, 1);

                // Select the added step, give a default name by its index
                selectStep (ist);
                ist.SetNumber (Steps.FindIndex (o => { return o == selStep; }));

                // Refresh the Properties View and draw Progression elements/colors
                updatePropertyView ();
                drawIProgressions ();

                expStepProperty.IsExpanded = true;
                expProgressionProperty.IsExpanded = true;
            }

            private void deleteStep (ItemStep ist) {
                int iStep = Steps.FindIndex (obj => { return obj == ist; });

                int iFrom = (ist.Step.ProgressFrom > iStep)
                    ? ist.Step.ProgressFrom - 1 : ist.Step.ProgressFrom;
                int iTo = (ist.Step.ProgressTo > iStep)
                    ? ist.Step.ProgressTo - 1 : ist.Step.ProgressTo;

                // Remove the selected Step from the stack and visual
                Steps.RemoveAt (iStep);
                canvasDesigner.Children.Remove (ist);
                foreach (ItemStep.UIEProgression uiep in ist.IProgressions)
                    canvasDesigner.Children.Remove (uiep);

                foreach (ItemStep s in Steps) {
                    // Adjust all references past the index -= 1
                    if (s.Step.ProgressTo > iStep)
                        s.Step.ProgressTo -= 1;
                    if (s.Step.ProgressFrom > iStep)
                        s.Step.ProgressFrom -= 1;

                    // Tie any references to the removed Step to its references Steps
                    if (s.Step.ProgressTo == iStep)
                        s.Step.ProgressTo = iTo;
                    if (s.Step.ProgressFrom == iStep)
                        s.Step.ProgressFrom = iFrom;

                    // Remove any optional Progressions that target the deleted Step
                    for (int i = 0; i < s.Step.Progressions.Count; i++) {
                        Scenario.Step.Progression p = s.Step.Progressions [i];

                        if (p.DestinationIndex == iStep)
                            s.Step.Progressions.RemoveAt (i);
                    }
                }

                // Set all Steps' indices for their Labels
                for (int i = 0; i < Steps.Count; i++)
                    Steps [i].SetNumber (i);

                // Refresh all IProgressions (visual lines)
                drawIProgressions ();
            }

            private void addProgression (ItemStep stepFrom, ItemStep stepTo) {
                if (stepFrom == stepTo)
                    return;

                int indexFrom = Steps.FindIndex (o => { return o == stepFrom; });
                int indexTo = Steps.FindIndex (o => { return o == stepTo; });

                if (stepTo.Step.ProgressFrom < 0)
                    stepTo.Step.ProgressFrom = indexFrom;

                if (stepFrom.Step.ProgressTo < 0)               // Create a default progression
                    stepFrom.Step.ProgressTo = indexTo;
                else                                            // Create an optional progression
                    stepFrom.Step.Progressions.Add (new Scenario.Step.Progression (indexTo));

                drawIProgressions ();
                updatePropertyView ();

                expStepProperty.IsExpanded = false;
                expProgressionProperty.IsExpanded = true;
            }

            private void deleteDefaultProgression () {
                selStep.Step.ProgressTo = -1;
                selStep.Step.ProgressTimer = -1;

                updatePropertyView ();
                drawIProgressions ();
            }

            private void loadIProgressions (object sender, EventArgs e) {
                if (isLoading) {
                    updateIProgressions ();
                    isLoading = false;
                }
            }

            private void drawIProgressions () {
                // Completely recreate and add all progression lines to list and canvas
                foreach (ItemStep iStep in Steps) {
                    foreach (ItemStep.UIEProgression uiep in iStep.IProgressions)
                        canvasDesigner.Children.Remove (uiep);

                    iStep.IProgressions.Clear ();

                    if (iStep.Step.ProgressTo > -1 && iStep.Step.ProgressTo < Steps.Count) {
                        // Draw default progress
                        ItemStep iTo = Steps [iStep.Step.ProgressTo];
                        ItemStep.UIEProgression uiep = new ItemStep.UIEProgression (iStep, iTo, canvasDesigner);
                        iStep.IProgressions.Add (uiep);
                    }

                    foreach (Scenario.Step.Progression p in iStep.Step.Progressions) {
                        if (p.DestinationIndex >= Steps.Count)
                            continue;

                        ItemStep iTo = Steps [p.DestinationIndex];
                        ItemStep.UIEProgression uiep = new ItemStep.UIEProgression (iStep, iTo, canvasDesigner);
                        iStep.IProgressions.Add (uiep);
                    }

                    // Add all new progression lines to canvas
                    foreach (ItemStep.UIEProgression uiep in iStep.IProgressions) {
                        canvasDesigner.Children.Add (uiep);
                        Canvas.SetZIndex (uiep, 0);
                    }

                    // Color IStepEnd depending on whether it has progressions
                    if (iStep?.IProgressions?.Count == 0)
                        iStep.IStepEnd.Fill = iStep.Fill_StepEndNoProgression;
                    else if (iStep?.IProgressions?.Count == 1)
                        iStep.IStepEnd.Fill = iStep.Fill_StepEndNoOptionalProgression;
                    else if (iStep?.IProgressions?.Count > 1)
                        iStep.IStepEnd.Fill = iStep.Fill_StepEndMultipleProgressions;
                }
            }

            private void updateIProgressions () {
                // Redraw progressions between sources to destinations
                foreach (ItemStep iStep in Steps) {
                    foreach (ItemStep.UIEProgression uiep in iStep.IProgressions)
                        uiep.UpdatePositions ();
                }
            }

            private void IStep_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) {
                selectStep (((ItemStep.UIEStep)sender).ItemStep);
                selectStepEnd (null);

                Mouse.Capture (sender as ItemStep.UIEStep);
                mouseCaptured = true;

                xShape = selStep.Left;
                yShape = selStep.Top;
                xCanvas = e.GetPosition (LayoutRoot).X;
                yCanvas = e.GetPosition (LayoutRoot).Y;

                updatePropertyView ();

                expStepProperty.IsExpanded = true;
                expProgressionProperty.IsExpanded = true;
            }

            private void IStep_MouseLeftButtonUp (object sender, MouseButtonEventArgs e) {
                Mouse.Capture (null);
                mouseCaptured = false;
            }

            private void IStep_MouseMove (object sender, MouseEventArgs e) {
                if (mouseCaptured) {
                    double x = e.GetPosition (LayoutRoot).X;
                    double y = e.GetPosition (LayoutRoot).Y;
                    xShape += x - xCanvas;
                    xCanvas = x;
                    yShape += y - yCanvas;
                    yCanvas = y;

                    ItemStep istep = ((ItemStep.UIEStep)sender).ItemStep;
                    Canvas.SetLeft (selStep, II.Math.Clamp (xShape, 0, canvasDesigner.ActualWidth - istep.ActualWidth));
                    Canvas.SetTop (selStep, II.Math.Clamp (yShape, 0, canvasDesigner.ActualHeight - istep.ActualHeight));
                    updateIProgressions ();
                }
            }

            private void IStepEnd_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) {
                selectStep (((ItemStep.UEIStepEnd)sender).ItemStep);
                selectStepEnd ((ItemStep.UEIStepEnd)sender);

                updatePropertyView ();

                expStepProperty.IsExpanded = false;
                expProgressionProperty.IsExpanded = true;
            }

            protected override void OnMouseLeftButtonUp (MouseButtonEventArgs e) {
                base.OnMouseLeftButtonUp (e);

                if (selEnd != null) {
                    if (Mouse.DirectlyOver is ItemStep.UEIStepEnd) {
                        addProgression (selStep, ((ItemStep.UEIStepEnd)Mouse.DirectlyOver).ItemStep);
                    } else if (Mouse.DirectlyOver is ItemStep.UIEStep)
                        addProgression (selStep, ((ItemStep.UIEStep)Mouse.DirectlyOver).ItemStep);
                    else if (Mouse.DirectlyOver is ItemStep)
                        addProgression (selStep, (ItemStep)Mouse.DirectlyOver);
                }
            }
        }
    }
         *
         *
         */
    }
}