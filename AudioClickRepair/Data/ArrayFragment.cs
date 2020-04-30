// <copyright file="ArrayFragment.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace AudioClickRepair.Data
{
    using System;
    using System.Collections.Immutable;

    /// <summary>
    /// Contains a sequence of samples from a larger array.
    /// </summary>
    public class ArrayFragment : AbstractFragment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayFragment"/> class.
        /// </summary>
        /// <param name="internalArray">Contains short sequence of samples.</param>
        /// <param name="startPosition">Relative position of the first sample.</param>
        public ArrayFragment(double[] internalArray, int startPosition)
        {
            if (internalArray is null)
            {
                throw new ArgumentNullException(nameof(internalArray));
            }

            this.internalArray = internalArray;
            this.StartPosition = startPosition;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayFragment"/> class
        /// and copies samples from immutable array.
        /// </summary>
        /// <param name="immutableArray">Source of samples.</param>
        /// <param name="rangeStart">Relative position of the first sample.</param>
        /// <param name="rangeLength">Length of sequence to be copied.</param>
        /// <returns>RangeData.</returns>
        public ArrayFragment(
            ImmutableArray<double> immutableArray,
            int rangeStart,
            int rangeLength)
            : this(
                  GetFragment(immutableArray, rangeStart, rangeLength),
                  rangeStart)
        {
        }

        private static double[] GetFragment(
            ImmutableArray<double> immutableArray,
            int rangeStart,
            int rangeLength)
        {
            var shortArray = new double[rangeLength];
            immutableArray.CopyTo(rangeStart, shortArray, 0, rangeLength);

            return shortArray;
        }
    }
}
