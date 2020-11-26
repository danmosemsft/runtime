// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

namespace System.Speech.Recognition
{
	/// <summary>Provides data for the <see langword="EmulateRecognizeCompleted" /> event of the <see cref="T:System.Speech.Recognition.SpeechRecognizer" /> and <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> classes.</summary>
	public class EmulateRecognizeCompletedEventArgs : AsyncCompletedEventArgs
	{
		private RecognitionResult _result;

		/// <summary>Gets the results of emulated recognition.</summary>
		/// <returns>Detailed information about the results of recognition, or <see langword="null" /> if an error occurred.</returns>
		public RecognitionResult Result => _result;

		internal EmulateRecognizeCompletedEventArgs(RecognitionResult result, Exception error, bool cancelled, object userState)
			: base(error, cancelled, userState)
		{
			_result = result;
		}
	}
}
