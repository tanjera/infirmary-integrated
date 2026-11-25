using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Globalization;

using II;

namespace Tests {

    [TestClass]
    public class PhysiologyTests {

        [TestMethod]
        public async System.Threading.Tasks.Task SaveLoad_UsesInvariantCultureForAmplitudeArrays () {
            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            Physiology source = new (null);
            Physiology loaded = new (null);

            try {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo ("fr-FR");

                source.Q_Amplitude = new double [] { 0.25d, -1.5d };
                source.R_Amplitude = new double [] { 2.75d, -3.5d };
                source.S_Amplitude = new double [] { 4.25d, -5.5d };
                source.T_Amplitude = new double [] { 6.75d, -7.5d };
                source.ST_Elevation = new double [] { 8.25d, -9.5d };

                string saved = source.Save ();

                Assert.AreEqual ("0.25, -1.5", GetFieldValue (saved, "Q_Amplitude"));
                Assert.AreEqual ("2.75, -3.5", GetFieldValue (saved, "R_Amplitude"));
                Assert.AreEqual ("4.25, -5.5", GetFieldValue (saved, "S_Amplitude"));
                Assert.AreEqual ("6.75, -7.5", GetFieldValue (saved, "T_Amplitude"));
                Assert.AreEqual ("8.25, -9.5", GetFieldValue (saved, "ST_Elevation"));

                loaded.Q_Amplitude = new double [2];
                loaded.R_Amplitude = new double [2];
                loaded.S_Amplitude = new double [2];
                loaded.T_Amplitude = new double [2];
                loaded.ST_Elevation = new double [2];

                await loaded.Load (saved);

                CollectionAssert.AreEqual (source.Q_Amplitude, loaded.Q_Amplitude);
                CollectionAssert.AreEqual (source.R_Amplitude, loaded.R_Amplitude);
                CollectionAssert.AreEqual (source.S_Amplitude, loaded.S_Amplitude);
                CollectionAssert.AreEqual (source.T_Amplitude, loaded.T_Amplitude);
                CollectionAssert.AreEqual (source.ST_Elevation, loaded.ST_Elevation);
            } finally {
                loaded.Dispose ();
                source.Dispose ();
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        [TestMethod]
        public void Save_OmitsNullAmplitudeArraysAndWritesEmptyArrays () {
            Physiology physiology = new (null);

            try {
                physiology.Q_Amplitude = null;
                physiology.R_Amplitude = null;
                physiology.S_Amplitude = null;
                physiology.T_Amplitude = null;
                physiology.ST_Elevation = null;

                string nullArraysSaved = physiology.Save ();

                Assert.IsNull (GetFieldValue (nullArraysSaved, "Q_Amplitude"));
                Assert.IsNull (GetFieldValue (nullArraysSaved, "R_Amplitude"));
                Assert.IsNull (GetFieldValue (nullArraysSaved, "S_Amplitude"));
                Assert.IsNull (GetFieldValue (nullArraysSaved, "T_Amplitude"));
                Assert.IsNull (GetFieldValue (nullArraysSaved, "ST_Elevation"));

                physiology.Q_Amplitude = Array.Empty<double> ();
                physiology.R_Amplitude = Array.Empty<double> ();
                physiology.S_Amplitude = Array.Empty<double> ();
                physiology.T_Amplitude = Array.Empty<double> ();
                physiology.ST_Elevation = Array.Empty<double> ();

                string emptyArraysSaved = physiology.Save ();

                Assert.AreEqual (string.Empty, GetFieldValue (emptyArraysSaved, "Q_Amplitude"));
                Assert.AreEqual (string.Empty, GetFieldValue (emptyArraysSaved, "R_Amplitude"));
                Assert.AreEqual (string.Empty, GetFieldValue (emptyArraysSaved, "S_Amplitude"));
                Assert.AreEqual (string.Empty, GetFieldValue (emptyArraysSaved, "T_Amplitude"));
                Assert.AreEqual (string.Empty, GetFieldValue (emptyArraysSaved, "ST_Elevation"));
            } finally {
                physiology.Dispose ();
            }
        }

        private static string? GetFieldValue (string content, string fieldName) {
            foreach (string line in content.Split (Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)) {
                string field = line.Trim ();
                if (field.StartsWith ($"{fieldName}:", StringComparison.Ordinal))
                    return field.Substring (fieldName.Length + 1);
            }

            return null;
        }
    }
}