# CarefulAudioRepair
This is a library for removing short sharp noises (clicks, pops, etc.) from audio.

## Using
```
var inputMono = new double[...];
// Update inputMono with input audio samples

var audio = new Mono(inputMono, new AudioProcessingSettings() { SampleRate = <your_audio_sample_rate> });

var status = new Progress<string>();
var progress = new Progress<double>();

await audio.ScanAsync(status, progress);

var patches = audio.GetPatches(ChannelType.Left);

var outputMono = audio.GetOutputArray(ChannelType.Left);
```

OR

```
var inputLeft = new double[...];
var inputRight = new double[...];
// Update inputLeft and inputRight with input audio samples

var audio = new Stereo(inputLeft, inputRight, new AudioProcessingSettings() { SampleRate = <your_audio_sample_rate> });

var status = new Progress<string>();
var progress = new Progress<double>();

await audio.ScanAsync(status, progress);

var patchesLeft = audio.GetPatches(ChannelType.Left);
var patchesRight = audio.GetPatches(ChannelType.Right);

var outputLeft = audio.GetOutputArray(ChannelType.Left);
var outputRight = audio.GetOutputArray(ChannelType.Right);
```
