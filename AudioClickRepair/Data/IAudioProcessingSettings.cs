namespace AudioClickRepair.Data
{
    public interface IAudioProcessingSettings
    {
        public int HistoryLengthSamples { get; }

        public int CoefficientsNumber { get; }

        public float ThresholdForDetection { get; }

        public int MaxLengthOfCorrection { get; }

        public int SampleRate { get; }
        double MaxConnectionError { get; }
    }
}