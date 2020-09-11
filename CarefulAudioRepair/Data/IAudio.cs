// <copyright file="IAudio.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

// Make internal classes of whole the project visible to the Unit tests project
[assembly: InternalsVisibleTo("NUnitTests")]

namespace CarefulAudioRepair.Data
{
    /// <summary>
    /// Public interface of Mono and Stereo classes.
    /// </summary>
    public interface IAudio : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether the class contains stereo.
        /// </summary>
        public bool IsStereo { get; }

        /// <summary>
        /// Gets length of audio data in samples.
        /// </summary>
        public int LengthSamples { get; }

        /// <summary>
        /// Gets audio settings associated with data inside the class.
        /// </summary>
        public IAudioProcessingSettings Settings { get; }

        /// <summary>
        /// Asynchronously scans input data for damaged samples.
        /// </summary>
        /// <param name="status">IProress object to get reports on status of scanning.</param>
        /// <param name="progress">IProress object to get reports on progress of scanning.</param>
        /// <returns>Task.</returns>
        public Task ScanAsync(IProgress<string> status, IProgress<double> progress);

        /// <summary>
        /// Returns total number of patches applied to input data.
        /// </summary>
        /// <returns>Number of patches.</returns>
        public int GetTotalNumberOfPatches();

        /// <summary>
        /// Returns number of patches applied to channel.
        /// </summary>
        /// <param name="channelType">Left or Right channel.</param>
        /// <returns>Number of patches.</returns>
        public int GetNumberOfPatches(ChannelType channelType);

        /// <summary>
        /// Returns array of patches that were applied to channel during scanning.
        /// </summary>
        /// <param name="channelType">Left or Right channel.</param>
        /// <returns>Array of patches.</returns>
        public Patch[] GetPatches(ChannelType channelType);

        /// <summary>
        /// Returns true if the channel was preprocessed and ready for rescan.
        /// </summary>
        /// <param name="channelType">Left or Right channel.</param>
        /// <returns>True if the channel was preprocessed.</returns>
        public bool ChannelIsPreprocessed(ChannelType channelType);

        /// <summary>
        /// Returns value of input sample from specified channel at specified position.
        /// </summary>
        /// <param name="channelType">Left or Right channel.</param>
        /// <param name="position">Position of sample.</param>
        /// <returns>Value of sample.</returns>
        public double GetInputSample(ChannelType channelType, int position);

        /// <summary>
        /// Gets range of samples from input array.
        /// </summary>
        /// <param name="channelType">Left or Right channel.</param>
        /// <param name="start">Start index.</param>
        /// <param name="length">Range length.</param>
        /// <returns>Array of input samples.</returns>
        public double[] GetInputRange(ChannelType channelType, int start, int length);

        public ImmutableArray<float> GetInputArray(ChannelType channelType);

        /// <summary>
        /// Returns value of output sample from specified channel at specified position.
        /// </summary>
        /// <param name="channelType">Left or Right channel.</param>
        /// <param name="position">Position of sample.</param>
        /// <returns>Value of sample.</returns>
        public double GetOutputSample(ChannelType channelType, int position);

        /// <summary>
        /// Returns prediction error from specified channel at specified position.
        /// </summary>
        /// <param name="channelType">Left or Right channel.</param>
        /// <param name="position">Position of sample.</param>
        /// <returns>Prediction error.</returns>
        public double GetPredictionErr(ChannelType channelType, int position);

        /// <summary>
        /// Returns array of output samples from specified channel.
        /// </summary>
        /// <param name="channelType">Left or Right channel.</param>
        /// <returns>Array of samples.</returns>
        public float[] GetOutputArray(ChannelType channelType);
    }
}