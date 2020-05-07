namespace AudioClickRepair.Processing
{
    using AudioClickRepair.Data;

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

                if (bestPatch is null || patch.RegenerationError < bestPatch?.RegenerationError)
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

            for (var length = minLengthOfCorrection;
                length <= maxLengthOfCorrection;
                length++)
            {
                var arrayFragment = new ArrayFragment(new double[length], start);
                var connectionError = this.regenerarator.RestoreFragment(arrayFragment);

                if (bestPatch is null || connectionError < bestPatch.RegenerationError)
                {
                    bestPatch = new Patch(
                        arrayFragment.GetInternalArray(),
                        start,
                        errorLevelAtDetection);

                    bestPatch.RegenerationError = connectionError;
                }
            }

            return bestPatch;
        }
    }
}
