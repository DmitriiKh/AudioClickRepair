// <copyright file="Channel.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace CarefulAudioRepair.Data
{
    internal interface IChannel
    {
        bool IsPreprocessed { get; }
        int LengthSamples { get; }
        int NumberOfPatches { get; }
        Patch[] GetAllPatches();
        double[] GetInputRange(int start, int length);
        double GetInputSample(int position);
        double GetOutputSample(int position);
        double GetPredictionErr(int position);
        Task ScanAsync(string parentStatus, IProgress<string> status, IProgress<double> progress);
        ImmutableArray<double> GetInputArray();
        double[] GetOutputArray();
    }
}