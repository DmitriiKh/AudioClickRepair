// <copyright file="IAudioProcessingSettings.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace CarefulAudioRepair.Data
{
    /// <summary>
    /// Interface for audio settings.
    /// </summary>
    public interface IAudioProcessingSettings
    {
        /// <summary>
        /// Gets or sets number of samples used to calculate prediction errors.
        /// </summary>
        public int HistoryLengthSamples { get; set; }

        /// <summary>
        /// Gets or sets number of coefficients used to calculate prediction errors.
        /// </summary>
        public int CoefficientsNumber { get; set; }

        /// <summary>
        /// Gets or sets threshold for error level.
        /// Error level is ratio of current prediction error value
        /// to statistically normal error.
        /// Lower threshold makes the algorithm more sensitive.
        /// </summary>
        public double ThresholdForDetection { get; set; }

        /// <summary>
        /// Gets or sets max number of samples that can be reconstructed.
        /// Lower value makes scanning faster, but longer sequences
        /// of damaged samples may not be fixed.
        /// </summary>
        public int MaxLengthOfCorrection { get; set; }

        /// <summary>
        /// Gets or sets sample rate of audio data.
        /// </summary>
        public int SampleRate { get; set; }
    }
}