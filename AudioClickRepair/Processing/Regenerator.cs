﻿// <copyright file="Regenerator.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace AudioClickRepair.Processing
{
    using AudioClickRepair.Data;

    internal class Regenerator : IRegenerator
    {
        private IPatcher inputSource;
        private IPredictor predictor;

        public Regenerator(IPatcher inputSource, IPredictor predictor)
        {
            this.inputSource = inputSource;
            this.predictor = predictor;
        }

        public void RestoreFragment(AbstractFragment fragment)
        {
            var backwardRestoredSamples = this.GetBackwardArray(fragment);
            var forwardRestoredSamples = this.GetForwardArray(fragment);
            var joinedRestoredSamples = this.ApplyWindowFunction(forwardRestoredSamples, backwardRestoredSamples);

            fragment.SetInternalArray(joinedRestoredSamples);
        }

        private double[] ApplyWindowFunction(
            double[] forwardRestoredSamples,
            double[] backwardRestoredSamples)
        {
            /*
            . = 1.0                                         . = 1.0 (coefficient for backward)
                    .                               .
                            .               .
                                    .
                            .               .
                    .                              .
            . = 0.0                                         . = 0.0 (coefficient for forward)
            0       1       2       3       4       5       6       (index)
            */

            var length = forwardRestoredSamples.Length;
            var increment = 1 / (length - 1);
            var outputArray = new double[length];

            for (var index = 0; index < length; index++)
            {
                var backwardCoef = index * increment;
                var forwardCoef = 1 - backwardCoef;

                outputArray[index] = (forwardRestoredSamples[index] * forwardCoef)
                    + (backwardRestoredSamples[index] * backwardCoef);
            }

            return outputArray;
        }

        private double[] GetForwardArray(AbstractFragment fragment)
        {
            /*
               |-------------------------------|+++++++++|
                        extension                fragment
             */

            var expandSize = this.predictor.InputDataSize;
            var totalSize = expandSize + fragment.Length;

            var samples = this.inputSource.GetRange(
                fragment.StartPosition - expandSize,
                totalSize);

            for (var index = expandSize; index < samples.Length; index++)
            {
                samples[index] = this.predictor.GetForward(samples, index);
            }

            return samples[expandSize..];
        }

        private double[] GetBackwardArray(AbstractFragment fragment)
        {
            /*
               |+++++++++|-------------------------------|
                 fragment           extension
             */

            var expandSize = this.predictor.InputDataSize;
            var totalSize = expandSize + fragment.Length;

            var samples = this.inputSource.GetRange(
                fragment.StartPosition,
                totalSize);

            for (var index = fragment.Length - 1; index >= 0; index--)
            {
                samples[index] = this.predictor.GetBackward(samples, index);
            }

            return samples[..fragment.Length];
        }
    }
}