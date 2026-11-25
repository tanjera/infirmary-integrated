/* Waveform.Vertex.cs
 * Infirmary Integrated
 * By Ibi Keller (Tanjera), (c) 2026
 */

using II.Drawing;

namespace II.Waveform {
    public class Vertex : PointD {
        public Waveforms Waveform = Waveforms.Null;
            
        public enum Waveforms {
            Null,
            P, PR, Q, R, S, T, ST,
            Isoelectric,
            Systolic,
            Diastolic
        }

        public Vertex () {
            Y = 0;
        }
        
        public Vertex (double y) {
            Y = y;
        }
            
        public Vertex (double y, Waveforms wf) {
            Y = y;
            Waveform = wf;
        }
    }
}
