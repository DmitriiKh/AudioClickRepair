// <copyright file="AveragedMaxErrorAnalyzer.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace CarefulAudioRepair.Processing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CarefulAudioRepair.Properties;

    /// <summary>
    /// Analyzes errors. Finds maximums for each block of errors and than averages them.
    /// </summary>
    public class AveragedMaxErrorAnalyzer : IAnalyzer
    {
        private const int BlockSize = 16;
        private const int BlocksNumber = 16;

        /// <inheritdoc/>
        public int InputDataSize => BlockSize * BlocksNumber;

        /// <inheritdoc/>
        public double DefaultResult => 0;

        /// <inheritdoc/>
        public double GetResult(double[] errors)
        {
            if (errors is null)
            {
                throw new ArgumentNullException(nameof(errors));
            }

            if (errors.Length != this.InputDataSize)
            {
                throw new ArgumentException(
                    Resources.Incorrect_size_of_errors + nameof(errors));
            }

            return this.Slice(errors)
                .Select(block => block.Select(Math.Abs).Max())
                .Average();
        }

        private IEnumerable<double[]> Slice(double[] array)
        {
            for (int blockStart = 0, blockEndExcluding = BlockSize;
                blockEndExcluding <= array.Length;
                blockStart += BlockSize, blockEndExcluding += BlockSize)
            {
                yield return array[blockStart..blockEndExcluding];
            }
        }
    }
}
