namespace AudioClickRepair.Processing
{
    using System;
    using AudioClickRepair.Data;

    class DamagedSampleDetector : IDetector
    {
        private readonly IAnalyzer normCalculator;
        private readonly IPatcher predictionErrPatcher;
        private readonly IPatcher inputPatcher;
        private readonly IPredictor predictor;

        public DamagedSampleDetector(
            IPatcher predictionErrPatcher,
            IPatcher inputPatcher,
            IAnalyzer normCalculator,
            IPredictor predictor)
        {
            this.predictionErrPatcher = predictionErrPatcher;
            this.inputPatcher = inputPatcher;
            this.normCalculator = normCalculator;
            this.predictor = predictor;
        }

        public int InputDataSize =>
            this.predictor.InputDataSize
            + this.normCalculator.InputDataSize;

        public double GetErrorLevel(int position, AbstractPatch anotherPatch)
        {
            var errors = this.predictionErrPatcher.GetRange(
                    position - this.normCalculator.InputDataSize,
                    this.normCalculator.InputDataSize,
                    anotherPatch);

            var normalError = this.normCalculator.GetResult(errors);

            var inputSamples = this.inputPatcher.GetRange(
                position - this.predictor.InputDataSize,
                this.predictor.InputDataSize,
                anotherPatch);

            var errorAtPosition = Math.Abs(this.predictor.GetForward(inputSamples)
                - this.inputPatcher.GetValue(position));

            return errorAtPosition / normalError;
        }
    }
}
