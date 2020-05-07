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

        public AbstractPatch NewPatch(
            int position,
            int maxLengthOfCorrection,
            double errorLevelAtDetection)
        {
            AbstractPatch bestPatch = null;

            for (var leftShift = 0; leftShift <= MaxLeftShift; leftShift++)
            {
                var patch = this.FindOptimal(position - leftShift, maxLengthOfCorrection, errorLevelAtDetection);

                if (patch.ConnectionError < bestPatch?.ConnectionError)
                {
                    bestPatch = patch;
                }
            }

            return bestPatch;
        }

        private AbstractPatch FindOptimal(
            int start,
            int maxLengthOfCorrection,
            double errorLevelAtDetection)
        {
            AbstractPatch bestPatch = null;

            for (var length = 1; length <= maxLengthOfCorrection; length++)
            {
                var arrayFragment = new ArrayFragment(new double[length], start);
                var connectionError = this.regenerarator.RestoreFragment(arrayFragment);

                if (connectionError < bestPatch?.ConnectionError)
                {
                    bestPatch = new Patch(
                        arrayFragment.GetInternalArray(),
                        start,
                        errorLevelAtDetection);
                }
            }

            return bestPatch;
        }
    }
}
