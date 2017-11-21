using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infirmary_Integrated {
    public class Patient {
        
        public int HR, SBP, DBP, MAP, SpO2;
        public float T;
        public Rhythms.Cardiac_Rhythms Heart_Rhythm;

        public Patient() {
            HR = 80;
            SBP = 120;
            DBP = 80;
            MAP = 95;
            SpO2 = 98;
            T = 38;

            Heart_Rhythm = Rhythms.Cardiac_Rhythms.Normal_Sinus;
        }

        public void MAP_Calculate () {
            MAP = DBP + ((SBP - DBP) / 3);
        }
    }
}
