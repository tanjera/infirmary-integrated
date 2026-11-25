/* Physiology.cs
 * Infirmary Integrated
 * By Ibi Keller (Tanjera), (c) 2017-2023
 *
 * All patient's physiology modeling takes place in Physiology.cs, consisting of:
 * - Variables: vital signs and modeling parameters
 * - Timers: for modeling cardiac and respiratory rhythms, etc.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace II {
    public class Physiology {
        /* References */
        private Timer? _timerSimulation;
        
        /* Mirroring variables */
        public DateTime Updated;                    // DateTime this Patient was last updated

        /* Parameters for patient simulation, e.g. vital signs */

        public VitalSigns VitalSignsSettings = new (),
                            VitalSignsActual = new ();

        /* Basic vital signs and advanced hemodynamics (partial) */
        public CardiacRhythms CardiacRhythm = new ();
        public Respiratory_Rhythms RespiratoryRhythm = new ();
        public PulmonaryArteryRhythms PulmonaryArteryPlacement = new ();

        /* Respiratory profile (partial) */
        public bool MechanicallyVentilated;
        public bool RespirationInflated;

        /* Cardiac profile */
        public int PacemakerThreshold;            // Patient's threshold for electrical capture to pacemaker spike

        public bool PulsusParadoxus,
                    PulsusAlternans,
                    ElectricalAlternans;

        public Cardiac_Axes CardiacAxis = new ();
        public double QRSInterval, QTcInterval;           // In seconds (e.g. 0.1)
        
        // Amplitude coefficients are multipliers; 1.0 is a non-modifying value
        // ReSharper disable once InconsistentNaming
        public double []? PAmplitude, QAmplitude, RAmplitude, SAmplitude, TAmplitude; 
        
        // Elevation factors are additive (not multiplicative!); 0d is a non-modifying value
        public double []? STElevation;

        /* Obstetric profile */

        public int ObstetricFetalRateVariability,           // Variability in bpm
                    ObstetricContractionFrequency,          // Frequency in seconds
                    ObstetricContractionDuration;           // Duration in seconds

        public int ObstetricUterineRestingTone,                 // mmHg Tocograph of uterus' resting tone
                    ObstetricContractionIntensity;      // mmHg Tocograph of each contraction

        public FetalHeart_Rhythms ObstetricFetalHeartRhythm = new ();

        /* Device (implanted?) parameters */

        public int PacemakerRate,                  // DeviceDefib's transcutaneous pacemaker rate
                    PacemakerEnergy;               // DeviceDefib's pacemaker energy delivery amount

        /* Intra-aortic balloon pump parameters */
        public int IABPAP, IABPDBP, IABPMAP;     // Intra-aortic balloon pump blood pressures

        // Default IABP settings:
        public bool IABPActive;            // Is the Device_IABP currently augmenting?

        public int IABPDelayDynamic = 280;                             // Delay between trigger to inflation (in milliseconds); dynamically adjusted
        public int IABPDelayManual = 0;                                // Manual delay adjustment (in milliseconds)
        public IABPTriggers IABPTrigger = IABPTriggers.ECG;          // Device_IABP's trigger; data backflow for strip processing

        public enum IABPTriggers { ECG, Pressure }

        public static int [] IABPDelays = [280, 200];    // Default delays to use for trigger timing

        /* Timers and multipliers for temporal modeling */

        public int TimerObstetricMultiplier = 1;

        private readonly Timer _timerCardiacBaseline = new (),
                        _timerCardiacAtrialElectric = new (),
                        _timerCardiacVentricularElectric = new (),
                        _timerCardiacAtrialMechanical = new (),
                        _timerCardiacVentricularMechanical = new (),
                        _timerIABPBalloonTrigger = new (),
                        _timerDefibrillation = new (),
                        _timerPacemakerBaseline = new (),
                        _timerPacemakerSpike = new (),
                        _timerRespiratoryBaseline = new (),
                        _timerRespiratoryInspiration = new (),
                        _timerRespiratoryExpiration = new (),
                        _timerObstetricBaseline = new (),
                        _timerObstetricFetalBaseline = new (),
                        _timerObstetricFetalVariability = new (),
                        _timerObstetricContraction = new ();

        private static readonly int DefaultElectromechanicalDelay = 180;   // Delay in electrical to mechanical capture in milliseconds

        /* Internal counters, flags, buffers for propogating aberrancies and rhythms*/

        private int _counterCardiacAberrancy,
                    _counterCardiacArrhythmia,
                    _counterRespiratoryArrhythmia;

        private bool _switchParadoxus,
                     _switchCardiacArrhythmia,
                     _switchRespiratoryArrhythmia;

        private bool _flagObstetricContraction;
        private int _bufferFetalHeartRhythmWander = 1;
        private FetalHeart_Rhythms.States _stateFetalHeartRhythm = FetalHeart_Rhythms.States.Interval;

        /* Definitions for Vital_Signs class */

        public class VitalSigns {
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

            public double RRIEI,               // Inspiratory to expiratory ratio
                         RRIEE;

            public int FetalHR;          // Baseline fetal heart rate

            public VitalSigns () {
            }

            public VitalSigns (VitalSigns v)
                => Set (v);

            public Task Set (VitalSigns v) {
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
                RRIEI = v.RRIEI;
                RRIEE = v.RRIEE;
                FetalHR = v.FetalHR;

                return Task.CompletedTask;
            }
        }

        public Physiology (Timer? timerSimulation) {
            _timerSimulation = timerSimulation;
            
            List<Task> t = [
                UpdateParameters_Cardiac (
                    // Basic cardiac vital signs
                    80,
                    120, 80, 95,
                    98,
                    37.0d,
                    CardiacRhythms.Values.SinusRhythm,

                    // Advanced hemodynamics
                    6,
                    120, 80, 95,
                    6d,
                    PulmonaryArteryRhythms.Values.PulmonaryArtery,
                    22, 12, 16,
                    8,
                    1,

                    // Cardiac profile
                    50,
                    false, false, false,
                    Cardiac_Axes.Values.Normal,
                    0.08d, 0.4d,

                    // Amplitude coefficients are multipliers; 1.0 is a non-modifying value
                    [1d, 1d, 1d, 1d, 1d, 1d, 1d, 1d, 1d, 1d, 1d, 1d],
                    [1d, 1d, 1d, 1d, 1d, 1d, 1d, 1d, 1d, 1d, 1d, 1d],
                    [1d, 1d, 1d, 1d, 1d, 1d, 1d, 1d, 1d, 1d, 1d, 1d],
                    [1d, 1d, 1d, 1d, 1d, 1d, 1d, 1d, 1d, 1d, 1d, 1d],
                    [1d, 1d, 1d, 1d, 1d, 1d, 1d, 1d, 1d, 1d, 1d, 1d],

                    // Elevation factors are additive (not multiplicative!); 0d is a non-modifying value
                    [0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d]
                ),

                UpdateParameters_Respiratory (
                    16,
                    Respiratory_Rhythms.Values.Regular,
                    40,
                    false,
                    1d, 2d),

                UpdateParameters_Obstetric (
                    145,
                    20,
                    FetalHeart_Rhythms.Values.Baseline,
                    120,
                    60,
                    50, 10)
            ];

            Task.WaitAll (t.ToArray());
            
            Task.WhenAny(InitTimers ());
            Task.WhenAny (ResetStartTimers ());
        }

        ~Physiology () => Dispose ();

        public void Dispose () {
            Task.WaitAny(UnsubscribePhysiologyEvent());

            _timerCardiacBaseline.Dispose ();
            _timerCardiacAtrialElectric.Dispose ();
            _timerCardiacVentricularElectric.Dispose ();
            _timerCardiacAtrialMechanical.Dispose ();
            _timerCardiacVentricularMechanical.Dispose ();
            _timerIABPBalloonTrigger.Dispose ();
            _timerDefibrillation.Dispose ();
            _timerPacemakerBaseline.Dispose ();
            _timerPacemakerSpike.Dispose ();
            _timerRespiratoryBaseline.Dispose ();
            _timerRespiratoryInspiration.Dispose ();
            _timerRespiratoryExpiration.Dispose ();
            _timerObstetricBaseline.Dispose ();
            _timerObstetricFetalBaseline.Dispose ();
            _timerObstetricFetalVariability.Dispose ();
            _timerObstetricContraction.Dispose ();
        }

        public int HR {
            get { return VitalSignsActual.HR; }
            set { VitalSignsSettings.HR = value; }
        }

        public int RR {
            get { return VitalSignsActual.RR; }
            set { VitalSignsSettings.RR = value; }
        }

        public int ETCO2 {
            get { return VitalSignsActual.ETCO2; }
            set { VitalSignsSettings.ETCO2 = value; }
        }

        public int SPO2 {
            get { return VitalSignsActual.SPO2; }
            set { VitalSignsSettings.SPO2 = value; }
        }

        public int CVP {
            get { return VitalSignsActual.CVP; }
            set { VitalSignsSettings.CVP = value; }
        }

        public int NSBP {
            get { return VitalSignsActual.NSBP; }
            set { VitalSignsSettings.NSBP = value; }
        }

        public int NDBP {
            get { return VitalSignsActual.NDBP; }
            set { VitalSignsSettings.NDBP = value; }
        }

        public int NMAP {
            get { return VitalSignsActual.NMAP; }
            set { VitalSignsSettings.NMAP = value; }
        }

        public int ASBP {
            get { return VitalSignsActual.ASBP; }
            set { VitalSignsSettings.ASBP = value; }
        }

        public int ADBP {
            get { return VitalSignsActual.ADBP; }
            set { VitalSignsSettings.ADBP = value; }
        }

        public int AMAP {
            get { return VitalSignsActual.AMAP; }
            set { VitalSignsSettings.AMAP = value; }
        }

        public double CO {
            get { return VitalSignsActual.CO; }
            set { VitalSignsSettings.CO = value; }
        }

        public int PSP {
            get { return VitalSignsActual.PSP; }
            set { VitalSignsSettings.PSP = value; }
        }

        public int PDP {
            get { return VitalSignsActual.PDP; }
            set { VitalSignsSettings.PDP = value; }
        }

        public int PMP {
            get { return VitalSignsActual.PMP; }
            set { VitalSignsSettings.PMP = value; }
        }

        public int ICP {
            get { return VitalSignsActual.ICP; }
            set { VitalSignsSettings.ICP = value; }
        }

        public int IAP {
            get { return VitalSignsActual.IAP; }
            set { VitalSignsSettings.IAP = value; }
        }

        public double T {
            get { return VitalSignsActual.T; }
            set { VitalSignsSettings.T = value; }
        }

        public double RRIEI {
            get { return VitalSignsActual.RRIEI; }
            set { VitalSignsSettings.RRIEI = value; }
        }

        public double RRIEE {
            get { return VitalSignsActual.RRIEE; }
            set { VitalSignsSettings.RRIEE = value; }
        }

        public int FetalHR {
            get { return VitalSignsActual.FetalHR; }
            set { VitalSignsSettings.FetalHR = value; }
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

        // Calculates R-to-R interval,
        public static double CalculateHRInterval (int hr) {
            return 60d / System.Math.Max (1, hr);
        }

        public static int CalculateMAP (int sbp, int dbp) {
            return dbp + ((sbp - dbp) / 3);
        }

        public static int CalculateCPP (int? icp, int? map) {
            if (icp is null || map is null)
                return 0;

            return (int)(map - icp);
        }

        // Allows for pulling historical sets of vital signs; useful for calculating beat times after electromechanical delay has caused additional
        // cardiac events to take over the newest set of vital signs!
        public VitalSigns GetLastVS (PhysiologyEventTypes pe, int amountEvents = 1) {
            List<PhysiologyEventArgs> peList;
            
            lock (ListPhysiologyEvents) {
                peList = ListPhysiologyEvents.Where (p => p.EventType == pe).ToList ();
            }

            if (peList.Count < (2 + amountEvents))
                return VitalSignsActual;

            return peList [peList.Count - (1 + amountEvents)].Vitals;
        }

        public double GetHRInterval { get { return CalculateHRInterval (VitalSignsActual.HR); } }

        // Gets Respiratory Rate interval... *not* to be confused with R-to-R interval (ECG)
        public double GetRRInterval { get { return CalculateHRInterval (VitalSignsActual.RR); } }

        public double GetRRIntervalInspiratory { get { return (GetRRInterval / (RRIEI + RRIEE)) * RRIEI; } }
        public double GetRRIntervalExpiratory { get { return (GetRRInterval / (RRIEI + RRIEE)) * RRIEE; } }

        /* A functional length in seconds of pulsatile rhythms:
           - Normocardia (60-100): 1/3 of cardiac cycle (RR interval) is systole; 0.4 bridges better
           - Bradycardia (<60): RR interval is elongated and would generate stretched waveforms if used
             ... approximate with 3.3 * QRS interval (bridges @ 60 and holds consistent downward)
           - Tachycardia: (>100): RR interval is short and would cause overdrawing abnormally narrowed waveforms
             ... approximate with 2.7 * QRS interval (bridges @ 100 and mimics increased ino/dromotropy
             ... Goal coefficient should have allow for normal diastolic times @ 100 but have drastically
                 decreased distolic times > 180-200 to mimic this cause of poor cardiac output
        */ 
        public double GetSystoleSeconds {
            get {
                if (VitalSignsSettings.HR < 60) {                          // Brady
                    return Math.Clamp (3.3 * QRSInterval, 0.1, 1);     // Clamp to prevent GIGO
                } else if (VitalSignsSettings.HR > 100) {                  // Tachy
                    return Math.Clamp (2.7 * QRSInterval, 0.1, 1);     // Clamp to prevent GIGO
                } else {                                            // Normocardia
                    return CalculateHRInterval (VitalSignsSettings.HR) * 0.4d; // Bridges w/ brady<->tachy well
                }
            }
        }

        // Using Fridericia Formula for QT <-> QTc calculation
        public double GetQTInterval {
            get {
                return System.Math.Pow ((60d / System.Math.Max (1, VitalSignsActual.HR)), (1d / 3d)) * QTcInterval;
            }
        }

        public double GetSTInterval { get { return GetQTInterval - QRSInterval; } }
        public double GetSTSegment { get { return GetSTInterval * (1d / 3d); } }
        public double GetTInterval { get { return GetSTInterval * (2d / 3d); } }

        public int MeasureHR_ECG (double offsetSeconds)
            => MeasureHR (offsetSeconds);

        public int MeasureHR_SPO2 (double offsetSeconds)
            => MeasureHR (offsetSeconds, true);

        public int MeasureHR (double offsetSeconds, bool isPulse = false) {
            CleanListPhysiologyEvents ();

            if (isPulse && !CardiacRhythm.HasVentricularPulse)
                return 0;

            /* Note on sampling and counting measurements:
             * Attempting numerous sampling methods including:
             * - Sampling at multiple timeframes (3 sec, 6, 12, 15, 20, and 30 seconds)
             * - Presenting one, an average, or a simple moving average of the sampling periods
             * None of them presented as true-to-life as a simple 6-second or 12-second sample
             * 6 second samples: resolve easily on the base-10 for regular vital sign ranges
             * 12 second samples: resolve easily to the base-5 for regular vital sign ranges
             * They also update to an interval consistent with the visual strip (6-second strips are routine)
             * and therefore correspond to visual input for correlation with manual counting.
             */
            int sampleTime = _timerSimulation?.Epoch switch {
                < 6000 => 3,
                < 9000 => 6,
                < 12000 => 9,
                _ => 12
            };
            int counter = 0;
            
            lock (LockListPhysiologyEvents) {
                foreach (PhysiologyEventArgs ea in ListPhysiologyEvents
                             .Where (o => o.EventType == PhysiologyEventTypes.CardiacVentricularElectric)) {
                    if (ea.Occurred >= (_timerSimulation?.Epoch + (-(sampleTime + offsetSeconds) * 1000))
                        && ea.Occurred <= (_timerSimulation?.Epoch + (-offsetSeconds * 1000)))
                        counter += 1;                                      // Store how many events occured in the sampling period
                }
            }
            
            int raw = (int)System.Math.Round(counter / (sampleTime / 60d), MidpointRounding.ToEven);
            // Only return a round number (0, 5, 10) or an even number (52)
            return raw % 5 == 0 ? raw : raw - (raw % 2);
        }

        public int MeasureRR (double offsetSeconds) {
            CleanListPhysiologyEvents ();

            /* See note on sampling in MeasureHR
             * Major difference for respiratory rate: sampling times need to be longer due to reduced rate of respirations
             * compared to heart rates             
             */
            int sampleTime = _timerSimulation?.Epoch switch {
                < 10000 => 5,
                < 20000 => 10,
                < 40000 => 20,
                < 60000 => 40,
                _ => 60
            };
            int counter = 0;

            lock (LockListPhysiologyEvents) {
                foreach (PhysiologyEventArgs ea in ListPhysiologyEvents
                             .Where (o => o.EventType == PhysiologyEventTypes.RespiratoryInspiration)) {
                    if (ea.Occurred >= (_timerSimulation?.Epoch + (-(sampleTime + offsetSeconds) * 1000))
                        && ea.Occurred <= (_timerSimulation?.Epoch + (-offsetSeconds * 1000)))
                        counter += 1;                                      // Store how many events occured in the sampling period
                }
            }
            
            int raw = (int)System.Math.Round(counter / (sampleTime / 60d), MidpointRounding.ToEven);
            // Only return a an even number (52)
            return raw % 2 == 0 ? raw : raw - (raw % 2);
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
        public readonly object LockListPhysiologyEvents = new ();
        public List<PhysiologyEventArgs> ListPhysiologyEvents = [];

        public event EventHandler<PhysiologyEventArgs>? PhysiologyEvent;

        public class PhysiologyEventArgs : EventArgs {
            public Physiology? Physiology;                  // Remains as a pointer
            public VitalSigns Vitals;                      // Copies over as a clone, not a pointer
            public PhysiologyEventTypes EventType;
            public ulong Occurred;                          // Use Epoch timing for Physiology! 

            public PhysiologyEventArgs (Physiology? p, PhysiologyEventTypes e, Timer? t) {
                p ??= new (t);

                EventType = e;
                Physiology = p;
                Vitals = new VitalSigns (p.VitalSignsActual);
                Occurred = t?.Epoch ?? 0;
            }
        }

        public enum PhysiologyEventTypes {
            VitalsChange,
            CardiacBaseline,
            CardiacAtrialElectric,
            CardiacVentricularElectric,
            CardiacAtrialMechanical,
            CardiacVentricularMechanical,
            IABPBalloonInflation,
            Defibrillation,
            PacermakerSpike,
            RespiratoryBaseline,
            RespiratoryInspiration,
            RespiratoryExpiration,
            ObstetricBaseline,
            ObstetricFetalBaseline,
            ObstetricContractionStart,
            ObstetricContractionEnd
        }

        public Task OnPhysiologyEvent (PhysiologyEventTypes e, Timer? t) {
            PhysiologyEventArgs ea = new (this, e, t);
            
            lock (LockListPhysiologyEvents) {
                ListPhysiologyEvents.Add (ea);
            }

            PhysiologyEvent?.Invoke (this, ea);

            return Task.CompletedTask;
        }

        public void CleanListPhysiologyEvents () {
            // Remove all listings older than 1 minute... prevent cluttering memory
            // ... But make sure to wait a minute before doing the math (uint overflow!)
            lock (LockListPhysiologyEvents) {
                if (_timerSimulation?.Epoch is not null) {
                    for (int i = ListPhysiologyEvents.Count - 1; i >= 0; i--) {
                        if (_timerSimulation?.Epoch > 60000
                            && ListPhysiologyEvents [i].Occurred < _timerSimulation?.Epoch - 60000) {
                            ListPhysiologyEvents.RemoveAt (i);
                        }
                    }
                }
            }
        }

        /* Process all timers for patient modeling */

        public void ProcessTimers (object? sender, EventArgs e) {
            /* For cross-platform compatibility with different timers ...
             * When creating a Patient object, create a native thread-safe Timer object,
             * short interval, and call this function on its Tick to process all Patient
             * timers.
             */
            
            _timerCardiacBaseline.Process ();
            _timerCardiacAtrialElectric.Process ();
            _timerCardiacVentricularElectric.Process ();
            _timerCardiacAtrialMechanical.Process ();
            _timerCardiacVentricularMechanical.Process ();
            _timerIABPBalloonTrigger.Process ();
            _timerDefibrillation.Process ();
            _timerPacemakerBaseline.Process ();
            _timerPacemakerSpike.Process ();
            _timerRespiratoryBaseline.Process ();
            _timerRespiratoryInspiration.Process ();
            _timerRespiratoryExpiration.Process ();
            _timerObstetricBaseline.Process ();
            _timerObstetricFetalBaseline.Process ();
            _timerObstetricFetalVariability.Process ();
            _timerObstetricContraction.Process ();
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
                            // Patient/scenario information
                            case "Updated": Updated = Utility.DateTime_FromString (pValue); break;

                            // Basic vital signs
                            case "HR": VitalSignsSettings.HR = int.Parse (pValue); break;
                            case "NSBP": VitalSignsSettings.NSBP = int.Parse (pValue); break;
                            case "NDBP": VitalSignsSettings.NDBP = int.Parse (pValue); break;
                            case "NMAP": VitalSignsSettings.NMAP = int.Parse (pValue); break;
                            case "RR": VitalSignsSettings.RR = int.Parse (pValue); break;
                            case "SPO2": VitalSignsSettings.SPO2 = int.Parse (pValue); break;
                            case "T": VitalSignsSettings.T = double.Parse (pValue, CultureInfo.InvariantCulture); break;

                            // Rhythms
                            case "Cardiac_Rhythm": CardiacRhythm.Value = (CardiacRhythms.Values)Enum.Parse (typeof (CardiacRhythms.Values), pValue); break;
                            case "Respiratory_Rhythm": RespiratoryRhythm.Value = (Respiratory_Rhythms.Values)Enum.Parse (typeof (Respiratory_Rhythms.Values), pValue); break;
                            case "PulmonaryArtery_Rhythm": PulmonaryArteryPlacement.Value = (PulmonaryArteryRhythms.Values)Enum.Parse (typeof (PulmonaryArteryRhythms.Values), pValue); break;

                            // Advanced hemodynamics
                            case "ETCO2": VitalSignsSettings.ETCO2 = int.Parse (pValue); break;
                            case "CVP": VitalSignsSettings.CVP = int.Parse (pValue); break;
                            case "ASBP": VitalSignsSettings.ASBP = int.Parse (pValue); break;
                            case "ADBP": VitalSignsSettings.ADBP = int.Parse (pValue); break;
                            case "AMAP": VitalSignsSettings.AMAP = int.Parse (pValue); break;
                            case "CO": VitalSignsSettings.CO = double.Parse (pValue, CultureInfo.InvariantCulture); break;
                            case "PSP": VitalSignsSettings.PSP = int.Parse (pValue); break;
                            case "PDP": VitalSignsSettings.PDP = int.Parse (pValue); break;
                            case "PMP": VitalSignsSettings.PMP = int.Parse (pValue); break;
                            case "ICP": VitalSignsSettings.ICP = int.Parse (pValue); break;
                            case "IAP": VitalSignsSettings.IAP = int.Parse (pValue); break;

                            // Respiratory profile
                            case "Mechanically_Ventilated": MechanicallyVentilated = bool.Parse (pValue); break;
                            case "Respiratory_IERatio_I": VitalSignsSettings.RRIEI = double.Parse (pValue, CultureInfo.InvariantCulture); break;
                            case "Respiratory_IERatio_E": VitalSignsSettings.RRIEE = double.Parse (pValue, CultureInfo.InvariantCulture); break;

                            // Cardiac profile
                            case "Pacemaker_Threshold": PacemakerThreshold = int.Parse (pValue); break;
                            case "Pulsus_Paradoxus": PulsusParadoxus = bool.Parse (pValue); break;
                            case "Pulsus_Alternans": PulsusAlternans = bool.Parse (pValue); break;
                            case "Electrical_Alternans": ElectricalAlternans = bool.Parse (pValue); break;
                            case "Cardiac_Axis": CardiacAxis.Value = (Cardiac_Axes.Values)Enum.Parse (typeof (Cardiac_Axes.Values), pValue); break;
                            case "QRS_Interval": QRSInterval = double.Parse (pValue, CultureInfo.InvariantCulture); break;
                            case "QTc_Interval": QTcInterval = double.Parse (pValue, CultureInfo.InvariantCulture); break;
                            
                            case "P_Amplitude":
                                string [] eP = [.. pValue.Split (',').Where ((o) => o != "")];
                                for (int i = 0; i < eP.Length && i < PAmplitude?.Length; i++)
                                    PAmplitude [i] = double.Parse (eP [i], CultureInfo.InvariantCulture);
                                break;
                            
                            case "Q_Amplitude":
                                string [] eQ = [.. pValue.Split (',').Where ((o) => o != "")];
                                for (int i = 0; i < eQ.Length && i < QAmplitude?.Length; i++)
                                    QAmplitude [i] = double.Parse (eQ [i], CultureInfo.InvariantCulture);
                                break;
                            
                            case "R_Amplitude":
                                string [] eR = [.. pValue.Split (',').Where ((o) => o != "")];
                                for (int i = 0; i < eR.Length && i < RAmplitude?.Length; i++)
                                    RAmplitude [i] = double.Parse (eR [i], CultureInfo.InvariantCulture);
                                break;

                            case "S_Amplitude":
                                string [] eS = [.. pValue.Split (',').Where ((o) => o != "")];
                                for (int i = 0; i < eS.Length && i < SAmplitude?.Length; i++)
                                    SAmplitude [i] = double.Parse (eS [i], CultureInfo.InvariantCulture);
                                break;

                            case "T_Amplitude":
                                string [] eT = [.. pValue.Split (',').Where ((o) => o != "")];
                                for (int i = 0; i < eT.Length && i < TAmplitude?.Length; i++)
                                    TAmplitude [i] = double.Parse (eT [i], CultureInfo.InvariantCulture);
                                break;

                            case "ST_Elevation":
                                string [] eST = [.. pValue.Split (',').Where ((o) => o != "")];
                                for (int i = 0; i < eST.Length && i < STElevation?.Length; i++)
                                    STElevation [i] = double.Parse (eST [i], CultureInfo.InvariantCulture);
                                break;

                            // Obstetric profile
                            case "ObstetricFetalHeartRate": VitalSignsSettings.FetalHR = int.Parse (pValue); break;
                            case "ObstetricFetalRateVariability": ObstetricFetalRateVariability = int.Parse (pValue); break;
                            case "ObstetricFetalHeartRhythm": ObstetricFetalHeartRhythm.Value = (FetalHeart_Rhythms.Values)Enum.Parse (typeof (FetalHeart_Rhythms.Values), pValue); break;
                            case "ObstetricContractionFrequency": ObstetricContractionFrequency = int.Parse (pValue); break;
                            case "ObstetricContractionDuration": ObstetricContractionDuration = int.Parse (pValue); break;
                            case "ObstetricContractionIntensity": ObstetricContractionIntensity = int.Parse (pValue); break;
                            case "ObstetricUterineRestingTone": ObstetricUterineRestingTone = int.Parse (pValue); break;

                            // Device settings

                            case "Pacemaker_Rate": PacemakerRate = int.Parse (pValue); break;
                            case "Pacemaker_Energy": PacemakerEnergy = int.Parse (pValue); break;

                            case "IABP_AP": IABPAP = int.Parse (pValue); break;
                            case "IABP_DBP": IABPDBP = int.Parse (pValue); break;
                            case "IABP_MAP": IABPMAP = int.Parse (pValue); break;
                            case "IABP_Active": IABPActive = bool.Parse (pValue); break;
                            case "IABP_Trigger": IABPTrigger = (IABPTriggers)Enum.Parse (typeof (IABPTriggers), pValue); break;
                        }
                    }
                }
            } catch {
                /* If the load fails... just bail on the actual value parsing and continue the load process */
            }

            sRead.Close ();

            // Reset measurements to set parameters
            await VitalSignsActual.Set (VitalSignsSettings);

            await ResetStartTimers ();
            await OnCardiac_Baseline ();
            await OnRespiratory_Baseline ();

            await OnPhysiologyEvent (PhysiologyEventTypes.VitalsChange, _timerSimulation);
        }

        /* Process for saving Patient{} information to simulation file  */

        public string Save (int indent = 1) {
            string dent = Utility.Indent (indent);
            StringBuilder sWrite = new ();

            // File/scenario information
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Updated", Utility.DateTime_ToString (Updated)));

            // Basic vital signs
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "HR", VitalSignsSettings.HR));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "NSBP", VitalSignsSettings.NSBP));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "NDBP", VitalSignsSettings.NDBP));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "NMAP", VitalSignsSettings.NMAP));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "RR", VitalSignsSettings.RR));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "SPO2", VitalSignsSettings.SPO2));
            sWrite.AppendLine (String.Format (CultureInfo.InvariantCulture, "{0}{1}:{2}", dent, "T", VitalSignsSettings.T));

            // Rhythms
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Cardiac_Rhythm", CardiacRhythm.Value));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Respiratory_Rhythm", RespiratoryRhythm.Value));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "PulmonaryArtery_Rhythm", PulmonaryArteryPlacement.Value));

            // Advanced hemodynamics
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "ETCO2", VitalSignsSettings.ETCO2));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "CVP", VitalSignsSettings.CVP));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "ASBP", VitalSignsSettings.ASBP));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "ADBP", VitalSignsSettings.ADBP));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "AMAP", VitalSignsSettings.AMAP));
            sWrite.AppendLine (String.Format (CultureInfo.InvariantCulture, "{0}{1}:{2}", dent, "CO", VitalSignsSettings.CO));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "PSP", VitalSignsSettings.PSP));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "PDP", VitalSignsSettings.PDP));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "PMP", VitalSignsSettings.PMP));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "ICP", VitalSignsSettings.ICP));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "IAP", VitalSignsSettings.IAP));

            // Respiratory profile
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Mechanically_Ventilated", MechanicallyVentilated));
            sWrite.AppendLine (String.Format (CultureInfo.InvariantCulture, "{0}{1}:{2}", dent, "Respiratory_IERatio_I", VitalSignsSettings.RRIEI));
            sWrite.AppendLine (String.Format (CultureInfo.InvariantCulture, "{0}{1}:{2}", dent, "Respiratory_IERatio_E", VitalSignsSettings.RRIEE));

            // Cardiac profile
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Pacemaker_Threshold", PacemakerThreshold));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Pulsus_Paradoxus", PulsusParadoxus));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Pulsus_Alternans", PulsusAlternans));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Electrical_Alternans", ElectricalAlternans));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Cardiac_Axis", CardiacAxis.Value));
            sWrite.AppendLine (String.Format (CultureInfo.InvariantCulture, "{0}{1}:{2}", dent, "QRS_Interval", QRSInterval));
            sWrite.AppendLine (String.Format (CultureInfo.InvariantCulture, "{0}{1}:{2}", dent, "QTc_Interval", QTcInterval));

            // Implement InvariantCulture for doubles
            if (PAmplitude is not null) {
                string invariantResult = string.Join (", ", PAmplitude.Select (n => n.ToString (CultureInfo.InvariantCulture)));
                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "P_Amplitude", invariantResult));
            }

            if (QAmplitude is not null) {
                string invariantResult = string.Join (", ", QAmplitude.Select (n => n.ToString (CultureInfo.InvariantCulture)));
                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Q_Amplitude", invariantResult));
            }

            if (RAmplitude is not null) {
                string invariantResult = string.Join (", ", RAmplitude.Select (n => n.ToString (CultureInfo.InvariantCulture)));
                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "R_Amplitude", invariantResult));
            }

            if (SAmplitude is not null) {
                string invariantResult = string.Join (", ", SAmplitude.Select (n => n.ToString (CultureInfo.InvariantCulture)));
                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "S_Amplitude", invariantResult));
            }

            if (TAmplitude is not null) {
                string invariantResult = string.Join (", ", TAmplitude.Select (n => n.ToString (CultureInfo.InvariantCulture)));
                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "T_Amplitude", invariantResult));
            }

            if (STElevation is not null) {
                string invariantResult = string.Join (", ", STElevation.Select (n => n.ToString (CultureInfo.InvariantCulture)));
                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "ST_Elevation", invariantResult));
            }

            // Obstetric profile
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "ObstetricFetalHeartRate", VitalSignsSettings.FetalHR));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "ObstetricFetalRateVariability", ObstetricFetalRateVariability));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "ObstetricFetalHeartRhythm", ObstetricFetalHeartRhythm.Value));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "ObstetricContractionFrequency", ObstetricContractionFrequency));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "ObstetricContractionDuration", ObstetricContractionDuration));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "ObstetricContractionIntensity", ObstetricContractionIntensity));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "ObstetricUterineRestingTone", ObstetricUterineRestingTone));

            // Device settings

            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Pacemaker_Rate", PacemakerRate));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Pacemaker_Energy", PacemakerEnergy));

            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "IABP_AP", IABPAP));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "IABP_DBP", IABPDBP));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "IABP_MAP", IABPMAP));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "IABP_Active", IABPActive));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "IABP_Trigger", IABPTrigger));

            return sWrite.ToString ();
        }

        public async Task UpdateParameters_Cardiac (
                    // Basic vital signs
                    int hr,
                    int nsbp, int ndbp, int nmap,
                    int spo2,
                    double t,
                    CardiacRhythms.Values cardRhythm,

                    // Advanced hemodynamics
                    int cvp,
                    int asbp, int adbp, int amap,
                    double co,
                    PulmonaryArteryRhythms.Values paPlacement,
                    int psp, int pdp, int pmp,
                    int icp, int iap,

                    // Cardiac profile
                    int pacerThreshold,
                    bool pulsParadoxus, bool pulsAlternans, bool elecAlternans,
                    Cardiac_Axes.Values cardAxis,
                    double qrsInt, 
                    double qtcInt,
                    double []? pAmp,
                    double []? qAmp,
                    double []? rAmp,
                    double []? sAmp,
                    double []? tAmp,
                    double []? stElev) {
            Updated = DateTime.UtcNow;

            // Basic vital signs
            VitalSignsSettings.HR = hr;
            VitalSignsSettings.NSBP = nsbp;
            VitalSignsSettings.NDBP = ndbp;
            VitalSignsSettings.NMAP = nmap;
            VitalSignsSettings.SPO2 = spo2;
            VitalSignsSettings.T = t;

            // Change in cardiac or respiratory rhythm? Reset all buffer counters and switches
            if (CardiacRhythm.Value != cardRhythm) {
                _counterCardiacAberrancy = 0;
                _counterCardiacArrhythmia = 0;
                _switchCardiacArrhythmia = false;
                CardiacRhythm.AberrantBeat = false;
                CardiacRhythm.AlternansBeat = false;
            }

            CardiacRhythm.Value = cardRhythm;

            // Advanced hemodynamics
            VitalSignsSettings.CVP = cvp;
            VitalSignsSettings.ASBP = asbp;
            VitalSignsSettings.ADBP = adbp;
            VitalSignsSettings.AMAP = amap;

            VitalSignsSettings.CO = co;

            PulmonaryArteryPlacement.Value = paPlacement;

            VitalSignsSettings.PSP = psp;
            VitalSignsSettings.PDP = pdp;
            VitalSignsSettings.PMP = pmp;
            VitalSignsSettings.ICP = icp;
            VitalSignsSettings.IAP = iap;

            // Cardiac profile
            PacemakerThreshold = pacerThreshold;

            // Reset buffers and switches for pulsus paradoxus
            _switchParadoxus = false;
            PulsusParadoxus = pulsParadoxus;
            PulsusAlternans = pulsAlternans;
            ElectricalAlternans = elecAlternans;

            QRSInterval = qrsInt;
            QTcInterval = qtcInt;

            CardiacAxis.Value = cardAxis;
            PAmplitude = pAmp;
            QAmplitude = qAmp;
            RAmplitude = rAmp;
            SAmplitude = sAmp;
            TAmplitude = tAmp;
            STElevation = stElev;

            // Reset actual vital signs to set parameters
            await VitalSignsActual.Set (VitalSignsSettings);

            await OnCardiac_Baseline ();
            await OnPhysiologyEvent (PhysiologyEventTypes.VitalsChange, _timerSimulation);
        }

        public async Task UpdateParameters_Respiratory (
                    int rr,
                    Respiratory_Rhythms.Values respRhythm,
                    int etco2,
                    bool mechVent,
                    double respIerI, double respIerE) {
            Updated = DateTime.UtcNow;

            VitalSignsSettings.RR = rr;

            // Change in cardiac or respiratory rhythm? Reset all buffer counters and switches

            if (RespiratoryRhythm.Value != respRhythm) {
                RespirationInflated = false;
                _counterRespiratoryArrhythmia = 0;
                _switchRespiratoryArrhythmia = false;
            }

            RespiratoryRhythm.Value = respRhythm;

            // Advanced hemodynamics
            VitalSignsSettings.ETCO2 = etco2;

            // Respiratory profile
            MechanicallyVentilated = mechVent;
            VitalSignsSettings.RRIEI = respIerI;
            VitalSignsSettings.RRIEE = respIerE;

            // Reset actual vital signs to set parameters
            await VitalSignsActual.Set (VitalSignsSettings);

            await OnRespiratory_Baseline ();
            await OnPhysiologyEvent (PhysiologyEventTypes.VitalsChange, _timerSimulation);
        }

        public async Task UpdateParameters_Obstetric (
                    int fhr, int fhrVar, FetalHeart_Rhythms.Values fhrRhythm,
                    int ucFreq, int ucDuration, int ucIntensity, int ucResting) {
            Updated = DateTime.UtcNow;

            // Obstetric profile
            VitalSignsSettings.FetalHR = fhr;
            ObstetricFetalRateVariability = fhrVar;
            ObstetricFetalHeartRhythm.Value = fhrRhythm;
            ObstetricContractionFrequency = ucFreq;
            ObstetricContractionDuration = ucDuration;
            ObstetricContractionIntensity = ucIntensity;
            ObstetricUterineRestingTone = ucResting;

            // Reset actual vital signs to set parameters
            await VitalSignsActual.Set (VitalSignsSettings);

            await OnObstetric_Baseline ();
            await OnPhysiologyEvent (PhysiologyEventTypes.VitalsChange, _timerSimulation);
        }

        public async Task ClampVitals (
                    int hrMin, int hrMax,
                    int spo2Min, int spo2Max,
                    int etco2Min, int etco2Max,
                    int sbpMin, int sbpMax, int dbpMin, int dbpMax,
                    int pspMin, int pspMax, int pdpMin, int pdpMax) {
            VitalSignsSettings.HR = Math.Clamp (VitalSignsSettings.HR, hrMin, hrMax);
            VitalSignsSettings.SPO2 = Math.Clamp (VitalSignsSettings.SPO2, spo2Min, spo2Max);
            VitalSignsSettings.ETCO2 = Math.Clamp (VitalSignsSettings.ETCO2, etco2Min, etco2Max);
            VitalSignsSettings.NSBP = Math.Clamp (VitalSignsSettings.NSBP, sbpMin, sbpMax);
            VitalSignsSettings.NDBP = Math.Clamp (VitalSignsSettings.NDBP, dbpMin, dbpMax);
            VitalSignsSettings.NMAP = CalculateMAP (VitalSignsSettings.NSBP, VitalSignsSettings.NDBP);
            VitalSignsSettings.ASBP = Math.Clamp (VitalSignsSettings.ASBP, sbpMin, sbpMax);
            VitalSignsSettings.ADBP = Math.Clamp (VitalSignsSettings.ADBP, sbpMin, sbpMax);
            VitalSignsSettings.AMAP = CalculateMAP (VitalSignsSettings.ASBP, VitalSignsSettings.ADBP);
            VitalSignsSettings.PSP = Math.Clamp (VitalSignsSettings.PSP, pspMin, pspMax);
            VitalSignsSettings.PDP = Math.Clamp (VitalSignsSettings.PDP, pdpMin, pdpMax);
            VitalSignsSettings.PMP = CalculateMAP (VitalSignsSettings.PSP, VitalSignsSettings.PDP);

            await VitalSignsActual.Set (VitalSignsSettings);

            _switchParadoxus = false;

            await ResetStartTimers ();
            await OnCardiac_Baseline ();
            await OnRespiratory_Baseline ();
            await OnObstetric_Baseline ();

            await OnPhysiologyEvent (PhysiologyEventTypes.VitalsChange, _timerSimulation);
        }

        private Task InitTimers () {
            _timerCardiacBaseline.Tick += async delegate { await OnCardiac_Baseline (); };
            _timerCardiacAtrialElectric.Tick += async delegate { await OnCardiac_Atrial_Electric (); };
            _timerCardiacVentricularElectric.Tick += async delegate { await OnCardiac_Ventricular_Electric (); };
            _timerCardiacAtrialMechanical.Tick += async delegate { await OnCardiac_Atrial_Mechanical (); };
            _timerCardiacVentricularMechanical.Tick += async delegate { await OnCardiac_Ventricular_Mechanical (); };
            _timerIABPBalloonTrigger.Tick += async delegate { await OnIABP_Balloon_Inflate (); };
            _timerDefibrillation.Tick += async delegate { await OnDefibrillation_End (); };
            _timerPacemakerBaseline.Tick += async delegate { await OnPacemakerBaseline (); };
            _timerPacemakerSpike.Tick += async delegate { await OnPacemaker_Spike (); };

            _timerRespiratoryBaseline.Tick += async delegate { await OnRespiratory_Baseline (); };
            _timerRespiratoryInspiration.Tick += async delegate { await OnRespiratory_Inspiration (); };
            _timerRespiratoryExpiration.Tick += async delegate { await OnRespiratory_Expiration (); };

            _timerObstetricBaseline.Tick += async delegate { await OnObstetric_Baseline (); };
            _timerObstetricFetalBaseline.Tick += async delegate { await OnObstetric_Fetal_Baseline (); };
            _timerObstetricFetalVariability.Tick += async delegate { await OnObstetric_Fetal_Variability (); };
            _timerObstetricContraction.Tick += async delegate { await OnObstetric_Contraction (); };

            return Task.CompletedTask;
        }

        private async Task ResetStartTimers () {
            await ResetStartTimers_Cardiac ();
            await ResetStartTimers_Respiratory ();
            await ResetStartTimers_Obstetric ();
        }

        private async Task ResetStartTimers_Cardiac () {
            await _timerCardiacBaseline.ResetStart ((int)(GetHRInterval * 1000d));
            await _timerCardiacAtrialElectric.Stop ();
            await _timerCardiacVentricularElectric.Stop ();
            await _timerCardiacAtrialMechanical.Stop ();
            await _timerCardiacVentricularMechanical.Stop ();
            await _timerIABPBalloonTrigger.Stop ();

            await _timerDefibrillation.ResetStart ();
            if (_timerPacemakerBaseline.IsRunning)
                await _timerPacemakerBaseline.ResetStart ();
            else
                await _timerPacemakerBaseline.Reset ();
            await _timerPacemakerSpike.Stop ();
        }

        private async Task ResetStartTimers_Respiratory () {
            await _timerRespiratoryBaseline.ResetStart ((int)(GetRRInterval * 1000d));
            await _timerRespiratoryInspiration.Stop ();
            await _timerRespiratoryExpiration.Stop ();
        }

        private async Task ResetStartTimers_Obstetric () {
            await _timerObstetricBaseline.ResetStart (1);
            await _timerObstetricFetalBaseline.ResetStart (1);

            await _timerObstetricFetalVariability.Stop ();
            await _timerObstetricContraction.Stop ();
        }

        private async Task StopTimers () {
            await StopTimers_Cardiac ();
            await StopTimers_Respiratory ();
            await StopTimers_Obstetric ();
        }

        private async Task StopTimers_Cardiac () {
            await _timerCardiacBaseline.Stop ();
            await _timerCardiacAtrialElectric.Stop ();
            await _timerCardiacVentricularElectric.Stop ();
            await _timerCardiacAtrialMechanical.Stop ();
            await _timerCardiacVentricularMechanical.Stop ();
            await _timerIABPBalloonTrigger.Stop ();
            await _timerDefibrillation.Stop ();
            await _timerPacemakerBaseline.Stop ();
            await _timerPacemakerSpike.Stop ();
        }

        private async Task StopTimers_Respiratory () {
            await _timerRespiratoryBaseline.Stop ();
            await _timerRespiratoryInspiration.Stop ();
            await _timerRespiratoryExpiration.Stop ();
        }

        private async Task StopTimers_Obstetric () {
            await _timerObstetricBaseline.Stop ();
            await _timerObstetricFetalBaseline.Stop ();
            await _timerObstetricFetalVariability.Stop ();
            await _timerObstetricContraction.Stop ();
        }

        public async Task SetTimerMultiplier_Obstetric (int multiplier) {
            TimerObstetricMultiplier = multiplier;
            await OnObstetric_RunTimers ();
        }

        public async Task Defibrillate ()
            => await InitDefibrillation (false);

        public async Task Cardiovert ()
            => await InitDefibrillation (true);

        public async Task Pacemaker (bool active, int rate, int energy) {
            PacemakerRate = rate;
            PacemakerEnergy = energy;

            // If rate == 0, must stop timer! Otherwise timer.Interval is set to 0!
            if (!active || rate == 0 || energy == 0)
                await StopPacemaker ();
            else if (active)
                await StartPacemaker ();
        }

        public async Task PacemakerPause ()
            => await _timerPacemakerBaseline.Set (4000);

        private async Task InitDefibrillation (bool toSynchronize) {
            if (toSynchronize)
                _timerCardiacVentricularElectric.Tick += OnCardioversion;
            else
                await OnDefibrillation ();
        }

        public bool PacemakerHasCapture {
            get {
                return _timerPacemakerBaseline.IsRunning
                    && PacemakerEnergy > 0 && PacemakerRate > 0
                    && PacemakerEnergy >= PacemakerThreshold;
            }
        }

        private async Task StartPacemaker ()
            => await _timerPacemakerBaseline.ResetStart ((int)((60d / PacemakerRate) * 1000));

        private async Task StopPacemaker () {
            await _timerPacemakerBaseline.Stop ();
            await _timerPacemakerSpike.Stop ();
        }

        private async Task OnDefibrillation () {
            await _timerCardiacBaseline.Stop ();
            await _timerCardiacAtrialElectric.Stop ();
            await _timerCardiacVentricularElectric.Stop ();
            await _timerCardiacAtrialMechanical.Stop ();
            await _timerCardiacVentricularMechanical.Stop ();
            await _timerDefibrillation.ResetStart (20);

            // Invoke the defibrillation event *after* starting the timer- IsDefibrillating() checks the timer!
            await OnPhysiologyEvent (PhysiologyEventTypes.Defibrillation, _timerSimulation);
        }

        private async Task OnDefibrillation_End () {
            await _timerDefibrillation.Stop ();
            await _timerCardiacBaseline.ResetStart ();
        }

        private void OnCardioversion (object? sender, EventArgs e) {
            _timerCardiacVentricularElectric.Tick -= OnCardioversion;
            Task.WaitAny(OnDefibrillation ());
        }

        private async Task OnPacemakerBaseline () {
            if (PacemakerEnergy > 0)
                await OnPhysiologyEvent (PhysiologyEventTypes.PacermakerSpike, _timerSimulation);

            if (PacemakerEnergy >= PacemakerThreshold)
                await _timerPacemakerSpike.ResetStart (40);          // Adds an interval between the spike and the QRS complex

            await StartPacemaker ();                                // In case pacemaker was paused... updates .Interval
        }

        private async Task OnPacemaker_Spike () {
            await _timerPacemakerSpike.Stop ();

            // Trigger the QRS complex, then reset the heart's intrinsic timers
            if (CardiacRhythm.CanPace) {
                CardiacRhythm.AberrantBeat = true;
                await OnCardiac_Ventricular_Electric ();
                CardiacRhythm.AberrantBeat = false;

                await OnPhysiologyEvent (PhysiologyEventTypes.CardiacBaseline, _timerSimulation);  // Triggers drawing isoelectric lines (important for a-fib/flutter)
                await _timerCardiacBaseline.ResetStart ();                   // Resets heart's intrinsic timer, allows pacemaker overdrive modeling
            }
        }

        private async Task OnCardiac_Baseline () {
            await OnPhysiologyEvent (PhysiologyEventTypes.CardiacBaseline, _timerSimulation);
            await _timerCardiacBaseline.Set ((int)(GetHRInterval * 1000d));

            switch (CardiacRhythm.Value) {
                default:
                case CardiacRhythms.Values.Asystole:
                    break;

                // Traced as "regular V" Rhythms
                case CardiacRhythms.Values.AtrialFlutter:
                case CardiacRhythms.Values.CPRArtifact:
                case CardiacRhythms.Values.Junctional:
                case CardiacRhythms.Values.Idioventricular:
                case CardiacRhythms.Values.SupraventricularTachycardia:
                case CardiacRhythms.Values.VentricularTachycardiaMonomorphicPulsed:
                case CardiacRhythms.Values.VentricularTachycardiaMonomorphicPulseless:
                case CardiacRhythms.Values.VentricularTachycardiaPolymorphic:
                case CardiacRhythms.Values.VentricularFibrillationCoarse:
                case CardiacRhythms.Values.VentricularFibrillationFine:
                    await _timerCardiacVentricularElectric.ResetStart (1);
                    break;

                // Traced as "regular A" or "regular A -> V" Rhythms
                case CardiacRhythms.Values.AVBlock1Degree:
                case CardiacRhythms.Values.AVBlock2Mobitz:
                case CardiacRhythms.Values.AVBlock2Wenckebach:
                case CardiacRhythms.Values.BundleBranchBlock:
                case CardiacRhythms.Values.SinusRhythm:
                case CardiacRhythms.Values.PulselessElectricalActivity:
                case CardiacRhythms.Values.VentricularStandstill:
                    await _timerCardiacAtrialElectric.ResetStart (1);
                    break;

                // Traced as "irregular V" rhythms
                case CardiacRhythms.Values.AtrialFibrillation:
                    VitalSignsActual.HR = (int)(VitalSignsSettings.HR * Math.RandomDbl (0.6, 1.4));
                    await _timerCardiacVentricularElectric.ResetStart (1);
                    break;

                /* Special Cases */
                case CardiacRhythms.Values.AVBlock3Degree:
                    if (!_timerCardiacAtrialElectric.IsRunning)
                        await _timerCardiacAtrialElectric.ResetStart ((int)(_timerCardiacBaseline.Interval * 0.6d));
                    await _timerCardiacVentricularElectric.ResetStart (160);
                    break;

                case CardiacRhythms.Values.SickSinusSyndrome:
                    /* Countdown to 0; on 0, switch between tachy/brady; brady runs 8-12 beats, tachy runs 20-30 beats */
                    if (_counterCardiacArrhythmia <= 0) {
                        _switchCardiacArrhythmia = !_switchCardiacArrhythmia;
                        if (_switchCardiacArrhythmia) {
                            VitalSignsActual.HR = (int)(VitalSignsSettings.HR * 0.60);
                            _counterCardiacArrhythmia = new Random ().Next (8, 12);
                        } else {
                            VitalSignsActual.HR = (int)(VitalSignsSettings.HR * 1.8);
                            _counterCardiacArrhythmia = new Random ().Next (20, 30);
                        }
                    } else
                        _counterCardiacArrhythmia--;

                    await _timerCardiacAtrialElectric.ResetStart (1);
                    break;

                case CardiacRhythms.Values.SinusArrhythmia:
                    if (RespirationInflated)
                        VitalSignsActual.HR = (int)(VitalSignsSettings.HR * 1.075d);
                    else
                        VitalSignsActual.HR = (int)(VitalSignsSettings.HR * 0.925d);

                    await _timerCardiacAtrialElectric.ResetStart (1);
                    break;

                case CardiacRhythms.Values.SinusRhythmWithArrest:

                    // Every 10-25 beats, sinus arrest
                    if (_counterCardiacArrhythmia <= 0) {
                        Random r = new ();
                        _counterCardiacArrhythmia = r.Next (10, 16);
                        VitalSignsActual.HR = VitalSignsSettings.HR / 8;
                    } else {
                        VitalSignsActual.HR = VitalSignsSettings.HR;
                        _counterCardiacArrhythmia--;
                    }

                    await _timerCardiacAtrialElectric.ResetStart (1);
                    break;

                case CardiacRhythms.Values.SinusRhythmWithPACs:
                    _counterCardiacAberrancy -= 1;
                    VitalSignsActual.HR = VitalSignsSettings.HR;
                    if (_counterCardiacAberrancy <= 0) {
                        _counterCardiacAberrancy = new Random ().Next (4, 8);
                        VitalSignsActual.HR = (int)(VitalSignsSettings.HR * Math.RandomDbl (0.6d, 0.8d));
                    } else {
                        VitalSignsActual.HR = VitalSignsSettings.HR;
                    }
                    await _timerCardiacAtrialElectric.ResetStart (1);
                    break;

                case CardiacRhythms.Values.SinusRhythmWithPJCs:
                    _counterCardiacAberrancy -= 1;
                    VitalSignsActual.HR = VitalSignsSettings.HR;
                    if (_counterCardiacAberrancy <= 0) {
                        _counterCardiacAberrancy = new Random ().Next (4, 8);
                        await _timerCardiacVentricularElectric.ResetStart (1);
                    } else {
                        if (_counterCardiacAberrancy == 1)
                            VitalSignsActual.HR = (int)(VitalSignsSettings.HR * Math.RandomDbl (0.7d, 0.9d));
                        await _timerCardiacAtrialElectric.ResetStart (1);
                    }
                    break;

                case CardiacRhythms.Values.SinusRhythmWithBigeminy:
                case CardiacRhythms.Values.SinusRhythmWithTrigeminy:
                    _counterCardiacAberrancy -= 1;
                    if (_counterCardiacAberrancy == 0) {
                        await _timerCardiacBaseline.Set ((int)(_timerCardiacBaseline.Interval * 0.8d));
                    } else if (_counterCardiacAberrancy < 0) {   // Then throw the PVC and reset the counters
                        if (CardiacRhythm.Value == CardiacRhythms.Values.SinusRhythmWithBigeminy)
                            _counterCardiacAberrancy = 1;
                        else if (CardiacRhythm.Value == CardiacRhythms.Values.SinusRhythmWithTrigeminy)
                            _counterCardiacAberrancy = 2;
                        CardiacRhythm.AberrantBeat = true;
                        await _timerCardiacVentricularElectric.ResetStart (1);
                        break;
                    }
                    await _timerCardiacAtrialElectric.ResetStart (1);
                    CardiacRhythm.AberrantBeat = false;
                    break;

                case CardiacRhythms.Values.SinusRhythmWithPVCsUnifocal:
                case CardiacRhythms.Values.SinusRhythmWithPVCsMultifocal:
                    _counterCardiacAberrancy -= 1;
                    VitalSignsActual.HR = VitalSignsSettings.HR;
                    if (_counterCardiacAberrancy == 0) {  // Shorten the beat preceding the PVC, making it premature
                        VitalSignsActual.HR = (int)(VitalSignsSettings.HR * 0.8d);
                    } else if (_counterCardiacAberrancy < 0) {   // Then throw the PVC and reset the counters
                        _counterCardiacAberrancy = new Random ().Next (4, 9);
                        CardiacRhythm.AberrantBeat = true;
                        await _timerCardiacVentricularElectric.ResetStart (1);
                        break;
                    }
                    CardiacRhythm.AberrantBeat = false;
                    await _timerCardiacAtrialElectric.ResetStart (1);
                    break;
            }
        }

        private async Task OnCardiac_Atrial_Electric () {
            await OnPhysiologyEvent (PhysiologyEventTypes.CardiacAtrialElectric, _timerSimulation);

            if (CardiacRhythm.HasAtrialPulse)
                await _timerCardiacAtrialMechanical.ResetStart (DefaultElectromechanicalDelay);

            switch (CardiacRhythm.Value) {
                default:
                case CardiacRhythms.Values.Asystole:
                    break;

                // Regular A Rhythms
                case CardiacRhythms.Values.VentricularStandstill:
                    await _timerCardiacAtrialElectric.Stop ();
                    break;

                // Regular A -> V rhythms
                case CardiacRhythms.Values.BundleBranchBlock:
                case CardiacRhythms.Values.SickSinusSyndrome:
                case CardiacRhythms.Values.SinusArrhythmia:
                case CardiacRhythms.Values.SinusRhythm:
                case CardiacRhythms.Values.SinusRhythmWithArrest:
                case CardiacRhythms.Values.SinusRhythmWithPACs:
                case CardiacRhythms.Values.SinusRhythmWithPJCs:
                case CardiacRhythms.Values.SinusRhythmWithBigeminy:
                case CardiacRhythms.Values.SinusRhythmWithTrigeminy:
                case CardiacRhythms.Values.SinusRhythmWithPVCsUnifocal:
                case CardiacRhythms.Values.SinusRhythmWithPVCsMultifocal:
                case CardiacRhythms.Values.PulselessElectricalActivity:
                    await _timerCardiacAtrialElectric.Stop ();
                    await _timerCardiacVentricularElectric.ResetStart (160);
                    break;

                /* Special cases */

                case CardiacRhythms.Values.AVBlock1Degree:
                    await _timerCardiacAtrialElectric.Stop ();
                    await _timerCardiacVentricularElectric.ResetStart (240);
                    break;

                case CardiacRhythms.Values.AVBlock2Mobitz:
                    await _timerCardiacAtrialElectric.Stop ();
                    _counterCardiacAberrancy -= 1;
                    if (_counterCardiacAberrancy < 0) {
                        _counterCardiacAberrancy = 2;
                        CardiacRhythm.AberrantBeat = true;
                    } else {
                        await _timerCardiacVentricularElectric.ResetStart (160);
                        CardiacRhythm.AberrantBeat = false;
                    }
                    break;

                case CardiacRhythms.Values.AVBlock2Wenckebach:
                    await _timerCardiacAtrialElectric.Stop ();
                    _counterCardiacAberrancy -= 1;
                    if (_counterCardiacAberrancy < 0) {
                        _counterCardiacAberrancy = 3;
                        CardiacRhythm.AberrantBeat = true;
                    } else {
                        await _timerCardiacBaseline.Set ((int)(_timerCardiacBaseline.Interval + (160d * (3d - _counterCardiacAberrancy))));
                        await _timerCardiacVentricularElectric.ResetStart ((int)(160d * (3d - _counterCardiacAberrancy)));
                        CardiacRhythm.AberrantBeat = false;
                    }
                    break;

                case CardiacRhythms.Values.AVBlock3Degree:
                    /* Specifically let atrial timer continue to run and propogate P-waves! */
                    break;
            }
        }

        private async Task OnCardiac_Ventricular_Electric () {
            await OnPhysiologyEvent (PhysiologyEventTypes.CardiacVentricularElectric, _timerSimulation);

            if (IABPActive && IABPTrigger == IABPTriggers.ECG) {
                IABPDelayDynamic = (int)((GetHRInterval * 1000) * .34);
                await _timerIABPBalloonTrigger.ResetStart (DefaultElectromechanicalDelay + IABPDelayDynamic + IABPDelayManual);
            }

            if (CardiacRhythm.HasVentricularPulse)
                await _timerCardiacVentricularMechanical.ResetStart (DefaultElectromechanicalDelay);

            /* Flip the switch on pulsus alternans or electrical alternans */
            CardiacRhythm.AlternansBeat = (PulsusAlternans || ElectricalAlternans) && !CardiacRhythm.AlternansBeat;

            await _timerCardiacVentricularElectric.Stop ();
        }

        private async Task OnCardiac_Atrial_Mechanical () {
            await OnPhysiologyEvent (PhysiologyEventTypes.CardiacAtrialMechanical, _timerSimulation);

            await _timerCardiacAtrialMechanical.Stop ();
        }

        private async Task OnCardiac_Ventricular_Mechanical () {
            await OnPhysiologyEvent (PhysiologyEventTypes.CardiacVentricularMechanical, _timerSimulation);

            if (IABPActive && IABPTrigger == IABPTriggers.Pressure) {
                IABPDelayDynamic = (int)((GetHRInterval * 1000) * .34);
                await _timerIABPBalloonTrigger.ResetStart (IABPDelayDynamic + IABPDelayManual);
            }

            if (PulsusAlternans) {
                VitalSignsActual.ASBP += CardiacRhythm.AlternansBeat ? (int)(VitalSignsSettings.ASBP * 0.15d) : -(int)(VitalSignsSettings.ASBP * 0.15d);
                VitalSignsActual.ADBP += CardiacRhythm.AlternansBeat ? (int)(VitalSignsSettings.ADBP * 0.15d) : -(int)(VitalSignsSettings.ADBP * 0.15d);
                VitalSignsActual.AMAP += CardiacRhythm.AlternansBeat ? (int)(VitalSignsSettings.AMAP * 0.15d) : -(int)(VitalSignsSettings.AMAP * 0.15d);
                IABPAP += CardiacRhythm.AlternansBeat ? -(int)(VitalSignsSettings.ASBP * 0.05d) : (int)(VitalSignsSettings.ASBP * 0.05d);

                VitalSignsActual.CVP += CardiacRhythm.AlternansBeat ? -(int)(VitalSignsSettings.CVP * 0.15d) : (int)(VitalSignsSettings.CVP * 0.15d);
                VitalSignsActual.PSP += CardiacRhythm.AlternansBeat ? (int)(VitalSignsSettings.PSP * 0.15d) : -(int)(VitalSignsSettings.PSP * 0.15d);
                VitalSignsActual.PDP += CardiacRhythm.AlternansBeat ? (int)(VitalSignsSettings.PDP * 0.15d) : -(int)(VitalSignsSettings.PDP * 0.15d);
                VitalSignsActual.PMP += CardiacRhythm.AlternansBeat ? (int)(VitalSignsSettings.PMP * 0.15d) : -(int)(VitalSignsSettings.PMP * 0.15d);
            }

            await _timerCardiacVentricularMechanical.Stop ();
        }

        private async Task OnIABP_Balloon_Inflate () {
            await OnPhysiologyEvent (PhysiologyEventTypes.IABPBalloonInflation, _timerSimulation);

            await _timerIABPBalloonTrigger.Stop ();
        }

        private async Task OnRespiratory_Baseline () {
            await OnPhysiologyEvent (PhysiologyEventTypes.RespiratoryBaseline, _timerSimulation);
            await _timerRespiratoryBaseline.Set ((int)(GetRRInterval * 1000d));

            double c;

            switch (RespiratoryRhythm.Value) {
                default:
                case Respiratory_Rhythms.Values.Apnea:
                    return;

                case Respiratory_Rhythms.Values.Agonal:
                    c = Math.RandomDbl (0.8d, 1.2d);
                    VitalSignsActual.RR = (int)(c * VitalSignsSettings.RR);
                    break;

                case Respiratory_Rhythms.Values.Apneustic:
                    VitalSignsActual.RR = (Math.RandomDbl (0, 1d) < 0.1d) ? 6 : VitalSignsSettings.RR;
                    break;

                case Respiratory_Rhythms.Values.Ataxic:
                    if (Math.RandomDbl (0, 1) < 0.1)
                        VitalSignsActual.RR = 4;
                    else {
                        c = Math.RandomDbl (0.8d, 1.2d);
                        VitalSignsActual.RR = (int)(c * VitalSignsSettings.RR);
                        VitalSignsActual.RRIEE = (int)(c * VitalSignsSettings.RRIEE);
                    }
                    break;

                case Respiratory_Rhythms.Values.Biot:
                    if (_counterRespiratoryArrhythmia < 0) {
                        VitalSignsActual.RR = 3;                               // Period of apnea, 20 sec
                        _counterRespiratoryArrhythmia = (int)(VitalSignsSettings.RR * 0.75d);   // Counter for ~45 seconds of regular rate
                    } else {
                        VitalSignsActual.RR = VitalSignsSettings.RR;                  // Regular breathing
                        _counterRespiratoryArrhythmia -= 1;
                    }
                    break;

                case Respiratory_Rhythms.Values.CheyneStokes:
                    if (!_switchRespiratoryArrhythmia && _counterRespiratoryArrhythmia <= 10) {
                        VitalSignsActual.RR += 2;                              // Ramp up breath rate
                        if (_counterRespiratoryArrhythmia == 10)        // Flip the switch when ramped up entirely
                            _switchRespiratoryArrhythmia = true;
                        else {
                            if (_counterRespiratoryArrhythmia == 0)
                                VitalSignsActual.RR = VitalSignsSettings.RR;
                            _counterRespiratoryArrhythmia += 1;
                        }
                    } else if (_switchRespiratoryArrhythmia && _counterRespiratoryArrhythmia > 0) {
                        VitalSignsActual.RR -= 2;                              // Ramp breaths down until counter is 0
                        _counterRespiratoryArrhythmia -= 1;
                    } else {
                        VitalSignsActual.RR = 3;                               // Apnea for 20 seconds
                        _switchRespiratoryArrhythmia = false;           // Reset switch and counter
                        _counterRespiratoryArrhythmia = 0;
                    }
                    break;

                case Respiratory_Rhythms.Values.Regular:
                    break;
            }

            await _timerRespiratoryInspiration.ResetStart (1);
        }

        private async Task OnRespiratory_Inspiration () {
            RespirationInflated = true;
            await OnPhysiologyEvent (PhysiologyEventTypes.RespiratoryInspiration, _timerSimulation);
            await _timerRespiratoryInspiration.Stop ();

            // Process pulsus paradoxus (numerical values) for inspiration here
            // Flip counterCardiac_Paradoxus when flipping SBP portion
            if (PulsusParadoxus && RespiratoryRhythm.Value != Respiratory_Rhythms.Values.Apnea
                && (MechanicallyVentilated || _switchParadoxus)) {
                _switchParadoxus = true;
                VitalSignsActual.ASBP += MechanicallyVentilated ? -(int)(VitalSignsSettings.ASBP * 0.15d) : (int)(VitalSignsSettings.ASBP * 0.15d);
                VitalSignsActual.ADBP += MechanicallyVentilated ? -(int)(VitalSignsSettings.ADBP * 0.15d) : (int)(VitalSignsSettings.ADBP * 0.15d);
                VitalSignsActual.AMAP += MechanicallyVentilated ? -(int)(VitalSignsSettings.AMAP * 0.15d) : (int)(VitalSignsSettings.AMAP * 0.15d);
                IABPAP += MechanicallyVentilated ? -(int)(VitalSignsSettings.ASBP * 0.05d) : (int)(VitalSignsSettings.ASBP * 0.05d);
            }

            // Process pressure responses to increased intrathoracic pressure
            VitalSignsActual.CVP += MechanicallyVentilated ? (int)(VitalSignsSettings.CVP * 0.1d) : -(int)(VitalSignsSettings.CVP * 0.1d);
            VitalSignsActual.PSP += MechanicallyVentilated ? -(int)(VitalSignsSettings.PSP * 0.1d) : (int)(VitalSignsSettings.PSP * 0.1d);
            VitalSignsActual.PDP += MechanicallyVentilated ? -(int)(VitalSignsSettings.PDP * 0.1d) : (int)(VitalSignsSettings.PDP * 0.1d);
            VitalSignsActual.PMP += MechanicallyVentilated ? -(int)(VitalSignsSettings.PMP * 0.1d) : (int)(VitalSignsSettings.PMP * 0.1d);

            switch (RespiratoryRhythm.Value) {
                default:
                case Respiratory_Rhythms.Values.Apnea:
                    break;

                case Respiratory_Rhythms.Values.Agonal:
                case Respiratory_Rhythms.Values.Apneustic:
                case Respiratory_Rhythms.Values.Ataxic:
                case Respiratory_Rhythms.Values.Biot:
                case Respiratory_Rhythms.Values.CheyneStokes:
                case Respiratory_Rhythms.Values.Regular:
                    await _timerRespiratoryExpiration.ResetStart ((int)(GetRRIntervalInspiratory * 1000d));     // Expiration.Interval marks end inspiration
                    break;
            }
        }

        private async Task OnRespiratory_Expiration () {
            RespirationInflated = false;
            await OnPhysiologyEvent (PhysiologyEventTypes.RespiratoryExpiration, _timerSimulation);
            await _timerRespiratoryExpiration.Stop ();

            // Process pulsus paradoxus (numerical values) for expiration here
            // Flip counterCardiac_Paradoxus when flipping SBP portion
            if (PulsusParadoxus && RespiratoryRhythm.Value != Respiratory_Rhythms.Values.Apnea
                && (!MechanicallyVentilated || _switchParadoxus)) {
                _switchParadoxus = true;
                VitalSignsActual.ASBP += MechanicallyVentilated ? (int)(VitalSignsSettings.ASBP * 0.15d) : -(int)(VitalSignsSettings.ASBP * 0.15d);
                VitalSignsActual.ADBP += MechanicallyVentilated ? (int)(VitalSignsSettings.ADBP * 0.15d) : -(int)(VitalSignsSettings.ADBP * 0.15d);
                VitalSignsActual.AMAP += MechanicallyVentilated ? (int)(VitalSignsSettings.AMAP * 0.15d) : -(int)(VitalSignsSettings.AMAP * 0.15d);
                IABPAP += MechanicallyVentilated ? (int)(VitalSignsSettings.ASBP * 0.05d) : -(int)(VitalSignsSettings.ASBP * 0.05d);
            }

            // Process pressure responses to increased intrathoracic pressure
            VitalSignsActual.CVP += MechanicallyVentilated ? -(int)(VitalSignsSettings.CVP * 0.1d) : (int)(VitalSignsSettings.CVP * 0.1d);
            VitalSignsActual.PSP += MechanicallyVentilated ? (int)(VitalSignsSettings.PSP * 0.1d) : -(int)(VitalSignsSettings.PSP * 0.1d);
            VitalSignsActual.PDP += MechanicallyVentilated ? (int)(VitalSignsSettings.PDP * 0.1d) : -(int)(VitalSignsSettings.PDP * 0.1d);
            VitalSignsActual.PMP += MechanicallyVentilated ? (int)(VitalSignsSettings.PMP * 0.1d) : -(int)(VitalSignsSettings.PMP * 0.1d);
        }

        private async Task OnObstetric_Baseline () {
            await OnPhysiologyEvent (PhysiologyEventTypes.ObstetricBaseline, _timerSimulation);
            await _timerObstetricBaseline.ResetStart (1000);

            await OnObstetric_RunTimers ();
        }

        private async Task OnObstetric_Fetal_Baseline () {
            await OnPhysiologyEvent (PhysiologyEventTypes.ObstetricFetalBaseline, _timerSimulation);

            // Must be divided by the TimerObstetric_Multiplier or else the speed multiplier effects the actual waveform drawing
            await _timerObstetricFetalBaseline.ResetStart (2500 / TimerObstetricMultiplier);

            /* Baseline adjusting/walking of the vitals/waveforms *****
             */
            int fvar = Math.RandomInt (1, ObstetricFetalRateVariability / 2);

            switch (_stateFetalHeartRhythm) {
                case FetalHeart_Rhythms.States.Interval:
                case FetalHeart_Rhythms.States.Refractory:
                    // Add baseline variability to the fetal heart rate, based on the variability parameter, using a random "walk"
                    // If the fetal heart rate "walks" out of its set min/max for variability, bounce it towarsd "center"
                    if (VitalSignsActual.FetalHR > VitalSignsSettings.FetalHR + (ObstetricFetalRateVariability / 2))
                        VitalSignsActual.FetalHR = System.Math.Max (0, VitalSignsActual.FetalHR - fvar);
                    else if (VitalSignsActual.FetalHR < VitalSignsSettings.FetalHR - (ObstetricFetalRateVariability / 2))
                        VitalSignsActual.FetalHR = System.Math.Max (0, VitalSignsActual.FetalHR + fvar);
                    else
                        VitalSignsActual.FetalHR = System.Math.Max (0, VitalSignsActual.FetalHR + (int)Math.RandomDbl (-fvar, fvar));
                    break;

                case FetalHeart_Rhythms.States.Accelerating:
                    // Walk the heart rate upwards towards a normal acceleration of >15 bpm increase
                    if (VitalSignsActual.FetalHR < VitalSignsSettings.FetalHR + Math.RandomInt (15, 30))
                        VitalSignsActual.FetalHR = System.Math.Max (0, VitalSignsActual.FetalHR + _bufferFetalHeartRhythmWander);
                    else  // While at "goal", can walk randomly
                        VitalSignsActual.FetalHR = System.Math.Max (0, VitalSignsActual.FetalHR + (int)Math.RandomDbl (-fvar, fvar));

                    break;

                case FetalHeart_Rhythms.States.Decelerating:
                    // Walk the heart rate downwards towards a normal deceleration of >15 bpm increase
                    if (VitalSignsActual.FetalHR > System.Math.Max (0, VitalSignsSettings.FetalHR - Math.RandomInt (15, 30)))
                        VitalSignsActual.FetalHR = System.Math.Max (0, VitalSignsActual.FetalHR - _bufferFetalHeartRhythmWander);
                    else  // While at "goal", can walk randomly
                        VitalSignsActual.FetalHR = System.Math.Max (0, VitalSignsActual.FetalHR + (int)Math.RandomDbl (-fvar, fvar));
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

            if (_stateFetalHeartRhythm == FetalHeart_Rhythms.States.Interval) {
                switch (ObstetricFetalHeartRhythm.Value) {
                    case FetalHeart_Rhythms.Values.Acceleration:                // Accelerations
                        _stateFetalHeartRhythm = FetalHeart_Rhythms.States.Accelerating;
                        _bufferFetalHeartRhythmWander = Math.RandomInt (2, 5);
                        await _timerObstetricFetalVariability.ResetStart (Math.RandomInt (15, 120) * 1000 / TimerObstetricMultiplier);
                        break;

                    case FetalHeart_Rhythms.Values.DecelerationVariable:        // Variable Deceleration
                        _stateFetalHeartRhythm = FetalHeart_Rhythms.States.Decelerating;
                        _bufferFetalHeartRhythmWander = Math.RandomInt (2, 5);
                        await _timerObstetricFetalVariability.ResetStart (Math.RandomInt (15, 120) * 1000 / TimerObstetricMultiplier);
                        break;
                }
            }
        }

        private async Task OnObstetric_Fetal_Variability () {
            switch (ObstetricFetalHeartRhythm.Value) {
                default:
                case FetalHeart_Rhythms.Values.Baseline:
                    // This is the "fall-through" location if the user makes a change that needs to stop the variability cycle
                    _stateFetalHeartRhythm = FetalHeart_Rhythms.States.Interval;
                    await _timerObstetricFetalVariability.Stop ();
                    break;

                case FetalHeart_Rhythms.Values.Acceleration:
                case FetalHeart_Rhythms.Values.DecelerationVariable:
                    // Variability triggered w/ refractory lockout mechanism
                    switch (_stateFetalHeartRhythm) {
                        case FetalHeart_Rhythms.States.Accelerating:
                        case FetalHeart_Rhythms.States.Decelerating:

                            // Entering a refractory period to prevent repeated triggering of variability
                            _stateFetalHeartRhythm = FetalHeart_Rhythms.States.Refractory;
                            await _timerObstetricFetalVariability.ResetStart (Math.RandomInt (60, 300) * 1000 / TimerObstetricMultiplier);
                            break;

                        case FetalHeart_Rhythms.States.Refractory:
                            _stateFetalHeartRhythm = FetalHeart_Rhythms.States.Interval;
                            await _timerObstetricFetalVariability.Stop ();
                            break;
                    }
                    break;

                case FetalHeart_Rhythms.Values.DecelerationEarly:
                    // Variability triggered by contractions- skip the refractory lockout mechanism
                    _stateFetalHeartRhythm = FetalHeart_Rhythms.States.Interval;
                    await _timerObstetricFetalVariability.Stop ();
                    break;

                case FetalHeart_Rhythms.Values.DecelerationLate:
                    switch (_stateFetalHeartRhythm) {
                        default:
                        case FetalHeart_Rhythms.States.Interval:
                            // Start the late deceleration, reset the timer for the second half
                            _stateFetalHeartRhythm = FetalHeart_Rhythms.States.Decelerating;
                            _bufferFetalHeartRhythmWander = Math.RandomInt (2, 5);
                            await _timerObstetricFetalVariability.ResetStart ((int)((ObstetricContractionDuration / 5d * 3d) * 1000d / TimerObstetricMultiplier));
                            break;

                        case FetalHeart_Rhythms.States.Decelerating:
                            // Ending of the late deceleration, return to baseline
                            _stateFetalHeartRhythm = FetalHeart_Rhythms.States.Interval;
                            await _timerObstetricFetalVariability.Stop ();
                            break;
                    }
                    break;
            }
        }

        private async Task OnObstetric_RunTimers () {
            if (!InLabor) {
                _flagObstetricContraction = false;
                await _timerObstetricContraction.Stop ();
            } else if (InLabor) {
                if (!_flagObstetricContraction)
                    await _timerObstetricContraction.Continue ((int)(ObstetricContractionFrequency * 1000d / TimerObstetricMultiplier));
                else if (_flagObstetricContraction)
                    await _timerObstetricContraction.Continue ((int)(ObstetricContractionDuration * 1000d / TimerObstetricMultiplier));
            }
        }

        private async Task OnObstetric_Contraction () {
            if (!_flagObstetricContraction) {
                // Contraction onset
                _flagObstetricContraction = true;
                await _timerObstetricContraction.ResetStart ((int)(ObstetricContractionDuration * 1000d / TimerObstetricMultiplier));

                await OnPhysiologyEvent (PhysiologyEventTypes.ObstetricContractionStart, _timerSimulation);

                // No need for triggering lockouts (e.g. Refractory state) when triggering based on contractions
                switch (ObstetricFetalHeartRhythm.Value) {
                    case FetalHeart_Rhythms.Values.DecelerationEarly:
                        _stateFetalHeartRhythm = FetalHeart_Rhythms.States.Decelerating;
                        _bufferFetalHeartRhythmWander = Math.RandomInt (2, 5);
                        await _timerObstetricFetalVariability.ResetStart ((int)((ObstetricContractionDuration / 5d * 3d) * 1000d / TimerObstetricMultiplier));
                        break;

                    case FetalHeart_Rhythms.Values.DecelerationLate:
                        // Use the variability timer to wait out 2/3 the contraction, then trigger the late deceleration
                        _stateFetalHeartRhythm = FetalHeart_Rhythms.States.Interval;
                        await _timerObstetricFetalVariability.ResetStart ((int)((ObstetricContractionDuration / 5d * 3) * 1000d / TimerObstetricMultiplier));
                        break;
                }
            } else {
                // Contraction ending
                _flagObstetricContraction = false;
                await _timerObstetricContraction.ResetStart ((int)(ObstetricContractionFrequency * 1000d / TimerObstetricMultiplier));

                await OnPhysiologyEvent (PhysiologyEventTypes.ObstetricContractionEnd, _timerSimulation);
            }
        }
    }
}