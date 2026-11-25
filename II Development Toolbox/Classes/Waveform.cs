using System.Collections.Generic;
using System.IO;
using System.Text;

using II.Drawing;

namespace IIDT;

public class Waveform {
    /* Default values for producing baseline waveforms */
    private static double GetHR_Seconds = 1d;
    private static Lead.Values LeadValue = Lead.Values.ECG_II;

    public static List<PointD> Generate (int DrawResolution) {
        double _Amplitude = 1d;
        
        // Add code here for generating waveform!

        List<PointD> thisBeat = new List<PointD> ();
        return thisBeat;
    }
}
