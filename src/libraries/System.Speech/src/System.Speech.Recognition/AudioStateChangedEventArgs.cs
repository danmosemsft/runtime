// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Recognition
{
    /// <summary>Provides data for the <see langword="AudioStateChanged" /> event of the <see cref="T:System.Speech.Recognition.SpeechRecognizer" /> or the <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> class.</summary>
    public class AudioStateChangedEventArgs : EventArgs
    {
        private AudioState _audioState;

        /// <summary>Gets the new state of audio input to the recognizer.</summary>
        /// <returns>The state of audio input after a <see cref="E:System.Speech.Recognition.SpeechRecognitionEngine.AudioStateChanged" /> or a <see cref="E:System.Speech.Recognition.SpeechRecognizer.AudioStateChanged" /> event is raised.</returns>
        public AudioState AudioState => _audioState;

        internal AudioStateChangedEventArgs(AudioState audioState)
        {
            _audioState = audioState;
        }
    }
}
