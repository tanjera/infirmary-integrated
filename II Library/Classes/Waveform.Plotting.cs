using System;
using System.Collections.Generic;
using System.Text;

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

            List<PointD> _Output = new();
            for (int i = 0; i < _Addition.Vertices.Length; i++)
                _Output.Add (new PointD (
                    (double)_Addition.DrawResolution / 1000 * i * lengthCoeff,
                    _Addition.Vertices [i]));

            return _Output;
        }

        public static List<PointD> Normalize (List<PointD> _Addition, double _Min, double _Max) {
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
            List<PointD> _Output = new();
            for (int i = 0; i < _Addition.Count; i++) {
                _Output.Add (new PointD (
                    _Addition [i].X,
                    (_Min + (((_Addition [i].Y - oldMin) * (_Max - _Min)) / (oldMax - oldMin)))));
            }

            return _Output;
        }

        public static List<PointD> Convert (Dictionary.Plot _Addition) {
            List<PointD> _Output = new();
            for (int i = 0; i < _Addition.Vertices.Length; i++)
                _Output.Add (new PointD (
                    (double)_Addition.DrawResolution / 1000 * i,
                    _Addition.Vertices [i]));

            return _Output;
        }

        public static double Slope (PointD _P1, PointD _P2) {
            return ((_P2.Y - _P1.Y) / (_P2.X - _P1.X));
        }

        public static PointD Bezier (PointD _Start, PointD _Control, PointD _End, double _Percent) {
            return (_Start * ((1 - _Percent) * (1 - _Percent)))
                + (_Control * (2 * _Percent * (1 - _Percent)))
                + (_End * (_Percent * _Percent));
        }

        public static List<PointD> Curve (int DrawResolution, double _Length, double _mV_Middle, double _mV_End, PointD _Start) {
            if (_Length < 0)
                return new List<PointD> ();

            int i;
            double x;
            List<PointD> _Out = new();
            double Resolution = (2 * (DrawResolution / 1000d)) / _Length;

            for (i = 1; (x = i * Resolution) <= 1; i++)
                _Out.Add (Bezier (new PointD (0, _Start.Y), new PointD (_Length / 4, _mV_Middle), new PointD (_Length / 2, _mV_Middle), x));

            for (i = 1; (x = i * Resolution) <= 1; i++)
                _Out.Add (Bezier (new PointD (_Length / 2, _mV_Middle), new PointD (_Length / 4 * 3, _mV_Middle), new PointD (_Length, _mV_End), x));

            _Out.Add (new PointD (_Length, _mV_End));        // Finish the curve

            return _Out;
        }

        public static List<PointD> Peak (int DrawResolution, double _Length, double _mV, double _mV_End, PointD _Start) {
            if (_Length < 0)
                return new List<PointD> ();

            int i;
            double x;
            List<PointD> _Out = new();
            double Resolution = (2 * (DrawResolution / 1000d)) / _Length;

            for (i = 1; (x = i * Resolution) <= 1; i++)
                _Out.Add (Bezier (new PointD (0, _Start.Y), new PointD (_Length / 3, (double)_mV / 1), new PointD (_Length / 2, _mV), x));

            for (i = 1; (x = i * Resolution) <= 1; i++)
                _Out.Add (Bezier (new PointD (_Length / 2, _mV), new PointD (_Length / 5 * 3, _mV / 1), new PointD (_Length, _mV_End), x));

            _Out.Add (new PointD (_Length, _mV_End));        // Finish the curve

            return _Out;
        }

        public static List<PointD> Line (int DrawResolution, double _Length, double _mV, PointD _Start) {
            if (_Length < 0)
                return new List<PointD> ();

            List<PointD> Out = new();
            PointD Start = new(0, _Start.Y);
            PointD End = new(_Length, _mV);

            for (double x = 0; x <= _Length; x += (DrawResolution / 1000d))
                Out.Add (PointD.Lerp (Start, End, x / _Length));

            return Out;
        }
    }
}