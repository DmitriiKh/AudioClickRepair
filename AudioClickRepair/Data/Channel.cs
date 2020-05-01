using AudioClickRepair.Processing;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;

namespace AudioClickRepair.Data
{
    internal class Channel
    {
        private readonly BlockingCollection<AbstractPatch> _patchCollection;
        private readonly ImmutableArray<double> _input;
        private readonly ImmutableArray<double> _predictionErr;
        private readonly IPatcher _inputPatcher;
        private readonly IPatcher _predictionErrPatcher;

        internal Channel(double[] inputSamples)
        {
            if (inputSamples is null)
                throw new ArgumentNullException(nameof(inputSamples));

            _patchCollection = new BlockingCollection<AbstractPatch>();

            _input = ImmutableArray.Create(inputSamples);
            _inputPatcher = new Patcher(
                _input,
                _patchCollection,
                (patch, position) => patch.GetValue(position));

            _predictionErr = ImmutableArray.Create(new double[inputSamples.Length]);
            _predictionErrPatcher = new Patcher(
                _predictionErr,
                _patchCollection,
                (_, __) => AbstractPatch.MinimalPredictionError);

            IsReadyForScan = false;
        }

        internal void GetReadyForScan()
        {

            IsReadyForScan = true;
        }

        internal bool IsReadyForScan { get; private set; }

        internal int LengthSamples() => _input.Length;

        internal double GetInputSample(int position) => _input[position];

        internal double GetOutputSample(int position)
        {
            var patch = GetPatchOrNullAt(position);

            return patch is null
                ? _input[position]
                : patch.GetValue(position);
        }

        internal double GetPredictionErr(int position)
        {
            var patch = GetPatchOrNullAt(position);

            return patch is null
                ? _predictionErr[position]
                // TODO return zero in this case
                : AbstractPatch.MinimalPredictionError;
        }

        private AbstractPatch GetPatchOrNullAt(int position) =>
            _patchCollection.FirstOrDefault(
                c => c.StartPosition <= position
                    && c.EndPosition >= position);

        internal int GetNumberOfPatches() => _patchCollection.Count;

        private void RemoveAllPatches()
        {
            while (_patchCollection.TryTake(out _)) { };
        }

        internal AbstractPatch[] GetAllPatches()
        {
            var patchList = _patchCollection.ToList();
            patchList.Sort();
            return patchList.ToArray();
        }

        private void RegisterPatch(AbstractPatch patch)
        {
            _patchCollection.Add(patch);
            patch.Updater += this.PatchUpdater;
        }

        private void PatchUpdater(object sender, PatchEventArgs e)
        {
            // remove sender from the patch collection
            // update array
            // update current error level
            // add the sender back to the patch collection
            throw new NotImplementedException();
        }

        private double GetPredictionErrNorm(int position, IAnalyzer normCalculator)
        {
            var startIndex = position - normCalculator.GetInputDataSize();

            // If there is not enough data to analyze
            if (startIndex < 0)
                return normCalculator.GetDefaultResult();

            return normCalculator.GetResult(
                _predictionErrPatcher.GetRange(
                    position - normCalculator.GetInputDataSize(),
                    normCalculator.GetInputDataSize()));
        }
    }
}