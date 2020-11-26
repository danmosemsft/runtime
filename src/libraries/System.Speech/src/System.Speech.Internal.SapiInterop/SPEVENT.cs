// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Internal.SapiInterop
{
	internal struct SPEVENT
	{
		public SPEVENTENUM eEventId;

		public SPEVENTLPARAMTYPE elParamType;

		public uint ulStreamNum;

		public ulong ullAudioStreamOffset;

		public IntPtr wParam;

		public IntPtr lParam;
	}
}
