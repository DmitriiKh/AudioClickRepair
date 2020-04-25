using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;

namespace AudioClickRepair.Data
{
    public class Patcher
    {
        private readonly ImmutableArray<double> _immutableArray;
        private readonly BlockingCollection<IPatch> _patchCollection;
        private readonly Func<IPatch, int, double> _updateFunc;

        public Patcher(ImmutableArray<double> immutableArray, 
            BlockingCollection<IPatch> patchCollection, 
            Func<IPatch, int, double> updateFunc)
        {
            _immutableArray = immutableArray;
            _patchCollection = patchCollection;
            _updateFunc = updateFunc;
        }

        public RangeData GetRangeBefore(
            int positionExcluding,
            int length)
        {
            var range = RangeData.GetRangeFromImmutable(
                _immutableArray,
                positionExcluding - length,
                length);

            var patches = GetPatchesForRange(range);

            foreach (var patch in patches)
                UpdateRange(range, patch);

            return range;
        }

        private void UpdateRange(
            RangeData range,
            IPatch patch)
        {
            var start = Math.Max(patch.StartPosition, range.StartPosition);
            var end = Math.Min(patch.GetEndPosition(), range.EndPosition);

            for (var position = start; position <= end; position++)
                range.SetValue(position, _updateFunc(patch, position));
        }

        private IPatch[] GetPatchesForRange(RangeData range)
        {
            var patchesForRange = _patchCollection.Where(
                p => p?.StartPosition <= range.EndPosition &&
                p?.GetEndPosition() >= range.StartPosition);

            return patchesForRange.ToArray();
        }
    }
}
