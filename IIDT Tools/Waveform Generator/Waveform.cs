using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waveform_Generator {

    public class Waveform {
        /* Default values for producing baseline waveforms */
        private static double GetHR_Seconds = 1d;
        private static Lead.Values LeadValue = Lead.Values.ECG_II;
        private static int DrawResolution = 10;

        public static List<Point> Generate (int DrawResolution, out string WaveName) {
            double _Amplitude = 1f;
            WaveName = "";

            // Add code here for generating waveform!

            List<Point> thisBeat = new List<Point> ();
            return thisBeat;
        }
    }
}