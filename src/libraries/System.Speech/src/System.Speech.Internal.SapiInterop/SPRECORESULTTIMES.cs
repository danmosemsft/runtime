// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Internal.SapiInterop
{
    [Serializable]
    internal struct SPRECORESULTTIMES
    {
        internal FILETIME ftStreamTime;

        internal ulong ullLength;

        internal uint dwTickCount;

        internal ulong ullStart;
    }
}
