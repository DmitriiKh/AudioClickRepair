﻿// <copyright file="Regenerator.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace AudioClickRepair.Processing
{
    using System;
    using AudioClickRepair.Data;

    internal class Regenerator : IRegenerator
    {
        private IPatcher inputSource;
        private IPredictor predictor;

        public int InputDataSize => this.predictor.InputDataSize;

        public Regenerator(IPatcher inputSource, IPredictor predictor)
        {
            this.inputSource = inputSource;
            this.predictor = predictor;
        }

        public double RestoreFragment(AbstractFragment fragment)
        {
            var forwardRestoredSamples = this.GetForwardArray(fragment);
            var backwardRestoredSamples = this.GetBackwardArray(fragment);
            var joinedRestoredSamples = this.ApplyWindowFunction(
                forwardRestoredSamples,
                backwardRestoredSamples);

            fragment.SetInternalArray(joinedRestoredSamples);

            var middleIndex = forwardRestoredSamples.Length / 2;

            return Math.Abs(forwardRestoredSamples[middleIndex]
                - backwardRestoredSamples[middleIndex]);
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

            if (length == 1)
            {
                return new double[]
                    {
                        (forwardRestoredSamples[0] + backwardRestoredSamples[0]) / 2,
                    };
            }

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

            var output = new double[fragment.Length];

            for (var index = 0; index < output.Length; index++)
            {
                var input = this.inputSource.GetRange(
                    fragment.StartPosition - expandSize + index,
                    expandSize);

                output[index] = this.predictor.GetForward(input);
            }

            return output;
        }

        private double[] GetBackwardArray(AbstractFragment fragment)
        {
            /*
               |+++++++++|-------------------------------|
                 fragment           extension
             */

            var expandSize = this.predictor.InputDataSize;

            var output = new double[fragment.Length];

            for (var index = 0; index < output.Length; index++)
            {
                var input = this.inputSource.GetRange(
                    fragment.StartPosition + index + 1,
                    expandSize);

                output[index] = this.predictor.GetBackward(input);
            }

            return output;
        }
    }
}