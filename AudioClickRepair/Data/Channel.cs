// <copyright file="Channel.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace AudioClickRepair.Data
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AudioClickRepair.Processing;

    internal class Channel
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

        internal Channel(double[] inputSamples, IAudioProcessingSettings settings)
        {
            if (inputSamples is null)
            {
                throw new ArgumentNullException(nameof(inputSamples));
            }

            this.patchCollection = new BlockingCollection<AbstractPatch>();

            this.input = ImmutableArray.Create(inputSamples);
            this.inputPatcher = new Patcher(
                this.input,
                this.patchCollection,
                (patch, position) => patch.GetValue(position));

            this.predictor = new FastBurgPredictor(
                settings.CoefficientsNumber,
                settings.HistoryLengthSamples);

            this.normCalculator = new AveragedMaxErrorAnalyzer();

            this.settings = settings;

            this.IsReadyForScan = false;
        }

        internal void GetReadyForScan(
            IProgress<string> status,
            IProgress<double> progress)
        {
            var errors = this.CalculatePredictionErrors(status, progress);

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

            // The fields are initialized
            this.IsReadyForScan = true;
        }

        private double[] CalculatePredictionErrors(
            IProgress<string> status,
            IProgress<double> progress)
        {
            status.Report("Preparation");
            progress.Report(0);

            var errors = new double[this.Length];

            var inputDataSize = this.predictor.InputDataSize;

            var part = Partitioner.Create(
                inputDataSize,
                this.Length,
                (this.Length - inputDataSize) / Environment.ProcessorCount);

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

            progress.Report(100);

            return errors;
        }

        internal async Task ScanAsync(
            IProgress<string> status,
            IProgress<double> progress)
        {
            await Task.Run(() => this.Scan(status, progress)).ConfigureAwait(false);
        }

        internal void Scan(IProgress<string> status, IProgress<double> progress)
        {
            if (!this.IsReadyForScan)
            {
                this.GetReadyForScan(status, progress);
            }

            this.RemoveAllPatches();

            var suspects = this.DetectSuspiciousPositions(status, progress);

            this.GenerateNewPatches(suspects, status, progress);

            status.Report(String.Empty);
            progress.Report(100);
        }

        private void GenerateNewPatches(
            (int, double)[] suspects,
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


        private (int, double)[] DetectSuspiciousPositions(
            IProgress<string> status,
            IProgress<double> progress)
        {
            status.Report("Detection");
            progress.Report(0);

            var suspectsList = new List<(int, double)>();

            var start = Math.Max(
                this.patchMaker.InputDataSize,
                this.damageDetector.InputDataSize);

            var end = this.Length - this.patchMaker.InputDataSize;

            var part = Partitioner.Create(
                start,
                end,
                (end - start) / Environment.ProcessorCount);

            Parallel.ForEach(part, (range, state, index) =>
            {
                for (var position = range.Item1; position < range.Item2; position++)
                {
                    var errorLevelAtDetection =
                        this.damageDetector.GetErrorLevel(position);

                    if (errorLevelAtDetection > this.settings.ThresholdForDetection)
                    {
                        suspectsList.Add((position, errorLevelAtDetection));
                        position += this.damageDetector.InputDataSize;
                    }

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

        private void CheckSuspect((int start, double errorLevelAtDetection) suspect)
        {

            var firstPatch = this.patchMaker.NewPatch(
                    suspect.start,
                    this.settings.MaxLengthOfCorrection,
                    suspect.errorLevelAtDetection);

            this.RegisterPatch(firstPatch);

            var maxCheckLength =
                this.settings.MaxLengthOfCorrection +
                this.damageDetector.InputDataSize;

            var end = suspect.start + maxCheckLength;

            for (var position = firstPatch.EndPosition + 1; position < end; position++)
            {
                var errorLevelAtDetection = this.damageDetector.GetErrorLevel(position);

                if (errorLevelAtDetection > this.settings.ThresholdForDetection)
                {
                    var patch = this.patchMaker.NewPatch(
                    position,
                    this.settings.MaxLengthOfCorrection,
                    suspect.errorLevelAtDetection);

                    this.RegisterPatch(patch);

                    var newEnd = firstPatch.EndPosition + maxCheckLength;
                    end = Math.Max(end, newEnd);
                }
            }
        }

        internal bool IsReadyForScan { get; private set; }

        internal int Length => this.input.Length;

        internal double GetInputSample(int position) => this.input[position];

        internal double GetOutputSample(int position) =>
            this.inputPatcher.GetValue(position);

        internal double GetPredictionErr(int position) =>
            this.predictionErrPatcher.GetValue(position);

        internal int NumberOfPatches => this.patchCollection.Count;

        private void RemoveAllPatches()
        {
            while (this.patchCollection.TryTake(out _))
            {
            }
        }

        internal Patch[] GetAllPatches()
        {
            var patchList = this.patchCollection.ToList();
            patchList.Sort();
            return patchList.Select(p => p as Patch).ToArray();
        }

        private void RegisterPatch(AbstractPatch patch)
        {
            this.patchCollection.Add(patch);
            patch.Updater += this.PatchUpdater;
        }

        private void PatchUpdater(object sender, EventArgs e) =>
            this.regenerarator.RestorePatch(sender as AbstractPatch);
    }
}