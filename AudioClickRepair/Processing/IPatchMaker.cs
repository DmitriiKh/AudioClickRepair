// <copyright file="IPatchMaker.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace AudioClickRepair.Processing
{
    using AudioClickRepair.Data;

    internal interface IPatchMaker
    {
        /// <summary>
        /// Gets size of input data array needed to perform calculations.
        /// </summary>
        /// <returns>Size.</returns>
        int InputDataSize { get; }

        AbstractPatch NewPatch(
            int position,
            int maxLengthOfCorrection,
            double errorLevelAtDetection);
    }
}