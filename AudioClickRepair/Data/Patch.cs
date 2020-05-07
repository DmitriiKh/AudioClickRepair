// <copyright file="Patch.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace AudioClickRepair.Data
{
    /// <summary>
    /// Contains information on sequences of damaged samples.
    /// </summary>
    public class Patch : AbstractPatch
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Patch"/> class.
        /// </summary>
        /// <param name="patchedSamples">Array of corrected samples.</param>
        /// <param name="startPosition">Relative position of beginning of a sequence of
        /// samples in the input audio data.</param>
        /// <param name="errorLevelAtDetection">Prediction error to average
        /// error ratio.</param>
        public Patch(
            double[] patchedSamples,
            int startPosition,
            double errorLevelAtDetection)
            : base(patchedSamples, startPosition, errorLevelAtDetection)
        {
        }

        /// <summary>
        /// Expands patch on beginning.
        /// </summary>
        public void ExpandLeft() =>
            this.OnChange(new PatchEventArgs(this.StartPosition - 1, this.Length + 1));

        /// <summary>
        /// Shortens patch on beginning.
        /// </summary>
        public void ShrinkLeft() =>
            this.OnChange(new PatchEventArgs(this.StartPosition + 1, this.Length - 1));

        /// <summary>
        /// Shortens patch on end.
        /// </summary>
        public void ShrinkRight() =>
            this.OnChange(new PatchEventArgs(this.StartPosition, this.Length - 1));

        /// <summary>
        /// Expands patch on end.
        /// </summary>
        public void ExpandRight() =>
            this.OnChange(new PatchEventArgs(this.StartPosition, this.Length + 1));
    }
}