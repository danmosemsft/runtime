namespace System.Speech.Recognition
{
	/// <summary>Enumerates values of the recognizer's state.</summary>
	public enum RecognizerState
	{
		/// <summary>The recognition engine is not receiving or analyzing audio input.</summary>
		Stopped,
		/// <summary>The recognition engine is available to receive and analyze audio input.</summary>
		Listening
	}
}
