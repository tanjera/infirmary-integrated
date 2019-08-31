using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;

using II;

namespace Unit_Testing {

    [TestClass]
    public class PatientTest {
        /* Default Vital Sign parameters */

        private Patient.Vital_Signs Default_VS = new Patient.Vital_Signs () {
            HR = 100,
            NSBP = 200,
            NDBP = 100,
            NMAP = 150,
            RR = 40,
            SPO2 = 80,
            T = 38.9f,
            ETCO2 = 25,
            CVP = 14,
            ASBP = 200,
            ADBP = 100,
            AMAP = 150,
            PSP = 45,
            PDP = 30,
            PMP = 37,
            ICP = 25,
            IAP = 15,
            RR_IE_I = 1.1f,
            RR_IE_E = 2.2f
        };

        /* Default Patient parameters */
        private static DateTime Updated = DateTime.Now;

        /* Basic vital signs and advanced hemodynamics (partial) */
        private static Cardiac_Rhythms.Values Cardiac_Rhythm = Cardiac_Rhythms.Values.Atrial_Fibrillation;
        private static Respiratory_Rhythms.Values Respiratory_Rhythm = Respiratory_Rhythms.Values.Apnea;
        private static PulmonaryArtery_Rhythms.Values PulmonaryArtery_Placement = PulmonaryArtery_Rhythms.Values.Pulmonary_Capillary_Wedge;

        /* Respiratory profile (partial) */
        private static bool Mechanically_Ventilated = true;
        private static bool Respiration_Inflated = true;

        /* Cardiac profile */
        private static int Pacemaker_Threshold = 75;
        private static bool Pulsus_Paradoxus = true;
        private static bool Pulsus_Alternans = true;
        private static Cardiac_Axes.Values Cardiac_Axis = Cardiac_Axes.Values.Indeterminate;
        private static double [] ST_Elevation = new double [] { 0.1d, 0.1d, 0.1d, 0.1d, 0.1d, 0.1d, 0.1d, 0.1d, 0.1d, 0.1d, 0.1d, 0.1d };
        private static double [] T_Elevation = new double [] { 0.1d, 0.1d, 0.1d, 0.1d, 0.1d, 0.1d, 0.1d, 0.1d, 0.1d, 0.1d, 0.1d, 0.1d };

        /* Obstetric profile */
        private static Scales.Intensity.Values UC_Intensity = Scales.Intensity.Values.Mild;
        private static Scales.Intensity.Values FHR_Variability = Scales.Intensity.Values.Moderate;
        private static int UC_Frequency = 64;
        private static int UC_Duration = 32;
        private static int FHR = 142;

        private static List<FetalHeartDecelerations.Values> FHR_Decelerations = new List<FetalHeartDecelerations.Values> () {
            FetalHeartDecelerations.Values.Acceleration,
            FetalHeartDecelerations.Values.DecelerationVariable
        };

        /* General Device Settings */
        private static bool TransducerZeroed_CVP = true;
        private static bool TransducerZeroed_ABP = true;
        private static bool TransducerZeroed_PA = true;
        private static bool TransducerZeroed_ICP = true;
        private static bool TransducerZeroed_IAP = true;

        /* Defibrillator parameters */
        private static int Pacemaker_Rate = 66;
        private static int Pacemaker_Energy = 37;

        /* Intra-aortic balloon pump parameters */
        private static int IABP_AP = 100;
        private static int IABP_DBP = 50;
        private static int IABP_MAP = 80;
        private static bool IABP_Active = true;
        private static string IABP_Trigger = "Semi-Auto";

        [TestMethod]
        public void VitalSigns_Set () {
            Patient p = new Patient ();

            p.VS_Settings.Set (Default_VS);
            p.VS_Actual.Set (Default_VS);

            Assert.AreEqual (Default_VS.HR, p.VS_Settings.HR);
            Assert.AreEqual (Default_VS.NSBP, p.VS_Settings.NSBP);
            Assert.AreEqual (Default_VS.NDBP, p.VS_Settings.NDBP);
            Assert.AreEqual (Default_VS.NMAP, p.VS_Settings.NMAP);
            Assert.AreEqual (Default_VS.RR, p.VS_Settings.RR);
            Assert.AreEqual (Default_VS.T, p.VS_Settings.T);
            Assert.AreEqual (Default_VS.ETCO2, p.VS_Settings.ETCO2);
            Assert.AreEqual (Default_VS.CVP, p.VS_Settings.CVP);
            Assert.AreEqual (Default_VS.ASBP, p.VS_Settings.ASBP);
            Assert.AreEqual (Default_VS.ADBP, p.VS_Settings.ADBP);
            Assert.AreEqual (Default_VS.AMAP, p.VS_Settings.AMAP);
            Assert.AreEqual (Default_VS.PSP, p.VS_Settings.PSP);
            Assert.AreEqual (Default_VS.PDP, p.VS_Settings.PDP);
            Assert.AreEqual (Default_VS.PMP, p.VS_Settings.PMP);
            Assert.AreEqual (Default_VS.PDP, p.VS_Settings.PDP);
            Assert.AreEqual (Default_VS.ICP, p.VS_Settings.ICP);
            Assert.AreEqual (Default_VS.IAP, p.VS_Settings.IAP);
            Assert.AreEqual (Default_VS.RR_IE_I, p.VS_Settings.RR_IE_I);
            Assert.AreEqual (Default_VS.RR_IE_E, p.VS_Settings.RR_IE_E);

            Assert.AreEqual (Default_VS.HR, p.VS_Actual.HR);
            Assert.AreEqual (Default_VS.NSBP, p.VS_Actual.NSBP);
            Assert.AreEqual (Default_VS.NDBP, p.VS_Actual.NDBP);
            Assert.AreEqual (Default_VS.NMAP, p.VS_Actual.NMAP);
            Assert.AreEqual (Default_VS.RR, p.VS_Actual.RR);
            Assert.AreEqual (Default_VS.T, p.VS_Actual.T);
            Assert.AreEqual (Default_VS.ETCO2, p.VS_Actual.ETCO2);
            Assert.AreEqual (Default_VS.CVP, p.VS_Actual.CVP);
            Assert.AreEqual (Default_VS.ASBP, p.VS_Actual.ASBP);
            Assert.AreEqual (Default_VS.ADBP, p.VS_Actual.ADBP);
            Assert.AreEqual (Default_VS.AMAP, p.VS_Actual.AMAP);
            Assert.AreEqual (Default_VS.PSP, p.VS_Actual.PSP);
            Assert.AreEqual (Default_VS.PDP, p.VS_Actual.PDP);
            Assert.AreEqual (Default_VS.PMP, p.VS_Actual.PMP);
            Assert.AreEqual (Default_VS.PDP, p.VS_Actual.PDP);
            Assert.AreEqual (Default_VS.ICP, p.VS_Actual.ICP);
            Assert.AreEqual (Default_VS.IAP, p.VS_Actual.IAP);
            Assert.AreEqual (Default_VS.RR_IE_I, p.VS_Actual.RR_IE_I);
            Assert.AreEqual (Default_VS.RR_IE_E, p.VS_Actual.RR_IE_E);
        }

        [TestMethod]
        public void UpdateParameters () {
            Patient p = new Patient ();

            p.UpdateParameters (Default_VS.HR,
                Default_VS.NSBP, Default_VS.NDBP, Default_VS.NMAP,
                Default_VS.RR,
                Default_VS.SPO2,
                Default_VS.T,
                Cardiac_Rhythm, Respiratory_Rhythm,
                Default_VS.ETCO2,
                Default_VS.CVP,
                Default_VS.ASBP, Default_VS.ADBP, Default_VS.AMAP,
                PulmonaryArtery_Placement,
                Default_VS.PSP, Default_VS.PDP, Default_VS.PMP,
                Default_VS.ICP,
                Default_VS.IAP,
                Mechanically_Ventilated,
                Default_VS.RR_IE_I, Default_VS.RR_IE_E,
                Pacemaker_Threshold,
                Pulsus_Paradoxus, Pulsus_Alternans,
                Cardiac_Axis,
                ST_Elevation, T_Elevation,
                FHR,
                FHR_Variability, FHR_Decelerations,
                UC_Frequency, UC_Duration, UC_Intensity);

            Assert.AreEqual (Default_VS.HR, p.VS_Settings.HR);
            Assert.AreEqual (Default_VS.NSBP, p.VS_Settings.NSBP);
            Assert.AreEqual (Default_VS.NDBP, p.VS_Settings.NDBP);
            Assert.AreEqual (Default_VS.NMAP, p.VS_Settings.NMAP);
            Assert.AreEqual (Default_VS.RR, p.VS_Settings.RR);
            Assert.AreEqual (Default_VS.SPO2, p.VS_Settings.SPO2);
            Assert.AreEqual (Default_VS.T, p.VS_Settings.T);
            Assert.AreEqual (Cardiac_Rhythm, p.Cardiac_Rhythm.Value);
            Assert.AreEqual (Respiratory_Rhythm, p.Respiratory_Rhythm.Value);
            Assert.AreEqual (Default_VS.ETCO2, p.VS_Settings.ETCO2);
            Assert.AreEqual (Default_VS.CVP, p.VS_Settings.CVP);
            Assert.AreEqual (Default_VS.ASBP, p.VS_Settings.ASBP);
            Assert.AreEqual (Default_VS.ADBP, p.VS_Settings.ADBP);
            Assert.AreEqual (Default_VS.AMAP, p.VS_Settings.AMAP);
            Assert.AreEqual (PulmonaryArtery_Placement, p.PulmonaryArtery_Placement.Value);
            Assert.AreEqual (Default_VS.PSP, p.VS_Settings.PSP);
            Assert.AreEqual (Default_VS.PDP, p.VS_Settings.PDP);
            Assert.AreEqual (Default_VS.PMP, p.VS_Settings.PMP);
            Assert.AreEqual (Default_VS.ICP, p.VS_Settings.ICP);
            Assert.AreEqual (Default_VS.IAP, p.VS_Settings.IAP);
            Assert.AreEqual (Mechanically_Ventilated, p.Mechanically_Ventilated);
            Assert.AreEqual (Default_VS.RR_IE_I, p.VS_Settings.RR_IE_I);
            Assert.AreEqual (Default_VS.RR_IE_E, p.VS_Settings.RR_IE_E);
            Assert.AreEqual (Pacemaker_Threshold, p.Pacemaker_Threshold);
            Assert.AreEqual (Pulsus_Paradoxus, p.Pulsus_Paradoxus);
            Assert.AreEqual (Pulsus_Alternans, p.Pulsus_Alternans);
            Assert.AreEqual (Cardiac_Axis, p.Cardiac_Axis.Value);
            Assert.AreEqual (ST_Elevation, p.ST_Elevation);
            Assert.AreEqual (T_Elevation, p.T_Elevation);
            Assert.AreEqual (FHR, p.FHR);
            Assert.AreEqual (FHR_Variability, p.FHR_Variability.Value);
            Assert.AreEqual (FHR_Decelerations, p.FHR_Decelerations.ValueList);
            Assert.AreEqual (UC_Frequency, p.UC_Frequency);
            Assert.AreEqual (UC_Duration, p.UC_Duration);
            Assert.AreEqual (UC_Intensity, p.UC_Intensity.Value);
        }
    }
}