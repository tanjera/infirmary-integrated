using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waveform_Generator {

    public static class Plotting {
        /*
	     * Shaping and point plotting functions
	     */

        public static Point Last (List<Point> _Original) {
            if (_Original.Count < 1)
                return new Point (0, 0);
            else
                return _Original [_Original.Count - 1];
        }

        public static List<Point> Concatenate (List<Point> _Original, List<Point> _Addition) {

            // Offsets the X value of a Point[] so that it can be placed at the end
            // of an existing Point[] and continue from that point on.

            // Nothing to add? Return something.
            if (_Original.Count == 0 && _Addition.Count == 0)
                return new List<Point> ();
            else if (_Addition.Count == 0)
                return _Original;

            double _Offset = 0f;
            if (_Original.Count == 0)
                _Offset = 0;
            else if (_Original.Count > 0)
                _Offset = _Original [_Original.Count - 1].X;

            foreach (Point eachVector in _Addition)
                _Original.Add (new Point (eachVector.X + _Offset, eachVector.Y));

            return _Original;
        }

        public static double Slope (Point _P1, Point _P2) {
            return ((_P2.Y - _P1.Y) / (_P2.X - _P1.X));
        }

        public static Point Bezier (Point _Start, Point _Control, Point _End, double _Percent) {
            return (((1 - _Percent) * (1 - _Percent)) * _Start) + (2 * _Percent * (1 - _Percent) * _Control) + ((_Percent * _Percent) * _End);
        }

        public static List<Point> Curve (int DrawResolution, double _Length, double _mV_Middle, double _mV_End, Point _Start) {
            if (_Length < 0)
                return new List<Point> ();

            int i;
            double x;
            List<Point> Out = new List<Point> ();
            double Resolution = (2 * (DrawResolution / 1000d)) / _Length;

            for (i = 1; (x = i * Resolution) <= 1; i++)
                Out.Add (Bezier (new Point (0, _Start.Y), new Point (_Length / 4, _mV_Middle), new Point (_Length / 2, _mV_Middle), x));

            for (i = 1; (x = i * Resolution) <= 1; i++)
                Out.Add (Bezier (new Point (_Length / 2, _mV_Middle), new Point (_Length / 4 * 3, _mV_Middle), new Point (_Length, _mV_End), x));

            Out.Add (new Point (_Length, _mV_End));        // Finish the curve

            return Out;
        }

        public static List<Point> Peak (int DrawResolution, double _Length, double _mV, double _mV_End, Point _Start) {
            if (_Length < 0)
                return new List<Point> ();

            int i;
            double x;
            List<Point> Out = new List<Point> ();
            double Resolution = (2 * (DrawResolution / 1000d)) / _Length;

            for (i = 1; (x = i * Resolution) <= 1; i++)
                Out.Add (Bezier (new Point (0, _Start.Y), new Point (_Length / 3, _mV / 1), new Point (_Length / 2, _mV), x));

            for (i = 1; (x = i * Resolution) <= 1; i++)
                Out.Add (Bezier (new Point (_Length / 2, _mV), new Point (_Length / 5 * 3, _mV / 1), new Point (_Length, _mV_End), x));

            Out.Add (new Point (_Length, _mV_End));        // Finish the curve

            return Out;
        }

        public static List<Point> Line (int DrawResolution, double _Length, double _mV, Point _Start) {
            if (_Length < 0)
                return new List<Point> ();

            List<Point> Out = new List<Point> ();
            Point Start = new Point (0, _Start.Y);
            Point End = new Point (_Length, _mV);

            for (double x = 0; x <= _Length; x += (DrawResolution / 1000d))
                Out.Add (Point.Lerp (Start, End, x / _Length));

            return Out;
        }
    }
}