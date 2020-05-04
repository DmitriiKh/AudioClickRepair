// <copyright file="Channel.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace AudioClickRepair.Data
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Immutable;
    using System.Linq;
    using AudioClickRepair.Processing;

    internal class Channel
    {
        private readonly BlockingCollection<AbstractPatch> patchCollection;
        private readonly ImmutableArray<double> input;
        private readonly IPatcher inputPatcher;
        private readonly IPatcher predictionErrPatcher;
        private readonly IAnalyzer normCalculator;
        private readonly IRegenerator regenerarator;
        private readonly IAudioProcessingSettings settings;
        private readonly IPredictor predictor;
        private readonly IPatchMaker patchMaker;
        private ImmutableArray<double> predictionErr;

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

            // Create empty immutable array for errors. It will be replaced later.
            this.predictionErr = ImmutableArray.Create(new double[inputSamples.Length]);
            this.predictionErrPatcher = new Patcher(
                this.predictionErr,
                this.patchCollection,
                (_, __) => AbstractPatch.MinimalPredictionError);

            this.predictor = new FastBurgPredictor(
                settings.CoefficientsNumber,
                settings.HistoryLengthSamples);

            this.normCalculator = new AveragedMaxErrorAnalyzer();

            this.regenerarator = new Regenerator(this.inputPatcher, this.predictor);

            this.patchMaker = new PatchMaker(this.regenerarator);

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
                        this.inputPatcher.GetRange(inputDataStart, inputDataSize),
                        position);
            }

            this.predictionErr = ImmutableArray.Create(errors);

            this.IsReadyForScan = true;
        }

        internal void Scan()
        {
            if (!this.IsReadyForScan)
            {
                this.GetReadyForScan();
            }

            this.RemoveAllPatches();

            for (var position = 0; position < this.Length; position++)
            {
                var errorLevelAtDetection = this.GetErrorLevel(position);

                if (errorLevelAtDetection >= this.settings.ThresholdForDetection)
                {
                    var patch = this.patchMaker.NewPatch(
                        position,
                        this.settings.MaxLengthOfCorrection,
                        errorLevelAtDetection);

                    if (patch.ConnectionError < this.settings.MaxConnectionError)
                    {
                        this.RegisterPatch(patch);
                        position = patch.EndPosition + 1;
                    }
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

        internal AbstractPatch[] GetAllPatches()
        {
            var patchList = this.patchCollection.ToList();
            patchList.Sort();
            return patchList.ToArray();
        }

        private void RegisterPatch(AbstractPatch patch)
        {
            this.patchCollection.Add(patch);
            patch.Updater += this.PatchUpdater;
        }

        private void PatchUpdater(object sender, PatchEventArgs e)
        {
            this.regenerarator.RestoreFragment(e.Patched);
            e.NewErrorLevelAtStart = this.GetErrorLevel(e.Patched.StartPosition);
        }

        private double GetErrorLevel(int position) =>
            this.predictionErr[position]
                / this.GetPredictionErrNorm(position);

        private double GetPredictionErrNorm(int position)
        {
            var startIndex = position - this.normCalculator.InputDataSize;

            // If there is not enough data to analyze
            if (startIndex < 0)
            {
                return this.normCalculator.DefaultResult;
            }

            return this.normCalculator.GetResult(
                this.predictionErrPatcher.GetRange(
                    position - this.normCalculator.InputDataSize,
                    this.normCalculator.InputDataSize));
        }
    }
}