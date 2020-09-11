// <copyright file="IPatcher.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace CarefulAudioRepair.Processing
{
    /// <summary>
    /// Interface of a Patcher.
    /// </summary>
    internal interface IPatcher
    {
        /// <summary>
        /// Gets array containing sequence of patched samples of specified length
        /// starting from the specified position.
        /// </summary>
        /// <param name="start">Start position of range.</param>
        /// <param name="length">Length of range.</param>
        /// <param name="anotherPatch">One more optional AbstractPatch
        /// that is not in the Collection yet.</param>
        /// <returns>Array of patched samples.</returns>
        double[] GetRange(int start, int length, Data.AbstractPatch anotherPatch = null);

        /// <summary>
        /// Returns patched value of a sample.
        /// </summary>
        /// <param name="position">Position of the required sample.</param>
        /// <returns>Value of the sample.</returns>
        double GetValue(int position);

        float[] GetAll();
    }
}