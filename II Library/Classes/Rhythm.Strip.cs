/* Strip.cs
 * Infirmary Integrated
 * By Ibi Keller (Tanjera), (c) 2017-2023
 *
 * Actual management of geometric plotting of waveforms as a single tracing is in Strip.cs.
 * Concatenation of waveform complexes, overwriting and underwriting (combining) wave points,
 * marquee scrolling of the waveforms, managing and resizing a future edge buffer (important
 * for continuity of drawing) and cleaning old data points from data collections.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;

using II.Drawing;
using II.Waveform;

namespace II.Rhythm {

    public class Strip {
        /* Default variables for easy modification of multiple measurement/tracing functions */
        public const double DefaultLength = 6.0d;
        public const double DefaultBufferLength = .2d;
        public const double DefaultRespiratoryCoefficient = 3d;

        /* Default offsets and amplitudes */
        public const double DefaultOffset_ReferenceZero = -0.015d;      // Accounts for line thickness, for clarity

        /* Reference pressures for scaling transduced waveforms based on systolic/diastolic */
        public const int DefaultAutoScale_Iterations = 10;
        public const double DefaultScaleMargin = 0.2d;
        public const int DefaultScaleMin_ABP = 0;
        public const int DefaultScaleMin_PA = -10;
        public const int DefaultScaleMax_ABP = 200;
        public const int DefaultScaleMax_PA = 50;

        /* Treat FHR and TOCO similarly to transduced waveforms to prevent auto-scaling */
        public const int DefaultScaleMin_FHR = 30;
        public const int DefaultScaleMax_FHR = 240;
        public const int DefaultScaleMin_TOCO = 0;
        public const int DefaultScaleMax_TOCO = 100;

        /* Variables for real-time strip tracing processing */
        public double Length = 6.0d;                      // Strip length in seconds
        public double DisplayLength = 6.0d;

        /* Coefficients for decreasing tracing resolution (performance vs quality) */
        public int Resolution_Cardiac = 1;
        public int Resolution_Respiratory = 3;
        public int Resolution_Obstetric = 100;

        private double forwardBuffer = 1.0d;              // Coefficient of Length to draw into future as "now" for buffer
        private DateTime scrolledLast = DateTime.UtcNow;
        private bool scrollingUnpausing = false;

        public Offsets Offset = Offsets.Center;
        public double Amplitude = 1d;

        public bool ScaleAuto;
        public double ScaleMargin = DefaultScaleMargin;
        public int ScaleMin;                              // For scaling waveforms to pressure limits
        public int ScaleMax;

        /* Data structures for tracing information */
        public Lead? Lead;
        public readonly object lockPoints = new ();
        public List<PointD>? Points;                      // Clinical waveform tracing points

        public enum Offsets {
            Center,
            Stretch,
            Scaled
        }

        public Strip (Lead.Values lead) {
            double length = IsRespiratory ? DefaultLength * DefaultRespiratoryCoefficient : DefaultLength;
            Initialize (lead, length, length);
        }

        public Strip (Lead.Values lead, double length)
            => Initialize (lead, length, length);

        public Strip (Lead.Values lead, double length, double displayLength)
            => Initialize (lead, length, displayLength);

        public void Initialize (Lead.Values lead, double length, double displayLength) {
            Lead = new Lead (lead);

            Length = length;
            DisplayLength = displayLength;

            lock (lockPoints)
                Points = new List<PointD> ();

            SetScale ();
            SetOffset ();
        }

        private bool IsECG {
            get {
                if (Lead is null)
                    return false;

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
                if (Lead is null)
                    return false;

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
                if (Lead is null)
                    return false;

                return Lead.Value == Lead.Values.ETCO2
                    || Lead.Value == Lead.Values.RR;
            }
        }

        private bool IsObstetric {
            get {
                if (Lead is null)
                    return false;

                return Lead.Value == Lead.Values.FHR
                    || Lead.Value == Lead.Values.TOCO;
            }
        }

        public bool CanScale {
            get {
                if (Lead is null)
                    return false;

                return Lead.Value == Lead.Values.ABP
                    || Lead.Value == Lead.Values.PA;
            }
        }

        public void SetLead (Lead.Values lead) {
            Lead = new Lead (lead);
            Length = IsRespiratory ? DefaultLength * DefaultRespiratoryCoefficient : DefaultLength;

            SetScale ();
            SetOffset ();
        }

        private void SetScale () {
            switch (Lead?.Value) {
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

        public void SetAutoScale (Physiology? _P) {
            if (_P is null || !CanScale || !ScaleAuto)
                return;

            int peak = 0;
            int trough = 0;

            int j = 0;

            lock (_P.lockListPhysiologyEvents) {
                for (int i = _P.ListPhysiologyEvents.Count; i > 0 && j < DefaultAutoScale_Iterations;) {
                    i--;

                    if (_P.ListPhysiologyEvents [i].EventType != Physiology.PhysiologyEventTypes.Cardiac_Ventricular_Electric)
                        continue;
                    else
                        j++;

                    switch (Lead.Value) {
                        default: return;
                        case Lead.Values.ABP:
                            peak += _P.ListPhysiologyEvents [i].Vitals.ASBP;
                            trough += _P.ListPhysiologyEvents [i].Vitals.ADBP;
                            break;

                        case Lead.Values.PA:
                            peak += _P.ListPhysiologyEvents [i].Vitals.PSP;
                            trough += _P.ListPhysiologyEvents [i].Vitals.PDP;
                            break;
                    }
                }
            }

            trough /= DefaultAutoScale_Iterations;
            peak /= DefaultAutoScale_Iterations;

            ScaleMin = trough - (int)(trough * DefaultScaleMargin);
            ScaleMax = peak + (int)(peak * DefaultScaleMargin);
        }

        private void SetOffset () {
            /* Define yOffset based on lead type; pressure waveforms offset down, electric remains centered */
            switch (Lead?.Value) {
                default:
                    Offset = Offsets.Center;
                    Amplitude = 1f;
                    break;

                case Lead.Values.ETCO2:
                case Lead.Values.SPO2:
                case Lead.Values.IABP:
                    Offset = Offsets.Stretch;
                    break;

                case Lead.Values.FHR:
                case Lead.Values.TOCO:
                    Offset = Offsets.Stretch;
                    ScaleMargin = 0.05d;
                    break;

                case Lead.Values.ABP:
                case Lead.Values.PA:
                    Offset = Offsets.Scaled;
                    break;

                case Lead.Values.CVP:
                    Offset = Offsets.Center;
                    Amplitude = 0.5f;
                    break;

                case Lead.Values.IAP:
                case Lead.Values.ICP:
                    Offset = Offsets.Center;
                    break;
            }
        }

        private void SetForwardBuffer (Physiology patient, bool onClear = false) {
            /* Set the forward edge buffer (a coefficient of lengthSeconds!) to be the length of 2 beats/breaths */
            if (IsCardiac)
                forwardBuffer = System.Math.Max (1 + (2 * (patient.GetHRInterval / Length)),
                    (onClear ? 1.1f : forwardBuffer));
            else if (IsRespiratory)
                forwardBuffer = System.Math.Max (1 + (2 * (patient.GetRRInterval / Length)),
                    (onClear ? 1.1f : forwardBuffer));
            else if (IsObstetric)
                forwardBuffer = System.Math.Max (1 + (patient.ObstetricContractionFrequency / Length),
                    (onClear ? 1.1f : forwardBuffer));
        }

        public void DecreaseAmplitude ()
            => Amplitude = System.Math.Max (Amplitude - 0.2d, 0.2d);

        public void IncreaseAmplitude ()
            => Amplitude = System.Math.Min (Amplitude + 0.2d, 2.0d);

        public Task Reset () {
            lock (lockPoints)
                Points?.Clear ();

            return Task.CompletedTask;
        }

        public void ClearFuture (Physiology? patient) {
            if (Points is null || patient is null)
                return;

            SetForwardBuffer (patient, true);         // Since accounting for forward edge buffer, recalculate

            lock (lockPoints) {
                for (int i = Points.Count - 1; i >= 0; i--) {
                    /* Must account for forwardEdgeBuffer... otherwise will cause period of "asystole"
                     * Also **must** have a grace period coefficient for forwardBuffer - otherwise was
                     * erasing waveforms as they were being added to List<PointD> */
                    if (Points [i].X > (Length * forwardBuffer * 1.5))
                        Points.RemoveAt (i);
                }
            }
        }

        public PointD Last (List<PointD>? _In) {
            if (_In is null || _In.Count < 1)
                return new PointD ((double)Length, 0);
            else
                return _In [_In.Count - 1];
        }

        public void Concatenate (List<PointD> addition) {
            if (Points is null || addition.Count == 0)
                return;

            double offsetX = Last (Points).X;
            lock (lockPoints) {
                for (int i = 0; i < addition.Count; i++)
                    Points.Add (new PointD (addition [i].X + offsetX, addition [i].Y));
            }
        }

        // Splices in a set of points, replacing the existing set of points in that time (x-axis)
        public void Replace (List<PointD> splice) {
            if (Points is null || splice.Count == 0)
                return;

            // Offset the splice to meet the leading/future edge of the strip
            for (int i = 0; i < splice.Count; i++)
                splice [i].X += Length * forwardBuffer;

            lock (lockPoints) {
                Points.RemoveAll (p => { return p.X > splice [0].X && p.X < splice.Last ().X; });
                Points.AddRange (splice);
            }
        }

        // Splice in a set of points *after* the Y value supercedes the original strip's Y value (used to transition)
        public void ReplaceAtOver (List<PointD> splice, bool dirUp = true) {
            if (Points is null || splice.Count == 0)
                return;

            // Offset the splice to meet the leading/future edge of the strip
            for (int i = 0; i < splice.Count; i++)
                splice [i].X += Length * forwardBuffer;

            // Sort the splice and the Points so we can walk through them
            splice.Sort (delegate (PointD p1, PointD p2) {
                if (p1 is null && p2 is null) return 0;
                else if (p1 is null) return -1;
                else if (p2 is null) return 1;
                else return p1.X.CompareTo (p2.X);
            });

            if (Points.Count == 0
                || Points.Min (x => x.X) > splice.Last ().X
                || Points.Max (x => x.X) < splice.First ().X) {
                // Utilizes the regular Replace() routine... calling the whole routine would shift the offset again...
                lock (lockPoints) {
                    Points.RemoveAll (p => { return p.X > splice [0].X && p.X < splice.Last ().X; });
                    Points.AddRange (splice);
                }

                return;
            }

            lock (lockPoints) {
                Points.Sort (delegate (PointD p1, PointD p2) {
                    if (p1 is null && p2 is null) return 0;
                    else if (p1 is null) return -1;
                    else if (p2 is null) return 1;
                    else return p1.X.CompareTo (p2.X);
                });

                double spliceStart = splice.First ().X,
                    spliceEnd = splice.Last ().X;

                bool isReplacing = false;
                int lastPoint = 0, lastSplice = 0;

                for (int i = 0, j = 0; i < Points.Count && j < splice.Count; i++) {
                    if (Points [i].X > spliceStart && Points [i].X < spliceEnd) {
                        // Sync i (Points) with j (splice) based on X axis
                        while (Points [i].X > splice [j].X && j < splice.Count - 1) {
                            j++;
                        }

                        // Once splice's Y axis supercedes/overcomes Points' in the set direction, trigger the replace functionality
                        if ((dirUp && splice [j].Y >= Points [i].Y)
                            || (!dirUp && splice [j].Y <= Points [i].Y)) {
                            isReplacing = true;
                            lastPoint = i;
                            lastSplice = j;
                            break;
                        }
                    }
                }

                if (isReplacing) {
                    Points.RemoveAll (p => { return p.X > Points [lastPoint].X && p.X < splice.Last ().X; });
                    splice.RemoveRange (0, lastSplice);
                    Points.AddRange (splice);
                }
            }
        }

        // Splices in a set of points, combining their Y values
        public void Combine (List<PointD> splice) {
            if (Points is null || splice.Count == 0)
                return;

            // Offset the splice to meet the leading/future edge of the strip
            for (int i = 0; i < splice.Count; i++)
                splice [i].X += Length * forwardBuffer;

            // Sort the splice and the Points so we can walk through them
            splice.Sort (delegate (PointD p1, PointD p2) {
                if (p1 is null && p2 is null) return 0;
                else if (p1 is null) return -1;
                else if (p2 is null) return 1;
                else return p1.X.CompareTo (p2.X);
            });

            lock (lockPoints) {
                Points.Sort (delegate (PointD p1, PointD p2) {
                    if (p1 is null && p2 is null) return 0;
                    else if (p1 is null) return -1;
                    else if (p2 is null) return 1;
                    else return p1.X.CompareTo (p2.X);
                });

                double spliceStart = splice.First ().X,
                    spliceEnd = splice.Last ().X;

                double lastCombine = 0d;

                for (int i = Points.Count - 1, j = splice.Count - 1; i >= 0 && j >= 0; i--) {
                    if (Points [i].X > spliceStart && Points [i].X < spliceEnd) {                                               // Several comparisons to account for different draw resolutions (X intervals)
                        if (i > 0 && (System.Math.Abs (Points [i - 1].X - splice [j].X)
                            < System.Math.Abs (Points [i].X - splice [j].X))) {                                                  // If the next Point is closer in X to the current splice
                            Points [i].Y += lastCombine;                                                                        // Add the last splice to this Point as well
                        } else if (j < splice.Count - 1 && (System.Math.Abs (Points [i].X - splice [j + 1].X)                    // If the next Splice is closer in X to the current Point
                            < System.Math.Abs (Points [i].X - splice [j].X))) {
                            i++;                                                                                                // Repeat the comparison with current Point ...
                            j--;                                                                                                // ... against the next splice
                        } else {                                                                                                // If this Point is closest to this splice
                            lastCombine = splice [j].Y;                                                                         // The new combine amount
                            Points [i].Y += splice [j].Y;                                                                       // And combine them
                            j--;                                                                                                // Iterating to the next splice
                        }
                    }
                }
            }
        }

        // Splices in a set of points, only showing the splice if it's Y is larger than the existing
        public void Underwrite (List<PointD> splice) {
            if (Points is null || splice.Count == 0)
                return;

            // Offset the splice to meet the leading/future edge of the strip
            for (int i = 0; i < splice.Count; i++)
                splice [i].X += Length * forwardBuffer;

            // Sort the splice and the Points so we can walk through them
            splice.Sort (delegate (PointD p1, PointD p2) {
                if (p1 is null && p2 is null) return 0;
                else if (p1 is null) return -1;
                else if (p2 is null) return 1;
                else return p1.X.CompareTo (p2.X);
            });

            lock (lockPoints) {
                Points.Sort (delegate (PointD p1, PointD p2) {
                    if (p1 is null && p2 is null) return 0;
                    else if (p1 is null) return -1;
                    else if (p2 is null) return 1;
                    else return p1.X.CompareTo (p2.X);
                });

                double spliceStart = splice.First ().X,
                    spliceEnd = splice.Last ().X;

                bool hadSplice = false;
                double lastSplice = 0d;

                for (int i = Points.Count - 1, j = splice.Count - 1; i >= 0 && j >= 0; i--) {
                    if (Points [i].X > spliceStart && Points [i].X < spliceEnd) {                                               // Several comparisons to account for different draw resolutions (X intervals)
                        if (i > 0 && (System.Math.Abs (Points [i - 1].X - splice [j].X)
                            < System.Math.Abs (Points [i].X - splice [j].X))) {                                                  // If the next Point is closer in X to the current splice
                            Points [i].Y = hadSplice ? lastSplice : Points [i].Y;                                                // Add the last splice to this Point as well
                        } else if (j < splice.Count - 1 && (System.Math.Abs (Points [i].X - splice [j + 1].X)                    // If the next Splice is closer in X to the current Point
                            < System.Math.Abs (Points [i].X - splice [j].X))) {
                            i++;                                                                                                // Repeat the comparison with current Point ...
                            j--;                                                                                                // ... against the next splice
                        } else {                                                                                                // If this Point is closest to this splice
                            if ((Points [i].Y == 0)
                                || (Points [i].Y < 0 && splice [j].Y < 0 && splice [j].Y < Points [i].Y)
                                || (Points [i].Y > 0 && splice [j].Y > 0 && splice [j].Y > Points [i].Y)) {
                                lastSplice = splice [j].Y;
                                hadSplice = true;
                                Points [i].Y = splice [j].Y;
                            } else {
                                hadSplice = false;
                            }

                            j--;                                                                                                // Iterating to the next splice
                        }
                    }
                }
            }
        }

        public void TrimPoints () {
            lock (lockPoints) {
                Points?.RemoveAll (p => { return p is null || p.X < -Length; });
            }
        }

        public void SortPoints () {
            lock (lockPoints) {
                Points?.Sort (delegate (PointD p1, PointD p2) {
                    if (p1 is null && p2 is null) return 0;
                    else if (p1 is null) return -1;
                    else if (p2 is null) return 1;
                    else return p1.X.CompareTo (p2.X);
                });
            }
        }

        public void Scroll (int? multiplier = 1) {
            if (Points is null)
                return;

            if (scrollingUnpausing) {
                scrollingUnpausing = false;
                scrolledLast = DateTime.UtcNow;
                return;
            }

            multiplier ??= 1;

            double scrollBy = (double)(((DateTime.UtcNow - scrolledLast).TotalMilliseconds / 1000) * multiplier);
            scrolledLast = DateTime.UtcNow;

            lock (lockPoints) {
                for (int i = Points.Count - 1; i >= 0; i--)
                    Points [i].X = Points [i].X - scrollBy;
            }
        }

        public void Unpause () {
            scrollingUnpausing = true;
        }

        public List<PointD> Scale (Physiology? p, List<PointD> addition) {
            if (p is null || Lead is null || !CanScale || addition.Count == 0)
                return addition;

            int peak, trough;
            switch (Lead?.Value) {
                default: return addition;

                case Lead.Values.ABP:
                    peak = p.ASBP;
                    trough = p.ADBP;
                    break;

                case Lead.Values.PA:
                    peak = p.PSP;
                    trough = p.PDP;
                    break;
            }

            // Get the existing max values; minimum for scaling should be 0
            double min = 0, max = 0;
            for (int i = 1; i < addition.Count; i++)
                max = (max > addition [i].Y) ? max : addition [i].Y;
            max = (min != max) ? max : 1;           // Scaled waveforms should be 0.0 to 1.0

            // Get new min and max values for the desired tracing
            double newMin = II.Math.InverseLerp (ScaleMin, ScaleMax, trough);
            double newMax = II.Math.InverseLerp (ScaleMin, ScaleMax, peak);

            // Run the List<PointD> through the normalization equation
            for (int i = 0; i < addition.Count; i++)
                addition [i] = new PointD (addition [i].X, (((addition [i].Y - min) * ((newMax - newMin) / (max - min))) + newMin));

            return addition;
        }

        public void Add_Baseline (Physiology? p) {
            if (p is null || Lead is null)
                return;

            if (IsCardiac)
                Add_Beat__Cardiac_Baseline (p);
            else if (IsRespiratory)
                Add_Breath__Respiratory_Baseline (p);
            else if (IsObstetric)
                Add_Beat__Obstetric_Baseline (p);
        }

        public void Add_Beat__Cardiac_Baseline (Physiology? p) {
            if (p is null || Lead is null || !IsCardiac)
                return;

            SetForwardBuffer (p);
            TrimPoints ();

            if (IsECG) {
                // Note: resolution scaling takes place within ECG_Isoelectric()
                p.Cardiac_Rhythm.ECG_Isoelectric (p, this);
            } else if (CanScale) {
                double fill = (Length * forwardBuffer) - Last (Points).X;
                
                // Interpolate HR Interval (.75 - 5 seconds) to a resolution coefficient of 1 - 10
                // w/ default resolution of 10ms: @ HR 60 bpm, resolve @ 10ms, @ ~12 bpm, resolve @ 100ms 
                // Note: high resolution times (e.g. 100ms) cuts into drawing of following concatenation!
                double length = fill > p.GetHRInterval ? fill : p.GetHRInterval;
                double ilerpHRi = Math.Clamp(Math.InverseLerp (.75, 5, length), 0, 1);
                double scaleRes = Math.Lerp(1, 10, ilerpHRi);
                
                Concatenate (Scale (p, Draw.Flat_Line (length, 0d, scaleRes)));
            } else {
                /* Fill waveform through to future buffer with flatline */
                double fill = (Length * forwardBuffer) - Last (Points).X;
                
                // Interpolate HR Interval (.75 - 5 seconds) to a resolution coefficient of 1 - 10
                // w/ default resolution of 10ms: @ HR 60 bpm, resolve @ 10ms, @ ~12 bpm, resolve @ 100ms 
                // Note: high resolution times (e.g. 100ms) cuts into drawing of following concatenation!
                double length = fill > p.GetHRInterval ? fill : p.GetHRInterval;
                double ilerpHRi = Math.Clamp(Math.InverseLerp (.75, 5, length), 0, 1);
                double scaleRes = Math.Lerp(1, 10, ilerpHRi);
                
                Concatenate (Draw.Flat_Line (length, 0d, scaleRes));
            }

            SortPoints ();
        }

        public void Add_Beat__Cardiac_Atrial_Electrical (Physiology? p) {
            if (p is null || Lead is null || !IsECG)
                return;

            p.Cardiac_Rhythm.ECG_Atrial (p, this);

            SortPoints ();
        }

        public void Add_Beat__Cardiac_Ventricular_Electrical (Physiology? p) {
            if (p is null || Lead is null || !IsECG)
                return;

            p.Cardiac_Rhythm.ECG_Ventricular (p, this);

            SortPoints ();
        }

        public void Add_Beat__Cardiac_Atrial_Mechanical (Physiology? p) {
            if (p is null || Lead is null)
                return;

            return;
        }

        public void Add_Beat__Cardiac_Ventricular_Mechanical (Physiology? p) {
            if (p is null || Lead is null)
                return;

            switch (Lead?.Value) {
                default: return;

                case Lead.Values.SPO2:
                    ReplaceAtOver (Draw.SPO2_Rhythm (p, 1d));
                    break;

                case Lead.Values.ABP:
                    if (p.IABP_Active)
                        // IABP causes important downward deflections- do not use ReplaceAtOver!
                        Replace (Scale (p, Draw.IABP_ABP_Rhythm (p, 1d)));
                    else if (p.Cardiac_Rhythm.HasPulse_Ventricular)
                        ReplaceAtOver (Scale (p, Draw.ABP_Rhythm (p, 1d)));
                    break;

                case Lead.Values.CVP:
                    ReplaceAtOver (Draw.CVP_Rhythm (p, 1d));
                    break;

                case Lead.Values.PA:    // Vary PA waveforms based on PA catheter placement
                    if (p.PulmonaryArtery_Placement.Value == PulmonaryArtery_Rhythms.Values.Right_Atrium) {
                        ReplaceAtOver (Scale (p, Draw.CVP_Rhythm (p, 1d)));
                    } else if (p.PulmonaryArtery_Placement.Value == PulmonaryArtery_Rhythms.Values.Right_Ventricle)
                        ReplaceAtOver (Scale (p, Draw.RV_Rhythm (p, 1d)));
                    else if (p.PulmonaryArtery_Placement.Value == PulmonaryArtery_Rhythms.Values.Pulmonary_Artery)
                        ReplaceAtOver (Scale (p, Draw.PA_Rhythm (p, 1d)));
                    else if (p.PulmonaryArtery_Placement.Value == PulmonaryArtery_Rhythms.Values.Pulmonary_Capillary_Wedge)
                        ReplaceAtOver (Scale (p, Draw.PCW_Rhythm (p, 1d)));
                    break;

                case Lead.Values.ICP:
                    ReplaceAtOver (Draw.ICP_Rhythm (p, .3d));
                    break;
            }

            SortPoints ();
        }

        public void Add_Beat__IABP_Balloon (Physiology? p) {
            if (p is null || Lead is null)
                return;

            if (p.IABP_Active) {
                switch (Lead?.Value) {
                    default: return;

                    case Lead.Values.IABP:
                        if (p.Cardiac_Rhythm.HasWaveform_Ventricular && p.IABP_Trigger == Physiology.IABP_Triggers.ECG) {
                            /* ECG Trigger works only if ventricular ECG waveform */
                            // IABP causes important downward deflections- do not use ReplaceAtOver!
                            Replace (Draw.IABP_Balloon_Rhythm (p, 1d));
                        } else if (p.Cardiac_Rhythm.HasPulse_Ventricular && p.IABP_Trigger == Physiology.IABP_Triggers.Pressure) {
                            /* Pressure Trigger works only if ventricular pressure impulse */
                            // IABP causes important downward deflections- do not use ReplaceAtOver!
                            Replace (Draw.IABP_Balloon_Rhythm (p, 1d));
                        }
                        break;

                    case Lead.Values.ABP:
                        if (!p.Cardiac_Rhythm.HasPulse_Ventricular) {
                            // IABP causes important downward deflections- do not use ReplaceAtOver!
                            Replace (Scale (p, Draw.IABP_ABP_Rhythm (p, 1d)));
                        }
                        break;
                }

                SortPoints ();
            }
        }

        public void Add_Beat__Cardiac_Defibrillation (Physiology? p) {
            if (p is null || Lead is null)
                return;

            if (!IsECG)
                return;

            Replace (Draw.ECG_Defibrillation (p, Lead));

            SortPoints ();
        }

        public void Add_Beat__Cardiac_Pacemaker (Physiology? p) {
            if (p is null || Lead is null)
                return;

            if (!IsECG)
                return;

            Replace (Draw.ECG_Pacemaker (p, Lead));

            SortPoints ();
        }

        public void Add_Breath__Respiratory_Baseline (Physiology? p) {
            if (p is null || Lead is null || !IsRespiratory)
                return;

            SetForwardBuffer (p);
            TrimPoints ();

            /* Fill waveform through to future buffer with flatline */
            double fill = (Length * forwardBuffer) - Last (Points).X;
            Concatenate (Draw.Flat_Line (fill > (double)p.GetRRInterval ? fill : (double)p.GetRRInterval, 0d, Resolution_Respiratory));

            SortPoints ();
        }

        public void Add_Breath__Respiratory_Inspiration (Physiology? p) {
            if (p is null || Lead is null)
                return;

            switch (Lead?.Value) {
                default: return;
                case Lead.Values.RR: Replace (Draw.RR_Rhythm (p, true, Resolution_Respiratory)); break;
                case Lead.Values.ETCO2: break;    // End-tidal waveform is only present on expiration!! Is flatline on inspiration.
                
                case Lead.Values.IAP:
                    ReplaceAtOver (Draw.IAP_Rhythm (true, p, 1d));
                    break;
            }

            SortPoints ();
        }

        public void Add_Breath__Respiratory_Expiration (Physiology? p) {
            if (p is null || Lead is null)
                return;

            switch (Lead?.Value) {
                default: break;
                case Lead.Values.RR: Replace (Draw.RR_Rhythm (p, false, Resolution_Respiratory)); break;
                case Lead.Values.ETCO2: Replace (Draw.ETCO2_Rhythm (p)); break;
                
                case Lead.Values.IAP:
                    Replace (Draw.IAP_Rhythm (false, p, -1d));
                    break;
            }

            SortPoints ();
        }

        public void Add_Beat__Obstetric_Baseline (Physiology? p) {
            /* Only TOCO needs to be drawn at baseline, in DeviceEFM */
            if (p is null || Lead is null || !IsObstetric)
                return;

            SetForwardBuffer (p);
            TrimPoints ();

            /* Fill waveform through to future buffer with flatline */
            double fill = (Length * forwardBuffer) - Last (Points).X;

            switch (Lead.Value) {
                default:
                case Lead.Values.FHR:
                    break;

                case Lead.Values.TOCO:
                    Concatenate (Draw.Flat_Line (fill,
                        Math.Clamp (Math.InverseLerp (DefaultScaleMin_TOCO, DefaultScaleMax_TOCO, p.ObstetricUterineRestingTone)),
                         Resolution_Obstetric));
                    break;
            }

            SortPoints ();
        }

        public void Add_Beat__Obstetric_Fetal_Baseline (Physiology? p) {
            /* Only FHR needs to be drawn at baseline, in DeviceEFM */
            if (p is null || Lead is null || !IsObstetric || Lead.Value != Lead.Values.FHR)
                return;

            SetForwardBuffer (p);
            TrimPoints ();

            /* Fill waveform through to future buffer with flatline */
            double fill = (Length * forwardBuffer) - Last (Points).X;

            // Calculate fetal heart rate variability in amplitude, then draw fetal heart rate line for next period of time
            Replace (Draw.FHR_Rhythm (p));

            SortPoints ();
        }

        public void Add_Beat__Obstetric_Contraction_Start (Physiology? p) {
            /* Only TOCO needs to be drawn on ContractionStart, in DeviceEFM */
            if (p is null || Lead is null || Lead.Value != Lead.Values.TOCO)
                return;

            Replace (Draw.TOCO_Rhythm (p));

            SortPoints ();
        }
    }
}