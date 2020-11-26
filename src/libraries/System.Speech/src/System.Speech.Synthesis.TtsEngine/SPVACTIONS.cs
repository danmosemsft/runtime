// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace System.Speech.Synthesis.TtsEngine
{
    [TypeLibType(16)]
    internal enum SPVACTIONS
    {
        SPVA_Speak,
        SPVA_Silence,
        SPVA_Pronounce,
        SPVA_Bookmark,
        SPVA_SpellOut,
        SPVA_Section,
        SPVA_ParseUnknownTag
    }
}
