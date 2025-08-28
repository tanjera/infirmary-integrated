using System;
using System.IO;

namespace Tone_Generator {

    internal class Program {

        private static void Main (string [] args) {
            string outDir = Path.Combine (Directory.GetCurrentDirectory (), "audio");
            Directory.CreateDirectory (outDir);

            // Generate DeviceDefib -> Charging
            Console.WriteLine ("Generating DeviceDefib -> Charging");
            Generate (Path.Combine (outDir, "defib_charging.wav"), 3, 440, true);

            Console.WriteLine ("Generating DeviceDefib -> Charged");
            Generate (Path.Combine (outDir, "defib_charged.wav"), 30, 660, true);

            // Generate DeviceDefib/Monitor -> QRS tone
            Console.WriteLine ("Generating DeviceDefib/Monitor -> QRS tone");
            Generate (Path.Combine (outDir, "tone_qrs.wav"), 0.15, 660, true);

            // Generate DeviceDefib/Monitor -> SpO2 tones
            for (int i = 0; i <= 100; i++) {
                Console.WriteLine ($"DeviceDefib/Monitor -> SpO2 tones: SpO2 {i:000}");
                Generate (Path.Combine (outDir, $"tone_spo2_{i:000}.wav"), 0.15,
                    Lerp (110, 330, (double)(i) / 100),
                    true);
            }

            Console.WriteLine ($"Complete! Files written to {outDir}");
        }

        private static void Generate (string output, double seconds = 0.1, double frequency = 220, bool fixpop = true) {
            FileStream stream = new FileStream (output, FileMode.Create);
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

                if (fixpop && i > samplesTotal - 1000) {
                    double c = InverseLerp (samplesTotal, samplesTotal - 1000, i);
                    s = Convert.ToInt16 (s * c);
                }

                writer.Write (s);
            }

            writer.Close ();
            stream.Close ();
        }

        public static double Lerp (double min, double max, double t) {
            return min * (1 - t) + max * t;
        }

        public static int Lerp (int min, int max, double t) {
            return (int)(min * (1 - t) + max * t);
        }

        public static double InverseLerp (double min, double max, double current) {
            return (current - min) / (max - min);
        }

        public static double InverseLerp (int min, int max, double current) {
            return ((current - min) / (max - min));
        }
    }
}