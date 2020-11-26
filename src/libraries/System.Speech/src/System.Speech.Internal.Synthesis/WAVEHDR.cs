// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Internal.Synthesis
{
    internal struct WAVEHDR
    {
        internal IntPtr lpData;

        internal uint dwBufferLength;

        internal uint dwBytesRecorded;

        internal uint dwUser;

        internal uint dwFlags;

        internal uint dwLoops;

        internal IntPtr lpNext;

        internal uint reserved;
    }
}
