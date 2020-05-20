// <copyright file="IDetector.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace CarefulAudioRepair.Processing
{
    using CarefulAudioRepair.Data;

    /// <summary>
    /// Detects prediction error level.
    /// </summary>
    public interface IDetector
    {
        /// <summary>
        /// Gets size of input data needed to perform calculations.
        /// </summary>
        int InputDataSize { get; }

        /// <summary>
        /// Returns prediction error level for a sample at specific position.
        /// </summary>
        /// <param name="position">Position of sample.</param>
        /// <param name="anotherPatch">Additional patch that is not included
        /// in the patch collection (optional).</param>
        /// <returns>Prediction error level.</returns>
        double GetErrorLevel(int position, AbstractPatch anotherPatch = null);
    }
}