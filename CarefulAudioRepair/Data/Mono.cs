// <copyright file="Mono.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace CarefulAudioRepair.Data
{
    using System;
    using System.Collections.Immutable;
    using System.Threading.Tasks;

    /// <summary>
    ///     Represents mono audio samples and includes information
    ///     about damaged samples.
    /// </summary>
    public sealed class Mono : IAudio
    {
        private readonly IChannel monoChannel;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mono"/> class.
        /// </summary>
        /// <param name="samples">Input samples.</param>
        /// <param name="settings">Settings associated with this audio data.</param>
        /// <param name="memoryEfficient"></param>
        public Mono(float[] samples, IAudioProcessingSettings settings, bool memoryEfficient = false)
        {
            this.Settings = settings ?? throw new ArgumentNullException(nameof(settings));

            if (memoryEfficient)
            {
                this.monoChannel = new MemoryEfficientChannel(samples, settings);
            }
            else
            {
                this.monoChannel = new Channel(samples, settings);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mono"/> class.
        /// </summary>
        /// <param name="samples">Input samples.</param>
        /// <param name="settings">Settings associated with this audio data.</param>
        /// <param name="memoryEfficient"></param>
        public Mono(ImmutableArray<float> samples, IAudioProcessingSettings settings, bool memoryEfficient = false)
        {
            this.Settings = settings ?? throw new ArgumentNullException(nameof(settings));

            if (memoryEfficient)
            {
                this.monoChannel = new MemoryEfficientChannel(samples, settings);
            }
            else
            {
                this.monoChannel = new Channel(samples, settings);
            }
        }

        /// <inheritdoc/>
        public bool IsStereo => false;

        /// <inheritdoc/>
        public int LengthSamples => this.monoChannel.LengthSamples;

        /// <inheritdoc/>
        public IAudioProcessingSettings Settings { get; }

        /// <inheritdoc/>
        public async Task ScanAsync(
            IProgress<string> status,
            IProgress<double> progress)
        {
            await this.monoChannel.ScanAsync("Mono-", status, progress)
                .ConfigureAwait(false);

            status?.Report(string.Empty);
        }

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
            this.monoChannel.IsPreprocessed;

        /// <inheritdoc/>
        public double GetInputSample(ChannelType channelType, int index) =>
            this.monoChannel.GetInputSample(index);

        public ImmutableArray<float> GetInputArray(ChannelType channelType) =>
            this.monoChannel.GetInputArray();

        /// <inheritdoc/>
        public double GetOutputSample(ChannelType channelType, int position) =>
            this.monoChannel.GetOutputSample(position);

        /// <inheritdoc/>
        public double GetPredictionErr(ChannelType channelType, int index) =>
            this.monoChannel.GetPredictionErr(index);

        /// <inheritdoc/>
        public float[] GetOutputArray(ChannelType channelType) =>
            this.monoChannel.GetOutputArray();

        /// <inheritdoc/>
        public void Dispose()
        {
            if (this.monoChannel is IDisposable channel)
            {
                channel.Dispose();
            }
        }

        /// <inheritdoc/>
        public double[] GetInputRange(ChannelType channelType, int start, int length)
        {
            return this.monoChannel.GetInputRange(start, length);
        }
    }
}