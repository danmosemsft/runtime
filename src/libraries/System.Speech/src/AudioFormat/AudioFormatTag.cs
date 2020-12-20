//------------------------------------------------------------------
// <copyright file="AudioFormatTag.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;

namespace System.Speech.AudioFormat
{

    /// TODOC <_include file='doc\AudioFormat.uex' path='docs/doc[@for="AudioFormatTag"]/*' />
    // These enumeration values are the same values used in the WAVEFORMATEX structure used in wave files.
    public enum AudioFormatTag
    {
        /// TODOC <_include file='doc\AudioFormat.uex' path='docs/doc[@for="AudioFormatTag.PCM"]/*' />
        Pcm = 0x0001,

        /// TODOC <_include file='doc\AudioFormat.uex' path='docs/doc[@for="AudioFormatTag.ALaw"]/*' />
        ALaw = 0x0006,

        /// TODOC <_include file='doc\AudioFormat.uex' path='docs/doc[@for="AudioFormatTag.ULaw"]/*' />
        ULaw = 0x0007,

#if SPEECHSERVER
        /// TODOC <_include file='doc\AudioFormat.uex' path='docs/doc[@for="AudioFormatTag.Custom"]/*' />
        Custom = 0x0000, // Used only when a Guid is specified as the format.

#endif
    }

}
