// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace System.Speech.Internal.SapiInterop
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    internal class SPPHRASERULE
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        internal string pszName;

        internal uint ulId;

        internal uint ulFirstElement;

        internal uint ulCountOfElements;

        internal IntPtr pNextSibling;

        internal IntPtr pFirstChild;

        internal float SREngineConfidence;

        internal byte Confidence;
    }
}
