// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace System.Speech.Internal.SapiInterop
{
    [StructLayout(LayoutKind.Sequential)]
    internal class SPSERIALIZEDPHRASEELEMENT
    {
        internal uint ulAudioTimeOffset;

        internal uint ulAudioSizeTime;

        internal uint ulAudioStreamOffset;

        internal uint ulAudioSizeBytes;

        internal uint ulRetainedStreamOffset;

        internal uint ulRetainedSizeBytes;

        internal uint pszDisplayTextOffset;

        internal uint pszLexicalFormOffset;

        internal uint pszPronunciationOffset;

        internal byte bDisplayAttributes;

        internal char RequiredConfidence;

        internal char ActualConfidence;

        internal byte Reserved;

        internal float SREngineConfidence;
    }
}
