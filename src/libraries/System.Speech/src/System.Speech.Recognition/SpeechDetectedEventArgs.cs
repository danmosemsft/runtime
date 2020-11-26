// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Recognition
{
    /// <summary>Returns data from <see cref="System.Speech.Recognition.SpeechRecognitionEngine.SpeechDetected" /> or <see cref="System.Speech.Recognition.SpeechRecognizer.SpeechDetected" /> events.</summary>
    public class SpeechDetectedEventArgs : EventArgs
    {
        private TimeSpan _audioPosition;

        /// <summary>Gets the position in the audio stream where speech was detected.</summary>
        /// <returns>Returns the location of a detected phrase within a recognition engine's speech buffer.</returns>
        public TimeSpan AudioPosition => _audioPosition;

        internal SpeechDetectedEventArgs(TimeSpan audioPosition)
        {
            _audioPosition = audioPosition;
        }
    }
}
