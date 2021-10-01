using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

using II;
using II.Rhythm;
using II.Waveform;

namespace II_Avalonia {

    public partial class DeviceECG : Window {
        private bool isPaused = false;

        private List<Controls.ECGTracing> listTracings = new List<Controls.ECGTracing> ();

        private Timer timerTracing = new Timer ();
        private ColorSchemes colorScheme = ColorSchemes.Dark;
        private ImageBrush gridBackground;

        public enum ColorSchemes {
            Grid,
            Dark,
            Light
        }

        public DeviceECG () {
            InitializeComponent ();
#if DEBUG
            this.AttachDevTools ();
#endif

            DataContext = this;

            InitTimers ();
            InitInterface ();
            SetColorScheme (colorScheme);
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        ~DeviceECG () => Dispose ();

        public void Dispose () {
            /* Clean subscriptions from the Main Timer */
            App.Timer_Main.Elapsed -= timerTracing.Process;

            /* Dispose of local Timers */
            timerTracing.Dispose ();

            /* Unsubscribe from the main Patient event listing */
            App.Patient.PatientEvent -= OnPatientEvent;
        }

        private void InitTimers () {
            timerTracing.Set (Draw.RefreshTime);
            App.Timer_Main.Elapsed += timerTracing.Process;
            timerTracing.Tick += OnTick_Tracing;
            timerTracing.Start ();
        }

        private void InitInterface () {
            Grid layoutGrid = this.FindControl<Grid> ("layoutGrid");

            /* Populate UI strings per language selection */
            this.FindControl<Window> ("wdwDeviceECG").Title = App.Language.Localize ("ECG:WindowTitle");
            this.FindControl<MenuItem> ("menuDevice").Header = App.Language.Localize ("MENU:MenuDeviceOptions");
            this.FindControl<MenuItem> ("menuPauseDevice").Header = App.Language.Localize ("MENU:MenuPauseDevice");
            this.FindControl<MenuItem> ("menuShowGrid").Header = App.Language.Localize ("MENU:MenuShowGrid");
            this.FindControl<MenuItem> ("menuCloseDevice").Header = App.Language.Localize ("MENU:MenuCloseDevice");
            this.FindControl<MenuItem> ("menuColor").Header = App.Language.Localize ("MENU:MenuColorScheme");
            this.FindControl<MenuItem> ("menuColorLight").Header = App.Language.Localize ("MENU:MenuColorSchemeLight");
            this.FindControl<MenuItem> ("menuColorDark").Header = App.Language.Localize ("MENU:MenuColorSchemeDark");

            /* Set background image for grid lines */
            var assets = AvaloniaLocator.Current.GetService<Avalonia.Platform.IAssetLoader> ();
            gridBackground = new ImageBrush (new Bitmap (assets.Open (new Uri ("avares://Infirmary Integrated/Resources/12L_ECG_Grid.png"))));
            gridBackground.Stretch = Stretch.Fill;

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
                    listTracings.Add (new Controls.ECGTracing (new Strip (listLeads [indexLeads], (4 - iColumns) * 2.5f, 2.5f)));
                    listTracings [indexLeads].SetValue (Grid.ColumnProperty, iColumns);
                    listTracings [indexLeads].SetValue (Grid.RowProperty, iRows);
                    layoutGrid.Children.Add (listTracings [indexLeads]);
                    indexLeads++;
                }
            }

            // Add Lead II running along bottom spanning all columns
            Controls.ECGTracing leadII = new Controls.ECGTracing (new Strip (Lead.Values.ECG_II, 10f));
            leadII.SetValue (Grid.ColumnProperty, 0);
            leadII.SetValue (Grid.RowProperty, 4);
            leadII.SetValue (Grid.ColumnSpanProperty, 4);
            listTracings.Add (leadII);
            layoutGrid.RowDefinitions.Add (new RowDefinition ());
            layoutGrid.Children.Add (listTracings [indexLeads]);
        }

        private void UpdateInterface () {
            for (int i = 0; i < listTracings.Count; i++)
                listTracings [i].SetColors (colorScheme);

            Window wdwDeviceECG = this.FindControl<Window> ("wdwDeviceECG");

            switch (colorScheme) {
                default:
                case ColorSchemes.Grid:
                    wdwDeviceECG.Background = gridBackground;
                    break;

                case ColorSchemes.Light:
                    wdwDeviceECG.Background = Brushes.White;
                    break;

                case ColorSchemes.Dark:
                    wdwDeviceECG.Background = Brushes.Black;
                    break;
            }
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
                            case "colorScheme": colorScheme = (ColorSchemes)Enum.Parse (typeof (ColorSchemes), pValue); break;
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
            sWrite.AppendLine (String.Format ("{0}:{1}", "colorScheme", colorScheme));

            return sWrite.ToString ();
        }

        private void SetColorScheme (ColorSchemes scheme) {
            colorScheme = scheme;

            UpdateInterface ();
        }

        private void TogglePause () {
            isPaused = !isPaused;

            if (!isPaused)
                listTracings.ForEach (c => c.Strip.Unpause ());
        }

        private void MenuClose_Click (object s, RoutedEventArgs e)
            => this.Close ();

        private void MenuTogglePause_Click (object s, RoutedEventArgs e)
            => TogglePause ();

        private void MenuShowGrid_Click (object sender, RoutedEventArgs e)
            => SetColorScheme (ColorSchemes.Grid);

        private void MenuColorScheme_Light (object sender, RoutedEventArgs e)
            => SetColorScheme (ColorSchemes.Light);

        private void MenuColorScheme_Dark (object sender, RoutedEventArgs e)
            => SetColorScheme (ColorSchemes.Dark);

        private void OnClosed (object sender, EventArgs e)
            => this.Dispose ();

        private void OnTick_Tracing (object sender, EventArgs e) {
            if (isPaused)
                return;

            for (int i = 0; i < listTracings.Count; i++) {
                listTracings [i].Strip.Scroll ();
                Dispatcher.UIThread.InvokeAsync (listTracings [i].DrawTracing);
            }
        }

        public void OnPatientEvent (object? sender, Patient.PatientEventArgs e) {
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