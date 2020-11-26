// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace System.Speech.Internal.SapiInterop
{
    [StructLayout(LayoutKind.Sequential)]
    internal class SPSEMANTICERRORINFO
    {
        internal uint ulLineNumber;

        internal uint pszScriptLineOffset;

        internal uint pszSourceOffset;

        internal uint pszDescriptionOffset;

        internal int hrResultCode;
    }
}
