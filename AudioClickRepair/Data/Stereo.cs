// <copyright file="Stereo.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace AudioClickRepair.Data
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    ///     Represents stereo audio samples and includes information
    ///     about damaged samples.
    /// </summary>
    public sealed class Stereo : IAudio
    {
        private readonly Channel leftChannel;
        private readonly Channel rightChannel;

        /// <summary>
        /// Initializes a new instance of the <see cref="Stereo"/> class.
        /// </summary>
        /// <param name="leftChannelSamples">Input samples (left channel).</param>
        /// <param name="rightChannelSamples">Input samples (right channel.</param>
        /// <param name="settings">Settings associated with this audio data.</param>
        public Stereo(
            double[] leftChannelSamples,
            double[] rightChannelSamples,
            IAudioProcessingSettings settings)
        {
            if (settings is null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            this.Settings = settings;
            this.leftChannel = new Channel(leftChannelSamples, settings);
            this.rightChannel = new Channel(rightChannelSamples, settings);
        }

        /// <inheritdoc/>
        public bool IsStereo => true;

        /// <inheritdoc/>
        public int LengthSamples => this.leftChannel.LengthSamples;

        /// <inheritdoc/>
        public IAudioProcessingSettings Settings { get; }

        /// <inheritdoc/>
        public async Task ScanAsync(
            IProgress<string> status,
            IProgress<double> progress)
        {
            await this.leftChannel.ScanAsync(status, progress)
                .ConfigureAwait(false);
            await this.rightChannel.ScanAsync(status, progress)
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public int GetTotalNumberOfPatches() =>
            this.leftChannel.NumberOfPatches + this.rightChannel.NumberOfPatches;

        /// <inheritdoc/>
        public int GetNumberOfPatches(ChannelType channelType) =>
            channelType == ChannelType.Left
            ? this.leftChannel.NumberOfPatches
            : this.rightChannel.NumberOfPatches;

        /// <inheritdoc/>
        public Patch[] GetPatches(ChannelType channelType) =>
            channelType == ChannelType.Left
            ? this.leftChannel.GetAllPatches()
            : this.rightChannel.GetAllPatches();

        /// <inheritdoc/>
        public bool ChannelIsPreprocessed(ChannelType channelType) =>
            channelType == ChannelType.Left
            ? this.leftChannel.IsPreprocessed
            : this.rightChannel.IsPreprocessed;

        /// <inheritdoc/>
        public double GetInputSample(ChannelType channelType, int index) =>
            channelType == ChannelType.Left
            ? this.leftChannel.GetInputSample(index)
            : this.rightChannel.GetInputSample(index);

        /// <inheritdoc/>
        public double GetOutputSample(ChannelType channelType, int position) =>
            channelType == ChannelType.Left
            ? this.leftChannel.GetOutputSample(position)
            : this.rightChannel.GetOutputSample(position);

        /// <inheritdoc/>
        public double GetPredictionErr(ChannelType channelType, int index) =>
            channelType == ChannelType.Left
            ? this.leftChannel.GetPredictionErr(index)
            : this.rightChannel.GetPredictionErr(index);

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
            this.leftChannel.Dispose();
            this.rightChannel.Dispose();
        }
    }
}