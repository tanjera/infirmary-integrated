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

    public partial class PanelParameters : UserControl {
        /* Pointer to main data structure for the scenario, patient, devices, etc. */
        public Scenario.Step? Step;
        public Patient? Patient;
        private WindowMain IMain;

        public PanelParameters () {
            InitializeComponent ();

            DataContext = this;

            _ = InitViewModel ();
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
            Patient = Step?.Patient;

            await UpdateViewModel ();
        }

        private Task InitViewModel () {
            // Populate enum string lists for readable display
            List<string> cardiacRhythms = new List<string> (),
                respiratoryRhythms = new List<string> (),
                pulmonaryRhythms = new List<string> (),
                cardiacAxes = new List<string> (),
                intensityScale = new List<string> (),
                fetalHeartRhythms = new List<string> ();

            if (App.Language != null) {
                foreach (Cardiac_Rhythms.Values v in Enum.GetValues (typeof (Cardiac_Rhythms.Values)))
                    cardiacRhythms.Add (App.Language.Dictionary [Cardiac_Rhythms.LookupString (v)]);

                foreach (Respiratory_Rhythms.Values v in Enum.GetValues (typeof (Respiratory_Rhythms.Values)))
                    respiratoryRhythms.Add (App.Language.Dictionary [Respiratory_Rhythms.LookupString (v)]);

                foreach (PulmonaryArtery_Rhythms.Values v in Enum.GetValues (typeof (PulmonaryArtery_Rhythms.Values)))
                    pulmonaryRhythms.Add (App.Language.Dictionary [PulmonaryArtery_Rhythms.LookupString (v)]);

                foreach (Cardiac_Axes.Values v in Enum.GetValues (typeof (Cardiac_Axes.Values)))
                    cardiacAxes.Add (App.Language.Dictionary [Cardiac_Axes.LookupString (v)]);

                foreach (Scales.Intensity.Values v in Enum.GetValues (typeof (Scales.Intensity.Values)))
                    intensityScale.Add (App.Language.Dictionary [Scales.Intensity.LookupString (v)]);

                foreach (FetalHeartRhythms.Values v in Enum.GetValues (typeof (FetalHeartRhythms.Values)))
                    fetalHeartRhythms.Add (App.Language.Dictionary [FetalHeartRhythms.LookupString (v)]);
            }

            // Find all controls and attach to reference
            PropertyBP pbpNBP = this.FindControl<PropertyBP> ("pbpNBP");
            PropertyBP pbpABP = this.FindControl<PropertyBP> ("pbpABP");
            PropertyBP pbpPBP = this.FindControl<PropertyBP> ("pbpPBP");

            PropertyCheck pchkMechanicallyVentilated = this.FindControl<PropertyCheck> ("pchkMechanicallyVentilated");
            PropertyCheck pchkPulsusParadoxus = this.FindControl<PropertyCheck> ("pchkPulsusParadoxus");
            PropertyCheck pchkPulsusAlternans = this.FindControl<PropertyCheck> ("pchkPulsusAlternans");
            PropertyCheck pchkElectricalAlternans = this.FindControl<PropertyCheck> ("pchkElectricalAlternans");

            PropertyDouble pdblT = this.FindControl<PropertyDouble> ("pdblT");
            PropertyDouble pdblCO = this.FindControl<PropertyDouble> ("pdblCO");
            PropertyDouble pdblQRSInterval = this.FindControl<PropertyDouble> ("pdblQRSInterval");
            PropertyDouble pdblQTcInterval = this.FindControl<PropertyDouble> ("pdblQTcInterval");
            PropertyDouble pdblInspiratoryRatio = this.FindControl<PropertyDouble> ("pdblInspiratoryRatio");
            PropertyDouble pdblExpiratoryRatio = this.FindControl<PropertyDouble> ("pdblExpiratoryRatio");

            PropertyECGSegment pecgSTSegment = this.FindControl<PropertyECGSegment> ("pecgSTSegment");
            PropertyECGSegment pecgTWave = this.FindControl<PropertyECGSegment> ("pecgTWave");

            PropertyEnum penmCardiacRhythms = this.FindControl<PropertyEnum> ("penmCardiacRhythms");
            PropertyEnum penmRespiratoryRhythms = this.FindControl<PropertyEnum> ("penmRespiratoryRhythms");
            PropertyEnum penmCardiacAxis = this.FindControl<PropertyEnum> ("penmCardiacAxis");
            PropertyEnum penmPACatheterRhythm = this.FindControl<PropertyEnum> ("penmPACatheterRhythm");
            PropertyEnum penmFHRVariability = this.FindControl<PropertyEnum> ("penmFHRVariability");
            PropertyEnum penmUCIntensity = this.FindControl<PropertyEnum> ("penmUCIntensity");

            PropertyList plistFetalHeartRhythm = this.FindControl<PropertyList> ("plistFetalHeartRhythm");

            PropertyInt pintHR = this.FindControl<PropertyInt> ("pintHR");
            PropertyInt pintRR = this.FindControl<PropertyInt> ("pintRR");
            PropertyInt pintSPO2 = this.FindControl<PropertyInt> ("pintSPO2");
            PropertyInt pintETCO2 = this.FindControl<PropertyInt> ("pintETCO2");
            PropertyInt pintCVP = this.FindControl<PropertyInt> ("pintCVP");
            PropertyInt pintICP = this.FindControl<PropertyInt> ("pintICP");
            PropertyInt pintIAP = this.FindControl<PropertyInt> ("pintIAP");
            PropertyInt pintPacemakerThreshold = this.FindControl<PropertyInt> ("pintPacemakerThreshold");
            PropertyInt pintFHR = this.FindControl<PropertyInt> ("pintFHR");
            PropertyInt pintUCFreq = this.FindControl<PropertyInt> ("pintUCFreq");
            PropertyInt pintUCDur = this.FindControl<PropertyInt> ("pintUCDur");

            // Initiate controls for editing Patient values
            pbpNBP.Init (PropertyBP.Keys.NSBP, 5, 0, 300, 5, 0, 200);
            pbpABP.Init (PropertyBP.Keys.ASBP, 5, 0, 300, 5, 0, 200);
            pbpPBP.Init (PropertyBP.Keys.PSP, 5, 0, 200, 5, 0, 200);

            pchkMechanicallyVentilated.Init (PropertyCheck.Keys.MechanicallyVentilated);
            pchkPulsusParadoxus.Init (PropertyCheck.Keys.PulsusParadoxus);
            pchkPulsusAlternans.Init (PropertyCheck.Keys.PulsusAlternans);
            pchkElectricalAlternans.Init (PropertyCheck.Keys.ElectricalAlternans);

            pdblT.Init (PropertyDouble.Keys.T, 0.2, 0, 100);
            pdblCO.Init (PropertyDouble.Keys.CO, 0.1, 0, 20);
            pdblQRSInterval.Init (PropertyDouble.Keys.QRSInterval, 0.02, 0.04, 0.4);
            pdblQTcInterval.Init (PropertyDouble.Keys.QTcInterval, 0.02, 0.2, 0.8);
            pdblInspiratoryRatio.Init (PropertyDouble.Keys.RRInspiratoryRatio, 0.1, 0.1, 10);
            pdblExpiratoryRatio.Init (PropertyDouble.Keys.RRExpiratoryRatio, 0.1, 0.1, 10);

            pecgSTSegment.Init (PropertyECGSegment.Keys.STElevation);
            pecgTWave.Init (PropertyECGSegment.Keys.TWave);

            penmCardiacRhythms.Init (PropertyEnum.Keys.Cardiac_Rhythms,
                Enum.GetNames (typeof (Cardiac_Rhythms.Values)), cardiacRhythms);
            penmRespiratoryRhythms.Init (PropertyEnum.Keys.Respiratory_Rhythms,
                Enum.GetNames (typeof (Respiratory_Rhythms.Values)), respiratoryRhythms);
            penmPACatheterRhythm.Init (PropertyEnum.Keys.PACatheter_Rhythms,
                Enum.GetNames (typeof (PulmonaryArtery_Rhythms.Values)), pulmonaryRhythms);
            penmCardiacAxis.Init (PropertyEnum.Keys.Cardiac_Axis,
                Enum.GetNames (typeof (Cardiac_Axes.Values)), cardiacAxes);
            penmFHRVariability.Init (PropertyEnum.Keys.FetalRhythmVariability,
                Enum.GetNames (typeof (Scales.Intensity.Values)), intensityScale);
            penmUCIntensity.Init (PropertyEnum.Keys.UterineContractionIntensity,
                Enum.GetNames (typeof (Scales.Intensity.Values)), intensityScale);
            plistFetalHeartRhythm.Init (PropertyList.Keys.FetalHeartRhythms,
                Enum.GetNames (typeof (FetalHeartRhythms.Values)), fetalHeartRhythms);

            pintHR.Init (PropertyInt.Keys.HR, 5, 0, 500);
            pintRR.Init (PropertyInt.Keys.RR, 2, 0, 100);
            pintSPO2.Init (PropertyInt.Keys.SPO2, 2, 0, 100);
            pintICP.Init (PropertyInt.Keys.ICP, 1, -100, 100);
            pintIAP.Init (PropertyInt.Keys.IAP, 1, -100, 100);
            pintPacemakerThreshold.Init (PropertyInt.Keys.PacemakerThreshold, 5, 0, 200);
            pintETCO2.Init (PropertyInt.Keys.ETCO2, 2, 0, 100);
            pintCVP.Init (PropertyInt.Keys.CVP, 1, -100, 100);
            pintFHR.Init (PropertyInt.Keys.FHR, 5, 0, 500);
            pintUCFreq.Init (PropertyInt.Keys.UCFrequency, 30, 60, 600);
            pintUCDur.Init (PropertyInt.Keys.UCDuration, 10, 30, 600);

            pbpNBP.PropertyChanged += UpdatePatient;
            pbpABP.PropertyChanged += UpdatePatient;
            pbpPBP.PropertyChanged += UpdatePatient;

            pchkMechanicallyVentilated.PropertyChanged += UpdatePatient;
            pchkPulsusParadoxus.PropertyChanged += UpdatePatient;
            pchkPulsusAlternans.PropertyChanged += UpdatePatient;
            pchkElectricalAlternans.PropertyChanged += UpdatePatient;

            pdblT.PropertyChanged += UpdatePatient;
            pdblCO.PropertyChanged += UpdatePatient;
            pdblQRSInterval.PropertyChanged += UpdatePatient;
            pdblQTcInterval.PropertyChanged += UpdatePatient;
            pdblInspiratoryRatio.PropertyChanged += UpdatePatient;
            pdblExpiratoryRatio.PropertyChanged += UpdatePatient;

            pecgSTSegment.PropertyChanged += UpdatePatient;
            pecgTWave.PropertyChanged += UpdatePatient;

            penmCardiacRhythms.PropertyChanged += UpdatePatient;
            penmRespiratoryRhythms.PropertyChanged += UpdatePatient;
            penmPACatheterRhythm.PropertyChanged += UpdatePatient;
            penmCardiacAxis.PropertyChanged += UpdatePatient;
            penmFHRVariability.PropertyChanged += UpdatePatient;
            penmUCIntensity.PropertyChanged += UpdatePatient;
            plistFetalHeartRhythm.PropertyChanged += UpdatePatient;

            penmCardiacRhythms.PropertyChanged += UpdateCardiacRhythm;
            penmRespiratoryRhythms.PropertyChanged += UpdateRespiratoryRhythm;
            penmPACatheterRhythm.PropertyChanged += UpdatePACatheterRhythm;

            pintICP.PropertyChanged += UpdatePatient;
            pintIAP.PropertyChanged += UpdatePatient;
            pintHR.PropertyChanged += UpdatePatient;
            pintRR.PropertyChanged += UpdatePatient;
            pintSPO2.PropertyChanged += UpdatePatient;
            pintETCO2.PropertyChanged += UpdatePatient;
            pintCVP.PropertyChanged += UpdatePatient;
            pintPacemakerThreshold.PropertyChanged += UpdatePatient;
            pintFHR.PropertyChanged += UpdatePatient;
            pintUCFreq.PropertyChanged += UpdatePatient;
            pintUCDur.PropertyChanged += UpdatePatient;

            return Task.CompletedTask;
        }

        private void UpdatePatient (object? sender, PropertyInt.PropertyIntEventArgs e) {
            if (Patient != null) {
                switch (e.Key) {
                    default: break;
                    case PropertyInt.Keys.HR: Patient.HR = e.Value; break;
                    case PropertyInt.Keys.RR: Patient.RR = e.Value; break;
                    case PropertyInt.Keys.ETCO2: Patient.ETCO2 = e.Value; break;
                    case PropertyInt.Keys.SPO2: Patient.SPO2 = e.Value; break;
                    case PropertyInt.Keys.CVP: Patient.CVP = e.Value; break;
                    case PropertyInt.Keys.ICP: Patient.ICP = e.Value; break;
                    case PropertyInt.Keys.IAP: Patient.IAP = e.Value; break;
                    case PropertyInt.Keys.PacemakerThreshold: Patient.Pacemaker_Threshold = e.Value; break;
                    case PropertyInt.Keys.FHR: Patient.FHR = e.Value; break;
                    case PropertyInt.Keys.UCFrequency: Patient.Contraction_Frequency = e.Value; break;
                    case PropertyInt.Keys.UCDuration: Patient.Contraction_Duration = e.Value; break;
                }
            }
        }

        private void UpdatePatient (object? sender, PropertyDouble.PropertyDoubleEventArgs e) {
            if (Patient != null) {
                switch (e.Key) {
                    default: break;
                    case PropertyDouble.Keys.T: Patient.T = e.Value ?? 0d; break;
                    case PropertyDouble.Keys.CO: Patient.CO = e.Value ?? 0d; break;
                    case PropertyDouble.Keys.QRSInterval: Patient.QRS_Interval = e.Value ?? 0d; break;
                    case PropertyDouble.Keys.QTcInterval: Patient.QTc_Interval = e.Value ?? 0d; break;
                    case PropertyDouble.Keys.RRInspiratoryRatio: Patient.RR_IE_I = e.Value ?? 0d; break;
                    case PropertyDouble.Keys.RRExpiratoryRatio: Patient.RR_IE_E = e.Value ?? 0d; break;
                }
            }
        }

        private void UpdatePatient (object? sender, PropertyBP.PropertyIntEventArgs e) {
            if (Patient != null) {
                switch (e.Key) {
                    default: break;
                    case PropertyBP.Keys.NSBP: Patient.NSBP = e.Value ?? 0; break;
                    case PropertyBP.Keys.NDBP: Patient.NDBP = e.Value ?? 0; break;
                    case PropertyBP.Keys.NMAP: Patient.NMAP = e.Value ?? 0; break;
                    case PropertyBP.Keys.ASBP: Patient.ASBP = e.Value ?? 0; break;
                    case PropertyBP.Keys.ADBP: Patient.ADBP = e.Value ?? 0; break;
                    case PropertyBP.Keys.AMAP: Patient.AMAP = e.Value ?? 0; break;
                    case PropertyBP.Keys.PSP: Patient.PSP = e.Value ?? 0; break;
                    case PropertyBP.Keys.PDP: Patient.PDP = e.Value ?? 0; break;
                    case PropertyBP.Keys.PMP: Patient.PMP = e.Value ?? 0; break;
                }
            }
        }

        private void UpdatePatient (object? sender, PropertyEnum.PropertyEnumEventArgs e) {
            if (e.Value != null && Patient != null) {
                switch (e.Key) {
                    default: break;

                    case PropertyEnum.Keys.Cardiac_Axis:
                        Patient.Cardiac_Axis.Value = (Cardiac_Axes.Values)Enum.Parse (typeof (Cardiac_Axes.Values), e.Value);
                        break;

                    case PropertyEnum.Keys.Cardiac_Rhythms:
                        Patient.Cardiac_Rhythm.Value = (Cardiac_Rhythms.Values)Enum.Parse (typeof (Cardiac_Rhythms.Values), e.Value);
                        break;

                    case PropertyEnum.Keys.Respiratory_Rhythms:
                        Patient.Respiratory_Rhythm.Value = (Respiratory_Rhythms.Values)Enum.Parse (typeof (Respiratory_Rhythms.Values), e.Value);
                        break;

                    case PropertyEnum.Keys.PACatheter_Rhythms:
                        Patient.PulmonaryArtery_Placement.Value = (PulmonaryArtery_Rhythms.Values)Enum.Parse (typeof (PulmonaryArtery_Rhythms.Values), e.Value);
                        break;

                    case PropertyEnum.Keys.FetalRhythmVariability:
                        Patient.FHR_Variability.Value = (Scales.Intensity.Values)Enum.Parse (typeof (Scales.Intensity.Values), e.Value);
                        break;

                    case PropertyEnum.Keys.UterineContractionIntensity:
                        Patient.Contraction_Intensity.Value = (Scales.Intensity.Values)Enum.Parse (typeof (Scales.Intensity.Values), e.Value);
                        break;
                }
            }
        }

        private void UpdatePatient (object? sender, PropertyList.PropertyListEventArgs e) {
            if (e.Values != null && Patient != null) {
                switch (e.Key) {
                    default: break;

                    case PropertyList.Keys.FetalHeartRhythms:
                        if (Patient.FetalHeartRhythm.ValueList is null)
                            Patient.FetalHeartRhythm.ValueList = new ();
                        else
                            Patient.FetalHeartRhythm.ValueList.Clear ();

                        foreach (string s in e.Values)
                            Patient.FetalHeartRhythm.ValueList.Add ((FetalHeartRhythms.Values)Enum.Parse (typeof (FetalHeartRhythms.Values), s));
                        break;
                }
            }
        }

        private void UpdatePatient (object? sender, PropertyCheck.PropertyCheckEventArgs e) {
            if (Patient != null) {
                switch (e.Key) {
                    default: break;
                    case PropertyCheck.Keys.PulsusParadoxus: Patient.Pulsus_Paradoxus = e.Value; break;
                    case PropertyCheck.Keys.PulsusAlternans: Patient.Pulsus_Alternans = e.Value; break;
                    case PropertyCheck.Keys.ElectricalAlternans: Patient.Electrical_Alternans = e.Value; break;
                    case PropertyCheck.Keys.MechanicallyVentilated: Patient.Mechanically_Ventilated = e.Value; break;
                }
            }
        }

        private void UpdatePatient (object? sender, PropertyECGSegment.PropertyECGEventArgs e) {
            if (Patient != null) {
                switch (e.Key) {
                    default: break;
                    case PropertyECGSegment.Keys.STElevation: Patient.ST_Elevation = e.Values ?? new double [] { 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d }; break;
                    case PropertyECGSegment.Keys.TWave: Patient.T_Elevation = e.Values ?? new double [] { 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d }; break;
                }
            }
        }

        private void UpdateCardiacRhythm (object? sender, PropertyEnum.PropertyEnumEventArgs e) {
            CheckBox chkClampVitals = this.FindControl<CheckBox> ("chkClampVitals");
            if ((!chkClampVitals.IsChecked ?? false) || e.Value == null || this.Patient == null)
                return;

            Cardiac_Rhythms.Default_Vitals v = Cardiac_Rhythms.DefaultVitals (
                (Cardiac_Rhythms.Values)Enum.Parse (typeof (Cardiac_Rhythms.Values), e.Value));

            Patient.HR = (int)II.Math.Clamp ((double)Patient.VS_Settings.HR, v.HRMin, v.HRMax);
            Patient.RR = (int)II.Math.Clamp ((double)Patient.VS_Settings.RR, v.RRMin, v.RRMax);
            Patient.SPO2 = (int)II.Math.Clamp ((double)Patient.VS_Settings.SPO2, v.SPO2Min, v.SPO2Max);
            Patient.ETCO2 = (int)II.Math.Clamp ((double)Patient.VS_Settings.ETCO2, v.ETCO2Min, v.ETCO2Max);
            Patient.NSBP = (int)II.Math.Clamp ((double)Patient.VS_Settings.NSBP, v.SBPMin, v.SBPMax);
            Patient.NDBP = (int)II.Math.Clamp ((double)Patient.VS_Settings.NDBP, v.DBPMin, v.DBPMax);
            Patient.ASBP = (int)II.Math.Clamp ((double)Patient.VS_Settings.ASBP, v.SBPMin, v.SBPMax);
            Patient.ADBP = (int)II.Math.Clamp ((double)Patient.VS_Settings.ADBP, v.DBPMin, v.DBPMax);
            Patient.PSP = (int)II.Math.Clamp ((double)Patient.VS_Settings.PSP, v.PSPMin, v.PSPMax);
            Patient.PDP = (int)II.Math.Clamp ((double)Patient.VS_Settings.PDP, v.PDPMin, v.PDPMax);
            Patient.QRS_Interval = (double)II.Math.Clamp ((double)Patient.QRS_Interval, v.QRSIntervalMin, v.QRSIntervalMax);
            Patient.QTc_Interval = (double)II.Math.Clamp ((double)Patient.QTc_Interval, v.QTCIntervalMin, v.QTCIntervalMax);

            _ = UpdateViewModel ();
        }

        private void UpdateRespiratoryRhythm (object? sender, PropertyEnum.PropertyEnumEventArgs e) {
            CheckBox chkClampVitals = this.FindControl<CheckBox> ("chkClampVitals");

            if ((!chkClampVitals.IsChecked ?? false) || e.Value == null || Patient == null)
                return;

            Respiratory_Rhythms.Default_Vitals v = Respiratory_Rhythms.DefaultVitals (
                (Respiratory_Rhythms.Values)Enum.Parse (typeof (Respiratory_Rhythms.Values), e.Value));

            Patient.RR = (int)II.Math.Clamp ((double)Patient.RR, v.RRMin, v.RRMax);
            Patient.RR_IE_I = (int)II.Math.Clamp ((double)Patient.RR_IE_I, v.RR_IE_I_Min, v.RR_IE_I_Max);
            Patient.RR_IE_E = (int)II.Math.Clamp ((double)Patient.RR_IE_E, v.RR_IE_E_Min, v.RR_IE_E_Max);

            _ = UpdateViewModel ();
        }

        private void UpdatePACatheterRhythm (object? sender, PropertyEnum.PropertyEnumEventArgs e) {
            if (e.Value == null || Patient == null)
                return;

            PulmonaryArtery_Rhythms.Default_Vitals v = PulmonaryArtery_Rhythms.DefaultVitals (
                (PulmonaryArtery_Rhythms.Values)Enum.Parse (typeof (PulmonaryArtery_Rhythms.Values), e.Value));

            Patient.PSP = (int)II.Math.Clamp ((double)Patient.PSP, v.PSPMin, v.PSPMax);
            Patient.PDP = (int)II.Math.Clamp ((double)Patient.PDP, v.PDPMin, v.PDPMax);

            _ = UpdateViewModel ();
        }

        private async Task UpdateViewModel () {
            Label lblActiveStep = this.FindControl<Label> ("lblActiveStep");

            CheckBox chkClampVitals = this.FindControl<CheckBox> ("chkClampVitals");

            PropertyBP pbpNBP = this.FindControl<PropertyBP> ("pbpNBP");
            PropertyBP pbpABP = this.FindControl<PropertyBP> ("pbpABP");
            PropertyBP pbpPBP = this.FindControl<PropertyBP> ("pbpPBP");

            PropertyCheck pchkMechanicallyVentilated = this.FindControl<PropertyCheck> ("pchkMechanicallyVentilated");
            PropertyCheck pchkPulsusParadoxus = this.FindControl<PropertyCheck> ("pchkPulsusParadoxus");
            PropertyCheck pchkPulsusAlternans = this.FindControl<PropertyCheck> ("pchkPulsusAlternans");
            PropertyCheck pchkElectricalAlternans = this.FindControl<PropertyCheck> ("pchkElectricalAlternans");

            PropertyDouble pdblT = this.FindControl<PropertyDouble> ("pdblT");
            PropertyDouble pdblCO = this.FindControl<PropertyDouble> ("pdblCO");
            PropertyDouble pdblQRSInterval = this.FindControl<PropertyDouble> ("pdblQRSInterval");
            PropertyDouble pdblQTcInterval = this.FindControl<PropertyDouble> ("pdblQTcInterval");
            PropertyDouble pdblInspiratoryRatio = this.FindControl<PropertyDouble> ("pdblInspiratoryRatio");
            PropertyDouble pdblExpiratoryRatio = this.FindControl<PropertyDouble> ("pdblExpiratoryRatio");

            PropertyECGSegment pecgSTSegment = this.FindControl<PropertyECGSegment> ("pecgSTSegment");
            PropertyECGSegment pecgTWave = this.FindControl<PropertyECGSegment> ("pecgTWave");

            PropertyEnum penmCardiacRhythms = this.FindControl<PropertyEnum> ("penmCardiacRhythms");
            PropertyEnum penmRespiratoryRhythms = this.FindControl<PropertyEnum> ("penmRespiratoryRhythms");
            PropertyEnum penmCardiacAxis = this.FindControl<PropertyEnum> ("penmCardiacAxis");
            PropertyEnum penmPACatheterRhythm = this.FindControl<PropertyEnum> ("penmPACatheterRhythm");
            PropertyEnum penmFHRVariability = this.FindControl<PropertyEnum> ("penmFHRVariability");
            PropertyEnum penmUCIntensity = this.FindControl<PropertyEnum> ("penmUCIntensity");

            PropertyList plistFetalHeartRhythm = this.FindControl<PropertyList> ("plistFetalHeartRhythm");

            PropertyInt pintHR = this.FindControl<PropertyInt> ("pintHR");
            PropertyInt pintRR = this.FindControl<PropertyInt> ("pintRR");
            PropertyInt pintSPO2 = this.FindControl<PropertyInt> ("pintSPO2");
            PropertyInt pintETCO2 = this.FindControl<PropertyInt> ("pintETCO2");
            PropertyInt pintCVP = this.FindControl<PropertyInt> ("pintCVP");
            PropertyInt pintICP = this.FindControl<PropertyInt> ("pintICP");
            PropertyInt pintIAP = this.FindControl<PropertyInt> ("pintIAP");
            PropertyInt pintPacemakerThreshold = this.FindControl<PropertyInt> ("pintPacemakerThreshold");
            PropertyInt pintFHR = this.FindControl<PropertyInt> ("pintFHR");
            PropertyInt pintUCFreq = this.FindControl<PropertyInt> ("pintUCFreq");
            PropertyInt pintUCDur = this.FindControl<PropertyInt> ("pintUCDur");

            lblActiveStep.Content = String.Format ("Editing Step: {0} ({1})",
                Step is null ? "N/A" : Step.Name,
                Step is null ? "N/A" : Step.Description);

            // Enable/Disable controls based on if Patient is null!
            chkClampVitals.IsEnabled = (Patient != null);

            pbpNBP.IsEnabled = (Patient != null);
            pbpABP.IsEnabled = (Patient != null);
            pbpPBP.IsEnabled = (Patient != null);

            pchkMechanicallyVentilated.IsEnabled = (Patient != null);
            pchkPulsusParadoxus.IsEnabled = (Patient != null);
            pchkPulsusAlternans.IsEnabled = (Patient != null);
            pchkElectricalAlternans.IsEnabled = (Patient != null);

            pdblT.IsEnabled = (Patient != null);
            pdblCO.IsEnabled = (Patient != null);
            pdblQRSInterval.IsEnabled = (Patient != null);
            pdblQTcInterval.IsEnabled = (Patient != null);
            pdblInspiratoryRatio.IsEnabled = (Patient != null);
            pdblExpiratoryRatio.IsEnabled = (Patient != null);

            pecgSTSegment.IsEnabled = (Patient != null);
            pecgTWave.IsEnabled = (Patient != null);

            penmCardiacRhythms.IsEnabled = (Patient != null);
            penmRespiratoryRhythms.IsEnabled = (Patient != null);
            penmPACatheterRhythm.IsEnabled = (Patient != null);
            penmCardiacAxis.IsEnabled = (Patient != null);
            penmFHRVariability.IsEnabled = (Patient != null);
            penmUCIntensity.IsEnabled = (Patient != null);
            plistFetalHeartRhythm.IsEnabled = (Patient != null);

            pintHR.IsEnabled = (Patient != null);
            pintRR.IsEnabled = (Patient != null);
            pintSPO2.IsEnabled = (Patient != null);
            pintETCO2.IsEnabled = (Patient != null);
            pintCVP.IsEnabled = (Patient != null);
            pintICP.IsEnabled = (Patient != null);
            pintIAP.IsEnabled = (Patient != null);
            pintPacemakerThreshold.IsEnabled = (Patient != null);
            pintFHR.IsEnabled = (Patient != null);
            pintUCFreq.IsEnabled = (Patient != null);
            pintUCDur.IsEnabled = (Patient != null);

            if (Patient != null) {
                // Update all controls with Patient values
                await pbpNBP.Set (Patient.VS_Settings.NSBP, Patient.VS_Settings.NDBP);
                await pbpABP.Set (Patient.VS_Settings.ASBP, Patient.VS_Settings.ADBP);
                await pbpPBP.Set (Patient.VS_Settings.PSP, Patient.VS_Settings.PDP);

                await pchkMechanicallyVentilated.Set (Patient.Mechanically_Ventilated);
                await pchkPulsusParadoxus.Set (Patient.Pulsus_Paradoxus);
                await pchkPulsusAlternans.Set (Patient.Pulsus_Alternans);
                await pchkElectricalAlternans.Set (Patient.Electrical_Alternans);

                await pdblT.Set (Patient.VS_Settings.T);
                await pdblCO.Set (Patient.VS_Settings.CO);
                await pdblQRSInterval.Set (Patient.QRS_Interval);
                await pdblQTcInterval.Set (Patient.QTc_Interval);
                await pdblInspiratoryRatio.Set (Patient.VS_Settings.RR_IE_I);
                await pdblExpiratoryRatio.Set (Patient.VS_Settings.RR_IE_E);

                await pecgSTSegment.Set (Patient.ST_Elevation);
                await pecgTWave.Set (Patient.T_Elevation);

                await penmCardiacRhythms.Set ((int)Patient.Cardiac_Rhythm.Value);
                await penmRespiratoryRhythms.Set ((int)Patient.Respiratory_Rhythm.Value);
                await penmPACatheterRhythm.Set ((int)Patient.PulmonaryArtery_Placement.Value);
                await penmCardiacAxis.Set ((int)Patient.Cardiac_Axis.Value);
                await penmFHRVariability.Set ((int)Patient.FHR_Variability.Value);
                await penmUCIntensity.Set ((int)Patient.Contraction_Intensity.Value);
                await plistFetalHeartRhythm.Set (new List<string> (Patient.FetalHeartRhythm.ValueList.Select (s => s.ToString ())));

                await pintHR.Set (Patient.VS_Settings.HR);
                await pintRR.Set (Patient.VS_Settings.RR);
                await pintSPO2.Set (Patient.VS_Settings.SPO2);
                await pintETCO2.Set (Patient.VS_Settings.ETCO2);
                await pintCVP.Set (Patient.VS_Settings.CVP);
                await pintICP.Set (Patient.VS_Settings.ICP);
                await pintIAP.Set (Patient.VS_Settings.IAP);
                await pintPacemakerThreshold.Set (Patient.Pacemaker_Threshold);
                await pintFHR.Set (Patient.FHR);
                await pintUCFreq.Set (Patient.Contraction_Frequency);
                await pintUCDur.Set (Patient.Contraction_Duration);
            }
        }

        /* Generic Menu Items (across all Panels) */

        private void MenuFileNew_Click (object sender, RoutedEventArgs e)
            => IMain.MenuFileNew_Click (sender, e);

        private void MenuFileLoad_Click (object sender, RoutedEventArgs e)
            => IMain.MenuFileLoad_Click (sender, e);

        private void MenuFileSave_Click (object sender, RoutedEventArgs e)
            => IMain.MenuFileSave_Click (sender, e);

        private void MenuFileSaveAs_Click (object sender, RoutedEventArgs e)
            => IMain.MenuFileSaveAs_Click (sender, e);

        private void MenuFileExit_Click (object sender, RoutedEventArgs e)
            => IMain.MenuFileExit_Click (sender, e);

        private void MenuHelpAbout_Click (object sender, RoutedEventArgs e)
            => IMain.MenuHelpAbout_Click (sender, e);
    }
}