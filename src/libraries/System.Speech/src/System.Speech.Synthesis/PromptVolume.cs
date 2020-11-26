// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Synthesis
{
	/// <summary>Enumerates values for volume levels (loudness) in prompts.</summary>
	public enum PromptVolume
	{
		/// <summary>Indicates that the volume level is not set.</summary>
		NotSet,
		/// <summary>Indicates a muted volume level.</summary>
		Silent,
		/// <summary>Indicates an extra soft volume level.</summary>
		ExtraSoft,
		/// <summary>Indicates a soft volume level.</summary>
		Soft,
		/// <summary>Indicates a medium volume level.</summary>
		Medium,
		/// <summary>Indicates a loud volume level.</summary>
		Loud,
		/// <summary>Indicates an extra loud volume level.</summary>
		ExtraLoud,
		/// <summary>Indicates the engine-specific default volume level.</summary>
		Default
	}
}
