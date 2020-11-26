// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Internal.Synthesis
{
	internal enum MMSYSERR
	{
		NOERROR = 0,
		ERROR = 1,
		BADDEVICEID = 2,
		NOTENABLED = 3,
		ALLOCATED = 4,
		INVALHANDLE = 5,
		NODRIVER = 6,
		NOMEM = 7,
		NOTSUPPORTED = 8,
		BADERRNUM = 9,
		INVALFLAG = 10,
		INVALPARAM = 11,
		HANDLEBUSY = 12,
		INVALIDALIAS = 13,
		BADDB = 14,
		KEYNOTFOUND = 0xF,
		READERROR = 0x10,
		WRITEERROR = 17,
		DELETEERROR = 18,
		VALNOTFOUND = 19,
		NODRIVERCB = 20,
		LASTERROR = 20
	}
}
