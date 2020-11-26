// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Internal.SapiInterop
{
    [Flags]
    internal enum SPRESULTALPHABET
    {
        SPRA_NONE = 0x0,
        SPRA_APP_UPS = 0x1,
        SPRA_ENGINE_UPS = 0x2
    }
}
