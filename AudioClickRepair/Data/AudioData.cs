namespace AudioClickRepair.Data
{
    public abstract class AudioData
    {
        public bool IsStereo { get; set; }

        public IAudioProcessingSettings AudioProcessingSettings { get; protected set; }

        public abstract int GetTotalNumberOfClicks();

        public abstract int LengthSamples();

        public abstract bool ChannelIsPreprocessed(ChannelType channelType);

        public abstract int GetNumberOfClicksIn(ChannelType channelType);

        public abstract AbstractPatch[] GetAllClicks();

        public abstract double GetInputSample(ChannelType channelType, int position);

        public abstract double GetOutputSample(ChannelType channelType, int position);

        public abstract double GetPredictionErr(ChannelType channelType, int position);
    }
}