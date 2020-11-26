// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Internal.SapiInterop
{
	[Flags]
	internal enum SPCOMMITFLAGS
	{
		SPCF_NONE = 0x0,
		SPCF_ADD_TO_USER_LEXICON = 0x1,
		SPCF_DEFINITE_CORRECTION = 0x2
	}
}
