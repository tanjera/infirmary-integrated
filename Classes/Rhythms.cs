using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace II.Rhythms {

    public class Point {
        public float X, Y;

        public Point (float x, float y) {
            X = x;
            Y = y;
        }

        public static Point Lerp (Point a, Point b, float t) {
            return new Point (_.Lerp(a.X, b.X, t), _.Lerp(a.Y, b.Y, t));
        }

        public static Point operator * (float f, Point p) {
            return new Point (p.X * f, p.Y * f);
        }

        public static Point operator + (Point a, Point b) {
            return new Point (a.X + b.X, a.Y + b.Y);
        }
    }

    public enum Leads {
        ECG_I, ECG_II, ECG_III,
        ECG_AVR, ECG_AVL, ECG_AVF,
        ECG_V1, ECG_V2, ECG_V3, ECG_V4, ECG_V5, ECG_V6,

        SpO2,
        CVP,
        ABP,
        PA,
        IABP,

        RR,
        ETCO2
    }

    public enum Cardiac_Rhythm {
        Normal_Sinus,
        Sinus_Tachycardia,
        Sinus_Bradycardia,

        Atrial_Flutter,
        Atrial_Fibrillation,
        Premature_Atrial_Contractions,
        Supraventricular_Tachycardia,

        AV_Block__1st_Degree,
        AV_Block__Wenckebach,
        AV_Block__Mobitz_II,
        AV_Block__3rd_Degree,
        Junctional,
        Premature_Junctional_Contractions,

        Block__Bundle_Branch,
        Premature_Ventricular_Contractions,

        Idioventricular,
        Ventricular_Tachycardia,
        Ventricular_Fibrillation,
        Ventricular_Standstill,
        Pulseless_Electrical_Activity,
        Asystole
    }

    public enum Cardiac_Axis_Shifts
    {
        Normal,
        Left_Physiologic,
        Left_Pathologic,
        Right,
        Extreme,
        Indeterminate
    }

    
    public struct Range {
        public int Min, Max;
        public Range (int min, int max) { Min = min; Max = max; }
    }

    public struct _Rhythm {
        public string           Name_Long,
                                Name_Short;
        public Cardiac_Rhythm   Name_Enum;
        public bool             Pulse;
        public Range            Range_HR,
                                Range_SpO2,
                                Range_SBP,
                                Range_DBP;            
        public Beat             Beat_ECG_Isoelectric,
                                Beat_ECG_Atrial,
                                Beat_ECG_Ventricular;

        public delegate void Beat (Patient p, Strip s);

        public _Rhythm (Cardiac_Rhythm nameEnum, string nameLong, string nameShort,
                    bool hasPulse,
                    Range rangeHR, Range rangeSpO2, Range rangeSBP, Range rangeDBP,
                    Beat delECG_Iso, Beat delECG_Atria, Beat delECG_Vent) {

            Name_Long = nameLong;
            Name_Short = nameShort;
            Name_Enum = nameEnum;
            Pulse = hasPulse;
            Range_HR = rangeHR;
            Range_SpO2 = rangeSpO2;
            Range_SBP = rangeSBP;
            Range_DBP = rangeDBP;

            Beat_ECG_Isoelectric = delECG_Iso;
            Beat_ECG_Atrial = delECG_Atria;
            Beat_ECG_Ventricular = delECG_Vent;
            
        }

        public void Vitals (Patient p) {
            p.HR = _.Clamp (p.HR, Range_HR.Min, Range_HR.Max);
            p.SpO2 = _.Clamp (p.SpO2, Range_SpO2.Min, Range_SpO2.Max);
            p.NSBP = _.Clamp (p.NSBP, Range_SBP.Min, Range_SBP.Max);
            p.NDBP = _.Clamp (p.NDBP, Range_DBP.Min, Range_DBP.Max);
            p.NMAP = Patient.CalculateMAP (p.NSBP, p.NDBP);

            p.ASBP = _.Clamp(p.ASBP, Range_SBP.Min, Range_SBP.Max);
            p.ADBP = _.Clamp(p.ADBP, Range_DBP.Min, Range_DBP.Max);
            p.AMAP = Patient.CalculateMAP(p.ASBP, p.ADBP);
        }
    }


    public static class Rhythm_Index {
        public static _Rhythm Get_Rhythm (Cardiac_Rhythm r) {
            return Cardiac_Rhythms.Find (x => { return x.Name_Enum == r; });
        }

        static List<_Rhythm> Cardiac_Rhythms = new List<_Rhythm> (new _Rhythm[] {
            new _Rhythm (Cardiac_Rhythm.Asystole, "Asystole", "ASYS",
                false,
                new Range (0, 0), new Range (0, 0), new Range (0, 30), new Range (0, 15),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.Waveform_Flatline((60f / Math.Max (1, p.HR)), 0f)); },
                delegate (Patient p, Strip s) { return; },
                delegate (Patient p, Strip s) { return; }),

            new _Rhythm (Cardiac_Rhythm.Atrial_Fibrillation, "Atrial Fibrillation", "AFIB",
                true,
                new Range (50, 160), new Range (90, 96), new Range (50, 300), new Range (30, 200),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.ECG_Isoelectric__Atrial_Fibrillation(p, s.Lead)); },
                delegate (Patient p, Strip s) { return; },
                delegate (Patient p, Strip s) { s.Overwrite(Rhythm.ECG_Complex__QRST_Normal(p, s.Lead)); }),

            new _Rhythm (Cardiac_Rhythm.Atrial_Flutter, "Atrial Flutter", "AFLUT",
                true,
                new Range (50, 160), new Range (92, 98), new Range (50, 300), new Range (30, 200),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.ECG_Isoelectric__Atrial_Flutter(p, s.Lead)); },
                delegate (Patient p, Strip s) { return; },
                delegate (Patient p, Strip s) { s.Overwrite(Rhythm.ECG_Complex__QRST_Normal(p, s.Lead)); }),

            new _Rhythm (Cardiac_Rhythm.AV_Block__1st_Degree, "AV Block, 1st Degree", "AVB-1D",
                true,
                new Range (30, 120), new Range (94, 100), new Range (50, 300), new Range (30, 200),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.Waveform_Flatline((60f / Math.Max (1, p.HR)), 0f)); },
                delegate (Patient p, Strip s) { s.Overwrite(Rhythm.ECG_Complex__P_Normal(p, s.Lead)); },
                delegate (Patient p, Strip s) { s.Overwrite(Rhythm.ECG_Complex__QRST_Normal(p, s.Lead)); }),

            new _Rhythm (Cardiac_Rhythm.AV_Block__3rd_Degree, "AV Block, 3rd Degree", "AVB-3D",
                true,
                new Range (30, 100), new Range (88, 96), new Range (50, 300), new Range (30, 200),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.Waveform_Flatline((60f / Math.Max (1, p.HR)), 0f)); },
                delegate (Patient p, Strip s) { s.Overwrite(Rhythm.ECG_Complex__P_Normal(p, s.Lead)); },
                delegate (Patient p, Strip s) { s.Overwrite(Rhythm.ECG_Complex__QRST_Normal(p, s.Lead)); }),

            new _Rhythm (Cardiac_Rhythm.AV_Block__Mobitz_II, "AV Block, Mobitz II", "AVB-MOB2",
                true,
                new Range (30, 140), new Range (92, 97), new Range (50, 300), new Range (30, 200),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.Waveform_Flatline((60f / Math.Max (1, p.HR)), 0f)); },
                delegate (Patient p, Strip s) { s.Overwrite(Rhythm.ECG_Complex__P_Normal(p, s.Lead)); },
                delegate (Patient p, Strip s) { s.Overwrite(Rhythm.ECG_Complex__QRST_Normal(p, s.Lead)); }),

            new _Rhythm (Cardiac_Rhythm.AV_Block__Wenckebach, "AV Block, Wenckebach", "ABV-WEN",
                true,
                new Range (30, 100),new Range (92, 98), new Range (50, 300), new Range (30, 200),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.Waveform_Flatline((60f / Math.Max (1, p.HR)), 0f)); },
                delegate (Patient p, Strip s) { s.Overwrite(Rhythm.ECG_Complex__P_Normal(p, s.Lead)); },
                delegate (Patient p, Strip s) { s.Overwrite(Rhythm.ECG_Complex__QRST_Normal(p, s.Lead)); }),

            new _Rhythm (Cardiac_Rhythm.Block__Bundle_Branch, "Bundle Branch Block", "BBB",
                true,
                new Range (30, 140), new Range (94, 100), new Range (50, 300), new Range (30, 200),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.Waveform_Flatline((60f / Math.Max (1, p.HR)), 0f)); },
                delegate (Patient p, Strip s) { s.Overwrite(Rhythm.ECG_Complex__P_Normal(p, s.Lead)); },
                delegate (Patient p, Strip s) { s.Overwrite(Rhythm.ECG_Complex__QRST_BBB(p, s.Lead)); }),

            new _Rhythm (Cardiac_Rhythm.Idioventricular, "Idioventricular", "IDIO",
                true,
                new Range (20, 60), new Range (85, 95), new Range (50, 140), new Range (30, 100),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.Waveform_Flatline((60f / Math.Max (1, p.HR)), 0f)); },
                delegate (Patient p, Strip s) { return; },
                delegate (Patient p, Strip s) { s.Overwrite(Rhythm.ECG_Complex__Idioventricular(p, s.Lead)); }),

            new _Rhythm (Cardiac_Rhythm.Junctional, "Junctional", "JUNC",
                true,
                new Range (40, 100), new Range (90, 96), new Range (50, 300), new Range (30, 200),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.Waveform_Flatline((60f / Math.Max (1, p.HR)), 0f)); },
                delegate (Patient p, Strip s) { return; },
                delegate (Patient p, Strip s) { s.Overwrite(Rhythm.ECG_Complex__QRST_Normal(p, s.Lead)); }),

            new _Rhythm (Cardiac_Rhythm.Normal_Sinus, "Normal Sinus", "NSR",
                true,
                new Range (60, 100), new Range (95, 100), new Range (50, 300), new Range (30, 200),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.Waveform_Flatline((60f / Math.Max (1, p.HR)), 0f)); },
                delegate (Patient p, Strip s) { s.Overwrite(Rhythm.ECG_Complex__P_Normal(p, s.Lead)); },
                delegate (Patient p, Strip s) { s.Overwrite(Rhythm.ECG_Complex__QRST_Normal(p, s.Lead)); }),

            new _Rhythm (Cardiac_Rhythm.Premature_Atrial_Contractions, "Premature Atrial Contractions", "PAC",
                true,
                new Range (60, 100), new Range (95, 100), new Range (50, 300), new Range (30, 200),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.Waveform_Flatline((60f / Math.Max (1, p.HR)), 0f)); },
                delegate (Patient p, Strip s) { s.Overwrite(Rhythm.ECG_Complex__P_Normal(p, s.Lead)); },
                delegate (Patient p, Strip s) { s.Overwrite(Rhythm.ECG_Complex__QRST_Normal(p, s.Lead)); }),

            new _Rhythm (Cardiac_Rhythm.Premature_Junctional_Contractions, "Premature Junctional Contractions", "PJC",
                true,
                new Range (60, 100), new Range (92, 98), new Range (50, 300), new Range (30, 200),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.Waveform_Flatline((60f / Math.Max (1, p.HR)), 0f)); },
                delegate (Patient p, Strip s) { s.Overwrite(Rhythm.ECG_Complex__P_Normal(p, s.Lead)); },
                delegate (Patient p, Strip s) { s.Overwrite(Rhythm.ECG_Complex__QRST_Normal(p, s.Lead)); }),

            new _Rhythm (Cardiac_Rhythm.Premature_Ventricular_Contractions, "Premature Ventricular Contractions", "PVC",
                true,
                new Range (60, 100), new Range (92, 98), new Range (50, 300), new Range (30, 200),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.Waveform_Flatline((60f / Math.Max (1, p.HR)), 0f)); },
                delegate (Patient p, Strip s) { s.Overwrite(Rhythm.ECG_Complex__P_Normal(p, s.Lead)); },
                delegate (Patient p, Strip s) { s.Overwrite(Rhythm.ECG_Complex__QRST_Normal(p, s.Lead)); }),

            new _Rhythm (Cardiac_Rhythm.Pulseless_Electrical_Activity, "Pulseless Electrical Activity", "PEA",
                false,
                new Range (40, 120), new Range (0, 0), new Range (0, 30), new Range (0, 15),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.Waveform_Flatline((60f / Math.Max (1, p.HR)), 0f)); },
                delegate (Patient p, Strip s) { s.Overwrite(Rhythm.ECG_Complex__P_Normal(p, s.Lead)); },
                delegate (Patient p, Strip s) { s.Overwrite(Rhythm.ECG_Complex__QRST_Normal(p, s.Lead)); }),

            new _Rhythm (Cardiac_Rhythm.Sinus_Bradycardia, "Sinus Bradycardia", "BRADY",
                true,
                new Range (40, 55), new Range (95, 100), new Range (50, 300), new Range (30, 200),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.Waveform_Flatline((60f / Math.Max (1, p.HR)), 0f)); },
                delegate (Patient p, Strip s) { s.Overwrite(Rhythm.ECG_Complex__P_Normal(p, s.Lead)); },
                delegate (Patient p, Strip s) { s.Overwrite(Rhythm.ECG_Complex__QRST_Normal(p, s.Lead)); }),

            new _Rhythm (Cardiac_Rhythm.Sinus_Tachycardia, "Sinus Tachycardia", "TACHY",
                true,
                new Range (110, 140), new Range (95, 100), new Range (50, 300), new Range (30, 200),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.Waveform_Flatline((60f / Math.Max (1, p.HR)), 0f)); },
                delegate (Patient p, Strip s) { s.Overwrite(Rhythm.ECG_Complex__P_Normal(p, s.Lead)); },
                delegate (Patient p, Strip s) { s.Overwrite(Rhythm.ECG_Complex__QRST_Normal(p, s.Lead)); }),

            new _Rhythm (Cardiac_Rhythm.Supraventricular_Tachycardia, "Supraventricular Tachycardia", "SVT",
                true,
                new Range (150, 210), new Range (86, 94), new Range (50, 300), new Range (30, 200),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.Waveform_Flatline((60f / Math.Max (1, p.HR)), 0f)); },
                delegate (Patient p, Strip s) { return; },
                delegate (Patient p, Strip s) { s.Overwrite(Rhythm.ECG_Complex__QRST_SVT(p, s.Lead)); }),

            new _Rhythm (Cardiac_Rhythm.Ventricular_Fibrillation, "Ventricular Fibrillation", "VFIB",
                false,
                new Range (200, 300), new Range (0, 0), new Range (0, 30), new Range (0, 15),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.Waveform_Flatline((60f / Math.Max (1, p.HR)), 0f)); },
                delegate (Patient p, Strip s) { return; },
                delegate (Patient p, Strip s) { s.Overwrite(Rhythm.ECG_Complex__QRST_VF(p, s.Lead)); }),

            new _Rhythm (Cardiac_Rhythm.Ventricular_Tachycardia, "Ventricular Tachycardia", "VTACH",
                false,
                new Range (100, 160), new Range (0, 0), new Range (0, 30), new Range (0, 15),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.Waveform_Flatline((60f / Math.Max (1, p.HR)), 0f)); },
                delegate (Patient p, Strip s) { return; },
                delegate (Patient p, Strip s) { s.Overwrite(Rhythm.ECG_Complex__QRST_VT(p, s.Lead)); }),

            new _Rhythm (Cardiac_Rhythm.Ventricular_Standstill, "Ventricular Standstill", "VSS",
                false,
                new Range (30, 100), new Range (0, 0), new Range (0, 30), new Range (0, 15),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.Waveform_Flatline((60f / Math.Max (1, p.HR)), 0f)); },
                delegate (Patient p, Strip s) { s.Overwrite(Rhythm.ECG_Complex__P_Normal(p, s.Lead)); },
                delegate (Patient p, Strip s) { return; })
        });
    }
    

    public static class Rhythm {

        public const float Draw_Resolve = 0.01f;        // Tracing resolution (seconds per drawing point) in seconds
        public const int Draw_Refresh = 10;             // Tracing draw refresh time in milliseconds

        public static List<Point> Waveform_Flatline(float _Length, float _Isoelectric) {
            return Line_Long (_Length, _Isoelectric, new Point (0, _Isoelectric));
        }
        public static List<Point> SpO2_Rhythm(Patient _P, float _Amplitude) {
            /* SpO2 during normal sinus perfusion is similar to a sine wave leaning right with dicrotic notch
		     */
            float _Portion = (60f / Math.Max (1, _P.HR)) / 10f;

            List<Point> thisBeat = new List<Point>();
            thisBeat = Concatenate (thisBeat, Curve (_Portion * 2, 0.5f * _Amplitude, 0.4f * _Amplitude, Last(thisBeat)));
            thisBeat = Concatenate (thisBeat, Curve (_Portion * 2, 0.375f * _Amplitude, 0.0f, Last (thisBeat)));
            return thisBeat;
        }
        public static List<Point> ABP_Rhythm (Patient _P, float _Amplitude) {
            /* ABP during normal sinus perfusion is similar to a sine wave leaning right with dicrotic notch
		     */            
            float _Portion = (60f / Math.Max (1, _P.HR)) / 4;

            List<Point> thisBeat = new List<Point> ();
            thisBeat = Concatenate (thisBeat, Curve (_Portion, 0.8f * _Amplitude, 0.7f * _Amplitude, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Curve (_Portion, 0.6f * _Amplitude, 0.4f * _Amplitude, Last (thisBeat)));            
            return thisBeat;
        }      
        public static List<Point> ECG_Isoelectric__Atrial_Fibrillation (Patient _P, Leads _L) {
            int Fibrillations = (int)Math.Ceiling ((60f / _.Clamp (_P.HR, 1, 160)) / 0.06);

            List<Point> thisBeat = new List<Point> ();
            for (int i = 1; i < Fibrillations; i++)
                thisBeat = Concatenate (thisBeat, ECG_P (_P, _L, 0.06f, .04f, 0f, Last (thisBeat)));
            return thisBeat;
        }    
        public static List<Point> ECG_Isoelectric__Atrial_Flutter (Patient _P, Leads _L) {
            int Flutters = (int)Math.Ceiling((60f / _.Clamp (_P.HR, 1, 160)) / 0.16f);

            List<Point> thisBeat = new List<Point> ();            
            for (int i = 1; i < Flutters; i++)
                thisBeat = Concatenate (thisBeat, ECG_P (_P, _L, 0.16f, .08f, 0f, Last (thisBeat)));
            return thisBeat;
        }
        public static List<Point> ECG_Complex__P_Normal (Patient _P, Leads _L) {
            float PR = _.Lerp (0.16f, 0.2f, _.Clamp (_.InverseLerp (160, 60, _P.HR)));            
            return ECG_P (_P, _L, (PR * 2) / 3, .1f, 0f, new Point (0, 0f));
        }
        public static List<Point> ECG_Complex__QRST_Normal (Patient _P, Leads _L) {            
            float lerpCoeff = _.Clamp (_.InverseLerp (160, 60, _P.HR)),                
                QRS = _.Lerp (0.08f, 0.12f, lerpCoeff),
                QT = _.Lerp (0.235f, 0.4f, lerpCoeff);

            List<Point> thisBeat = new List<Point> ();
            thisBeat = Concatenate (thisBeat, ECG_Q (_P, _L, QRS / 4, -0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_R (_P, _L, QRS / 4, 0.9f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_S (_P, _L, QRS / 4, -0.3f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_J (_P, _L, QRS / 4, -0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_ST (_P, _L, ((QT - QRS) * 2) / 5, 0f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_T (_P, _L, ((QT - QRS) * 3) / 5, 0.2f, 0f, Last (thisBeat)));            
            return thisBeat;
        }
        public static List<Point> ECG_Complex__QRST_BBB (Patient _P, Leads _L) {            
            float lerpCoeff = _.Clamp (_.InverseLerp (160, 60, _P.HR)),
                QRS = _.Lerp (0.12f, 0.22f, lerpCoeff),
                QT = _.Lerp (0.235f, 0.4f, lerpCoeff);

            List<Point> thisBeat = new List<Point> ();
            thisBeat = Concatenate (thisBeat, ECG_Q (_P, _L, QRS / 6, -0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_R (_P, _L, QRS / 6, 0.9f, Last (thisBeat)));

            thisBeat = Concatenate (thisBeat, ECG_Q (_P, _L, QRS / 6, 0.3f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_R (_P, _L, QRS / 6, 0.7f, Last (thisBeat)));

            thisBeat = Concatenate (thisBeat, ECG_S (_P, _L, QRS / 6, -0.3f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_J (_P, _L, QRS / 6, -0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_ST (_P, _L, ((QT - QRS) * 2) / 5, 0f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_T (_P, _L, ((QT - QRS) * 3) / 5, 0.1f, 0f, Last (thisBeat)));            
            return thisBeat;
        }
        public static List<Point> ECG_Complex__QRST_SVT (Patient _P, Leads _L) {
            float lerpCoeff = _.Clamp (_.InverseLerp (240, 160, _P.HR)),                        
                        QRS = _.Lerp (0.05f, 0.08f, lerpCoeff),
                        QT = _.Lerp (0.17f, 0.235f, lerpCoeff);

            List<Point> thisBeat = new List<Point> ();                        
            thisBeat = Concatenate (thisBeat, ECG_Q (_P, _L, QRS / 4, -0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_R (_P, _L, QRS / 4, 0.9f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_S (_P, _L, QRS / 4, -0.3f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_J (_P, _L, QRS / 4, -0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_ST (_P, _L, ((QT - QRS) * 2) / 5, -0.06f, Last (thisBeat)));
            return thisBeat;
        }
        public static List<Point> ECG_Complex__QRST_VT (Patient _P, Leads _L) {
            float _Length = 60f / Math.Max(1, _P.HR);

            List<Point> thisBeat = new List<Point> ();
            thisBeat = Concatenate (thisBeat, Curve (_Length / 4,
                -0.1f * leadCoeff[(int)_L, (int)wavePart.Q],
                -0.2f * leadCoeff[(int)_L, (int)wavePart.Q], Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Curve (_Length / 4,
                -1f * leadCoeff[(int)_L, (int)wavePart.R],
                -0.3f * leadCoeff[(int)_L, (int)wavePart.R], Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Curve (_Length / 4,
                0.1f * leadCoeff[(int)_L, (int)wavePart.T],
                0.1f * leadCoeff[(int)_L, (int)wavePart.T],
                Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Curve (_Length / 4,
                0.4f * leadCoeff[(int)_L, (int)wavePart.T],
                leadCoeff[(int)_L, (int)wavePart.T],
                Last (thisBeat)));            
            return thisBeat;
        }
        public static List<Point> ECG_Complex__QRST_VF (Patient _P, Leads _L) {
            float _Length = (60f / Math.Max(1, _P.HR)),
                    _Wave = (60f / Math.Max(1, _P.HR)) / 5f,
                    _Amplitude = _.RandomFloat (0.3f, 0.6f);

            List<Point> thisBeat = new List<Point> ();
            while (_Length > 0f) {
                thisBeat = Concatenate (thisBeat, Curve (_Wave, _Amplitude, 0f, Last (thisBeat)));
                // Flip the sign of amplitude and randomly crawl larger/smaller, models the
                // flippant waves in v-fib.
                _Amplitude = 0 - _.Clamp (_.RandomFloat (_Amplitude - 0.1f, _Amplitude + 0.1f), -1f, 1f);
                _Length -= _Wave;
            }
            return thisBeat;
        }
        public static List<Point> ECG_Complex__Idioventricular (Patient _P, Leads _L) {            
            float lerpCoeff = _.Clamp (_.InverseLerp (75, 25, _P.HR)),
                    QRS = _.Lerp (0.3f, 0.4f, lerpCoeff),
                    SQ = ((60 / _P.HR) - QRS);

            List<Point> thisBeat = new List<Point> ();
            thisBeat = Concatenate (thisBeat, Curve (QRS / 2,
                1.0f * leadCoeff[(int)_L, (int)wavePart.Q],
                -0.3f * leadCoeff[(int)_L, (int)wavePart.Q], Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Curve (QRS / 2,
                -0.3f * leadCoeff[(int)_L, (int)wavePart.R],
                -0.4f * leadCoeff[(int)_L, (int)wavePart.R], Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Curve (SQ / 3,
                0.1f * leadCoeff[(int)_L, (int)wavePart.T], 0, Last (thisBeat)));            
            return thisBeat;
        }
        

        /* 
	     * Shaping and point plotting functions 
	     */

        static Point Last (List<Point> _Original) {
            if (_Original.Count < 1)
                return new Point(0, 0);
            else
                return _Original[_Original.Count - 1];
        }
        static List<Point> Concatenate(List<Point> _Original, List<Point> _Addition) {
            // Offsets the X value of a Point[] so that it can be placed at the end
            // of an existing Point[] and continue from that point on. 

            // Nothing to add? Return something.
            if (_Original.Count == 0 && _Addition.Count == 0)
                return new List<Point>();
            else if (_Addition.Count == 0)
                return _Original;

            float _Offset = 0f;
            if (_Original.Count == 0)
                _Offset = 0;
            else if (_Original.Count > 0)
                _Offset = _Original[_Original.Count - 1].X;

            foreach (Point eachVector in _Addition)
                _Original.Add(new Point(eachVector.X + _Offset, eachVector.Y));

            return _Original;
        }

        static float Slope (Point _P1, Point _P2) {
            return ((_P2.Y - _P1.Y) / (_P2.X - _P1.X));
        }
        static Point Bezier (Point _Start, Point _Control, Point _End, float _Percent) {
            return (((1 - _Percent) * (1 - _Percent)) * _Start) + (2 * _Percent * (1 - _Percent) * _Control) + ((_Percent * _Percent) * _End);
        }

        static List<Point> Curve(float _Length, float _mV, float _mV_End, Point _Start) {
            if (_Length < 0)
                return new List<Point> ();

            int i;
            float x;
            List<Point> _Out = new List<Point>();

            for (i = 1; i * ((2 * Draw_Resolve) / _Length) <= 1; i++) {
                x = i * ((2 * Draw_Resolve) / _Length);
                _Out.Add(Bezier(new Point(0, _Start.Y), new Point(_Length / 4, _mV), new Point(_Length / 2, _mV), x));
            }

            for (i = 1; i * ((2 * Draw_Resolve) / _Length) <= 1; i++) {
                x = i * ((2 * Draw_Resolve) / _Length);
                _Out.Add(Bezier(new Point(_Length / 2, _mV), new Point(_Length / 4 * 3, _mV), new Point(_Length, _mV_End), x));
            }

            _Out.Add(new Point(_Length, _mV_End));        // Finish the curve

            return _Out;
        }
        static List<Point> Peak(float _Length, float _mV, float _mV_End, Point _Start) {
            if (_Length < 0)
                return new List<Point> ();

            int i;
            float x;
            List<Point> _Out = new List<Point>();

            for (i = 1; i * ((2 * Draw_Resolve) / _Length) <= 1; i++) {
                x = i * ((2 * Draw_Resolve) / _Length);
                _Out.Add(Bezier(new Point(0, _Start.Y), new Point(_Length / 3, _mV / 1), new Point(_Length / 2, _mV), x));
            }

            for (i = 1; i * ((2 * Draw_Resolve) / _Length) <= 1; i++) {
                x = i * ((2 * Draw_Resolve) / _Length);
                _Out.Add(Bezier(new Point(_Length / 2, _mV), new Point(_Length / 5 * 3, _mV / 1), new Point(_Length, _mV_End), x));
            }

            _Out.Add(new Point(_Length, _mV_End));        // Finish the curve

            return _Out;
        }
        static List<Point> Line(float _Length, float _mV, Point _Start) {
            if (_Length < 0)
                return new List<Point> ();

            List<Point> _Out = new List<Point>();
            _Out.Add(new Point(_Length, _mV));
            return _Out;
        }
        static List<Point> Line_Long (float _Length, float _mV, Point _Start) {
            if (_Length < 0)
                return new List<Point> ();

            List<Point> _Out = new List<Point> ();
            for (float x = 0; x <= _Length; x += Draw_Resolve)
                _Out.Add (new Point (_Start.X + x, _mV));
            return _Out;
        }


        /* 
	     * Individual waveform functions for generating portions of an individual waveform- to be
         * concatenated into a single beat. Default ECG waveform is based on Lead 2 inflections/deflections; 
         * other leads use coefficients to scale or reverse inflections/deflections.
         * 
         * _S == starting Point
         * _L == length (seconds)
         * _mV == inflection height
         * _mV_End == end deflection height
	     */

        static List<Point> ECG_P(Patient p, Leads l, Point _S) { return ECG_P(p, l, .08f, .15f, 0f, _S); }
        static List<Point> ECG_P(Patient p, Leads l, float _L, float _mV, float _mV_End, Point _S) {            
            return Peak(_L, _mV * leadCoeff[(int)l, (int)wavePart.P], _mV_End, _S);
        }

        static List<Point> ECG_Q(Patient p, Leads l, Point _S) { return ECG_Q(p, l, 1f, -.1f, _S); }
        static List<Point> ECG_Q(Patient p, Leads l, float _L, float _mV, Point _S) {
            return Line(_L, _mV * leadCoeff[(int)l, (int)wavePart.Q], _S);
        }

        static List<Point> ECG_R(Patient p, Leads l, Point _S) { return ECG_R(p, l, 1f, .9f, _S); }
        static List<Point> ECG_R(Patient p, Leads l, float _L, float _mV, Point _S) {
            return Line(_L, _mV * leadCoeff[(int)l, (int)wavePart.R], _S);
        }

        static List<Point> ECG_S(Patient p, Leads l, Point _S) { return ECG_S(p, l, 1f, -.3f, _S); }
        static List<Point> ECG_S(Patient p, Leads l, float _L, float _mV, Point _S) {
            return Line(_L, _mV * leadCoeff[(int)l, (int)wavePart.S], _S);
        }

        static List<Point> ECG_J(Patient p, Leads l, Point _S) { return ECG_J(p, l, 1f, -.1f, _S); }
        static List<Point> ECG_J(Patient p, Leads l, float _L, float _mV, Point _S) {
            return Line(_L, (_mV * leadCoeff[(int)l, (int)wavePart.J]) + p.ST_Elevation[(int)l], _S);
        }

        static List<Point> ECG_T(Patient p, Leads l, Point _S) { return ECG_T(p, l, .16f, .3f, 0f, _S); }
        static List<Point> ECG_T(Patient p, Leads l, float _L, float _mV, float _mV_End, Point _S) {
            return Peak(_L, (_mV * leadCoeff[(int)l, (int)wavePart.T]) + p.T_Elevation[(int)l], _mV_End, _S);
        }

        static List<Point> ECG_PR(Patient p, Leads l, Point _S) { return ECG_PR(p, l, .08f, 0f, _S); }
        static List<Point> ECG_PR(Patient p, Leads l, float _L, float _mV, Point _S) {
            return Line(_L, _mV + leadCoeff[(int)l, (int)wavePart.PR], _S);
        }

        static List<Point> ECG_ST(Patient p, Leads l, Point _S) { return ECG_ST(p, l, .1f, 0f, _S); }
        static List<Point> ECG_ST(Patient p, Leads l, float _L, float _mV, Point _S) {
            return Line(_L, _mV + leadCoeff[(int)l, (int)wavePart.ST] + p.ST_Elevation[(int)l], _S);
        }

        static List<Point> ECG_TP(Patient p, Leads l, Point _S) { return ECG_TP(p, l, .48f, .0f, _S); }
        static List<Point> ECG_TP(Patient p, Leads l, float _L, float _mV, Point _S) {
            return Line(_L, _mV + leadCoeff[(int)l, (int)wavePart.TP], _S);
        }


        /*
         * Coefficients for waveform portions for each lead
         */

        enum wavePart {
            P, Q, R, S, J, T, PR, ST, TP
        }

        static float[,] leadCoeff = new float[,] {   
            // P through T are multipliers; segments are additions
            { 0.7f,     0.7f,   0.7f,   0.7f,   0.7f,   0.8f,       0f,     0f,     0f },     // L1
            { 1f,       1f,     1f,     1f,     1f,     1f,         0f,     0f,     0f },     // L2
            { 0.5f,     0.5f,   0.5f,   0.5f,   0.5f,   0.2f,       0f,     0f,     0f },     // L3
            { -1f,      -1f,    -0.8f,  -1f,    -1f,    -0.9f,      0f,     0f,     0f },     // AVR
            { -1f,      0.3f,   0.2f,   0.4f,   0.3f,   0.6f,       0f,     0f,     0f },     // AVL
            { 0.7f,     0.8f,   0.8f,   0.8f,   0.8f,   0.4f,       0f,     0f,     0f },     // AVF
            { 0.2f,     -0.7f,  -1f,    0f,     0f,     0.3f,       0f,     0f,     0f },     // V1
            { 0.2f,     -1.8f,  -1.2f,  0f,     -1f,    1.4f,       0f,     0.1f,   0f },     // V2
            { 0.2f,     -3.0f,  -1.4f,  0f,     0f,     1.8f,       0f,     0.1f,   0f },     // V3
            { 0.7f,     -9.0f,  -0.8f,  0f,     0f,     1.4f,       0f,     0.1f,   0f },     // V4
            { 0.7f,     -10.0f, -0.2f,  0f,     0f,     1.0f,       0f,     0.1f,   0f },     // V5
            { 1f,       -9.0f,  -0.1f,  0f,     0f,     0.8f,       0f,     0f,     0f }      // V6            
        };
    }
}