// <copyright file="Patch.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace CarefulAudioRepair.Data
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
        public void ExpandLeft()
        {
            this.StartPosition--;
            this.SetInternalArray(new double[this.Length + 1]);
            this.OnChange();
        }

        /// <summary>
        /// Shortens patch on beginning.
        /// </summary>
        public void ShrinkLeft()
        {
            this.StartPosition++;
            this.SetInternalArray(new double[this.Length - 1]);
            this.OnChange();
        }

        /// <summary>
        /// Shortens patch on end.
        /// </summary>
        public void ShrinkRight()
        {
            this.SetInternalArray(new double[this.Length - 1]);
            this.OnChange();
        }

        /// <summary>
        /// Expands patch on end.
        /// </summary>
        public void ExpandRight()
        {
            this.SetInternalArray(new double[this.Length + 1]);
            this.OnChange();
        }
    }
}