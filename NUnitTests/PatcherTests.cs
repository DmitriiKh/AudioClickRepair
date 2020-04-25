using AudioClickRepair.Data;
using NUnit.Framework;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;

namespace NUnitTests
{
    public class PatcherTests
    {
        private const int _testArrayLength = 1000;
        private ImmutableArray<double> _arrayValueEqualsIndex;
        private BlockingCollection<IPatch> _patchCollection;
        private Patcher _patcher;

        private class PatchForTest : IPatch
        {
            private readonly double[] _output;

            public PatchForTest(int patchStart, int patchLength)
            {
                StartPosition = patchStart;
                Length = patchLength;

                _output = new double[patchLength];
            }

            public int StartPosition { get; private set; }
            public int Length { get; private set; }

            public int GetEndPosition() => StartPosition + Length - 1;

            public double GetOutputSample(int position) =>
                _output[position - StartPosition];

            public void SetOutputSample(int position, double value) =>
                _output[position - StartPosition] = value;
        }

        [SetUp]
        public void Setup()
        {
            _arrayValueEqualsIndex = Enumerable.Range(0, _testArrayLength)
                .Select(i => (double)i)
                .ToImmutableArray();

            _patchCollection = new BlockingCollection<IPatch>();

            _patcher = new Patcher(
                _arrayValueEqualsIndex,
                _patchCollection,
                (patch, position) => patch.GetOutputSample(position));
        }

        [TestCase(200, 100, 45, 10)] // patch outside of range (left)
        [TestCase(200, 100, 95, 10)] // patch covers beginning of range
        [TestCase(200, 100, 145, 10)] // patch in the middle
        [TestCase(200, 100, 195, 10)] // patch covers ending of range
        [TestCase(200, 100, 245, 10)] // patch outside of range (right)
        [TestCase(200, 100, 95, 110)] // patch covers entire range
        public void RangeData_GetRangeBefore_ReturnsCorrectRange(
            int positionExcluding, 
            int length,
            int patchStart,
            int patchLength)
        {
            var patch = new PatchForTest(
                patchStart,
                patchLength);

            for (var position = patch.StartPosition; 
                position <= patch.GetEndPosition(); 
                position++)
                // Change sign to opposite
                patch.SetOutputSample(position, -_arrayValueEqualsIndex[position]);

            _patchCollection.Add(patch);

            var range = _patcher.GetRangeBefore(positionExcluding, length);

            for (var position = range.StartPosition; 
                position <= range.EndPosition; 
                position++)
            {
                var patched = position >= patch.StartPosition && 
                    position <= patch.GetEndPosition();
                var expected = patched ? -position : position;
                   
                Assert.AreEqual(expected, range.GetValue(position));
            }
        }
    }
}