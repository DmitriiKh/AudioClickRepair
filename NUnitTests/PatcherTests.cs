using AudioClickRepair.Data;
using NUnit.Framework;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using AudioClickRepair.Processing;

namespace NUnitTests
{
    public class PatcherTests
    {
        private const int _testArrayLength = 1000;
        private ImmutableArray<double> _arrayValueEqualsIndex;
        private BlockingCollection<AbstractPatch> _patchCollection;
        private IPatcher _patcher;

        [SetUp]
        public void Setup()
        {
            _arrayValueEqualsIndex = Enumerable.Range(0, _testArrayLength)
                .Select(i => (double)i)
                .ToImmutableArray();

            _patchCollection = new BlockingCollection<AbstractPatch>();

            _patcher = new Patcher(
                _arrayValueEqualsIndex,
                _patchCollection,
                (patch, position) => patch.GetValue(position));
        }

        [TestCase(100, 100, 45, 10)] // patch outside of range (left)
        [TestCase(100, 100, 95, 10)] // patch covers beginning of range
        [TestCase(100, 100, 145, 10)] // patch in the middle
        [TestCase(100, 100, 195, 10)] // patch covers ending of range
        [TestCase(100, 100, 245, 10)] // patch outside of range (right)
        [TestCase(100, 100, 95, 110)] // patch covers entire range
        public void RangeData_GetRange_OnePatch_ReturnsCorrectArray(
            int rangeStart,
            int rangeLength,
            int patchStart,
            int patchLength)
        {
            var patch = CreatePatchChangedSign(patchStart, patchLength);
            _patchCollection.Add(patch);

            var rangeArray = _patcher.GetRange(rangeStart, rangeLength);

            for (var position = rangeStart;
                position < rangeStart + rangeLength;
                position++)
            {
                var patched = position >= patch.StartPosition &&
                    position <= patch.EndPosition;
                var expected = patched ? -position : position;

                Assert.AreEqual(expected, rangeArray[position - rangeStart]);
            }
        }

        [TestCase(100, 100, 110, 10, 150, 10)]
        public void RangeData_GetRange_TwoPatches_ReturnsCorrectArray(
            int rangeStart,
            int rangeLength,
            int patchStartFirst,
            int patchLengthFirst,
            int patchStartSecond,
            int patchLengthSecond)
        {
            var firstPatch = CreatePatchChangedSign(patchStartFirst, patchLengthFirst);
            _patchCollection.Add(firstPatch);

            var secondPatch = CreatePatchChangedSign(patchStartSecond, patchLengthSecond);
            _patchCollection.Add(secondPatch);

            var rangeArray = _patcher.GetRange(rangeStart, rangeLength);

            for (var position = rangeStart;
                position < rangeStart + rangeLength;
                position++)
            {
                var patchedFirst = position >= firstPatch.StartPosition &&
                    position <= firstPatch.EndPosition;
                var patchedSecond = position >= secondPatch.StartPosition &&
                    position <= secondPatch.EndPosition;

                var expected = patchedFirst || patchedSecond ? -position : position;

                Assert.AreEqual(expected, rangeArray[position - rangeStart]);
            }
        }

        private PatchForTest CreatePatchChangedSign(int patchStart, int patchLength)
        {
            var patch = new PatchForTest(
                patchStart,
                patchLength);

            for (var position = patch.StartPosition;
                position <= patch.EndPosition;
                position++)
                // Change sign to opposite
                patch.SetValue(position, -_arrayValueEqualsIndex[position]);

            return patch;
        }

        private class PatchForTest : AbstractPatch
        {
            public PatchForTest(int patchStart, int patchLength)
                : base(new double[patchLength], patchStart, 3.0d)
            {
            }
        }
    }
}