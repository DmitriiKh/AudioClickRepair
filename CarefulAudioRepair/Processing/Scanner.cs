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

    /// <summary>
    /// This class contains behavior for scanning audio for damaged samples.
    /// </summary>
    internal class Scanner : IScanner
    {
        private readonly ScannerTools tools;

        /// <summary>
        /// Initializes a new instance of the <see cref="Scanner"/> class.
        /// </summary>
        /// <param name="tools">Set of tools.</param>
        public Scanner(ScannerTools tools)
        {
            this.tools = tools;
        }

        /// <inheritdoc/>
        public async Task<ScannerTools> ScanAsync(
            IProgress<string> status,
            IProgress<double> progress)
        {
            return await Task.Run(() => this.Scan(status, progress)).ConfigureAwait(false);
        }

        private ScannerTools Scan(IProgress<string> status, IProgress<double> progress)
        {
            if (!this.tools.IsPreprocessed)
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

        private Suspect[] DetectSuspiciousSamples(
            IProgress<string> status,
            IProgress<double> progress)
        {
            status.Report("Detection");
            progress.Report(0);

            var suspectsList = new List<Suspect>();

            var start = Math.Max(
                this.tools.PatchMaker.InputDataSize,
                this.tools.DamageDetector.InputDataSize);

            var end = this.tools.Input.Length
                - (this.tools.PatchMaker.InputDataSize
                    + this.tools.Settings.MaxLengthOfCorrection);

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

                    if (errorLevelAtDetection > this.tools.Settings.ThresholdForDetection)
                    {
                        var lengthToSkip = this.tools.Settings.MaxLengthOfCorrection
                            + this.tools.DamageDetector.InputDataSize;

                        suspectsList.Add(new Suspect(position, lengthToSkip, errorLevelAtDetection));
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
            Suspect[] suspects,
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

            this.tools.PatchCollection.Finalize();

            progress.Report(100);
        }

        private void CheckSuspect(Suspect suspect)
        {
            var firstPatch = this.tools.PatchMaker.NewPatch(
                    suspect.Start,
                    this.tools.Settings.MaxLengthOfCorrection,
                    suspect.ErrorLevelAtDetection);

            this.tools.PatchCollection.Add(firstPatch);

            var end = suspect.Start + suspect.Length;

            for (var position = firstPatch.EndPosition + 1; position < end; position++)
            {
                var errorLevelAtDetection = this.tools.DamageDetector.GetErrorLevel(position);

                if (errorLevelAtDetection > this.tools.Settings.ThresholdForDetection)
                {
                    var patch = this.tools.PatchMaker.NewPatch(
                    position,
                    this.tools.Settings.MaxLengthOfCorrection,
                    suspect.ErrorLevelAtDetection);

                    this.tools.PatchCollection.Add(patch);

                    var newEnd = patch.StartPosition + suspect.Length;
                    end = Math.Max(end, newEnd);
                }
            }
        }

        private struct Suspect
        {
            public Suspect(int start, int length, double errorLevelAtDetection)
            {
                this.Start = start;
                this.Length = length;
                this.ErrorLevelAtDetection = errorLevelAtDetection;
            }

            public int Start { get; }

            public int Length { get; }

            public double ErrorLevelAtDetection { get; }
        }
    }
}
