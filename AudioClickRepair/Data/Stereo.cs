using System.Collections.Generic;

namespace AudioClickRepair.Data
{
    /// <summary>
    ///     Represents stereo audio samples and includes information
    ///     about damaged samples
    /// </summary>
    public class Stereo : AudioData
    {
        private readonly Channel _leftChannel;
        private readonly Channel _rightChannel;

        public Stereo(double[] leftChannelSamples, double[] rightChannelSamples)
        {
            IsStereo = true;
            _leftChannel = new Channel(leftChannelSamples);
            _rightChannel = new Channel(rightChannelSamples);

            AudioProcessingSettings = new AudioProcessingSettings();
        }

        public override int GetTotalNumberOfClicks()
        {
            return _leftChannel.GetNumberOfPatches() + _rightChannel.GetNumberOfPatches();
        }

        public override bool ChannelIsPreprocessed(ChannelType channelType)
            => channelType == ChannelType.Left
            ? _leftChannel.IsReadyForScan
            : _rightChannel.IsReadyForScan;

        public override double GetInputSample(ChannelType channelType, int index)
            => channelType == ChannelType.Left
            ? _leftChannel.GetInputSample(index)
            : _rightChannel.GetInputSample(index);

        public override double GetPredictionErr(ChannelType channelType, int index)
            => channelType == ChannelType.Left
            ? _leftChannel.GetPredictionErr(index)
            : _rightChannel.GetPredictionErr(index);

        public override int LengthSamples() => _leftChannel.LengthSamples();

        public override int GetNumberOfClicksIn(ChannelType channelType) =>
            channelType == ChannelType.Left
            ? _leftChannel.GetNumberOfPatches()
            : _rightChannel.GetNumberOfPatches();

        public override IPatch[] GetAllClicks()
        {
            var allClicks = new List<IPatch>();
            allClicks.AddRange(_leftChannel.GetAllPatches());
            allClicks.AddRange(_rightChannel.GetAllPatches());

            return allClicks.ToArray();
        }

        public override double GetOutputSample(ChannelType channelType, int position)
            => channelType == ChannelType.Left
            ? _leftChannel.GetOutputSample(position)
            : _rightChannel.GetOutputSample(position);
    }
}