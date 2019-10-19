/* Strip.cs
 * Infirmary Integrated
 * By Ibi Keller (Tanjera), (c) 2017
 *
 * Actual management of geometric plotting of waveforms in a single unit is in Strip.cs.
 * Concatenation of waveform complexes, overwriting and underwriting (combining) wave points,
 * marquee scrolling of the waveforms, managing and resizing a future edge buffer (important
 * for continuity of drawing) and cleaning old data points from data collections.
 */

using System;
using System.Collections.Generic;

using II.Waveform;

namespace II.Rhythm {
    public class Strip {
        /* Default variables for easy modification of multiple measurement/tracing functions */
        public static double DefaultLength = 6.0d,
            DefaultBufferLength = .2d,
            DefaultRespiratoryCoefficient = 3d;

        public double lengthSeconds = 6.0d;               // Strip length in seconds
        double forwardBuffer = 1.20d;                     // Coefficient of Length to draw into future as "now" for buffer
        DateTime scrolledLast = DateTime.UtcNow;
        bool scrollingUnpausing = false;

        public Lead Lead;
        public List<Point> Points;

        public Strip (double length, Lead.Values lead) {
            Lead = new Lead (lead);
            lengthSeconds = length;
            Points = new List<Point> ();
        }

        public Strip (Lead.Values lead) {
            Lead = new Lead (lead);
            lengthSeconds = IsRespiratory ? DefaultLength * DefaultRespiratoryCoefficient : DefaultLength;
            Points = new List<Point> ();
        }

        private bool IsECG {
            get {
                return Lead.Value == Lead.Values.ECG_I || Lead.Value == Lead.Values.ECG_II
                    || Lead.Value == Lead.Values.ECG_III || Lead.Value == Lead.Values.ECG_AVR
                    || Lead.Value == Lead.Values.ECG_AVL || Lead.Value == Lead.Values.ECG_AVF
                    || Lead.Value == Lead.Values.ECG_V1 || Lead.Value == Lead.Values.ECG_V2
                    || Lead.Value == Lead.Values.ECG_V3 || Lead.Value == Lead.Values.ECG_V4
                    || Lead.Value == Lead.Values.ECG_V5 || Lead.Value == Lead.Values.ECG_V6;
            }
        }

        private bool IsCardiac {
            get {
                return Lead.Value == Lead.Values.ECG_I || Lead.Value == Lead.Values.ECG_II
                    || Lead.Value == Lead.Values.ECG_III || Lead.Value == Lead.Values.ECG_AVR
                    || Lead.Value == Lead.Values.ECG_AVL || Lead.Value == Lead.Values.ECG_AVF
                    || Lead.Value == Lead.Values.ECG_V1 || Lead.Value == Lead.Values.ECG_V2
                    || Lead.Value == Lead.Values.ECG_V3 || Lead.Value == Lead.Values.ECG_V4
                    || Lead.Value == Lead.Values.ECG_V5 || Lead.Value == Lead.Values.ECG_V6
                    || Lead.Value == Lead.Values.ABP
                    || Lead.Value == Lead.Values.CVP
                    || Lead.Value == Lead.Values.IABP
                    || Lead.Value == Lead.Values.IAP
                    || Lead.Value == Lead.Values.ICP
                    || Lead.Value == Lead.Values.PA
                    || Lead.Value == Lead.Values.SPO2;
            }
        }

        private bool IsRespiratory {
            get {
                return Lead.Value == Lead.Values.ETCO2
                    || Lead.Value == Lead.Values.RR;
            }
        }

        public void SetLead (Lead.Values lead) {
            Lead = new Lead (lead);
            lengthSeconds = IsRespiratory ? DefaultLength * DefaultRespiratoryCoefficient : DefaultLength;
        }

        private void SetForwardBuffer (Patient patient, bool onClear = false) {

            // Set the forward edge buffer (a coefficient of lengthSeconds!) to be the length of 2 beats/breaths
            if (IsCardiac)
                forwardBuffer = Math.Max (1 + (2 * (patient.GetHR_Seconds / lengthSeconds)),
                    (onClear ? 1.1d : forwardBuffer));
            else if (IsRespiratory)
                forwardBuffer = Math.Max (1 + (2 * (patient.GetRR_Seconds / lengthSeconds)),
                    (onClear ? 1.1d : forwardBuffer));
        }

        public void Reset () {
            Points.Clear ();
        }

        public void ClearFuture (Patient patient) {
            SetForwardBuffer (patient, true);         // Since accounting for forward edge buffer, recalculate

            for (int i = Points.Count - 1; i >= 0; i--) {

                // Must account for forwardEdgeBuffer... otherwise will cause period of "asystole"
                if (Points [i].X > (lengthSeconds * forwardBuffer))
                    Points.RemoveAt (i);
            }
        }

        public Point Last (List<Point> _In) {
            if (_In.Count < 1)

                // New vectors are added to Points beginning at 5 seconds to marquee backwards to 0
                return new Point (lengthSeconds, 0);
            else
                return _In [_In.Count - 1];
        }

        public void Concatenate (List<Point> addition) {
            if (addition.Count == 0)
                return;

            double offsetX = Last (Points).X;

            for (int i = 0; i < addition.Count; i++)
                Points.Add (new Point (addition [i].X + offsetX, addition [i].Y));
        }

        public void Overwrite (List<Point> replacement) {
            if (replacement.Count == 0)
                return;

            // Inserts into future of strip, which is X offset by Length
            for (int i = 0; i < replacement.Count; i++)
                replacement [i].X += lengthSeconds * forwardBuffer;

            double minX = replacement [0].X,
                maxX = replacement [replacement.Count - 1].X;

            for (int i = Points.Count - 1; i >= 0; i--)
                if (Points [i].X > minX && Points [i].X < maxX)
                    Points.RemoveAt (i);

            Points.AddRange (replacement);
        }

        public void Underwrite (List<Point> replacement) {
            if (replacement.Count == 0)
                return;

            // Inserts into future of strip, which is X offset by Length
            for (int i = 0; i < replacement.Count; i++)
                replacement [i].X += lengthSeconds * forwardBuffer;

            double minX = replacement [0].X,
                maxX = replacement [replacement.Count - 1].X;

            for (int i = Points.Count - 1; i >= 0; i--)
                if (Points [i].X > minX && Points [i].X < maxX) {
                    if (Points [i].Y == 0f)
                        Points.RemoveAt (i);
                    else
                        return;
                }

            Points.AddRange (replacement);
        }

        public void RemoveNull () {
            for (int i = Points.Count - 1; i >= 0; i--)
                if (Points [i] == null)
                    Points.RemoveAt (i);
        }

        public void Sort () {
            Points.Sort (delegate (Point p1, Point p2) {
                if (p1 == null && p2 == null) return 0;
                else if (p1 == null) return -1;
                else if (p2 == null) return 1;
                else return p1.X.CompareTo (p2.X);
            });
        }

        public void Scroll () {
            if (scrollingUnpausing) {
                scrollingUnpausing = false;
                scrolledLast = DateTime.UtcNow;
                return;
            }

            double scrollBy = (DateTime.UtcNow - scrolledLast).TotalMilliseconds / 1000;
            scrolledLast = DateTime.UtcNow;

            for (int i = Points.Count - 1; i >= 0; i--)
                Points [i].X -= scrollBy;

            for (int i = Points.Count - 1; i >= 0; i--) {
                if (Points [i].X < -lengthSeconds)
                    Points.RemoveAt (i);
            }
        }

        public void Unpause () {
            scrollingUnpausing = true;
        }

        public void Add_Beat__Cardiac_Defibrillation (Patient p) {
            if (IsECG)
                Overwrite (Draw.ECG_Defibrillation (p, Lead));
        }

        public void Add_Beat__Cardiac_Pacemaker (Patient p) {
            if (IsECG)
                Overwrite (Draw.ECG_Pacemaker (p, Lead));
        }

        public void Add_Beat__Cardiac_Baseline (Patient p) {
            SetForwardBuffer (p);

            if (IsECG) {
                p.Cardiac_Rhythm.ECG_Isoelectric (p, this);
            } else if (Lead.Value != Lead.Values.RR && Lead.Value != Lead.Values.ETCO2) {

                // Fill waveform through to future buffer with flatline
                double fill = (lengthSeconds * forwardBuffer) - Last (Points).X;
                Concatenate (Draw.Waveform_Flatline (fill > p.GetHR_Seconds ? fill : p.GetHR_Seconds, 0f));
            }

            if (Lead.Value == Lead.Values.CVP
                || (Lead.Value == Lead.Values.PA   // PA catheter in RA has CVP waveform
                    && p.PulmonaryArtery_Placement.Value == PulmonaryArtery_Rhythms.Values.Right_Atrium)) {
                if (p.Cardiac_Rhythm.HasPulse_Atrial && !p.Cardiac_Rhythm.HasPulse_Ventricular)
                    Overwrite (Draw.CVP_Rhythm (p, 0.25f));
                else if (!p.Cardiac_Rhythm.HasPulse_Atrial && p.Cardiac_Rhythm.HasPulse_Ventricular)
                    Overwrite (Draw.CVP_Rhythm (p, 0.5f));
                else if (p.Cardiac_Rhythm.HasPulse_Atrial && p.Cardiac_Rhythm.HasPulse_Ventricular)
                    Overwrite (Draw.CVP_Rhythm (p, 1f));
            }
        }

        public void Add_Beat__Cardiac_Atrial (Patient p) {
            if (IsECG)
                p.Cardiac_Rhythm.ECG_Atrial (p, this);
        }

        public void Add_Beat__Cardiac_Ventricular (Patient p) {
            if (IsECG)
                p.Cardiac_Rhythm.ECG_Ventricular (p, this);
            else if (Lead.Value == Lead.Values.SPO2 && p.Cardiac_Rhythm.HasPulse_Ventricular)
                Overwrite (Draw.SPO2_Rhythm (p, 1f));
            else if (Lead.Value == Lead.Values.ABP) {
                if (p.IABP_Active)
                    Overwrite (Draw.IABP_ABP_Rhythm (p, 1f));
                else if (p.Cardiac_Rhythm.HasPulse_Ventricular)
                    Overwrite (Draw.ABP_Rhythm (p, 1f));
            } else if (Lead.Value == Lead.Values.PA && p.Cardiac_Rhythm.HasPulse_Ventricular) {

                // Vary PA waveforms based on PA catheter placement
                if (p.PulmonaryArtery_Placement.Value == PulmonaryArtery_Rhythms.Values.Right_Ventricle)
                    Overwrite (Draw.RV_Rhythm (p, 1f));
                else if (p.PulmonaryArtery_Placement.Value == PulmonaryArtery_Rhythms.Values.Pulmonary_Artery)
                    Overwrite (Draw.PA_Rhythm (p, 1f));
                else if (p.PulmonaryArtery_Placement.Value == PulmonaryArtery_Rhythms.Values.Pulmonary_Capillary_Wedge)
                    Overwrite (Draw.PCW_Rhythm (p, 1f));
            } else if (Lead.Value == Lead.Values.ICP && p.Cardiac_Rhythm.HasPulse_Ventricular)
                Overwrite (Draw.ICP_Rhythm (p, 1f));
            else if (Lead.Value == Lead.Values.IAP && p.Cardiac_Rhythm.HasPulse_Ventricular)
                Overwrite (Draw.IAP_Rhythm (p, 1f));

            if (Lead.Value == Lead.Values.IABP && p.IABP_Active) {
                if (p.Cardiac_Rhythm.HasWaveform_Ventricular && p.IABP_Trigger == "ECG") {

                    // ECG Trigger works only if ventricular ECG waveform
                    Overwrite (Draw.IABP_Balloon_Rhythm (p, 1f));
                } else if (p.Cardiac_Rhythm.HasPulse_Ventricular && p.IABP_Trigger == "Pressure") {

                    // Pressure Trigger works only if ventricular pressure impulse
                    Overwrite (Draw.IABP_Balloon_Rhythm (p, 1f));
                }
            }
        }

        public void Add_Beat__Respiratory_Baseline (Patient p) {
            SetForwardBuffer (p);

            if (Lead.Value == Lead.Values.RR || Lead.Value == Lead.Values.ETCO2) {

                // Fill waveform through to future buffer with flatline
                double fill = (lengthSeconds * forwardBuffer) - Last (Points).X;
                Concatenate (Draw.Waveform_Flatline (fill > p.GetRR_Seconds ? fill : p.GetRR_Seconds, 0f));
            }
        }

        public void Add_Beat__Respiratory_Inspiration (Patient p) {
            switch (Lead.Value) {
                default: break;
                case Lead.Values.RR: Overwrite (Draw.RR_Rhythm (p, true)); break;
                case Lead.Values.ETCO2: break;    // End-tidal waveform is only present on expiration!! Is flatline on inspiration.
            }
        }

        public void Add_Beat__Respiratory_Expiration (Patient p) {
            switch (Lead.Value) {
                default: break;
                case Lead.Values.RR: Overwrite (Draw.RR_Rhythm (p, false)); break;
                case Lead.Values.ETCO2: Overwrite (Draw.ETCO2_Rhythm (p)); break;
            }
        }
    }
}