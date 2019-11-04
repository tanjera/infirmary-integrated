using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace II.Waveform {
    public static class Plotting {
        /*
	     * Shaping and point plotting functions
	     */

        public static PointF Last (List<PointF> _Original) {
            if (_Original.Count < 1)
                return new PointF (0, 0);
            else
                return _Original [_Original.Count - 1];
        }

        public static List<PointF> Multiply (List<PointF> _Original, float _Coeff) {
            for (int i = 0; i < _Original.Count; i++)
                _Original [i] = new PointF (_Original [i].X, _Original [i].Y * _Coeff);

            return _Original;
        }

        public static List<PointF> Concatenate (List<PointF> _Original, List<PointF> _Addition, float _Amplitude = 1f) {
            Concatenate (ref _Original, _Addition, _Amplitude);
            return _Original;
        }

        public static void Concatenate (ref List<PointF> _Original, List<PointF> _Addition, float _Amplitude = 1f) {
            /* Offsets the X value of a Point[] so that it can be placed at the end
             * of an existing Point[] and continue from that point on. */

            // Nothing to add? Return something.
            if ((_Original.Count == 0 && _Addition.Count == 0) || (_Addition.Count == 0))
                return;

            float _Offset = 0f;
            if (_Original.Count == 0)
                _Offset = 0;
            else if (_Original.Count > 0)
                _Offset = _Original [_Original.Count - 1].X;

            foreach (PointF eachVector in _Addition)
                _Original.Add (new PointF (
                    eachVector.X + _Offset,
                    eachVector.Y * _Amplitude));
        }

        public static List<PointF> Stretch (Dictionary.Plot _Addition, float _Length) {
            int lengthAddition = (_Addition.Vertices.Length - 1) * _Addition.DrawResolution;
            float lengthCoeff = (_Length * 1000) / lengthAddition;

            List<PointF> _Output = new List<PointF> ();
            for (int i = 0; i < _Addition.Vertices.Length; i++)
                _Output.Add (new PointF (
                    (float)_Addition.DrawResolution / 1000 * i * lengthCoeff,
                    _Addition.Vertices [i]));

            return _Output;
        }

        public static List<PointF> Convert (Dictionary.Plot _Addition) {
            List<PointF> _Output = new List<PointF> ();
            for (int i = 0; i < _Addition.Vertices.Length; i++)
                _Output.Add (new PointF (
                    (float)_Addition.DrawResolution / 1000 * i,
                    _Addition.Vertices [i]));

            return _Output;
        }

        public static float Slope (PointF _P1, PointF _P2) {
            return ((_P2.Y - _P1.Y) / (_P2.X - _P1.X));
        }
        public static PointF Bezier (PointF _Start, PointF _Control, PointF _End, float _Percent) {
            return Math.Add (Math.Add (Math.Multiply (_Start, ((1 - _Percent) * (1 - _Percent))),
                 Math.Multiply (_Control, (2 * _Percent * (1 - _Percent)))),
                 Math.Multiply (_End, (_Percent * _Percent)));
        }

        public static List<PointF> Curve (int DrawResolution, float _Length, float _mV_Middle, float _mV_End, PointF _Start) {
            if (_Length < 0)
                return new List<PointF> ();

            int i;
            float x;
            List<PointF> _Out = new List<PointF> ();
            float Resolution = (2 * (DrawResolution / 1000f)) / _Length;

            for (i = 1; (x = i * Resolution) <= 1; i++)
                _Out.Add (Bezier (new PointF (0, _Start.Y), new PointF (_Length / 4, _mV_Middle), new PointF (_Length / 2, _mV_Middle), x));

            for (i = 1; (x = i * Resolution) <= 1; i++)
                _Out.Add (Bezier (new PointF (_Length / 2, _mV_Middle), new PointF (_Length / 4 * 3, _mV_Middle), new PointF (_Length, _mV_End), x));

            _Out.Add (new PointF (_Length, _mV_End));        // Finish the curve

            return _Out;
        }

        public static List<PointF> Peak (int DrawResolution, float _Length, float _mV, float _mV_End, PointF _Start) {
            if (_Length < 0)
                return new List<PointF> ();

            int i;
            float x;
            List<PointF> _Out = new List<PointF> ();
            float Resolution = (2 * (DrawResolution / 1000f)) / _Length;

            for (i = 1; (x = i * Resolution) <= 1; i++)
                _Out.Add (Bezier (new PointF (0, _Start.Y), new PointF (_Length / 3, (float)_mV / 1), new PointF (_Length / 2, _mV), x));

            for (i = 1; (x = i * Resolution) <= 1; i++)
                _Out.Add (Bezier (new PointF (_Length / 2, _mV), new PointF (_Length / 5 * 3, _mV / 1), new PointF (_Length, _mV_End), x));

            _Out.Add (new PointF (_Length, _mV_End));        // Finish the curve

            return _Out;
        }

        public static List<PointF> Line (int DrawResolution, float _Length, float _mV, PointF _Start) {
            if (_Length < 0)
                return new List<PointF> ();

            List<PointF> Out = new List<PointF> ();
            PointF Start = new PointF (0, _Start.Y);
            PointF End = new PointF (_Length, _mV);

            for (float x = 0; x <= _Length; x += (DrawResolution / 1000f))
                Out.Add (Math.Lerp (Start, End, x / _Length));

            return Out;
        }
    }
}