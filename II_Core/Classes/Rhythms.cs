using System;
using System.Drawing;
using System.Collections.Generic;

namespace II {

    public class Cardiac_Rhythms {
        public Values Value;
        public Cardiac_Rhythms (Values v) { Value = v; }
        public Cardiac_Rhythms () { Value = Values.Sinus_Rhythm; }

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
            Sinus_Rhythm,
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

        public string Description { get { return Descriptions[(int)Value]; } }
        public static Values Parse_Description (string inc) {
            int i = Array.IndexOf<string> (Descriptions, inc);
            return i >= 0 ? (Values)Enum.GetValues (typeof (Values)).GetValue (i) : Values.Sinus_Rhythm;
        }

        public static string[] Descriptions = new string[] {
            "Asystole",
            "Atrial Fibrillation",
            "Atrial Flutter",
            "AV Block, 1st Degree",
            "AV Block, 3rd Degree",
            "AV Block, Mobitz II",
            "AV Block, Wenckebach",
            "Bundle Branch Block",
            "Idioventricular",
            "Junctional",
            "Pulseless Electrical Activity",
            "Sinus Rhythm",
            "Sinus Rhythm with Bigeminy",
            "Sinus Rhythm with Trigeminy",
            "Sinus Rhythm with PACs",
            "Sinus Rhythm with PJCs",
            "Sinus Rhythm with PVCs (Multifocal)",
            "Sinus Rhythm with PVCs (Unifocal)",
            "Supraventricular Tachycardia",
            "Ventricular Fibrillation (Coarse)",
            "Ventricular Fibrillation (Fine)",
            "Ventricular Standstill",
            "Ventricular Tachycardia (Monomorphic, w/ pulse)",
            "Ventricular Tachycardia (Monomorphic, w/out pulse)",
            "Ventricular Tachycardia (Polymorphic)"
        };

        public bool Pulse_Atrial {
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

        public bool Pulse_Ventricular {
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

        public class Default_Vitals {
            public int  HRMin, HRMax,
                        SPO2Min, SPO2Max,
                        ETCO2Min, ETCO2Max,
                        SBPMin, SBPMax, DBPMin, DBPMax,
                        PSPMin, PSPMax, PDPMin, PDPMax;

            public Default_Vitals (
                    int hrMin, int hrMax,
                    int spo2Min, int spo2Max,
                    int etco2Min, int etco2Max,
                    int sbpMin, int sbpMax, int dbpMin, int dbpMax,
                    int pspMin, int pspMax, int pdpMin, int pdpMax) {

                HRMin = hrMin; HRMax = hrMax;
                SPO2Min = spo2Min; SPO2Max = spo2Max;
                ETCO2Min = etco2Min; ETCO2Max = etco2Max;
                SBPMin = sbpMin; SBPMax = sbpMax; DBPMin = dbpMin; DBPMax = dbpMax;
                PSPMin = pspMin; PSPMax = pspMax; PDPMin = pdpMin; PDPMax = pdpMax;
            }
        }

        public static Default_Vitals DefaultVitals (Values Rhythm) {
            switch (Rhythm) {
                default: return new Default_Vitals (0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                case Values.Asystole: return new Default_Vitals (0, 0, 0, 35, 0, 30, 0, 30, 0, 10, 0, 10, 0, 0);
                case Values.Atrial_Fibrillation: return new Default_Vitals (80, 140, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12);
                case Values.Atrial_Flutter: return new Default_Vitals (80, 140, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12);
                case Values.AV_Block__1st_Degree: return new Default_Vitals (60, 100, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12);
                case Values.AV_Block__3rd_Degree: return new Default_Vitals (30, 50, 78, 82, 30, 35, 70, 80, 35, 40, 10, 20, 4, 6);
                case Values.AV_Block__Mobitz_II: return new Default_Vitals (60, 80, 88, 98, 35, 45, 80, 120, 50, 70, 20, 30, 8, 12);
                case Values.AV_Block__Wenckebach: return new Default_Vitals (60, 80, 88, 98, 35, 45, 80, 120, 50, 70, 20, 30, 8, 12);
                case Values.Bundle_Branch_Block: return new Default_Vitals (60, 100, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12);
                case Values.Idioventricular: return new Default_Vitals (20, 40, 78, 82, 30, 35, 70, 80, 35, 40, 10, 20, 4, 6);
                case Values.Junctional: return new Default_Vitals (60, 100, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12);
                case Values.Pulseless_Electrical_Activity: return new Default_Vitals (60, 100, 0, 35, 0, 30, 0, 30, 0, 10, 0, 10, 0, 0);
                case Values.Sinus_Rhythm: return new Default_Vitals (60, 100, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12);
                case Values.Sinus_Rhythm_with_Bigeminy: return new Default_Vitals (60, 100, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12);
                case Values.Sinus_Rhythm_with_Trigeminy: return new Default_Vitals (60, 100, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12);
                case Values.Sinus_Rhythm_with_PACs: return new Default_Vitals (60, 100, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12);
                case Values.Sinus_Rhythm_with_PJCs: return new Default_Vitals (60, 100, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12);
                case Values.Sinus_Rhythm_with_PVCs_Multifocal: return new Default_Vitals (60, 100, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12);
                case Values.Sinus_Rhythm_with_PVCs_Unifocal: return new Default_Vitals (60, 100, 93, 98, 35, 45, 100, 140, 70, 90, 20, 30, 8, 12);
                case Values.Supraventricular_Tachycardia: return new Default_Vitals (140, 280, 88, 98, 35, 45, 80, 120, 50, 70, 20, 30, 8, 12);
                case Values.Ventricular_Fibrillation_Coarse: return new Default_Vitals (400, 500, 0, 35, 0, 30, 0, 30, 0, 10, 0, 10, 0, 0);
                case Values.Ventricular_Fibrillation_Fine: return new Default_Vitals (400, 500, 0, 35, 0, 30, 0, 30, 0, 10, 0, 10, 0, 0);
                case Values.Ventricular_Standstill: return new Default_Vitals (40, 100, 0, 35, 0, 30, 0, 30, 0, 10, 0, 10, 0, 0);
                case Values.Ventricular_Tachycardia_Monomorphic_Pulsed: return new Default_Vitals (110, 250, 88, 98, 35, 45, 80, 110, 60, 80, 20, 30, 8, 12);
                case Values.Ventricular_Tachycardia_Monomorphic_Pulseless: return new Default_Vitals (110, 250, 0, 35, 0, 30, 0, 30, 0, 10, 0, 10, 0, 0);
                case Values.Ventricular_Tachycardia_Polymorphic: return new Default_Vitals (200, 240, 0, 35, 0, 30, 0, 30, 0, 10, 0, 10, 0, 0);
            }
        }

        public void ECG_Isoelectric (Patient p, Rhythm.Strip s) {
            switch (Value) {
                case Values.Asystole: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.HR_Seconds, 0f)); return;
                case Values.Atrial_Fibrillation: s.Concatenate (Rhythm.Waveforms.ECG_Isoelectric__Atrial_Fibrillation (p, s.Lead)); return;
                case Values.Atrial_Flutter: s.Concatenate (Rhythm.Waveforms.ECG_Isoelectric__Atrial_Flutter (p, s.Lead)); return;
                case Values.AV_Block__1st_Degree: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.HR_Seconds, 0f)); return;
                case Values.AV_Block__3rd_Degree: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.HR_Seconds, 0f)); return;
                case Values.AV_Block__Mobitz_II: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.HR_Seconds, 0f)); return;
                case Values.AV_Block__Wenckebach: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.HR_Seconds, 0f)); return;
                case Values.Bundle_Branch_Block: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.HR_Seconds, 0f)); return;
                case Values.Idioventricular: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.HR_Seconds, 0f)); return;
                case Values.Junctional: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.HR_Seconds, 0f)); return;
                case Values.Pulseless_Electrical_Activity: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.HR_Seconds, 0f)); return;
                case Values.Sinus_Rhythm: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.HR_Seconds, 0f)); return;
                case Values.Sinus_Rhythm_with_Bigeminy: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.HR_Seconds, 0f)); return;
                case Values.Sinus_Rhythm_with_Trigeminy: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.HR_Seconds, 0f)); return;
                case Values.Sinus_Rhythm_with_PACs: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.HR_Seconds, 0f)); return;
                case Values.Sinus_Rhythm_with_PJCs: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.HR_Seconds, 0f)); return;
                case Values.Sinus_Rhythm_with_PVCs_Multifocal: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.HR_Seconds, 0f)); return;
                case Values.Sinus_Rhythm_with_PVCs_Unifocal: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.HR_Seconds, 0f)); return;
                case Values.Supraventricular_Tachycardia: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.HR_Seconds, 0f)); return;
                case Values.Ventricular_Fibrillation_Coarse: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.HR_Seconds, 0f)); return;
                case Values.Ventricular_Fibrillation_Fine: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.HR_Seconds, 0f)); return;
                case Values.Ventricular_Standstill: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.HR_Seconds, 0f)); return;
                case Values.Ventricular_Tachycardia_Monomorphic_Pulsed: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.HR_Seconds, 0f)); return;
                case Values.Ventricular_Tachycardia_Monomorphic_Pulseless: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.HR_Seconds, 0f)); return;
                case Values.Ventricular_Tachycardia_Polymorphic: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.HR_Seconds, 0f)); return;
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
                case Values.Sinus_Rhythm: s.Overwrite (Rhythm.Waveforms.ECG_Complex__P_Normal (p, s.Lead)); return;
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
            switch (Value) {
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
                case Values.Pulseless_Electrical_Activity:
                case Values.Sinus_Rhythm: s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_Normal (p, s.Lead)); return;

                case Values.Sinus_Rhythm_with_Bigeminy:
                    if (p.Cardiac_Rhythm__Flag) {
                        s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_Aberrant_1 (p, s.Lead));
                    } else
                        s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_Normal (p, s.Lead));
                    return;

                case Values.Sinus_Rhythm_with_Trigeminy:
                    if (p.Cardiac_Rhythm__Flag) {
                        s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_Aberrant_1 (p, s.Lead));
                    } else
                        s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_Normal (p, s.Lead));
                    return;

                case Values.Sinus_Rhythm_with_PACs: s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_Normal (p, s.Lead)); return;
                case Values.Sinus_Rhythm_with_PJCs: s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_Normal (p, s.Lead)); return;

                case Values.Sinus_Rhythm_with_PVCs_Multifocal:
                    if (p.Cardiac_Rhythm__Flag) {
                        switch (new Random ().Next (0, 3)) {
                            default:
                            case 0: s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_Aberrant_1 (p, s.Lead)); break;
                            case 1: s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_Aberrant_2 (p, s.Lead)); break;
                            case 2: s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_Aberrant_3 (p, s.Lead)); break;
                        }
                    } else
                        s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_Normal (p, s.Lead));
                    return;

                case Values.Sinus_Rhythm_with_PVCs_Unifocal:

                    if (p.Cardiac_Rhythm__Flag) {
                        s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_Aberrant_3 (p, s.Lead));
                    } else
                        s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_Normal (p, s.Lead));
                    return;

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

        public string Description { get { return Descriptions [(int)Value]; } }
        public static Values Parse_Description (string inc) {
            int i = Array.IndexOf<string> (Descriptions, inc);
            return i >= 0 ? (Values)Enum.GetValues (typeof (Values)).GetValue (i) : Values.Regular;
        }

        public enum Values {
            Apnea,
            Regular
        }

        public static string [] Descriptions = new string [] {
            "Apnea",
            "Regular"
        };
    }


    public class Cardiac_Axes {
        public Values Value;
        public Cardiac_Axes (Values v) { Value = v; }
        public Cardiac_Axes () { Value = Values.Normal; }

        public string Description { get { return Descriptions [(int)Value]; } }
        public static Values Parse_Description (string inc) {
            int i = Array.IndexOf<string> (Descriptions, inc);
            return i >= 0 ? (Values)Enum.GetValues (typeof (Values)).GetValue (i) : Values.Normal;
        }

        public enum Values {
            Normal,
            Left_Physiologic,
            Left_Pathologic,
            Right,
            Extreme,
            Indeterminate
        }

        public static string [] Descriptions = new string [] {
            "Normal",
            "Left (Physiologic)",
            "Left (Pathologic)",
            "Right",
            "Extreme",
            "Indeterminate"
        };

    }
}