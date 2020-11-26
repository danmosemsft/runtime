// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Internal.SapiInterop
{
    internal enum SPEVENTLPARAMTYPE : ushort
    {
        SPET_LPARAM_IS_UNDEFINED,
        SPET_LPARAM_IS_TOKEN,
        SPET_LPARAM_IS_OBJECT,
        SPET_LPARAM_IS_POINTER,
        SPET_LPARAM_IS_STRING
    }
}