using System;
using System.Collections.Generic;

namespace II.Waveform {
    public static partial class Dictionary {
        public class Plot {
            public int DrawResolution;
            public int IndexOffset;
            public float [] Vertices;

            public Plot () { }
            public Plot (int drawResolution, int indexOffset) {
                DrawResolution = drawResolution;
                IndexOffset = indexOffset;
            }
        }

        public static Plot Lerp (Plot _Plot1, Plot _Plot2, float _Percent) {
            /* Creates a Plot with a lerp of all Y axis points
             * Note: IndexOffset and DrawResolution are only averaged; loss of accuracy possible
             */

            Plot _Out = new Plot (
                (_Plot1.DrawResolution + _Plot2.DrawResolution) / 2,
                (_Plot1.IndexOffset + _Plot2.IndexOffset) / 2);

            List<float> vertices = new List<float> ();

            for (int i = 0; i < _Plot1.Vertices.Length || i < _Plot2.Vertices.Length; i++) {
                if (i < _Plot1.Vertices.Length && i < _Plot2.Vertices.Length)
                    vertices.Add (II.Math.Lerp (_Plot1.Vertices [i], _Plot2.Vertices [i], _Percent));
                else if (i < _Plot1.Vertices.Length)
                    vertices.Add (_Plot1.Vertices [i]);
                else if (i < _Plot2.Vertices.Length)
                    vertices.Add (_Plot2.Vertices [i]);
            }

            _Out.Vertices = vertices.ToArray ();
            return _Out;
        }
    }
}