// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Internal.SapiInterop
{
	[Flags]
	internal enum SpeechEmulationCompareFlags
	{
		SECFIgnoreCase = 0x1,
		SECFIgnoreKanaType = 0x10000,
		SECFIgnoreWidth = 0x20000,
		SECFNoSpecialChars = 0x20000000,
		SECFEmulateResult = 0x40000000,
		SECFDefault = 0x30001
	}
}
