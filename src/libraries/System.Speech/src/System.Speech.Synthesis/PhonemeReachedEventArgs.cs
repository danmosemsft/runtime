namespace System.Speech.Synthesis
{
	/// <summary>Returns data from the <see cref="E:System.Speech.Synthesis.SpeechSynthesizer.PhonemeReached" /> event.</summary>
	public class PhonemeReachedEventArgs : PromptEventArgs
	{
		private string _currentPhoneme;

		private TimeSpan _audioPosition;

		private TimeSpan _duration;

		private SynthesizerEmphasis _emphasis;

		private string _nextPhoneme;

		/// <summary>The phoneme associated with the <see cref="E:System.Speech.Synthesis.SpeechSynthesizer.PhonemeReached" /> event.</summary>
		/// <returns>A string containing the phoneme.</returns>
		public string Phoneme => _currentPhoneme;

		/// <summary>Gets the audio position of the phoneme.</summary>
		/// <returns>A <see langword="TimeSpan" /> object indicating the audio position.</returns>
		public TimeSpan AudioPosition => _audioPosition;

		/// <summary>Gets the duration of the phoneme.</summary>
		/// <returns>A <see langword="TimeSpan" /> object indicating the duration.</returns>
		public TimeSpan Duration => _duration;

		/// <summary>Gets the emphasis of the phoneme.</summary>
		/// <returns>A <see langword="SynthesizerEmphasis" /> member indicating the level of emphasis.</returns>
		public SynthesizerEmphasis Emphasis => _emphasis;

		/// <summary>Gets the phoneme following the phoneme associated with the <see cref="E:System.Speech.Synthesis.SpeechSynthesizer.PhonemeReached" /> event.</summary>
		/// <returns>A string containing the next phoneme.</returns>
		public string NextPhoneme => _nextPhoneme;

		internal PhonemeReachedEventArgs(Prompt prompt, string currentPhoneme, TimeSpan audioPosition, TimeSpan duration, SynthesizerEmphasis emphasis, string nextPhoneme)
			: base(prompt)
		{
			_currentPhoneme = currentPhoneme;
			_audioPosition = audioPosition;
			_duration = duration;
			_emphasis = emphasis;
			_nextPhoneme = nextPhoneme;
		}
	}
}
