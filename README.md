# CarefulAudioRepair
This is a library for removing short sharp noises (clicks, pops, etc.) from audio.

## Using
```
var inputMono = new double[...];
// Update inputMono with input audio samples

var audio = new Mono(inputMono, new AudioProcessingSettings());

var status = new Progress<string>();
var progress = new Progress<double>();
await audio.ScanAsync(status, progress);

var patches = audio.GetPatches(ChannelType.Left);
```

OR

```
var inputLeft = new double[...];
var inputRight = new double[...];
// Update inputLeft and inputRight with input audio samples

var audio = new Stereo(inputLeft, inputRight, new AudioProcessingSettings());

var status = new Progress<string>();
var progress = new Progress<double>();
await audio.ScanAsync(status, progress);

var patchesLeft = audio.GetPatches(ChannelType.Left);
var patchesRight = audio.GetPatches(ChannelType.Right);
```
