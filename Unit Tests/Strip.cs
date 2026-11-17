using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Linq;

using II.Drawing;
using II.Rhythm;

namespace Tests {

    [TestClass]
    public class StripTests {

        [TestMethod]
        public void Underwrite_InterpolatesSpliceAtReferenceX () {
            Strip strip = CreateStrip (new PointD (1d, 0d), new PointD (2d, 0d), new PointD (3d, 0d));

            strip.Underwrite (new List<PointD> { new (0d, 0d), new (2d, 4d) });

            Assert.AreEqual (2d, PointAt (strip, 2d).Y, 1e-9);
            Assert.AreEqual (4d, PointAt (strip, 3d).Y, 1e-9);
        }

        [TestMethod]
        public void Underwrite_InsertsStrongerSplicePointBetweenReferencePoints () {
            Strip strip = CreateStrip (new PointD (1d, 0d), new PointD (3d, 0d));

            strip.Underwrite (new List<PointD> { new (0d, 0d), new (1d, 5d), new (2d, 0d) });

            Assert.AreEqual (3, strip.Points!.Count);
            Assert.AreEqual (5d, PointAt (strip, 2d).Y, 1e-9);
        }

        [TestMethod]
        public void Underwrite_InterpolatesReferenceAtSpliceXBeforeInserting () {
            Strip strip = CreateStrip (new PointD (1d, 2d), new PointD (3d, 4d));

            strip.Underwrite (new List<PointD> { new (0d, 0d), new (1d, 3.1d), new (2d, 0d) });

            Assert.AreEqual (3, strip.Points!.Count);
            Assert.AreEqual (3.1d, PointAt (strip, 2d).Y, 1e-9);
            Assert.AreEqual (2d, PointAt (strip, 1d).Y, 1e-9);
            Assert.AreEqual (4d, PointAt (strip, 3d).Y, 1e-9);
        }

        [TestMethod]
        public void Underwrite_UsesAbsoluteMagnitudeAndLeavesTiesUnchanged () {
            Strip strip = CreateStrip (new PointD (1d, 4d), new PointD (2d, 4d), new PointD (3d, 4d));

            strip.Underwrite (new List<PointD> { new (0d, -4d), new (2d, -4d) });

            Assert.AreEqual (4d, PointAt (strip, 1d).Y, 1e-9);
            Assert.AreEqual (4d, PointAt (strip, 2d).Y, 1e-9);
            Assert.AreEqual (4d, PointAt (strip, 3d).Y, 1e-9);
        }

        [TestMethod]
        public void Underwrite_DoesNotReplaceWithOppositeSignSpliceValues () {
            Strip strip = CreateStrip (new PointD (1d, 3d), new PointD (2d, 3d), new PointD (3d, 3d));

            strip.Underwrite (new List<PointD> { new (0d, -5d), new (2d, -5d) });

            Assert.AreEqual (3d, PointAt (strip, 1d).Y, 1e-9);
            Assert.AreEqual (3d, PointAt (strip, 2d).Y, 1e-9);
            Assert.AreEqual (3d, PointAt (strip, 3d).Y, 1e-9);
        }

        [TestMethod]
        public void Underwrite_ReplacesOnlyWithStrongerSameSignValues () {
            Strip strip = CreateStrip (new PointD (1d, 3d), new PointD (2d, -3d), new PointD (3d, 3d), new PointD (4d, -3d));

            strip.Underwrite (new List<PointD> { new (0d, 4d), new (1d, 5d), new (2d, 2d), new (3d, -4d) });

            Assert.AreEqual (4d, PointAt (strip, 1d).Y, 1e-9);
            Assert.AreEqual (-3d, PointAt (strip, 2d).Y, 1e-9);
            Assert.AreEqual (3d, PointAt (strip, 3d).Y, 1e-9);
            Assert.AreEqual (-4d, PointAt (strip, 4d).Y, 1e-9);
        }

        [TestMethod]
        public void Underwrite_ReplacesZeroWithEitherSign () {
            Strip strip = CreateStrip (new PointD (1d, 0d), new PointD (2d, 0d));

            strip.Underwrite (new List<PointD> { new (0d, -2d), new (1d, 2d) });

            Assert.AreEqual (-2d, PointAt (strip, 1d).Y, 1e-9);
            Assert.AreEqual (2d, PointAt (strip, 2d).Y, 1e-9);
        }

        [TestMethod]
        public void Underwrite_DoesNotReplaceLargerOriginalValuesWithSmallerSpliceValues () {
            Strip strip = CreateStrip (new PointD (1d, 0d), new PointD (2d, 9d), new PointD (2.8d, 1d));

            strip.Underwrite (new List<PointD> { new (-1d, 0d), new (0d, 0d), new (2d, 2d) });

            Assert.AreEqual (9d, PointAt (strip, 2d).Y, 1e-9);
            Assert.AreEqual (1.8d, PointAt (strip, 2.8d).Y, 1e-9);
        }

        [TestMethod]
        public void Underwrite_IncludesSpliceEndpointsAndDoesNotExtrapolatePastReference () {
            Strip strip = CreateStrip (new PointD (2d, 1d), new PointD (3d, 1d), new PointD (4d, 1d));

            strip.Underwrite (new List<PointD> { new (0d, 10d), new (2d, 10d), new (4d, 10d) });

            Assert.AreEqual (3, strip.Points!.Count);
            Assert.AreEqual (10d, PointAt (strip, 2d).Y, 1e-9);
            Assert.AreEqual (10d, PointAt (strip, 3d).Y, 1e-9);
            Assert.AreEqual (10d, PointAt (strip, 4d).Y, 1e-9);
        }

        [TestMethod]
        public void Underwrite_LeavesNonOverlappingReferenceUnchanged () {
            Strip strip = CreateStrip (new PointD (10d, 2d), new PointD (11d, 3d));

            strip.Underwrite (new List<PointD> { new (0d, 8d), new (2d, 8d) });

            Assert.AreEqual (2, strip.Points!.Count);
            Assert.AreEqual (2d, PointAt (strip, 10d).Y, 1e-9);
            Assert.AreEqual (3d, PointAt (strip, 11d).Y, 1e-9);
        }

        [TestMethod]
        public void Underwrite_HandlesEmptyAndSinglePointInputs () {
            Strip emptyStrip = CreateStrip ();
            emptyStrip.Underwrite (new List<PointD> { new (1d, 5d) });
            Assert.AreEqual (0, emptyStrip.Points!.Count);

            Strip singlePointStrip = CreateStrip (new PointD (2d, 1d));
            singlePointStrip.Underwrite (new List<PointD> { new (1d, 3d) });

            Assert.AreEqual (1, singlePointStrip.Points!.Count);
            Assert.AreEqual (3d, PointAt (singlePointStrip, 2d).Y, 1e-9);
        }

        [TestMethod]
        public void Underwrite_HandlesDuplicateXCoordinates () {
            Strip strip = CreateStrip (new PointD (1d, 2d), new PointD (1d, 2d), new PointD (3d, 2d));

            strip.Underwrite (new List<PointD> { new (0d, 4d), new (0d, 4d), new (1d, 4d), new (2d, 4d) });

            Assert.AreEqual (4, strip.Points!.Count);
            Assert.AreEqual (4d, strip.Points [0].Y, 1e-9);
            Assert.AreEqual (4d, strip.Points [1].Y, 1e-9);
            Assert.AreEqual (4d, PointAt (strip, 2d).Y, 1e-9);
            Assert.IsTrue (strip.Points.Zip (strip.Points.Skip (1), (left, right) => left.X <= right.X).All (isSorted => isSorted));
        }

        [TestMethod]
        public void Underwrite_PreservesDuplicateGroupTieBehaviorDuringInterpolation () {
            Strip strip = CreateStrip (new PointD (3d, 0d), new PointD (4d, 0d));

            strip.Underwrite (new List<PointD> {
                new (0d, 0d), new (1d, 0d), new (3d, 5d), new (3d, -5d)
            });

            Assert.AreEqual (2.5d, PointAt (strip, 3d).Y, 1e-9);
            Assert.AreEqual (-5d, PointAt (strip, 4d).Y, 1e-9);
        }

        [TestMethod]
        public void Underwrite_InsertsMultipleInterleavedSplicePointsInOrder () {
            Strip strip = CreateStrip (new PointD (1d, 0d), new PointD (5d, 0d));

            strip.Underwrite (new List<PointD> {
                new (0d, 0d), new (1d, 4d), new (2d, 0d), new (3d, 5d), new (4d, 0d)
            });

            Assert.AreEqual (4, strip.Points!.Count);
            Assert.AreEqual (4d, PointAt (strip, 2d).Y, 1e-9);
            Assert.AreEqual (5d, PointAt (strip, 4d).Y, 1e-9);
            Assert.IsTrue (strip.Points.Zip (strip.Points.Skip (1), (left, right) => left.X <= right.X).All (isSorted => isSorted));
        }

        private static Strip CreateStrip (params PointD [] points) {
            Strip strip = new (II.Lead.Values.ECG_I, 1d, null);
            strip.Points!.AddRange (points);
            return strip;
        }

        private static PointD PointAt (Strip strip, double x)
            => strip.Points!.Single (point => Math.Abs (point.X - x) < 1e-9);
    }
}