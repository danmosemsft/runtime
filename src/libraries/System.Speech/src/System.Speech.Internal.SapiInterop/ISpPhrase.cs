// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace System.Speech.Internal.SapiInterop
{
    [ComImport]
    [Guid("1A5C0354-B621-4b5a-8791-D306ED379E53")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ISpPhrase
    {
        void GetPhrase(out IntPtr ppCoMemPhrase);

        void GetSerializedPhrase(out IntPtr ppCoMemPhrase);

        void GetText(uint ulStart, uint ulCount, [MarshalAs(UnmanagedType.Bool)] bool fUseTextReplacements, [MarshalAs(UnmanagedType.LPWStr)] out string ppszCoMemText, out byte pbDisplayAttributes);

        void Discard(uint dwValueTypes);
    }
}
