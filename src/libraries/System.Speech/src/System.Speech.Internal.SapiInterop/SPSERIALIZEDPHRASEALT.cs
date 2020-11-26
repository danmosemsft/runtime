// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace System.Speech.Internal.SapiInterop
{
    [StructLayout(LayoutKind.Sequential)]
    internal class SPSERIALIZEDPHRASEALT
    {
        internal uint ulStartElementInParent;

        internal uint cElementsInParent;

        internal uint cElementsInAlternate;

        internal uint cbAltExtra;
    }
}
