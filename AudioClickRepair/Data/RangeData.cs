using System.Collections.Immutable;

namespace AudioClickRepair.Data
{
    public class RangeData
    {
        private readonly double[] _internalArray;

        public int StartPosition { get; private set; }
        public int EndPosition { get; private set; }

        public RangeData(double[] internalArray, int startPosition)
        {
            _internalArray = internalArray;
            StartPosition = startPosition;
            EndPosition = startPosition + internalArray.Length - 1;
        }

        public static RangeData GetRangeFromImmutable(
            ImmutableArray<double> immutableArray,
            int rangeStartPosition,
            int rangeLength)
        {
            var shortArray = new double[rangeLength];
            immutableArray.CopyTo(rangeStartPosition, shortArray, 0, rangeLength);

            return new RangeData(shortArray, rangeStartPosition);
        }

        public void SetValue(int position, double value) =>
            _internalArray[position - StartPosition] = value;

        public double GetValue(int position) =>
            _internalArray[position - StartPosition];

        public double[] GetInternalArray() => _internalArray;
    }
}
