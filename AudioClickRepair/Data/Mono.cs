namespace AudioClickRepair.Data
{
    /// <summary>
    ///     Represents mono audio samples and includes information
    ///     about damaged samples
    /// </summary>
    public class Mono : AudioData
    {
        private readonly Channel _monoChannel;

        public Mono(double[] samples)
        {
            IsStereo = false;
            _monoChannel = new Channel(samples);

            AudioProcessingSettings = new AudioProcessingSettings();
        }

        public override int GetTotalNumberOfClicks()
        {
            return _monoChannel.GetNumberOfPatches();
        }

        public override bool ChannelIsPreprocessed(ChannelType channelType)
            => _monoChannel.IsReadyForScan;

        public override double GetInputSample(ChannelType channelType, int index)
            => _monoChannel.GetInputSample(index);

        public override double GetPredictionErr(ChannelType channelType, int index)
            => _monoChannel.GetPredictionErr(index);

        public override int LengthSamples() => _monoChannel.LengthSamples();

        public override int GetNumberOfClicksIn(ChannelType channelType) =>
            _monoChannel.GetNumberOfPatches();

        public override IPatch[] GetAllClicks() => _monoChannel.GetAllPatches();

        public override double GetOutputSample(ChannelType channelType, int position) =>
            _monoChannel.GetOutputSample(position);
    }
}