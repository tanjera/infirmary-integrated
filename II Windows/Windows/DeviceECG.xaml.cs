﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using II;
using II.Rhythm;
using II.Waveform;

namespace II_Windows {

    /// <summary>
    /// Interaction logic for DeviceECG.xaml
    /// </summary>
    public partial class DeviceECG : Window {

        private bool isFullscreen = false,
             isPaused = false;

        private List<Controls.ECGTracing> listTracings = new List<Controls.ECGTracing> ();

        private Timer timerTracing = new Timer ();
        private bool showGrid = false;
        private ColorSchemes colorScheme = ColorSchemes.Light;
        private ImageBrush gridBackground;

        // Define WPF UI commands for binding
        private ICommand icToggleFullscreen, icPauseDevice, icCloseDevice, icExitProgram;

        public ICommand IC_ToggleFullscreen { get { return icToggleFullscreen; } }
        public ICommand IC_PauseDevice { get { return icPauseDevice; } }
        public ICommand IC_CloseDevice { get { return icCloseDevice; } }
        public ICommand IC_ExitProgram { get { return icExitProgram; } }

        public enum ColorSchemes {
            Dark,
            Light
        }

        public DeviceECG () {
            InitializeComponent ();
            DataContext = this;

            InitTimers ();
            InitInterface ();
            SetColorScheme (colorScheme);
        }

        private void InitTimers () {
            timerTracing.Set (Draw.RefreshTime);
            App.Timer_Main.Tick += timerTracing.Process;
            timerTracing.Tick += OnTick_Tracing;
            timerTracing.Start ();
        }

        private void InitInterface () {
            /* Initiate ICommands for KeyBindings */
            icToggleFullscreen = new ActionCommand (() => ToggleFullscreen ());
            icPauseDevice = new ActionCommand (() => TogglePause ());
            icCloseDevice = new ActionCommand (() => this.Close ());
            icExitProgram = new ActionCommand (() => App.Patient_Editor.Exit ());

            /* Populate UI strings per language selection */
            wdwDeviceECG.Title = App.Language.Dictionary ["ECG:WindowTitle"];
            menuDevice.Header = App.Language.Dictionary ["MENU:MenuDeviceOptions"];
            menuPauseDevice.Header = App.Language.Dictionary ["MENU:MenuPauseDevice"];
            menuShowGrid.Header = App.Language.Dictionary ["MENU:MenuShowGrid"];
            menuToggleFullscreen.Header = App.Language.Dictionary ["MENU:MenuToggleFullscreen"];
            menuCloseDevice.Header = App.Language.Dictionary ["MENU:MenuCloseDevice"];
            menuExitProgram.Header = App.Language.Dictionary ["MENU:MenuExitProgram"];
            menuColor.Header = App.Language.Dictionary ["MENU:MenuColorScheme"];
            menuColorLight.Header = App.Language.Dictionary ["MENU:MenuColorSchemeLight"];
            menuColorDark.Header = App.Language.Dictionary ["MENU:MenuColorSchemeDark"];

            /* Set background image for grid lines */
            gridBackground = new ImageBrush (new BitmapImage (
                new Uri ("pack://application:,,,/Resources/12L ECG Grid.png")));

            /* 12 Lead ECG Interface layout */
            List<Lead.Values> listLeads = new List<Lead.Values> ();
            foreach (Lead.Values v in Enum.GetValues (typeof (Lead.Values))) {
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
                    listTracings.Add (new Controls.ECGTracing (new Strip (listLeads [indexLeads], (4 - iColumns) * 2.5d, 2.5d)));
                    listTracings [indexLeads].SetValue (Grid.ColumnProperty, iColumns);
                    listTracings [indexLeads].SetValue (Grid.RowProperty, iRows);
                    layoutGrid.Children.Add (listTracings [indexLeads]);
                    indexLeads++;
                }
            }

            // Add Lead II running along bottom spanning all columns
            Controls.ECGTracing leadII = new Controls.ECGTracing (new Strip (Lead.Values.ECG_II, 10d));
            leadII.SetValue (Grid.ColumnProperty, 0);
            leadII.SetValue (Grid.RowProperty, 4);
            leadII.SetValue (Grid.ColumnSpanProperty, 4);
            listTracings.Add (leadII);
            layoutGrid.RowDefinitions.Add (new RowDefinition ());
            layoutGrid.Children.Add (listTracings [indexLeads]);
        }

        private void UpdateInterface () {
            for (int i = 0; i < listTracings.Count; i++)
                listTracings [i].SetColors (colorScheme, showGrid);

            switch (colorScheme) {
                default: break;

                case ColorSchemes.Light:
                    menuColorLight.IsChecked = true;
                    menuColorDark.IsChecked = false;
                    layoutGrid.Background = Brushes.White;
                    break;

                case ColorSchemes.Dark:
                    menuColorDark.IsChecked = true;
                    menuColorLight.IsChecked = false;
                    layoutGrid.Background = Brushes.Black;
                    break;
            }

            if (showGrid)
                layoutGrid.Background = gridBackground;
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
            } catch {
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

        private void SetColorScheme (ColorSchemes scheme) {
            colorScheme = scheme;

            UpdateInterface ();
        }

        private void ToggleGrid () {
            showGrid = !showGrid;
            menuShowGrid.IsChecked = showGrid;

            UpdateInterface ();
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
                listTracings.ForEach (c => c.Strip.Unpause ());
        }

        private void ToggleFullscreen () {
            isFullscreen = !isFullscreen;
            ApplyFullScreen ();
        }

        private void MenuClose_Click (object s, RoutedEventArgs e)
            => this.Close ();

        private void MenuExit_Click (object s, RoutedEventArgs e)
            => App.Patient_Editor.Exit ();

        private void MenuTogglePause_Click (object s, RoutedEventArgs e)
            => TogglePause ();

        private void MenuShowGrid_Click (object sender, RoutedEventArgs e)
            => ToggleGrid ();

        private void MenuFullscreen_Click (object sender, RoutedEventArgs e)
            => ToggleFullscreen ();

        private void MenuColorScheme_Light (object sender, RoutedEventArgs e)
            => SetColorScheme (ColorSchemes.Light);

        private void MenuColorScheme_Dark (object sender, RoutedEventArgs e)
            => SetColorScheme (ColorSchemes.Dark);

        private void OnTick_Tracing (object sender, EventArgs e) {
            if (isPaused)
                return;

            for (int i = 0; i < listTracings.Count; i++) {
                listTracings [i].Strip.Scroll ();
                listTracings [i].DrawTracing ();
            }
        }

        public void OnPatientEvent (object sender, Patient.PatientEventArgs e) {
            switch (e.EventType) {
                default: break;
                case Patient.PatientEventTypes.Vitals_Change:
                    listTracings.ForEach (c => {
                        c.Strip.ClearFuture (App.Patient);
                        c.Strip.Add_Beat__Cardiac_Baseline (App.Patient);
                    });
                    break;

                case Patient.PatientEventTypes.Defibrillation:
                    listTracings.ForEach (c => c.Strip.Add_Beat__Cardiac_Defibrillation (App.Patient));
                    break;

                case Patient.PatientEventTypes.Pacermaker_Spike:
                    listTracings.ForEach (c => c.Strip.Add_Beat__Cardiac_Pacemaker (App.Patient));
                    break;

                case Patient.PatientEventTypes.Cardiac_Baseline:
                    listTracings.ForEach (c => c.Strip.Add_Beat__Cardiac_Baseline (App.Patient));
                    break;

                case Patient.PatientEventTypes.Cardiac_Atrial_Electric:
                    listTracings.ForEach (c => c.Strip.Add_Beat__Cardiac_Atrial_Electrical (App.Patient));
                    break;

                case Patient.PatientEventTypes.Cardiac_Ventricular_Electric:
                    listTracings.ForEach (c => c.Strip.Add_Beat__Cardiac_Ventricular_Electrical (App.Patient));
                    break;

                case Patient.PatientEventTypes.Cardiac_Atrial_Mechanical:
                    listTracings.ForEach (c => c.Strip.Add_Beat__Cardiac_Atrial_Mechanical (App.Patient));
                    break;

                case Patient.PatientEventTypes.Cardiac_Ventricular_Mechanical:
                    listTracings.ForEach (c => c.Strip.Add_Beat__Cardiac_Ventricular_Mechanical (App.Patient));
                    break;
            }
        }
    }
}