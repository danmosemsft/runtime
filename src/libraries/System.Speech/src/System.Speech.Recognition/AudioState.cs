// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Recognition
{
	/// <summary>Contains a list of possible states for the audio input to a speech recognition engine.</summary>
	public enum AudioState
	{
		/// <summary>Not processing audio input.</summary>
		Stopped,
		/// <summary>Receiving silence or non-speech background noise.</summary>
		Silence,
		/// <summary>Receiving speech input.</summary>
		Speech
	}
}
