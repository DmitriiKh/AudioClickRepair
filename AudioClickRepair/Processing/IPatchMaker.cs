// <copyright file="IPatchMaker.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace CarefulAudioRepair.Processing
{
    using CarefulAudioRepair.Data;

    /// <summary>
    /// Creates patches.
    /// </summary>
    internal interface IPatchMaker
    {
        /// <summary>
        /// Gets size of input data array needed to perform calculations.
        /// </summary>
        int InputDataSize { get; }

        /// <summary>
        /// Create patch at position.
        /// </summary>
        /// <param name="startPosition">Start position for the new patch.</param>
        /// <param name="maxLengthOfCorrection">Max limit for patch length.</param>
        /// <param name="errorLevelAtDetection">Error level calculated on detection stage.</param>
        /// <returns>New AbstractPatch.</returns>
        AbstractPatch NewPatch(
            int startPosition,
            int maxLengthOfCorrection,
            double errorLevelAtDetection);
    }
}