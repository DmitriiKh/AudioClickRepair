using AudioClickRepair.Processing;
using NUnit.Framework;
using System;
using System.Linq;

namespace NUnitTests
{
    public class AveragedMaxErrorTests
    {
        private AveragedMaxErrorAnalyzer _analyzer;

        [SetUp]
        public void Setup()
        {
            _analyzer = new AveragedMaxErrorAnalyzer();
        }

        [Test]
        public void Analyzer_GetsZeros_ReturnsDefaultValue()
        {
            var zeros = new double[_analyzer.GetInputDataSize()];
            var result = _analyzer.GetResult(zeros);

            Assert.AreEqual(_analyzer.GetDefaultResult(), result);
        }

        [Test]
        public void Analyzer_GetsOnes_ReturnsOne()
        {
            var ones = Enumerable.Repeat(1.0d, _analyzer.GetInputDataSize())
                .ToArray();
            var result = _analyzer.GetResult(ones);

            Assert.AreEqual(1.0d, result);
        }

        [Test]
        public void Analyzer_GetsPositiveInput_ReturnsCorrectResult()
        {
            // Input is 1 to 256 for _blockSize = 16 and _blocksNumber = 16
            var startValue = 1;
            var errors = Enumerable.Range(startValue, _analyzer.GetInputDataSize())
                .Select(i => (double)i)
                .ToArray();
            var result = _analyzer.GetResult(errors);

            // Correct result based on _blockSize = 16 and _blocksNumber = 16
            const double correctResult = (double)(16 + 32 + 48 + 64 + 80 + 96
                + 112 + 128 + 144 + 160 + 176 + 192 + 208 + 224 + 240 + 256)
                / 16;

            Assert.AreEqual(correctResult, result);
        }

        [Test]
        public void Analyzer_GetsMixedInput_ReturnsCorrectResult()
        {
            // Make a half of input values negative
            // Input is -128 to 127 for _blockSize = 16 and _blocksNumber = 16
            var startValue = -_analyzer.GetInputDataSize() / 2;
            var errors = Enumerable.Range(startValue, _analyzer.GetInputDataSize())
                .Select(i => (double)i)
                .ToArray();
            var result = _analyzer.GetResult(errors);

            // Correct result based on _blockSize = 16 and _blocksNumber = 16
            const double correctResult = (double)(128 + 112 + 96 + 80 + 64
                + 48 + 32 + 16 + 15 + 31 + 47 + 63 + 79 + 95 + 111 + 127)
                / 16;

            Assert.AreEqual(correctResult, result);
        }

        [Test]
        public void Analyzer_GetsSmallerInput_Throws()
        {
            var smallerArray = Enumerable.Repeat(0d, _analyzer.GetInputDataSize() - 1)
                .ToArray();

            Assert.Throws<ArgumentException>(() => _analyzer.GetResult(smallerArray));
        }

        [Test]
        public void Analyzer_GetsLargerInput_Throws()
        {
            var largerArray = Enumerable.Repeat(0d, _analyzer.GetInputDataSize() + 1)
                .ToArray();

            Assert.Throws<ArgumentException>(() => _analyzer.GetResult(largerArray));
        }
    }
}