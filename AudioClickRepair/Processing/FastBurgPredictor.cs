// <copyright file="FastBurgPredictor.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace AudioClickRepair.Processing
{
    /// <summary>
    /// Gives predictions for a sequence of samples using Fast Burg Algorithm.
    /// </summary>
    internal class FastBurgPredictor : IPredictor
    {
        private readonly int coefficientsNumber;
        private readonly int historyLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="FastBurgPredictor"/> class.
        /// </summary>
        /// <param name="coefficientsNumber">Number of coefficients for
        /// the Burg algorithm.</param>
        /// <param name="historyLength">Number of samples used as input
        /// for training the Burg Algorithm.</param>
        public FastBurgPredictor(int coefficientsNumber, int historyLength)
        {
            this.coefficientsNumber = coefficientsNumber;
            this.historyLength = historyLength;
        }

        /// <inheritdoc/>
        public int InputDataSize => this.historyLength;

        /// <inheritdoc/>
        public double GetForward(double[] samples)
        {
            var fastBurgAlgorithm = new FastBurgAlgorithm64(samples);
            fastBurgAlgorithm.Train(
                samples.Length,
                this.coefficientsNumber,
                this.historyLength);

            return fastBurgAlgorithm.GetForwardPrediction();
        }

        /// <inheritdoc/>
        public double GetBackward(double[] samples)
        {
            var fastBurgAlgorithm = new FastBurgAlgorithm64(samples);
            fastBurgAlgorithm.Train(
                samples.Length,
                this.coefficientsNumber,
                this.historyLength);

            return fastBurgAlgorithm.GetBackwardPrediction();
        }
    }
}