﻿// <copyright file="Channel.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace CarefulAudioRepair.Data
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading.Tasks;
    using CarefulAudioRepair.Processing;

    /// <summary>
    /// Represents audio samples for one channel.
    /// </summary>
    internal class Channel : IDisposable, IChannel
    {
        private ScannerTools scannerTools;

        /// <summary>
        /// Initializes a new instance of the <see cref="Channel"/> class.
        /// </summary>
        /// <param name="inputSamples">Input audio samples.</param>
        /// <param name="settings">Audio setting.</param>
        public Channel(float[] inputSamples, IAudioProcessingSettings settings)
        {
            if (inputSamples is null)
            {
                throw new ArgumentNullException(nameof(inputSamples));
            }

            var inputImmutable = ImmutableArray.Create(inputSamples);

            this.scannerTools = new ScannerTools(inputImmutable, settings);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Channel"/> class.
        /// </summary>
        /// <param name="inputSamples">Input audio samples.</param>
        /// <param name="settings">Audio setting.</param>
        public Channel(ImmutableArray<float> inputSamples, IAudioProcessingSettings settings)
        {
            this.scannerTools = new ScannerTools(inputSamples, settings);
        }

        /// <summary>
        /// Gets a value indicating whether scan was performed once on this data
        /// so the prediction errors were calculated.
        /// </summary>
        public bool IsPreprocessed => this.scannerTools.IsPreprocessed;

        /// <summary>
        /// Gets length of audio in samples.
        /// </summary>
        public int LengthSamples => this.scannerTools.Input.Length;

        /// <summary>
        /// Gets number of patches.
        /// </summary>
        public int NumberOfPatches => this.scannerTools.PatchCollection.Count;

        /// <summary>
        /// Asynchronously scans audio for damaged samples and repairs them.
        /// </summary>
        /// <param name="status">Parameter to report status through.</param>
        /// <param name="progress">Parameter to report progress through.</param>
        /// <returns>Task.</returns>
        public async Task ScanAsync(
            string parentStatus,
            IProgress<string> status,
            IProgress<double> progress)
        {
            var scanner = new Scanner(this.scannerTools);

            this.scannerTools =
                await scanner.ScanAsync(parentStatus, status, progress).ConfigureAwait(false);

            foreach (var patch in this.scannerTools.PatchCollection.ToList())
            {
                this.RegisterPatch(patch);
            }
        }

        public ImmutableArray<float> GetInputArray() => this.scannerTools.Input;

        public float[] GetOutputArray() =>
            this.scannerTools.InputPatcher.GetAll();

        /// <summary>
        /// Returns array of patches generated by ScanAsync method.
        /// </summary>
        /// <returns>Array of patches.</returns>
        public Patch[] GetAllPatches()
        {
            var patchList = this.scannerTools.PatchCollection.ToList();
            patchList.Sort();
            return patchList.Select(p => p as Patch).ToArray();
        }

        /// <summary>
        /// Returns value of input sample at position.
        /// </summary>
        /// <param name="position">Position of input sample.</param>
        /// <returns>Value.</returns>
        public double GetInputSample(int position) => this.scannerTools.Input[position];

        /// <summary>
        /// Returns value of output sample at position.
        /// </summary>
        /// <param name="position">Position of output sample.</param>
        /// <returns>Value.</returns>
        public double GetOutputSample(int position) =>
            this.scannerTools.InputPatcher.GetValue(position);

        /// <summary>
        /// Returns value of prediction error at position.
        /// </summary>
        /// <param name="position">Position of prediction error.</param>
        /// <returns>Value.</returns>
        public double GetPredictionErr(int position) =>
            this.scannerTools.PredictionErrPatcher.GetValue(position);

        /// <inheritdoc/>
        public void Dispose()
        {
            this.scannerTools.Dispose();
        }

        /// <summary>
        /// Gets range of samples from input array.
        /// </summary>
        /// <param name="start">Start index.</param>
        /// <param name="length">Range length.</param>
        /// <returns>Array of input samples.</returns>
        public double[] GetInputRange(int start, int length) =>
            this.scannerTools.Input.Skip(start - 1).Take(length).Select(s => (double)s).ToArray();

        private void RegisterPatch(AbstractPatch patch)
        {
            patch.Updater += this.PatchUpdater;
        }

        private void PatchUpdater(object sender, EventArgs e) =>
            this.scannerTools.Regenerarator.RestorePatch(sender as AbstractPatch);
    }
}