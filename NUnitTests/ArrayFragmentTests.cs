using CarefulAudioRepair.Data;
using NUnit.Framework;
using System.Collections.Immutable;
using System.Linq;

namespace NUnitTests
{
    public class ArrayFragmentTests
    {
        private const int _testArrayLength = 1000;
        private ImmutableArray<double> _arrayValueEqualsIndex;

        [SetUp]
        public void Setup()
        {
            _arrayValueEqualsIndex = Enumerable.Range(0, _testArrayLength)
                .Select(i => (double)i)
                .ToImmutableArray();
        }

        [TestCase(0, 100)] // Beginning
        [TestCase(_testArrayLength - 100, 100)] // End
        public void ArrayFragment_Converts_ReturnsCorrectArray(
            int rangeStart,
            int rangeLength)
        {
            var range = new ArrayFragment(
                _arrayValueEqualsIndex,
                rangeStart,
                rangeLength);

            Assert.AreEqual(
                Enumerable.Range(rangeStart, rangeLength).ToArray(),
                range.GetInternalArray());
        }

        [Test]
        public void ArrayFragment_OneElementLength_WorkCorrectly()
        {
            const int rangeStart = 100;
            var range = new ArrayFragment(
                _arrayValueEqualsIndex,
                rangeStart,
                1);

            Assert.AreEqual(
                _arrayValueEqualsIndex[rangeStart],
                range.GetValue(rangeStart));
        }

        [Test]
        public void ArrayFragment_SetGet_WorkCorrectly()
        {
            const int position = 100;
            var range = new ArrayFragment(
                _arrayValueEqualsIndex,
                position,
                1);
            // Change sign to opposite
            range.SetValue(position, -range.GetValue(position));

            Assert.Zero(
                _arrayValueEqualsIndex[position] + range.GetValue(position));
        }
    }
}