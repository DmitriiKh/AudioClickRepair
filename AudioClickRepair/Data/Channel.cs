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
        private readonly ImmutableArray<double> predictionErr;
        private readonly IPatcher inputPatcher;
        private readonly IPatcher predictionErrPatcher;
        private readonly IAnalyzer normCalculator;

        internal Channel(double[] inputSamples)
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

            this.predictionErr = ImmutableArray.Create(new double[inputSamples.Length]);
            this.predictionErrPatcher = new Patcher(
                this.predictionErr,
                this.patchCollection,
                (_, __) => AbstractPatch.MinimalPredictionError);

            this.IsReadyForScan = false;
        }

        internal void GetReadyForScan()
        {
            // calculate prediction errors
            this.IsReadyForScan = true;
        }

        internal bool IsReadyForScan { get; private set; }

        internal int LengthSamples() => this.input.Length;

        internal double GetInputSample(int position) => this.input[position];

        internal double GetOutputSample(int position) =>
            this.inputPatcher.GetValue(position);

        internal double GetPredictionErr(int position) =>
            this.predictionErrPatcher.GetValue(position);

        internal int GetNumberOfPatches() => this.patchCollection.Count;

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
            // remove sender from the patch collection
            // update array
            // update current error level
            // add the sender back to the patch collection
            throw new NotImplementedException();
        }

        private double GetPredictionErrNorm(int position)
        {
            var startIndex = position - this.normCalculator.GetInputDataSize();

            // If there is not enough data to analyze
            if (startIndex < 0)
            {
                return this.normCalculator.GetDefaultResult();
            }

            return this.normCalculator.GetResult(
                this.predictionErrPatcher.GetRange(
                    position - this.normCalculator.GetInputDataSize(),
                    this.normCalculator.GetInputDataSize()));
        }
    }
}