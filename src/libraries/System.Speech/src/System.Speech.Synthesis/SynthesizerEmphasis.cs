namespace System.Speech.Synthesis
{
	/// <summary>Enumerates levels of synthesizer emphasis.</summary>
	[Flags]
	public enum SynthesizerEmphasis
	{
		/// <summary>Indicates a high level of synthesizer emphasis.</summary>
		Stressed = 0x1,
		/// <summary>Indicates a low level of synthesizer emphasis.</summary>
		Emphasized = 0x2
	}
}
