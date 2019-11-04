using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

using II;
using II.Scenario_Editor.Controls;

namespace II.Scenario_Editor {

    public partial class Editor : Window {

        // Master List of all Steps in the Scenario and other Scenario variables
        private List<ItemStep> Steps = new List<ItemStep> ();

        private string ScenarioAuthor, ScenarioName, ScenarioDescription;

        // Variables and pointers for using UI Elements
        private Canvas canvasDesigner;

        private ItemStep selStep;
        private ItemStep.UEIStepEnd selEnd;

        // For copy/pasting Patient parameters
        private Patient copiedPatient;

        // Variables for capturing mouse and dragging UI elements
        private bool mouseCaptured = false;

        private double xShape, yShape,
            xCanvas, yCanvas;

        // Switch for processing elements ina  loading sequence
        private bool isLoading = false;

        // Define WPF UI commands for binding
        private ICommand icNewFile, icLoadFile, icSaveFile;

        public ICommand IC_NewFile { get { return icNewFile; } }
        public ICommand IC_LoadFile { get { return icLoadFile; } }
        public ICommand IC_SaveFile { get { return icSaveFile; } }

        public Editor () {
            InitializeComponent ();
            DataContext = this;

            canvasDesigner = cnvsDesigner;

            // Initiate ICommands for KeyBindings
            icNewFile = new ActionCommand (() => newScenario ());
            icLoadFile = new ActionCommand (() => loadSequence ());
            icSaveFile = new ActionCommand (() => saveScenario ());

            initScenarioProperty ();
            initPropertyView ();
        }

        private MessageBoxResult promptUnsavedWork () {
            if (Steps.Count > 0)
                return MessageBox.Show (
                        "Are you sure you want to continue? All unsaved work will be lost!",
                        "Lose Unsaved Work?",
                        MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            else
                return MessageBoxResult.OK;
        }

        private void newScenario () {
            if (promptUnsavedWork () != MessageBoxResult.OK)
                return;

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

            updateScenarioProperty ();
        }

        private void loadSequence () {
            if (promptUnsavedWork () != MessageBoxResult.OK)
                return;

            string filepath = loadDialog ();
            if (String.IsNullOrEmpty (filepath))
                return;

            loadFile (filepath);
        }

        private string loadDialog () {
            Microsoft.Win32.OpenFileDialog dlgLoad = new Microsoft.Win32.OpenFileDialog ();

            dlgLoad.Filter = "Infirmary Integrated simulation files (*.ii)|*.ii|All files (*.*)|*.*";
            dlgLoad.FilterIndex = 1;
            dlgLoad.RestoreDirectory = true;

            if (dlgLoad.ShowDialog () == true)
                return dlgLoad.FileName;
            else
                return null;
        }

        private void loadFile (string filepath) {
            StreamReader sr = new StreamReader (filepath);

            // Read savefile metadata indicating data formatting
            // Supports II:T1 file structure
            string metadata = sr.ReadLine ();
            if (!metadata.StartsWith (".ii:t1")) {
                loadFail ();
                return;
            }

            // Savefile type 1: validated and encrypted
            // Line 1 is metadata (.ii:t1)
            // Line 2 is hash for validation (hash taken of raw string data, unobfuscated)
            // Line 3 is savefile data encrypted by AES encoding
            string hash = sr.ReadLine ().Trim ();
            string file = Encryption.DecryptAES (sr.ReadToEnd ().Trim ());

            if (hash != Encryption.HashSHA256 (file)) {
                loadFail ();
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
                loadFail ();
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
            }

            // Refresh the Properties View with the newly selected step
            selectStep (Steps.Count > 0 ? Steps [0] : null);
            updatePropertyView ();
            updateScenarioProperty ();
            drawIProgressions ();
            expStepProperty.IsExpanded = true;
            expProgressionProperty.IsExpanded = true;
        }

        private void loadFail () {
            MessageBox.Show (
                    "The selected file was unable to be loaded. Perhaps the file was damaged or edited outside of Infirmary Integrated.",
                    "Unable to Load File", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void saveScenario () {

            // Prepare Scenario for saving
            Scenario sc = new Scenario (false);

            sc.Updated = DateTime.Now;
            sc.Author = ScenarioAuthor;
            sc.Name = ScenarioName;
            sc.Description = ScenarioDescription;

            for (int i = 0; i < Steps.Count; i++) {

                // Set metadata for saving
                Steps [i].Step.IPositionX = Steps [i].Left;
                Steps [i].Step.IPositionY = Steps [i].Top;

                // And add to the main Scenario stack
                sc.Steps.Add (Steps [i].Step);
            }

            // Initiate IO stream, show Save File dialog to select file destination
            Stream s;
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
        }

        private void initScenarioProperty () {

            // Initiate controls for editing Scenario properties
            pstrScenarioAuthor.Init (PropertyString.Keys.ScenarioAuthor);
            pstrScenarioAuthor.PropertyChanged += updateProperty;

            pstrScenarioName.Init (PropertyString.Keys.ScenarioName);
            pstrScenarioName.PropertyChanged += updateProperty;

            pstrScenarioDescription.Init (PropertyString.Keys.ScenarioDescription);
            pstrScenarioDescription.PropertyChanged += updateProperty;
        }

        private void updateScenarioProperty () {
            pstrScenarioAuthor.Set (ScenarioAuthor ?? "");
            pstrScenarioName.Set (ScenarioName ?? "");
            pstrScenarioDescription.Set (ScenarioDescription ?? "");
        }

        private void initPropertyView () {

            // Populate enum string lists for readable display
            List<string> cardiacRhythms = new List<string> (),
                respiratoryRhythms = new List<string> (),
                pulmonaryRhythms = new List<string> (),
                cardiacAxes = new List<string> (),
                intensityScale = new List<string> (),
                fetalHeartRhythms = new List<string> ();

            foreach (Cardiac_Rhythms.Values v in Enum.GetValues (typeof (Cardiac_Rhythms.Values)))
                cardiacRhythms.Add (App.Language.Dictionary [Cardiac_Rhythms.LookupString (v)]);

            foreach (Respiratory_Rhythms.Values v in Enum.GetValues (typeof (Respiratory_Rhythms.Values)))
                respiratoryRhythms.Add (App.Language.Dictionary [Respiratory_Rhythms.LookupString (v)]);

            foreach (PulmonaryArtery_Rhythms.Values v in Enum.GetValues (typeof (PulmonaryArtery_Rhythms.Values)))
                pulmonaryRhythms.Add (App.Language.Dictionary [PulmonaryArtery_Rhythms.LookupString (v)]);

            foreach (Cardiac_Axes.Values v in Enum.GetValues (typeof (Cardiac_Axes.Values)))
                cardiacAxes.Add (App.Language.Dictionary [Cardiac_Axes.LookupString (v)]);

            // Initiate controls for editing Patient values
            pstrStepName.Init (PropertyString.Keys.StepName);
            pstrStepName.PropertyChanged += updateProperty;

            pstrStepDescription.Init (PropertyString.Keys.StepDescription);
            pstrStepDescription.PropertyChanged += updateProperty;

            pintHR.Init (PropertyInt.Keys.HR, 5, 0, 500);
            pintHR.PropertyChanged += updateProperty;

            pbpNBP.Init (PropertyBP.Keys.NSBP,
                5, 0, 300,
                5, 0, 200);
            pbpNBP.PropertyChanged += updateProperty;

            pintRR.Init (PropertyInt.Keys.RR, 2, 0, 100);
            pintRR.PropertyChanged += updateProperty;

            pintSPO2.Init (PropertyInt.Keys.SPO2, 2, 0, 100);
            pintSPO2.PropertyChanged += updateProperty;

            pdblT.Init (PropertyDouble.Keys.T, 0.2, 0, 100);
            pdblT.PropertyChanged += updateProperty;

            penmCardiacRhythms.Init (PropertyEnum.Keys.Cardiac_Rhythms,
                Enum.GetNames (typeof (Cardiac_Rhythms.Values)), cardiacRhythms);
            penmCardiacRhythms.PropertyChanged += updateProperty;
            penmCardiacRhythms.PropertyChanged += updateCardiacRhythm;

            penmRespiratoryRhythms.Init (PropertyEnum.Keys.Respiratory_Rhythms,
                Enum.GetNames (typeof (Respiratory_Rhythms.Values)), respiratoryRhythms);
            penmRespiratoryRhythms.PropertyChanged += updateProperty;
            penmRespiratoryRhythms.PropertyChanged += updateRespiratoryRhythm;

            pintETCO2.Init (PropertyInt.Keys.ETCO2, 2, 0, 100);
            pintETCO2.PropertyChanged += updateProperty;

            pintCVP.Init (PropertyInt.Keys.CVP, 1, -100, 100);
            pintCVP.PropertyChanged += updateProperty;

            pbpABP.Init (PropertyBP.Keys.ASBP,
                5, 0, 300,
                5, 0, 200);
            pbpABP.PropertyChanged += updateProperty;

            penmPACatheterRhythm.Init (PropertyEnum.Keys.PACatheter_Rhythms,
                Enum.GetNames (typeof (PulmonaryArtery_Rhythms.Values)), pulmonaryRhythms);
            penmPACatheterRhythm.PropertyChanged += updateProperty;
            penmPACatheterRhythm.PropertyChanged += updatePACatheterRhythm;

            pbpPBP.Init (PropertyBP.Keys.PSP,
                5, 0, 200,
                5, 0, 200);
            pbpPBP.PropertyChanged += updateProperty;

            pintICP.Init (PropertyInt.Keys.ICP, 1, -100, 100);
            pintICP.PropertyChanged += updateProperty;

            pintIAP.Init (PropertyInt.Keys.IAP, 1, -100, 100);
            pintIAP.PropertyChanged += updateProperty;

            pchkMechanicallyVentilated.Init (PropertyCheck.Keys.MechanicallyVentilated);
            pchkMechanicallyVentilated.PropertyChanged += updateProperty;

            pdblInspiratoryRatio.Init (PropertyFloat.Keys.RRInspiratoryRatio, 0.1, 0.1, 10);
            pdblInspiratoryRatio.PropertyChanged += updateProperty;

            pdblExpiratoryRatio.Init (PropertyFloat.Keys.RRExpiratoryRatio, 0.1, 0.1, 10);
            pdblExpiratoryRatio.PropertyChanged += updateProperty;

            pintPacemakerThreshold.Init (PropertyInt.Keys.PacemakerThreshold, 5, 0, 200);
            pintPacemakerThreshold.PropertyChanged += updateProperty;

            pchkPulsusParadoxus.Init (PropertyCheck.Keys.PulsusParadoxus);
            pchkPulsusParadoxus.PropertyChanged += updateProperty;

            pchkPulsusAlternans.Init (PropertyCheck.Keys.PulsusAlternans);
            pchkPulsusAlternans.PropertyChanged += updateProperty;

            penmCardiacAxis.Init (PropertyEnum.Keys.Cardiac_Axis,
                Enum.GetNames (typeof (Cardiac_Axes.Values)), cardiacAxes);
            penmCardiacAxis.PropertyChanged += updateProperty;

            pecgSTSegment.Init (PropertyECGSegment.Keys.STElevation);
            pecgSTSegment.PropertyChanged += updateProperty;

            pecgTWave.Init (PropertyECGSegment.Keys.TWave);
            pecgTWave.PropertyChanged += updateProperty;

            pintProgressFrom.Init (PropertyInt.Keys.ProgressFrom, 1, -1, 1000);
            pintProgressFrom.PropertyChanged += updateProperty;

            pintProgressTo.Init (PropertyInt.Keys.ProgressTo, 1, -1, 1000);
            pintProgressTo.PropertyChanged += updateProperty;

            pintProgressTimer.Init (PropertyInt.Keys.ProgressTimer, 1, -1, 1000);
            pintProgressTimer.PropertyChanged += updateProperty;
        }

        private void updatePropertyView () {
            if (selStep == null)
                return;

            // Update all controls with Patient values
            pstrStepName.Set (selStep.Step.Name ?? "");
            pstrStepDescription.Set (selStep.Step.Description ?? "");
            pintHR.Set (selStep.Patient.VS_Settings.HR);
            pbpNBP.Set (selStep.Patient.VS_Settings.NSBP, selStep.Patient.VS_Settings.NDBP);
            pintRR.Set (selStep.Patient.VS_Settings.RR);
            pintSPO2.Set (selStep.Patient.VS_Settings.SPO2);
            pdblT.Set (selStep.Patient.VS_Settings.T);
            penmCardiacRhythms.Set ((int)selStep.Patient.Cardiac_Rhythm.Value);
            penmRespiratoryRhythms.Set ((int)selStep.Patient.Respiratory_Rhythm.Value);
            pintETCO2.Set (selStep.Patient.VS_Settings.ETCO2);
            pintCVP.Set (selStep.Patient.VS_Settings.CVP);
            pbpABP.Set (selStep.Patient.VS_Settings.ASBP, selStep.Patient.VS_Settings.ADBP);
            penmPACatheterRhythm.Set ((int)selStep.Patient.PulmonaryArtery_Placement.Value);
            pbpPBP.Set (selStep.Patient.VS_Settings.PSP, selStep.Patient.VS_Settings.PDP);
            pintICP.Set (selStep.Patient.VS_Settings.ICP);
            pintIAP.Set (selStep.Patient.VS_Settings.IAP);
            pchkMechanicallyVentilated.Set (selStep.Patient.Mechanically_Ventilated);
            pdblInspiratoryRatio.Set (selStep.Patient.VS_Settings.RR_IE_I);
            pdblExpiratoryRatio.Set (selStep.Patient.VS_Settings.RR_IE_E);
            pintPacemakerThreshold.Set (selStep.Patient.Pacemaker_Threshold);
            pchkPulsusParadoxus.Set (selStep.Patient.Pulsus_Paradoxus);
            pchkPulsusAlternans.Set (selStep.Patient.Pulsus_Alternans);
            penmCardiacAxis.Set ((int)selStep.Patient.Cardiac_Axis.Value);
            pecgSTSegment.Set (selStep.Patient.ST_Elevation);
            pecgTWave.Set (selStep.Patient.T_Elevation);

            // Update progression controls with values
            pintProgressFrom.Set (selStep.Step.ProgressFrom);
            pintProgressTo.Set (selStep.Step.ProgressTo);
            pintProgressTimer.Set (selStep.Step.ProgressTimer);

            updateOptionalProgressionView ();
        }

        private void updateOptionalProgressionView () {
            stackOptionalProgressions.Children.Clear ();

            for (int i = 0; i < selStep.Step.Progressions.Count; i++) {
                Scenario.Step.Progression p = selStep.Step.Progressions [i];
                PropertyOptProgression pp = new PropertyOptProgression ();
                pp.Init (i, p.DestinationIndex, p.Description);
                pp.PropertyChanged += updateProperty;
                stackOptionalProgressions.Children.Add (pp);
            }
        }

        private void updateProperty (object sender, PropertyOptProgression.PropertyOptProgressionEventArgs e) {
            if (e.Index >= selStep.Step.Progressions.Count)
                return;

            Scenario.Step.Progression p = selStep.Step.Progressions [e.Index];
            p.DestinationIndex = e.IndexStepTo;
            p.Description = e.Description;

            // Deletes an optional progression via this route
            if (e.ToDelete) {
                selStep.Step.Progressions.RemoveAt (e.Index);
                updateOptionalProgressionView ();
                drawIProgressions ();
            }
        }

        private void updateProperty (object sender, PropertyString.PropertyStringEventArgs e) {
            switch (e.Key) {
                default: break;
                case PropertyString.Keys.ScenarioAuthor: ScenarioAuthor = e.Value; break;
                case PropertyString.Keys.ScenarioName: ScenarioName = e.Value; break;
                case PropertyString.Keys.ScenarioDescription: ScenarioDescription = e.Value; break;
                case PropertyString.Keys.StepName: selStep.SetName (e.Value); break;
                case PropertyString.Keys.StepDescription: selStep.Step.Description = e.Value; break;
            }
        }

        private void updateProperty (object sender, PropertyInt.PropertyIntEventArgs e) {
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

        private void updateProperty (object sender, PropertyDouble.PropertyDoubleEventArgs e) {
            switch (e.Key) {
                default: break;
                case PropertyDouble.Keys.T: selStep.Patient.T = e.Value; break;
            }
        }

        private void updateProperty (object sender, PropertyFloat.PropertyFloatEventArgs e) {
            switch (e.Key) {
                default: break;
                case PropertyFloat.Keys.RRInspiratoryRatio: selStep.Patient.RR_IE_I = e.Value; break;
                case PropertyFloat.Keys.RRExpiratoryRatio: selStep.Patient.RR_IE_E = e.Value; break;
            }
        }

        private void updateProperty (object sender, PropertyBP.PropertyIntEventArgs e) {
            switch (e.Key) {
                default: break;
                case PropertyBP.Keys.NSBP: selStep.Patient.NSBP = e.Value; break;
                case PropertyBP.Keys.NDBP: selStep.Patient.NDBP = e.Value; break;
                case PropertyBP.Keys.NMAP: selStep.Patient.NMAP = e.Value; break;
                case PropertyBP.Keys.ASBP: selStep.Patient.ASBP = e.Value; break;
                case PropertyBP.Keys.ADBP: selStep.Patient.ADBP = e.Value; break;
                case PropertyBP.Keys.AMAP: selStep.Patient.AMAP = e.Value; break;
                case PropertyBP.Keys.PSP: selStep.Patient.PSP = e.Value; break;
                case PropertyBP.Keys.PDP: selStep.Patient.PDP = e.Value; break;
                case PropertyBP.Keys.PMP: selStep.Patient.PMP = e.Value; break;
            }
        }

        private void updateProperty (object sender, PropertyEnum.PropertyEnumEventArgs e) {
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

        private void updateProperty (object sender, PropertyCheck.PropertyCheckEventArgs e) {
            switch (e.Key) {
                default: break;
                case PropertyCheck.Keys.PulsusParadoxus: selStep.Patient.Pulsus_Paradoxus = e.Value; break;
                case PropertyCheck.Keys.PulsusAlternans: selStep.Patient.Pulsus_Alternans = e.Value; break;
                case PropertyCheck.Keys.MechanicallyVentilated: selStep.Patient.Mechanically_Ventilated = e.Value; break;
            }
        }

        private void updateProperty (object sender, PropertyECGSegment.PropertyECGEventArgs e) {
            switch (e.Key) {
                default: break;
                case PropertyECGSegment.Keys.STElevation: selStep.Patient.ST_Elevation = e.Values; break;
                case PropertyECGSegment.Keys.TWave: selStep.Patient.T_Elevation = e.Values; break;
            }
        }

        private void updateCardiacRhythm (object sender, PropertyEnum.PropertyEnumEventArgs e) {
            if (!chkClampVitals.IsChecked ?? false || selStep == null)
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

            updatePropertyView ();
        }

        private void updateRespiratoryRhythm (object sender, PropertyEnum.PropertyEnumEventArgs e) {
            if (!chkClampVitals.IsChecked ?? false || selStep == null)
                return;

            Patient p = ((ItemStep)selStep).Patient;

            Respiratory_Rhythms.Default_Vitals v = Respiratory_Rhythms.DefaultVitals (
                (Respiratory_Rhythms.Values)Enum.Parse (typeof (Respiratory_Rhythms.Values), e.Value));

            p.RR = (int)II.Math.Clamp ((double)p.RR, v.RRMin, v.RRMax);
            p.RR_IE_I = (int)II.Math.Clamp ((double)p.RR_IE_I, v.RR_IE_I_Min, v.RR_IE_I_Max);
            p.RR_IE_E = (int)II.Math.Clamp ((double)p.RR_IE_E, v.RR_IE_E_Min, v.RR_IE_E_Max);

            updatePropertyView ();
        }

        private void updatePACatheterRhythm (object sender, PropertyEnum.PropertyEnumEventArgs e) {
            if (selStep == null)
                return;

            Patient p = ((ItemStep)selStep).Patient;

            PulmonaryArtery_Rhythms.Default_Vitals v = PulmonaryArtery_Rhythms.DefaultVitals (
                (PulmonaryArtery_Rhythms.Values)Enum.Parse (typeof (PulmonaryArtery_Rhythms.Values), e.Value));

            p.PSP = (int)II.Math.Clamp ((double)p.PSP, v.PSPMin, v.PSPMax);
            p.PDP = (int)II.Math.Clamp ((double)p.PDP, v.PDPMin, v.PDPMax);

            updatePropertyView ();
        }

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

        private void ButtonAddStep_Click (object sender, RoutedEventArgs e)
            => addStep (null);

        private void ButtonDuplicateStep_Click (object sender, RoutedEventArgs e) {
            if (selStep == null)
                return;

            addStep (selStep.Duplicate ());
        }

        private void BtnDeleteStep_Click (object sender, RoutedEventArgs e) {
            if (selStep == null)
                return;

            deleteStep (selStep);
        }

        private void BtnCopyPatient_Click (object sender, RoutedEventArgs e) {
            if (selStep == null)
                return;

            copiedPatient = new Patient ();
            copiedPatient.Load_Process (selStep.Patient.Save ());
        }

        private void BtnPastePatient_Click (object sender, RoutedEventArgs e) {
            if (selStep == null)
                return;

            if (copiedPatient != null) {
                selStep.Patient.Load_Process (copiedPatient.Save ());
            }

            updatePropertyView ();
        }

        private void BtnDeleteDefaultProgression_Click (object sender, RoutedEventArgs e)
            => deleteDefaultProgression ();

        private void MenuItemNew_Click (object sender, RoutedEventArgs e)
            => newScenario ();

        private void MenuItemLoad_Click (object sender, RoutedEventArgs e)
            => loadSequence ();

        private void MenuSave_Click (object sender, RoutedEventArgs e)
            => saveScenario ();

        private void MenuItemExit_Click (object sender, RoutedEventArgs e)
            => Application.Current.Shutdown ();

        private void MenuItemAbout_Click (object sender, RoutedEventArgs e) {
            About dlgAbout = new About ();
            dlgAbout.ShowDialog ();
        }

        private void CanvasDesigner_DragDropped (object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent (DataFormats.FileDrop)) {
                string [] files = (string [])e.Data.GetData (DataFormats.FileDrop);
                if (files.Length > 0)
                    loadFile (files [0]);
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