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

        public static List<Point> Generate (int DrawResolution, out string WaveName) {
            double _Amplitude = 1f;
            WaveName = "ECG_Pacemaker";

            List<Point> thisBeat = new List<Point> ();
            thisBeat = Plotting.Concatenate (thisBeat, Plotting.Line (DrawResolution, .02d, 0, Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, Plotting.Line (DrawResolution, .02d,
                0.2f * baseLeadCoeff [(int)LeadValue, (int)WavePart.Q], Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, Plotting.Line (DrawResolution, .02d,
                0f * baseLeadCoeff [(int)LeadValue, (int)WavePart.Q], Plotting.Last (thisBeat)));
            thisBeat = Plotting.Concatenate (thisBeat, Plotting.Line (DrawResolution, .02d, 0, Plotting.Last (thisBeat)));
            return thisBeat;
        }

        private enum WavePart {
            P, Q, R, S, J, T, PR, ST, TP
        }

        // Coefficients to transform base lead (Lead 2) into 12 lead ECG
        private static double [,] baseLeadCoeff = new double [,] {

            // P through T are multipliers; segments are additions
            { 0.7f,     0.7f,   0.7f,   0.7f,   0.7f,   0.8f,       0f,     0f,     0f },     // L1
            { 1f,       1f,     1f,     1f,     1f,     1f,         0f,     0f,     0f },     // L2
            { 0.5f,     0.5f,   0.5f,   0.5f,   0.5f,   0.2f,       0f,     0f,     0f },     // L3
            { -1f,      -1f,    -0.8f,  -1f,    -1f,    -0.9f,      0f,     0f,     0f },     // AVR
            { -1f,      0.3f,   0.2f,   0.4f,   0.3f,   0.6f,       0f,     0f,     0f },     // AVL
            { 0.7f,     0.8f,   0.8f,   0.8f,   0.8f,   0.4f,       0f,     0f,     0f },     // AVF
            { 0.2f,     -0.7f,  -1f,    0f,     0f,     0.3f,       0f,     0f,     0f },     // V1
            { 0.2f,     -1.8f,  -1.2f,  0f,     -1f,    1.4f,       0f,     0.1f,   0f },     // V2
            { 0.2f,     -3.0f,  -1.4f,  0f,     0f,     1.8f,       0f,     0.1f,   0f },     // V3
            { 0.7f,     -9.0f,  -0.8f,  0f,     0f,     1.4f,       0f,     0.1f,   0f },     // V4
            { 0.7f,     -10.0f, -0.2f,  0f,     0f,     1.0f,       0f,     0.1f,   0f },     // V5
            { 1f,       -9.0f,  -0.1f,  0f,     0f,     0.8f,       0f,     0f,     0f }      // V6
        };
    }
}