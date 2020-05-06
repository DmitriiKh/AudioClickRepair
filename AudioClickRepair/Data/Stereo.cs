namespace AudioClickRepair.Data
{
    using System.Collections.Generic;

    /// <summary>
    ///     Represents stereo audio samples and includes information
    ///     about damaged samples
    /// </summary>
    public class Stereo : IAudio
    {
        private readonly Channel _leftChannel;
        private readonly Channel _rightChannel;

        public Stereo(double[] leftChannelSamples, double[] rightChannelSamples, AudioProcessingSettings settings)
        {
            this.AudioProcessingSettings = settings;
            _leftChannel = new Channel(leftChannelSamples, settings);
            _rightChannel = new Channel(rightChannelSamples, settings);
        }

        public bool IsStereo => true;

        public int LengthSamples => _leftChannel.Length;

        public IAudioProcessingSettings AudioProcessingSettings { get; }

        public void Scan()
        {
            _leftChannel.Scan();
            _rightChannel.Scan();
        }

        public int GetTotalNumberOfPatches() =>
            _leftChannel.NumberOfPatches + _rightChannel.NumberOfPatches;

        public int GetNumberOfPatches(ChannelType channelType) =>
            channelType == ChannelType.Left
            ? _leftChannel.NumberOfPatches
            : _rightChannel.NumberOfPatches;

        public AbstractPatch[] GetAllPatches()
        {
            var allClicks = new List<AbstractPatch>();
            allClicks.AddRange(_leftChannel.GetAllPatches());
            allClicks.AddRange(_rightChannel.GetAllPatches());

            return allClicks.ToArray();
        }

        public bool ChannelIsPreprocessed(ChannelType channelType)
            => channelType == ChannelType.Left
            ? _leftChannel.IsReadyForScan
            : _rightChannel.IsReadyForScan;

        public double GetInputSample(ChannelType channelType, int index)
            => channelType == ChannelType.Left
            ? _leftChannel.GetInputSample(index)
            : _rightChannel.GetInputSample(index);

        public double GetOutputSample(ChannelType channelType, int position)
            => channelType == ChannelType.Left
            ? _leftChannel.GetOutputSample(position)
            : _rightChannel.GetOutputSample(position);

        public double GetPredictionErr(ChannelType channelType, int index)
            => channelType == ChannelType.Left
            ? _leftChannel.GetPredictionErr(index)
            : _rightChannel.GetPredictionErr(index);
    }
}