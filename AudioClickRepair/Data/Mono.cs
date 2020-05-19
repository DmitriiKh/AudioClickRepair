// <copyright file="Mono.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace AudioClickRepair.Data
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    ///     Represents mono audio samples and includes information
    ///     about damaged samples.
    /// </summary>
    public sealed class Mono : IAudio, IDisposable
    {
        private readonly Channel monoChannel;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mono"/> class.
        /// </summary>
        /// <param name="samples">Input samples.</param>
        /// <param name="settings">Settings associated with this audio data.</param>
        public Mono(double[] samples, IAudioProcessingSettings settings)
        {
            if (settings is null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            this.Settings = settings;
            this.monoChannel = new Channel(samples, settings);
        }

        /// <inheritdoc/>
        public bool IsStereo => false;

        /// <inheritdoc/>
        public int LengthSamples => this.monoChannel.Length;

        /// <inheritdoc/>
        public IAudioProcessingSettings Settings { get; }

        /// <inheritdoc/>
        public async Task ScanAsync(
            IProgress<string> status,
            IProgress<double> progress) =>
            await this.monoChannel.ScanAsync(status, progress)
                .ConfigureAwait(false);

        /// <inheritdoc/>
        public int GetTotalNumberOfPatches() =>
            this.monoChannel.NumberOfPatches;

        /// <inheritdoc/>
        public int GetNumberOfPatches(ChannelType channelType) =>
            this.monoChannel.NumberOfPatches;

        /// <inheritdoc/>
        public Patch[] GetPatches(ChannelType channelType) =>
            this.monoChannel.GetAllPatches();

        /// <inheritdoc/>
        public bool ChannelIsPreprocessed(ChannelType channelType) =>
            this.monoChannel.IsReadyForScan;

        /// <inheritdoc/>
        public double GetInputSample(ChannelType channelType, int index) =>
            this.monoChannel.GetInputSample(index);

        /// <inheritdoc/>
        public double GetOutputSample(ChannelType channelType, int position) =>
            this.monoChannel.GetOutputSample(position);

        /// <inheritdoc/>
        public double GetPredictionErr(ChannelType channelType, int index) =>
            this.monoChannel.GetPredictionErr(index);

        /// <inheritdoc/>
        public double[] GetOutputArray(ChannelType channelType)
        {
            var array = new double[this.LengthSamples];

            for (var index = 0; index < array.Length; index++)
            {
                array[index] = this.GetOutputSample(channelType, index);
            }

            return array;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.monoChannel.Dispose();
        }
    }
}