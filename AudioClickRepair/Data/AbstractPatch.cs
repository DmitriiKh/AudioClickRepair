// <copyright file="AbstractPatch.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace AudioClickRepair.Data
{
    using System;

    /// <summary>
    /// Basic behavior for Patch class.
    /// </summary>
    public abstract class AbstractPatch : AbstractFragment, IComparable<AbstractPatch>
    {
        /// <summary>
        /// Minimal allowed value for prediction errors.
        /// </summary>
        public const double MinimalPredictionError = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractPatch"/> class.
        /// </summary>
        /// <param name="patchedSamples">Array of corrected samples.</param>
        /// <param name="startPosition">Relative position of beginning of a sequence of
        /// samples in the input audio data.</param>
        /// <param name="errorLevelAtDetection">Prediction error to average
        /// error ratio.</param>
        public AbstractPatch(
            double[] patchedSamples,
            int startPosition,
            double errorLevelAtDetection)
        {
            this.internalArray = patchedSamples;
            this.StartPosition = startPosition;
            this.ErrorLevelAtDetection = errorLevelAtDetection;
            this.CurrentErrorLevel = errorLevelAtDetection;
            this.Approved = true;
        }

        /// <summary>
        /// Updater that called when patch needs updating.
        /// </summary>
        public event EventHandler<PatchEventArgs> Updater;

        /// <summary>
        /// Gets error level at the start position that was found at detection process.
        /// </summary>
        public double ErrorLevelAtDetection { get; }

        /// <summary>
        /// Gets current error level.
        /// </summary>
        public double CurrentErrorLevel { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether patch was approved by user.
        /// </summary>
        public bool Approved { get; set; }

        /// <summary>
        /// Compares start positions and lengths of operands.
        /// </summary>
        /// <param name="left">First operand.</param>
        /// <param name="right">Second operand.</param>
        /// <returns>Returns true if start positions and lengths of operands are equal.</returns>
        public static bool operator ==(AbstractPatch left, AbstractPatch right) =>
            left?.StartPosition == right?.StartPosition &&
                   left?.Length == right?.Length;

        /// <summary>
        /// Compares start positions and lengths of operands.
        /// </summary>
        /// <param name="left">First operand.</param>
        /// <param name="right">Second operand.</param>
        /// <returns>Returns true if start positions and lengths of operands are not equal.</returns>
        public static bool operator !=(AbstractPatch left, AbstractPatch right) =>
            left?.StartPosition != right?.StartPosition ||
                   left?.Length != right?.Length;

        /// <summary>
        /// Compares start positions of operands.
        /// </summary>
        /// <param name="left">First operand.</param>
        /// <param name="right">Second operand.</param>
        /// <returns>Returns true if start position of the first operand is less.</returns>
        public static bool operator <(AbstractPatch left, AbstractPatch right) =>
            left?.StartPosition < right?.StartPosition;

        /// <summary>
        /// Compares start positions of operands.
        /// </summary>
        /// <param name="left">First operand.</param>
        /// <param name="right">Second operand.</param>
        /// <returns>Returns true if start position of the first operand is less or equal.</returns>
        public static bool operator <=(AbstractPatch left, AbstractPatch right) =>
            left?.StartPosition <= right?.StartPosition;

        /// <summary>
        /// Compares start positions of operands.
        /// </summary>
        /// <param name="left">First operand.</param>
        /// <param name="right">Second operand.</param>
        /// <returns>Returns true if start position of the first operand is larger or equal.</returns>
        public static bool operator >=(AbstractPatch left, AbstractPatch right) =>
            left?.StartPosition >= right?.StartPosition;

        /// <summary>
        /// Compares start positions of operands.
        /// </summary>
        /// <param name="left">First operand.</param>
        /// <param name="right">Second operand.</param>
        /// <returns>Returns true if start position of the first operand is larger.</returns>
        public static bool operator >(AbstractPatch left, AbstractPatch right) =>
            left?.StartPosition > right?.StartPosition;

        /// <summary>
        /// Compares by position.
        /// </summary>
        /// <param name="otherPatch">Other instance.</param>
        /// <returns>Result of CompareTo of StartPosition.</returns>
        public int CompareTo(AbstractPatch otherPatch) =>
            this.StartPosition.CompareTo(otherPatch?.StartPosition);

        /// <summary>
        /// Checks if start positions are equal.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <returns>Returns true if start positions are equal.</returns>
        public override bool Equals(object obj) =>
            this.StartPosition == (obj as Patch)?.StartPosition;

        /// <summary>
        /// Gets hash code based on start position and length.
        /// </summary>
        /// <returns>Returns hash code.</returns>
        public override int GetHashCode() =>
            this.StartPosition.GetHashCode() ^
                   this.Length.GetHashCode();

        /// <summary>
        /// Invokes Updater method and then updates internal array,
        /// start position and current error level.
        /// </summary>
        /// <param name="args">Arguments (input and output).</param>
        protected virtual void OnChange(PatchEventArgs args)
        {
            this.Updater?.Invoke(this, args);
            this.internalArray = args.Patched.GetInternalArray();
            this.StartPosition = args.Patched.StartPosition;
            this.CurrentErrorLevel = args.NewErrorLevelAtStart;
        }

        /// <summary>
        /// Toggles approved state.
        /// </summary>
        public void ChangeApproved() => this.Approved = !this.Approved;
    }
}
