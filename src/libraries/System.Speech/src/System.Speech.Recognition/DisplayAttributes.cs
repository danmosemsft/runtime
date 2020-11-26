// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Recognition
{
    /// <summary>Lists the options that the <see cref="System.Speech.Recognition.SpeechRecognitionEngine" /> object can use to specify white space for the display of a word or punctuation mark.</summary>
    [Flags]
    public enum DisplayAttributes
    {
        /// <summary>The item does not specify how white space is handled.</summary>
        None = 0x0,
        /// <summary>The item has no spaces following it.</summary>
        ZeroTrailingSpaces = 0x2,
        /// <summary>The item has one space following it.</summary>
        OneTrailingSpace = 0x4,
        /// <summary>The item has two spaces following it.</summary>
        TwoTrailingSpaces = 0x8,
        /// <summary>The item has no spaces preceding it.</summary>
        ConsumeLeadingSpaces = 0x10
    }
}
