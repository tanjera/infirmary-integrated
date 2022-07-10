using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace IISIM {

    public partial class DeviceEFM : DeviceWindow {
        private Color.Schemes colorScheme = Color.Schemes.Light;

        private List<Controls.EFMTracing> listTracings = new ();

        private ImageBrush gridFHR, gridToco;

        public DeviceEFM () {
            InitializeComponent ();
#if DEBUG
            this.AttachDevTools ();
#endif

            DataContext = this;

            InitInterface ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        private void InitInterface () {
            /* Populate UI strings per language selection */
            this.FindControl<Window> ("wdwDeviceEFM").Title = App.Language.Localize ("EFM:WindowTitle");
            this.FindControl<MenuItem> ("menuDevice").Header = App.Language.Localize ("MENU:MenuDeviceOptions");
            this.FindControl<MenuItem> ("menuPauseDevice").Header = App.Language.Localize ("MENU:MenuPauseDevice");
            this.FindControl<MenuItem> ("menuCloseDevice").Header = App.Language.Localize ("MENU:MenuCloseDevice");
            this.FindControl<MenuItem> ("menuLength").Header = App.Language.Localize ("MENU:StripLength");
            this.FindControl<MenuItem> ("menu1Min").Header = App.Language.Localize ("MENU:StripLength_1Min");
            this.FindControl<MenuItem> ("menu10Min").Header = App.Language.Localize ("MENU:StripLength_10Min");
            this.FindControl<MenuItem> ("menuColor").Header = App.Language.Localize ("MENU:MenuColorScheme");
            this.FindControl<MenuItem> ("menuColorLight").Header = App.Language.Localize ("MENU:MenuColorSchemeLight");
            this.FindControl<MenuItem> ("menuColorDark").Header = App.Language.Localize ("MENU:MenuColorSchemeDark");

            Grid displayGrid = this.FindControl<Grid> ("displayGrid");

            // Instantiate and load backgroung images
            var assets = AvaloniaLocator.Current.GetService<Avalonia.Platform.IAssetLoader> ();
            gridFHR = new ImageBrush (new Bitmap (assets.Open (new Uri ("avares://Infirmary Integrated/Resources/FHR_Grid.png"))));
            gridFHR.Stretch = Stretch.Fill;

            gridToco = new ImageBrush (new Bitmap (assets.Open (new Uri ("avares://Infirmary Integrated/Resources/Toco_Grid.png"))));
            gridToco.Stretch = Stretch.Fill;

            // Instantiate and add Tracings to UI
            Controls.EFMTracing fhrTracing = new (new Strip (Lead.Values.FHR, 600d), colorScheme);
            fhrTracing.SetValue (Grid.RowProperty, 0);
            fhrTracing.SetValue (Grid.ColumnProperty, 0);
            fhrTracing.Background = gridFHR;
            listTracings.Add (fhrTracing);
            displayGrid.Children.Add (fhrTracing);

            Controls.EFMTracing tocoTracing = new (new Strip (Lead.Values.TOCO, 600d), colorScheme);
            tocoTracing.SetValue (Grid.RowProperty, 2);
            tocoTracing.SetValue (Grid.ColumnProperty, 0);
            tocoTracing.Background = gridToco;
            listTracings.Add (tocoTracing);
            displayGrid.Children.Add (tocoTracing);
        }

        private void UpdateInterface () {
            Dispatcher.UIThread.InvokeAsync (() => {
                for (int i = 0; i < listTracings.Count; i++)
                    listTracings [i].SetColorScheme (colorScheme);

                Window window = this.FindControl<Window> ("wdwDeviceEFM");
                window.Background = Color.GetBackground (Color.Devices.DeviceEFM, colorScheme);
            });
        }

        public void Load_Process (string inc) {
            using StringReader sRead = new (inc);

            try {
                string line;
                while ((line = sRead.ReadLine ()) != null) {
                    if (line.Contains (":")) {
                        string pName = line.Substring (0, line.IndexOf (':')),
                                pValue = line.Substring (line.IndexOf (':') + 1);
                        switch (pName) {
                            default: break;
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

            return sWrite.ToString ();
        }

        public void SetColorScheme (Color.Schemes scheme) {
            colorScheme = scheme;
            UpdateInterface ();
        }

        private void SetStripLength (int seconds) {
            listTracings.ForEach (c => {
                c.Strip = new Strip (c.Strip.Lead.Value, seconds);
            });
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

        private void MenuLength_1m (object sender, RoutedEventArgs e)
            => SetStripLength (60);

        private void MenuLength_10m (object sender, RoutedEventArgs e)
            => SetStripLength (600);

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
                case Patient.PatientEventTypes.Obstetric_Baseline:
                    listTracings.ForEach (c => c.Strip?.Add_Beat__Obstetric_Baseline (App.Patient));
                    break;

                case Patient.PatientEventTypes.Obstetric_Contraction_Start:
                    listTracings.ForEach (c => c.Strip?.ClearFuture (App.Patient));
                    listTracings.ForEach (c => c.Strip?.Add_Beat__Obstetric_Contraction_Start (App.Patient));
                    break;

                case Patient.PatientEventTypes.Obstetric_Contraction_End:
                    listTracings.ForEach (c => c.Strip?.ClearFuture (App.Patient));
                    listTracings.ForEach (c => c.Strip?.Add_Beat__Obstetric_Baseline (App.Patient));
                    break;
            }
        }
    }
}