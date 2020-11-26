// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Recognition.SrgsGrammar
{
    /// <summary>Indicates the type of input that the grammar, defined by the <see cref="System.Speech.Recognition.SrgsGrammar.SrgsDocument" />, will match.</summary>
    public enum SrgsGrammarMode
    {
        /// <summary>The <see cref="System.Speech.Recognition.SrgsGrammar.SrgsDocument" /> object will match speech input.</summary>
        Voice,
        /// <summary>The <see cref="System.Speech.Recognition.SrgsGrammar.SrgsDocument" /> object will match DTMF tones similar to those found on a telephone, instead of speech.</summary>
        Dtmf
    }
}
