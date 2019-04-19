using System;
using System.Collections.Generic;

namespace II.Rhythm {

    public class Strip {

        public double lengthSeconds = 5.0d;               // Strip length in seconds
        double forwardEdgeBuffer = 1.1d;                  // Coefficient of Length to draw into future as "now" for buffer
        DateTime scrolledLast = DateTime.UtcNow;
        bool scrollingUnpausing = false;

        public Leads Lead;
        public List<Point> Points;

        public Strip (double l, Leads.Values v) {
            Lead = new Leads(v);
            lengthSeconds = l;
            Points = new List<Point> ();
        }

        public void Reset () {
            Points.Clear ();
        }

        public void ClearFuture () {
            for (int i = Points.Count - 1; i >= 0; i--) {
                if (Points[i].X > lengthSeconds)
                    Points.RemoveAt (i);
            }
        }

        public Point Last (List<Point> _In) {
            if (_In.Count < 1)
                // New vectors are added to Points beginning at 5 seconds to marquee backwards to 0
                return new Point (lengthSeconds, 0);
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
                _Replacement[i].X += lengthSeconds * forwardEdgeBuffer;

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
                _Replacement[i].X += lengthSeconds * forwardEdgeBuffer;

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

        public void RemoveNull() {
            for (int i = Points.Count - 1; i >= 0; i--)
                if (Points[i] == null)
                    Points.RemoveAt (i);
        }

        public void Sort() {
            Points.Sort (delegate (Point p1, Point p2) {
                if (p1 == null && p2 == null) return 0;
                else if (p1 == null) return -1;
                else if (p2 == null) return 1;
                else return p1.X.CompareTo (p2.X); });
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
                Points[i].X -= scrollBy;

            for (int i = Points.Count - 1; i >= 0; i--) {
                if (Points[i].X < -lengthSeconds)
                    Points.RemoveAt (i);
            }
        }

        public void Unpause() {
            scrollingUnpausing = true;
        }

        public void Add_Beat__Cardiac_Defibrillation (Patient _Patient) {
            if (IsECG ())
                Overwrite (Waveforms.ECG_Defibrillation (_Patient, Lead));
        }

        public void Add_Beat__Cardiac_Pacemaker (Patient _Patient) {
            if (IsECG ())
                Overwrite (Waveforms.ECG_Pacemaker (_Patient, Lead));
        }

        public void Add_Beat__Cardiac_Baseline (Patient _Patient) {
            if (IsECG ()) {
                _Patient.CardiacRhythm.ECG_Isoelectric (_Patient, this);
            } else if (Lead.Value != Leads.Values.RR && Lead.Value != Leads.Values.ETCO2) {
                // Fill waveform through to future buffer with flatline
                double fill = (lengthSeconds * forwardEdgeBuffer) - Last (Points).X;
                Concatenate (Waveforms.Waveform_Flatline (fill > _Patient.HR_Seconds ? fill : _Patient.HR_Seconds, 0f));
            }

            if (Lead.Value == Leads.Values.CVP) {
                if (_Patient.CardiacRhythm.HasPulse_Atrial && !_Patient.CardiacRhythm.HasPulse_Ventricular)
                    Overwrite (Waveforms.CVP_Rhythm (_Patient, 0.25f));
                else if (!_Patient.CardiacRhythm.HasPulse_Atrial && _Patient.CardiacRhythm.HasPulse_Ventricular)
                    Overwrite (Waveforms.CVP_Rhythm (_Patient, 0.5f));
                else if (_Patient.CardiacRhythm.HasPulse_Atrial && _Patient.CardiacRhythm.HasPulse_Ventricular)
                    Overwrite (Waveforms.CVP_Rhythm (_Patient,
                        _Patient.CardiacRhythm.AberrantBeat ? 0.5f : 1f));
            }
        }

        public void Add_Beat__Cardiac_Atrial (Patient _Patient) {
            if (IsECG ())
                _Patient.CardiacRhythm.ECG_Atrial (_Patient, this);
        }

        public void Add_Beat__Cardiac_Ventricular (Patient _Patient) {
            if (IsECG ())
                _Patient.CardiacRhythm.ECG_Ventricular (_Patient, this);
            else if (Lead.Value == Leads.Values.SPO2 && _Patient.CardiacRhythm.HasPulse_Ventricular)
                Overwrite (Waveforms.SPO2_Rhythm (_Patient,
                    _Patient.CardiacRhythm.AberrantBeat ? 0.5f : 1f));
            else if (Lead.Value == Leads.Values.ABP) {
                if (_Patient.IABP_Active)
                    Overwrite (Waveforms.IABP_ABP_Rhythm (_Patient, 1f));
                else if (_Patient.CardiacRhythm.HasPulse_Ventricular)
                    Overwrite (Waveforms.ABP_Rhythm (_Patient,
                        _Patient.CardiacRhythm.AberrantBeat ? 0.5f : 1f));
            } else if (Lead.Value == Leads.Values.PA && _Patient.CardiacRhythm.HasPulse_Ventricular)
                Overwrite (Waveforms.PA_Rhythm (_Patient,
                    _Patient.CardiacRhythm.AberrantBeat ? 0.5f : 1f));
            else if (Lead.Value == Leads.Values.ICP && _Patient.CardiacRhythm.HasPulse_Ventricular)
                Overwrite(Waveforms.ICP_Rhythm(_Patient,
                    _Patient.CardiacRhythm.AberrantBeat ? 0.5f : 1f));
            else if (Lead.Value == Leads.Values.IAP && _Patient.CardiacRhythm.HasPulse_Ventricular)
                Overwrite(Waveforms.IAP_Rhythm(_Patient,
                    _Patient.CardiacRhythm.AberrantBeat ? 0.5f : 1f));

            if (Lead.Value == Leads.Values.IABP && _Patient.IABP_Active) {
                if (_Patient.CardiacRhythm.HasWaveform_Ventricular && _Patient.IABP_Trigger == "ECG") {
                    // ECG Trigger works only if ventricular ECG waveform
                    Overwrite (Waveforms.IABP_Balloon_Rhythm (_Patient, 1f));
                } else if (_Patient.CardiacRhythm.HasPulse_Ventricular && _Patient.IABP_Trigger == "Pressure") {
                    // Pressure Trigger works only if ventricular pressure impulse
                    Overwrite (Waveforms.IABP_Balloon_Rhythm (_Patient, 1f));
                }
            }
        }

        public void Add_Beat__Respiratory_Baseline (Patient _Patient) {
            if (Lead.Value == Leads.Values.RR || Lead.Value == Leads.Values.ETCO2) {
                // Fill waveform through to future buffer with flatline
                double fill = (lengthSeconds * forwardEdgeBuffer) - Last (Points).X;
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

        private bool IsECG() {
            return Lead.Value ==  Leads.Values.ECG_I || Lead.Value ==  Leads.Values.ECG_II
                || Lead.Value ==  Leads.Values.ECG_III || Lead.Value ==  Leads.Values.ECG_AVR
                || Lead.Value ==  Leads.Values.ECG_AVL || Lead.Value ==  Leads.Values.ECG_AVF
                || Lead.Value ==  Leads.Values.ECG_V1 || Lead.Value ==  Leads.Values.ECG_V2
                || Lead.Value ==  Leads.Values.ECG_V3 || Lead.Value ==  Leads.Values.ECG_V4
                || Lead.Value ==  Leads.Values.ECG_V5 || Lead.Value ==  Leads.Values.ECG_V6;
        }
    }
}