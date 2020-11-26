// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Recognition
{
    /// <summary>Provides information for the <see cref="E:System.Speech.Recognition.SpeechRecognizer.SpeechRecognitionRejected" /> and <see cref="E:System.Speech.Recognition.SpeechRecognitionEngine.SpeechRecognitionRejected" /> events.</summary>
    [Serializable]
    public class SpeechRecognitionRejectedEventArgs : RecognitionEventArgs
    {
        internal SpeechRecognitionRejectedEventArgs(RecognitionResult result)
            : base(result)
        {
        }
    }
}
