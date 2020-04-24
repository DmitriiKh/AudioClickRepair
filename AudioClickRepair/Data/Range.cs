namespace AudioClickRepair.Data
{
    public class Range
    {
        private readonly double[] _internalArray;

        public int StartPosition { get; private set; }
        public int EndPosition { get; private set; }

        public Range(double[] internalArray, int startPosition)
        {
            _internalArray = internalArray;
            StartPosition = startPosition;
            EndPosition = startPosition + internalArray.Length - 1;
        }

        public void SetValue(int position, double value) =>
            _internalArray[position - StartPosition] = value;

        public double GetValue(int position) =>
            _internalArray[position - StartPosition];

        public double[] GetInternalArray() => _internalArray;
    }
}
