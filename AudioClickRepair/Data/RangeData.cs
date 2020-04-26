// <copyright file="RangeData.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace AudioClickRepair.Data
{
    using System;
    using System.Collections.Immutable;

    /// <summary>
    /// Contains a sequence of samples from a larger array.
    /// </summary>
    public class RangeData
    {
        private readonly double[] internalArray;

        /// <summary>
        /// Initializes a new instance of the <see cref="RangeData"/> class.
        /// </summary>
        /// <param name="internalArray">Contains short sequence of samples.</param>
        /// <param name="startPosition">Relative position of the first sample.</param>
        public RangeData(double[] internalArray, int startPosition)
        {
            if (internalArray is null)
            {
                throw new ArgumentNullException(nameof(internalArray));
            }

            this.internalArray = internalArray;
            this.StartPosition = startPosition;
            this.EndPosition = startPosition + internalArray.Length - 1;
        }

        /// <summary>
        /// Gets relative position of the first sample of range.
        /// </summary>
        public int StartPosition { get; private set; }

        /// <summary>
        /// Gets relative position of the last sample of range.
        /// </summary>
        public int EndPosition { get; private set; }

        /// <summary>
        /// Creates a new instance of the <see cref="RangeData"/> class
        /// by copying samples from immutable array.
        /// </summary>
        /// <param name="immutableArray">Source of samples.</param>
        /// <param name="rangeStartPosition">Relative position of the first sample.</param>
        /// <param name="rangeLength">Length of sequence to be copied.</param>
        /// <returns>RangeData.</returns>
        public static RangeData GetRangeFromImmutable(
            ImmutableArray<double> immutableArray,
            int rangeStartPosition,
            int rangeLength)
        {
            var shortArray = new double[rangeLength];
            immutableArray.CopyTo(rangeStartPosition, shortArray, 0, rangeLength);

            return new RangeData(shortArray, rangeStartPosition);
        }

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
        /// Gets internal array of samples.
        /// </summary>
        /// <returns>Array of samples.</returns>
        public double[] GetInternalArray() => this.internalArray;
    }
}
