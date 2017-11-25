using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace II {
    public class Patient {
        
        public int  HR,
                    SBP, DBP, MAP, 
                    SpO2, 
                    CVP,
                    PSP, PDP, PMP;
        public float T;
        public Rhythms.Cardiac_Rhythm Heart_Rhythm;

        public Patient() {
            HR = 80;
            SBP = 120;
            DBP = 80;
            MAP = 95;
            SpO2 = 98;
            T = 38;
            Heart_Rhythm = Rhythms.Cardiac_Rhythm.Normal_Sinus;
        }

        public Patient (Patient p) {
            HR = p.HR;
            SBP = p.SBP;
            DBP = p.DBP;
            MAP = p.MAP;
            SpO2 = p.SpO2;
            T = p.T;

            CVP = 6;
            PSP = 22;
            PDP = 12;
            PMP = 16;

            Heart_Rhythm = p.Heart_Rhythm;
        }

        public void calcMAP () {
            MAP = DBP + ((SBP - DBP) / 3);
        }
    }
}
