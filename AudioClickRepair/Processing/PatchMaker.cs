namespace AudioClickRepair.Processing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AudioClickRepair.Data;

    class PatchMaker : IPatchMaker
    {
        private const int MaxLeftShift = 10;
        private const int NumberOfSamplesForAveraging = 5;
        private const int MinLengthForConvergeCheck =
            2 * NumberOfSamplesForAveraging;

        private const double MaxErrorLevelAfterEnd = 1.5;
        private readonly IRegenerator regenerarator;

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
            var patches = new List<AbstractPatch>();

            for (var leftShift = 0; leftShift <= MaxLeftShift; leftShift++)
            {
                var patchesNew = this.PlayWithLength(
                    position - leftShift,
                    1 + leftShift,
                    maxLengthOfCorrection,
                    errorLevelAtDetection);

                patches.AddRange(patchesNew);
            }

            return this.BestOf(patches);
        }

        private List<AbstractPatch> PlayWithLength(
            int start,
            int minLengthOfCorrection,
            int maxLengthOfCorrection,
            double errorLevelAtDetection)
        {
            var patches = new List<AbstractPatch>();

            for (var length = minLengthOfCorrection;
                length <= maxLengthOfCorrection;
                length++)
            {
                var newPatch = new Patch(
                    new double[length],
                    start,
                    errorLevelAtDetection);

                this.regenerarator.RestorePatch(newPatch);

                patches.Add(newPatch);

                // Break the loop if it takes too long
                // and errors are not dropping and also
                // there are no suspicious samples after the last fixed sample
                if (patches.Count > MinLengthForConvergeCheck
                    && !this.ErrorsAreDropping(patches)
                    && newPatch.ErrorLevelAfterEnd < MaxErrorLevelAfterEnd)
                {
                    break;
                }
            }

            return patches;
        }

        private AbstractPatch BestOf(List<AbstractPatch> patches)
        {
            const double ErrorAtStartWeight = 0.33;
            const double ConnectionErrorWeight = 0.33;
            const double ErrorAtEndWeight = 0.33;

            var maxErrorAtStart = patches.Select(p => p.ErrorLevelAtStart).Max();
            var minErrorAtStart = patches.Select(p => p.ErrorLevelAtStart).Min();
            var maxConnectionError = patches.Select(p => p.ConnectionError).Max();
            var minConnectionError = patches.Select(p => p.ConnectionError).Min();
            var maxErrorAtEnd = patches.Select(p => p.ErrorLevelAfterEnd).Max();
            var minErrorAtEnd = patches.Select(p => p.ErrorLevelAfterEnd).Min();

            AbstractPatch bestPatch = null;
            var bestPatchError = 0.0;

            foreach (var patch in patches)
            {
                var errorAtStartCoef = ErrorAtStartWeight *
                    (patch.ErrorLevelAtStart - minErrorAtStart) /
                    (maxErrorAtStart - minErrorAtStart);

                var connectionErrorCoef = ConnectionErrorWeight *
                    (patch.ConnectionError - minConnectionError) /
                    (maxConnectionError - minConnectionError);

                var errorAtEndCoef = ErrorAtEndWeight *
                    (patch.ErrorLevelAfterEnd - minErrorAtEnd) /
                    (maxErrorAtEnd - minErrorAtEnd);

                var patchError = errorAtStartCoef + connectionErrorCoef + errorAtEndCoef;

                if (bestPatch is null || patchError < bestPatchError)
                {
                    bestPatch = patch;
                    bestPatchError = patchError;
                }
            }

            return bestPatch;
        }

        private bool ErrorsAreDropping(List<AbstractPatch> patches)
        {
            var length = NumberOfSamplesForAveraging;

            var connectionErrors = patches.Select(p => p.ConnectionError).ToArray();
            var errorsAfterEnd = patches.Select(p => p.ErrorLevelAfterEnd).ToArray();

            var connectionErrorsDrop = connectionErrors.Reverse().Take(length).Average()
                < connectionErrors.Reverse().Skip(length).Take(length).Average();

            var errorsAfterEndDrop = errorsAfterEnd.Reverse().Take(length).Average()
                < errorsAfterEnd.Reverse().Skip(length).Take(length).Average();

            return connectionErrorsDrop || errorsAfterEndDrop;
        }
    }
}
