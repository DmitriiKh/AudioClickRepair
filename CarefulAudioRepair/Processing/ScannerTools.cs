// <copyright file="ScannerTools.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace CarefulAudioRepair.Processing
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Threading.Tasks;
    using CarefulAudioRepair.Data;

    /// <summary>
    /// Contains a set of objects to support the Scanner class.
    /// </summary>
    internal class ScannerTools : IDisposable
    {
        private ImmutableArray<float> predictionErr;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScannerTools"/> class.
        /// </summary>
        /// <param name="inputSamples">Input audio samples.</param>
        /// <param name="settings">Settings for processing audio.</param>
        /// <param name="patches"></param>
        public ScannerTools(ImmutableArray<float> inputSamples, IAudioProcessingSettings settings,
            List<AbstractPatch> patches = null)
        {
            this.PatchCollection = new PatchCollection(patches);

            this.Input = inputSamples;

            this.Settings = settings;

            this.InputPatcher = new Patcher(
                this.Input,
                this.PatchCollection,
                (patch, position) => patch.GetValue(position));

            this.Predictor = new FastBurgPredictor(
                settings.CoefficientsNumber,
                settings.HistoryLengthSamples);

            this.NormCalculator = new AveragedMaxErrorAnalyzer();
        }

        /// <summary>
        /// Gets a value indicating whether prediction errors were calculated.
        /// </summary>
        public bool IsPreprocessed => this.PredictionErrPatcher != null;

        /// <summary>
        /// Gets IDetector for search of damaged samples.
        /// </summary>
        public IDetector DamageDetector { get; private set; }

        /// <summary>
        /// Gets IPredictor for finding predictions.
        /// </summary>
        public IPredictor Predictor { get; }

        /// <summary>
        /// Gets IAnalyzer for finding normal errors levels.
        /// </summary>
        public IAnalyzer NormCalculator { get; }

        /// <summary>
        /// Gets collection of patches.
        /// </summary>
        public PatchCollection PatchCollection { get; }

        /// <summary>
        /// Gets input samples.
        /// </summary>
        public ImmutableArray<float> Input { get; }

        /// <summary>
        /// Gets settings for processing audio.
        /// </summary>
        public IAudioProcessingSettings Settings { get; }

        /// <summary>
        /// Gets patcher for prediction errors.
        /// </summary>
        internal IPatcher PredictionErrPatcher { get; private set; }

        /// <summary>
        /// Gets a patch maker.
        /// </summary>
        internal IPatchMaker PatchMaker { get; private set; }

        /// <summary>
        /// Gets a regenerator for restoring damaged audio samples.
        /// </summary>
        internal IRegenerator Regenerarator { get; private set; }

        /// <summary>
        /// Gets patcher for input samples.
        /// </summary>
        internal IPatcher InputPatcher { get; }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.PatchCollection.Dispose();
        }

        /// <summary>
        /// Calculates prediction errors for input and gets ready for the detection phase.
        /// </summary>
        /// <param name="parentStatus"></param>
        /// <param name="status">Parameter to report status through.</param>
        /// <param name="progress">Parameter to report progress through.</param>
        public void GetReady(
            string parentStatus,
            IProgress<string> status,
            IProgress<double> progress)
        {
            status.Report(parentStatus + "Preparation");
            progress.Report(0);

            var errors = Array.ConvertAll(
                this.CalculatePredictionErrors(progress),
                (e) => (float)e);

            // Initialize fields that depend on prediction errors
            this.predictionErr = ImmutableArray.Create(errors);
            this.PredictionErrPatcher = new Patcher(
                this.predictionErr,
                this.PatchCollection,
                (_, __) => AbstractPatch.MinimalPredictionError);

            this.DamageDetector = new DamagedSampleDetector(
                this.PredictionErrPatcher,
                this.InputPatcher,
                this.NormCalculator,
                this.Predictor);

            this.Regenerarator = new Regenerator(
                this.InputPatcher,
                this.Predictor,
                this.DamageDetector);

            this.PatchMaker = new PatchMaker(this.Regenerarator);

            progress.Report(100);

            // The fields are initialized
        }

        private double[] CalculatePredictionErrors(
            IProgress<double> progress)
        {
            var errors = new double[this.Input.Length];

            var inputDataSize = this.Predictor.InputDataSize;

            var start = inputDataSize;
            var end = this.Input.Length;

            if (start >= end)
            {
                return errors;
            }

            var chunkSize = Math.Max(
                inputDataSize,
                (end - start) / Environment.ProcessorCount);

            var part = Partitioner.Create(start, end, chunkSize);

            Parallel.ForEach(part, (range, state, index) =>
            {
                for (var position = range.Item1; position < range.Item2; position++)
                {
                    var inputDataStart = position - inputDataSize;

                    errors[position] = this.Input[position]
                        - this.Predictor.GetForward(
                            this.InputPatcher.GetRange(inputDataStart, inputDataSize));

                    // Only the first thread reports
                    // Throttling by 1000 samples
                    if (index == 0 && position % 1000 == 0)
                    {
                        progress.Report(
                            100.0 * (position - range.Item1)
                            / (range.Item2 - range.Item1));
                    }
                }
            });

            return errors;
        }
    }
}
