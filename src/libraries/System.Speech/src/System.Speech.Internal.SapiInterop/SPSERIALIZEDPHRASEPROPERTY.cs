// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace System.Speech.Internal.SapiInterop
{
    [StructLayout(LayoutKind.Sequential)]
    internal class SPSERIALIZEDPHRASEPROPERTY
    {
        internal uint pszNameOffset;

        internal uint ulId;

        internal uint pszValueOffset;

        internal ushort vValue;

        internal ulong SpVariantSubset;

        internal uint ulFirstElement;

        internal uint ulCountOfElements;

        internal uint pNextSiblingOffset;

        internal uint pFirstChildOffset;

        internal float SREngineConfidence;

        internal sbyte Confidence;
    }
}
