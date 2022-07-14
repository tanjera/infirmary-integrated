using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace II {
    public class Audio {
        private static Task<MemoryStream> ToneGenerator (double seconds = 1, double frequency = 220) {
            MemoryStream stream = new ();
            BinaryWriter writer = new BinaryWriter (stream);

            int RIFF = 0x46464952;
            int WAVE = 0x45564157;
            int formatChunkSize = 16;
            int headerSize = 8;
            int format = 0x20746D66;
            short formatType = 1;
            short tracks = 1;
            int samplesPerSecond = 44100;
            short bitsPerSample = 16;
            short frameSize = (short)(tracks * ((bitsPerSample + 7) / 8));
            int bytesPerSecond = samplesPerSecond * frameSize;
            int waveSize = 4;
            int data = 0x61746164;
            int samplesTotal = (int)(samplesPerSecond * seconds);
            int dataChunkSize = samplesTotal * frameSize;
            int fileSize = waveSize + headerSize + formatChunkSize + headerSize + dataChunkSize;

            writer.Write (RIFF);
            writer.Write (fileSize);
            writer.Write (WAVE);
            writer.Write (format);
            writer.Write (formatChunkSize);
            writer.Write (formatType);
            writer.Write (tracks);
            writer.Write (samplesPerSecond);
            writer.Write (bytesPerSecond);
            writer.Write (frameSize);
            writer.Write (bitsPerSample);
            writer.Write (data);
            writer.Write (dataChunkSize);

            double ampl = 10000;

            for (int i = 0; i < samplesTotal; i++) {
                double t = (double)i / (double)samplesPerSecond;
                short s = (short)(ampl * (System.Math.Sin (t * frequency * 2.0 * System.Math.PI)));
                writer.Write (s);
            }

            writer.Close ();
            stream.Close ();

            return Task.FromResult (stream);
        }
    }
}