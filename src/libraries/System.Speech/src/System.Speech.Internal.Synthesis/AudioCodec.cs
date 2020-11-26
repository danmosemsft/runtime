// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Internal.Synthesis
{
    internal enum AudioCodec
    {
        PCM16 = 0x80,
        PCM8 = 0x7F,
        G711U = 0,
        G711A = 8,
        Undefined = -1
    }
}
