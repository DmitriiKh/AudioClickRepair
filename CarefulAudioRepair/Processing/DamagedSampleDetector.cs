// <copyright file="DamagedSampleDetector.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace CarefulAudioRepair.Processing
{
    using System;
    using CarefulAudioRepair.Data;

    /// <summary>
    /// Detects prediction error level as a ratio of current prediction error
    /// and normal prediction error.
    /// </summary>
    internal class DamagedSampleDetector : IDetector
    {
        private readonly IAnalyzer normCalculator;
        private readonly IPatcher predictionErrPatcher;
        private readonly IPatcher inputPatcher;
        private readonly IPredictor predictor;

        /// <summary>
        /// Initializes a new instance of the <see cref="DamagedSampleDetector"/> class.
        /// </summary>
        /// <param name="predictionErrPatcher">Source of prediction errors
        /// for normCalculator.</param>
        /// <param name="inputPatcher">Source of input samples for
        /// calculating prediction errors.</param>
        /// <param name="normCalculator">Calculator for normal prediction errors.</param>
        /// <param name="predictor">Calculator for predictions.</param>
        public DamagedSampleDetector(
            IPatcher predictionErrPatcher,
            IPatcher inputPatcher,
            IAnalyzer normCalculator,
            IPredictor predictor)
        {
            this.predictionErrPatcher = predictionErrPatcher;
            this.inputPatcher = inputPatcher;
            this.normCalculator = normCalculator;
            this.predictor = predictor;
        }

        /// <inheritdoc/>
        public int InputDataSize =>
            this.predictor.InputDataSize
            + this.normCalculator.InputDataSize;

        /// <inheritdoc/>
        public double GetErrorLevel(int position, AbstractPatch anotherPatch)
        {
            var errors = this.predictionErrPatcher.GetRange(
                    position - this.normCalculator.InputDataSize,
                    this.normCalculator.InputDataSize,
                    anotherPatch);

            var normalError = this.normCalculator.GetResult(errors);

            var inputSamples = this.inputPatcher.GetRange(
                position - this.predictor.InputDataSize,
                this.predictor.InputDataSize,
                anotherPatch);

            var errorAtPosition = Math.Abs(this.predictor.GetForward(inputSamples)
                - this.inputPatcher.GetValue(position));

            return errorAtPosition / normalError;
        }
    }
}
