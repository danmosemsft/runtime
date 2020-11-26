// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Recognition
{
	/// <summary>Returns notification from <see cref="E:System.Speech.Recognition.SpeechRecognitionEngine.SpeechHypothesized" /> or <see cref="E:System.Speech.Recognition.SpeechRecognizer.SpeechHypothesized" /> events.  
	///  This class supports the .NET Framework infrastructure and is not intended to be used directly from application code.</summary>
	[Serializable]
	public class SpeechHypothesizedEventArgs : RecognitionEventArgs
	{
		internal SpeechHypothesizedEventArgs(RecognitionResult result)
			: base(result)
		{
		}
	}
}
