// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Synthesis.TtsEngine
{
    /// <summary>Enumerates the types of data pointers passed to speech synthesis events.</summary>
    public enum EventParameterType
    {
        /// <summary>Indicates that the <paramref name="param2" /> argument to the <see cref="System.Speech.Synthesis.TtsEngine.SpeechEventInfo" /> is undefined.</summary>
        Undefined,
        /// <summary>Indicates that the <paramref name="param2" /> argument to the <see cref="System.Speech.Synthesis.TtsEngine.SpeechEventInfo" /> is a</summary>
        Token,
        /// <summary>Currently not supported.</summary>
        Object,
        /// <summary>Currently not supported.</summary>
        Pointer,
        /// <summary>Indicates that the <paramref name="param2" /> argument to the <see cref="System.Speech.Synthesis.TtsEngine.SpeechEventInfo" /> is a <see langword="System.IntPtr" /> created using <see langword="System.Runtime.InteropServices.Marshal.StringToCoTaskMemUni" /> referencing a <see langword="System.String" /> object; <paramref name="param1" /> may take on any value.</summary>
        String
    }
}
