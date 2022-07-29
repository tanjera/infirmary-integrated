using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

namespace IISIM {

    public partial class ChartMAR : ChartWindow {
        private Grid? gridMain;

        public ChartMAR () {
            InitializeComponent ();
        }

        public ChartMAR (App? app) : base (app) {
            InitializeComponent ();
#if DEBUG
            this.AttachDevTools ();
#endif

            DataContext = this;

            _ = InitInterface ();

#if DEBUG
            List<Chart.Order.Drug> testDrugs = new List<Chart.Order.Drug> () {
                new Chart.Order.Drug() {
                    DrugName = "Cefazolin",
                    DoseAmount = 1,
                    DoseUnit = Chart.Order.Drug.DoseUnits.G,
                    Route = Chart.Order.Drug.Routes.Intravenous,
                    PeriodType = Chart.Order.Drug.PeriodTypes.Repeats,
                    PeriodAmount = 6,
                    PeriodUnit = Chart.Order.Drug.PeriodUnits.Hour,
                    TotalDoses = 6,
                    Priority = Chart.Order.Drug.Priorities.Routine,
                    Notes = "Mix in 100 mL D5W, Adminster over 30 min"
                    },

                new Chart.Order.Drug() {
                    DrugName = "Morphine",
                    DoseAmount = 2,
                    DoseUnit = Chart.Order.Drug.DoseUnits.MG,
                    Route = Chart.Order.Drug.Routes.Intravenous,
                    PeriodType = Chart.Order.Drug.PeriodTypes.PRN,
                    PeriodAmount = 6,
                    PeriodUnit = Chart.Order.Drug.PeriodUnits.Hour,
                    TotalDoses = 6,
                    Priority = Chart.Order.Drug.Priorities.Routine
                    }
                };

            _ = InitDrugs (testDrugs, 8);
#endif
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        private Task ReferenceView () {
            gridMain = this.FindControl<Grid> ("gridMain");

            return Task.CompletedTask;
        }

        private async Task InitInterface () {
            /* Populate UI strings per language selection */
            if (Instance is not null) {
                this.FindControl<Window> ("wdwChartMAR").Title = Instance.Language.Localize ("MAR:WindowTitle");
                this.FindControl<MenuItem> ("menuOptions").Header = Instance.Language.Localize ("MENU:MenuOptions");
                this.FindControl<MenuItem> ("menuClose").Header = Instance.Language.Localize ("MENU:MenuClose");
            }

            /* Establish references */
            await ReferenceView ();

            if (gridMain is null)
                return;

            /* Populate column headers for time periods */
            int column = 24;
            for (int hour = 0; hour < 24; hour++) {
                Label lbl = new Label () {
                    Content = $"{hour:00} : 00",
                    Margin = new Thickness (20, 5),
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                };

                lbl.SetValue (Grid.RowProperty, 0);
                lbl.SetValue (Grid.ColumnProperty, column);

                gridMain?.Children.Add (lbl);

                column -= 1;
            }
        }

        private async Task InitDrugs (List<Chart.Order.Drug> listDrugs, int currentHour) {
            await ReferenceView ();

            if (gridMain is null)
                return;

            int startRow = 1;           // The row to start populating drugs on
            int startCol = 24;          // The right-most column at time 00:00

            /* Color-coding */
            SolidColorBrush colorScheduled = new (Avalonia.Media.Color.Parse ("#D1FFFC"));
            SolidColorBrush colorPRN = new (Avalonia.Media.Color.Parse ("#DEFFD1"));

            /* The main drug information (along the left-hand column */
            for (int i = 0; i < listDrugs.Count; i++) {
                gridMain?.RowDefinitions.Add (new RowDefinition () {
                    Height = GridLength.Auto,
                    MinHeight = 100
                });

                var d = listDrugs [i];
                int row = startRow + i;

                TextBlock tbDrug = new TextBlock () {
                    Text = $"{d.DrugName} {d.DoseAmount} {d.DoseUnit} {d.Route}\n{d.PeriodType} {d.PeriodAmount} {d.PeriodUnit}\n{d.Priority}\n{d.StartTime}\n{d.EndTime}",
                    Padding = new Thickness (10),
                    Background = d.IsScheduled ? colorScheduled : colorPRN
                };

                tbDrug.SetValue (Grid.RowProperty, row);
                tbDrug.SetValue (Grid.ColumnProperty, 0);
                gridMain?.Children.Add (tbDrug);

                /* Populate the doses across the calendar grid */
                switch (d.PeriodType) {
                    default:
                    case Chart.Order.Drug.PeriodTypes.Once:
                    case Chart.Order.Drug.PeriodTypes.PRN:
                        TextBlock singleDose = new TextBlock () {
                            Text = $"{d.DoseAmount} {d.DoseUnit}\n{d.Route}\n{d.Priority}",
                            Padding = new Thickness (10),
                            Background = d.IsScheduled ? colorScheduled : colorPRN
                        };

                        singleDose.SetValue (Grid.RowProperty, row);
                        singleDose.SetValue (Grid.ColumnProperty, startCol - currentHour);
                        gridMain?.Children.Add (singleDose);
                        break;

                    case Chart.Order.Drug.PeriodTypes.Repeats:
                        if (d.PeriodAmount is null || d.PeriodUnit is null)
                            break;

                        int doseHour = currentHour;
                        for (int j = 0; j < d.TotalDoses && doseHour <= 24; j++) {
                            TextBlock repeatDose = new TextBlock () {
                                Text = $"{d.DoseAmount} {d.DoseUnit}\n{d.Route}\n{d.Priority}",
                                Padding = new Thickness (10),
                                Background = colorScheduled
                            };

                            repeatDose.SetValue (Grid.RowProperty, row);
                            repeatDose.SetValue (Grid.ColumnProperty, startCol - doseHour);
                            gridMain?.Children.Add (repeatDose);

                            doseHour += d.PeriodUnit switch {
                                Chart.Order.Drug.PeriodUnits.Hour => d.PeriodAmount ?? 1,
                                Chart.Order.Drug.PeriodUnits.Day => (d.PeriodAmount ?? 1) * 24,
                                Chart.Order.Drug.PeriodUnits.Week => (d.PeriodAmount ?? 1) * 24 * 7,
                                _ => d.PeriodAmount ?? 1,
                            };
                        }
                        break;
                }
            }
        }

        public void Load_Process (string inc) {
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

        private void MenuClose_Click (object s, RoutedEventArgs e)
            => this.Close ();
    }
}