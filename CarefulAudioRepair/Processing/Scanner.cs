// <copyright file="Scanner.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace CarefulAudioRepair.Processing
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CarefulAudioRepair.Data;

    internal class Scanner : IScanner
    {
        private readonly IAudioProcessingSettings settings;
        private bool isPreprocessed = false;
        private ScannerTools tools;

        public Scanner(ScannerTools tools)
        {
            this.tools = tools;

            this.settings = settings;
        }

        public async Task<ScannerTools> ScanAsync(
            IProgress<string> status,
            IProgress<double> progress)
        {
            return await Task.Run(() => this.Scan(status, progress)).ConfigureAwait(false);
        }

        private ScannerTools Scan(IProgress<string> status, IProgress<double> progress)
        {
            if (!this.isPreprocessed)
            {
                this.tools.GetReady(status, progress);
            }

            var suspects = this.DetectSuspiciousSamples(status, progress);

            if (suspects.Any())
            {
                this.GenerateNewPatches(suspects, status, progress);
            }

            status.Report(string.Empty);
            progress.Report(100);

            return this.tools;
        }

        private (int start, int length, double errorLevelAtDetection)[] DetectSuspiciousSamples(
            IProgress<string> status,
            IProgress<double> progress)
        {
            status.Report("Detection");
            progress.Report(0);

            var suspectsList = new List<(int, int, double)>();

            var start = Math.Max(
                this.tools.PatchMaker.InputDataSize,
                this.tools.DamageDetector.InputDataSize);

            var end = this.tools.Input.Length
                - (this.tools.PatchMaker.InputDataSize
                    + this.settings.MaxLengthOfCorrection);

            if (start >= end)
            {
                progress.Report(100);
                return suspectsList.ToArray();
            }

            var chunkSize = (end - start) / Environment.ProcessorCount;

            var part = Partitioner.Create(start, end, chunkSize);

            Parallel.ForEach(part, (range, state, index) =>
            {
                for (var position = range.Item1; position < range.Item2; position++)
                {
                    var errorLevelAtDetection =
                        this.tools.DamageDetector.GetErrorLevel(position);

                    if (errorLevelAtDetection > this.settings.ThresholdForDetection)
                    {
                        var lengthToSkip = this.settings.MaxLengthOfCorrection
                            + this.tools.DamageDetector.InputDataSize;

                        suspectsList.Add((position, lengthToSkip, errorLevelAtDetection));
                        position += lengthToSkip;
                    }

                    // Only the first thread reports
                    // Throttling by 1000 samples
                    if (index == 0 && position % 1000 == 0)
                    {
                        progress.Report(
                            100.0 * (position - range.Item1)
                            / (range.Item2 - range.Item1));
                    }
                }
            });

            progress.Report(100);

            return suspectsList.ToArray();
        }

        private void GenerateNewPatches(
            (int start, int length, double errorLevelAtDetection)[] suspects,
            IProgress<string> status,
            IProgress<double> progress)
        {
            status.Report("Restoration");
            progress.Report(0);

            var suspectRange = Partitioner.Create(
                0,
                suspects.Length,
                (int)Math.Ceiling((double)suspects.Length / Environment.ProcessorCount));

            Parallel.ForEach(suspectRange, (range, state, index) =>
            {
                for (var suspectIndex = range.Item1;
                    suspectIndex < range.Item2;
                    suspectIndex++)
                {
                    this.CheckSuspect(suspects[suspectIndex]);

                    // Only the first thread reports
                    if (index == 0)
                    {
                        progress.Report(
                            100.0 * (suspectIndex - range.Item1)
                            / (range.Item2 - range.Item1));
                    }
                }
            });

            progress.Report(100);
        }

        private void CheckSuspect((int start, int length, double errorLevelAtDetection) suspect)
        {
            var firstPatch = this.tools.PatchMaker.NewPatch(
                    suspect.start,
                    this.settings.MaxLengthOfCorrection,
                    suspect.errorLevelAtDetection);

            this.tools.PatchCollection.Add(firstPatch);

            var end = suspect.start + suspect.length;

            for (var position = firstPatch.EndPosition + 1; position < end; position++)
            {
                var errorLevelAtDetection = this.tools.DamageDetector.GetErrorLevel(position);

                if (errorLevelAtDetection > this.settings.ThresholdForDetection)
                {
                    var patch = this.tools.PatchMaker.NewPatch(
                    position,
                    this.settings.MaxLengthOfCorrection,
                    suspect.errorLevelAtDetection);

                    this.tools.PatchCollection.Add(patch);

                    var newEnd = patch.StartPosition + suspect.length;
                    end = Math.Max(end, newEnd);
                }
            }
        }
    }
}
