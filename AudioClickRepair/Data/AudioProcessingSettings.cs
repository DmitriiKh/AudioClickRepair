namespace AudioClickRepair.Data
{
    public class AudioProcessingSettings : IAudioProcessingSettings
    {
        public AudioProcessingSettings()
        {
            HistoryLengthSamples = 512;
            CoefficientsNumber = 4;
            ThresholdForDetection = 10;
            MaxLengthOfCorrection = 250;
        }

        public int HistoryLengthSamples { get; set; }

        public int CoefficientsNumber { get; set; }

        public double ThresholdForDetection { get; set; }

        public int MaxLengthOfCorrection { get; set; }

        public int SampleRate { get; set; } = -1;

        public double MaxConnectionError { get; set; } = 0.1;
    }
}