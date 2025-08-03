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

using II;
using II.Localization;
using II.Server;

using IISIM;

using Xceed.Wpf.Toolkit;
using Xceed.Wpf.Toolkit.Primitives;

namespace IISIM.Windows {

    /// <summary>
    /// Interaction logic for Control.xaml
    /// </summary>
    public partial class Control : Window {
        public App? Instance;

        private bool HideDeviceLabels = false;

        /* Buffers for ViewModel handling and temporal smoothing of upstream Model data changes */
        private Physiology? ApplyBuffer;

        private bool ApplyPending_Cardiac = false,
                     ApplyPending_Respiratory = false,
                     ApplyPending_Obstetric = false;

        private II.Timer ApplyTimer_Cardiac = new (),
                      ApplyTimer_Respiratory = new (),
                      ApplyTimer_Obstetric = new ();

        /* Variables for UI loading */
        private bool IsUILoadCompleted = false;

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
            if (!II.Settings.Simulator.Exists ()) {
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

            lblDeviceMonitor.Content = Instance.Language.Localize ("PE:CardiacMonitor");
            lblDevice12LeadECG.Content = Instance.Language.Localize ("PE:12LeadECG");
            lblDeviceDefibrillator.Content = Instance.Language.Localize ("PE:Defibrillator");
            lblDeviceIABP.Content = Instance.Language.Localize ("PE:IABP");
            lblDeviceEFM.Content = Instance.Language.Localize ("PE:EFM");

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

            chkAutoApplyChanges.Content = Instance.Language.Localize ("BUTTON:AutoApplyChanges");
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

            Task.Run (async () => {
                await Instance.Mirror.timerUpdate.ResetStart (5000);
                await UpdateMirrorStatus ();
            });
        }

        private void InitScenario (bool toInit) {
            // TODO: Implement
        }

        private async Task UnloadScenario () {
            // TODO: Implement
        }

        private void NewScenario () => _ = RefreshScenario (true);

        private async Task RefreshScenario (bool toInit) {
            // TODO: Implement
        }

        private void InitTimers () {
            // TODO: Implement
        }

        private void InitScenarioStep () {
            // TODO: Implement
        }

        private void InitPhysiologyEvents () {
            // TODO: Implement
        }

        private async Task UnloadPatientEvents () {
            // TODO: Implement
        }

        private async Task InitDeviceMonitor () {
            // TODO: Implement
        }

        private async Task InitDeviceECG () {
            // TODO: Implement
        }

        private async Task InitDeviceDefib () {
            // TODO: Implement
        }

        private async Task InitDeviceIABP () {
            // TODO: Implement
        }

        private async Task InitDeviceEFM () {
            // TODO: Implement
        }

        private async Task MessageAudioUnavailable () {
            // TODO: Implement
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
            // TODO: Implement
        }

        private async Task DialogMirrorReceive () {
            // TODO: Implement
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
            // TODO: Implement
        }

        public void SetAudio_On () => _ = SetAudio (true);

        public void SetAudio_Off () => _ = SetAudio (false);

        public async Task SetAudio (bool toSet) {
            // TODO: Implement
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
                    DialogInformation dlg = new (Instance) {
                        Title = Instance.Language.Localize ("UPGRADE:Upgrade"),
                        Message = Instance.Language.Localize ("UPGRADE:NoUpdateAvailable"),
                        Button = Instance.Language.Localize ("BUTTON:Continue")
                    };
                    dlg.Activate ();
                    dlg.ShowDialog ();
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
            // TODO: Implement
        }

        private async Task SetParameterStatus (bool autoApplyChanges) {
            // TODO: Implement
        }

        private async Task AdvanceParameterStatus (ParameterStatuses status) {
            // TODO: Implement
        }

        private async Task UpdateParameterIndicators () {
            // TODO: Implement
        }

        private async Task LoadFile () {
            // TODO: Implement
        }

        private async Task LoadOpen (string fileName) {
            // TODO: Implement
        }

        private async Task LoadInit (string incFile) {
            // TODO: Implement
        }

        private async Task LoadValidateT1 (string data) {
            // TODO: Implement
        }

        private async Task LoadProcess (string incFile) {
            // TODO: Implement
        }

        private async Task LoadOptions (string inc) {
            // TODO: Implement
        }

        private async Task LoadFail () {
            // TODO: Implement
        }

        private async Task SaveFile () {
            // TODO: Implement
        }

        private async Task SaveT1 (string filename) {
            // TODO: Implement
        }

        private string SaveOptions () {
            // TODO: Implement
            return "";
        }

        public async Task Exit () {
            // TODO: Implement
        }

        private void OnMirrorTick (object? sender, EventArgs e) {
            // TODO: Implement
        }

        private void OnStepChangeRequest (object? sender, EventArgs e)
            => _ = UnloadPatientEvents ();

        private void OnStepChanged (object? sender, EventArgs e) {
            // TODO: Implement
        }

        private void InitStep () {
            // TODO: Implement
        }

        private async Task NextStep () {
            // TODO: Implement
        }

        private async Task PreviousStep () {
            // TODO: Implement
        }

        private async Task PauseStep () {
            // TODO: Implement
        }

        private async Task PlayStep () {
            // TODO: Implement
        }

        private async Task ResetPhysiologyParameters () {
            // TODO: Implement
        }

        private async Task ApplyPhysiologyParameters () {
            // TODO: Implement
        }

        private async Task ApplyPhysiologyParameters_Buffer (Physiology? p) {
            // TODO: Implement
        }

        private void ApplyPhysiologyParameters_Cardiac (object? sender, EventArgs e) {
            // TODO: Implement
        }

        private void ApplyPhysiologyParameters_Respiratory (object? sender, EventArgs e) {
            // TODO: Implement
        }

        private void ApplyPhysiologyParameters_Obstetric (object? sender, EventArgs e) {
            // TODO: Implement
        }

        private void UpdateView (Physiology? p) {
            // TODO: Implement
        }

        public void ToggleFullscreen () {
            // TODO: Implement
        }

        private void MenuNewSimulation_Click (object sender, RoutedEventArgs e)
            => _ = RefreshScenario (true);

        private void MenuLoadFile_Click (object s, RoutedEventArgs e)
            => _ = LoadFile ();

        private void MenuSaveFile_Click (object s, RoutedEventArgs e)
            => _ = SaveFile ();

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

        private void OnActivated (object sender, EventArgs e) {
            // TODO: Implement
        }

        private void OnClosed (object sender, EventArgs e)
            => _ = Exit ();

        private void OnLayoutUpdated (object sender, EventArgs e) {
            // TODO: Implement
        }

        private void OnAutoApplyChanges_Changed (object sender, RoutedEventArgs e) {
            // TODO: Implement
        }

        private void OnUIPhysiologyParameter_KeyDown (object sender, KeyEventArgs e) {
            // TODO: Implement
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
            // TODO: Implement
        }

        private void OnCardiacRhythm_Selected (object sender, SelectionChangedEventArgs e) {
            // TODO: Implement
        }

        private void OnRespiratoryRhythm_Selected (object sender, SelectionChangedEventArgs e) {
            // TODO: Implement
        }

        private void OnCentralVenousPressure_Changed (object sender, RoutedPropertyChangedEventArgs<object> e) {
            // TODO: Implement
        }

        private void OnPulmonaryArteryRhythm_Selected (object sender, SelectionChangedEventArgs e) {
            // TODO: Implement
        }

        private void OnPhysiologyEvent (object? sender, Physiology.PhysiologyEventArgs e) {
            // TODO: Implement
        }
    }
}