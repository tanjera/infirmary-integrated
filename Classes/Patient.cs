using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace II {
    public class Patient {
        
        public int  HR, RR, ETCO2, SpO2, CVP,
                    NSBP, NDBP, NMAP,
                    ASBP, ADBP, AMAP,
                    PSP, PDP, PMP;
        public float T;
        
        public float[] ST_Elevation, T_Elevation;
        public Rhythms.Cardiac_Rhythm Cardiac_Rhythm;
        public Rhythms.Cardiac_Axis_Shifts Cardiac_Axis_Shift;
        
        public bool Respiratory_Inflated;
        public float Respiratory_IERatio;

        private Timer timerCardiac_Baseline = new Timer (),
                        timerCardiac_Atrial = new Timer (),
                        timerCardiac_Ventricular = new Timer (),
                        timerRespiratory_Baseline = new Timer (),
                        timerRespiratory_Inspiratory = new Timer(),
                        timerRespiratory_Expiratory = new Timer();


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
                            new float[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f },
                            new float[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f },
                            Rhythms.Cardiac_Rhythm.Normal_Sinus,
                            Rhythms.Cardiac_Axis_Shifts.Normal,
                            1);

            initTimers ();
            setTimers ();
        }
        
        public void UpdateVitals(
                    int hr,     int rr,     int spo2,
                    float t,
                    int cvp,    int etco2,
                    int nsbp,   int ndbp,   int nmap,
                    int asbp,   int adbp,   int amap,
                    int psp,    int pdp,    int pmp,                        
                    float[] st_elev,        float[] t_elev,
                    Rhythms.Cardiac_Rhythm      card_rhythm,
                    Rhythms.Cardiac_Axis_Shifts card_axis_shift,
                    float resp_ie_ratio) {
            
            HR = hr;    RR = rr;    SpO2 = spo2;
            T = t;
            CVP = cvp;  ETCO2 = etco2;

            NSBP = nsbp;    NDBP = ndbp;    NMAP = nmap;
            ASBP = asbp;    ADBP = adbp;    AMAP = amap;
            PSP = psp;      PDP = pdp;      PMP = pmp;

            Cardiac_Rhythm = card_rhythm;
            Cardiac_Axis_Shift = card_axis_shift;
            ST_Elevation = st_elev;
            T_Elevation = t_elev;
            
            Respiratory_IERatio = resp_ie_ratio;

            setTimers ();
            PatientEvent?.Invoke (this, new PatientEvent_Args (this, PatientEvent_Args.EventTypes.Vitals_Change));
        }

        private void initTimers() {
            timerCardiac_Baseline.Tick += delegate {
                onCardiac_Baseline ();
                PatientEvent?.Invoke (this, new PatientEvent_Args (this, PatientEvent_Args.EventTypes.Cardiac_Baseline));
            };
            
            timerCardiac_Atrial.Tick += delegate {
                onCardiac_Atrial ();
                PatientEvent?.Invoke (this, new PatientEvent_Args (this, PatientEvent_Args.EventTypes.Cardiac_Atrial));
            };
            
            timerCardiac_Ventricular.Tick += delegate {
                onCardiac_Ventricular ();
                PatientEvent?.Invoke (this, new PatientEvent_Args (this, PatientEvent_Args.EventTypes.Cardiac_Ventricular));
            };
            
            timerRespiratory_Baseline.Tick += delegate {                
                PatientEvent?.Invoke (this, new PatientEvent_Args (this, PatientEvent_Args.EventTypes.Respiratory_Baseline));                
            };

            Respiratory_Inflated = false;
            timerRespiratory_Inspiratory.Tick += delegate {
                Respiratory_Inflated = true;
                PatientEvent?.Invoke (this, new PatientEvent_Args (this, PatientEvent_Args.EventTypes.Respiratory_Inspiration));
                timerRespiratory_Expiratory.Interval = (int)(timerRespiratory_Inspiratory.Interval / (Respiratory_IERatio + 1));
                timerRespiratory_Expiratory.Start ();
            };
            
            timerRespiratory_Expiratory.Tick += delegate {
                Respiratory_Inflated = false;
                PatientEvent?.Invoke (this, new PatientEvent_Args (this, PatientEvent_Args.EventTypes.Respiratory_Expiration));
                timerRespiratory_Expiratory.Stop ();
            };
        }

        private void setTimers() {
            timerCardiac_Baseline.Interval = (int) ((60f / Math.Max (1, HR)) * 1000);
            timerRespiratory_Baseline.Interval = (int)((60f / Math.Max (1, RR)) * 1000);

            timerCardiac_Baseline.Start ();
            timerCardiac_Atrial.Stop ();
            timerCardiac_Ventricular.Stop ();

            timerRespiratory_Baseline.Start ();
            timerRespiratory_Inspiratory.Stop ();
            timerRespiratory_Expiratory.Stop ();
        }

        private void onCardiac_Baseline() {
            timerCardiac_Baseline.Interval = (int)((60f / Math.Max (1, HR)) * 1000);

            switch (Cardiac_Rhythm) {
                default: break;
                

                case Rhythms.Cardiac_Rhythm.Atrial_Flutter: break;
                case Rhythms.Cardiac_Rhythm.Atrial_Fibrillation: break;
                case Rhythms.Cardiac_Rhythm.Premature_Atrial_Contractions: break;
                case Rhythms.Cardiac_Rhythm.Supraventricular_Tachycardia: break;

                case Rhythms.Cardiac_Rhythm.AV_Block__1st_Degree: break;
                case Rhythms.Cardiac_Rhythm.AV_Block__Wenckebach: break;
                case Rhythms.Cardiac_Rhythm.AV_Block__Mobitz_II: break;
                case Rhythms.Cardiac_Rhythm.AV_Block__3rd_Degree: break;                
                case Rhythms.Cardiac_Rhythm.Premature_Junctional_Contractions: break;

                
                case Rhythms.Cardiac_Rhythm.Premature_Ventricular_Contractions: break;


                    

                // Traced as "regular V" Rhythms                
                case Rhythms.Cardiac_Rhythm.Junctional:
                case Rhythms.Cardiac_Rhythm.Idioventricular:
                case Rhythms.Cardiac_Rhythm.Ventricular_Tachycardia:
                case Rhythms.Cardiac_Rhythm.Ventricular_Fibrillation:
                    timerCardiac_Ventricular.Interval = 1;
                    timerCardiac_Ventricular.Start ();
                    break;

                // Traced as "regular A" or "regular A/V" Rhythms
                case Rhythms.Cardiac_Rhythm.Block__Bundle_Branch:
                case Rhythms.Cardiac_Rhythm.Normal_Sinus:
                case Rhythms.Cardiac_Rhythm.Pulseless_Electrical_Activity:
                case Rhythms.Cardiac_Rhythm.Sinus_Tachycardia:
                case Rhythms.Cardiac_Rhythm.Sinus_Bradycardia:
                case Rhythms.Cardiac_Rhythm.Ventricular_Standstill:
                    timerCardiac_Atrial.Interval = 1;
                    timerCardiac_Atrial.Start ();
                    break;
            }
        }
        private void onCardiac_Atrial () {
            switch (Cardiac_Rhythm) {
                default: break;
                    
                case Rhythms.Cardiac_Rhythm.Atrial_Flutter: break;
                case Rhythms.Cardiac_Rhythm.Atrial_Fibrillation: break;
                case Rhythms.Cardiac_Rhythm.Premature_Atrial_Contractions: break;
                case Rhythms.Cardiac_Rhythm.Supraventricular_Tachycardia: break;

                case Rhythms.Cardiac_Rhythm.AV_Block__1st_Degree: break;
                case Rhythms.Cardiac_Rhythm.AV_Block__Wenckebach: break;
                case Rhythms.Cardiac_Rhythm.AV_Block__Mobitz_II: break;
                case Rhythms.Cardiac_Rhythm.AV_Block__3rd_Degree: break;
                
                case Rhythms.Cardiac_Rhythm.Premature_Junctional_Contractions: break;
                    
                case Rhythms.Cardiac_Rhythm.Premature_Ventricular_Contractions: break;
                    
                


                case Rhythms.Cardiac_Rhythm.Ventricular_Standstill:
                    timerCardiac_Atrial.Stop ();
                    break;

                case Rhythms.Cardiac_Rhythm.Pulseless_Electrical_Activity:
                case Rhythms.Cardiac_Rhythm.Block__Bundle_Branch:
                case Rhythms.Cardiac_Rhythm.Normal_Sinus:
                case Rhythms.Cardiac_Rhythm.Sinus_Tachycardia:
                case Rhythms.Cardiac_Rhythm.Sinus_Bradycardia:
                    timerCardiac_Atrial.Stop ();
                    timerCardiac_Ventricular.Interval = 160;
                    timerCardiac_Ventricular.Start ();
                    break;
            }
        }
        private void onCardiac_Ventricular () {
            switch (Cardiac_Rhythm) {
                default: break;
                    

                case Rhythms.Cardiac_Rhythm.Atrial_Flutter: break;
                case Rhythms.Cardiac_Rhythm.Atrial_Fibrillation: break;
                case Rhythms.Cardiac_Rhythm.Premature_Atrial_Contractions: break;
                case Rhythms.Cardiac_Rhythm.Supraventricular_Tachycardia: break;

                case Rhythms.Cardiac_Rhythm.AV_Block__1st_Degree: break;
                case Rhythms.Cardiac_Rhythm.AV_Block__Wenckebach: break;
                case Rhythms.Cardiac_Rhythm.AV_Block__Mobitz_II: break;
                case Rhythms.Cardiac_Rhythm.AV_Block__3rd_Degree: break;
                
                case Rhythms.Cardiac_Rhythm.Premature_Junctional_Contractions: break;
                    
                case Rhythms.Cardiac_Rhythm.Premature_Ventricular_Contractions: break;
                    
                




                case Rhythms.Cardiac_Rhythm.Junctional:
                case Rhythms.Cardiac_Rhythm.Idioventricular:
                case Rhythms.Cardiac_Rhythm.Ventricular_Tachycardia:
                case Rhythms.Cardiac_Rhythm.Ventricular_Fibrillation:                    
                    timerCardiac_Ventricular.Stop ();
                    break;

                case Rhythms.Cardiac_Rhythm.Pulseless_Electrical_Activity:
                case Rhythms.Cardiac_Rhythm.Block__Bundle_Branch:
                case Rhythms.Cardiac_Rhythm.Normal_Sinus:
                case Rhythms.Cardiac_Rhythm.Sinus_Tachycardia:
                case Rhythms.Cardiac_Rhythm.Sinus_Bradycardia:
                    timerCardiac_Ventricular.Stop ();
                    break;
            }
        }

        public static int CalculateMAP (int sbp, int dbp) {
            return dbp + ((sbp - dbp) / 3);
        }
    }
}
