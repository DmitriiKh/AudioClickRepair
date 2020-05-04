// <copyright file="IPatchMaker.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace AudioClickRepair.Processing
{
    using AudioClickRepair.Data;

    internal interface IPatchMaker
    {
        AbstractPatch NewPatch(
            int position,
            int maxLengthOfCorrection,
            double errorLevelAtDetection);
    }
}