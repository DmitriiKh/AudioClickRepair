using System;
using System.Collections.Generic;
using System.Linq;

namespace AudioClickRepair.Processing
{
    public class AveragedMaxErrorAnalyzer : IAnalyzer
    {
        private const int _blockSize = 16;
        private const int _blocksNumber = 16;

        public int GetInputDataSize() => _blockSize * _blocksNumber;

        public double GetDefaultResult() => 0;

        public double GetResult(double[] errors)
        {
            if (errors.Length != GetInputDataSize())
                throw new ArgumentException("Not correct length of " + nameof(errors));

            return Slice(errors)
                .Select(block => block.Select(Math.Abs).Max())
                .Average();
        }

        private IEnumerable<double[]> Slice(double[] array)
        {
            for (int blockStart = 0, blockEndExcluding = _blockSize;
                blockEndExcluding <= array.Length;
                blockStart += _blockSize, blockEndExcluding += _blockSize)

                yield return array[blockStart..blockEndExcluding];
        }
    }
}
