﻿using System;
using System.Threading.Tasks;

namespace AudioClickRepair.Data
{
    public interface IAudio
    {
        public bool IsStereo { get; }

        public int LengthSamples { get; }

        public IAudioProcessingSettings Settings { get; }

        public Task ScanAsync(IProgress<string> status);

        public int GetTotalNumberOfPatches();

        public int GetNumberOfPatches(ChannelType channelType);

        public Patch[] GetPatches(ChannelType channelType);

        public bool ChannelIsPreprocessed(ChannelType channelType);

        public double GetInputSample(ChannelType channelType, int position);

        public double GetOutputSample(ChannelType channelType, int position);

        public double GetPredictionErr(ChannelType channelType, int position);

        public double[] GetOutputArray(ChannelType channelType);
    }
}