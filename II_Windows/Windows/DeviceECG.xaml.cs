using II;
using II.Rhythm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace II_Windows {

    /// <summary>
    /// Interaction logic for DeviceECG.xaml
    /// </summary>
    public partial class DeviceECG : Window {

        private bool isFullscreen = false,
             isPaused = false;

        private List<Controls.ECGTracing> listTracings = new List<Controls.ECGTracing> ();

        private Timer timerTracing = new Timer ();

        // Define WPF UI commands for binding
        private ICommand icToggleFullscreen, icPauseDevice, icCloseDevice, icExitProgram;

        public ICommand IC_ToggleFullscreen { get { return icToggleFullscreen; } }
        public ICommand IC_PauseDevice { get { return icPauseDevice; } }
        public ICommand IC_CloseDevice { get { return icCloseDevice; } }
        public ICommand IC_ExitProgram { get { return icExitProgram; } }

        public DeviceECG () {
            InitializeComponent ();
            DataContext = this;

            InitTimers ();
            InitInterface ();
        }

        private void InitTimers () {
            timerTracing.Interval = Waveforms.Draw_Refresh;
            App.Timer_Main.Tick += timerTracing.Process;
            timerTracing.Tick += OnTick_Tracing;
            timerTracing.Start ();
        }

        private void InitInterface () {
            // Initiate ICommands for KeyBindings
            icToggleFullscreen = new ActionCommand (() => ToggleFullscreen ());
            icPauseDevice = new ActionCommand (() => TogglePause ());
            icCloseDevice = new ActionCommand (() => this.Close ());
            icExitProgram = new ActionCommand (() => App.Patient_Editor.RequestExit ());

            // Populate UI strings per language selection
            wdwDeviceECG.Title = App.Language.Dictionary ["ECG:WindowTitle"];
            menuDevice.Header = App.Language.Dictionary ["MENU:MenuDeviceOptions"];
            menuPauseDevice.Header = App.Language.Dictionary ["MENU:MenuPauseDevice"];
            menuToggleFullscreen.Header = App.Language.Dictionary ["MENU:MenuToggleFullscreen"];
            menuCloseDevice.Header = App.Language.Dictionary ["MENU:MenuCloseDevice"];
            menuExitProgram.Header = App.Language.Dictionary ["MENU:MenuExitProgram"];

            /* 12 Lead ECG Interface layout */
            List<Leads.Values> listLeads = new List<Leads.Values> ();
            foreach (Leads.Values v in Enum.GetValues (typeof (Leads.Values))) {
                if (v.ToString ().StartsWith ("ECG"))
                    listLeads.Add (v);
            }

            int amtRows = 3,
                amtColumns = 4,
                indexLeads = 0;

            // Set grid's row and column definitions
            for (int i = 0; i < amtRows; i++)
                layoutGrid.RowDefinitions.Add (new RowDefinition ());
            for (int i = 0; i < amtColumns; i++)
                layoutGrid.ColumnDefinitions.Add (new ColumnDefinition ());

            // Populate the grid with tracings for each lead
            for (int iColumns = 0; iColumns < amtColumns; iColumns++) {
                for (int iRows = 0; iRows < amtRows && indexLeads < listLeads.Count; iRows++) {
                    listTracings.Add (new Controls.ECGTracing (new Strip (3f, listLeads [indexLeads])));
                    listTracings [indexLeads].SetValue (Grid.ColumnProperty, iColumns);
                    listTracings [indexLeads].SetValue (Grid.RowProperty, iRows);
                    layoutGrid.Children.Add (listTracings [indexLeads]);
                    indexLeads++;
                }
            }

            // Add Lead II running along bottom spanning all columns
            Controls.ECGTracing leadII = new Controls.ECGTracing (new Strip (12f, Leads.Values.ECG_II));
            leadII.SetValue (Grid.ColumnProperty, 0);
            leadII.SetValue (Grid.RowProperty, 4);
            leadII.SetValue (Grid.ColumnSpanProperty, 4);
            listTracings.Add (leadII);
            layoutGrid.RowDefinitions.Add (new RowDefinition ());
            layoutGrid.Children.Add (listTracings [indexLeads]);
        }

        public void Load_Process (string inc) {
            StringReader sRead = new StringReader (inc);

            try {
                string line;
                while ((line = sRead.ReadLine ()) != null) {
                    if (line.Contains (":")) {
                        string pName = line.Substring (0, line.IndexOf (':')),
                                pValue = line.Substring (line.IndexOf (':') + 1);
                        switch (pName) {
                            default: break;
                            case "isPaused": isPaused = bool.Parse (pValue); break;
                            case "isFullscreen": isFullscreen = bool.Parse (pValue); break;
                        }
                    }
                }
            } catch (Exception e) {
                App.Server.Post_Exception (e);
                throw e;
            } finally {
                sRead.Close ();
            }
        }

        public string Save () {
            StringBuilder sWrite = new StringBuilder ();

            sWrite.AppendLine (String.Format ("{0}:{1}", "isPaused", isPaused));
            sWrite.AppendLine (String.Format ("{0}:{1}", "isFullscreen", isFullscreen));

            return sWrite.ToString ();
        }

        private void ApplyFullScreen () {
            menuToggleFullscreen.IsChecked = isFullscreen;

            switch (isFullscreen) {
                default:
                case false:
                    wdwDeviceECG.WindowStyle = WindowStyle.SingleBorderWindow;
                    wdwDeviceECG.WindowState = WindowState.Normal;
                    wdwDeviceECG.ResizeMode = ResizeMode.CanResize;
                    break;

                case true:
                    wdwDeviceECG.WindowStyle = WindowStyle.None;
                    wdwDeviceECG.WindowState = WindowState.Maximized;
                    wdwDeviceECG.ResizeMode = ResizeMode.NoResize;
                    break;
            }
        }

        private void TogglePause () {
            isPaused = !isPaused;
            menuPauseDevice.IsChecked = isPaused;

            if (!isPaused)
                listTracings.ForEach (c => c.wfStrip.Unpause ());
        }

        private void ToggleFullscreen () {
            isFullscreen = !isFullscreen;
            ApplyFullScreen ();
        }

        private void MenuClose_Click (object s, RoutedEventArgs e) => this.Close ();

        private void MenuExit_Click (object s, RoutedEventArgs e) => App.Patient_Editor.RequestExit ();

        private void MenuTogglePause_Click (object s, RoutedEventArgs e) => TogglePause ();

        private void MenuFullscreen_Click (object sender, RoutedEventArgs e) => ToggleFullscreen ();

        private void OnTick_Tracing (object sender, EventArgs e) {
            if (isPaused)
                return;

            listTracings.ForEach (c => {
                c.wfStrip.Scroll ();
                c.Draw ();
            });
        }

        public void OnPatientEvent (object sender, Patient.PatientEvent_Args e) {
            switch (e.EventType) {
                default: break;
                case Patient.PatientEvent_Args.EventTypes.Vitals_Change:
                    listTracings.ForEach (c => {
                        c.wfStrip.ClearFuture (App.Patient);
                        c.wfStrip.Add_Beat__Cardiac_Baseline (App.Patient);
                    });
                    break;

                case Patient.PatientEvent_Args.EventTypes.Cardiac_Defibrillation:
                    listTracings.ForEach (c => c.wfStrip.Add_Beat__Cardiac_Defibrillation (App.Patient));
                    break;

                case Patient.PatientEvent_Args.EventTypes.Cardiac_PacerSpike:
                    listTracings.ForEach (c => c.wfStrip.Add_Beat__Cardiac_Pacemaker (App.Patient));
                    break;

                case Patient.PatientEvent_Args.EventTypes.Cardiac_Baseline:
                    listTracings.ForEach (c => c.wfStrip.Add_Beat__Cardiac_Baseline (App.Patient));
                    break;

                case Patient.PatientEvent_Args.EventTypes.Cardiac_Atrial:
                    listTracings.ForEach (c => c.wfStrip.Add_Beat__Cardiac_Atrial (App.Patient));
                    break;

                case Patient.PatientEvent_Args.EventTypes.Cardiac_Ventricular:
                    listTracings.ForEach (c => c.wfStrip.Add_Beat__Cardiac_Ventricular (App.Patient));
                    break;
            }
        }
    }
}