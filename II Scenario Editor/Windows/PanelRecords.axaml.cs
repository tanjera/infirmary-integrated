/* Infirmary Integrated Scenario Editor
 * By Ibi Keller (Tanjera), (c) 2023
 */

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

        public int SelectedAllergy = -1;
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
            PropertyString pstrSex = this.FindControl<PropertyString> ("pstrSex");
            PropertyString pstrHomeAddress = this.FindControl<PropertyString> ("pstrHomeAddress");
            PropertyString pstrTelephoneNumber = this.FindControl<PropertyString> ("pstrTelephoneNumber");
            PropertyString pstrInsuranceProvider = this.FindControl<PropertyString> ("pstrInsuranceProvider");
            PropertyString pstrInsuranceAccount = this.FindControl<PropertyString> ("pstrInsuranceAccount");
            PropertyString pstrDemographicNotes = this.FindControl<PropertyString> ("pstrDemographicNotes");
            PropertyString pstrDoseComment = this.FindControl<PropertyString> ("pstrDoseComment");

            PropertyAllergy pallAllergy = this.FindControl<PropertyAllergy> ("pallAllergy");
            PropertyRxOrder prxOrder = this.FindControl<PropertyRxOrder> ("prxoRxOrder");

            ListBox lbAllergies = this.FindControl<ListBox> ("lbAllergies");
            ListBox lbRxOrders = this.FindControl<ListBox> ("lbRxOrders");

            pdpSimDate.Init (PropertyDate.Keys.SimulationDate);
            pdpDOB.Init (PropertyDate.Keys.DemographicsDOB);

            ptpSimTime.Init (PropertyTime.Keys.SimulationTime);

            penmCodeStatus.Init (PropertyEnum.Keys.CodeStatus,
                Enum.GetNames (typeof (Record.CodeStatuses.Values)), codeStatuses);

            pstrName.Init (PropertyString.Keys.DemographicsName);
            pstrMRN.Init (PropertyString.Keys.DemographicsMRN);
            pstrSex.Init (PropertyString.Keys.DemographicsSex);
            pstrHomeAddress.Init (PropertyString.Keys.DemographicsHomeAddress);
            pstrTelephoneNumber.Init (PropertyString.Keys.DemographicsTelephoneNumber);
            pstrInsuranceProvider.Init (PropertyString.Keys.DemographicsInsuranceProvider);
            pstrInsuranceAccount.Init (PropertyString.Keys.DemographicsInsuranceAccount);
            pstrDemographicNotes.Init (PropertyString.Keys.DemographicsNotes);
            pstrDoseComment.Init (PropertyString.Keys.DoseComment);

            pdpSimDate.PropertyChanged += UpdateRecords;
            pdpDOB.PropertyChanged += UpdateRecords;

            ptpSimTime.PropertyChanged += UpdateRecords;

            penmCodeStatus.PropertyChanged += UpdateRecords;

            pstrName.PropertyChanged += UpdateRecords;
            pstrMRN.PropertyChanged += UpdateRecords;
            pstrSex.PropertyChanged += UpdateRecords;
            pstrHomeAddress.PropertyChanged += UpdateRecords;
            pstrTelephoneNumber.PropertyChanged += UpdateRecords;
            pstrInsuranceProvider.PropertyChanged += UpdateRecords;
            pstrInsuranceAccount.PropertyChanged += UpdateRecords;
            pstrDemographicNotes.PropertyChanged += UpdateRecords;
            pstrDoseComment.PropertyChanged += UpdateRecords;

            pallAllergy.PropertyChanged += UpdateRecords;
            lbAllergies.SelectionChanged += LbAllergies_SelectionChanged;

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
            PropertyString pstrSex = this.FindControl<PropertyString> ("pstrSex");
            PropertyString pstrHomeAddress = this.FindControl<PropertyString> ("pstrHomeAddress");
            PropertyString pstrTelephoneNumber = this.FindControl<PropertyString> ("pstrTelephoneNumber");
            PropertyString pstrInsuranceProvider = this.FindControl<PropertyString> ("pstrInsuranceProvider");
            PropertyString pstrInsuranceAccount = this.FindControl<PropertyString> ("pstrInsuranceAccount");
            PropertyString pstrDemographicNotes = this.FindControl<PropertyString> ("pstrDemographicNotes");

            PropertyAllergy pallAllergy = this.FindControl<PropertyAllergy> ("pallAllergy");
            PropertyRxOrder prxOrder = this.FindControl<PropertyRxOrder> ("prxoRxOrder");

            ListBox lbAllergies = this.FindControl<ListBox> ("lbAllergies");
            ListBox lbRxOrders = this.FindControl<ListBox> ("lbRxOrders");

            Button btnAddAllergy = this.FindControl<Button> ("btnAddAllergy");
            Button btnDelAllergy = this.FindControl<Button> ("btnDelAllergy");

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
            pstrSex.IsEnabled = (Records != null);
            pstrHomeAddress.IsEnabled = (Records != null);
            pstrTelephoneNumber.IsEnabled = (Records != null);
            pstrInsuranceProvider.IsEnabled = (Records != null);
            pstrInsuranceAccount.IsEnabled = (Records != null);
            pstrDemographicNotes.IsEnabled = (Records != null);

            pallAllergy.IsEnabled = lbAllergies.IsEnabled && lbAllergies.SelectedIndex >= 0;
            prxOrder.IsEnabled = lbRxOrders.IsEnabled && lbRxOrders.SelectedIndex >= 0;

            lbAllergies.IsEnabled = (Records != null);
            lbRxOrders.IsEnabled = (Records != null);

            btnAddAllergy.IsEnabled = (Records != null);
            btnDelAllergy.IsEnabled = (Records != null);

            btnAddRxOrder.IsEnabled = (Records != null);
            btnDelRxOrder.IsEnabled = (Records != null);

            if (Records != null) {
                pdpSimDate.Set (Records?.CurrentTime);
                ptpSimTime.Set (Records?.CurrentTime);

                pdpDOB.Set (Records?.DOB);

                if (Records?.CodeStatus is not null)
                    penmCodeStatus.Set ((int)(Records.CodeStatus));

                pstrName.Set (Records?.Name ?? "");
                pstrMRN.Set (Records?.MRN ?? "");
                pstrSex.Set (Records?.Sex ?? "");
                pstrHomeAddress.Set (Records?.HomeAddress ?? "");
                pstrTelephoneNumber.Set (Records?.TelephoneNumber ?? "");
                pstrInsuranceProvider.Set (Records?.InsuranceProvider ?? "");
                pstrInsuranceAccount.Set (Records?.InsuranceAccount ?? "");
                pstrDemographicNotes.Set (Records?.DemographicNotes ?? "");
            }

            UpdateView_AllergyList ();
            UpdateView_RxOrderList ();
            UpdateView_RxDoseList ();

            return Task.CompletedTask;
        }

        private void UpdateView_AllergyList () {
            if (Records is null)
                return;

            ListBox lbAllergies = this.FindControl<ListBox> ("lbAllergies");

            lbAllergies.SelectionChanged -= LbAllergies_SelectionChanged;

            List<string> llbi = new ();

            SortAllergies ();

            foreach (var allergy in Records.Allergies) {
                if (App.Language is not null) {
                    llbi.Add (String.Format ("{0}: {1} ({2})",
                        allergy.Allergen,
                        allergy.Reaction,
                        App.Language.Localize (II.Scales.Intensity.LookupString (allergy.Intensity ?? II.Scales.Intensity.Values.Absent))
                        ));
                } else {
                    llbi.Add (String.Format ("{0}: {1}",
                        allergy.Allergen,
                        allergy.Reaction
                        ));
                }
            }

            lbAllergies.Items = llbi;
            if (SelectedAllergy >= 0 && SelectedAllergy < llbi.Count)
                lbAllergies.SelectedIndex = SelectedAllergy;
            else
                lbAllergies.UnselectAll ();

            lbAllergies.SelectionChanged += LbAllergies_SelectionChanged;
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
                    case PropertyString.Keys.DemographicsSex: Records.Sex = e.Value; break;
                    case PropertyString.Keys.DemographicsHomeAddress: Records.HomeAddress = e.Value; break;
                    case PropertyString.Keys.DemographicsTelephoneNumber: Records.TelephoneNumber = e.Value; break;
                    case PropertyString.Keys.DemographicsInsuranceProvider: Records.InsuranceProvider = e.Value; break;
                    case PropertyString.Keys.DemographicsInsuranceAccount: Records.InsuranceAccount = e.Value; break;
                    case PropertyString.Keys.DemographicsNotes: Records.DemographicNotes = e.Value; break;
                    case PropertyString.Keys.DoseComment: Action_SetRxDoseComment (e.Value ?? ""); break;
                }
            }
        }

        private void UpdateRecords (object? sender, PropertyAllergy.PropertyAllergyEventArgs e) {
            if (Records is null)
                return;

            ListBox lbAllergies = this.FindControl<ListBox> ("lbAllergies");

            if (SelectedAllergy >= 0 && SelectedAllergy < Records.Allergies.Count) {
                Records.Allergies [SelectedAllergy] = e.Allergy;
            }

            UpdateView_AllergyList ();
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

        private void SortAllergies () {
            string? selUUID = null;
            if (SelectedAllergy >= 0 && Records?.Allergies.Count > SelectedAllergy)
                selUUID = Records.Allergies [SelectedAllergy].UUID;

            Records?.Allergies.Sort ((a, b) => {
                return String.Compare (a.Allergen, b.Allergen);
            });

            if (selUUID is null)
                SelectedAllergy = -1;
            else
                SelectedAllergy = Records?.Allergies.FindIndex (o => o.UUID == selUUID) ?? -1;
        }

        private void Action_SelectAllergy () {
            PropertyAllergy pallAllergy = this.FindControl<PropertyAllergy> ("pallAllergy");
            ListBox lbAllergies = this.FindControl<ListBox> ("lbAllergies");

            if (Records is null || lbAllergies.SelectedIndex < 0 || lbAllergies.SelectedIndex >= Records.Allergies.Count) {
                pallAllergy.IsEnabled = false;
                pallAllergy.Init (new II.Allergy ());
                return;
            }

            pallAllergy.PropertyChanged -= UpdateRecords;
            pallAllergy.IsEnabled = true;

            SelectedAllergy = lbAllergies.SelectedIndex;
            pallAllergy.Init (Records.Allergies [SelectedAllergy]);

            pallAllergy.PropertyChanged += UpdateRecords;
        }

        private void Action_AddAllergy () {
            if (Records is null)
                return;

            II.Allergy allergy = new ();
            Records.Allergies.Add (allergy);

            UpdateView_AllergyList ();

            ListBox lbAllergies = this.FindControl<ListBox> ("lbAllergies");
            lbAllergies.SelectedIndex = Records.Allergies.FindIndex (o => o.UUID == allergy.UUID);
            SelectedAllergy = lbAllergies.SelectedIndex;

            Action_SelectAllergy ();
        }

        private void Action_DeleteAllergy () {
            if (Records is null)
                return;

            ListBox lbAllergies = this.FindControl<ListBox> ("lbAllergies");
            SelectedAllergy = lbAllergies.SelectedIndex;

            if (SelectedAllergy < 0 || SelectedAllergy >= Records.Allergies.Count)
                return;

            II.Allergy allergy = Records.Allergies [SelectedAllergy];
            Records.Allergies.RemoveAt (SelectedAllergy);

            SelectedAllergy = SelectedAllergy > -1 ? SelectedAllergy - 1 : -1;

            UpdateView_AllergyList ();
            Action_SelectAllergy ();
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

        /* Any other Routed events for this Panel */

        private void LbAllergies_SelectionChanged (object? sender, SelectionChangedEventArgs e)
            => Action_SelectAllergy ();

        private void ButtonAddAllergy_Click (object sender, RoutedEventArgs e)
            => Action_AddAllergy ();

        private void ButtonDeleteAllergy_Click (object sender, RoutedEventArgs e)
            => Action_DeleteAllergy ();

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