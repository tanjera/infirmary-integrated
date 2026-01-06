using II;

using II;

using II.Localization;
using II.Server;

using IISIM;

using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Xceed.Wpf.Toolkit;
using Xceed.Wpf.Toolkit.Primitives;

namespace IISIM.Windows {

    /// <summary>
    /// Interaction logic for Control.xaml
    /// </summary>
    public partial class Control : Window {
        public App? Instance;

        private bool HideDeviceLabels = false;
        private bool IsUILoadCompleted = false;

        /* Buffers for ViewModel handling and temporal smoothing of upstream Model data changes */
        private Physiology? ApplyBuffer;

        private bool ApplyPending_Cardiac = false,
                     ApplyPending_Respiratory = false,
                     ApplyPending_Obstetric = false;

        private II.Timer ApplyTimer_Cardiac = new (),
                      ApplyTimer_Respiratory = new (),
                      ApplyTimer_Obstetric = new ();

        /* Variables for Auto-Apply functionality */
        private ParameterStatuses ParameterStatus = ParameterStatuses.Loading;

        private enum ParameterStatuses {
            Loading,
            AutoApply,
            ChangesPending,
            ChangesApplied
        }

        public Control (App? app) {
            InitializeComponent ();

            Instance = app;

            Init ();
        }

        public Control () {
            InitializeComponent ();

            Instance = (IISIM.App)App.Current;

            Init ();
        }

        private void Init () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (Init)}");
                return;
            }

            DataContext = this;

            Activated += this.OnActivated;
            Closed += this.OnClosed;

            Instance.MainWindow = this;

            InitInitialRun ();

            /* Init essential functions first */
            InitInterface ();
            InitMirroring ();
            InitScenario (true);
            InitTimers ();

            App.Current.Dispatcher.InvokeAsync (async () => {
                /* Init important but non-essential functions */
                if (Instance.StartArgs?.Length > 0) {
                    string loadfile = Instance.StartArgs [0].Trim (' ', '\n', '\r');
                    if (!String.IsNullOrEmpty (loadfile))
                        await LoadOpen (loadfile);
                }

                /* Update UI from loading functionality */
                await SetParameterStatus (Instance?.Settings.AutoApplyChanges ?? false);

                /* Run useful but otherwise vanity functions last */

                await InitUpgrade ();
            });
        }

        private void InitInitialRun () {
            if (!II.Settings.Simulator.Exists () || !(Instance?.Settings.AcceptedEULA ?? false)) {
                DialogEULA ();
            }
        }

        private void InitInterface () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (InitInterface)}");
                return;
            }

            /* Populate UI strings per language selection */

            wdwControl.Title = Instance.Language.Localize ("PE:WindowTitle");
            menuNew.Header = Instance.Language.Localize ("PE:MenuNewFile");
            menuFile.Header = Instance.Language.Localize ("PE:MenuFile");
            menuLoad.Header = Instance.Language.Localize ("PE:MenuLoadSimulation");
            menuSave.Header = Instance.Language.Localize ("PE:MenuSaveSimulation");
            menuToggleFullscreen.Header = Instance.Language.Localize ("MENU:MenuToggleFullscreen");
            menuExit.Header = Instance.Language.Localize ("PE:MenuExitProgram");

            menuOptions.Header = Instance.Language.Localize ("PE:MenuOptions");
            menuPauseSimulation.Header = Instance.Language.Localize ("PE:MenuPause");

            menuMirror.Header = Instance.Language.Localize ("PE:MenuMirror");
            menuMirrorDeactivate.Header = Instance.Language.Localize ("PE:MenuMirrorDeactivate");
            menuMirrorReceive.Header = Instance.Language.Localize ("PE:MenuMirrorReceive");
            menuMirrorBroadcast.Header = Instance.Language.Localize ("PE:MenuMirrorBroadcast");

            menuSettings.Header = Instance.Language.Localize ("PE:MenuSettings");
            menuToggleAudio.Header = String.Format ("{0}: {1}",
                Instance.Language.Localize ("PE:MenuToggleAudio"),
                Instance.Settings.AudioEnabled ? Instance.Language.Localize ("BOOLEAN:On") : Instance.Language.Localize ("BOOLEAN:Off"));
            menuSetLanguage.Header = Instance.Language.Localize ("PE:MenuSetLanguage");

            menuHelp.Header = Instance.Language.Localize ("PE:MenuHelp");
            menuCheckUpdate.Header = Instance.Language.Localize ("PE:MenuCheckUpdates");
            menuAbout.Header = Instance.Language.Localize ("PE:MenuAboutProgram");

            gbDevices.Header = Instance.Language.Localize ("PE:Devices");
            lblDeviceMonitor.Content = Instance.Language.Localize ("PE:CardiacMonitor");
            lblDevice12LeadECG.Content = Instance.Language.Localize ("PE:12LeadECG");
            lblDeviceDefibrillator.Content = Instance.Language.Localize ("PE:Defibrillator");
            lblDeviceIABP.Content = Instance.Language.Localize ("PE:IABP");
            lblDeviceEFM.Content = Instance.Language.Localize ("PE:EFM");

            gbOptions.Header = Instance.Language.Localize ("PE:Options");
            lblOptionsHide.Content = Instance.Language.Localize ("PE:HideDevices");

            lblGroupScenarioPlayer.Content = Instance.Language.Localize ("PE:ScenarioPlayer");
            lblProgressionOptions.Header = Instance.Language.Localize ("PE:ProgressionOptions");

            lblGroupVitalSigns.Content = Instance.Language.Localize ("PE:VitalSigns");
            lblHR.Content = $"{Instance.Language.Localize ("PE:HeartRate")}:";
            lblNIBP.Content = $"{Instance.Language.Localize ("PE:BloodPressure")}:";
            lblRR.Content = $"{Instance.Language.Localize ("PE:RespiratoryRate")}:";
            lblSPO2.Content = $"{Instance.Language.Localize ("PE:PulseOximetry")}:";
            lblT.Content = $"{Instance.Language.Localize ("PE:Temperature")}:";
            lblCardiacRhythm.Content = $"{Instance.Language.Localize ("PE:CardiacRhythm")}:";
            checkDefaultVitals.Content = Instance.Language.Localize ("PE:UseDefaultVitalSignRanges");

            lblGroupHemodynamics.Content = Instance.Language.Localize ("PE:AdvancedHemodynamics");
            lblETCO2.Content = $"{Instance.Language.Localize ("PE:EndTidalCO2")}:";
            lblCVP.Content = $"{Instance.Language.Localize ("PE:CentralVenousPressure")}:";
            lblASBP.Content = $"{Instance.Language.Localize ("PE:ArterialBloodPressure")}:";
            lblPACatheterPlacement.Content = $"{Instance.Language.Localize ("PE:PulmonaryArteryCatheterPlacement")}:";
            lblCO.Content = $"{Instance.Language.Localize ("PE:CardiacOutput")}:";
            lblPSP.Content = $"{Instance.Language.Localize ("PE:PulmonaryArteryPressure")}:";
            lblICP.Content = $"{Instance.Language.Localize ("PE:IntracranialPressure")}:";
            lblIAP.Content = $"{Instance.Language.Localize ("PE:IntraabdominalPressure")}:";

            lblGroupRespiratoryProfile.Content = Instance.Language.Localize ("PE:RespiratoryProfile");
            lblRespiratoryRhythm.Content = $"{Instance.Language.Localize ("PE:RespiratoryRhythm")}:";
            lblMechanicallyVentilated.Content = $"{Instance.Language.Localize ("PE:MechanicallyVentilated")}:";
            lblInspiratoryRatio.Content = $"{Instance.Language.Localize ("PE:InspiratoryExpiratoryRatio")}:";

            lblGroupCardiacProfile.Content = Instance.Language.Localize ("PE:CardiacProfile");
            lblPacemakerCaptureThreshold.Content = $"{Instance.Language.Localize ("PE:PacemakerCaptureThreshold")}:";
            lblPulsusParadoxus.Content = $"{Instance.Language.Localize ("PE:PulsusParadoxus")}:";
            lblPulsusAlternans.Content = $"{Instance.Language.Localize ("PE:PulsusAlternans")}:";
            lblElectricalAlternans.Content = $"{Instance.Language.Localize ("PE:ElectricalAlternans")}:";
            lblQRSInterval.Content = $"{Instance.Language.Localize ("PE:QRSInterval")}:";
            lblQTcInterval.Content = $"{Instance.Language.Localize ("PE:QTcInterval")}:";
            lblCardiacAxis.Content = $"{Instance.Language.Localize ("PE:CardiacAxis")}:";
            grpSTSegmentElevation.Header = Instance.Language.Localize ("PE:STSegmentElevation");
            grpTWaveElevation.Header = Instance.Language.Localize ("PE:TWaveElevation");

            lblGroupObstetricProfile.Content = Instance.Language.Localize ("PE:ObstetricProfile");
            lblFHR.Content = $"{Instance.Language.Localize ("PE:FetalHeartRate")}:";
            lblFHRRhythms.Content = $"{Instance.Language.Localize ("PE:FetalHeartRhythms")}:";
            lblFHRVariability.Content = $"{Instance.Language.Localize ("PE:FetalHeartRateVariability")}:";
            lblUCFrequency.Content = $"{Instance.Language.Localize ("PE:UterineContractionFrequency")}:";
            lblUCDuration.Content = $"{Instance.Language.Localize ("PE:UterineContractionDuration")}:";
            lblUCIntensity.Content = $"{Instance.Language.Localize ("PE:UterineContractionIntensity")}:";
            lblUCResting.Content = $"{Instance.Language.Localize ("PE:UterineRestingTone")}:";

            lblAutoApplyChanges.Content = Instance.Language.Localize ("BUTTON:AutoApplyChanges");
            lblParametersApply.Content = Instance.Language.Localize ("BUTTON:ApplyChanges");
            lblParametersReset.Content = Instance.Language.Localize ("BUTTON:ResetParameters");

            chkAutoApplyChanges.IsChecked = Instance.Settings.AutoApplyChanges;
            btnParametersReset.IsEnabled = !Instance.Settings.AutoApplyChanges;

            ItemCollection icCardiacRhythms = comboCardiacRhythm.Items;
            foreach (Cardiac_Rhythms.Values v in Enum.GetValues (typeof (Cardiac_Rhythms.Values)))
                icCardiacRhythms.Add (new ComboBoxItem () {
                    Content = Instance.Language.Localize (Cardiac_Rhythms.LookupString (v))
                });

            ItemCollection icRespiratoryRhythms = comboRespiratoryRhythm.Items;
            foreach (Respiratory_Rhythms.Values v in Enum.GetValues (typeof (Respiratory_Rhythms.Values)))
                icRespiratoryRhythms.Add (new ComboBoxItem () {
                    Tag = v.ToString (),
                    Content = Instance.Language.Localize (Respiratory_Rhythms.LookupString (v))
                });

            ItemCollection icPACatheterPlacement = comboPACatheterPlacement.Items;
            foreach (PulmonaryArtery_Rhythms.Values v in Enum.GetValues (typeof (PulmonaryArtery_Rhythms.Values)))
                icPACatheterPlacement.Add (new ComboBoxItem () {
                    Tag = v.ToString (),
                    Content = Instance.Language.Localize (PulmonaryArtery_Rhythms.LookupString (v))
                });

            ItemCollection icCardiacAxes = comboCardiacAxis.Items;
            foreach (Cardiac_Axes.Values v in Enum.GetValues (typeof (Cardiac_Axes.Values)))
                icCardiacAxes.Add (new ComboBoxItem () {
                    Tag = v.ToString (),
                    Content = Instance.Language.Localize (Cardiac_Axes.LookupString (v))
                });

            ItemCollection icFetalHeartRhythms = comboFHRRhythm.Items;
            foreach (FetalHeart_Rhythms.Values v in Enum.GetValues (typeof (FetalHeart_Rhythms.Values)))
                icFetalHeartRhythms.Add (new ComboBoxItem () {
                    Tag = v.ToString (),
                    Content = Instance.Language.Localize (FetalHeart_Rhythms.LookupString (v))
                });

            /* Init Hotkeys (Commands & InputBinding) */

            RoutedCommand
                cmdMenuNewSimulation_Click = new (),
                cmdMenuLoadFile_Click = new (),
                cmdMenuSaveFile_Click = new (),
                cmdMenuPauseSimulation_Click = new (),
                cmdMenuToggleFullscreen_Click = new (),
                cmdMenuToggleAudio_Click = new ();

            cmdMenuNewSimulation_Click.InputGestures.Add (new KeyGesture (Key.N, ModifierKeys.Control));
            CommandBindings.Add (new CommandBinding (cmdMenuNewSimulation_Click, MenuNewSimulation_Click));

            cmdMenuLoadFile_Click.InputGestures.Add (new KeyGesture (Key.O, ModifierKeys.Control));
            CommandBindings.Add (new CommandBinding (cmdMenuLoadFile_Click, MenuLoadFile_Click));

            cmdMenuSaveFile_Click.InputGestures.Add (new KeyGesture (Key.S, ModifierKeys.Control));
            CommandBindings.Add (new CommandBinding (cmdMenuSaveFile_Click, MenuSaveFile_Click));

            cmdMenuPauseSimulation_Click.InputGestures.Add (new KeyGesture (Key.Pause));
            CommandBindings.Add (new CommandBinding (cmdMenuPauseSimulation_Click, MenuPauseSimulation_Click));

            cmdMenuToggleFullscreen_Click.InputGestures.Add (new KeyGesture (Key.Enter, ModifierKeys.Alt));
            CommandBindings.Add (new CommandBinding (cmdMenuToggleFullscreen_Click, MenuToggleFullscreen_Click));

            cmdMenuToggleAudio_Click.InputGestures.Add (new KeyGesture (Key.A, ModifierKeys.Control));
            CommandBindings.Add (new CommandBinding (cmdMenuToggleAudio_Click, MenuToggleAudio_Click));
        }

        private async Task InitUpgrade () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (InitUpgrade)}");
                return;
            }

            // Newer version available? Check Server, populate status bar, prompt user for upgrade
            await Instance.Server.Get_LatestVersion ();

            string version = Assembly.GetExecutingAssembly ()?.GetName ()?.Version?.ToString (3) ?? "0.0.0";
            bool upgradeAvailable = Utility.IsNewerVersion (version, Instance.Server.UpgradeVersion);

            if (!upgradeAvailable) {    // If no update available, no status update; remove any notification muting
                Instance.Settings.MuteUpgrade = false;
                Instance.Settings.Save ();
                return;
            }

            if (Instance.Settings.MuteUpgrade) {
                if (DateTime.Compare (Instance.Settings.MuteUpgradeDate, DateTime.Now - new TimeSpan (30, 0, 0, 0)) < 0) {
                    Instance.Settings.MuteUpgrade = false;              // Reset the notification mute every 30 days
                    Instance.Settings.Save ();
                } else {        // Mutes update popup notification
                    return;
                }
            }

            // Show the upgrade dialog to the user
            await DialogUpgrade ();
        }

        private void InitMirroring () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (InitMirroring)}");
                return;
            }

            Instance.Timer_Main.Elapsed += Instance.Mirror.ProcessTimer;
            Instance.Mirror.timerUpdate.Tick += OnMirrorTick;

            Task.WhenAll (
                Instance.Mirror.timerUpdate.ResetStart (5000),
                UpdateMirrorStatus ()
            );
        }

        private void InitScenario (bool toInit) {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (InitScenario)}");
                return;
            }

            Instance.Scenario = new Scenario (Instance.Settings, toInit);
            Instance.Scenario.StepChangeRequest += OnStepChangeRequest;     // Allows unlinking of Timers immediately prior to Step change
            Instance.Scenario.StepChanged += OnStepChanged;                 // Updates IIApp.Patient, allows PatientEditor UI to update
            Instance.Timer_Main.Elapsed += Instance.Scenario.ProcessTimer;

            if (toInit)         // If toInit is false, Patient is null- InitPatient() will need to be called manually
                InitScenarioStep ();
        }

        private Task UnloadScenario () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (UnloadScenario)}");
                return Task.CompletedTask;
            }

            if (Instance.Scenario != null) {
                Instance.Timer_Main.Elapsed -= Instance.Scenario.ProcessTimer;   // Unlink Scenario from App/Main Timer
                Instance.Scenario.Dispose ();        // Disposes Scenario's events and timer, and all Patients' events and timers
            }

            return Task.CompletedTask;
        }

        private void NewScenario () => _ = RefreshScenario (true);

        private async Task RefreshScenario (bool toInit) {
            await UnloadScenario ();
            InitScenario (toInit);

            await UpdateExpanders ();
        }

        private void InitTimers () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (InitTimers)}");
                return;
            }

            /* Tie the Patient's Timer to the Main Timer */
            
            Instance.Timer_Main.Elapsed += Instance.Settings.ProcessTime;
            Instance.Settings.State = II.Settings.Simulator.States.Running;
            
            Instance.Timer_Main.Elapsed += ApplyTimer_Cardiac.Process;
            Instance.Timer_Main.Elapsed += ApplyTimer_Respiratory.Process;
            Instance.Timer_Main.Elapsed += ApplyTimer_Obstetric.Process;

            ApplyTimer_Cardiac.Tick += ApplyPhysiologyParameters_Cardiac;
            ApplyTimer_Respiratory.Tick += ApplyPhysiologyParameters_Respiratory;
            ApplyTimer_Obstetric.Tick += ApplyPhysiologyParameters_Obstetric;

            Task.WhenAll (
                ApplyTimer_Cardiac.Set (5000),
                ApplyTimer_Respiratory.Set (5000),
                ApplyTimer_Obstetric.Set (30000)
            );
        }

        private void InitScenarioStep () {
            InitPhysiologyEvents ();
            InitStep ();

            if (Instance?.Device_Monitor is not null && Instance?.Scenario?.DeviceMonitor is not null) {
                Instance?.Device_Monitor.SetNumerics (Instance.Scenario.DeviceMonitor);
                Instance?.Device_Monitor.SetTracings (Instance.Scenario.DeviceMonitor);
                Instance?.Device_Monitor.RefreshLayout ();
            }
        }

        private void InitPhysiologyEvents () {
            if (Instance?.Physiology is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (InitPhysiologyEvents)}");
                return;
            }

            Instance.Timer_Main.Elapsed += Instance.Physiology.ProcessTimers;
            
            /* Tie PatientEvents to the PatientEditor UI! And trigger. */
            Instance.Physiology.PhysiologyEvent += OnPhysiologyEvent;

            /* Tie PatientEvents to each device! So devices change and trace according to the patient! */
            if (Instance.Physiology is not null) {
                if (Instance.Device_Monitor is not null)
                    Instance.Physiology.PhysiologyEvent += Instance.Device_Monitor.OnPhysiologyEvent;
                if (Instance.Device_Defib is not null)
                    Instance.Physiology.PhysiologyEvent += Instance.Device_Defib.OnPhysiologyEvent;
                if (Instance.Device_ECG is not null)
                    Instance.Physiology.PhysiologyEvent += Instance.Device_ECG.OnPhysiologyEvent;
                if (Instance.Device_EFM is not null)
                    Instance.Physiology.PhysiologyEvent += Instance.Device_EFM.OnPhysiologyEvent;
                if (Instance.Device_IABP is not null)
                    Instance.Physiology.PhysiologyEvent += Instance.Device_IABP.OnPhysiologyEvent;
            }

            OnPhysiologyEvent (this, new Physiology.PhysiologyEventArgs (
                Instance.Physiology, 
                Physiology.PhysiologyEventTypes.Vitals_Change,
                Instance.Settings.Time));
        }

        private async Task UnloadPatientEvents () {
            if (Instance?.Physiology is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (UnloadPatientEvents)}");
                return;
            }

            /* Unloading the Patient from the Main Timer also stops all the Patient's Timers
            /* and results in that Patient not triggering PatientEvent's */
            Instance.Timer_Main.Elapsed -= Instance.Physiology.ProcessTimers;

            /* But it's still important to clear PatientEvent subscriptions so they're not adding
            /* as duplicates when InitPatientEvents() is called!! */
            await Instance.Physiology.UnsubscribePhysiologyEvent ();
        }

        private async Task InitDeviceMonitor () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (InitDeviceMonitor)}");
                return;
            }

            await App.Current.Dispatcher.InvokeAsync (() => {
                if (Instance.Device_Monitor is null || Instance.Device_Monitor.State == DeviceMonitor.States.Closed)
                    Instance.Device_Monitor = new DeviceMonitor (Instance);
                
                Instance.Device_Monitor.Activate ();
                Instance.Device_Monitor.Show ();

                if (Instance.Physiology is not null)
                    Instance.Physiology.PhysiologyEvent += Instance.Device_Monitor.OnPhysiologyEvent;
            });
        }

        private async Task InitDeviceECG () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (InitDeviceECG)}");
                return;
            }

            await App.Current.Dispatcher.InvokeAsync (() => {
                if (Instance.Device_ECG is null || Instance.Device_ECG.State == DeviceECG.States.Closed)
                    Instance.Device_ECG = new DeviceECG (Instance);

                Instance.Device_ECG.Activate ();
                Instance.Device_ECG.Show ();

                if (Instance.Physiology is not null)
                    Instance.Physiology.PhysiologyEvent += Instance.Device_ECG.OnPhysiologyEvent;
            });
        }

        private async Task InitDeviceDefib () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (InitDeviceDefib)}");
                return;
            }

            await App.Current.Dispatcher.InvokeAsync (() => {
                if (Instance.Device_Defib is null || Instance.Device_Defib.State == DeviceDefib.States.Closed)
                    Instance.Device_Defib = new DeviceDefib (Instance);

                Instance.Device_Defib.Activate ();
                Instance.Device_Defib.Show ();

                if (Instance.Physiology is not null)
                    Instance.Physiology.PhysiologyEvent += Instance.Device_Defib.OnPhysiologyEvent;
            });
        }

        private async Task InitDeviceIABP () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (InitDeviceIABP)}");
                return;
            }

            await App.Current.Dispatcher.InvokeAsync (() => {
                if (Instance.Device_IABP is null || Instance.Device_IABP.State == DeviceIABP.States.Closed)
                    Instance.Device_IABP = new DeviceIABP (Instance);

                Instance.Device_IABP.Activate ();
                Instance.Device_IABP.Show ();

                if (Instance.Physiology is not null)
                    Instance.Physiology.PhysiologyEvent += Instance.Device_IABP.OnPhysiologyEvent;
            });
        }

        private async Task InitDeviceEFM () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (InitDeviceEFM)}");
                return;
            }
            await App.Current.Dispatcher.InvokeAsync (() => {
                if (Instance.Device_EFM is null || Instance.Device_EFM.State == DeviceEFM.States.Closed)
                    Instance.Device_EFM = new DeviceEFM (Instance);

                Instance.Device_EFM.Activate ();
                Instance.Device_EFM.Show ();

                if (Instance.Physiology is not null)
                    Instance.Physiology.PhysiologyEvent += Instance.Device_EFM.OnPhysiologyEvent;
            });
        }

        private void DialogEULA () {
            App.Current.Dispatcher.InvokeAsync (() => {
                DialogEULA dlg = new (Instance);
                dlg.Activate ();
                dlg.ShowDialog ();
            });
        }

        private async Task DialogLanguage (bool reloadUI = false) {
            await App.Current.Dispatcher.InvokeAsync (() => {
                var oldLang = Instance?.Language.Value;
                DialogLanguage dlg = new (Instance);
                dlg.Activate ();
                dlg.ShowDialog ();

                reloadUI = oldLang != Instance?.Language.Value;

                if (reloadUI)
                    InitInterface ();
            });
        }

        private async Task DialogMirrorBroadcast () {
            await App.Current.Dispatcher.InvokeAsync (async () => {
                DialogMirrorBroadcast dlg = new (Instance);
                dlg.Activate ();
                dlg.ShowDialog ();

                if (Instance is not null)
                    await Instance?.Mirror?.PostStep (
                        new Scenario.Step (Instance.Settings) {
                            Physiology = Instance.Physiology ?? new Physiology (Instance.Settings),
                        },
                        Instance.Server);
            });
        }

        private async Task DialogMirrorReceive () {
            await App.Current.Dispatcher.InvokeAsync (() => {
                DialogMirrorReceive dlg = new (Instance);
                dlg.Activate ();
                dlg.ShowDialog ();
            });
        }

        public async Task DialogAbout () {
            await App.Current.Dispatcher.InvokeAsync (() => {
                DialogAbout dlg = new (Instance);
                dlg.Activate ();
                dlg.ShowDialog ();
            });
        }

        private async Task DialogUpgrade () {
            await App.Current.Dispatcher.InvokeAsync (() => {
                DialogUpgrade.UpgradeOptions decision = IISIM.Windows.DialogUpgrade.UpgradeOptions.None;

                DialogUpgrade dlg = new (Instance);
                dlg.Activate ();

                dlg.OnUpgradeRoute += (s, ea) => decision = ea.Route;

                dlg.ShowDialog ();

                dlg.OnUpgradeRoute -= (s, ea) => decision = ea.Route;

                switch (decision) {
                    default:
                    case IISIM.Windows.DialogUpgrade.UpgradeOptions.None:
                    case IISIM.Windows.DialogUpgrade.UpgradeOptions.Delay:
                        return;

                    case IISIM.Windows.DialogUpgrade.UpgradeOptions.Mute:
                        if (Instance is not null) {
                            Instance.Settings.MuteUpgrade = true;
                            Instance.Settings.MuteUpgradeDate = DateTime.Now;
                            Instance.Settings.Save ();
                        }
                        return;

                    case IISIM.Windows.DialogUpgrade.UpgradeOptions.Website:
                        string url = String.IsNullOrEmpty (Instance?.Server.UpgradeWebpage)
                            ? "https://github.com/tanjera/infirmary-integrated/releases".Replace ("&", "^&")
                            : Instance.Server.UpgradeWebpage;
                        Process.Start (new ProcessStartInfo ("cmd", $"/c start {url}") { CreateNoWindow = true });
                        return;
                }
            });
        }

        public async Task ToggleAudio () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (ToggleAudio)}");
                return;
            }

            await SetAudio (!Instance.Settings.AudioEnabled);
        }

        public void SetAudio_On () => _ = SetAudio (true);

        public void SetAudio_Off () => _ = SetAudio (false);

        public async Task SetAudio (bool toSet) {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (SetAudio)}");
                return;
            }

            Instance.Settings.AudioEnabled = toSet;
            Instance.Settings.Save ();

            await App.Current.Dispatcher.InvokeAsync (() => {
                menuToggleAudio.Header = String.Format ("{0}: {1}",
                    Instance.Language.Localize ("PE:MenuToggleAudio"),
                    Instance.Settings.AudioEnabled ? Instance.Language.Localize ("BOOLEAN:On") : Instance.Language.Localize ("BOOLEAN:Off"));
            });
        }

        private async Task ToggleHideDevices () {
            HideDeviceLabels = !HideDeviceLabels;

            await App.Current.Dispatcher.InvokeAsync (() => {
                panelDevicesExpanded.Visibility = HideDeviceLabels ? Visibility.Hidden : Visibility.Visible;
                panelDevicesHidden.Visibility = HideDeviceLabels ? Visibility.Visible : Visibility.Hidden; ;

                colPanelDevices.MaxWidth = HideDeviceLabels
                ? panelDevicesHidden.ActualWidth + panelDevicesHidden.Margin.Left + panelDevicesHidden.Margin.Right
                : panelDevicesExpanded.ActualWidth + panelDevicesExpanded.Margin.Left + panelDevicesExpanded.Margin.Right;
            });
        }

        private async Task CheckUpgrade () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (CheckUpgrade)}");
                return;
            }

            // Check with server for updated version of Infirmary Integrated- notify user either way
            await Instance.Server.Get_LatestVersion ();

            string version = Assembly.GetExecutingAssembly ()?.GetName ()?.Version?.ToString (3) ?? "0.0.0";
            if (Utility.IsNewerVersion (version, Instance.Server.UpgradeVersion)) {
                await DialogUpgrade ();
            } else {
                await App.Current.Dispatcher.InvokeAsync (() => {
                    System.Windows.MessageBox.Show (
                    Instance.Language.Localize ("UPGRADE:NoUpdateAvailable"),
                    Instance.Language.Localize ("UPGRADE:Upgrade"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        private Task OpenUpgrade () {
            string url = String.IsNullOrEmpty (Instance?.Server.UpgradeWebpage)
                            ? "https://github.com/tanjera/infirmary-integrated/releases".Replace ("&", "^&")
                            : Instance.Server.UpgradeWebpage;
            Process.Start (new ProcessStartInfo ("cmd", $"/c start {url}") { CreateNoWindow = true });
            return Task.CompletedTask;
        }

        private async Task MirrorDeactivate () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (MirrorDeactivate)}");
                return;
            }

            Instance.Mirror.Status = Mirror.Statuses.INACTIVE;

            await UpdateMirrorStatus ();
            await UpdateExpanders (false);
        }

        private async Task MirrorBroadcast () {
            await DialogMirrorBroadcast ();

            await UpdateMirrorStatus ();
            await UpdateExpanders (false);
        }

        private async Task MirrorReceive () {
            await DialogMirrorReceive ();

            await UpdateMirrorStatus ();
            await UpdateExpanders (false);
        }

        private Task UpdateMirrorStatus () {
            App.Current.Dispatcher.Invoke (() => {
                menuMirrorStatus.Header = (Instance?.Mirror.Status) switch {
                    Mirror.Statuses.HOST => $"{Instance?.Language.Localize ("MIRROR:Status")}: {Instance?.Language.Localize ("MIRROR:Server")}",
                    Mirror.Statuses.CLIENT => $"{Instance?.Language.Localize ("MIRROR:Status")}: {Instance?.Language.Localize ("MIRROR:Client")}",
                    _ => $"{Instance?.Language.Localize ("MIRROR:Status")}: {Instance?.Language.Localize ("MIRROR:Inactive")}",
                };
            });

            return Task.CompletedTask;
        }

        private async Task UpdateExpanders ()
            => await UpdateExpanders (Instance?.Scenario?.IsLoaded ?? false);

        private async Task UpdateExpanders (bool isScene) {
            await App.Current.Dispatcher.InvokeAsync (() => {
                expScenarioPlayer.IsEnabled = isScene;
                expScenarioPlayer.IsExpanded = isScene;
            });
        }

        private async Task SetParameterStatus (bool autoApplyChanges) {
            ParameterStatus = autoApplyChanges
               ? ParameterStatuses.AutoApply
               : ParameterStatuses.ChangesApplied;

            await UpdateParameterIndicators ();
        }

        private async Task AdvanceParameterStatus (ParameterStatuses status) {
            /* Toggles between pending changes or changes applied; bypasses if auto-applying or null */

            if (status == ParameterStatuses.ChangesApplied && ParameterStatus == ParameterStatuses.ChangesPending)
                ParameterStatus = ParameterStatuses.ChangesApplied;
            else if (status == ParameterStatuses.ChangesPending && ParameterStatus == ParameterStatuses.ChangesApplied)
                ParameterStatus = ParameterStatuses.ChangesPending;

            await UpdateParameterIndicators ();
        }

        private async Task UpdateParameterIndicators () {
            await App.Current.Dispatcher.InvokeAsync (() => {
                brdPendingChangesIndicator.BorderBrush = ParameterStatus switch {
                    ParameterStatuses.ChangesPending => Brushes.Red,
                    ParameterStatuses.ChangesApplied => Brushes.Green,
                    ParameterStatuses.AutoApply => Brushes.Orange,
                    _ => Brushes.Transparent,
                };
            });
        }

        private async Task LoadFile () {
            Stream s;
            Microsoft.Win32.OpenFileDialog dlgLoad = new Microsoft.Win32.OpenFileDialog ();

            dlgLoad.Filter = "Infirmary Integrated simulation files (*.ii)|*.ii|All files (*.*)|*.*";
            dlgLoad.FilterIndex = 1;
            dlgLoad.RestoreDirectory = true;

            if (dlgLoad.ShowDialog (this) == true) {
                if ((s = dlgLoad.OpenFile ()) != null) {
                    await LoadInit (s);
                    s.Close ();
                }
            }
        }

        private async Task LoadOpen (string fileName) {
            if (System.IO.File.Exists (fileName)) {
                await LoadInit (fileName);
            } else {
                await LoadFail ();
            }

            OnPhysiologyEvent (this, new Physiology.PhysiologyEventArgs (
                Instance?.Physiology, 
                Physiology.PhysiologyEventTypes.Vitals_Change,
                Instance?.Settings.Time ?? 0));
        }

        private async Task LoadInit (Stream incFile) {
            using StreamReader sr = new (incFile);
            string? metadata = await sr.ReadLineAsync ();
            string? data = await sr.ReadToEndAsync ();
            sr.Close ();

            /* Read savefile metadata indicating data formatting
                * Multiple data formats for forward compatibility
                */

            if (!String.IsNullOrEmpty (metadata) && metadata.StartsWith (".ii:t1"))
                await LoadValidateT1 (data);
            else
                await LoadFail ();
        }

        private async Task LoadInit (string incFile) {
            using StreamReader sr = new (incFile);
            string? metadata = await sr.ReadLineAsync ();
            string? data = await sr.ReadToEndAsync ();
            sr.Close ();

            /* Read savefile metadata indicating data formatting
                * Multiple data formats for forward compatibility
                */

            if (!String.IsNullOrEmpty (metadata) && metadata.StartsWith (".ii:t1"))
                await LoadValidateT1 (data);
            else
                await LoadFail ();
        }

        private async Task LoadValidateT1 (string data) {
            using StringReader sr = new (data);

            try {
                /* Savefile type 1: validated and encrypted
                    * Line 1 is metadata (.ii:t1)
                    * Line 2 is hash for validation (hash taken of raw string data, unobfuscated)
                    * Line 3 is savefile data encrypted by AES encoding
                    */

                string? hash = (await sr.ReadLineAsync ())?.Trim ();
                string? file = Encryption.DecryptAES ((await sr.ReadToEndAsync ())?.Trim ());
                sr.Close ();

                // Original save files used MD5, later changed to SHA256
                if (hash == Encryption.HashSHA256 (file) || hash == Encryption.HashMD5 (file))
                    await LoadProcess (file);
                else
                    await LoadFail ();
            } catch {
                await LoadFail ();
            } finally {
                sr.Close ();
            }
        }

        private async Task LoadProcess (string incFile) {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (LoadProcess)}");
                return;
            }

            using StringReader sRead = new (incFile);
            string? line, pline;
            StringBuilder pbuffer;

            try {
                while ((line = (await sRead.ReadLineAsync ())?.Trim ()) != null) {
                    if (Instance is null)
                        continue;

                    if (line == "> Begin: Physiology") {           // Load files saved by Infirmary Integrated (base)
                        if (Instance.Scenario?.Physiology is null)
                            Instance.Scenario = new (Instance.Settings, true);

                        pbuffer = new StringBuilder ();
                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null
                                && pline != "> End: Physiology")
                            pbuffer.AppendLine (pline);

                        await RefreshScenario (true);
                        await (Instance?.Physiology?.Load (pbuffer.ToString ()) ?? Task.CompletedTask);
                    } else if (line == "> Begin: Scenario") {   // Load files saved by Infirmary Integrated Scenario Editor
                        Instance.Scenario ??= new (Instance.Settings,false);

                        pbuffer = new StringBuilder ();
                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null
                                && pline != "> End: Scenario")
                            pbuffer.AppendLine (pline);

                        await RefreshScenario (false);
                        await Instance.Scenario.Load (pbuffer.ToString ());
                        InitScenarioStep ();     // Needs to be called manually since InitScenario(false) doesn't init a Patient
                    } else if (line == "> Begin: Editor") {
                        pbuffer = new StringBuilder ();
                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null
                                && pline != "> End: Editor")
                            pbuffer.AppendLine (pline);

                        await this.LoadOptions (pbuffer.ToString ());
                    } else if (line == "> Begin: Cardiac Monitor") {
                        pbuffer = new StringBuilder ();
                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null
                                && pline != "> End: Cardiac Monitor")
                            pbuffer.AppendLine (pline);

                        Instance.Device_Monitor = new DeviceMonitor (Instance);
                        await InitDeviceMonitor ();
                        await Instance.Device_Monitor.Load (pbuffer.ToString ());
                    } else if (line == "> Begin: 12 Lead ECG") {
                        pbuffer = new StringBuilder ();
                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null
                                && pline != "> End: 12 Lead ECG")
                            pbuffer.AppendLine (pline);

                        Instance.Device_ECG = new DeviceECG (Instance);
                        await InitDeviceECG ();
                        await Instance.Device_ECG.Load (pbuffer.ToString ());
                    } else if (line == "> Begin: Defibrillator") {
                        pbuffer = new StringBuilder ();
                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null
                                && pline != "> End: Defibrillator")
                            pbuffer.AppendLine (pline);

                        Instance.Device_Defib = new DeviceDefib (Instance);
                        await InitDeviceDefib ();
                        await Instance.Device_Defib.Load (pbuffer.ToString ());
                    } else if (line == "> Begin: Intra-aortic Balloon Pump") {
                        pbuffer = new StringBuilder ();
                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null
                                && pline != "> End: Intra-aortic Balloon Pump")
                            pbuffer.AppendLine (pline);

                        Instance.Device_IABP = new DeviceIABP (Instance);
                        await InitDeviceIABP ();
                        await Instance.Device_IABP.Load (pbuffer.ToString ());
                    }
                }
            } catch {
                await LoadFail ();
            } finally {
                sRead.Close ();
            }

            // On loading a file, ensure Mirroring is not in Client mode! Will conflict...
            if (Instance.Mirror.Status == Mirror.Statuses.CLIENT) {
                Instance.Mirror.Status = Mirror.Statuses.INACTIVE;
                Instance.Mirror.CancelOperation ();      // Attempt to cancel any possible Mirror downloads
            }

            // Initialize the first step of the scenario
            if (Instance?.Scenario?.IsLoaded ?? false) {
                InitStep ();

                if (Instance.Scenario.DeviceMonitor.IsEnabled)
                    await InitDeviceMonitor ();
                if (Instance.Scenario.DeviceDefib.IsEnabled)
                    await InitDeviceDefib ();
                if (Instance.Scenario.DeviceECG.IsEnabled)
                    await InitDeviceECG ();
                if (Instance.Scenario.DeviceIABP.IsEnabled)
                    await InitDeviceIABP ();
            }

            // Set UI Expanders IsExpanded and IsEnabled on whether is a Scenario
            await UpdateExpanders ();

            /* Load completed but possibly in any order (e.g. physiology before devices)
             * Fire events to begin synchronizing devices with physiology
             */
            Instance?.Physiology?.OnPhysiologyEvent (Physiology.PhysiologyEventTypes.Vitals_Change);
        }

        private async Task LoadOptions (string inc) {
            using StringReader sRead = new (inc);

            try {
                string? line;
                while ((line = await sRead.ReadLineAsync ()) != null) {
                    if (line.Contains (':')) {
                        string pName = line.Substring (0, line.IndexOf (':')),
                                pValue = line.Substring (line.IndexOf (':') + 1);
                        switch (pName) {
                            default: break;
                            case "checkDefaultVitals": checkDefaultVitals.IsChecked = bool.Parse (pValue); break;
                        }
                    }
                }
            } catch {
                sRead.Close ();
                return;
            }

            sRead.Close ();
        }

        private Task LoadFail () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (LoadFail)}");
                return Task.CompletedTask;
            }

            System.Windows.MessageBox.Show (
                    Instance.Language.Localize ("PE:LoadFailMessage"),
                    Instance.Language.Localize ("PE:LoadFailTitle"),
                    MessageBoxButton.OK, MessageBoxImage.Error);

            return Task.CompletedTask;
        }

        private async Task SaveFile () {
            // Only save single Patient files in base Infirmary Integrated!
            // Scenario files should be created/edited/saved via II Scenario Editor!

            if (Instance?.Scenario?.IsLoaded ?? false) {
                System.Windows.MessageBox.Show (
                    Instance.Language.Localize ("PE:SaveFailScenarioMessage"),
                    Instance.Language.Localize ("PE:SaveFailScenarioTitle"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Stream s;
            Microsoft.Win32.SaveFileDialog dlgSave = new Microsoft.Win32.SaveFileDialog ();

            dlgSave.Filter = "Infirmary Integrated simulation files (*.ii)|*.ii|All files (*.*)|*.*";
            dlgSave.FilterIndex = 1;
            dlgSave.RestoreDirectory = true;

            if (dlgSave.ShowDialog () == true) {
                if ((s = dlgSave.OpenFile ()) != null) {
                    await SaveT1 (s);
                }
            }
        }

        private async Task SaveT1 (string filename) {
            if (System.IO.File.Exists (filename))
                System.IO.File.Delete (filename);

            using FileStream s = new (filename, FileMode.OpenOrCreate, FileAccess.Write);
            await SaveT1 (s);
        }

        private async Task SaveT1 (Stream stream) {
            // Ensure only saving Patient file, not Scenario file; is screened in SaveFile()
            if (Instance?.Scenario != null && Instance.Scenario.IsLoaded) {
                stream.Close ();
                return;
            }

            StringBuilder sb = new ();

            sb.AppendLine ("> Begin: Physiology");
            sb.Append (Instance?.Physiology?.Save ());
            sb.AppendLine ("> End: Physiology");

            sb.AppendLine ("> Begin: Editor");
            sb.Append (this.SaveOptions ());
            sb.AppendLine ("> End: Editor");

            if (Instance?.Device_Monitor is not null) {
                sb.AppendLine ("> Begin: Cardiac Monitor");
                sb.Append (Instance.Device_Monitor.Save ());
                sb.AppendLine ("> End: Cardiac Monitor");
            }
            if (Instance?.Device_ECG is not null) {
                sb.AppendLine ("> Begin: 12 Lead ECG");
                sb.Append (Instance.Device_ECG.Save ());
                sb.AppendLine ("> End: 12 Lead ECG");
            }
            if (Instance?.Device_Defib is not null) {
                sb.AppendLine ("> Begin: Defibrillator");
                sb.Append (Instance.Device_Defib.Save ());
                sb.AppendLine ("> End: Defibrillator");
            }
            if (Instance?.Device_IABP is not null) {
                sb.AppendLine ("> Begin: Intra-aortic Balloon Pump");
                sb.Append (Instance.Device_IABP.Save ());
                sb.AppendLine ("> End: Intra-aortic Balloon Pump");
            }

            using StreamWriter sw = new (stream);
            await sw.WriteLineAsync (".ii:t1");                                           // Metadata (type 1 savefile)
            await sw.WriteLineAsync (Encryption.HashSHA256 (sb.ToString ().Trim ()));     // Hash for validation
            await sw.WriteAsync (Encryption.EncryptAES (sb.ToString ().Trim ()));         // Savefile data encrypted with AES
            await sw.FlushAsync ();

            sw.Close ();
            stream.Close ();
        }

        private string SaveOptions () {
            StringBuilder sWrite = new ();

            sWrite.AppendLine (String.Format ("{0}:{1}", "checkDefaultVitals", checkDefaultVitals.IsChecked));

            return sWrite.ToString ();
        }

        public Task Exit () {
            Instance?.Settings.Save ();
            Instance?.Shutdown ();

            return Task.CompletedTask;
        }

        private void OnMirrorTick (object? sender, EventArgs e) {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (OnMirrorTick)}");
                return;
            }

            Instance?.Mirror.TimerTick (
                new Scenario.Step (Instance.Settings) {
                    Physiology = Instance.Physiology
                },
                Instance.Server);

            if (Instance?.Mirror.Status == Mirror.Statuses.CLIENT) {
                UpdateView (Instance.Physiology);
            }
        }

        private void OnStepChangeRequest (object? sender, EventArgs e)
            => Task.WaitAll (UnloadPatientEvents ());

        private void OnStepChanged (object? sender, EventArgs e) {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (OnStepChanged)}");
                return;
            }

            InitPhysiologyEvents ();
            InitStep ();

            UpdateView (Instance?.Physiology);
        }

        private void InitStep () {
            Scenario.Step step = Instance?.Scenario?.Current ?? new Scenario.Step (Instance.Settings);

            // Set Previous, Next, Pause, and Play buttons .IsEnabled based on Step properties
            btnPreviousStep.IsEnabled = (!String.IsNullOrEmpty (step.DefaultSource));
            btnNextStep.IsEnabled = (!String.IsNullOrEmpty (step.DefaultProgression?.UUID) || step.Progressions.Count > 0);
            btnPauseStep.IsEnabled = (step.ProgressTimer > 0);
            btnPlayStep.IsEnabled = false;

            // Display Scenario's Step count
            lblScenarioStep.Content = $"{Instance?.Language.Localize ("PE:ProgressionStep")}: {step.Name}";

            // Display Progress Timer if applicable, otherwise instruct that the Step requires manual progression
            if (step.ProgressTimer == -1)
                lblTimerStep.Content = Instance?.Language.Localize ("PE:ProgressionManual");
            else
                lblTimerStep.Content = String.Format ("{0} {1} {2}",
                    Instance?.Language.Localize ("PE:ProgressionAutomatic"),
                    step.ProgressTimer,
                    Instance?.Language.Localize ("PE:ProgressionSeconds"));

            // Re-populate a StackPanel with RadioButtons for Progression options, including "Default Option"
            stackProgressions.Children.Clear ();

            if (step.Progressions.Count == 0) {
                stackProgressions.Children.Add (new Label () {
                    Content = Instance?.Language.Localize ("PE:ProgressionNoneAvailable"),
                    Margin = new Thickness (10)
                });
            } else {
                for (int i = 0; i < step.Progressions.Count; i++) {
                    Scenario.Step.Progression prog = step.Progressions [i];
                    Scenario.Step? stepTo = Instance?.Scenario?.Steps?.Find (s => s.UUID == prog.DestinationUUID);

                    stackProgressions.Children.Add (new RadioButton () {
                        IsChecked = (step.DefaultProgression?.UUID == prog.UUID),
                        Content = String.Format ("{0}{1}",
                            ((step.DefaultProgression?.UUID == prog.UUID) ? $"{Instance?.Language.Localize ("PE:ProgressionDefault")}: " : ""),
                            !String.IsNullOrEmpty (prog.Description) ? prog.Description : stepTo?.Name),
                        Name = String.Format ("radioProgression_{0}", prog.DestinationUUID),
                        GroupName = "ProgressionOptions",
                        Margin = (i == step.Progressions.Count - 1 ? new Thickness (10, 5, 10, 10) : new Thickness (10, 5, 10, 5))
                    });
                }
            }
        }

        private async Task NextStep () {
            if (Instance?.Scenario?.Current?.Progressions.Count == 0)
                await Instance.Scenario.NextStep ();
            else {
                foreach (RadioButton rb in stackProgressions.Children.Cast<RadioButton> ())
                    if (rb.IsChecked ?? false && rb.Name.Contains ('_')) {
                        string? prog = rb.Name?.Substring (rb.Name.IndexOf ('_') + 1);
                        if (Instance?.Scenario is not null)
                            await Instance.Scenario.NextStep (prog == "Default" ? null : prog);
                        break;
                    }
            }
        }

        private async Task PreviousStep () {
            if (Instance?.Scenario is not null)
                await Instance.Scenario.LastStep ();
        }

        private async Task PauseStep () {
            btnPauseStep.IsEnabled = false;
            btnPlayStep.IsEnabled = true;

            if (Instance?.Scenario is not null)
                await Instance.Scenario.PauseStep ();

            lblTimerStep.Content = Instance?.Language.Localize ("PE:ProgressionPaused");
        }

        private async Task PlayStep () {
            btnPauseStep.IsEnabled = true;
            btnPlayStep.IsEnabled = false;

            if (Instance?.Scenario is not null)
                await Instance.Scenario.PlayStep ();

            if (Instance?.Scenario?.Current?.ProgressTimer == -1)
                lblTimerStep.Content = Instance.Language.Localize ("PE:ProgressionManual");
            else
                lblTimerStep.Content = String.Format ("{0} {1} {2}",
                    Instance?.Language.Localize ("PE:ProgressionAutomatic"),
                    Instance?.Scenario?.Current?.ProgressTimer - (Instance?.Scenario?.ProgressTimer.Elapsed / 1000),
                    Instance?.Language.Localize ("PE:ProgressionSeconds"));
        }

        private Task ResetPhysiologyParameters () {
            UpdateView (Instance?.Physiology);
            return Task.CompletedTask;
        }

        private async Task ApplyPhysiologyParameters () {
            await AdvanceParameterStatus (ParameterStatuses.ChangesApplied);

            ApplyBuffer ??= new (Instance.Settings);

            await ApplyPhysiologyParameters_Buffer (ApplyBuffer);
            ApplyPending_Cardiac = true;
            ApplyPending_Respiratory = true;
            ApplyPending_Obstetric = true;

            await ApplyTimer_Cardiac.ResetStart ();
            await ApplyTimer_Respiratory.ResetStart ();
            await ApplyTimer_Obstetric.ResetStart ();
        }

        private async Task ApplyPhysiologyParameters_Buffer (Physiology? p) {
            if (p is null)
                return;

            await p.UpdateParameters_Cardiac (
                // Basic vital signs
                (int)(numHR?.Value ?? 0),
                (int)(numNSBP?.Value ?? 0),
                (int)(numNDBP?.Value ?? 0),
                Physiology.CalculateMAP ((int)(numNSBP?.Value ?? 0), (int)(numNDBP?.Value ?? 0)),
                (int)(numSPO2?.Value ?? 0),
                (double)(numT?.Value ?? 0),

                (Cardiac_Rhythms.Values)(Enum.GetValues (typeof (Cardiac_Rhythms.Values)).GetValue (
                    comboCardiacRhythm.SelectedIndex < 0 ? 0 : comboCardiacRhythm.SelectedIndex)
                    ?? Cardiac_Rhythms.Values.Sinus_Rhythm),

                // Advanced hemodynamics
                (int)(numCVP?.Value ?? 0),
                (int)(numASBP?.Value ?? 0),
                (int)(numADBP?.Value ?? 0),
                Physiology.CalculateMAP ((int)(numASBP?.Value ?? 0), (int)(numADBP?.Value ?? 0)),

                (float)(numCO?.Value ?? 0),

                (PulmonaryArtery_Rhythms.Values)(Enum.GetValues (typeof (PulmonaryArtery_Rhythms.Values)).GetValue (
                    comboPACatheterPlacement.SelectedIndex < 0 ? 0 : comboPACatheterPlacement.SelectedIndex)
                    ?? PulmonaryArtery_Rhythms.Values.Right_Atrium),

                (int)(numPSP?.Value ?? 0),
                (int)(numPDP?.Value ?? 0),
                Physiology.CalculateMAP ((int)(numPSP?.Value ?? 0),
                (int)(numPDP?.Value ?? 0)),

                (int)(numICP?.Value ?? 0),
                (int)(numIAP?.Value ?? 0),

                // Cardiac Profile
                (int)(numPacemakerCaptureThreshold?.Value ?? 0),
                chkPulsusParadoxus.IsChecked ?? false,
                chkPulsusAlternans.IsChecked ?? false,
                chkElectricalAlternans.IsChecked ?? false,

                (Cardiac_Axes.Values)(Enum.GetValues (typeof (Cardiac_Axes.Values)).GetValue (
                    comboCardiacAxis.SelectedIndex < 0 ? 0 : comboCardiacAxis.SelectedIndex)
                    ?? Cardiac_Axes.Values.Normal),

                (double)(numQRSInterval?.Value ?? 0),
                (double)(numQTcInterval?.Value ?? 0),

                new double [] {
                    (double)(numSTE_I?.Value ?? 0),
                    (double)(numSTE_II?.Value ?? 0),
                    (double)(numSTE_III?.Value ?? 0),
                    (double)(numSTE_aVR?.Value ?? 0),
                    (double)(numSTE_aVL?.Value ?? 0),
                    (double)(numSTE_aVF?.Value ?? 0),
                    (double)(numSTE_V1?.Value ?? 0),
                    (double)(numSTE_V2?.Value ?? 0),
                    (double)(numSTE_V3?.Value ?? 0),
                    (double)(numSTE_V4?.Value ?? 0),
                    (double)(numSTE_V5?.Value ?? 0),
                    (double)(numSTE_V6?.Value?? 0)
                },
                new double [] {
                    (double)(numTWE_I?.Value ?? 0),
                    (double)(numTWE_II?.Value ?? 0),
                    (double)(numTWE_III?.Value ?? 0),
                    (double)(numTWE_aVR?.Value ?? 0),
                    (double)(numTWE_aVL?.Value ?? 0),
                    (double)(numTWE_aVF?.Value ?? 0),
                    (double)(numTWE_V1?.Value ?? 0),
                    (double)(numTWE_V2?.Value ?? 0),
                    (double)(numTWE_V3?.Value ?? 0),
                    (double)(numTWE_V4?.Value ?? 0),
                    (double)(numTWE_V5?.Value ?? 0),
                    (double)(numTWE_V6?.Value ?? 0)
                }
                );

            await p.UpdateParameters_Respiratory (

                (int)(numRR?.Value ?? 0),
                (Respiratory_Rhythms.Values)(Enum.GetValues (typeof (Respiratory_Rhythms.Values)).GetValue (
                    comboRespiratoryRhythm.SelectedIndex < 0 ? 0 : comboRespiratoryRhythm.SelectedIndex)
                    ?? Respiratory_Rhythms.Values.Regular),
                (int)(numETCO2?.Value ?? 0),

                chkMechanicallyVentilated.IsChecked ?? false,
                (float)(numInspiratoryRatio?.Value ?? 0),
                (float)(numExpiratoryRatio?.Value ?? 0));

            await p.UpdateParameters_Obstetric (
                (int)(numFHR?.Value ?? 0),
                (int)(numFHRVariability?.Value ?? 0),

                (FetalHeart_Rhythms.Values)(Enum.GetValues (typeof (FetalHeart_Rhythms.Values)).GetValue (
                    comboFHRRhythm.SelectedIndex < 0 ? 0 : comboFHRRhythm.SelectedIndex)
                    ?? FetalHeart_Rhythms.Values.Baseline),

                (int)(numUCFrequency?.Value ?? 0),
                (int)(numUCDuration?.Value ?? 0),
                (int)(numUCIntensity?.Value ?? 0),
                (int)(numUCResting?.Value ?? 0));
        }

        private void ApplyPhysiologyParameters_Cardiac (object? sender, EventArgs e) {
            if (ApplyPending_Cardiac != true || ApplyBuffer is null)
                return;

            ApplyPending_Cardiac = false;
            _ = ApplyTimer_Cardiac.ResetStop ();

            if (Instance?.Physiology is not null) {
                _ = Instance.Physiology.UpdateParameters_Cardiac (
                    ApplyBuffer.HR,
                    ApplyBuffer.NSBP, ApplyBuffer.NDBP, ApplyBuffer.NMAP,
                    ApplyBuffer.SPO2,
                    ApplyBuffer.T,
                    ApplyBuffer.Cardiac_Rhythm.Value,

                    ApplyBuffer.CVP,
                    ApplyBuffer.ASBP, ApplyBuffer.ADBP, ApplyBuffer.AMAP,

                    ApplyBuffer.CO,
                    ApplyBuffer.PulmonaryArtery_Placement.Value,
                    ApplyBuffer.PSP, ApplyBuffer.PDP, ApplyBuffer.PMP,

                    ApplyBuffer.ICP,
                    ApplyBuffer.IAP,

                    ApplyBuffer.Pacemaker_Threshold,
                    ApplyBuffer.Pulsus_Paradoxus,
                    ApplyBuffer.Pulsus_Alternans,
                    ApplyBuffer.Electrical_Alternans,

                    ApplyBuffer.Cardiac_Axis.Value,
                    ApplyBuffer.QRS_Interval, ApplyBuffer.QTc_Interval,
                    ApplyBuffer.ST_Elevation, ApplyBuffer.T_Elevation);
            }

            if (Instance?.Mirror is not null && Instance?.Server is not null)
                _ = Instance.Mirror.PostStep (
                    new Scenario.Step (Instance.Settings) {
                        Physiology = Instance.Physiology,
                    },
                    Instance.Server);
        }

        private void ApplyPhysiologyParameters_Respiratory (object? sender, EventArgs e) {
            if (ApplyPending_Respiratory != true || ApplyBuffer is null)
                return;

            ApplyPending_Respiratory = false;
            _ = ApplyTimer_Respiratory.ResetStop ();

            if (Instance?.Physiology is not null) {
                _ = Instance.Physiology.UpdateParameters_Respiratory (
                    ApplyBuffer.RR,
                    ApplyBuffer.Respiratory_Rhythm.Value,
                    ApplyBuffer.ETCO2,
                    ApplyBuffer.Mechanically_Ventilated,
                    ApplyBuffer.RR_IE_I, ApplyBuffer.RR_IE_E);
            }

            if (Instance?.Mirror is not null && Instance?.Server is not null)
                _ = Instance.Mirror.PostStep (
                    new Scenario.Step (Instance.Settings) {
                        Physiology = Instance.Physiology,
                    },
                    Instance.Server);
        }

        private void ApplyPhysiologyParameters_Obstetric (object? sender, EventArgs e) {
            if (ApplyPending_Obstetric != true || ApplyBuffer is null)
                return;

            ApplyPending_Obstetric = false;
            _ = ApplyTimer_Obstetric.ResetStop ();

            if (Instance?.Physiology is not null) {
                _ = Instance.Physiology.UpdateParameters_Obstetric (
                    ApplyBuffer.Fetal_HR,
                    ApplyBuffer.ObstetricFetalRateVariability,
                    ApplyBuffer.ObstetricFetalHeartRhythm.Value,
                    ApplyBuffer.ObstetricContractionFrequency,
                    ApplyBuffer.ObstetricContractionDuration,
                    ApplyBuffer.ObstetricContractionIntensity,
                    ApplyBuffer.ObstetricUterineRestingTone);
            }

            if (Instance?.Mirror is not null && Instance?.Server is not null)
                _ = Instance.Mirror.PostStep (
                    new Scenario.Step (Instance.Settings) {
                        Physiology = Instance.Physiology,
                    },
                    Instance.Server);
        }

        private void UpdateView (Physiology? p) {
            App.Current.Dispatcher.InvokeAsync (() => {                     // Updating the UI requires being on the proper thread
                ParameterStatus = ParameterStatuses.Loading;                // To prevent each form update from auto-applying back to Patient

                if (p is null) {
                    Debug.WriteLine ($"Null return at {this.Name}.{nameof (InitDeviceMonitor)}");
                    return;
                }

                if (!ApplyPending_Cardiac) {
                    // Basic vital signs
                    numHR.Value = p.VS_Settings.HR;
                    numNSBP.Value = p.VS_Settings.NSBP;
                    numNDBP.Value = p.VS_Settings.NDBP;
                    numSPO2.Value = p.VS_Settings.SPO2;
                    numT.Value = (decimal)p.VS_Settings.T;
                    comboCardiacRhythm.SelectedIndex = (int)p.Cardiac_Rhythm.Value;

                    // Advanced hemodynamics
                    numCVP.Value = p.VS_Settings.CVP;
                    numASBP.Value = p.VS_Settings.ASBP;
                    numADBP.Value = p.VS_Settings.ADBP;
                    numCO.Value = (decimal)p.VS_Settings.CO;
                    comboPACatheterPlacement.SelectedIndex = (int)p.PulmonaryArtery_Placement.Value;
                    numPSP.Value = p.VS_Settings.PSP;
                    numPDP.Value = p.VS_Settings.PDP;
                    numICP.Value = p.VS_Settings.ICP;
                    numIAP.Value = p.VS_Settings.IAP;

                    // Cardiac profile
                    numPacemakerCaptureThreshold.Value = p.Pacemaker_Threshold;
                    chkPulsusParadoxus.IsChecked = p.Pulsus_Paradoxus;
                    chkPulsusAlternans.IsChecked = p.Pulsus_Alternans;
                    chkElectricalAlternans.IsChecked = p.Electrical_Alternans;
                    comboCardiacAxis.SelectedIndex = (int)p.Cardiac_Axis.Value;

                    numQRSInterval.Value = (decimal)p.QRS_Interval;
                    numQTcInterval.Value = (decimal)p.QTc_Interval;

                    if (p.ST_Elevation is not null) {
                        numSTE_I.Value = (decimal)p.ST_Elevation [(int)Lead.Values.ECG_I];
                        numSTE_II.Value = (decimal)p.ST_Elevation [(int)Lead.Values.ECG_II];
                        numSTE_III.Value = (decimal)p.ST_Elevation [(int)Lead.Values.ECG_III];
                        numSTE_aVR.Value = (decimal)p.ST_Elevation [(int)Lead.Values.ECG_AVR];
                        numSTE_aVL.Value = (decimal)p.ST_Elevation [(int)Lead.Values.ECG_AVL];
                        numSTE_aVF.Value = (decimal)p.ST_Elevation [(int)Lead.Values.ECG_AVF];
                        numSTE_V1.Value = (decimal)p.ST_Elevation [(int)Lead.Values.ECG_V1];
                        numSTE_V2.Value = (decimal)p.ST_Elevation [(int)Lead.Values.ECG_V2];
                        numSTE_V3.Value = (decimal)p.ST_Elevation [(int)Lead.Values.ECG_V3];
                        numSTE_V4.Value = (decimal)p.ST_Elevation [(int)Lead.Values.ECG_V4];
                        numSTE_V5.Value = (decimal)p.ST_Elevation [(int)Lead.Values.ECG_V5];
                        numSTE_V6.Value = (decimal)p.ST_Elevation [(int)Lead.Values.ECG_V6];
                    }

                    if (p.T_Elevation is not null) {
                        numTWE_I.Value = (decimal)p.T_Elevation [(int)Lead.Values.ECG_I];
                        numTWE_II.Value = (decimal)p.T_Elevation [(int)Lead.Values.ECG_II];
                        numTWE_III.Value = (decimal)p.T_Elevation [(int)Lead.Values.ECG_III];
                        numTWE_aVR.Value = (decimal)p.T_Elevation [(int)Lead.Values.ECG_AVR];
                        numTWE_aVL.Value = (decimal)p.T_Elevation [(int)Lead.Values.ECG_AVL];
                        numTWE_aVF.Value = (decimal)p.T_Elevation [(int)Lead.Values.ECG_AVF];
                        numTWE_V1.Value = (decimal)p.T_Elevation [(int)Lead.Values.ECG_V1];
                        numTWE_V2.Value = (decimal)p.T_Elevation [(int)Lead.Values.ECG_V2];
                        numTWE_V3.Value = (decimal)p.T_Elevation [(int)Lead.Values.ECG_V3];
                        numTWE_V4.Value = (decimal)p.T_Elevation [(int)Lead.Values.ECG_V4];
                        numTWE_V5.Value = (decimal)p.T_Elevation [(int)Lead.Values.ECG_V5];
                        numTWE_V6.Value = (decimal)p.T_Elevation [(int)Lead.Values.ECG_V6];
                    }
                }

                if (!ApplyPending_Respiratory) {
                    // Respiratory profile
                    numRR.Value = p.VS_Settings.RR;
                    comboRespiratoryRhythm.SelectedIndex = (int)p.Respiratory_Rhythm.Value;
                    numETCO2.Value = p.VS_Settings.ETCO2;
                    chkMechanicallyVentilated.IsChecked = p.Mechanically_Ventilated;
                    numInspiratoryRatio.Value = (decimal)p.VS_Settings.RR_IE_I;
                    numExpiratoryRatio.Value = (decimal)p.VS_Settings.RR_IE_E;
                }

                if (!ApplyPending_Obstetric) {
                    // Obstetric profile
                    numFHR.Value = p.VS_Settings.FetalHR;
                    numFHRVariability.Value = p.ObstetricFetalRateVariability;
                    numUCFrequency.Value = (decimal)p.ObstetricContractionFrequency;
                    numUCDuration.Value = p.ObstetricContractionDuration;
                    numUCIntensity.Value = p.ObstetricContractionIntensity;
                    numUCResting.Value = p.ObstetricUterineRestingTone;
                    comboFHRRhythm.SelectedIndex = (int)p.ObstetricFetalHeartRhythm.Value;
                }

                _ = SetParameterStatus (Instance?.Settings.AutoApplyChanges ?? false);     // Re-establish parameter status
            });
        }

        public void PauseSimulation () {
            if (Instance?.Physiology == null) {
                return;
            }

            /* Change the Physiology State and link/unlink the main Timer*/
            if (Instance.Settings.State == II.Settings.Simulator.States.Stopped) {
                Instance.Settings.State = II.Settings.Simulator.States.Running;
                Instance.Timer_Main.Start ();
            } else if (Instance.Settings.State == II.Settings.Simulator.States.Running) {
                Instance.Settings.State = II.Settings.Simulator.States.Stopped;
                Instance.Timer_Main.Stop ();
            }

            if (Instance.Device_Monitor is not null)
                Instance.Device_Monitor.PauseDevice (Instance.Settings.State == II.Settings.Simulator.States.Stopped);
            if (Instance.Device_Defib is not null)
                Instance.Device_Defib.PauseDevice (Instance.Settings.State == II.Settings.Simulator.States.Stopped);
            if (Instance.Device_ECG is not null)
                Instance.Device_ECG.PauseDevice (Instance.Settings.State == II.Settings.Simulator.States.Stopped);
            if (Instance.Device_EFM is not null)
                Instance.Device_EFM.PauseDevice (Instance.Settings.State == II.Settings.Simulator.States.Stopped);
            if (Instance.Device_IABP is not null)
                Instance.Device_IABP.PauseDevice (Instance.Settings.State == II.Settings.Simulator.States.Stopped);
        }

        public void ToggleFullscreen () {
            if (wdwControl.WindowState == System.Windows.WindowState.Maximized)
                wdwControl.WindowState = System.Windows.WindowState.Normal;
            else
                wdwControl.WindowState = System.Windows.WindowState.Maximized;
        }

        private void MenuNewSimulation_Click (object sender, RoutedEventArgs e)
            => _ = RefreshScenario (true);

        private void MenuLoadFile_Click (object s, RoutedEventArgs e)
            => _ = LoadFile ();

        private void MenuSaveFile_Click (object s, RoutedEventArgs e)
            => _ = SaveFile ();

        private void MenuPauseSimulation_Click (object s, RoutedEventArgs e)
            => PauseSimulation ();

        private void MenuToggleFullscreen_Click (object s, RoutedEventArgs e)
            => ToggleFullscreen ();

        private void MenuExit_Click (object s, RoutedEventArgs e)
            => _ = Exit ();

        private void MenuToggleAudio_Click (object s, RoutedEventArgs e)
            => _ = ToggleAudio ();

        private void MenuSetLanguage_Click (object s, RoutedEventArgs e)
            => _ = DialogLanguage (true);

        private void MenuMirrorDeactivate_Click (object s, RoutedEventArgs e)
            => _ = MirrorDeactivate ();

        private void MenuMirrorBroadcast_Click (object s, RoutedEventArgs e)
            => _ = MirrorBroadcast ();

        private void MenuMirrorReceive_Click (object s, RoutedEventArgs e)
            => _ = MirrorReceive ();

        private void MenuAbout_Click (object s, RoutedEventArgs e)
            => _ = DialogAbout ();

        private void MenuCheckUpdate_Click (object s, RoutedEventArgs e)
            => _ = CheckUpgrade ();

        private void ButtonDeviceMonitor_Click (object s, RoutedEventArgs e)
            => _ = InitDeviceMonitor ();

        private void ButtonDeviceDefib_Click (object s, RoutedEventArgs e)
            => _ = InitDeviceDefib ();

        private void ButtonDeviceECG_Click (object s, RoutedEventArgs e)
            => _ = InitDeviceECG ();

        private void ButtonDeviceIABP_Click (object s, RoutedEventArgs e)
            => _ = InitDeviceIABP ();

        private void ButtonDeviceEFM_Click (object s, RoutedEventArgs e)
            => _ = InitDeviceEFM ();

        private void ButtonOptionsHide_Click (object s, RoutedEventArgs e)
            => _ = ToggleHideDevices ();

        private void ButtonPreviousStep_Click (object s, RoutedEventArgs e)
            => _ = PreviousStep ();

        private void ButtonNextStep_Click (object s, RoutedEventArgs e)
            => _ = NextStep ();

        private void ButtonPauseStep_Click (object s, RoutedEventArgs e)
            => _ = PauseStep ();

        private void ButtonPlayStep_Click (object s, RoutedEventArgs e)
            => _ = PlayStep ();

        private void ButtonResetParameters_Click (object s, RoutedEventArgs e)
            => _ = ResetPhysiologyParameters ();

        private void ButtonApplyParameters_Click (object sender, RoutedEventArgs e)
            => _ = ApplyPhysiologyParameters ();

        private void OnClosed (object? sender, EventArgs e)
            => _ = Exit ();

        private void OnActivated (object? sender, EventArgs e) {
            if (!IsUILoadCompleted) {
                /* Re-apply window settings from prior run */
                wdwControl.Left = Instance?.Settings.WindowPosition.X ?? 0;
                wdwControl.Top = Instance?.Settings.WindowPosition.Y ?? 0;
                IsUILoadCompleted = true;
            }
        }

        private void OnLayoutUpdated (object sender, EventArgs e) {
            if (!IsUILoadCompleted) {
                wdwControl.Width = Instance?.Settings.WindowSize.X ?? 0;
                wdwControl.Height = Instance?.Settings.WindowSize.Y ?? 0;
            } else if (Instance is not null) {
                Instance.Settings.WindowSize.X = (int)wdwControl.Width;
                Instance.Settings.WindowSize.Y = (int)wdwControl.Height;
            }
        }

        private void OnAutoApplyChanges_Changed (object sender, RoutedEventArgs e) {
            if (ParameterStatus == ParameterStatuses.Loading || Instance is null)
                return;

            Instance.Settings.AutoApplyChanges = !Instance.Settings.AutoApplyChanges;
            chkAutoApplyChanges.IsChecked = Instance.Settings.AutoApplyChanges;
            btnParametersReset.IsEnabled = !Instance.Settings.AutoApplyChanges;
            Instance.Settings.Save ();

            _ = SetParameterStatus (Instance.Settings.AutoApplyChanges);
        }

        private void OnUIPhysiologyParameter_KeyDown (object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter)
                OnUIPhysiologyParameter_Process (sender, e);
        }

        private void OnUIPhysiologyParameter_Changed (object sender, RoutedPropertyChangedEventArgs<object> e)
            => OnUIPhysiologyParameter_Process (sender, e);

        private void OnUIPhysiologyParameter_Changed (object sender, SelectionChangedEventArgs e)
            => OnUIPhysiologyParameter_Process (sender, e);

        private void OnUIPhysiologyParameter_Changed (object sender, RoutedEventArgs e)
            => OnUIPhysiologyParameter_Process (sender, e);

        private void OnUIPhysiologyParameter_LostFocus (object sender, RoutedEventArgs e)
            => OnUIPhysiologyParameter_Process (sender, e);

        private void OnUIPhysiologyParameter_Process (object sender, RoutedEventArgs e) {
            switch (ParameterStatus) {
                default:
                case ParameterStatuses.Loading:            // For loading state
                    break;

                case ParameterStatuses.ChangesApplied:
                case ParameterStatuses.ChangesPending:
                    _ = AdvanceParameterStatus (ParameterStatuses.ChangesPending);
                    break;

                case ParameterStatuses.AutoApply:
                    _ = ApplyPhysiologyParameters ();
                    _ = UpdateParameterIndicators ();
                    break;
            }
        }

        private void OnCardiacRhythm_Selected (object sender, SelectionChangedEventArgs e) {
            if (!checkDefaultVitals.IsChecked ?? false || Instance?.Physiology == null)
                return;

            int si = comboCardiacRhythm.SelectedIndex;
            Array ev = Enum.GetValues (typeof (Cardiac_Rhythms.Values));
            if (si < 0 || si > ev.Length - 1)
                return;

            Cardiac_Rhythms.Default_Vitals v = Cardiac_Rhythms.DefaultVitals (
                (Cardiac_Rhythms.Values)(ev.GetValue (si) ?? Cardiac_Rhythms.Values.Sinus_Rhythm));

            numHR.Value = II.Math.Clamp (numHR?.Value ?? 0, v.HRMin, v.HRMax);
            numNSBP.Value = II.Math.Clamp (numNSBP?.Value ?? 0, v.SBPMin, v.SBPMax);
            numNDBP.Value = II.Math.Clamp (numNDBP?.Value ?? 0, v.DBPMin, v.DBPMax);
            numRR.Value = II.Math.Clamp (numRR?.Value ?? 0, v.RRMin, v.RRMax);
            numSPO2.Value = II.Math.Clamp (numSPO2?.Value ?? 0, v.SPO2Min, v.SPO2Max);
            numETCO2.Value = II.Math.Clamp (numETCO2?.Value ?? 0, v.ETCO2Min, v.ETCO2Max);
            numASBP.Value = II.Math.Clamp (numASBP?.Value ?? 0, v.SBPMin, v.SBPMax);
            numADBP.Value = II.Math.Clamp (numADBP?.Value ?? 0, v.DBPMin, v.DBPMax);
            numPSP.Value = II.Math.Clamp (numPSP?.Value ?? 0, v.PSPMin, v.PSPMax);
            numPDP.Value = II.Math.Clamp (numPDP?.Value ?? 0, v.PDPMin, v.PDPMax);
            numQRSInterval.Value = (decimal)II.Math.Clamp ((double)(numQRSInterval?.Value ?? 0), v.QRSIntervalMin, v.QRSIntervalMax);
            numQTcInterval.Value = (decimal)II.Math.Clamp ((double)(numQTcInterval?.Value ?? 0), v.QTCIntervalMin, v.QTCIntervalMax);

            OnUIPhysiologyParameter_Process (sender, e);
        }

        private void OnRespiratoryRhythm_Selected (object sender, SelectionChangedEventArgs e) {
            OnUIPhysiologyParameter_Process (sender, e);
        }

        private void OnCentralVenousPressure_Changed (object sender, RoutedPropertyChangedEventArgs<object> e) {
            if (Instance?.Physiology is not null
                && checkDefaultVitals.IsChecked == true
                && Instance.Physiology.PulmonaryArtery_Placement.Value == PulmonaryArtery_Rhythms.Values.Right_Atrium) {
                /* This handles logic if the PA catheter is in the right atrium, because the PA pressures and the CVP
                 * are reading the same physiologic pressure, but the monitor filters the CVP into one average pressure,
                 * whereas the PA pressure will display as a systolic / diastolic (mean).
                 */

                int nv = e?.NewValue is null || e.NewValue is not int
                    ? 1 : (int)e.NewValue;

                numPSP.Value = (int)System.Math.Ceiling (nv + System.Math.Max (1, nv * 0.25m));
                numPDP.Value = (int)System.Math.Floor (nv - System.Math.Max (1, nv * 0.25m));

                if (e is not null)
                    OnUIPhysiologyParameter_Process (sender, e);
            }
        }

        private void OnPulmonaryArteryRhythm_Selected (object sender, SelectionChangedEventArgs e) {
            if (Instance?.Physiology is null)
                return;

            int si = comboPACatheterPlacement.SelectedIndex;
            Array ev = Enum.GetValues (typeof (PulmonaryArtery_Rhythms.Values));
            if (si < 0 || si > ev.Length - 1)
                return;

            PulmonaryArtery_Rhythms.Values sel = (PulmonaryArtery_Rhythms.Values)(ev.GetValue (si) ?? PulmonaryArtery_Rhythms.Values.Pulmonary_Artery);

            // If the PA placement is RV, PA, or PAWP, utilize default vital sign ranges
            // But if it is in the RA... utilize the current CVP reading
            if (checkDefaultVitals.IsChecked == true) {
                if (sel != PulmonaryArtery_Rhythms.Values.Right_Atrium) {
                    PulmonaryArtery_Rhythms.Default_Vitals v = PulmonaryArtery_Rhythms.DefaultVitals (sel);
                    numPSP.Value = (int)II.Math.Clamp ((double)(numPSP?.Value ?? 0), v.PSPMin, v.PSPMax);
                    numPDP.Value = (int)II.Math.Clamp ((double)(numPDP?.Value ?? 0), v.PDPMin, v.PDPMax);
                } else if (sel == PulmonaryArtery_Rhythms.Values.Right_Atrium) {
                    numPSP.Value = (int)System.Math.Ceiling (Instance.Physiology.CVP + System.Math.Max (1, Instance.Physiology.CVP * 0.25));
                    numPDP.Value = (int)System.Math.Floor (Instance.Physiology.CVP - System.Math.Max (1, Instance.Physiology.CVP * 0.25));
                }
            }

            OnUIPhysiologyParameter_Process (sender, e);
        }

        private void OnPhysiologyEvent (object? sender, Physiology.PhysiologyEventArgs e) {
            if (e.Physiology is null)
                return;

            switch (e.EventType) {
                default:
                    break;

                case Physiology.PhysiologyEventTypes.Cardiac_Baseline:
                    ApplyPhysiologyParameters_Cardiac (sender, new EventArgs ());
                    break;

                case Physiology.PhysiologyEventTypes.Respiratory_Baseline:
                    ApplyPhysiologyParameters_Respiratory (sender, new EventArgs ());
                    break;

                case Physiology.PhysiologyEventTypes.Obstetric_Baseline:
                    ApplyPhysiologyParameters_Obstetric (sender, new EventArgs ());
                    break;

                case Physiology.PhysiologyEventTypes.Vitals_Change:
                    UpdateView (e.Physiology);
                    break;
            }
        }
    }
}