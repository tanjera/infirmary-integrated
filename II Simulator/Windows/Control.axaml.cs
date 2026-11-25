/* Infirmary Integrated Simulator
 * By Ibi Keller (Tanjera), (c) 2023
 */

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
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

using II;
using II.Localization;
using II.Server;

namespace IISIM {

    public partial class Control : Window {
        public App? Instance;

        public WindowStates WindowStatus = WindowStates.Null;   // Monitors status of the Window; Active? Closed?
        
        private bool _hideDeviceLabels = false;

        /* Buffers for ViewModel handling and temporal smoothing of upstream Model data changes */
        private Physiology? _applyBuffer;

        private bool _applyPendingCardiac = false,
                     _applyPendingRespiratory = false,
                     _applyPendingObstetric = false;

        private Timer _applyTimerCardiac = new (),
                      _applyTimerRespiratory = new (),
                      _applyTimerObstetric = new ();

        
        /* Variables for Auto-Apply functionality */
        private ParameterStatuses _parameterStatus = ParameterStatuses.Loading;

        public enum WindowStates {
            Null,
            Active,
            Closed
        }
        
        private enum ParameterStatuses {
            Loading,
            AutoApply,
            ChangesPending,
            ChangesApplied
        }

        public Control (App? app) {
            Instance = app;

            InitializeComponent ();

            Init ();
        }

        public Control () {
            InitializeComponent ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        private void Init () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (Init)}");
                return;
            }

            DataContext = this;

            Instance.Window_Main = this;

            InitInitialRun ();

            /* Init essential functions first */
            InitInterface ();
            InitMirroring ();
            InitScenario (true);
            InitTimers ();

            Dispatcher.UIThread.InvokeAsync (async () => {
                /* Init important but non-essential functions */
                if (Instance.Start_Args?.Length > 0) {
                    string loadfile = Instance.Start_Args [0].Trim (' ', '\n', '\r');
                    if (!String.IsNullOrEmpty (loadfile))
                        await LoadOpen (loadfile);
                }

                /* Update UI from loading functionality */
                await SetParameterStatus (Instance?.Settings.AutoApplyChanges ?? false);

                /* Run useful but otherwise vanity functions last */

                if (Instance?.AudioLib is null)
                    await MessageAudioUnavailable ();

                await InitUpgrade ();
            });

        }

        private void InitInitialRun () {
            if (!II.Settings.Instance.Exists () || !(Instance?.Settings.AcceptedEULA ?? false)) {
                DialogEULA ();
            }
        }

        private void InitInterface () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (InitInterface)}");
                return;
            }

            /* Populate UI strings per language selection */

            this.GetControl<Window> ("wdwControl").Title = Instance.Language.Localize ("PE:WindowTitle");
            this.GetControl<MenuItem> ("menuNew").Header = Instance.Language.Localize ("PE:MenuNewFile");
            this.GetControl<MenuItem> ("menuFile").Header = Instance.Language.Localize ("PE:MenuFile");
            this.GetControl<MenuItem> ("menuLoad").Header = Instance.Language.Localize ("PE:MenuLoadSimulation");
            this.GetControl<MenuItem> ("menuSave").Header = Instance.Language.Localize ("PE:MenuSaveSimulation");
            this.GetControl<MenuItem> ("menuExit").Header = Instance.Language.Localize ("PE:MenuExitProgram");

            this.GetControl<MenuItem> ("menuOptions").Header = Instance.Language.Localize ("MENU:MenuDeviceOptions");
            this.GetControl<MenuItem> ("menuToggleFullscreen").Header = Instance.Language.Localize ("MENU:MenuToggleFullscreen");
            this.GetControl<MenuItem> ("menuPauseDevices").Header = Instance.Language.Localize ("MENU:PauseSimulation");
            
            this.GetControl<MenuItem> ("menuMirror").Header = Instance.Language.Localize ("PE:MenuMirror");
            this.GetControl<MenuItem> ("menuMirrorDeactivate").Header = Instance.Language.Localize ("PE:MenuMirrorDeactivate");
            this.GetControl<MenuItem> ("menuMirrorReceive").Header = Instance.Language.Localize ("PE:MenuMirrorReceive");
            this.GetControl<MenuItem> ("menuMirrorBroadcast").Header = Instance.Language.Localize ("PE:MenuMirrorBroadcast");

            this.GetControl<MenuItem> ("menuSettings").Header = Instance.Language.Localize ("PE:MenuSettings");
            this.GetControl<MenuItem> ("menuToggleAudio").Header = String.Format ("{0}: {1}",
                Instance.Language.Localize ("PE:MenuToggleAudio"),
                Instance.Settings.AudioEnabled ? Instance.Language.Localize ("BOOLEAN:On") : Instance.Language.Localize ("BOOLEAN:Off"));
            this.GetControl<MenuItem> ("menuSetLanguage").Header = Instance.Language.Localize ("PE:MenuSetLanguage");

            this.GetControl<MenuItem> ("menuHelp").Header = Instance.Language.Localize ("PE:MenuHelp");
            this.GetControl<MenuItem> ("menuCheckUpdate").Header = Instance.Language.Localize ("PE:MenuCheckUpdates");
            this.GetControl<MenuItem> ("menuAbout").Header = Instance.Language.Localize ("PE:MenuAboutProgram");

            this.GetControl<HeaderedContentControl> ("lblGroupDevices").Header = Instance.Language.Localize ("PE:Devices");
            this.GetControl<Label> ("lblDeviceMonitor").Content = Instance.Language.Localize ("PE:CardiacMonitor");
            this.GetControl<Label> ("lblDevice12LeadECG").Content = Instance.Language.Localize ("PE:12LeadECG");
            this.GetControl<Label> ("lblDeviceDefibrillator").Content = Instance.Language.Localize ("PE:Defibrillator");
            this.GetControl<Label> ("lblDeviceIABP").Content = Instance.Language.Localize ("PE:IABP");
            this.GetControl<Label> ("lblDeviceEFM").Content = Instance.Language.Localize ("PE:EFM");

            this.GetControl<HeaderedContentControl> ("lblGroupOptions").Header = Instance.Language.Localize ("PE:Options");
            this.GetControl<Label> ("lblOptionsHide").Content = Instance.Language.Localize ("PE:HideDevices");

            this.GetControl<Label> ("lblGroupScenarioPlayer").Content = Instance.Language.Localize ("PE:ScenarioPlayer");
            this.GetControl<HeaderedContentControl> ("lblProgressionOptions").Header = Instance.Language.Localize ("PE:ProgressionOptions");

            this.GetControl<Label> ("lblGroupVitalSigns").Content = Instance.Language.Localize ("PE:VitalSigns");
            this.GetControl<Label> ("lblHR").Content = $"{Instance.Language.Localize ("PE:HeartRate")}:";
            this.GetControl<Label> ("lblNIBP").Content = $"{Instance.Language.Localize ("PE:BloodPressure")}:";
            this.GetControl<Label> ("lblRR").Content = $"{Instance.Language.Localize ("PE:RespiratoryRate")}:";
            this.GetControl<Label> ("lblSPO2").Content = $"{Instance.Language.Localize ("PE:PulseOximetry")}:";
            this.GetControl<Label> ("lblT").Content = $"{Instance.Language.Localize ("PE:Temperature")}:";
            this.GetControl<Label> ("lblCardiacRhythm").Content = $"{Instance.Language.Localize ("PE:CardiacRhythm")}:";
            this.GetControl<CheckBox> ("checkDefaultVitals").Content = Instance.Language.Localize ("PE:UseDefaultVitalSignRanges");

            this.GetControl<Label> ("lblGroupHemodynamics").Content = Instance.Language.Localize ("PE:AdvancedHemodynamics");
            this.GetControl<Label> ("lblETCO2").Content = $"{Instance.Language.Localize ("PE:EndTidalCO2")}:";
            this.GetControl<Label> ("lblCVP").Content = $"{Instance.Language.Localize ("PE:CentralVenousPressure")}:";
            this.GetControl<Label> ("lblASBP").Content = $"{Instance.Language.Localize ("PE:ArterialBloodPressure")}:";
            this.GetControl<Label> ("lblPACatheterPlacement").Content = $"{Instance.Language.Localize ("PE:PulmonaryArteryCatheterPlacement")}:";
            this.GetControl<Label> ("lblCO").Content = $"{Instance.Language.Localize ("PE:CardiacOutput")}:";
            this.GetControl<Label> ("lblPSP").Content = $"{Instance.Language.Localize ("PE:PulmonaryArteryPressure")}:";
            this.GetControl<Label> ("lblICP").Content = $"{Instance.Language.Localize ("PE:IntracranialPressure")}:";
            this.GetControl<Label> ("lblIAP").Content = $"{Instance.Language.Localize ("PE:IntraabdominalPressure")}:";

            this.GetControl<Label> ("lblGroupRespiratoryProfile").Content = Instance.Language.Localize ("PE:RespiratoryProfile");
            this.GetControl<Label> ("lblRespiratoryRhythm").Content = $"{Instance.Language.Localize ("PE:RespiratoryRhythm")}:";
            this.GetControl<Label> ("lblMechanicallyVentilated").Content = $"{Instance.Language.Localize ("PE:MechanicallyVentilated")}:";
            this.GetControl<Label> ("lblInspiratoryRatio").Content = $"{Instance.Language.Localize ("PE:InspiratoryExpiratoryRatio")}:";

            this.GetControl<Label> ("lblGroupCardiacProfile").Content = Instance.Language.Localize ("PE:CardiacProfile");
            this.GetControl<Label> ("lblPacemakerCaptureThreshold").Content = $"{Instance.Language.Localize ("PE:PacemakerCaptureThreshold")}:";
            this.GetControl<Label> ("lblPulsusParadoxus").Content = $"{Instance.Language.Localize ("PE:PulsusParadoxus")}:";
            this.GetControl<Label> ("lblPulsusAlternans").Content = $"{Instance.Language.Localize ("PE:PulsusAlternans")}:";
            this.GetControl<Label> ("lblElectricalAlternans").Content = $"{Instance.Language.Localize ("PE:ElectricalAlternans")}:";
            this.GetControl<Label> ("lblQRSInterval").Content = $"{Instance.Language.Localize ("PE:QRSInterval")}:";
            this.GetControl<Label> ("lblQTcInterval").Content = $"{Instance.Language.Localize ("PE:QTcInterval")}:";
            this.GetControl<Label> ("lblCardiacAxis").Content = $"{Instance.Language.Localize ("PE:CardiacAxis")}:";
            
            this.GetControl<HeaderedContentControl> ("grpPWaveAmplitude").Header
                = $"{Instance.Language.Localize ("PE:PWaveAmplitude")} ({Instance.Language.Localize ("PE:Multiplier")})";
            this.GetControl<HeaderedContentControl> ("grpQWaveAmplitude").Header 
                = $"{Instance.Language.Localize ("PE:QWaveAmplitude")} ({Instance.Language.Localize ("PE:Multiplier")})";
            this.GetControl<HeaderedContentControl> ("grpRWaveAmplitude").Header 
                = $"{Instance.Language.Localize ("PE:RWaveAmplitude")} ({Instance.Language.Localize ("PE:Multiplier")})";
            this.GetControl<HeaderedContentControl> ("grpSWaveAmplitude").Header 
                = $"{Instance.Language.Localize ("PE:SWaveAmplitude")} ({Instance.Language.Localize ("PE:Multiplier")})";
            this.GetControl<HeaderedContentControl> ("grpTWaveAmplitude").Header 
                = $"{Instance.Language.Localize ("PE:TWaveAmplitude")} ({Instance.Language.Localize ("PE:Multiplier")})";
            
            this.GetControl<HeaderedContentControl> ("grpSTSegmentElevation").Header 
                = $"{Instance.Language.Localize ("PE:STSegmentElevation")} ({Instance.Language.Localize ("PE:Offset")})";
            
            this.GetControl<Label> ("lblGroupObstetricProfile").Content = Instance.Language.Localize ("PE:ObstetricProfile");
            this.GetControl<Label> ("lblFHR").Content = $"{Instance.Language.Localize ("PE:FetalHeartRate")}:";
            this.GetControl<Label> ("lblFHRRhythms").Content = $"{Instance.Language.Localize ("PE:FetalHeartRhythms")}:";
            this.GetControl<Label> ("lblFHRVariability").Content = $"{Instance.Language.Localize ("PE:FetalHeartRateVariability")}:";
            this.GetControl<Label> ("lblUCFrequency").Content = $"{Instance.Language.Localize ("PE:UterineContractionFrequency")}:";
            this.GetControl<Label> ("lblUCDuration").Content = $"{Instance.Language.Localize ("PE:UterineContractionDuration")}:";
            this.GetControl<Label> ("lblUCIntensity").Content = $"{Instance.Language.Localize ("PE:UterineContractionIntensity")}:";
            this.GetControl<Label> ("lblUCResting").Content = $"{Instance.Language.Localize ("PE:UterineRestingTone")}:";

            this.GetControl<CheckBox> ("chkAutoApplyChanges").Content = Instance.Language.Localize ("BUTTON:AutoApplyChanges");
            this.GetControl<Label> ("lblParametersApply").Content = Instance.Language.Localize ("BUTTON:ApplyChanges");
            this.GetControl<Label> ("lblParametersReset").Content = Instance.Language.Localize ("BUTTON:ResetParameters");

            this.GetControl<CheckBox> ("chkAutoApplyChanges").IsChecked = Instance.Settings.AutoApplyChanges;
            this.GetControl<Button> ("btnParametersReset").IsEnabled = !Instance.Settings.AutoApplyChanges;


            ItemCollection icCardiacRhythms = this.GetControl<ComboBox> ("comboCardiacRhythm").Items;
            foreach (CardiacRhythms.Values v in Enum.GetValues (typeof (CardiacRhythms.Values)))
                icCardiacRhythms.Add (new ComboBoxItem () {
                    Content = Instance.Language.Localize (CardiacRhythms.LookupString (v))
                });

            ItemCollection icRespiratoryRhythms = this.GetControl<ComboBox> ("comboRespiratoryRhythm").Items;
            foreach (Respiratory_Rhythms.Values v in Enum.GetValues (typeof (Respiratory_Rhythms.Values)))
                icRespiratoryRhythms.Add (new ComboBoxItem () {
                    Tag = v.ToString (),
                    Content = Instance.Language.Localize (Respiratory_Rhythms.LookupString (v))
                });

            ItemCollection icPACatheterPlacement = this.GetControl<ComboBox> ("comboPACatheterPlacement").Items;
            foreach (PulmonaryArteryRhythms.Values v in Enum.GetValues (typeof (PulmonaryArteryRhythms.Values)))
                icPACatheterPlacement.Add (new ComboBoxItem () {
                    Tag = v.ToString (),
                    Content = Instance.Language.Localize (PulmonaryArteryRhythms.LookupString (v))
                });

            
            ItemCollection icCardiacAxes = this.GetControl<ComboBox> ("comboCardiacAxis").Items;
            foreach (Cardiac_Axes.Values v in Enum.GetValues (typeof (Cardiac_Axes.Values)))
                icCardiacAxes.Add (new ComboBoxItem () {
                    Tag = v.ToString (),
                    Content = Instance.Language.Localize (Cardiac_Axes.LookupString (v))
                });

            ItemCollection icFetalHeartRhythms = this.GetControl<ComboBox> ("comboFHRRhythm").Items;
            foreach (FetalHeart_Rhythms.Values v in Enum.GetValues (typeof (FetalHeart_Rhythms.Values)))
                icFetalHeartRhythms.Add (new ComboBoxItem () {
                    Tag = v.ToString (),
                    Content = Instance.Language.Localize (FetalHeart_Rhythms.LookupString (v))
                });
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

            if (!upgradeAvailable) {            // If no update available, no status update; remove any notification muting
                Instance.Settings.MuteUpgrade = false;
                Instance.Settings.Save ();
                return;
            } 

            if (Instance.Settings.MuteUpgrade) {
                if (DateTime.Compare (Instance.Settings.MuteUpgradeDate,
                        DateTime.Now - new TimeSpan (30, 0, 0, 0)) < 0) {
                    Instance.Settings.MuteUpgrade = false; // Reset the notification mute every 30 days
                    Instance.Settings.Save ();
                } else {
                    // Mutes update popup notification
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
            Instance.Mirror.TimerUpdate.Tick += OnMirrorTick;

            Task.WhenAll (
                Instance.Mirror.TimerUpdate.ResetStart (5000),
                UpdateMirrorStatus ()
            );
        }

        private void InitScenario (bool toInit) {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (InitScenario)}");
                return;
            }

            Instance.Scenario = new Scenario (Instance.Timer_Simulation, toInit);
            Instance.Scenario.StepChangeRequest += OnStepChangeRequest;    // Allows unlinking of Timers immediately prior to Step change
            Instance.Scenario.StepChanged += OnStepChanged;                  // Updates IIApp.Patient, allows PatientEditor UI to update
            Instance.Timer_Simulation.Tick += Instance.Scenario.ProcessTimer;

            if (toInit)         // If toInit is false, Patient is null- InitPatient() will need to be called manually
                InitScenarioStep ();
        }

        private async Task UnloadScenario () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (UnloadScenario)}");
                return;
            }

            if (Instance.Scenario != null) {
                Instance.Timer_Simulation.Tick -= Instance.Scenario.ProcessTimer;   // Unlink Scenario from App/Main Timer
                Instance.Scenario.Dispose ();        // Disposes Scenario's events and timer, and all Patients' events and timers
            }
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

            Instance.Timer_Simulation.Tick += _applyTimerCardiac.Process;
            Instance.Timer_Simulation.Tick += _applyTimerRespiratory.Process;
            Instance.Timer_Simulation.Tick += _applyTimerObstetric.Process;

            _applyTimerCardiac.Tick += ApplyPhysiologyParameters_Cardiac;
            _applyTimerRespiratory.Tick += ApplyPhysiologyParameters_Respiratory;
            _applyTimerObstetric.Tick += ApplyPhysiologyParameters_Obstetric;

            Task.WhenAll(
                _applyTimerCardiac.Set (5000),
                _applyTimerRespiratory.Set (5000),
                _applyTimerObstetric.Set (30000)
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

            /* Tie the Patient's Timer to the main Simulation Timer */
            Instance.Timer_Simulation.Tick += Instance.Physiology.ProcessTimers;

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
            
            OnPhysiologyEvent (this, new Physiology.PhysiologyEventArgs (Instance.Physiology, 
                Physiology.PhysiologyEventTypes.VitalsChange, 
                Instance.Timer_Simulation));
        }

        private async Task UnloadPatientEvents () {
            if (Instance?.Physiology is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (UnloadPatientEvents)}");
                return;
            }

            /* Unloading the Patient from the Main Timer also stops all the Patient's Timers
            /* and results in that Patient not triggering PatientEvent's */
            Instance.Timer_Simulation.Tick -= Instance.Physiology.ProcessTimers;

            /* But it's still important to clear PatientEvent subscriptions so they're not adding
            /* as duplicates when InitPatientEvents() is called!! */
            await Instance.Physiology.UnsubscribePhysiologyEvent ();
        }

        private Task InitDeviceMonitor () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (InitDeviceMonitor)}");
                return Task.CompletedTask;
            }

            if (!this.IsVisible)                    // Avalonia's parent must be visible to attach a window
                this.Show ();

            if (Instance.Device_Monitor is null || Instance.Device_Monitor.WindowStatus != DeviceWindow.WindowStates.Active)
                Instance.Device_Monitor = new DeviceMonitor (Instance);

            Instance.Device_Monitor.Activate ();
            Instance.Device_Monitor.Show ();

            if (Instance.Physiology is not null)
                Instance.Physiology.PhysiologyEvent += Instance.Device_Monitor.OnPhysiologyEvent;

            return Task.CompletedTask;
        }

        private async Task InitDeviceECG () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (InitDeviceECG)}");
                return;
            }

            await Dispatcher.UIThread.InvokeAsync (() => {
                if (!this.IsVisible)                    // Avalonia's parent must be visible to attach a window
                    this.Show ();

                if (Instance.Device_ECG is null || Instance.Device_ECG.WindowStatus != DeviceWindow.WindowStates.Active)
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

            await Dispatcher.UIThread.InvokeAsync (() => {
                if (!this.IsVisible)                    // Avalonia's parent must be visible to attach a window
                    this.Show ();

                if (Instance.Device_Defib is null || Instance.Device_Defib.WindowStatus != DeviceWindow.WindowStates.Active)
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

            await Dispatcher.UIThread.InvokeAsync (() => {
                if (!this.IsVisible)                    // Avalonia's parent must be visible to attach a window
                    this.Show ();

                if (Instance.Device_IABP is null || Instance.Device_IABP.WindowStatus != DeviceWindow.WindowStates.Active)
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
            await Dispatcher.UIThread.InvokeAsync (() => {
                if (!this.IsVisible)                    // Avalonia's parent must be visible to attach a window
                    this.Show ();

                if (Instance.Device_EFM is null || Instance.Device_EFM.WindowStatus != DeviceWindow.WindowStates.Active)
                    Instance.Device_EFM = new DeviceEFM (Instance);

                Instance.Device_EFM.Activate ();
                Instance.Device_EFM.Show ();

                if (Instance.Physiology is not null)
                    Instance.Physiology.PhysiologyEvent += Instance.Device_EFM.OnPhysiologyEvent;
            });
        }

        private async Task MessageAudioUnavailable () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (MessageAudioUnavailable)}");
                return;
            }

            await Dispatcher.UIThread.InvokeAsync (async () => {
                DialogMessage dlg = new (Instance) {
                    Message = Instance.Language.Localize ("MESSAGE:AudioUnavailableMessage"),
                    Title = Instance.Language.Localize ("MESSAGE:AudioUnavailableTitle"),
                    Indicator = DialogMessage.Indicators.Error,
                    Option = DialogMessage.Options.Ok,
                };

                if (!this.IsVisible)                    // Avalonia's parent must be visible to attach a window
                    this.Show ();

                await dlg.AsyncShow (this);
            });
        }

        private void DialogEULA () {
            Dispatcher.UIThread.InvokeAsync (async () => {
                DialogEULA dlg = new (Instance);
                dlg.Activate ();

                if (!this.IsVisible)                    // Avalonia's parent must be visible to attach a window
                    this.Show ();

                await dlg.ShowDialog (this);
            });
        }

        private async Task DialogLanguage (bool reloadUi = false) {
            await Dispatcher.UIThread.InvokeAsync (async () => {
                var oldLang = Instance?.Language.Selection;
                DialogLanguage dlg = new (Instance);
                dlg.Activate ();

                if (!this.IsVisible)                    // Avalonia's parent must be visible to attach a window
                    this.Show ();

                await dlg.ShowDialog (this);

                reloadUi = oldLang != Instance?.Language.Selection;

                if (reloadUi)
                    InitInterface ();
            });
        }

        private async Task DialogMirrorBroadcast () {
            await Dispatcher.UIThread.InvokeAsync (async () => {
                DialogMirrorBroadcast dlg = new (Instance);
                dlg.Activate ();

                if (!this.IsVisible)                    // Avalonia's parent must be visible to attach a window
                    this.Show ();

                await dlg.ShowDialog (this);

                if (Instance is not null)
                    await Instance.Mirror.PostStep (
                        new Scenario.Step (Instance.Timer_Simulation) {
                            Physiology = Instance.Physiology ?? new Physiology (Instance.Timer_Simulation),
                        },
                        Instance.Server);
            });
        }

        private async Task DialogMirrorReceive () {
            await Dispatcher.UIThread.InvokeAsync (async () => {
                DialogMirrorReceive dlg = new (Instance);
                dlg.Activate ();

                if (!this.IsVisible)                    // Avalonia's parent must be visible to attach a window
                    this.Show ();

                await dlg.ShowDialog (this);
            });
        }

        public async Task DialogAbout () {
            await Dispatcher.UIThread.InvokeAsync (async () => {
                DialogAbout dlg = new (Instance);
                dlg.Activate ();

                if (!this.IsVisible)                    // Avalonia's parent must be visible to attach a window
                    this.Show ();

                await dlg.ShowDialog (this);
            });
        }

        private async Task DialogUpgrade () {
            await Dispatcher.UIThread.InvokeAsync (async () => {
                DialogUpgrade.UpgradeOptions decision = IISIM.DialogUpgrade.UpgradeOptions.None;

                DialogUpgrade dlg = new (Instance);
                dlg.Activate ();

                dlg.OnUpgradeRoute += (s, ea) => decision = ea.Route;

                if (!this.IsVisible)                    // Avalonia's parent must be visible to attach a window
                    this.Show ();

                await dlg.ShowDialog (this);

                dlg.Close();

                switch (decision) {
                    default:
                    case IISIM.DialogUpgrade.UpgradeOptions.None:
                    case IISIM.DialogUpgrade.UpgradeOptions.Delay:
                        return;

                    case IISIM.DialogUpgrade.UpgradeOptions.Mute:
                        if (Instance is not null) {
                            Instance.Settings.MuteUpgrade = true;
                            Instance.Settings.MuteUpgradeDate = DateTime.Now;
                            Instance.Settings.Save ();
                        }
                        return;

                    case IISIM.DialogUpgrade.UpgradeOptions.Website:
                        if (!String.IsNullOrEmpty (Instance?.Server.UpgradeWebpage))
                            InterOp.OpenBrowser (Instance.Server.UpgradeWebpage);
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

            await Dispatcher.UIThread.InvokeAsync (() => {
                this.GetControl<MenuItem> ("menuToggleAudio").Header = String.Format ("{0}: {1}",
                    Instance.Language.Localize ("PE:MenuToggleAudio"),
                    Instance.Settings.AudioEnabled ? Instance.Language.Localize ("BOOLEAN:On") : Instance.Language.Localize ("BOOLEAN:Off"));
            });
        }

        private async Task ToggleHideDevices () {
            _hideDeviceLabels = !_hideDeviceLabels;

            await Dispatcher.UIThread.InvokeAsync (() => {
                this.GetControl<Panel> ("panelDevicesExpanded").IsVisible = !_hideDeviceLabels;
                this.GetControl<Panel> ("panelDevicesHidden").IsVisible = _hideDeviceLabels;
            });
        }

        private async Task CheckUpgrade () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (CheckUpgrade)}");
                return;
            }

            // Check with server for updated version of Infirmary Integrated - notify user either way
            await Instance.Server.Get_LatestVersion ();

            string version = Assembly.GetExecutingAssembly ()?.GetName ()?.Version?.ToString (3) ?? "0.0.0";
            if (Utility.IsNewerVersion (version, Instance.Server.UpgradeVersion)) {
                await DialogUpgrade ();
            } else {
                await Dispatcher.UIThread.InvokeAsync (async () => {
                    DialogMessage dlg = new (Instance) {
                        Title =  Instance.Language.Localize ("UPGRADE:Upgrade"),
                        Message = Instance.Language.Localize ("UPGRADE:NoUpdateAvailable"),
                        Indicator = DialogMessage.Indicators.Information,
                        Option = DialogMessage.Options.Ok,
                    };
                    dlg.Activate ();

                    if (!this.IsVisible)                    // Avalonia's parent must be visible to attach a window
                        this.Show ();

                    await dlg.AsyncShow (this);
                });
            }
        }

        private Task OpenUpgrade () {
            if (!String.IsNullOrEmpty (Instance?.Server.UpgradeWebpage))
                InterOp.OpenBrowser (Instance.Server.UpgradeWebpage);

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
            Dispatcher.UIThread.InvokeAsync (() => {
                MenuItem miStatus = this.GetControl<MenuItem> ("menuMirrorStatus");

                miStatus.Header = (Instance?.Mirror.Status) switch {
                    Mirror.Statuses.HOST => $"{Instance?.Language.Localize ("MIRROR:Status")}: {Instance?.Language.Localize ("MIRROR:Server")}",
                    Mirror.Statuses.CLIENT => $"{Instance?.Language.Localize ("MIRROR:Status")}: {Instance?.Language.Localize ("MIRROR:Client")}",
                    _ => $"{Instance?.Language.Localize ("MIRROR:Status")}: {Instance?.Language.Localize ("MIRROR:Inactive")}",
                };
            });

            return Task.CompletedTask;
        }

        private async Task UpdateExpanders ()
            => await UpdateExpanders (Instance?.Scenario?.IsLoaded ?? false);

        private Task UpdateExpanders (bool isScene) {
            Dispatcher.UIThread.InvokeAsync (() => {
                this.GetControl<Border> ("brdScenarioPlayer").IsVisible = isScene;
                this.GetControl<Expander> ("expScenarioPlayer").IsEnabled = isScene;
                this.GetControl<Expander> ("expScenarioPlayer").IsExpanded = isScene;
            });

            return Task.CompletedTask;
        }

        private async Task SetParameterStatus (bool autoApplyChanges) {
            _parameterStatus = autoApplyChanges
               ? ParameterStatuses.AutoApply
               : ParameterStatuses.ChangesApplied;

            await UpdateParameterIndicators ();
        }

        private async Task AdvanceParameterStatus (ParameterStatuses status) {
            /* Toggles between pending changes or changes applied; bypasses if auto-applying or null */

            if (status == ParameterStatuses.ChangesApplied && _parameterStatus == ParameterStatuses.ChangesPending)
                _parameterStatus = ParameterStatuses.ChangesApplied;
            else if (status == ParameterStatuses.ChangesPending && _parameterStatus == ParameterStatuses.ChangesApplied)
                _parameterStatus = ParameterStatuses.ChangesPending;

            await UpdateParameterIndicators ();
        }

        private Task UpdateParameterIndicators () {
            Dispatcher.UIThread.InvokeAsync (() => {
                Border brdPendingChangesIndicator = this.GetControl<Border> ("brdPendingChangesIndicator");

                brdPendingChangesIndicator.BorderBrush = _parameterStatus switch {
                    ParameterStatuses.ChangesPending => Brushes.Red,
                    ParameterStatuses.ChangesApplied => Brushes.Green,
                    ParameterStatuses.AutoApply => Brushes.Orange,
                    _ => Brushes.Transparent,
                };
            });
            return Task.CompletedTask;
        }

        private async Task LoadFile () {
            Window wdwControl = this.GetControl<Window> ("wdwControl");
            
            if (!wdwControl.StorageProvider.CanOpen) {
                return;
            }
            
            var fpoo = new FilePickerOpenOptions ()
            {
                SuggestedFileName = "simulation",
                AllowMultiple = false,
                
                FileTypeFilter = [
                    new FilePickerFileType("Infirmary Integrated Simulations")
                    {
                        Patterns = ["*.ii"],
                        MimeTypes = ["text/plain"]
                    }
                ],
                SuggestedStartLocation = await wdwControl.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents)
            };

            var files = await wdwControl.StorageProvider.OpenFilePickerAsync(fpoo);

            if (files.First()?.TryGetLocalPath() is not null) {
                await LoadInit (files?.First()?.TryGetLocalPath() ?? "");
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
                Physiology.PhysiologyEventTypes.VitalsChange,
                Instance?.Timer_Simulation));
        }

        private async Task LoadInit (string incFile) {
            if (String.IsNullOrEmpty (incFile))
                return;
            
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
                            Instance.Scenario = new (Instance.Timer_Simulation,true);

                        pbuffer = new StringBuilder ();
                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null
                                && pline != "> End: Physiology")
                            pbuffer.AppendLine (pline);

                        await RefreshScenario (true);
                        await (Instance?.Physiology?.Load (pbuffer.ToString ()) ?? Task.CompletedTask);
                    } else if (line == "> Begin: Scenario") {   // Load files saved by Infirmary Integrated Scenario Editor
                        Instance.Scenario ??= new (Instance.Timer_Simulation,false);

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
            Instance?.Physiology?.OnPhysiologyEvent (
                Physiology.PhysiologyEventTypes.VitalsChange,
                Instance?.Timer_Simulation);
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
                            case "checkDefaultVitals": this.GetControl<CheckBox> ("checkDefaultVitals").IsChecked = bool.Parse (pValue); break;
                        }
                    }
                }
            } catch {
                sRead.Close ();
                return;
            }

            sRead.Close ();
        }

        private async Task LoadFail () {
            await Dispatcher.UIThread.InvokeAsync (async () => {
                DialogMessage dlg = new (Instance) {
                    Message = Instance?.Language.Localize ("PE:LoadFailMessage"),
                    Title = Instance?.Language.Localize ("PE:LoadFailTitle"),
                    Indicator = DialogMessage.Indicators.Error,
                    Option = DialogMessage.Options.Ok,
                };

                if (!this.IsVisible)                    // Avalonia's parent must be visible to attach a window
                    this.Show ();

                await dlg.AsyncShow (this);
            });
        }

        private async Task SaveFile () {
            // Only save single Patient files in base Infirmary Integrated!
            // Scenario files should be created/edited/saved via II Scenario Editor!
            if (Instance?.Scenario?.IsLoaded ?? false) {
                await Dispatcher.UIThread.InvokeAsync (async () => {
                    DialogMessage dlg = new (Instance) {
                        Message = Instance.Language.Localize ("PE:SaveFailScenarioMessage"),
                        Title = Instance.Language.Localize ("PE:SaveFailScenarioTitle"),
                        Indicator = DialogMessage.Indicators.Error,
                        Option = DialogMessage.Options.Ok,
                    };

                    if (!this.IsVisible)                    // Avalonia's parent must be visible to attach a window
                        this.Show ();

                    await dlg.AsyncShow (this);
                });

                return;
            }

            Window wdwControl = this.GetControl<Window> ("wdwControl");
            
            if (!wdwControl.StorageProvider.CanSave) {
                return;
            }

            // 3. Define options for the file picker
            var fpso = new FilePickerSaveOptions
            {
                SuggestedFileName = "simulation",
                DefaultExtension = ".ii",
                FileTypeChoices = [
                    new FilePickerFileType("Infirmary Integrated Simulations")
                    {
                        Patterns = ["*.ii"],
                        MimeTypes = ["text/plain"]
                    }
                ],
                SuggestedStartLocation = await wdwControl.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents)
            };

            // 4. Call SaveFilePickerAsync
            IStorageFile? file = await wdwControl.StorageProvider.SaveFilePickerAsync(fpso);

            // 5. Process the result
            if (file is not null && file.TryGetLocalPath () is not null) {
                await SaveT1 (file.TryGetLocalPath () ?? ""); 
            }
        }

        private async Task SaveT1 (string filename) {
            if (String.IsNullOrEmpty (filename))
                return;
            
            if (System.IO.File.Exists (filename))
                System.IO.File.Delete (filename);

            using FileStream s = new (filename, FileMode.OpenOrCreate, FileAccess.Write);

            // Ensure only saving Patient file, not Scenario file; is screened in SaveFile()
            if (Instance?.Scenario != null && Instance.Scenario.IsLoaded) {
                s.Close ();
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

            using StreamWriter sw = new (s);
            await sw.WriteLineAsync (".ii:t1");                                           // Metadata (type 1 savefile)
            await sw.WriteLineAsync (Encryption.HashSHA256 (sb.ToString ().Trim ()));     // Hash for validation
            await sw.WriteAsync (Encryption.EncryptAES (sb.ToString ().Trim ()));         // Savefile data encrypted with AES
            await sw.FlushAsync ();

            sw.Close ();
            s.Close ();
        }

        private string SaveOptions () {
            StringBuilder sWrite = new ();

            sWrite.AppendLine (String.Format ("{0}:{1}", "checkDefaultVitals", this.GetControl<CheckBox> ("checkDefaultVitals").IsChecked));

            return sWrite.ToString ();
        }

        public Task Exit () {
            Instance?.Settings.Save ();
            Instance?.Exit ();

            return Task.CompletedTask;
        }

        private void OnMirrorTick (object? sender, EventArgs e) {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (OnMirrorTick)}");
                return;
            }

            Instance?.Mirror.TimerTick (
                new Scenario.Step (Instance.Timer_Simulation) {
                    Physiology = Instance.Physiology
                },
                Instance.Server);

            if (Instance?.Mirror.Status == Mirror.Statuses.CLIENT) {
                UpdateView (Instance.Physiology);
            }
        }

        private void OnStepChangeRequest (object? sender, EventArgs e)
            => _ = UnloadPatientEvents ();

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
            Scenario.Step step = Instance?.Scenario?.Current ?? new Scenario.Step (Instance?.Timer_Simulation);

            Label lblScenarioStep = this.GetControl<Label> ("lblScenarioStep");
            Label lblTimerStep = this.GetControl<Label> ("lblTimerStep");
            StackPanel stackProgressions = this.GetControl<StackPanel> ("stackProgressions");

            // Set Previous, Next, Pause, and Play buttons .IsEnabled based on Step properties
            this.GetControl<Button> ("btnPreviousStep").IsEnabled = (!String.IsNullOrEmpty (step.DefaultSource));
            this.GetControl<Button> ("btnNextStep").IsEnabled = (!String.IsNullOrEmpty (step.DefaultProgression?.UUID) || step.Progressions.Count > 0);
            this.GetControl<Button> ("btnPauseStep").IsEnabled = (step.ProgressTimer > 0);
            this.GetControl<Button> ("btnPlayStep").IsEnabled = false;

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
                        Margin = (i == step.Progressions.Count - 1 ? new Thickness (10, 5, 10, 10) : new Thickness (10, 5))
                    });
                }
            }
        }

        private async Task NextStep () {
            StackPanel stackProgressions = this.GetControl<StackPanel> ("stackProgressions");

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
            this.GetControl<Button> ("btnPauseStep").IsEnabled = false;
            this.GetControl<Button> ("btnPlayStep").IsEnabled = true;

            if (Instance?.Scenario is not null)
                await Instance.Scenario.PauseStep ();

            this.GetControl<Label> ("lblTimerStep").Content = Instance?.Language.Localize ("PE:ProgressionPaused");
        }

        private async Task PlayStep () {
            this.GetControl<Button> ("btnPauseStep").IsEnabled = true;
            this.GetControl<Button> ("btnPlayStep").IsEnabled = false;

            if (Instance?.Scenario is not null)
                await Instance.Scenario.PlayStep ();

            Label lblTimerStep = this.GetControl<Label> ("lblTimerStep");

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

            _applyBuffer ??= new (Instance?.Timer_Simulation);

            await ApplyPhysiologyParameters_Buffer (_applyBuffer);
            _applyPendingCardiac = true;
            _applyPendingRespiratory = true;
            _applyPendingObstetric = true;

            await _applyTimerCardiac.ResetStart ();
            await _applyTimerRespiratory.ResetStart ();
            await _applyTimerObstetric.ResetStart ();
        }

        private async Task ApplyPhysiologyParameters_Buffer (Physiology? p) {
            if (p is null)
                return;

            ComboBox comboCardiacRhythm = this.GetControl<ComboBox> ("comboCardiacRhythm");
            ComboBox comboRespiratoryRhythm = this.GetControl<ComboBox> ("comboRespiratoryRhythm");
            ComboBox comboPACatheterPlacement = this.GetControl<ComboBox> ("comboPACatheterPlacement");
            ComboBox comboCardiacAxis = this.GetControl<ComboBox> ("comboCardiacAxis");
            ComboBox comboFHRRhythm = this.GetControl<ComboBox> ("comboFHRRhythm");

            await p.UpdateParameters_Cardiac (
                // Basic vital signs
                (int)(this.GetControl<NumericUpDown> ("numHR")?.Value ?? 0),
                (int)(this.GetControl<NumericUpDown> ("numNSBP")?.Value ?? 0),
                (int)(this.GetControl<NumericUpDown> ("numNDBP")?.Value ?? 0),
                Physiology.CalculateMAP (
                    (int)(this.GetControl<NumericUpDown> ("numNSBP")?.Value ?? 0), 
                    (int)(this.GetControl<NumericUpDown> ("numNDBP")?.Value ?? 0)),
                (int)(this.GetControl<NumericUpDown> ("numSPO2")?.Value ?? 0),
                (double)(this.GetControl<NumericUpDown> ("numT")?.Value ?? 0),

                (CardiacRhythms.Values)(Enum.GetValues (typeof (CardiacRhythms.Values)).GetValue (
                    comboCardiacRhythm.SelectedIndex < 0 ? 0 : comboCardiacRhythm.SelectedIndex)
                    ?? CardiacRhythms.Values.SinusRhythm),

                // Advanced hemodynamics
                (int)(this.GetControl<NumericUpDown> ("numCVP")?.Value ?? 0),
                (int)(this.GetControl<NumericUpDown> ("numASBP")?.Value ?? 0),
                (int)(this.GetControl<NumericUpDown> ("numADBP")?.Value ?? 0),
                Physiology.CalculateMAP ((int)(this.GetControl<NumericUpDown> ("numASBP")?.Value ?? 0), (int)(this.GetControl<NumericUpDown> ("numADBP")?.Value ?? 0)),

                (float)(this.GetControl<NumericUpDown> ("numCO")?.Value ?? 0),

                (PulmonaryArteryRhythms.Values)(Enum.GetValues (typeof (PulmonaryArteryRhythms.Values)).GetValue (
                    comboPACatheterPlacement.SelectedIndex < 0 ? 0 : comboPACatheterPlacement.SelectedIndex)
                    ?? PulmonaryArteryRhythms.Values.RightAtrium),

                (int)(this.GetControl<NumericUpDown> ("numPSP")?.Value ?? 0),
                (int)(this.GetControl<NumericUpDown> ("numPDP")?.Value ?? 0),
                Physiology.CalculateMAP ((int)(this.GetControl<NumericUpDown> ("numPSP")?.Value ?? 0),
                (int)(this.GetControl<NumericUpDown> ("numPDP")?.Value ?? 0)),

                (int)(this.GetControl<NumericUpDown> ("numICP")?.Value ?? 0),
                (int)(this.GetControl<NumericUpDown> ("numIAP")?.Value ?? 0),

                // Cardiac Profile
                (int)(this.GetControl<NumericUpDown> ("numPacemakerCaptureThreshold")?.Value ?? 0),
                this.GetControl<CheckBox> ("chkPulsusParadoxus").IsChecked ?? false,
                this.GetControl<CheckBox> ("chkPulsusAlternans").IsChecked ?? false,
                this.GetControl<CheckBox> ("chkElectricalAlternans").IsChecked ?? false,

                (Cardiac_Axes.Values)(Enum.GetValues (typeof (Cardiac_Axes.Values)).GetValue (
                    comboCardiacAxis.SelectedIndex < 0 ? 0 : comboCardiacAxis.SelectedIndex)
                    ?? Cardiac_Axes.Values.Normal),

                (double)(this.GetControl<NumericUpDown> ("numQRSInterval")?.Value ?? 0),
                (double)(this.GetControl<NumericUpDown> ("numQTcInterval")?.Value ?? 0),

                [
                    (double)(this.GetControl<NumericUpDown>("numPAmp_I")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numPAmp_II")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numPAmp_III")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numPAmp_aVR")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numPAmp_aVL")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numPAmp_aVF")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numPAmp_V1")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numPAmp_V2")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numPAmp_V3")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numPAmp_V4")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numPAmp_V5")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numPAmp_V6")?.Value ?? 1)
                ],
                [
                    (double)(this.GetControl<NumericUpDown>("numQAmp_I")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numQAmp_II")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numQAmp_III")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numQAmp_aVR")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numQAmp_aVL")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numQAmp_aVF")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numQAmp_V1")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numQAmp_V2")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numQAmp_V3")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numQAmp_V4")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numQAmp_V5")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numQAmp_V6")?.Value ?? 1)
                ],
                [
                    (double)(this.GetControl<NumericUpDown>("numRAmp_I")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numRAmp_II")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numRAmp_III")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numRAmp_aVR")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numRAmp_aVL")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numRAmp_aVF")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numRAmp_V1")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numRAmp_V2")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numRAmp_V3")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numRAmp_V4")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numRAmp_V5")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numRAmp_V6")?.Value ?? 0)
                ],
                [
                    (double)(this.GetControl<NumericUpDown>("numSAmp_I")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numSAmp_II")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numSAmp_III")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numSAmp_aVR")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numSAmp_aVL")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numSAmp_aVF")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numSAmp_V1")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numSAmp_V2")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numSAmp_V3")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numSAmp_V4")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numSAmp_V5")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numSAmp_V6")?.Value ?? 1)
                ],
                [
                    (double)(this.GetControl<NumericUpDown>("numTAmp_I")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numTAmp_II")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numTAmp_III")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numTAmp_aVR")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numTAmp_aVL")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numTAmp_aVF")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numTAmp_V1")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numTAmp_V2")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numTAmp_V3")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numTAmp_V4")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numTAmp_V5")?.Value ?? 1),
                    (double)(this.GetControl<NumericUpDown>("numTAmp_V6")?.Value ?? 1)
                ],

                [
                    (double)(this.GetControl<NumericUpDown>("numSTE_I")?.Value ?? 0),
                    (double)(this.GetControl<NumericUpDown>("numSTE_II")?.Value ?? 0),
                    (double)(this.GetControl<NumericUpDown>("numSTE_III")?.Value ?? 0),
                    (double)(this.GetControl<NumericUpDown>("numSTE_aVR")?.Value ?? 0),
                    (double)(this.GetControl<NumericUpDown>("numSTE_aVL")?.Value ?? 0),
                    (double)(this.GetControl<NumericUpDown>("numSTE_aVF")?.Value ?? 0),
                    (double)(this.GetControl<NumericUpDown>("numSTE_V1")?.Value ?? 0),
                    (double)(this.GetControl<NumericUpDown>("numSTE_V2")?.Value ?? 0),
                    (double)(this.GetControl<NumericUpDown>("numSTE_V3")?.Value ?? 0),
                    (double)(this.GetControl<NumericUpDown>("numSTE_V4")?.Value ?? 0),
                    (double)(this.GetControl<NumericUpDown>("numSTE_V5")?.Value ?? 0),
                    (double)(this.GetControl<NumericUpDown>("numSTE_V6")?.Value ?? 0)
                ]
            );

            await p.UpdateParameters_Respiratory (

                (int)(this.GetControl<NumericUpDown> ("numRR")?.Value ?? 0),
                (Respiratory_Rhythms.Values)(Enum.GetValues (typeof (Respiratory_Rhythms.Values)).GetValue (
                    comboRespiratoryRhythm.SelectedIndex < 0 ? 0 : comboRespiratoryRhythm.SelectedIndex)
                    ?? Respiratory_Rhythms.Values.Regular),
                (int)(this.GetControl<NumericUpDown> ("numETCO2")?.Value ?? 0),

                this.GetControl<CheckBox> ("chkMechanicallyVentilated").IsChecked ?? false,
                (float)(this.GetControl<NumericUpDown> ("numInspiratoryRatio")?.Value ?? 0),
                (float)(this.GetControl<NumericUpDown> ("numExpiratoryRatio")?.Value ?? 0));

            await p.UpdateParameters_Obstetric (
                (int)(this.GetControl<NumericUpDown> ("numFHR")?.Value ?? 0),
                (int)(this.GetControl<NumericUpDown> ("numFHRVariability")?.Value ?? 0),

                (FetalHeart_Rhythms.Values)(Enum.GetValues (typeof (FetalHeart_Rhythms.Values)).GetValue (
                    comboFHRRhythm.SelectedIndex < 0 ? 0 : comboFHRRhythm.SelectedIndex)
                    ?? FetalHeart_Rhythms.Values.Baseline),

                (int)(this.GetControl<NumericUpDown> ("numUCFrequency")?.Value ?? 0),
                (int)(this.GetControl<NumericUpDown> ("numUCDuration")?.Value ?? 0),
                (int)(this.GetControl<NumericUpDown> ("numUCIntensity")?.Value ?? 0),
                (int)(this.GetControl<NumericUpDown> ("numUCResting")?.Value ?? 0));
        }

        private void ApplyPhysiologyParameters_Cardiac (object? sender, EventArgs e) {
            if (_applyPendingCardiac != true || _applyBuffer is null)
                return;

            _applyPendingCardiac = false;
            _ = _applyTimerCardiac.ResetStop ();

            if (Instance?.Physiology is not null) {
                _ = Instance.Physiology.UpdateParameters_Cardiac (
                    _applyBuffer.HR,
                    _applyBuffer.NSBP, _applyBuffer.NDBP, _applyBuffer.NMAP,
                    _applyBuffer.SPO2,
                    _applyBuffer.T,
                    _applyBuffer.CardiacRhythm.Value,

                    _applyBuffer.CVP,
                    _applyBuffer.ASBP, _applyBuffer.ADBP, _applyBuffer.AMAP,

                    _applyBuffer.CO,
                    _applyBuffer.PulmonaryArteryPlacement.Value,
                    _applyBuffer.PSP, _applyBuffer.PDP, _applyBuffer.PMP,

                    _applyBuffer.ICP,
                    _applyBuffer.IAP,

                    _applyBuffer.PacemakerThreshold,
                    _applyBuffer.PulsusParadoxus,
                    _applyBuffer.PulsusAlternans,
                    _applyBuffer.ElectricalAlternans,

                    _applyBuffer.CardiacAxis.Value,
                    _applyBuffer.QRSInterval, 
                    _applyBuffer.QTcInterval,
                    
                    _applyBuffer.PAmplitude,
                    _applyBuffer.QAmplitude,
                    _applyBuffer.RAmplitude,
                    _applyBuffer.SAmplitude,
                    _applyBuffer.TAmplitude,
                    
                    _applyBuffer.STElevation);
            }

            if (Instance?.Mirror is not null && Instance?.Server is not null)
                _ = Instance.Mirror.PostStep (
                    new Scenario.Step (Instance.Timer_Simulation) {
                        Physiology = Instance.Physiology,
                    },
                    Instance.Server);
        }

        private void ApplyPhysiologyParameters_Respiratory (object? sender, EventArgs e) {
            if (_applyPendingRespiratory != true || _applyBuffer is null)
                return;

            _applyPendingRespiratory = false;
            _ = _applyTimerRespiratory.ResetStop ();

            if (Instance?.Physiology is not null) {
                _ = Instance.Physiology.UpdateParameters_Respiratory (
                    _applyBuffer.RR,
                    _applyBuffer.RespiratoryRhythm.Value,
                    _applyBuffer.ETCO2,
                    _applyBuffer.MechanicallyVentilated,
                    _applyBuffer.RRIEI, _applyBuffer.RRIEE);
            }

            if (Instance?.Mirror is not null && Instance?.Server is not null)
                _ = Instance.Mirror.PostStep (
                    new Scenario.Step (Instance.Timer_Simulation) {
                        Physiology = Instance.Physiology,
                    },
                    Instance.Server);
        }

        private void ApplyPhysiologyParameters_Obstetric (object? sender, EventArgs e) {
            if (_applyPendingObstetric != true || _applyBuffer is null)
                return;

            _applyPendingObstetric = false;
            _ = _applyTimerObstetric.ResetStop ();

            if (Instance?.Physiology is not null) {
                _ = Instance.Physiology.UpdateParameters_Obstetric (
                    _applyBuffer.FetalHR,
                    _applyBuffer.ObstetricFetalRateVariability,
                    _applyBuffer.ObstetricFetalHeartRhythm.Value,
                    _applyBuffer.ObstetricContractionFrequency,
                    _applyBuffer.ObstetricContractionDuration,
                    _applyBuffer.ObstetricContractionIntensity,
                    _applyBuffer.ObstetricUterineRestingTone);
            }

            if (Instance?.Mirror is not null && Instance?.Server is not null)
                _ = Instance.Mirror.PostStep (
                    new Scenario.Step (Instance.Timer_Simulation) {
                        Physiology = Instance.Physiology,
                    },
                    Instance.Server);
        }

        private void UpdateView (Physiology? p) {
            Dispatcher.UIThread.InvokeAsync (() => {                        // Updating the UI requires being on the proper thread
                _parameterStatus = ParameterStatuses.Loading;                // To prevent each form update from auto-applying back to Patient

                if (p is null) {
                    Debug.WriteLine ($"Null return at {this.Name}.{nameof (InitDeviceMonitor)}");
                    return;
                }

                if (!_applyPendingCardiac) {
                    // Basic vital signs
                    this.GetControl<NumericUpDown> ("numHR").Value = p.VitalSignsSettings.HR;
                    this.GetControl<NumericUpDown> ("numNSBP").Value = p.VitalSignsSettings.NSBP;
                    this.GetControl<NumericUpDown> ("numNDBP").Value = p.VitalSignsSettings.NDBP;
                    this.GetControl<NumericUpDown> ("numSPO2").Value = p.VitalSignsSettings.SPO2;
                    this.GetControl<NumericUpDown> ("numT").Value = (decimal)p.VitalSignsSettings.T;
                    this.GetControl<ComboBox> ("comboCardiacRhythm").SelectedIndex = (int)p.CardiacRhythm.Value;

                    // Advanced hemodynamics
                    this.GetControl<NumericUpDown> ("numCVP").Value = p.VitalSignsSettings.CVP;
                    this.GetControl<NumericUpDown> ("numASBP").Value = p.VitalSignsSettings.ASBP;
                    this.GetControl<NumericUpDown> ("numADBP").Value = p.VitalSignsSettings.ADBP;
                    this.GetControl<NumericUpDown> ("numCO").Value = (decimal)p.VitalSignsSettings.CO;
                    this.GetControl<ComboBox> ("comboPACatheterPlacement").SelectedIndex =
                        (int)p.PulmonaryArteryPlacement.Value;
                    this.GetControl<NumericUpDown> ("numPSP").Value = p.VitalSignsSettings.PSP;
                    this.GetControl<NumericUpDown> ("numPDP").Value = p.VitalSignsSettings.PDP;
                    this.GetControl<NumericUpDown> ("numICP").Value = p.VitalSignsSettings.ICP;
                    this.GetControl<NumericUpDown> ("numIAP").Value = p.VitalSignsSettings.IAP;

                    // Cardiac profile
                    this.GetControl<NumericUpDown> ("numPacemakerCaptureThreshold").Value = p.PacemakerThreshold;
                    this.GetControl<CheckBox> ("chkPulsusParadoxus").IsChecked = p.PulsusParadoxus;
                    this.GetControl<CheckBox> ("chkPulsusAlternans").IsChecked = p.PulsusAlternans;
                    this.GetControl<CheckBox> ("chkElectricalAlternans").IsChecked = p.ElectricalAlternans;
                    this.GetControl<ComboBox> ("comboCardiacAxis").SelectedIndex = (int)p.CardiacAxis.Value;

                    this.GetControl<NumericUpDown> ("numQRSInterval").Value = (decimal)p.QRSInterval;
                    this.GetControl<NumericUpDown> ("numQTcInterval").Value = (decimal)p.QTcInterval;

                    if (p.PAmplitude is not null) {
                        this.GetControl<NumericUpDown> ("numPAmp_I").Value =
                            (decimal)p.PAmplitude [(int)Lead.Values.ECGI];
                        this.GetControl<NumericUpDown> ("numPAmp_II").Value =
                            (decimal)p.PAmplitude [(int)Lead.Values.ECGII];
                        this.GetControl<NumericUpDown> ("numPAmp_III").Value =
                            (decimal)p.PAmplitude [(int)Lead.Values.ECGIII];
                        this.GetControl<NumericUpDown> ("numPAmp_aVR").Value =
                            (decimal)p.PAmplitude [(int)Lead.Values.ECGAVR];
                        this.GetControl<NumericUpDown> ("numPAmp_aVL").Value =
                            (decimal)p.PAmplitude [(int)Lead.Values.ECGAVL];
                        this.GetControl<NumericUpDown> ("numPAmp_aVF").Value =
                            (decimal)p.PAmplitude [(int)Lead.Values.ECGAVF];
                        this.GetControl<NumericUpDown> ("numPAmp_V1").Value =
                            (decimal)p.PAmplitude [(int)Lead.Values.ECGV1];
                        this.GetControl<NumericUpDown> ("numPAmp_V2").Value =
                            (decimal)p.PAmplitude [(int)Lead.Values.ECGV2];
                        this.GetControl<NumericUpDown> ("numPAmp_V3").Value =
                            (decimal)p.PAmplitude [(int)Lead.Values.ECGV3];
                        this.GetControl<NumericUpDown> ("numPAmp_V4").Value =
                            (decimal)p.PAmplitude [(int)Lead.Values.ECGV4];
                        this.GetControl<NumericUpDown> ("numPAmp_V5").Value =
                            (decimal)p.PAmplitude [(int)Lead.Values.ECGV5];
                        this.GetControl<NumericUpDown> ("numPAmp_V6").Value =
                            (decimal)p.PAmplitude [(int)Lead.Values.ECGV6];
                    }

                    if (p.QAmplitude is not null) {
                        this.GetControl<NumericUpDown> ("numQAmp_I").Value =
                            (decimal)p.QAmplitude [(int)Lead.Values.ECGI];
                        this.GetControl<NumericUpDown> ("numQAmp_II").Value =
                            (decimal)p.QAmplitude [(int)Lead.Values.ECGII];
                        this.GetControl<NumericUpDown> ("numQAmp_III").Value =
                            (decimal)p.QAmplitude [(int)Lead.Values.ECGIII];
                        this.GetControl<NumericUpDown> ("numQAmp_aVR").Value =
                            (decimal)p.QAmplitude [(int)Lead.Values.ECGAVR];
                        this.GetControl<NumericUpDown> ("numQAmp_aVL").Value =
                            (decimal)p.QAmplitude [(int)Lead.Values.ECGAVL];
                        this.GetControl<NumericUpDown> ("numQAmp_aVF").Value =
                            (decimal)p.QAmplitude [(int)Lead.Values.ECGAVF];
                        this.GetControl<NumericUpDown> ("numQAmp_V1").Value =
                            (decimal)p.QAmplitude [(int)Lead.Values.ECGV1];
                        this.GetControl<NumericUpDown> ("numQAmp_V2").Value =
                            (decimal)p.QAmplitude [(int)Lead.Values.ECGV2];
                        this.GetControl<NumericUpDown> ("numQAmp_V3").Value =
                            (decimal)p.QAmplitude [(int)Lead.Values.ECGV3];
                        this.GetControl<NumericUpDown> ("numQAmp_V4").Value =
                            (decimal)p.QAmplitude [(int)Lead.Values.ECGV4];
                        this.GetControl<NumericUpDown> ("numQAmp_V5").Value =
                            (decimal)p.QAmplitude [(int)Lead.Values.ECGV5];
                        this.GetControl<NumericUpDown> ("numQAmp_V6").Value =
                            (decimal)p.QAmplitude [(int)Lead.Values.ECGV6];
                    }

                    if (p.RAmplitude is not null) {
                        this.GetControl<NumericUpDown> ("numRAmp_I").Value =
                            (decimal)p.RAmplitude [(int)Lead.Values.ECGI];
                        this.GetControl<NumericUpDown> ("numRAmp_II").Value =
                            (decimal)p.RAmplitude [(int)Lead.Values.ECGII];
                        this.GetControl<NumericUpDown> ("numRAmp_III").Value =
                            (decimal)p.RAmplitude [(int)Lead.Values.ECGIII];
                        this.GetControl<NumericUpDown> ("numRAmp_aVR").Value =
                            (decimal)p.RAmplitude [(int)Lead.Values.ECGAVR];
                        this.GetControl<NumericUpDown> ("numRAmp_aVL").Value =
                            (decimal)p.RAmplitude [(int)Lead.Values.ECGAVL];
                        this.GetControl<NumericUpDown> ("numRAmp_aVF").Value =
                            (decimal)p.RAmplitude [(int)Lead.Values.ECGAVF];
                        this.GetControl<NumericUpDown> ("numRAmp_V1").Value =
                            (decimal)p.RAmplitude [(int)Lead.Values.ECGV1];
                        this.GetControl<NumericUpDown> ("numRAmp_V2").Value =
                            (decimal)p.RAmplitude [(int)Lead.Values.ECGV2];
                        this.GetControl<NumericUpDown> ("numRAmp_V3").Value =
                            (decimal)p.RAmplitude [(int)Lead.Values.ECGV3];
                        this.GetControl<NumericUpDown> ("numRAmp_V4").Value =
                            (decimal)p.RAmplitude [(int)Lead.Values.ECGV4];
                        this.GetControl<NumericUpDown> ("numRAmp_V5").Value =
                            (decimal)p.RAmplitude [(int)Lead.Values.ECGV5];
                        this.GetControl<NumericUpDown> ("numRAmp_V6").Value =
                            (decimal)p.RAmplitude [(int)Lead.Values.ECGV6];
                    }

                    if (p.SAmplitude is not null) {
                        this.GetControl<NumericUpDown> ("numSAmp_I").Value =
                            (decimal)p.SAmplitude [(int)Lead.Values.ECGI];
                        this.GetControl<NumericUpDown> ("numSAmp_II").Value =
                            (decimal)p.SAmplitude [(int)Lead.Values.ECGII];
                        this.GetControl<NumericUpDown> ("numSAmp_III").Value =
                            (decimal)p.SAmplitude [(int)Lead.Values.ECGIII];
                        this.GetControl<NumericUpDown> ("numSAmp_aVR").Value =
                            (decimal)p.SAmplitude [(int)Lead.Values.ECGAVR];
                        this.GetControl<NumericUpDown> ("numSAmp_aVL").Value =
                            (decimal)p.SAmplitude [(int)Lead.Values.ECGAVL];
                        this.GetControl<NumericUpDown> ("numSAmp_aVF").Value =
                            (decimal)p.SAmplitude [(int)Lead.Values.ECGAVF];
                        this.GetControl<NumericUpDown> ("numSAmp_V1").Value =
                            (decimal)p.SAmplitude [(int)Lead.Values.ECGV1];
                        this.GetControl<NumericUpDown> ("numSAmp_V2").Value =
                            (decimal)p.SAmplitude [(int)Lead.Values.ECGV2];
                        this.GetControl<NumericUpDown> ("numSAmp_V3").Value =
                            (decimal)p.SAmplitude [(int)Lead.Values.ECGV3];
                        this.GetControl<NumericUpDown> ("numSAmp_V4").Value =
                            (decimal)p.SAmplitude [(int)Lead.Values.ECGV4];
                        this.GetControl<NumericUpDown> ("numSAmp_V5").Value =
                            (decimal)p.SAmplitude [(int)Lead.Values.ECGV5];
                        this.GetControl<NumericUpDown> ("numSAmp_V6").Value =
                            (decimal)p.SAmplitude [(int)Lead.Values.ECGV6];
                    }

                    if (p.TAmplitude is not null) {
                        this.GetControl<NumericUpDown> ("numTAmp_I").Value =
                            (decimal)p.TAmplitude [(int)Lead.Values.ECGI];
                        this.GetControl<NumericUpDown> ("numTAmp_II").Value =
                            (decimal)p.TAmplitude [(int)Lead.Values.ECGII];
                        this.GetControl<NumericUpDown> ("numTAmp_III").Value =
                            (decimal)p.TAmplitude [(int)Lead.Values.ECGIII];
                        this.GetControl<NumericUpDown> ("numTAmp_aVR").Value =
                            (decimal)p.TAmplitude [(int)Lead.Values.ECGAVR];
                        this.GetControl<NumericUpDown> ("numTAmp_aVL").Value =
                            (decimal)p.TAmplitude [(int)Lead.Values.ECGAVL];
                        this.GetControl<NumericUpDown> ("numTAmp_aVF").Value =
                            (decimal)p.TAmplitude [(int)Lead.Values.ECGAVF];
                        this.GetControl<NumericUpDown> ("numTAmp_V1").Value =
                            (decimal)p.TAmplitude [(int)Lead.Values.ECGV1];
                        this.GetControl<NumericUpDown> ("numTAmp_V2").Value =
                            (decimal)p.TAmplitude [(int)Lead.Values.ECGV2];
                        this.GetControl<NumericUpDown> ("numTAmp_V3").Value =
                            (decimal)p.TAmplitude [(int)Lead.Values.ECGV3];
                        this.GetControl<NumericUpDown> ("numTAmp_V4").Value =
                            (decimal)p.TAmplitude [(int)Lead.Values.ECGV4];
                        this.GetControl<NumericUpDown> ("numTAmp_V5").Value =
                            (decimal)p.TAmplitude [(int)Lead.Values.ECGV5];
                        this.GetControl<NumericUpDown> ("numTAmp_V6").Value =
                            (decimal)p.TAmplitude [(int)Lead.Values.ECGV6];
                    }

                    if (p.STElevation is not null) {
                        this.GetControl<NumericUpDown> ("numSTE_I").Value =
                            (decimal)p.STElevation [(int)Lead.Values.ECGI];
                        this.GetControl<NumericUpDown> ("numSTE_II").Value =
                            (decimal)p.STElevation [(int)Lead.Values.ECGII];
                        this.GetControl<NumericUpDown> ("numSTE_III").Value =
                            (decimal)p.STElevation [(int)Lead.Values.ECGIII];
                        this.GetControl<NumericUpDown> ("numSTE_aVR").Value =
                            (decimal)p.STElevation [(int)Lead.Values.ECGAVR];
                        this.GetControl<NumericUpDown> ("numSTE_aVL").Value =
                            (decimal)p.STElevation [(int)Lead.Values.ECGAVL];
                        this.GetControl<NumericUpDown> ("numSTE_aVF").Value =
                            (decimal)p.STElevation [(int)Lead.Values.ECGAVF];
                        this.GetControl<NumericUpDown> ("numSTE_V1").Value =
                            (decimal)p.STElevation [(int)Lead.Values.ECGV1];
                        this.GetControl<NumericUpDown> ("numSTE_V2").Value =
                            (decimal)p.STElevation [(int)Lead.Values.ECGV2];
                        this.GetControl<NumericUpDown> ("numSTE_V3").Value =
                            (decimal)p.STElevation [(int)Lead.Values.ECGV3];
                        this.GetControl<NumericUpDown> ("numSTE_V4").Value =
                            (decimal)p.STElevation [(int)Lead.Values.ECGV4];
                        this.GetControl<NumericUpDown> ("numSTE_V5").Value =
                            (decimal)p.STElevation [(int)Lead.Values.ECGV5];
                        this.GetControl<NumericUpDown> ("numSTE_V6").Value =
                            (decimal)p.STElevation [(int)Lead.Values.ECGV6];
                    }
                }

                if (!_applyPendingRespiratory) {
                    // Respiratory profile
                    this.GetControl<NumericUpDown> ("numRR").Value = p.VitalSignsSettings.RR;
                    this.GetControl<ComboBox> ("comboRespiratoryRhythm").SelectedIndex = (int)p.RespiratoryRhythm.Value;
                    this.GetControl<NumericUpDown> ("numETCO2").Value = p.VitalSignsSettings.ETCO2;
                    this.GetControl<CheckBox> ("chkMechanicallyVentilated").IsChecked = p.MechanicallyVentilated;
                    this.GetControl<NumericUpDown> ("numInspiratoryRatio").Value = (decimal)p.VitalSignsSettings.RRIEI;
                    this.GetControl<NumericUpDown> ("numExpiratoryRatio").Value = (decimal)p.VitalSignsSettings.RRIEE;
                }

                if (!_applyPendingObstetric) {
                    // Obstetric profile
                    this.GetControl<NumericUpDown> ("numFHR").Value = p.VitalSignsSettings.FetalHR;
                    this.GetControl<NumericUpDown> ("numFHRVariability").Value = p.ObstetricFetalRateVariability;
                    this.GetControl<NumericUpDown> ("numUCFrequency").Value = (decimal)p.ObstetricContractionFrequency;
                    this.GetControl<NumericUpDown> ("numUCDuration").Value = p.ObstetricContractionDuration;
                    this.GetControl<NumericUpDown> ("numUCIntensity").Value = p.ObstetricContractionIntensity;
                    this.GetControl<NumericUpDown> ("numUCResting").Value = p.ObstetricUterineRestingTone;
                    this.GetControl<ComboBox> ("comboFHRRhythm").SelectedIndex = (int)p.ObstetricFetalHeartRhythm.Value;
                }

                _ = SetParameterStatus (Instance?.Settings.AutoApplyChanges ?? false);     // Re-establish parameter status
            });
        }

        public void ToggleFullscreen () {
            if (WindowState == WindowState.FullScreen)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.FullScreen;
        }
        
        protected virtual void PauseSimulation () 
           => Instance?.PauseSimulation();
        
        
        private void MenuNewSimulation_Click (object sender, RoutedEventArgs e)
            => _ = RefreshScenario (true);

        private void MenuLoadFile_Click (object s, RoutedEventArgs e)
            => _ = LoadFile ();

        private void MenuSaveFile_Click (object s, RoutedEventArgs e)
            => _ = SaveFile ();

        private void MenuToggleFullscreen_Click (object s, RoutedEventArgs e)
            => ToggleFullscreen ();

        private void MenuPauseDevices_Click (object s, RoutedEventArgs e)
            => PauseSimulation ();
        
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

        private void MenuUpdate_Click (object s, RoutedEventArgs e)
            => _ = OpenUpgrade ();

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
        
        private void OnLoaded (object? sender, RoutedEventArgs e) {
            WindowStatus = WindowStates.Active;
            
            if (Instance?.Settings.UI?.Control is not null) {
                var pos = new PixelPoint (
                    Instance.Settings.UI.Control.X < 0 ? 0 : Instance.Settings.UI.Control.X ?? Position.X,
                    Instance.Settings.UI.Control.Y < 0 ? 0 : Instance.Settings.UI.Control.Y ?? Position.Y);

                if (Screens.All.Any(o => o.WorkingArea.Contains (pos))) { 
                    Position = pos;
                }

                if (Instance.Settings.UI.Control.WindowState == WindowState.Normal) {
                    SizeToContent = SizeToContent.Manual;
                    Width = Instance.Settings.UI.Control.Width ?? Width;
                    Height = Instance.Settings.UI.Control.Height ?? Height;
                } else {
                    WindowState = Instance.Settings.UI.Control.WindowState ?? WindowState;
                }
            }
        }
        
        private void OnClosing (object? sender, WindowClosingEventArgs e) {
            if (Instance?.Settings.UI is not null && WindowStatus == WindowStates.Active) {
                Instance.Settings.UI.Control = new() {
                    X = Position.X,
                    Y = Position.Y,
                    Width = Width,
                    Height = Height,
                    WindowState = WindowState
                };
                
                Instance.Settings.Save();
            }
        }

        private void OnClosed (object sender, EventArgs e) {
            WindowStatus = WindowStates.Closed;
            
            Exit ();
        }

        private void OnAutoApplyChanges_Changed (object sender, RoutedEventArgs e) {
            if (_parameterStatus == ParameterStatuses.Loading || Instance is null)
                return;

            Instance.Settings.AutoApplyChanges = this.GetControl<CheckBox> ("chkAutoApplyChanges").IsChecked ?? true;
            this.GetControl<Button> ("btnParametersReset").IsEnabled = !Instance.Settings.AutoApplyChanges;
            Instance.Settings.Save ();

            _ = SetParameterStatus (Instance.Settings.AutoApplyChanges);
        }

        private void OnUIPhysiologyParameter_KeyDown (object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter)
                OnUIPhysiologyParameter_Process (sender, e);
        }

        private void OnUIPhysiologyParameter_Changed (object sender, NumericUpDownValueChangedEventArgs e)
            => OnUIPhysiologyParameter_Process (sender, e);

        private void OnUIPhysiologyParameter_Changed (object sender, SelectionChangedEventArgs e)
            => OnUIPhysiologyParameter_Process (sender, e);

        private void OnUIPhysiologyParameter_Changed (object sender, RoutedEventArgs e)
            => OnUIPhysiologyParameter_Process (sender, e);

        private void OnUIPhysiologyParameter_LostFocus (object sender, RoutedEventArgs e)
            => OnUIPhysiologyParameter_Process (sender, e);

        private void OnUIPhysiologyParameter_Process (object sender, RoutedEventArgs e) {
            switch (_parameterStatus) {
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
            if (!this.GetControl<CheckBox> ("checkDefaultVitals").IsChecked ?? false || Instance?.Physiology == null)
                return;

            int si = this.GetControl<ComboBox> ("comboCardiacRhythm").SelectedIndex;
            Array ev = Enum.GetValues (typeof (CardiacRhythms.Values));
            if (si < 0 || si > ev.Length - 1)
                return;

            CardiacRhythms.DefaultVitals v = CardiacRhythms.Defaults (
                (CardiacRhythms.Values)(ev.GetValue (si) ?? CardiacRhythms.Values.SinusRhythm));

            this.GetControl<NumericUpDown> ("numHR").Value = (int)II.Math.Clamp ((double)(this.GetControl<NumericUpDown> ("numHR")?.Value ?? 0), v.HRMin, v.HRMax);
            this.GetControl<NumericUpDown> ("numNSBP").Value = (int)II.Math.Clamp ((double)(this.GetControl<NumericUpDown> ("numNSBP")?.Value ?? 0), v.SBPMin, v.SBPMax);
            this.GetControl<NumericUpDown> ("numNDBP").Value = (int)II.Math.Clamp ((double)(this.GetControl<NumericUpDown> ("numNDBP")?.Value ?? 0), v.DBPMin, v.DBPMax);
            this.GetControl<NumericUpDown> ("numRR").Value = (int)II.Math.Clamp ((double)(this.GetControl<NumericUpDown> ("numRR")?.Value ?? 0), v.RRMin, v.RRMax);
            this.GetControl<NumericUpDown> ("numSPO2").Value = (int)II.Math.Clamp ((double)(this.GetControl<NumericUpDown> ("numSPO2")?.Value ?? 0), v.SPO2Min, v.SPO2Max);
            this.GetControl<NumericUpDown> ("numETCO2").Value = (int)II.Math.Clamp ((double)(this.GetControl<NumericUpDown> ("numETCO2")?.Value ?? 0), v.ETCO2Min, v.ETCO2Max);
            this.GetControl<NumericUpDown> ("numASBP").Value = (int)II.Math.Clamp ((double)(this.GetControl<NumericUpDown> ("numASBP")?.Value ?? 0), v.SBPMin, v.SBPMax);
            this.GetControl<NumericUpDown> ("numADBP").Value = (int)II.Math.Clamp ((double)(this.GetControl<NumericUpDown> ("numADBP")?.Value ?? 0), v.DBPMin, v.DBPMax);
            this.GetControl<NumericUpDown> ("numPSP").Value = (int)II.Math.Clamp ((double)(this.GetControl<NumericUpDown> ("numPSP")?.Value ?? 0), v.PSPMin, v.PSPMax);
            this.GetControl<NumericUpDown> ("numPDP").Value = (int)II.Math.Clamp ((double)(this.GetControl<NumericUpDown> ("numPDP")?.Value ?? 0), v.PDPMin, v.PDPMax);
            this.GetControl<NumericUpDown> ("numQRSInterval").Value = (decimal)II.Math.Clamp ((double)(this.GetControl<NumericUpDown> ("numQRSInterval")?.Value ?? 0), v.QRSIntervalMin, v.QRSIntervalMax);
            this.GetControl<NumericUpDown> ("numQTcInterval").Value = (decimal)II.Math.Clamp ((double)(this.GetControl<NumericUpDown> ("numQTcInterval")?.Value ?? 0), v.QTCIntervalMin, v.QTCIntervalMax);

            OnUIPhysiologyParameter_Process (sender, e);
        }

        private void OnRespiratoryRhythm_Selected (object sender, SelectionChangedEventArgs e) {
            OnUIPhysiologyParameter_Process (sender, e);
        }

        private void OnCentralVenousPressure_Changed (object sender, NumericUpDownValueChangedEventArgs e) {
            if (Instance?.Physiology is not null
                && Instance.Physiology.PulmonaryArteryPlacement.Value == PulmonaryArteryRhythms.Values.RightAtrium) {
                NumericUpDown numPSP = this.GetControl<NumericUpDown> ("numPSP");
                NumericUpDown numPDP = this.GetControl<NumericUpDown> ("numPDP");

                numPSP.Value = (int)System.Math.Ceiling ((e.NewValue ?? 1) + System.Math.Max (1, (e.NewValue ?? 1) * 0.25m));
                numPDP.Value = (int)System.Math.Floor ((e.NewValue ?? 1) - System.Math.Max (1, (e.NewValue ?? 1) * 0.25m));
            }

            OnUIPhysiologyParameter_Process (sender, e);
        }

        private void OnPulmonaryArteryRhythm_Selected (object sender, SelectionChangedEventArgs e) {
            if (Instance?.Physiology is null)
                return;

            int si = this.GetControl<ComboBox> ("comboPACatheterPlacement").SelectedIndex;
            Array ev = Enum.GetValues (typeof (PulmonaryArteryRhythms.Values));
            if (si < 0 || si > ev.Length - 1)
                return;

            PulmonaryArteryRhythms.Values sel = (PulmonaryArteryRhythms.Values)(ev.GetValue (si) ?? PulmonaryArteryRhythms.Values.PulmonaryArtery);

            NumericUpDown numPSP = this.GetControl<NumericUpDown> ("numPSP");
            NumericUpDown numPDP = this.GetControl<NumericUpDown> ("numPDP");

            // If the PA placement is RV, PA, or PAWP, utilize default vital sign ranges
            // But if it is in the RA... utilize the current CVP reading
            if (sel != PulmonaryArteryRhythms.Values.RightAtrium) {
                PulmonaryArteryRhythms.Default_Vitals v = PulmonaryArteryRhythms.DefaultVitals (sel);
                numPSP.Value = (int)II.Math.Clamp ((double)(numPSP?.Value ?? 0), v.PSPMin, v.PSPMax);
                numPDP.Value = (int)II.Math.Clamp ((double)(numPDP?.Value ?? 0), v.PDPMin, v.PDPMax);
            } else if (sel == PulmonaryArteryRhythms.Values.RightAtrium) {
                numPSP.Value = (int)System.Math.Ceiling (Instance.Physiology.CVP + System.Math.Max (1, Instance.Physiology.CVP * 0.25));
                numPDP.Value = (int)System.Math.Floor (Instance.Physiology.CVP - System.Math.Max (1, Instance.Physiology.CVP * 0.25));
            }

            // Disable the PA pressure input if the catheter is in the RA (and pressures are all based on CVP!)
            numPSP.IsEnabled = sel != PulmonaryArteryRhythms.Values.RightAtrium;
            numPDP.IsEnabled = sel != PulmonaryArteryRhythms.Values.RightAtrium;

            OnUIPhysiologyParameter_Process (sender, e);
        }

        private void OnPhysiologyEvent (object? sender, Physiology.PhysiologyEventArgs e) {
            if (e.Physiology is null)
                return;

            switch (e.EventType) {
                default:
                    break;

                case Physiology.PhysiologyEventTypes.CardiacBaseline:
                    ApplyPhysiologyParameters_Cardiac (sender, new EventArgs ());
                    break;

                case Physiology.PhysiologyEventTypes.RespiratoryBaseline:
                    ApplyPhysiologyParameters_Respiratory (sender, new EventArgs ());
                    break;

                case Physiology.PhysiologyEventTypes.ObstetricBaseline:
                    ApplyPhysiologyParameters_Obstetric (sender, new EventArgs ());
                    break;

                case Physiology.PhysiologyEventTypes.VitalsChange:
                    UpdateView (e.Physiology);
                    break;
            }
        }
    }
}