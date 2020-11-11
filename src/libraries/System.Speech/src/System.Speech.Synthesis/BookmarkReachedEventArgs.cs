namespace System.Speech.Synthesis
{
	/// <summary>Returns data from the <see cref="E:System.Speech.Synthesis.SpeechSynthesizer.BookmarkReached" /> event.</summary>
	public class BookmarkReachedEventArgs : PromptEventArgs
	{
		private string _bookmark;

		private TimeSpan _audioPosition;

		/// <summary>Gets the name of the bookmark that was reached.</summary>
		/// <returns>Returns a value for the name of the bookmark.</returns>
		public string Bookmark => _bookmark;

		/// <summary>Gets the time offset at which the bookmark was reached.</summary>
		/// <returns>Returns the location in the audio input stream of a synthesis engine when the event was raised.</returns>
		public TimeSpan AudioPosition => _audioPosition;

		internal BookmarkReachedEventArgs(Prompt prompt, string bookmark, TimeSpan audioPosition)
			: base(prompt)
		{
			_bookmark = bookmark;
			_audioPosition = audioPosition;
		}
	}
}
