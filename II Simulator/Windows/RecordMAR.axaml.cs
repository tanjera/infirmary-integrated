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

    public partial class RecordMAR : RecordWindow {
        private DateTime viewAtTime = DateTime.Now;

        private DateTime? viewStartTime,
                            viewEndTime;

        /* References for UI elements */
        private Grid? gridMain;

        private List<TextBlock> tbDoses = new ();

        public RecordMAR () {
            InitializeComponent ();
        }

        public RecordMAR (App? app) : base (app) {
            InitializeComponent ();
#if DEBUG
            this.AttachDevTools ();
#endif

            DataContext = this;

            /* Establish reference variables */
            gridMain = this.FindControl<Grid> ("gridMain");

            Task.Run (async () => {
                await Dispatcher.UIThread.InvokeAsync (async () => {
                    await InitInterface ();
                    await PopulateHeaders ();
                    await PopulateDrugs ();
                    await PopulateDoses ();
                });
            });
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        private Task InitInterface () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (InitInterface)}");
                return Task.CompletedTask;
            }

            /* Populate UI strings per language selection */

            this.FindControl<Window> ("wdwRecordMAR").Title = Instance.Language.Localize ("MAR:WindowTitle");
            this.FindControl<MenuItem> ("menuOptions").Header = Instance.Language.Localize ("MENU:MenuOptions");
            this.FindControl<MenuItem> ("menuClose").Header = Instance.Language.Localize ("MENU:MenuClose");
            this.FindControl<MenuItem> ("menuRefresh").Header = Instance.Language.Localize ("MENU:MenuRefresh");

            this.FindControl<Label> ("lblPatientName").Content = Instance?.Records?.Name;

            this.FindControl<Label> ("lblPatientDOB").Content = String.Format ("{0}: {1}",
                Instance?.Language.Localize ("CHART:DateOfBirth"),
                Instance?.Records?.DOB.ToShortDateString ());

            this.FindControl<Label> ("lblPatientMRN").Content = String.Format ("{0}: {1}",
                Instance?.Language.Localize ("CHART:MedicalRecordNumber"),
                Instance?.Records?.MRN);

            /* Set the View time (time of MAR being viewed) to the Chart.CurrentTime */
            viewAtTime = new DateTime (
                Instance?.Records?.CurrentTime?.Year ?? DateTime.Now.Year,
                Instance?.Records?.CurrentTime?.Month ?? DateTime.Now.Month,
                Instance?.Records?.CurrentTime?.Day ?? DateTime.Now.Day,
                Instance?.Records?.CurrentTime?.Hour ?? DateTime.Now.Hour,
                0, 0);

            DatePicker pdpAtTime = this.FindControl<DatePicker> ("dpAtTime");
            pdpAtTime.SelectedDate = new DateTimeOffset (viewAtTime);
            pdpAtTime.SelectedDateChanged += DateSelected_DateChanged;

            TimePicker ptpAtTime = this.FindControl<TimePicker> ("tpAtTime");
            ptpAtTime.SelectedTime = new TimeSpan (viewAtTime.Hour, viewAtTime.Minute, 0);
            ptpAtTime.SelectedTimeChanged += TimeSelected_TimeChanged;

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
                Instance?.Records?.DOB.ToShortDateString ());

            this.FindControl<Label> ("lblPatientMRN").Content = String.Format ("{0}: {1}",
                Instance?.Language.Localize ("CHART:MedicalRecordNumber"),
                Instance?.Records?.MRN);

            /* Set the View time (time of MAR being viewed) to the Chart.CurrentTime */
            viewAtTime = new DateTime (
                Instance?.Records?.CurrentTime?.Year ?? DateTime.Now.Year,
                Instance?.Records?.CurrentTime?.Month ?? DateTime.Now.Month,
                Instance?.Records?.CurrentTime?.Day ?? DateTime.Now.Day,
                Instance?.Records?.CurrentTime?.Hour ?? DateTime.Now.Hour,
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
            if (Instance?.Records is null || gridMain is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (PopulateHeaders)}");
                return Task.CompletedTask;
            }

            const int headerRow = 1;
            const int columnAmount = 12;

            /* Set some variables used by other functions (defining what the calendar view extends to */
            viewStartTime = viewAtTime - new TimeSpan (6, 0, 0);
            viewEndTime = viewStartTime + new TimeSpan (columnAmount - 1, 59, 59);

            /* Clear any items on the Grid in the Drug space */
            for (int i = gridMain.Children.Count - 1; i >= 0; i--) {
                if (gridMain.Children [i].GetValue (Grid.RowProperty) == headerRow
                    && gridMain.Children [i].GetValue (Grid.ColumnProperty) > 0) {
                    gridMain.Children.RemoveAt (i);
                }
            }

            /* Start working from 6 hours prior to viewAtTime, on the hour (00:00) */
            DateTime atTime = viewStartTime.Value;

            /* Populate column headers for time periods */
            for (int hour = 0; hour < columnAmount; hour++) {
                bool isAtCurrentTime = Instance?.Records?.CurrentTime?.ToString ("yyyyMMddHH") == atTime.ToString ("yyyyMMddHH");

                Controls.MARHeader mh = new () {
                    Date = atTime.ToShortDateString (),
                    Time = atTime.ToString ("HH:mm"),
                    Bold = isAtCurrentTime,
                    Background = isAtCurrentTime ? Brushes.LightBlue : Brushes.Transparent
                };

                mh.SetValue (Grid.RowProperty, headerRow);
                mh.SetValue (Grid.ColumnProperty, columnAmount - hour);

                gridMain?.Children.Add (mh);

                atTime += new TimeSpan (1, 0, 0);
            }

            return Task.CompletedTask;
        }

        private Task PopulateDrugs () {
            if (Instance?.Records is null || gridMain is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (PopulateHeaders)}");
                return Task.CompletedTask;
            }

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
            for (int i = 0; i < Instance.Records.RxOrders.Count; i++) {
                if (gridMain?.RowDefinitions.Count < startRow + i + 1) {
                    gridMain?.RowDefinitions.Add (new RowDefinition () {
                        Height = GridLength.Auto,
                        MinHeight = 100
                    });
                }

                var order = Instance.Records.RxOrders [i];
                int row = startRow + i;

                TextBlock tbDrug = new TextBlock () {
                    Text = $"{order.DrugName} {order.DoseAmount} "
                            + $"{Instance.Language.Localize (Medication.Order.DoseUnits.LookupString (order.DoseUnit ?? Medication.Order.DoseUnits.Values.L))} "
                            + $"{Instance.Language.Localize (Medication.Order.Routes.LookupString (order.Route ?? Medication.Order.Routes.Values.IV))}\n"
                            + $"{Instance.Language.Localize (Medication.Order.PeriodTypes.LookupString (order.PeriodType ?? Medication.Order.PeriodTypes.Values.Repeats))} "
                            + $"{order.PeriodAmount} "
                            + $"{Instance.Language.Localize (Medication.Order.PeriodUnits.LookupString (order.PeriodUnit ?? Medication.Order.PeriodUnits.Values.Minute))}\n"
                            + $"{Instance.Language.Localize (Medication.Order.Priorities.LookupString (order.Priority ?? Medication.Order.Priorities.Values.Routine))}\n"
                            + $"{order?.StartTime?.ToShortDateString ()} {order?.StartTime?.ToString ("HH:mm")}\n"
                            + $"{order?.EndTime?.ToShortDateString ()} {order?.EndTime?.ToString ("HH:mm")}\n"
                            + $"{order?.Indication}\n{order?.Notes}",
                    Padding = new Thickness (10),
                    Background = (order?.IsScheduled ?? true) ? colorScheduled : colorPRN
                };

                tbDrug.SetValue (Grid.RowProperty, row);
                tbDrug.SetValue (Grid.ColumnProperty, drugColumn);
                gridMain?.Children.Add (tbDrug);
            }

            return Task.CompletedTask;
        }

        private Task PopulateDoses () {
            if (Instance?.Records is null || gridMain is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (PopulateDoses)}");
                return Task.CompletedTask;
            }

            const int doseStartColumn = 1;
            const int doseStartRow = 2;
            const int columnAmount = 12;

            /* Color-coding */
            SolidColorBrush colorScheduled = new (Avalonia.Media.Color.Parse ("#D1FFFC"));
            SolidColorBrush colorPRN = new (Avalonia.Media.Color.Parse ("#DEFFD1"));

            /* Clear all Doses off the Grid */
            foreach (TextBlock tbd in tbDoses) {
                tbd.PointerPressed -= Dose_Click;
                gridMain.Children.Remove (tbd);
            }
            tbDoses.Clear ();

            /* Populate the doses across the calendar grid */
            for (int i = 0; i < Instance.Records.RxOrders.Count; i++) {
                var order = Instance.Records.RxOrders [i];
                var doses = Instance.Records.RxDoses.FindAll (d
                    => d.OrderUUID == order.UUID
                    && d.ScheduledTime >= viewStartTime && d.ScheduledTime <= viewEndTime);

                foreach (var dose in doses) {
                    if (dose.ScheduledTime is null || viewStartTime is null)
                        continue;

                    IBrush bgColor = Brushes.Black;
                    if (dose.Administered) {
                        bgColor = Brushes.LightGray;
                    } else if (!dose.Administered) {
                        if (order.Priority == Medication.Order.Priorities.Values.Stat) {
                            bgColor = Brushes.HotPink;
                        } else if (Instance.Records.CurrentTime - dose.ScheduledTime > new TimeSpan (1, 0, 0)) {
                            bgColor = Brushes.Red;      // Use Instance.Chart.CurrentTime to determine if doses are late!
                        } else if (order.PeriodType == Medication.Order.PeriodTypes.Values.PRN) {
                            bgColor = colorPRN;
                        } else {
                            bgColor = colorScheduled;
                        }
                    }

                    TextBlock tbDose = new TextBlock () {
                        Text = $"{(order.PeriodType == Medication.Order.PeriodTypes.Values.PRN ? $"{Instance.Language.Localize ("ENUM:PeriodTypes:PRN")}\n" : "")}"
                                + $"{order.DoseAmount} "
                                + $"{Instance.Language.Localize (Medication.Order.DoseUnits.LookupString (order.DoseUnit ?? Medication.Order.DoseUnits.Values.L))} "
                                + $"{Instance.Language.Localize (Medication.Order.Routes.LookupString (order.Route ?? Medication.Order.Routes.Values.IV))}\n"
                                + $"{Instance.Language.Localize (Medication.Order.Priorities.LookupString (order.Priority ?? Medication.Order.Priorities.Values.Routine))}\n"
                                + $"{dose.ScheduledTime.Value.ToShortDateString ()} {dose.ScheduledTime.Value.ToString ("HH:mm")}\n"
                                + (dose.Administered ? $"\n{Instance.Language.Localize ("ENUM:AdministrationStatuses:Administered")}" : "")
                                + (!String.IsNullOrEmpty (dose.Comment) ? $"\n{dose.Comment}" : ""),
                        Padding = new Thickness (10),
                        Background = bgColor,
                        Tag = dose
                    };

                    tbDose.SetValue (Grid.RowProperty, doseStartRow + i);              // Get Grid.Row based on Rx Order iteration
                    tbDose.SetValue (Grid.ColumnProperty, doseStartColumn + (columnAmount - 1) - (dose.ScheduledTime - viewStartTime).Value.Hours);

                    tbDose.PointerPressed += Dose_Click;

                    tbDoses.Add (tbDose);
                    gridMain.Children.Add (tbDose);
                }
            }

            return Task.CompletedTask;
        }

        private void AdjustViewTime (int hours) {
            AdjustViewTime (viewAtTime + new TimeSpan (hours, 0, 0));
        }

        private void AdjustViewTime (DateTime newDate) {
            if (Instance?.Records is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (AdjustViewTime)}");
                return;
            }

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

        private async Task DialogAdministerDose (Medication.Dose? rxDose) {
            if (rxDose is null)
                return;

            await Dispatcher.UIThread.InvokeAsync (async () => {
                DialogMARDose dlg = new (Instance,
                    Instance?.Records?.RxOrders.Find (o => o.UUID == rxDose.OrderUUID),
                    rxDose);

                dlg.Activate ();

                if (!this.IsVisible)                    // Avalonia's parent must be visible to attach a window
                    this.Show ();

                rxDose = await dlg.AsyncShow (this);

                _ = PopulateDoses ();
            });
        }

        private void ButtonRefresh_Click (object? s, RoutedEventArgs e)
            => _ = RefreshInterface ();

        private void ButtonTimeForward1_Click (object? s, RoutedEventArgs e)
            => AdjustViewTime (1);

        private void ButtonTimeForward12_Click (object? s, RoutedEventArgs e)
            => AdjustViewTime (12);

        private void ButtonTimeBackwards1_Click (object? s, RoutedEventArgs e)
            => AdjustViewTime (-1);

        private void ButtonTimeBackwards12_Click (object? s, RoutedEventArgs e)
            => AdjustViewTime (-12);

        private void DateSelected_DateChanged (object? s, DatePickerSelectedValueChangedEventArgs e) {
            if (e.NewDate is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (DateSelected_DateChanged)}");
                return;
            }

            AdjustViewTime (new DateTime (
                e.NewDate.Value.Year,
                e.NewDate.Value.Month,
                e.NewDate.Value.Day,
                viewAtTime.Hour,
                0, 0));
        }

        private void TimeSelected_TimeChanged (object? s, TimePickerSelectedValueChangedEventArgs e) {
            if (e.NewTime is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (TimeSelected_TimeChanged)}");
                return;
            }

            AdjustViewTime (new DateTime (
                viewAtTime.Year,
                viewAtTime.Month,
                viewAtTime.Day,
                e.NewTime.Value.Hours,
                0, 0));
        }

        private void Dose_Click (object? s, Avalonia.Input.PointerPressedEventArgs e) {
            if (s is null || ((TextBlock)s).Tag is null || ((TextBlock)s).Tag is not Medication.Dose)
                return;

            _ = DialogAdministerDose (((TextBlock)s).Tag as Medication.Dose);
        }

        private void MenuClose_Click (object s, RoutedEventArgs e)
            => this.Close ();

        private void MenuRefresh_Click (object s, RoutedEventArgs e)
            => _ = RefreshInterface ();
    }
}