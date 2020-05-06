namespace AudioClickRepair.Data
{
    /// <summary>
    ///     Represents mono audio samples and includes information
    ///     about damaged samples
    /// </summary>
    public class Mono : IAudio
    {
        private readonly Channel _monoChannel;

        public Mono(double[] samples, IAudioProcessingSettings settings)
        {
            this.AudioProcessingSettings = settings;
            _monoChannel = new Channel(samples, settings);
        }

        public bool IsStereo => false;

        public int LengthSamples => _monoChannel.Length;

        public IAudioProcessingSettings AudioProcessingSettings { get; }

        public void Scan() => _monoChannel.Scan();

        public int GetTotalNumberOfPatches() =>
            _monoChannel.NumberOfPatches;

        public int GetNumberOfPatches(ChannelType channelType) =>
            _monoChannel.NumberOfPatches;

        public AbstractPatch[] GetPatches(ChannelType channelType) =>
            _monoChannel.GetAllPatches();

        public bool ChannelIsPreprocessed(ChannelType channelType) =>
            _monoChannel.IsReadyForScan;

        public double GetInputSample(ChannelType channelType, int index) =>
            _monoChannel.GetInputSample(index);

        public double GetOutputSample(ChannelType channelType, int position) =>
            _monoChannel.GetOutputSample(position);

        public double GetPredictionErr(ChannelType channelType, int index) =>
            _monoChannel.GetPredictionErr(index);
    }
}