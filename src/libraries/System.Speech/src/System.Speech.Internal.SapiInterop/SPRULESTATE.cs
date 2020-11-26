// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Internal.SapiInterop
{
	internal enum SPRULESTATE
	{
		SPRS_INACTIVE = 0,
		SPRS_ACTIVE = 1,
		SPRS_ACTIVE_WITH_AUTO_PAUSE = 3,
		SPRS_ACTIVE_USER_DELIMITED = 4
	}
}
