using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;

namespace II.Rhythm {
    public class Strip {
        double Length = 5.0d;               // Strip length in seconds
        double EdgeBuffer = 1.1d;           // Coefficient of Length to draw into future as "now" for buffer
        DateTime Scrolled_Last = DateTime.UtcNow;
        bool Scrolled_Unpausing = false;

        public Leads Lead;
        public List<Point> Points;


        public Strip (double l, Leads.Values v) {
            Lead = new Leads(v);
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

            double offsetX = Last (Points).X;

            for (int i = 0; i < _Addition.Count; i++)
                Points.Add (new Point (_Addition[i].X + offsetX, _Addition[i].Y));
        }
        public void Overwrite(List<Point> _Replacement) {
            if (_Replacement.Count == 0)
                return;

            // Inserts into future of strip, which is X offset by Length
            for (int i = 0; i < _Replacement.Count; i++)
                _Replacement[i].X += Length * EdgeBuffer;

            double minX = _Replacement[0].X,
                maxX = _Replacement[_Replacement.Count - 1].X;

            for (int i = Points.Count - 1; i >= 0; i--)
                if (Points[i].X > minX && Points[i].X < maxX)
                    Points.RemoveAt (i);

            Points.AddRange (_Replacement);
        }
        public void Underwrite (List<Point> _Replacement) {
            if (_Replacement.Count == 0)
                return;

            // Inserts into future of strip, which is X offset by Length
            for (int i = 0; i < _Replacement.Count; i++)
                _Replacement[i].X += Length * EdgeBuffer;

            double minX = _Replacement[0].X,
                maxX = _Replacement[_Replacement.Count - 1].X;

            for (int i = Points.Count - 1; i >= 0; i--)
                if (Points[i].X > minX && Points[i].X < maxX) {
                    if (Points[i].Y == 0f)
                        Points.RemoveAt (i);
                    else
                        return;
                }

            Points.AddRange (_Replacement);
        }
        private void RemoveNull() {
            for (int i = Points.Count - 1; i >= 0; i--)
                if (Points[i] == null)
                    Points.RemoveAt (i);
        }
        private void Sort() {
            Points.Sort (delegate (Point p1, Point p2) {
                if (p1 == null && p2 == null) return 0;
                else if (p1 == null) return -1;
                else if (p2 == null) return 1;
                else return p1.X.CompareTo (p2.X); });
        }
        public void Scroll () {
            if (Scrolled_Unpausing) {
                Scrolled_Unpausing = false;
                Scrolled_Last = DateTime.UtcNow;
                return;
            }

            double scrollBy = (DateTime.UtcNow - Scrolled_Last).TotalMilliseconds / 1000;
            Scrolled_Last = DateTime.UtcNow;

            for (int i = Points.Count - 1; i >= 0; i--)
                Points[i].X -= scrollBy;

            for (int i = Points.Count - 1; i >= 0; i--) {
                if (Points[i].X < -Length)
                    Points.RemoveAt (i);
            }
        }
        public void Unpause() {
            Scrolled_Unpausing = true;
        }

        public void Add_Beat__Cardiac_Baseline (Patient _Patient) {
            if (isECG ())
                _Patient.Cardiac_Rhythm.ECG_Isoelectric (_Patient, this);
            else if (Lead.Value != Leads.Values.RR && Lead.Value != Leads.Values.ETCO2) {
                // Fill waveform through to future buffer with flatline
                double fill = (Length * EdgeBuffer) - Last (Points).X;
                Concatenate (Waveforms.Waveform_Flatline (fill > _Patient.HR_Seconds ? fill : _Patient.HR_Seconds, 0f));
            }
        }

        public void Add_Beat__Cardiac_Atrial (Patient _Patient) {
            if (isECG ())
                _Patient.Cardiac_Rhythm.ECG_Atrial (_Patient, this);
        }

        public void Add_Beat__Cardiac_Ventricular (Patient _Patient) {
            if (isECG ())
                _Patient.Cardiac_Rhythm.ECG_Ventricular (_Patient, this);
            else if (Lead.Value == Leads.Values.SpO2 && _Patient.Cardiac_Rhythm.Pulse_Ventricular)
                Overwrite (Waveforms.SpO2_Rhythm (_Patient, 1f));
            else if (Lead.Value == Leads.Values.ABP && _Patient.Cardiac_Rhythm.Pulse_Ventricular)
                Overwrite (Waveforms.ABP_Rhythm (_Patient, 1f));
        }

        public void Add_Beat__Respiratory_Baseline (Patient _Patient) {
            if (Lead.Value == Leads.Values.RR || Lead.Value == Leads.Values.ETCO2) {
                // Fill waveform through to future buffer with flatline
                double fill = (Length * EdgeBuffer) - Last (Points).X;
                Concatenate (Waveforms.Waveform_Flatline (fill > _Patient.RR_Seconds ? fill : _Patient.RR_Seconds, 0f));
            }
        }

        public void Add_Beat__Respiratory_Inspiration (Patient _Patient) {
            switch (Lead.Value) {
                default: break;
                case Leads.Values.RR: Overwrite (Waveforms.RR_Rhythm(_Patient, true)); break;
                case Leads.Values.ETCO2: break;    // End-tidal waveform is only present on expiration!! Is flatline on inspiration.
            }
        }

        public void Add_Beat__Respiratory_Expiration (Patient _Patient) {
            switch (Lead.Value) {
                default: break;
                case Leads.Values.RR: Overwrite (Waveforms.RR_Rhythm (_Patient, false)); break;
                case Leads.Values.ETCO2: Overwrite (Waveforms.ETCO2_Rhythm (_Patient)); break;
            }
        }

        private bool isECG() {
            return Lead.Value ==  Leads.Values.ECG_I || Lead.Value ==  Leads.Values.ECG_II
                || Lead.Value ==  Leads.Values.ECG_III || Lead.Value ==  Leads.Values.ECG_AVR
                || Lead.Value ==  Leads.Values.ECG_AVL || Lead.Value ==  Leads.Values.ECG_AVF
                || Lead.Value ==  Leads.Values.ECG_V1 || Lead.Value ==  Leads.Values.ECG_V2
                || Lead.Value ==  Leads.Values.ECG_V3 || Lead.Value ==  Leads.Values.ECG_V4
                || Lead.Value ==  Leads.Values.ECG_V5 || Lead.Value ==  Leads.Values.ECG_V6;
        }


        public class Renderer {

            Control panel;
            Strip strip;
            public Color pcolor;
            _.ColorScheme scheme;

            int offX, offY;
            double multX, multY;

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

                strip.RemoveNull ();
                strip.Sort ();

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