// <copyright file="Scanner.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace CarefulAudioRepair.Processing
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading.Tasks;
    using CarefulAudioRepair.Data;

    class Scanner : IScanner
    {
        private readonly BlockingCollection<AbstractPatch> patchCollection;
        private readonly ImmutableArray<double> input;
        private readonly IPatcher inputPatcher;
        private readonly IAnalyzer normCalculator;
        private readonly IAudioProcessingSettings settings;
        private readonly IPredictor predictor;
        private IRegenerator regenerarator;
        private IPatchMaker patchMaker;
        private IDetector damageDetector;
        private ImmutableArray<double> predictionErr;
        private IPatcher predictionErrPatcher;
        private bool isPreprocessed = false;

        public Scanner(ImmutableArray<double> inputSamples, IAudioProcessingSettings settings)
        {
            this.patchCollection = new BlockingCollection<AbstractPatch>();

            this.input = inputSamples;

            this.settings = settings;

            this.inputPatcher = new Patcher(
                this.input,
                this.patchCollection,
                (patch, position) => patch.GetValue(position));

            this.predictor = new FastBurgPredictor(
                settings.CoefficientsNumber,
                settings.HistoryLengthSamples);

            this.normCalculator = new AveragedMaxErrorAnalyzer();
        }

        public async Task<(BlockingCollection<AbstractPatch>, IPatcher, IPatcher, IRegenerator)> ScanAsync(
            IProgress<string> status,
            IProgress<double> progress)
        {
            return await Task.Run(() => this.Scan(status, progress)).ConfigureAwait(false);
        }

        private (BlockingCollection<AbstractPatch>, IPatcher, IPatcher, IRegenerator) Scan(IProgress<string> status, IProgress<double> progress)
        {
            if (!this.isPreprocessed)
            {
                this.GetReady(status, progress);
            }

            var suspects = this.DetectSuspiciousSamples(status, progress);

            if (suspects.Any())
            {
                this.GenerateNewPatches(suspects, status, progress);
            }

            status.Report(string.Empty);
            progress.Report(100);

            return (this.patchCollection,this.inputPatcher, this.predictionErrPatcher, this.regenerarator);
        }

        private void GetReady(
            IProgress<string> status,
            IProgress<double> progress)
        {
            status.Report("Preparation");
            progress.Report(0);

            var errors = this.CalculatePredictionErrors(progress);

            // Initialize fields that depend on prediction errors
            this.predictionErr = ImmutableArray.Create(errors);
            this.predictionErrPatcher = new Patcher(
                this.predictionErr,
                this.patchCollection,
                (_, __) => AbstractPatch.MinimalPredictionError);

            this.damageDetector = new DamagedSampleDetector(
                this.predictionErrPatcher,
                this.inputPatcher,
                this.normCalculator,
                this.predictor);

            this.regenerarator = new Regenerator(
                this.inputPatcher,
                this.predictor,
                this.damageDetector);

            this.patchMaker = new PatchMaker(this.regenerarator);

            progress.Report(100);

            // The fields are initialized
            this.isPreprocessed = true;
        }

        private double[] CalculatePredictionErrors(
            IProgress<double> progress)
        {
            var errors = new double[this.input.Length];

            var inputDataSize = this.predictor.InputDataSize;

            var start = inputDataSize;
            var end = this.input.Length;

            if (start >= end)
            {
                return errors;
            }

            var chunkSize = Math.Max(
                inputDataSize,
                (end - start) / Environment.ProcessorCount);

            var part = Partitioner.Create(start, end, chunkSize);

            Parallel.ForEach(part, (range, state, index) =>
            {
                for (var position = range.Item1; position < range.Item2; position++)
                {
                    var inputDataStart = position - inputDataSize;

                    errors[position] = this.input[position]
                        - this.predictor.GetForward(
                            this.inputPatcher.GetRange(inputDataStart, inputDataSize));

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

            return errors;
        }

        private (int start, int length, double errorLevelAtDetection)[] DetectSuspiciousSamples(
            IProgress<string> status,
            IProgress<double> progress)
        {
            status.Report("Detection");
            progress.Report(0);

            var suspectsList = new List<(int, int, double)>();

            var start = Math.Max(
                this.patchMaker.InputDataSize,
                this.damageDetector.InputDataSize);

            var end = this.input.Length
                - (this.patchMaker.InputDataSize
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
                        this.damageDetector.GetErrorLevel(position);

                    if (errorLevelAtDetection > this.settings.ThresholdForDetection)
                    {
                        var lengthToSkip = this.settings.MaxLengthOfCorrection
                            + this.damageDetector.InputDataSize;

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
            var firstPatch = this.patchMaker.NewPatch(
                    suspect.start,
                    this.settings.MaxLengthOfCorrection,
                    suspect.errorLevelAtDetection);

            this.patchCollection.Add(firstPatch);

            var end = suspect.start + suspect.length;

            for (var position = firstPatch.EndPosition + 1; position < end; position++)
            {
                var errorLevelAtDetection = this.damageDetector.GetErrorLevel(position);

                if (errorLevelAtDetection > this.settings.ThresholdForDetection)
                {
                    var patch = this.patchMaker.NewPatch(
                    position,
                    this.settings.MaxLengthOfCorrection,
                    suspect.errorLevelAtDetection);

                    this.patchCollection.Add(patch);

                    var newEnd = patch.StartPosition + suspect.length;
                    end = Math.Max(end, newEnd);
                }
            }
        }
    }
}
