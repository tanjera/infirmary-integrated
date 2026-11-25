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
using II.Rhythm;
using II.Drawing;

namespace II.Waveform {

    public static class Draw {
        public const int ResolutionTime = 10;           // Tracing resolution milliseconds per drawing point
        public const int RefreshTime = 17;              // Tracing draw refresh time in milliseconds (60 fps = ~17ms)

        /*
         * Rhythm amplitude modifications
         */

        private static void VaryAmplitude_Random (double margin, ref double amplitude)
            => amplitude *= (new Random ().NextDouble () * margin) + (1 - margin);

        private static void DampenAmplitude_EctopicBeat (Physiology p, ref double amplitude)
            => amplitude *= p.CardiacRhythm.AberrantBeat ? 0.7d : 1d;

        // Decreases pulsatility based on decreased diastolic filling time. Linear interpolation of where actual HR
        // is compared to set HR (e.g. for tachycardic runs of irregular rhythms, or premature contractions)
        private static void DampenAmplitude_DiastolicFillTime (Physiology p, ref double amplitude) {
            // Utilize actual vital signs from 1 entry ago (because of electromechanical delay, most recent actual HR is for forthcoming pulsatile beat)
            amplitude *= Math.Lerp (1.0d, 0.5d,
                Math.Clamp ((p.GetLastVS (Physiology.PhysiologyEventTypes.CardiacBaseline).HR - p.VitalSignsSettings.HR) / (double)p.VitalSignsSettings.HR));
        }

        private static void DampenAmplitude_PulsusParadoxus (Physiology p, ref double amplitude) {
            if (p.PulsusParadoxus)
                amplitude *=
                    ((!p.MechanicallyVentilated && !p.RespirationInflated)
                    || (p.MechanicallyVentilated && p.RespirationInflated))
                        ? 0.65d : 1.0d;
        }

        private static void DampenAmplitude_PulsusAlternans (Physiology p, ref double amplitude)
            => amplitude *= (p.PulsusAlternans && p.CardiacRhythm.AlternansBeat) ? 0.5d : 1d;

        private static double GetAmplitude_ElectricalAlternans (Physiology? p)
            => ((p?.ElectricalAlternans ?? false) && p.CardiacRhythm.AlternansBeat) ? 0.6d : 1d;

        // For venous rhythms- increased intrathoracic pressure congests venous return
        private static void MagnifyAmplitude_IntrathoracicPressure (Physiology p, ref double amplitude) {
            if (p.MechanicallyVentilated)
                amplitude *= p.RespirationInflated ? 1d : 1.1d;
            else
                amplitude *= !p.RespirationInflated ? 1d : 1.1d;
        }

        /*
         * Generic flat-line
         * Note: Resolution is a coefficient for draw time resolution in milliseconds; e.g. base
         * ResolutionTime (default 10) * _Resolution (2) = 20ms between each PointD plotted
         */

        public static List<PointD> Flat_Line (double length, double isoelectric, double resolution = 1) {
            // Ensure _Resolution as a double does not cause hyper-resolution (clamp at >= 1)
            resolution = System.Math.Max (1, resolution);
            
            return Plotting.Line ((int)(ResolutionTime * resolution), length, isoelectric, new PointD (0, isoelectric));
        }

        /*
         * Pulsatile Rhythms
         */

        public static List<PointD> SPO2_Rhythm (Physiology p, double amplitude) {
            DampenAmplitude_EctopicBeat (p, ref amplitude);
            DampenAmplitude_PulsusAlternans (p, ref amplitude);
            DampenAmplitude_PulsusParadoxus (p, ref amplitude);
            DampenAmplitude_DiastolicFillTime (p, ref amplitude);

            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Stretch_BySystoleRatio (Dictionary.SPO2Rhythm, p), 
                amplitude);
        }

        public static List<PointD> RR_Rhythm (Physiology p, bool inspire, int resolution) {
            double portion = 0.1d;

            List<PointD> thisBeat = new ();
            Plotting.Concatenate (ref thisBeat, Plotting.Line (ResolutionTime * resolution, portion, 0.3d * (inspire ? 1 : -1), Plotting.Last (thisBeat)));
            Plotting.Concatenate (ref thisBeat, Plotting.Line (ResolutionTime * resolution, portion, 0d, Plotting.Last (thisBeat)));
            return thisBeat;
        }

        public static List<PointD> ETCO2_Rhythm (Physiology p) {
            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Stretch (Dictionary.ETCO2Default, p.GetRRIntervalExpiratory),
                Math.InverseLerp(0, 80, Math.Clamp(p.ETCO2, 0, 80)) * 1.5);
        }

        public static List<PointD> ICP_Rhythm (Physiology p, double amplitude) {
            DampenAmplitude_EctopicBeat (p, ref amplitude);
            DampenAmplitude_PulsusAlternans (p, ref amplitude);
            DampenAmplitude_PulsusParadoxus (p, ref amplitude);
            VaryAmplitude_Random (0.1d, ref amplitude);

            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Stretch (  // Lerp to change waveform based on intracranial compliance due to ICP
                    Plot.Lerp (Dictionary.ICPHighCompliance, Dictionary.ICPLowCompliance,
                        Math.Clamp (Math.InverseLerp (15, 25, p.ICP), 0, 1)),    // ICP compliance coefficient
                    p.GetHRInterval),
                amplitude);
        }

        public static List<PointD> IAP_Rhythm (bool isInspiration, Physiology p, double amplitude) {
            DampenAmplitude_EctopicBeat (p, ref amplitude);
            DampenAmplitude_PulsusAlternans (p, ref amplitude);
            DampenAmplitude_PulsusParadoxus (p, ref amplitude);
            VaryAmplitude_Random (0.1d, ref amplitude);

            // Abdominal pressure increase on inspiration
            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Stretch (Dictionary.IAPDefault, 
                    isInspiration ? p.GetRRIntervalInspiratory : p.GetRRIntervalExpiratory),
                amplitude);
        }

        public static List<PointD> ABP_Rhythm (Physiology p, double amplitude) {
            DampenAmplitude_EctopicBeat (p, ref amplitude);
            DampenAmplitude_PulsusAlternans (p, ref amplitude);
            DampenAmplitude_PulsusParadoxus (p, ref amplitude);
            DampenAmplitude_DiastolicFillTime (p, ref amplitude);

            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Stretch_BySystoleRatio(Dictionary.ABPDefault, p),
                amplitude);
        }

        public static List<PointD> CVP_Rhythm (Physiology p, double amplitude) {
            MagnifyAmplitude_IntrathoracicPressure (p, ref amplitude);
            DampenAmplitude_EctopicBeat (p, ref amplitude);
            DampenAmplitude_DiastolicFillTime (p, ref amplitude);

            if (p.CardiacRhythm.HasAtrialPulse && !p.CardiacRhythm.AberrantBeat)
                return Plotting.Concatenate (new List<PointD> (),
                    Plotting.Stretch_BySystoleRatio (Dictionary.CVPAtrioventricular, p),
                    amplitude);
            else
                return Plotting.Concatenate (new List<PointD> (),
                    Plotting.Stretch_BySystoleRatio (Dictionary.CVPVentricular, p),
                    amplitude);
        }

        public static List<PointD> RV_Rhythm (Physiology p, double amplitude) {
            MagnifyAmplitude_IntrathoracicPressure (p, ref amplitude);
            DampenAmplitude_EctopicBeat (p, ref amplitude);

            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Stretch_BySystoleRatio (Dictionary.RVDefault, p),
                amplitude);
        }

        public static List<PointD> PA_Rhythm (Physiology p, double amplitude) {
            MagnifyAmplitude_IntrathoracicPressure (p, ref amplitude);
            DampenAmplitude_EctopicBeat (p, ref amplitude);

            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Stretch_BySystoleRatio (Dictionary.PADefault, p),
                amplitude);
        }

        public static List<PointD> PCW_Rhythm (Physiology p, double amplitude) {
            MagnifyAmplitude_IntrathoracicPressure (p, ref amplitude);
            DampenAmplitude_EctopicBeat (p, ref amplitude);

            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Stretch_BySystoleRatio (Dictionary.PCWDefault, p),
                amplitude);
        }

        public static List<PointD> IABP_Balloon_Rhythm (Physiology p, double amplitude) {
            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Stretch (Dictionary.IABPBalloonDefault,
                Physiology.CalculateHRInterval (p.GetLastVS (Physiology.PhysiologyEventTypes.CardiacVentricularElectric).HR) * 0.7f),
                amplitude);
        }

        public static List<PointD> IABP_ABP_Rhythm (Physiology p, double amplitude) {
            DampenAmplitude_PulsusAlternans (p, ref amplitude);
            DampenAmplitude_PulsusParadoxus (p, ref amplitude);

            if (!p.CardiacRhythm.HasVentricularPulse)
                return Plotting.Concatenate (new List<PointD> (),
                    Plotting.Stretch (Dictionary.IABPArterialNonpulsatile, p.GetHRInterval),
                    amplitude);
            else if (p.CardiacRhythm.AberrantBeat)
                return Plotting.Concatenate (new List<PointD> (),
                    Plotting.Stretch (Dictionary.IABPArterialEctopic, p.GetHRInterval),
                    amplitude);
            else
                return Plotting.Concatenate (new List<PointD> (),
                    Plotting.Stretch (Dictionary.IABPArterialDefault, Physiology.CalculateHRInterval (p.GetLastVS (Physiology.PhysiologyEventTypes.CardiacVentricularElectric).HR)),
                    amplitude);
        }

        public static List<PointD> FHR_Rhythm (Physiology p) {
            return new List<PointD> () {
                new PointD(0, Math.Clamp (Math.InverseLerp (Strip.DefaultScaleMinFHR, Strip.DefaultScaleMaxFHR, p.VitalSignsActual.FetalHR)))
            };
        }

        public static List<PointD> TOCO_Rhythm (Physiology p) {
            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Normalize (
                    Plotting.Stretch (Dictionary.EFMContraction, p.ObstetricContractionDuration),
                    Math.Clamp (Math.InverseLerp (Strip.DefaultScaleMinToco, Strip.DefaultScaleMaxToco, p.ObstetricUterineRestingTone)),
                    Math.Clamp (Math.InverseLerp (Strip.DefaultScaleMinToco, Strip.DefaultScaleMaxToco, p.ObstetricContractionIntensity))));
        }

        /* ********************************************************************
         *
         * ECG Rhythms
         *
         */

        public static List<PointD> ECG_Isoelectric__Atrial_Fibrillation (Physiology? p, Lead? l) {
            if (p is null || l is null)
                return new ();
            
            int fibrillations = (int)System.Math.Ceiling (p.GetHRInterval / 0.08);
            List<PointD> thisBeat = new ();
            for (int i = 1; i < fibrillations; i++)
                // Note: Wave amplitude coefficient is utilized in the individual wave function (e.g. ECG_P())
                thisBeat = Plotting.Concatenate (thisBeat, ECG_P (p, l,
                    Math.RandomDbl (0.04d, 0.12d),                                                                          // fibrillatory wave interval
                    (Math.RandomInt (0, 3) == 0 ? Math.RandomDbl (-0.04d, -0.02d) : Math.RandomDbl (0.02d, 0.06d)),        // fibrillation amplitude
                    0d, Plotting.Last (thisBeat)));

            return thisBeat;
        }

        public static List<PointD> ECG_Isoelectric__Atrial_Flutter (Physiology? p, Lead? l) {
            if (p is null || l is null)
                return new ();

            int flutters = (int)Math.Clamp (System.Math.Floor (60 / p.GetHRInterval * 4), 2, 5);
            double lengthFlutter = p.GetHRInterval / flutters;

            List<PointD> thisBeat = new ();
            for (int i = 1; i < flutters; i++)
                // Note: Wave amplitude coefficient is utilized in the individual wave function (e.g. ECG_P())
                thisBeat = Plotting.Concatenate (thisBeat, ECG_P (p, l, lengthFlutter, .1d,       // Amplitude coefficient 
                    0d, Plotting.Last (thisBeat)));
            return thisBeat;
        }

        public static List<PointD> ECG_Complex__P_Normal (Physiology? p, Lead? l) {
            if (p is null || l is null)
                return new ();

            double pr = Math.Lerp (0.16d, 0.2d, Math.Clamp (Math.InverseLerp (60, 160, p.HR)));
            // Note: Wave amplitude coefficient is utilized in the individual wave function (e.g. ECG_P())
            return ECG_P (p, l, (pr * 2) / 3, .1d,                                                          // Amplitude coefficient
                0d, new PointD (0, 0d));
        }

        public static List<PointD> ECG_Complex__QRST_Normal (Physiology? p, Lead? l) {
            if (p is null || l is null)
                return new ();

            double amplitudeAlternans = GetAmplitude_ElectricalAlternans (p);

            List<PointD> thisBeat = new ();
            // Note: Wave amplitude coefficient is utilized in the individual wave function (e.g. ECG_P())
            thisBeat = Plotting.Concatenate (thisBeat, ECG_Q (p, l, p.QRSInterval / 4, 
                -0.05d * amplitudeAlternans, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_R (p, l, p.QRSInterval / 4, 
                0.9d * amplitudeAlternans, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_S (p, l, p.QRSInterval / 4, 
                -0.3d * amplitudeAlternans, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_J (p, l, p.QRSInterval / 4, 
                -0.1d * amplitudeAlternans, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_ST (p, l, p.GetSTSegment, 
                0d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_T (p, l, p.GetTInterval, 
                0.2d, 0d, Plotting.Last (thisBeat)));
            return thisBeat;
        }

        public static List<PointD> ECG_Complex__QRST_Aberrant_1 (Physiology? p, Lead? l) {
            if (p is null || l is null)
                return new ();

            double amplitudeAlternans = GetAmplitude_ElectricalAlternans (p);

            List<PointD> thisBeat = new ();
            // Note: Wave amplitude coefficient is utilized in the individual wave function (e.g. ECG_P())
            thisBeat = Plotting.Concatenate (thisBeat, ECG_Q (p, l, p.QRSInterval / 6, 
                0.1d * amplitudeAlternans, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_R (p, l, p.QRSInterval / 3, 
                -0.9d * amplitudeAlternans, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_S (p, l, p.QRSInterval / 6, 
                0.3d * amplitudeAlternans, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_J (p, l, p.QRSInterval / 3, 
                0.1d * amplitudeAlternans, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_ST (p, l, p.GetSTSegment, 
                0d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_T (p, l, p.GetTInterval, 
                0.1d, 0d, Plotting.Last (thisBeat)));
            return thisBeat;
        }

        public static List<PointD> ECG_Complex__QRST_Aberrant_2 (Physiology? p, Lead? l) {
            if (p is null || l is null)
                return new ();

            double amplitudeAlternans = GetAmplitude_ElectricalAlternans (p);

            List<PointD> thisBeat = new ();
            // Note: Wave amplitude coefficient is utilized in the individual wave function (e.g. ECG_P())
            thisBeat = Plotting.Concatenate (thisBeat, ECG_Q (p, l, p.QRSInterval / 3, 
                -0.8d * amplitudeAlternans, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_R (p, l, p.QRSInterval / 6, 
                -0.2d * amplitudeAlternans, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_J (p, l, p.QRSInterval / 6, 
                0.1d * amplitudeAlternans, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_ST (p, l, p.GetSTSegment, 
                0d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_T (p, l, p.GetTInterval, 
                -0.1d, 0d, Plotting.Last (thisBeat)));
            return thisBeat;
        }

        public static List<PointD> ECG_Complex__QRST_Aberrant_3 (Physiology? p, Lead? l) {
            if (p is null || l is null)
                return new ();

            double amplitudeAlternans = GetAmplitude_ElectricalAlternans (p);

            List<PointD> thisBeat = new ();
            // Note: Wave amplitude coefficient is utilized in the individual wave function (e.g. ECG_P())
            thisBeat = Plotting.Concatenate (thisBeat, ECG_Q (p, l, p.QRSInterval / 3, 
                0.1d * amplitudeAlternans, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_R (p, l, p.QRSInterval / 6, 
                -0.7d * amplitudeAlternans, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_J (p, l, p.QRSInterval / 6, 
                -0.6d * amplitudeAlternans, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_ST (p, l, p.GetSTSegment, 
                0.1d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_T (p, l, p.GetTInterval, 
                0.3d, 0d, Plotting.Last (thisBeat)));
            return thisBeat;
        }

        public static List<PointD> ECG_Complex__QRST_BBB (Physiology? p, Lead? l) {
            if (p is null || l is null)
                return new ();

            double amplitudeAlternans = GetAmplitude_ElectricalAlternans (p);

            List<PointD> thisBeat = new ();
            // Note: Wave amplitude coefficient is utilized in the individual wave function (e.g. ECG_P())
            thisBeat = Plotting.Concatenate (thisBeat, ECG_Q (p, l, p.QRSInterval / 6, 
                -0.1d * amplitudeAlternans, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_R (p, l, p.QRSInterval / 6, 
                0.9d * amplitudeAlternans, Plotting.Last (thisBeat)));

            thisBeat = Plotting.Concatenate (thisBeat, ECG_Q (p, l, p.QRSInterval / 6, 
                0.3d * amplitudeAlternans, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_R (p, l, p.QRSInterval / 6, 
                0.7d * amplitudeAlternans, Plotting.Last (thisBeat)));

            thisBeat = Plotting.Concatenate (thisBeat, ECG_S (p, l, p.QRSInterval / 6, 
                -0.3d * amplitudeAlternans, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_J (p, l, p.QRSInterval / 6, 
                -0.1d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_ST (p, l, p.GetSTSegment, 
                0d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_T (p, l, p.GetTInterval, 
                0.1d, 0d, Plotting.Last (thisBeat)));
            return thisBeat;
        }

        public static List<PointD> ECG_Complex__QRST_SVT (Physiology? p, Lead? l) {
            if (p is null || l is null)
                return new ();

            double amplitudeAlternans = GetAmplitude_ElectricalAlternans (p);

            List<PointD> thisBeat = new ();
            // Note: Wave amplitude coefficient is utilized in the individual wave function (e.g. ECG_P())
            thisBeat = Plotting.Concatenate (thisBeat, ECG_Q (p, l, p.QRSInterval / 4, 
                -0.1d * amplitudeAlternans, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_R (p, l, p.QRSInterval / 4, 
                0.9d * amplitudeAlternans, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_S (p, l, p.QRSInterval / 4, 
                -0.3d * amplitudeAlternans, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_J (p, l, p.QRSInterval / 4, 
                -0.1d * amplitudeAlternans, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_ST (p, l, p.GetSTSegment, 
                -0.06d, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, ECG_T (p, l, p.GetTInterval, 
                0.15d, 0d, Plotting.Last (thisBeat)));
            return thisBeat;
        }

        public static List<PointD> ECG_Complex__QRST_VT (Physiology? p, Lead? l) {
            if (p is null || l is null)
                return new ();

            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Multiply (
                    // Draws better w/ regular Stretch(); too much cropping/overdraw w/ Stretch_BySystoleRatio
                    Plotting.Stretch (Dictionary.ECGVentricularTachycardia, p.GetSystoleSeconds),
                    BaseLeadCoeff [(int)l.Value, (int)WavePart.QRST] * (p.RAmplitude? [(int)l.Value] ?? 1d)),
                GetAmplitude_ElectricalAlternans (p));
        }

        public static List<PointD> ECG_Complex__QRST_VF (Physiology? p, Lead? l, double amp) {
            if (p is null || l is null)
                return new ();

            double length = p.GetHRInterval,
                    wave = p.GetHRInterval / 2d,
                    amplitude = Math.RandomDbl (0.3d, 0.6d) * amp * (p.RAmplitude?[(int)l.Value] ?? 1d);
            
            List<PointD> thisBeat = new ();
            while (length > 0f) {
                thisBeat = Plotting.Concatenate (thisBeat, 
                    Plotting.Curve (ResolutionTime, wave, 
                        amplitude, 0d, Plotting.Last (thisBeat)));

                // Flip the sign of amplitude and randomly crawl larger/smaller, models the
                // flippant waves in v-fib.
                amplitude = 0 - Math.Clamp (Math.RandomDbl (amplitude - 0.1d, amplitude + 0.1d), -1d, 1d);
                length -= wave;
            }
            return thisBeat;
        }

        public static List<PointD> ECG_Complex__Idioventricular (Physiology? p, Lead? l) {
            if (p is null || l is null)
                return new ();
            
            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Multiply (
                    Plotting.Stretch_BySystoleRatio(Dictionary.ECGIdioventricular, p),
                BaseLeadCoeff [(int)l.Value, (int)WavePart.QRST] * (p.RAmplitude?[(int)l.Value] ?? 1d)),
                GetAmplitude_ElectricalAlternans (p));
        }

        public static List<PointD> ECG_CPR_Artifact (Physiology? p, Lead? l) {
            if (p is null || l is null)
                return new ();
            
            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Multiply (
                    Plotting.Stretch_BySystoleRatio (Dictionary.ECGChestCompressions, p),
                BaseLeadCoeff [(int)l.Value, (int)WavePart.QRST] * (p.RAmplitude?[(int)l.Value] ?? 1d)));
        }

        public static List<PointD> ECG_Defibrillation (Physiology? p, Lead? l) {
            if (p is null || l is null)
                return new ();

            return Plotting.Concatenate (new List<PointD> (),
                Plotting.Multiply (Plotting.Convert (Dictionary.ECGDefibrillation),
                BaseLeadCoeff [(int)l.Value, (int)WavePart.QRST]));
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

        private static List<PointD> ECG_P (Physiology p, Lead l, double len, double mV, double mVEnd, PointD s) {
            return Plotting.Peak (ResolutionTime, len, 
                mV * BaseLeadCoeff [(int)l.Value, (int)WavePart.P] * (p.PAmplitude?[(int)l.Value] ?? 1d), 
                mVEnd, s);
        }

        private static List<PointD> ECG_Q (Physiology p, Lead l, double len, double mV, PointD s) {
            return Plotting.Line (ResolutionTime, len, 
                mV * BaseLeadCoeff [(int)l.Value, (int)WavePart.Q]
                * AxisLeadCoeff [(int)p.CardiacAxis.Value, (int)l.Value, (int)AxisPart.Q]
                * (p.QAmplitude?[(int)l.Value] ?? 1d),
                s);
        }

        private static List<PointD> ECG_R (Physiology p, Lead l, double len, double mV, PointD s) {
            return Plotting.Line (ResolutionTime, len, mV * BaseLeadCoeff [(int)l.Value, (int)WavePart.R]
                * AxisLeadCoeff [(int)p.CardiacAxis.Value, (int)l.Value, (int)AxisPart.R]
                * (p.RAmplitude?[(int)l.Value] ?? 1d), 
                s);
        }

        private static List<PointD> ECG_S (Physiology p, Lead l, double len, double mV, PointD s) {
            return Plotting.Line (ResolutionTime, len, mV * BaseLeadCoeff [(int)l.Value, (int)WavePart.S]
                * AxisLeadCoeff [(int)p.CardiacAxis.Value, (int)l.Value, (int)AxisPart.S]
                * (p.SAmplitude?[(int)l.Value] ?? 1d), 
                s);
        }

        private static List<PointD> ECG_J (Physiology p, Lead l, double len, double mV, PointD s) {
            return Plotting.Line (ResolutionTime, len, 
                (mV * BaseLeadCoeff [(int)l.Value, (int)WavePart.J]) 
                + (p.STElevation?[(int)l.Value] ?? 0), 
                s);
        }

        private static List<PointD> ECG_T (Physiology p, Lead l, double len, double mV, double mVEnd, PointD s) {
            return Plotting.Peak (ResolutionTime, len, 
                (mV * BaseLeadCoeff [(int)l.Value, (int)WavePart.T]) 
                * (p.TAmplitude? [(int)l.Value] ?? 1), 
                mVEnd, s);
        }

        private static List<PointD> ECG_ST (Physiology p, Lead l, double len, double mV, PointD s) {
            return Plotting.Line (ResolutionTime, len, 
                mV + BaseLeadCoeff [(int)l.Value, (int)WavePart.ST] 
                    + (p.STElevation?[(int)l.Value] ?? 0), 
                s);
        }

        /*
         * Coefficients for waveform portions for each lead
         */

        private enum WavePart {
            QRST, P, Q, R, S, J, T, ST
        }

        // Coefficients to transform base lead (Lead 2) into 12 lead ECG
        private static readonly double [,] BaseLeadCoeff = new [,] {
            // P through T are multipliers; segments are additions
            { 0.7d,     0.7d,     0.7d,   0.7d,   0.7d,   0.7d,   0.8d,       0d },     // L1
            { 1d,       1d,       1d,     1d,     1d,     1d,     1d,         0d },     // L2
            { 0.5d,     0.5d,     0.5d,   0.5d,   0.5d,   0.5d,   0.2d,       0d },     // L3
            { -0.9d,    -1d,      -1d,    -0.8d,  -1d,    -1d,    -0.9d,      0d },     // AVR
            { 0.2d,     -1d,      0.3d,   0.2d,   0.4d,   0.3d,   0.6d,       0d },     // AVL
            { 0.8d,     0.7d,     0.8d,   0.8d,   0.8d,   0.8d,   0.4d,       0d },     // AVF
            { -1d,      0.2d,     -0.7d,  -1d,    0d,     0d,     0.3d,       0d },     // V1
            { -1.2d,    0.2d,     -1.8d,  -1.2d,  0d,     -1d,    1.4d,       0.1d },     // V2
            { -1.4d,    0.2d,     -3.0d,  -1.4d,  0d,     0d,     1.8d,       0.1d },     // V3
            { -0.8d,    0.7d,     -9.0d,  -0.8d,  0d,     0d,     1.4d,       0.1d },     // V4
            { -0.2d,    0.7d,     -10.0d, -0.2d,  0d,     0d,     1.0d,       0.1d },     // V5
            { -0.1d,    1d,       -9.0d,  -0.1d,  0d,     0d,     0.8d,       0d }      // V6
        };

        private enum AxisPart {
            Q, R, S
        }

        // Coefficients to modify 12 lead ECG per cardiac axis deviation
        private static readonly double [,,] AxisLeadCoeff = new [,,] {
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