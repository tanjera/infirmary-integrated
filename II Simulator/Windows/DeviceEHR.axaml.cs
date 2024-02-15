/* Infirmary Integrated Simulator
 * By Ibi Keller (Tanjera), (c) 2023
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reactive;
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

namespace IISIM {

    public partial class DeviceEHR : RecordWindow {
        /* References for UI elements */
        public ScrollViewer cntlContent;

        public PanelDemographics Panel_Demographics;
        public PanelNotes Panel_Notes;
        public PanelMAR Panel_MAR;

        public Records SelectedRecord;

        public enum Records {
            Demographics,
            Notes,
            Flowsheet,
            Results,
            MAR
        }

        public DeviceEHR () {
            InitializeComponent ();
        }

        public DeviceEHR (App? app) : base (app) {
            InitializeComponent ();
#if DEBUG
            this.AttachDevTools ();
#endif

            DataContext = this;

            /* Establish reference variables */
            cntlContent = this.FindControl<ScrollViewer> ("svContent");

            InitInterface ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        private Task InitInterface () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (InitInterface)}");
                return Task.CompletedTask;
            }

            /* Initiate content Panels & references */
            Panel_Demographics = new PanelDemographics (Instance);
            Panel_Notes = new PanelNotes (Instance);
            Panel_MAR = new PanelMAR (Instance);

            /* Populate UI strings per language selection */

            this.FindControl<Window> ("wdwDeviceEHR").Title = Instance.Language.Localize ("EHR:WindowTitle");
            this.FindControl<MenuItem> ("menuOptions").Header = Instance.Language.Localize ("MENU:MenuOptions");
            this.FindControl<MenuItem> ("menuClose").Header = Instance.Language.Localize ("MENU:MenuClose");
            this.FindControl<MenuItem> ("menuToggleFullscreen").Header = Instance.Language.Localize ("MENU:MenuToggleFullscreen");
            this.FindControl<MenuItem> ("menuRefresh").Header = Instance.Language.Localize ("MENU:MenuRefresh");

            this.FindControl<Label> ("lblDemographics").Content = Instance.Language.Localize ("PE:Demographics");
            this.FindControl<Label> ("lblNotes").Content = Instance.Language.Localize ("PE:Notes");
            this.FindControl<Label> ("lblFlowsheet").Content = Instance.Language.Localize ("PE:Flowsheet");
            this.FindControl<Label> ("lblResults").Content = Instance.Language.Localize ("PE:LabResults");
            this.FindControl<Label> ("lblMAR").Content = Instance.Language.Localize ("PE:MAR");

            this.FindControl<Label> ("lblPatientName").Content = Instance?.Records?.Name;

            this.FindControl<Label> ("lblPatientDOB").Content = String.Format ("{0}: {1}",
                Instance?.Language.Localize ("CHART:DateOfBirth"),
                Instance?.Records?.DOB?.ToShortDateString ());

            this.FindControl<Label> ("lblPatientMRN").Content = String.Format ("{0}: {1}",
                Instance?.Language.Localize ("CHART:MedicalRecordNumber"),
                Instance?.Records?.MRN);

            SelectRecord (Records.Demographics);

            return Task.CompletedTask;
        }

        private async Task RefreshInterface () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (RefreshInterface)}");
                return;
            }

            this.FindControl<Label> ("lblPatientName").Content = Instance?.Records?.Name;

            this.FindControl<Label> ("lblPatientDOB").Content = String.Format ("{0}: {1}",
                Instance?.Language.Localize ("CHART:DateOfBirth"),
                Instance?.Records?.DOB?.ToShortDateString ());

            this.FindControl<Label> ("lblPatientMRN").Content = String.Format ("{0}: {1}",
                Instance?.Language.Localize ("CHART:MedicalRecordNumber"),
                Instance?.Records?.MRN);

            switch (SelectedRecord) {
                default:
                    break;

                case Records.Demographics:
                    await Panel_Demographics.RefreshInterface ();
                    break;

                case Records.Notes:
                    await Panel_Notes.RefreshInterface ();
                    break;

                case Records.Flowsheet:
                case Records.Results:
                    break;

                case Records.MAR:
                    await Panel_MAR.RefreshInterface ();
                    break;
            }
        }

        public void Load (string inc) {
            using StringReader sRead = new (inc);

            try {
                string? line;
                while ((line = sRead.ReadLine ()) != null) {
                    if (line.Contains (':')) {
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

        private void SelectRecord_Demographics () => SelectRecord (Records.Demographics);

        private void SelectRecord_Notes () => SelectRecord (Records.Notes);

        private void SelectRecord_MAR () => SelectRecord (Records.MAR);

        private void SelectRecord (Records incType) {
            SelectedRecord = incType;

            switch (SelectedRecord) {
                default:
                    break;

                case Records.Demographics:
                    cntlContent.Content = Panel_Demographics;
                    break;

                case Records.Notes:
                    cntlContent.Content = Panel_Notes;
                    break;

                case Records.Flowsheet:
                case Records.Results:
                    break;

                case Records.MAR:
                    cntlContent.Content = Panel_MAR;
                    break;
            }

            _ = RefreshInterface ();
        }

        private void ButtonRefresh_Click (object? s, RoutedEventArgs e)
            => _ = RefreshInterface ();

        public void ToggleFullscreen () {
            if (WindowState == WindowState.FullScreen)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.FullScreen;
        }

        private void MenuToggleFullscreen_Click (object s, RoutedEventArgs e)
            => ToggleFullscreen ();

        private void MenuClose_Click (object s, RoutedEventArgs e)
            => this.Close ();

        private void MenuRefresh_Click (object s, RoutedEventArgs e)
            => _ = RefreshInterface ();

        private void ButtonDemographics_Click (object s, RoutedEventArgs e)
            => SelectRecord_Demographics ();

        private void ButtonNotes_Click (object s, RoutedEventArgs e)
            => SelectRecord_Notes ();

        private void ButtonFlowsheet_Click (object s, RoutedEventArgs e) {
        }

        private void ButtonResults_Click (object s, RoutedEventArgs e) {
        }

        private void ButtonMAR_Click (object s, RoutedEventArgs e)
            => SelectRecord_MAR ();
    }
}