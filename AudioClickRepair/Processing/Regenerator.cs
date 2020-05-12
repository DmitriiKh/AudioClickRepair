// <copyright file="Regenerator.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace AudioClickRepair.Processing
{
    using System;
    using AudioClickRepair.Data;

    internal class Regenerator : IRegenerator
    {
        private readonly IPatcher inputSource;
        private readonly IPredictor predictor;
        private readonly IDetector detector;

        public int InputDataSize => this.predictor.InputDataSize;

        public Regenerator(
            IPatcher inputSource,
            IPredictor predictor,
            IDetector detector)
        {
            this.inputSource = inputSource;
            this.predictor = predictor;
            this.detector = detector;
        }

        public void RestorePatch(AbstractPatch patch)
        {
            var forwardRestoredSamples = this.GetForwardArray(patch);
            var backwardRestoredSamples = this.GetBackwardArray(patch);
            var joinedRestoredSamples = this.ApplyWindowFunction(
                forwardRestoredSamples,
                backwardRestoredSamples);

            patch.SetInternalArray(joinedRestoredSamples);

            patch.ErrorLevelAtStart = this.GetErrorLevelAtStart(patch);
            patch.ConnectionError = this.GetConnectionError(
                forwardRestoredSamples,
                backwardRestoredSamples);
            patch.ErrorLevelAfterEnd = this.GetErrorLevelAfterEnd(patch);
        }

        private double GetErrorLevelAtStart(AbstractPatch patch) =>
            this.detector.GetErrorLevel(patch.StartPosition, patch);

        private double GetConnectionError(
            double[] forwardRestoredSamples,
            double[] backwardRestoredSamples)
        {
            var errorSum = 0.0;

            for (var index = 0; index < forwardRestoredSamples.Length; index++)
            {
                errorSum += Math.Abs(
                    forwardRestoredSamples[index] -
                    backwardRestoredSamples[index]);
            }

            return errorSum / forwardRestoredSamples.Length;
        }

        private double GetErrorLevelAfterEnd(AbstractPatch patch) =>
            (this.detector.GetErrorLevel(patch.EndPosition + 1, patch) +
            this.detector.GetErrorLevel(patch.EndPosition + 2, patch) +
            this.detector.GetErrorLevel(patch.EndPosition + 3, patch)) / 3;

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

            var increment = 1.0 / (length - 1);
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
                samples[index] = this.predictor.GetForward(
                    samples[(index - expandSize) .. index]);
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
                samples[index] = this.predictor.GetBackward(
                    samples[(index + 1) .. (index + expandSize + 1)]);
            }

            return samples[..fragment.Length];
        }
    }
}