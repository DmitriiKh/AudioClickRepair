// <copyright file="AudioProcessingSettings.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace AudioClickRepair.Data
{
    /// <summary>
    /// Set of settings for audio data.
    /// </summary>
    public class AudioProcessingSettings : IAudioProcessingSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AudioProcessingSettings"/> class.
        /// </summary>
        public AudioProcessingSettings()
        {
            this.HistoryLengthSamples = 512;
            this.CoefficientsNumber = 4;
            this.ThresholdForDetection = 10;
            this.MaxLengthOfCorrection = 250;
        }

        /// <inheritdoc/>
        public int HistoryLengthSamples { get; set; }

        /// <inheritdoc/>
        public int CoefficientsNumber { get; set; }

        /// <inheritdoc/>
        public double ThresholdForDetection { get; set; }

        /// <inheritdoc/>
        public int MaxLengthOfCorrection { get; set; }

        /// <inheritdoc/>
        public int SampleRate { get; set; } = -1;
    }
}