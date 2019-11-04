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
using System.Drawing;
using System.IO;

namespace II.Waveform {
    public static class Draw {
        public const int ResolutionTime = 10;           // Tracing resolution milliseconds per drawing point
        public const int RefreshTime = 17;              // Tracing draw refresh time in milliseconds (60 fps = ~17ms)

        private static void VaryAmplitude_Random (float _Margin, ref float _Amplitude)
            => _Amplitude *= ((float)new Random ().NextDouble () * _Margin) + (1 - _Margin);

        private static void DampenAmplitude_EctopicBeat (Patient _P, ref float _Amplitude)
            => _Amplitude *= _P.Cardiac_Rhythm.AberrantBeat ? 0.5f : 1f;

        private static void DampenAmplitude_PulsusAlternans (Patient _P, ref float _Amplitude)
            => _Amplitude *= (_P.Pulsus_Alternans && _P.Cardiac_Rhythm.AlternansBeat) ? 0.5f : 1f;

        private static void DampenAmplitude_PulsusParadoxus (Patient _P, ref float _Amplitude) {
            if (_P.Pulsus_Paradoxus)
                _Amplitude *=
                    ((!_P.Mechanically_Ventilated && !_P.Respiration_Inflated)
                    || (_P.Mechanically_Ventilated && _P.Respiration_Inflated))
                        ? 0.65f : 1.0f;
        }

        private static void DampenAmplitude_IntrathoracicPressure (Patient _P, ref float _Amplitude) {
            if (_P.Mechanically_Ventilated)
                _Amplitude *= _P.Respiration_Inflated ? 1f : 0.7f;
            else
                _Amplitude *= !_P.Respiration_Inflated ? 1f : 0.85f;
        }

        public static List<PointF> Flat_Line (float _Length, float _Isoelectric) {
            return Plotting.Line (ResolutionTime, _Length, _Isoelectric, new PointF (0, _Isoelectric));
        }

        public static List<PointF> SPO2_Rhythm (Patient _P, float _Amplitude) {
            VaryAmplitude_Random (0.1f, ref _Amplitude);
            DampenAmplitude_EctopicBeat (_P, ref _Amplitude);
            DampenAmplitude_PulsusAlternans (_P, ref _Amplitude);
            DampenAmplitude_PulsusParadoxus (_P, ref _Amplitude);

            return Plotting.Concatenate (new List<PointF> (),
                Plotting.Stretch (Dictionary.SPO2_Rhythm, _P.GetHR_Seconds),
                _Amplitude);
        }

        public static List<PointF> RR_Rhythm (Patient _P, bool _Inspire) {
            float _Portion = 0.1f;

            List<PointF> thisBeat = new List<PointF> ();
            Plotting.Concatenate (ref thisBeat, Plotting.Line (ResolutionTime, _Portion, 0.3f * (_Inspire ? 1 : -1), Plotting.Last (thisBeat)));
            Plotting.Concatenate (ref thisBeat, Plotting.Line (ResolutionTime, _Portion, 0f, Plotting.Last (thisBeat)));
            return thisBeat;
        }

        public static List<PointF> ETCO2_Rhythm (Patient _P) {
            return Plotting.Concatenate (new List<PointF> (),
                Plotting.Stretch (Dictionary.ETCO2_Default, _P.GetRR_Seconds_E));
        }

        public static List<PointF> ICP_Rhythm (Patient _P, float _Amplitude) {
            DampenAmplitude_EctopicBeat (_P, ref _Amplitude);
            VaryAmplitude_Random (0.1f, ref _Amplitude);

            return Plotting.Concatenate (new List<PointF> (),
                Plotting.Stretch (  // Lerp to change waveform based on intracranial compliance due to ICP
                    Dictionary.Lerp (Dictionary.ICP_HighCompliance, Dictionary.ICP_LowCompliance,
                        Math.Clamp (Math.InverseLerp (15, 25, _P.ICP), 0, 1)),    // ICP compliance coefficient
                    _P.GetHR_Seconds),
                _Amplitude);
        }

        public static List<PointF> IAP_Rhythm (Patient _P, float _Amplitude) {
            DampenAmplitude_IntrathoracicPressure (_P, ref _Amplitude);
            DampenAmplitude_EctopicBeat (_P, ref _Amplitude);
            VaryAmplitude_Random (0.1f, ref _Amplitude);

            return Plotting.Concatenate (new List<PointF> (),
                Plotting.Stretch (Dictionary.IAP_Default, _P.GetHR_Seconds),
                _Amplitude);
        }

        public static List<PointF> ABP_Rhythm (Patient _P, float _Amplitude) {
            VaryAmplitude_Random (0.1f, ref _Amplitude);

            return Plotting.Concatenate (new List<PointF> (),
                Plotting.Stretch (Dictionary.ABP_Default, _P.GetPulsatility_Seconds),
                _Amplitude);
        }

        public static List<PointF> CVP_Rhythm (Patient _P, float _Amplitude) {
            VaryAmplitude_Random (0.1f, ref _Amplitude);
            DampenAmplitude_IntrathoracicPressure (_P, ref _Amplitude);
            DampenAmplitude_EctopicBeat (_P, ref _Amplitude);

            if (_P.Cardiac_Rhythm.HasPulse_Atrial && !_P.Cardiac_Rhythm.AberrantBeat)
                return Plotting.Concatenate (new List<PointF> (),
                    Plotting.Stretch (Dictionary.CVP_Atrioventricular, _P.GetHR_Seconds),
                    _Amplitude);
            else
                return Plotting.Concatenate (new List<PointF> (),
                    Plotting.Stretch (Dictionary.CVP_Ventricular, _P.GetHR_Seconds),
                    _Amplitude);
        }

        public static List<PointF> RV_Rhythm (Patient _P, float _Amplitude) {
            DampenAmplitude_IntrathoracicPressure (_P, ref _Amplitude);
            DampenAmplitude_EctopicBeat (_P, ref _Amplitude);

            return Plotting.Concatenate (new List<PointF> (),
                Plotting.Stretch (Dictionary.RV_Default, _P.GetPulsatility_Seconds),
                _Amplitude);
        }

        public static List<PointF> PA_Rhythm (Patient _P, float _Amplitude) {
            DampenAmplitude_IntrathoracicPressure (_P, ref _Amplitude);
            DampenAmplitude_EctopicBeat (_P, ref _Amplitude);

            return Plotting.Concatenate (new List<PointF> (),
                Plotting.Stretch (Dictionary.PA_Default, _P.GetPulsatility_Seconds),
                _Amplitude);
        }

        public static List<PointF> PCW_Rhythm (Patient _P, float _Amplitude) {
            DampenAmplitude_IntrathoracicPressure (_P, ref _Amplitude);
            DampenAmplitude_EctopicBeat (_P, ref _Amplitude);

            return Plotting.Concatenate (new List<PointF> (),
                Plotting.Stretch (Dictionary.PCW_Default, _P.GetHR_Seconds),
                _Amplitude);
        }

        public static List<PointF> IABP_Balloon_Rhythm (Patient _P, float _Amplitude) {
            return Plotting.Concatenate (new List<PointF> (),
                Plotting.Stretch (Dictionary.IABP_Balloon_Default, _P.GetHR_Seconds * 0.6f),
                _Amplitude);
        }

        public static List<PointF> IABP_ABP_Rhythm (Patient _P, float _Amplitude) {
            if (!_P.Cardiac_Rhythm.HasPulse_Ventricular)
                return Plotting.Concatenate (new List<PointF> (),
                    Plotting.Stretch (Dictionary.IABP_ABP_Nonpulsatile, _P.GetHR_Seconds),
                    _Amplitude);
            else if (_P.Cardiac_Rhythm.AberrantBeat)
                return Plotting.Concatenate (new List<PointF> (),
                    Plotting.Stretch (Dictionary.IABP_ABP_Ectopic, _P.GetHR_Seconds),
                    _Amplitude);
            else
                return Plotting.Concatenate (new List<PointF> (),
                    Plotting.Stretch (Dictionary.IABP_ABP_Default, _P.GetHR_Seconds),
                    _Amplitude);
        }

        public static List<PointF> ECG_Isoelectric__Atrial_Fibrillation (Patient _P, Lead _L) {
            int Fibrillations = (int)System.Math.Ceiling (_P.GetHR_Seconds / 0.06);

            List<PointF> thisBeat = new List<PointF> ();
            for (int i = 1; i < Fibrillations; i++)
                thisBeat = Plotting.Concatenate (thisBeat, ECG_P (_P, _L, 0.06f, .04f, 0f, Plotting.Last (thisBeat)));
            return thisBeat;
        }

        public static List<PointF> ECG_Isoelectric__Atrial_Flutter (Patient _P, Lead _L) {
            int Flutters = (int)System.Math.Ceiling (_P.GetHR_Seconds / 0.16f);

            List<PointF> thisBeat = new List<PointF> ();
            for (int i = 1; i < Flutters; i++)
                thisBeat = Plotting.Concatenate (thisBeat, ECG_P (_P, _L, 0.16f, .08f, 0f, Plotting.Last (thisBeat)));
            return thisBeat;
        }

        public static List<PointF> ECG_Complex__P_Normal (Patient _P, Lead _L) {
            float PR = Math.Lerp (0.16f, 0.2f, Math.Clamp (Math.InverseLerp (60, 160, _P.HR)));
            return ECG_P (_P, _L, (PR * 2) / 3, .1f, 0f, new PointF (0, 0f));
        }

        public static List<PointF> ECG_Complex__QRST_Normal (Patient _P, Lead _L) {
            float lerpCoeff = Math.Clamp (Math.InverseLerp (60, 160, _P.HR)),
                QRS = Math.Lerp (0.08f, 0.12f, 1 - lerpCoeff),
                QT = Math.Lerp (0.235f, 0.4f, 1 - lerpCoeff);

            List<PointF> thisBeat = new List<PointF> ();
            thisBeat = Plotting.Concatenate (thisBeat, ECG_Q (_P, _L, QRS / 4, -0.05f, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_R (_P, _L, QRS / 4, 0.9f, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_S (_P, _L, QRS / 4, -0.3f, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_J (_P, _L, QRS / 4, -0.1f, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_ST (_P, _L, ((QT - QRS) * 2) / 5, 0f, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_T (_P, _L, ((QT - QRS) * 3) / 5, 0.2f, 0f, Plotting.Last (thisBeat)));
            return thisBeat;
        }

        public static List<PointF> ECG_Complex__QRST_Aberrant_1 (Patient _P, Lead _L) {
            float lerpCoeff = Math.Clamp (Math.InverseLerp (60, 160, _P.HR)),
                QRS = Math.Lerp (0.12f, 0.26f, 1 - lerpCoeff),
                QT = Math.Lerp (0.25f, 0.6f, 1 - lerpCoeff);

            List<PointF> thisBeat = new List<PointF> ();
            thisBeat = Plotting.Concatenate (thisBeat, ECG_Q (_P, _L, QRS / 6, 0.1f, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_R (_P, _L, QRS / 3, -0.9f, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_S (_P, _L, QRS / 6, 0.3f, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_J (_P, _L, QRS / 3, 0.1f, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_ST (_P, _L, ((QT - QRS) * 2) / 5, 0f, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_T (_P, _L, ((QT - QRS) * 3) / 5, 0.1f, 0f, Plotting.Last (thisBeat)));
            return thisBeat;
        }

        public static List<PointF> ECG_Complex__QRST_Aberrant_2 (Patient _P, Lead _L) {
            float lerpCoeff = Math.Clamp (Math.InverseLerp (60, 160, _P.HR)),
                QRS = Math.Lerp (0.12f, 0.26f, 1 - lerpCoeff),
                QT = Math.Lerp (0.25f, 0.6f, 1 - lerpCoeff);

            List<PointF> thisBeat = new List<PointF> ();
            thisBeat = Plotting.Concatenate (thisBeat, ECG_Q (_P, _L, QRS / 3, -0.8f, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_R (_P, _L, QRS / 6, -0.2f, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_J (_P, _L, QRS / 6, 0.1f, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_ST (_P, _L, ((QT - QRS) * 2) / 5, 0f, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_T (_P, _L, ((QT - QRS) * 3) / 5, -0.1f, 0f, Plotting.Last (thisBeat)));
            return thisBeat;
        }

        public static List<PointF> ECG_Complex__QRST_Aberrant_3 (Patient _P, Lead _L) {
            float lerpCoeff = Math.Clamp (Math.InverseLerp (30, 160, _P.HR)),
                QRS = Math.Lerp (0.12f, 0.26f, 1 - lerpCoeff),
                QT = Math.Lerp (0.25f, 0.6f, 1 - lerpCoeff);

            List<PointF> thisBeat = new List<PointF> ();
            thisBeat = Plotting.Concatenate (thisBeat, ECG_Q (_P, _L, QRS / 3, 0.1f, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_R (_P, _L, QRS / 6, -0.7f, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_J (_P, _L, QRS / 6, -0.6f, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_ST (_P, _L, ((QT - QRS) * 2) / 5, 0.1f, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_T (_P, _L, ((QT - QRS) * 3) / 5, 0.3f, 0f, Plotting.Last (thisBeat)));
            return thisBeat;
        }

        public static List<PointF> ECG_Complex__QRST_BBB (Patient _P, Lead _L) {
            float lerpCoeff = Math.Clamp (Math.InverseLerp (60, 160, _P.HR)),
                QRS = Math.Lerp (0.12f, 0.22f, 1 - lerpCoeff),
                QT = Math.Lerp (0.235f, 0.4f, 1 - lerpCoeff);

            List<PointF> thisBeat = new List<PointF> ();
            thisBeat = Plotting.Concatenate (thisBeat, ECG_Q (_P, _L, QRS / 6, -0.1f, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_R (_P, _L, QRS / 6, 0.9f, Plotting.Last (thisBeat)));

            thisBeat = Plotting.Concatenate (thisBeat, ECG_Q (_P, _L, QRS / 6, 0.3f, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_R (_P, _L, QRS / 6, 0.7f, Plotting.Last (thisBeat)));

            thisBeat = Plotting.Concatenate (thisBeat, ECG_S (_P, _L, QRS / 6, -0.3f, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_J (_P, _L, QRS / 6, -0.1f, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_ST (_P, _L, ((QT - QRS) * 2) / 5, 0f, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_T (_P, _L, ((QT - QRS) * 3) / 5, 0.1f, 0f, Plotting.Last (thisBeat)));
            return thisBeat;
        }

        public static List<PointF> ECG_Complex__QRST_SVT (Patient _P, Lead _L) {
            float _Length = _P.GetHR_Seconds;
            float lerpCoeff = Math.Clamp (Math.InverseLerp (160, 240, _P.HR)),
                        QRS = Math.Lerp (0.05f, 0.12f, 1 - lerpCoeff),
                        QT = Math.Lerp (0.22f, 0.36f, 1 - lerpCoeff);

            List<PointF> thisBeat = new List<PointF> ();
            thisBeat = Plotting.Concatenate (thisBeat, ECG_Q (_P, _L, QRS / 4, -0.1f, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_R (_P, _L, QRS / 4, 0.9f, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_S (_P, _L, QRS / 4, -0.3f, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_J (_P, _L, QRS / 4, -0.1f, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_ST (_P, _L, ((QT - QRS) * 1) / 5, -0.06f, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_T (_P, _L, ((QT - QRS) * 2) / 5, 0.15f, 0.06f, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_T (_P, _L, ((QT - QRS) * 2) / 5, 0.08f, 0f, Plotting.Last (thisBeat)));
            return thisBeat;
        }

        public static List<PointF> ECG_Complex__QRST_VT (Patient _P, Lead _L) {
            return Plotting.Concatenate (new List<PointF> (),
                Plotting.Multiply (
                    Plotting.Stretch (Dictionary.ECG_Complex_VT, _P.GetHR_Seconds),
                baseLeadCoeff [(int)_L.Value, (int)WavePart.QRST]));
        }

        public static List<PointF> ECG_Complex__QRST_VF (Patient _P, Lead _L, float _Amp) {
            float _Length = _P.GetHR_Seconds,
                    _Wave = _P.GetHR_Seconds / 2f,
                    _Amplitude = (float)(Math.RandomDouble (0.3f, 0.6f) * _Amp);

            List<PointF> thisBeat = new List<PointF> ();
            while (_Length > 0f) {
                thisBeat = Plotting.Concatenate (thisBeat, Plotting.Curve (ResolutionTime, _Wave, _Amplitude, 0f, Plotting.Last (thisBeat)));

                // Flip the sign of amplitude and randomly crawl larger/smaller, models the
                // flippant waves in v-fib.
                _Amplitude = 0 - (float)Math.Clamp (Math.RandomDouble (_Amplitude - 0.1f, _Amplitude + 0.1f), -1f, 1f);
                _Length -= _Wave;
            }
            return thisBeat;
        }

        public static List<PointF> ECG_Complex__Idioventricular (Patient _P, Lead _L) {
            return Plotting.Concatenate (new List<PointF> (),
                Plotting.Multiply (
                    Plotting.Stretch (Dictionary.ECG_Complex_Idioventricular, _P.GetHR_Seconds),
                baseLeadCoeff [(int)_L.Value, (int)WavePart.QRST]));
        }

        public static List<PointF> ECG_CPR_Artifact (Patient _P, Lead _L) {
            return Plotting.Concatenate (new List<PointF> (),
                Plotting.Multiply (
                    Plotting.Stretch (Dictionary.ECG_CPR_Artifact, _P.GetHR_Seconds),
                baseLeadCoeff [(int)_L.Value, (int)WavePart.QRST]));
        }

        public static List<PointF> ECG_Defibrillation (Patient _P, Lead _L) {
            return Plotting.Concatenate (new List<PointF> (),
                Plotting.Multiply (Plotting.Convert (Dictionary.ECG_Defibrillation),
                baseLeadCoeff [(int)_L.Value, (int)WavePart.QRST]));
        }

        public static List<PointF> ECG_Pacemaker (Patient _P, Lead _L) {
            return Plotting.Concatenate (new List<PointF> (),
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

        private static List<PointF> ECG_P (Patient p, Lead l, PointF _S) {
            return ECG_P (p, l, .08f, .15f, 0f, _S);
        }

        private static List<PointF> ECG_P (Patient p, Lead l, float _L, float _mV, float _mV_End, PointF _S) {
            return Plotting.Peak (ResolutionTime, _L, _mV * baseLeadCoeff [(int)l.Value, (int)WavePart.P], _mV_End, _S);
        }

        private static List<PointF> ECG_Q (Patient p, Lead l, PointF _S) {
            return ECG_Q (p, l, 1f, -.1f, _S);
        }

        private static List<PointF> ECG_Q (Patient p, Lead l, float _L, float _mV, PointF _S) {
            return Plotting.Line (ResolutionTime, _L, _mV * baseLeadCoeff [(int)l.Value, (int)WavePart.Q]
                * axisLeadCoeff [(int)p.Cardiac_Axis.Value, (int)l.Value, (int)AxisPart.Q], _S);
        }

        private static List<PointF> ECG_R (Patient p, Lead l, PointF _S) {
            return ECG_R (p, l, 1f, .9f, _S);
        }

        private static List<PointF> ECG_R (Patient p, Lead l, float _L, float _mV, PointF _S) {
            return Plotting.Line (ResolutionTime, _L, _mV * baseLeadCoeff [(int)l.Value, (int)WavePart.R]
                * axisLeadCoeff [(int)p.Cardiac_Axis.Value, (int)l.Value, (int)AxisPart.R], _S);
        }

        private static List<PointF> ECG_S (Patient p, Lead l, PointF _S) {
            return ECG_S (p, l, 1f, -.3f, _S);
        }

        private static List<PointF> ECG_S (Patient p, Lead l, float _L, float _mV, PointF _S) {
            return Plotting.Line (ResolutionTime, _L, _mV * baseLeadCoeff [(int)l.Value, (int)WavePart.S]
                * axisLeadCoeff [(int)p.Cardiac_Axis.Value, (int)l.Value, (int)AxisPart.S], _S);
        }

        private static List<PointF> ECG_J (Patient p, Lead l, PointF _S) {
            return ECG_J (p, l, 1f, -.1f, _S);
        }

        private static List<PointF> ECG_J (Patient p, Lead l, float _L, float _mV, PointF _S) {
            return Plotting.Line (ResolutionTime, _L, (_mV * baseLeadCoeff [(int)l.Value, (int)WavePart.J]) + p.ST_Elevation [(int)l.Value], _S);
        }

        private static List<PointF> ECG_T (Patient p, Lead l, PointF _S) {
            return ECG_T (p, l, .16f, .3f, 0f, _S);
        }

        private static List<PointF> ECG_T (Patient p, Lead l, float _L, float _mV, float _mV_End, PointF _S) {
            return Plotting.Peak (ResolutionTime, _L, (_mV * baseLeadCoeff [(int)l.Value, (int)WavePart.T]) + p.T_Elevation [(int)l.Value], _mV_End, _S);
        }

        private static List<PointF> ECG_PR (Patient p, Lead l, PointF _S) {
            return ECG_PR (p, l, .08f, 0f, _S);
        }

        private static List<PointF> ECG_PR (Patient p, Lead l, float _L, float _mV, PointF _S) {
            return Plotting.Line (ResolutionTime, _L, _mV + baseLeadCoeff [(int)l.Value, (int)WavePart.PR], _S);
        }

        private static List<PointF> ECG_ST (Patient p, Lead l, PointF _S) {
            return ECG_ST (p, l, .1f, 0f, _S);
        }

        private static List<PointF> ECG_ST (Patient p, Lead l, float _L, float _mV, PointF _S) {
            return Plotting.Line (ResolutionTime, _L, _mV + baseLeadCoeff [(int)l.Value, (int)WavePart.ST] + p.ST_Elevation [(int)l.Value], _S);
        }

        private static List<PointF> ECG_TP (Patient p, Lead l, PointF _S) {
            return ECG_TP (p, l, .48f, .0f, _S);
        }

        private static List<PointF> ECG_TP (Patient p, Lead l, float _L, float _mV, PointF _S) {
            return Plotting.Line (ResolutionTime, _L, _mV + baseLeadCoeff [(int)l.Value, (int)WavePart.TP], _S);
        }

        /*
         * Coefficients for waveform portions for each lead
         */

        private enum WavePart {
            QRST, P, Q, R, S, J, T, PR, ST, TP
        }

        // Coefficients to transform base lead (Lead 2) into 12 lead ECG
        private static float [,] baseLeadCoeff = new float [,] {

            // P through T are multipliers; segments are additions
            { 0.7f,     0.7f,     0.7f,   0.7f,   0.7f,   0.7f,   0.8f,       0f,     0f,     0f },     // L1
            { 1f,       1f,       1f,     1f,     1f,     1f,     1f,         0f,     0f,     0f },     // L2
            { 0.5f,     0.5f,     0.5f,   0.5f,   0.5f,   0.5f,   0.2f,       0f,     0f,     0f },     // L3
            { -0.9f,    -1f,      -1f,    -0.8f,  -1f,    -1f,    -0.9f,      0f,     0f,     0f },     // AVR
            { 0.2f,     -1f,      0.3f,   0.2f,   0.4f,   0.3f,   0.6f,       0f,     0f,     0f },     // AVL
            { 0.8f,     0.7f,     0.8f,   0.8f,   0.8f,   0.8f,   0.4f,       0f,     0f,     0f },     // AVF
            { -1f,      0.2f,     -0.7f,  -1f,    0f,     0f,     0.3f,       0f,     0f,     0f },     // V1
            { -1.2f,    0.2f,     -1.8f,  -1.2f,  0f,     -1f,    1.4f,       0f,     0.1f,   0f },     // V2
            { -1.4f,    0.2f,     -3.0f,  -1.4f,  0f,     0f,     1.8f,       0f,     0.1f,   0f },     // V3
            { -0.8f,    0.7f,     -9.0f,  -0.8f,  0f,     0f,     1.4f,       0f,     0.1f,   0f },     // V4
            { -0.2f,    0.7f,     -10.0f, -0.2f,  0f,     0f,     1.0f,       0f,     0.1f,   0f },     // V5
            { -0.1f,    1f,       -9.0f,  -0.1f,  0f,     0f,     0.8f,       0f,     0f,     0f }      // V6
        };

        private enum AxisPart {
            Q, R, S
        }

        // Coefficients to modify 12 lead ECG per cardiac axis deviation
        private static float [,,] axisLeadCoeff = new float [,,] {

            // P through T are multipliers; segments are additions
            {   // Normal axis
                {   1f,     1f,     1f  },              // L1
                {   1f,     1f,     1f  },              // L2
                {   1f,     1f,     1f  },              // L3
                {   1f,     1f,     1f  },              // AVR
                {   1f,     1f,     1f  },              // AVL
                {   1f,     1f,     1f  },              // AVF
                {   1f,     1f,     1f  },              // V1
                {   1f,     1f,     1f  },              // V2
                {   1f,     1f,     1f  },              // V3
                {   1f,     1f,     1f  },              // V4
                {   1f,     1f,     1f  },              // V5
                {   1f,     1f,     1f  },              // V6
            },
            {   // Left physiologic (minor) axis
                {   1f,     1f,     1f  },              // L1
                {   1f,     0.5f,   2f  },              // L2
                {  -1f,    -1f,    -1f  },              // L3
                {   1f,     1f,     1f  },              // AVR
                {   1f,     1f,     1f  },              // AVL
                {  -1f,    -1f,    -1f  },              // AVF
                {   1f,     1f,     1f  },              // V1
                {   1f,     1f,     1f  },              // V2
                {   1f,     1f,     1f  },              // V3
                {   1f,     1f,     1f  },              // V4
                {   1f,     1f,     1f  },              // V5
                {   1f,     1f,     1f  },              // V6
            },
            {   // Left pathologic (major) axis
                {   1f,     1f,     1f  },              // L1
                {  -1f,    -1f,    -1f  },              // L2
                {  -1f,    -1f,    -1f  },              // L3
                {   1f,     1f,     1f  },              // AVR
                {   1f,     1f,     1f  },              // AVL
                {  -1f,    -1f,    -1f  },              // AVF
                {   1f,     1f,     1f  },              // V1
                {   1f,     1f,     1f  },              // V2
                {   1f,     1f,     1f  },              // V3
                {   1f,     1f,     1f  },              // V4
                {   1f,     1f,     1f  },              // V5
                {   1f,     1f,     1f  },              // V6
            },
            {   // Right axis
                {  -1f,    -1f,    -1f  },              // L1
                {   1f,     1f,     1f  },              // L2
                {   1f,     1f,     1f  },              // L3
                {   1f,     1f,     1f  },              // AVR
                {   1f,     1f,     1f  },              // AVL
                {   1f,     1f,     1f  },              // AVF
                {   1f,     1f,     1f  },              // V1
                {   1f,     1f,     1f  },              // V2
                {   1f,     1f,     1f  },              // V3
                {   1f,     1f,     1f  },              // V4
                {   1f,     1f,     1f  },              // V5
                {   1f,     1f,     1f  },              // V6
            },
            {   // Extreme axis
                {  -1f,    -1f,    -1f  },              // L1
                {  -1f,    -1f,    -1f  },              // L2
                {  -1f,    -1f,    -1f  },              // L3
                {   1f,     1f,     1f  },              // AVR
                {   1f,     1f,     1f  },              // AVL
                {  -1f,    -1f,    -1f  },              // AVF
                {   1f,     1f,     1f  },              // V1
                {   1f,     1f,     1f  },              // V2
                {   1f,     1f,     1f  },              // V3
                {   1f,     1f,     1f  },              // V4
                {   1f,     1f,     1f  },              // V5
                {   1f,     1f,     1f  },              // V6
            },
            {   // Indeterminate axis
                {   1f,     0.5f,   2f  },              // L1
                {   1f,     0.5f,   2f  },              // L2
                {   1f,     0.5f,   2f  },              // L3
                {   1f,     1f,     1f  },              // AVR
                {   1f,     1f,     1f  },              // AVL
                {   1f,     0.5f,   2f  },              // AVF
                {   1f,     1f,     1f  },              // V1
                {   1f,     1f,     1f  },              // V2
                {   1f,     1f,     1f  },              // V3
                {   1f,     1f,     1f  },              // V4
                {   1f,     1f,     1f  },              // V5
                {   1f,     1f,     1f  },              // V6
            },
        };
    }
}