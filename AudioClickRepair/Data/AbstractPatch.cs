namespace AudioClickRepair.Data
{
    public abstract class AbstractPatch : AbstractFragment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractPatch"/> class.
        /// </summary>
        /// <param name="startPosition"> Position of beginning of a sequence of
        /// damaged samples in the input audio data.</param>
        /// <param name="errorLevelDetected"> Prediction error to average
        /// error ratio.</param>
        public AbstractPatch(
            int startPosition,
            double errorLevelDetected)
        {
            this.StartPosition = startPosition;
            this.ErrorLevelAtDetection = errorLevelDetected;
            this.Aproved = true;
        }

        public const double MinimalPredictionError = 0;

        public double ErrorLevelAtDetection { get; }

        public bool Aproved { get; set; }

        public static bool operator ==(AbstractPatch left, AbstractPatch right) =>
            left?.StartPosition == right?.StartPosition &&
                   left?.Length == right?.Length;

        public static bool operator !=(AbstractPatch left, AbstractPatch right) =>
            left?.StartPosition != right?.StartPosition ||
                   left?.Length != right?.Length;

        public static bool operator <(AbstractPatch left, AbstractPatch right) =>
            left?.StartPosition < right?.StartPosition;

        public static bool operator <=(AbstractPatch left, AbstractPatch right) =>
            left?.StartPosition <= right?.StartPosition;

        public static bool operator >=(AbstractPatch left, AbstractPatch right) =>
            left?.StartPosition >= right?.StartPosition;

        public static bool operator >(AbstractPatch left, AbstractPatch right) =>
            left?.StartPosition > right?.StartPosition;

        /// <summary>
        /// Compares by position.
        /// </summary>
        /// <param name="otherPatch">Other instance.</param>
        /// <returns>Result of CompareTo of StartPosition.</returns>
        public int CompareTo(Patch otherPatch) =>
            this.StartPosition.CompareTo(otherPatch?.StartPosition);

        public override bool Equals(object obj) =>
            this.StartPosition == (obj as Patch)?.StartPosition;

        public override int GetHashCode() =>
            this.StartPosition.GetHashCode() ^
                   this.Length.GetHashCode();

    }
}
