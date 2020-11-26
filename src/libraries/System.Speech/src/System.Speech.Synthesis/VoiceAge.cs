// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Synthesis
{
	/// <summary>Defines the values for the age of a synthesized voice.</summary>
	public enum VoiceAge
	{
		/// <summary>Indicates that no voice age is specified.</summary>
		NotSet = 0,
		/// <summary>Indicates a child voice (age 10).</summary>
		Child = 10,
		/// <summary>Indicates a teenage voice (age 15).</summary>
		Teen = 0xF,
		/// <summary>Indicates an adult voice (age 30).</summary>
		Adult = 30,
		/// <summary>Indicates a senior voice (age 65).</summary>
		Senior = 65
	}
}
