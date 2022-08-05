using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using II;

namespace IISE.Controls {

    public partial class PropertyRxOrder : UserControl {
        private bool isInitiated = false;

        public new event EventHandler<PropertyRxOrderEventArgs>? PropertyChanged;

        public class PropertyRxOrderEventArgs : EventArgs {
            public Medication.Order RxOrder = new ();
        }

        public PropertyRxOrder () {
            InitializeComponent ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public void Init (Medication.Order rxOrder) {
            if (PropertyChanged != null) {              // In case of re-initiation, need to wipe all subscriptions
                foreach (Delegate d in PropertyChanged.GetInvocationList ())
                    PropertyChanged -= (EventHandler<PropertyRxOrderEventArgs>)d;
            }

            TextBox? ptxtDrugName = this.FindControl<TextBox> ("txtDrugName");
            TextBox? ptxtIndication = this.FindControl<TextBox> ("txtIndication");
            TextBox? ptxtNotes = this.FindControl<TextBox> ("txtNotes");
            ComboBox? pcmbDoseUnit = this.FindControl<ComboBox> ("cmbDoseUnit");
            ComboBox? pcmbDoseRoute = this.FindControl<ComboBox> ("cmbDoseRoute");
            ComboBox? pcmbPeriodType = this.FindControl<ComboBox> ("cmbPeriodType");
            ComboBox? pcmbPeriodUnit = this.FindControl<ComboBox> ("cmbPeriodUnit");
            ComboBox? pcmbPriority = this.FindControl<ComboBox> ("cmbPriority");
            NumericUpDown? pnumDoseAmount = this.FindControl<NumericUpDown> ("numDoseAmount");
            NumericUpDown? pnumPeriodAmount = this.FindControl<NumericUpDown> ("numPeriodAmount");
            NumericUpDown? pnumTotalDoses = this.FindControl<NumericUpDown> ("numTotalDoses");
            DatePicker? pdateStart = this.FindControl<DatePicker> ("dateStart");
            DatePicker? pdateEnd = this.FindControl<DatePicker> ("dateEnd");
            TimePicker? ptimeStart = this.FindControl<TimePicker> ("timeStart");
            TimePicker? ptimeEnd = this.FindControl<TimePicker> ("timeEnd");

            // Populate enum string lists for readable display
            List<string> doseUnits = new List<string> (),
                doseRoutes = new List<string> (),
                periodTypes = new List<string> (),
                periodUnits = new List<string> (),
                priorities = new List<string> ();

            if (App.Language != null) {
                foreach (var v in Enum.GetValues<Medication.Order.DoseUnits.Values> ())
                    doseUnits.Add (App.Language.Dictionary [Medication.Order.DoseUnits.LookupString (v)]);

                foreach (var v in Enum.GetValues<Medication.Order.Routes.Values> ())
                    doseRoutes.Add (App.Language.Dictionary [Medication.Order.Routes.LookupString (v)]);

                foreach (var v in Enum.GetValues<Medication.Order.PeriodTypes.Values> ())
                    periodTypes.Add (App.Language.Dictionary [Medication.Order.PeriodTypes.LookupString (v)]);

                foreach (var v in Enum.GetValues<Medication.Order.PeriodUnits.Values> ())
                    periodUnits.Add (App.Language.Dictionary [Medication.Order.PeriodUnits.LookupString (v)]);

                foreach (var v in Enum.GetValues<Medication.Order.Priorities.Values> ())
                    priorities.Add (App.Language.Dictionary [Medication.Order.Priorities.LookupString (v)]);
            }

            pcmbDoseUnit.Items = doseUnits;
            pcmbDoseRoute.Items = doseRoutes;
            pcmbPeriodType.Items = periodTypes;
            pcmbPeriodUnit.Items = periodUnits;
            pcmbPriority.Items = priorities;

            ptxtDrugName.Text = rxOrder.DrugName;
            pnumDoseAmount.Value = rxOrder.DoseAmount ?? 0d;
            pcmbDoseUnit.SelectedIndex = rxOrder.DoseUnit.GetHashCode ();
            pcmbDoseRoute.SelectedIndex = rxOrder.Route.GetHashCode ();
            pcmbPeriodType.SelectedIndex = rxOrder.PeriodType.GetHashCode ();
            pnumPeriodAmount.Value = rxOrder.PeriodAmount ?? 0;
            pcmbPeriodUnit.SelectedIndex = rxOrder.PeriodUnit.GetHashCode ();
            pnumTotalDoses.Value = rxOrder.TotalDoses ?? 0;
            pcmbPriority.SelectedIndex = rxOrder.Priority.GetHashCode ();

            pdateStart.SelectedDate = rxOrder.StartTime;
            ptimeStart.SelectedTime = new TimeSpan (
                rxOrder.StartTime?.Hour ?? 0,
                rxOrder.StartTime?.Minute ?? 0,
                0);

            pdateEnd.SelectedDate = rxOrder.EndTime;
            ptimeEnd.SelectedTime = new TimeSpan (
                rxOrder.EndTime?.Hour ?? 0,
                rxOrder.EndTime?.Minute ?? 0,
                0);

            ptxtIndication.Text = rxOrder.Indication;
            ptxtNotes.Text = rxOrder.Notes;

            if (!isInitiated) {
                ptxtDrugName.TextInput += SendPropertyChange;
                ptxtDrugName.LostFocus += SendPropertyChange;

                ptxtIndication.TextInput += SendPropertyChange;
                ptxtIndication.LostFocus += SendPropertyChange;

                ptxtNotes.TextInput += SendPropertyChange;
                ptxtNotes.LostFocus += SendPropertyChange;

                pcmbDoseUnit.SelectionChanged += SendPropertyChange;
                pcmbDoseRoute.SelectionChanged += SendPropertyChange;
                pcmbPeriodType.SelectionChanged += SendPropertyChange;
                pcmbPeriodUnit.SelectionChanged += SendPropertyChange;
                pcmbPriority.SelectionChanged += SendPropertyChange;

                pnumDoseAmount.ValueChanged += SendPropertyChange;
                pnumPeriodAmount.ValueChanged += SendPropertyChange;
                pnumTotalDoses.ValueChanged += SendPropertyChange;

                pdateStart.SelectedDateChanged += SendPropertyChange;
                pdateEnd.SelectedDateChanged += SendPropertyChange;

                ptimeStart.SelectedTimeChanged += SendPropertyChange;
                ptimeEnd.SelectedTimeChanged += SendPropertyChange;
            }

            isInitiated = true;
        }

        private void UpdateView (Medication.Order rxOrder) {
            TextBox? ptxtDrugName = this.FindControl<TextBox> ("txtDrugName");
            TextBox? ptxtIndication = this.FindControl<TextBox> ("txtIndication");
            TextBox? ptxtNotes = this.FindControl<TextBox> ("txtNotes");
            ComboBox? pcmbDoseUnit = this.FindControl<ComboBox> ("cmbDoseUnit");
            ComboBox? pcmbDoseRoute = this.FindControl<ComboBox> ("cmbDoseRoute");
            ComboBox? pcmbPeriodType = this.FindControl<ComboBox> ("cmbPeriodType");
            ComboBox? pcmbPeriodUnit = this.FindControl<ComboBox> ("cmbPeriodUnit");
            ComboBox? pcmbPriority = this.FindControl<ComboBox> ("cmbPriority");
            NumericUpDown? pnumDoseAmount = this.FindControl<NumericUpDown> ("numDoseAmount");
            NumericUpDown? pnumPeriodAmount = this.FindControl<NumericUpDown> ("numPeriodAmount");
            NumericUpDown? pnumTotalDoses = this.FindControl<NumericUpDown> ("numTotalDoses");
            DatePicker? pdateStart = this.FindControl<DatePicker> ("dateStart");
            DatePicker? pdateEnd = this.FindControl<DatePicker> ("dateEnd");
            TimePicker? ptimeStart = this.FindControl<TimePicker> ("timeStart");
            TimePicker? ptimeEnd = this.FindControl<TimePicker> ("timeEnd");

            pcmbPeriodUnit.IsEnabled = rxOrder.PeriodType != Medication.Order.PeriodTypes.Values.Once;
            pnumPeriodAmount.IsEnabled = rxOrder.PeriodType != Medication.Order.PeriodTypes.Values.Once;
            pnumTotalDoses.IsEnabled = rxOrder.PeriodType != Medication.Order.PeriodTypes.Values.Once;
        }

        private void SendPropertyChange (object? sender, EventArgs e) {
            TextBox? ptxtDrugName = this.FindControl<TextBox> ("txtDrugName");
            TextBox? ptxtIndication = this.FindControl<TextBox> ("txtIndication");
            TextBox? ptxtNotes = this.FindControl<TextBox> ("txtNotes");
            ComboBox? pcmbDoseUnit = this.FindControl<ComboBox> ("cmbDoseUnit");
            ComboBox? pcmbDoseRoute = this.FindControl<ComboBox> ("cmbDoseRoute");
            ComboBox? pcmbPeriodType = this.FindControl<ComboBox> ("cmbPeriodType");
            ComboBox? pcmbPeriodUnit = this.FindControl<ComboBox> ("cmbPeriodUnit");
            ComboBox? pcmbPriority = this.FindControl<ComboBox> ("cmbPriority");
            NumericUpDown? pnumDoseAmount = this.FindControl<NumericUpDown> ("numDoseAmount");
            NumericUpDown? pnumPeriodAmount = this.FindControl<NumericUpDown> ("numPeriodAmount");
            NumericUpDown? pnumTotalDoses = this.FindControl<NumericUpDown> ("numTotalDoses");
            DatePicker? pdateStart = this.FindControl<DatePicker> ("dateStart");
            DatePicker? pdateEnd = this.FindControl<DatePicker> ("dateEnd");
            TimePicker? ptimeStart = this.FindControl<TimePicker> ("timeStart");
            TimePicker? ptimeEnd = this.FindControl<TimePicker> ("timeEnd");

            PropertyRxOrderEventArgs ea = new PropertyRxOrderEventArgs ();

            ea.RxOrder.DrugName = ptxtDrugName.Text;
            ea.RxOrder.DoseAmount = pnumDoseAmount.Value;
            ea.RxOrder.DoseUnit = Enum.GetValues<Medication.Order.DoseUnits.Values> () [
                pcmbDoseUnit.SelectedIndex < 0 ? 0 : pcmbDoseUnit.SelectedIndex];

            ea.RxOrder.Route = Enum.GetValues<Medication.Order.Routes.Values> () [
                pcmbDoseRoute.SelectedIndex < 0 ? 0 : pcmbDoseRoute.SelectedIndex];

            ea.RxOrder.PeriodType = Enum.GetValues<Medication.Order.PeriodTypes.Values> () [
                pcmbPeriodType.SelectedIndex < 0 ? 0 : pcmbPeriodType.SelectedIndex];
            ea.RxOrder.PeriodAmount = (int)pnumPeriodAmount.Value;
            ea.RxOrder.PeriodUnit = Enum.GetValues<Medication.Order.PeriodUnits.Values> () [
                pcmbPeriodUnit.SelectedIndex < 0 ? 0 : pcmbPeriodUnit.SelectedIndex];

            ea.RxOrder.TotalDoses = (int)pnumTotalDoses.Value;

            ea.RxOrder.Priority = Enum.GetValues<Medication.Order.Priorities.Values> () [
                pcmbPriority.SelectedIndex < 0 ? 0 : pcmbPriority.SelectedIndex];

            ea.RxOrder.StartTime = new DateTime (
                pdateStart?.SelectedDate?.Year ?? new DateTime ().Year,
                pdateStart?.SelectedDate?.Month ?? new DateTime ().Month,
                pdateStart?.SelectedDate?.Day ?? new DateTime ().Day,
                ptimeStart?.SelectedTime?.Hours ?? new DateTime ().Hour,
                ptimeStart?.SelectedTime?.Minutes ?? new DateTime ().Minute,
                0);

            ea.RxOrder.EndTime = new DateTime (
                pdateEnd?.SelectedDate?.Year ?? new DateTime ().Year,
                pdateEnd?.SelectedDate?.Month ?? new DateTime ().Month,
                pdateEnd?.SelectedDate?.Day ?? new DateTime ().Day,
                ptimeEnd?.SelectedTime?.Hours ?? new DateTime ().Hour,
                ptimeEnd?.SelectedTime?.Minutes ?? new DateTime ().Minute,
                0);
            ea.RxOrder.Indication = ptxtIndication.Text;
            ea.RxOrder.Notes = ptxtNotes.Text;

            Debug.WriteLine ($"PropertyChanged: RxOrder");

            PropertyChanged?.Invoke (this, ea);

            UpdateView (ea.RxOrder);
        }

        private void SendPropertyChange (object? sender, DatePickerSelectedValueChangedEventArgs e)
            => SendPropertyChange (sender, new EventArgs ());

        private void SendPropertyChange (object? sender, TimePickerSelectedValueChangedEventArgs e)
            => SendPropertyChange (sender, new EventArgs ());
    }
}