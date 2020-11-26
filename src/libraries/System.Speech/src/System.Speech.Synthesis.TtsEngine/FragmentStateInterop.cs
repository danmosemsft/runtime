// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace System.Speech.Synthesis.TtsEngine
{
    internal struct FragmentStateInterop
    {
        internal TtsEngineAction _action;

        internal int _langId;

        internal int _emphasis;

        internal int _duration;

        internal IntPtr _sayAs;

        internal IntPtr _prosody;

        internal IntPtr _phoneme;
    }
}
