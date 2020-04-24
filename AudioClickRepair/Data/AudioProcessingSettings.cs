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

        public int HistoryLengthSamples { get; }
        public int CoefficientsNumber { get; }
        public float ThresholdForDetection { get; set; }
        public int MaxLengthOfCorrection { get; set; }
        public int SampleRate { get; set; } = -1;
    }
}