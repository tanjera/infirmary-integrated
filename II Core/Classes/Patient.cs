﻿/* Patient.cs
 * Infirmary Integrated
 * By Ibi Keller (Tanjera), (c) 2017
 *
 * All patient modeling takes place in Patient.cs, consisting of:
 * - Variables: vital signs and modeling parameters
 * - Timers: for modeling cardiac and respiratory rhythms, etc.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace II {
    public class Patient {
        /* Mirroring variables */
        public DateTime Updated;                    // DateTime this Patient was last updated

        /* Parameters for patient simulation, e.g. vital signs */

        // Vital Signs
        public class Vital_Signs {
            public int HR, RR, ETCO2, SPO2,             // Heart rate, respiratory rate, end-tidal capnography, pulse oximetry
                        CVP,                            // Central venous pressure,
                        NSBP, NDBP, NMAP,               // Non-invasive blood pressures
                        ASBP, ADBP, AMAP,               // Arterial line blood pressures
                        PSP, PDP, PMP,                  // Pulmonary artery pressures
                        ICP, IAP;                       // Intracranial pressure, intra-abdominal pressure
            public double T;                            // Temperature
            public float RR_IE_I,
                         RR_IE_E;

            public void Set (Vital_Signs v) {
                HR = v.HR;
                RR = v.RR;
                ETCO2 = v.ETCO2;
                SPO2 = v.SPO2;
                CVP = v.CVP;
                NSBP = v.NSBP;
                NDBP = v.NDBP;
                NMAP = v.NMAP;
                ASBP = v.ASBP;
                ADBP = v.ADBP;
                AMAP = v.AMAP;
                PSP = v.PSP;
                PDP = v.PDP;
                PMP = v.PMP;
                ICP = v.ICP;
                IAP = v.IAP;
                T = v.T;
                RR_IE_I = v.RR_IE_I;
                RR_IE_E = v.RR_IE_E;
            }
        }

        public Vital_Signs VS_Settings = new Vital_Signs (),
                            VS_Actual = new Vital_Signs ();

        public int HR { get { return VS_Actual.HR; } }
        public int RR { get { return VS_Actual.RR; } }
        public int ETCO2 { get { return VS_Actual.ETCO2; } }
        public int SPO2 { get { return VS_Actual.SPO2; } }
        public int CVP { get { return VS_Actual.CVP; } }
        public int NSBP { get { return VS_Actual.NSBP; } }
        public int NDBP { get { return VS_Actual.NDBP; } }
        public int NMAP { get { return VS_Actual.NMAP; } }
        public int ASBP { get { return VS_Actual.ASBP; } }
        public int ADBP { get { return VS_Actual.ADBP; } }
        public int AMAP { get { return VS_Actual.AMAP; } }
        public int PSP { get { return VS_Actual.PSP; } }
        public int PDP { get { return VS_Actual.PDP; } }
        public int PMP { get { return VS_Actual.PMP; } }
        public int ICP { get { return VS_Actual.ICP; } }
        public int IAP { get { return VS_Actual.IAP; } }
        public double T { get { return VS_Actual.T; } }
        public float RR_IE_I { get { return VS_Actual.RR_IE_I; } }
        public float RR_IE_E { get { return VS_Actual.RR_IE_E; } }

        /* Cardiac Profile */
        public int IABP_AP, IABP_DBP, IABP_MAP;    // Intra-aortic balloon pump blood pressures
        public double [] ST_Elevation, T_Elevation;
        public Cardiac_Rhythms Cardiac_Rhythm = new Cardiac_Rhythms ();
        public PulmonaryArtery_Rhythms PulmonaryArtery_Placement = new PulmonaryArtery_Rhythms ();
        public CardiacAxes Cardiac_Axis = new CardiacAxes ();
        public bool Pulsus_Paradoxus = false,
                    Pulsus_Alternans = false;

        /* Respiratory Profile */
        public Respiratory_Rhythms Respiratory_Rhythm = new Respiratory_Rhythms ();
        public bool Respiration_Inflated = false,
                    Mechanically_Ventilated = false;

        /* Obstetric Profile */
        public Intensity UC_Intensity = new Intensity (),
                         FHR_Variability = new Intensity ();
        public int UC_Frequency, UC_Duration, FHR;
        public FetalHeartDecelerations FHR_Decelerations = new FetalHeartDecelerations ();

        /* General Device Settings */
        public bool TransducerZeroed_CVP = false,
                    TransducerZeroed_ABP = false,
                    TransducerZeroed_PA = false,
                    TransducerZeroed_ICP = false,
                    TransducerZeroed_IAP = false;

        public bool IABP_Active = false;            // Is the Device_IABP currently augmenting?
        public string IABP_Trigger;                 // Device_IABP's trigger; data backflow for strip processing

        public int Pacemaker_Rate,                  // DeviceDefib's transcutaneous pacemaker rate
                    Pacemaker_Energy,               // DeviceDefib's pacemaker energy delivery amount
                    Pacemaker_Threshold;            // Patient's threshold for electrical capture to pacemaker spike

        /* Scales, ratings, etc. for patient parameters */
        public class Intensity {
            public Values Value;
            public enum Values { Absent, Mild, Moderate, Severe }

            public Intensity (Values v) { Value = v; }
            public Intensity () { Value = Values.Absent; }

            public string LookupString () => LookupString (Value);
            public static string LookupString (Values v) {
                return String.Format ("INTENSITY:{0}", Enum.GetValues (typeof (Values)).GetValue ((int)v).ToString ());
            }
        }

        /* Timers for modeling */
        private Timer timerCardiac_Baseline = new Timer (),
                        timerCardiac_Atrial = new Timer (),
                        timerCardiac_Ventricular = new Timer (),
                        timerDefibrillation = new Timer (),
                        timerPacemaker_Baseline = new Timer (),
                        timerPacemaker_Spike = new Timer (),
                        timerRespiratory_Baseline = new Timer (),
                        timerRespiratory_Inspiration = new Timer (),
                        timerRespiratory_Expiration = new Timer (),
                        timerObstetric_Baseline = new Timer (),
                        timerObstetric_ContractionFrequency = new Timer (),
                        timerObstetric_ContractionDuration = new Timer (),
                        timerObstetric_FHRVariationFrequency = new Timer ();

        /* Internal counters and buffers for propogating aberrancies */
        private int counterCardiac_Aberrancy = 0,
                    counterCardiac_Arrhythmia = 0,
                    counterRespiratory_Arrhythmia = 0;
        private bool switchParadoxus = false,
                     switchCardiac_Arrhythmia = false,
                     switchRespiratory_Arrhythmia = false;

        public Patient () {
            UpdateParameters (
                // Basic vital signs
                80,
                120, 80, 95,
                18,
                98,
                38.0f,
                Cardiac_Rhythms.Values.Sinus_Rhythm,
                Respiratory_Rhythms.Values.Regular,

                // Advanced hemodynamics
                40,
                6,
                120, 80, 95,
                PulmonaryArtery_Rhythms.Values.Pulmonary_Artery,
                22, 12, 16,
                8,
                1,

                // Respiratory profile
                false,
                1f, 2f,

                // Cardiac profile
                50,
                false, false,
                CardiacAxes.Values.Normal,
                new double [] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f },
                new double [] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f },

                // Obstetric profile
                150,
                Intensity.Values.Absent,
                new List<FetalHeartDecelerations.Values> (),
                60,
                30,
                Intensity.Values.Moderate);

            InitTimers ();
            SetTimers ();
        }

        /* PatientEvent event, handler, and caller */
        public List<PatientEventArgs> ListPatientEvents = new List<PatientEventArgs> ();
        public event EventHandler<PatientEventArgs> PatientEvent;
        public class PatientEventArgs : EventArgs {
            public Patient Patient;
            public PatientEventTypes EventType;
            public DateTime Occurred;

            public PatientEventArgs (Patient p, PatientEventTypes e) {
                EventType = e;
                Patient = p;
                Occurred = DateTime.Now;
            }
        }

        public enum PatientEventTypes {
            Vitals_Change,
            Cardiac_Baseline,
            Cardiac_Atrial,
            Cardiac_Ventricular,
            Cardiac_Defibrillation,
            Cardiac_PacerSpike,
            Respiratory_Baseline,
            Respiratory_Inspiration,
            Respiratory_Expiration,
            IABP_Inflate,
            IABP_Deflate,
            Obstetric_Baseline,
            Obstetric_ContractionStart,
            Obstetric_ContractionEnd,
            Obstetric_FetalHeartVariation
        }

        public void OnPatientEvent (PatientEventTypes e) {
            PatientEventArgs ea = new PatientEventArgs (this, e);
            ListPatientEvents.Add (ea);
            PatientEvent?.Invoke (this, ea);
        }

        public void CleanListPatientEvents () {
            // Remove all listings older than 1 minute... prevent cluttering memory
            for (int i = ListPatientEvents.Count - 1; i >= 0; i--)
                if (ListPatientEvents [i].Occurred.CompareTo (DateTime.Now.AddMinutes (-1)) < 0)
                    ListPatientEvents.RemoveAt (i);
        }

        /* Methods for counting, calculating, and measuring vital signs, timing re: vital signs, etc. */
        public static int CalculateMAP (int sbp, int dbp) { return dbp + ((sbp - dbp) / 3); }
        public static int CalculateCPP (int icp, int map) { return map - icp; }
        public double GetHR_Seconds { get { return 60d / Math.Max (1, VS_Actual.HR); } }
        public double GetRR_Seconds { get { return 60d / Math.Max (1, VS_Actual.RR); } }
        public double GetRR_Seconds_I { get { return (GetRR_Seconds / (RR_IE_I + RR_IE_E)) * RR_IE_I; } }
        public double GetRR_Seconds_E { get { return (GetRR_Seconds / (RR_IE_I + RR_IE_E)) * RR_IE_E; } }

        public int MeasureHR_ECG (double lengthSeconds, double offsetSeconds)
            => MeasureHR (lengthSeconds, offsetSeconds, false);
        public int MeasureHR_SPO2 (double lengthSeconds, double offsetSeconds)
            => MeasureHR (lengthSeconds, offsetSeconds, true);

        public int MeasureHR (double lengthSeconds, double offsetSeconds, bool isPulse = false) {
            CleanListPatientEvents ();

            if (isPulse && !Cardiac_Rhythm.HasPulse_Ventricular)
                return 0;

            int counter = 0;
            foreach (PatientEventArgs ea in ListPatientEvents)
                if (ea.EventType == PatientEventTypes.Cardiac_Ventricular
                    && ea.Occurred.CompareTo (DateTime.Now.AddSeconds (-(lengthSeconds + offsetSeconds))) >= 0
                    && ea.Occurred.CompareTo (DateTime.Now.AddSeconds (-offsetSeconds)) <= 0)
                    counter++;

            return (int)(counter / (lengthSeconds / 60));
        }

        public int MeasureRR (double lengthSeconds, double offsetSeconds) {
            CleanListPatientEvents ();

            int counter = 0;
            foreach (PatientEventArgs ea in ListPatientEvents)
                if (ea.EventType == PatientEventTypes.Respiratory_Inspiration
                    && ea.Occurred.CompareTo (DateTime.Now.AddSeconds (-(lengthSeconds + offsetSeconds))) >= 0
                    && ea.Occurred.CompareTo (DateTime.Now.AddSeconds (-offsetSeconds)) <= 0)
                    counter++;

            return (int)(counter / (lengthSeconds / 60));
        }

        /* Process all timers for patient modeling */
        public void Timers_Process (object sender, EventArgs e) {
            /* For cross-platform compatibility with different timers ...
             * When creating a Patient object, create a native thread-safe Timer object,
             * short interval, and call this function on its Tick to process all Patient
             * timers.
             */
            timerCardiac_Baseline.Process ();
            timerCardiac_Atrial.Process ();
            timerCardiac_Ventricular.Process ();
            timerDefibrillation.Process ();
            timerPacemaker_Baseline.Process ();
            timerPacemaker_Spike.Process ();
            timerRespiratory_Baseline.Process ();
            timerRespiratory_Inspiration.Process ();
            timerRespiratory_Expiration.Process ();
            timerObstetric_Baseline.Process ();
            timerObstetric_ContractionDuration.Process ();
            timerObstetric_ContractionFrequency.Process ();
            timerObstetric_FHRVariationFrequency.Process ();
        }

        /* Process for loading Patient{} information from simulation file */
        public void Load_Process (string inc) {
            StringReader sRead = new StringReader (inc);

            try {
                string line;
                while ((line = sRead.ReadLine ()) != null) {
                    if (line.Contains (":")) {
                        string pName = line.Substring (0, line.IndexOf (':')),
                                pValue = line.Substring (line.IndexOf (':') + 1);
                        switch (pName) {
                            default: break;
                            // Patient/scenario information
                            case "Updated": Updated = Utility.DateTime_FromString (pValue); break;

                            // Device information
                            case "TransducerZeroed_ABP": TransducerZeroed_ABP = bool.Parse (pValue); break;
                            case "TransducerZeroed_CVP": TransducerZeroed_CVP = bool.Parse (pValue); break;
                            case "TransducerZeroed_PA": TransducerZeroed_PA = bool.Parse (pValue); break;
                            case "TransducerZeroed_ICP": TransducerZeroed_ICP = bool.Parse (pValue); break;
                            case "TransducerZeroed_IAP": TransducerZeroed_IAP = bool.Parse (pValue); break;

                            // Basic vital signs
                            case "HR": VS_Settings.HR = int.Parse (pValue); break;
                            case "NSBP": VS_Settings.NSBP = int.Parse (pValue); break;
                            case "NDBP": VS_Settings.NDBP = int.Parse (pValue); break;
                            case "NMAP": VS_Settings.NMAP = int.Parse (pValue); break;
                            case "RR": VS_Settings.RR = int.Parse (pValue); break;
                            case "SPO2": VS_Settings.SPO2 = int.Parse (pValue); break;
                            case "T": VS_Settings.T = double.Parse (pValue); break;
                            case "Cardiac_Rhythm": Cardiac_Rhythm.Value = (Cardiac_Rhythms.Values)Enum.Parse (typeof (Cardiac_Rhythms.Values), pValue); break;
                            case "Respiratory_Rhythm": Respiratory_Rhythm.Value = (Respiratory_Rhythms.Values)Enum.Parse (typeof (Respiratory_Rhythms.Values), pValue); break;

                            // Advanced hemodynamics
                            case "ETCO2": VS_Settings.ETCO2 = int.Parse (pValue); break;
                            case "CVP": VS_Settings.CVP = int.Parse (pValue); break;
                            case "ASBP": VS_Settings.ASBP = int.Parse (pValue); break;
                            case "ADBP": VS_Settings.ADBP = int.Parse (pValue); break;
                            case "AMAP": VS_Settings.AMAP = int.Parse (pValue); break;
                            case "PulmonaryArtery_Rhythm": PulmonaryArtery_Placement.Value = (PulmonaryArtery_Rhythms.Values)Enum.Parse (typeof (PulmonaryArtery_Rhythms.Values), pValue); break;
                            case "PSP": VS_Settings.PSP = int.Parse (pValue); break;
                            case "PDP": VS_Settings.PDP = int.Parse (pValue); break;
                            case "PMP": VS_Settings.PMP = int.Parse (pValue); break;
                            case "ICP": VS_Settings.ICP = int.Parse (pValue); break;
                            case "IAP": VS_Settings.IAP = int.Parse (pValue); break;

                            // Respiratory profile
                            case "Mechanically_Ventilated": Mechanically_Ventilated = bool.Parse (pValue); break;
                            case "Respiratory_IERatio_I": VS_Settings.RR_IE_I = int.Parse (pValue); break;
                            case "Respiratory_IERatio_E": VS_Settings.RR_IE_E = int.Parse (pValue); break;
                            case "Respiration_Inflated": Respiration_Inflated = bool.Parse (pValue); break;

                            // Cardiac profile
                            case "Pacemaker_Threshold": Pacemaker_Threshold = int.Parse (pValue); break;
                            case "Pacemaker_Rate": Pacemaker_Rate = int.Parse (pValue); break;
                            case "Pacemaker_Energy": Pacemaker_Energy = int.Parse (pValue); break;
                            case "PulsusParadoxus": Pulsus_Paradoxus = bool.Parse (pValue); break;
                            case "PulsusAlternans": Pulsus_Alternans = bool.Parse (pValue); break;
                            case "Cardiac_Axis": Cardiac_Axis.Value = (CardiacAxes.Values)Enum.Parse (typeof (CardiacAxes.Values), pValue); break;

                            case "ST_Elevation":
                                string [] e_st = pValue.Split (',').Where ((o) => o != "").ToArray ();
                                for (int i = 0; i < e_st.Length && i < ST_Elevation.Length; i++)
                                    ST_Elevation [i] = double.Parse (e_st [i]);
                                break;

                            case "T_Elevation":
                                string [] e_t = pValue.Split (',').Where ((o) => o != "").ToArray ();
                                for (int i = 0; i < e_t.Length && i < T_Elevation.Length; i++)
                                    T_Elevation [i] = double.Parse (e_t [i]);
                                break;

                            // Obstetric profile
                            case "FHR": FHR = int.Parse (pValue); break;
                            case "FHR_Variability": FHR_Variability.Value = (Intensity.Values)Enum.Parse (typeof (Intensity.Values), pValue); break;
                            case "FHR_Rhythms":
                                foreach (string fhr_rhythm in pValue.Split (',').Where ((o) => o != ""))
                                    FHR_Decelerations.ValueList.Add ((FetalHeartDecelerations.Values)Enum.Parse (typeof (FetalHeartDecelerations.Values), fhr_rhythm));
                                break;

                            case "UterineContraction_Frequency": UC_Frequency = int.Parse (pValue); break;
                            case "UterineContraction_Duration": UC_Duration = int.Parse (pValue); break;
                            case "UterineContraction_Intensity": UC_Intensity.Value = (Intensity.Values)Enum.Parse (typeof (Intensity.Values), pValue); break;
                        }
                    }
                }
            } catch (Exception e) {
                new Server.Servers ().Post_Exception (e);
                // If the load fails... just bail on the actual value parsing and continue the load process
            }

            sRead.Close ();

            // Reset measurements to set parameters
            VS_Actual.Set (VS_Settings);

            SetTimers ();
            OnCardiac_Baseline ();
            OnRespiratory_Baseline ();

            OnPatientEvent (PatientEventTypes.Vitals_Change);
        }

        /* Process for saving Patient{} information to simulation file  */
        public string Save () {
            StringBuilder sWrite = new StringBuilder ();
            // File/scenario information
            sWrite.AppendLine (String.Format ("{0}:{1}", "Updated", Utility.DateTime_ToString (Updated)));

            // Device information
            sWrite.AppendLine (String.Format ("{0}:{1}", "TransducerZeroed_ABP", TransducerZeroed_ABP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "TransducerZeroed_CVP", TransducerZeroed_CVP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "TransducerZeroed_PA", TransducerZeroed_PA));
            sWrite.AppendLine (String.Format ("{0}:{1}", "TransducerZeroed_ICP", TransducerZeroed_ICP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "TransducerZeroed_IAP", TransducerZeroed_IAP));

            // Basic vital signs
            sWrite.AppendLine (String.Format ("{0}:{1}", "HR", VS_Settings.HR));
            sWrite.AppendLine (String.Format ("{0}:{1}", "NSBP", VS_Settings.NSBP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "NDBP", VS_Settings.NDBP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "NMAP", VS_Settings.NMAP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "RR", VS_Settings.RR));
            sWrite.AppendLine (String.Format ("{0}:{1}", "SPO2", VS_Settings.SPO2));
            sWrite.AppendLine (String.Format ("{0}:{1}", "T", VS_Settings.T));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Cardiac_Rhythm", Cardiac_Rhythm.Value));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Respiratory_Rhythm", Respiratory_Rhythm.Value));

            // Advanced hemodynamics
            sWrite.AppendLine (String.Format ("{0}:{1}", "ETCO2", VS_Settings.ETCO2));
            sWrite.AppendLine (String.Format ("{0}:{1}", "CVP", VS_Settings.CVP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "ASBP", VS_Settings.ASBP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "ADBP", VS_Settings.ADBP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "AMAP", VS_Settings.AMAP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "PulmonaryArtery_Rhythm", PulmonaryArtery_Placement.Value));
            sWrite.AppendLine (String.Format ("{0}:{1}", "PSP", VS_Settings.PSP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "PDP", VS_Settings.PDP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "PMP", VS_Settings.PMP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "ICP", VS_Settings.ICP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "IAP", VS_Settings.IAP));

            // Respiratory profile
            sWrite.AppendLine (String.Format ("{0}:{1}", "Mechanically_Ventilated", Mechanically_Ventilated));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Respiratory_IERatio_I", VS_Settings.RR_IE_I));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Respiratory_IERatio_E", VS_Settings.RR_IE_E));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Respiration_Inflated", Respiration_Inflated));

            // Cardiac profile
            sWrite.AppendLine (String.Format ("{0}:{1}", "Pacemaker_Threshold", Pacemaker_Threshold));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Pacemaker_Rate", Pacemaker_Rate));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Pacemaker_Energy", Pacemaker_Energy));
            sWrite.AppendLine (String.Format ("{0}:{1}", "PulsusParadoxus", Pulsus_Paradoxus));
            sWrite.AppendLine (String.Format ("{0}:{1}", "PulsusAlternans", Pulsus_Alternans));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Cardiac_Axis", Cardiac_Axis.Value));
            sWrite.AppendLine (String.Format ("{0}:{1}", "ST_Elevation", string.Join (",", ST_Elevation)));
            sWrite.AppendLine (String.Format ("{0}:{1}", "T_Elevation", string.Join (",", T_Elevation)));

            // Obstetric profile
            sWrite.AppendLine (String.Format ("{0}:{1}", "FHR", FHR));
            sWrite.AppendLine (String.Format ("{0}:{1}", "FHR_Variability", FHR_Variability.Value));
            sWrite.AppendLine (String.Format ("{0}:{1}", "UterineContraction_Frequency", UC_Frequency));
            sWrite.AppendLine (String.Format ("{0}:{1}", "UterineContraction_Duration", UC_Duration));
            sWrite.AppendLine (String.Format ("{0}:{1}", "UterineContraction_Intensity", UC_Intensity.Value));
            sWrite.AppendLine (String.Format ("{0}:{1}", "FHR_Rhythms", string.Join (",", FHR_Decelerations.ValueList)));

            return sWrite.ToString ();
        }

        public void UpdateParameters (
                    // Basic vital signs
                    int hr,
                    int nsbp, int ndbp, int nmap,
                    int rr,
                    int spo2,
                    double t,
                    Cardiac_Rhythms.Values card_rhythm,
                    Respiratory_Rhythms.Values resp_rhythm,

                    // Advanced hemodynamics
                    int etco2,
                    int cvp,
                    int asbp, int adbp, int amap,
                    PulmonaryArtery_Rhythms.Values pa_placement,
                    int psp, int pdp, int pmp,
                    int icp, int iap,

                    // Respiratory profile
                    bool mech_vent,
                    float resp_ier_i, float resp_ier_e,

                    // Cardiac profile
                    int pacer_threshold,
                    bool puls_paradoxus, bool puls_alternans,
                    CardiacAxes.Values card_axis,
                    double [] st_elev, double [] t_elev,

                    // Obstetric profile
                    int fhr, Intensity.Values fhr_var, List<FetalHeartDecelerations.Values> fhr_rhythms,
                    int uc_freq, int uc_duration, Intensity.Values uc_intensity) {
            Updated = DateTime.UtcNow;

            // Basic vital signs
            VS_Settings.HR = hr;
            VS_Settings.NSBP = nsbp;
            VS_Settings.NDBP = ndbp;
            VS_Settings.NMAP = nmap;
            VS_Settings.RR = rr;
            VS_Settings.SPO2 = spo2;
            VS_Settings.T = t;

            // Change in cardiac or respiratory rhythm? Reset all buffer counters and switches
            if (Cardiac_Rhythm.Value != card_rhythm) {
                counterCardiac_Aberrancy = 0;
                counterCardiac_Arrhythmia = 0;
                switchCardiac_Arrhythmia = false;
                Cardiac_Rhythm.AberrantBeat = false;
                Cardiac_Rhythm.AlternansBeat = false;
            }
            if (Respiratory_Rhythm.Value != resp_rhythm) {
                Respiration_Inflated = false;
                counterRespiratory_Arrhythmia = 0;
                switchRespiratory_Arrhythmia = false;
            }

            Cardiac_Rhythm.Value = card_rhythm;
            Respiratory_Rhythm.Value = resp_rhythm;

            // Advanced hemodynamics
            VS_Settings.ETCO2 = etco2;
            VS_Settings.CVP = cvp;
            VS_Settings.ASBP = asbp;
            VS_Settings.ADBP = adbp;
            VS_Settings.AMAP = amap;

            PulmonaryArtery_Placement.Value = pa_placement;

            VS_Settings.PSP = psp;
            VS_Settings.PDP = pdp;
            VS_Settings.PMP = pmp;
            VS_Settings.ICP = icp;
            VS_Settings.IAP = iap;

            // Respiratory profile
            Mechanically_Ventilated = mech_vent;
            VS_Settings.RR_IE_I = resp_ier_i;
            VS_Settings.RR_IE_E = resp_ier_e;

            // Cardiac profile
            Pacemaker_Threshold = pacer_threshold;

            // Reset buffers and switches for pulsus paradoxus
            switchParadoxus = false;
            Pulsus_Paradoxus = puls_paradoxus;
            Pulsus_Alternans = puls_alternans;

            Cardiac_Axis.Value = card_axis;
            ST_Elevation = st_elev;
            T_Elevation = t_elev;

            // Obstetric profile
            FHR = fhr;
            FHR_Variability.Value = fhr_var;
            FHR_Decelerations.ValueList = fhr_rhythms;
            UC_Frequency = uc_freq;
            UC_Duration = uc_duration;
            UC_Intensity.Value = uc_intensity;

            // Reset actual vital signs to set parameters
            VS_Actual.Set (VS_Settings);

            SetTimers ();
            OnCardiac_Baseline ();
            OnRespiratory_Baseline ();

            OnPatientEvent (PatientEventTypes.Vitals_Change);
        }

        public void ClampVitals (
                    int hrMin, int hrMax,
                    int spo2Min, int spo2Max,
                    int etco2Min, int etco2Max,
                    int sbpMin, int sbpMax, int dbpMin, int dbpMax,
                    int pspMin, int pspMax, int pdpMin, int pdpMax) {
            VS_Settings.HR = Utility.Clamp (VS_Settings.HR, hrMin, hrMax);
            VS_Settings.SPO2 = Utility.Clamp (VS_Settings.SPO2, spo2Min, spo2Max);
            VS_Settings.ETCO2 = Utility.Clamp (VS_Settings.ETCO2, etco2Min, etco2Max);
            VS_Settings.NSBP = Utility.Clamp (VS_Settings.NSBP, sbpMin, sbpMax);
            VS_Settings.NDBP = Utility.Clamp (VS_Settings.NDBP, dbpMin, dbpMax);
            VS_Settings.NMAP = Patient.CalculateMAP (VS_Settings.NSBP, VS_Settings.NDBP);
            VS_Settings.ASBP = Utility.Clamp (VS_Settings.ASBP, sbpMin, sbpMax);
            VS_Settings.ADBP = Utility.Clamp (VS_Settings.ADBP, sbpMin, sbpMax);
            VS_Settings.AMAP = Patient.CalculateMAP (VS_Settings.ASBP, VS_Settings.ADBP);
            VS_Settings.PSP = Utility.Clamp (VS_Settings.PSP, pspMin, pspMax);
            VS_Settings.PDP = Utility.Clamp (VS_Settings.PDP, pdpMin, pdpMax);
            VS_Settings.PMP = Patient.CalculateMAP (VS_Settings.PSP, VS_Settings.PDP);

            VS_Actual.Set (VS_Settings);

            switchParadoxus = false;

            SetTimers ();
            OnCardiac_Baseline ();
            OnRespiratory_Baseline ();
            OnPatientEvent (PatientEventTypes.Vitals_Change);
        }

        private void InitTimers () {
            timerCardiac_Baseline.Tick += delegate { OnCardiac_Baseline (); };
            timerCardiac_Atrial.Tick += delegate { OnCardiac_Atrial (); };
            timerCardiac_Ventricular.Tick += delegate { OnCardiac_Ventricular (); };
            timerDefibrillation.Tick += delegate { OnDefibrillation_End (); };
            timerPacemaker_Baseline.Tick += delegate { OnPacemaker_Baseline (); };
            timerPacemaker_Spike.Tick += delegate { OnPacemaker_Spike (); };

            timerRespiratory_Baseline.Tick += delegate { OnRespiratory_Baseline (); };
            timerRespiratory_Inspiration.Tick += delegate { OnRespiratory_Inspiration (); };
            timerRespiratory_Expiration.Tick += delegate { OnRespiratory_Expiration (); };

            timerObstetric_Baseline.Tick += delegate { OnObstetric_Baseline (); };
            timerObstetric_ContractionFrequency.Tick += delegate { OnObstetric_ContractionStart (); };
            timerObstetric_ContractionDuration.Tick += delegate { OnObstetric_ContractionEnd (); };
            timerObstetric_FHRVariationFrequency.Tick += delegate { OnObstetric_FetalHeartVariationStart (); };
        }

        private void SetTimers () {
            timerCardiac_Baseline.ResetAuto ((int)(GetHR_Seconds * 1000f));
            timerCardiac_Atrial.Stop ();
            timerCardiac_Ventricular.Stop ();

            timerDefibrillation.ResetAuto ();
            if (timerPacemaker_Baseline.IsRunning)
                timerPacemaker_Baseline.ResetAuto ();
            else
                timerPacemaker_Baseline.Reset ();
            timerPacemaker_Spike.Stop ();

            timerRespiratory_Baseline.ResetAuto ((int)(GetRR_Seconds * 1000f));
            timerRespiratory_Inspiration.Stop ();
            timerRespiratory_Expiration.Stop ();

            timerObstetric_Baseline.ResetAuto (1000);
            timerObstetric_ContractionDuration.Stop ();
            timerObstetric_ContractionFrequency.Stop ();
            timerObstetric_FHRVariationFrequency.Stop ();
        }

        public void Defibrillate ()
            => InitDefibrillation (false);

        public void Cardiovert ()
            => InitDefibrillation (true);

        public void Pacemaker (bool active, int rate, int energy) {
            Pacemaker_Rate = rate;
            Pacemaker_Energy = energy;

            // If rate == 0, must stop timer! Otherwise timer.Interval is set to 0!
            if (!active || rate == 0 || energy == 0)
                StopPacemaker ();
            else if (active)
                StartPacemaker ();
        }

        public void PacemakerPause () => timerPacemaker_Baseline.Interval = 4000;

        private void InitDefibrillation (bool toSynchronize) {
            if (toSynchronize)
                timerCardiac_Ventricular.Tick += OnCardioversion;
            else
                OnDefibrillation ();
        }

        public bool Pacemaker_HasCapture {
            get {
                return timerPacemaker_Baseline.IsRunning
                    && Pacemaker_Energy > 0 && Pacemaker_Rate > 0
                    && Pacemaker_Energy >= Pacemaker_Threshold;
            }
        }

        private void StartPacemaker ()
            => timerPacemaker_Baseline.ResetAuto ((int)((60d / Pacemaker_Rate) * 1000));

        private void StopPacemaker () {
            timerPacemaker_Baseline.Stop ();
            timerPacemaker_Spike.Stop ();
        }

        private void OnDefibrillation () {
            timerCardiac_Baseline.Stop ();
            timerCardiac_Atrial.Stop ();
            timerCardiac_Ventricular.Stop ();
            timerDefibrillation.ResetAuto (20);
            // Invoke the defibrillation event *after* starting the timer- IsDefibrillating() checks the timer!
            OnPatientEvent (PatientEventTypes.Cardiac_Defibrillation);
        }

        private void OnDefibrillation_End () {
            timerDefibrillation.Stop ();
            timerCardiac_Baseline.ResetAuto (timerCardiac_Baseline.Interval);
        }

        private void OnCardioversion (object sender, EventArgs e) {
            timerCardiac_Ventricular.Tick -= OnCardioversion;
            OnDefibrillation ();
        }

        private void OnPacemaker_Baseline () {
            if (Pacemaker_Energy > 0)
                OnPatientEvent (PatientEventTypes.Cardiac_PacerSpike);

            if (Pacemaker_Energy >= Pacemaker_Threshold)
                timerPacemaker_Spike.ResetAuto (40);        // Adds an interval between the spike and the QRS complex

            StartPacemaker ();                          // In case pacemaker was paused... updates .Interval
        }

        private void OnPacemaker_Spike () {
            timerPacemaker_Spike.Stop ();
            // Trigger the QRS complex, then reset the heart's intrinsic timers
            Cardiac_Rhythm.AberrantBeat = true;
            OnPatientEvent (PatientEventTypes.Cardiac_Ventricular);
            Cardiac_Rhythm.AberrantBeat = false;
            timerCardiac_Baseline.ResetAuto ();
        }

        private void OnCardiac_Baseline () {
            OnPatientEvent (PatientEventTypes.Cardiac_Baseline);
            timerCardiac_Baseline.Set ((int)(GetHR_Seconds * 1000f));

            switch (Cardiac_Rhythm.Value) {
                default:
                case Cardiac_Rhythms.Values.Asystole:
                    break;

                // Traced as "regular V" Rhythms
                case Cardiac_Rhythms.Values.Atrial_Flutter:
                case Cardiac_Rhythms.Values.Junctional:
                case Cardiac_Rhythms.Values.Idioventricular:
                case Cardiac_Rhythms.Values.Supraventricular_Tachycardia:
                case Cardiac_Rhythms.Values.Ventricular_Tachycardia_Monomorphic_Pulsed:
                case Cardiac_Rhythms.Values.Ventricular_Tachycardia_Monomorphic_Pulseless:
                case Cardiac_Rhythms.Values.Ventricular_Tachycardia_Polymorphic:
                case Cardiac_Rhythms.Values.Ventricular_Fibrillation_Coarse:
                case Cardiac_Rhythms.Values.Ventricular_Fibrillation_Fine:
                    timerCardiac_Ventricular.ResetAuto (1);
                    break;

                // Traced as "regular A" or "regular A -> V" Rhythms
                case Cardiac_Rhythms.Values.AV_Block__1st_Degree:
                case Cardiac_Rhythms.Values.AV_Block__Mobitz_II:
                case Cardiac_Rhythms.Values.AV_Block__Wenckebach:
                case Cardiac_Rhythms.Values.Bundle_Branch_Block:
                case Cardiac_Rhythms.Values.Sinus_Rhythm:
                case Cardiac_Rhythms.Values.Pulseless_Electrical_Activity:
                case Cardiac_Rhythms.Values.Ventricular_Standstill:
                    timerCardiac_Atrial.ResetAuto (1);
                    break;

                // Traced as "irregular V" rhythms
                case Cardiac_Rhythms.Values.Atrial_Fibrillation:
                    VS_Actual.HR = (int)(VS_Settings.HR * Utility.RandomDouble (0.6, 1.4));
                    timerCardiac_Ventricular.ResetAuto (1);
                    break;

                /* Special Cases */
                case Cardiac_Rhythms.Values.AV_Block__3rd_Degree:
                    if (!timerCardiac_Atrial.IsRunning)
                        timerCardiac_Atrial.ResetAuto ((int)(timerCardiac_Baseline.Interval * 0.6));
                    timerCardiac_Ventricular.ResetAuto (160);
                    break;

                case Cardiac_Rhythms.Values.Sick_Sinus_Syndrome:
                    // Countdown to 0; on 0, switch between tachy/brady; brady runs 8-12 beats, tachy runs 20-30 beats
                    if (counterCardiac_Arrhythmia <= 0) {
                        switchCardiac_Arrhythmia = !switchCardiac_Arrhythmia;
                        if (switchCardiac_Arrhythmia) {
                            VS_Actual.HR = (int)(VS_Settings.HR * 0.60);
                            counterCardiac_Arrhythmia = new Random ().Next (8, 12);
                        } else {
                            VS_Actual.HR = (int)(VS_Settings.HR * 1.8);
                            counterCardiac_Arrhythmia = new Random ().Next (20, 30);
                        }
                    } else
                        counterCardiac_Arrhythmia--;

                    timerCardiac_Atrial.ResetAuto (1);
                    break;

                case Cardiac_Rhythms.Values.Sinus_Arrhythmia:
                    if (Respiration_Inflated)
                        VS_Actual.HR = (int)(VS_Settings.HR * 1.075);
                    else
                        VS_Actual.HR = (int)(VS_Settings.HR * 0.925);

                    timerCardiac_Atrial.ResetAuto (1);
                    break;

                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_Arrest:
                    // Every 10-25 beats, sinus arrest
                    if (counterCardiac_Arrhythmia <= 0) {
                        Random r = new Random ();
                        counterCardiac_Arrhythmia = r.Next (10, 16);
                        VS_Actual.HR = VS_Settings.HR / 8;
                    } else {
                        VS_Actual.HR = VS_Settings.HR;
                        counterCardiac_Arrhythmia--;
                    }

                    timerCardiac_Atrial.ResetAuto (1);
                    break;

                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_PACs:
                    counterCardiac_Aberrancy -= 1;
                    VS_Actual.HR = VS_Settings.HR;
                    if (counterCardiac_Aberrancy <= 0) {
                        counterCardiac_Aberrancy = new Random ().Next (4, 8);
                        VS_Actual.HR = (int)(VS_Settings.HR * Utility.RandomDouble (0.6, 0.8));
                    } else {
                        VS_Actual.HR = VS_Settings.HR;
                    }
                    timerCardiac_Atrial.ResetAuto (1);
                    break;

                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_PJCs:
                    counterCardiac_Aberrancy -= 1;
                    VS_Actual.HR = VS_Settings.HR;
                    if (counterCardiac_Aberrancy <= 0) {
                        counterCardiac_Aberrancy = new Random ().Next (4, 8);
                        timerCardiac_Ventricular.ResetAuto (1);
                    } else {
                        if (counterCardiac_Aberrancy == 1)
                            VS_Actual.HR = (int)(VS_Settings.HR * Utility.RandomDouble (0.7, 0.9));
                        timerCardiac_Atrial.ResetAuto (1);
                    }
                    break;

                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_Bigeminy:
                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_Trigeminy:
                    counterCardiac_Aberrancy -= 1;
                    if (counterCardiac_Aberrancy == 0) {
                        timerCardiac_Baseline.Set ((int)(timerCardiac_Baseline.Interval * 0.8));
                    } else if (counterCardiac_Aberrancy < 0) {   // Then throw the PVC and reset the counters
                        if (Cardiac_Rhythm.Value == Cardiac_Rhythms.Values.Sinus_Rhythm_with_Bigeminy)
                            counterCardiac_Aberrancy = 1;
                        else if (Cardiac_Rhythm.Value == Cardiac_Rhythms.Values.Sinus_Rhythm_with_Trigeminy)
                            counterCardiac_Aberrancy = 2;
                        Cardiac_Rhythm.AberrantBeat = true;
                        timerCardiac_Ventricular.ResetAuto (1);
                        break;
                    }
                    timerCardiac_Atrial.ResetAuto (1);
                    Cardiac_Rhythm.AberrantBeat = false;
                    break;

                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_PVCs_Unifocal:
                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_PVCs_Multifocal:
                    counterCardiac_Aberrancy -= 1;
                    VS_Actual.HR = VS_Settings.HR;
                    if (counterCardiac_Aberrancy == 0) {  // Shorten the beat preceding the PVC, making it premature
                        VS_Actual.HR = (int)(VS_Settings.HR * 0.8);
                    } else if (counterCardiac_Aberrancy < 0) {   // Then throw the PVC and reset the counters
                        counterCardiac_Aberrancy = new Random ().Next (4, 9);
                        Cardiac_Rhythm.AberrantBeat = true;
                        timerCardiac_Ventricular.ResetAuto (1);
                        break;
                    }
                    Cardiac_Rhythm.AberrantBeat = false;
                    timerCardiac_Atrial.ResetAuto (1);
                    break;
            }
        }

        private void OnCardiac_Atrial () {
            OnPatientEvent (PatientEventTypes.Cardiac_Atrial);

            switch (Cardiac_Rhythm.Value) {
                default:
                case Cardiac_Rhythms.Values.Asystole:
                    break;

                // Regular A Rhythms
                case Cardiac_Rhythms.Values.Ventricular_Standstill:
                    timerCardiac_Atrial.Stop ();
                    break;

                // Regular A -> V rhythms
                case Cardiac_Rhythms.Values.Bundle_Branch_Block:
                case Cardiac_Rhythms.Values.Sick_Sinus_Syndrome:
                case Cardiac_Rhythms.Values.Sinus_Arrhythmia:
                case Cardiac_Rhythms.Values.Sinus_Rhythm:
                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_Arrest:
                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_PACs:
                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_PJCs:
                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_Bigeminy:
                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_Trigeminy:
                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_PVCs_Unifocal:
                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_PVCs_Multifocal:
                case Cardiac_Rhythms.Values.Pulseless_Electrical_Activity:
                    timerCardiac_Atrial.Stop ();
                    timerCardiac_Ventricular.ResetAuto (160);
                    break;

                /* Special cases */

                case Cardiac_Rhythms.Values.AV_Block__1st_Degree:
                    timerCardiac_Atrial.Stop ();
                    timerCardiac_Ventricular.ResetAuto (240);
                    break;

                case Cardiac_Rhythms.Values.AV_Block__Mobitz_II:
                    timerCardiac_Atrial.Stop ();
                    counterCardiac_Aberrancy -= 1;
                    if (counterCardiac_Aberrancy < 0) {
                        counterCardiac_Aberrancy = 2;
                        Cardiac_Rhythm.AberrantBeat = true;
                    } else {
                        timerCardiac_Ventricular.ResetAuto (160);
                        Cardiac_Rhythm.AberrantBeat = false;
                    }
                    break;

                case Cardiac_Rhythms.Values.AV_Block__Wenckebach:
                    timerCardiac_Atrial.Stop ();
                    counterCardiac_Aberrancy -= 1;
                    if (counterCardiac_Aberrancy < 0) {
                        counterCardiac_Aberrancy = 3;
                        Cardiac_Rhythm.AberrantBeat = true;
                    } else {
                        timerCardiac_Baseline.Set ((int)(timerCardiac_Baseline.Interval + (160 * (3 - counterCardiac_Aberrancy))));
                        timerCardiac_Ventricular.ResetAuto ((int)(160 * (3 - counterCardiac_Aberrancy)));
                        Cardiac_Rhythm.AberrantBeat = false;
                    }
                    break;

                case Cardiac_Rhythms.Values.AV_Block__3rd_Degree:
                    // Specifically let atrial timer continue to run and propogate P-waves!
                    break;
            }
        }

        private void OnCardiac_Ventricular () {
            OnPatientEvent (PatientEventTypes.Cardiac_Ventricular);

            // Flip the switch on pulsus alternans
            Cardiac_Rhythm.AlternansBeat = Pulsus_Alternans ? !Cardiac_Rhythm.AlternansBeat : false;

            switch (Cardiac_Rhythm.Value) {
                default: break;
            }

            timerCardiac_Ventricular.Stop ();
        }

        private void OnRespiratory_Baseline () {
            OnPatientEvent (PatientEventTypes.Respiratory_Baseline);
            timerRespiratory_Baseline.Set ((int)(GetRR_Seconds * 1000f));

            double c;

            switch (Respiratory_Rhythm.Value) {
                default:
                case Respiratory_Rhythms.Values.Apnea:
                    return;

                case Respiratory_Rhythms.Values.Agonal:
                    c = Utility.RandomDouble (0.8, 1.2);
                    VS_Actual.RR = (int)(c * VS_Settings.RR);
                    break;

                case Respiratory_Rhythms.Values.Apneustic:
                    VS_Actual.RR = (Utility.RandomDouble (0, 1) < 0.1) ? 6 : VS_Settings.RR;
                    break;

                case Respiratory_Rhythms.Values.Ataxic:
                    if (Utility.RandomDouble (0, 1) < 0.1)
                        VS_Actual.RR = 4;
                    else {
                        c = Utility.RandomDouble (0.8, 1.2);
                        VS_Actual.RR = (int)(c * VS_Settings.RR);
                        VS_Actual.RR_IE_E = (int)(c * VS_Settings.RR_IE_E);
                    }
                    break;

                case Respiratory_Rhythms.Values.Biot:
                    if (counterRespiratory_Arrhythmia < 0) {
                        VS_Actual.RR = 3;                               // Period of apnea, 20 sec
                        counterRespiratory_Arrhythmia = (int)(VS_Settings.RR * 0.75);   // Counter for ~45 seconds of regular rate
                    } else {
                        VS_Actual.RR = VS_Settings.RR;                  // Regular breathing
                        counterRespiratory_Arrhythmia -= 1;
                    }
                    break;

                case Respiratory_Rhythms.Values.Cheyne_Stokes:
                    if (!switchRespiratory_Arrhythmia && counterRespiratory_Arrhythmia <= 10) {
                        VS_Actual.RR += 2;                              // Ramp up breath rate
                        if (counterRespiratory_Arrhythmia == 10)        // Flip the switch when ramped up entirely
                            switchRespiratory_Arrhythmia = true;
                        else {
                            if (counterRespiratory_Arrhythmia == 0)
                                VS_Actual.RR = VS_Settings.RR;
                            counterRespiratory_Arrhythmia += 1;
                        }
                    } else if (switchRespiratory_Arrhythmia && counterRespiratory_Arrhythmia > 0) {
                        VS_Actual.RR -= 2;                              // Ramp breaths down until counter is 0
                        counterRespiratory_Arrhythmia -= 1;
                    } else {
                        VS_Actual.RR = 3;                               // Apnea for 20 seconds
                        switchRespiratory_Arrhythmia = false;           // Reset switch and counter
                        counterRespiratory_Arrhythmia = 0;
                    }
                    break;

                case Respiratory_Rhythms.Values.Regular:
                    break;
            }

            timerRespiratory_Inspiration.ResetAuto (1);
        }

        private void OnRespiratory_Inspiration () {
            Respiration_Inflated = true;
            OnPatientEvent (PatientEventTypes.Respiratory_Inspiration);
            timerRespiratory_Inspiration.Stop ();

            // Process pulsus paradoxus (numerical values) for inspiration here
            // Flip counterCardiac_Paradoxus when flipping SBP portion
            if (Pulsus_Paradoxus && Respiratory_Rhythm.Value != Respiratory_Rhythms.Values.Apnea
                && (Mechanically_Ventilated || switchParadoxus)) {
                switchParadoxus = true;
                VS_Actual.ASBP += Mechanically_Ventilated
                    ? -(int)(VS_Settings.ASBP * 0.15)
                    : (int)(VS_Settings.ASBP * 0.15);
                IABP_AP += Mechanically_Ventilated
                    ? -(int)(VS_Settings.ASBP * 0.05)
                    : (int)(VS_Settings.ASBP * 0.05);
            }

            switch (Respiratory_Rhythm.Value) {
                default:
                case Respiratory_Rhythms.Values.Apnea:
                    break;

                case Respiratory_Rhythms.Values.Agonal:
                case Respiratory_Rhythms.Values.Apneustic:
                case Respiratory_Rhythms.Values.Ataxic:
                case Respiratory_Rhythms.Values.Biot:
                case Respiratory_Rhythms.Values.Cheyne_Stokes:
                case Respiratory_Rhythms.Values.Regular:
                    timerRespiratory_Expiration.ResetAuto ((int)(GetRR_Seconds_I * 1000f));     // Expiration.Interval marks end inspiration
                    break;
            }
        }

        private void OnRespiratory_Expiration () {
            Respiration_Inflated = false;
            OnPatientEvent (PatientEventTypes.Respiratory_Expiration);
            timerRespiratory_Expiration.Stop ();

            // Process pulsus paradoxus (numerical values) for expiration here
            // Flip counterCardiac_Paradoxus when flipping SBP portion
            if (Pulsus_Paradoxus && Respiratory_Rhythm.Value != Respiratory_Rhythms.Values.Apnea
                && (!Mechanically_Ventilated || switchParadoxus)) {
                switchParadoxus = true;
                VS_Actual.ASBP += Mechanically_Ventilated
                    ? (int)(VS_Settings.ASBP * 0.15)
                    : -(int)(VS_Settings.ASBP * 0.15);
                IABP_AP += Mechanically_Ventilated
                    ? (int)(VS_Settings.ASBP * 0.05)
                    : -(int)(VS_Settings.ASBP * 0.05);
            }
        }

        private void OnObstetric_Baseline () {
            OnPatientEvent (PatientEventTypes.Obstetric_Baseline);

            if (UC_Frequency > 0 && !timerObstetric_ContractionDuration.IsRunning) {
                timerObstetric_ContractionFrequency.Continue (UC_Frequency * 1000);
            } else if (UC_Frequency <= 0) {
                timerObstetric_ContractionDuration.Stop ();
                timerObstetric_ContractionFrequency.Stop ();
            }

            if (FHR_Variability.Value == Intensity.Values.Absent)
                timerObstetric_FHRVariationFrequency.Stop ();
            else
                timerObstetric_FHRVariationFrequency.Continue (20000);
        }

        private void OnObstetric_ContractionStart () {
            OnPatientEvent (PatientEventTypes.Obstetric_ContractionStart);
            timerObstetric_ContractionDuration.ResetAuto (UC_Duration * 1000);
        }

        private void OnObstetric_ContractionEnd () {
            OnPatientEvent (PatientEventTypes.Obstetric_ContractionEnd);
            timerObstetric_ContractionDuration.Stop ();
        }

        private void OnObstetric_FetalHeartVariationStart () {
            OnPatientEvent (PatientEventTypes.Obstetric_FetalHeartVariation);
        }
    }
}