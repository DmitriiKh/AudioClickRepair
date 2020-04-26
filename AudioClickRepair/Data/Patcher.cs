// <copyright file="Patcher.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace AudioClickRepair.Data
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Replaces samples from immutable array that were patched using update function.
    /// </summary>
    public class Patcher
    {
        private readonly ImmutableArray<double> immutableArray;
        private readonly BlockingCollection<IPatch> patchCollection;
        private readonly Func<IPatch, int, double> updateFunc;

        /// <summary>
        /// Initializes a new instance of the <see cref="Patcher"/> class.
        /// </summary>
        /// <param name="immutableArray">Immutable array of samples.</param>
        /// <param name="patchCollection">Collection of patches.</param>
        /// <param name="updateFunc">Function to call to get the patched value.
        /// Example: (patch, position) => patch.GetOutputSample(position).</param>
        public Patcher(
            ImmutableArray<double> immutableArray,
            BlockingCollection<IPatch> patchCollection,
            Func<IPatch, int, double> updateFunc)
        {
            this.immutableArray = immutableArray;
            this.patchCollection = patchCollection;
            this.updateFunc = updateFunc;
        }

        /// <summary>
        /// Gets range containing sequence of patched samples of specified length
        /// before the specified position.
        /// </summary>
        /// <param name="positionExcluding">End position of the returned range plus one.</param>
        /// <param name="length">Length of range.</param>
        /// <returns>RangeData.</returns>
        public RangeData GetRangeBefore(
            int positionExcluding,
            int length)
        {
            var range = RangeData.GetRangeFromImmutable(
                this.immutableArray,
                positionExcluding - length,
                length);

            var patches = this.GetPatchesForRange(range);

            foreach (var patch in patches)
            {
                this.UpdateRange(range, patch);
            }

            return range;
        }

        private void UpdateRange(
            RangeData range,
            IPatch patch)
        {
            var start = Math.Max(patch.StartPosition, range.StartPosition);
            var end = Math.Min(patch.EndPosition, range.EndPosition);

            for (var position = start; position <= end; position++)
            {
                range.SetValue(position, this.updateFunc(patch, position));
            }
        }

        private IPatch[] GetPatchesForRange(RangeData range)
        {
            var patchesForRange = this.patchCollection.Where(
                p => p?.StartPosition <= range.EndPosition &&
                p?.EndPosition >= range.StartPosition);

            return patchesForRange.ToArray();
        }
    }
}
