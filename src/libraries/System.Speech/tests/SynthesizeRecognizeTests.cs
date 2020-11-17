// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Speech.AudioFormat;
using System.Speech.Recognition;
using System.Speech.Recognition.SrgsGrammar;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Xml;
using Xunit;

namespace SampleSynthesisTests
{
    public class SynthesizeRecognizeTests : FileCleanupTestBase
    {
        [Fact]
        public void SpeechSynthesizerToSpeechRecognitionEngine()
        {
            if (Thread.CurrentThread.CurrentCulture.ToString() != "en-US")
                return;

            using var ms = new MemoryStream();

            using (var synth = new SpeechSynthesizer())
            {
                synth.SetOutputToWaveStream(ms);
                var prompt = new Prompt("synthesizer");
                synth.Speak(prompt);
            }

            ms.Position = 0;

            using (var rec = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("en-US")))
            {
                rec.LoadGrammar(new DictationGrammar());
                rec.SetInputToWaveStream(ms);
                RecognitionResult result = rec.Recognize();

                Assert.True(result.Confidence > 0.1);
                // handles "synthesizer", "synthesizes", etc.
                Assert.StartsWith("synthe", result.Text, StringComparison.OrdinalIgnoreCase);
            }
        }

        [Fact]
        public void SpeechSynthesizerToWavAndRepeat()
        {
            string wav = GetTestFilePath() + ".wav";

            using (var synth = new SpeechSynthesizer())
            {
                synth.SetOutputToWaveFile(wav);
                synth.Speak("hello");
            }

            Assert.True(new FileInfo(wav).Length > 0);

            using var ms = new MemoryStream();
            using (var synth = new SpeechSynthesizer())
            {
                synth.SetOutputToWaveStream(ms);

                var builder = new PromptBuilder();
                builder.AppendAudio(wav);
                synth.Speak(builder);

                Assert.True(ms.Position > 0);
            }
        }

        [Fact]
        public void SpeechSynthesizerEventsAndProperties()
        {
            if (Thread.CurrentThread.CurrentCulture.ToString() != "en-US")
                return;

            using (var synth = new SpeechSynthesizer())
            {
                using var ms = new MemoryStream();

                synth.SetOutputToNull();
                synth.SetOutputToAudioStream(ms, new SpeechAudioFormatInfo(16000, AudioBitsPerSample.Sixteen, AudioChannel.Stereo));
                synth.SelectVoiceByHints(VoiceGender.Male, VoiceAge.Adult);
                Assert.True(synth.Volume > 0);
                Assert.NotNull(synth.Voice);
                Assert.NotEmpty(synth.GetInstalledVoices());
                Assert.Null(synth.GetCurrentlySpokenPrompt());

                var builder = new PromptBuilder();
                builder.AppendText("synthesizer");

                int events = 0;
                synth.BookmarkReached += (object o, BookmarkReachedEventArgs e) => events++;
                synth.PhonemeReached += (object o, PhonemeReachedEventArgs e) => events++;
                synth.SpeakProgress += (object o, SpeakProgressEventArgs e) => events++;
                synth.SpeakStarted += (object o, SpeakStartedEventArgs e) => events++;
                synth.VisemeReached += (object o, VisemeReachedEventArgs e) => events++;
                synth.VoiceChange += (object o, VoiceChangeEventArgs e) => events++;
                synth.StateChanged += (object o, System.Speech.Synthesis.StateChangedEventArgs e) => events++;
                synth.SpeakCompleted += (object o, SpeakCompletedEventArgs e) =>
                {
                    events++;
                    Assert.Equal(34, events++);
                };

                Assert.Equal(SynthesizerState.Ready, synth.State);
                synth.SpeakSsml(builder.ToXml());
                Assert.Equal(SynthesizerState.Ready, synth.State);
                synth.Pause();
                Assert.Equal(SynthesizerState.Paused, synth.State);
                synth.Resume();
                Assert.Equal(SynthesizerState.Ready, synth.State);
            }
        }
    }
}
