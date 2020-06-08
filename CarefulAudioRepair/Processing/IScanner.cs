// <copyright file="IScanner.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace CarefulAudioRepair.Processing
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for Scanner.
    /// </summary>
    internal interface IScanner
    {
        /// <summary>
        /// Scans audio using ScannerTools.
        /// Changes ScannerTools.PatchCollection and ScannerTools.PredictionErrPatcher.
        /// </summary>
        /// <param name="status">Parameter to report status through.</param>
        /// <param name="progress">Parameter to report progress through.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public Task<ScannerTools> ScanAsync(
            IProgress<string> status,
            IProgress<double> progress);
    }
}
