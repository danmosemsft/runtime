namespace System.Speech.Synthesis
{
	/// <summary>Returns data from the <see cref="E:System.Speech.Synthesis.SpeechSynthesizer.SpeakProgress" /> event.</summary>
	public class SpeakProgressEventArgs : PromptEventArgs
	{
		private TimeSpan _audioPosition;

		private int _iWordPos;

		private int _cWordLen;

		private string _word;

		/// <summary>Gets the audio position of the event.</summary>
		/// <returns>Returns the position of the event in the audio output stream.</returns>
		public TimeSpan AudioPosition => _audioPosition;

		/// <summary>Gets the number of characters and spaces from the beginning of the prompt to the position before the first letter of the word that was just spoken.</summary>
		/// <returns>Returns the number of characters and spaces from the beginning of the prompt to the position before the first letter of the word that was just spoken.</returns>
		public int CharacterPosition => _iWordPos;

		/// <summary>Gets the number of characters in the word that was spoken just before the event was raised.</summary>
		/// <returns>Returns the number of characters in the word that was spoken just before the event was raised.</returns>
		public int CharacterCount
		{
			get
			{
				return _cWordLen;
			}
			internal set
			{
				_cWordLen = value;
			}
		}

		/// <summary>The text that was just spoken when the event was raised.</summary>
		/// <returns>Returns the text that was just spoken when the event was raised.</returns>
		public string Text
		{
			get
			{
				return _word;
			}
			internal set
			{
				_word = value;
			}
		}

		internal SpeakProgressEventArgs(Prompt prompt, TimeSpan audioPosition, int iWordPos, int cWordLen)
			: base(prompt)
		{
			_audioPosition = audioPosition;
			_iWordPos = iWordPos;
			_cWordLen = cWordLen;
		}
	}
}
