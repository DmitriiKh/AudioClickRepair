namespace AudioClickRepair.Processing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AudioClickRepair.Data;

    class PatchMaker : IPatchMaker
    {
        private const int MaxLeftShift = 10;
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
                // and not converging and also
                // there are no suspicious samples after the last fixed sample
                if (patches.Count > 10
                    && !this.IsConverging(patches)
                    && newPatch.ErrorLevelAfterEnd < 1.5)
                {
                    break;
                }
            }

            return patches;
        }

        private AbstractPatch BestOf(List<AbstractPatch> patches)
        {
            const double ErrorAtStartCoefficientWeight = 0.33;
            const double RegenerationErrorCoefficientWeight = 0.33;
            const double ErrorAtEndCoefficientWeight = 0.33;

            var max1 = patches.Select(p => p.ErrorLevelAtStart).Max();
            var min1 = patches.Select(p => p.ErrorLevelAtStart).Min();
            var max2 = patches.Select(p => p.ConnectionError).Max();
            var min2 = patches.Select(p => p.ConnectionError).Min();
            var max3 = patches.Select(p => p.ErrorLevelAfterEnd).Max();
            var min3 = patches.Select(p => p.ErrorLevelAfterEnd).Min();

            AbstractPatch bestPatch = null;
            var bestError = 0.0;

            foreach (var patch in patches)
            {
                var currentErrorLevelCoef = ErrorAtStartCoefficientWeight * (patch.ErrorLevelAtStart - min1) / (max1 - min1);
                var regenerationErrorCoef = RegenerationErrorCoefficientWeight * (patch.ConnectionError - min2) / (max2 - min2);
                var currentErrorLevelAtEndCoef = ErrorAtEndCoefficientWeight * (patch.ErrorLevelAfterEnd - min3) / (max3 - min3);

                var patchError = currentErrorLevelCoef + regenerationErrorCoef + currentErrorLevelAtEndCoef;

                if (bestPatch is null || patchError < bestError)
                {
                    bestPatch = patch;
                    bestError = patchError;
                }
            }

            return bestPatch;
        }

        private bool IsConverging(List<AbstractPatch> patches)
        {
            var diff1 = patches.Select(p => p.ConnectionError);
            var lastAv1 = diff1.AsEnumerable().Reverse().Take(5).Average();
            var preLastAv1 = diff1.AsEnumerable().Reverse().Skip(5).Take(5).Average();

            var diff2 = patches.Select(p => p.ErrorLevelAfterEnd);
            var lastAv2 = diff2.AsEnumerable().Reverse().Take(5).Average();
            var preLastAv2 = diff2.AsEnumerable().Reverse().Skip(5).Take(5).Average();

            return lastAv1 < preLastAv1 || lastAv2 < preLastAv2;
        }
    }
}
