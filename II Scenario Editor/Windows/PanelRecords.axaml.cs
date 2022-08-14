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

    public partial class PanelRecords : UserControl {
        /* Pointer to main data structure for the scenario, patient, devices, etc. */
        public Scenario.Step? Step;
        public Record? Records;

        public int SelectedRxOrder = -1;

        private WindowMain? IMain;

        public PanelRecords () {
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
            Records = step?.Records;

            await UpdateView ();
        }

        private Task InitView () {
            // Populate enum string lists for readable display
            List<string> codeStatuses = new List<string> ();

            if (App.Language != null) {
                foreach (Record.CodeStatuses.Values v in Enum.GetValues (typeof (Record.CodeStatuses.Values)))
                    codeStatuses.Add (App.Language.Dictionary [Record.CodeStatuses.LookupString (v)]);
            }

            // Find all controls and attach to reference
            PropertyDate pdpSimDate = this.FindControl<PropertyDate> ("pdpSimDate");
            PropertyDate pdpDOB = this.FindControl<PropertyDate> ("pdpDOB");

            PropertyTime ptpSimTime = this.FindControl<PropertyTime> ("ptpSimTime");

            PropertyEnum penmCodeStatus = this.FindControl<PropertyEnum> ("penmCodeStatus");

            PropertyString pstrName = this.FindControl<PropertyString> ("pstrName");
            PropertyString pstrMRN = this.FindControl<PropertyString> ("pstrMRN");
            PropertyString pstrHomeAddress = this.FindControl<PropertyString> ("pstrHomeAddress");
            PropertyString pstrTelephoneNumber = this.FindControl<PropertyString> ("pstrTelephoneNumber");
            PropertyString pstrInsuranceProvider = this.FindControl<PropertyString> ("pstrInsuranceProvider");
            PropertyString pstrInsuranceAccount = this.FindControl<PropertyString> ("pstrInsuranceAccount");
            PropertyString pstrDoseComment = this.FindControl<PropertyString> ("pstrDoseComment");

            PropertyRxOrder prxOrder = this.FindControl<PropertyRxOrder> ("prxoRxOrder");

            ListBox lbRxOrders = this.FindControl<ListBox> ("lbRxOrders");

            pdpSimDate.Init (PropertyDate.Keys.SimulationDate);
            pdpDOB.Init (PropertyDate.Keys.DemographicsDOB);

            ptpSimTime.Init (PropertyTime.Keys.SimulationTime);

            penmCodeStatus.Init (PropertyEnum.Keys.CodeStatus,
                Enum.GetNames (typeof (Record.CodeStatuses.Values)), codeStatuses);

            pstrName.Init (PropertyString.Keys.DemographicsName);
            pstrMRN.Init (PropertyString.Keys.DemographicsMRN);
            pstrHomeAddress.Init (PropertyString.Keys.DemographicsHomeAddress);
            pstrTelephoneNumber.Init (PropertyString.Keys.DemographicsTelephoneNumber);
            pstrInsuranceProvider.Init (PropertyString.Keys.DemographicsInsuranceProvider);
            pstrInsuranceAccount.Init (PropertyString.Keys.DemographicsInsuranceAccount);
            pstrDoseComment.Init (PropertyString.Keys.DoseComment);

            pdpSimDate.PropertyChanged += UpdateRecords;
            pdpDOB.PropertyChanged += UpdateRecords;

            ptpSimTime.PropertyChanged += UpdateRecords;

            penmCodeStatus.PropertyChanged += UpdateRecords;

            pstrName.PropertyChanged += UpdateRecords;
            pstrMRN.PropertyChanged += UpdateRecords;
            pstrHomeAddress.PropertyChanged += UpdateRecords;
            pstrTelephoneNumber.PropertyChanged += UpdateRecords;
            pstrInsuranceProvider.PropertyChanged += UpdateRecords;
            pstrInsuranceAccount.PropertyChanged += UpdateRecords;
            pstrDoseComment.PropertyChanged += UpdateRecords;

            prxOrder.PropertyChanged += UpdateRecords;
            lbRxOrders.SelectionChanged += LbRxOrders_SelectionChanged;

            return Task.CompletedTask;
        }

        private Task UpdateView () {
            Label lblActiveStep = this.FindControl<Label> ("lblActiveStep");

            PropertyDate pdpSimDate = this.FindControl<PropertyDate> ("pdpSimDate");
            PropertyDate pdpDOB = this.FindControl<PropertyDate> ("pdpDOB");

            PropertyTime ptpSimTime = this.FindControl<PropertyTime> ("ptpSimTime");

            PropertyEnum penmCodeStatus = this.FindControl<PropertyEnum> ("penmCodeStatus");

            PropertyString pstrName = this.FindControl<PropertyString> ("pstrName");
            PropertyString pstrMRN = this.FindControl<PropertyString> ("pstrMRN");
            PropertyString pstrHomeAddress = this.FindControl<PropertyString> ("pstrHomeAddress");
            PropertyString pstrTelephoneNumber = this.FindControl<PropertyString> ("pstrTelephoneNumber");
            PropertyString pstrInsuranceProvider = this.FindControl<PropertyString> ("pstrInsuranceProvider");
            PropertyString pstrInsuranceAccount = this.FindControl<PropertyString> ("pstrInsuranceAccount");

            PropertyRxOrder prxOrder = this.FindControl<PropertyRxOrder> ("prxoRxOrder");

            ListBox lbRxOrders = this.FindControl<ListBox> ("lbRxOrders");

            Button btnAddRxOrder = this.FindControl<Button> ("btnAddRxOrder");
            Button btnDelRxOrder = this.FindControl<Button> ("btnDelRxOrder");

            lblActiveStep.Content = String.Format ("Editing Step: {0} ({1})",
                Step is null ? "N/A" : Step.Name,
                Step is null ? "N/A" : Step.Description);

            pdpSimDate.IsEnabled = (Records != null);
            pdpDOB.IsEnabled = (Records != null);

            ptpSimTime.IsEnabled = (Records != null);

            penmCodeStatus.IsEnabled = (Records != null);

            pstrName.IsEnabled = (Records != null);
            pstrMRN.IsEnabled = (Records != null);
            pstrHomeAddress.IsEnabled = (Records != null);
            pstrTelephoneNumber.IsEnabled = (Records != null);
            pstrInsuranceProvider.IsEnabled = (Records != null);
            pstrInsuranceAccount.IsEnabled = (Records != null);

            prxOrder.IsEnabled = lbRxOrders.IsEnabled && lbRxOrders.SelectedIndex >= 0;

            lbRxOrders.IsEnabled = (Records != null);

            btnAddRxOrder.IsEnabled = (Records != null);
            btnDelRxOrder.IsEnabled = (Records != null);

            if (Records != null) {
                pdpSimDate.Set (Records?.CurrentTime);
                ptpSimTime.Set (Records?.CurrentTime);

                pdpDOB.Set (Records?.DOB ?? new DateOnly ());

                penmCodeStatus.Set ((int)(Records?.CodeStatus ?? Record.CodeStatuses.Values.FullCode));

                pstrName.Set (Records?.Name ?? "");
                pstrMRN.Set (Records?.MRN ?? "");
                pstrHomeAddress.Set (Records?.HomeAddress ?? "");
                pstrTelephoneNumber.Set (Records?.TelephoneNumber ?? "");
                pstrInsuranceProvider.Set (Records?.InsuranceProvider ?? "");
                pstrInsuranceAccount.Set (Records?.InsuranceAccount ?? "");
            }

            UpdateView_RxOrderList ();
            UpdateView_RxDoseList ();

            return Task.CompletedTask;
        }

        private void UpdateView_RxOrderList () {
            if (Records is null)
                return;

            ListBox lbRxOrders = this.FindControl<ListBox> ("lbRxOrders");

            lbRxOrders.SelectionChanged -= LbRxOrders_SelectionChanged;

            List<string> llbi = new ();

            SortRxOrders ();

            foreach (var order in Records.RxOrders) {
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
            if (Records is null || Records.RxOrders is null
                || SelectedRxOrder < 0 || Records.RxOrders.Count <= SelectedRxOrder)
                return;

            ListBox lbRxDoses = this.FindControl<ListBox> ("lbRxDoses");

            lbRxDoses.SelectionChanged -= LbRxDoses_SelectionChanged;

            var order = Records.RxOrders [SelectedRxOrder];
            var doses = Records.RxDoses.FindAll (d => d.OrderUUID == order.UUID);

            List<string> llbi = new ();

            for (int i = 0; i < doses.Count; i++) {
                if (App.Language is not null) {
                    llbi.Add (String.Format ("{0}: {1} {2}, {3}: {4}",
                        i,
                        doses [i].ScheduledTime?.ToShortDateString (),
                        doses [i].ScheduledTime?.ToString ("HH:mm"),
                        App.Language.Localize (doses [i].Administered ? "ENUM:AdministrationStatuses:Administered" : "ENUM:AdministrationStatuses:NotAdministered"),
                        doses [i].Comment
                        ));
                }
            }

            // Save old selection
            List<int> selections = new ();
            foreach (var dose in lbRxDoses.SelectedItems.Cast<string> ())
                selections.Add (int.Parse (dose.Substring (0, dose.IndexOf (':'))));
            lbRxDoses.SelectedItems.Clear ();

            // Update list to display
            lbRxDoses.Items = llbi;

            // Restore selection as available
            foreach (int i in selections) {
                if (i < llbi.Count)
                    lbRxDoses.SelectedItems.Add (llbi [i]);
            }

            lbRxDoses.SelectionChanged += LbRxDoses_SelectionChanged;
        }

        private void UpdateRecords (object? sender, PropertyDate.PropertyDateEventArgs e) {
            if (e.Value != null && Records != null) {
                switch (e.Key) {
                    default: break;

                    case PropertyDate.Keys.SimulationDate:
                        Records.CurrentTime = new DateTime (
                        e.Value?.Year ?? new DateTime ().Year,
                        e.Value?.Month ?? new DateTime ().Month,
                        e.Value?.Day ?? new DateTime ().Day,
                        Records?.CurrentTime?.Hour ?? 0,
                        Records?.CurrentTime?.Minute ?? 0,
                        0);
                        break;

                    case PropertyDate.Keys.DemographicsDOB: Records.DOB = e.Value ?? new DateOnly (); break;
                }
            }
        }

        private void UpdateRecords (object? sender, PropertyTime.PropertyTimeEventArgs e) {
            if (e.Value != null && Records != null) {
                switch (e.Key) {
                    default: break;

                    case PropertyTime.Keys.SimulationTime:
                        Records.CurrentTime = new DateTime (
                        Records?.CurrentTime?.Year ?? new DateTime ().Year,
                        Records?.CurrentTime?.Month ?? new DateTime ().Month,
                        Records?.CurrentTime?.Day ?? new DateTime ().Day,
                        e.Value?.Hour ?? 0,
                        e.Value?.Minute ?? 0,
                        0);
                        break;
                }
            }
        }

        private void UpdateRecords (object? sender, PropertyEnum.PropertyEnumEventArgs e) {
            if (e.Value != null && Records != null) {
                switch (e.Key) {
                    default: break;

                    case PropertyEnum.Keys.CodeStatus:
                        Records.CodeStatus = (Record.CodeStatuses.Values)Enum.Parse (typeof (Record.CodeStatuses.Values), e.Value);
                        break;
                }
            }
        }

        private void UpdateRecords (object? sender, PropertyString.PropertyStringEventArgs e) {
            if (Records != null) {
                switch (e.Key) {
                    default: break;
                    case PropertyString.Keys.DemographicsName: Records.Name = e.Value; break;
                    case PropertyString.Keys.DemographicsMRN: Records.MRN = e.Value; break;
                    case PropertyString.Keys.DemographicsHomeAddress: Records.HomeAddress = e.Value; break;
                    case PropertyString.Keys.DemographicsTelephoneNumber: Records.TelephoneNumber = e.Value; break;
                    case PropertyString.Keys.DemographicsInsuranceProvider: Records.InsuranceProvider = e.Value; break;
                    case PropertyString.Keys.DemographicsInsuranceAccount: Records.InsuranceAccount = e.Value; break;
                    case PropertyString.Keys.DoseComment: Action_SetRxDoseComment (e.Value ?? ""); break;
                }
            }
        }

        private void UpdateRecords (object? sender, PropertyRxOrder.PropertyRxOrderEventArgs e) {
            if (Records is null)
                return;

            ListBox lbRxOrders = this.FindControl<ListBox> ("lbRxOrders");

            if (SelectedRxOrder >= 0 && SelectedRxOrder < Records.RxOrders.Count) {
                Records.RxOrders [SelectedRxOrder] = e.RxOrder;
            }

            UpdateView_RxOrderList ();
        }

        private void SortRxOrders () {
            string? selUUID = null;
            if (SelectedRxOrder >= 0 && Records?.RxOrders.Count > SelectedRxOrder)
                selUUID = Records.RxOrders [SelectedRxOrder].UUID;

            Records?.RxOrders.Sort ((a, b) => {
                if (a.PeriodType != Medication.Order.PeriodTypes.Values.PRN && b.PeriodType == Medication.Order.PeriodTypes.Values.PRN)
                    return -1;
                else if (a.PeriodType == Medication.Order.PeriodTypes.Values.PRN && b.PeriodType != Medication.Order.PeriodTypes.Values.PRN)
                    return 1;
                else return String.Compare (a.DrugName, b.DrugName);
            });

            if (selUUID is null)
                SelectedRxOrder = -1;
            else
                SelectedRxOrder = Records?.RxOrders.FindIndex (o => o.UUID == selUUID) ?? -1;
        }

        private void PopulateRxDose (int rxOrderIndex) {
            if (Records is null || Records.RxOrders.Count < rxOrderIndex)
                return;

            if (!Records.RxOrders [rxOrderIndex].IsComplete)
                return;

            if (Records.RxDoses is null)
                Records.RxDoses = new List<Medication.Dose> ();

            Medication.Order order = Records.RxOrders [rxOrderIndex];

            // Clear all doses for the currently selected drug
            for (int i = Records.RxDoses.Count - 1; i >= 0; i--) {
                if (Records.RxDoses [i].OrderUUID == order.UUID)
                    Records.RxDoses.RemoveAt (i);
            }

            // Repopulate doses for the currently selected drug
            switch (order.PeriodType) {
                default:
                case Medication.Order.PeriodTypes.Values.Once:
                case Medication.Order.PeriodTypes.Values.PRN:
                    if (Records.CurrentTime >= order.StartTime && Records.CurrentTime <= order.EndTime)
                        Records.RxDoses.Add (new Medication.Dose () {
                            OrderUUID = order.UUID,
                            ScheduledTime = Records.CurrentTime,
                            Administered = false
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
                        var scheduledTime = order.StartTime + timeInterval;

                        if (scheduledTime >= order.StartTime && scheduledTime <= order.EndTime)
                            Records.RxDoses.Add (new Medication.Dose () {
                                OrderUUID = order.UUID,
                                ScheduledTime = scheduledTime,
                                Administered = false
                            });
                    }
                    break;
            }
        }

        private void Action_SelectRxOrder () {
            PropertyRxOrder prxOrder = this.FindControl<PropertyRxOrder> ("prxoRxOrder");
            ListBox lbRxOrders = this.FindControl<ListBox> ("lbRxOrders");

            if (Records is null || lbRxOrders.SelectedIndex < 0 || lbRxOrders.SelectedIndex >= Records.RxOrders.Count) {
                prxOrder.IsEnabled = false;
                prxOrder.Init (Records is null ? new Medication.Order () : new Medication.Order (Records));
                return;
            }

            prxOrder.PropertyChanged -= UpdateRecords;
            prxOrder.IsEnabled = true;

            SelectedRxOrder = lbRxOrders.SelectedIndex;
            prxOrder.Init (Records.RxOrders [SelectedRxOrder]);

            prxOrder.PropertyChanged += UpdateRecords;

            UpdateView_RxDoseList ();
        }

        private void Action_AddRxOrder () {
            if (Records is null)
                return;

            Medication.Order order = new (Records);
            Records.RxOrders.Add (order);

            UpdateView_RxOrderList ();

            ListBox lbRxOrders = this.FindControl<ListBox> ("lbRxOrders");
            lbRxOrders.SelectedIndex = Records.RxOrders.FindIndex (o => o.UUID == order.UUID);
            SelectedRxOrder = lbRxOrders.SelectedIndex;

            Action_SelectRxOrder ();
        }

        private void Action_DeleteRxOrder () {
            if (Records is null)
                return;

            ListBox lbRxOrders = this.FindControl<ListBox> ("lbRxOrders");
            SelectedRxOrder = lbRxOrders.SelectedIndex;

            if (SelectedRxOrder < 0 || SelectedRxOrder >= Records.RxOrders.Count)
                return;

            Medication.Order order = Records.RxOrders [SelectedRxOrder];
            Records.RxDoses.RemoveAll (d => d.OrderUUID == order.UUID);
            Records.RxOrders.RemoveAt (SelectedRxOrder);

            SelectedRxOrder = SelectedRxOrder > -1 ? SelectedRxOrder - 1 : -1;

            UpdateView_RxOrderList ();
            Action_SelectRxOrder ();
        }

        private void Action_SelectRxDoses () {
            this.FindControl<PropertyString> ("pstrDoseComment").Set ("");
        }

        private void Action_PopulateAllRxDoses () {
            for (int i = 0; i < Records?.RxOrders.Count; i++)
                PopulateRxDose (i);

            UpdateView_RxDoseList ();
        }

        private void Action_PopulateThisRxDoses () {
            PopulateRxDose (SelectedRxOrder);
            UpdateView_RxDoseList ();
        }

        private void Action_ToggleRxDoseAdministration () {
            if (Records is null || Records.RxOrders is null
                || SelectedRxOrder < 0 || Records.RxOrders.Count <= SelectedRxOrder)
                return;

            ListBox lbRxDoses = this.FindControl<ListBox> ("lbRxDoses");

            var order = Records.RxOrders [SelectedRxOrder];
            var doses = Records.RxDoses.FindAll (d => d.OrderUUID == order.UUID);

            foreach (var dose in lbRxDoses.SelectedItems.Cast<string> ()) {
                int i = int.Parse (dose.Substring (0, dose.IndexOf (':')));

                if (i < doses.Count)
                    doses [i].Administered = !doses [i].Administered;
            }

            UpdateView_RxDoseList ();
        }

        private void Action_SetRxDoseComment (string comment) {
            if (Records is null || Records.RxOrders is null
                || SelectedRxOrder < 0 || Records.RxOrders.Count <= SelectedRxOrder)
                return;

            ListBox lbRxDoses = this.FindControl<ListBox> ("lbRxDoses");

            var order = Records.RxOrders [SelectedRxOrder];
            var doses = Records.RxDoses.FindAll (d => d.OrderUUID == order.UUID);

            foreach (var dose in lbRxDoses.SelectedItems.Cast<string> ()) {
                int i = int.Parse (dose.Substring (0, dose.IndexOf (':')));

                if (i < doses.Count)
                    doses [i].Comment = comment;
            }

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

        private void LbRxDoses_SelectionChanged (object? sender, SelectionChangedEventArgs e)
            => Action_SelectRxDoses ();

        private void ButtonAddRxOrder_Click (object sender, RoutedEventArgs e)
            => Action_AddRxOrder ();

        private void ButtonDeleteRxOrder_Click (object sender, RoutedEventArgs e)
            => Action_DeleteRxOrder ();

        private void ButtonPopulateAllRxDoses_Click (object sender, RoutedEventArgs e)
            => Action_PopulateAllRxDoses ();

        private void ButtonPopulateThisRxDoses_Click (object sender, RoutedEventArgs e)
            => Action_PopulateThisRxDoses ();

        private void ButtonToggleDoseAdministration_Click (object sender, RoutedEventArgs e)
            => Action_ToggleRxDoseAdministration ();
    }
}