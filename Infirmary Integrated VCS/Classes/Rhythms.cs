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
            Ventricular_Tachycardia_Pulsed,
            Ventricular_Tachycardia_Pulseless
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
            "Ventricular Tachycardia (w/ pulse)",
            "Ventricular Tachycardia (w/out pulse)",
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
                    case Values.Ventricular_Tachycardia_Pulsed:
                    case Values.Ventricular_Tachycardia_Pulseless:
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
                    case Values.Ventricular_Tachycardia_Pulseless:
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
                    case Values.Ventricular_Tachycardia_Pulsed:
                        return true;
                }
            }
        }

        public void DefaultVitals_Clamp (Patient p) {
            switch (Value) {
                case Values.Asystole:
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
                case Values.Ventricular_Fibrillation_Fine:
                case Values.Ventricular_Standstill:
                case Values.Ventricular_Tachycardia_Pulsed:
                case Values.Ventricular_Tachycardia_Pulseless:

                    return;

            }
            /*
            p.HR = _.Clamp (p.HR, Range_HR.Min, Range_HR.Max);
            p.SpO2 = _.Clamp (p.SpO2, Range_SpO2.Min, Range_SpO2.Max);
            p.NSBP = _.Clamp (p.NSBP, Range_SBP.Min, Range_SBP.Max);
            p.NDBP = _.Clamp (p.NDBP, Range_DBP.Min, Range_DBP.Max);
            p.NMAP = Patient.CalculateMAP (p.NSBP, p.NDBP);

            p.ASBP = _.Clamp (p.ASBP, Range_SBP.Min, Range_SBP.Max);
            p.ADBP = _.Clamp (p.ADBP, Range_DBP.Min, Range_DBP.Max);
            p.AMAP = Patient.CalculateMAP (p.ASBP, p.ADBP);
            */
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
                case Values.Ventricular_Tachycardia_Pulsed: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.HR_Seconds, 0f)); return;
                case Values.Ventricular_Tachycardia_Pulseless: s.Concatenate (Rhythm.Waveforms.Waveform_Flatline (p.HR_Seconds, 0f)); return;
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
                case Values.Ventricular_Tachycardia_Pulsed: return;
                case Values.Ventricular_Tachycardia_Pulseless: return;
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
                case Values.Ventricular_Fibrillation_Coarse: s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_VF (p, s.Lead, 1f)); return;
                case Values.Ventricular_Fibrillation_Fine: s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_VF (p, s.Lead, 0.2f)); return;
                case Values.Ventricular_Standstill: return;
                case Values.Ventricular_Tachycardia_Pulsed: s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_VT (p, s.Lead)); return;
                case Values.Ventricular_Tachycardia_Pulseless: s.Overwrite (Rhythm.Waveforms.ECG_Complex__QRST_VT (p, s.Lead)); return;
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