using Microsoft.VisualStudio.TestTools.UnitTesting;

using II;

namespace Unit_Testing {

    [TestClass]
    public class UtilityTest {

        [TestMethod]
        public void IsNewerVersion () {
            string [] t1_p1 = new string [] { "1.0.0", "1.0.", "1.1", "1.2", "1.0.1", "1.9" };
            string [] t1_p2 = new string [] { "1.0.1", "1.0.1", "1.2", "1.21", "2.0.0", "2.1" };

            Assert.IsTrue (true);

            for (int i = 0; i < t1_p1.Length && i < t1_p2.Length; i++) {
                // Test older -> newer version numbers
                Assert.IsTrue (Utility.IsNewerVersion (t1_p1 [i], t1_p2 [i]));
                // Test newer -> older version numbers
                Assert.IsFalse (Utility.IsNewerVersion (t1_p2 [i], t1_p1 [i]));
                // Test version numbers against same
                Assert.IsFalse (Utility.IsNewerVersion (t1_p1 [i], t1_p1 [i]));
            }
        }
    }
}