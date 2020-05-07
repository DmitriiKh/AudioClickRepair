namespace AudioClickRepair.Data
{
    public interface IAudioProcessingSettings
    {
        public int HistoryLengthSamples { get; set; }

        public int CoefficientsNumber { get; set; }

        public double ThresholdForDetection { get; set; }

        public int MaxLengthOfCorrection { get; set; }

        public int SampleRate { get; set; }

        double MaxRegenerationError { get; set; }
    }
}