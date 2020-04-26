// <copyright file="IPatch.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace AudioClickRepair.Data
{
    /// <summary>
    /// IPatch interface.
    /// </summary>
    public interface IPatch
    {
        /// <summary>
        /// Gets relative position of the first sample of patch.
        /// </summary>
        int StartPosition { get; }

        /// <summary>
        /// Gets relative position of the last sample of patch.
        /// </summary>
        int EndPosition { get; }

        /// <summary>
        /// Gets minimal allowed value of prediction error.
        /// </summary>
        static int MinimalPredictionError { get; } = 0;

        /// <summary>
        /// Returns patched value for relative position.
        /// </summary>
        /// <returns>Patched value (double).</returns>
        /// <param name="position">Relative position.</param>
        double GetOutputSample(int position);
    }
}