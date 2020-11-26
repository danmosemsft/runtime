// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Synthesis
{
    /// <summary>Returns data from the <see cref="E:System.Speech.Synthesis.SpeechSynthesizer.StateChanged" /> event.</summary>
    public class StateChangedEventArgs : EventArgs
    {
        private SynthesizerState _state;

        private SynthesizerState _previousState;

        /// <summary>Gets the state of the <see cref="T:System.Speech.Synthesis.SpeechSynthesizer" /> before the <see cref="E:System.Speech.Synthesis.SpeechSynthesizer.StateChanged" /> event.</summary>
        /// <returns>The state of the synthesizer after the state changed.</returns>
        public SynthesizerState State => _state;

        /// <summary>Gets the state of the <see cref="T:System.Speech.Synthesis.SpeechSynthesizer" /> before the <see cref="E:System.Speech.Synthesis.SpeechSynthesizer.StateChanged" /> event.</summary>
        /// <returns>Returns the state of the synthesizer before the state changed.</returns>
        public SynthesizerState PreviousState => _previousState;

        internal StateChangedEventArgs(SynthesizerState state, SynthesizerState previousState)
        {
            _state = state;
            _previousState = previousState;
        }
    }
}
