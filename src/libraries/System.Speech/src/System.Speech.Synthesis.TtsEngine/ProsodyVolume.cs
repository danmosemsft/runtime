// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Synthesis.TtsEngine
{
	/// <summary>Enumerates values for the <see langword="Volume" /> property of a <see langword="Prosody" /> object.</summary>
	public enum ProsodyVolume
	{
		/// <summary>Current default volume value, same as the value returned by the <see cref="P:System.Speech.Synthesis.TtsEngine.ITtsEngineSite.Volume" /> property on the <see cref="T:System.Speech.Synthesis.TtsEngine.ITtsEngineSite" /> site supplied to that engine.</summary>
		Default = -1,
		/// <summary>Volume off</summary>
		Silent = -2,
		/// <summary>Approximately 20% of maximum volume.</summary>
		ExtraSoft = -3,
		/// <summary>Approximately 40% of maximum volume.</summary>
		Soft = -4,
		/// <summary>Approximately 60% of maximum volume.</summary>
		Medium = -5,
		/// <summary>Approximately 80% of maximum volume.</summary>
		Loud = -6,
		/// <summary>Maximum volume.</summary>
		ExtraLoud = -7
	}
}
