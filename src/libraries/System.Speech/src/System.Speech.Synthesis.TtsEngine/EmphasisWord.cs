// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Synthesis.TtsEngine
{
    /// <summary>Enumerates the values of <see langword="EmphasisWord" /> for a specific <see langword="TextFragment" />.</summary>
    public enum EmphasisWord
    {
        /// <summary>Indicates an engine-specific default level of emphasis.</summary>
        Default,
        /// <summary>Indicates strong emphasis.</summary>
        Strong,
        /// <summary>Indicates moderate emphasis.</summary>
        Moderate,
        /// <summary>Indicates no emphasis specified.</summary>
        None,
        /// <summary>Indicates reduced emphasis.</summary>
        Reduced
    }
}
