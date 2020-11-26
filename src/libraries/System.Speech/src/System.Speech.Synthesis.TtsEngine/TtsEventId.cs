// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Synthesis.TtsEngine
{
    /// <summary>Enumerates types of speech synthesis events.</summary>
    public enum TtsEventId
    {
        /// <summary>Identifies events generated when a speech synthesize engine a begins speaking a stream.</summary>
        StartInputStream = 1,
        /// <summary>Identifies events generated when a speech synthesize engine encounters the end of its input stream while speaking.</summary>
        EndInputStream,
        /// <summary>Identifies events generated when a speech synthesize engine encounters a change of Voice while speaking.</summary>
        VoiceChange,
        /// <summary>Identifies events generated when a speech synthesize engine encounters a bookmark while speaking.</summary>
        Bookmark,
        /// <summary>Identifies events generated when a speech synthesize engine completes a word while speaking.</summary>
        WordBoundary,
        /// <summary>Identifies events generated when a speech synthesize engine completes a phoneme while speaking.</summary>
        Phoneme,
        /// <summary>Identifies events generated when a speech synthesize engine completes a sentence while speaking.</summary>
        SentenceBoundary,
        /// <summary>Identifies events generated when a speech synthesize engine completes a viseme while speaking.</summary>
        Viseme,
        /// <summary>Identifies events generated when a speech synthesize engine completes an audio level change while speaking.</summary>
        AudioLevel
    }
}
