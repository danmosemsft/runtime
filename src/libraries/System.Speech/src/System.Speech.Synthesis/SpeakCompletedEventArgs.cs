// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Synthesis
{
	/// <summary>Returns notification from the <see cref="E:System.Speech.Synthesis.SpeechSynthesizer.SpeakCompleted" /> event.</summary>
	public class SpeakCompletedEventArgs : PromptEventArgs
	{
		internal SpeakCompletedEventArgs(Prompt prompt)
			: base(prompt)
		{
		}
	}
}
