namespace System.Speech.Recognition
{
	/// <summary>Provides information about speech recognition events.</summary>
	[Serializable]
	public abstract class RecognitionEventArgs : EventArgs
	{
		private RecognitionResult _result;

		/// <summary>Gets the recognition result data associated with the speech recognition event.</summary>
		/// <returns>The <see cref="P:System.Speech.Recognition.RecognitionEventArgs.Result" /> property returns the <see cref="T:System.Speech.Recognition.RecognitionResult" /> that contains the information about the recognition.</returns>
		public RecognitionResult Result => _result;

		internal RecognitionEventArgs(RecognitionResult result)
		{
			_result = result;
		}
	}
}
