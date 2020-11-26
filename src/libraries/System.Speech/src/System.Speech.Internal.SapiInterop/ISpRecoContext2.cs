// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace System.Speech.Internal.SapiInterop
{
    [ComImport]
    [Guid("BEAD311C-52FF-437f-9464-6B21054CA73D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ISpRecoContext2
    {
        void SetGrammarOptions(SPGRAMMAROPTIONS eGrammarOptions);

        void Slot2();

        void SetAdaptationData2([MarshalAs(UnmanagedType.LPWStr)] string pAdaptationData, uint cch, [MarshalAs(UnmanagedType.LPWStr)] string pTopicName, SPADAPTATIONSETTINGS eSettings, SPADAPTATIONRELEVANCE eRelevance);
    }
}
