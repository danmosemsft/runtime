// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace System.Speech.Synthesis.TtsEngine
{
    internal struct ProsodyInterop
    {
        internal ProsodyNumber _pitch;

        internal ProsodyNumber _range;

        internal ProsodyNumber _rate;

        internal int _duration;

        internal ProsodyNumber _volume;

        internal IntPtr _contourPoints;
    }
}
