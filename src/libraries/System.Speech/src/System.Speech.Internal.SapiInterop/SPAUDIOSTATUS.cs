// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Internal.SapiInterop
{
    internal struct SPAUDIOSTATUS
    {
        internal int cbFreeBuffSpace;

        internal uint cbNonBlockingIO;

        internal SPAUDIOSTATE State;

        internal ulong CurSeekPos;

        internal ulong CurDevicePos;

        internal uint dwAudioLevel;

        internal uint dwReserved2;
    }
}
