namespace AudioClickRepair.Processing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class AveragedMaxErrorAnalyzer : IAnalyzer
    {
        private const int _blockSize = 16;
        private const int _blocksNumber = 16;

        public int InputDataSize => _blockSize * _blocksNumber;

        public double DefaultResult => 0;

        public double GetResult(double[] errors)
        {
            if (errors.Length != InputDataSize)
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
