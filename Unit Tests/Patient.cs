using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;

using II;

namespace Tests {

    [TestClass]
    public class Patient {
        /* Default Vital Sign parameters */

        private II.Patient.Vital_Signs Default_VS = new II.Patient.Vital_Signs () {
            HR = 100,
            NSBP = 200,
            NDBP = 100,
            NMAP = 150,
            RR = 40,
            SPO2 = 80,
            T = 38.9d,
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
            RR_IE_I = 1.1d,
            RR_IE_E = 2.2d
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

        private static List<FHRAccelDecels.Values> FHR_Decelerations = new List<FHRAccelDecels.Values> () {
            FHRAccelDecels.Values.Acceleration,
            FHRAccelDecels.Values.DecelerationVariable
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
        public void SaveLoad_Consistency () {
            II.Patient p1 = new II.Patient ();

            p1.VS_Settings.Set (Default_VS);
            p1.VS_Actual.Set (Default_VS);

            II.Patient p2 = new II.Patient ();
            p2.Load_Process (p1.Save ());

            // File metadata
            Assert.AreEqual (II.Utility.DateTime_ToString (p1.Updated), II.Utility.DateTime_ToString (p2.Updated));

            // Compare vital sign settings
            Assert.AreEqual (p1.VS_Settings.HR, p2.VS_Settings.HR);
            Assert.AreEqual (p1.VS_Settings.NSBP, p2.VS_Settings.NSBP);
            Assert.AreEqual (p1.VS_Settings.NDBP, p2.VS_Settings.NDBP);
            Assert.AreEqual (p1.VS_Settings.NMAP, p2.VS_Settings.NMAP);
            Assert.AreEqual (p1.VS_Settings.RR, p2.VS_Settings.RR);
            Assert.AreEqual (p1.VS_Settings.SPO2, p2.VS_Settings.SPO2);
            Assert.AreEqual (p1.VS_Settings.T, p2.VS_Settings.T);
            Assert.AreEqual (p1.VS_Settings.ETCO2, p2.VS_Settings.ETCO2);
            Assert.AreEqual (p1.VS_Settings.CVP, p2.VS_Settings.CVP);
            Assert.AreEqual (p1.VS_Settings.ASBP, p2.VS_Settings.ASBP);
            Assert.AreEqual (p1.VS_Settings.ADBP, p2.VS_Settings.ADBP);
            Assert.AreEqual (p1.VS_Settings.AMAP, p2.VS_Settings.AMAP);
            Assert.AreEqual (p1.VS_Settings.PSP, p2.VS_Settings.PSP);
            Assert.AreEqual (p1.VS_Settings.PDP, p2.VS_Settings.PDP);
            Assert.AreEqual (p1.VS_Settings.PMP, p2.VS_Settings.PMP);
            Assert.AreEqual (p1.VS_Settings.CO, p2.VS_Settings.CO);
            Assert.AreEqual (p1.VS_Settings.ICP, p2.VS_Settings.ICP);
            Assert.AreEqual (p1.VS_Settings.IAP, p2.VS_Settings.IAP);
            Assert.AreEqual (p1.VS_Settings.RR_IE_I, p2.VS_Settings.RR_IE_I);
            Assert.AreEqual (p1.VS_Settings.RR_IE_E, p2.VS_Settings.RR_IE_E);

            // Compare actual vital signs
            Assert.AreEqual (p1.VS_Actual.HR, p2.VS_Actual.HR);
            Assert.AreEqual (p1.VS_Actual.NSBP, p2.VS_Actual.NSBP);
            Assert.AreEqual (p1.VS_Actual.NDBP, p2.VS_Actual.NDBP);
            Assert.AreEqual (p1.VS_Actual.NMAP, p2.VS_Actual.NMAP);
            Assert.AreEqual (p1.VS_Actual.RR, p2.VS_Actual.RR);
            Assert.AreEqual (p1.VS_Actual.SPO2, p2.VS_Actual.SPO2);
            Assert.AreEqual (p1.VS_Actual.T, p2.VS_Actual.T);
            Assert.AreEqual (p1.VS_Actual.ETCO2, p2.VS_Actual.ETCO2);
            Assert.AreEqual (p1.VS_Actual.CVP, p2.VS_Actual.CVP);
            Assert.AreEqual (p1.VS_Actual.ASBP, p2.VS_Actual.ASBP);
            Assert.AreEqual (p1.VS_Actual.ADBP, p2.VS_Actual.ADBP);
            Assert.AreEqual (p1.VS_Actual.AMAP, p2.VS_Actual.AMAP);
            Assert.AreEqual (p1.VS_Actual.PSP, p2.VS_Actual.PSP);
            Assert.AreEqual (p1.VS_Actual.PDP, p2.VS_Actual.PDP);
            Assert.AreEqual (p1.VS_Actual.PMP, p2.VS_Actual.PMP);
            Assert.AreEqual (p1.VS_Actual.CO, p2.VS_Actual.CO);
            Assert.AreEqual (p1.VS_Actual.ICP, p2.VS_Actual.ICP);
            Assert.AreEqual (p1.VS_Actual.IAP, p2.VS_Actual.IAP);
            Assert.AreEqual (p1.VS_Actual.RR_IE_I, p2.VS_Actual.RR_IE_I);
            Assert.AreEqual (p1.VS_Actual.RR_IE_E, p2.VS_Actual.RR_IE_E);

            // Compare additional fields
            Assert.AreEqual (p1.Cardiac_Rhythm.Value, p2.Cardiac_Rhythm.Value);
            Assert.AreEqual (p1.Respiratory_Rhythm.Value, p2.Respiratory_Rhythm.Value);
            Assert.AreEqual (p1.PulmonaryArtery_Placement.Value, p2.PulmonaryArtery_Placement.Value);

            // Advanced respiratory profile
            Assert.AreEqual (p1.Mechanically_Ventilated, p2.Mechanically_Ventilated);
            Assert.AreEqual (p1.Respiration_Inflated, p2.Respiration_Inflated);

            // Advanced cardiac profile
            Assert.AreEqual (p1.Pacemaker_Threshold, p2.Pacemaker_Threshold);
            Assert.AreEqual (p1.Pulsus_Paradoxus, p2.Pulsus_Paradoxus);
            Assert.AreEqual (p1.Pulsus_Alternans, p2.Pulsus_Alternans);
            Assert.AreEqual (p1.Cardiac_Axis.Value, p2.Cardiac_Axis.Value);

            Assert.AreEqual (p1.ST_Elevation.Length, p2.ST_Elevation.Length);
            for (int i = 0; i < p1.ST_Elevation.Length; i++)
                Assert.AreEqual (p1.ST_Elevation [i], p2.ST_Elevation [i]);

            Assert.AreEqual (p1.T_Elevation.Length, p2.T_Elevation.Length);
            for (int i = 0; i < p1.T_Elevation.Length; i++)
                Assert.AreEqual (p1.T_Elevation [i], p2.T_Elevation [i]);

            // Obstetric profile
            Assert.AreEqual (p1.Contraction_Intensity.Value, p2.Contraction_Intensity.Value);
            Assert.AreEqual (p1.FHR_Variability.Value, p2.FHR_Variability.Value);
            Assert.AreEqual (p1.Contraction_Frequency, p2.Contraction_Frequency);
            Assert.AreEqual (p1.Contraction_Duration, p2.Contraction_Duration);
            Assert.AreEqual (p1.FHR, p2.FHR);

            Assert.AreEqual (p1.FHR_AccelDecels.ValueList.Count, p2.FHR_AccelDecels.ValueList.Count);
            for (int i = 0; i < p1.FHR_AccelDecels.ValueList.Count; i++)
                Assert.AreEqual (p1.FHR_AccelDecels.ValueList [i], p2.FHR_AccelDecels.ValueList [i]);

            Assert.AreEqual (p1.Uterus_Contracted, p2.Uterus_Contracted);

            // General device settings
            Assert.AreEqual (p1.TransducerZeroed_CVP, p2.TransducerZeroed_CVP);
            Assert.AreEqual (p1.TransducerZeroed_ABP, p2.TransducerZeroed_ABP);
            Assert.AreEqual (p1.TransducerZeroed_PA, p2.TransducerZeroed_PA);
            Assert.AreEqual (p1.TransducerZeroed_ICP, p2.TransducerZeroed_ICP);
            Assert.AreEqual (p1.TransducerZeroed_IAP, p2.TransducerZeroed_IAP);

            // Defibrillator parameters
            Assert.AreEqual (p1.Pacemaker_Rate, p2.Pacemaker_Rate);
            Assert.AreEqual (p1.Pacemaker_Energy, p2.Pacemaker_Energy);

            // Intra-aortic balloon pump parameters
            Assert.AreEqual (p1.IABP_AP, p2.IABP_AP);
            Assert.AreEqual (p1.IABP_DBP, p2.IABP_DBP);
            Assert.AreEqual (p1.IABP_MAP, p2.IABP_MAP);
            Assert.AreEqual (p1.IABP_Active, p2.IABP_Active);
            Assert.AreEqual (p1.IABP_Trigger, p2.IABP_Trigger);
        }

        [TestMethod]
        public void VitalSigns_Set () {
            II.Patient p = new II.Patient ();

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
            II.Patient p = new II.Patient ();

            p.UpdateParameters (Default_VS.HR,
                Default_VS.NSBP, Default_VS.NDBP, Default_VS.NMAP,
                Default_VS.RR,
                Default_VS.SPO2,
                Default_VS.T,
                Cardiac_Rhythm, Respiratory_Rhythm,
                Default_VS.ETCO2,
                Default_VS.CVP,
                Default_VS.ASBP, Default_VS.ADBP, Default_VS.AMAP,
                Default_VS.CO,
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
            Assert.AreEqual (FHR_Decelerations, p.FHR_AccelDecels.ValueList);
            Assert.AreEqual (UC_Frequency, p.Contraction_Frequency);
            Assert.AreEqual (UC_Duration, p.Contraction_Duration);
            Assert.AreEqual (UC_Intensity, p.Contraction_Intensity.Value);
        }
    }
}