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
        IABP
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
        public Beat             Beat_ECG,
                                Beat_SpO2,
                                Beat_ABP;

        public delegate void Beat (Patient p, Strip s);

        public _Rhythm (Cardiac_Rhythm nameEnum, string nameLong, string nameShort,
                    bool hasPulse,
                    Range rangeHR, Range rangeSpO2, Range rangeSBP, Range rangeDBP,
                    Beat delECG, Beat delSpO2, Beat delABP) {

            Name_Long = nameLong;
            Name_Short = nameShort;
            Name_Enum = nameEnum;
            Pulse = hasPulse;
            Range_HR = rangeHR;
            Range_SpO2 = rangeSpO2;
            Range_SBP = rangeSBP;
            Range_DBP = rangeDBP;
            
            Beat_ECG = delECG;            
            Beat_SpO2 = delSpO2;
            Beat_ABP = delABP;
        }

        public void Vitals (Patient p) {
            p.HR = _.Clamp (p.HR, Range_HR.Min, Range_HR.Max);
            p.SpO2 = _.Clamp (p.SpO2, Range_SpO2.Min, Range_SpO2.Max);
            p.NSBP = _.Clamp (p.NSBP, Range_SBP.Min, Range_SBP.Max);
            p.NDBP = _.Clamp (p.NDBP, Range_DBP.Min, Range_DBP.Max);
            p.NMAP = Patient.calcMAP (p.NSBP, p.NDBP);

            p.ASBP = _.Clamp(p.ASBP, Range_SBP.Min, Range_SBP.Max);
            p.ADBP = _.Clamp(p.ADBP, Range_DBP.Min, Range_DBP.Max);
            p.AMAP = Patient.calcMAP(p.ASBP, p.ADBP);
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
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.EKG_Rhythm__Asystole (p, s.Lead, 1f, 0f)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.SpO2_Rhythm(p, 1f, 0f)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.ABP_Rhythm(p, 1f, 0f)); }),

            new _Rhythm (Cardiac_Rhythm.Atrial_Fibrillation, "Atrial Fibrillation", "AFIB",
                true,
                new Range (50, 160), new Range (90, 96), new Range (50, 300), new Range (30, 200),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.EKG_Rhythm__Atrial_Fibrillation (p, s.Lead, p.HR, 0f, .3f, p.HR / 2)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.SpO2_Rhythm (p, p.HR, 1f)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.ABP_Rhythm (p, p.HR, 1f)); }),

            new _Rhythm (Cardiac_Rhythm.Atrial_Flutter, "Atrial Flutter", "AFLUT",
                true,
                new Range (50, 160), new Range (92, 98), new Range (50, 300), new Range (30, 200),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.EKG_Rhythm__Atrial_Flutter (p, s.Lead, p.HR, 0f)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.SpO2_Rhythm (p, p.HR, 1f)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.ABP_Rhythm (p, p.HR, 1f)); }),

            new _Rhythm (Cardiac_Rhythm.AV_Block__1st_Degree, "AV Block, 1st Degree", "AVB-1D",
                true,
                new Range (30, 120), new Range (94, 100), new Range (50, 300), new Range (30, 200),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.EKG_Rhythm__AV_Block__1st_Degree (p, s.Lead, p.HR, 0f)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.SpO2_Rhythm (p, p.HR, 1f)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.ABP_Rhythm (p, p.HR, 1f)); }),

            new _Rhythm (Cardiac_Rhythm.AV_Block__3rd_Degree, "AV Block, 3rd Degree", "AVB-3D",
                true,
                new Range (30, 100), new Range (88, 96), new Range (50, 300), new Range (30, 200),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.EKG_Rhythm__AV_Block__3rd_Degree (p, s.Lead, p.HR, 0f)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.SpO2_Rhythm (p, p.HR, 1f)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.ABP_Rhythm (p, p.HR, 1f)); }),

            new _Rhythm (Cardiac_Rhythm.AV_Block__Mobitz_II, "AV Block, Mobitz II", "AVB-MOB2",
                true,
                new Range (30, 140), new Range (92, 97), new Range (50, 300), new Range (30, 200),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.EKG_Rhythm__AV_Block__Mobitz_II (p, s.Lead, p.HR, 0f, .3f)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.SpO2_Rhythm (p, p.HR, 1f)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.ABP_Rhythm (p, p.HR, 1f)); }),

            new _Rhythm (Cardiac_Rhythm.AV_Block__Wenckebach, "AV Block, Wenckebach", "ABV-WEN",
                true,
                new Range (30, 100),new Range (92, 98), new Range (50, 300), new Range (30, 200),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.EKG_Rhythm__AV_Block__Wenckebach (p, s.Lead, p.HR, 0f, 4)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.SpO2_Rhythm (p, p.HR, 1f)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.ABP_Rhythm (p, p.HR, 1f)); }),

            new _Rhythm (Cardiac_Rhythm.Block__Bundle_Branch, "Bundle Branch Block", "BBB",
                true,
                new Range (30, 140), new Range (94, 100), new Range (50, 300), new Range (30, 200),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.EKG_Rhythm__Block__Bundle_Branch (p, s.Lead, p.HR, 0f)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.SpO2_Rhythm (p, p.HR, 1f)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.ABP_Rhythm (p, p.HR, 1f)); }),

            new _Rhythm (Cardiac_Rhythm.Idioventricular, "Idioventricular", "IDIO",
                true,
                new Range (20, 60), new Range (85, 95), new Range (50, 140), new Range (30, 100),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.EKG_Rhythm__Idioventricular (p, s.Lead, p.HR, 0f)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.SpO2_Rhythm (p, p.HR, 1f)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.ABP_Rhythm (p, p.HR, 1f)); }),

            new _Rhythm (Cardiac_Rhythm.Junctional, "Junctional", "JUNC",
                true,
                new Range (40, 100), new Range (90, 96), new Range (50, 300), new Range (30, 200),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.EKG_Rhythm__Junctional (p, s.Lead, p.HR, 0f)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.SpO2_Rhythm (p, p.HR, 1f)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.ABP_Rhythm (p, p.HR, 1f)); }),

            new _Rhythm (Cardiac_Rhythm.Normal_Sinus, "Normal Sinus", "NSR",
                true,
                new Range (60, 100), new Range (95, 100), new Range (50, 300), new Range (30, 200),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.EKG_Rhythm__Normal_Sinus (p, s.Lead, p.HR, 0f)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.SpO2_Rhythm (p, p.HR, 1f)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.ABP_Rhythm (p, p.HR, 1f)); }),

            new _Rhythm (Cardiac_Rhythm.Premature_Atrial_Contractions, "Premature Atrial Contractions", "PAC",
                true,
                new Range (60, 100), new Range (95, 100), new Range (50, 300), new Range (30, 200),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.EKG_Rhythm__Premature_Atrial_Contractions (p, s.Lead, p.HR, 0f, .4f, p.HR / 2)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.SpO2_Rhythm (p, p.HR, 1f)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.ABP_Rhythm (p, p.HR, 1f)); }),

            new _Rhythm (Cardiac_Rhythm.Premature_Junctional_Contractions, "Premature Junctional Contractions", "PJC",
                true,
                new Range (60, 100), new Range (92, 98), new Range (50, 300), new Range (30, 200),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.EKG_Rhythm__Premature_Junctional_Contractions (p, s.Lead, p.HR, 0f, .4f, p.HR / 2)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.SpO2_Rhythm (p, p.HR, 1f)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.ABP_Rhythm (p, p.HR, 1f)); }),

            new _Rhythm (Cardiac_Rhythm.Premature_Ventricular_Contractions, "Premature Ventricular Contractions", "PVC",
                true,
                new Range (60, 100), new Range (92, 98), new Range (50, 300), new Range (30, 200),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.EKG_Rhythm__Premature_Ventricular_Contractions (p, s.Lead, p.HR, 0f, .4f, p.HR / 2)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.SpO2_Rhythm (p, p.HR, 1f)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.ABP_Rhythm (p, p.HR, 1f)); }),

            new _Rhythm (Cardiac_Rhythm.Pulseless_Electrical_Activity, "Pulseless Electrical Activity", "PEA",
                false,
                new Range (40, 120), new Range (0, 0), new Range (0, 30), new Range (0, 15),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.EKG_Rhythm__Normal_Sinus (p, s.Lead, p.HR, 0f)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.SpO2_Rhythm (p, p.HR, 0f)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.ABP_Rhythm (p, p.HR, 0f)); }),

            new _Rhythm (Cardiac_Rhythm.Sinus_Bradycardia, "Sinus Bradycardia", "BRADY",
                true,
                new Range (40, 55), new Range (95, 100), new Range (50, 300), new Range (30, 200),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.EKG_Rhythm__Normal_Sinus (p, s.Lead, p.HR, 0f)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.SpO2_Rhythm (p, p.HR, 1f)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.ABP_Rhythm (p, p.HR, 1f)); }),

            new _Rhythm (Cardiac_Rhythm.Sinus_Tachycardia, "Sinus Tachycardia", "TACHY",
                true,
                new Range (110, 140), new Range (95, 100), new Range (50, 300), new Range (30, 200),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.EKG_Rhythm__Normal_Sinus (p, s.Lead, p.HR, 0f)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.SpO2_Rhythm (p, p.HR, 0.8f)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.ABP_Rhythm (p, p.HR, 1f)); }),

            new _Rhythm (Cardiac_Rhythm.Supraventricular_Tachycardia, "Supraventricular Tachycardia", "SVT",
                true,
                new Range (150, 210), new Range (86, 94), new Range (50, 300), new Range (30, 200),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.EKG_Rhythm__Supraventricular_Tachycardia (p, s.Lead, p.HR, 0f)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.SpO2_Rhythm (p, p.HR, 0.6f)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.ABP_Rhythm (p, p.HR, 1f)); }),

            new _Rhythm (Cardiac_Rhythm.Ventricular_Fibrillation, "Ventricular Fibrillation", "VFIB",
                false,
                new Range (200, 300), new Range (0, 0), new Range (0, 30), new Range (0, 15),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.EKG_Rhythm__Ventricular_Fibrillation (p, s.Lead, 0.5f, 0f)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.SpO2_Rhythm (p, 0.5f, 0f)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.ABP_Rhythm (p, 0.5f, 0f)); }),

            new _Rhythm (Cardiac_Rhythm.Ventricular_Tachycardia, "Ventricular Tachycardia", "VTACH",
                false,
                new Range (100, 160), new Range (0, 0), new Range (0, 30), new Range (0, 15),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.EKG_Rhythm__Ventricular_Tachycardia (p, s.Lead, p.HR, 0f)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.SpO2_Rhythm (p, p.HR, 0f)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.ABP_Rhythm (p, p.HR, 0f)); }),

            new _Rhythm (Cardiac_Rhythm.Ventricular_Standstill, "Ventricular Standstill", "VSS",
                false,
                new Range (30, 100), new Range (0, 0), new Range (0, 30), new Range (0, 15),
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.EKG_Rhythm__Ventricular_Standstill (p, s.Lead, p.HR, 0f)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.SpO2_Rhythm (p, p.HR, 0f)); },
                delegate (Patient p, Strip s) { s.Concatenate(Rhythm.ABP_Rhythm (p, p.HR, 0f)); })
        });
    }
    

    public static class Rhythm {
        public static List<Point> SpO2_Rhythm(Patient _P, float _Rate, float _Amplitude) {
            /* SpO2 during normal sinus perfusion is similar to a sine wave leaning right with dicrotic notch
		     */
            float _Length = 60 / _Rate;
            float _Portion = _Length / 6;

            List<Point> thisBeat = new List<Point>();
            thisBeat = Concatenate (thisBeat, Line (_Length / 2, 0.0f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Curve (_Portion, 0.5f * _Amplitude, 0.4f * _Amplitude, Last(thisBeat)));
            thisBeat = Concatenate (thisBeat, Curve (_Portion * 2, 0.3f * _Amplitude, 0.0f, Last (thisBeat)));
            return thisBeat;
        }
        public static List<Point> ABP_Rhythm (Patient _P, float _Rate, float _Amplitude) {
            /* ABP during normal sinus perfusion is similar to a sine wave leaning right with dicrotic notch
		     */
            float _Length = 60 / _Rate;
            float _Portion = _Length / 4;

            List<Point> thisBeat = new List<Point> ();
            thisBeat = Concatenate (thisBeat, Line (_Portion, 0.2f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Line (_Portion, 0.0f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Curve (_Portion, 0.8f * _Amplitude, 0.7f * _Amplitude, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Curve (_Portion, 0.6f * _Amplitude, 0.4f * _Amplitude, Last (thisBeat)));            
            return thisBeat;
        }
        public static List<Point> EKG_Rhythm__Asystole (Patient _P, Leads Lead, float _Length, float _Isoelectric) {
            /* Asystole is the absence of electrical activity.
		     */

            List<Point> thisBeat = new List<Point> ();
            thisBeat.Add (new Point (0, _Isoelectric));
            thisBeat = Concatenate (thisBeat, Line (_Length, _Isoelectric, new Point (0, _Isoelectric)));
            return thisBeat;
        }
        public static List<Point> EKG_Rhythm__Atrial_Fibrillation (Patient _P, Leads Lead, float _Rate, float _Isoelectric, float _Prematurity, float _Variance) {
            /* Atrial fibrillation is marked by fibrillation in place of any P wave,
             * irregular pattern of ventricular firing.
             */

            _Rate = _.Clamp (_Rate, 1, 160);
            // Determine speed of some waves and segments based on rate using lerp
            float lerpCoeff = _.Clamp (_.InverseLerp (160, 60, _Rate)),
                    QRS = _.Lerp (0.08f, 0.12f, lerpCoeff),
                    QT = _.Lerp (0.235f, 0.4f, lerpCoeff);

            float _Wave = 0f,
                    _Amplitude = 0.02f;
            /* Vary TQ interval since AFIB intermittently leads to shortened RR intervals
             * To maintain the stated rate (that is printed on the cardiac monitor...) the
             * model needs to account for % of time rate is increased with premature firing,
             * and when rate is *not* increased, we need to lengthen the RR interval to compensate.
             */
            float TQ = (60 / (_.RandomFloat(0.0f, 1.0f) <= _Prematurity
                        ? _Rate + _.RandomFloat(0, _Variance)
                        : _Rate - ((1 - _Prematurity) * (_Variance / 2))))
                    - QT;

            List<Point> thisBeat = new List<Point> ();

            while (TQ > 0f) {
                // Fibrillate!
                _Wave = 0.04f;

                thisBeat = Concatenate (thisBeat, Curve (_Wave, _Isoelectric + _Amplitude, _Isoelectric, Last (thisBeat)));
                // Flip the amplitude's sign *without* crawling
                _Amplitude = -_Amplitude;
                TQ -= _Wave;
            }

            thisBeat = Concatenate (thisBeat, ECG_Q (_P, Lead, QRS / 4, _Isoelectric - 0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_R (_P, Lead, QRS / 4, _Isoelectric + 0.9f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_S (_P, Lead, QRS / 4, _Isoelectric - 0.3f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_J (_P, Lead, QRS / 4, _Isoelectric - 0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_ST (_P, Lead, ((QT - QRS) * 2) / 5, _Isoelectric, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_T (_P, Lead, ((QT - QRS) * 3) / 5, _Isoelectric + 0.1f, _Isoelectric, Last (thisBeat)));
            return thisBeat;
        }
        public static List<Point> EKG_Rhythm__Atrial_Flutter(Patient _P, Leads Lead, float _Rate, float _Isoelectric) {
            return EKG_Rhythm__Atrial_Flutter (_P, Lead, _Rate, 4, _Isoelectric);
        }
        public static List<Point> EKG_Rhythm__Atrial_Flutter(Patient _P, Leads Lead, float _Rate, int _Flutters, float _Isoelectric) {
            /* Atrial flutter is normal sinus rhythm with repeated P waves throughout
		     * TP interval. Clamped from 1-160.
		     */

            _Rate = _.Clamp(_Rate, 1, 160);
            _Flutters = _.Clamp(_Flutters, 2, 5);
            // Determine speed of some waves and segments based on rate using lerp
            float lerpCoeff = _.Clamp(_.InverseLerp(160, 60, _Rate)),
                    QRS = _.Lerp(0.08f, 0.12f, lerpCoeff),
                    QT = _.Lerp(0.10f, 0.16f, lerpCoeff),
                    TP = ((60 / _Rate) - QT);

            List<Point> thisBeat = new List<Point>();
            thisBeat = Concatenate(thisBeat, ECG_P(_P, Lead, TP / _Flutters, _Isoelectric + .1f, _Isoelectric, new Point(0, _Isoelectric)));
            for (int i = 1; i < _Flutters; i++)
                thisBeat = Concatenate(thisBeat, ECG_P(_P, Lead, TP / _Flutters, _Isoelectric + .08f, _Isoelectric, Last(thisBeat)));

            thisBeat = Concatenate(thisBeat, ECG_Q(_P, Lead, QRS / 4, _Isoelectric - 0.1f, Last(thisBeat)));
            thisBeat = Concatenate(thisBeat, ECG_R(_P, Lead, QRS / 4, _Isoelectric + 0.9f, Last(thisBeat)));
            thisBeat = Concatenate(thisBeat, ECG_S(_P, Lead, QRS / 4, _Isoelectric - 0.3f, Last(thisBeat)));
            thisBeat = Concatenate(thisBeat, ECG_J(_P, Lead, QRS / 4, _Isoelectric - 0.1f, Last(thisBeat)));
            thisBeat = Concatenate(thisBeat, ECG_ST(_P, Lead, QT - QRS, _Isoelectric, Last(thisBeat)));
            return thisBeat;
        }
        public static List<Point> EKG_Rhythm__AV_Block__1st_Degree (Patient _P, Leads Lead, float _Rate, float _Isoelectric) {
            /* 1st degree AV block consists of normal sinus rhythm with a PR interval > .20 seconds
		     */

            _Rate = _.Clamp (_Rate, 1, 160);
            // Determine speed of some waves and segments based on rate using lerp
            float lerpCoeff = _.Clamp (_.InverseLerp (160, 60, _Rate)),
                    PR = _.Lerp (0.26f, 0.36f, lerpCoeff),
                    QRS = _.Lerp (0.08f, 0.12f, lerpCoeff),
                    QT = _.Lerp (0.235f, 0.4f, lerpCoeff),
                    TP = ((60 / _Rate) - (PR + QT));

            List<Point> thisBeat = new List<Point> ();
            thisBeat = Concatenate (thisBeat, ECG_P (_P, Lead, PR / 3, _Isoelectric + .05f, _Isoelectric, new Point (0, _Isoelectric)));
            thisBeat = Concatenate (thisBeat, ECG_PR (_P, Lead, (PR * 2) / 3, _Isoelectric, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_Q (_P, Lead, QRS / 4, _Isoelectric - 0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_R (_P, Lead, QRS / 4, _Isoelectric + 0.9f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_S (_P, Lead, QRS / 4, _Isoelectric - 0.3f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_J (_P, Lead, QRS / 4, _Isoelectric - 0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_ST (_P, Lead, ((QT - QRS) * 2) / 5, _Isoelectric, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_T (_P, Lead, ((QT - QRS) * 3) / 5, _Isoelectric + 0.1f, _Isoelectric, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_TP (_P, Lead, TP, _Isoelectric, Last (thisBeat)));
            return thisBeat;
        }
        public static List<Point> EKG_Rhythm__AV_Block__3rd_Degree (Patient _P, Leads Lead, float _Rate, float _Isoelectric) {
            /* 3rd degree AV block consists of a regular P wave and a regular QRS complex, but they
             * both proceed at their own rates- a ratio of 3:2 atrial : ventricular beats.
             */

            _Rate = _.Clamp (_Rate, 1, 130);
            // Determine speed of some waves and segments based on rate using lerp
            float lerpCoeff = _.Clamp (_.InverseLerp (160, 60, _Rate)),
                    PR = _.Lerp (0.16f, 0.2f, lerpCoeff),
                    QRS = _.Lerp (0.08f, 0.12f, lerpCoeff),
                    QT = _.Lerp (0.235f, 0.4f, lerpCoeff),
                    TP = ((60 / _Rate) - (PR + QT));

            List<Point> thisBeat = new List<Point> ();
            thisBeat = Concatenate (thisBeat, ECG_P (_P, Lead, (PR * 2) / 3, _Isoelectric + .05f, _Isoelectric, new Point (0, _Isoelectric)));
            thisBeat = Concatenate (thisBeat, ECG_PR (_P, Lead, PR / 3, _Isoelectric, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_Q (_P, Lead, QRS / 4, _Isoelectric - 0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_R (_P, Lead, QRS / 4, _Isoelectric + 0.9f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_S (_P, Lead, QRS / 4, _Isoelectric - 0.3f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_J (_P, Lead, QRS / 4, _Isoelectric - 0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_ST (_P, Lead, ((QT - QRS) * 2) / 5, _Isoelectric, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_T (_P, Lead, ((QT - QRS) * 3) / 5, _Isoelectric + 0.1f, _Isoelectric, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_TP (_P, Lead, TP / 2, _Isoelectric, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_P (_P, Lead, (PR * 2) / 3, _Isoelectric + .05f, _Isoelectric, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_TP (_P, Lead, TP / 2, _Isoelectric, Last (thisBeat)));

            return thisBeat;
        }        
        public static List<Point> EKG_Rhythm__AV_Block__Mobitz_II (Patient _P, Leads Lead, float _Rate, float _Isoelectric, float _Occurrance) {
            /* AV Block 2nd degree Mobitz Type II is a normal sinus rhythm with occasional
		     * dropped QRS complexes.
		     */

            _Rate = _.Clamp (_Rate, 1, 160);
            // Determine speed of some waves and segments based on rate using lerp
            float lerpCoeff = _.Clamp (_.InverseLerp (160, 60, _Rate)),
                        PR = _.Lerp (0.16f, 0.2f, lerpCoeff),
                        QRS = _.Lerp (0.08f, 0.12f, lerpCoeff),
                        QT = _.Lerp (0.235f, 0.4f, lerpCoeff),
                        TP = ((60 / _Rate) - (PR + QT));

            List<Point> thisBeat = new List<Point> ();
            thisBeat = Concatenate (thisBeat, ECG_P (_P, Lead, (PR * 2) / 3, _Isoelectric + .05f, _Isoelectric, new Point (0, _Isoelectric)));

            if (_.RandomFloat (0.0f, 1.0f) <= _Occurrance)
                thisBeat = Concatenate (thisBeat, Line ((PR / 3) + QT + TP, _Isoelectric, Last (thisBeat)));
            else {
                thisBeat = Concatenate (thisBeat, ECG_PR (_P, Lead, PR / 3, _Isoelectric, Last (thisBeat)));
                thisBeat = Concatenate (thisBeat, ECG_Q (_P, Lead, QRS / 4, _Isoelectric - 0.1f, Last (thisBeat)));
                thisBeat = Concatenate (thisBeat, ECG_R (_P, Lead, QRS / 4, _Isoelectric + 0.9f, Last (thisBeat)));
                thisBeat = Concatenate (thisBeat, ECG_S (_P, Lead, QRS / 4, _Isoelectric - 0.3f, Last (thisBeat)));
                thisBeat = Concatenate (thisBeat, ECG_J (_P, Lead, QRS / 4, _Isoelectric - 0.1f, Last (thisBeat)));
                thisBeat = Concatenate (thisBeat, ECG_ST (_P, Lead, ((QT - QRS) * 2) / 5, _Isoelectric, Last (thisBeat)));
                thisBeat = Concatenate (thisBeat, ECG_T (_P, Lead, ((QT - QRS) * 3) / 5, _Isoelectric + 0.1f, _Isoelectric, Last (thisBeat)));
                thisBeat = Concatenate (thisBeat, ECG_TP (_P, Lead, TP, _Isoelectric, Last (thisBeat)));
            }

            return thisBeat;
        }
        public static List<Point> EKG_Rhythm__AV_Block__Wenckebach (Patient _P, Leads Lead, float _Rate, float _Isoelectric, int _Drops_On = 4) {
            /* AV Block 2nd degree Wenckebach is a normal sinus rhythm marked by lengthening
		     * PQ intervals for 2-4 beats then a dropped QRS complex.
		     * 
		     * Renders amount of beats _Drops_On, PQ interval lengthens and QRS drops on _Drops_On
		     */

            _Rate = _.Clamp (_Rate, 40, 120);
            _Drops_On = _.Clamp (_Drops_On, 2, 4);
            List<Point> thisBeat = new List<Point> ();

            for (int currBeat = 1; currBeat <= _Drops_On; currBeat++) {
                // Determine speed of some waves and segments based on rate using lerp
                float lerpCoeff = _.Clamp (_.InverseLerp (120, 40, _Rate)),
                        PR = _.Lerp (0.16f, 0.2f, lerpCoeff),
                        // PR segment varies due to Wenckebach
                        PRc = (PR / 3) * currBeat,
                        QRS = _.Lerp (0.08f, 0.12f, lerpCoeff),
                        QT = _.Lerp (0.235f, 0.4f, lerpCoeff),
                        TP = ((60 / _Rate) - (PR + QT + PRc));
                
                thisBeat = Concatenate (thisBeat, ECG_P (_P, Lead, (PR * 2) / 3, _Isoelectric + .05f, _Isoelectric, new Point (0, _Isoelectric)));
                thisBeat = Concatenate (thisBeat, ECG_PR (_P, Lead, PRc, _Isoelectric, Last (thisBeat)));

                if (currBeat != _Drops_On) {
                    // Render QRS complex on the beats it's not dropped...
                    thisBeat = Concatenate (thisBeat, ECG_Q (_P, Lead, QRS / 4, _Isoelectric - 0.1f, Last (thisBeat)));
                    thisBeat = Concatenate (thisBeat, ECG_R (_P, Lead, QRS / 4, _Isoelectric + 0.9f, Last (thisBeat)));
                    thisBeat = Concatenate (thisBeat, ECG_S (_P, Lead, QRS / 4, _Isoelectric - 0.3f, Last (thisBeat)));
                    thisBeat = Concatenate (thisBeat, ECG_J (_P, Lead, QRS / 4, _Isoelectric - 0.1f, Last (thisBeat)));
                    thisBeat = Concatenate (thisBeat, ECG_ST (_P, Lead, ((QT - QRS) * 2) / 5, _Isoelectric, Last (thisBeat)));
                    thisBeat = Concatenate (thisBeat, ECG_T (_P, Lead, ((QT - QRS) * 3) / 5, _Isoelectric + 0.1f, _Isoelectric, Last (thisBeat)));
                } else {
                    // If QRS is dropped, add its time to the TP segment to keep the rhythm at a normal rate
                    TP += QRS;
                    TP += QT - QRS;
                }

                thisBeat = Concatenate (thisBeat, ECG_TP (_P, Lead, TP, _Isoelectric, Last (thisBeat)));
            }

            return thisBeat;
        }
        public static List<Point> EKG_Rhythm__Block__Bundle_Branch (Patient _P, Leads Lead, float _Rate, float _Isoelectric) {
            /* Bundle branch blocks are marked by QRS complexes that are widened or split
             * as one ventricle fires slightly after the other.
             */

            _Rate = _.Clamp (_Rate, 1, 160);
            // Determine speed of some waves and segments based on rate using lerp
            float lerpCoeff = _.Clamp (_.InverseLerp (160, 60, _Rate)),
                    PR = _.Lerp (0.16f, 0.2f, lerpCoeff),
                    QRS = _.Lerp (0.12f, 0.22f, lerpCoeff),
                    QT = _.Lerp (0.235f, 0.4f, lerpCoeff),
                    TP = ((60 / _Rate) - (PR + QT));

            List<Point> thisBeat = new List<Point> ();
            thisBeat = Concatenate (thisBeat, ECG_P (_P, Lead, (PR * 2) / 3, _Isoelectric + .05f, _Isoelectric, new Point (0, _Isoelectric)));
            thisBeat = Concatenate (thisBeat, ECG_PR (_P, Lead, PR / 3, _Isoelectric, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_Q (_P, Lead, QRS / 6, _Isoelectric - 0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_R (_P, Lead, QRS / 6, _Isoelectric + 0.9f, Last (thisBeat)));

            thisBeat = Concatenate (thisBeat, ECG_Q (_P, Lead, QRS / 6, _Isoelectric + 0.3f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_R (_P, Lead, QRS / 6, _Isoelectric + 0.7f, Last (thisBeat)));

            thisBeat = Concatenate (thisBeat, ECG_S (_P, Lead, QRS / 6, _Isoelectric - 0.3f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_J (_P, Lead, QRS / 6, _Isoelectric - 0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_ST (_P, Lead, ((QT - QRS) * 2) / 5, _Isoelectric, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_T (_P, Lead, ((QT - QRS) * 3) / 5, _Isoelectric + 0.1f, _Isoelectric, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_TP (_P, Lead, TP, _Isoelectric, Last (thisBeat)));
            return thisBeat;
        }
        public static List<Point> EKG_Rhythm__Idioventricular (Patient _P, Leads Lead, float _Rate, float _Isoelectric) {
            /* Idioventricular rhythms originate in the ventricules (fascicles, bundle branches,
		     * or Bundle of His) and can have different and erratic shapes varying by origin.
		     * Marked by absent P waves, wide and distorted QRS complexes. Regular idioventricular
		     * rhythms run from 15-45 bpm, accelerated idioventricular rhythms are 45-100 bpm.
		     */

            _Rate = _.Clamp (_Rate, 1, 100);
            // Determine speed of some waves and segments based on rate using lerp
            float lerpCoeff = _.Clamp (_.InverseLerp (75, 25, _Rate)),
                    QRS = _.Lerp (0.3f, 0.4f, lerpCoeff),
                    SQ = ((60 / _Rate) - QRS);

            List<Point> thisBeat = new List<Point> ();
            thisBeat = Concatenate (thisBeat, Curve (QRS / 2, 
                _Isoelectric + 1.0f * leadCoeff[(int)Lead, (int)wavePart.Q], 
                _Isoelectric - 0.3f * leadCoeff[(int)Lead, (int)wavePart.Q], Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Curve (QRS / 2, 
                _Isoelectric - 0.3f * leadCoeff[(int)Lead, (int)wavePart.R], 
                _Isoelectric - 0.4f * leadCoeff[(int)Lead, (int)wavePart.R], Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Curve (SQ / 3, 
                _Isoelectric + 0.1f * leadCoeff[(int)Lead, (int)wavePart.T], 
                _Isoelectric * leadCoeff[(int)Lead, (int)wavePart.T], 
                Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Line ((SQ * 2) / 3, _Isoelectric, Last (thisBeat)));
            return thisBeat;
        }
        public static List<Point> EKG_Rhythm__Junctional(Patient _P, Leads Lead, float _Rate, float _Isoelectric) {
            return EKG_Rhythm__Junctional (_P, Lead, _Rate, _Isoelectric, false);
        }
        public static List<Point> EKG_Rhythm__Junctional(Patient _P, Leads Lead, float _Rate, float _Isoelectric, bool _P_Inverted) {
            /* Junctional rhythm is normal sinus with either an absent or inverted P wave,
		     * regularly between 40-60 bpm, with accelerated junctional tachycardia from
		     * 60-115 bpm. Function clamped at 1-130.
		     */

            _Rate = _.Clamp(_Rate, 1, 130);
            // Determine speed of some waves and segments based on rate using lerp
            float lerpCoeff = _.Clamp(_.InverseLerp(130, 60, _Rate)),
                        PR = _.Lerp(0.16f, 0.2f, lerpCoeff),
                        QRS = _.Lerp(0.08f, 0.12f, lerpCoeff),
                        QT = _.Lerp(0.235f, 0.4f, lerpCoeff),
                        TP = ((60 / _Rate) - (PR + QT));

            List<Point> thisBeat = new List<Point>();
            thisBeat = Concatenate(thisBeat, ECG_P(_P, Lead, (PR * 2) / 3,
                (_P_Inverted ? _Isoelectric - .05f : _Isoelectric),
                _Isoelectric, new Point(0, _Isoelectric)));
            thisBeat = Concatenate(thisBeat, ECG_PR(_P, Lead, PR / 3, _Isoelectric, Last(thisBeat)));
            thisBeat = Concatenate(thisBeat, ECG_Q(_P, Lead, QRS / 4, _Isoelectric - 0.1f, Last(thisBeat)));
            thisBeat = Concatenate(thisBeat, ECG_R(_P, Lead, QRS / 4, _Isoelectric + 0.9f, Last(thisBeat)));
            thisBeat = Concatenate(thisBeat, ECG_S(_P, Lead, QRS / 4, _Isoelectric - 0.3f, Last(thisBeat)));
            thisBeat = Concatenate(thisBeat, ECG_J(_P, Lead, QRS / 4, _Isoelectric - 0.1f, Last(thisBeat)));
            thisBeat = Concatenate(thisBeat, ECG_ST(_P, Lead, ((QT - QRS) * 2) / 5, _Isoelectric - 0.06f, Last(thisBeat)));
            thisBeat = Concatenate(thisBeat, ECG_T(_P, Lead, ((QT - QRS) * 3) / 5, _Isoelectric + 0.1f, _Isoelectric, Last(thisBeat)));
            thisBeat = Concatenate(thisBeat, ECG_TP(_P, Lead, TP, _Isoelectric, Last(thisBeat)));
            return thisBeat;
        }
        public static List<Point> EKG_Rhythm__Normal_Sinus (Patient _P, Leads Lead, float _Rate, float _Isoelectric) {
            /* Normal sinus rhythm (NSR) includes bradycardia (1-60), normocardia (60-100), 
		     * and sinus tachycardia (100 - 160)
		     */

            _Rate = _.Clamp (_Rate, 1, 160);
            // Determine speed of some waves and segments based on rate using lerp
            float lerpCoeff = _.Clamp (_.InverseLerp (160, 60, _Rate)),
                    PR = _.Lerp (0.16f, 0.2f, lerpCoeff),
                    QRS = _.Lerp (0.08f, 0.12f, lerpCoeff),
                    QT = _.Lerp (0.235f, 0.4f, lerpCoeff),
                    TP = ((60 / _Rate) - (PR + QT));

            List<Point> thisBeat = new List<Point> ();
            thisBeat = Concatenate (thisBeat, ECG_P (_P, Lead, (PR * 2) / 3, _Isoelectric + .1f, _Isoelectric, new Point (0, _Isoelectric)));
            thisBeat = Concatenate (thisBeat, ECG_PR (_P, Lead, PR / 3, _Isoelectric, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_Q (_P, Lead, QRS / 4, _Isoelectric - 0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_R (_P, Lead, QRS / 4, _Isoelectric + 0.9f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_S (_P, Lead, QRS / 4, _Isoelectric - 0.3f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_J (_P, Lead, QRS / 4, _Isoelectric - 0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_ST (_P, Lead, ((QT - QRS) * 2) / 5, _Isoelectric, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_T (_P, Lead, ((QT - QRS) * 3) / 5, _Isoelectric + 0.2f, _Isoelectric, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_TP (_P, Lead, TP, _Isoelectric, Last (thisBeat)));
            return thisBeat;
        }
        public static List<Point> EKG_Rhythm__Premature_Atrial_Contractions(Patient _P, Leads Lead, float _Rate, float _Isoelectric, float _Occurrance, float _Variance) {
            /* Quick overload for single beat, does not prevent 2 consecutive PACs or lengthen following beat... */
            return EKG_Rhythm__Premature_Atrial_Contractions(_P, Lead, _Rate, 1, _Isoelectric, _Occurrance, _Variance);
        }
        public static List<Point> EKG_Rhythm__Premature_Atrial_Contractions(Patient _P, Leads Lead, float _Rate, int _Beats, float _Isoelectric, float _Occurrance, float _Variance) {
            /* Premature atrial contractions (PAC) are normal sinus rhythm with occasionally shortening 
		     * TP segments, so will just run normal sinus with a random range of heart rate.
		     * Occurrance is percentage chance that a PAC will occur rather than an NSR.
		     */

            bool wasPAC = false;

            List<Point> theseBeats = new List<Point>();
            theseBeats.Add(new Point(0, _Isoelectric));
            for (int i = 0; i < _Beats; i++) {
                // Prevent 2 PAC's from happening consecutively by checking wasPAC
                if ((_.RandomFloat(0.0f, 1.0f) <= _Occurrance) && !wasPAC) {
                    wasPAC = true;
                    return EKG_Rhythm__Normal_Sinus(_P, Lead, _Rate + _Variance, _Isoelectric);
                } else {
                    // If there was a PAC last beat...
                    if (wasPAC) {
                        wasPAC = false;
                        return EKG_Rhythm__Normal_Sinus(_P, Lead, _Rate - (_Variance / 4), _Isoelectric);
                    }
                    // If there was no PAC last beat and no occurrance this beat
                    else if (!wasPAC)
                        return EKG_Rhythm__Normal_Sinus(_P, Lead, _Rate, _Isoelectric);
                }
            }

            return new List<Point>();
        }
        public static List<Point> EKG_Rhythm__Premature_Junctional_Contractions (Patient _P, Leads Lead, float _Rate, float _Isoelectric, float _Occurrance, float _Variance) {
            /* Premature junctional contractions are the occurrance of premature regular QRS complexes
             * with the absence of a P wave- much like a PAC but started in the AV node.
             */

            if (_.RandomFloat(0.0f, 1.0f) <= _Occurrance) {
                return EKG_Rhythm__Junctional (_P, Lead, _Rate - (_Variance / 4), _Isoelectric);
            } else
                return EKG_Rhythm__Normal_Sinus (_P, Lead, _Rate + _Variance, _Isoelectric);
        }
        public static List<Point> EKG_Rhythm__Premature_Ventricular_Contractions (Patient _P, Leads Lead, float _Rate, float _Isoelectric, float _Occurrance, float _Variance) {
            /* Premature ventricular contractions are the occurrance of premature abnormal QRS complexes
             * with the absence of a P wave- much like a PJC but with an abnormally shaped QRS complex.
             */

            if ( _.RandomFloat(0.0f, 1.0f) <= _Occurrance) {
                EKG_Rhythm__Normal_Sinus (_P, Lead, _Rate + _Variance, _Isoelectric);

                _Rate = _Rate - (_Variance / 4);
                // Determine speed of some waves and segments based on rate using lerp
                float lerpCoeff = _.Clamp (_.InverseLerp (160, 60, _Rate)),
                        QRS = _.Lerp (0.14f, 0.22f, lerpCoeff),
                        QT = _.Lerp (0.235f, 0.4f, lerpCoeff),
                        TP = ((60 / _Rate) - QT);

                List<Point> thisBeat = new List<Point> ();

                thisBeat = Concatenate (thisBeat, ECG_ST (_P, Lead, QT - QRS, _Isoelectric, Last (thisBeat)));
                thisBeat = Concatenate (thisBeat, ECG_Q (_P, Lead, QRS / 4, _Isoelectric - 0.8f, Last (thisBeat)));
                thisBeat = Concatenate (thisBeat, ECG_R (_P, Lead, QRS / 4, _Isoelectric - 0.6f, Last (thisBeat)));
                thisBeat = Concatenate (thisBeat, ECG_S (_P, Lead, QRS / 4, _Isoelectric + 0.4f, Last (thisBeat)));
                thisBeat = Concatenate (thisBeat, ECG_J (_P, Lead, QRS / 4, _Isoelectric, Last (thisBeat)));
                thisBeat = Concatenate (thisBeat, ECG_TP (_P, Lead, TP, _Isoelectric, Last (thisBeat)));
                return thisBeat;

            } else
                return EKG_Rhythm__Normal_Sinus (_P, Lead, _Rate, _Isoelectric);
        }
        public static List<Point> EKG_Rhythm__Supraventricular_Tachycardia (Patient _P, Leads Lead, float _Rate, float _Isoelectric) {
            /* Supraventricular tachycardia (SVT) includes heart rates between 160
		     * to 240 beats per minute. Essentially it is NSR without a PR interval
		     * or a P wave (P is mixed with T).
		     */

            _Rate = _.Clamp (_Rate, 160, 240);
            // Determine speed of some waves and segments based on rate using lerp
            float lerpCoeff = _.Clamp (_.InverseLerp (240, 160, _Rate)),
                        PR = _.Lerp (0.03f, 0.05f, lerpCoeff),
                        QRS = _.Lerp (0.05f, 0.08f, lerpCoeff),
                        QT = _.Lerp (0.17f, 0.235f, lerpCoeff);

            List<Point> thisBeat = new List<Point> ();
            thisBeat.Add (new Point (0, _Isoelectric));
            thisBeat = Concatenate (thisBeat, ECG_PR (_P, Lead, PR, _Isoelectric, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_Q (_P, Lead, QRS / 4, _Isoelectric - 0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_R (_P, Lead, QRS / 4, _Isoelectric + 0.9f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_S (_P, Lead, QRS / 4, _Isoelectric - 0.3f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_J (_P, Lead, QRS / 4, _Isoelectric - 0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_ST (_P, Lead, ((QT - QRS) * 2) / 5, _Isoelectric - 0.06f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_T (_P, Lead, ((QT - QRS) * 3) / 5, _Isoelectric + 0.15f, _Isoelectric, Last (thisBeat)));
            return thisBeat;
        }
        public static List<Point> EKG_Rhythm__Ventricular_Tachycardia (Patient _P, Leads Lead, float _Rate, float _Isoelectric) {
            /* Ventricular tachycardia is an accelerated ventricular rhythm.
		     */

            float _Length = 60 / _Rate;

            List<Point> thisBeat = new List<Point> ();
            thisBeat = Concatenate (thisBeat, Curve (_Length / 4,
                _Isoelectric - 0.1f * leadCoeff[(int)Lead, (int)wavePart.Q],
                _Isoelectric - 0.2f * leadCoeff[(int)Lead, (int)wavePart.Q], Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Curve (_Length / 4,
                _Isoelectric - 1f * leadCoeff[(int)Lead, (int)wavePart.R],
                _Isoelectric - 0.3f * leadCoeff[(int)Lead, (int)wavePart.R], Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Curve (_Length / 4,
                _Isoelectric + 0.1f * leadCoeff[(int)Lead, (int)wavePart.T],
                _Isoelectric + 0.1f* leadCoeff[(int)Lead, (int)wavePart.T],
                Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Curve (_Length / 4,
                _Isoelectric + 0.4f * leadCoeff[(int)Lead, (int)wavePart.T],
                _Isoelectric * leadCoeff[(int)Lead, (int)wavePart.T],
                Last (thisBeat)));
            return thisBeat;
        }
        public static List<Point> EKG_Rhythm__Ventricular_Fibrillation(Patient _P, Leads Lead, float _Length, float _Isoelectric) {
            /* Ventricular fibrillation is random peaks/curves with no recognizable waves, no regularity.
		     */

            float _Wave = _Length / 5,
                    _Amplitude = _.RandomFloat(0.3f, 0.6f);
            List<Point> thisBeat = new List<Point>();

            while (_Length > 0f) {
                thisBeat = Concatenate(thisBeat, Curve(_Wave, _Isoelectric + _Amplitude, _Isoelectric, Last(thisBeat)));
                // Flip the sign of amplitude and randomly crawl larger/smaller, models the
                // flippant waves in v-fib.
                _Amplitude = 0 - _.Clamp(_.RandomFloat(_Amplitude - 0.1f, _Amplitude + 0.1f), -1f, 1f);
                _Length -= _Wave;
            }

            return thisBeat;
        }
        public static List<Point> EKG_Rhythm__Ventricular_Standstill(Patient _P, Leads Lead, float _Rate, float _Isoelectric) {
            /* Ventricular standstill is the absence of ventricular activity- only P waves exist
		     */

            _Rate = _.Clamp(_Rate, 1, 160);
            // Determine speed of some waves and segments based on rate using lerp
            float lerpCoeff = _.Clamp(_.InverseLerp(160f, 60f, _Rate)),
                        P = _.Lerp(0.10f, 0.14f, lerpCoeff),
                        TP = ((60 / _Rate) - P);

            List<Point> thisBeat = new List<Point>();
            thisBeat = Concatenate(thisBeat, ECG_P(_P, Lead, P, _Isoelectric + .05f, _Isoelectric, new Point(0, _Isoelectric)));
            thisBeat = Concatenate(thisBeat, ECG_TP(_P, Lead, TP, _Isoelectric, Last(thisBeat)));
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
            int i;
            float x;
            List<Point> _Out = new List<Point>();

            for (i = 1; i * ((2 * _.Draw_Resolve) / _Length) <= 1; i++) {
                x = i * ((2 * _.Draw_Resolve) / _Length);
                _Out.Add(Bezier(new Point(0, _Start.Y), new Point(_Length / 4, _mV), new Point(_Length / 2, _mV), x));
            }

            for (i = 1; i * ((2 * _.Draw_Resolve) / _Length) <= 1; i++) {
                x = i * ((2 * _.Draw_Resolve) / _Length);
                _Out.Add(Bezier(new Point(_Length / 2, _mV), new Point(_Length / 4 * 3, _mV), new Point(_Length, _mV_End), x));
            }

            _Out.Add(new Point(_Length, _mV_End));        // Finish the curve

            return _Out;
        }
        static List<Point> Peak(float _Length, float _mV, float _mV_End, Point _Start) {
            int i;
            float x;
            List<Point> _Out = new List<Point>();

            for (i = 1; i * ((2 * _.Draw_Resolve) / _Length) <= 1; i++) {
                x = i * ((2 * _.Draw_Resolve) / _Length);
                _Out.Add(Bezier(new Point(0, _Start.Y), new Point(_Length / 3, _mV / 1), new Point(_Length / 2, _mV), x));
            }

            for (i = 1; i * ((2 * _.Draw_Resolve) / _Length) <= 1; i++) {
                x = i * ((2 * _.Draw_Resolve) / _Length);
                _Out.Add(Bezier(new Point(_Length / 2, _mV), new Point(_Length / 5 * 3, _mV / 1), new Point(_Length, _mV_End), x));
            }

            _Out.Add(new Point(_Length, _mV_End));        // Finish the curve

            return _Out;
        }
        static List<Point> Line(float _Length, float _mV, Point _Start) {
            List<Point> _Out = new List<Point>();
            _Out.Add(new Point(_Length, _mV));

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