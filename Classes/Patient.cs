using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    
        public Patient() {
            HR = 80;        RR = 18;        SpO2 = 98;
            T = 38.0f;      CVP = 6;        ETCO2 = 40;

            NSBP = 120;     NDBP = 80;      NMAP = 95;
            ASBP = 120;     ADBP = 80;      AMAP = 95;            
            PSP = 22;       PDP = 12;       PMP = 16;

            Cardiac_Rhythm = Rhythms.Cardiac_Rhythm.Normal_Sinus;
            Cardiac_Axis_Shift = Rhythms.Cardiac_Axis_Shifts.Normal;

            ST_Elevation = new float[] 
                { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f };
            T_Elevation = new float[]
                { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f };
        }
        
        public Patient (Patient p) {
            HR = p.HR;          RR = p.RR;          SpO2 = p.SpO2;
            T = p.T;            CVP = p.CVP;        ETCO2 = p.ETCO2;

            NSBP = p.NSBP;      NDBP = p.NDBP;      NMAP = p.NMAP;
            ASBP = p.ASBP;      ADBP = p.ADBP;      AMAP = p.AMAP;
            PSP = p.PSP;        PDP = p.PDP;        PMP = p.PMP;
            
            Cardiac_Rhythm = p.Cardiac_Rhythm;
            Cardiac_Axis_Shift = p.Cardiac_Axis_Shift;

            ST_Elevation = p.ST_Elevation;
            T_Elevation = p.T_Elevation;
        }

        public static int calcMAP (int sbp, int dbp) {
            return dbp + ((sbp - dbp) / 3);
        }
    }
}
