using System;
using System.Collections.Generic;

namespace II.Rhythm {

    public class Point {
        public double X, Y;
        public Point (double x, double y) { X = x; Y = y; }

        public static Point Lerp (Point a, Point b, double t) {
            return new Point (Utility.Lerp(a.X, b.X, t), Utility.Lerp(a.Y, b.Y, t));
        }

        public static Point operator * (double f, Point p) {
            return new Point (p.X * f, p.Y * f);
        }

        public static Point operator + (Point a, Point b) {
            return new Point (a.X + b.X, a.Y + b.Y);
        }
    }

    public static class Waveforms {

        public const double Draw_Resolve = 0.01f;        // Tracing resolution (seconds per drawing point) in seconds
        public const int Draw_Refresh = 17;              // Tracing draw refresh time in milliseconds (60 fps = ~17ms)

        public static List<Point> Waveform_Flatline(double _Length, double _Isoelectric) {
            return Line_Long (_Length, _Isoelectric, new Point (0, _Isoelectric));
        }

        public static List<Point> SPO2_Rhythm(Patient _P, double _Amplitude) {
            double _Portion = _P.HR_Seconds / 4f;

            List<Point> thisBeat = new List<Point>();
            thisBeat = Concatenate (thisBeat, Line (_Portion, 0f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Curve (_Portion, 0.7f * _Amplitude, 0.6f * _Amplitude, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Curve (_Portion * 2, 0.4f * _Amplitude, 0f, Last (thisBeat)));
            return thisBeat;
        }

        public static List<Point> ABP_Rhythm (Patient _P, double _Amplitude) {
            double _Portion = _P.HR_Seconds / 4;

            List<Point> thisBeat = new List<Point> ();
            thisBeat = Concatenate (thisBeat, Line (_Portion, 0f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Curve (_Portion, 0.8f * _Amplitude, 0.7f * _Amplitude, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Curve (_Portion, 0.4f * _Amplitude, 0.1f * _Amplitude, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Line (_Portion, 0f, Last (thisBeat)));
            return thisBeat;
        }

        public static List<Point> CVP_Rhythm (Patient _P, double _Amplitude) {
            double _Portion = _P.HR_Seconds / 5;

            List<Point> thisBeat = new List<Point> ();
            thisBeat = Concatenate (thisBeat, Curve (_Portion, 0f, 0.3f * _Amplitude, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Curve (_Portion, 0.5f * _Amplitude, 0.3f * _Amplitude, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Curve (_Portion, 0.35f * _Amplitude, -0.05f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Curve (_Portion, -0.1f, -0.05f * _Amplitude, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Curve (_Portion, 0.2f * _Amplitude, 0f, Last (thisBeat)));
            return thisBeat;
        }


        public static List<Point> PA_Rhythm (Patient _P, double _Amplitude) {
            /* ABP during normal sinus perfusion is similar to a sine wave leaning right with dicrotic notch
		     */
            double _Portion = _P.HR_Seconds / 4;

            List<Point> thisBeat = new List<Point> ();
            thisBeat = Concatenate (thisBeat, Line (_Portion, 0f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Curve (_Portion, 0.6f * _Amplitude, 0.5f * _Amplitude, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Curve (_Portion, 0.3f * _Amplitude, 0.15f * _Amplitude, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Curve (_Portion, 0.05f * _Amplitude, 0f, Last (thisBeat)));
            return thisBeat;
        }

        public static List<Point> IABP_Balloon_Rhythm (Patient _P, double _Amplitude) {
            double _Portion = _P.HR_Seconds / 20;

            List<Point> thisBeat = new List<Point> ();
            thisBeat = Concatenate (thisBeat, Line (_Portion * 8, 0f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Line (_Portion, _Amplitude, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Line (_Portion * 2, _Amplitude, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Curve (_Portion * 5, 0.5f * _Amplitude, 0.4f * _Amplitude, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Line (_Portion, -0.3f * _Amplitude, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Line (_Portion * 2, -0.3f * _Amplitude, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Line (_Portion, 0f, Last (thisBeat)));
            return thisBeat;
        }

        public static List<Point> IABP_ABP_Rhythm (Patient _P, double _Amplitude) {
            double _Portion = _P.HR_Seconds / 4;

            List<Point> thisBeat = new List<Point> ();

            thisBeat = Concatenate (thisBeat, Line (_Portion, 0f, Last (thisBeat)));

            if (_P.CardiacRhythm.HasPulse_Ventricular)
                thisBeat = Concatenate (thisBeat, Curve (_Portion, 0.8f * _Amplitude, 0.6f * _Amplitude, Last (thisBeat)));
            else
                thisBeat = Concatenate (thisBeat, Line (_Portion, 0f, Last (thisBeat)));

            thisBeat = Concatenate (thisBeat, Curve (_Portion, 1f * _Amplitude, 0f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Curve (_Portion, -0.2f * _Amplitude, 0f, Last (thisBeat)));
            return thisBeat;
        }

        public static List<Point> RR_Rhythm (Patient _P, bool _Inspire) {
            /* Respiratory rate/status indicator; on waveform shows as an open arrow
             */
            double _Portion = 0.1f;

            List<Point> thisBeat = new List<Point> ();
            thisBeat = Concatenate (thisBeat, Line (_Portion, 0.3f * (_Inspire ? 1 : -1), Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Line (_Portion, 0f, Last (thisBeat)));
            return thisBeat;
        }

        public static List<Point> ETCO2_Rhythm (Patient _P) {
            double _Length = _P.RR_Seconds_E,
                    _Portion = 0.2d;

            List<Point> thisBeat = new List<Point> ();
            thisBeat = Concatenate (thisBeat, Curve (_Portion, 0.6f, 0.65f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Line (_Length - (_Portion * 2), 0.7f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Curve (_Portion, 0.7f, 0f, Last (thisBeat)));
            return thisBeat;
        }

        public static List<Point> ECG_Isoelectric__Atrial_Fibrillation (Patient _P, Leads _L) {
            int Fibrillations = (int)Math.Ceiling (_P.HR_Seconds / 0.06);

            List<Point> thisBeat = new List<Point> ();
            for (int i = 1; i < Fibrillations; i++)
                thisBeat = Concatenate (thisBeat, ECG_P (_P, _L, 0.06f, .04f, 0f, Last (thisBeat)));
            return thisBeat;
        }

        public static List<Point> ECG_Isoelectric__Atrial_Flutter (Patient _P, Leads _L) {
            int Flutters = (int)Math.Ceiling(_P.HR_Seconds / 0.16f);

            List<Point> thisBeat = new List<Point> ();
            for (int i = 1; i < Flutters; i++)
                thisBeat = Concatenate (thisBeat, ECG_P (_P, _L, 0.16f, .08f, 0f, Last (thisBeat)));
            return thisBeat;
        }

        public static List<Point> ECG_Complex__P_Normal (Patient _P, Leads _L) {
            double PR = Utility.Lerp (0.16f, 0.2f, Utility.Clamp (Utility.InverseLerp (60, 160, _P.HR)));
            return ECG_P (_P, _L, (PR * 2) / 3, .1f, 0f, new Point (0, 0f));
        }

        public static List<Point> ECG_Complex__QRST_Normal (Patient _P, Leads _L) {
            double lerpCoeff = Utility.Clamp (Utility.InverseLerp (60, 160, _P.HR)),
                QRS = Utility.Lerp (0.08f, 0.12f, lerpCoeff),
                QT = Utility.Lerp (0.235f, 0.4f, lerpCoeff);

            List<Point> thisBeat = new List<Point> ();
            thisBeat = Concatenate (thisBeat, ECG_Q (_P, _L, QRS / 4, -0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_R (_P, _L, QRS / 4, 0.9f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_S (_P, _L, QRS / 4, -0.3f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_J (_P, _L, QRS / 4, -0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_ST (_P, _L, ((QT - QRS) * 2) / 5, 0f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_T (_P, _L, ((QT - QRS) * 3) / 5, 0.2f, 0f, Last (thisBeat)));
            return thisBeat;
        }

        public static List<Point> ECG_Complex__QRST_Aberrant_1 (Patient _P, Leads _L) {
            double lerpCoeff = Utility.Clamp (Utility.InverseLerp (60, 160, _P.HR)),
                QRS = Utility.Lerp (0.12f, 0.26f, lerpCoeff),
                QT = Utility.Lerp (0.235f, 0.6f, lerpCoeff);

            List<Point> thisBeat = new List<Point> ();
            thisBeat = Concatenate (thisBeat, ECG_Q (_P, _L, QRS / 6, 0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_R (_P, _L, QRS / 3, -0.9f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_S (_P, _L, QRS / 6, 0.3f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_J (_P, _L, QRS / 3, 0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_ST (_P, _L, ((QT - QRS) * 2) / 5, 0f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_T (_P, _L, ((QT - QRS) * 3) / 5, 0.1f, 0f, Last (thisBeat)));
            return thisBeat;
        }

        public static List<Point> ECG_Complex__QRST_Aberrant_2 (Patient _P, Leads _L) {
            double lerpCoeff = Utility.Clamp (Utility.InverseLerp (60, 160, _P.HR)),
                QRS = Utility.Lerp (0.11f, 0.25f, lerpCoeff),
                QT = Utility.Lerp (0.25f, 0.6f, lerpCoeff);

            List<Point> thisBeat = new List<Point> ();
            thisBeat = Concatenate (thisBeat, ECG_Q (_P, _L, QRS / 3, -0.8f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_R (_P, _L, QRS / 6, -0.2f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_J (_P, _L, QRS / 6, 0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_ST (_P, _L, ((QT - QRS) * 2) / 5, 0f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_T (_P, _L, ((QT - QRS) * 3) / 5, -0.1f, 0f, Last (thisBeat)));
            return thisBeat;
        }

        public static List<Point> ECG_Complex__QRST_Aberrant_3 (Patient _P, Leads _L) {
            double lerpCoeff = Utility.Clamp (Utility.InverseLerp (60, 160, _P.HR)),
                QRS = Utility.Lerp (0.14f, 0.28f, lerpCoeff),
                QT = Utility.Lerp (0.22f, 0.54f, lerpCoeff);

            List<Point> thisBeat = new List<Point> ();
            thisBeat = Concatenate (thisBeat, ECG_Q (_P, _L, QRS / 3, 0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_R (_P, _L, QRS / 6, -0.7f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_J (_P, _L, QRS / 6, -0.6f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_ST (_P, _L, ((QT - QRS) * 2) / 5, 0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_T (_P, _L, ((QT - QRS) * 3) / 5, 0.2f, 0f, Last (thisBeat)));
            return thisBeat;
        }

        public static List<Point> ECG_Complex__QRST_BBB (Patient _P, Leads _L) {
            double lerpCoeff = Utility.Clamp (Utility.InverseLerp (60, 160, _P.HR)),
                QRS = Utility.Lerp (0.12f, 0.22f, lerpCoeff),
                QT = Utility.Lerp (0.235f, 0.4f, lerpCoeff);

            List<Point> thisBeat = new List<Point> ();
            thisBeat = Concatenate (thisBeat, ECG_Q (_P, _L, QRS / 6, -0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_R (_P, _L, QRS / 6, 0.9f, Last (thisBeat)));

            thisBeat = Concatenate (thisBeat, ECG_Q (_P, _L, QRS / 6, 0.3f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_R (_P, _L, QRS / 6, 0.7f, Last (thisBeat)));

            thisBeat = Concatenate (thisBeat, ECG_S (_P, _L, QRS / 6, -0.3f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_J (_P, _L, QRS / 6, -0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_ST (_P, _L, ((QT - QRS) * 2) / 5, 0f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_T (_P, _L, ((QT - QRS) * 3) / 5, 0.1f, 0f, Last (thisBeat)));
            return thisBeat;
        }

        public static List<Point> ECG_Complex__QRST_SVT (Patient _P, Leads _L) {
            double _Length = _P.HR_Seconds;
            double lerpCoeff = Utility.Clamp (Utility.InverseLerp (160, 240, _P.HR)),
                        QRS = Utility.Lerp (0.05f, 0.12f, lerpCoeff),
                        QT = Utility.Lerp (0.22f, 0.36f, lerpCoeff);

            List<Point> thisBeat = new List<Point> ();
            thisBeat = Concatenate (thisBeat, ECG_Q (_P, _L, QRS / 4, -0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_R (_P, _L, QRS / 4, 0.9f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_S (_P, _L, QRS / 4, -0.3f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_J (_P, _L, QRS / 4, -0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_ST (_P, _L, ((QT - QRS) * 1) / 5, -0.06f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_T (_P, _L, ((QT - QRS) * 2) / 5, 0.15f, 0.06f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, ECG_T (_P, _L, ((QT - QRS) * 2) / 5, 0.08f, 0f, Last (thisBeat)));
            return thisBeat;
        }

        public static List<Point> ECG_Complex__QRST_VT (Patient _P, Leads _L) {
            List<Point> thisBeat = new List<Point> ();

            thisBeat = Concatenate (thisBeat, Curve (_P.HR_Seconds / 4,
                -0.1f * leadCoeff[(int)_L.Value, (int)WavePart.Q],
                -0.2f * leadCoeff[(int)_L.Value, (int)WavePart.Q], Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Curve (_P.HR_Seconds / 4,
                -1f * leadCoeff[(int)_L.Value, (int)WavePart.R],
                -0.3f * leadCoeff[(int)_L.Value, (int)WavePart.R], Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Curve (_P.HR_Seconds / 2,
                0.4f * leadCoeff[(int)_L.Value, (int)WavePart.T],
                0.1f * leadCoeff[(int)_L.Value, (int)WavePart.T],
                Last (thisBeat)));

            return thisBeat;
        }

        public static List<Point> ECG_Complex__QRST_VF (Patient _P, Leads _L, float _Amp) {
            double _Length = _P.HR_Seconds,
                    _Wave = _P.HR_Seconds / 2f,
                    _Amplitude = Utility.RandomDouble (0.3f, 0.6f) * _Amp;

            List<Point> thisBeat = new List<Point> ();
            while (_Length > 0f) {
                thisBeat = Concatenate (thisBeat, Curve (_Wave, _Amplitude, 0f, Last (thisBeat)));
                // Flip the sign of amplitude and randomly crawl larger/smaller, models the
                // flippant waves in v-fib.
                _Amplitude = 0 - Utility.Clamp (Utility.RandomDouble (_Amplitude - 0.1f, _Amplitude + 0.1f), -1f, 1f);
                _Length -= _Wave;
            }
            return thisBeat;
        }

        public static List<Point> ECG_Complex__Idioventricular (Patient _P, Leads _L) {
            double lerpCoeff = Utility.Clamp (Utility.InverseLerp (25, 75, _P.HR)),
                    QRS = Utility.Lerp (0.3f, 0.4f, lerpCoeff),
                    SQ = (_P.HR_Seconds - QRS);

            List<Point> thisBeat = new List<Point> ();
            thisBeat = Concatenate (thisBeat, Curve (QRS / 2,
                1.0f * leadCoeff[(int)_L.Value, (int)WavePart.Q],
                -0.3f * leadCoeff[(int)_L.Value, (int)WavePart.Q], Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Curve (QRS / 2,
                -0.3f * leadCoeff[(int)_L.Value, (int)WavePart.R],
                -0.4f * leadCoeff[(int)_L.Value, (int)WavePart.R], Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Curve (SQ / 3,
                0.1f * leadCoeff[(int)_L.Value, (int)WavePart.T], 0, Last (thisBeat)));
            return thisBeat;
        }


        /*
	     * Shaping and point plotting functions
	     */

        static Point Last (List<Point> _Original) {
            if (_Original.Count < 1)
                return new Point(0, 0);
            else
                return _Original[_Original.Count - 1];
        }
        static List<Point> Concatenate(List<Point> _Original, List<Point> _Addition) {
            // Offsets the X value of a Point[] so that it can be placed at the end
            // of an existing Point[] and continue from that point on.

            // Nothing to add? Return something.
            if (_Original.Count == 0 && _Addition.Count == 0)
                return new List<Point>();
            else if (_Addition.Count == 0)
                return _Original;

            double _Offset = 0f;
            if (_Original.Count == 0)
                _Offset = 0;
            else if (_Original.Count > 0)
                _Offset = _Original[_Original.Count - 1].X;

            foreach (Point eachVector in _Addition)
                _Original.Add(new Point(eachVector.X + _Offset, eachVector.Y));

            return _Original;
        }

        static double Slope (Point _P1, Point _P2) {
            return ((_P2.Y - _P1.Y) / (_P2.X - _P1.X));
        }
        static Point Bezier (Point _Start, Point _Control, Point _End, double _Percent) {
            return (((1 - _Percent) * (1 - _Percent)) * _Start) + (2 * _Percent * (1 - _Percent) * _Control) + ((_Percent * _Percent) * _End);
        }

        static List<Point> Curve(double _Length, double _mV_Middle, double _mV_End, Point _Start) {
            if (_Length < 0)
                return new List<Point> ();

            int i;
            double x;
            List<Point> _Out = new List<Point>();

            for (i = 1; i * ((2 * Draw_Resolve) / _Length) <= 1; i++) {
                x = i * ((2 * Draw_Resolve) / _Length);
                _Out.Add(Bezier(new Point(0, _Start.Y), new Point(_Length / 4, _mV_Middle), new Point(_Length / 2, _mV_Middle), x));
            }

            for (i = 1; i * ((2 * Draw_Resolve) / _Length) <= 1; i++) {
                x = i * ((2 * Draw_Resolve) / _Length);
                _Out.Add(Bezier(new Point(_Length / 2, _mV_Middle), new Point(_Length / 4 * 3, _mV_Middle), new Point(_Length, _mV_End), x));
            }

            _Out.Add(new Point(_Length, _mV_End));        // Finish the curve

            return _Out;
        }

        static List<Point> Peak(double _Length, double _mV, double _mV_End, Point _Start) {
            if (_Length < 0)
                return new List<Point> ();

            int i;
            double x;
            List<Point> _Out = new List<Point>();

            for (i = 1; i * ((2 * Draw_Resolve) / _Length) <= 1; i++) {
                x = i * ((2 * Draw_Resolve) / _Length);
                _Out.Add(Bezier(new Point(0, _Start.Y), new Point(_Length / 3, _mV / 1), new Point(_Length / 2, _mV), x));
            }

            for (i = 1; i * ((2 * Draw_Resolve) / _Length) <= 1; i++) {
                x = i * ((2 * Draw_Resolve) / _Length);
                _Out.Add(Bezier(new Point(_Length / 2, _mV), new Point(_Length / 5 * 3, _mV / 1), new Point(_Length, _mV_End), x));
            }

            _Out.Add(new Point(_Length, _mV_End));        // Finish the curve

            return _Out;
        }

        static List<Point> Line(double _Length, double _mV, Point _Start) {
            if (_Length < 0)
                return new List<Point> ();

            List<Point> _Out = new List<Point>();
            _Out.Add(new Point(_Length, _mV));
            return _Out;
        }

        static List<Point> Line_Long (double _Length, double _mV, Point _Start) {
            if (_Length < 0)
                return new List<Point> ();

            List<Point> _Out = new List<Point> ();
            for (double x = 0; x <= _Length; x += Draw_Resolve)
                _Out.Add (new Point (_Start.X + x, _mV));
            return _Out;
        }


        /*
	     * Individual waveform functions for generating portions of an individual waveform- to be
         * concatenated into a single beat. Default ECG waveform is based on Lead 2 inflections/deflections;
         * other leads use coefficients to scale or reverse inflections/deflections.
         *
         * _S == starting Point
         * _L == length (seconds)
         * _mV == inflection height
         * _mV_End == end deflection height
	     */

        static List<Point> ECG_P(Patient p, Leads l, Point _S) { return ECG_P(p, l, .08f, .15f, 0f, _S); }
        static List<Point> ECG_P(Patient p, Leads l, double _L, double _mV, double _mV_End, Point _S) {
            return Peak(_L, _mV * leadCoeff[(int)l.Value, (int)WavePart.P], _mV_End, _S);
        }

        static List<Point> ECG_Q(Patient p, Leads l, Point _S) { return ECG_Q(p, l, 1f, -.1f, _S); }
        static List<Point> ECG_Q(Patient p, Leads l, double _L, double _mV, Point _S) {
            return Line(_L, _mV * leadCoeff[(int)l.Value, (int)WavePart.Q], _S);
        }

        static List<Point> ECG_R(Patient p, Leads l, Point _S) { return ECG_R(p, l, 1f, .9f, _S); }
        static List<Point> ECG_R(Patient p, Leads l, double _L, double _mV, Point _S) {
            return Line(_L, _mV * leadCoeff[(int)l.Value, (int)WavePart.R], _S);
        }

        static List<Point> ECG_S(Patient p, Leads l, Point _S) { return ECG_S(p, l, 1f, -.3f, _S); }
        static List<Point> ECG_S(Patient p, Leads l, double _L, double _mV, Point _S) {
            return Line(_L, _mV * leadCoeff[(int)l.Value, (int)WavePart.S], _S);
        }

        static List<Point> ECG_J(Patient p, Leads l, Point _S) { return ECG_J(p, l, 1f, -.1f, _S); }
        static List<Point> ECG_J(Patient p, Leads l, double _L, double _mV, Point _S) {
            return Line(_L, (_mV * leadCoeff[(int)l.Value, (int)WavePart.J]) + p.STElevation[(int)l.Value], _S);
        }

        static List<Point> ECG_T(Patient p, Leads l, Point _S) { return ECG_T(p, l, .16f, .3f, 0f, _S); }
        static List<Point> ECG_T(Patient p, Leads l, double _L, double _mV, double _mV_End, Point _S) {
            return Peak(_L, (_mV * leadCoeff[(int)l.Value, (int)WavePart.T]) + p.TElevation[(int)l.Value], _mV_End, _S);
        }

        static List<Point> ECG_PR(Patient p, Leads l, Point _S) { return ECG_PR(p, l, .08f, 0f, _S); }
        static List<Point> ECG_PR(Patient p, Leads l, double _L, double _mV, Point _S) {
            return Line(_L, _mV + leadCoeff[(int)l.Value, (int)WavePart.PR], _S);
        }

        static List<Point> ECG_ST(Patient p, Leads l, Point _S) { return ECG_ST(p, l, .1f, 0f, _S); }
        static List<Point> ECG_ST(Patient p, Leads l, double _L, double _mV, Point _S) {
            return Line(_L, _mV + leadCoeff[(int)l.Value, (int)WavePart.ST] + p.STElevation[(int)l.Value], _S);
        }

        static List<Point> ECG_TP(Patient p, Leads l, Point _S) { return ECG_TP(p, l, .48f, .0f, _S); }
        static List<Point> ECG_TP(Patient p, Leads l, double _L, double _mV, Point _S) {
            return Line(_L, _mV + leadCoeff[(int)l.Value, (int)WavePart.TP], _S);
        }


        /*
         * Coefficients for waveform portions for each lead
         */

        enum WavePart {
            P, Q, R, S, J, T, PR, ST, TP
        }

        static double[,] leadCoeff = new double[,] {
            // P through T are multipliers; segments are additions
            { 0.7f,     0.7f,   0.7f,   0.7f,   0.7f,   0.8f,       0f,     0f,     0f },     // L1
            { 1f,       1f,     1f,     1f,     1f,     1f,         0f,     0f,     0f },     // L2
            { 0.5f,     0.5f,   0.5f,   0.5f,   0.5f,   0.2f,       0f,     0f,     0f },     // L3
            { -1f,      -1f,    -0.8f,  -1f,    -1f,    -0.9f,      0f,     0f,     0f },     // AVR
            { -1f,      0.3f,   0.2f,   0.4f,   0.3f,   0.6f,       0f,     0f,     0f },     // AVL
            { 0.7f,     0.8f,   0.8f,   0.8f,   0.8f,   0.4f,       0f,     0f,     0f },     // AVF
            { 0.2f,     -0.7f,  -1f,    0f,     0f,     0.3f,       0f,     0f,     0f },     // V1
            { 0.2f,     -1.8f,  -1.2f,  0f,     -1f,    1.4f,       0f,     0.1f,   0f },     // V2
            { 0.2f,     -3.0f,  -1.4f,  0f,     0f,     1.8f,       0f,     0.1f,   0f },     // V3
            { 0.7f,     -9.0f,  -0.8f,  0f,     0f,     1.4f,       0f,     0.1f,   0f },     // V4
            { 0.7f,     -10.0f, -0.2f,  0f,     0f,     1.0f,       0f,     0.1f,   0f },     // V5
            { 1f,       -9.0f,  -0.1f,  0f,     0f,     0.8f,       0f,     0f,     0f }      // V6
        };
    }
}