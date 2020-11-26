// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Recognition
{
    /// <summary>Returns data from the <see cref="E:System.Speech.Recognition.SpeechRecognizer.StateChanged" /> event.</summary>
    public class StateChangedEventArgs : EventArgs
    {
        private RecognizerState _recognizerState;

        /// <summary>Gets the current state of the shared speech recognition engine in Windows.</summary>
        /// <returns>A <see cref="T:System.Speech.Recognition.RecognizerState" /> instance that indicates whether the state of a shared speech recognition engine is <see langword="Listening" /> or <see langword="Stopped" />.</returns>
        public RecognizerState RecognizerState => _recognizerState;

        internal StateChangedEventArgs(RecognizerState recognizerState)
        {
            _recognizerState = recognizerState;
        }
    }
}
