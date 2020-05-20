// <copyright file="IPredictor.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace CarefulAudioRepair.Processing
{
    /// <summary>
    /// Gives predictions for a sequence of samples.
    /// </summary>
    public interface IPredictor
    {
        /// <summary>
        /// Gets number of samples needed to make calculations.
        /// </summary>
        int InputDataSize { get; }

        /// <summary>
        /// Returns forward prediction for the sequence of samples.
        /// </summary>
        /// <param name="samples">Sequence of samples.</param>
        /// <returns>Forward prediction.</returns>
        double GetForward(double[] samples);

        /// <summary>
        /// Returns backward prediction for the sequence of samples.
        /// </summary>
        /// <param name="samples">Sequence of samples.</param>
        /// <returns>Backward prediction.</returns>
        double GetBackward(double[] samples);
    }
}
