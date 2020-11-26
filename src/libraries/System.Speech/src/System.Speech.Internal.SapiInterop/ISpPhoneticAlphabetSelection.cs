// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace System.Speech.Internal.SapiInterop
{
    [ComImport]
    [Guid("B2745EFD-42CE-48CA-81F1-A96E02538A90")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ISpPhoneticAlphabetSelection
    {
        void IsAlphabetUPS([MarshalAs(UnmanagedType.Bool)] out bool pfIsUPS);

        void SetAlphabetToUPS([MarshalAs(UnmanagedType.Bool)] bool fForceUPS);
    }
}
