namespace System.Speech.Synthesis
{
	/// <summary>Enumerates values for the state of the <see cref="T:System.Speech.Synthesis.SpeechSynthesizer" />.</summary>
	public enum SynthesizerState
	{
		/// <summary>Indicates that the <see cref="T:System.Speech.Synthesis.SpeechSynthesizer" /> is ready to generate speech from a prompt.</summary>
		Ready,
		/// <summary>Indicates that the <see cref="T:System.Speech.Synthesis.SpeechSynthesizer" /> is speaking.</summary>
		Speaking,
		/// <summary>Indicates that the <see cref="T:System.Speech.Synthesis.SpeechSynthesizer" /> is paused.</summary>
		Paused
	}
}
