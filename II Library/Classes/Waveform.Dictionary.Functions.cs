/* Waveform.Dictionary.Functions.cs
 * Infirmary Integrated
 * By Ibi Keller (Tanjera), (c) 2023
 */

using System.Collections.Generic;

namespace II.Waveform {
    public static partial class Dictionary {
        public class Plot {
            public int DrawResolution;
            public int IndexOffset;
            public int SystoleLength;
            public double [] Vertices;

            public double LengthSeconds {
                get { return (Vertices.Length * DrawResolution) / 1000d; }
            }

            public int Length_Milliseconds {
                get { return Vertices.Length * DrawResolution; }
            }
            
            public Plot () {
            }

            public Plot (int drawResolution, int indexOffset) {
                DrawResolution = drawResolution;
                IndexOffset = indexOffset;
            }
        }

        public static Plot Lerp (Plot plot1, Plot _Plot2, double _Percent) {
            /* Creates a Plot with a lerp of all Y axis points
             * Note: IndexOffset and DrawResolution are only averaged; loss of accuracy possible
             */

            Plot _Out = new (
                (plot1.DrawResolution + _Plot2.DrawResolution) / 2,
                (plot1.IndexOffset + _Plot2.IndexOffset) / 2);
            _Out.SystoleLength = (plot1.SystoleLength + _Plot2.SystoleLength) / 2;

            List<double> vertices = new ();

            for (int i = 0; i < plot1.Vertices.Length || i < _Plot2.Vertices.Length; i++) {
                if (i < plot1.Vertices.Length && i < _Plot2.Vertices.Length)
                    vertices.Add (Math.Lerp (plot1.Vertices [i], _Plot2.Vertices [i], _Percent));
                else if (i < plot1.Vertices.Length)
                    vertices.Add (plot1.Vertices [i]);
                else if (i < _Plot2.Vertices.Length)
                    vertices.Add (_Plot2.Vertices [i]);
            }

            _Out.Vertices = vertices.ToArray ();
            return _Out;
        }
    }
}