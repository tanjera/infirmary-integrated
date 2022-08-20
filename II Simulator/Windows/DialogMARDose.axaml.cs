using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;

using II;

namespace IISIM {

    public partial class DialogMARDose : Window {
        public App? Instance;

        private Medication.Order? RxOrder;
        private Medication.Dose? RxDose;

        private bool updateDose = false;

        public DialogMARDose () {
            InitializeComponent ();
        }

        public DialogMARDose (App? app, Medication.Order? rxOrder, Medication.Dose? rxDose) {
            InitializeComponent ();
#if DEBUG
            this.AttachDevTools ();
#endif

            DataContext = this;
            Instance = app;

            RxOrder = rxOrder;
            RxDose = rxDose;

            InitInterface ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        private void InitInterface () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (InitInterface)}");
                return;
            }

            /* Color-coding */
            SolidColorBrush colorScheduled = new (Avalonia.Media.Color.Parse ("#D1FFFC"));
            SolidColorBrush colorPRN = new (Avalonia.Media.Color.Parse ("#DEFFD1"));

            /* Populate UI strings per language selection */

            this.FindControl<Window> ("dlgMARDose").Title = Instance.Language.Localize ("PE:MAR");

            this.FindControl<Button> ("btnCancel").Content = Instance.Language.Localize ("BUTTON:Cancel");
            this.FindControl<Button> ("btnContinue").Content = Instance.Language.Localize ("BUTTON:Continue");
            this.FindControl<Label> ("lblStatus").Content = Instance.Language.Localize ("MAR:Status");
            this.FindControl<Label> ("lblComment").Content = Instance.Language.Localize ("MAR:Comment");

            List<ComboBoxItem> statuses = new ();

            statuses.Add (new ComboBoxItem () {
                Tag = true,
                Content = Instance.Language.Localize ("ENUM:AdministrationStatuses:Administered")
            });

            statuses.Add (new ComboBoxItem () {
                Tag = false,
                Content = Instance.Language.Localize ("ENUM:AdministrationStatuses:NotAdministered")
            });

            ComboBox cmbStatus = this.FindControl<ComboBox> ("cmbStatus");
            cmbStatus.Items = statuses;
            cmbStatus.SelectedIndex = (RxDose?.Administered ?? false) ? 0 : 1;

            this.FindControl<TextBox> ("txtComment").Text = RxDose?.Comment ?? "";

            if (RxDose is not null && RxOrder is not null) {
                TextBlock tbDose = this.FindControl<TextBlock> ("tbDose");

                tbDose.Text =
                    $"{RxOrder.DrugName}\n"
                    + $"{RxOrder.DoseAmount} "
                    + $"{Instance.Language.Localize (Medication.Order.DoseUnits.LookupString (RxOrder.DoseUnit ?? Medication.Order.DoseUnits.Values.L))} "
                    + $"{Instance.Language.Localize (Medication.Order.Routes.LookupString (RxOrder.Route ?? Medication.Order.Routes.Values.IV))}\n"
                    + $"{(RxOrder.PeriodType == Medication.Order.PeriodTypes.Values.PRN ? $"{Instance.Language.Localize ("ENUM:PeriodTypes:PRN")}\n" : "")}"
                    + $"{Instance.Language.Localize (Medication.Order.Priorities.LookupString (RxOrder.Priority ?? Medication.Order.Priorities.Values.Routine))}\n"
                    + $"{(RxDose.ScheduledTime ?? new DateTime ()).ToShortDateString ()} {(RxDose.ScheduledTime ?? new DateTime ()).ToString ("HH:mm")}\n"
                    + (RxDose.Administered ? $"\n{Instance.Language.Localize ("ENUM:AdministrationStatuses:Administered")}" : "")
                    + (!String.IsNullOrEmpty (RxDose.Comment) ? $"\n{RxDose.Comment}" : "");

                tbDose.Background = (RxOrder?.IsScheduled ?? true) ? colorScheduled : colorPRN;
            }
        }

        public async Task<Medication.Dose?> AsyncShow (Window parent) {
            if (RxDose is null)
                return null;

            if (!parent.IsVisible)                    // Avalonia's parent must be visible to attach a window
                parent.Show ();

            this.Activate ();
            await this.ShowDialog (parent);

            if (updateDose) {
                RxDose.Comment = this.FindControl<TextBox> ("txtComment").Text;

                ComboBox cmbStatus = this.FindControl<ComboBox> ("cmbStatus");
                if (cmbStatus.SelectedItem is not null && cmbStatus.SelectedItem is ComboBoxItem
                    && ((ComboBoxItem)cmbStatus.SelectedItem).Tag is bool) {
                    RxDose.Administered = (bool?)(((ComboBoxItem)cmbStatus.SelectedItem).Tag) ?? false;
                }
            }

            return RxDose;
        }

        public void btnContinue_Click (object sender, RoutedEventArgs e) {
            updateDose = true;
            this.Close ();
        }

        public void btnCancel_Click (object sender, RoutedEventArgs e) {
            updateDose = false;
            this.Close ();
        }
    }
}