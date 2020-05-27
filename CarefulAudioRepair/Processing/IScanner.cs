// <copyright file="IScanner.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace CarefulAudioRepair.Processing
{
    using System;
    using System.Threading.Tasks;

    internal interface IScanner
    {
        public Task<ScannerTools> ScanAsync(
            IProgress<string> status,
            IProgress<double> progress);
    }
}
