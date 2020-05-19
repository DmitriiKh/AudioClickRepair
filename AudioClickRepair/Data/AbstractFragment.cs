// <copyright file="AbstractFragment.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace AudioClickRepair.Data
{
    /// <summary>
    /// This class represents a fragment of an array.
    /// The position indexes are the same as they would be in the original array.
    /// </summary>
    public abstract class AbstractFragment
    {
        private double[] internalArray;

        /// <summary>
        /// Gets or sets relative position of the first sample of fragment.
        /// </summary>
        public int StartPosition { get; protected set; }

        /// <summary>
        /// Gets relative position of the last sample of fragment.
        /// </summary>
        public int EndPosition => this.StartPosition + this.Length - 1;

        /// <summary>
        /// Gets length of fragment.
        /// </summary>
        public int Length => this.internalArray.Length;

        /// <summary>
        /// Sets value of internal array using relative position.
        /// </summary>
        /// <param name="position">Relative position.</param>
        /// <param name="value">Value.</param>
        public void SetValue(int position, double value) =>
            this.internalArray[position - this.StartPosition] = value;

        /// <summary>
        /// Gets value from internal array using relative position.
        /// </summary>
        /// <param name="position">Relative position.</param>
        /// <returns>Value.</returns>
        public double GetValue(int position) =>
            this.internalArray[position - this.StartPosition];

        /// <summary>
        /// Gets internal array.
        /// </summary>
        /// <returns>Array of samples.</returns>
        public double[] GetInternalArray() => this.internalArray;

        /// <summary>
        /// Sets internal array.
        /// </summary>
        /// <param name="array">New internal array.</param>
        public void SetInternalArray(double[] array) =>
            this.internalArray = array;
    }
}
