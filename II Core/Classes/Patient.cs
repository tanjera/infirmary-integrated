/* Patient.cs
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
using System.Threading.Tasks;

namespace II {

    public class Patient {
        /* Mirroring variables */
        public DateTime Updated;                    // DateTime this Patient was last updated

        /* Parameters for patient simulation, e.g. vital signs */

        public Vital_Signs VS_Settings = new Vital_Signs (),
                            VS_Actual = new Vital_Signs ();

        /* Basic vital signs and advanced hemodynamics (partial) */
        public Cardiac_Rhythms Cardiac_Rhythm = new Cardiac_Rhythms ();
        public Respiratory_Rhythms Respiratory_Rhythm = new Respiratory_Rhythms ();
        public PulmonaryArtery_Rhythms PulmonaryArtery_Placement = new PulmonaryArtery_Rhythms ();

        /* Respiratory profile (partial) */
        public bool Mechanically_Ventilated = false;
        public bool Respiration_Inflated = false;

        /* Cardiac profile */
        public int Pacemaker_Threshold;            // Patient's threshold for electrical capture to pacemaker spike

        public bool Pulsus_Paradoxus = false,
                    Pulsus_Alternans = false;

        public Cardiac_Axes Cardiac_Axis = new Cardiac_Axes ();
        public double [] ST_Elevation, T_Elevation;

        /* Obstetric profile */

        public Scales.Intensity Contraction_Intensity = new Scales.Intensity (),
                         FHR_Variability = new Scales.Intensity ();

        public double Contraction_Frequency;     // Frequency in seconds

        public int Contraction_Duration,         // Duration in seconds
                    FHR;                         // Baseline fetal heart rate

        public FHRAccelDecels FHR_AccelDecels = new FHRAccelDecels ();
        public bool Uterus_Contracted = true;

        /* General Device Settings */

        public bool TransducerZeroed_CVP = false,
                    TransducerZeroed_ABP = false,
                    TransducerZeroed_PA = false,
                    TransducerZeroed_ICP = false,
                    TransducerZeroed_IAP = false;

        /* Defibrillator parameters */

        public int Pacemaker_Rate,                  // DeviceDefib's transcutaneous pacemaker rate
                    Pacemaker_Energy;               // DeviceDefib's pacemaker energy delivery amount

        /* Intra-aortic balloon pump parameters */
        public int IABP_AP, IABP_DBP, IABP_MAP;     // Intra-aortic balloon pump blood pressures
        public bool IABP_Active = false;            // Is the Device_IABP currently augmenting?
        public string IABP_Trigger = "";            // Device_IABP's trigger; data backflow for strip processing

        /* Timers for modeling */

        private Timer timerCardiac_Baseline = new Timer (),
                        timerCardiac_Atrial_Electric = new Timer (),
                        timerCardiac_Ventricular_Electric = new Timer (),
                        timerCardiac_Atrial_Mechanical = new Timer (),
                        timerCardiac_Ventricular_Mechanical = new Timer (),
                        timerIABP_Balloon_Trigger = new Timer (),
                        timerDefibrillation = new Timer (),
                        timerPacemaker_Baseline = new Timer (),
                        timerPacemaker_Spike = new Timer (),
                        timerRespiratory_Baseline = new Timer (),
                        timerRespiratory_Inspiration = new Timer (),
                        timerRespiratory_Expiration = new Timer (),
                        timerObstetric_Baseline = new Timer (),
                        timerObstetric_Contraction = new Timer ();

        private static int Default_Electromechanical_Delay = 180;   // Delay in electrical to mechanical capture in milliseconds

        /* Internal counters and buffers for propogating aberrancies */

        private int counterCardiac_Aberrancy = 0,
                    counterCardiac_Arrhythmia = 0,
                    counterRespiratory_Arrhythmia = 0;

        private bool switchParadoxus = false,
                     switchCardiac_Arrhythmia = false,
                     switchRespiratory_Arrhythmia = false;

        /* Definitions for Vital_Signs class */

        public class Vital_Signs {
            /* Basic vital signs */

            public int HR,                      // Heart rate
                NSBP, NDBP, NMAP,               // Non-invasive blood pressures
                RR, SPO2;                       // Respiratory rate, pulse oximetry

            public double T;                    // Temperature

            /* Advanced hemodynamics */

            public int ETCO2, CVP,              // End-tidal capnography, central venous pressure,
                ASBP, ADBP, AMAP;               // Arterial line blood pressures

            public int PSP, PDP, PMP,           // Pulmonary artery pressures
                        ICP, IAP;               // Intracranial pressure, intra-abdominal pressure

            public double CO;                    // Cardiac output

            /* Respiratory profile */

            public double RR_IE_I,               // Inspiratory to expiratory ratio
                         RR_IE_E;

            public Vital_Signs () {
            }

            public Vital_Signs (Vital_Signs v)
                => Set (v);

            public async Task Set (Vital_Signs v) {
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
                CO = v.CO;
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

        public int HR {
            get { return VS_Actual.HR; }
            set { VS_Settings.HR = value; }
        }

        public int RR {
            get { return VS_Actual.RR; }
            set { VS_Settings.RR = value; }
        }

        public int ETCO2 {
            get { return VS_Actual.ETCO2; }
            set { VS_Settings.ETCO2 = value; }
        }

        public int SPO2 {
            get { return VS_Actual.SPO2; }
            set { VS_Settings.SPO2 = value; }
        }

        public int CVP {
            get { return VS_Actual.CVP; }
            set { VS_Settings.CVP = value; }
        }

        public int NSBP {
            get { return VS_Actual.NSBP; }
            set { VS_Settings.NSBP = value; }
        }

        public int NDBP {
            get { return VS_Actual.NDBP; }
            set { VS_Settings.NDBP = value; }
        }

        public int NMAP {
            get { return VS_Actual.NMAP; }
            set { VS_Settings.NMAP = value; }
        }

        public int ASBP {
            get { return VS_Actual.ASBP; }
            set { VS_Settings.ASBP = value; }
        }

        public int ADBP {
            get { return VS_Actual.ADBP; }
            set { VS_Settings.ADBP = value; }
        }

        public int AMAP {
            get { return VS_Actual.AMAP; }
            set { VS_Settings.AMAP = value; }
        }

        public double CO {
            get { return VS_Actual.CO; }
            set { VS_Settings.CO = value; }
        }

        public int PSP {
            get { return VS_Actual.PSP; }
            set { VS_Settings.PSP = value; }
        }

        public int PDP {
            get { return VS_Actual.PDP; }
            set { VS_Settings.PDP = value; }
        }

        public int PMP {
            get { return VS_Actual.PMP; }
            set { VS_Settings.PMP = value; }
        }

        public int ICP {
            get { return VS_Actual.ICP; }
            set { VS_Settings.ICP = value; }
        }

        public int IAP {
            get { return VS_Actual.IAP; }
            set { VS_Settings.IAP = value; }
        }

        public double T {
            get { return VS_Actual.T; }
            set { VS_Settings.T = value; }
        }

        public double RR_IE_I {
            get { return VS_Actual.RR_IE_I; }
            set { VS_Settings.RR_IE_I = value; }
        }

        public double RR_IE_E {
            get { return VS_Actual.RR_IE_E; }
            set { VS_Settings.RR_IE_E = value; }
        }

        public Patient () {
            Task.Run (async () => await UpdateParameters (

                            // Basic vital signs
                            80,
                            120, 80, 95,
                            18,
                            98,
                            38.0d,
                            Cardiac_Rhythms.Values.Sinus_Rhythm,
                            Respiratory_Rhythms.Values.Regular,

                            // Advanced hemodynamics
                            40,
                            6,
                            120, 80, 95,
                            6d,
                            PulmonaryArtery_Rhythms.Values.Pulmonary_Artery,
                            22, 12, 16,
                            8,
                            1,

                            // Respiratory profile
                            false,
                            1d, 2d,

                            // Cardiac profile
                            50,
                            false, false,
                            Cardiac_Axes.Values.Normal,
                            new double [] { 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d },
                            new double [] { 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d },

                            // Obstetric profile
                            150,
                            Scales.Intensity.Values.Mild,
                            new List<FHRAccelDecels.Values> (),
                            300,
                            60,
                            Scales.Intensity.Values.Moderate));

            Task.Run (async () => await InitTimers ());
            Task.Run (async () => await SetTimers ());
        }

        ~Patient () => Task.Run (async () => await Dispose ());

        public async Task Dispose () {
            await UnsubscribePatientEvent ();

            timerCardiac_Baseline.Dispose ();
            timerCardiac_Atrial_Electric.Dispose ();
            timerCardiac_Ventricular_Electric.Dispose ();
            timerCardiac_Atrial_Mechanical.Dispose ();
            timerCardiac_Ventricular_Mechanical.Dispose ();
            timerIABP_Balloon_Trigger.Dispose ();
            timerDefibrillation.Dispose ();
            timerPacemaker_Baseline.Dispose ();
            timerPacemaker_Spike.Dispose ();
            timerRespiratory_Baseline.Dispose ();
            timerRespiratory_Inspiration.Dispose ();
            timerRespiratory_Expiration.Dispose ();
            timerObstetric_Baseline.Dispose ();
            timerObstetric_Contraction.Dispose ();
        }

        public async Task Activate () {
            await SetTimers ();
        }

        public async Task Deactivate () {
            await UnsubscribePatientEvent ();
            await StopTimers ();
        }

        public async Task UnsubscribePatientEvent () {
            if (PatientEvent != null) {
                foreach (Delegate d in PatientEvent?.GetInvocationList ())
                    PatientEvent -= (EventHandler<PatientEventArgs>)d;
            }
        }

        /* PatientEvent event, handler, and caller */
        public readonly object lockListPatientEvents = new object ();
        public List<PatientEventArgs> ListPatientEvents = new List<PatientEventArgs> ();

        public event EventHandler<PatientEventArgs> PatientEvent;

        public class PatientEventArgs : EventArgs {
            public Patient Patient;                         // Remains as a pointer
            public Vital_Signs Vitals;                      // Copies over as a clone, not a pointer
            public PatientEventTypes EventType;
            public DateTime Occurred;

            public PatientEventArgs (Patient p, PatientEventTypes e) {
                EventType = e;
                Patient = p;
                Vitals = new Vital_Signs (p.VS_Actual);
                Occurred = DateTime.Now;
            }
        }

        public enum PatientEventTypes {
            Vitals_Change,
            Cardiac_Baseline,
            Cardiac_Atrial_Electric,
            Cardiac_Ventricular_Electric,
            Cardiac_Atrial_Mechanical,
            Cardiac_Ventricular_Mechanical,
            IABP_Balloon_Inflation,
            Defibrillation,
            Pacermaker_Spike,
            Respiratory_Baseline,
            Respiratory_Inspiration,
            Respiratory_Expiration,
            Obstetric_Baseline,
            Obstetric_Contraction_Start,
            Obstetric_Contraction_End
        }

        public async Task OnPatientEvent (PatientEventTypes e) {
            PatientEventArgs ea = new PatientEventArgs (this, e);

            lock (lockListPatientEvents) {
                ListPatientEvents.Add (ea);
            }

            PatientEvent?.Invoke (this, ea);
        }

        public async Task CleanListPatientEvents () {
            // Remove all listings older than 1 minute... prevent cluttering memory
            lock (lockListPatientEvents) {
                for (int i = ListPatientEvents.Count - 1; i >= 0; i--)
                    if (ListPatientEvents [i].Occurred.CompareTo (DateTime.Now.AddMinutes (-1)) < 0)
                        ListPatientEvents.RemoveAt (i);
            }
        }

        /* Methods for counting, calculating, and measuring vital signs, timing re: vital signs, etc. */

        public static int CalculateMAP (int sbp, int dbp) {
            return dbp + ((sbp - dbp) / 3);
        }

        public static int CalculateCPP (int icp, int map) {
            return map - icp;
        }

        public double GetHR_Seconds { get { return 60d / System.Math.Max (1, VS_Actual.HR); } }
        public double GetRR_Seconds { get { return 60d / System.Math.Max (1, VS_Actual.RR); } }
        public double GetRR_Seconds_I { get { return (GetRR_Seconds / (RR_IE_I + RR_IE_E)) * RR_IE_I; } }
        public double GetRR_Seconds_E { get { return (GetRR_Seconds / (RR_IE_I + RR_IE_E)) * RR_IE_E; } }
        public double GetPulsatility_Seconds { get { return System.Math.Min (GetHR_Seconds * 0.75d, 0.75d); } }

        public int MeasureHR_ECG (double lengthSeconds, double offsetSeconds)
            => MeasureHR (lengthSeconds, offsetSeconds, false);

        public int MeasureHR_SPO2 (double lengthSeconds, double offsetSeconds)
            => MeasureHR (lengthSeconds, offsetSeconds, true);

        public int MeasureHR (double lengthSeconds, double offsetSeconds, bool isPulse = false) {
            _ = Task.Run (async () => { await CleanListPatientEvents (); });

            if (isPulse && !Cardiac_Rhythm.HasPulse_Ventricular)
                return 0;

            int counter = 0;

            lock (lockListPatientEvents) {
                foreach (PatientEventArgs ea in ListPatientEvents)
                    if (ea.EventType == PatientEventTypes.Cardiac_Ventricular_Electric
                        && ea.Occurred.CompareTo (DateTime.Now.AddSeconds (-(lengthSeconds + offsetSeconds))) >= 0
                        && ea.Occurred.CompareTo (DateTime.Now.AddSeconds (-offsetSeconds)) <= 0)
                        counter++;
            }

            return (int)(counter / (lengthSeconds / 60d));
        }

        public int MeasureRR (double lengthSeconds, double offsetSeconds) {
            _ = Task.Run (async () => { await CleanListPatientEvents (); });

            int counter = 0;

            lock (lockListPatientEvents) {
                foreach (PatientEventArgs ea in ListPatientEvents)
                    if (ea.EventType == PatientEventTypes.Respiratory_Inspiration
                        && ea.Occurred.CompareTo (DateTime.Now.AddSeconds (-(lengthSeconds + offsetSeconds))) >= 0
                        && ea.Occurred.CompareTo (DateTime.Now.AddSeconds (-offsetSeconds)) <= 0)
                        counter++;
            }

            return (int)(counter / (lengthSeconds / 60));
        }

        /* Process all timers for patient modeling */

        public void ProcessTimers (object sender, EventArgs e) {
            /* For cross-platform compatibility with different timers ...
             * When creating a Patient object, create a native thread-safe Timer object,
             * short interval, and call this function on its Tick to process all Patient
             * timers.
             */
            timerCardiac_Baseline.Process ();
            timerCardiac_Atrial_Electric.Process ();
            timerCardiac_Ventricular_Electric.Process ();
            timerCardiac_Atrial_Mechanical.Process ();
            timerCardiac_Ventricular_Mechanical.Process ();
            timerIABP_Balloon_Trigger.Process ();
            timerDefibrillation.Process ();
            timerPacemaker_Baseline.Process ();
            timerPacemaker_Spike.Process ();
            timerRespiratory_Baseline.Process ();
            timerRespiratory_Inspiration.Process ();
            timerRespiratory_Expiration.Process ();
            timerObstetric_Baseline.Process ();
            timerObstetric_Contraction.Process ();
        }

        /* Process for loading Patient{} information from simulation file */

        public async Task Load_Process (string inc) {
            StringReader sRead = new StringReader (inc);

            try {
                string line;
                while ((line = sRead.ReadLine ()) != null) {
                    if (line.Contains (":")) {
                        string pName = line.Substring (0, line.IndexOf (':')),
                                pValue = line.Substring (line.IndexOf (':') + 1).Trim ();
                        switch (pName) {
                            default: break;

                            // Patient/scenario information
                            case "Updated": Updated = Utility.DateTime_FromString (pValue); break;

                            // Basic vital signs
                            case "HR": VS_Settings.HR = int.Parse (pValue); break;
                            case "NSBP": VS_Settings.NSBP = int.Parse (pValue); break;
                            case "NDBP": VS_Settings.NDBP = int.Parse (pValue); break;
                            case "NMAP": VS_Settings.NMAP = int.Parse (pValue); break;
                            case "RR": VS_Settings.RR = int.Parse (pValue); break;
                            case "SPO2": VS_Settings.SPO2 = int.Parse (pValue); break;
                            case "T": VS_Settings.T = double.Parse (pValue); break;

                            // Rhythms
                            case "Cardiac_Rhythm": Cardiac_Rhythm.Value = (Cardiac_Rhythms.Values)Enum.Parse (typeof (Cardiac_Rhythms.Values), pValue); break;
                            case "Respiratory_Rhythm": Respiratory_Rhythm.Value = (Respiratory_Rhythms.Values)Enum.Parse (typeof (Respiratory_Rhythms.Values), pValue); break;
                            case "PulmonaryArtery_Rhythm": PulmonaryArtery_Placement.Value = (PulmonaryArtery_Rhythms.Values)Enum.Parse (typeof (PulmonaryArtery_Rhythms.Values), pValue); break;

                            // Advanced hemodynamics
                            case "ETCO2": VS_Settings.ETCO2 = int.Parse (pValue); break;
                            case "CVP": VS_Settings.CVP = int.Parse (pValue); break;
                            case "ASBP": VS_Settings.ASBP = int.Parse (pValue); break;
                            case "ADBP": VS_Settings.ADBP = int.Parse (pValue); break;
                            case "AMAP": VS_Settings.AMAP = int.Parse (pValue); break;
                            case "CO": VS_Settings.CO = double.Parse (pValue); break;
                            case "PSP": VS_Settings.PSP = int.Parse (pValue); break;
                            case "PDP": VS_Settings.PDP = int.Parse (pValue); break;
                            case "PMP": VS_Settings.PMP = int.Parse (pValue); break;
                            case "ICP": VS_Settings.ICP = int.Parse (pValue); break;
                            case "IAP": VS_Settings.IAP = int.Parse (pValue); break;

                            // Respiratory profile
                            case "Mechanically_Ventilated": Mechanically_Ventilated = bool.Parse (pValue); break;
                            case "Respiratory_IERatio_I": VS_Settings.RR_IE_I = double.Parse (pValue); break;
                            case "Respiratory_IERatio_E": VS_Settings.RR_IE_E = double.Parse (pValue); break;
                            case "Respiration_Inflated": Respiration_Inflated = bool.Parse (pValue); break;

                            // Cardiac profile
                            case "Pacemaker_Threshold": Pacemaker_Threshold = int.Parse (pValue); break;
                            case "PulsusParadoxus": Pulsus_Paradoxus = bool.Parse (pValue); break;
                            case "PulsusAlternans": Pulsus_Alternans = bool.Parse (pValue); break;
                            case "Cardiac_Axis": Cardiac_Axis.Value = (Cardiac_Axes.Values)Enum.Parse (typeof (Cardiac_Axes.Values), pValue); break;

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
                            case "FHR_Variability": FHR_Variability.Value = (Scales.Intensity.Values)Enum.Parse (typeof (Scales.Intensity.Values), pValue); break;
                            case "FHR_Rhythms":
                                foreach (string fhr_rhythm in pValue.Split (',').Where ((o) => o != ""))
                                    FHR_AccelDecels.ValueList.Add ((FHRAccelDecels.Values)Enum.Parse (typeof (FHRAccelDecels.Values), fhr_rhythm));
                                break;

                            case "UterineContraction_Frequency": Contraction_Frequency = double.Parse (pValue); break;
                            case "UterineContraction_Duration": Contraction_Duration = int.Parse (pValue); break;
                            case "UterineContraction_Intensity": Contraction_Intensity.Value = (Scales.Intensity.Values)Enum.Parse (typeof (Scales.Intensity.Values), pValue); break;
                            case "Uterus_Contracted": Uterus_Contracted = bool.Parse (pValue); break;

                            // Device settings
                            case "TransducerZeroed_ABP": TransducerZeroed_ABP = bool.Parse (pValue); break;
                            case "TransducerZeroed_CVP": TransducerZeroed_CVP = bool.Parse (pValue); break;
                            case "TransducerZeroed_PA": TransducerZeroed_PA = bool.Parse (pValue); break;
                            case "TransducerZeroed_ICP": TransducerZeroed_ICP = bool.Parse (pValue); break;
                            case "TransducerZeroed_IAP": TransducerZeroed_IAP = bool.Parse (pValue); break;

                            case "Pacemaker_Rate": Pacemaker_Rate = int.Parse (pValue); break;
                            case "Pacemaker_Energy": Pacemaker_Energy = int.Parse (pValue); break;

                            case "IABP_AP": IABP_AP = int.Parse (pValue); break;
                            case "IABP_DBP": IABP_DBP = int.Parse (pValue); break;
                            case "IABP_MAP": IABP_MAP = int.Parse (pValue); break;
                            case "IABP_Active": IABP_Active = bool.Parse (pValue); break;
                            case "IABP_Trigger": IABP_Trigger = pValue; break;
                        }
                    }
                }
            } catch {
                /* If the load fails... just bail on the actual value parsing and continue the load process */
            }

            sRead.Close ();

            // Reset measurements to set parameters
            await VS_Actual.Set (VS_Settings);

            await SetTimers ();
            await OnCardiac_Baseline ();
            await OnRespiratory_Baseline ();

            await OnPatientEvent (PatientEventTypes.Vitals_Change);
        }

        /* Process for saving Patient{} information to simulation file  */

        public string Save () {
            StringBuilder sWrite = new StringBuilder ();

            // File/scenario information
            sWrite.AppendLine (String.Format ("{0}:{1}", "Updated", Utility.DateTime_ToString (Updated)));

            // Basic vital signs
            sWrite.AppendLine (String.Format ("{0}:{1}", "HR", VS_Settings.HR));
            sWrite.AppendLine (String.Format ("{0}:{1}", "NSBP", VS_Settings.NSBP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "NDBP", VS_Settings.NDBP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "NMAP", VS_Settings.NMAP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "RR", VS_Settings.RR));
            sWrite.AppendLine (String.Format ("{0}:{1}", "SPO2", VS_Settings.SPO2));
            sWrite.AppendLine (String.Format ("{0}:{1}", "T", VS_Settings.T));

            // Rhythms
            sWrite.AppendLine (String.Format ("{0}:{1}", "Cardiac_Rhythm", Cardiac_Rhythm.Value));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Respiratory_Rhythm", Respiratory_Rhythm.Value));
            sWrite.AppendLine (String.Format ("{0}:{1}", "PulmonaryArtery_Rhythm", PulmonaryArtery_Placement.Value));

            // Advanced hemodynamics
            sWrite.AppendLine (String.Format ("{0}:{1}", "ETCO2", VS_Settings.ETCO2));
            sWrite.AppendLine (String.Format ("{0}:{1}", "CVP", VS_Settings.CVP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "ASBP", VS_Settings.ASBP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "ADBP", VS_Settings.ADBP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "AMAP", VS_Settings.AMAP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "CO", VS_Settings.CO));
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
            sWrite.AppendLine (String.Format ("{0}:{1}", "PulsusParadoxus", Pulsus_Paradoxus));
            sWrite.AppendLine (String.Format ("{0}:{1}", "PulsusAlternans", Pulsus_Alternans));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Cardiac_Axis", Cardiac_Axis.Value));
            sWrite.AppendLine (String.Format ("{0}:{1}", "ST_Elevation", string.Join (",", ST_Elevation)));
            sWrite.AppendLine (String.Format ("{0}:{1}", "T_Elevation", string.Join (",", T_Elevation)));

            // Obstetric profile
            sWrite.AppendLine (String.Format ("{0}:{1}", "FHR", FHR));
            sWrite.AppendLine (String.Format ("{0}:{1}", "FHR_Variability", FHR_Variability.Value));
            sWrite.AppendLine (String.Format ("{0}:{1}", "FHR_Rhythms", string.Join (",", FHR_AccelDecels.ValueList)));
            sWrite.AppendLine (String.Format ("{0}:{1}", "UterineContraction_Frequency", Contraction_Frequency));
            sWrite.AppendLine (String.Format ("{0}:{1}", "UterineContraction_Duration", Contraction_Duration));
            sWrite.AppendLine (String.Format ("{0}:{1}", "UterineContraction_Intensity", Contraction_Intensity.Value));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Uterus_Contracted", Uterus_Contracted));

            // Device settings
            sWrite.AppendLine (String.Format ("{0}:{1}", "TransducerZeroed_ABP", TransducerZeroed_ABP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "TransducerZeroed_CVP", TransducerZeroed_CVP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "TransducerZeroed_PA", TransducerZeroed_PA));
            sWrite.AppendLine (String.Format ("{0}:{1}", "TransducerZeroed_ICP", TransducerZeroed_ICP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "TransducerZeroed_IAP", TransducerZeroed_IAP));

            sWrite.AppendLine (String.Format ("{0}:{1}", "Pacemaker_Rate", Pacemaker_Rate));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Pacemaker_Energy", Pacemaker_Energy));

            sWrite.AppendLine (String.Format ("{0}:{1}", "IABP_AP", IABP_AP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "IABP_DBP", IABP_DBP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "IABP_MAP", IABP_MAP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "IABP_Active", IABP_Active));
            sWrite.AppendLine (String.Format ("{0}:{1}", "IABP_Trigger", IABP_Trigger));

            return sWrite.ToString ();
        }

        public async Task UpdateParameters (

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
                    double co,
                    PulmonaryArtery_Rhythms.Values pa_placement,
                    int psp, int pdp, int pmp,
                    int icp, int iap,

                    // Respiratory profile
                    bool mech_vent,
                    double resp_ier_i, double resp_ier_e,

                    // Cardiac profile
                    int pacer_threshold,
                    bool puls_paradoxus, bool puls_alternans,
                    Cardiac_Axes.Values card_axis,
                    double [] st_elev, double [] t_elev,

                    // Obstetric profile
                    int fhr, Scales.Intensity.Values fhr_var, List<FHRAccelDecels.Values> fhr_rhythms,
                    double uc_freq, int uc_duration, Scales.Intensity.Values uc_intensity) {
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

            VS_Settings.CO = co;

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
            FHR_AccelDecels.ValueList = fhr_rhythms;
            Contraction_Frequency = uc_freq;
            Contraction_Duration = uc_duration;
            Contraction_Intensity.Value = uc_intensity;

            // Reset actual vital signs to set parameters
            await VS_Actual.Set (VS_Settings);

            await SetTimers ();
            await OnCardiac_Baseline ();
            await OnRespiratory_Baseline ();

            await OnPatientEvent (PatientEventTypes.Vitals_Change);
        }

        public async Task ClampVitals (
                    int hrMin, int hrMax,
                    int spo2Min, int spo2Max,
                    int etco2Min, int etco2Max,
                    int sbpMin, int sbpMax, int dbpMin, int dbpMax,
                    int pspMin, int pspMax, int pdpMin, int pdpMax) {
            VS_Settings.HR = II.Math.Clamp (VS_Settings.HR, hrMin, hrMax);
            VS_Settings.SPO2 = II.Math.Clamp (VS_Settings.SPO2, spo2Min, spo2Max);
            VS_Settings.ETCO2 = II.Math.Clamp (VS_Settings.ETCO2, etco2Min, etco2Max);
            VS_Settings.NSBP = II.Math.Clamp (VS_Settings.NSBP, sbpMin, sbpMax);
            VS_Settings.NDBP = II.Math.Clamp (VS_Settings.NDBP, dbpMin, dbpMax);
            VS_Settings.NMAP = Patient.CalculateMAP (VS_Settings.NSBP, VS_Settings.NDBP);
            VS_Settings.ASBP = II.Math.Clamp (VS_Settings.ASBP, sbpMin, sbpMax);
            VS_Settings.ADBP = II.Math.Clamp (VS_Settings.ADBP, sbpMin, sbpMax);
            VS_Settings.AMAP = Patient.CalculateMAP (VS_Settings.ASBP, VS_Settings.ADBP);
            VS_Settings.PSP = II.Math.Clamp (VS_Settings.PSP, pspMin, pspMax);
            VS_Settings.PDP = II.Math.Clamp (VS_Settings.PDP, pdpMin, pdpMax);
            VS_Settings.PMP = Patient.CalculateMAP (VS_Settings.PSP, VS_Settings.PDP);

            await VS_Actual.Set (VS_Settings);

            switchParadoxus = false;

            await SetTimers ();
            await OnCardiac_Baseline ();
            await OnRespiratory_Baseline ();
            await OnObstetric_Baseline ();

            await OnPatientEvent (PatientEventTypes.Vitals_Change);
        }

        private async Task InitTimers () {
            timerCardiac_Baseline.Tick += async delegate { await OnCardiac_Baseline (); };
            timerCardiac_Atrial_Electric.Tick += async delegate { await OnCardiac_Atrial_Electric (); };
            timerCardiac_Ventricular_Electric.Tick += async delegate { await OnCardiac_Ventricular_Electric (); };
            timerCardiac_Atrial_Mechanical.Tick += async delegate { await OnCardiac_Atrial_Mechanical (); };
            timerCardiac_Ventricular_Mechanical.Tick += async delegate { await OnCardiac_Ventricular_Mechanical (); };
            timerIABP_Balloon_Trigger.Tick += async delegate { await OnIABP_Balloon_Inflate (); };
            timerDefibrillation.Tick += async delegate { await OnDefibrillation_End (); };
            timerPacemaker_Baseline.Tick += async delegate { await OnPacemaker_Baseline (); };
            timerPacemaker_Spike.Tick += async delegate { await OnPacemaker_Spike (); };

            timerRespiratory_Baseline.Tick += async delegate { await OnRespiratory_Baseline (); };
            timerRespiratory_Inspiration.Tick += async delegate { await OnRespiratory_Inspiration (); };
            timerRespiratory_Expiration.Tick += async delegate { await OnRespiratory_Expiration (); };

            timerObstetric_Baseline.Tick += async delegate { await OnObstetric_Baseline (); };
            timerObstetric_Contraction.Tick += async delegate { await OnObstetric_Contraction (); };
        }

        private async Task SetTimers () {
            timerCardiac_Baseline.ResetAuto ((int)(GetHR_Seconds * 1000d));
            timerCardiac_Atrial_Electric.Stop ();
            timerCardiac_Ventricular_Electric.Stop ();
            timerCardiac_Atrial_Mechanical.Stop ();
            timerCardiac_Ventricular_Mechanical.Stop ();
            timerIABP_Balloon_Trigger.Stop ();

            timerDefibrillation.ResetAuto ();
            if (timerPacemaker_Baseline.IsRunning)
                timerPacemaker_Baseline.ResetAuto ();
            else
                timerPacemaker_Baseline.Reset ();
            timerPacemaker_Spike.Stop ();

            timerRespiratory_Baseline.ResetAuto ((int)(GetRR_Seconds * 1000d));
            timerRespiratory_Inspiration.Stop ();
            timerRespiratory_Expiration.Stop ();

            timerObstetric_Baseline.ResetAuto (1);
            timerObstetric_Contraction.Stop ();
        }

        private async Task StopTimers () {
            timerCardiac_Baseline.Stop ();
            timerCardiac_Atrial_Electric.Stop ();
            timerCardiac_Ventricular_Electric.Stop ();
            timerCardiac_Atrial_Mechanical.Stop ();
            timerCardiac_Ventricular_Mechanical.Stop ();
            timerIABP_Balloon_Trigger.Stop ();
            timerDefibrillation.Stop ();
            timerPacemaker_Baseline.Stop ();
            timerPacemaker_Spike.Stop ();

            timerRespiratory_Baseline.Stop ();
            timerRespiratory_Inspiration.Stop ();
            timerRespiratory_Expiration.Stop ();

            timerObstetric_Baseline.Stop ();
            timerObstetric_Contraction.Stop ();
        }

        public async Task Defibrillate ()
            => await InitDefibrillation (false);

        public async Task Cardiovert ()
            => await InitDefibrillation (true);

        public async Task Pacemaker (bool active, int rate, int energy) {
            Pacemaker_Rate = rate;
            Pacemaker_Energy = energy;

            // If rate == 0, must stop timer! Otherwise timer.Interval is set to 0!
            if (!active || rate == 0 || energy == 0)
                await StopPacemaker ();
            else if (active)
                await StartPacemaker ();
        }

        public async Task PacemakerPause () => timerPacemaker_Baseline.Set (4000);

        private async Task InitDefibrillation (bool toSynchronize) {
            if (toSynchronize)
                timerCardiac_Ventricular_Electric.Tick += OnCardioversion;
            else
                await OnDefibrillation ();
        }

        public bool Pacemaker_HasCapture {
            get {
                return timerPacemaker_Baseline.IsRunning
                    && Pacemaker_Energy > 0 && Pacemaker_Rate > 0
                    && Pacemaker_Energy >= Pacemaker_Threshold;
            }
        }

        private async Task StartPacemaker ()
            => timerPacemaker_Baseline.ResetAuto ((int)((60d / Pacemaker_Rate) * 1000));

        private async Task StopPacemaker () {
            timerPacemaker_Baseline.Stop ();
            timerPacemaker_Spike.Stop ();
        }

        private async Task OnDefibrillation () {
            timerCardiac_Baseline.Stop ();
            timerCardiac_Atrial_Electric.Stop ();
            timerCardiac_Ventricular_Electric.Stop ();
            timerCardiac_Atrial_Mechanical.Stop ();
            timerCardiac_Ventricular_Mechanical.Stop ();
            timerDefibrillation.ResetAuto (20);

            // Invoke the defibrillation event *after* starting the timer- IsDefibrillating() checks the timer!
            await OnPatientEvent (PatientEventTypes.Defibrillation);
        }

        private async Task OnDefibrillation_End () {
            timerDefibrillation.Stop ();
            timerCardiac_Baseline.ResetAuto ();
        }

        private void OnCardioversion (object sender, EventArgs e) {
            timerCardiac_Ventricular_Electric.Tick -= OnCardioversion;
            Task.Run (async () => OnDefibrillation ());
        }

        private async Task OnPacemaker_Baseline () {
            if (Pacemaker_Energy > 0)
                await OnPatientEvent (PatientEventTypes.Pacermaker_Spike);

            if (Pacemaker_Energy >= Pacemaker_Threshold)
                timerPacemaker_Spike.ResetAuto (40);        // Adds an interval between the spike and the QRS complex

            await StartPacemaker ();                          // In case pacemaker was paused... updates .Interval
        }

        private async Task OnPacemaker_Spike () {
            timerPacemaker_Spike.Stop ();

            // Trigger the QRS complex, then reset the heart's intrinsic timers
            if (Cardiac_Rhythm.CanBe_Paced) {
                Cardiac_Rhythm.AberrantBeat = true;
                await OnCardiac_Ventricular_Electric ();
                Cardiac_Rhythm.AberrantBeat = false;
                timerCardiac_Baseline.ResetAuto ();
            }
        }

        private async Task OnCardiac_Baseline () {
            await OnPatientEvent (PatientEventTypes.Cardiac_Baseline);
            timerCardiac_Baseline.Set ((int)(GetHR_Seconds * 1000d));

            switch (Cardiac_Rhythm.Value) {
                default:
                case Cardiac_Rhythms.Values.Asystole:
                    break;

                // Traced as "regular V" Rhythms
                case Cardiac_Rhythms.Values.Atrial_Flutter:
                case Cardiac_Rhythms.Values.CPR_Artifact:
                case Cardiac_Rhythms.Values.Junctional:
                case Cardiac_Rhythms.Values.Idioventricular:
                case Cardiac_Rhythms.Values.Supraventricular_Tachycardia:
                case Cardiac_Rhythms.Values.Ventricular_Tachycardia_Monomorphic_Pulsed:
                case Cardiac_Rhythms.Values.Ventricular_Tachycardia_Monomorphic_Pulseless:
                case Cardiac_Rhythms.Values.Ventricular_Tachycardia_Polymorphic:
                case Cardiac_Rhythms.Values.Ventricular_Fibrillation_Coarse:
                case Cardiac_Rhythms.Values.Ventricular_Fibrillation_Fine:
                    timerCardiac_Ventricular_Electric.ResetAuto (1);
                    break;

                // Traced as "regular A" or "regular A -> V" Rhythms
                case Cardiac_Rhythms.Values.AV_Block__1st_Degree:
                case Cardiac_Rhythms.Values.AV_Block__Mobitz_II:
                case Cardiac_Rhythms.Values.AV_Block__Wenckebach:
                case Cardiac_Rhythms.Values.Bundle_Branch_Block:
                case Cardiac_Rhythms.Values.Sinus_Rhythm:
                case Cardiac_Rhythms.Values.Pulseless_Electrical_Activity:
                case Cardiac_Rhythms.Values.Ventricular_Standstill:
                    timerCardiac_Atrial_Electric.ResetAuto (1);
                    break;

                // Traced as "irregular V" rhythms
                case Cardiac_Rhythms.Values.Atrial_Fibrillation:
                    VS_Actual.HR = (int)(VS_Settings.HR * II.Math.RandomDouble (0.6, 1.4));
                    timerCardiac_Ventricular_Electric.ResetAuto (1);
                    break;

                /* Special Cases */
                case Cardiac_Rhythms.Values.AV_Block__3rd_Degree:
                    if (!timerCardiac_Atrial_Electric.IsRunning)
                        timerCardiac_Atrial_Electric.ResetAuto ((int)(timerCardiac_Baseline.Interval * 0.6d));
                    timerCardiac_Ventricular_Electric.ResetAuto (160);
                    break;

                case Cardiac_Rhythms.Values.Sick_Sinus_Syndrome:
                    /* Countdown to 0; on 0, switch between tachy/brady; brady runs 8-12 beats, tachy runs 20-30 beats */
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

                    timerCardiac_Atrial_Electric.ResetAuto (1);
                    break;

                case Cardiac_Rhythms.Values.Sinus_Arrhythmia:
                    if (Respiration_Inflated)
                        VS_Actual.HR = (int)(VS_Settings.HR * 1.075d);
                    else
                        VS_Actual.HR = (int)(VS_Settings.HR * 0.925d);

                    timerCardiac_Atrial_Electric.ResetAuto (1);
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

                    timerCardiac_Atrial_Electric.ResetAuto (1);
                    break;

                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_PACs:
                    counterCardiac_Aberrancy -= 1;
                    VS_Actual.HR = VS_Settings.HR;
                    if (counterCardiac_Aberrancy <= 0) {
                        counterCardiac_Aberrancy = new Random ().Next (4, 8);
                        VS_Actual.HR = (int)(VS_Settings.HR * II.Math.RandomDouble (0.6d, 0.8d));
                    } else {
                        VS_Actual.HR = VS_Settings.HR;
                    }
                    timerCardiac_Atrial_Electric.ResetAuto (1);
                    break;

                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_PJCs:
                    counterCardiac_Aberrancy -= 1;
                    VS_Actual.HR = VS_Settings.HR;
                    if (counterCardiac_Aberrancy <= 0) {
                        counterCardiac_Aberrancy = new Random ().Next (4, 8);
                        timerCardiac_Ventricular_Electric.ResetAuto (1);
                    } else {
                        if (counterCardiac_Aberrancy == 1)
                            VS_Actual.HR = (int)(VS_Settings.HR * II.Math.RandomDouble (0.7d, 0.9d));
                        timerCardiac_Atrial_Electric.ResetAuto (1);
                    }
                    break;

                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_Bigeminy:
                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_Trigeminy:
                    counterCardiac_Aberrancy -= 1;
                    if (counterCardiac_Aberrancy == 0) {
                        timerCardiac_Baseline.Set ((int)(timerCardiac_Baseline.Interval * 0.8d));
                    } else if (counterCardiac_Aberrancy < 0) {   // Then throw the PVC and reset the counters
                        if (Cardiac_Rhythm.Value == Cardiac_Rhythms.Values.Sinus_Rhythm_with_Bigeminy)
                            counterCardiac_Aberrancy = 1;
                        else if (Cardiac_Rhythm.Value == Cardiac_Rhythms.Values.Sinus_Rhythm_with_Trigeminy)
                            counterCardiac_Aberrancy = 2;
                        Cardiac_Rhythm.AberrantBeat = true;
                        timerCardiac_Ventricular_Electric.ResetAuto (1);
                        break;
                    }
                    timerCardiac_Atrial_Electric.ResetAuto (1);
                    Cardiac_Rhythm.AberrantBeat = false;
                    break;

                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_PVCs_Unifocal:
                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_PVCs_Multifocal:
                    counterCardiac_Aberrancy -= 1;
                    VS_Actual.HR = VS_Settings.HR;
                    if (counterCardiac_Aberrancy == 0) {  // Shorten the beat preceding the PVC, making it premature
                        VS_Actual.HR = (int)(VS_Settings.HR * 0.8d);
                    } else if (counterCardiac_Aberrancy < 0) {   // Then throw the PVC and reset the counters
                        counterCardiac_Aberrancy = new Random ().Next (4, 9);
                        Cardiac_Rhythm.AberrantBeat = true;
                        timerCardiac_Ventricular_Electric.ResetAuto (1);
                        break;
                    }
                    Cardiac_Rhythm.AberrantBeat = false;
                    timerCardiac_Atrial_Electric.ResetAuto (1);
                    break;
            }
        }

        private async Task OnCardiac_Atrial_Electric () {
            await OnPatientEvent (PatientEventTypes.Cardiac_Atrial_Electric);

            if (Cardiac_Rhythm.HasPulse_Atrial)
                timerCardiac_Atrial_Mechanical.ResetAuto (Default_Electromechanical_Delay);

            switch (Cardiac_Rhythm.Value) {
                default:
                case Cardiac_Rhythms.Values.Asystole:
                    break;

                // Regular A Rhythms
                case Cardiac_Rhythms.Values.Ventricular_Standstill:
                    timerCardiac_Atrial_Electric.Stop ();
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
                    timerCardiac_Atrial_Electric.Stop ();
                    timerCardiac_Ventricular_Electric.ResetAuto (160);
                    break;

                /* Special cases */

                case Cardiac_Rhythms.Values.AV_Block__1st_Degree:
                    timerCardiac_Atrial_Electric.Stop ();
                    timerCardiac_Ventricular_Electric.ResetAuto (240);
                    break;

                case Cardiac_Rhythms.Values.AV_Block__Mobitz_II:
                    timerCardiac_Atrial_Electric.Stop ();
                    counterCardiac_Aberrancy -= 1;
                    if (counterCardiac_Aberrancy < 0) {
                        counterCardiac_Aberrancy = 2;
                        Cardiac_Rhythm.AberrantBeat = true;
                    } else {
                        timerCardiac_Ventricular_Electric.ResetAuto (160);
                        Cardiac_Rhythm.AberrantBeat = false;
                    }
                    break;

                case Cardiac_Rhythms.Values.AV_Block__Wenckebach:
                    timerCardiac_Atrial_Electric.Stop ();
                    counterCardiac_Aberrancy -= 1;
                    if (counterCardiac_Aberrancy < 0) {
                        counterCardiac_Aberrancy = 3;
                        Cardiac_Rhythm.AberrantBeat = true;
                    } else {
                        timerCardiac_Baseline.Set ((int)(timerCardiac_Baseline.Interval + (160d * (3d - counterCardiac_Aberrancy))));
                        timerCardiac_Ventricular_Electric.ResetAuto ((int)(160d * (3d - counterCardiac_Aberrancy)));
                        Cardiac_Rhythm.AberrantBeat = false;
                    }
                    break;

                case Cardiac_Rhythms.Values.AV_Block__3rd_Degree:
                    /* Specifically let atrial timer continue to run and propogate P-waves! */
                    break;
            }
        }

        private async Task OnCardiac_Ventricular_Electric () {
            await OnPatientEvent (PatientEventTypes.Cardiac_Ventricular_Electric);

            if (Cardiac_Rhythm.HasPulse_Ventricular)
                timerCardiac_Ventricular_Mechanical.ResetAuto (Default_Electromechanical_Delay);

            /* Flip the switch on pulsus alternans */
            Cardiac_Rhythm.AlternansBeat = Pulsus_Alternans ? !Cardiac_Rhythm.AlternansBeat : false;

            timerCardiac_Ventricular_Electric.Stop ();
        }

        private async Task OnCardiac_Atrial_Mechanical () {
            await OnPatientEvent (PatientEventTypes.Cardiac_Atrial_Mechanical);

            timerCardiac_Atrial_Mechanical.Stop ();
        }

        private async Task OnCardiac_Ventricular_Mechanical () {
            await OnPatientEvent (PatientEventTypes.Cardiac_Ventricular_Mechanical);

            if (IABP_Active)
                timerIABP_Balloon_Trigger.ResetAuto ((int)(GetHR_Seconds * 1000d * 0.35d));

            timerCardiac_Ventricular_Mechanical.Stop ();
        }

        private async Task OnIABP_Balloon_Inflate () {
            await OnPatientEvent (PatientEventTypes.IABP_Balloon_Inflation);

            timerIABP_Balloon_Trigger.Stop ();
        }

        private async Task OnRespiratory_Baseline () {
            await OnPatientEvent (PatientEventTypes.Respiratory_Baseline);
            timerRespiratory_Baseline.Set ((int)(GetRR_Seconds * 1000d));

            double c;

            switch (Respiratory_Rhythm.Value) {
                default:
                case Respiratory_Rhythms.Values.Apnea:
                    return;

                case Respiratory_Rhythms.Values.Agonal:
                    c = II.Math.RandomDouble (0.8d, 1.2d);
                    VS_Actual.RR = (int)(c * VS_Settings.RR);
                    break;

                case Respiratory_Rhythms.Values.Apneustic:
                    VS_Actual.RR = (II.Math.RandomDouble (0, 1d) < 0.1d) ? 6 : VS_Settings.RR;
                    break;

                case Respiratory_Rhythms.Values.Ataxic:
                    if (II.Math.RandomDouble (0, 1) < 0.1)
                        VS_Actual.RR = 4;
                    else {
                        c = II.Math.RandomDouble (0.8d, 1.2d);
                        VS_Actual.RR = (int)(c * VS_Settings.RR);
                        VS_Actual.RR_IE_E = (int)(c * VS_Settings.RR_IE_E);
                    }
                    break;

                case Respiratory_Rhythms.Values.Biot:
                    if (counterRespiratory_Arrhythmia < 0) {
                        VS_Actual.RR = 3;                               // Period of apnea, 20 sec
                        counterRespiratory_Arrhythmia = (int)(VS_Settings.RR * 0.75d);   // Counter for ~45 seconds of regular rate
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

        private async Task OnRespiratory_Inspiration () {
            Respiration_Inflated = true;
            await OnPatientEvent (PatientEventTypes.Respiratory_Inspiration);
            timerRespiratory_Inspiration.Stop ();

            // Process pulsus paradoxus (numerical values) for inspiration here
            // Flip counterCardiac_Paradoxus when flipping SBP portion
            if (Pulsus_Paradoxus && Respiratory_Rhythm.Value != Respiratory_Rhythms.Values.Apnea
                && (Mechanically_Ventilated || switchParadoxus)) {
                switchParadoxus = true;
                VS_Actual.ASBP += Mechanically_Ventilated
                    ? -(int)(VS_Settings.ASBP * 0.15d)
                    : (int)(VS_Settings.ASBP * 0.15d);
                IABP_AP += Mechanically_Ventilated
                    ? -(int)(VS_Settings.ASBP * 0.05d)
                    : (int)(VS_Settings.ASBP * 0.05d);
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
                    timerRespiratory_Expiration.ResetAuto ((int)(GetRR_Seconds_I * 1000d));     // Expiration.Interval marks end inspiration
                    break;
            }
        }

        private async Task OnRespiratory_Expiration () {
            Respiration_Inflated = false;
            await OnPatientEvent (PatientEventTypes.Respiratory_Expiration);
            timerRespiratory_Expiration.Stop ();

            // Process pulsus paradoxus (numerical values) for expiration here
            // Flip counterCardiac_Paradoxus when flipping SBP portion
            if (Pulsus_Paradoxus && Respiratory_Rhythm.Value != Respiratory_Rhythms.Values.Apnea
                && (!Mechanically_Ventilated || switchParadoxus)) {
                switchParadoxus = true;
                VS_Actual.ASBP += Mechanically_Ventilated
                    ? (int)(VS_Settings.ASBP * 0.15d)
                    : -(int)(VS_Settings.ASBP * 0.15d);
                IABP_AP += Mechanically_Ventilated
                    ? (int)(VS_Settings.ASBP * 0.05d)
                    : -(int)(VS_Settings.ASBP * 0.05d);
            }
        }

        private async Task OnObstetric_Baseline () {
            await OnPatientEvent (PatientEventTypes.Obstetric_Baseline);
            timerObstetric_Baseline.ResetAuto (1000);

            await OnObstetric_RunTimers ();
        }

        private async Task OnObstetric_RunTimers () {
            if (Contraction_Frequency <= 0) {
                timerObstetric_Contraction.Stop ();
                Uterus_Contracted = false;
            } else if (Contraction_Frequency > 0) {
                if (!Uterus_Contracted)
                    timerObstetric_Contraction.Continue ((int)(Contraction_Frequency * 1000d));
                else if (Uterus_Contracted)
                    timerObstetric_Contraction.Continue ((int)(Contraction_Duration * 1000d));
            }
        }

        private async Task OnObstetric_Contraction () {
            if (!Uterus_Contracted) {       // Contraction onset
                Uterus_Contracted = true;
                timerObstetric_Contraction.ResetAuto ((int)(Contraction_Duration * 1000d));

                await OnPatientEvent (PatientEventTypes.Obstetric_Contraction_Start);
            } else {                        // Contraction ending
                Uterus_Contracted = false;
                timerObstetric_Contraction.ResetAuto ((int)(Contraction_Frequency * 1000d));

                await OnPatientEvent (PatientEventTypes.Obstetric_Contraction_End);
            }
        }
    }
}