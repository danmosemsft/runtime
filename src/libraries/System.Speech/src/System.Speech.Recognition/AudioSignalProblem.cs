// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Recognition
{
    /// <summary>Contains a list of possible problems in the audio signal coming in to a speech recognition engine.</summary>
    public enum AudioSignalProblem
    {
        /// <summary>No problems with audio input.</summary>
        None,
        /// <summary>Audio input has too much background noise.</summary>
        TooNoisy,
        /// <summary>Audio input is not detected.</summary>
        NoSignal,
        /// <summary>Audio input is too loud.</summary>
        TooLoud,
        /// <summary>Audio input is too quiet.</summary>
        TooSoft,
        /// <summary>Audio input is too fast.</summary>
        TooFast,
        /// <summary>Audio input is too slow.</summary>
        TooSlow
    }
}
