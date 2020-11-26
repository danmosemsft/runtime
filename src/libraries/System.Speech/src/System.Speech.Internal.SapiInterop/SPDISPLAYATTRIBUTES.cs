// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Internal.SapiInterop
{
    [Flags]
    internal enum SPDISPLAYATTRIBUTES
    {
        SPAF_ZERO_TRAILING_SPACE = 0x0,
        SPAF_ONE_TRAILING_SPACE = 0x2,
        SPAF_TWO_TRAILING_SPACES = 0x4,
        SPAF_CONSUME_LEADING_SPACES = 0x8,
        SPAF_USER_SPECIFIED = 0x80
    }
}
