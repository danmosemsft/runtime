// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Recognition
{
    /// <summary>Provides data for the <c>AudioSignalProblemOccurred</c> event of a <see cref="System.Speech.Recognition.SpeechRecognizer" /> or a <see cref="System.Speech.Recognition.SpeechRecognitionEngine" />.</summary>
    public class AudioSignalProblemOccurredEventArgs : EventArgs
    {
        private AudioSignalProblem _audioSignalProblem;

        private TimeSpan _recognizerPosition;

        private TimeSpan _audioPosition;

        private int _audioLevel;

        /// <summary>Gets the audio signal problem.</summary>
        /// <returns>The audio signal problem that caused the <c>AudioSignalProblemOccurred</c> event to be raised.</returns>
        public AudioSignalProblem AudioSignalProblem => _audioSignalProblem;

        /// <summary>Gets the audio level associated with the event.</summary>
        /// <returns>The level of audio input when the <c>AudioSignalProblemOccurred</c> event was raised.</returns>
        public int AudioLevel => _audioLevel;

        /// <summary>Gets the position in the input device's audio stream that indicates where the problem occurred.</summary>
        /// <returns>The position in the input device's audio stream when the <c>AudioSignalProblemOccurred</c> event was raised.</returns>
        public TimeSpan AudioPosition => _audioPosition;

        /// <summary>Gets the position in the audio input that the recognizer has received that indicates where the problem occurred.</summary>
        /// <returns>The position in the audio input that the recognizer has received when the <c>AudioSignalProblemOccurred</c> event was raised.</returns>
        public TimeSpan RecognizerAudioPosition => _recognizerPosition;

        internal AudioSignalProblemOccurredEventArgs(AudioSignalProblem audioSignalProblem, int audioLevel, TimeSpan audioPosition, TimeSpan recognizerPosition)
        {
            _audioSignalProblem = audioSignalProblem;
            _audioLevel = audioLevel;
            _audioPosition = audioPosition;
            _recognizerPosition = recognizerPosition;
        }
    }
}
