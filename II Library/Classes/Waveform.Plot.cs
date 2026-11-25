/* Waveform.Plot.cs
 * Infirmary Integrated
 * By Ibi Keller (Tanjera), (c) 2026
 */

using System.Collections.Generic;

namespace II.Waveform {
    public class Plot {
        public int DrawResolution;
        public int IndexOffset;
        public int SystoleLength;
        public List<Vertex> Vertices;
            
        public double LengthSeconds {
            get { return (Vertices.Count * DrawResolution) / 1000d; }
        }

        public int Length_Milliseconds {
            get { return Vertices.Count * DrawResolution; }
        }
            
        public Plot () {
        }

        public Plot (int drawResolution, int indexOffset) {
            DrawResolution = drawResolution;
            IndexOffset = indexOffset;
        }
        
        public static Plot Lerp (Plot plot1, Plot plot2, double percent) {
            /* Creates a Plot with a lerp of all Y axis points
             * Note: IndexOffset and DrawResolution are only averaged; loss of accuracy possible
             */

            Plot result = new (
                (plot1.DrawResolution + plot2.DrawResolution) / 2,
                (plot1.IndexOffset + plot2.IndexOffset) / 2);
            result.SystoleLength = (plot1.SystoleLength + plot2.SystoleLength) / 2;

            List<Vertex> vertices = new ();

            for (int i = 0; i < plot1.Vertices.Count || i < plot2.Vertices.Count; i++) {
                if (i < plot1.Vertices.Count && i < plot2.Vertices.Count) {
                    var v = new Vertex(Math.Lerp (plot1.Vertices [i].Y, plot2.Vertices [i].Y, percent));
                    
                    if (plot1.Vertices [i].Waveform == plot2.Vertices [i].Waveform)
                        v.Waveform = plot1.Vertices [i].Waveform;
                    else 
                        v.Waveform = (plot1.Vertices [i].Waveform != Vertex.Waveforms.Null) ? plot1.Vertices [i].Waveform : plot2.Vertices [i].Waveform;
                    
                    vertices.Add(v);
                } else if (i < plot1.Vertices.Count)
                    vertices.Add (plot1.Vertices [i]);
                else if (i < plot2.Vertices.Count)
                    vertices.Add (plot2.Vertices [i]);
            }

            result.Vertices = new(vertices);
            return result;
        }
    }
}