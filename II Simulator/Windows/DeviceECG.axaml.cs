using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;

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

namespace IISIM {

    public partial class DeviceECG : DeviceWindow {
        private Color.Schemes colorScheme = Color.Schemes.Dark;
        private ImageBrush? gridBackground;

        private List<Controls.ECGTracing> listTracings = new ();

        public DeviceECG () {
            InitializeComponent ();
        }

        public DeviceECG (App? app) : base (app) {
            InitializeComponent ();
#if DEBUG
            this.AttachDevTools ();
#endif

            DataContext = this;

            InitInterface ();
            SetColorScheme (colorScheme);
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        private void InitInterface () {
            Grid layoutGrid = this.FindControl<Grid> ("layoutGrid");

            /* Populate UI strings per language selection */
            if (Instance?.Language is not null) {
                this.FindControl<Window> ("wdwDeviceECG").Title = Instance.Language.Localize ("ECG:WindowTitle");
                this.FindControl<MenuItem> ("menuDevice").Header = Instance.Language.Localize ("MENU:MenuDeviceOptions");
                this.FindControl<MenuItem> ("menuPauseDevice").Header = Instance.Language.Localize ("MENU:MenuPauseDevice");
                this.FindControl<MenuItem> ("menuShowGrid").Header = Instance.Language.Localize ("MENU:MenuShowGrid");
                this.FindControl<MenuItem> ("menuCloseDevice").Header = Instance.Language.Localize ("MENU:MenuCloseDevice");
                this.FindControl<MenuItem> ("menuColor").Header = Instance.Language.Localize ("MENU:MenuColorScheme");
                this.FindControl<MenuItem> ("menuColorLight").Header = Instance.Language.Localize ("MENU:MenuColorSchemeLight");
                this.FindControl<MenuItem> ("menuColorDark").Header = Instance.Language.Localize ("MENU:MenuColorSchemeDark");
            }

            /* Set background image for grid lines */
            var assets = AvaloniaLocator.Current.GetService<Avalonia.Platform.IAssetLoader> ();
            gridBackground = new ImageBrush (new Bitmap (assets.Open (new Uri ("avares://Infirmary Integrated/Resources/12L_ECG_Grid.png"))));
            gridBackground.Stretch = Stretch.Fill;

            /* 12 Lead ECG Interface layout */
            List<Lead.Values> listLeads = new ();
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
                    listTracings.Add (new Controls.ECGTracing (Instance, new Strip (listLeads [indexLeads], (4 - iColumns) * 2.5f, 2.5f), colorScheme));
                    listTracings [indexLeads].SetValue (Grid.ColumnProperty, iColumns);
                    listTracings [indexLeads].SetValue (Grid.RowProperty, iRows);
                    layoutGrid.Children.Add (listTracings [indexLeads]);
                    indexLeads++;
                }
            }

            // Add Lead II running along bottom spanning all columns
            Controls.ECGTracing leadII = new (Instance, new Strip (Lead.Values.ECG_II, 10f), colorScheme);
            leadII.SetValue (Grid.ColumnProperty, 0);
            leadII.SetValue (Grid.RowProperty, 4);
            leadII.SetValue (Grid.ColumnSpanProperty, 4);
            listTracings.Add (leadII);
            layoutGrid.RowDefinitions.Add (new RowDefinition ());
            layoutGrid.Children.Add (listTracings [indexLeads]);
        }

        private void UpdateInterface () {
            Dispatcher.UIThread.InvokeAsync (() => {
                for (int i = 0; i < listTracings.Count; i++)
                    listTracings [i].SetColorScheme (colorScheme);

                Window window = this.FindControl<Window> ("wdwDeviceECG");
                if (colorScheme == Color.Schemes.Grid)
                    window.Background = gridBackground;
                else
                    window.Background = Color.GetBackground (Color.Devices.DeviceECG, colorScheme);
            });
        }

        public async Task Load (string inc) {
            using StringReader sRead = new (inc);

            try {
                string? line;
                while ((line = await sRead.ReadLineAsync ()) != null) {
                    if (line.Contains (':')) {
                        string pName = line.Substring (0, line.IndexOf (':')),
                                pValue = line.Substring (line.IndexOf (':') + 1);
                        switch (pName) {
                            default: break;
                            case "colorScheme": colorScheme = (Color.Schemes)Enum.Parse (typeof (Color.Schemes), pValue); break;
                        }
                    }
                }
            } catch {
            } finally {
                sRead.Close ();
            }
        }

        public string Save () {
            StringBuilder sWrite = new ();

            sWrite.AppendLine (String.Format ("{0}:{1}", "colorScheme", colorScheme));

            return sWrite.ToString ();
        }

        public void SetColorScheme (Color.Schemes scheme) {
            colorScheme = scheme;
            UpdateInterface ();
        }

        public override void TogglePause () {
            base.TogglePause ();

            if (State == States.Running)
                listTracings.ForEach (c => c.Strip?.Unpause ());
        }

        private void MenuClose_Click (object s, RoutedEventArgs e)
            => this.Close ();

        private void MenuTogglePause_Click (object s, RoutedEventArgs e)
            => TogglePause ();

        private void MenuShowGrid_Click (object sender, RoutedEventArgs e)
            => SetColorScheme (Color.Schemes.Grid);

        private void MenuColorScheme_Light (object sender, RoutedEventArgs e)
            => SetColorScheme (Color.Schemes.Light);

        private void MenuColorScheme_Dark (object sender, RoutedEventArgs e)
            => SetColorScheme (Color.Schemes.Dark);

        public override void OnTick_Tracing (object? sender, EventArgs e) {
            if (State != States.Running)
                return;

            for (int i = 0; i < listTracings.Count; i++) {
                listTracings [i].Strip?.Scroll ();
                Dispatcher.UIThread.InvokeAsync (listTracings [i].DrawTracing);
            }
        }

        public override void OnPatientEvent (object? sender, Patient.PatientEventArgs e) {
            switch (e.EventType) {
                default: break;
                case Patient.PatientEventTypes.Vitals_Change:
                    listTracings.ForEach (c => {
                        c.Strip?.ClearFuture (Instance?.Patient);
                        c.Strip?.Add_Beat__Cardiac_Baseline (Instance?.Patient);
                    });

                    break;

                case Patient.PatientEventTypes.Defibrillation:
                    listTracings.ForEach (c => c.Strip?.Add_Beat__Cardiac_Defibrillation (Instance?.Patient));
                    break;

                case Patient.PatientEventTypes.Pacermaker_Spike:
                    listTracings.ForEach (c => c.Strip?.Add_Beat__Cardiac_Pacemaker (Instance?.Patient));
                    break;

                case Patient.PatientEventTypes.Cardiac_Baseline:
                    listTracings.ForEach (c => c.Strip?.Add_Beat__Cardiac_Baseline (Instance?.Patient));
                    break;

                case Patient.PatientEventTypes.Cardiac_Atrial_Electric:
                    listTracings.ForEach (c => c.Strip?.Add_Beat__Cardiac_Atrial_Electrical (Instance?.Patient));
                    break;

                case Patient.PatientEventTypes.Cardiac_Ventricular_Electric:
                    listTracings.ForEach (c => c.Strip?.Add_Beat__Cardiac_Ventricular_Electrical (Instance?.Patient));
                    break;

                case Patient.PatientEventTypes.Cardiac_Atrial_Mechanical:
                    listTracings.ForEach (c => c.Strip?.Add_Beat__Cardiac_Atrial_Mechanical (Instance?.Patient));
                    break;

                case Patient.PatientEventTypes.Cardiac_Ventricular_Mechanical:
                    listTracings.ForEach (c => c.Strip?.Add_Beat__Cardiac_Ventricular_Mechanical (Instance?.Patient));
                    break;
            }
        }
    }
}