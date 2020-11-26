// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Recognition.SrgsGrammar
{
    /// <summary>Enumerates the supported phonetic alphabets.</summary>
    public enum SrgsPhoneticAlphabet
    {
        /// <summary>Speech API phoneme set.</summary>
        Sapi,
        /// <summary>International Phonetic Alphabet phoneme set.</summary>
        Ipa,
        /// <summary>Universal Phone Set phoneme set, which is ASCII encoding of phonemes for IPA.</summary>
        Ups
    }
}
