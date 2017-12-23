using System;
using System.IO;
using System.Text;
using System.Windows.Forms;


namespace II {
    public class Patient {

        public int  HR, RR, ETCO2, SPO2, CVP,
                    NSBP, NDBP, NMAP,
                    ASBP, ADBP, AMAP,
                    PSP, PDP, PMP;
        public double T;

        public double[] ST_Elevation, T_Elevation;
        public Cardiac_Rhythms Cardiac_Rhythm = new Cardiac_Rhythms();
        public Cardiac_Axes Cardiac_Axis = new Cardiac_Axes ();
        public bool Cardiac_Rhythm__Flag;               // Used for signaling aberrant beats as needed

        public Respiratory_Rhythms Respiratory_Rhythm = new Respiratory_Rhythms();
        public bool Respiratory_Inflated;
        public int Respiratory_IERatio_I, Respiratory_IERatio_E;

        public bool Paused { get { return _Paused; } }
        public double HR_Seconds { get { return 60d / Math.Max (1, HR); } }
        public double RR_Seconds { get { return 60d / Math.Max (1, RR); } }
        public double RR_Seconds_I { get { return (RR_Seconds / (Respiratory_IERatio_I + Respiratory_IERatio_E)) * Respiratory_IERatio_I; } }
        public double RR_Seconds_E { get { return (RR_Seconds / (Respiratory_IERatio_I + Respiratory_IERatio_E)) * Respiratory_IERatio_E; } }

        private bool _Paused = false;

        private Timer timerCardiac_Baseline = new Timer (),
                        timerCardiac_Atrial = new Timer (),
                        timerCardiac_Ventricular = new Timer (),
                        timerRespiratory_Baseline = new Timer (),
                        timerRespiratory_Inspiration = new Timer(),
                        timerRespiratory_Expiration = new Timer();

        private int counterCardiac;

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
                Respiratory_Baseline,
                Respiratory_Inspiration,
                Respiratory_Expiration
            }
        }


        public Patient () {
            UpdateVitals (  80, 18, 98,
                            38.0f, 6, 40,
                            120, 80, 95,
                            120, 80, 95,
                            22, 12, 16,
                            new double[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f },
                            new double[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f },
                            Cardiac_Rhythms.Values.Sinus_Rhythm,
                            Cardiac_Axes.Values.Normal,
                            Respiratory_Rhythms.Values.Regular,
                            1, 1);

            InitTimers ();
            SetTimers ();
        }

        public void Load_Process (string inc) {
            StringReader sRead = new StringReader (inc);

            try {
                string line;
                while ((line = sRead.ReadLine()) != null) {
                    if (line.Contains(":")) {
                        string pName = line.Substring (0, line.IndexOf (':')),
                                pValue = line.Substring (line.IndexOf (':') + 1);
                        switch (pName) {
                            default: break;
                            case "HR": HR = int.Parse (pValue); break;
                            case "RR": RR = int.Parse (pValue); break;
                            case "ETCO2": ETCO2 = int.Parse (pValue); break;
                            case "SPO2": SPO2 = int.Parse (pValue); break;
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
                            case "T": T = int.Parse (pValue); break;
                            case "ST_Elevation":
                                string[] e_st = pValue.Split (',');
                                for (int i = 0; i < e_st.Length; i++)
                                    ST_Elevation [i] = double.Parse (e_st [i]);
                                break;
                            case "T_Elevation":
                                string [] e_t = pValue.Split (',');
                                for (int i = 0; i < e_t.Length; i++)
                                    T_Elevation [i] = double.Parse (e_t [i]);
                                break;
                            case "Cardiac_Rhythm": Cardiac_Rhythm.Value = (Cardiac_Rhythms.Values) Enum.Parse(typeof(Cardiac_Rhythms.Values), pValue); break;
                            case "Cardiac_Rhythm__Flag": Cardiac_Rhythm__Flag = bool.Parse (pValue); break;
                            case "Cardiac_Axis_Shift": Cardiac_Axis.Value = (Cardiac_Axes.Values)Enum.Parse (typeof (Cardiac_Axes.Values), pValue); break;
                            case "Respiratory_Rhythm": Respiratory_Rhythm.Value = (Respiratory_Rhythms.Values)Enum.Parse (typeof (Respiratory_Rhythms.Values), pValue); break;
                            case "Respiratory_Inflated": Respiratory_Inflated = bool.Parse (pValue); break;
                            case "Respiratory_IERatio_I": Respiratory_IERatio_I = int.Parse (pValue); break;
                            case "Respiratory_IERatio_E": Respiratory_IERatio_E = int.Parse (pValue); break;
                        }
                    }
                }
            } catch {
                sRead.Close ();
                return;
            }

            sRead.Close ();

            SetTimers ();
            OnCardiac_Baseline ();
            OnRespiratory_Baseline ();

            PatientEvent?.Invoke (this, new PatientEvent_Args (this, PatientEvent_Args.EventTypes.Vitals_Change));
        }

        public string Save () {
            StringBuilder sWrite = new StringBuilder ();

            sWrite.AppendLine (String.Format ("{0}:{1}", "HR", HR));
            sWrite.AppendLine (String.Format ("{0}:{1}", "RR", RR));
            sWrite.AppendLine (String.Format ("{0}:{1}", "ETCO2", ETCO2));
            sWrite.AppendLine (String.Format ("{0}:{1}", "SPO2", SPO2));
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
            sWrite.AppendLine (String.Format ("{0}:{1}", "T", T));
            sWrite.AppendLine (String.Format ("{0}:{1}", "ST_Elevation", string.Join(",", ST_Elevation)));
            sWrite.AppendLine (String.Format ("{0}:{1}", "T_Elevation", string.Join(",", T_Elevation)));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Cardiac_Rhythm", Cardiac_Rhythm.Value));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Cardiac_Rhythm__Flag", Cardiac_Rhythm__Flag));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Cardiac_Axis_Shift", Cardiac_Axis.Value));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Respiratory_Rhythm", Respiratory_Rhythm.Value));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Respiratory_Inflated", Respiratory_Inflated));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Respiratory_IERatio_I", Respiratory_IERatio_I));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Respiratory_IERatio_E", Respiratory_IERatio_E));

            return sWrite.ToString ();
        }

        public void TogglePause() {
            _Paused = !_Paused;
            ApplyPause ();
        }

        private void ApplyPause() {
            switch (_Paused) {
                case true:
                    timerCardiac_Baseline.Stop ();
                    timerRespiratory_Baseline.Stop ();
                    break;

                case false:
                    timerCardiac_Baseline.Start ();
                    timerRespiratory_Baseline.Start ();
                    break;
            }
        }

        public void UpdateVitals(
                    int hr,     int rr,     int spo2,
                    double t,
                    int cvp,    int etco2,
                    int nsbp,   int ndbp,   int nmap,
                    int asbp,   int adbp,   int amap,
                    int psp,    int pdp,    int pmp,
                    double[] st_elev,        double[] t_elev,
                    Cardiac_Rhythms.Values      card_rhythm,
                    Cardiac_Axes.Values         card_axis,
                    Respiratory_Rhythms.Values  resp_rhythm,
                    int resp_ier_i, int resp_ier_e) {

            HR = hr;    RR = rr;    SPO2 = spo2;
            T = t;
            CVP = cvp;  ETCO2 = etco2;

            NSBP = nsbp;    NDBP = ndbp;    NMAP = nmap;
            ASBP = asbp;    ADBP = adbp;    AMAP = amap;
            PSP = psp;      PDP = pdp;      PMP = pmp;

            Cardiac_Rhythm.Value = card_rhythm;
            Cardiac_Axis.Value = card_axis;
            ST_Elevation = st_elev;
            T_Elevation = t_elev;

            Respiratory_Rhythm.Value = resp_rhythm;
            Respiratory_IERatio_I = resp_ier_i;
            Respiratory_IERatio_E = resp_ier_e;

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

            HR = _.Clamp (HR, hrMin, hrMax);
            SPO2 = _.Clamp (SPO2, spo2Min, spo2Max);
            ETCO2 = _.Clamp (ETCO2, etco2Min, etco2Max);

            NSBP = _.Clamp (NSBP, sbpMin, sbpMax);
            NDBP = _.Clamp (NDBP, dbpMin, dbpMax);
            NMAP = Patient.CalculateMAP (NSBP, NDBP);

            ASBP = _.Clamp (ASBP, sbpMin, sbpMax);
            ADBP = _.Clamp (ADBP, sbpMin, sbpMax);
            AMAP = Patient.CalculateMAP (ASBP, ADBP);

            PSP = _.Clamp (PSP, pspMin, pspMax);
            PDP = _.Clamp (PDP, pdpMin, pdpMax);
            PMP = Patient.CalculateMAP (PSP, PDP);

            SetTimers ();
            OnCardiac_Baseline ();
            OnRespiratory_Baseline ();
            PatientEvent?.Invoke (this, new PatientEvent_Args (this, PatientEvent_Args.EventTypes.Vitals_Change));
        }

        private void InitTimers() {
            timerCardiac_Baseline.Tick += delegate {
                OnCardiac_Baseline ();
            };

            timerCardiac_Atrial.Tick += delegate {
                OnCardiac_Atrial ();
            };

            timerCardiac_Ventricular.Tick += delegate {
                OnCardiac_Ventricular ();
            };

            timerRespiratory_Baseline.Tick += delegate {
                OnRespiratory_Baseline ();
            };

            timerRespiratory_Inspiration.Tick += delegate {
                OnRespiratory_Inspiration ();
            };

            timerRespiratory_Expiration.Tick += delegate {
                OnRespiratory_Expiration ();
            };
        }

        private void SetTimers() {
            timerCardiac_Baseline.Interval = (int) (HR_Seconds * 1000f);
            timerRespiratory_Baseline.Interval = (int)(RR_Seconds * 1000f);

            timerCardiac_Baseline.Start ();
            timerCardiac_Atrial.Stop ();
            timerCardiac_Ventricular.Stop ();

            timerRespiratory_Baseline.Start ();
            timerRespiratory_Inspiration.Stop ();
            timerRespiratory_Expiration.Stop ();
        }

        private void OnCardiac_Baseline() {
            PatientEvent?.Invoke (this, new PatientEvent_Args (this, PatientEvent_Args.EventTypes.Cardiac_Baseline));
            timerCardiac_Baseline.Interval = (int)(HR_Seconds * 1000f);

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
                    timerCardiac_Ventricular.Interval = 1;
                    timerCardiac_Ventricular.Start ();
                    break;

                // Traced as "regular A" or "regular A -> V" Rhythms
                case Cardiac_Rhythms.Values.AV_Block__1st_Degree:
                case Cardiac_Rhythms.Values.AV_Block__Mobitz_II:
                case Cardiac_Rhythms.Values.AV_Block__Wenckebach:
                case Cardiac_Rhythms.Values.Bundle_Branch_Block:
                case Cardiac_Rhythms.Values.Sinus_Rhythm:
                case Cardiac_Rhythms.Values.Pulseless_Electrical_Activity:
                case Cardiac_Rhythms.Values.Ventricular_Standstill:
                    timerCardiac_Atrial.Interval = 1;
                    timerCardiac_Atrial.Start ();
                    break;

                // Traced as "irregular V" rhythms
                case Cardiac_Rhythms.Values.Atrial_Fibrillation:
                    timerCardiac_Baseline.Interval = (int)(timerCardiac_Baseline.Interval * _.RandomDouble (0.6, 1.4));
                    timerCardiac_Ventricular.Interval = 1;
                    timerCardiac_Ventricular.Start ();
                    break;

                /* Special Cases */
                case Cardiac_Rhythms.Values.AV_Block__3rd_Degree:
                    timerCardiac_Atrial.Interval = (int)(timerCardiac_Baseline.Interval * 0.6);
                    timerCardiac_Atrial.Start ();
                    timerCardiac_Ventricular.Interval = 160;
                    timerCardiac_Ventricular.Start ();
                    break;

                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_PACs:
                    counterCardiac -= 1;
                    if (counterCardiac <= 0) {
                        counterCardiac = new Random ().Next (4, 8);
                        timerCardiac_Baseline.Interval = (int)(timerCardiac_Baseline.Interval * _.RandomDouble (0.6, 0.8));
                    }
                    timerCardiac_Atrial.Interval = 1;
                    timerCardiac_Atrial.Start ();
                    break;

                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_PJCs:
                    counterCardiac -= 1;
                    if (counterCardiac <= 0) {
                        counterCardiac = new Random ().Next (4, 8);
                        timerCardiac_Ventricular.Interval = 1;
                        timerCardiac_Ventricular.Start ();
                    } else {
                        if (counterCardiac == 1)
                            timerCardiac_Baseline.Interval = (int)(timerCardiac_Baseline.Interval * _.RandomDouble (0.7, 0.9));
                        timerCardiac_Atrial.Interval = 1;
                        timerCardiac_Atrial.Start ();
                    }
                    break;

                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_Bigeminy:
                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_Trigeminy:
                    counterCardiac -= 1;
                    if (counterCardiac == 0) {
                        timerCardiac_Baseline.Interval = (int)(timerCardiac_Baseline.Interval * 0.8);
                    } else if (counterCardiac < 0) {   // Then throw the PVC and reset the counters
                        if (Cardiac_Rhythm.Value == Cardiac_Rhythms.Values.Sinus_Rhythm_with_Bigeminy)
                            counterCardiac = 1;
                        else if (Cardiac_Rhythm.Value == Cardiac_Rhythms.Values.Sinus_Rhythm_with_Trigeminy)
                            counterCardiac = 2;
                        Cardiac_Rhythm__Flag = true;
                        timerCardiac_Ventricular.Interval = 1;
                        timerCardiac_Ventricular.Start ();
                        break;
                    }
                    timerCardiac_Atrial.Interval = 1;
                    timerCardiac_Atrial.Start ();
                    break;

                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_PVCs_Unifocal:
                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_PVCs_Multifocal:
                    counterCardiac -= 1;
                    if (counterCardiac == 0 || Cardiac_Rhythm__Flag) {  // Shorten the beat preceding the PVC, making it premature
                        timerCardiac_Baseline.Interval = (int)(timerCardiac_Baseline.Interval * 0.8);
                    }
                    if (counterCardiac < 0 || Cardiac_Rhythm__Flag) {   // Then throw the PVC and reset the counters
                        counterCardiac = new Random().Next(4, 9);
                        Cardiac_Rhythm__Flag = true;
                        timerCardiac_Ventricular.Interval = 1;
                        timerCardiac_Ventricular.Start ();
                        break;
                    }
                    timerCardiac_Atrial.Interval = 1;
                    timerCardiac_Atrial.Start ();
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
                    timerCardiac_Ventricular.Interval = 160;
                    timerCardiac_Ventricular.Start ();
                    break;

                /* Special cases */

                case Cardiac_Rhythms.Values.AV_Block__1st_Degree:
                    timerCardiac_Atrial.Stop ();
                    timerCardiac_Ventricular.Interval = 240;
                    timerCardiac_Ventricular.Start ();
                    break;

                case Cardiac_Rhythms.Values.AV_Block__Mobitz_II:
                    timerCardiac_Atrial.Stop ();
                    counterCardiac += 1;
                    if (counterCardiac > 2) {
                        counterCardiac = 0;
                    } else {
                        timerCardiac_Ventricular.Interval = 160;
                        timerCardiac_Ventricular.Start ();
                    }
                    break;

                case Cardiac_Rhythms.Values.AV_Block__Wenckebach:
                    timerCardiac_Atrial.Stop ();
                    counterCardiac += 1;
                    if (counterCardiac >= 4) {
                        counterCardiac = 0;
                    } else {
                        timerCardiac_Baseline.Interval = (int)(timerCardiac_Baseline.Interval + (160 * counterCardiac));
                        timerCardiac_Ventricular.Interval = (int)(160 * counterCardiac);
                        timerCardiac_Ventricular.Start ();
                    }
                    break;

                case Cardiac_Rhythms.Values.AV_Block__3rd_Degree:
                    // Specifically let atrial timer continue to run and propogate P-waves!
                    break;
            }
        }

        private void OnCardiac_Ventricular () {
            PatientEvent?.Invoke (this, new PatientEvent_Args (this, PatientEvent_Args.EventTypes.Cardiac_Ventricular));

            switch (Cardiac_Rhythm.Value) {
                default:
                    Cardiac_Rhythm__Flag = false;
                    break;

                case Cardiac_Rhythms.Values.Sinus_Rhythm_with_PVCs_Unifocal:
                    Cardiac_Rhythm__Flag = new Random ().Next (0, 7) == 0;       // 1/7 chance to potentiate runs of PVCs
                    break;
            }

            timerCardiac_Ventricular.Stop ();
        }

        private void OnRespiratory_Baseline() {
            PatientEvent?.Invoke (this, new PatientEvent_Args (this, PatientEvent_Args.EventTypes.Respiratory_Baseline));
            timerRespiratory_Baseline.Interval = (int)(RR_Seconds * 1000f);

            switch (Respiratory_Rhythm.Value) {
                default:
                case Respiratory_Rhythms.Values.Apnea:
                    break;

                case Respiratory_Rhythms.Values.Regular:
                    timerRespiratory_Inspiration.Interval = 1;
                    timerRespiratory_Inspiration.Start ();
                    break;
            }
        }

        private void OnRespiratory_Inspiration() {
            Respiratory_Inflated = true;
            PatientEvent?.Invoke (this, new PatientEvent_Args (this, PatientEvent_Args.EventTypes.Respiratory_Inspiration));

            timerRespiratory_Inspiration.Stop ();

            switch (Respiratory_Rhythm.Value) {
                default:
                case Respiratory_Rhythms.Values.Apnea:
                    break;

                case Respiratory_Rhythms.Values.Regular:
                    timerRespiratory_Expiration.Interval = (int)(RR_Seconds_I * 1000f);     // Expiration.Interval marks end inspiration
                    timerRespiratory_Expiration.Start ();
                    break;
            }
        }

        private void OnRespiratory_Expiration() {
            Respiratory_Inflated = false;
            PatientEvent?.Invoke (this, new PatientEvent_Args (this, PatientEvent_Args.EventTypes.Respiratory_Expiration));

            timerRespiratory_Expiration.Stop ();
        }

        public static int CalculateMAP (int sbp, int dbp) {
            return dbp + ((sbp - dbp) / 3);
        }
    }
}
