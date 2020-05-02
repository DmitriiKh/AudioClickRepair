// <copyright file="Patcher.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace AudioClickRepair.Processing
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Replaces samples from immutable array that were patched using update function.
    /// </summary>
    public class Patcher : IPatcher
    {
        private readonly ImmutableArray<double> immutableArray;
        private readonly BlockingCollection<AbstractPatch> patchCollection;
        private readonly Func<AbstractPatch, int, double> updateFunc;

        /// <summary>
        /// Initializes a new instance of the <see cref="Patcher"/> class.
        /// </summary>
        /// <param name="immutableArray">Immutable array of samples.</param>
        /// <param name="patchCollection">Collection of patches.</param>
        /// <param name="updateFunc">Function to call to get the patched value.
        /// Example: (patch, position) => patch.GetOutputSample(position).</param>
        public Patcher(
            ImmutableArray<double> immutableArray,
            BlockingCollection<AbstractPatch> patchCollection,
            Func<AbstractPatch, int, double> updateFunc)
        {
            this.immutableArray = immutableArray;
            this.patchCollection = patchCollection;
            this.updateFunc = updateFunc;
        }

        /// <summary>
        /// Gets array containing sequence of patched samples of specified length
        /// starting from the specified position.
        /// </summary>
        /// <param name="start">Start position of range.</param>
        /// <param name="length">Length of range.</param>
        /// <returns>Array of patched samples.</returns>
        public double[] GetRange(int start, int length)
        {
            var range = new ArrayFragment(
                this.immutableArray,
                start,
                length);

            var patches = this.GetPatchesForRange(range);

            foreach (var patch in patches)
            {
                this.UpdateRange(range, patch);
            }

            return range.GetInternalArray();
        }

        public double GetValue(int position)
        {
            var patchForPosition = this.PatchForPosition(position);

            return patchForPosition is null
                ? this.immutableArray[position]
                : this.updateFunc(patchForPosition, position);
        }

        private void UpdateRange(AbstractFragment range, AbstractPatch patch)
        {
            var start = Math.Max(patch.StartPosition, range.StartPosition);
            var end = Math.Min(patch.EndPosition, range.EndPosition);

            for (var position = start; position <= end; position++)
            {
                range.SetValue(position, this.updateFunc(patch, position));
            }
        }

        private AbstractPatch[] GetPatchesForRange(AbstractFragment range)
        {
            var patchesForRange = this.patchCollection.Where(
                p => p?.StartPosition <= range.EndPosition &&
                p?.EndPosition >= range.StartPosition);

            return patchesForRange.ToArray();
        }

        private AbstractPatch PatchForPosition(int position) =>
            this.patchCollection.FirstOrDefault(
                p => p?.StartPosition <= position &&
                p?.EndPosition >= position);
    }
}
