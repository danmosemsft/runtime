// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Recognition
{
    /// <summary>Provides data for the <see langword="AudioLevelUpdated" /> event of the <see cref="T:System.Speech.Recognition.SpeechRecognizer" /> or the <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> class.</summary>
    public class AudioLevelUpdatedEventArgs : EventArgs
    {
        private int _audioLevel;

        /// <summary>Gets the new level of audio input after the <see cref="E:System.Speech.Recognition.SpeechRecognitionEngine.AudioLevelUpdated" /> or the <see cref="E:System.Speech.Recognition.SpeechRecognizer.AudioLevelUpdated" /> event is raised.</summary>
        /// <returns>The new level of audio input.</returns>
        public int AudioLevel => _audioLevel;

        internal AudioLevelUpdatedEventArgs(int audioLevel)
        {
            _audioLevel = audioLevel;
        }
    }
}
