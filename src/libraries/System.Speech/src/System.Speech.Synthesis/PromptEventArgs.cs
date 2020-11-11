using System.ComponentModel;

namespace System.Speech.Synthesis
{
	/// <summary>Represents the base class for <see langword="EventArgs" /> classes in the <see cref="N:System.Speech.Synthesis" /> namespace.</summary>
	public abstract class PromptEventArgs : AsyncCompletedEventArgs
	{
		private Prompt _prompt;

		/// <summary>Gets the prompt associated with the event.</summary>
		/// <returns>The <see langword="Prompt" /> object associated with the event.</returns>
		public Prompt Prompt => _prompt;

		internal PromptEventArgs(Prompt prompt)
			: base(prompt._exception, prompt._exception != null, prompt)
		{
			_prompt = prompt;
		}
	}
}
