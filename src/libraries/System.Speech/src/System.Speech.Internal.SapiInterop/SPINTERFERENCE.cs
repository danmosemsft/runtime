// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Internal.SapiInterop
{
    internal enum SPINTERFERENCE
    {
        SPINTERFERENCE_NONE,
        SPINTERFERENCE_NOISE,
        SPINTERFERENCE_NOSIGNAL,
        SPINTERFERENCE_TOOLOUD,
        SPINTERFERENCE_TOOQUIET,
        SPINTERFERENCE_TOOFAST,
        SPINTERFERENCE_TOOSLOW
    }
}
