/* Waveform.Plotting.cs
 * Infirmary Integrated
 * By Ibi Keller (Tanjera), (c) 2023
 */

using System.Collections.Generic;
using II.Drawing;

namespace II.Waveform {
    public static class Plotting {
        /*
	     * Shaping and point plotting functions
	     */

        public static PointD Last (List<PointD> _Original) {
            if (_Original.Count < 1)
                return new PointD (0, 0);
            else
                return _Original [_Original.Count - 1];
        }

        public static List<PointD> Multiply (List<PointD> _Original, double _Coeff) {
            for (int i = 0; i < _Original.Count; i++)
                _Original [i] = new PointD (_Original [i].X, _Original [i].Y * _Coeff);

            return _Original;
        }

        public static List<PointD> Concatenate (List<PointD> _Original, List<PointD> _Addition, double _Amplitude = 1d) {
            Concatenate (ref _Original, _Addition, _Amplitude);
            return _Original;
        }

        public static void Concatenate (ref List<PointD> _Original, List<PointD> _Addition, double _Amplitude = 1d) {
            /* Offsets the X value of a Point[] so that it can be placed at the end
             * of an existing Point[] and continue from that point on. */

            // Nothing to add? Return something.
            if ((_Original.Count == 0 && _Addition.Count == 0) || (_Addition.Count == 0))
                return;

            double _Offset = 0d;
            if (_Original.Count == 0)
                _Offset = 0;
            else if (_Original.Count > 0)
                _Offset = _Original [_Original.Count - 1].X;

            foreach (PointD eachVector in _Addition)
                _Original.Add (new PointD (
                    eachVector.X + _Offset,
                    eachVector.Y * _Amplitude));
        }

        public static List<PointD> Stretch (Dictionary.Plot _Addition, double _Length) {
            int lengthAddition = (_Addition.Vertices.Length - 1) * _Addition.DrawResolution;
            double lengthCoeff = (_Length * 1000) / lengthAddition;

            List<PointD> _Output = new ();
            for (int i = 0; i < _Addition.Vertices.Length; i++)
                _Output.Add (new PointD (
                    (double)_Addition.DrawResolution / 1000 * i * lengthCoeff,
                    _Addition.Vertices [i]));

            return _Output;
        }

        public static List<PointD> Stretch_BySystoleRatio (Dictionary.Plot _Addition, Physiology _P) {
            // Interpolate desired length of waveform in seconds to draw from:
            // Patient model's systolic time compared to waveform systolic time
            // extrapolated 't' to ratio of waveform length versus waveform's systolic portion of length 
            
            int ptSystole = (int)(_P.GetSystoleSeconds * 1000d);

            double length;

            if (_Addition.SystoleLength == 0 || ptSystole == 0) {   // Upstream error
                length = _P.GetHRInterval;
            } else {
                double ratioPatientToPlot = (double)ptSystole / (double)_Addition.SystoleLength;
                length = _Addition.LengthSeconds * ratioPatientToPlot;
            }

            return Stretch (_Addition, length);
        }

        public static List<PointD> Normalize (List<PointD> _Addition, double _Min, double max) {
            if (_Addition.Count == 0)
                return new List<PointD> ();

            double oldMin = _Addition [0].Y,
                  oldMax = _Addition [0].Y;

            // Obtain existing minimum and maximum
            for (int i = 0; i < _Addition.Count; i++) {
                oldMin = (_Addition [i].Y < oldMin) ? _Addition [i].Y : oldMin;
                oldMax = (_Addition [i].Y > oldMax) ? _Addition [i].Y : oldMax;
            }

            // Rescale (min-max normalization) of vertex set
            List<PointD> output = new ();
            for (int i = 0; i < _Addition.Count; i++) {
                output.Add (new PointD (
                    _Addition [i].X,
                    (_Min + (((_Addition [i].Y - oldMin) * (max - _Min)) / (oldMax - oldMin)))));
            }

            return output;
        }

        public static List<PointD> Convert (Dictionary.Plot _Addition) {
            List<PointD> _Output = new ();
            for (int i = 0; i < _Addition.Vertices.Length; i++)
                _Output.Add (new PointD (
                    (double)_Addition.DrawResolution / 1000 * i,
                    _Addition.Vertices [i]));

            return _Output;
        }

        public static double Slope (PointD _P1, PointD _P2) {
            return ((_P2.Y - _P1.Y) / (_P2.X - _P1.X));
        }

        public static PointD Bezier (PointD _Start, PointD _Control, PointD _End, double percent) {
            return (_Start * ((1 - percent) * (1 - percent)))
                + (_Control * (2 * percent * (1 - percent)))
                + (_End * (percent * percent));
        }

        public static List<PointD> Curve (int drawResolution, double length, double mVMiddle, double mVEnd, PointD start) {
            if (length < 0)
                return new List<PointD> ();

            int i;
            double x;
            List<PointD> result = new ();
            double resolution = (2 * (drawResolution / 1000d)) / length;

            for (i = 1; (x = i * resolution) <= 1; i++)
                result.Add (Bezier (new PointD (0, start.Y), new PointD (length / 4, mVMiddle), new PointD (length / 2, mVMiddle), x));

            for (i = 1; (x = i * resolution) <= 1; i++)
                result.Add (Bezier (new PointD (length / 2, mVMiddle), new PointD (length / 4 * 3, mVMiddle), new PointD (length, mVEnd), x));

            result.Add (new PointD (length, mVEnd));        // Finish the curve

            return result;
        }

        public static List<PointD> Peak (int drawResolution, double length, double mV, double mVEnd, PointD start) {
            if (length < 0)
                return new List<PointD> ();

            int i;
            double x;
            List<PointD> result = new ();
            
            double resolution = (2 * (drawResolution / 1000d)) / length;

            for (i = 1; (x = i * resolution) <= 1; i++)
                result.Add (Bezier (new PointD (0, start.Y), new PointD (length / 3, (double)mV / 1), new PointD (length / 2, mV), x));

            for (i = 1; (x = i * resolution) <= 1; i++)
                result.Add (Bezier (new PointD (length / 2, mV), new PointD (length / 5 * 3, mV / 1), new PointD (length, mVEnd), x));

            result.Add (new PointD (length, mVEnd));        // Finish the curve

            return result;
        }

        public static List<PointD> Line (int drawResolution, double length, double mV, PointD start) {
            if (length < 0)
                return new List<PointD> ();

            List<PointD> result = new ();
            PointD begin = new (0, start.Y);
            PointD end = new (length, mV);

            for (double x = 0; x <= length; x += (drawResolution / 1000d))
                result.Add (PointD.Lerp (begin, end, x / length));

            return result;
        }
    }
}