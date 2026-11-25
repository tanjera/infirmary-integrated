/* Rhythms.cs
 * Infirmary Integrated
 * By Ibi Keller (Tanjera), (c) 2017-2023
 *
 * Enumeration of rhythms, properties of actual rhythms (e.g. has a pulse? has an atrial
 * pulse? an atrial waveform?), default vital signs for clamping.
 *
 * Note: Actual timer triggering of rhythms takes place in Patient.cs.
 */

using System;
using II.Waveform;

namespace II {
    public class CardiacRhythms {
        public Values Value;

        public bool AberrantBeat = false;           // Signals for aberrancy in rhythm generation
        public bool AlternansBeat = false;          // Signals for switching in pulsus alternans

        public CardiacRhythms (Values v) {
            Value = v;
        }

        public CardiacRhythms () {
            Value = Values.SinusRhythm;
        }

        public enum Values {
            Asystole,
            AtrialFibrillation,
            AtrialFlutter,
            AVBlock1Degree,
            AVBlock3Degree,
            AVBlock2Mobitz,
            AVBlock2Wenckebach,
            BundleBranchBlock,
            CPRArtifact,
            Idioventricular,
            Junctional,
            PulselessElectricalActivity,
            SickSinusSyndrome,
            SinusArrhythmia,
            SinusRhythm,
            SinusRhythmWithArrest,
            SinusRhythmWithBigeminy,
            SinusRhythmWithTrigeminy,
            SinusRhythmWithPACs,
            SinusRhythmWithPJCs,
            SinusRhythmWithPVCsMultifocal,
            SinusRhythmWithPVCsUnifocal,
            SupraventricularTachycardia,
            VentricularFibrillationCoarse,
            VentricularFibrillationFine,
            VentricularStandstill,
            VentricularTachycardiaMonomorphicPulsed,
            VentricularTachycardiaMonomorphicPulseless,
            VentricularTachycardiaPolymorphic
        }

        public static string LookupString (Values value) {
            return String.Format ("RHYTHM:{0}", Enum.GetValues (typeof (Values)).GetValue ((int)value)?.ToString ());
        }

        public bool CanPace {
            get {
                switch (Value) {
                    // This is unrealistic, but fits the modeling
                    case Values.VentricularTachycardiaMonomorphicPulsed:
                    case Values.VentricularTachycardiaMonomorphicPulseless:
                        return true;

                    case Values.CPRArtifact:
                        return false;

                    default: return HasVentricularPulse;
                }
            }
        }

        public bool HasAtrialPulse {
            get {
                switch (Value) {
                    case Values.Asystole:
                    case Values.AtrialFibrillation:
                    case Values.AtrialFlutter:
                    case Values.AVBlock3Degree:
                    case Values.CPRArtifact:
                    case Values.Idioventricular:
                    case Values.Junctional:
                    case Values.PulselessElectricalActivity:
                    case Values.SupraventricularTachycardia:
                    case Values.VentricularFibrillationCoarse:
                    case Values.VentricularFibrillationFine:
                    case Values.VentricularTachycardiaMonomorphicPulsed:
                    case Values.VentricularTachycardiaMonomorphicPulseless:
                    case Values.VentricularTachycardiaPolymorphic:
                        return false;

                    default:
                    case Values.AVBlock1Degree:
                    case Values.AVBlock2Mobitz:
                    case Values.AVBlock2Wenckebach:
                    case Values.BundleBranchBlock:
                    case Values.SinusRhythm:
                    case Values.SinusRhythmWithBigeminy:
                    case Values.SinusRhythmWithTrigeminy:
                    case Values.SinusRhythmWithPACs:
                    case Values.SinusRhythmWithPJCs:
                    case Values.SinusRhythmWithPVCsMultifocal:
                    case Values.SinusRhythmWithPVCsUnifocal:
                    case Values.VentricularStandstill:
                        return true;
                }
            }
        }

        public bool HasVentricularPulse {
            get {
                switch (Value) {
                    case Values.Asystole:
                    case Values.PulselessElectricalActivity:
                    case Values.VentricularFibrillationCoarse:
                    case Values.VentricularFibrillationFine:
                    case Values.VentricularStandstill:
                    case Values.VentricularTachycardiaMonomorphicPulseless:
                    case Values.VentricularTachycardiaPolymorphic:
                        return false;

                    default:
                    case Values.AtrialFibrillation:
                    case Values.AtrialFlutter:
                    case Values.AVBlock1Degree:
                    case Values.AVBlock3Degree:
                    case Values.AVBlock2Mobitz:
                    case Values.AVBlock2Wenckebach:
                    case Values.BundleBranchBlock:
                    case Values.CPRArtifact:
                    case Values.Idioventricular:
                    case Values.Junctional:
                    case Values.SinusRhythm:
                    case Values.SinusRhythmWithBigeminy:
                    case Values.SinusRhythmWithTrigeminy:
                    case Values.SinusRhythmWithPACs:
                    case Values.SinusRhythmWithPJCs:
                    case Values.SinusRhythmWithPVCsMultifocal:
                    case Values.SinusRhythmWithPVCsUnifocal:
                    case Values.SupraventricularTachycardia:
                    case Values.VentricularTachycardiaMonomorphicPulsed:
                        return true;
                }
            }
        }

        public bool HasVentricularWaveform {
            get {
                switch (Value) {
                    case Values.Asystole:
                    case Values.VentricularFibrillationFine:
                    case Values.VentricularStandstill:
                        return false;

                    default:
                    case Values.AtrialFibrillation:
                    case Values.AtrialFlutter:
                    case Values.AVBlock1Degree:
                    case Values.AVBlock3Degree:
                    case Values.AVBlock2Mobitz:
                    case Values.AVBlock2Wenckebach:
                    case Values.BundleBranchBlock:
                    case Values.CPRArtifact:
                    case Values.Idioventricular:
                    case Values.Junctional:
                    case Values.PulselessElectricalActivity:
                    case Values.SinusRhythm:
                    case Values.SinusRhythmWithBigeminy:
                    case Values.SinusRhythmWithTrigeminy:
                    case Values.SinusRhythmWithPACs:
                    case Values.SinusRhythmWithPJCs:
                    case Values.SinusRhythmWithPVCsMultifocal:
                    case Values.SinusRhythmWithPVCsUnifocal:
                    case Values.SupraventricularTachycardia:
                    case Values.VentricularFibrillationCoarse:
                    case Values.VentricularTachycardiaMonomorphicPulsed:
                    case Values.VentricularTachycardiaMonomorphicPulseless:
                    case Values.VentricularTachycardiaPolymorphic:
                        return true;
                }
            }
        }

        public class DefaultVitals {
            public int HRMin, HRMax,
                        RRMin, RRMax,
                        SPO2Min, SPO2Max,
                        ETCO2Min, ETCO2Max,
                        SBPMin, SBPMax, DBPMin, DBPMax,
                        PSPMin, PSPMax, PDPMin, PDPMax;

            public double QRSIntervalMin, QRSIntervalMax,
                        QTCIntervalMin, QTCIntervalMax;

            public DefaultVitals (
                    int hrMin, int hrMax,
                    int rrMin, int rrMax,
                    int spo2Min, int spo2Max,
                    int etco2Min, int etco2Max,
                    int sbpMin, int sbpMax, int dbpMin, int dbpMax,
                    int pspMin, int pspMax, int pdpMin, int pdpMax,
                    double qrsMin, double qrsMax,
                    double qtcMin, double qtcMax) {
                HRMin = hrMin; HRMax = hrMax;
                RRMin = rrMin; RRMax = rrMax;
                SPO2Min = spo2Min; SPO2Max = spo2Max;
                ETCO2Min = etco2Min; ETCO2Max = etco2Max;
                SBPMin = sbpMin; SBPMax = sbpMax; DBPMin = dbpMin; DBPMax = dbpMax;
                PSPMin = pspMin; PSPMax = pspMax; PDPMin = pdpMin; PDPMax = pdpMax;
                QRSIntervalMin = qrsMin; QRSIntervalMax = qrsMax;
                QTCIntervalMin = qtcMin; QTCIntervalMax = qtcMax;
            }
        }

        public static DefaultVitals Defaults (Values Rhythm) {
            switch (Rhythm) {
                default: return new DefaultVitals (0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                case Values.Asystole: return new DefaultVitals (0, 0, 0, 0, 0, 35, 0, 30, 0, 30, 0, 10, 0, 10, 0, 0, 0, 0, 0, 0);
                case Values.AtrialFibrillation: return new DefaultVitals (80, 140, 8, 22, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12, 0.06, .1, .36, .44);
                case Values.AtrialFlutter: return new DefaultVitals (80, 140, 8, 22, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12, 0.06, .1, .36, .44);
                case Values.AVBlock1Degree: return new DefaultVitals (60, 100, 8, 22, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12, 0.06, .1, .36, .44);
                case Values.AVBlock3Degree: return new DefaultVitals (30, 50, 6, 28, 78, 82, 30, 35, 70, 80, 35, 40, 10, 20, 4, 6, .12, .2, .36, .44);
                case Values.AVBlock2Mobitz: return new DefaultVitals (60, 80, 6, 28, 88, 98, 35, 45, 80, 120, 50, 70, 20, 30, 8, 12, 0.06, .1, .36, .44);
                case Values.AVBlock2Wenckebach: return new DefaultVitals (60, 80, 6, 28, 88, 98, 35, 45, 80, 120, 50, 70, 20, 30, 8, 12, 0.06, .1, .36, .44);
                case Values.BundleBranchBlock: return new DefaultVitals (60, 100, 8, 22, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12, 0.12, .2, .36, .44);
                case Values.CPRArtifact: return new DefaultVitals (100, 120, 0, 20, 0, 45, 0, 15, 0, 50, 0, 25, 0, 8, 0, 4, 0.08, 0.08, 0.36, 0.44);
                case Values.Idioventricular: return new DefaultVitals (20, 40, 6, 28, 78, 82, 30, 35, 70, 80, 35, 40, 10, 20, 4, 6, 0.14, 0.2, 0.36, 0.44);
                case Values.Junctional: return new DefaultVitals (60, 100, 8, 22, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12, 0.06, .1, .36, .44);
                case Values.PulselessElectricalActivity: return new DefaultVitals (60, 100, 0, 0, 0, 35, 0, 30, 0, 30, 0, 10, 0, 10, 0, 0, 0.06, .1, .36, .44);
                case Values.SickSinusSyndrome: return new DefaultVitals (60, 90, 8, 22, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12, 0.06, .1, .36, .44);
                case Values.SinusArrhythmia: return new DefaultVitals (60, 100, 8, 22, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12, 0.06, .1, .36, .44);
                case Values.SinusRhythm: return new DefaultVitals (60, 100, 8, 22, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12, 0.06, .1, .36, .44);
                case Values.SinusRhythmWithArrest: return new DefaultVitals (50, 80, 8, 22, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12, 0.06, .1, .36, .44);
                case Values.SinusRhythmWithBigeminy: return new DefaultVitals (60, 100, 8, 22, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12, 0.06, .1, .36, .44);
                case Values.SinusRhythmWithTrigeminy: return new DefaultVitals (60, 100, 8, 22, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12, 0.06, .1, .36, .44);
                case Values.SinusRhythmWithPACs: return new DefaultVitals (60, 100, 8, 22, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12, 0.06, .1, .36, .44);
                case Values.SinusRhythmWithPJCs: return new DefaultVitals (60, 100, 8, 22, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12, 0.06, .1, .36, .44);
                case Values.SinusRhythmWithPVCsMultifocal: return new DefaultVitals (60, 100, 8, 22, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12, 0.06, .1, .36, .44);
                case Values.SinusRhythmWithPVCsUnifocal: return new DefaultVitals (60, 100, 8, 22, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12, 0.06, .1, .36, .44);
                case Values.SupraventricularTachycardia: return new DefaultVitals (180, 280, 8, 30, 88, 98, 35, 45, 80, 120, 50, 70, 20, 30, 8, 12, 0.08, .1, .32, .4);
                case Values.VentricularFibrillationCoarse: return new DefaultVitals (400, 500, 0, 0, 0, 35, 0, 30, 0, 30, 0, 10, 0, 10, 0, 0, 0.14, 0.2, 0.36, 0.44);
                case Values.VentricularFibrillationFine: return new DefaultVitals (400, 500, 0, 0, 0, 35, 0, 30, 0, 30, 0, 10, 0, 10, 0, 0, 0.14, 0.2, 0.36, 0.44);
                case Values.VentricularStandstill: return new DefaultVitals (40, 100, 0, 0, 0, 35, 0, 30, 0, 30, 0, 10, 0, 10, 0, 0, 0, 0, 0, 0);
                case Values.VentricularTachycardiaMonomorphicPulsed: return new DefaultVitals (110, 250, 8, 22, 88, 98, 35, 45, 80, 110, 60, 80, 20, 30, 8, 12, 0.12, .2, .44, .6);
                case Values.VentricularTachycardiaMonomorphicPulseless: return new DefaultVitals (110, 250, 0, 0, 0, 35, 0, 30, 0, 30, 0, 10, 0, 10, 0, 0, 0.12, .2, .44, .6);
                case Values.VentricularTachycardiaPolymorphic: return new DefaultVitals (200, 240, 0, 0, 0, 35, 0, 30, 0, 30, 0, 10, 0, 10, 0, 0, 0.12, 0.2, 0.44, 0.66);
            }
        }

        public void ECG_Isoelectric (Physiology p, Rhythm.Strip s) {
            // Interpolate HR Interval (.75 - 5 seconds) to a resolution coefficient of 1 - 10
            // w/ default resolution of 10ms: @ HR 60 bpm, resolve @ 10ms, @ ~12 bpm, resolve @ 10ms
            // Note: high resolution times (e.g. 100ms) cuts into drawing of following concatenation!
            double ilerpHRi = Math.Clamp(Math.InverseLerp (.75, 5, p.GetHRInterval), 0, 1);
            double scaleRes = Math.Lerp(1, 10, ilerpHRi);
            
            switch (Value) {
                case Values.Asystole: s.Concatenate (Draw.Flat_Line (p.GetHRInterval, 0f, scaleRes)); return;
                case Values.AtrialFibrillation: s.Concatenate (Draw.ECG_Isoelectric__Atrial_Fibrillation (p, s.Lead)); return;
                case Values.AtrialFlutter: s.Concatenate (Draw.ECG_Isoelectric__Atrial_Flutter (p, s.Lead)); return;
                case Values.AVBlock1Degree: s.Concatenate (Draw.Flat_Line (p.GetHRInterval, 0f, scaleRes)); return;
                case Values.AVBlock3Degree: s.Concatenate (Draw.Flat_Line (p.GetHRInterval, 0f, scaleRes)); return;
                case Values.AVBlock2Mobitz: s.Concatenate (Draw.Flat_Line (p.GetHRInterval, 0f, scaleRes)); return;
                case Values.AVBlock2Wenckebach: s.Concatenate (Draw.Flat_Line (p.GetHRInterval, 0f, scaleRes)); return;
                case Values.BundleBranchBlock: s.Concatenate (Draw.Flat_Line (p.GetHRInterval, 0f, scaleRes)); return;
                case Values.CPRArtifact: s.Concatenate (Draw.Flat_Line (p.GetHRInterval, 0f, scaleRes)); return;
                case Values.Idioventricular: s.Concatenate (Draw.Flat_Line (p.GetHRInterval, 0f, scaleRes)); return;
                case Values.Junctional: s.Concatenate (Draw.Flat_Line (p.GetHRInterval, 0f, scaleRes)); return;
                case Values.PulselessElectricalActivity: s.Concatenate (Draw.Flat_Line (p.GetHRInterval, 0f, scaleRes)); return;
                case Values.SickSinusSyndrome: s.Concatenate (Draw.Flat_Line (p.GetHRInterval, 0f, scaleRes)); return;
                case Values.SinusArrhythmia: s.Concatenate (Draw.Flat_Line (p.GetHRInterval, 0f, scaleRes)); return;
                case Values.SinusRhythm: s.Concatenate (Draw.Flat_Line (p.GetHRInterval, 0f, scaleRes)); return;
                case Values.SinusRhythmWithArrest: s.Concatenate (Draw.Flat_Line (p.GetHRInterval, 0f, scaleRes)); return;
                case Values.SinusRhythmWithBigeminy: s.Concatenate (Draw.Flat_Line (p.GetHRInterval, 0f, scaleRes)); return;
                case Values.SinusRhythmWithTrigeminy: s.Concatenate (Draw.Flat_Line (p.GetHRInterval, 0f, scaleRes)); return;
                case Values.SinusRhythmWithPACs: s.Concatenate (Draw.Flat_Line (p.GetHRInterval, 0f, scaleRes)); return;
                case Values.SinusRhythmWithPJCs: s.Concatenate (Draw.Flat_Line (p.GetHRInterval, 0f, scaleRes)); return;
                case Values.SinusRhythmWithPVCsMultifocal: s.Concatenate (Draw.Flat_Line (p.GetHRInterval, 0f, scaleRes)); return;
                case Values.SinusRhythmWithPVCsUnifocal: s.Concatenate (Draw.Flat_Line (p.GetHRInterval, 0f, scaleRes)); return;
                case Values.SupraventricularTachycardia: s.Concatenate (Draw.Flat_Line (p.GetHRInterval, 0f, scaleRes)); return;
                case Values.VentricularFibrillationCoarse: s.Concatenate (Draw.Flat_Line (p.GetHRInterval, 0f, scaleRes)); return;
                case Values.VentricularFibrillationFine: s.Concatenate (Draw.Flat_Line (p.GetHRInterval, 0f, scaleRes)); return;
                case Values.VentricularStandstill: s.Concatenate (Draw.Flat_Line (p.GetHRInterval, 0f, scaleRes)); return;
                case Values.VentricularTachycardiaMonomorphicPulsed: s.Concatenate (Draw.Flat_Line (p.GetHRInterval, 0f, scaleRes)); return;
                case Values.VentricularTachycardiaMonomorphicPulseless: s.Concatenate (Draw.Flat_Line (p.GetHRInterval, 0f, scaleRes)); return;
                case Values.VentricularTachycardiaPolymorphic: s.Concatenate (Draw.Flat_Line (p.GetHRInterval, 0f, scaleRes)); return;
            }
        }

        public void ECG_Atrial (Physiology p, Rhythm.Strip s) {
            switch (Value) {
                case Values.Asystole: return;
                case Values.AtrialFibrillation: return;
                case Values.AtrialFlutter: return;
                case Values.AVBlock1Degree: s.Replace (Draw.ECG_Complex__P_Normal (p, s.Lead)); return;
                case Values.AVBlock2Wenckebach: s.Replace (Draw.ECG_Complex__P_Normal (p, s.Lead)); return;
                case Values.AVBlock2Mobitz: s.Replace (Draw.ECG_Complex__P_Normal (p, s.Lead)); return;
                case Values.AVBlock3Degree: s.Combine (Draw.ECG_Complex__P_Normal (p, s.Lead)); return;
                case Values.BundleBranchBlock: s.Replace (Draw.ECG_Complex__P_Normal (p, s.Lead)); return;
                case Values.CPRArtifact: return;
                case Values.Idioventricular: return;
                case Values.Junctional: return;
                case Values.PulselessElectricalActivity:
                case Values.SickSinusSyndrome: s.Replace (Draw.ECG_Complex__P_Normal (p, s.Lead)); return;
                case Values.SinusArrhythmia: s.Replace (Draw.ECG_Complex__P_Normal (p, s.Lead)); return;
                case Values.SinusRhythm: s.Replace (Draw.ECG_Complex__P_Normal (p, s.Lead)); return;
                case Values.SinusRhythmWithArrest: s.Replace (Draw.ECG_Complex__P_Normal (p, s.Lead)); return;
                case Values.SinusRhythmWithBigeminy: s.Replace (Draw.ECG_Complex__P_Normal (p, s.Lead)); return;
                case Values.SinusRhythmWithTrigeminy: s.Replace (Draw.ECG_Complex__P_Normal (p, s.Lead)); return;
                case Values.SinusRhythmWithPACs: s.Replace (Draw.ECG_Complex__P_Normal (p, s.Lead)); return;
                case Values.SinusRhythmWithPJCs: s.Replace (Draw.ECG_Complex__P_Normal (p, s.Lead)); return;
                case Values.SinusRhythmWithPVCsMultifocal: s.Replace (Draw.ECG_Complex__P_Normal (p, s.Lead)); return;
                case Values.SinusRhythmWithPVCsUnifocal: s.Replace (Draw.ECG_Complex__P_Normal (p, s.Lead)); return;
                case Values.SupraventricularTachycardia: return;
                case Values.VentricularFibrillationCoarse: return;
                case Values.VentricularFibrillationFine: return;
                case Values.VentricularStandstill: s.Replace (Draw.ECG_Complex__P_Normal (p, s.Lead)); return;
                case Values.VentricularTachycardiaMonomorphicPulsed: return;
                case Values.VentricularTachycardiaMonomorphicPulseless: return;
                case Values.VentricularTachycardiaPolymorphic: return;
            }
        }

        public void ECG_Ventricular (Physiology p, Rhythm.Strip s) {
            // Handle aberrant beats (may be triggered by pacemaker...)
            if (p.CardiacRhythm.AberrantBeat)
                switch (Value) {
                    default: return;
                    case Values.Asystole:
                    case Values.AtrialFlutter:
                    case Values.AtrialFibrillation:
                    case Values.AVBlock1Degree:
                    case Values.AVBlock2Wenckebach:
                    case Values.AVBlock2Mobitz:
                    case Values.AVBlock3Degree:
                    case Values.BundleBranchBlock:
                    case Values.Idioventricular:
                    case Values.Junctional:
                    case Values.PulselessElectricalActivity:
                    case Values.SickSinusSyndrome:
                    case Values.SinusArrhythmia:
                    case Values.SinusRhythm:
                    case Values.SinusRhythmWithArrest:
                    case Values.SinusRhythmWithBigeminy:
                    case Values.SinusRhythmWithTrigeminy:
                    case Values.SinusRhythmWithPACs:
                    case Values.SinusRhythmWithPJCs:
                    case Values.SinusRhythmWithPVCsUnifocal:
                    case Values.SupraventricularTachycardia:
                    case Values.VentricularStandstill:
                    case Values.VentricularTachycardiaMonomorphicPulsed:
                    case Values.VentricularTachycardiaMonomorphicPulseless:
                        s.Replace (Draw.ECG_Complex__QRST_Aberrant_3 (p, s.Lead));
                        return;

                    case Values.SinusRhythmWithPVCsMultifocal:
                        switch (new Random ().Next (0, 3)) {
                            default:
                            case 0: s.Replace (Draw.ECG_Complex__QRST_Aberrant_1 (p, s.Lead)); break;
                            case 1: s.Replace (Draw.ECG_Complex__QRST_Aberrant_2 (p, s.Lead)); break;
                            case 2: s.Replace (Draw.ECG_Complex__QRST_Aberrant_3 (p, s.Lead)); break;
                        }
                        return;
                }

            // Handle non-aberrant beats
            switch (Value) {
                default: return;
                case Values.Asystole: return;
                case Values.AtrialFlutter: s.Replace (Draw.ECG_Complex__QRST_Normal (p, s.Lead)); return;
                case Values.AtrialFibrillation: s.Replace (Draw.ECG_Complex__QRST_Normal (p, s.Lead)); return;
                case Values.AVBlock1Degree: s.Replace (Draw.ECG_Complex__QRST_Normal (p, s.Lead)); return;
                case Values.AVBlock2Wenckebach: s.Replace (Draw.ECG_Complex__QRST_Normal (p, s.Lead)); return;
                case Values.AVBlock2Mobitz: s.Replace (Draw.ECG_Complex__QRST_Normal (p, s.Lead)); return;
                case Values.AVBlock3Degree: s.Replace (Draw.ECG_Complex__QRST_Aberrant_1 (p, s.Lead)); return;
                case Values.BundleBranchBlock: s.Replace (Draw.ECG_Complex__QRST_BBB (p, s.Lead)); return;
                case Values.CPRArtifact: s.Replace (Draw.ECG_CPR_Artifact (p, s.Lead)); return;
                case Values.Idioventricular: s.Replace (Draw.ECG_Complex__Idioventricular (p, s.Lead)); return;
                case Values.Junctional: s.Replace (Draw.ECG_Complex__QRST_Normal (p, s.Lead)); return;
                case Values.PulselessElectricalActivity: s.Replace (Draw.ECG_Complex__QRST_Normal (p, s.Lead)); return;
                case Values.SickSinusSyndrome: s.Replace (Draw.ECG_Complex__QRST_Normal (p, s.Lead)); return;
                case Values.SinusArrhythmia: s.Replace (Draw.ECG_Complex__QRST_Normal (p, s.Lead)); return;
                case Values.SinusRhythm: s.Replace (Draw.ECG_Complex__QRST_Normal (p, s.Lead)); return;
                case Values.SinusRhythmWithArrest: s.Replace (Draw.ECG_Complex__QRST_Normal (p, s.Lead)); return;
                case Values.SinusRhythmWithBigeminy: s.Replace (Draw.ECG_Complex__QRST_Normal (p, s.Lead)); return;
                case Values.SinusRhythmWithTrigeminy: s.Replace (Draw.ECG_Complex__QRST_Normal (p, s.Lead)); return;
                case Values.SinusRhythmWithPACs: s.Replace (Draw.ECG_Complex__QRST_Normal (p, s.Lead)); return;
                case Values.SinusRhythmWithPJCs: s.Replace (Draw.ECG_Complex__QRST_Normal (p, s.Lead)); return;
                case Values.SinusRhythmWithPVCsMultifocal: s.Replace (Draw.ECG_Complex__QRST_Normal (p, s.Lead)); return;
                case Values.SinusRhythmWithPVCsUnifocal: s.Replace (Draw.ECG_Complex__QRST_Normal (p, s.Lead)); return;
                case Values.SupraventricularTachycardia: s.Replace (Draw.ECG_Complex__QRST_SVT (p, s.Lead)); return;
                case Values.VentricularFibrillationCoarse: s.Replace (Draw.ECG_Complex__QRST_VF (p, s.Lead, 0.7f)); return;
                case Values.VentricularFibrillationFine: s.Replace (Draw.ECG_Complex__QRST_VF (p, s.Lead, 0.1f)); return;
                case Values.VentricularStandstill: return;
                case Values.VentricularTachycardiaMonomorphicPulsed: s.Replace (Draw.ECG_Complex__QRST_VT (p, s.Lead)); return;
                case Values.VentricularTachycardiaMonomorphicPulseless: s.Replace (Draw.ECG_Complex__QRST_VT (p, s.Lead)); return;
                case Values.VentricularTachycardiaPolymorphic: s.Replace (Draw.ECG_Complex__QRST_VF (p, s.Lead, 1f)); return;
            }
        }
    }

    public class Respiratory_Rhythms {
        public Values Value;

        public Respiratory_Rhythms (Values v) {
            Value = v;
        }

        public Respiratory_Rhythms () {
            Value = Values.Regular;
        }

        public static string LookupString (Values value) {
            return String.Format ("RHYTHM:{0}", Enum.GetValues (typeof (Values)).GetValue ((int)value)?.ToString ());
        }

        public enum Values {
            Agonal,
            Apnea,
            Apneustic,
            Ataxic,
            Biot,
            CheyneStokes,
            Regular
        }

        public class Default_Vitals {
            public int RRMin, RRMax;

            public float RRIEIMin, RRIEIMax,
                         RRIEEMin, RRIEEMax;

            public Default_Vitals (
                    int rrMin, int rrMax,
                    float rr_IE_I_Min, float rr_IE_I_Max,
                    float rr_IE_E_Min, float rr_IE_E_Max) {
                RRMin = rrMin;
                RRMax = rrMax;
                RRIEIMin = rr_IE_I_Min;
                RRIEIMax = rr_IE_I_Max;
                RRIEEMin = rr_IE_E_Min;
                RRIEEMax = rr_IE_E_Max;
            }
        }

        public static Default_Vitals DefaultVitals (Values Rhythm) {
            switch (Rhythm) {
                default: return new Default_Vitals (0, 0, 1f, 1f, 2f, 4f);
                case Values.Agonal: return new Default_Vitals (2, 6, 1f, 1f, 4f, 6f);
                case Values.Apnea: return new Default_Vitals (0, 0, 1f, 1f, 2f, 4f);
                case Values.Apneustic: return new Default_Vitals (4, 8, 1f, 1f, 0.5f, 1f);
                case Values.Ataxic: return new Default_Vitals (6, 20, 1f, 1f, 2f, 4f);
                case Values.Biot: return new Default_Vitals (8, 40, 1f, 1f, 1f, 4f);
                case Values.CheyneStokes: return new Default_Vitals (14, 20, 1f, 1f, 2f, 4f);
                case Values.Regular: return new Default_Vitals (8, 22, 1f, 1f, 2f, 4f);
            }
        }
    }

    public class PulmonaryArteryRhythms {
        public Values Value;

        public PulmonaryArteryRhythms (Values v) {
            Value = v;
        }

        public PulmonaryArteryRhythms () {
            Value = Values.PulmonaryArtery;
        }

        public static string LookupString (Values value) {
            return String.Format ("RHYTHM:{0}", Enum.GetValues (typeof (Values)).GetValue ((int)value)?.ToString ());
        }

        public enum Values {
            RightAtrium,
            RightVentricle,
            PulmonaryArtery,
            PulmonaryCapillaryWedge
        }

        public class Default_Vitals {
            public int PSPMin, PSPMax, PDPMin, PDPMax;

            public Default_Vitals (
                   int pspMin, int pspMax, int pdpMin, int pdpMax) {
                PSPMin = pspMin; PSPMax = pspMax; PDPMin = pdpMin; PDPMax = pdpMax;
            }
        }

        public static Default_Vitals DefaultVitals (Values Rhythm) {
            switch (Rhythm) {
                case Values.RightAtrium: return new Default_Vitals (7, 11, 0, 3);
                case Values.RightVentricle: return new Default_Vitals (21, 25, -7, -3);

                default:
                case Values.PulmonaryArtery: return new Default_Vitals (21, 25, 10, 12);
                case Values.PulmonaryCapillaryWedge: return new Default_Vitals (10, 12, 6, 8);
            }
        }
    }

    public class Cardiac_Axes {
        public Values Value;

        public Cardiac_Axes (Values v) {
            Value = v;
        }

        public Cardiac_Axes () {
            Value = Values.Normal;
        }

        public static string LookupString (Values value) {
            return String.Format ("CARDIAC_AXIS:{0}", Enum.GetValues (typeof (Values)).GetValue ((int)value)?.ToString ());
        }

        public enum Values {
            Normal,
            LeftPhysiologic,
            LeftPathologic,
            Right,
            Extreme,
            Indeterminate
        }
    }

    public class FetalHeart_Rhythms {
        public Values Value;

        public FetalHeart_Rhythms (Values v) {
            Value = v;
        }

        public FetalHeart_Rhythms () {
            Value = Values.Baseline;
        }

        public static string LookupString (Values v) {
            return String.Format ("FETAL_RHYTHMS:{0}", Enum.GetValues (typeof (Values)).GetValue ((int)v)?.ToString ());
        }

        public enum Values {
            Baseline,
            Acceleration,
            DecelerationEarly,
            DecelerationLate,
            DecelerationVariable
        }

        public enum States {
            Interval,
            Accelerating,
            Decelerating,
            Refractory
        }
    }
}