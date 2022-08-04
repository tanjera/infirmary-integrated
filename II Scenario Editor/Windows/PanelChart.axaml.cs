using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;

using II;
using IISE.Controls;

namespace IISE.Windows {

    public partial class PanelChart : UserControl {
        /* Pointer to main data structure for the scenario, patient, devices, etc. */
        public Scenario.Step? Step;
        public Chart? Chart;

        public int SelectedRxOrder = -1;

        private WindowMain? IMain;

        public PanelChart () {
            InitializeComponent ();

            DataContext = this;

            _ = InitView ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public Task InitReferences (WindowMain main) {
            IMain = main;

            return Task.CompletedTask;
        }

        public async Task SetStep (Scenario.Step? step) {
            Step = step;
            Chart = step?.Chart;

            await UpdateView ();
        }

        private Task InitView () {
            PropertyRxOrder prxOrder = this.FindControl<PropertyRxOrder> ("prxoRxOrder");
            ListBox lbRxOrders = this.FindControl<ListBox> ("lbRxOrders");
            DatePicker pdateCurrent = this.FindControl<DatePicker> ("dateCurrent");
            TimePicker ptimeCurrent = this.FindControl<TimePicker> ("timeCurrent");

            pdateCurrent.SelectedDateChanged += SendPropertyChange;
            ptimeCurrent.SelectedTimeChanged += SendPropertyChange;

            prxOrder.PropertyChanged += UpdateChart;
            lbRxOrders.SelectionChanged += LbRxOrders_SelectionChanged;

            return Task.CompletedTask;
        }

        private Task UpdateView () {
            Label lblActiveStep = this.FindControl<Label> ("lblActiveStep");

            DatePicker pdateCurrent = this.FindControl<DatePicker> ("dateCurrent");
            TimePicker ptimeCurrent = this.FindControl<TimePicker> ("timeCurrent");
            ListBox lbRxOrders = this.FindControl<ListBox> ("lbRxOrders");
            Button btnAddRxOrder = this.FindControl<Button> ("btnAddRxOrder");
            Button btnDelRxOrder = this.FindControl<Button> ("btnDelRxOrder");
            PropertyRxOrder prxOrder = this.FindControl<PropertyRxOrder> ("prxoRxOrder");

            lblActiveStep.Content = String.Format ("Editing Step: {0} ({1})",
                Step is null ? "N/A" : Step.Name,
                Step is null ? "N/A" : Step.Description);

            pdateCurrent.IsEnabled = (Chart != null);
            ptimeCurrent.IsEnabled = (Chart != null);
            lbRxOrders.IsEnabled = (Chart != null);
            btnAddRxOrder.IsEnabled = (Chart != null);
            btnDelRxOrder.IsEnabled = (Chart != null);
            prxOrder.IsEnabled = lbRxOrders.IsEnabled && lbRxOrders.SelectedIndex >= 0;

            if (Chart is not null) {
                pdateCurrent.SelectedDateChanged -= SendPropertyChange;
                ptimeCurrent.SelectedTimeChanged -= SendPropertyChange;

                pdateCurrent.SelectedDate = Chart.CurrentTime;
                ptimeCurrent.SelectedTime = new TimeSpan (
                    Chart.CurrentTime?.Hour ?? 0,
                    Chart.CurrentTime?.Minute ?? 0,
                    0);

                pdateCurrent.SelectedDateChanged += SendPropertyChange;
                ptimeCurrent.SelectedTimeChanged += SendPropertyChange;
            }

            UpdateView_RxOrderList ();
            UpdateView_RxDoseList ();

            return Task.CompletedTask;
        }

        private void UpdateView_RxOrderList () {
            if (Chart is null)
                return;

            ListBox lbRxOrders = this.FindControl<ListBox> ("lbRxOrders");

            lbRxOrders.SelectionChanged -= LbRxOrders_SelectionChanged;

            List<string> llbi = new ();

            foreach (var order in Chart.RxOrders) {
                if (order.IsComplete) {
                    if (App.Language is not null) {
                        llbi.Add (String.Format ("{0} {1} {2} {3}",
                            order.DrugName,
                            order.DoseAmount,
                            App.Language.Localize (Medication.Order.DoseUnits.LookupString (order.DoseUnit ?? Medication.Order.DoseUnits.Values.L)),
                            App.Language.Localize (Medication.Order.Routes.LookupString (order.Route ?? Medication.Order.Routes.Values.IV))
                            ));
                    } else {
                        llbi.Add (String.Format ("{0} {1}",
                            order.DrugName,
                            order.DoseAmount
                            ));
                    }
                } else if (!order.IsComplete) {
                    llbi.Add ("Incomplete Order");
                }
            }

            lbRxOrders.Items = llbi;
            if (SelectedRxOrder >= 0 && SelectedRxOrder < llbi.Count)
                lbRxOrders.SelectedIndex = SelectedRxOrder;
            else
                lbRxOrders.UnselectAll ();

            lbRxOrders.SelectionChanged += LbRxOrders_SelectionChanged;
        }

        private void UpdateView_RxDoseList () {
            if (Chart is null || Chart.RxOrders.Count <= SelectedRxOrder)
                return;

            ListBox lbRxDoses = this.FindControl<ListBox> ("lbRxDoses");

            lbRxDoses.SelectionChanged += LbRxDoses_SelectionChanged;

            var order = Chart.RxOrders [SelectedRxOrder];
            var doses = Chart.RxDoses.FindAll (d => d.OrderUUID == order.UUID);

            List<string> llbi = new ();

            foreach (var dose in doses) {
                if (App.Language is not null) {
                    llbi.Add (String.Format ("{0} {1}, {2}, {3}",
                        dose?.ScheduledTime?.ToShortDateString (),
                        dose?.ScheduledTime?.ToString ("HH:mm"),
                        App.Language.Localize (Medication.Dose.TimeStatuses.LookupString (dose.TimeStatus ?? Medication.Dose.TimeStatuses.Values.Pending)),
                        App.Language.Localize (Medication.Dose.AdministrationStatuses.LookupString (dose.AdministrationStatus ?? Medication.Dose.AdministrationStatuses.Values.NotAdministered))
                        ));
                }
            }

            lbRxDoses.Items = llbi;

            lbRxDoses.SelectionChanged += LbRxDoses_SelectionChanged;
        }

        private void UpdateChart (object? sender, EventArgs e) {
            if (Chart is null)
                return;

            DatePicker pdateCurrent = this.FindControl<DatePicker> ("dateCurrent");
            TimePicker ptimeCurrent = this.FindControl<TimePicker> ("timeCurrent");

            Chart.CurrentTime = new DateTime (
                pdateCurrent.SelectedDate.Value.Year,
                pdateCurrent.SelectedDate.Value.Month,
                pdateCurrent.SelectedDate.Value.Day,
                ptimeCurrent.SelectedTime.Value.Hours,
                ptimeCurrent.SelectedTime.Value.Minutes,
                0);
        }

        private void UpdateChart (object? sender, PropertyRxOrder.PropertyRxOrderEventArgs e) {
            if (Chart is null)
                return;

            ListBox lbRxOrders = this.FindControl<ListBox> ("lbRxOrders");

            if (SelectedRxOrder >= 0 && SelectedRxOrder < Chart.RxOrders.Count) {
                Chart.RxOrders [SelectedRxOrder] = e.RxOrder;
            }

            UpdateView_RxOrderList ();
        }

        private void PopulateRxDose (int rxOrderIndex) {
            if (Chart is null || Chart.RxOrders.Count < rxOrderIndex)
                return;

            if (!Chart.RxOrders [rxOrderIndex].IsComplete)
                return;

            if (Chart.RxDoses is null)
                Chart.RxDoses = new List<Medication.Dose> ();

            Medication.Order order = Chart.RxOrders [rxOrderIndex];

            // Clear all doses for the currently selected drug
            for (int i = Chart.RxDoses.Count - 1; i >= 0; i--) {
                if (Chart.RxDoses [i].OrderUUID == order.UUID)
                    Chart.RxDoses.RemoveAt (i);
            }

            // Repopulate doses for the currently selected drug
            switch (Chart.RxOrders [rxOrderIndex].PeriodType) {
                default:
                case Medication.Order.PeriodTypes.Values.Once:
                case Medication.Order.PeriodTypes.Values.PRN:
                    Chart.RxDoses.Add (new Medication.Dose () {
                        OrderUUID = order.UUID,
                        ScheduledTime = Chart.CurrentTime,
                        TimeStatus = Medication.Dose.TimeStatuses.Values.Pending,
                        AdministrationStatus = Medication.Dose.AdministrationStatuses.Values.NotAdministered
                    });
                    break;

                case Medication.Order.PeriodTypes.Values.Repeats:
                    int timeMultiplier = order.PeriodUnit switch {
                        Medication.Order.PeriodUnits.Values.Minute => 1,
                        Medication.Order.PeriodUnits.Values.Hour => 60,
                        Medication.Order.PeriodUnits.Values.Day => 60 * 24,
                        Medication.Order.PeriodUnits.Values.Week => 60 * 24 * 7,
                        _ => 1,
                    };

                    for (int j = 0; j < order.TotalDoses; j++) {
                        TimeSpan timeInterval = new TimeSpan (0, j * timeMultiplier * (order.PeriodAmount ?? 1), 0);

                        Chart.RxDoses.Add (new Medication.Dose () {
                            OrderUUID = order.UUID,
                            ScheduledTime = order.StartTime + timeInterval,
                            TimeStatus = order.StartTime + timeInterval < Chart.CurrentTime
                            ? Medication.Dose.TimeStatuses.Values.Late
                            : Medication.Dose.TimeStatuses.Values.Pending,
                            AdministrationStatus = Medication.Dose.AdministrationStatuses.Values.NotAdministered
                        });
                    }
                    break;
            }
        }

        private void Action_SelectRxOrder () {
            PropertyRxOrder prxOrder = this.FindControl<PropertyRxOrder> ("prxoRxOrder");
            ListBox lbRxOrders = this.FindControl<ListBox> ("lbRxOrders");

            if (Chart is null || lbRxOrders.SelectedIndex < 0 || lbRxOrders.SelectedIndex >= Chart.RxOrders.Count) {
                prxOrder.IsEnabled = false;
                prxOrder.Init (new Medication.Order ());
                return;
            }

            prxOrder.PropertyChanged -= UpdateChart;
            prxOrder.IsEnabled = true;

            SelectedRxOrder = lbRxOrders.SelectedIndex;
            prxOrder.Init (Chart.RxOrders [SelectedRxOrder]);

            prxOrder.PropertyChanged += UpdateChart;

            UpdateView_RxDoseList ();
        }

        private void Action_AddRxOrder () {
            if (Chart is null)
                return;

            Chart.RxOrders.Add (new Medication.Order ());

            UpdateView_RxOrderList ();

            ListBox lbRxOrders = this.FindControl<ListBox> ("lbRxOrders");
            lbRxOrders.SelectedIndex = Chart.RxOrders.Count - 1;
            SelectedRxOrder = lbRxOrders.SelectedIndex;

            Action_SelectRxOrder ();
        }

        private void Action_DeleteRxOrder () {
            if (Chart is null)
                return;

            ListBox lbRxOrders = this.FindControl<ListBox> ("lbRxOrders");

            int index = lbRxOrders.SelectedIndex;
            for (int i = Chart.RxDoses.Count - 1; i >= 0; i--) {
                if (Chart.RxDoses [i].OrderUUID == Chart.RxOrders [index].UUID)
                    Chart.RxDoses.RemoveAt (i);
            }

            Chart.RxOrders.RemoveAt (lbRxOrders.SelectedIndex);
            SelectedRxOrder -= 1;
            lbRxOrders.SelectedIndex = SelectedRxOrder;

            UpdateView_RxOrderList ();
            Action_SelectRxOrder ();
        }

        private void Action_PopulateAllRxDoses () {
            for (int i = 0; i < Chart?.RxOrders.Count; i++)
                PopulateRxDose (i);

            UpdateView_RxDoseList ();
        }

        private void Action_PopulateThisRxDoses () {
            PopulateRxDose (SelectedRxOrder);
            UpdateView_RxDoseList ();
        }

        /* Generic Menu Items (across all Panels) */

        private void MenuFileNew_Click (object sender, RoutedEventArgs e)
            => IMain?.MenuFileNew_Click (sender, e);

        private void MenuFileLoad_Click (object sender, RoutedEventArgs e)
            => IMain?.MenuFileLoad_Click (sender, e);

        private void MenuFileSave_Click (object sender, RoutedEventArgs e)
            => IMain?.MenuFileSave_Click (sender, e);

        private void MenuFileSaveAs_Click (object sender, RoutedEventArgs e)
            => IMain?.MenuFileSaveAs_Click (sender, e);

        private void MenuFileExit_Click (object sender, RoutedEventArgs e)
            => IMain?.MenuFileExit_Click (sender, e);

        private void MenuHelpAbout_Click (object sender, RoutedEventArgs e)
            => IMain?.MenuHelpAbout_Click (sender, e);

        /* Any other Routed events for this Panel */

        private void LbRxOrders_SelectionChanged (object? sender, SelectionChangedEventArgs e)
            => Action_SelectRxOrder ();

        private void LbRxDoses_SelectionChanged (object? sender, SelectionChangedEventArgs e) {
        }

        private void ButtonAddRxOrder_Click (object sender, RoutedEventArgs e)
            => Action_AddRxOrder ();

        private void ButtonDeleteRxOrder_Click (object sender, RoutedEventArgs e)
            => Action_DeleteRxOrder ();

        private void ButtonPopulateAllRxDoses_Click (object sender, RoutedEventArgs e)
            => Action_PopulateAllRxDoses ();

        private void ButtonPopulateThisRxDoses_Click (object sender, RoutedEventArgs e)
            => Action_PopulateThisRxDoses ();

        private void SendPropertyChange (object? sender, DatePickerSelectedValueChangedEventArgs e)
            => UpdateChart (sender, new EventArgs ());

        private void SendPropertyChange (object? sender, TimePickerSelectedValueChangedEventArgs e)
            => UpdateChart (sender, new EventArgs ());
    }
}