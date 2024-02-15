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

    public partial class PanelDemographics : RecordPanel {
        /* References for UI elements */

        public PanelDemographics () {
            InitializeComponent ();
        }

        public PanelDemographics (App? app) : base (app) {
            InitializeComponent ();

            DataContext = this;

            /* Establish reference variables */

            InitInterface ();
            PopulateInterface ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        private void InitInterface () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {Name}.{nameof (InitInterface)}");
                return;
            }

            this.FindControl<Label> ("lblName").Content = Instance.Language.Localize ("CHART:Name");
            this.FindControl<Label> ("lblMRN").Content = Instance.Language.Localize ("CHART:MedicalRecordNumber");
            this.FindControl<Label> ("lblAge").Content = Instance.Language.Localize ("CHART:Age");
            this.FindControl<Label> ("lblDOB").Content = Instance.Language.Localize ("CHART:DateOfBirth");
            this.FindControl<Label> ("lblSex").Content = Instance.Language.Localize ("CHART:Sex");
            this.FindControl<Label> ("lblAllergies").Content = Instance.Language.Localize ("CHART:Allergies");
            this.FindControl<Label> ("lblCodeStatus").Content = Instance.Language.Localize ("CHART:CodeStatus");
            this.FindControl<Label> ("lblHomeAddress").Content = Instance.Language.Localize ("CHART:HomeAddress");
            this.FindControl<Label> ("lblTelephoneNumber").Content = Instance.Language.Localize ("CHART:TelephoneNumber");
            this.FindControl<Label> ("lblInsuranceProvider").Content = Instance.Language.Localize ("CHART:InsuranceProvider");
            this.FindControl<Label> ("lblInsuranceAccount").Content = Instance.Language.Localize ("CHART:InsuranceAccountNumber");
            this.FindControl<Label> ("lblDemographicNotes").Content = Instance.Language.Localize ("CHART:Notes");
        }

        public async Task RefreshInterface () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {Name}.{nameof (RefreshInterface)}");
                return;
            }

            await PopulateInterface ();
        }

        public Task PopulateInterface () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {Name}.{nameof (RefreshInterface)}");
                return Task.CompletedTask;
            }

            StringBuilder sbAllergies = new StringBuilder ();
            foreach (var allergy in Instance?.Records?.Allergies ?? new List<Allergy> ())
                sbAllergies.AppendLine ($"{allergy.Allergen}: {allergy.Reaction} ({allergy.Intensity})");

            this.FindControl<TextBlock> ("tbName").Text = Instance?.Records?.Name;
            this.FindControl<TextBlock> ("tbMRN").Text = Instance?.Records?.MRN;
            this.FindControl<TextBlock> ("tbAge").Text = Instance?.Records?.Age?.ToString () ?? "";
            this.FindControl<TextBlock> ("tbDOB").Text = Instance?.Records?.DOB?.ToShortDateString () ?? "";
            this.FindControl<TextBlock> ("tbSex").Text = Instance?.Records?.Sex;
            this.FindControl<TextBlock> ("tbAllergies").Text = sbAllergies.ToString ();

            this.FindControl<TextBlock> ("tbCodeStatus").Text = Instance?.Language.Localize (
                $"ENUM:CodeStatuses:{Instance?.Records?.CodeStatus.ToString ()}");

            this.FindControl<TextBlock> ("tbHomeAddress").Text = Instance?.Records?.HomeAddress;
            this.FindControl<TextBlock> ("tbTelephoneNumber").Text = Instance?.Records?.TelephoneNumber;
            this.FindControl<TextBlock> ("tbInsuranceProvider").Text = Instance?.Records?.InsuranceProvider;
            this.FindControl<TextBlock> ("tbInsuranceAccount").Text = Instance?.Records?.InsuranceAccount;
            this.FindControl<TextBlock> ("tbDemographicNotes").Text = Instance?.Records?.DemographicNotes;

            return Task.CompletedTask;
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
    }
}