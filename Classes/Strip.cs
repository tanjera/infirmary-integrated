using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;

namespace II.Rhythms {

    public class Strip {
        
        public List<Point> Points;

        static float Time_Length = 5.0f;                // Strip length in seconds
        
        public Strip () {
            Points = new List<Point> ();
        }

        public void Reset () {
            Points.Clear ();
        }

        public void Clear_Future () {
            for (int i = Points.Count - 1; i >= 0; i--) {
                if (Points[i].X > Time_Length)
                    Points.RemoveAt (i);
            }
        }

        public Point Last (List<Point> _In) {
            if (_In.Count < 1)
                // New vectors are added to Points beginning at 5 seconds to marquee backwards to 0
                return new Point (Time_Length, 0);
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
                if (Points[i].X < -Time_Length)
                    Points.RemoveAt (i);
            }    
        }
        
        public void Add_Beat (Patient _Patient) {            
            if (Last(Points).X > Time_Length * 2)
                return;

            Rhythm_Index.Get_Rhythm (_Patient.Heart_Rhythm).Beat_ECG(_Patient, this);            
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
                multX = panel.Width / Time_Length;
                multY = -panel.Height / 2;

                if (s.Points.Count < 2)
                    return;
                
                System.Drawing.Point lastPoint = new System.Drawing.Point (
                        (int)(s.Points[0].X * multX) + offX,
                        (int)(s.Points[0].Y * multY) + offY);

                for (int i = 1; i < s.Points.Count; i++) {
                    if (s.Points[i].X > Time_Length * 2)
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