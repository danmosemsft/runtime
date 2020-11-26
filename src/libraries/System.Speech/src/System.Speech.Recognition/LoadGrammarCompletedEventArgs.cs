// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

namespace System.Speech.Recognition
{
    /// <summary>Provides data for the <see langword="LoadGrammarCompleted" /> event of a <see cref="T:System.Speech.Recognition.SpeechRecognizer" /> or <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> object.</summary>
    public class LoadGrammarCompletedEventArgs : AsyncCompletedEventArgs
    {
        private Grammar _grammar;

        /// <summary>The <see cref="T:System.Speech.Recognition.Grammar" /> object that has completed loading.</summary>
        /// <returns>The <see cref="T:System.Speech.Recognition.Grammar" /> that was loaded by the recognizer.</returns>
        public Grammar Grammar => _grammar;

        internal LoadGrammarCompletedEventArgs(Grammar grammar, Exception error, bool cancelled, object userState)
            : base(error, cancelled, userState)
        {
            _grammar = grammar;
        }
    }
}
