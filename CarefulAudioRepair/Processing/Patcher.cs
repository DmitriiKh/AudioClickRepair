// <copyright file="Patcher.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace CarefulAudioRepair.Processing
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Immutable;
    using System.Linq;
    using CarefulAudioRepair.Data;

    /// <summary>
    /// Replaces samples from immutable array using update function
    /// if the elements were patched.
    /// </summary>
    internal class Patcher : IPatcher
    {
        private readonly ImmutableArray<double> immutableArray;
        private readonly PatchCollection patchCollection;
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
            PatchCollection patchCollection,
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
        /// <param name="anotherPatch">One more optional AbstractPatch
        /// that is not in the Collection yet.</param>
        /// <returns>Array of patched samples.</returns>
        public double[] GetRange(int start, int length, AbstractPatch anotherPatch = null)
        {
            var range = new ArrayFragment(
                this.immutableArray,
                start,
                length);

            var patches = this.patchCollection.GetPatchesForRange(range);

            foreach (var patch in patches)
            {
                this.UpdateRange(range, patch);
            }

            if (anotherPatch != null)
            {
                this.UpdateRange(range, anotherPatch);
            }

            return range.GetInternalArray();
        }

        /// <summary>
        /// Returns either value of sample from the immutable array or from a patch
        /// (if the patch covers the position).
        /// </summary>
        /// <param name="position">Position of required sample.</param>
        /// <returns>Value of sample.</returns>
        public double GetValue(int position)
        {
            var patchForPosition = this.patchCollection.GetPatchForPosition(position);

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

        //private AbstractPatch[] GetPatchesForRange(AbstractFragment range)
        //{
        //    var patchesForRange = this.patchCollection.Where(
        //        p => p?.StartPosition <= range.EndPosition &&
        //        p?.EndPosition >= range.StartPosition);

        //    return patchesForRange.ToArray();
        //}

        //private AbstractPatch PatchForPosition(int position) =>
        //    this.patchCollection.FirstOrDefault(
        //        p => p?.StartPosition <= position &&
        //        p?.EndPosition >= position);
    }
}
