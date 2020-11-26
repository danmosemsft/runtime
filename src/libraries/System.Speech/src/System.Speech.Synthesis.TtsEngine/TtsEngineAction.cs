// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Synthesis.TtsEngine
{
    /// <summary>Specifies the Speech Synthesis Markup Language (SSML) action to be taken in rendering a given <see langword="TextFragment" />.</summary>
    public enum TtsEngineAction
    {
        /// <summary>Requests that the associated <see cref="T:System.Speech.Synthesis.TtsEngine.TextFragment" /> should be processed and spoken.</summary>
        Speak,
        /// <summary>Indicates that a <see cref="T:System.Speech.Synthesis.TtsEngine.TextFragment" /> contains no text to be rendered as speech.</summary>
        Silence,
        /// <summary>Requests that input <see cref="T:System.Speech.Synthesis.TtsEngine.TextFragment" /> text be interpreted as phonemes.</summary>
        Pronounce,
        /// <summary>Indicates that <see cref="T:System.Speech.Synthesis.TtsEngine.TextFragment" /> is to be used as the contents of a bookmark.</summary>
        Bookmark,
        /// <summary>Indicates that text values provided by a <see cref="T:System.Speech.Synthesis.TtsEngine.TextFragment" /> through its <see cref="P:System.Speech.Synthesis.TtsEngine.TextFragment.TextToSpeak" /> property are to be synthesize as individual characters.</summary>
        SpellOut,
        /// <summary>Indicates start of sentence.</summary>
        StartSentence,
        /// <summary>Indicates state of paragraph.</summary>
        StartParagraph,
        /// <summary>Indicates that no action has been determined from SSML input.</summary>
        ParseUnknownTag
    }
}
