// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Synthesis.TtsEngine
{
    /// <summary>Enumerates values for lengths of <see langword="EmphasisBreak" /> between spoken words.</summary>
    public enum EmphasisBreak
    {
        /// <summary>No word break.</summary>
        None = -1,
        /// <summary>Very small word break.</summary>
        ExtraWeak = -2,
        /// <summary>Small word break.</summary>
        Weak = -3,
        /// <summary>Moderate word break.</summary>
        Medium = -4,
        /// <summary>Long word break.</summary>
        Strong = -5,
        /// <summary>Longest word break.</summary>
        ExtraStrong = -6,
        /// <summary>Normal word break.</summary>
        Default = -7
    }
}
