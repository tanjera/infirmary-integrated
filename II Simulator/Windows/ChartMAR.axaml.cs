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
            _ = InitDrugs ();
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

        private async Task InitDrugs () {
            if (Instance?.Chart is null)
                return;

            await ReferenceView ();

            if (gridMain is null)
                return;

            int startRow = 1;           // The row to start populating drugs on
            int startCol = 24;          // The right-most column at time 00:00

            /* Color-coding */
            SolidColorBrush colorScheduled = new (Avalonia.Media.Color.Parse ("#D1FFFC"));
            SolidColorBrush colorPRN = new (Avalonia.Media.Color.Parse ("#DEFFD1"));

            /* The main drug information (along the left-hand column */
            for (int i = 0; i < Instance.Chart.MedicationOrders.Count; i++) {
                gridMain?.RowDefinitions.Add (new RowDefinition () {
                    Height = GridLength.Auto,
                    MinHeight = 100
                });

                var order = Instance.Chart.MedicationOrders [i];
                int row = startRow + i;

                TextBlock tbDrug = new TextBlock () {
                    Text = $"{order.DrugName} {order.DoseAmount} {order.DoseUnit} {order.Route}\n"
                            + $"{order.PeriodType} {order.PeriodAmount} {order.PeriodUnit}\n"
                            + $"{order.Priority}\n{order.StartTime}\n{order.EndTime}",
                    Padding = new Thickness (10),
                    Background = order.IsScheduled ? colorScheduled : colorPRN
                };

                tbDrug.SetValue (Grid.RowProperty, row);
                tbDrug.SetValue (Grid.ColumnProperty, 0);
                gridMain?.Children.Add (tbDrug);

                /* Populate the doses across the calendar grid */
                for (int j = 0; j < Instance.Chart.MedicationDoses.Count; j++) {
                    var dose = Instance.Chart.MedicationDoses [j];

                    throw new NotImplementedException ();
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