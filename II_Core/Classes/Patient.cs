using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace II {
    public class Patient {
        // Mirroring variables
        public DateTime Updated;                    // DateTime this Patient was last updated

        /* Parameters for patient simulation, e.g. vital signs */
        // Vital Signs
        public int HR, RR, ETCO2, SPO2,            // Heart rate, respiratory rate, end-tidal capnography, pulse oximetry
                    CVP,                            // Central venous pressure,
                    NSBP, NDBP, NMAP,               // Non-invasive blood pressures
                    ASBP, ADBP, AMAP,               // Arterial line blood pressures
                    PSP, PDP, PMP,                  // Pulmonary artery pressures
                    ICP, IAP,                       // Intracranial pressure, intra-abdominal pressure
                    IABP_AP, IABP_DBP, IABP_MAP;    // Intra-aortic balloon pump blood pressures
        public double T;                            // Temperature

        public bool IABP_Active = false;            // Is the Device_IABP currently augmenting?
        public string IABP_Trigger;                 // Device_IABP's trigger; data backflow for strip processing

        public int Pacemaker_Rate,                  // DeviceDefib's transcutaneous pacemaker rate
                    Pacemaker_Energy,               // DeviceDefib's pacemaker energy delivery amount
                    Pacemaker_Threshold;            // Patient's threshold for electrical capture to pacemaker spike

        // Cardiac Profile
        public double [] ST_Elevation, T_Elevation;
        public Cardiac_Rhythms Cardiac_Rhythm = new Cardiac_Rhythms ();
        public CardiacAxes Cardiac_Axis = new CardiacAxes ();
        public bool Pulsus_Paradoxus = false,
                    Pulsus_Alternans = false;

        // Respiratory Profile
        public Respiratory_Rhythms Respiratory_Rhythm = new Respiratory_Rhythms ();
        public bool Respiration_Inflated = false,
                    Mechanically_Ventilated = false;
        public float Respiratory_IERatio_I = 1f,
                    Respiratory_IERatio_E = 2f;

        // General Device Settings
        public bool TransducerZeroed_CVP = false,
                    TransducerZeroed_ABP = false,
                    TransducerZeroed_PA = false,
                    TransducerZeroed_ICP = false,
                    TransducerZeroed_IAP = false;

        // Obstetric Profile
        public Intensity UC_Intensity = new Intensity (),
                         FHR_Variability = new Intensity ();
        public int UC_Frequency, UC_Duration, FHR;
        public FetalHeartDecelerations FHR_Decelerations = new FetalHeartDecelerations ();

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

        /* Properties, Counters, Handlers, Timers, etc ... Programmatic Stuff */
        public double HR_Seconds { get { return 60d / Math.Max (1, HR); } }
        public double RR_Seconds { get { return 60d / Math.Max (1, RR); } }
        public double RR_Seconds_I { get { return (RR_Seconds / (Respiratory_IERatio_I + Respiratory_IERatio_E)) * Respiratory_IERatio_I; } }
        public double RR_Seconds_E { get { return (RR_Seconds / (Respiratory_IERatio_I + Respiratory_IERatio_E)) * Respiratory_IERatio_E; } }

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
        private int counterCardiac_Aberrancy = 0;
        private int bufferParadoxus_BaseASBP = 0;
        private bool switchParadoxus = false;

        public static int CalculateMAP (int sbp, int dbp) {
            return dbp + ((sbp - dbp) / 3);
        }

        public static int CalculateCPP (int icp, int map) {
            return map - icp;
        }

        public event EventHandler<PatientEvent_Args> PatientEvent;
        public class PatientEvent_Args : EventArgs {
            public Patient Patient;
            public EventTypes EventType;

            public PatientEvent_Args (Patient p, EventTypes e) {
                Patient = p;
                EventType = e;
            }

            public enum EventTypes {
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
        }

        public Patient () {
            UpdateParameters (80, 98, 18, 40,
                            38.0f, 6,
                            120, 80, 95,
                            120, 80, 95,
                            22, 12, 16,
                            8, 1, 50,
                            false, false,
                            new double [] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f },
                            new double [] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f },
                            Cardiac_Rhythms.Values.Sinus_Rhythm,
                            CardiacAxes.Values.Normal,
                            Respiratory_Rhythms.Values.Regular,
                            1f, 1f, false,
                            150, Intensity.Values.Absent, new List<FetalHeartDecelerations.Values> (),
                            60, 30, Intensity.Values.Moderate);

            InitTimers ();
            SetTimers ();
        }

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
                            case "Updated": Updated = Utility.DateTime_FromString (pValue); break;
                            case "HR": HR = int.Parse (pValue); break;
                            case "SPO2": SPO2 = int.Parse (pValue); break;
                            case "RR": RR = int.Parse (pValue); break;
                            case "ETCO2": ETCO2 = int.Parse (pValue); break;
                            case "CVP": CVP = int.Parse (pValue); break;
                            case "NSBP": NSBP = int.Parse (pValue); break;
                            case "NDBP": NDBP = int.Parse (pValue); break;
                            case "NMAP": NMAP = int.Parse (pValue); break;
                            case "ASBP": ASBP = int.Parse (pValue); break;
                            case "ADBP": ADBP = int.Parse (pValue); break;
                            case "AMAP": AMAP = int.Parse (pValue); break;
                            case "PSP": PSP = int.Parse (pValue); break;
                            case "PDP": PDP = int.Parse (pValue); break;
                            case "PMP": PMP = int.Parse (pValue); break;
                            case "ICP": ICP = int.Parse (pValue); break;
                            case "IAP": IAP = int.Parse (pValue); break;
                            case "T": T = double.Parse (pValue); break;
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

                            case "Cardiac_Rhythm": Cardiac_Rhythm.Value = (Cardiac_Rhythms.Values)Enum.Parse (typeof (Cardiac_Rhythms.Values), pValue); break;
                            case "Cardiac_Axis": Cardiac_Axis.Value = (CardiacAxes.Values)Enum.Parse (typeof (CardiacAxes.Values), pValue); break;
                            case "PulsusParadoxus": Pulsus_Paradoxus = bool.Parse (pValue); break;
                            case "PulsusAlternans": Pulsus_Alternans = bool.Parse (pValue); break;

                            case "Pacemaker_Rate": Pacemaker_Rate = int.Parse (pValue); break;
                            case "Pacemaker_Energy": Pacemaker_Energy = int.Parse (pValue); break;
                            case "Pacemaker_Threshold": Pacemaker_Threshold = int.Parse (pValue); break;

                            case "TransducerZeroed_ABP": TransducerZeroed_ABP = bool.Parse (pValue); break;
                            case "TransducerZeroed_CVP": TransducerZeroed_CVP = bool.Parse (pValue); break;
                            case "TransducerZeroed_PA": TransducerZeroed_PA = bool.Parse (pValue); break;
                            case "TransducerZeroed_ICP": TransducerZeroed_ICP = bool.Parse (pValue); break;
                            case "TransducerZeroed_IAP": TransducerZeroed_IAP = bool.Parse (pValue); break;

                            case "Respiratory_Rhythm": Respiratory_Rhythm.Value = (Respiratory_Rhythms.Values)Enum.Parse (typeof (Respiratory_Rhythms.Values), pValue); break;
                            case "Respiration_Inflated": Respiration_Inflated = bool.Parse (pValue); break;
                            case "Respiratory_IERatio_I": Respiratory_IERatio_I = int.Parse (pValue); break;
                            case "Respiratory_IERatio_E": Respiratory_IERatio_E = int.Parse (pValue); break;
                            case "Mechanically_Ventilated": Mechanically_Ventilated = bool.Parse (pValue); break;

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

            SetTimers ();
            OnCardiac_Baseline ();
            OnRespiratory_Baseline ();

            PatientEvent?.Invoke (this, new PatientEvent_Args (this, PatientEvent_Args.EventTypes.Vitals_Change));
        }

        public string Save () {
            StringBuilder sWrite = new StringBuilder ();

            sWrite.AppendLine (String.Format ("{0}:{1}", "Updated", Utility.DateTime_ToString (Updated)));
            sWrite.AppendLine (String.Format ("{0}:{1}", "HR", HR));
            sWrite.AppendLine (String.Format ("{0}:{1}", "RR", RR));
            sWrite.AppendLine (String.Format ("{0}:{1}", "ETCO2", ETCO2));
            sWrite.AppendLine (String.Format ("{0}:{1}", "SPO2", SPO2));
            sWrite.AppendLine (String.Format ("{0}:{1}", "T", T));
            sWrite.AppendLine (String.Format ("{0}:{1}", "CVP", CVP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "NSBP", NSBP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "NDBP", NDBP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "NMAP", NMAP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "ASBP", ASBP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "ADBP", ADBP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "AMAP", AMAP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "PSP", PSP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "PDP", PDP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "PMP", PMP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "ICP", ICP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "IAP", IAP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "ST_Elevation", string.Join (",", ST_Elevation)));
            sWrite.AppendLine (String.Format ("{0}:{1}", "T_Elevation", string.Join (",", T_Elevation)));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Cardiac_Rhythm", Cardiac_Rhythm.Value));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Cardiac_Axis", Cardiac_Axis.Value));
            sWrite.AppendLine (String.Format ("{0}:{1}", "PulsusParadoxus", Pulsus_Paradoxus));
            sWrite.AppendLine (String.Format ("{0}:{1}", "PulsusAlternans", Pulsus_Alternans));

            sWrite.AppendLine (String.Format ("{0}:{1}", "Pacemaker_Rate", Pacemaker_Rate));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Pacemaker_Energy", Pacemaker_Energy));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Pacemaker_Threshold", Pacemaker_Threshold));

            sWrite.AppendLine (String.Format ("{0}:{1}", "TransducerZeroed_ABP", TransducerZeroed_ABP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "TransducerZeroed_CVP", TransducerZeroed_CVP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "TransducerZeroed_PA", TransducerZeroed_PA));
            sWrite.AppendLine (String.Format ("{0}:{1}", "TransducerZeroed_ICP", TransducerZeroed_ICP));
            sWrite.AppendLine (String.Format ("{0}:{1}", "TransducerZeroed_IAP", TransducerZeroed_IAP));

            sWrite.AppendLine (String.Format ("{0}:{1}", "Respiratory_Rhythm", Respiratory_Rhythm.Value));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Respiration_Inflated", Respiration_Inflated));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Respiratory_IERatio_I", Respiratory_IERatio_I));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Respiratory_IERatio_E", Respiratory_IERatio_E));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Mechanically_Ventilated", Mechanically_Ventilated));

            sWrite.AppendLine (String.Format ("{0}:{1}", "FHR", FHR));
            sWrite.AppendLine (String.Format ("{0}:{1}", "FHR_Variability", FHR_Variability.Value));
            sWrite.AppendLine (String.Format ("{0}:{1}", "UterineContraction_Frequency", UC_Frequency));
            sWrite.AppendLine (String.Format ("{0}:{1}", "UterineContraction_Duration", UC_Duration));
            sWrite.AppendLine (String.Format ("{0}:{1}", "UterineContraction_Intensity", UC_Intensity.Value));
            sWrite.AppendLine (String.Format ("{0}:{1}", "FHR_Rhythms", string.Join (",", FHR_Decelerations.ValueList)));

            return sWrite.ToString ();
        }

        public void UpdateParameters (
                    int hr, int spo2, int rr, int etco2,
                    double t,
                    int cvp,
                    int nsbp, int ndbp, int nmap,
                    int asbp, int adbp, int amap,
                    int psp, int pdp, int pmp,
                    int icp, int iap,
                    int pacer_threshold,
                    bool puls_paradoxus, bool puls_alternans,
                    double [] st_elev, double [] t_elev,
                    Cardiac_Rhythms.Values card_rhythm,
                    CardiacAxes.Values card_axis,
                    Respiratory_Rhythms.Values resp_rhythm,
                    float resp_ier_i, float resp_ier_e,
                    bool mech_vent,
                    int fhr, Intensity.Values fhr_var, List<FetalHeartDecelerations.Values> fhr_rhythms,
                    int uc_freq, int uc_duration, Intensity.Values uc_intensity) {
            Updated = DateTime.UtcNow;

            HR = hr; RR = rr; SPO2 = spo2; ETCO2 = etco2;
            T = t;
            CVP = cvp; ICP = icp; IAP = iap;

            NSBP = nsbp; NDBP = ndbp; NMAP = nmap;
            ASBP = asbp; ADBP = adbp; AMAP = amap;
            PSP = psp; PDP = pdp; PMP = pmp;

            Cardiac_Rhythm.Value = card_rhythm;
            Cardiac_Axis.Value = card_axis;
            Pacemaker_Threshold = pacer_threshold;
            Pulsus_Paradoxus = puls_paradoxus;
            Pulsus_Alternans = puls_alternans;
            ST_Elevation = st_elev;
            T_Elevation = t_elev;

            Respiratory_Rhythm.Value = resp_rhythm;
            Respiratory_IERatio_I = resp_ier_i;
            Respiratory_IERatio_E = resp_ier_e;
            Mechanically_Ventilated = mech_vent;

            FHR = fhr;
            FHR_Variability.Value = fhr_var;
            FHR_Decelerations.ValueList = fhr_rhythms;
            UC_Frequency = uc_freq;
            UC_Duration = uc_duration;
            UC_Intensity.Value = uc_intensity;

            bufferParadoxus_BaseASBP = ASBP;
            switchParadoxus = false;

            SetTimers ();
            OnCardiac_Baseline ();
            OnRespiratory_Baseline ();

            PatientEvent?.Invoke (this, new PatientEvent_Args (this, PatientEvent_Args.EventTypes.Vitals_Change));
        }

        public void ClampVitals (
                    int hrMin, int hrMax,
                    int spo2Min, int spo2Max,
                    int etco2Min, int etco2Max,
                    int sbpMin, int sbpMax, int dbpMin, int dbpMax,
                    int pspMin, int pspMax, int pdpMin, int pdpMax) {
            HR = Utility.Clamp (HR, hrMin, hrMax);
            SPO2 = Utility.Clamp (SPO2, spo2Min, spo2Max);
            ETCO2 = Utility.Clamp (ETCO2, etco2Min, etco2Max);

            NSBP = Utility.Clamp (NSBP, sbpMin, sbpMax);
            NDBP = Utility.Clamp (NDBP, dbpMin, dbpMax);
            NMAP = Patient.CalculateMAP (NSBP, NDBP);

            ASBP = Utility.Clamp (ASBP, sbpMin, sbpMax);
            ADBP = Utility.Clamp (ADBP, sbpMin, sbpMax);
            AMAP = Patient.CalculateMAP (ASBP, ADBP);

            PSP = Utility.Clamp (PSP, pspMin, pspMax);
            PDP = Utility.Clamp (PDP, pdpMin, pdpMax);
            PMP = Patient.CalculateMAP (PSP, PDP);

            bufferParadoxus_BaseASBP = ASBP;
            switchParadoxus = false;

            SetTimers ();
            OnCardiac_Baseline ();
            OnRespiratory_Baseline ();
            PatientEvent?.Invoke (this, new PatientEvent_Args (this, PatientEvent_Args.EventTypes.Vitals_Change));
        }

        public void Defibrillate () => InitDefibrillation (false);
        public void Cardiovert () => InitDefibrillation (true);

        public void Pacemaker (bool active, int rate = 0, int energy = 0) {
            if (active && !timerPacemaker_Baseline.IsRunning)
                InitPacemaker (rate, energy);
            else if (!active && timerPacemaker_Baseline.IsRunning)
                StopPacemaker ();
            else if (active && timerPacemaker_Baseline.IsRunning)
                UpdatePacemaker (rate, energy);
        }
        public void PacemakerPause () => timerPacemaker_Baseline.Interval = 4000;

        private void InitDefibrillation (bool toSynchronize) {
            if (toSynchronize)
                timerCardiac_Ventricular.Tick += OnCardioversion;
            else
                OnDefibrillation ();
        }

        private void InitPacemaker (int rate, int energy) {
            Pacemaker_Rate = rate;
            Pacemaker_Energy = energy;
            timerPacemaker_Baseline.Reset ((int)((60d / rate) * 1000));
        }

        private void UpdatePacemaker () => UpdatePacemaker (Pacemaker_Rate, Pacemaker_Energy);

        private void UpdatePacemaker (int rate, int energy) {
            Pacemaker_Rate = rate;
            Pacemaker_Energy = energy;
            timerPacemaker_Baseline.Interval = (int)((60d / rate) * 1000);
        }

        private void StopPacemaker () {
            timerPacemaker_Baseline.Stop ();
            timerPacemaker_Spike.Stop ();
        }

        private void OnDefibrillation () {
            timerCardiac_Baseline.Stop ();
            timerCardiac_Atrial.Stop ();
            timerCardiac_Ventricular.Stop ();
            timerDefibrillation.Reset (20);
            // Invoke the defibrillation event *after* starting the timer- IsDefibrillating() checks the timer!
            PatientEvent?.Invoke (this, new PatientEvent_Args (this, PatientEvent_Args.EventTypes.Cardiac_Defibrillation));
        }

        private void OnDefibrillation_End () {
            timerDefibrillation.Stop ();
            timerCardiac_Baseline.Reset (timerCardiac_Baseline.Interval);
        }

        private void OnCardioversion (object sender, EventArgs e) {
            timerCardiac_Ventricular.Tick -= OnCardioversion;
            OnDefibrillation ();
        }

        private void OnPacemaker_Baseline () {
            if (Pacemaker_Energy > 0)
                PatientEvent?.Invoke (this, new PatientEvent_Args (this, PatientEvent_Args.EventTypes.Cardiac_PacerSpike));

            if (Pacemaker_Energy >= Pacemaker_Threshold)
                timerPacemaker_Spike.Reset (40);        // Adds an interval between the spike and the QRS complex

            UpdatePacemaker ();         // In case pacemaker was paused... updates .Interval
        }

        private void OnPacemaker_Spike () {
            timerPacemaker_Spike.Stop ();
            // Trigger the QRS complex, then reset the heart's intrinsic timers
            Cardiac_Rhythm.AberrantBeat = true;
            PatientEvent?.Invoke (this, new PatientEvent_Args (this, PatientEvent_Args.EventTypes.Cardiac_Ventricular));
            Cardiac_Rhythm.AberrantBeat = false;
            timerCardiac_Baseline.Reset ();
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
            timerCardiac_Baseline.Reset ((int)(HR_Seconds * 1000f));
            timerCardiac_Atrial.Stop ();
            timerCardiac_Ventricular.Stop ();

            timerDefibrillation.Reset ();
            timerPacemaker_Baseline.Reset ();
            timerPacemaker_Spike.Stop ();

            timerRespiratory_Baseline.Reset ((int)(RR_Seconds * 1000f));
            timerRespiratory_Inspiration.Stop ();
            timerRespiratory_Expiration.Stop ();

            timerObstetric_Baseline.Reset (1000);
            timerObstetric_ContractionDuration.Stop ();
            timerObstetric_ContractionFrequency.Stop ();
            timerObstetric_FHRVariationFrequency.Stop ();
        }

        private void OnCardiac_Baseline () {
            PatientEvent?.Invoke (this, new PatientEvent_Args (this, PatientEvent_Args.EventTypes.Cardiac_Baseline));
            timerCardiac_Baseline.Set ((int)(HR_Seconds * 1000f));

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
                    timerCardiac_Ventricular.Reset (1);
                    break;

                // Traced as "regular A" or "regular A -> V" Rhythms
                case Cardiac_Rhythms.Values.AV_Block__1st_Degree:
                case Cardiac_Rhythms.Values.AV_Block__Mobitz_II:
                case Cardiac_Rhythms.Values.AV_Block__Wenckebach:
                case Cardiac_Rhythms.Values.Bundle_Branch_Block:
                case Cardiac_Rhythms.Values.Sinus_Rhythm:
                case Cardiac_Rhythms.Values.Pulseless_Electrical_Activity:
                case Cardiac_Rhythms.Values.Ventricular_Standstill:
                    timerCardiac_Atrial.Reset (1);
                    break;

                // Traced as "irregular V" rhythms
                case Cardiac_Rhythms.Values.Atrial_Fibrillation:
                    timerCardiac_Baseline.Set ((int)(timerCardiac_Baseline.Interval * Utility.RandomDouble (0.6, 1.4)));
                    timerCardiac_Ventricular.Reset (1);
                    break;

                /* Special Cases */
                case Cardiac_Rhythms.Values.AV_Block__3rd_Degree:
                    if (!timerCardiac_Atrial.IsRunning)
                        timerCardiac_Atrial.Reset ((int)(timerCardiac_Baseline.Interval * 0.6));
                    timerCardiac_Ventricular.Reset (160);
                    break;

                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_PACs:
                    counterCardiac_Aberrancy -= 1;
                    if (counterCardiac_Aberrancy <= 0) {
                        counterCardiac_Aberrancy = new Random ().Next (4, 8);
                        timerCardiac_Baseline.Set ((int)(timerCardiac_Baseline.Interval * Utility.RandomDouble (0.6, 0.8)));
                    }
                    timerCardiac_Atrial.Reset (1);
                    break;

                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_PJCs:
                    counterCardiac_Aberrancy -= 1;
                    if (counterCardiac_Aberrancy <= 0) {
                        counterCardiac_Aberrancy = new Random ().Next (4, 8);
                        timerCardiac_Ventricular.Reset (1);
                    } else {
                        if (counterCardiac_Aberrancy == 1)
                            timerCardiac_Baseline.Set ((int)(timerCardiac_Baseline.Interval * Utility.RandomDouble (0.7, 0.9)));
                        timerCardiac_Atrial.Reset (1);
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
                        timerCardiac_Ventricular.Reset (1);
                        break;
                    }
                    timerCardiac_Atrial.Reset (1);
                    Cardiac_Rhythm.AberrantBeat = false;
                    break;

                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_PVCs_Unifocal:
                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_PVCs_Multifocal:
                    counterCardiac_Aberrancy -= 1;
                    if (counterCardiac_Aberrancy == 0) {  // Shorten the beat preceding the PVC, making it premature
                        timerCardiac_Baseline.Set ((int)(timerCardiac_Baseline.Interval * 0.8));
                    } else if (counterCardiac_Aberrancy < 0) {   // Then throw the PVC and reset the counters
                        counterCardiac_Aberrancy = new Random ().Next (4, 9);
                        Cardiac_Rhythm.AberrantBeat = true;
                        timerCardiac_Ventricular.Reset (1);
                        break;
                    }
                    Cardiac_Rhythm.AberrantBeat = false;
                    timerCardiac_Atrial.Reset (1);
                    break;
            }
        }

        private void OnCardiac_Atrial () {
            PatientEvent?.Invoke (this, new PatientEvent_Args (this, PatientEvent_Args.EventTypes.Cardiac_Atrial));

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
                case Cardiac_Rhythms.Values.Sinus_Rhythm:
                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_PACs:
                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_PJCs:
                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_Bigeminy:
                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_Trigeminy:
                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_PVCs_Unifocal:
                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_PVCs_Multifocal:
                case Cardiac_Rhythms.Values.Pulseless_Electrical_Activity:
                    timerCardiac_Atrial.Stop ();
                    timerCardiac_Ventricular.Reset (160);
                    break;

                /* Special cases */

                case Cardiac_Rhythms.Values.AV_Block__1st_Degree:
                    timerCardiac_Atrial.Stop ();
                    timerCardiac_Ventricular.Reset (240);
                    break;

                case Cardiac_Rhythms.Values.AV_Block__Mobitz_II:
                    timerCardiac_Atrial.Stop ();
                    counterCardiac_Aberrancy -= 1;
                    if (counterCardiac_Aberrancy < 0) {
                        counterCardiac_Aberrancy = 2;
                        Cardiac_Rhythm.AberrantBeat = true;
                    } else {
                        timerCardiac_Ventricular.Reset (160);
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
                        timerCardiac_Ventricular.Reset ((int)(160 * (3 - counterCardiac_Aberrancy)));
                        Cardiac_Rhythm.AberrantBeat = false;
                    }
                    break;

                case Cardiac_Rhythms.Values.AV_Block__3rd_Degree:
                    // Specifically let atrial timer continue to run and propogate P-waves!
                    break;
            }
        }

        private void OnCardiac_Ventricular () {
            PatientEvent?.Invoke (this, new PatientEvent_Args (this, PatientEvent_Args.EventTypes.Cardiac_Ventricular));

            // Flip the switch on pulsus alternans
            Cardiac_Rhythm.AlternansBeat = Pulsus_Alternans ? !Cardiac_Rhythm.AlternansBeat : false;

            switch (Cardiac_Rhythm.Value) {
                default: break;
            }

            timerCardiac_Ventricular.Stop ();
        }

        private void OnRespiratory_Baseline () {
            PatientEvent?.Invoke (this, new PatientEvent_Args (this, PatientEvent_Args.EventTypes.Respiratory_Baseline));
            timerRespiratory_Baseline.Set ((int)(RR_Seconds * 1000f));

            switch (Respiratory_Rhythm.Value) {
                default:
                case Respiratory_Rhythms.Values.Apnea:
                    break;

                case Respiratory_Rhythms.Values.Regular:
                    timerRespiratory_Inspiration.Reset (1);
                    break;
            }
        }

        private void OnRespiratory_Inspiration () {
            Respiration_Inflated = true;
            PatientEvent?.Invoke (this, new PatientEvent_Args (this, PatientEvent_Args.EventTypes.Respiratory_Inspiration));
            timerRespiratory_Inspiration.Stop ();

            // Process pulsus paradoxus (numerical values) for inspiration here
            // Flip counterCardiac_Paradoxus when flipping SBP portion
            if (Pulsus_Paradoxus && Respiratory_Rhythm.Value != Respiratory_Rhythms.Values.Apnea
                && (Mechanically_Ventilated || switchParadoxus)) {
                switchParadoxus = true;
                ASBP += Mechanically_Ventilated
                    ? -(int)(bufferParadoxus_BaseASBP * 0.15)
                    : (int)(bufferParadoxus_BaseASBP * 0.15);
                IABP_AP += Mechanically_Ventilated
                    ? -(int)(bufferParadoxus_BaseASBP * 0.05)
                    : (int)(bufferParadoxus_BaseASBP * 0.05);
            }

            switch (Respiratory_Rhythm.Value) {
                default:
                case Respiratory_Rhythms.Values.Apnea:
                    break;

                case Respiratory_Rhythms.Values.Regular:
                    timerRespiratory_Expiration.Reset ((int)(RR_Seconds_I * 1000f));     // Expiration.Interval marks end inspiration
                    break;
            }
        }

        private void OnRespiratory_Expiration () {
            Respiration_Inflated = false;
            PatientEvent?.Invoke (this, new PatientEvent_Args (this, PatientEvent_Args.EventTypes.Respiratory_Expiration));
            timerRespiratory_Expiration.Stop ();

            // Process pulsus paradoxus (numerical values) for expiration here
            // Flip counterCardiac_Paradoxus when flipping SBP portion
            if (Pulsus_Paradoxus && Respiratory_Rhythm.Value != Respiratory_Rhythms.Values.Apnea
                && (!Mechanically_Ventilated || switchParadoxus)) {
                switchParadoxus = true;
                ASBP += Mechanically_Ventilated
                    ? (int)(bufferParadoxus_BaseASBP * 0.15)
                    : -(int)(bufferParadoxus_BaseASBP * 0.15);
                IABP_AP += Mechanically_Ventilated
                    ? (int)(bufferParadoxus_BaseASBP * 0.05)
                    : -(int)(bufferParadoxus_BaseASBP * 0.05);
            }
        }

        private void OnObstetric_Baseline () {
            PatientEvent?.Invoke (this, new PatientEvent_Args (this, PatientEvent_Args.EventTypes.Obstetric_Baseline));

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
            PatientEvent?.Invoke (this, new PatientEvent_Args (this, PatientEvent_Args.EventTypes.Obstetric_ContractionStart));
            timerObstetric_ContractionDuration.Reset (UC_Duration * 1000);
        }

        private void OnObstetric_ContractionEnd () {
            PatientEvent?.Invoke (this, new PatientEvent_Args (this, PatientEvent_Args.EventTypes.Obstetric_ContractionEnd));
            timerObstetric_ContractionDuration.Stop ();
        }

        private void OnObstetric_FetalHeartVariationStart () {
            PatientEvent?.Invoke (this, new PatientEvent_Args (this, PatientEvent_Args.EventTypes.Obstetric_FetalHeartVariation));
        }
    }
}