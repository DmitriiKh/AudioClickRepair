// <copyright file="PredictorBurg.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace AudioClickRepair.Processing
{
    internal class FastBurgPredictor : IPredictor
    {
        public FastBurgPredictor()
        {
        }

        public double GetForward(double[] samples, int index)
        {
            throw new System.NotImplementedException();
        }

        public double GetBackward(double[] samples, int index)
        {
            throw new System.NotImplementedException();
        }
    }
}