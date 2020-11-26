// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Internal.SapiInterop
{
	[Flags]
	internal enum SPBOOKMARKOPTIONS
	{
		SPBO_NONE = 0x0,
		SPBO_PAUSE = 0x1,
		SPBO_AHEAD = 0x2,
		SPBO_TIME_UNITS = 0x4
	}
}
