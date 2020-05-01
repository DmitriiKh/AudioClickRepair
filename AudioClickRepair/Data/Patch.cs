using System;

namespace AudioClickRepair.Data
{
    /// <summary>
    /// Contains information on sequences of damaged samples.
    /// </summary>
    public sealed class Patch : AbstractPatch
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
        public Patch(
            int position,
            int length,
            double errorLevelDetected,
            AudioData audioData)
            : base(position, errorLevelDetected)
        {
            _audioDataOwningThisPatch = audioData;

            UpdateOutput();
        }

        public double GetErrorLevelAtStartPosition() => 0;
        //HelperCalculator.CalculateDetectionLevel(
        //    _audioDataOwningThisClick,
        //    FromChannel,
        //    StartPosition);

        public Patch DeepCopy() =>
            new Patch(
                StartPosition,
                Length,
                ErrorLevelAtDetection,
                _audioDataOwningThisPatch);

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

            //for (var index = StartPosition; index < EndPosition; index++)
            //    internalArray[index - StartPosition] =
            //        _audioDataOwningThisPatch.GetInputSample(FromChannel, index);

            //ClickRepairer.Repair(this, _audioDataOwningThisClick);
        }
    }
}