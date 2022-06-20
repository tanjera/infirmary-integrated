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

namespace II_Scenario_Editor {

    public partial class Main : Window {

        // Master List of all Steps in the Scenario and other Scenario variables
        private List<ItemStep> Steps = new List<ItemStep> ();

        private string ScenarioAuthor, ScenarioName, ScenarioDescription;

        // Variables and pointers for using UI Elements
        private Canvas ICanvasSteps;

        private ItemStep? ISelectedStep;
        private bool IsSelectedStepEnd = false;

        // For copy/pasting Patient parameters
        private Patient CopiedPatient;

        // Variables for capturing mouse and dragging UI elements
        private Point? PointerPosition = null;

        // Switch for processing elements ina  loading sequence
        private bool IsLoading = false;

        public Main () {
            InitializeComponent ();

            DataContext = this;

            ICanvasSteps = this.FindControl<Canvas> ("cnvsDesigner");

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

            // Reset buffer parameters
            ISelectedStep = null;
            IsSelectedStepEnd = false;
            PointerPosition = null;

            // Clear master lists and UI elements
            ICanvasSteps.Children.Clear ();
            Steps.Clear ();

            // Clear scenario data
            ScenarioAuthor = "";
            ScenarioName = "";
            ScenarioDescription = "";

            UpdateScenarioProperty ();
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
            ICanvasSteps.Children.Clear ();

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
            AddStep ();
        }

        private void ButtonDuplicateStep_Click (object sender, RoutedEventArgs e) {
            if (ISelectedStep == null)
                return;

            AddStep (ISelectedStep);
        }

        private void BtnDeleteStep_Click (object sender, RoutedEventArgs e) {
            if (ISelectedStep != null)
                DeleteStep (ISelectedStep);
        }

        private async void BtnCopyPatient_Click (object sender, RoutedEventArgs e) {
            if (ISelectedStep == null)
                return;

            CopiedPatient = new Patient ();
            await CopiedPatient.Load_Process (ISelectedStep.Patient.Save ());
        }

        private async void BtnPastePatient_Click (object sender, RoutedEventArgs e) {
            if (ISelectedStep == null)
                return;

            if (CopiedPatient != null) {
                await ISelectedStep.Patient.Load_Process (CopiedPatient.Save ());
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
            PropertyInt pintProgressTimer = this.FindControl<PropertyInt> ("pintProgressTimer");

            PropertyString pstrProgressFrom = this.FindControl<PropertyString> ("pstrProgressFrom");
            PropertyString pstrProgressTo = this.FindControl<PropertyString> ("pstrProgressTo");
            PropertyString pstrStepName = this.FindControl<PropertyString> ("pstrStepName");
            PropertyString pstrStepDescription = this.FindControl<PropertyString> ("pstrStepDescription");

            // Initiate controls for editing Patient values
            pstrProgressFrom.Init (PropertyString.Keys.ProgressFrom);
            pstrProgressTo.Init (PropertyString.Keys.ProgressTo);
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
            pintProgressTimer.PropertyChanged += UpdateProperty;

            pstrProgressFrom.PropertyChanged += UpdateProperty;
            pstrProgressTo.PropertyChanged += UpdateProperty;
            pstrStepName.PropertyChanged += UpdateProperty;
            pstrStepDescription.PropertyChanged += UpdateProperty;
        }

        private void UpdatePropertyView () {
            if (ISelectedStep == null)
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
            PropertyInt pintProgressTimer = this.FindControl<PropertyInt> ("pintProgressTimer");

            PropertyString pstrProgressFrom = this.FindControl<PropertyString> ("pstrProgressFrom");
            PropertyString pstrProgressTo = this.FindControl<PropertyString> ("pstrProgressTo");
            PropertyString pstrStepName = this.FindControl<PropertyString> ("pstrStepName");
            PropertyString pstrStepDescription = this.FindControl<PropertyString> ("pstrStepDescription");

            // Update all controls with Patient values
            pbpNBP.Set (ISelectedStep.Patient.VS_Settings.NSBP, ISelectedStep.Patient.VS_Settings.NDBP);
            pbpABP.Set (ISelectedStep.Patient.VS_Settings.ASBP, ISelectedStep.Patient.VS_Settings.ADBP);
            pbpPBP.Set (ISelectedStep.Patient.VS_Settings.PSP, ISelectedStep.Patient.VS_Settings.PDP);

            pchkMechanicallyVentilated.Set (ISelectedStep.Patient.Mechanically_Ventilated);
            pchkPulsusParadoxus.Set (ISelectedStep.Patient.Pulsus_Paradoxus);
            pchkPulsusAlternans.Set (ISelectedStep.Patient.Pulsus_Alternans);

            pdblT.Set (ISelectedStep.Patient.VS_Settings.T);
            pdblCO.Set (ISelectedStep.Patient.VS_Settings.CO);
            pdblInspiratoryRatio.Set (ISelectedStep.Patient.VS_Settings.RR_IE_I);
            pdblExpiratoryRatio.Set (ISelectedStep.Patient.VS_Settings.RR_IE_E);

            pecgSTSegment.Set (ISelectedStep.Patient.ST_Elevation);
            pecgTWave.Set (ISelectedStep.Patient.T_Elevation);

            penmCardiacRhythms.Set ((int)ISelectedStep.Patient.Cardiac_Rhythm.Value);
            penmRespiratoryRhythms.Set ((int)ISelectedStep.Patient.Respiratory_Rhythm.Value);
            penmPACatheterRhythm.Set ((int)ISelectedStep.Patient.PulmonaryArtery_Placement.Value);
            penmCardiacAxis.Set ((int)ISelectedStep.Patient.Cardiac_Axis.Value);

            pintHR.Set (ISelectedStep.Patient.VS_Settings.HR);
            pintRR.Set (ISelectedStep.Patient.VS_Settings.RR);
            pintSPO2.Set (ISelectedStep.Patient.VS_Settings.SPO2);
            pintETCO2.Set (ISelectedStep.Patient.VS_Settings.ETCO2);
            pintCVP.Set (ISelectedStep.Patient.VS_Settings.CVP);
            pintICP.Set (ISelectedStep.Patient.VS_Settings.ICP);
            pintIAP.Set (ISelectedStep.Patient.VS_Settings.IAP);
            pintPacemakerThreshold.Set (ISelectedStep.Patient.Pacemaker_Threshold);
            pintProgressTimer.Set (ISelectedStep.Step.ProgressTimer);

            pstrProgressFrom.Set (ISelectedStep.Step.ProgressFrom);
            pstrProgressTo.Set (ISelectedStep.Step.ProgressTo);
            pstrStepName.Set (ISelectedStep.Step.Name ?? "");
            pstrStepDescription.Set (ISelectedStep.Step.Description ?? "");

            UpdateOptionalProgressionView ();
        }

        private void UpdateOptionalProgressionView () {
            StackPanel stackOptionalProgressions = this.FindControl<StackPanel> ("stackOptionalProgressions");
            stackOptionalProgressions.Children.Clear ();

            if (ISelectedStep != null) {
                for (int i = 0; i < ISelectedStep.Step.Progressions.Count; i++) {
                    Scenario.Step.Progression p = ISelectedStep.Step.Progressions [i];
                    PropertyOptProgression pp = new PropertyOptProgression ();
                    pp.Init (i, p.ToStepUUID, p.Description);
                    pp.PropertyChanged += updateProperty;
                    stackOptionalProgressions.Children.Add (pp);
                }
            }
        }

        private void updateProperty (object? sender, PropertyOptProgression.PropertyOptProgressionEventArgs e) {
            if (e.Index >= ISelectedStep.Step.Progressions.Count)
                return;

            Scenario.Step.Progression p = ISelectedStep.Step.Progressions [e.Index];
            p.ToStepUUID = e.StepToUUID;
            p.Description = e.Description ?? "";

            // Deletes an optional progression via this route
            if (e.ToDelete) {
                ISelectedStep.Step.Progressions.RemoveAt (e.Index);
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

            if (ISelectedStep != null) {
                switch (e.Key) {
                    default: break;
                    case PropertyString.Keys.ProgressFrom: ISelectedStep.Step.ProgressFrom = e.Value; break;
                    case PropertyString.Keys.ProgressTo: ISelectedStep.Step.ProgressTo = e.Value; break;

                    case PropertyString.Keys.StepName: ISelectedStep.SetName (e.Value ?? ""); break;
                    case PropertyString.Keys.StepDescription: ISelectedStep.Step.Description = e.Value ?? ""; break;
                }
            }
        }

        private void UpdateProperty (object? sender, PropertyInt.PropertyIntEventArgs e) {
            if (ISelectedStep != null) {
                switch (e.Key) {
                    default: break;
                    case PropertyInt.Keys.HR: ISelectedStep.Patient.HR = e.Value; break;
                    case PropertyInt.Keys.RR: ISelectedStep.Patient.RR = e.Value; break;
                    case PropertyInt.Keys.ETCO2: ISelectedStep.Patient.ETCO2 = e.Value; break;
                    case PropertyInt.Keys.SPO2: ISelectedStep.Patient.SPO2 = e.Value; break;
                    case PropertyInt.Keys.CVP: ISelectedStep.Patient.CVP = e.Value; break;
                    case PropertyInt.Keys.ICP: ISelectedStep.Patient.ICP = e.Value; break;
                    case PropertyInt.Keys.IAP: ISelectedStep.Patient.IAP = e.Value; break;
                    case PropertyInt.Keys.PacemakerThreshold: ISelectedStep.Patient.Pacemaker_Threshold = e.Value; break;
                    case PropertyInt.Keys.ProgressTimer: ISelectedStep.Step.ProgressTimer = e.Value; break;
                }
            }
        }

        private void UpdateProperty (object? sender, PropertyDouble.PropertyDoubleEventArgs e) {
            if (ISelectedStep != null) {
                switch (e.Key) {
                    default: break;
                    case PropertyDouble.Keys.T: ISelectedStep.Patient.T = e.Value ?? 0d; break;
                    case PropertyDouble.Keys.CO: ISelectedStep.Patient.CO = e.Value ?? 0d; break;
                    case PropertyDouble.Keys.RRInspiratoryRatio: ISelectedStep.Patient.RR_IE_I = e.Value ?? 0d; break;
                    case PropertyDouble.Keys.RRExpiratoryRatio: ISelectedStep.Patient.RR_IE_E = e.Value ?? 0d; break;
                }
            }
        }

        private void UpdateProperty (object? sender, PropertyBP.PropertyIntEventArgs e) {
            if (ISelectedStep != null) {
                switch (e.Key) {
                    default: break;
                    case PropertyBP.Keys.NSBP: ISelectedStep.Patient.NSBP = e.Value ?? 0; break;
                    case PropertyBP.Keys.NDBP: ISelectedStep.Patient.NDBP = e.Value ?? 0; break;
                    case PropertyBP.Keys.NMAP: ISelectedStep.Patient.NMAP = e.Value ?? 0; break;
                    case PropertyBP.Keys.ASBP: ISelectedStep.Patient.ASBP = e.Value ?? 0; break;
                    case PropertyBP.Keys.ADBP: ISelectedStep.Patient.ADBP = e.Value ?? 0; break;
                    case PropertyBP.Keys.AMAP: ISelectedStep.Patient.AMAP = e.Value ?? 0; break;
                    case PropertyBP.Keys.PSP: ISelectedStep.Patient.PSP = e.Value ?? 0; break;
                    case PropertyBP.Keys.PDP: ISelectedStep.Patient.PDP = e.Value ?? 0; break;
                    case PropertyBP.Keys.PMP: ISelectedStep.Patient.PMP = e.Value ?? 0; break;
                }
            }
        }

        private void UpdateProperty (object? sender, PropertyEnum.PropertyEnumEventArgs e) {
            if (e.Value != null && ISelectedStep != null) {
                switch (e.Key) {
                    default: break;

                    case PropertyEnum.Keys.Cardiac_Axis:
                        ISelectedStep.Patient.Cardiac_Axis.Value = (Cardiac_Axes.Values)Enum.Parse (typeof (Cardiac_Axes.Values), e.Value);
                        break;

                    case PropertyEnum.Keys.Cardiac_Rhythms:
                        ISelectedStep.Patient.Cardiac_Rhythm.Value = (Cardiac_Rhythms.Values)Enum.Parse (typeof (Cardiac_Rhythms.Values), e.Value);
                        break;

                    case PropertyEnum.Keys.Respiratory_Rhythms:
                        ISelectedStep.Patient.Respiratory_Rhythm.Value = (Respiratory_Rhythms.Values)Enum.Parse (typeof (Respiratory_Rhythms.Values), e.Value);
                        break;

                    case PropertyEnum.Keys.PACatheter_Rhythms:
                        ISelectedStep.Patient.PulmonaryArtery_Placement.Value = (PulmonaryArtery_Rhythms.Values)Enum.Parse (typeof (PulmonaryArtery_Rhythms.Values), e.Value);
                        break;
                }
            }
        }

        private void UpdateProperty (object? sender, PropertyCheck.PropertyCheckEventArgs e) {
            if (ISelectedStep != null) {
                switch (e.Key) {
                    default: break;
                    case PropertyCheck.Keys.PulsusParadoxus: ISelectedStep.Patient.Pulsus_Paradoxus = e.Value; break;
                    case PropertyCheck.Keys.PulsusAlternans: ISelectedStep.Patient.Pulsus_Alternans = e.Value; break;
                    case PropertyCheck.Keys.MechanicallyVentilated: ISelectedStep.Patient.Mechanically_Ventilated = e.Value; break;
                }
            }
        }

        private void UpdateProperty (object? sender, PropertyECGSegment.PropertyECGEventArgs e) {
            if (ISelectedStep != null) {
                switch (e.Key) {
                    default: break;
                    case PropertyECGSegment.Keys.STElevation: ISelectedStep.Patient.ST_Elevation = e.Values ?? new double [] { 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d }; break;
                    case PropertyECGSegment.Keys.TWave: ISelectedStep.Patient.T_Elevation = e.Values ?? new double [] { 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d }; break;
                }
            }
        }

        private void UpdateCardiacRhythm (object? sender, PropertyEnum.PropertyEnumEventArgs e) {
            CheckBox chkClampVitals = this.FindControl<CheckBox> ("chkClampVitals");
            if ((!chkClampVitals.IsChecked ?? false) || e.Value == null || ISelectedStep == null)
                return;

            Patient p = ((ItemStep)ISelectedStep).Patient;

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

            if ((!chkClampVitals.IsChecked ?? false) || e.Value == null || ISelectedStep == null)
                return;

            Patient p = ((ItemStep)ISelectedStep).Patient;

            Respiratory_Rhythms.Default_Vitals v = Respiratory_Rhythms.DefaultVitals (
                (Respiratory_Rhythms.Values)Enum.Parse (typeof (Respiratory_Rhythms.Values), e.Value));

            p.RR = (int)II.Math.Clamp ((double)p.RR, v.RRMin, v.RRMax);
            p.RR_IE_I = (int)II.Math.Clamp ((double)p.RR_IE_I, v.RR_IE_I_Min, v.RR_IE_I_Max);
            p.RR_IE_E = (int)II.Math.Clamp ((double)p.RR_IE_E, v.RR_IE_E_Min, v.RR_IE_E_Max);

            UpdatePropertyView ();
        }

        private void UpdatePACatheterRhythm (object? sender, PropertyEnum.PropertyEnumEventArgs e) {
            if (e.Value == null || ISelectedStep == null)
                return;

            Patient p = ((ItemStep)ISelectedStep).Patient;

            PulmonaryArtery_Rhythms.Default_Vitals v = PulmonaryArtery_Rhythms.DefaultVitals (
                (PulmonaryArtery_Rhythms.Values)Enum.Parse (typeof (PulmonaryArtery_Rhythms.Values), e.Value));

            p.PSP = (int)II.Math.Clamp ((double)p.PSP, v.PSPMin, v.PSPMax);
            p.PDP = (int)II.Math.Clamp ((double)p.PDP, v.PDPMin, v.PDPMax);

            UpdatePropertyView ();
        }

        private void UpdateItemBorders () {
            foreach (ItemStep i in Steps) {
                i.SetBorder_Step (i == ISelectedStep);
                i.SetBorder_End (IsSelectedStepEnd && i == ISelectedStep);
            }
        }

        private async void AddStep (ItemStep? incItem = null) {
            ItemStep item = new ();

            if (incItem != null) {  // Duplicate
                // Copy interface properties and interface item properties
                item.SetName (incItem.Label);

                // Copy data structures
                item.Step.Name = incItem.Step.Name;
                item.Step.Description = incItem.Step.Description;

                await item.Step.Patient.Load_Process (incItem.Patient.Save ());
            }

            // Add to lists and display elements
            Steps.Add (item);
            ICanvasSteps.Children.Add (item);
            item.ZIndex = 1;

            // Select the added step, give a default name by its index
            item.SetNumber (Steps.FindIndex (o => { return o == item; }));

            item.PointerPressed += Item_PointerPressed;
            item.PointerReleased += Item_PointerReleased;
            item.PointerMoved += Item_PointerMoved;
            item.IStepEnd.PointerPressed += Item_PointerPressed;

            // Refresh the Properties View and draw Progression elements/colors
            UpdatePropertyView ();
            UpdateItemBorders ();
            /* TODO: IMPLEMENT
            drawIProgressions ();
            */

            Expander expStepProperty = this.FindControl<Expander> ("expStepProperty");
            Expander expProgressionProperty = this.FindControl<Expander> ("expProgressionProperty");

            expStepProperty.IsExpanded = true;
            expProgressionProperty.IsExpanded = true;
        }

        private void DeleteStep (ItemStep item) {
            // Remove the selected Step from the stack and visual
            Steps.Remove (item);
            ICanvasSteps.Children.Remove (item);

            foreach (Line line in item.IProgressions)
                ICanvasSteps.Children.Remove (line);

            foreach (ItemStep s in Steps) {
                // Nullify any default progressions that
                if (s.Step.ProgressTo == item.UUID)
                    s.Step.ProgressTo = null;

                // Remove all optional progressions targeting the Step being removed
                for (int i = s.Step.Progressions.Count - 1; i >= 0; i--) {
                    if (s.Step.Progressions [i].ToStepUUID == item.UUID)
                        s.Step.Progressions.RemoveAt (i);
                }
            }

            // Set all Steps' indices for their Labels
            for (int i = 0; i < Steps.Count; i++)
                Steps [i].SetNumber (i);

            // Refresh all IProgressions (visual lines)
            /* TODO: IMPLEMENT
            drawIProgressions ();
            */
        }

        private void Item_PointerPressed (object? sender, PointerPressedEventArgs e) {
            if (PointerPosition != null)                        // An object on a stack of Controls may have already been pressed!
                return;

            if (sender is ItemStep) {
                ISelectedStep = (ItemStep)sender;
                IsSelectedStepEnd = false;

                e.Pointer.Capture ((ItemStep)sender);
            } else if (sender is ItemStepEnd) {
                ISelectedStep = ((ItemStepEnd)sender).Step;
                IsSelectedStepEnd = true;

                e.Pointer.Capture (((ItemStepEnd)sender).Step);
            }

            PointerPosition = e.GetPosition (null);

            UpdateItemBorders ();
        }

        private void Item_PointerReleased (object? sender, PointerReleasedEventArgs e) {
            e.Pointer.Capture (null);
            PointerPosition = null;

            UpdateItemBorders ();
        }

        private void Item_PointerMoved (object? sender, PointerEventArgs e) {
            if (PointerPosition != null && sender != null && sender is ItemStep && ISelectedStep == (ItemStep)sender) {
                Point p = e.GetPosition (null);
                Point deltaPosition = p - (Point)PointerPosition;
                PointerPosition = p;

                double left = ISelectedStep.Bounds.Left + deltaPosition.X;
                double top = ISelectedStep.Bounds.Top + deltaPosition.Y;

                Canvas.SetLeft (ISelectedStep, left);
                Canvas.SetTop (ISelectedStep, top);
            }
        }

        /* TODO: IMPLEMENT
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
        */
    }
}