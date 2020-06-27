// <copyright file="PatchCollection.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace CarefulAudioRepair.Data
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    internal class PatchCollection : IDisposable
    {
        private readonly BlockingCollection<AbstractPatch> patchCollection =
            new BlockingCollection<AbstractPatch>();

        private ImmutableSortedDictionary<int, AbstractPatch> patchesSorted;

        private int[] startPositions;

        public bool Finalized { get; private set; } = false;

        public PatchCollection()
        {
        }

        public void Add(AbstractPatch patch)
        {
            this.patchCollection.Add(patch);

            if (this.Finalized)
            {
                this.Finalize();
            }
        }

        public void Finalize()
        {
            this.patchesSorted = this.patchCollection
                .ToImmutableSortedDictionary(p => p.StartPosition, p => p);

            this.startPositions = this.patchesSorted.Keys.ToArray();

            this.Finalized = true;
        }

        public AbstractPatch GetPatchForPosition(int position)
        {
            if (this.Finalized)
            {
                var index = this.GetIndexOnOrBefore(position);

                if (index == -1)
                {
                    return null;
                }

                var possiblePatch = this.patchesSorted[this.startPositions[index]];

                if (position <= possiblePatch.EndPosition)
                {
                    return possiblePatch;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                var patch = this.patchCollection.FirstOrDefault(
                p => p?.StartPosition <= position &&
                p?.EndPosition >= position);

                return patch;
            }
        }

        public AbstractPatch[] GetPatchesForRange(AbstractFragment range)
        {
            if (this.Finalized)
            {
                var index = this.GetIndexOnOrBefore(range.EndPosition);

                var list = new List<AbstractPatch>();

                while (index >= 0)
                {
                    var possiblePatch = this.patchesSorted[this.startPositions[index]];

                    if (range.StartPosition <= possiblePatch.EndPosition)
                    {
                        list.Add(possiblePatch);
                    }
                    else
                    {
                        break;
                    }

                    index--;
                }

                return list.ToArray();
            }
            else
            {
                var patchesForRange = this.patchCollection.Where(
                p => p?.StartPosition <= range.EndPosition &&
                p?.EndPosition >= range.StartPosition);

                return patchesForRange.ToArray();
            }
        }

        public void Dispose()
        {
            this.patchCollection.Dispose();
        }

        private int GetIndexOnOrBefore(int position)
        {
            var index = Array.BinarySearch(this.startPositions, position);

            if (index < 0)
            {
                index = ~index - 1;
            }

            return index;
        }
    }
}
