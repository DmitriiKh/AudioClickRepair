// <copyright file="FastBurgPredictor.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace AudioClickRepair.Processing
{
    internal class FastBurgPredictor : IPredictor
    {
        private readonly int coefficientsNumber;
        private readonly int historyLength;

        public int InputDataSize => this.historyLength;

        public FastBurgPredictor(int coefficientsNumber, int historyLength)
        {
            this.coefficientsNumber = coefficientsNumber;
            this.historyLength = historyLength;
        }

        public double GetForward(double[] samples, int position)
        {
            var fastBurgAlgorithm = new FastBurgAlgorithm64(samples);
            fastBurgAlgorithm.Train(position, this.coefficientsNumber, this.historyLength);

            return fastBurgAlgorithm.GetForwardPrediction();
        }

        public double GetBackward(double[] samples, int position)
        {
            var fastBurgAlgorithm = new FastBurgAlgorithm64(samples);
            fastBurgAlgorithm.Train(position, this.coefficientsNumber, this.historyLength);

            return fastBurgAlgorithm.GetBackwardPrediction();
        }
    }
}