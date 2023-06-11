/* Physiology.cs
 * Infirmary Integrated
 * By Ibi Keller (Tanjera), (c) 2017
 *
 * All patient's physiology modeling takes place in Physiology.cs, consisting of:
 * - Variables: vital signs and modeling parameters
 * - Timers: for modeling cardiac and respiratory rhythms, etc.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace II {

    public class Physiology {
        /* Mirroring variables */
        public DateTime Updated;                    // DateTime this Patient was last updated

        /* Parameters for patient simulation, e.g. vital signs */

        public Vital_Signs VS_Settings = new (),
                            VS_Actual = new ();

        /* Basic vital signs and advanced hemodynamics (partial) */
        public Cardiac_Rhythms Cardiac_Rhythm = new ();
        public Respiratory_Rhythms Respiratory_Rhythm = new ();
        public PulmonaryArtery_Rhythms PulmonaryArtery_Placement = new ();

        /* Respiratory profile (partial) */
        public bool Mechanically_Ventilated = false;
        public bool Respiration_Inflated = false;

        /* Cardiac profile */
        public int Pacemaker_Threshold;            // Patient's threshold for electrical capture to pacemaker spike

        public bool Pulsus_Paradoxus = false,
                    Pulsus_Alternans = false,
                    Electrical_Alternans = false;

        public Cardiac_Axes Cardiac_Axis = new ();
        public double QRS_Interval, QTc_Interval;
        public double []? ST_Elevation, T_Elevation;

        /* Obstetric profile */

        public int ObstetricFetalRateVariability,           // Variability in bpm
                    ObstetricContractionFrequency,          // Frequency in seconds
                    ObstetricContractionDuration;           // Duration in seconds

        public int ObstetricUterineRestingTone,                 // mmHg Tocograph of uterus' resting tone
                    ObstetricContractionIntensity;      // mmHg Tocograph of each contraction

        public FetalHeart_Rhythms ObstetricFetalHeartRhythm = new ();

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

        /* Timers and multipliers for temporal modeling */

        public int TimerObstetric_Multiplier = 1;

        private Timer TimerCardiac_Baseline = new (),
                        TimerCardiac_Atrial_Electric = new (),
                        TimerCardiac_Ventricular_Electric = new (),
                        TimerCardiac_Atrial_Mechanical = new (),
                        TimerCardiac_Ventricular_Mechanical = new (),
                        TimerIABP_Balloon_Trigger = new (),
                        TimerDefibrillation = new (),
                        TimerPacemaker_Baseline = new (),
                        TimerPacemaker_Spike = new (),
                        TimerRespiratory_Baseline = new (),
                        TimerRespiratory_Inspiration = new (),
                        TimerRespiratory_Expiration = new (),
                        TimerObstetric_Baseline = new (),
                        TimerObstetric_Fetal_Baseline = new (),
                        TimerObstetric_Fetal_Variability = new (),
                        TimerObstetric_Contraction = new ();

        private static int Default_Electromechanical_Delay = 180;   // Delay in electrical to mechanical capture in milliseconds

        /* Internal counters, flags, buffers for propogating aberrancies and rhythms*/

        private int counterCardiac_Aberrancy = 0,
                    counterCardiac_Arrhythmia = 0,
                    counterRespiratory_Arrhythmia = 0;

        private bool switchParadoxus = false,
                     switchCardiac_Arrhythmia = false,
                     switchRespiratory_Arrhythmia = false;

        private bool flagObstetricContraction = false;
        private int bufferFetalHeartRhythmWander = 1;
        private FetalHeart_Rhythms.States stateFetalHeartRhythm = FetalHeart_Rhythms.States.Interval;

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

            public int FetalHR;          // Baseline fetal heart rate

            public Vital_Signs () {
            }

            public Vital_Signs (Vital_Signs v)
                => Set (v);

            public Task Set (Vital_Signs v) {
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
                FetalHR = v.FetalHR;

                return Task.CompletedTask;
            }
        }

        public Physiology () {
            Task.Run (async () => await UpdateParameters_Cardiac (
                            // Basic cardiac vital signs
                            80,
                            120, 80, 95,
                            98,
                            38.0d,
                            Cardiac_Rhythms.Values.Sinus_Rhythm,

                            // Advanced hemodynamics
                            6,
                            120, 80, 95,
                            6d,
                            PulmonaryArtery_Rhythms.Values.Pulmonary_Artery,
                            22, 12, 16,
                            8,
                            1,

                            // Cardiac profile
                            50,
                            false, false, false,
                            Cardiac_Axes.Values.Normal,
                            0.08d, 0.4d,
                            new double [] { 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d },
                            new double [] { 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d }
                            ));

            Task.Run (async () => await UpdateParameters_Respiratory (
                            16,
                            Respiratory_Rhythms.Values.Regular,
                            40,
                            false,
                            1d, 2d));

            Task.Run (async () => await UpdateParameters_Obstetric (
                            145,
                            20,
                            FetalHeart_Rhythms.Values.Baseline,
                            120,
                            60,
                            50, 10));

            Task.Run (async () => await InitTimers ());
            Task.Run (async () => await ResetStartTimers ());
        }

        ~Physiology () => Task.Run (async () => await Dispose ());

        public async Task Dispose () {
            await UnsubscribePhysiologyEvent ();

            TimerCardiac_Baseline.Dispose ();
            TimerCardiac_Atrial_Electric.Dispose ();
            TimerCardiac_Ventricular_Electric.Dispose ();
            TimerCardiac_Atrial_Mechanical.Dispose ();
            TimerCardiac_Ventricular_Mechanical.Dispose ();
            TimerIABP_Balloon_Trigger.Dispose ();
            TimerDefibrillation.Dispose ();
            TimerPacemaker_Baseline.Dispose ();
            TimerPacemaker_Spike.Dispose ();
            TimerRespiratory_Baseline.Dispose ();
            TimerRespiratory_Inspiration.Dispose ();
            TimerRespiratory_Expiration.Dispose ();
            TimerObstetric_Baseline.Dispose ();
            TimerObstetric_Fetal_Baseline.Dispose ();
            TimerObstetric_Fetal_Variability.Dispose ();
            TimerObstetric_Contraction.Dispose ();
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

        public int Fetal_HR {
            get { return VS_Actual.FetalHR; }
            set { VS_Settings.FetalHR = value; }
        }

        /* Flagging properties (e.g. Is...?, In...? */

        public bool InLabor {
            get {
                return ObstetricContractionFrequency > 0
                    && ObstetricContractionIntensity > 0
                    && ObstetricContractionIntensity > ObstetricUterineRestingTone;
            }
        }

        /* Methods for counting, calculating, and measuring vital signs, timing re: vital signs, etc. */

        public static int CalculateMAP (int sbp, int dbp) {
            return dbp + ((sbp - dbp) / 3);
        }

        public static int CalculateCPP (int? icp, int? map) {
            if (icp is null || map is null)
                return 0;

            return (int)(map - icp);
        }

        public double GetHR_Seconds { get { return 60d / System.Math.Max (1, VS_Actual.HR); } }
        public double GetRR_Seconds { get { return 60d / System.Math.Max (1, VS_Actual.RR); } }
        public double GetRR_Seconds_I { get { return (GetRR_Seconds / (RR_IE_I + RR_IE_E)) * RR_IE_I; } }
        public double GetRR_Seconds_E { get { return (GetRR_Seconds / (RR_IE_I + RR_IE_E)) * RR_IE_E; } }

        // A functional length in seconds of pulsatile rhythms; based on set heart rate (not actual!)
        // for consistency in irregular rhythms
        public double GetPulsatility_Seconds { get { return 60d / System.Math.Max (1, VS_Settings.HR) * 0.9d; } }

        // Using Fridericia Formula for QT <-> QTc calculation
        public double GetQTInterval { get { return System.Math.Pow ((60d / System.Math.Max (1, VS_Actual.HR)), (1 / 3)) * QTc_Interval; } }

        public double GetSTInterval { get { return GetQTInterval - QRS_Interval; } }
        public double GetSTSegment { get { return GetSTInterval * (1d / 3d); } }
        public double GetTInterval { get { return GetSTInterval * (2d / 3d); } }

        public int MeasureHR_ECG (double lengthSeconds, double offsetSeconds)
            => MeasureHR (lengthSeconds, offsetSeconds, false);

        public int MeasureHR_SPO2 (double lengthSeconds, double offsetSeconds)
            => MeasureHR (lengthSeconds, offsetSeconds, true);

        public int MeasureHR (double lengthSeconds, double offsetSeconds, bool isPulse = false) {
            CleanListPhysiologyEvents ();

            if (isPulse && !Cardiac_Rhythm.HasPulse_Ventricular)
                return 0;

            int counter = 0;

            lock (lockListPhysiologyEvents) {
                foreach (PhysiologyEventArgs ea in ListPhysiologyEvents)
                    if (ea.EventType == PhysiologyEventTypes.Cardiac_Ventricular_Electric
                        && ea.Occurred.CompareTo (DateTime.Now.AddSeconds (-(lengthSeconds + offsetSeconds))) >= 0
                        && ea.Occurred.CompareTo (DateTime.Now.AddSeconds (-offsetSeconds)) <= 0)
                        counter++;
            }

            return (int)(counter / (lengthSeconds / 60d));
        }

        public int MeasureRR (double lengthSeconds, double offsetSeconds) {
            CleanListPhysiologyEvents ();

            int counter = 0;

            lock (lockListPhysiologyEvents) {
                foreach (PhysiologyEventArgs ea in ListPhysiologyEvents)
                    if (ea.EventType == PhysiologyEventTypes.Respiratory_Inspiration
                        && ea.Occurred.CompareTo (DateTime.Now.AddSeconds (-(lengthSeconds + offsetSeconds))) >= 0
                        && ea.Occurred.CompareTo (DateTime.Now.AddSeconds (-offsetSeconds)) <= 0)
                        counter++;
            }

            return (int)(counter / (lengthSeconds / 60));
        }

        public async Task Activate () {
            await ResetStartTimers ();
        }

        public async Task Deactivate () {
            await UnsubscribePhysiologyEvent ();
            await StopTimers ();
        }

        public Task UnsubscribePhysiologyEvent () {
            if (PhysiologyEvent != null) {
                foreach (Delegate d in PhysiologyEvent.GetInvocationList ())
                    PhysiologyEvent -= (EventHandler<PhysiologyEventArgs>)d;
            }

            return Task.CompletedTask;
        }

        /* PatientEvent event, handler, and caller */
        public readonly object lockListPhysiologyEvents = new ();
        public List<PhysiologyEventArgs> ListPhysiologyEvents = new ();

        public event EventHandler<PhysiologyEventArgs>? PhysiologyEvent;

        public class PhysiologyEventArgs : EventArgs {
            public Physiology? Physiology;                         // Remains as a pointer
            public Vital_Signs Vitals;                      // Copies over as a clone, not a pointer
            public PhysiologyEventTypes EventType;
            public DateTime Occurred;

            public PhysiologyEventArgs (Physiology? p, PhysiologyEventTypes e) {
                p ??= new ();

                EventType = e;
                Physiology = p;
                Vitals = new Vital_Signs (p.VS_Actual);
                Occurred = DateTime.Now;
            }
        }

        public enum PhysiologyEventTypes {
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
            Obstetric_Fetal_Baseline,
            Obstetric_Contraction_Start,
            Obstetric_Contraction_End
        }

        public Task OnPhysiologyEvent (PhysiologyEventTypes e) {
            PhysiologyEventArgs ea = new (this, e);

            lock (lockListPhysiologyEvents) {
                ListPhysiologyEvents.Add (ea);
            }

            PhysiologyEvent?.Invoke (this, ea);

            return Task.CompletedTask;
        }

        public void CleanListPhysiologyEvents () {
            // Remove all listings older than 1 minute... prevent cluttering memory
            lock (lockListPhysiologyEvents) {
                for (int i = ListPhysiologyEvents.Count - 1; i >= 0; i--)
                    if (ListPhysiologyEvents [i].Occurred.CompareTo (DateTime.Now.AddMinutes (-1)) < 0)
                        ListPhysiologyEvents.RemoveAt (i);
            }
        }

        /* Process all timers for patient modeling */

        public void ProcessTimers (object? sender, EventArgs e) {
            /* For cross-platform compatibility with different timers ...
             * When creating a Patient object, create a native thread-safe Timer object,
             * short interval, and call this function on its Tick to process all Patient
             * timers.
             */
            TimerCardiac_Baseline.Process ();
            TimerCardiac_Atrial_Electric.Process ();
            TimerCardiac_Ventricular_Electric.Process ();
            TimerCardiac_Atrial_Mechanical.Process ();
            TimerCardiac_Ventricular_Mechanical.Process ();
            TimerIABP_Balloon_Trigger.Process ();
            TimerDefibrillation.Process ();
            TimerPacemaker_Baseline.Process ();
            TimerPacemaker_Spike.Process ();
            TimerRespiratory_Baseline.Process ();
            TimerRespiratory_Inspiration.Process ();
            TimerRespiratory_Expiration.Process ();
            TimerObstetric_Baseline.Process ();
            TimerObstetric_Fetal_Baseline.Process ();
            TimerObstetric_Fetal_Variability.Process ();
            TimerObstetric_Contraction.Process ();
        }

        /* Process for loading Patient{} information from simulation file */

        public async Task Load (string inc) {
            using StringReader sRead = new (inc);

            try {
                string? line;
                while (!String.IsNullOrEmpty (line = sRead.ReadLine ())) {
                    line = line.Trim ();

                    if (line.Contains (':')) {
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

                            // Cardiac profile
                            case "Pacemaker_Threshold": Pacemaker_Threshold = int.Parse (pValue); break;
                            case "Pulsus_Paradoxus": Pulsus_Paradoxus = bool.Parse (pValue); break;
                            case "Pulsus_Alternans": Pulsus_Alternans = bool.Parse (pValue); break;
                            case "Electrical_Alternans": Electrical_Alternans = bool.Parse (pValue); break;
                            case "Cardiac_Axis": Cardiac_Axis.Value = (Cardiac_Axes.Values)Enum.Parse (typeof (Cardiac_Axes.Values), pValue); break;
                            case "QRS_Interval": QRS_Interval = double.Parse (pValue); break;
                            case "QTc_Interval": QTc_Interval = double.Parse (pValue); break;

                            case "ST_Elevation":
                                string [] e_st = pValue.Split (',').Where ((o) => o != "").ToArray ();
                                for (int i = 0; i < e_st.Length && i < ST_Elevation?.Length; i++)
                                    ST_Elevation [i] = double.Parse (e_st [i]);
                                break;

                            case "T_Elevation":
                                string [] e_t = pValue.Split (',').Where ((o) => o != "").ToArray ();
                                for (int i = 0; i < e_t.Length && i < T_Elevation?.Length; i++)
                                    T_Elevation [i] = double.Parse (e_t [i]);
                                break;

                            // Obstetric profile
                            case "ObstetricFetalHeartRate": VS_Settings.FetalHR = int.Parse (pValue); break;
                            case "ObstetricFetalRateVariability": ObstetricFetalRateVariability = int.Parse (pValue); break;
                            case "ObstetricFetalHeartRhythm": ObstetricFetalHeartRhythm.Value = (FetalHeart_Rhythms.Values)Enum.Parse (typeof (FetalHeart_Rhythms.Values), pValue); break;
                            case "ObstetricContractionFrequency": ObstetricContractionFrequency = int.Parse (pValue); break;
                            case "ObstetricContractionDuration": ObstetricContractionDuration = int.Parse (pValue); break;
                            case "ObstetricContractionIntensity": ObstetricContractionIntensity = int.Parse (pValue); break;
                            case "ObstetricUterineRestingTone": ObstetricUterineRestingTone = int.Parse (pValue); break;

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

            await ResetStartTimers ();
            await OnCardiac_Baseline ();
            await OnRespiratory_Baseline ();

            await OnPhysiologyEvent (PhysiologyEventTypes.Vitals_Change);
        }

        /* Process for saving Patient{} information to simulation file  */

        public string Save (int indent = 1) {
            string dent = Utility.Indent (indent);
            StringBuilder sWrite = new ();

            // File/scenario information
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Updated", Utility.DateTime_ToString (Updated)));

            // Basic vital signs
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "HR", VS_Settings.HR));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "NSBP", VS_Settings.NSBP));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "NDBP", VS_Settings.NDBP));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "NMAP", VS_Settings.NMAP));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "RR", VS_Settings.RR));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "SPO2", VS_Settings.SPO2));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "T", VS_Settings.T));

            // Rhythms
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Cardiac_Rhythm", Cardiac_Rhythm.Value));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Respiratory_Rhythm", Respiratory_Rhythm.Value));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "PulmonaryArtery_Rhythm", PulmonaryArtery_Placement.Value));

            // Advanced hemodynamics
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "ETCO2", VS_Settings.ETCO2));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "CVP", VS_Settings.CVP));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "ASBP", VS_Settings.ASBP));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "ADBP", VS_Settings.ADBP));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "AMAP", VS_Settings.AMAP));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "CO", VS_Settings.CO));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "PSP", VS_Settings.PSP));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "PDP", VS_Settings.PDP));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "PMP", VS_Settings.PMP));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "ICP", VS_Settings.ICP));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "IAP", VS_Settings.IAP));

            // Respiratory profile
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Mechanically_Ventilated", Mechanically_Ventilated));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Respiratory_IERatio_I", VS_Settings.RR_IE_I));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Respiratory_IERatio_E", VS_Settings.RR_IE_E));

            // Cardiac profile
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Pacemaker_Threshold", Pacemaker_Threshold));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Pulsus_Paradoxus", Pulsus_Paradoxus));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Pulsus_Alternans", Pulsus_Alternans));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Electrical_Alternans", Electrical_Alternans));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Cardiac_Axis", Cardiac_Axis.Value));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "QRS_Interval", QRS_Interval));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "QTc_Interval", QTc_Interval));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "ST_Elevation", string.Join (",", ST_Elevation ?? new double [12])));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "T_Elevation", string.Join (",", T_Elevation ?? new double [12])));

            // Obstetric profile
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "ObstetricFetalHeartRate", VS_Settings.FetalHR));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "ObstetricFetalRateVariability", ObstetricFetalRateVariability));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "ObstetricFetalHeartRhythm", ObstetricFetalHeartRhythm.Value));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "ObstetricContractionFrequency", ObstetricContractionFrequency));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "ObstetricContractionDuration", ObstetricContractionDuration));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "ObstetricContractionIntensity", ObstetricContractionIntensity));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "ObstetricUterineRestingTone", ObstetricUterineRestingTone));

            // Device settings
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "TransducerZeroed_ABP", TransducerZeroed_ABP));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "TransducerZeroed_CVP", TransducerZeroed_CVP));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "TransducerZeroed_PA", TransducerZeroed_PA));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "TransducerZeroed_ICP", TransducerZeroed_ICP));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "TransducerZeroed_IAP", TransducerZeroed_IAP));

            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Pacemaker_Rate", Pacemaker_Rate));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Pacemaker_Energy", Pacemaker_Energy));

            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "IABP_AP", IABP_AP));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "IABP_DBP", IABP_DBP));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "IABP_MAP", IABP_MAP));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "IABP_Active", IABP_Active));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "IABP_Trigger", IABP_Trigger));

            return sWrite.ToString ();
        }

        public async Task UpdateParameters_Cardiac (
                    // Basic vital signs
                    int hr,
                    int nsbp, int ndbp, int nmap,
                    int spo2,
                    double t,
                    Cardiac_Rhythms.Values card_rhythm,

                    // Advanced hemodynamics
                    int cvp,
                    int asbp, int adbp, int amap,
                    double co,
                    PulmonaryArtery_Rhythms.Values pa_placement,
                    int psp, int pdp, int pmp,
                    int icp, int iap,

                    // Cardiac profile
                    int pacer_threshold,
                    bool puls_paradoxus, bool puls_alternans, bool elec_alternans,
                    Cardiac_Axes.Values card_axis,
                    double qrs_int, double qtc_int,
                    double []? st_elev, double []? t_elev) {
            Updated = DateTime.UtcNow;

            // Basic vital signs
            VS_Settings.HR = hr;
            VS_Settings.NSBP = nsbp;
            VS_Settings.NDBP = ndbp;
            VS_Settings.NMAP = nmap;
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

            Cardiac_Rhythm.Value = card_rhythm;

            // Advanced hemodynamics
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

            // Cardiac profile
            Pacemaker_Threshold = pacer_threshold;

            // Reset buffers and switches for pulsus paradoxus
            switchParadoxus = false;
            Pulsus_Paradoxus = puls_paradoxus;
            Pulsus_Alternans = puls_alternans;
            Electrical_Alternans = elec_alternans;

            QRS_Interval = qrs_int;
            QTc_Interval = qtc_int;

            Cardiac_Axis.Value = card_axis;
            ST_Elevation = st_elev;
            T_Elevation = t_elev;

            // Reset actual vital signs to set parameters
            await VS_Actual.Set (VS_Settings);

            await OnCardiac_Baseline ();
            await OnPhysiologyEvent (PhysiologyEventTypes.Vitals_Change);
        }

        public async Task UpdateParameters_Respiratory (
                    int rr,
                    Respiratory_Rhythms.Values resp_rhythm,
                    int etco2,
                    bool mech_vent,
                    double resp_ier_i, double resp_ier_e) {
            Updated = DateTime.UtcNow;

            VS_Settings.RR = rr;

            // Change in cardiac or respiratory rhythm? Reset all buffer counters and switches

            if (Respiratory_Rhythm.Value != resp_rhythm) {
                Respiration_Inflated = false;
                counterRespiratory_Arrhythmia = 0;
                switchRespiratory_Arrhythmia = false;
            }

            Respiratory_Rhythm.Value = resp_rhythm;

            // Advanced hemodynamics
            VS_Settings.ETCO2 = etco2;

            // Respiratory profile
            Mechanically_Ventilated = mech_vent;
            VS_Settings.RR_IE_I = resp_ier_i;
            VS_Settings.RR_IE_E = resp_ier_e;

            // Reset actual vital signs to set parameters
            await VS_Actual.Set (VS_Settings);

            await OnRespiratory_Baseline ();
            await OnPhysiologyEvent (PhysiologyEventTypes.Vitals_Change);
        }

        public async Task UpdateParameters_Obstetric (
                    int fhr, int fhr_var, FetalHeart_Rhythms.Values fhr_rhythm,
                    int uc_freq, int uc_duration, int uc_intensity, int uc_resting) {
            Updated = DateTime.UtcNow;

            // Obstetric profile
            VS_Settings.FetalHR = fhr;
            ObstetricFetalRateVariability = fhr_var;
            ObstetricFetalHeartRhythm.Value = fhr_rhythm;
            ObstetricContractionFrequency = uc_freq;
            ObstetricContractionDuration = uc_duration;
            ObstetricContractionIntensity = uc_intensity;
            ObstetricUterineRestingTone = uc_resting;

            // Reset actual vital signs to set parameters
            await VS_Actual.Set (VS_Settings);

            await OnObstetric_Baseline ();
            await OnPhysiologyEvent (PhysiologyEventTypes.Vitals_Change);
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
            VS_Settings.NMAP = Physiology.CalculateMAP (VS_Settings.NSBP, VS_Settings.NDBP);
            VS_Settings.ASBP = II.Math.Clamp (VS_Settings.ASBP, sbpMin, sbpMax);
            VS_Settings.ADBP = II.Math.Clamp (VS_Settings.ADBP, sbpMin, sbpMax);
            VS_Settings.AMAP = Physiology.CalculateMAP (VS_Settings.ASBP, VS_Settings.ADBP);
            VS_Settings.PSP = II.Math.Clamp (VS_Settings.PSP, pspMin, pspMax);
            VS_Settings.PDP = II.Math.Clamp (VS_Settings.PDP, pdpMin, pdpMax);
            VS_Settings.PMP = Physiology.CalculateMAP (VS_Settings.PSP, VS_Settings.PDP);

            await VS_Actual.Set (VS_Settings);

            switchParadoxus = false;

            await ResetStartTimers ();
            await OnCardiac_Baseline ();
            await OnRespiratory_Baseline ();
            await OnObstetric_Baseline ();

            await OnPhysiologyEvent (PhysiologyEventTypes.Vitals_Change);
        }

        private Task InitTimers () {
            TimerCardiac_Baseline.Tick += async delegate { await OnCardiac_Baseline (); };
            TimerCardiac_Atrial_Electric.Tick += async delegate { await OnCardiac_Atrial_Electric (); };
            TimerCardiac_Ventricular_Electric.Tick += async delegate { await OnCardiac_Ventricular_Electric (); };
            TimerCardiac_Atrial_Mechanical.Tick += async delegate { await OnCardiac_Atrial_Mechanical (); };
            TimerCardiac_Ventricular_Mechanical.Tick += async delegate { await OnCardiac_Ventricular_Mechanical (); };
            TimerIABP_Balloon_Trigger.Tick += async delegate { await OnIABP_Balloon_Inflate (); };
            TimerDefibrillation.Tick += async delegate { await OnDefibrillation_End (); };
            TimerPacemaker_Baseline.Tick += async delegate { await OnPacemaker_Baseline (); };
            TimerPacemaker_Spike.Tick += async delegate { await OnPacemaker_Spike (); };

            TimerRespiratory_Baseline.Tick += async delegate { await OnRespiratory_Baseline (); };
            TimerRespiratory_Inspiration.Tick += async delegate { await OnRespiratory_Inspiration (); };
            TimerRespiratory_Expiration.Tick += async delegate { await OnRespiratory_Expiration (); };

            TimerObstetric_Baseline.Tick += async delegate { await OnObstetric_Baseline (); };
            TimerObstetric_Fetal_Baseline.Tick += async delegate { await OnObstetric_Fetal_Baseline (); };
            TimerObstetric_Fetal_Variability.Tick += async delegate { await OnObstetric_Fetal_Variability (); };
            TimerObstetric_Contraction.Tick += async delegate { await OnObstetric_Contraction (); };

            return Task.CompletedTask;
        }

        private async Task ResetStartTimers () {
            await ResetStartTimers_Cardiac ();
            await ResetStartTimers_Respiratory ();
            await ResetStartTimers_Obstetric ();
        }

        private async Task ResetStartTimers_Cardiac () {
            await TimerCardiac_Baseline.ResetStart ((int)(GetHR_Seconds * 1000d));
            await TimerCardiac_Atrial_Electric.Stop ();
            await TimerCardiac_Ventricular_Electric.Stop ();
            await TimerCardiac_Atrial_Mechanical.Stop ();
            await TimerCardiac_Ventricular_Mechanical.Stop ();
            await TimerIABP_Balloon_Trigger.Stop ();

            await TimerDefibrillation.ResetStart ();
            if (TimerPacemaker_Baseline.IsRunning)
                await TimerPacemaker_Baseline.ResetStart ();
            else
                await TimerPacemaker_Baseline.Reset ();
            await TimerPacemaker_Spike.Stop ();
        }

        private async Task ResetStartTimers_Respiratory () {
            await TimerRespiratory_Baseline.ResetStart ((int)(GetRR_Seconds * 1000d));
            await TimerRespiratory_Inspiration.Stop ();
            await TimerRespiratory_Expiration.Stop ();
        }

        private async Task ResetStartTimers_Obstetric () {
            await TimerObstetric_Baseline.ResetStart (1);
            await TimerObstetric_Fetal_Baseline.ResetStart (1);

            await TimerObstetric_Fetal_Variability.Stop ();
            await TimerObstetric_Contraction.Stop ();
        }

        private async Task StopTimers () {
            await StopTimers_Cardiac ();
            await StopTimers_Respiratory ();
            await StopTimers_Obstetric ();
        }

        private async Task StopTimers_Cardiac () {
            await TimerCardiac_Baseline.Stop ();
            await TimerCardiac_Atrial_Electric.Stop ();
            await TimerCardiac_Ventricular_Electric.Stop ();
            await TimerCardiac_Atrial_Mechanical.Stop ();
            await TimerCardiac_Ventricular_Mechanical.Stop ();
            await TimerIABP_Balloon_Trigger.Stop ();
            await TimerDefibrillation.Stop ();
            await TimerPacemaker_Baseline.Stop ();
            await TimerPacemaker_Spike.Stop ();
        }

        private async Task StopTimers_Respiratory () {
            await TimerRespiratory_Baseline.Stop ();
            await TimerRespiratory_Inspiration.Stop ();
            await TimerRespiratory_Expiration.Stop ();
        }

        private async Task StopTimers_Obstetric () {
            await TimerObstetric_Baseline.Stop ();
            await TimerObstetric_Fetal_Baseline.Stop ();
            await TimerObstetric_Fetal_Variability.Stop ();
            await TimerObstetric_Contraction.Stop ();
        }

        public async Task SetTimerMultiplier_Obstetric (int multiplier) {
            TimerObstetric_Multiplier = multiplier;
            await OnObstetric_RunTimers ();
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

        public async Task PacemakerPause ()
            => await TimerPacemaker_Baseline.Set (4000);

        private async Task InitDefibrillation (bool toSynchronize) {
            if (toSynchronize)
                TimerCardiac_Ventricular_Electric.Tick += OnCardioversion;
            else
                await OnDefibrillation ();
        }

        public bool Pacemaker_HasCapture {
            get {
                return TimerPacemaker_Baseline.IsRunning
                    && Pacemaker_Energy > 0 && Pacemaker_Rate > 0
                    && Pacemaker_Energy >= Pacemaker_Threshold;
            }
        }

        private async Task StartPacemaker ()
            => await TimerPacemaker_Baseline.ResetStart ((int)((60d / Pacemaker_Rate) * 1000));

        private async Task StopPacemaker () {
            await TimerPacemaker_Baseline.Stop ();
            await TimerPacemaker_Spike.Stop ();
        }

        private async Task OnDefibrillation () {
            await TimerCardiac_Baseline.Stop ();
            await TimerCardiac_Atrial_Electric.Stop ();
            await TimerCardiac_Ventricular_Electric.Stop ();
            await TimerCardiac_Atrial_Mechanical.Stop ();
            await TimerCardiac_Ventricular_Mechanical.Stop ();
            await TimerDefibrillation.ResetStart (20);

            // Invoke the defibrillation event *after* starting the timer- IsDefibrillating() checks the timer!
            await OnPhysiologyEvent (PhysiologyEventTypes.Defibrillation);
        }

        private async Task OnDefibrillation_End () {
            await TimerDefibrillation.Stop ();
            await TimerCardiac_Baseline.ResetStart ();
        }

        private void OnCardioversion (object? sender, EventArgs e) {
            TimerCardiac_Ventricular_Electric.Tick -= OnCardioversion;
            Task.Run (async () => await OnDefibrillation ());
        }

        private async Task OnPacemaker_Baseline () {
            if (Pacemaker_Energy > 0)
                await OnPhysiologyEvent (PhysiologyEventTypes.Pacermaker_Spike);

            if (Pacemaker_Energy >= Pacemaker_Threshold)
                await TimerPacemaker_Spike.ResetStart (40);          // Adds an interval between the spike and the QRS complex

            await StartPacemaker ();                                // In case pacemaker was paused... updates .Interval
        }

        private async Task OnPacemaker_Spike () {
            await TimerPacemaker_Spike.Stop ();

            // Trigger the QRS complex, then reset the heart's intrinsic timers
            if (Cardiac_Rhythm.CanBe_Paced) {
                Cardiac_Rhythm.AberrantBeat = true;
                await OnCardiac_Ventricular_Electric ();
                Cardiac_Rhythm.AberrantBeat = false;

                await OnPhysiologyEvent (PhysiologyEventTypes.Cardiac_Baseline);  // Triggers drawing isoelectric lines (important for a-fib/flutter)
                await TimerCardiac_Baseline.ResetStart ();                   // Resets heart's intrinsic timer, allows pacemaker overdrive modeling
            }
        }

        private async Task OnCardiac_Baseline () {
            await OnPhysiologyEvent (PhysiologyEventTypes.Cardiac_Baseline);
            await TimerCardiac_Baseline.Set ((int)(GetHR_Seconds * 1000d));

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
                    await TimerCardiac_Ventricular_Electric.ResetStart (1);
                    break;

                // Traced as "regular A" or "regular A -> V" Rhythms
                case Cardiac_Rhythms.Values.AV_Block__1st_Degree:
                case Cardiac_Rhythms.Values.AV_Block__Mobitz_II:
                case Cardiac_Rhythms.Values.AV_Block__Wenckebach:
                case Cardiac_Rhythms.Values.Bundle_Branch_Block:
                case Cardiac_Rhythms.Values.Sinus_Rhythm:
                case Cardiac_Rhythms.Values.Pulseless_Electrical_Activity:
                case Cardiac_Rhythms.Values.Ventricular_Standstill:
                    await TimerCardiac_Atrial_Electric.ResetStart (1);
                    break;

                // Traced as "irregular V" rhythms
                case Cardiac_Rhythms.Values.Atrial_Fibrillation:
                    VS_Actual.HR = (int)(VS_Settings.HR * II.Math.RandomDbl (0.6, 1.4));
                    await TimerCardiac_Ventricular_Electric.ResetStart (1);
                    break;

                /* Special Cases */
                case Cardiac_Rhythms.Values.AV_Block__3rd_Degree:
                    if (!TimerCardiac_Atrial_Electric.IsRunning)
                        await TimerCardiac_Atrial_Electric.ResetStart ((int)(TimerCardiac_Baseline.Interval * 0.6d));
                    await TimerCardiac_Ventricular_Electric.ResetStart (160);
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

                    await TimerCardiac_Atrial_Electric.ResetStart (1);
                    break;

                case Cardiac_Rhythms.Values.Sinus_Arrhythmia:
                    if (Respiration_Inflated)
                        VS_Actual.HR = (int)(VS_Settings.HR * 1.075d);
                    else
                        VS_Actual.HR = (int)(VS_Settings.HR * 0.925d);

                    await TimerCardiac_Atrial_Electric.ResetStart (1);
                    break;

                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_Arrest:

                    // Every 10-25 beats, sinus arrest
                    if (counterCardiac_Arrhythmia <= 0) {
                        Random r = new ();
                        counterCardiac_Arrhythmia = r.Next (10, 16);
                        VS_Actual.HR = VS_Settings.HR / 8;
                    } else {
                        VS_Actual.HR = VS_Settings.HR;
                        counterCardiac_Arrhythmia--;
                    }

                    await TimerCardiac_Atrial_Electric.ResetStart (1);
                    break;

                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_PACs:
                    counterCardiac_Aberrancy -= 1;
                    VS_Actual.HR = VS_Settings.HR;
                    if (counterCardiac_Aberrancy <= 0) {
                        counterCardiac_Aberrancy = new Random ().Next (4, 8);
                        VS_Actual.HR = (int)(VS_Settings.HR * II.Math.RandomDbl (0.6d, 0.8d));
                    } else {
                        VS_Actual.HR = VS_Settings.HR;
                    }
                    await TimerCardiac_Atrial_Electric.ResetStart (1);
                    break;

                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_PJCs:
                    counterCardiac_Aberrancy -= 1;
                    VS_Actual.HR = VS_Settings.HR;
                    if (counterCardiac_Aberrancy <= 0) {
                        counterCardiac_Aberrancy = new Random ().Next (4, 8);
                        await TimerCardiac_Ventricular_Electric.ResetStart (1);
                    } else {
                        if (counterCardiac_Aberrancy == 1)
                            VS_Actual.HR = (int)(VS_Settings.HR * II.Math.RandomDbl (0.7d, 0.9d));
                        await TimerCardiac_Atrial_Electric.ResetStart (1);
                    }
                    break;

                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_Bigeminy:
                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_Trigeminy:
                    counterCardiac_Aberrancy -= 1;
                    if (counterCardiac_Aberrancy == 0) {
                        await TimerCardiac_Baseline.Set ((int)(TimerCardiac_Baseline.Interval * 0.8d));
                    } else if (counterCardiac_Aberrancy < 0) {   // Then throw the PVC and reset the counters
                        if (Cardiac_Rhythm.Value == Cardiac_Rhythms.Values.Sinus_Rhythm_with_Bigeminy)
                            counterCardiac_Aberrancy = 1;
                        else if (Cardiac_Rhythm.Value == Cardiac_Rhythms.Values.Sinus_Rhythm_with_Trigeminy)
                            counterCardiac_Aberrancy = 2;
                        Cardiac_Rhythm.AberrantBeat = true;
                        await TimerCardiac_Ventricular_Electric.ResetStart (1);
                        break;
                    }
                    await TimerCardiac_Atrial_Electric.ResetStart (1);
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
                        await TimerCardiac_Ventricular_Electric.ResetStart (1);
                        break;
                    }
                    Cardiac_Rhythm.AberrantBeat = false;
                    await TimerCardiac_Atrial_Electric.ResetStart (1);
                    break;
            }
        }

        private async Task OnCardiac_Atrial_Electric () {
            await OnPhysiologyEvent (PhysiologyEventTypes.Cardiac_Atrial_Electric);

            if (Cardiac_Rhythm.HasPulse_Atrial)
                await TimerCardiac_Atrial_Mechanical.ResetStart (Default_Electromechanical_Delay);

            switch (Cardiac_Rhythm.Value) {
                default:
                case Cardiac_Rhythms.Values.Asystole:
                    break;

                // Regular A Rhythms
                case Cardiac_Rhythms.Values.Ventricular_Standstill:
                    await TimerCardiac_Atrial_Electric.Stop ();
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
                    await TimerCardiac_Atrial_Electric.Stop ();
                    await TimerCardiac_Ventricular_Electric.ResetStart (160);
                    break;

                /* Special cases */

                case Cardiac_Rhythms.Values.AV_Block__1st_Degree:
                    await TimerCardiac_Atrial_Electric.Stop ();
                    await TimerCardiac_Ventricular_Electric.ResetStart (240);
                    break;

                case Cardiac_Rhythms.Values.AV_Block__Mobitz_II:
                    await TimerCardiac_Atrial_Electric.Stop ();
                    counterCardiac_Aberrancy -= 1;
                    if (counterCardiac_Aberrancy < 0) {
                        counterCardiac_Aberrancy = 2;
                        Cardiac_Rhythm.AberrantBeat = true;
                    } else {
                        await TimerCardiac_Ventricular_Electric.ResetStart (160);
                        Cardiac_Rhythm.AberrantBeat = false;
                    }
                    break;

                case Cardiac_Rhythms.Values.AV_Block__Wenckebach:
                    await TimerCardiac_Atrial_Electric.Stop ();
                    counterCardiac_Aberrancy -= 1;
                    if (counterCardiac_Aberrancy < 0) {
                        counterCardiac_Aberrancy = 3;
                        Cardiac_Rhythm.AberrantBeat = true;
                    } else {
                        await TimerCardiac_Baseline.Set ((int)(TimerCardiac_Baseline.Interval + (160d * (3d - counterCardiac_Aberrancy))));
                        await TimerCardiac_Ventricular_Electric.ResetStart ((int)(160d * (3d - counterCardiac_Aberrancy)));
                        Cardiac_Rhythm.AberrantBeat = false;
                    }
                    break;

                case Cardiac_Rhythms.Values.AV_Block__3rd_Degree:
                    /* Specifically let atrial timer continue to run and propogate P-waves! */
                    break;
            }
        }

        private async Task OnCardiac_Ventricular_Electric () {
            await OnPhysiologyEvent (PhysiologyEventTypes.Cardiac_Ventricular_Electric);

            if (Cardiac_Rhythm.HasPulse_Ventricular)
                await TimerCardiac_Ventricular_Mechanical.ResetStart (Default_Electromechanical_Delay);

            /* Flip the switch on pulsus alternans or electrical alternans */
            Cardiac_Rhythm.AlternansBeat = (Pulsus_Alternans || Electrical_Alternans) && !Cardiac_Rhythm.AlternansBeat;

            await TimerCardiac_Ventricular_Electric.Stop ();
        }

        private async Task OnCardiac_Atrial_Mechanical () {
            await OnPhysiologyEvent (PhysiologyEventTypes.Cardiac_Atrial_Mechanical);

            await TimerCardiac_Atrial_Mechanical.Stop ();
        }

        private async Task OnCardiac_Ventricular_Mechanical () {
            await OnPhysiologyEvent (PhysiologyEventTypes.Cardiac_Ventricular_Mechanical);

            if (IABP_Active)
                await TimerIABP_Balloon_Trigger.ResetStart ((int)(GetHR_Seconds * 1000d * 0.35d));

            if (Pulsus_Alternans) {
                VS_Actual.ASBP += Cardiac_Rhythm.AlternansBeat ? (int)(VS_Settings.ASBP * 0.15d) : -(int)(VS_Settings.ASBP * 0.15d);
                VS_Actual.ADBP += Cardiac_Rhythm.AlternansBeat ? (int)(VS_Settings.ADBP * 0.15d) : -(int)(VS_Settings.ADBP * 0.15d);
                VS_Actual.AMAP += Cardiac_Rhythm.AlternansBeat ? (int)(VS_Settings.AMAP * 0.15d) : -(int)(VS_Settings.AMAP * 0.15d);
                IABP_AP += Cardiac_Rhythm.AlternansBeat ? -(int)(VS_Settings.ASBP * 0.05d) : (int)(VS_Settings.ASBP * 0.05d);

                VS_Actual.CVP += Cardiac_Rhythm.AlternansBeat ? -(int)(VS_Settings.CVP * 0.15d) : (int)(VS_Settings.CVP * 0.15d);
                VS_Actual.PSP += Cardiac_Rhythm.AlternansBeat ? (int)(VS_Settings.PSP * 0.15d) : -(int)(VS_Settings.PSP * 0.15d);
                VS_Actual.PDP += Cardiac_Rhythm.AlternansBeat ? (int)(VS_Settings.PDP * 0.15d) : -(int)(VS_Settings.PDP * 0.15d);
                VS_Actual.PMP += Cardiac_Rhythm.AlternansBeat ? (int)(VS_Settings.PMP * 0.15d) : -(int)(VS_Settings.PMP * 0.15d);
            }

            await TimerCardiac_Ventricular_Mechanical.Stop ();
        }

        private async Task OnIABP_Balloon_Inflate () {
            await OnPhysiologyEvent (PhysiologyEventTypes.IABP_Balloon_Inflation);

            await TimerIABP_Balloon_Trigger.Stop ();
        }

        private async Task OnRespiratory_Baseline () {
            await OnPhysiologyEvent (PhysiologyEventTypes.Respiratory_Baseline);
            await TimerRespiratory_Baseline.Set ((int)(GetRR_Seconds * 1000d));

            double c;

            switch (Respiratory_Rhythm.Value) {
                default:
                case Respiratory_Rhythms.Values.Apnea:
                    return;

                case Respiratory_Rhythms.Values.Agonal:
                    c = II.Math.RandomDbl (0.8d, 1.2d);
                    VS_Actual.RR = (int)(c * VS_Settings.RR);
                    break;

                case Respiratory_Rhythms.Values.Apneustic:
                    VS_Actual.RR = (II.Math.RandomDbl (0, 1d) < 0.1d) ? 6 : VS_Settings.RR;
                    break;

                case Respiratory_Rhythms.Values.Ataxic:
                    if (II.Math.RandomDbl (0, 1) < 0.1)
                        VS_Actual.RR = 4;
                    else {
                        c = II.Math.RandomDbl (0.8d, 1.2d);
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

            await TimerRespiratory_Inspiration.ResetStart (1);
        }

        private async Task OnRespiratory_Inspiration () {
            Respiration_Inflated = true;
            await OnPhysiologyEvent (PhysiologyEventTypes.Respiratory_Inspiration);
            await TimerRespiratory_Inspiration.Stop ();

            // Process pulsus paradoxus (numerical values) for inspiration here
            // Flip counterCardiac_Paradoxus when flipping SBP portion
            if (Pulsus_Paradoxus && Respiratory_Rhythm.Value != Respiratory_Rhythms.Values.Apnea
                && (Mechanically_Ventilated || switchParadoxus)) {
                switchParadoxus = true;
                VS_Actual.ASBP += Mechanically_Ventilated ? -(int)(VS_Settings.ASBP * 0.15d) : (int)(VS_Settings.ASBP * 0.15d);
                VS_Actual.ADBP += Mechanically_Ventilated ? -(int)(VS_Settings.ADBP * 0.15d) : (int)(VS_Settings.ADBP * 0.15d);
                VS_Actual.AMAP += Mechanically_Ventilated ? -(int)(VS_Settings.AMAP * 0.15d) : (int)(VS_Settings.AMAP * 0.15d);
                IABP_AP += Mechanically_Ventilated ? -(int)(VS_Settings.ASBP * 0.05d) : (int)(VS_Settings.ASBP * 0.05d);
            }

            // Process pressure responses to increased intrathoracic pressure
            VS_Actual.CVP += Mechanically_Ventilated ? (int)(VS_Settings.CVP * 0.1d) : -(int)(VS_Settings.CVP * 0.1d);
            VS_Actual.PSP += Mechanically_Ventilated ? -(int)(VS_Settings.PSP * 0.1d) : (int)(VS_Settings.PSP * 0.1d);
            VS_Actual.PDP += Mechanically_Ventilated ? -(int)(VS_Settings.PDP * 0.1d) : (int)(VS_Settings.PDP * 0.1d);
            VS_Actual.PMP += Mechanically_Ventilated ? -(int)(VS_Settings.PMP * 0.1d) : (int)(VS_Settings.PMP * 0.1d);

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
                    await TimerRespiratory_Expiration.ResetStart ((int)(GetRR_Seconds_I * 1000d));     // Expiration.Interval marks end inspiration
                    break;
            }
        }

        private async Task OnRespiratory_Expiration () {
            Respiration_Inflated = false;
            await OnPhysiologyEvent (PhysiologyEventTypes.Respiratory_Expiration);
            await TimerRespiratory_Expiration.Stop ();

            // Process pulsus paradoxus (numerical values) for expiration here
            // Flip counterCardiac_Paradoxus when flipping SBP portion
            if (Pulsus_Paradoxus && Respiratory_Rhythm.Value != Respiratory_Rhythms.Values.Apnea
                && (!Mechanically_Ventilated || switchParadoxus)) {
                switchParadoxus = true;
                VS_Actual.ASBP += Mechanically_Ventilated ? (int)(VS_Settings.ASBP * 0.15d) : -(int)(VS_Settings.ASBP * 0.15d);
                VS_Actual.ADBP += Mechanically_Ventilated ? (int)(VS_Settings.ADBP * 0.15d) : -(int)(VS_Settings.ADBP * 0.15d);
                VS_Actual.AMAP += Mechanically_Ventilated ? (int)(VS_Settings.AMAP * 0.15d) : -(int)(VS_Settings.AMAP * 0.15d);
                IABP_AP += Mechanically_Ventilated ? (int)(VS_Settings.ASBP * 0.05d) : -(int)(VS_Settings.ASBP * 0.05d);
            }

            // Process pressure responses to increased intrathoracic pressure
            VS_Actual.CVP += Mechanically_Ventilated ? -(int)(VS_Settings.CVP * 0.1d) : (int)(VS_Settings.CVP * 0.1d);
            VS_Actual.PSP += Mechanically_Ventilated ? (int)(VS_Settings.PSP * 0.1d) : -(int)(VS_Settings.PSP * 0.1d);
            VS_Actual.PDP += Mechanically_Ventilated ? (int)(VS_Settings.PDP * 0.1d) : -(int)(VS_Settings.PDP * 0.1d);
            VS_Actual.PMP += Mechanically_Ventilated ? (int)(VS_Settings.PMP * 0.1d) : -(int)(VS_Settings.PMP * 0.1d);
        }

        private async Task OnObstetric_Baseline () {
            await OnPhysiologyEvent (PhysiologyEventTypes.Obstetric_Baseline);
            await TimerObstetric_Baseline.ResetStart (1000);

            await OnObstetric_RunTimers ();
        }

        private async Task OnObstetric_Fetal_Baseline () {
            await OnPhysiologyEvent (PhysiologyEventTypes.Obstetric_Fetal_Baseline);

            // Must be divided by the TimerObstetric_Multiplier or else the speed multiplier effects the actual waveform drawing
            await TimerObstetric_Fetal_Baseline.ResetStart (2500 / TimerObstetric_Multiplier);

            /* Baseline adjusting/walking of the vitals/waveforms *****
             */
            int fvar = Math.RandomInt (1, ObstetricFetalRateVariability / 2);

            switch (stateFetalHeartRhythm) {
                default: break;

                case FetalHeart_Rhythms.States.Interval:
                case FetalHeart_Rhythms.States.Refractory:
                    // Add baseline variability to the fetal heart rate, based on the variability parameter, using a random "walk"
                    // If the fetal heart rate "walks" out of its set min/max for variability, bounce it towarsd "center"
                    if (VS_Actual.FetalHR > VS_Settings.FetalHR + (ObstetricFetalRateVariability / 2))
                        VS_Actual.FetalHR = System.Math.Max (0, VS_Actual.FetalHR - fvar);
                    else if (VS_Actual.FetalHR < VS_Settings.FetalHR - (ObstetricFetalRateVariability / 2))
                        VS_Actual.FetalHR = System.Math.Max (0, VS_Actual.FetalHR + fvar);
                    else
                        VS_Actual.FetalHR = System.Math.Max (0, VS_Actual.FetalHR + (int)Math.RandomDbl (-fvar, fvar));
                    break;

                case FetalHeart_Rhythms.States.Accelerating:
                    // Walk the heart rate upwards towards a normal acceleration of >15 bpm increase
                    if (VS_Actual.FetalHR < VS_Settings.FetalHR + Math.RandomInt (15, 30))
                        VS_Actual.FetalHR = System.Math.Max (0, VS_Actual.FetalHR + bufferFetalHeartRhythmWander);
                    else  // While at "goal", can walk randomly
                        VS_Actual.FetalHR = System.Math.Max (0, VS_Actual.FetalHR + (int)Math.RandomDbl (-fvar, fvar));

                    break;

                case FetalHeart_Rhythms.States.Decelerating:
                    // Walk the heart rate downwards towards a normal deceleration of >15 bpm increase
                    if (VS_Actual.FetalHR > System.Math.Max (0, VS_Settings.FetalHR - Math.RandomInt (15, 30)))
                        VS_Actual.FetalHR = System.Math.Max (0, VS_Actual.FetalHR - bufferFetalHeartRhythmWander);
                    else  // While at "goal", can walk randomly
                        VS_Actual.FetalHR = System.Math.Max (0, VS_Actual.FetalHR + (int)Math.RandomDbl (-fvar, fvar));
                    break;
            }

            /* Triggering of events *****
             * Trigger variable/random accelerations/decelerations if: rhythm is in interval and is not refractory
             * This prevents repeat triggering, triggering too often...
             * Note: Early and Late Decelerations are triggered by uterine contractions (OnObstetric_Contraction)
             *
             * Acceleration durations in seconds are hardcoded physiologic timing for normal duration per:
             * https://www.perinatology.com/Fetal%20Monitoring/Intrapartum%20Monitoring.htm
             * https://ncc-efm.org/filz/2021%20NCC%20MonographFinal_3-16-21%20CMYK.pdf
             */

            if (stateFetalHeartRhythm == FetalHeart_Rhythms.States.Interval) {
                switch (ObstetricFetalHeartRhythm.Value) {
                    default: break;

                    case FetalHeart_Rhythms.Values.Acceleration:                // Accelerations
                        stateFetalHeartRhythm = FetalHeart_Rhythms.States.Accelerating;
                        bufferFetalHeartRhythmWander = Math.RandomInt (2, 5);
                        await TimerObstetric_Fetal_Variability.ResetStart (Math.RandomInt (15, 120) * 1000 / TimerObstetric_Multiplier);
                        break;

                    case FetalHeart_Rhythms.Values.DecelerationVariable:        // Variable Deceleration
                        stateFetalHeartRhythm = FetalHeart_Rhythms.States.Decelerating;
                        bufferFetalHeartRhythmWander = Math.RandomInt (2, 5);
                        await TimerObstetric_Fetal_Variability.ResetStart (Math.RandomInt (15, 120) * 1000 / TimerObstetric_Multiplier);
                        break;
                }
            }
        }

        private async Task OnObstetric_Fetal_Variability () {
            switch (ObstetricFetalHeartRhythm.Value) {
                default:
                case FetalHeart_Rhythms.Values.Baseline:
                    // This is the "fall-through" location if the user makes a change that needs to stop the variability cycle
                    stateFetalHeartRhythm = FetalHeart_Rhythms.States.Interval;
                    await TimerObstetric_Fetal_Variability.Stop ();
                    break;

                case FetalHeart_Rhythms.Values.Acceleration:
                case FetalHeart_Rhythms.Values.DecelerationVariable:
                    // Variability triggered w/ refractory lockout mechanism
                    switch (stateFetalHeartRhythm) {
                        case FetalHeart_Rhythms.States.Accelerating:
                        case FetalHeart_Rhythms.States.Decelerating:

                            // Entering a refractory period to prevent repeated triggering of variability
                            stateFetalHeartRhythm = FetalHeart_Rhythms.States.Refractory;
                            await TimerObstetric_Fetal_Variability.ResetStart (Math.RandomInt (60, 300) * 1000 / TimerObstetric_Multiplier);
                            break;

                        case FetalHeart_Rhythms.States.Refractory:
                            stateFetalHeartRhythm = FetalHeart_Rhythms.States.Interval;
                            await TimerObstetric_Fetal_Variability.Stop ();
                            break;
                    }
                    break;

                case FetalHeart_Rhythms.Values.DecelerationEarly:
                    // Variability triggered by contractions- skip the refractory lockout mechanism
                    stateFetalHeartRhythm = FetalHeart_Rhythms.States.Interval;
                    await TimerObstetric_Fetal_Variability.Stop ();
                    break;

                case FetalHeart_Rhythms.Values.DecelerationLate:
                    switch (stateFetalHeartRhythm) {
                        default:
                        case FetalHeart_Rhythms.States.Interval:
                            // Start the late deceleration, reset the timer for the second half
                            stateFetalHeartRhythm = FetalHeart_Rhythms.States.Decelerating;
                            bufferFetalHeartRhythmWander = Math.RandomInt (2, 5);
                            await TimerObstetric_Fetal_Variability.ResetStart ((int)((ObstetricContractionDuration / 5 * 3) * 1000d / TimerObstetric_Multiplier));
                            break;

                        case FetalHeart_Rhythms.States.Decelerating:
                            // Ending of the late deceleration, return to baseline
                            stateFetalHeartRhythm = FetalHeart_Rhythms.States.Interval;
                            await TimerObstetric_Fetal_Variability.Stop ();
                            break;
                    }
                    break;
            }
        }

        private async Task OnObstetric_RunTimers () {
            if (!InLabor) {
                flagObstetricContraction = false;
                await TimerObstetric_Contraction.Stop ();
            } else if (InLabor) {
                if (!flagObstetricContraction)
                    await TimerObstetric_Contraction.Continue ((int)(ObstetricContractionFrequency * 1000d / TimerObstetric_Multiplier));
                else if (flagObstetricContraction)
                    await TimerObstetric_Contraction.Continue ((int)(ObstetricContractionDuration * 1000d / TimerObstetric_Multiplier));
            }
        }

        private async Task OnObstetric_Contraction () {
            if (!flagObstetricContraction) {
                // Contraction onset
                flagObstetricContraction = true;
                await TimerObstetric_Contraction.ResetStart ((int)(ObstetricContractionDuration * 1000d / TimerObstetric_Multiplier));

                await OnPhysiologyEvent (PhysiologyEventTypes.Obstetric_Contraction_Start);

                // No need for triggering lockouts (e.g. Refractory state) when triggering based on contractions
                switch (ObstetricFetalHeartRhythm.Value) {
                    default: break;

                    case FetalHeart_Rhythms.Values.DecelerationEarly:
                        stateFetalHeartRhythm = FetalHeart_Rhythms.States.Decelerating;
                        bufferFetalHeartRhythmWander = Math.RandomInt (2, 5);
                        await TimerObstetric_Fetal_Variability.ResetStart ((int)((ObstetricContractionDuration / 5 * 3) * 1000d / TimerObstetric_Multiplier));
                        break;

                    case FetalHeart_Rhythms.Values.DecelerationLate:
                        // Use the variability timer to wait out 2/3 the contraction, then trigger the late deceleration
                        stateFetalHeartRhythm = FetalHeart_Rhythms.States.Interval;
                        await TimerObstetric_Fetal_Variability.ResetStart ((int)((ObstetricContractionDuration / 5 * 3) * 1000d / TimerObstetric_Multiplier));
                        break;
                }
            } else {
                // Contraction ending
                flagObstetricContraction = false;
                await TimerObstetric_Contraction.ResetStart ((int)(ObstetricContractionFrequency * 1000d / TimerObstetric_Multiplier));

                await OnPhysiologyEvent (PhysiologyEventTypes.Obstetric_Contraction_End);
            }
        }
    }
}