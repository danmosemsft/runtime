namespace System.Speech.Synthesis
{
	/// <summary>Returns notification from the <see cref="E:System.Speech.Synthesis.SpeechSynthesizer.SpeakStarted" /> event.</summary>
	public class SpeakStartedEventArgs : PromptEventArgs
	{
		internal SpeakStartedEventArgs(Prompt prompt)
			: base(prompt)
		{
		}
	}
}
