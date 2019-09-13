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

        public Leads Lead;
        public List<Point> Points;

        public Strip (double length, Leads.Values lead) {
            Lead = new Leads (lead);
            lengthSeconds = length;
            Points = new List<Point> ();
        }

        public Strip (Leads.Values lead) {
            Lead = new Leads (lead);
            lengthSeconds = IsRespiratory ? DefaultLength * DefaultRespiratoryCoefficient : DefaultLength;
            Points = new List<Point> ();
        }

        private bool IsECG {
            get {
                return Lead.Value == Leads.Values.ECG_I || Lead.Value == Leads.Values.ECG_II
                    || Lead.Value == Leads.Values.ECG_III || Lead.Value == Leads.Values.ECG_AVR
                    || Lead.Value == Leads.Values.ECG_AVL || Lead.Value == Leads.Values.ECG_AVF
                    || Lead.Value == Leads.Values.ECG_V1 || Lead.Value == Leads.Values.ECG_V2
                    || Lead.Value == Leads.Values.ECG_V3 || Lead.Value == Leads.Values.ECG_V4
                    || Lead.Value == Leads.Values.ECG_V5 || Lead.Value == Leads.Values.ECG_V6;
            }
        }

        private bool IsCardiac {
            get {
                return Lead.Value == Leads.Values.ECG_I || Lead.Value == Leads.Values.ECG_II
                    || Lead.Value == Leads.Values.ECG_III || Lead.Value == Leads.Values.ECG_AVR
                    || Lead.Value == Leads.Values.ECG_AVL || Lead.Value == Leads.Values.ECG_AVF
                    || Lead.Value == Leads.Values.ECG_V1 || Lead.Value == Leads.Values.ECG_V2
                    || Lead.Value == Leads.Values.ECG_V3 || Lead.Value == Leads.Values.ECG_V4
                    || Lead.Value == Leads.Values.ECG_V5 || Lead.Value == Leads.Values.ECG_V6
                    || Lead.Value == Leads.Values.ABP
                    || Lead.Value == Leads.Values.CVP
                    || Lead.Value == Leads.Values.IABP
                    || Lead.Value == Leads.Values.IAP
                    || Lead.Value == Leads.Values.ICP
                    || Lead.Value == Leads.Values.PA
                    || Lead.Value == Leads.Values.SPO2;
            }
        }

        private bool IsRespiratory {
            get {
                return Lead.Value == Leads.Values.ETCO2
                    || Lead.Value == Leads.Values.RR;
            }
        }

        public void SetLead (Leads.Values lead) {
            Lead = new Leads (lead);
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
                Overwrite (Waveforms.ECG_Defibrillation (p, Lead));
        }

        public void Add_Beat__Cardiac_Pacemaker (Patient p) {
            if (IsECG)
                Overwrite (Waveforms.ECG_Pacemaker (p, Lead));
        }

        public void Add_Beat__Cardiac_Baseline (Patient p) {
            SetForwardBuffer (p);

            if (IsECG) {
                p.Cardiac_Rhythm.ECG_Isoelectric (p, this);
            } else if (Lead.Value != Leads.Values.RR && Lead.Value != Leads.Values.ETCO2) {

                // Fill waveform through to future buffer with flatline
                double fill = (lengthSeconds * forwardBuffer) - Last (Points).X;
                Concatenate (Waveforms.Waveform_Flatline (fill > p.GetHR_Seconds ? fill : p.GetHR_Seconds, 0f));
            }

            if (Lead.Value == Leads.Values.CVP
                || (Lead.Value == Leads.Values.PA   // PA catheter in RA has CVP waveform
                    && p.PulmonaryArtery_Placement.Value == PulmonaryArtery_Rhythms.Values.Right_Atrium)) {
                if (p.Cardiac_Rhythm.HasPulse_Atrial && !p.Cardiac_Rhythm.HasPulse_Ventricular)
                    Overwrite (Waveforms.CVP_Rhythm (p, 0.25f));
                else if (!p.Cardiac_Rhythm.HasPulse_Atrial && p.Cardiac_Rhythm.HasPulse_Ventricular)
                    Overwrite (Waveforms.CVP_Rhythm (p, 0.5f));
                else if (p.Cardiac_Rhythm.HasPulse_Atrial && p.Cardiac_Rhythm.HasPulse_Ventricular)
                    Overwrite (Waveforms.CVP_Rhythm (p, 1f));
            }
        }

        public void Add_Beat__Cardiac_Atrial (Patient p) {
            if (IsECG)
                p.Cardiac_Rhythm.ECG_Atrial (p, this);
        }

        public void Add_Beat__Cardiac_Ventricular (Patient p) {
            if (IsECG)
                p.Cardiac_Rhythm.ECG_Ventricular (p, this);
            else if (Lead.Value == Leads.Values.SPO2 && p.Cardiac_Rhythm.HasPulse_Ventricular)
                Overwrite (Waveforms.SPO2_Rhythm (p, 1f));
            else if (Lead.Value == Leads.Values.ABP) {
                if (p.IABP_Active)
                    Overwrite (Waveforms.IABP_ABP_Rhythm (p, 1f));
                else if (p.Cardiac_Rhythm.HasPulse_Ventricular)
                    Overwrite (Waveforms.ABP_Rhythm (p, 1f));
            } else if (Lead.Value == Leads.Values.PA && p.Cardiac_Rhythm.HasPulse_Ventricular) {

                // Vary PA waveforms based on PA catheter placement
                if (p.PulmonaryArtery_Placement.Value == PulmonaryArtery_Rhythms.Values.Right_Ventricle)
                    Overwrite (Waveforms.RV_Rhythm (p, 1f));
                else if (p.PulmonaryArtery_Placement.Value == PulmonaryArtery_Rhythms.Values.Pulmonary_Artery)
                    Overwrite (Waveforms.PA_Rhythm (p, 1f));
                else if (p.PulmonaryArtery_Placement.Value == PulmonaryArtery_Rhythms.Values.Pulmonary_Capillary_Wedge)
                    Overwrite (Waveforms.PCW_Rhythm (p, 1f));
            } else if (Lead.Value == Leads.Values.ICP && p.Cardiac_Rhythm.HasPulse_Ventricular)
                Overwrite (Waveforms.ICP_Rhythm (p, 1f));
            else if (Lead.Value == Leads.Values.IAP && p.Cardiac_Rhythm.HasPulse_Ventricular)
                Overwrite (Waveforms.IAP_Rhythm (p, 1f));

            if (Lead.Value == Leads.Values.IABP && p.IABP_Active) {
                if (p.Cardiac_Rhythm.HasWaveform_Ventricular && p.IABP_Trigger == "ECG") {

                    // ECG Trigger works only if ventricular ECG waveform
                    Overwrite (Waveforms.IABP_Balloon_Rhythm (p, 1f));
                } else if (p.Cardiac_Rhythm.HasPulse_Ventricular && p.IABP_Trigger == "Pressure") {

                    // Pressure Trigger works only if ventricular pressure impulse
                    Overwrite (Waveforms.IABP_Balloon_Rhythm (p, 1f));
                }
            }
        }

        public void Add_Beat__Respiratory_Baseline (Patient p) {
            SetForwardBuffer (p);

            if (Lead.Value == Leads.Values.RR || Lead.Value == Leads.Values.ETCO2) {

                // Fill waveform through to future buffer with flatline
                double fill = (lengthSeconds * forwardBuffer) - Last (Points).X;
                Concatenate (Waveforms.Waveform_Flatline (fill > p.GetRR_Seconds ? fill : p.GetRR_Seconds, 0f));
            }
        }

        public void Add_Beat__Respiratory_Inspiration (Patient p) {
            switch (Lead.Value) {
                default: break;
                case Leads.Values.RR: Overwrite (Waveforms.RR_Rhythm (p, true)); break;
                case Leads.Values.ETCO2: break;    // End-tidal waveform is only present on expiration!! Is flatline on inspiration.
            }
        }

        public void Add_Beat__Respiratory_Expiration (Patient p) {
            switch (Lead.Value) {
                default: break;
                case Leads.Values.RR: Overwrite (Waveforms.RR_Rhythm (p, false)); break;
                case Leads.Values.ETCO2: Overwrite (Waveforms.ETCO2_Rhythm (p)); break;
            }
        }
    }
}