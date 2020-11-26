// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Recognition
{
	/// <summary>Provides information for the <see cref="E:System.Speech.Recognition.Grammar.SpeechRecognized" />, <see cref="E:System.Speech.Recognition.SpeechRecognitionEngine.SpeechRecognized" />, and <see cref="E:System.Speech.Recognition.SpeechRecognizer.SpeechRecognized" /> events.</summary>
	[Serializable]
	public class SpeechRecognizedEventArgs : RecognitionEventArgs
	{
		internal SpeechRecognizedEventArgs(RecognitionResult result)
			: base(result)
		{
		}
	}
}
