// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Internal.SapiInterop
{
	internal struct SPVOICESTATUS
	{
		internal uint ulCurrentStream;

		internal uint ulLastStreamQueued;

		internal int hrLastResult;

		internal SpeechRunState dwRunningState;

		internal uint ulInputWordPos;

		internal uint ulInputWordLen;

		internal uint ulInputSentPos;

		internal uint ulInputSentLen;

		internal int lBookmarkId;

		internal ushort PhonemeId;

		internal int VisemeId;

		internal uint dwReserved1;

		internal uint dwReserved2;
	}
}
