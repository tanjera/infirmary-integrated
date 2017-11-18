using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infirmary_Integrated {
    public class Patient {

        public enum Rhythm {
            None,

            Normal_Sinus,
            Sinus_Tachycardia,
            Sinus_Bradycardia,

            Atrial_Flutter,
            Atrial_Fibrillation,
            Premature_Atrial_Contractions,
            Supraventricular_Tachycardia,

            AV_Block__1st_Degree,
            AV_Block__Wenckebach,
            AV_Block__Mobitz_II,
            AV_Block__3rd_Degree,
            Junctional,
            Premature_Junctional_Contractions,

            Block__Bundle_Branch,
            Premature_Ventricular_Contractions,
            Idioventricular,
            Ventricular_Fibrillation,

            Ventricular_Standstill,
            Asystole
        }


        public int HR, SBP, DBP, MAP, SpO2;
        public float T;
        public Rhythm Heart_Rhythm;

        public Patient() {
            HR = 80;
            SBP = 120;
            DBP = 80;
            MAP = 95;
            SpO2 = 98;
            T = 38;

            Heart_Rhythm = Rhythm.Normal_Sinus;
        }
    }
}
