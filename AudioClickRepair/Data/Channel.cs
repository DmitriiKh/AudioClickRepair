using AudioClickRepair.Processing;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;

namespace AudioClickRepair.Data
{
    internal class Channel
    {
        private readonly BlockingCollection<IPatch> _patchCollection;
        private readonly ImmutableArray<double> _input;
        private readonly ImmutableArray<double> _predictionErr;
        private readonly Patcher _inputPatcher;
        private readonly Patcher _predictionErrPatcher;

        internal Channel(double[] inputSamples)
        {
            if (inputSamples is null)
                throw new ArgumentNullException(nameof(inputSamples));
            
            _patchCollection = new BlockingCollection<IPatch>();

            _input = ImmutableArray.Create(inputSamples);
            _inputPatcher = new Patcher(
                _input, 
                _patchCollection, 
                (patch, position) => patch.GetOutputSample(position));

            _predictionErr = ImmutableArray.Create(new double[inputSamples.Length]);
            _inputPatcher = new Patcher(
                _predictionErr, 
                _patchCollection, 
                (_, __) => IPatch.MinimalPredictionError);

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
                : patch.GetOutputSample(position);
        }

        internal double GetPredictionErr(int position)
        {
            var patch = GetPatchOrNullAt(position);

            return patch is null
                ? _predictionErr[position]
                // TODO return zero in this case
                : IPatch.MinimalPredictionError;
        }

        private IPatch GetPatchOrNullAt(int position) =>
            _patchCollection.FirstOrDefault(
                c => c.StartPosition <= position
                    && c.GetEndPosition() >= position);

        internal int GetNumberOfPatches() => _patchCollection.Count;

        private void RemoveAllPatches()
        {
            while (_patchCollection.TryTake(out _)) { };
        }

        internal IPatch[] GetAllPatches()
        {
            var patchList = _patchCollection.ToList();
            patchList.Sort();
            return patchList.ToArray();
        }

        private double GetPredictionErrNorm(int position, IAnalyzer normCalculator)
        {
            var startIndex = position - normCalculator.GetInputDataSize();

            // If there is not enough data to analyze
            if (startIndex < 0)
                return normCalculator.GetDefaultResult();

            return normCalculator.GetResult(
                _predictionErrPatcher.GetRangeBefore(
                    position,
                    normCalculator.GetInputDataSize()));
        }
    }
}