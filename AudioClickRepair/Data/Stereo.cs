namespace AudioClickRepair.Data
{
    using System;

    /// <summary>
    ///     Represents stereo audio samples and includes information
    ///     about damaged samples.
    /// </summary>
    public class Stereo : IAudio
    {
        private readonly Channel leftChannel;
        private readonly Channel rightChannel;

        public Stereo(
            double[] leftChannelSamples,
            double[] rightChannelSamples,
            IAudioProcessingSettings settings)
        {
            if (settings is null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            this.AudioProcessingSettings = settings;
            this.leftChannel = new Channel(leftChannelSamples, settings);
            this.rightChannel = new Channel(rightChannelSamples, settings);
        }

        public bool IsStereo => true;

        public int LengthSamples => this.leftChannel.Length;

        public IAudioProcessingSettings AudioProcessingSettings { get; }

        public void Scan()
        {
            this.leftChannel.Scan();
            this.rightChannel.Scan();
        }

        public int GetTotalNumberOfPatches() =>
            this.leftChannel.NumberOfPatches + this.rightChannel.NumberOfPatches;

        public int GetNumberOfPatches(ChannelType channelType) =>
            channelType == ChannelType.Left
            ? this.leftChannel.NumberOfPatches
            : this.rightChannel.NumberOfPatches;

        public Patch[] GetPatches(ChannelType channelType) =>
            channelType == ChannelType.Left
            ? this.leftChannel.GetAllPatches()
            : this.rightChannel.GetAllPatches();

        public bool ChannelIsPreprocessed(ChannelType channelType) =>
            channelType == ChannelType.Left
            ? this.leftChannel.IsReadyForScan
            : this.rightChannel.IsReadyForScan;

        public double GetInputSample(ChannelType channelType, int index) =>
            channelType == ChannelType.Left
            ? this.leftChannel.GetInputSample(index)
            : this.rightChannel.GetInputSample(index);

        public double GetOutputSample(ChannelType channelType, int position) =>
            channelType == ChannelType.Left
            ? this.leftChannel.GetOutputSample(position)
            : this.rightChannel.GetOutputSample(position);

        public double GetPredictionErr(ChannelType channelType, int index) =>
            channelType == ChannelType.Left
            ? this.leftChannel.GetPredictionErr(index)
            : this.rightChannel.GetPredictionErr(index);
    }
}