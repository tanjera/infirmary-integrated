/* Strip.cs
 * Infirmary Integrated
 * By Ibi Keller (Tanjera), (c) 2017
 *
 * Actual management of geometric plotting of waveforms as a single tracing is in Strip.cs.
 * Concatenation of waveform complexes, overwriting and underwriting (combining) wave points,
 * marquee scrolling of the waveforms, managing and resizing a future edge buffer (important
 * for continuity of drawing) and cleaning old data points from data collections.
 */

using System;
using System.Collections.Generic;
using System.Drawing;

using II.Waveform;

namespace II.Rhythm {
    public class Strip {
        /* Default variables for easy modification of multiple measurement/tracing functions */
        public static float DefaultLength = 6.0f;
        public static float DefaultBufferLength = .2f;
        public static float DefaultRespiratoryCoefficient = 3f;

        /* Default offsets and amplitudes */
        public static float DefaultOffset_ReferenceZero = -0.015f;      // Accounts for line thickness, for clarity

        /* Reference pressures for scaling transduced waveforms based on systolic/diastolic */
        public const int DefaultAutoScale_Iterations = 10;
        public const float ScaleMargin = 0.2f;
        public const int DefaultScaleMin_ABP = 0;
        public const int DefaultScaleMin_PA = -10;
        public const int DefaultScaleMax_ABP = 200;
        public const int DefaultScaleMax_PA = 50;

        /* Variables for real-time strip tracing processing */
        public float Length = 6.0f;                      // Strip length in seconds
        public float DisplayLength = 6.0f;

        private float forwardBuffer = 1.0f;              // Coefficient of Length to draw into future as "now" for buffer
        private DateTime scrolledLast = DateTime.UtcNow;
        private bool scrollingUnpausing = false;

        public Offsets Offset = Offsets.Center;
        public float Amplitude = 1f;

        public bool ScaleAuto;
        public int ScaleMin;                              // For scaling waveforms to pressure limits
        public int ScaleMax;

        /* Data structures for tracing information */
        public Lead Lead;
        public Bitmap Tracing;                             // Waveform tracing in image format
        public List<PointF> Points;                        // Clinical waveform tracing points
        public List<PointF> Reference;                     // Reference line tracing points

        public enum Offsets {
            Center,
            Stretch,
            Scaled
        }

        public Strip (Lead.Values lead) {
            float length = IsRespiratory ? DefaultLength * DefaultRespiratoryCoefficient : DefaultLength;
            Initialize (lead, length, length);
        }

        public Strip (Lead.Values lead, float length)
            => Initialize (lead, length, length);

        public Strip (Lead.Values lead, float length, float displayLength)
            => Initialize (lead, length, displayLength);

        public void Initialize (Lead.Values lead, float length, float displayLength) {
            Lead = new Lead (lead);

            Length = length;
            DisplayLength = displayLength;

            Points = new List<PointF> ();
            Reference = new List<PointF> ();

            SetScale ();
            SetOffset ();
            SetReference ();
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

        public bool CanScale {
            get {
                return Lead.Value == Lead.Values.ABP
                    || Lead.Value == Lead.Values.PA;
            }
        }

        public void SetLead (Lead.Values lead) {
            Lead = new Lead (lead);
            Length = IsRespiratory ? DefaultLength * DefaultRespiratoryCoefficient : DefaultLength;

            SetScale ();
            SetOffset ();
            SetReference ();
        }

        private void SetScale () {
            switch (Lead.Value) {
                default: break;
                case Lead.Values.ABP:
                    ScaleAuto = true;
                    ScaleMin = DefaultScaleMin_ABP;
                    ScaleMax = DefaultScaleMax_ABP;
                    break;

                case Lead.Values.PA:
                    ScaleAuto = false;
                    ScaleMin = DefaultScaleMin_PA;
                    ScaleMax = DefaultScaleMax_PA;
                    break;
            }
        }

        public void SetAutoScale (Patient _P) {
            if (!CanScale || !ScaleAuto)
                return;

            int systolic = 0;
            int diastolic = 0;

            int j = 0;
            for (int i = _P.ListPatientEvents.Count; i > 0 && j < DefaultAutoScale_Iterations;) {
                i--;

                if (_P.ListPatientEvents [i].EventType != Patient.PatientEventTypes.Cardiac_Ventricular_Mechanical)
                    continue;
                else
                    j++;

                switch (Lead.Value) {
                    default: return;
                    case Lead.Values.ABP:
                        systolic += _P.ListPatientEvents [i].Vitals.ASBP;
                        diastolic += _P.ListPatientEvents [i].Vitals.ADBP;
                        break;

                    case Lead.Values.PA:
                        systolic += _P.ListPatientEvents [i].Vitals.PSP;
                        diastolic += _P.ListPatientEvents [i].Vitals.PDP;
                        break;
                }
            }

            diastolic = (diastolic / DefaultAutoScale_Iterations);
            systolic = (systolic / DefaultAutoScale_Iterations);

            ScaleMin = diastolic - (int)(diastolic * ScaleMargin);
            ScaleMax = systolic + (int)(systolic * ScaleMargin);
        }

        private void SetOffset () {
            /* Define yOffset based on lead type; pressure waveforms offset down, electric remains centered */
            switch (Lead.Value) {
                default:
                    Offset = Offsets.Center;
                    Amplitude = 1f;
                    break;

                case Lead.Values.ETCO2:
                case Lead.Values.SPO2:
                case Lead.Values.IABP:
                    Offset = Offsets.Stretch;
                    break;

                case Lead.Values.ABP:
                case Lead.Values.PA:
                    Offset = Offsets.Scaled;
                    break;

                case Lead.Values.CVP:
                case Lead.Values.IAP:
                case Lead.Values.ICP:
                    Offset = Offsets.Center;
                    break;
            }
        }

        private void SetReference () {
            /* Create reference line for specific leads */
            switch (Lead.Value) {
                default: break;

                case Lead.Values.ETCO2:
                case Lead.Values.SPO2:
                    Reference = new List<PointF> () {
                        new PointF(0, (float)DefaultOffset_ReferenceZero),
                        new PointF((float)Length, (float)DefaultOffset_ReferenceZero)};
                    break;

                case Lead.Values.ABP:
                case Lead.Values.PA:
                case Lead.Values.CVP:
                case Lead.Values.IAP:
                case Lead.Values.ICP:
                    break;
            }
        }

        private void SetForwardBuffer (Patient patient, bool onClear = false) {
            /* Set the forward edge buffer (a coefficient of lengthSeconds!) to be the length of 2 beats/breaths */
            if (IsCardiac)
                forwardBuffer = System.Math.Max (1 + (2 * (patient.GetHR_Seconds / Length)),
                    (onClear ? 1.1f : forwardBuffer));
            else if (IsRespiratory)
                forwardBuffer = System.Math.Max (1 + (2 * (patient.GetRR_Seconds / Length)),
                    (onClear ? 1.1f : forwardBuffer));
        }

        public void DecreaseAmplitude ()
            => Amplitude = System.Math.Max (Amplitude - 0.2f, 0.2f);

        public void IncreaseAmplitude ()
            => Amplitude = System.Math.Min (Amplitude + 0.2f, 2.0f);

        public void Reset () {
            Points.Clear ();
        }

        public void ClearFuture (Patient patient) {
            SetForwardBuffer (patient, true);         // Since accounting for forward edge buffer, recalculate

            for (int i = Points.Count - 1; i >= 0; i--) {
                /* Must account for forwardEdgeBuffer... otherwise will cause period of "asystole" */
                if (Points [i].X > (Length * forwardBuffer))
                    Points.RemoveAt (i);
            }
        }

        public PointF Last (List<PointF> _In) {
            if (_In.Count < 1)

                // New vectors are added to Points beginning at 5 seconds to marquee backwards to 0
                return new PointF ((float)Length, 0);
            else
                return _In [_In.Count - 1];
        }

        public void Concatenate (List<PointF> addition) {
            if (addition.Count == 0)
                return;

            float offsetX = Last (Points).X;

            for (int i = 0; i < addition.Count; i++)
                Points.Add (new PointF (addition [i].X + offsetX, addition [i].Y));
        }

        public void Overwrite (List<PointF> replacement) {
            if (replacement.Count == 0)
                return;

            // Inserts into future of strip, which is X offset by Length
            for (int i = 0; i < replacement.Count; i++)
                replacement [i] = new PointF (replacement [i].X + (Length * forwardBuffer), replacement [i].Y);

            double minX = replacement [0].X,
                maxX = replacement [replacement.Count - 1].X;

            Points.RemoveAll (p => { return p.X > minX && p.X < maxX; });
            Points.AddRange (replacement);
        }

        public void Underwrite (List<PointF> replacement) {
            if (replacement.Count == 0)
                return;

            // Inserts into future of strip, which is X offset by Length
            for (int i = 0; i < replacement.Count; i++)
                replacement [i] = new PointF (replacement [i].X + (Length * forwardBuffer), replacement [i].Y);

            double minX = replacement [0].X,
                maxX = replacement [replacement.Count - 1].X;

            Points.RemoveAll (p => { return p.X > minX && p.X < maxX && p.Y == 0d; });
            Points.AddRange (replacement);
        }

        public void TrimPoints ()
            => Points.RemoveAll (p => { return p == null || p.X < -Length; });

        public void SortPoints ()
            => Points.Sort (delegate (PointF p1, PointF p2) {
                if (p1 == null && p2 == null) return 0;
                else if (p1 == null) return -1;
                else if (p2 == null) return 1;
                else return p1.X.CompareTo (p2.X);
            });

        public void Scroll () {
            if (scrollingUnpausing) {
                scrollingUnpausing = false;
                scrolledLast = DateTime.UtcNow;
                return;
            }

            float scrollBy = (float)((DateTime.UtcNow - scrolledLast).TotalMilliseconds / 1000);
            scrolledLast = DateTime.UtcNow;

            for (int i = Points.Count - 1; i >= 0; i--)
                Points [i] = new PointF (Points [i].X - scrollBy, Points [i].Y);
        }

        public void Unpause () {
            scrollingUnpausing = true;
        }

        public List<PointF> Scale (Patient p, List<PointF> addition) {
            if (!CanScale || addition.Count == 0)
                return addition;

            int systolic, diastolic;
            switch (Lead.Value) {
                default: return addition;

                case Lead.Values.ABP:
                    systolic = p.ASBP;
                    diastolic = p.ADBP;
                    break;

                case Lead.Values.PA:
                    systolic = p.PSP;
                    diastolic = p.PDP;
                    break;
            }

            // Get the existing max values; minimum for scaling should be 0
            float min = 0, max = 0;
            for (int i = 1; i < addition.Count; i++)
                max = (max > addition [i].Y) ? max : addition [i].Y;
            max = (min != max) ? max : 1;           // Scaled waveforms should be 0.0 to 1.0

            // Get new min and max vaules for the desired tracing
            float newMin = II.Math.InverseLerp (ScaleMin, ScaleMax, diastolic);
            float newMax = II.Math.InverseLerp (ScaleMin, ScaleMax, systolic);

            // Run the List<PointF> through the normalization equation
            for (int i = 0; i < addition.Count; i++)
                addition [i] = new PointF (addition [i].X, (((addition [i].Y - min) * ((newMax - newMin) / (max - min))) + newMin));

            return addition;
        }

        public void Add_Beat__Cardiac_Baseline (Patient p) {
            SetForwardBuffer (p);
            TrimPoints ();

            if (IsECG) {
                p.Cardiac_Rhythm.ECG_Isoelectric (p, this);
            } else if (CanScale) {
                float fill = (Length * forwardBuffer) - Last (Points).X;
                Concatenate (Scale (p, Draw.Flat_Line (fill > p.GetHR_Seconds ? fill : p.GetHR_Seconds, 0f)));
            } else {
                /* Fill waveform through to future buffer with flatline */
                float fill = (Length * forwardBuffer) - Last (Points).X;
                Concatenate (Draw.Flat_Line (fill > p.GetHR_Seconds ? fill : p.GetHR_Seconds, 0f));
            }

            SortPoints ();
        }

        public void Add_Beat__Cardiac_Atrial_Electrical (Patient p) {
            if (!IsECG)
                return;

            p.Cardiac_Rhythm.ECG_Atrial (p, this);

            SortPoints ();
        }

        public void Add_Beat__Cardiac_Ventricular_Electrical (Patient p) {
            if (!IsECG)
                return;

            p.Cardiac_Rhythm.ECG_Ventricular (p, this);

            SortPoints ();
        }

        public void Add_Beat__Cardiac_Atrial_Mechanical (Patient p) {
            return;
        }

        public void Add_Beat__Cardiac_Ventricular_Mechanical (Patient p) {
            switch (Lead.Value) {
                default: return;

                case Lead.Values.SPO2:
                    Overwrite (Draw.SPO2_Rhythm (p, 1f));
                    break;

                case Lead.Values.ABP:
                    if (p.IABP_Active)
                        Overwrite (Scale (p, Draw.IABP_ABP_Rhythm (p, 1f)));
                    else if (p.Cardiac_Rhythm.HasPulse_Ventricular)
                        Overwrite (Scale (p, Draw.ABP_Rhythm (p, 1f)));
                    break;

                case Lead.Values.CVP:
                    Overwrite (Draw.CVP_Rhythm (p, 1f));
                    break;

                case Lead.Values.PA:    // Vary PA waveforms based on PA catheter placement
                    if (p.PulmonaryArtery_Placement.Value == PulmonaryArtery_Rhythms.Values.Right_Atrium)
                        Overwrite (Scale (p, Draw.CVP_Rhythm (p, 1f)));
                    else if (p.PulmonaryArtery_Placement.Value == PulmonaryArtery_Rhythms.Values.Right_Ventricle)
                        Overwrite (Scale (p, Draw.RV_Rhythm (p, 1f)));
                    else if (p.PulmonaryArtery_Placement.Value == PulmonaryArtery_Rhythms.Values.Pulmonary_Artery)
                        Overwrite (Scale (p, Draw.PA_Rhythm (p, 1f)));
                    else if (p.PulmonaryArtery_Placement.Value == PulmonaryArtery_Rhythms.Values.Pulmonary_Capillary_Wedge)
                        Overwrite (Scale (p, Draw.PCW_Rhythm (p, 1f)));
                    break;

                case Lead.Values.ICP:
                    Overwrite (Draw.ICP_Rhythm (p, 1f));
                    break;

                case Lead.Values.IAP:
                    Overwrite (Draw.IAP_Rhythm (p, 1f));
                    break;
            }

            SortPoints ();
        }

        public void Add_Beat__IABP_Balloon (Patient p) {
            if (Lead.Value != Lead.Values.IABP || !p.IABP_Active)
                return;

            if (p.Cardiac_Rhythm.HasWaveform_Ventricular && p.IABP_Trigger == "ECG") {
                /* ECG Trigger works only if ventricular ECG waveform */
                Overwrite (Draw.IABP_Balloon_Rhythm (p, 1f));
            } else if (p.Cardiac_Rhythm.HasPulse_Ventricular && p.IABP_Trigger == "Pressure") {
                /* Pressure Trigger works only if ventricular pressure impulse */
                Overwrite (Draw.IABP_Balloon_Rhythm (p, 1f));
            }

            SortPoints ();
        }

        public void Add_Beat__Cardiac_Defibrillation (Patient p) {
            if (!IsECG)
                return;

            Overwrite (Draw.ECG_Defibrillation (p, Lead));

            SortPoints ();
        }

        public void Add_Beat__Cardiac_Pacemaker (Patient p) {
            if (!IsECG)
                return;

            Overwrite (Draw.ECG_Pacemaker (p, Lead));

            SortPoints ();
        }

        public void Add_Breath__Respiratory_Baseline (Patient p) {
            SetForwardBuffer (p);
            TrimPoints ();

            if (Lead.Value != Lead.Values.RR && Lead.Value != Lead.Values.ETCO2)
                return;

            /* Fill waveform through to future buffer with flatline */
            float fill = (Length * forwardBuffer) - Last (Points).X;
            Concatenate (Draw.Flat_Line (fill > p.GetRR_Seconds ? fill : p.GetRR_Seconds, 0f));

            SortPoints ();
        }

        public void Add_Breath__Respiratory_Inspiration (Patient p) {
            switch (Lead.Value) {
                default: return;
                case Lead.Values.RR: Overwrite (Draw.RR_Rhythm (p, true)); break;
                case Lead.Values.ETCO2: break;    // End-tidal waveform is only present on expiration!! Is flatline on inspiration.
            }

            SortPoints ();
        }

        public void Add_Breath__Respiratory_Expiration (Patient p) {
            switch (Lead.Value) {
                default: break;
                case Lead.Values.RR: Overwrite (Draw.RR_Rhythm (p, false)); break;
                case Lead.Values.ETCO2: Overwrite (Draw.ETCO2_Rhythm (p)); break;
            }

            SortPoints ();
        }
    }
}