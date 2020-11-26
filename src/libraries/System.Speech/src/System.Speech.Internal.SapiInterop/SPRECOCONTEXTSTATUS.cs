// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace System.Speech.Internal.SapiInterop
{
    internal struct SPRECOCONTEXTSTATUS
    {
        internal SPINTERFERENCE eInterference;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 255)]
        internal short[] szRequestTypeOfUI;

        internal uint dwReserved1;

        internal uint dwReserved2;
    }
}
