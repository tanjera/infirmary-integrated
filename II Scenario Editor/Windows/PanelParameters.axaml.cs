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
using II.Localization;

using IISE.Controls;

namespace IISE.Windows {

    public partial class PanelParameters : UserControl {
        public App? Instance;
        
        /* Pointer to main data structure for the scenario, patient, devices, etc. */
        public Scenario.Step? Step;
        public Physiology? Physiology;

        public PanelParameters (App? app) {
            Instance = app;
            
            InitializeComponent ();

            DataContext = this;

            _ = InitView ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public async Task SetStep (Scenario.Step? step) {
            Step = step;
            Physiology = Step?.Physiology;

            await UpdateView ();
        }

        private Task InitView () {
            // Populate enum string lists for readable display
            List<string> cardiacRhythms = new List<string> (),
                respiratoryRhythms = new List<string> (),
                pulmonaryRhythms = new List<string> (),
                cardiacAxes = new List<string> (),
                fetalHeartRhythms = new List<string> ();

            if (Instance?.Language is not null) {
                foreach (CardiacRhythms.Values v in Enum.GetValues (typeof (CardiacRhythms.Values)))
                    cardiacRhythms.Add (Language.Localize(Language.Languages.ENG, CardiacRhythms.LookupString (v)) ?? "");

                foreach (Respiratory_Rhythms.Values v in Enum.GetValues (typeof (Respiratory_Rhythms.Values)))
                    respiratoryRhythms.Add (Language.Localize(Language.Languages.ENG, Respiratory_Rhythms.LookupString (v)) ?? "");

                foreach (PulmonaryArteryRhythms.Values v in Enum.GetValues (typeof (PulmonaryArteryRhythms.Values)))
                    pulmonaryRhythms.Add (Language.Localize(Language.Languages.ENG, PulmonaryArteryRhythms.LookupString (v)) ?? "");

                foreach (Cardiac_Axes.Values v in Enum.GetValues (typeof (Cardiac_Axes.Values)))
                    cardiacAxes.Add (Language.Localize(Language.Languages.ENG, Cardiac_Axes.LookupString (v)) ?? "");

                foreach (FetalHeart_Rhythms.Values v in Enum.GetValues (typeof (FetalHeart_Rhythms.Values)))
                    fetalHeartRhythms.Add (Language.Localize(Language.Languages.ENG, FetalHeart_Rhythms.LookupString (v)) ?? "");
            }

            // Find all controls and attach to reference
            PropertyBP pbpNBP = this.GetControl<PropertyBP> ("pbpNBP");
            PropertyBP pbpABP = this.GetControl<PropertyBP> ("pbpABP");
            PropertyBP pbpPBP = this.GetControl<PropertyBP> ("pbpPBP");

            PropertyCheck pchkMechanicallyVentilated = this.GetControl<PropertyCheck> ("pchkMechanicallyVentilated");
            PropertyCheck pchkPulsusParadoxus = this.GetControl<PropertyCheck> ("pchkPulsusParadoxus");
            PropertyCheck pchkPulsusAlternans = this.GetControl<PropertyCheck> ("pchkPulsusAlternans");
            PropertyCheck pchkElectricalAlternans = this.GetControl<PropertyCheck> ("pchkElectricalAlternans");

            PropertyDouble pdblT = this.GetControl<PropertyDouble> ("pdblT");
            PropertyDouble pdblCO = this.GetControl<PropertyDouble> ("pdblCO");
            PropertyDouble pdblQRSInterval = this.GetControl<PropertyDouble> ("pdblQRSInterval");
            PropertyDouble pdblQTcInterval = this.GetControl<PropertyDouble> ("pdblQTcInterval");
            PropertyDouble pdblInspiratoryRatio = this.GetControl<PropertyDouble> ("pdblInspiratoryRatio");
            PropertyDouble pdblExpiratoryRatio = this.GetControl<PropertyDouble> ("pdblExpiratoryRatio");

            PropertyECGAmplitude pecgPAmplitude = this.GetControl<PropertyECGAmplitude> ("pecgPAmplitude");
            PropertyECGAmplitude pecgQAmplitude = this.GetControl<PropertyECGAmplitude> ("pecgQAmplitude");
            PropertyECGAmplitude pecgRAmplitude = this.GetControl<PropertyECGAmplitude> ("pecgRAmplitude");
            PropertyECGAmplitude pecgSAmplitude = this.GetControl<PropertyECGAmplitude> ("pecgSAmplitude");
            PropertyECGAmplitude pecgTAmplitude = this.GetControl<PropertyECGAmplitude> ("pecgTAmplitude");
            
            PropertyECGSegment pecgSTSegment = this.GetControl<PropertyECGSegment> ("pecgSTSegment");

            PropertyEnum penmCardiacRhythms = this.GetControl<PropertyEnum> ("penmCardiacRhythms");
            PropertyEnum penmRespiratoryRhythms = this.GetControl<PropertyEnum> ("penmRespiratoryRhythms");
            PropertyEnum penmCardiacAxis = this.GetControl<PropertyEnum> ("penmCardiacAxis");
            PropertyEnum penmPACatheterRhythm = this.GetControl<PropertyEnum> ("penmPACatheterRhythm");
            PropertyEnum penmFetalHeartRhythm = this.GetControl<PropertyEnum> ("penmFetalHeartRhythm");

            PropertyInt pintHR = this.GetControl<PropertyInt> ("pintHR");
            PropertyInt pintRR = this.GetControl<PropertyInt> ("pintRR");
            PropertyInt pintSPO2 = this.GetControl<PropertyInt> ("pintSPO2");
            PropertyInt pintETCO2 = this.GetControl<PropertyInt> ("pintETCO2");
            PropertyInt pintCVP = this.GetControl<PropertyInt> ("pintCVP");
            PropertyInt pintICP = this.GetControl<PropertyInt> ("pintICP");
            PropertyInt pintIAP = this.GetControl<PropertyInt> ("pintIAP");
            PropertyInt pintPacemakerThreshold = this.GetControl<PropertyInt> ("pintPacemakerThreshold");
            PropertyInt pintFHR = this.GetControl<PropertyInt> ("pintFHR");
            PropertyInt pintFHRVariability = this.GetControl<PropertyInt> ("pintFHRVariability");
            PropertyInt pintUCFreq = this.GetControl<PropertyInt> ("pintUCFreq");
            PropertyInt pintUCDur = this.GetControl<PropertyInt> ("pintUCDur");
            PropertyInt pintUCIntensity = this.GetControl<PropertyInt> ("pintUCIntensity");
            PropertyInt pintUCResting = this.GetControl<PropertyInt> ("pintUCResting");

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

            pecgPAmplitude.Init (PropertyECGAmplitude.Keys.PAmplitude);
            pecgQAmplitude.Init (PropertyECGAmplitude.Keys.QAmplitude);
            pecgRAmplitude.Init (PropertyECGAmplitude.Keys.RAmplitude);
            pecgSAmplitude.Init (PropertyECGAmplitude.Keys.SAmplitude);
            pecgTAmplitude.Init (PropertyECGAmplitude.Keys.TAmplitude);
            
            pecgSTSegment.Init (PropertyECGSegment.Keys.STElevation);

            penmCardiacRhythms.Init (PropertyEnum.Keys.Cardiac_Rhythms,
                Enum.GetNames (typeof (CardiacRhythms.Values)), cardiacRhythms);
            penmRespiratoryRhythms.Init (PropertyEnum.Keys.Respiratory_Rhythms,
                Enum.GetNames (typeof (Respiratory_Rhythms.Values)), respiratoryRhythms);
            penmPACatheterRhythm.Init (PropertyEnum.Keys.PACatheter_Rhythms,
                Enum.GetNames (typeof (PulmonaryArteryRhythms.Values)), pulmonaryRhythms);
            penmCardiacAxis.Init (PropertyEnum.Keys.Cardiac_Axis,
                Enum.GetNames (typeof (Cardiac_Axes.Values)), cardiacAxes);
            penmFetalHeartRhythm.Init (PropertyEnum.Keys.FetalHeart_Rhythms,
                Enum.GetNames (typeof (FetalHeart_Rhythms.Values)), fetalHeartRhythms);

            pintHR.Init (PropertyInt.Keys.HR, 5, 0, 500);
            pintRR.Init (PropertyInt.Keys.RR, 2, 0, 100);
            pintSPO2.Init (PropertyInt.Keys.SPO2, 2, 0, 100);
            pintICP.Init (PropertyInt.Keys.ICP, 1, -100, 100);
            pintIAP.Init (PropertyInt.Keys.IAP, 1, -100, 100);
            pintPacemakerThreshold.Init (PropertyInt.Keys.PacemakerThreshold, 5, 0, 200);
            pintETCO2.Init (PropertyInt.Keys.ETCO2, 2, 0, 100);
            pintCVP.Init (PropertyInt.Keys.CVP, 1, -100, 100);
            pintFHR.Init (PropertyInt.Keys.FHR, 5, 0, 500);
            pintFHRVariability.Init (PropertyInt.Keys.ObstetricFHRVariability, 5, 0, 80);
            pintUCFreq.Init (PropertyInt.Keys.ObstetricContractionFrequency, 30, 60, 600);
            pintUCDur.Init (PropertyInt.Keys.ObstetricContractionDuration, 10, 30, 600);
            pintUCIntensity.Init (PropertyInt.Keys.ObstetricContractionIntensity, 5, 0, 100);
            pintUCResting.Init (PropertyInt.Keys.ObstetricUterineRestingTone, 5, 0, 100);

            pbpNBP.PropertyChanged += UpdatePhysiology;
            pbpABP.PropertyChanged += UpdatePhysiology;
            pbpPBP.PropertyChanged += UpdatePhysiology;

            pchkMechanicallyVentilated.PropertyChanged += UpdatePhysiology;
            pchkPulsusParadoxus.PropertyChanged += UpdatePhysiology;
            pchkPulsusAlternans.PropertyChanged += UpdatePhysiology;
            pchkElectricalAlternans.PropertyChanged += UpdatePhysiology;

            pdblT.PropertyChanged += UpdatePhysiology;
            pdblCO.PropertyChanged += UpdatePhysiology;
            pdblQRSInterval.PropertyChanged += UpdatePhysiology;
            pdblQTcInterval.PropertyChanged += UpdatePhysiology;
            pdblInspiratoryRatio.PropertyChanged += UpdatePhysiology;
            pdblExpiratoryRatio.PropertyChanged += UpdatePhysiology;
            
            pecgPAmplitude.PropertyChanged += UpdatePhysiology;
            pecgQAmplitude.PropertyChanged += UpdatePhysiology;
            pecgRAmplitude.PropertyChanged += UpdatePhysiology;
            pecgSAmplitude.PropertyChanged += UpdatePhysiology;
            pecgTAmplitude.PropertyChanged += UpdatePhysiology;
            
            pecgSTSegment.PropertyChanged += UpdatePhysiology;

            penmCardiacRhythms.PropertyChanged += UpdatePhysiology;
            penmRespiratoryRhythms.PropertyChanged += UpdatePhysiology;
            penmPACatheterRhythm.PropertyChanged += UpdatePhysiology;
            penmCardiacAxis.PropertyChanged += UpdatePhysiology;
            penmFetalHeartRhythm.PropertyChanged += UpdatePhysiology;

            penmCardiacRhythms.PropertyChanged += UpdateCardiacRhythm;
            penmRespiratoryRhythms.PropertyChanged += UpdateRespiratoryRhythm;
            penmPACatheterRhythm.PropertyChanged += UpdatePACatheterRhythm;

            pintICP.PropertyChanged += UpdatePhysiology;
            pintIAP.PropertyChanged += UpdatePhysiology;
            pintHR.PropertyChanged += UpdatePhysiology;
            pintRR.PropertyChanged += UpdatePhysiology;
            pintSPO2.PropertyChanged += UpdatePhysiology;
            pintETCO2.PropertyChanged += UpdatePhysiology;
            pintCVP.PropertyChanged += UpdatePhysiology;
            pintPacemakerThreshold.PropertyChanged += UpdatePhysiology;
            pintFHR.PropertyChanged += UpdatePhysiology;
            pintFHRVariability.PropertyChanged += UpdatePhysiology;
            pintUCFreq.PropertyChanged += UpdatePhysiology;
            pintUCDur.PropertyChanged += UpdatePhysiology;
            pintUCIntensity.PropertyChanged += UpdatePhysiology;
            pintUCResting.PropertyChanged += UpdatePhysiology;

            return Task.CompletedTask;
        }

        private void UpdatePhysiology (object? sender, PropertyInt.PropertyIntEventArgs e) {
            if (Physiology != null) {
                switch (e.Key) {
                    default: break;
                    case PropertyInt.Keys.HR: Physiology.HR = e.Value; break;
                    case PropertyInt.Keys.RR: Physiology.RR = e.Value; break;
                    case PropertyInt.Keys.ETCO2: Physiology.ETCO2 = e.Value; break;
                    case PropertyInt.Keys.SPO2: Physiology.SPO2 = e.Value; break;
                    case PropertyInt.Keys.CVP: Physiology.CVP = e.Value; break;
                    case PropertyInt.Keys.ICP: Physiology.ICP = e.Value; break;
                    case PropertyInt.Keys.IAP: Physiology.IAP = e.Value; break;
                    case PropertyInt.Keys.PacemakerThreshold: Physiology.PacemakerThreshold = e.Value; break;
                    case PropertyInt.Keys.FHR: Physiology.FetalHR = e.Value; break;
                    case PropertyInt.Keys.ObstetricFHRVariability: Physiology.ObstetricFetalRateVariability = e.Value; break;
                    case PropertyInt.Keys.ObstetricContractionFrequency: Physiology.ObstetricContractionFrequency = e.Value; break;
                    case PropertyInt.Keys.ObstetricContractionDuration: Physiology.ObstetricContractionDuration = e.Value; break;
                    case PropertyInt.Keys.ObstetricContractionIntensity: Physiology.ObstetricContractionIntensity = e.Value; break;
                    case PropertyInt.Keys.ObstetricUterineRestingTone: Physiology.ObstetricUterineRestingTone = e.Value; break;
                }
            }
        }

        private void UpdatePhysiology (object? sender, PropertyDouble.PropertyDoubleEventArgs e) {
            if (Physiology != null) {
                switch (e.Key) {
                    default: break;
                    case PropertyDouble.Keys.T: Physiology.T = e.Value ?? 0d; break;
                    case PropertyDouble.Keys.CO: Physiology.CO = e.Value ?? 0d; break;
                    case PropertyDouble.Keys.QRSInterval: Physiology.QRSInterval = e.Value ?? 0d; break;
                    case PropertyDouble.Keys.QTcInterval: Physiology.QTcInterval = e.Value ?? 0d; break;
                    case PropertyDouble.Keys.RRInspiratoryRatio: Physiology.RRIEI = e.Value ?? 0d; break;
                    case PropertyDouble.Keys.RRExpiratoryRatio: Physiology.RRIEE = e.Value ?? 0d; break;
                }
            }
        }

        private void UpdatePhysiology (object? sender, PropertyBP.PropertyIntEventArgs e) {
            if (Physiology != null) {
                switch (e.Key) {
                    default: break;
                    case PropertyBP.Keys.NSBP: Physiology.NSBP = e.Value ?? 0; break;
                    case PropertyBP.Keys.NDBP: Physiology.NDBP = e.Value ?? 0; break;
                    case PropertyBP.Keys.NMAP: Physiology.NMAP = e.Value ?? 0; break;
                    case PropertyBP.Keys.ASBP: Physiology.ASBP = e.Value ?? 0; break;
                    case PropertyBP.Keys.ADBP: Physiology.ADBP = e.Value ?? 0; break;
                    case PropertyBP.Keys.AMAP: Physiology.AMAP = e.Value ?? 0; break;
                    case PropertyBP.Keys.PSP: Physiology.PSP = e.Value ?? 0; break;
                    case PropertyBP.Keys.PDP: Physiology.PDP = e.Value ?? 0; break;
                    case PropertyBP.Keys.PMP: Physiology.PMP = e.Value ?? 0; break;
                }
            }
        }

        private void UpdatePhysiology (object? sender, PropertyEnum.PropertyEnumEventArgs e) {
            if (e.Value != null && Physiology != null) {
                switch (e.Key) {
                    default: break;

                    case PropertyEnum.Keys.Cardiac_Axis:
                        Physiology.CardiacAxis.Value = (Cardiac_Axes.Values)Enum.Parse (typeof (Cardiac_Axes.Values), e.Value);
                        break;

                    case PropertyEnum.Keys.Cardiac_Rhythms:
                        Physiology.CardiacRhythm.Value = (CardiacRhythms.Values)Enum.Parse (typeof (CardiacRhythms.Values), e.Value);
                        break;

                    case PropertyEnum.Keys.Respiratory_Rhythms:
                        Physiology.RespiratoryRhythm.Value = (Respiratory_Rhythms.Values)Enum.Parse (typeof (Respiratory_Rhythms.Values), e.Value);
                        break;

                    case PropertyEnum.Keys.PACatheter_Rhythms:
                        Physiology.PulmonaryArteryPlacement.Value = (PulmonaryArteryRhythms.Values)Enum.Parse (typeof (PulmonaryArteryRhythms.Values), e.Value);
                        break;

                    case PropertyEnum.Keys.FetalHeart_Rhythms:
                        Physiology.ObstetricFetalHeartRhythm.Value = (FetalHeart_Rhythms.Values)Enum.Parse (typeof (FetalHeart_Rhythms.Values), e.Value);
                        break;
                }
            }
        }

        private void UpdatePhysiology (object? sender, PropertyList.PropertyListEventArgs e) {
            if (e.Values != null && Physiology != null) {
                switch (e.Key) {
                    default: break;
                }
            }
        }

        private void UpdatePhysiology (object? sender, PropertyCheck.PropertyCheckEventArgs e) {
            if (Physiology != null) {
                switch (e.Key) {
                    default: break;
                    case PropertyCheck.Keys.PulsusParadoxus: Physiology.PulsusParadoxus = e.Value; break;
                    case PropertyCheck.Keys.PulsusAlternans: Physiology.PulsusAlternans = e.Value; break;
                    case PropertyCheck.Keys.ElectricalAlternans: Physiology.ElectricalAlternans = e.Value; break;
                    case PropertyCheck.Keys.MechanicallyVentilated: Physiology.MechanicallyVentilated = e.Value; break;
                }
            }
        }

        private void UpdatePhysiology (object? sender, PropertyECGAmplitude.PropertyECGEventArgs e) {
            if (Physiology != null) {
                switch (e.Key) {
                    default: break;
                    case PropertyECGAmplitude.Keys.PAmplitude: Physiology.PAmplitude = e.Values ?? new double [] { 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d }; break;
                    case PropertyECGAmplitude.Keys.QAmplitude: Physiology.QAmplitude = e.Values ?? new double [] { 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d }; break;
                    case PropertyECGAmplitude.Keys.RAmplitude: Physiology.RAmplitude = e.Values ?? new double [] { 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d }; break;
                    case PropertyECGAmplitude.Keys.SAmplitude: Physiology.SAmplitude = e.Values ?? new double [] { 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d }; break;
                    case PropertyECGAmplitude.Keys.TAmplitude: Physiology.TAmplitude = e.Values ?? new double [] { 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d }; break;
                }
            }
        }
        private void UpdatePhysiology (object? sender, PropertyECGSegment.PropertyECGEventArgs e) {
            if (Physiology != null) {
                switch (e.Key) {
                    default: break;
                    case PropertyECGSegment.Keys.STElevation: Physiology.STElevation = e.Values ?? new double [] { 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d }; break;
                }
            }
        }

        private void UpdateCardiacRhythm (object? sender, PropertyEnum.PropertyEnumEventArgs e) {
            CheckBox chkClampVitals = this.GetControl<CheckBox> ("chkClampVitals");
            if ((!chkClampVitals.IsChecked ?? false) || e.Value == null || this.Physiology == null)
                return;

            CardiacRhythms.DefaultVitals v = CardiacRhythms.Defaults (
                (CardiacRhythms.Values)Enum.Parse (typeof (CardiacRhythms.Values), e.Value));

            Physiology.HR = (int)II.Math.Clamp ((double)Physiology.VitalSignsSettings.HR, v.HRMin, v.HRMax);
            Physiology.RR = (int)II.Math.Clamp ((double)Physiology.VitalSignsSettings.RR, v.RRMin, v.RRMax);
            Physiology.SPO2 = (int)II.Math.Clamp ((double)Physiology.VitalSignsSettings.SPO2, v.SPO2Min, v.SPO2Max);
            Physiology.ETCO2 = (int)II.Math.Clamp ((double)Physiology.VitalSignsSettings.ETCO2, v.ETCO2Min, v.ETCO2Max);
            Physiology.NSBP = (int)II.Math.Clamp ((double)Physiology.VitalSignsSettings.NSBP, v.SBPMin, v.SBPMax);
            Physiology.NDBP = (int)II.Math.Clamp ((double)Physiology.VitalSignsSettings.NDBP, v.DBPMin, v.DBPMax);
            Physiology.ASBP = (int)II.Math.Clamp ((double)Physiology.VitalSignsSettings.ASBP, v.SBPMin, v.SBPMax);
            Physiology.ADBP = (int)II.Math.Clamp ((double)Physiology.VitalSignsSettings.ADBP, v.DBPMin, v.DBPMax);
            Physiology.PSP = (int)II.Math.Clamp ((double)Physiology.VitalSignsSettings.PSP, v.PSPMin, v.PSPMax);
            Physiology.PDP = (int)II.Math.Clamp ((double)Physiology.VitalSignsSettings.PDP, v.PDPMin, v.PDPMax);
            Physiology.QRSInterval = (double)II.Math.Clamp ((double)Physiology.QRSInterval, v.QRSIntervalMin, v.QRSIntervalMax);
            Physiology.QTcInterval = (double)II.Math.Clamp ((double)Physiology.QTcInterval, v.QTCIntervalMin, v.QTCIntervalMax);

            _ = UpdateView ();
        }

        private void UpdateRespiratoryRhythm (object? sender, PropertyEnum.PropertyEnumEventArgs e) {
            CheckBox chkClampVitals = this.GetControl<CheckBox> ("chkClampVitals");

            if ((!chkClampVitals.IsChecked ?? false) || e.Value == null || Physiology == null)
                return;

            Respiratory_Rhythms.Default_Vitals v = Respiratory_Rhythms.DefaultVitals (
                (Respiratory_Rhythms.Values)Enum.Parse (typeof (Respiratory_Rhythms.Values), e.Value));

            Physiology.RR = (int)II.Math.Clamp ((double)Physiology.RR, v.RRMin, v.RRMax);
            Physiology.RRIEI = (int)II.Math.Clamp ((double)Physiology.RRIEI, v.RRIEIMin, v.RRIEIMax);
            Physiology.RRIEE = (int)II.Math.Clamp ((double)Physiology.RRIEE, v.RRIEEMin, v.RRIEEMax);

            _ = UpdateView ();
        }

        private void UpdatePACatheterRhythm (object? sender, PropertyEnum.PropertyEnumEventArgs e) {
            if (e.Value == null || Physiology == null)
                return;

            PulmonaryArteryRhythms.Default_Vitals v = PulmonaryArteryRhythms.DefaultVitals (
                (PulmonaryArteryRhythms.Values)Enum.Parse (typeof (PulmonaryArteryRhythms.Values), e.Value));

            Physiology.PSP = (int)II.Math.Clamp ((double)Physiology.PSP, v.PSPMin, v.PSPMax);
            Physiology.PDP = (int)II.Math.Clamp ((double)Physiology.PDP, v.PDPMin, v.PDPMax);

            _ = UpdateView ();
        }

        public async Task UpdateView () {
            Label lblActiveStep = this.GetControl<Label> ("lblActiveStep");

            CheckBox chkClampVitals = this.GetControl<CheckBox> ("chkClampVitals");

            PropertyBP pbpNBP = this.GetControl<PropertyBP> ("pbpNBP");
            PropertyBP pbpABP = this.GetControl<PropertyBP> ("pbpABP");
            PropertyBP pbpPBP = this.GetControl<PropertyBP> ("pbpPBP");

            PropertyCheck pchkMechanicallyVentilated = this.GetControl<PropertyCheck> ("pchkMechanicallyVentilated");
            PropertyCheck pchkPulsusParadoxus = this.GetControl<PropertyCheck> ("pchkPulsusParadoxus");
            PropertyCheck pchkPulsusAlternans = this.GetControl<PropertyCheck> ("pchkPulsusAlternans");
            PropertyCheck pchkElectricalAlternans = this.GetControl<PropertyCheck> ("pchkElectricalAlternans");

            PropertyDouble pdblT = this.GetControl<PropertyDouble> ("pdblT");
            PropertyDouble pdblCO = this.GetControl<PropertyDouble> ("pdblCO");
            PropertyDouble pdblQRSInterval = this.GetControl<PropertyDouble> ("pdblQRSInterval");
            PropertyDouble pdblQTcInterval = this.GetControl<PropertyDouble> ("pdblQTcInterval");
            PropertyDouble pdblInspiratoryRatio = this.GetControl<PropertyDouble> ("pdblInspiratoryRatio");
            PropertyDouble pdblExpiratoryRatio = this.GetControl<PropertyDouble> ("pdblExpiratoryRatio");
            
            PropertyECGAmplitude pecgPAmplitude = this.GetControl<PropertyECGAmplitude> ("pecgPAmplitude");
            PropertyECGAmplitude pecgQAmplitude = this.GetControl<PropertyECGAmplitude> ("pecgQAmplitude");
            PropertyECGAmplitude pecgRAmplitude = this.GetControl<PropertyECGAmplitude> ("pecgRAmplitude");
            PropertyECGAmplitude pecgSAmplitude = this.GetControl<PropertyECGAmplitude> ("pecgSAmplitude");
            PropertyECGAmplitude pecgTAmplitude = this.GetControl<PropertyECGAmplitude> ("pecgTAmplitude");
            
            PropertyECGSegment pecgSTSegment = this.GetControl<PropertyECGSegment> ("pecgSTSegment");

            PropertyEnum penmCardiacRhythms = this.GetControl<PropertyEnum> ("penmCardiacRhythms");
            PropertyEnum penmRespiratoryRhythms = this.GetControl<PropertyEnum> ("penmRespiratoryRhythms");
            PropertyEnum penmCardiacAxis = this.GetControl<PropertyEnum> ("penmCardiacAxis");
            PropertyEnum penmPACatheterRhythm = this.GetControl<PropertyEnum> ("penmPACatheterRhythm");
            PropertyEnum penmFetalHeartRhythm = this.GetControl<PropertyEnum> ("penmFetalHeartRhythm");

            PropertyInt pintHR = this.GetControl<PropertyInt> ("pintHR");
            PropertyInt pintRR = this.GetControl<PropertyInt> ("pintRR");
            PropertyInt pintSPO2 = this.GetControl<PropertyInt> ("pintSPO2");
            PropertyInt pintETCO2 = this.GetControl<PropertyInt> ("pintETCO2");
            PropertyInt pintCVP = this.GetControl<PropertyInt> ("pintCVP");
            PropertyInt pintICP = this.GetControl<PropertyInt> ("pintICP");
            PropertyInt pintIAP = this.GetControl<PropertyInt> ("pintIAP");
            PropertyInt pintPacemakerThreshold = this.GetControl<PropertyInt> ("pintPacemakerThreshold");
            PropertyInt pintFHR = this.GetControl<PropertyInt> ("pintFHR");
            PropertyInt pintFHRVariability = this.GetControl<PropertyInt> ("pintFHRVariability");
            PropertyInt pintUCFreq = this.GetControl<PropertyInt> ("pintUCFreq");
            PropertyInt pintUCDur = this.GetControl<PropertyInt> ("pintUCDur");
            PropertyInt pintUCIntensity = this.GetControl<PropertyInt> ("pintUCIntensity");
            PropertyInt pintUCResting = this.GetControl<PropertyInt> ("pintUCResting");

            lblActiveStep.Content = String.Format ("Editing Step: {0} ({1})",
                Step is null ? "N/A" : Step.Name,
                Step is null ? "N/A" : Step.Description);

            // Enable/Disable controls based on if Patient is null!
            chkClampVitals.IsEnabled = (Physiology != null);

            pbpNBP.IsEnabled = (Physiology != null);
            pbpABP.IsEnabled = (Physiology != null);
            pbpPBP.IsEnabled = (Physiology != null);

            pchkMechanicallyVentilated.IsEnabled = (Physiology != null);
            pchkPulsusParadoxus.IsEnabled = (Physiology != null);
            pchkPulsusAlternans.IsEnabled = (Physiology != null);
            pchkElectricalAlternans.IsEnabled = (Physiology != null);

            pdblT.IsEnabled = (Physiology != null);
            pdblCO.IsEnabled = (Physiology != null);
            pdblQRSInterval.IsEnabled = (Physiology != null);
            pdblQTcInterval.IsEnabled = (Physiology != null);
            pdblInspiratoryRatio.IsEnabled = (Physiology != null);
            pdblExpiratoryRatio.IsEnabled = (Physiology != null);

            pecgPAmplitude.IsEnabled = (Physiology != null);
            pecgQAmplitude.IsEnabled = (Physiology != null);
            pecgRAmplitude.IsEnabled = (Physiology != null);
            pecgSAmplitude.IsEnabled = (Physiology != null);
            pecgTAmplitude.IsEnabled = (Physiology != null);
            
            pecgSTSegment.IsEnabled = (Physiology != null);

            penmCardiacRhythms.IsEnabled = (Physiology != null);
            penmRespiratoryRhythms.IsEnabled = (Physiology != null);
            penmPACatheterRhythm.IsEnabled = (Physiology != null);
            penmCardiacAxis.IsEnabled = (Physiology != null);
            penmFetalHeartRhythm.IsEnabled = (Physiology != null);

            pintHR.IsEnabled = (Physiology != null);
            pintRR.IsEnabled = (Physiology != null);
            pintSPO2.IsEnabled = (Physiology != null);
            pintETCO2.IsEnabled = (Physiology != null);
            pintCVP.IsEnabled = (Physiology != null);
            pintICP.IsEnabled = (Physiology != null);
            pintIAP.IsEnabled = (Physiology != null);
            pintPacemakerThreshold.IsEnabled = (Physiology != null);
            pintFHR.IsEnabled = (Physiology != null);
            pintFHRVariability.IsEnabled = (Physiology != null);
            pintUCFreq.IsEnabled = (Physiology != null);
            pintUCDur.IsEnabled = (Physiology != null);
            pintUCIntensity.IsEnabled = (Physiology != null);
            pintUCResting.IsEnabled = (Physiology != null);

            if (Physiology != null) {
                // Update all controls with Patient values
                await pbpNBP.Set (Physiology.VitalSignsSettings.NSBP, Physiology.VitalSignsSettings.NDBP);
                await pbpABP.Set (Physiology.VitalSignsSettings.ASBP, Physiology.VitalSignsSettings.ADBP);
                await pbpPBP.Set (Physiology.VitalSignsSettings.PSP, Physiology.VitalSignsSettings.PDP);

                await pchkMechanicallyVentilated.Set (Physiology.MechanicallyVentilated);
                await pchkPulsusParadoxus.Set (Physiology.PulsusParadoxus);
                await pchkPulsusAlternans.Set (Physiology.PulsusAlternans);
                await pchkElectricalAlternans.Set (Physiology.ElectricalAlternans);

                await pdblT.Set (Physiology.VitalSignsSettings.T);
                await pdblCO.Set (Physiology.VitalSignsSettings.CO);
                await pdblQRSInterval.Set (Physiology.QRSInterval);
                await pdblQTcInterval.Set (Physiology.QTcInterval);
                await pdblInspiratoryRatio.Set (Physiology.VitalSignsSettings.RRIEI);
                await pdblExpiratoryRatio.Set (Physiology.VitalSignsSettings.RRIEE);

                await pecgPAmplitude.Set (Physiology.PAmplitude);
                await pecgQAmplitude.Set (Physiology.QAmplitude);
                await pecgRAmplitude.Set (Physiology.RAmplitude);
                await pecgSAmplitude.Set (Physiology.SAmplitude);
                await pecgTAmplitude.Set (Physiology.TAmplitude);
                
                await pecgSTSegment.Set (Physiology.STElevation);

                await penmCardiacRhythms.Set ((int)Physiology.CardiacRhythm.Value);
                await penmRespiratoryRhythms.Set ((int)Physiology.RespiratoryRhythm.Value);
                await penmPACatheterRhythm.Set ((int)Physiology.PulmonaryArteryPlacement.Value);
                await penmCardiacAxis.Set ((int)Physiology.CardiacAxis.Value);
                await penmFetalHeartRhythm.Set ((int)Physiology.ObstetricFetalHeartRhythm.Value);

                await pintHR.Set (Physiology.VitalSignsSettings.HR);
                await pintRR.Set (Physiology.VitalSignsSettings.RR);
                await pintSPO2.Set (Physiology.VitalSignsSettings.SPO2);
                await pintETCO2.Set (Physiology.VitalSignsSettings.ETCO2);
                await pintCVP.Set (Physiology.VitalSignsSettings.CVP);
                await pintICP.Set (Physiology.VitalSignsSettings.ICP);
                await pintIAP.Set (Physiology.VitalSignsSettings.IAP);
                await pintPacemakerThreshold.Set (Physiology.PacemakerThreshold);
                await pintFHR.Set (Physiology.VitalSignsSettings.FetalHR);
                await pintFHRVariability.Set (Physiology.ObstetricFetalRateVariability);
                await pintUCFreq.Set (Physiology.ObstetricContractionFrequency);
                await pintUCDur.Set (Physiology.ObstetricContractionDuration);
                await pintUCIntensity.Set (Physiology.ObstetricContractionIntensity);
                await pintUCResting.Set (Physiology.ObstetricUterineRestingTone);
            }
        }

        /* Generic Menu Items (across all Panels) */

        private void MenuFileNew_Click (object sender, RoutedEventArgs e)
            => Instance?.WindowMain?.MenuFileNew_Click (sender, e);

        private void MenuFileLoad_Click (object sender, RoutedEventArgs e)
            => Instance?.WindowMain?.MenuFileLoad_Click (sender, e);

        private void MenuFileSave_Click (object sender, RoutedEventArgs e)
            => Instance?.WindowMain?.MenuFileSave_Click (sender, e);

        private void MenuFileSaveAs_Click (object sender, RoutedEventArgs e)
            => Instance?.WindowMain?.MenuFileSaveAs_Click (sender, e);

        private void MenuFileExit_Click (object sender, RoutedEventArgs e)
            => Instance?.WindowMain?.MenuFileExit_Click (sender, e);

        private void MenuHelpAbout_Click (object sender, RoutedEventArgs e)
            => Instance?.WindowMain?.MenuHelpAbout_Click (sender, e);
    }
}