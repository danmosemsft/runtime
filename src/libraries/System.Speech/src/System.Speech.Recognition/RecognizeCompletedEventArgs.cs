// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

namespace System.Speech.Recognition
{
    /// <summary>Provides data for the <see langword="RecognizeCompleted" /> event raised by a <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> or a <see cref="T:System.Speech.Recognition.SpeechRecognizer" /> object.</summary>
    public class RecognizeCompletedEventArgs : AsyncCompletedEventArgs
    {
        private RecognitionResult _result;

        private bool _initialSilenceTimeout;

        private bool _babbleTimeout;

        private bool _inputStreamEnded;

        private TimeSpan _audioPosition;

        /// <summary>Gets the recognition result.</summary>
        /// <returns>The recognition result if the recognition operation succeeded; otherwise, <see langword="null" />.</returns>
        public RecognitionResult Result => _result;

        /// <summary>Gets a value that indicates whether an initial silence timeout generated the <see cref="E:System.Speech.Recognition.SpeechRecognitionEngine.RecognizeCompleted" /> event.</summary>
        /// <returns>
        ///   <see langword="true" /> if the <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> has detected only silence for a longer time period than was specified by its <see cref="P:System.Speech.Recognition.SpeechRecognitionEngine.InitialSilenceTimeout" /> property; otherwise <see langword="false." /></returns>
        public bool InitialSilenceTimeout => _initialSilenceTimeout;

        /// <summary>Gets a value that indicates whether a babble timeout generated the <see cref="E:System.Speech.Recognition.SpeechRecognitionEngine.RecognizeCompleted" /> event.</summary>
        /// <returns>
        ///   <see langword="true" /> if the <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> has detected only background noise for longer than was specified by its <see cref="P:System.Speech.Recognition.SpeechRecognitionEngine.BabbleTimeout" /> property; otherwise <see langword="false." /></returns>
        public bool BabbleTimeout => _babbleTimeout;

        /// <summary>Gets a value indicating whether the input stream ended.</summary>
        /// <returns>
        ///   <see langword="true" /> if the recognizer no longer has audio input; otherwise, <see langword="false" />.</returns>
        public bool InputStreamEnded => _inputStreamEnded;

        /// <summary>Gets the location in the input device's audio stream associated with the <see cref="E:System.Speech.Recognition.SpeechRecognitionEngine.RecognizeCompleted" /> event.</summary>
        /// <returns>The location in the input device's audio stream associated with the <see cref="E:System.Speech.Recognition.SpeechRecognitionEngine.RecognizeCompleted" /> event.</returns>
        public TimeSpan AudioPosition => _audioPosition;

        internal RecognizeCompletedEventArgs(RecognitionResult result, bool initialSilenceTimeout, bool babbleTimeout, bool inputStreamEnded, TimeSpan audioPosition, Exception error, bool cancelled, object userState)
            : base(error, cancelled, userState)
        {
            _result = result;
            _initialSilenceTimeout = initialSilenceTimeout;
            _babbleTimeout = babbleTimeout;
            _inputStreamEnded = inputStreamEnded;
            _audioPosition = audioPosition;
        }
    }
}
