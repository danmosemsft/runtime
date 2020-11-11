namespace System.Speech.Synthesis
{
	/// <summary>Returns notification from the <see cref="E:System.Speech.Synthesis.SpeechSynthesizer.SpeakCompleted" /> event.</summary>
	public class SpeakCompletedEventArgs : PromptEventArgs
	{
		internal SpeakCompletedEventArgs(Prompt prompt)
			: base(prompt)
		{
		}
	}
}
