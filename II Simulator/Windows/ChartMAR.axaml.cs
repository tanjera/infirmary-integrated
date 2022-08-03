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

        private DateTime viewAtTime = DateTime.Now;

        private DateTime? viewStartTime,
                            viewEndTime;

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
            _ = PopulateHeaders ();
            _ = PopulateDrugs ();
            _ = PopulateDoses ();
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
                this.FindControl<MenuItem> ("menuRefresh").Header = Instance.Language.Localize ("MENU:MenuRefresh");
            }

            /* Establish references */
            await ReferenceView ();

            /* Set the View time (time of MAR being viewed) to the Chart.CurrentTime */
            viewAtTime = new DateTime (
                Instance?.Chart?.CurrentTime?.Year ?? DateTime.Now.Year,
                Instance?.Chart?.CurrentTime?.Month ?? DateTime.Now.Month,
                Instance?.Chart?.CurrentTime?.Day ?? DateTime.Now.Day,
                Instance?.Chart?.CurrentTime?.Hour ?? DateTime.Now.Hour,
                0, 0);

            DatePicker pdpAtTime = this.FindControl<DatePicker> ("dpAtTime");
            pdpAtTime.SelectedDate = new DateTimeOffset (viewAtTime);
            pdpAtTime.SelectedDateChanged += DateSelected_DateChanged;

            TimePicker ptpAtTime = this.FindControl<TimePicker> ("tpAtTime");
            ptpAtTime.SelectedTime = new TimeSpan (viewAtTime.Hour, viewAtTime.Minute, 0);
            ptpAtTime.SelectedTimeChanged += TimeSelected_TimeChanged;
        }

        private async Task RefreshInterface () {
            /* Set the View time (time of MAR being viewed) to the Chart.CurrentTime */
            viewAtTime = new DateTime (
                Instance?.Chart?.CurrentTime?.Year ?? DateTime.Now.Year,
                Instance?.Chart?.CurrentTime?.Month ?? DateTime.Now.Month,
                Instance?.Chart?.CurrentTime?.Day ?? DateTime.Now.Day,
                Instance?.Chart?.CurrentTime?.Hour ?? DateTime.Now.Hour,
                0, 0);

            DatePicker pdpAtTime = this.FindControl<DatePicker> ("dpAtTime");
            pdpAtTime.SelectedDateChanged -= DateSelected_DateChanged;
            pdpAtTime.SelectedDate = new DateTimeOffset (viewAtTime);
            pdpAtTime.SelectedDateChanged += DateSelected_DateChanged;

            TimePicker ptpAtTime = this.FindControl<TimePicker> ("tpAtTime");
            ptpAtTime.SelectedTimeChanged -= TimeSelected_TimeChanged;
            ptpAtTime.SelectedTime = new TimeSpan (viewAtTime.Hour, viewAtTime.Minute, 0);
            ptpAtTime.SelectedTimeChanged += TimeSelected_TimeChanged;

            await PopulateHeaders ();
            await PopulateDrugs ();
            await PopulateDoses ();
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

        private Task PopulateHeaders () {
            if (Instance?.Chart is null)
                return Task.CompletedTask;

            if (gridMain is null)
                return Task.CompletedTask;

            const int headerRow = 1;
            const int columnAmount = 12;

            /* Set some variables used by other functions (defining what the calendar view extends to */
            viewStartTime = viewAtTime - new TimeSpan (6, 0, 0);
            viewEndTime = viewStartTime + new TimeSpan (columnAmount - 1, 59, 59);

            /* Clear any items on the Grid in the Drug space */
            for (int i = gridMain.Children.Count - 1; i >= 0; i--) {
                if (gridMain.Children [i].GetValue (Grid.RowProperty) == headerRow) {
                    gridMain.Children.RemoveAt (i);
                }
            }

            /* Start working from 6 hours prior to viewAtTime, on the hour (00:00) */
            DateTime atTime = viewStartTime.Value;

            /* Populate column headers for time periods */
            for (int hour = 0; hour < columnAmount; hour++) {
                Label lbl = new Label () {
                    Content = $"{atTime.ToShortDateString ()}\n{atTime.ToShortTimeString ()}",
                    Margin = new Thickness (20, 5),
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    FontWeight = hour == columnAmount / 2 ? FontWeight.Bold : FontWeight.Normal
                };

                lbl.SetValue (Grid.RowProperty, headerRow);
                lbl.SetValue (Grid.ColumnProperty, columnAmount - hour);

                gridMain?.Children.Add (lbl);

                atTime += new TimeSpan (1, 0, 0);
            }

            return Task.CompletedTask;
        }

        private async Task PopulateDrugs () {
            if (Instance?.Chart is null)
                return;

            await ReferenceView ();

            if (gridMain is null)
                return;

            const int drugColumn = 0;
            const int startRow = 2;           // The row to start populating drugs on

            /* Color-coding */
            SolidColorBrush colorScheduled = new (Avalonia.Media.Color.Parse ("#D1FFFC"));
            SolidColorBrush colorPRN = new (Avalonia.Media.Color.Parse ("#DEFFD1"));

            /* Clear any items on the Grid in the Drug space */
            for (int i = gridMain.Children.Count - 1; i >= 0; i--) {
                if (gridMain.Children [i].GetValue (Grid.ColumnProperty) == drugColumn
                    && gridMain.Children [i].GetValue (Grid.RowProperty) >= startRow) {
                    gridMain.Children.RemoveAt (i);
                }
            }

            /* The main drug information (along the left-hand column) */
            for (int i = 0; i < Instance.Chart.RxOrders.Count; i++) {
                if (gridMain?.RowDefinitions.Count < startRow + i + 1) {
                    gridMain?.RowDefinitions.Add (new RowDefinition () {
                        Height = GridLength.Auto,
                        MinHeight = 100
                    });
                }

                var order = Instance.Chart.RxOrders [i];
                int row = startRow + i;

                TextBlock tbDrug = new TextBlock () {
                    Text = $"{order.DrugName} {order.DoseAmount} {order.DoseUnit} {order.Route}\n"
                            + $"{order.PeriodType} {order.PeriodAmount} {order.PeriodUnit}\n"
                            + $"{order.Priority}\n{order.StartTime}\n{order.EndTime}",
                    Padding = new Thickness (10),
                    Background = order.IsScheduled ? colorScheduled : colorPRN
                };

                tbDrug.SetValue (Grid.RowProperty, row);
                tbDrug.SetValue (Grid.ColumnProperty, drugColumn);
                gridMain?.Children.Add (tbDrug);
            }
        }

        private async Task PopulateDoses () {
            if (Instance?.Chart is null)
                return;

            await ReferenceView ();

            if (gridMain is null)
                return;

            const int doseStartColumn = 1;
            const int doseStartRow = 2;
            const int columnAmount = 12;

            /* Color-coding */
            SolidColorBrush colorScheduled = new (Avalonia.Media.Color.Parse ("#D1FFFC"));
            SolidColorBrush colorPRN = new (Avalonia.Media.Color.Parse ("#DEFFD1"));

            /* Clear any items on the Grid in the Dose space */
            for (int i = gridMain.Children.Count - 1; i >= 0; i--) {
                if (gridMain.Children [i].GetValue (Grid.ColumnProperty) >= doseStartColumn
                    && gridMain.Children [i].GetValue (Grid.RowProperty) >= doseStartRow) {
                    gridMain.Children.RemoveAt (i);
                }
            }

            /* Populate the doses across the calendar grid */
            for (int i = 0; i < Instance.Chart.RxOrders.Count; i++) {
                var order = Instance.Chart.RxOrders [i];
                var doses = Instance.Chart.RxDoses.FindAll (d
                    => d.OrderUUID == order.UUID
                    && d.ScheduledTime >= viewStartTime && d.ScheduledTime <= viewEndTime);

                foreach (var dose in doses) {
                    if (dose.ScheduledTime is null || viewStartTime is null)
                        continue;

                    IBrush bgColor = Brushes.Black;
                    if (dose.AdministrationStatus == Medication.Dose.AdministrationStatuses.Values.Administered) {
                        bgColor = Brushes.LightGray;
                    } else if (dose.AdministrationStatus == Medication.Dose.AdministrationStatuses.Values.NotAdministered) {
                        if (order.Priority == Medication.Order.Priorities.Values.Stat) {
                            bgColor = Brushes.HotPink;
                        } else if (Instance.Chart.CurrentTime - dose.ScheduledTime > new TimeSpan (1, 0, 0)) {
                            bgColor = Brushes.Red;      // Use Instance.Chart.CurrentTime to determine if doses are late!
                        } else if (order.PeriodType == Medication.Order.PeriodTypes.Values.PRN) {
                            bgColor = colorPRN;
                        } else {
                            bgColor = colorScheduled;
                        }
                    }

                    TextBlock tbDose = new TextBlock () {
                        Text = $"{order.DoseAmount} {order.DoseUnit}\n{order.Route}\n{order.Priority}\n{dose.ScheduledTime}",
                        Padding = new Thickness (10),
                        Background = bgColor
                    };

                    tbDose.SetValue (Grid.RowProperty, doseStartRow + i);              // Get Grid.Row based on Rx Order iteration
                    tbDose.SetValue (Grid.ColumnProperty, doseStartColumn + (columnAmount - 1) - (dose.ScheduledTime - viewStartTime).Value.Hours);
                    gridMain.Children.Add (tbDose);
                }
            }
        }

        private void AdjustViewTime (int hours) {
            AdjustViewTime (viewAtTime + new TimeSpan (hours, 0, 0));
        }

        private void AdjustViewTime (DateTime newDate) {
            if (Instance?.Chart is null)
                return;

            viewAtTime = newDate;

            /* Update the View with the new date (unsubscribe from change to prevent feedback loop */
            DatePicker pdpAtTime = this.FindControl<DatePicker> ("dpAtTime");
            pdpAtTime.SelectedDateChanged -= DateSelected_DateChanged;
            pdpAtTime.SelectedDate = new DateTimeOffset (newDate);
            pdpAtTime.SelectedDateChanged += DateSelected_DateChanged;

            TimePicker ptpAtTime = this.FindControl<TimePicker> ("tpAtTime");
            ptpAtTime.SelectedTimeChanged -= TimeSelected_TimeChanged;
            ptpAtTime.SelectedTime = new TimeSpan (newDate.Hour, newDate.Minute, 0);
            ptpAtTime.SelectedTimeChanged += TimeSelected_TimeChanged;

            _ = PopulateHeaders ();
            _ = PopulateDoses ();
        }

        private void ButtonTimeForward1_Click (object? s, RoutedEventArgs e)
            => AdjustViewTime (1);

        private void ButtonTimeForward12_Click (object? s, RoutedEventArgs e)
            => AdjustViewTime (12);

        private void ButtonTimeBackwards1_Click (object? s, RoutedEventArgs e)
            => AdjustViewTime (-1);

        private void ButtonTimeBackwards12_Click (object? s, RoutedEventArgs e)
            => AdjustViewTime (-12);

        private void DateSelected_DateChanged (object? s, DatePickerSelectedValueChangedEventArgs e) {
            if (e.NewDate is null)
                return;

            AdjustViewTime (new DateTime (
                e.NewDate.Value.Year,
                e.NewDate.Value.Month,
                e.NewDate.Value.Day,
                viewAtTime.Hour,
                0, 0));
        }

        private void TimeSelected_TimeChanged (object? s, TimePickerSelectedValueChangedEventArgs e) {
            if (e.NewTime is null)
                return;

            AdjustViewTime (new DateTime (
                viewAtTime.Year,
                viewAtTime.Month,
                viewAtTime.Day,
                e.NewTime.Value.Hours,
                0, 0));
        }

        private void MenuClose_Click (object s, RoutedEventArgs e)
            => this.Close ();

        private void MenuRefresh_Click (object s, RoutedEventArgs e)
            => _ = RefreshInterface ();
    }
}