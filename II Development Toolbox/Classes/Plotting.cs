using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using II.Drawing;

namespace IIDT {

    public static class Plotting {
        /*
	     * Shaping and PointD plotting functions
	     */

        public static PointD Last (List<PointD> _Original) {
            if (_Original.Count < 1)
                return new PointD (0, 0);
            else
                return _Original [_Original.Count - 1];
        }

        public static List<PointD> Concatenate (List<PointD> _Original, List<PointD> _Addition) {

            // Offsets the X value of a PointD[] so that it can be placed at the end
            // of an existing PointD[] and continue from that PointD on.

            // Nothing to add? Return something.
            if (_Original.Count == 0 && _Addition.Count == 0)
                return new List<PointD> ();
            else if (_Addition.Count == 0)
                return _Original;

            double _Offset = 0f;
            if (_Original.Count == 0)
                _Offset = 0;
            else if (_Original.Count > 0)
                _Offset = _Original [_Original.Count - 1].X;

            foreach (PointD eachVector in _Addition)
                _Original.Add (new PointD (eachVector.X + _Offset, eachVector.Y));

            return _Original;
        }

        public static double Slope (PointD _P1, PointD _P2) {
            return ((_P2.Y - _P1.Y) / (_P2.X - _P1.X));
        }

        public static PointD Bezier (PointD _Start, PointD _Control, PointD end, double _Percent) {
            return (((1 - _Percent) * (1 - _Percent)) * _Start) + (2 * _Percent * (1 - _Percent) * _Control) + ((_Percent * _Percent) * end);
        }

        public static List<PointD> Curve (int DrawResolution, double _Length, double _mV_Middle, double _mV_End, PointD _Start) {
            if (_Length < 0)
                return new List<PointD> ();

            int i;
            double x;
            List<PointD> @out = new List<PointD> ();
            double Resolution = (2 * (DrawResolution / 1000d)) / _Length;

            for (i = 1; (x = i * Resolution) <= 1; i++)
                @out.Add (Bezier (new PointD (0, _Start.Y), new PointD (_Length / 4, _mV_Middle), new PointD (_Length / 2, _mV_Middle), x));

            for (i = 1; (x = i * Resolution) <= 1; i++)
                @out.Add (Bezier (new PointD (_Length / 2, _mV_Middle), new PointD (_Length / 4 * 3, _mV_Middle), new PointD (_Length, _mV_End), x));

            @out.Add (new PointD (_Length, _mV_End));        // Finish the curve

            return @out;
        }

        public static List<PointD> Peak (int DrawResolution, double _Length, double _mV, double _mV_End, PointD _Start) {
            if (_Length < 0)
                return new List<PointD> ();

            int i;
            double x;
            List<PointD> Out = new List<PointD> ();
            double resolution = (2 * (DrawResolution / 1000d)) / _Length;

            for (i = 1; (x = i * resolution) <= 1; i++)
                Out.Add (Bezier (new PointD (0, _Start.Y), new PointD (_Length / 3, _mV / 1), new PointD (_Length / 2, _mV), x));

            for (i = 1; (x = i * resolution) <= 1; i++)
                Out.Add (Bezier (new PointD (_Length / 2, _mV), new PointD (_Length / 5 * 3, _mV / 1), new PointD (_Length, _mV_End), x));

            Out.Add (new PointD (_Length, _mV_End));        // Finish the curve

            return Out;
        }

        public static List<PointD> Line (int drawResolution, double _Length, double _mV, PointD _Start) {
            if (_Length < 0)
                return new List<PointD> ();

            List<PointD> Out = new List<PointD> ();
            PointD Start = new PointD (0, _Start.Y);
            PointD End = new PointD (_Length, _mV);

            for (double x = 0; x <= _Length; x += (drawResolution / 1000d))
                Out.Add (PointD.Lerp (Start, End, x / _Length));

            return Out;
        }
    }
}