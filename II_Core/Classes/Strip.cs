using System;
using System.Collections.Generic;

namespace II.Rhythm {
    public class Strip {
        public double lengthSeconds = 5.0d;               // Strip length in seconds
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

        private void SetForwardBuffer (Patient patient) {
            // Set the forward edge buffer to be the length (in seconds) of 1.5 beats
            if (IsCardiac)
                forwardBuffer = Utility.Clamp (1 + (1.5 * (patient.HR_Seconds / lengthSeconds)), 1.2d, 20d);
            else if (IsRespiratory)
                forwardBuffer = Utility.Clamp (1 + (1.5 * (patient.RR_Seconds / lengthSeconds)), 1.2d, 20d);
        }

        public void Reset () {
            Points.Clear ();
        }

        public void ClearFuture (Patient patient) {
            SetForwardBuffer (patient);         // Since accounting for forward edge buffer, recalculate

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

        public void Add_Beat__Cardiac_Defibrillation (Patient patient) {
            if (IsECG)
                Overwrite (Waveforms.ECG_Defibrillation (patient, Lead));
        }

        public void Add_Beat__Cardiac_Pacemaker (Patient patient) {
            if (IsECG)
                Overwrite (Waveforms.ECG_Pacemaker (patient, Lead));
        }

        public void Add_Beat__Cardiac_Baseline (Patient patient) {
            SetForwardBuffer (patient);

            if (IsECG) {
                patient.Cardiac_Rhythm.ECG_Isoelectric (patient, this);
            } else if (Lead.Value != Leads.Values.RR && Lead.Value != Leads.Values.ETCO2) {
                // Fill waveform through to future buffer with flatline
                double fill = (lengthSeconds * forwardBuffer) - Last (Points).X;
                Concatenate (Waveforms.Waveform_Flatline (fill > patient.HR_Seconds ? fill : patient.HR_Seconds, 0f));
            }

            if (Lead.Value == Leads.Values.CVP) {
                if (patient.Cardiac_Rhythm.HasPulse_Atrial && !patient.Cardiac_Rhythm.HasPulse_Ventricular)
                    Overwrite (Waveforms.CVP_Rhythm (patient, 0.25f));
                else if (!patient.Cardiac_Rhythm.HasPulse_Atrial && patient.Cardiac_Rhythm.HasPulse_Ventricular)
                    Overwrite (Waveforms.CVP_Rhythm (patient, 0.5f));
                else if (patient.Cardiac_Rhythm.HasPulse_Atrial && patient.Cardiac_Rhythm.HasPulse_Ventricular)
                    Overwrite (Waveforms.CVP_Rhythm (patient, 1f));
            }
        }

        public void Add_Beat__Cardiac_Atrial (Patient patient) {
            if (IsECG)
                patient.Cardiac_Rhythm.ECG_Atrial (patient, this);
        }

        public void Add_Beat__Cardiac_Ventricular (Patient patient) {
            if (IsECG)
                patient.Cardiac_Rhythm.ECG_Ventricular (patient, this);
            else if (Lead.Value == Leads.Values.SPO2 && patient.Cardiac_Rhythm.HasPulse_Ventricular)
                Overwrite (Waveforms.SPO2_Rhythm (patient, 1f));
            else if (Lead.Value == Leads.Values.ABP) {
                if (patient.IABP_Active)
                    Overwrite (Waveforms.IABP_ABP_Rhythm (patient, 1f));
                else if (patient.Cardiac_Rhythm.HasPulse_Ventricular)
                    Overwrite (Waveforms.ABP_Rhythm (patient, 1f));
            } else if (Lead.Value == Leads.Values.PA && patient.Cardiac_Rhythm.HasPulse_Ventricular)
                Overwrite (Waveforms.PA_Rhythm (patient, 1f));
            else if (Lead.Value == Leads.Values.ICP && patient.Cardiac_Rhythm.HasPulse_Ventricular)
                Overwrite (Waveforms.ICP_Rhythm (patient, 1f));
            else if (Lead.Value == Leads.Values.IAP && patient.Cardiac_Rhythm.HasPulse_Ventricular)
                Overwrite (Waveforms.IAP_Rhythm (patient, 1f));

            if (Lead.Value == Leads.Values.IABP && patient.IABP_Active) {
                if (patient.Cardiac_Rhythm.HasWaveform_Ventricular && patient.IABP_Trigger == "ECG") {
                    // ECG Trigger works only if ventricular ECG waveform
                    Overwrite (Waveforms.IABP_Balloon_Rhythm (patient, 1f));
                } else if (patient.Cardiac_Rhythm.HasPulse_Ventricular && patient.IABP_Trigger == "Pressure") {
                    // Pressure Trigger works only if ventricular pressure impulse
                    Overwrite (Waveforms.IABP_Balloon_Rhythm (patient, 1f));
                }
            }
        }

        public void Add_Beat__Respiratory_Baseline (Patient patient) {
            SetForwardBuffer (patient);

            if (Lead.Value == Leads.Values.RR || Lead.Value == Leads.Values.ETCO2) {
                // Fill waveform through to future buffer with flatline
                double fill = (lengthSeconds * forwardBuffer) - Last (Points).X;
                Concatenate (Waveforms.Waveform_Flatline (fill > patient.RR_Seconds ? fill : patient.RR_Seconds, 0f));
            }
        }

        public void Add_Beat__Respiratory_Inspiration (Patient patient) {
            switch (Lead.Value) {
                default: break;
                case Leads.Values.RR: Overwrite (Waveforms.RR_Rhythm (patient, true)); break;
                case Leads.Values.ETCO2: break;    // End-tidal waveform is only present on expiration!! Is flatline on inspiration.
            }
        }

        public void Add_Beat__Respiratory_Expiration (Patient patient) {
            switch (Lead.Value) {
                default: break;
                case Leads.Values.RR: Overwrite (Waveforms.RR_Rhythm (patient, false)); break;
                case Leads.Values.ETCO2: Overwrite (Waveforms.ETCO2_Rhythm (patient)); break;
            }
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
    }
}