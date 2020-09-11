﻿// <copyright file="MemoryEfficientChannel.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace CarefulAudioRepair.Data
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading.Tasks;
    using CarefulAudioRepair.Processing;

    /// <summary>
    /// Represents audio samples for one channel.
    /// </summary>
    internal class MemoryEfficientChannel : IChannel
    {
        private readonly ImmutableArray<double> inputImmutable;
        private readonly IAudioProcessingSettings settings;
        private readonly List<AbstractPatch> patches = new List<AbstractPatch>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryEfficientChannel"/> class.
        /// </summary>
        /// <param name="inputSamples">Input audio samples.</param>
        /// <param name="settings">Audio setting.</param>
        public MemoryEfficientChannel(double[] inputSamples, IAudioProcessingSettings settings)
        {
            if (inputSamples is null)
            {
                throw new ArgumentNullException(nameof(inputSamples));
            }

            this.inputImmutable = ImmutableArray.Create(inputSamples);
            this.settings = settings;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryEfficientChannel"/> class.
        /// </summary>
        /// <param name="inputSamples">Input audio samples.</param>
        /// <param name="settings">Audio setting.</param>
        public MemoryEfficientChannel(ImmutableArray<double> inputSamples, IAudioProcessingSettings settings)
        {
            this.inputImmutable = inputSamples;
            this.settings = settings;
        }

        /// <summary>
        /// Gets a value indicating whether scan was performed once on this data
        /// so the prediction errors were calculated.
        /// </summary>
        public bool IsPreprocessed => false;

        /// <summary>
        /// Gets length of audio in samples.
        /// </summary>
        public int LengthSamples => this.inputImmutable.Length;

        /// <summary>
        /// Gets number of patches.
        /// </summary>
        public int NumberOfPatches => this.patches.Count;

        /// <summary>
        /// Asynchronously scans audio for damaged samples and repairs them.
        /// </summary>
        /// <param name="parentStatus">Status from parent.</param>
        /// <param name="status">Parameter to report status through.</param>
        /// <param name="progress">Parameter to report progress through.</param>
        /// <returns>Task.</returns>
        public async Task ScanAsync(
            string parentStatus,
            IProgress<string> status,
            IProgress<double> progress)
        {
            this.patches.Clear();

            const int chunkLength = 1000000;
            var overlap = this.settings.HistoryLengthSamples * 2;

            for (int start = 0, chunkIndex = 1;
                start < this.LengthSamples;
                start += chunkLength, chunkIndex++)
            {
                var endExcluding = Math.Min(
                    start + chunkLength + overlap,
                    this.inputImmutable.Length);

                var input = Enumerable.Range(start, endExcluding - start)
                    .Select(i => this.inputImmutable[i])
                    .ToImmutableArray();

                var scanner = new Scanner(new ScannerTools(input, this.settings));

                var tools = await scanner.ScanAsync(
                        parentStatus + chunkIndex + "-",
                        status,
                        progress)
                    .ConfigureAwait(false);

                foreach (var patch in tools.PatchCollection.ToList())
                {
                    var newPatch = new Patch(
                        patch.GetInternalArray(),
                        patch.StartPosition + start,
                        patch.ErrorLevelAtDetection);

                    this.RegisterPatch(newPatch);

                    this.patches.Add(newPatch);
                }
            }
        }

        public ImmutableArray<double> GetInputArray() => this.inputImmutable;

        public double[] GetOutputArray()
        {
            var tools = new ScannerTools(this.inputImmutable, this.settings, this.patches);
            var outputArray = tools.InputPatcher.GetRange(0, this.LengthSamples - 1);
            tools.Dispose();

            return outputArray;
        }

        /// <summary>
        /// Returns array of patches generated by ScanAsync method.
        /// </summary>
        /// <returns>Array of patches.</returns>
        public Patch[] GetAllPatches()
        {
            var patchList = this.patches.ToList();
            patchList.Sort();
            return patchList.Select(p => p as Patch).ToArray();
        }

        /// <summary>
        /// Returns value of input sample at position.
        /// </summary>
        /// <param name="position">Position of input sample.</param>
        /// <returns>Value.</returns>
        public double GetInputSample(int position) => this.inputImmutable[position];

        /// <summary>
        /// Returns value of output sample at position.
        /// </summary>
        /// <param name="position">Position of output sample.</param>
        /// <returns>Value.</returns>
        public double GetOutputSample(int position)
        {
            var tools = new ScannerTools(this.inputImmutable, this.settings, this.patches);
            var outputSample = tools.InputPatcher.GetValue(position);
            tools.Dispose();

            return outputSample;
        }

        /// <summary>
        /// Returns value of prediction error at position.
        /// </summary>
        /// <param name="position">Position of prediction error.</param>
        /// <returns>Value.</returns>
        public double GetPredictionErr(int position)
        {
            var tools = new ScannerTools(this.inputImmutable, this.settings);
            var predictionErr = tools.PredictionErrPatcher.GetValue(position);
            tools.Dispose();

            return predictionErr;
        }

        /// <summary>
        /// Gets range of samples from input array.
        /// </summary>
        /// <param name="start">Start index.</param>
        /// <param name="length">Range length.</param>
        /// <returns>Array of input samples.</returns>
        public double[] GetInputRange(int start, int length) =>
            this.inputImmutable.Skip(start - 1).Take(length).ToArray();

        private void RegisterPatch(AbstractPatch patch)
        {
            patch.Updater += this.PatchUpdater;
        }

        private void PatchUpdater(object sender, EventArgs e)
        {
            var tools = new ScannerTools(this.inputImmutable, this.settings);
            tools.Regenerarator.RestorePatch(sender as AbstractPatch);
            tools.Dispose();
        }
    }
}