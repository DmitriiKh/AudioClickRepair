using System;

namespace AudioClickRepair.Data

{
    /// <summary>
    /// Contains information on sequences of damaged samples.
    /// </summary>
    public sealed class Patch : AbstractPatch, IComparable<Patch>
    {
        private readonly AudioData _audioDataOwningThisPatch;

        internal bool BetterThan(Patch anotherClick)
        {
            if (anotherClick is null ||
                Length < anotherClick.Length ||
                GetErrorLevelAtStartPosition() < anotherClick.GetErrorLevelAtStartPosition())
                return true;
            return false;
        }

        /// <summary>
        /// Creates new object containing information on sequence of damaged
        /// samples such as position, length etc 
        /// </summary>
        /// <param name="position"> Position of beginning of a sequence of
        /// damaged samples in the input audio data </param>
        /// <param name="length"> Length of sequence of damaged samples </param>
        /// <param name="errorLevelDetected"> Prediction error to average
        /// error ratio </param>
        /// <param name="audioData"> Object of type of AudioData containing
        /// audio containing this sequence of damaged samples</param>
        /// <param name="fromChannel"> The channel (left, right) containing
        /// this sequence of damaged samples</param>
        public Patch(
            int position,
            int length,
            double errorLevelDetected,
            AudioData audioData,
            ChannelType fromChannel)
        {
            StartPosition = position;
            //Length = length;
            this.ErrorLevelAtDetection = errorLevelDetected;
            _audioDataOwningThisPatch = audioData;
            FromChannel = fromChannel;
            Aproved = true;

            UpdateOutput();
        }

        public double ErrorLevelAtDetection { get; }

        public double GetErrorLevelAtStartPosition() => 0;
        //HelperCalculator.CalculateDetectionLevel(
        //    _audioDataOwningThisClick,
        //    FromChannel,
        //    StartPosition);

        public bool Aproved { get; private set; }

        public ChannelType FromChannel { get; }

        /// <summary>
        /// Comparison by position 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Patch other)
        {
            return other is null ? 1 : StartPosition.CompareTo(other.StartPosition);
            // return the same result as for positions comparison
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Patch audioClick))
                return false;

            return StartPosition == audioClick.StartPosition;
        }

        public double GetPredictionErr(int position)
        {
            if (position < StartPosition)
                return _audioDataOwningThisPatch.GetPredictionErr(FromChannel, position);
            else if (position <= EndPosition)
                return MinimalPredictionError;
            else
                return 0;
            //return GetOutputSample(position) - ClickRepairer.CalcBurgPred(_audioDataOwningThisClick, this, position);
        }

        public override int GetHashCode()
        {
            return StartPosition.GetHashCode() ^
                   Length.GetHashCode() ^
                   FromChannel.GetHashCode();
        }

        public static bool operator ==(Patch left, Patch right)
        {
            if (left is null || right is null)
                return false;

            return left.StartPosition == right.StartPosition &&
                   left.Length == right.Length &&
                   left.FromChannel == right.FromChannel;
        }

        public Patch DeepCopy() =>
            new Patch(
                StartPosition,
                Length,
                ErrorLevelAtDetection,
                _audioDataOwningThisPatch,
                FromChannel);

        public static bool operator !=(Patch left, Patch right)
        {
            if (left is null || right is null)
                return true;

            return left.StartPosition != right.StartPosition ||
                   left.Length != right.Length ||
                   left.FromChannel != right.FromChannel;
        }

        public static bool operator <(Patch left, Patch right)
        {
            if (left is null || right is null)
                return false;

            return left.StartPosition < right.StartPosition;
        }

        public static bool operator <=(Patch left, Patch right)
        {
            if (left is null || right is null)
                return false;

            return left.StartPosition <= right.StartPosition;
        }

        public static bool operator >=(Patch left, Patch right)
        {
            if (left is null || right is null)
                return true;

            return left.StartPosition >= right.StartPosition;
        }

        public static bool operator >(Patch left, Patch right)
        {
            if (left is null || right is null)
                return true;

            return left.StartPosition > right.StartPosition;
        }

        public void ChangeAproved()
        {
            Aproved = !Aproved;
        }

        public void ExpandLeft()
        {
            StartPosition--;
            //Length++;
            UpdateOutput();
        }

        public void ShrinkLeft()
        {
            StartPosition++;
            //Length--;
            UpdateOutput();
        }

        public void ShrinkRight()
        {
            //Length--;
            UpdateOutput();
        }

        public void ExpandRight()
        {
            //Length++;
            UpdateOutput();
        }

        private void UpdateOutput()
        {
            internalArray = new double[Length];

            for (var index = StartPosition; index < EndPosition; index++)
                internalArray[index - StartPosition] =
                    _audioDataOwningThisPatch.GetInputSample(FromChannel, index);

            //ClickRepairer.Repair(this, _audioDataOwningThisClick);
        }
    }
}