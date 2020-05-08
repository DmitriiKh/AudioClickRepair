namespace AudioClickRepair.Processing
{
    using AudioClickRepair.Data;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    class PatchMaker : IPatchMaker
    {
        private const int MaxLeftShift = 10;
        private IRegenerator regenerarator;

        public PatchMaker(IRegenerator regenerarator)
        {
            this.regenerarator = regenerarator;
        }

        public int InputDataSize =>
            this.regenerarator.InputDataSize + MaxLeftShift;

        public AbstractPatch NewPatch(
            int position,
            int maxLengthOfCorrection,
            double errorLevelAtDetection)
        {
            AbstractPatch bestPatch = null;

            for (var leftShift = 0; leftShift <= MaxLeftShift; leftShift++)
            {
                var patch = this.FindOptimal(
                    position - leftShift,
                    1 + leftShift,
                    maxLengthOfCorrection,
                    errorLevelAtDetection);

                if (bestPatch is null || patch.RegenerationError < bestPatch.RegenerationError)
                {
                    bestPatch = patch;
                }
            }

            return bestPatch;
        }

        private AbstractPatch FindOptimal(
            int start,
            int minLengthOfCorrection,
            int maxLengthOfCorrection,
            double errorLevelAtDetection)
        {
            AbstractPatch bestPatch = null;
            var errorsSequence = new List<double>();

            for (var length = minLengthOfCorrection;
                length <= maxLengthOfCorrection;
                length++)
            {
                var arrayFragment = new ArrayFragment(new double[length], start);
                var regenerationError = this.regenerarator.RestoreFragment(arrayFragment);

                if (bestPatch is null || regenerationError < bestPatch.RegenerationError)
                {
                    bestPatch = new Patch(
                        arrayFragment.GetInternalArray(),
                        start,
                        errorLevelAtDetection)
                    {
                        RegenerationError = regenerationError,
                    };
                }

                errorsSequence.Add(regenerationError);

                // Break the loop if too long and not converging
                if (errorsSequence.Count > 10 && !this.IsConverging(errorsSequence))
                {
                    break;
                }
            }

            return bestPatch;
        }

        private bool IsConverging(List<double> sequence)
        {
            var diff = new List<double>();
            for (var index = 1; index < sequence.Count - 1; index++)
            {
                diff.Add(Math.Abs(sequence.ElementAt(index) - sequence.ElementAt(index + 1)));
            }

            var lastAv = diff.AsEnumerable().Reverse().Take(5).Average();
            var preLastAv = diff.AsEnumerable().Reverse().Skip(5).Take(5).Average();

            return lastAv < preLastAv;
        }
    }
}
