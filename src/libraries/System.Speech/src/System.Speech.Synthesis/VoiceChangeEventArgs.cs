// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Synthesis
{
    /// <summary>Returns data from the <see cref="E:System.Speech.Synthesis.SpeechSynthesizer.VoiceChange" /> event.</summary>
    public class VoiceChangeEventArgs : PromptEventArgs
    {
        private VoiceInfo _voice;

        /// <summary>Gets the <see cref="T:System.Speech.Synthesis.VoiceInfo" /> object of the new voice.</summary>
        /// <returns>Returns information that describes and identifies the new voice.</returns>
        public VoiceInfo Voice => _voice;

        internal VoiceChangeEventArgs(Prompt prompt, VoiceInfo voice)
            : base(prompt)
        {
            _voice = voice;
        }
    }
}
