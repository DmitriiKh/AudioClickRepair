namespace AudioClickRepair.Data
{
    using System;
    using System.Linq;

    /// <summary>
    ///     Represents mono audio samples and includes information
    ///     about damaged samples.
    /// </summary>
    public class Mono : IAudio
    {
        private readonly Channel monoChannel;

        public Mono(double[] samples, IAudioProcessingSettings settings)
        {
            if (settings is null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            this.AudioProcessingSettings = settings;
            this.monoChannel = new Channel(samples, settings);
        }

        public bool IsStereo => false;

        public int LengthSamples => this.monoChannel.Length;

        public IAudioProcessingSettings AudioProcessingSettings { get; }

        public void Scan() => this.monoChannel.Scan();

        public int GetTotalNumberOfPatches() =>
            this.monoChannel.NumberOfPatches;

        public int GetNumberOfPatches(ChannelType channelType) =>
            this.monoChannel.NumberOfPatches;

        public Patch[] GetPatches(ChannelType channelType) =>
            this.monoChannel.GetAllPatches();

        public bool ChannelIsPreprocessed(ChannelType channelType) =>
            this.monoChannel.IsReadyForScan;

        public double GetInputSample(ChannelType channelType, int index) =>
            this.monoChannel.GetInputSample(index);

        public double GetOutputSample(ChannelType channelType, int position) =>
            this.monoChannel.GetOutputSample(position);

        public double GetPredictionErr(ChannelType channelType, int index) =>
            this.monoChannel.GetPredictionErr(index);
    }
}