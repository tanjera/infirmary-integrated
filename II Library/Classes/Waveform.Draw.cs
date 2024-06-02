/* Waveforms.cs
 * Infirmary Integrated
 * By Ibi Keller (Tanjera), (c) 2017-2023
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
using System.Diagnostics;
using System.Linq;

namespace II.Waveform {

    public static class Draw {
        public const int ResolutionTime = 10;           // Tracing resolution milliseconds per drawing point
        public const int RefreshTime = 17;              // Tracing draw refresh time in milliseconds (60 fps = ~17ms)

        /*
         * Rhythm amplitude modifications
         */

        private static void VaryAmplitude_Random (double _Margin, ref double _Amplitude)
            => _Amplitude *= ((double)new Random ().NextDouble () * _Margin) + (1 - _Margin);

        private static void DampenAmplitude_EctopicBeat (Physiology _P, ref double _Amplitude)
            => _Amplitude *= _P.Cardiac_Rhythm.AberrantBeat ? 0.7d : 1d;

        // Decreases pulsatility based on decreased diastolic filling time. Linear interpolation of where actual HR
        // is compared to set HR (e.g. for tachycardic runs of irregular rhythms, or premature contractions)
        private static void DampenAmplitude_DiastolicFillTime (Physiology _P, ref double _Amplitude) {
            // Utilize actual vital signs from 1 entry ago (because of electromechanical delay, most recent actual HR is for forthcoming pulsatile beat)
            _Amplitude *= Math.Lerp (1.0d, 0.5d,
                Math.Clamp ((_P.GetLastVS (Physiology.PhysiologyEventTypes.Cardiac_Baseline).HR - _P.VS_Settings.HR) / (double)_P.VS_Settings.HR));
        }

        private static void DampenAmplitude_PulsusParadoxus (Physiology _P, ref double _Amplitude) {
            if (_P.Pulsus_Paradoxus)
                _Amplitude *=
                    ((!_P.Mechanically_Ventilated && !_P.Respiration_Inflated)
                    || (_P.Mechanically_Ventilated && _P.Respiration_Inflated))
                        ? 0.65d : 1.0d;
        }

        private static void DampenAmplitude_PulsusAlternans (Physiology _P, ref double _Amplitude)
            => _Amplitude *= (_P.Pulsus_Alternans && _P.Cardiac_Rhythm.AlternansBeat) ? 0.5d : 1d;

        private static double GetAmplitude_ElectricalAlternans (Physiology _P)
            => (_P.Electrical_Alternans && _P.Cardiac_Rhythm.AlternansBeat) ? 0.6d : 1d;

        private static void DampenAmplitude_ElectricalAlternans (Physiology _P, ref double _Amplitude)
            => _Amplitude *= (_P.Electrical_Alternans && _P.Cardiac_Rhythm.AlternansBeat) ? 0.6d : 1d;

        // For arterial rhythms- increased intrathoracic pressure impairs cardiac output
        private static void DampenAmplitude_IntrathoracicPressure (Physiology _P, ref double _Amplitude) {
            if (_P.Mechanically_Ventilated)
                _Amplitude *= _P.Respiration_Inflated ? 1d : 0.7d;
            else
                _Amplitude *= !_P.Respiration_Inflated ? 1d : 0.85d;
        }

        // For venous rhythms- increased intrathoracic pressure congests venous return
        private static void MagnifyAmplitude_IntrathoracicPressure (Physiology _P, ref double _Amplitude) {
            if (_P.Mechanically_Ventilated)
                _Amplitude *= _P.Respiration_Inflated ? 1d : 1.1d;
            else
                _Amplitude *= !_P.Respiration_Inflated ? 1d : 1.1d;
        }

        /*
         * Generic flat-line
         * Note: Resolution is a coefficient for draw time resolution in milliseconds; e.g. base
         * ResolutionTime (default 10) * _Resolution (2) = 20ms between each PointD plotted
         */

        public static List<PointD> Flat_Line (double _Length, double _Isoelectric, double _Resolution = 1) {
            return Plotting.Line ((int)(ResolutionTime * _Resolution), _Length, _Isoelectric, new PointD (0, _Isoelectric));
        }

        /*
         * Pulsatile Rhythms
         */

        public static List<PointD> SPO2_Rhythm (Physiology _P, double _Amplitude) {
            DampenAmplitude_EctopicBeat (_P, ref _Amplitude);
            DampenAmplitude_PulsusAlternans (_P, ref _Amplitude);
            DampenAmplitude_PulsusParadoxus (_P, ref _Amplitude);
            DampenAmplitude_DiastolicFillTime (_P, ref _Amplitude);

            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Stretch (Dictionary.SPO2_Rhythm, (double)_P.GetPulsatility_Seconds),
                _Amplitude);
        }

        public static List<PointD> RR_Rhythm (Physiology _P, bool _Inspire, int _Resolution) {
            double _Portion = 0.1d;

            List<PointD> thisBeat = new ();
            Plotting.Concatenate (ref thisBeat, Plotting.Line (ResolutionTime * _Resolution, _Portion, 0.3d * (_Inspire ? 1 : -1), Plotting.Last (thisBeat)));
            Plotting.Concatenate (ref thisBeat, Plotting.Line (ResolutionTime * _Resolution, _Portion, 0d, Plotting.Last (thisBeat)));
            return thisBeat;
        }

        public static List<PointD> ETCO2_Rhythm (Physiology _P) {
            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Stretch (Dictionary.ETCO2_Default, (double)_P.GetRRInterval_Expiratory));
        }

        public static List<PointD> ICP_Rhythm (Physiology _P, double _Amplitude) {
            DampenAmplitude_EctopicBeat (_P, ref _Amplitude);
            DampenAmplitude_PulsusAlternans (_P, ref _Amplitude);
            DampenAmplitude_PulsusParadoxus (_P, ref _Amplitude);
            VaryAmplitude_Random (0.1d, ref _Amplitude);

            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Stretch (  // Lerp to change waveform based on intracranial compliance due to ICP
                    Dictionary.Lerp (Dictionary.ICP_HighCompliance, Dictionary.ICP_LowCompliance,
                        Math.Clamp (Math.InverseLerp (15, 25, _P.ICP), 0, 1)),    // ICP compliance coefficient
                    (double)_P.GetHRInterval),
                _Amplitude);
        }

        public static List<PointD> IAP_Rhythm (Physiology _P, double _Amplitude) {
            DampenAmplitude_EctopicBeat (_P, ref _Amplitude);
            DampenAmplitude_PulsusAlternans (_P, ref _Amplitude);
            DampenAmplitude_PulsusParadoxus (_P, ref _Amplitude);
            VaryAmplitude_Random (0.1d, ref _Amplitude);

            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Stretch (Dictionary.IAP_Default, (double)_P.GetHRInterval),
                _Amplitude);
        }

        public static List<PointD> ABP_Rhythm (Physiology _P, double _Amplitude) {
            DampenAmplitude_EctopicBeat (_P, ref _Amplitude);
            DampenAmplitude_PulsusAlternans (_P, ref _Amplitude);
            DampenAmplitude_PulsusParadoxus (_P, ref _Amplitude);
            DampenAmplitude_DiastolicFillTime (_P, ref _Amplitude);

            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Stretch (Dictionary.ABP_Default, (double)_P.GetPulsatility_Seconds),
                _Amplitude);
        }

        public static List<PointD> CVP_Rhythm (Physiology _P, double _Amplitude) {
            MagnifyAmplitude_IntrathoracicPressure (_P, ref _Amplitude);
            DampenAmplitude_EctopicBeat (_P, ref _Amplitude);
            DampenAmplitude_DiastolicFillTime (_P, ref _Amplitude);

            if (_P.Cardiac_Rhythm.HasPulse_Atrial && !_P.Cardiac_Rhythm.AberrantBeat)
                return Plotting.Concatenate (new List<PointD> (),
                    Plotting.Stretch (Dictionary.CVP_Atrioventricular, (double)_P.GetHRInterval),
                    _Amplitude);
            else
                return Plotting.Concatenate (new List<PointD> (),
                    Plotting.Stretch (Dictionary.CVP_Ventricular, (double)_P.GetHRInterval),
                    _Amplitude);
        }

        public static List<PointD> RV_Rhythm (Physiology _P, double _Amplitude) {
            MagnifyAmplitude_IntrathoracicPressure (_P, ref _Amplitude);
            DampenAmplitude_EctopicBeat (_P, ref _Amplitude);

            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Stretch (Dictionary.RV_Default, (double)_P.GetPulsatility_Seconds),
                _Amplitude);
        }

        public static List<PointD> PA_Rhythm (Physiology _P, double _Amplitude) {
            MagnifyAmplitude_IntrathoracicPressure (_P, ref _Amplitude);
            DampenAmplitude_EctopicBeat (_P, ref _Amplitude);

            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Stretch (Dictionary.PA_Default, (double)_P.GetPulsatility_Seconds),
                _Amplitude);
        }

        public static List<PointD> PCW_Rhythm (Physiology _P, double _Amplitude) {
            MagnifyAmplitude_IntrathoracicPressure (_P, ref _Amplitude);
            DampenAmplitude_EctopicBeat (_P, ref _Amplitude);

            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Stretch (Dictionary.PCW_Default, (double)_P.GetHRInterval),
                _Amplitude);
        }

        public static List<PointD> IABP_Balloon_Rhythm (Physiology _P, double _Amplitude) {
            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Stretch (Dictionary.IABP_Balloon_Default,
                Physiology.CalculateHRInterval (_P.GetLastVS (Physiology.PhysiologyEventTypes.Cardiac_Ventricular_Electric).HR) * 0.7f),
                _Amplitude);
        }

        public static List<PointD> IABP_ABP_Rhythm (Physiology _P, double _Amplitude) {
            DampenAmplitude_PulsusAlternans (_P, ref _Amplitude);
            DampenAmplitude_PulsusParadoxus (_P, ref _Amplitude);

            if (!_P.Cardiac_Rhythm.HasPulse_Ventricular)
                return Plotting.Concatenate (new List<PointD> (),
                    Plotting.Stretch (Dictionary.IABP_ABP_Nonpulsatile, (double)_P.GetHRInterval),
                    _Amplitude);
            else if (_P.Cardiac_Rhythm.AberrantBeat)
                return Plotting.Concatenate (new List<PointD> (),
                    Plotting.Stretch (Dictionary.IABP_ABP_Ectopic, (double)_P.GetHRInterval),
                    _Amplitude);
            else
                return Plotting.Concatenate (new List<PointD> (),
                    Plotting.Stretch (Dictionary.IABP_ABP_Default, Physiology.CalculateHRInterval (_P.GetLastVS (Physiology.PhysiologyEventTypes.Cardiac_Ventricular_Electric).HR)),
                    _Amplitude);
        }

        public static List<PointD> FHR_Rhythm (Physiology _P) {
            return new List<PointD> () {
                new PointD(0, Math.Clamp (Math.InverseLerp (Strip.DefaultScaleMin_FHR, Strip.DefaultScaleMax_FHR, _P.VS_Actual.FetalHR)))
            };
        }

        public static List<PointD> TOCO_Rhythm (Physiology _P) {
            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Normalize (
                    Plotting.Stretch (Dictionary.EFM_Contraction, _P.ObstetricContractionDuration),
                    Math.Clamp (Math.InverseLerp (Strip.DefaultScaleMin_TOCO, Strip.DefaultScaleMax_TOCO, _P.ObstetricUterineRestingTone)),
                    Math.Clamp (Math.InverseLerp (Strip.DefaultScaleMin_TOCO, Strip.DefaultScaleMax_TOCO, _P.ObstetricContractionIntensity))));
        }

        /* ********************************************************************
         *
         * ECG Rhythms
         *
         */

        public static List<PointD> ECG_Isoelectric__Atrial_Fibrillation (Physiology? _P, Lead? _L) {
            if (_P is null || _L is null)
                return new ();

            int Fibrillations = (int)System.Math.Ceiling (_P.GetHRInterval / 0.08);
            List<PointD> thisBeat = new ();
            for (int i = 1; i < Fibrillations; i++)
                thisBeat = Plotting.Concatenate (thisBeat, ECG_P (_P, _L,
                    Math.RandomDbl (0.04d, 0.12d),                                                                          // fibrillatory wave interval
                    (Math.RandomInt (0, 3) == 0 ? Math.RandomDbl (-0.04d, -0.02d) : Math.RandomDbl (0.02d, 0.06d)),         // fibrillation amplitude
                    0d, Plotting.Last (thisBeat)));

            return thisBeat;
        }

        public static List<PointD> ECG_Isoelectric__Atrial_Flutter (Physiology? _P, Lead? _L) {
            if (_P is null || _L is null)
                return new ();

            int Flutters = (int)Math.Clamp (System.Math.Floor (60 / _P.GetHRInterval * 4), 2, 5);
            double lengthFlutter = _P.GetHRInterval / Flutters;

            List<PointD> thisBeat = new ();
            for (int i = 1; i < Flutters; i++)
                thisBeat = Plotting.Concatenate (thisBeat, ECG_P (_P, _L, lengthFlutter, .1d, 0d, Plotting.Last (thisBeat)));
            return thisBeat;
        }

        public static List<PointD> ECG_Complex__P_Normal (Physiology? _P, Lead? _L) {
            if (_P is null || _L is null)
                return new ();

            double PR = Math.Lerp (0.16d, 0.2d, Math.Clamp (Math.InverseLerp (60, 160, _P.HR)));
            return ECG_P (_P, _L, (PR * 2) / 3, .1d, 0d, new PointD (0, 0d));
        }

        public static List<PointD> ECG_Complex__QRST_Normal (Physiology? _P, Lead? _L) {
            if (_P is null || _L is null)
                return new ();

            double ampl_coeff = GetAmplitude_ElectricalAlternans (_P);

            List<PointD> thisBeat = new ();
            thisBeat = Plotting.Concatenate (thisBeat, ECG_Q (_P, _L, _P.QRS_Interval / 4, -0.05d * ampl_coeff, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_R (_P, _L, _P.QRS_Interval / 4, 0.9d * ampl_coeff, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_S (_P, _L, _P.QRS_Interval / 4, -0.3d * ampl_coeff, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_J (_P, _L, _P.QRS_Interval / 4, -0.1d * ampl_coeff, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_ST (_P, _L, _P.GetSTSegment, 0d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_T (_P, _L, _P.GetTInterval, 0.2d, 0d, Plotting.Last (thisBeat)));
            return thisBeat;
        }

        public static List<PointD> ECG_Complex__QRST_Aberrant_1 (Physiology? _P, Lead? _L) {
            if (_P is null || _L is null)
                return new ();

            double ampl_coeff = GetAmplitude_ElectricalAlternans (_P);

            List<PointD> thisBeat = new ();
            thisBeat = Plotting.Concatenate (thisBeat, ECG_Q (_P, _L, _P.QRS_Interval / 6, 0.1d * ampl_coeff, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_R (_P, _L, _P.QRS_Interval / 3, -0.9d * ampl_coeff, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_S (_P, _L, _P.QRS_Interval / 6, 0.3d * ampl_coeff, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_J (_P, _L, _P.QRS_Interval / 3, 0.1d * ampl_coeff, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_ST (_P, _L, _P.GetSTSegment, 0d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_T (_P, _L, _P.GetTInterval, 0.1d, 0d, Plotting.Last (thisBeat)));
            return thisBeat;
        }

        public static List<PointD> ECG_Complex__QRST_Aberrant_2 (Physiology? _P, Lead? _L) {
            if (_P is null || _L is null)
                return new ();

            double ampl_coeff = GetAmplitude_ElectricalAlternans (_P);

            List<PointD> thisBeat = new ();
            thisBeat = Plotting.Concatenate (thisBeat, ECG_Q (_P, _L, _P.QRS_Interval / 3, -0.8d * ampl_coeff, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_R (_P, _L, _P.QRS_Interval / 6, -0.2d * ampl_coeff, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_J (_P, _L, _P.QRS_Interval / 6, 0.1d * ampl_coeff, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_ST (_P, _L, _P.GetSTSegment, 0d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_T (_P, _L, _P.GetTInterval, -0.1d, 0d, Plotting.Last (thisBeat)));
            return thisBeat;
        }

        public static List<PointD> ECG_Complex__QRST_Aberrant_3 (Physiology? _P, Lead? _L) {
            if (_P is null || _L is null)
                return new ();

            double ampl_coeff = GetAmplitude_ElectricalAlternans (_P);

            List<PointD> thisBeat = new ();
            thisBeat = Plotting.Concatenate (thisBeat, ECG_Q (_P, _L, _P.QRS_Interval / 3, 0.1d * ampl_coeff, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_R (_P, _L, _P.QRS_Interval / 6, -0.7d * ampl_coeff, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_J (_P, _L, _P.QRS_Interval / 6, -0.6d * ampl_coeff, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_ST (_P, _L, _P.GetSTSegment, 0.1d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_T (_P, _L, _P.GetTInterval, 0.3d, 0d, Plotting.Last (thisBeat)));
            return thisBeat;
        }

        public static List<PointD> ECG_Complex__QRST_BBB (Physiology? _P, Lead? _L) {
            if (_P is null || _L is null)
                return new ();

            double ampl_coeff = GetAmplitude_ElectricalAlternans (_P);

            List<PointD> thisBeat = new ();
            thisBeat = Plotting.Concatenate (thisBeat, ECG_Q (_P, _L, _P.QRS_Interval / 6, -0.1d * ampl_coeff, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_R (_P, _L, _P.QRS_Interval / 6, 0.9d * ampl_coeff, Plotting.Last (thisBeat)));

            thisBeat = Plotting.Concatenate (thisBeat, ECG_Q (_P, _L, _P.QRS_Interval / 6, 0.3d * ampl_coeff, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_R (_P, _L, _P.QRS_Interval / 6, 0.7d * ampl_coeff, Plotting.Last (thisBeat)));

            thisBeat = Plotting.Concatenate (thisBeat, ECG_S (_P, _L, _P.QRS_Interval / 6, -0.3d * ampl_coeff, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_J (_P, _L, _P.QRS_Interval / 6, -0.1d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_ST (_P, _L, _P.GetSTSegment, 0d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_T (_P, _L, _P.GetTInterval, 0.1d, 0d, Plotting.Last (thisBeat)));
            return thisBeat;
        }

        public static List<PointD> ECG_Complex__QRST_SVT (Physiology? _P, Lead? _L) {
            if (_P is null || _L is null)
                return new ();

            double ampl_coeff = GetAmplitude_ElectricalAlternans (_P);

            List<PointD> thisBeat = new ();
            thisBeat = Plotting.Concatenate (thisBeat, ECG_Q (_P, _L, _P.QRS_Interval / 4, -0.1d * ampl_coeff, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_R (_P, _L, _P.QRS_Interval / 4, 0.9d * ampl_coeff, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_S (_P, _L, _P.QRS_Interval / 4, -0.3d * ampl_coeff, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_J (_P, _L, _P.QRS_Interval / 4, -0.1d * ampl_coeff, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_ST (_P, _L, _P.GetSTSegment, -0.06d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_T (_P, _L, _P.GetTInterval, 0.15d, 0d, Plotting.Last (thisBeat)));
            return thisBeat;
        }

        public static List<PointD> ECG_Complex__QRST_VT (Physiology? _P, Lead? _L) {
            if (_P is null || _L is null)
                return new ();

            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Multiply (
                    Plotting.Stretch (Dictionary.ECG_Complex_VT, (double)_P.GetHRInterval),
                baseLeadCoeff [(int)_L.Value, (int)WavePart.QRST]),
                GetAmplitude_ElectricalAlternans (_P));
        }

        public static List<PointD> ECG_Complex__QRST_VF (Physiology? _P, Lead? _L, double _Amp) {
            if (_P is null || _L is null)
                return new ();

            double _Length = (double)_P.GetHRInterval,
                    _Wave = (double)_P.GetHRInterval / 2d,
                    _Amplitude = (double)(Math.RandomDbl (0.3d, 0.6d) * _Amp);

            List<PointD> thisBeat = new ();
            while (_Length > 0f) {
                thisBeat = Plotting.Concatenate (thisBeat, Plotting.Curve (ResolutionTime, _Wave, _Amplitude, 0d, Plotting.Last (thisBeat)));

                // Flip the sign of amplitude and randomly crawl larger/smaller, models the
                // flippant waves in v-fib.
                _Amplitude = 0 - (double)Math.Clamp (Math.RandomDbl (_Amplitude - 0.1d, _Amplitude + 0.1d), -1d, 1d);
                _Length -= _Wave;
            }
            return thisBeat;
        }

        public static List<PointD> ECG_Complex__Idioventricular (Physiology? _P, Lead? _L) {
            if (_P is null || _L is null)
                return new ();

            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Multiply (
                    Plotting.Stretch (Dictionary.ECG_Complex_Idioventricular, (double)_P.GetHRInterval),
                baseLeadCoeff [(int)_L.Value, (int)WavePart.QRST]),
                GetAmplitude_ElectricalAlternans (_P));
        }

        public static List<PointD> ECG_CPR_Artifact (Physiology? _P, Lead? _L) {
            if (_P is null || _L is null)
                return new ();

            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Multiply (
                    Plotting.Stretch (Dictionary.ECG_CPR_Artifact, (double)_P.GetHRInterval),
                baseLeadCoeff [(int)_L.Value, (int)WavePart.QRST]));
        }

        public static List<PointD> ECG_Defibrillation (Physiology? _P, Lead? _L) {
            if (_P is null || _L is null)
                return new ();

            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Multiply (Plotting.Convert (Dictionary.ECG_Defibrillation),
                baseLeadCoeff [(int)_L.Value, (int)WavePart.QRST]));
        }

        public static List<PointD> ECG_Pacemaker (Physiology? _P, Lead? _L) {
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

        private static List<PointD> ECG_P (Physiology p, Lead l, PointD _S) {
            return ECG_P (p, l, .08d, .15d, 0d, _S);
        }

        private static List<PointD> ECG_P (Physiology p, Lead l, double _L, double _mV, double _mV_End, PointD _S) {
            return Plotting.Peak (ResolutionTime, _L, _mV * baseLeadCoeff [(int)l.Value, (int)WavePart.P], _mV_End, _S);
        }

        private static List<PointD> ECG_Q (Physiology p, Lead l, PointD _S) {
            return ECG_Q (p, l, 1d, -.1d, _S);
        }

        private static List<PointD> ECG_Q (Physiology p, Lead l, double _L, double _mV, PointD _S) {
            return Plotting.Line (ResolutionTime, _L, _mV * baseLeadCoeff [(int)l.Value, (int)WavePart.Q]
                * axisLeadCoeff [(int)p.Cardiac_Axis.Value, (int)l.Value, (int)AxisPart.Q], _S);
        }

        private static List<PointD> ECG_R (Physiology p, Lead l, PointD _S) {
            return ECG_R (p, l, 1d, .9d, _S);
        }

        private static List<PointD> ECG_R (Physiology p, Lead l, double _L, double _mV, PointD _S) {
            return Plotting.Line (ResolutionTime, _L, _mV * baseLeadCoeff [(int)l.Value, (int)WavePart.R]
                * axisLeadCoeff [(int)p.Cardiac_Axis.Value, (int)l.Value, (int)AxisPart.R], _S);
        }

        private static List<PointD> ECG_S (Physiology p, Lead l, PointD _S) {
            return ECG_S (p, l, 1d, -.3d, _S);
        }

        private static List<PointD> ECG_S (Physiology p, Lead l, double _L, double _mV, PointD _S) {
            return Plotting.Line (ResolutionTime, _L, _mV * baseLeadCoeff [(int)l.Value, (int)WavePart.S]
                * axisLeadCoeff [(int)p.Cardiac_Axis.Value, (int)l.Value, (int)AxisPart.S], _S);
        }

        private static List<PointD> ECG_J (Physiology p, Lead l, PointD _S) {
            return ECG_J (p, l, 1d, -.1d, _S);
        }

        private static List<PointD> ECG_J (Physiology p, Lead l, double _L, double _mV, PointD _S) {
            return Plotting.Line (ResolutionTime, _L, (_mV * baseLeadCoeff [(int)l.Value, (int)WavePart.J]) + (double)p.ST_Elevation [(int)l.Value], _S);
        }

        private static List<PointD> ECG_T (Physiology p, Lead l, PointD _S) {
            return ECG_T (p, l, .16d, .3d, 0d, _S);
        }

        private static List<PointD> ECG_T (Physiology p, Lead l, double _L, double _mV, double _mV_End, PointD _S) {
            return Plotting.Peak (ResolutionTime, _L, (_mV * baseLeadCoeff [(int)l.Value, (int)WavePart.T]) + (double)p.T_Elevation [(int)l.Value], _mV_End, _S);
        }

        private static List<PointD> ECG_PR (Physiology p, Lead l, PointD _S) {
            return ECG_PR (p, l, .08d, 0d, _S);
        }

        private static List<PointD> ECG_PR (Physiology p, Lead l, double _L, double _mV, PointD _S) {
            return Plotting.Line (ResolutionTime, _L, _mV + baseLeadCoeff [(int)l.Value, (int)WavePart.PR], _S);
        }

        private static List<PointD> ECG_ST (Physiology p, Lead l, PointD _S) {
            return ECG_ST (p, l, .1d, 0d, _S);
        }

        private static List<PointD> ECG_ST (Physiology p, Lead l, double _L, double _mV, PointD _S) {
            return Plotting.Line (ResolutionTime, _L, _mV + baseLeadCoeff [(int)l.Value, (int)WavePart.ST] + (double)p.ST_Elevation [(int)l.Value], _S);
        }

        private static List<PointD> ECG_TP (Physiology p, Lead l, PointD _S) {
            return ECG_TP (p, l, .48d, .0d, _S);
        }

        private static List<PointD> ECG_TP (Physiology p, Lead l, double _L, double _mV, PointD _S) {
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