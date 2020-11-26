// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Internal.Synthesis
{
    internal interface ITtsEventSink
    {
        void AddEvent(TTSEvent evt);

        void FlushEvent();
    }
}
