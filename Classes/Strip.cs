﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;

namespace II.Rhythms {
    public class Strip {        
        float Length = 5.0f;                // Strip length in seconds

        public Leads Lead;
        public List<Point> Points;

        static public Color stripColors (Leads l) {
            switch (l) {
                default: return Color.Green;
                case Leads.SpO2: return Color.Orange;
                case Leads.CVP: return Color.Blue;
                case Leads.ABP: return Color.Red;
                case Leads.PA: return Color.Yellow;
                case Leads.IABP: return Color.Blue;
            }
        }

        public Strip (float l, Leads w) {
            Lead = w;
            Length = l;
            Points = new List<Point> ();
        }

        public void Reset () {
            Points.Clear ();
        }

        public void clearFuture () {
            for (int i = Points.Count - 1; i >= 0; i--) {
                if (Points[i].X > Length)
                    Points.RemoveAt (i);
            }
        }

        public Point Last (List<Point> _In) {
            if (_In.Count < 1)
                // New vectors are added to Points beginning at 5 seconds to marquee backwards to 0
                return new Point (Length, 0);
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
                eachVector.X -= Rhythm.Draw_Resolve;
            
            for (int i = Points.Count - 1; i >= 0; i--) {
                if (Points[i].X < -Length)
                    Points.RemoveAt (i);
            }    
        }
        
        public void Add_Beat (Patient _Patient) {            
            if (Last(Points).X > Length * 2)
                return;

            switch (Lead) {
                default:
                case Leads.ECG_I:
                case Leads.ECG_II:
                case Leads.ECG_III:
                case Leads.ECG_AVR:
                case Leads.ECG_AVL:
                case Leads.ECG_AVF:
                case Leads.ECG_V1:
                case Leads.ECG_V2:
                case Leads.ECG_V3:
                case Leads.ECG_V4:
                case Leads.ECG_V5:
                case Leads.ECG_V6:
                    Rhythm_Index.Get_Rhythm (_Patient.Cardiac_Rhythm).Beat_ECG (_Patient, this);
                    break;
                    
                case Leads.SpO2:
                    Rhythm_Index.Get_Rhythm (_Patient.Cardiac_Rhythm).Beat_SpO2 (_Patient, this);
                    break;
                    
                case Leads.CVP: break;
                case Leads.ABP:
                    Rhythm_Index.Get_Rhythm (_Patient.Cardiac_Rhythm).Beat_ABP (_Patient, this);
                    break;
                case Leads.PA: break;
                case Leads.IABP: break;
            }         
        }


        public class Renderer {

            Control panel;            
            Strip strip;
            public Color pcolor;
            _.ColorScheme scheme;

            int offX, offY;
            float multX, multY;
            
            public Renderer (Control _Panel, ref Strip _Strip, Color _Color) {
                panel = _Panel;
                pcolor = _Color;
                strip = _Strip;
            }

            public void setColorScheme (_.ColorScheme cs) {
                scheme = cs;
            }

            public void Draw (PaintEventArgs e) {
                Pen pen;
                switch (scheme) {
                    default:
                    case _.ColorScheme.Normal:
                        e.Graphics.Clear (Color.Black);
                        pen = new Pen (pcolor, 1f);
                        break;
                    case _.ColorScheme.Monochrome:
                        e.Graphics.Clear (Color.White);
                        pen = new Pen (Color.Black, 1f);
                        break;
                }                

                offX = 0;
                offY = panel.Height / 2;
                multX = panel.Width / strip.Length;
                multY = -panel.Height / 2;

                if (strip.Points.Count < 2)
                    return;
                
                System.Drawing.Point lastPoint = new System.Drawing.Point (
                        (int)(strip.Points[0].X * multX) + offX,
                        (int)(strip.Points[0].Y * multY) + offY);

                for (int i = 1; i < strip.Points.Count; i++) {
                    if (strip.Points[i].X > strip.Length * 2)
                        continue;

                    System.Drawing.Point thisPoint = new System.Drawing.Point (
                        (int)(strip.Points[i].X * multX) + offX,
                        (int)(strip.Points[i].Y * multY) + offY);

                    e.Graphics.DrawLine (pen, lastPoint, thisPoint);
                    lastPoint = thisPoint;
                }
            }
        }
    }
}