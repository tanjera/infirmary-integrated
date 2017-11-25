using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;

namespace II.Rhythms {
    public class Strip {        
        static float timeLength = 5.0f;                // Strip length in seconds

        public Leads Lead;
        public List<Point> Points;

        static public Color stripColors (Leads l) {
            switch (l) {
                default: return Color.Green;
                case Leads.SPO2: return Color.Orange;
                case Leads.CVP: return Color.Blue;
                case Leads.ABP: return Color.Red;
                case Leads.PA: return Color.Yellow;
                case Leads.IABP: return Color.Blue;
            }
        }

        public Strip (Leads w) {
            Lead = w;
            Points = new List<Point> ();
        }

        public void Reset () {
            Points.Clear ();
        }

        public void clearFuture () {
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

            switch (Lead) {
                default:
                case Leads.ECG_L1:
                case Leads.ECG_L2:
                case Leads.ECG_L3:
                case Leads.ECG_LAVR:
                case Leads.ECG_LAVL:
                case Leads.ECG_LAVF:
                case Leads.ECG_LV1:
                case Leads.ECG_LV2:
                case Leads.ECG_LV3:
                case Leads.ECG_LV4:
                case Leads.ECG_LV5:
                case Leads.ECG_LV6:
                    Rhythm_Index.Get_Rhythm (_Patient.Heart_Rhythm).Beat_ECG (_Patient, this);
                    break;
                    
                case Leads.SPO2:
                    Rhythm_Index.Get_Rhythm (_Patient.Heart_Rhythm).Beat_SpO2 (_Patient, this);
                    break;
                    
                case Leads.CVP: break;
                case Leads.ABP: break;
                case Leads.PA: break;
                case Leads.IABP: break;
            }         
        }


        public class Renderer {

            Control panel;            
            Strip s;
            public Pen pen;

            int offX, offY;
            float multX, multY;
            
            public Renderer (Control _Panel, ref Strip _Strip, Pen _Pen) {
                panel = _Panel;
                pen = _Pen;
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

                    e.Graphics.DrawLine (pen, lastPoint, thisPoint);
                    lastPoint = thisPoint;
                }
            }
        }
    }
}