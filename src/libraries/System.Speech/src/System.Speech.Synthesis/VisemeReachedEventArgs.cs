// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Synthesis
{
    /// <summary>Returns data from the <see cref="System.Speech.Synthesis.SpeechSynthesizer.VisemeReached" /> event.</summary>
    public class VisemeReachedEventArgs : PromptEventArgs
    {
        private int _currentViseme;

        private TimeSpan _audioPosition;

        private TimeSpan _duration;

        private SynthesizerEmphasis _emphasis;

        private int _nextViseme;

        /// <summary>Gets the value of the viseme.</summary>
        /// <returns>An <see cref="int" /> object that contains the value of the viseme.</returns>
        public int Viseme => _currentViseme;

        /// <summary>Gets the position of the viseme in the audio stream.</summary>
        /// <returns>A <see cref="System.TimeSpan" /> object that represents the position of the viseme.</returns>
        public TimeSpan AudioPosition => _audioPosition;

        /// <summary>Gets the duration of the viseme.</summary>
        /// <returns>A <see cref="System.TimeSpan" /> object that represents the duration of the viseme.</returns>
        public TimeSpan Duration => _duration;

        /// <summary>Gets a <see cref="System.Speech.Synthesis.SynthesizerEmphasis" /> object that describes the emphasis of the viseme.</summary>
        /// <returns>A <see cref="System.Speech.Synthesis.SynthesizerEmphasis" /> object that represents the emphasis of the viseme.</returns>
        public SynthesizerEmphasis Emphasis => _emphasis;

        /// <summary>Gets the value of the next viseme.</summary>
        /// <returns>An <see cref="int" /> object that contains the value of the next viseme.</returns>
        public int NextViseme => _nextViseme;

        internal VisemeReachedEventArgs(Prompt speakPrompt, int currentViseme, TimeSpan audioPosition, TimeSpan duration, SynthesizerEmphasis emphasis, int nextViseme)
            : base(speakPrompt)
        {
            _currentViseme = currentViseme;
            _audioPosition = audioPosition;
            _duration = duration;
            _emphasis = emphasis;
            _nextViseme = nextViseme;
        }
    }
}
