// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Recognition
{
	/// <summary>Returns data from a <see cref="E:System.Speech.Recognition.SpeechRecognitionEngine.RecognizerUpdateReached" /> or a <see cref="E:System.Speech.Recognition.SpeechRecognizer.RecognizerUpdateReached" /> event.</summary>
	public class RecognizerUpdateReachedEventArgs : EventArgs
	{
		private object _userToken;

		private TimeSpan _audioPosition;

		/// <summary>Gets the <c>UserToken</c> passed to the system when an application calls <see cref="Overload:System.Speech.Recognition.SpeechRecognitionEngine.RequestRecognizerUpdate" /> or <see cref="Overload:System.Speech.Recognition.SpeechRecognizer.RequestRecognizerUpdate" />.</summary>
		/// <returns>Returns an object that contains the <c>UserToken</c>.</returns>
		public object UserToken => _userToken;

		/// <summary>Gets the audio position associated with the event.</summary>
		/// <returns>Returns the location within the speech buffer of a <see cref="T:System.Speech.Recognition.SpeechRecognizer" /> or a <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> when it pauses and raises a <c>RecognizerUpdateReached</c> event.</returns>
		public TimeSpan AudioPosition => _audioPosition;

		internal RecognizerUpdateReachedEventArgs(object userToken, TimeSpan audioPosition)
		{
			_userToken = userToken;
			_audioPosition = audioPosition;
		}
	}
}
