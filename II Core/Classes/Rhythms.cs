/* Rhythms.cs
 * Infirmary Integrated
 * By Ibi Keller (Tanjera), (c) 2017
 *
 * Enumeration of rhythms, properties of actual rhythms (e.g. has a pulse? has an atrial
 * pulse? an atrial waveform?), default vital signs for clamping.
 *
 * Note: Actual timer triggering of rhythms takes place in Patient.cs.
 */

using System;
using System.Collections.Generic;

namespace II {
    public class Cardiac_Rhythms {
        public Values Value;
        public Cardiac_Rhythms (Values v) { Value = v; }
        public Cardiac_Rhythms () { Value = Values.Sinus_Rhythm; }
        public bool AberrantBeat = false;           // Signals for aberrancy in rhythm generation
        public bool AlternansBeat = false;          // Signals for switching in pulsus alternans

        public enum Values {
            Asystole,
            Atrial_Fibrillation,
            Atrial_Flutter,
            AV_Block__1st_Degree,
            AV_Block__3rd_Degree,
            AV_Block__Mobitz_II,
            AV_Block__Wenckebach,
            Bundle_Branch_Block,
            Idioventricular,
            Junctional,
            Pulseless_Electrical_Activity,
            Sick_Sinus_Syndrome,
            Sinus_Arrhythmia,
            Sinus_Rhythm,
            Sinus_Rhythm_with_Arrest,
            Sinus_Rhythm_with_Bigeminy,
            Sinus_Rhythm_with_Trigeminy,
            Sinus_Rhythm_with_PACs,
            Sinus_Rhythm_with_PJCs,
            Sinus_Rhythm_with_PVCs_Multifocal,
            Sinus_Rhythm_with_PVCs_Unifocal,
            Supraventricular_Tachycardia,
            Ventricular_Fibrillation_Coarse,
            Ventricular_Fibrillation_Fine,
            Ventricular_Standstill,
            Ventricular_Tachycardia_Monomorphic_Pulsed,
            Ventricular_Tachycardia_Monomorphic_Pulseless,
            Ventricular_Tachycardia_Polymorphic
        }

        public static string LookupString (Values value) {
            return String.Format ("RHYTHM:{0}", Enum.GetValues (typeof (Values)).GetValue ((int)value).ToString ());
        }

        public bool HasPulse_Atrial {
            get {
                switch (Value) {
                    case Values.Asystole:
                    case Values.Atrial_Fibrillation:
                    case Values.Atrial_Flutter:
                    case Values.AV_Block__3rd_Degree:
                    case Values.Idioventricular:
                    case Values.Junctional:
                    case Values.Pulseless_Electrical_Activity:
                    case Values.Supraventricular_Tachycardia:
                    case Values.Ventricular_Fibrillation_Coarse:
                    case Values.Ventricular_Fibrillation_Fine:
                    case Values.Ventricular_Tachycardia_Monomorphic_Pulsed:
                    case Values.Ventricular_Tachycardia_Monomorphic_Pulseless:
                    case Values.Ventricular_Tachycardia_Polymorphic:
                        return false;

                    default:
                    case Values.AV_Block__1st_Degree:
                    case Values.AV_Block__Mobitz_II:
                    case Values.AV_Block__Wenckebach:
                    case Values.Bundle_Branch_Block:
                    case Values.Sinus_Rhythm:
                    case Values.Sinus_Rhythm_with_Bigeminy:
                    case Values.Sinus_Rhythm_with_Trigeminy:
                    case Values.Sinus_Rhythm_with_PACs:
                    case Values.Sinus_Rhythm_with_PJCs:
                    case Values.Sinus_Rhythm_with_PVCs_Multifocal:
                    case Values.Sinus_Rhythm_with_PVCs_Unifocal:
                    case Values.Ventricular_Standstill:
                        return true;
                }
            }
        }

        public bool HasPulse_Ventricular {
            get {
                switch (Value) {
                    case Values.Asystole:
                    case Values.Pulseless_Electrical_Activity:
                    case Values.Ventricular_Fibrillation_Coarse:
                    case Values.Ventricular_Fibrillation_Fine:
                    case Values.Ventricular_Standstill:
                    case Values.Ventricular_Tachycardia_Monomorphic_Pulseless:
                    case Values.Ventricular_Tachycardia_Polymorphic:
                        return false;

                    default:
                    case Values.Atrial_Fibrillation:
                    case Values.Atrial_Flutter:
                    case Values.AV_Block__1st_Degree:
                    case Values.AV_Block__3rd_Degree:
                    case Values.AV_Block__Mobitz_II:
                    case Values.AV_Block__Wenckebach:
                    case Values.Bundle_Branch_Block:
                    case Values.Idioventricular:
                    case Values.Junctional:
                    case Values.Sinus_Rhythm:
                    case Values.Sinus_Rhythm_with_Bigeminy:
                    case Values.Sinus_Rhythm_with_Trigeminy:
                    case Values.Sinus_Rhythm_with_PACs:
                    case Values.Sinus_Rhythm_with_PJCs:
                    case Values.Sinus_Rhythm_with_PVCs_Multifocal:
                    case Values.Sinus_Rhythm_with_PVCs_Unifocal:
                    case Values.Supraventricular_Tachycardia:
                    case Values.Ventricular_Tachycardia_Monomorphic_Pulsed:
                        return true;
                }
            }
        }

        public bool HasWaveform_Ventricular {
            get {
                switch (Value) {
                    case Values.Asystole:
                    case Values.Ventricular_Fibrillation_Fine:
                    case Values.Ventricular_Standstill:
                        return false;

                    default:
                    case Values.Atrial_Fibrillation:
                    case Values.Atrial_Flutter:
                    case Values.AV_Block__1st_Degree:
                    case Values.AV_Block__3rd_Degree:
                    case Values.AV_Block__Mobitz_II:
                    case Values.AV_Block__Wenckebach:
                    case Values.Bundle_Branch_Block:
                    case Values.Idioventricular:
                    case Values.Junctional:
                    case Values.Pulseless_Electrical_Activity:
                    case Values.Sinus_Rhythm:
                    case Values.Sinus_Rhythm_with_Bigeminy:
                    case Values.Sinus_Rhythm_with_Trigeminy:
                    case Values.Sinus_Rhythm_with_PACs:
                    case Values.Sinus_Rhythm_with_PJCs:
                    case Values.Sinus_Rhythm_with_PVCs_Multifocal:
                    case Values.Sinus_Rhythm_with_PVCs_Unifocal:
                    case Values.Supraventricular_Tachycardia:
                    case Values.Ventricular_Fibrillation_Coarse:
                    case Values.Ventricular_Tachycardia_Monomorphic_Pulsed:
                    case Values.Ventricular_Tachycardia_Monomorphic_Pulseless:
                    case Values.Ventricular_Tachycardia_Polymorphic:
                        return true;
                }
            }
        }

        public class Default_Vitals {
            public int HRMin, HRMax,
                        RRMin, RRMax,
                        SPO2Min, SPO2Max,
                        ETCO2Min, ETCO2Max,
                        SBPMin, SBPMax, DBPMin, DBPMax,
                        PSPMin, PSPMax, PDPMin, PDPMax;

            public Default_Vitals (
                    int hrMin, int hrMax,
                    int rrMin, int rrMax,
                    int spo2Min, int spo2Max,
                    int etco2Min, int etco2Max,
                    int sbpMin, int sbpMax, int dbpMin, int dbpMax,
                    int pspMin, int pspMax, int pdpMin, int pdpMax) {
                HRMin = hrMin; HRMax = hrMax;
                RRMin = rrMin; RRMax = rrMax;
                SPO2Min = spo2Min; SPO2Max = spo2Max;
                ETCO2Min = etco2Min; ETCO2Max = etco2Max;
                SBPMin = sbpMin; SBPMax = sbpMax; DBPMin = dbpMin; DBPMax = dbpMax;
                PSPMin = pspMin; PSPMax = pspMax; PDPMin = pdpMin; PDPMax = pdpMax;
            }
        }

        public static Default_Vitals DefaultVitals (Values Rhythm) {
            switch (Rhythm) {
                default: return new Default_Vitals (0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                case Values.Asystole: return new Default_Vitals (0, 0, 0, 0, 0, 35, 0, 30, 0, 30, 0, 10, 0, 10, 0, 0);
                case Values.Atrial_Fibrillation: return new Default_Vitals (80, 140, 8, 22, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12);
                case Values.Atrial_Flutter: return new Default_Vitals (80, 140, 8, 22, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12);
                case Values.AV_Block__1st_Degree: return new Default_Vitals (60, 100, 8, 22, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12);
                case Values.AV_Block__3rd_Degree: return new Default_Vitals (30, 50, 6, 28, 78, 82, 30, 35, 70, 80, 35, 40, 10, 20, 4, 6);
                case Values.AV_Block__Mobitz_II: return new Default_Vitals (60, 80, 6, 28, 88, 98, 35, 45, 80, 120, 50, 70, 20, 30, 8, 12);
                case Values.AV_Block__Wenckebach: return new Default_Vitals (60, 80, 6, 28, 88, 98, 35, 45, 80, 120, 50, 70, 20, 30, 8, 12);
                case Values.Bundle_Branch_Block: return new Default_Vitals (60, 100, 8, 22, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12);
                case Values.Idioventricular: return new Default_Vitals (20, 40, 6, 28, 78, 82, 30, 35, 70, 80, 35, 40, 10, 20, 4, 6);
                case Values.Junctional: return new Default_Vitals (60, 100, 8, 22, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12);
                case Values.Pulseless_Electrical_Activity: return new Default_Vitals (60, 100, 0, 0, 0, 35, 0, 30, 0, 30, 0, 10, 0, 10, 0, 0);
                case Values.Sick_Sinus_Syndrome: return new Default_Vitals (60, 90, 8, 22, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12);
                case Values.Sinus_Arrhythmia: return new Default_Vitals (60, 100, 8, 22, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12);
                case Values.Sinus_Rhythm: return new Default_Vitals (60, 100, 8, 22, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12);
                case Values.Sinus_Rhythm_with_Arrest: return new Default_Vitals (50, 80, 8, 22, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12);
                case Values.Sinus_Rhythm_with_Bigeminy: return new Default_Vitals (60, 100, 8, 22, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12);
                case Values.Sinus_Rhythm_with_Trigeminy: return new Default_Vitals (60, 100, 8, 22, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12);
                case Values.Sinus_Rhythm_with_PACs: return new Default_Vitals (60, 100, 8, 22, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12);
                case Values.Sinus_Rhythm_with_PJCs: return new Default_Vitals (60, 100, 8, 22, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12);
                case Values.Sinus_Rhythm_with_PVCs_Multifocal: return new Default_Vitals (60, 100, 8, 22, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12);
                case Values.Sinus_Rhythm_with_PVCs_Unifocal: return new Default_Vitals (60, 100, 8, 22, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12);
                case Values.Supraventricular_Tachycardia: return new Default_Vitals (140, 280, 8, 30, 88, 98, 35, 45, 80, 120, 50, 70, 20, 30, 8, 12);
                case Values.Ventricular_Fibrillation_Coarse: return new Default_Vitals (400, 500, 0, 0, 0, 35, 0, 30, 0, 30, 0, 10, 0, 10, 0, 0);
                case Values.Ventricular_Fibrillation_Fine: return new Default_Vitals (400, 500, 0, 0, 0, 35, 0, 30, 0, 30, 0, 10, 0, 10, 0, 0);
                case Values.Ventricular_Standstill: return new Default_Vitals (40, 100, 0, 0, 0, 35, 0, 30, 0, 30, 0, 10, 0, 10, 0, 0);
                case Values.Ventricular_Tachycardia_Monomorphic_Pulsed: return new Default_Vitals (110, 250, 8, 22, 88, 98, 35, 45, 80, 110, 60, 80, 20, 30, 8, 12);
                case Values.Ventricular_Tachycardia_Monomorphic_Pulseless: return new Default_Vitals (110, 250, 0, 0, 0, 35, 0, 30, 0, 30, 0, 10, 0, 10, 0, 0);
                case Values.Ventricular_Tachycardia_Polymorphic: return new Default_Vitals (200, 240, 0, 0, 0, 35, 0, 30, 0, 30, 0, 10, 0, 10, 0, 0);
            }
        }

        public void ECG_Isoelectric (Patient p, Rhythm.Strip s) {
            switch (Value) {
                case Values.Asystole: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.GetHR_Seconds, 0f)); return;
                case Values.Atrial_Fibrillation: s.Concatenate (Rhythm.Waveforms.ECG_Isoelectric__Atrial_Fibrillation (p, s.Lead)); return;
                case Values.Atrial_Flutter: s.Concatenate (Rhythm.Waveforms.ECG_Isoelectric__Atrial_Flutter (p, s.Lead)); return;
                case Values.AV_Block__1st_Degree: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.GetHR_Seconds, 0f)); return;
                case Values.AV_Block__3rd_Degree: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.GetHR_Seconds, 0f)); return;
                case Values.AV_Block__Mobitz_II: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.GetHR_Seconds, 0f)); return;
                case Values.AV_Block__Wenckebach: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.GetHR_Seconds, 0f)); return;
                case Values.Bundle_Branch_Block: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.GetHR_Seconds, 0f)); return;
                case Values.Idioventricular: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.GetHR_Seconds, 0f)); return;
                case Values.Junctional: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.GetHR_Seconds, 0f)); return;
                case Values.Pulseless_Electrical_Activity: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.GetHR_Seconds, 0f)); return;
                case Values.Sick_Sinus_Syndrome: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.GetHR_Seconds, 0f)); return;
                case Values.Sinus_Arrhythmia: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.GetHR_Seconds, 0f)); return;
                case Values.Sinus_Rhythm: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.GetHR_Seconds, 0f)); return;
                case Values.Sinus_Rhythm_with_Arrest: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.GetHR_Seconds, 0f)); return;
                case Values.Sinus_Rhythm_with_Bigeminy: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.GetHR_Seconds, 0f)); return;
                case Values.Sinus_Rhythm_with_Trigeminy: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.GetHR_Seconds, 0f)); return;
                case Values.Sinus_Rhythm_with_PACs: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.GetHR_Seconds, 0f)); return;
                case Values.Sinus_Rhythm_with_PJCs: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.GetHR_Seconds, 0f)); return;
                case Values.Sinus_Rhythm_with_PVCs_Multifocal: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.GetHR_Seconds, 0f)); return;
                case Values.Sinus_Rhythm_with_PVCs_Unifocal: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.GetHR_Seconds, 0f)); return;
                case Values.Supraventricular_Tachycardia: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.GetHR_Seconds, 0f)); return;
                case Values.Ventricular_Fibrillation_Coarse: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.GetHR_Seconds, 0f)); return;
                case Values.Ventricular_Fibrillation_Fine: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.GetHR_Seconds, 0f)); return;
                case Values.Ventricular_Standstill: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.GetHR_Seconds, 0f)); return;
                case Values.Ventricular_Tachycardia_Monomorphic_Pulsed: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.GetHR_Seconds, 0f)); return;
                case Values.Ventricular_Tachycardia_Monomorphic_Pulseless: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.GetHR_Seconds, 0f)); return;
                case Values.Ventricular_Tachycardia_Polymorphic: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.GetHR_Seconds, 0f)); return;
            }
        }

        public void ECG_Atrial (Patient p, Rhythm.Strip s) {
            switch (Value) {
                case Values.Asystole: return;
                case Values.Atrial_Fibrillation: return;
                case Values.Atrial_Flutter: return;
                case Values.AV_Block__1st_Degree: s.Overwrite (Rhythm.Waveforms.ECG_Complex__P_Normal (p, s.Lead)); return;
                case Values.AV_Block__Wenckebach: s.Overwrite (Rhythm.Waveforms.ECG_Complex__P_Normal (p, s.Lead)); return;
                case Values.AV_Block__Mobitz_II: s.Overwrite (Rhythm.Waveforms.ECG_Complex__P_Normal (p, s.Lead)); return;
                case Values.AV_Block__3rd_Degree: s.Underwrite (Rhythm.Waveforms.ECG_Complex__P_Normal (p, s.Lead)); return;
                case Values.Bundle_Branch_Block: s.Overwrite (Rhythm.Waveforms.ECG_Complex__P_Normal (p, s.Lead)); return;
                case Values.Idioventricular: return;
                case Values.Junctional: return;
                case Values.Pulseless_Electrical_Activity:
                case Values.Sick_Sinus_Syndrome: s.Overwrite (Rhythm.Waveforms.ECG_Complex__P_Normal (p, s.Lead)); return;
                case Values.Sinus_Arrhythmia: s.Overwrite (Rhythm.Waveforms.ECG_Complex__P_Normal (p, s.Lead)); return;
                case Values.Sinus_Rhythm: s.Overwrite (Rhythm.Waveforms.ECG_Complex__P_Normal (p, s.Lead)); return;
                case Values.Sinus_Rhythm_with_Arrest: s.Overwrite (Rhythm.Waveforms.ECG_Complex__P_Normal (p, s.Lead)); return;
                case Values.Sinus_Rhythm_with_Bigeminy: s.Overwrite (Rhythm.Waveforms.ECG_Complex__P_Normal (p, s.Lead)); return;
                case Values.Sinus_Rhythm_with_Trigeminy: s.Overwrite (Rhythm.Waveforms.ECG_Complex__P_Normal (p, s.Lead)); return;
                case Values.Sinus_Rhythm_with_PACs: s.Overwrite (Rhythm.Waveforms.ECG_Complex__P_Normal (p, s.Lead)); return;
                case Values.Sinus_Rhythm_with_PJCs: s.Overwrite (Rhythm.Waveforms.ECG_Complex__P_Normal (p, s.Lead)); return;
                case Values.Sinus_Rhythm_with_PVCs_Multifocal: s.Overwrite (Rhythm.Waveforms.ECG_Complex__P_Normal (p, s.Lead)); return;
                case Values.Sinus_Rhythm_with_PVCs_Unifocal: s.Overwrite (Rhythm.Waveforms.ECG_Complex__P_Normal (p, s.Lead)); return;
                case Values.Supraventricular_Tachycardia: return;
                case Values.Ventricular_Fibrillation_Coarse: return;
                case Values.Ventricular_Fibrillation_Fine: return;
                case Values.Ventricular_Standstill: s.Overwrite (Rhythm.Waveforms.ECG_Complex__P_Normal (p, s.Lead)); return;
                case Values.Ventricular_Tachycardia_Monomorphic_Pulsed: return;
                case Values.Ventricular_Tachycardia_Monomorphic_Pulseless: return;
                case Values.Ventricular_Tachycardia_Polymorphic: return;
            }
        }

        public void ECG_Ventricular (Patient p, Rhythm.Strip s) {
            // Handle aberrant beats (may be triggered by pacemaker...)
            if (p.Cardiac_Rhythm.AberrantBeat)
                switch (Value) {
                    default: return;
                    case Values.Asystole:
                    case Values.Atrial_Flutter:
                    case Values.Atrial_Fibrillation:
                    case Values.AV_Block__1st_Degree:
                    case Values.AV_Block__Wenckebach:
                    case Values.AV_Block__Mobitz_II:
                    case Values.AV_Block__3rd_Degree:
                    case Values.Bundle_Branch_Block:
                    case Values.Idioventricular:
                    case Values.Junctional:
                    case Values.Pulseless_Electrical_Activity:
                    case Values.Sick_Sinus_Syndrome:
                    case Values.Sinus_Arrhythmia:
                    case Values.Sinus_Rhythm:
                    case Values.Sinus_Rhythm_with_Arrest:
                    case Values.Sinus_Rhythm_with_Bigeminy:
                    case Values.Sinus_Rhythm_with_Trigeminy:
                    case Values.Sinus_Rhythm_with_PACs:
                    case Values.Sinus_Rhythm_with_PJCs:
                    case Values.Sinus_Rhythm_with_PVCs_Unifocal:
                    case Values.Supraventricular_Tachycardia:
                    case Values.Ventricular_Standstill:
                    case Values.Ventricular_Tachycardia_Monomorphic_Pulsed:
                    case Values.Ventricular_Tachycardia_Monomorphic_Pulseless:
                        s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_Aberrant_3 (p, s.Lead));
                        return;

                    case Values.Sinus_Rhythm_with_PVCs_Multifocal:
                        switch (new Random ().Next (0, 3)) {
                            default:
                            case 0: s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_Aberrant_1 (p, s.Lead)); break;
                            case 1: s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_Aberrant_2 (p, s.Lead)); break;
                            case 2: s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_Aberrant_3 (p, s.Lead)); break;
                        }
                        return;
                }

            // Handle non-aberrant beats
            switch (Value) {
                default: return;
                case Values.Asystole: return;
                case Values.Atrial_Flutter: s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_Normal (p, s.Lead)); return;
                case Values.Atrial_Fibrillation: s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_Normal (p, s.Lead)); return;
                case Values.AV_Block__1st_Degree: s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_Normal (p, s.Lead)); return;
                case Values.AV_Block__Wenckebach: s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_Normal (p, s.Lead)); return;
                case Values.AV_Block__Mobitz_II: s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_Normal (p, s.Lead)); return;
                case Values.AV_Block__3rd_Degree: s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_Aberrant_1 (p, s.Lead)); return;
                case Values.Bundle_Branch_Block: s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_BBB (p, s.Lead)); return;
                case Values.Idioventricular: s.Overwrite (Rhythm.Waveforms.ECG_Complex__Idioventricular (p, s.Lead)); return;
                case Values.Junctional: s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_Normal (p, s.Lead)); return;
                case Values.Pulseless_Electrical_Activity: s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_Normal (p, s.Lead)); return;
                case Values.Sick_Sinus_Syndrome: s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_Normal (p, s.Lead)); return;
                case Values.Sinus_Arrhythmia: s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_Normal (p, s.Lead)); return;
                case Values.Sinus_Rhythm: s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_Normal (p, s.Lead)); return;
                case Values.Sinus_Rhythm_with_Arrest: s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_Normal (p, s.Lead)); return;
                case Values.Sinus_Rhythm_with_Bigeminy: s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_Normal (p, s.Lead)); return;
                case Values.Sinus_Rhythm_with_Trigeminy: s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_Normal (p, s.Lead)); return;
                case Values.Sinus_Rhythm_with_PACs: s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_Normal (p, s.Lead)); return;
                case Values.Sinus_Rhythm_with_PJCs: s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_Normal (p, s.Lead)); return;
                case Values.Sinus_Rhythm_with_PVCs_Multifocal: s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_Normal (p, s.Lead)); return;
                case Values.Sinus_Rhythm_with_PVCs_Unifocal: s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_Normal (p, s.Lead)); return;
                case Values.Supraventricular_Tachycardia: s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_SVT (p, s.Lead)); return;
                case Values.Ventricular_Fibrillation_Coarse: s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_VF (p, s.Lead, 0.7f)); return;
                case Values.Ventricular_Fibrillation_Fine: s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_VF (p, s.Lead, 0.1f)); return;
                case Values.Ventricular_Standstill: return;
                case Values.Ventricular_Tachycardia_Monomorphic_Pulsed: s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_VT (p, s.Lead)); return;
                case Values.Ventricular_Tachycardia_Monomorphic_Pulseless: s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_VT (p, s.Lead)); return;
                case Values.Ventricular_Tachycardia_Polymorphic: s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_VF (p, s.Lead, 1f)); return;
            }
        }
    }

    public class Respiratory_Rhythms {
        public Values Value;
        public Respiratory_Rhythms (Values v) { Value = v; }
        public Respiratory_Rhythms () { Value = Values.Regular; }

        public static string LookupString (Values value) {
            return String.Format ("RHYTHM:{0}", Enum.GetValues (typeof (Values)).GetValue ((int)value).ToString ());
        }

        public enum Values {
            Agonal,
            Apnea,
            Apneustic,
            Ataxic,
            Biot,
            Cheyne_Stokes,
            Regular
        }

        public class Default_Vitals {
            public int RRMin, RRMax;
            public float RR_IE_I_Min, RR_IE_I_Max,
                         RR_IE_E_Min, RR_IE_E_Max;

            public Default_Vitals (
                    int rrMin, int rrMax,
                    float rr_IE_I_Min, float rr_IE_I_Max,
                    float rr_IE_E_Min, float rr_IE_E_Max) {
                RRMin = rrMin;
                RRMax = rrMax;
                RR_IE_I_Min = rr_IE_I_Min;
                RR_IE_I_Max = rr_IE_I_Max;
                RR_IE_E_Min = rr_IE_E_Min;
                RR_IE_E_Max = rr_IE_E_Max;
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
                case Values.Cheyne_Stokes: return new Default_Vitals (14, 20, 1f, 1f, 2f, 4f);
                case Values.Regular: return new Default_Vitals (8, 22, 1f, 1f, 2f, 4f);
            }
        }
    }

    public class CardiacAxes {
        public Values Value;
        public CardiacAxes (Values v) { Value = v; }
        public CardiacAxes () { Value = Values.Normal; }

        public static string LookupString (Values value) {
            return String.Format ("CARDIAC_AXIS:{0}", Enum.GetValues (typeof (Values)).GetValue ((int)value).ToString ());
        }

        public enum Values {
            Normal,
            Left_Physiologic,
            Left_Pathologic,
            Right,
            Extreme,
            Indeterminate
        }
    }

    public class FetalHeartDecelerations {
        public List<Values> ValueList;
        public FetalHeartDecelerations (List<Values> v) { ValueList = v; }
        public FetalHeartDecelerations () { ValueList = new List<Values> (); }

        public static string LookupString (Values v) {
            return String.Format ("FETAL_RHYTHMS:{0}", Enum.GetValues (typeof (Values)).GetValue ((int)v).ToString ());
        }

        public enum Values {
            Acceleration,
            DecelerationEarly,
            DecelerationLate,
            DecelerationVariable
        }
    }
}