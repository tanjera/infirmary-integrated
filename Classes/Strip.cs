using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;

namespace II.Rhythms {

    public class Strip {
        Waveform sType;
        static float timeLength = 5.0f;                // Strip length in seconds

        public List<Point> Points;
        
        
        public enum Waveform {
            ECG_L1,
            ECG_L2,
            ECG_L3,
            ECG_LAVR,
            ECG_LAVL,
            ECG_LAVF,
            ECG_LV1,
            ECG_LV2,
            ECG_LV3,
            ECG_LV4,
            ECG_LV5,
            ECG_LV6,

            SPO2,
            CVP,
            ABP,
            PA,
            IABP
        }

        public Strip (Waveform w) {
            sType = w;
            Points = new List<Point> ();
        }

        public void Reset () {
            Points.Clear ();
        }

        public void Clear_Future () {
            for (int i = Points.Count - 1; i >= 0; i--) {
                if (Points[i].X > timeLength)
                    Points.RemoveAt (i);
            }
        }

        public Point Last (List<Point> _In) {
            if (_In.Count < 1)
                // New vectors are added to Points beginning at 5 seconds to marquee backwards to 0
                return new Point (timeLength, 0);
            else
                return _In[_In.Count - 1];
        }

        public void Concatenate (List<Point> _Addition) {
            if (_Addition.Count == 0)
                return;
            
            float offsetX = Last (Points).X;
    
            foreach (Point eachVector in _Addition)
                Points.Add (new Point (eachVector.X + offsetX, eachVector.Y));
        }
        public void Scroll () {
            foreach (Point eachVector in Points)
                eachVector.X -= _.Draw_Resolve;
            
            for (int i = Points.Count - 1; i >= 0; i--) {
                if (Points[i].X < -timeLength)
                    Points.RemoveAt (i);
            }    
        }
        
        public void Add_Beat (Patient _Patient) {            
            if (Last(Points).X > timeLength * 2)
                return;

            switch (sType) {
                default:
                case Waveform.ECG_L1:
                    Rhythm_Index.Get_Rhythm (_Patient.Heart_Rhythm).Beat_ECG_L1 (_Patient, this);
                    break;
                case Waveform.ECG_L2:
                    Rhythm_Index.Get_Rhythm (_Patient.Heart_Rhythm).Beat_ECG_L2 (_Patient, this);
                    break;
                case Waveform.ECG_L3:
                    Rhythm_Index.Get_Rhythm (_Patient.Heart_Rhythm).Beat_ECG_L3 (_Patient, this);
                    break;
                case Waveform.ECG_LAVR:
                    Rhythm_Index.Get_Rhythm (_Patient.Heart_Rhythm).Beat_ECG_LaVR (_Patient, this);
                    break;
                case Waveform.ECG_LAVL:
                    Rhythm_Index.Get_Rhythm (_Patient.Heart_Rhythm).Beat_ECG_LaVL (_Patient, this);
                    break;
                case Waveform.ECG_LAVF:
                    Rhythm_Index.Get_Rhythm (_Patient.Heart_Rhythm).Beat_ECG_LaVF (_Patient, this);
                    break;
                case Waveform.ECG_LV1:
                    Rhythm_Index.Get_Rhythm (_Patient.Heart_Rhythm).Beat_ECG_LV1 (_Patient, this);
                    break;
                case Waveform.ECG_LV2:
                    Rhythm_Index.Get_Rhythm (_Patient.Heart_Rhythm).Beat_ECG_LV2 (_Patient, this);
                    break;
                case Waveform.ECG_LV3:
                    Rhythm_Index.Get_Rhythm (_Patient.Heart_Rhythm).Beat_ECG_LV3 (_Patient, this);
                    break;
                case Waveform.ECG_LV4:
                    Rhythm_Index.Get_Rhythm (_Patient.Heart_Rhythm).Beat_ECG_LV4 (_Patient, this);
                    break;
                case Waveform.ECG_LV5:
                    Rhythm_Index.Get_Rhythm (_Patient.Heart_Rhythm).Beat_ECG_LV5 (_Patient, this);
                    break;
                case Waveform.ECG_LV6:
                    Rhythm_Index.Get_Rhythm (_Patient.Heart_Rhythm).Beat_ECG_LV6 (_Patient, this);
                    break;

                case Waveform.SPO2:
                    Rhythm_Index.Get_Rhythm (_Patient.Heart_Rhythm).Beat_SpO2 (_Patient, this);
                    break;


                case Waveform.CVP: break;
                case Waveform.ABP: break;
                case Waveform.PA: break;
                case Waveform.IABP: break;
            }         
        }


        public class Renderer {

            Control panel;
            Pen p;
            Strip s;

            int offX, offY;
            float multX, multY;
            
            public Renderer (Control _Panel, ref Strip _Strip, Pen _Pen) {
                panel = _Panel;
                p = _Pen;
                s = _Strip;
            }

            public void Draw (PaintEventArgs e) {
                e.Graphics.Clear (Color.Black);

                offX = 0;
                offY = panel.Height / 2;
                multX = panel.Width / timeLength;
                multY = -panel.Height / 2;

                if (s.Points.Count < 2)
                    return;
                
                System.Drawing.Point lastPoint = new System.Drawing.Point (
                        (int)(s.Points[0].X * multX) + offX,
                        (int)(s.Points[0].Y * multY) + offY);

                for (int i = 1; i < s.Points.Count; i++) {
                    if (s.Points[i].X > timeLength * 2)
                        continue;

                    System.Drawing.Point thisPoint = new System.Drawing.Point (
                        (int)(s.Points[i].X * multX) + offX,
                        (int)(s.Points[i].Y * multY) + offY);

                    e.Graphics.DrawLine (p, lastPoint, thisPoint);
                    lastPoint = thisPoint;
                }
            }
        }
    }
}