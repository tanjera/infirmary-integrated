using System;
using System.Windows.Forms;
using Newtonsoft.Json;


namespace II {
    public class Patient {

        public int  HR, RR, ETCO2, SpO2, CVP,
                    NSBP, NDBP, NMAP,
                    ASBP, ADBP, AMAP,
                    PSP, PDP, PMP;
        public double T;

        public double[] ST_Elevation, T_Elevation;
        public Cardiac_Rhythms Cardiac_Rhythm = new Cardiac_Rhythms();
        public bool Cardiac_Rhythm__Flag;               // Used for signaling aberrant beats as needed
        public Cardiac_Axis_Shifts Cardiac_Axis_Shift;

        public Respiratory_Rhythms Respiratory_Rhythm;
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
                            Cardiac_Axis_Shifts.Normal,
                            Respiratory_Rhythms.Regular,
                            1, 1);

            initTimers ();
            setTimers ();
        }

        public void Load(string json) {
            JsonConvert.PopulateObject (json, this);

            setTimers ();
            onCardiac_Baseline ();
            onRespiratory_Baseline ();
            PatientEvent?.Invoke (this, new PatientEvent_Args (this, PatientEvent_Args.EventTypes.Vitals_Change));
        }
        public string Save() { return JsonConvert.SerializeObject (this, Formatting.Indented); }

        public void TogglePause() {
            _Paused = !_Paused;

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
                    Cardiac_Rhythms.Values  card_rhythm,
                    Cardiac_Axis_Shifts     card_axis_shift,
                    Respiratory_Rhythms     resp_rhythm,
                    int resp_ier_i, int resp_ier_e) {

            HR = hr;    RR = rr;    SpO2 = spo2;
            T = t;
            CVP = cvp;  ETCO2 = etco2;

            NSBP = nsbp;    NDBP = ndbp;    NMAP = nmap;
            ASBP = asbp;    ADBP = adbp;    AMAP = amap;
            PSP = psp;      PDP = pdp;      PMP = pmp;

            Cardiac_Rhythm.Value = card_rhythm;
            Cardiac_Axis_Shift = card_axis_shift;
            ST_Elevation = st_elev;
            T_Elevation = t_elev;

            Respiratory_Rhythm = resp_rhythm;
            Respiratory_IERatio_I = resp_ier_i;
            Respiratory_IERatio_E = resp_ier_e;

            setTimers ();
            onCardiac_Baseline ();
            onRespiratory_Baseline ();
            PatientEvent?.Invoke (this, new PatientEvent_Args (this, PatientEvent_Args.EventTypes.Vitals_Change));
        }

        private void initTimers() {
            timerCardiac_Baseline.Tick += delegate {
                onCardiac_Baseline ();
            };

            timerCardiac_Atrial.Tick += delegate {
                onCardiac_Atrial ();
            };

            timerCardiac_Ventricular.Tick += delegate {
                onCardiac_Ventricular ();
            };

            timerRespiratory_Baseline.Tick += delegate {
                onRespiratory_Baseline ();
            };

            timerRespiratory_Inspiration.Tick += delegate {
                onRespiratory_Inspiration ();
            };

            timerRespiratory_Expiration.Tick += delegate {
                onRespiratory_Expiration ();
            };
        }

        private void setTimers() {
            timerCardiac_Baseline.Interval = (int) (HR_Seconds * 1000f);
            timerRespiratory_Baseline.Interval = (int)(RR_Seconds * 1000f);

            timerCardiac_Baseline.Start ();
            timerCardiac_Atrial.Stop ();
            timerCardiac_Ventricular.Stop ();

            timerRespiratory_Baseline.Start ();
            timerRespiratory_Inspiration.Stop ();
            timerRespiratory_Expiration.Stop ();
        }

        private void onCardiac_Baseline() {
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
                case Cardiac_Rhythms.Values.Ventricular_Tachycardia_Pulsed:
                case Cardiac_Rhythms.Values.Ventricular_Tachycardia_Pulseless:
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

        private void onCardiac_Atrial () {
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

        private void onCardiac_Ventricular () {
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

        private void onRespiratory_Baseline() {
            PatientEvent?.Invoke (this, new PatientEvent_Args (this, PatientEvent_Args.EventTypes.Respiratory_Baseline));
            timerRespiratory_Baseline.Interval = (int)(RR_Seconds * 1000f);

            switch (Respiratory_Rhythm) {
                default:
                case Respiratory_Rhythms.Apnea:
                    break;

                case Respiratory_Rhythms.Regular:
                    timerRespiratory_Inspiration.Interval = 1;
                    timerRespiratory_Inspiration.Start ();
                    break;
            }
        }

        private void onRespiratory_Inspiration() {
            Respiratory_Inflated = true;
            PatientEvent?.Invoke (this, new PatientEvent_Args (this, PatientEvent_Args.EventTypes.Respiratory_Inspiration));

            timerRespiratory_Inspiration.Stop ();

            switch (Respiratory_Rhythm) {
                default:
                case Respiratory_Rhythms.Apnea:
                    break;

                case Respiratory_Rhythms.Regular:
                    timerRespiratory_Expiration.Interval = (int)(RR_Seconds_I * 1000f);     // Expiration.Interval marks end inspiration
                    timerRespiratory_Expiration.Start ();
                    break;
            }
        }

        private void onRespiratory_Expiration() {
            Respiratory_Inflated = false;
            PatientEvent?.Invoke (this, new PatientEvent_Args (this, PatientEvent_Args.EventTypes.Respiratory_Expiration));

            timerRespiratory_Expiration.Stop ();
        }

        public static int CalculateMAP (int sbp, int dbp) {
            return dbp + ((sbp - dbp) / 3);
        }
    }
}
