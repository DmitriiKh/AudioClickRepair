// <copyright file="IScanner.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace CarefulAudioRepair.Processing
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using CarefulAudioRepair.Data;

    internal interface IScanner
    {
        public Task<(BlockingCollection<AbstractPatch>, IPatcher, IPatcher, IRegenerator)> ScanAsync(
            IProgress<string> status,
            IProgress<double> progress);
    }
}
