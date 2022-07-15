/* Waveforms.cs
 * Infirmary Integrated
 * By Ibi Keller (Tanjera), (c) 2017
 *
 * All mathematical modeling of waveforms are done in Waveforms.cs, including
 * the plotting of individual waves, the plotting/ordering of multiple waves into a
 * complex, and the transformation of waves across leads (e.g. plotting lead III
 * based on transformations of lead II).
 */

using System;
using System.Collections.Generic;
using System.IO;

using II.Rhythm;
using II.Drawing;

namespace II.Waveform {
    public static class Draw {
        public const int ResolutionTime = 10;           // Tracing resolution milliseconds per drawing point
        public const int RefreshTime = 17;              // Tracing draw refresh time in milliseconds (60 fps = ~17ms)

        private static void VaryAmplitude_Random (double _Margin, ref double _Amplitude)
            => _Amplitude *= ((double)new Random ().NextDouble () * _Margin) + (1 - _Margin);

        private static void DampenAmplitude_EctopicBeat (Patient _P, ref double _Amplitude)
            => _Amplitude *= _P.Cardiac_Rhythm.AberrantBeat ? 0.5d : 1d;

        private static void DampenAmplitude_PulsusAlternans (Patient _P, ref double _Amplitude)
            => _Amplitude *= (_P.Pulsus_Alternans && _P.Cardiac_Rhythm.AlternansBeat) ? 0.5d : 1d;

        private static void DampenAmplitude_PulsusParadoxus (Patient _P, ref double _Amplitude) {
            if (_P.Pulsus_Paradoxus)
                _Amplitude *=
                    ((!_P.Mechanically_Ventilated && !_P.Respiration_Inflated)
                    || (_P.Mechanically_Ventilated && _P.Respiration_Inflated))
                        ? 0.65d : 1.0d;
        }

        private static void DampenAmplitude_IntrathoracicPressure (Patient _P, ref double _Amplitude) {
            if (_P.Mechanically_Ventilated)
                _Amplitude *= _P.Respiration_Inflated ? 1d : 0.7d;
            else
                _Amplitude *= !_P.Respiration_Inflated ? 1d : 0.85d;
        }

        public static List<PointD> Flat_Line (double _Length, double _Isoelectric) {
            return Plotting.Line (ResolutionTime, _Length, _Isoelectric, new PointD (0, _Isoelectric));
        }

        public static List<PointD> SPO2_Rhythm (Patient _P, double _Amplitude) {
            VaryAmplitude_Random (0.1d, ref _Amplitude);
            DampenAmplitude_EctopicBeat (_P, ref _Amplitude);
            DampenAmplitude_PulsusAlternans (_P, ref _Amplitude);
            DampenAmplitude_PulsusParadoxus (_P, ref _Amplitude);

            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Stretch (Dictionary.SPO2_Rhythm, (double)_P.GetHR_Seconds),
                _Amplitude);
        }

        public static List<PointD> RR_Rhythm (Patient _P, bool _Inspire) {
            double _Portion = 0.1d;

            List<PointD> thisBeat = new ();
            Plotting.Concatenate (ref thisBeat, Plotting.Line (ResolutionTime, _Portion, 0.3d * (_Inspire ? 1 : -1), Plotting.Last (thisBeat)));
            Plotting.Concatenate (ref thisBeat, Plotting.Line (ResolutionTime, _Portion, 0d, Plotting.Last (thisBeat)));
            return thisBeat;
        }

        public static List<PointD> ETCO2_Rhythm (Patient _P) {
            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Stretch (Dictionary.ETCO2_Default, (double)_P.GetRR_Seconds_E));
        }

        public static List<PointD> ICP_Rhythm (Patient _P, double _Amplitude) {
            DampenAmplitude_EctopicBeat (_P, ref _Amplitude);
            VaryAmplitude_Random (0.1d, ref _Amplitude);

            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Stretch (  // Lerp to change waveform based on intracranial compliance due to ICP
                    Dictionary.Lerp (Dictionary.ICP_HighCompliance, Dictionary.ICP_LowCompliance,
                        Math.Clamp (Math.InverseLerp (15, 25, _P.ICP), 0, 1)),    // ICP compliance coefficient
                    (double)_P.GetHR_Seconds),
                _Amplitude);
        }

        public static List<PointD> IAP_Rhythm (Patient _P, double _Amplitude) {
            DampenAmplitude_IntrathoracicPressure (_P, ref _Amplitude);
            DampenAmplitude_EctopicBeat (_P, ref _Amplitude);
            VaryAmplitude_Random (0.1d, ref _Amplitude);

            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Stretch (Dictionary.IAP_Default, (double)_P.GetHR_Seconds),
                _Amplitude);
        }

        public static List<PointD> ABP_Rhythm (Patient _P, double _Amplitude) {
            VaryAmplitude_Random (0.1d, ref _Amplitude);

            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Stretch (Dictionary.ABP_Default, (double)_P.GetPulsatility_Seconds),
                _Amplitude);
        }

        public static List<PointD> CVP_Rhythm (Patient _P, double _Amplitude) {
            VaryAmplitude_Random (0.1d, ref _Amplitude);
            DampenAmplitude_IntrathoracicPressure (_P, ref _Amplitude);
            DampenAmplitude_EctopicBeat (_P, ref _Amplitude);

            if (_P.Cardiac_Rhythm.HasPulse_Atrial && !_P.Cardiac_Rhythm.AberrantBeat)
                return Plotting.Concatenate (new List<PointD> (),
                    Plotting.Stretch (Dictionary.CVP_Atrioventricular, (double)_P.GetHR_Seconds),
                    _Amplitude);
            else
                return Plotting.Concatenate (new List<PointD> (),
                    Plotting.Stretch (Dictionary.CVP_Ventricular, (double)_P.GetHR_Seconds),
                    _Amplitude);
        }

        public static List<PointD> RV_Rhythm (Patient _P, double _Amplitude) {
            DampenAmplitude_IntrathoracicPressure (_P, ref _Amplitude);
            DampenAmplitude_EctopicBeat (_P, ref _Amplitude);

            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Stretch (Dictionary.RV_Default, (double)_P.GetPulsatility_Seconds),
                _Amplitude);
        }

        public static List<PointD> PA_Rhythm (Patient _P, double _Amplitude) {
            DampenAmplitude_IntrathoracicPressure (_P, ref _Amplitude);
            DampenAmplitude_EctopicBeat (_P, ref _Amplitude);

            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Stretch (Dictionary.PA_Default, (double)_P.GetPulsatility_Seconds),
                _Amplitude);
        }

        public static List<PointD> PCW_Rhythm (Patient _P, double _Amplitude) {
            DampenAmplitude_IntrathoracicPressure (_P, ref _Amplitude);
            DampenAmplitude_EctopicBeat (_P, ref _Amplitude);

            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Stretch (Dictionary.PCW_Default, (double)_P.GetHR_Seconds),
                _Amplitude);
        }

        public static List<PointD> IABP_Balloon_Rhythm (Patient _P, double _Amplitude) {
            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Stretch (Dictionary.IABP_Balloon_Default, (double)_P.GetHR_Seconds * 0.6f),
                _Amplitude);
        }

        public static List<PointD> IABP_ABP_Rhythm (Patient _P, double _Amplitude) {
            if (!_P.Cardiac_Rhythm.HasPulse_Ventricular)
                return Plotting.Concatenate (new List<PointD> (),
                    Plotting.Stretch (Dictionary.IABP_ABP_Nonpulsatile, (double)_P.GetHR_Seconds),
                    _Amplitude);
            else if (_P.Cardiac_Rhythm.AberrantBeat)
                return Plotting.Concatenate (new List<PointD> (),
                    Plotting.Stretch (Dictionary.IABP_ABP_Ectopic, (double)_P.GetHR_Seconds),
                    _Amplitude);
            else
                return Plotting.Concatenate (new List<PointD> (),
                    Plotting.Stretch (Dictionary.IABP_ABP_Default, (double)_P.GetHR_Seconds),
                    _Amplitude);
        }

        public static List<PointD> FHR_Rhythm (Patient _P, bool isContraction) {
            //if (!isContraction) {               // Baseline FHR tracing
            double fhrAmplitude;
            switch (_P.FHR_Variability.Value) {
                default:
                case Scales.Intensity.Values.Absent: fhrAmplitude = 0.005d; break;
                case Scales.Intensity.Values.Mild: fhrAmplitude = 0.02d; break;
                case Scales.Intensity.Values.Moderate: fhrAmplitude = 0.04d; break;
                case Scales.Intensity.Values.Severe: fhrAmplitude = 0.075d; break;
            }

            double iLerp = Math.Clamp (Math.InverseLerp (Strip.DefaultScaleMin_FHR, Strip.DefaultScaleMax_FHR, _P.FHR));

            return Plotting.Normalize (
                Plotting.Stretch (Dictionary.EFM_Variability, 10f),
                iLerp - fhrAmplitude, iLerp + fhrAmplitude);

            //}
        }

        public static List<PointD> TOCO_Rhythm (Patient _P, bool isContraction) {
            if (!isContraction) {               // Baseline TOCO tracing
                return Flat_Line (60, 0d);
            } else {
                double tocoAmplitude;
                switch (_P.Contraction_Intensity.Value) {
                    default:
                    case Scales.Intensity.Values.Absent: tocoAmplitude = 0d; break;
                    case Scales.Intensity.Values.Mild: tocoAmplitude = 0.33d; break;
                    case Scales.Intensity.Values.Moderate: tocoAmplitude = 0.66d; break;
                    case Scales.Intensity.Values.Severe: tocoAmplitude = 1d; break;
                }

                return Plotting.Concatenate (new List<PointD> (),
                    Plotting.Stretch (Dictionary.EFM_Contraction, _P.Contraction_Duration),
                    tocoAmplitude);
            }
        }

        /* ********************************************************************
         *
         * ECG Rhythms
         *
         */

        public static List<PointD> ECG_Isoelectric__Atrial_Fibrillation (Patient? _P, Lead? _L) {
            if (_P is null || _L is null)
                return new ();

            int Fibrillations = (int)System.Math.Ceiling (_P.GetHR_Seconds / 0.06);

            List<PointD> thisBeat = new ();
            for (int i = 1; i < Fibrillations; i++)
                thisBeat = Plotting.Concatenate (thisBeat, ECG_P (_P, _L, 0.06d, .04d, 0d, Plotting.Last (thisBeat)));
            return thisBeat;
        }

        public static List<PointD> ECG_Isoelectric__Atrial_Flutter (Patient? _P, Lead? _L) {
            if (_P is null || _L is null)
                return new ();

            int Flutters = (int)Math.Clamp (System.Math.Ceiling (60 / _P.GetHR_Seconds * 4), 3, 6);
            double lengthFlutter = _P.GetHR_Seconds / Flutters;

            List<PointD> thisBeat = new ();
            for (int i = 1; i < Flutters; i++)
                thisBeat = Plotting.Concatenate (thisBeat, ECG_P (_P, _L, lengthFlutter, .08d, 0d, Plotting.Last (thisBeat)));
            return thisBeat;
        }

        public static List<PointD> ECG_Complex__P_Normal (Patient? _P, Lead? _L) {
            if (_P is null || _L is null)
                return new ();

            double PR = Math.Lerp (0.16d, 0.2d, Math.Clamp (Math.InverseLerp (60, 160, _P.HR)));
            return ECG_P (_P, _L, (PR * 2) / 3, .1d, 0d, new PointD (0, 0d));
        }

        public static List<PointD> ECG_Complex__QRST_Normal (Patient? _P, Lead? _L) {
            if (_P is null || _L is null)
                return new ();

            double lerpCoeff = Math.Clamp (Math.InverseLerp (60, 160, _P.HR)),
                QRS = Math.Lerp (0.08d, 0.12d, 1 - lerpCoeff),
                QT = Math.Lerp (0.235d, 0.4d, 1 - lerpCoeff);

            List<PointD> thisBeat = new ();
            thisBeat = Plotting.Concatenate (thisBeat, ECG_Q (_P, _L, QRS / 4, -0.05d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_R (_P, _L, QRS / 4, 0.9d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_S (_P, _L, QRS / 4, -0.3d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_J (_P, _L, QRS / 4, -0.1d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_ST (_P, _L, ((QT - QRS) * 2) / 5, 0d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_T (_P, _L, ((QT - QRS) * 3) / 5, 0.2d, 0d, Plotting.Last (thisBeat)));
            return thisBeat;
        }

        public static List<PointD> ECG_Complex__QRST_Aberrant_1 (Patient? _P, Lead? _L) {
            if (_P is null || _L is null)
                return new ();

            double lerpCoeff = Math.Clamp (Math.InverseLerp (60, 160, _P.HR)),
                QRS = Math.Lerp (0.12d, 0.26d, 1 - lerpCoeff),
                QT = Math.Lerp (0.25d, 0.6d, 1 - lerpCoeff);

            List<PointD> thisBeat = new ();
            thisBeat = Plotting.Concatenate (thisBeat, ECG_Q (_P, _L, QRS / 6, 0.1d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_R (_P, _L, QRS / 3, -0.9d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_S (_P, _L, QRS / 6, 0.3d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_J (_P, _L, QRS / 3, 0.1d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_ST (_P, _L, ((QT - QRS) * 2) / 5, 0d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_T (_P, _L, ((QT - QRS) * 3) / 5, 0.1d, 0d, Plotting.Last (thisBeat)));
            return thisBeat;
        }

        public static List<PointD> ECG_Complex__QRST_Aberrant_2 (Patient? _P, Lead? _L) {
            if (_P is null || _L is null)
                return new ();

            double lerpCoeff = Math.Clamp (Math.InverseLerp (60, 160, _P.HR)),
                QRS = Math.Lerp (0.12d, 0.26d, 1 - lerpCoeff),
                QT = Math.Lerp (0.25d, 0.6d, 1 - lerpCoeff);

            List<PointD> thisBeat = new ();
            thisBeat = Plotting.Concatenate (thisBeat, ECG_Q (_P, _L, QRS / 3, -0.8d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_R (_P, _L, QRS / 6, -0.2d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_J (_P, _L, QRS / 6, 0.1d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_ST (_P, _L, ((QT - QRS) * 2) / 5, 0d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_T (_P, _L, ((QT - QRS) * 3) / 5, -0.1d, 0d, Plotting.Last (thisBeat)));
            return thisBeat;
        }

        public static List<PointD> ECG_Complex__QRST_Aberrant_3 (Patient? _P, Lead? _L) {
            if (_P is null || _L is null)
                return new ();

            double lerpCoeff = Math.Clamp (Math.InverseLerp (30, 160, _P.HR)),
                QRS = Math.Lerp (0.12d, 0.26d, 1 - lerpCoeff),
                QT = Math.Lerp (0.25d, 0.6d, 1 - lerpCoeff);

            List<PointD> thisBeat = new ();
            thisBeat = Plotting.Concatenate (thisBeat, ECG_Q (_P, _L, QRS / 3, 0.1d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_R (_P, _L, QRS / 6, -0.7d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_J (_P, _L, QRS / 6, -0.6d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_ST (_P, _L, ((QT - QRS) * 2) / 5, 0.1d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_T (_P, _L, ((QT - QRS) * 3) / 5, 0.3d, 0d, Plotting.Last (thisBeat)));
            return thisBeat;
        }

        public static List<PointD> ECG_Complex__QRST_BBB (Patient? _P, Lead? _L) {
            if (_P is null || _L is null)
                return new ();

            double lerpCoeff = Math.Clamp (Math.InverseLerp (60, 160, _P.HR)),
                QRS = Math.Lerp (0.12d, 0.22d, 1 - lerpCoeff),
                QT = Math.Lerp (0.235d, 0.4d, 1 - lerpCoeff);

            List<PointD> thisBeat = new ();
            thisBeat = Plotting.Concatenate (thisBeat, ECG_Q (_P, _L, QRS / 6, -0.1d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_R (_P, _L, QRS / 6, 0.9d, Plotting.Last (thisBeat)));

            thisBeat = Plotting.Concatenate (thisBeat, ECG_Q (_P, _L, QRS / 6, 0.3d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_R (_P, _L, QRS / 6, 0.7d, Plotting.Last (thisBeat)));

            thisBeat = Plotting.Concatenate (thisBeat, ECG_S (_P, _L, QRS / 6, -0.3d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_J (_P, _L, QRS / 6, -0.1d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_ST (_P, _L, ((QT - QRS) * 2) / 5, 0d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_T (_P, _L, ((QT - QRS) * 3) / 5, 0.1d, 0d, Plotting.Last (thisBeat)));
            return thisBeat;
        }

        public static List<PointD> ECG_Complex__QRST_SVT (Patient? _P, Lead? _L) {
            if (_P is null || _L is null)
                return new ();

            double lerpCoeff = Math.Clamp (Math.InverseLerp (160, 240, _P.HR)),
                        QRS = Math.Lerp (0.05d, 0.12d, 1 - lerpCoeff),
                        QT = Math.Lerp (0.22d, 0.36d, 1 - lerpCoeff);

            List<PointD> thisBeat = new ();
            thisBeat = Plotting.Concatenate (thisBeat, ECG_Q (_P, _L, QRS / 4, -0.1d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_R (_P, _L, QRS / 4, 0.9d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_S (_P, _L, QRS / 4, -0.3d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_J (_P, _L, QRS / 4, -0.1d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_ST (_P, _L, ((QT - QRS) * 1) / 5, -0.06d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_T (_P, _L, ((QT - QRS) * 2) / 5, 0.15d, 0.06d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_T (_P, _L, ((QT - QRS) * 2) / 5, 0.08d, 0d, Plotting.Last (thisBeat)));
            return thisBeat;
        }

        public static List<PointD> ECG_Complex__QRST_VT (Patient? _P, Lead? _L) {
            if (_P is null || _L is null)
                return new ();

            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Multiply (
                    Plotting.Stretch (Dictionary.ECG_Complex_VT, (double)_P.GetHR_Seconds),
                baseLeadCoeff [(int)_L.Value, (int)WavePart.QRST]));
        }

        public static List<PointD> ECG_Complex__QRST_VF (Patient? _P, Lead? _L, double _Amp) {
            if (_P is null || _L is null)
                return new ();

            double _Length = (double)_P.GetHR_Seconds,
                    _Wave = (double)_P.GetHR_Seconds / 2d,
                    _Amplitude = (double)(Math.RandomDouble (0.3d, 0.6d) * _Amp);

            List<PointD> thisBeat = new ();
            while (_Length > 0f) {
                thisBeat = Plotting.Concatenate (thisBeat, Plotting.Curve (ResolutionTime, _Wave, _Amplitude, 0d, Plotting.Last (thisBeat)));

                // Flip the sign of amplitude and randomly crawl larger/smaller, models the
                // flippant waves in v-fib.
                _Amplitude = 0 - (double)Math.Clamp (Math.RandomDouble (_Amplitude - 0.1d, _Amplitude + 0.1d), -1d, 1d);
                _Length -= _Wave;
            }
            return thisBeat;
        }

        public static List<PointD> ECG_Complex__Idioventricular (Patient? _P, Lead? _L) {
            if (_P is null || _L is null)
                return new ();

            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Multiply (
                    Plotting.Stretch (Dictionary.ECG_Complex_Idioventricular, (double)_P.GetHR_Seconds),
                baseLeadCoeff [(int)_L.Value, (int)WavePart.QRST]));
        }

        public static List<PointD> ECG_CPR_Artifact (Patient? _P, Lead? _L) {
            if (_P is null || _L is null)
                return new ();

            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Multiply (
                    Plotting.Stretch (Dictionary.ECG_CPR_Artifact, (double)_P.GetHR_Seconds),
                baseLeadCoeff [(int)_L.Value, (int)WavePart.QRST]));
        }

        public static List<PointD> ECG_Defibrillation (Patient? _P, Lead? _L) {
            if (_P is null || _L is null)
                return new ();

            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Multiply (Plotting.Convert (Dictionary.ECG_Defibrillation),
                baseLeadCoeff [(int)_L.Value, (int)WavePart.QRST]));
        }

        public static List<PointD> ECG_Pacemaker (Patient? _P, Lead? _L) {
            if (_P is null || _L is null)
                return new ();

            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Multiply (Plotting.Convert (Dictionary.ECG_Pacemaker),
                baseLeadCoeff [(int)_L.Value, (int)WavePart.QRST]));
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

        private static List<PointD> ECG_P (Patient p, Lead l, PointD _S) {
            return ECG_P (p, l, .08d, .15d, 0d, _S);
        }

        private static List<PointD> ECG_P (Patient p, Lead l, double _L, double _mV, double _mV_End, PointD _S) {
            return Plotting.Peak (ResolutionTime, _L, _mV * baseLeadCoeff [(int)l.Value, (int)WavePart.P], _mV_End, _S);
        }

        private static List<PointD> ECG_Q (Patient p, Lead l, PointD _S) {
            return ECG_Q (p, l, 1d, -.1d, _S);
        }

        private static List<PointD> ECG_Q (Patient p, Lead l, double _L, double _mV, PointD _S) {
            return Plotting.Line (ResolutionTime, _L, _mV * baseLeadCoeff [(int)l.Value, (int)WavePart.Q]
                * axisLeadCoeff [(int)p.Cardiac_Axis.Value, (int)l.Value, (int)AxisPart.Q], _S);
        }

        private static List<PointD> ECG_R (Patient p, Lead l, PointD _S) {
            return ECG_R (p, l, 1d, .9d, _S);
        }

        private static List<PointD> ECG_R (Patient p, Lead l, double _L, double _mV, PointD _S) {
            return Plotting.Line (ResolutionTime, _L, _mV * baseLeadCoeff [(int)l.Value, (int)WavePart.R]
                * axisLeadCoeff [(int)p.Cardiac_Axis.Value, (int)l.Value, (int)AxisPart.R], _S);
        }

        private static List<PointD> ECG_S (Patient p, Lead l, PointD _S) {
            return ECG_S (p, l, 1d, -.3d, _S);
        }

        private static List<PointD> ECG_S (Patient p, Lead l, double _L, double _mV, PointD _S) {
            return Plotting.Line (ResolutionTime, _L, _mV * baseLeadCoeff [(int)l.Value, (int)WavePart.S]
                * axisLeadCoeff [(int)p.Cardiac_Axis.Value, (int)l.Value, (int)AxisPart.S], _S);
        }

        private static List<PointD> ECG_J (Patient p, Lead l, PointD _S) {
            return ECG_J (p, l, 1d, -.1d, _S);
        }

        private static List<PointD> ECG_J (Patient p, Lead l, double _L, double _mV, PointD _S) {
            return Plotting.Line (ResolutionTime, _L, (_mV * baseLeadCoeff [(int)l.Value, (int)WavePart.J]) + (double)p.ST_Elevation [(int)l.Value], _S);
        }

        private static List<PointD> ECG_T (Patient p, Lead l, PointD _S) {
            return ECG_T (p, l, .16d, .3d, 0d, _S);
        }

        private static List<PointD> ECG_T (Patient p, Lead l, double _L, double _mV, double _mV_End, PointD _S) {
            return Plotting.Peak (ResolutionTime, _L, (_mV * baseLeadCoeff [(int)l.Value, (int)WavePart.T]) + (double)p.T_Elevation [(int)l.Value], _mV_End, _S);
        }

        private static List<PointD> ECG_PR (Patient p, Lead l, PointD _S) {
            return ECG_PR (p, l, .08d, 0d, _S);
        }

        private static List<PointD> ECG_PR (Patient p, Lead l, double _L, double _mV, PointD _S) {
            return Plotting.Line (ResolutionTime, _L, _mV + baseLeadCoeff [(int)l.Value, (int)WavePart.PR], _S);
        }

        private static List<PointD> ECG_ST (Patient p, Lead l, PointD _S) {
            return ECG_ST (p, l, .1d, 0d, _S);
        }

        private static List<PointD> ECG_ST (Patient p, Lead l, double _L, double _mV, PointD _S) {
            return Plotting.Line (ResolutionTime, _L, _mV + baseLeadCoeff [(int)l.Value, (int)WavePart.ST] + (double)p.ST_Elevation [(int)l.Value], _S);
        }

        private static List<PointD> ECG_TP (Patient p, Lead l, PointD _S) {
            return ECG_TP (p, l, .48d, .0d, _S);
        }

        private static List<PointD> ECG_TP (Patient p, Lead l, double _L, double _mV, PointD _S) {
            return Plotting.Line (ResolutionTime, _L, _mV + baseLeadCoeff [(int)l.Value, (int)WavePart.TP], _S);
        }

        /*
         * Coefficients for waveform portions for each lead
         */

        private enum WavePart {
            QRST, P, Q, R, S, J, T, PR, ST, TP
        }

        // Coefficients to transform base lead (Lead 2) into 12 lead ECG
        private static readonly double [,] baseLeadCoeff = new double [,] {
            // P through T are multipliers; segments are additions
            { 0.7d,     0.7d,     0.7d,   0.7d,   0.7d,   0.7d,   0.8d,       0d,     0d,     0d },     // L1
            { 1d,       1d,       1d,     1d,     1d,     1d,     1d,         0d,     0d,     0d },     // L2
            { 0.5d,     0.5d,     0.5d,   0.5d,   0.5d,   0.5d,   0.2d,       0d,     0d,     0d },     // L3
            { -0.9d,    -1d,      -1d,    -0.8d,  -1d,    -1d,    -0.9d,      0d,     0d,     0d },     // AVR
            { 0.2d,     -1d,      0.3d,   0.2d,   0.4d,   0.3d,   0.6d,       0d,     0d,     0d },     // AVL
            { 0.8d,     0.7d,     0.8d,   0.8d,   0.8d,   0.8d,   0.4d,       0d,     0d,     0d },     // AVF
            { -1d,      0.2d,     -0.7d,  -1d,    0d,     0d,     0.3d,       0d,     0d,     0d },     // V1
            { -1.2d,    0.2d,     -1.8d,  -1.2d,  0d,     -1d,    1.4d,       0d,     0.1d,   0d },     // V2
            { -1.4d,    0.2d,     -3.0d,  -1.4d,  0d,     0d,     1.8d,       0d,     0.1d,   0d },     // V3
            { -0.8d,    0.7d,     -9.0d,  -0.8d,  0d,     0d,     1.4d,       0d,     0.1d,   0d },     // V4
            { -0.2d,    0.7d,     -10.0d, -0.2d,  0d,     0d,     1.0d,       0d,     0.1d,   0d },     // V5
            { -0.1d,    1d,       -9.0d,  -0.1d,  0d,     0d,     0.8d,       0d,     0d,     0d }      // V6
        };

        private enum AxisPart {
            Q, R, S
        }

        // Coefficients to modify 12 lead ECG per cardiac axis deviation
        private static readonly double [,,] axisLeadCoeff = new double [,,] {
            // P through T are multipliers; segments are additions
            {   // Normal axis
                {   1d,     1d,     1d  },              // L1
                {   1d,     1d,     1d  },              // L2
                {   1d,     1d,     1d  },              // L3
                {   1d,     1d,     1d  },              // AVR
                {   1d,     1d,     1d  },              // AVL
                {   1d,     1d,     1d  },              // AVF
                {   1d,     1d,     1d  },              // V1
                {   1d,     1d,     1d  },              // V2
                {   1d,     1d,     1d  },              // V3
                {   1d,     1d,     1d  },              // V4
                {   1d,     1d,     1d  },              // V5
                {   1d,     1d,     1d  },              // V6
            },
            {   // Left physiologic (minor) axis
                {   1d,     1d,     1d  },              // L1
                {   1d,     0.5d,   2d  },              // L2
                {  -1d,    -1d,    -1d  },              // L3
                {   1d,     1d,     1d  },              // AVR
                {   1d,     1d,     1d  },              // AVL
                {  -1d,    -1d,    -1d  },              // AVF
                {   1d,     1d,     1d  },              // V1
                {   1d,     1d,     1d  },              // V2
                {   1d,     1d,     1d  },              // V3
                {   1d,     1d,     1d  },              // V4
                {   1d,     1d,     1d  },              // V5
                {   1d,     1d,     1d  },              // V6
            },
            {   // Left pathologic (major) axis
                {   1d,     1d,     1d  },              // L1
                {  -1d,    -1d,    -1d  },              // L2
                {  -1d,    -1d,    -1d  },              // L3
                {   1d,     1d,     1d  },              // AVR
                {   1d,     1d,     1d  },              // AVL
                {  -1d,    -1d,    -1d  },              // AVF
                {   1d,     1d,     1d  },              // V1
                {   1d,     1d,     1d  },              // V2
                {   1d,     1d,     1d  },              // V3
                {   1d,     1d,     1d  },              // V4
                {   1d,     1d,     1d  },              // V5
                {   1d,     1d,     1d  },              // V6
            },
            {   // Right axis
                {  -1d,    -1d,    -1d  },              // L1
                {   1d,     1d,     1d  },              // L2
                {   1d,     1d,     1d  },              // L3
                {   1d,     1d,     1d  },              // AVR
                {   1d,     1d,     1d  },              // AVL
                {   1d,     1d,     1d  },              // AVF
                {   1d,     1d,     1d  },              // V1
                {   1d,     1d,     1d  },              // V2
                {   1d,     1d,     1d  },              // V3
                {   1d,     1d,     1d  },              // V4
                {   1d,     1d,     1d  },              // V5
                {   1d,     1d,     1d  },              // V6
            },
            {   // Extreme axis
                {  -1d,    -1d,    -1d  },              // L1
                {  -1d,    -1d,    -1d  },              // L2
                {  -1d,    -1d,    -1d  },              // L3
                {   1d,     1d,     1d  },              // AVR
                {   1d,     1d,     1d  },              // AVL
                {  -1d,    -1d,    -1d  },              // AVF
                {   1d,     1d,     1d  },              // V1
                {   1d,     1d,     1d  },              // V2
                {   1d,     1d,     1d  },              // V3
                {   1d,     1d,     1d  },              // V4
                {   1d,     1d,     1d  },              // V5
                {   1d,     1d,     1d  },              // V6
            },
            {   // Indeterminate axis
                {   1d,     0.5d,   2d  },              // L1
                {   1d,     0.5d,   2d  },              // L2
                {   1d,     0.5d,   2d  },              // L3
                {   1d,     1d,     1d  },              // AVR
                {   1d,     1d,     1d  },              // AVL
                {   1d,     0.5d,   2d  },              // AVF
                {   1d,     1d,     1d  },              // V1
                {   1d,     1d,     1d  },              // V2
                {   1d,     1d,     1d  },              // V3
                {   1d,     1d,     1d  },              // V4
                {   1d,     1d,     1d  },              // V5
                {   1d,     1d,     1d  },              // V6
            },
        };
    }
}