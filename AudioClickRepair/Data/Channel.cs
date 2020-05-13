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

        internal void GetReadyForScan()
        {
            var inputDataSize = this.predictor.InputDataSize;
            var errors = new double[this.Length];

            for (var position = inputDataSize; position < this.Length; position++)
            {
                var inputDataStart = position - inputDataSize;
                errors[position] = this.input[position]
                    - this.predictor.GetForward(
                        this.inputPatcher.GetRange(inputDataStart, inputDataSize));
            }

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

            this.IsReadyForScan = true;
        }

        internal void Scan()
        {
            if (!this.IsReadyForScan)
            {
                this.GetReadyForScan();
            }

            this.RemoveAllPatches();

            var start = Math.Max(
                this.patchMaker.InputDataSize,
                this.damageDetector.InputDataSize);

            var end = this.Length - this.patchMaker.InputDataSize;

            var suspectsList = new List<(int, double)>();

            for (var position = start; position < end; position++)
            {
                var errorLevelAtDetection = this.damageDetector.GetErrorLevel(position);

                if (errorLevelAtDetection > this.settings.ThresholdForDetection)
                {
                    suspectsList.Add((position, errorLevelAtDetection));
                    position += this.damageDetector.InputDataSize;
                }
            }

            suspectsList.AsParallel().ForAll(s => this.CheckSuspect(s));
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