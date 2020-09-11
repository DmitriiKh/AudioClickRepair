using CarefulAudioRepair.Data;
using NUnit.Framework;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace NUnitTests
{
    public class MonoTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [TestCase(0)]
        [TestCase(10)]
        [TestCase(512)]
        [TestCase(513)]
        [TestCase(1024)]
        public void Mono_ScansArray_DoNotThrow(int inputLength)
        {
            var zeros = new float[inputLength];
            var audio = new Mono(
                zeros,
                new AudioProcessingSettings() { HistoryLengthSamples = 512 });

            Assert.DoesNotThrowAsync(
                () => audio.ScanAsync(new Progress<string>(), new Progress<double>()));
        }
    }
}