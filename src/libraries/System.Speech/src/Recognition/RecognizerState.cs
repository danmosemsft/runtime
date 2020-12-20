//------------------------------------------------------------------
// <copyright file="RecognizerState.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------

using System;


namespace System.Speech.Recognition
{

    // Current recognizer state.
    /// TODOC <_include file='doc\RecognizerState.uex' path='docs/doc[@for="RecognizerState"]/*' />
    public enum RecognizerState
    {
        // The recognizer is currently stopped and not listening.
        /// TODOC <_include file='doc\RecognizerState.uex' path='docs/doc[@for="RecognizerState.Stopped"]/*' />
        Stopped,

        // The recognizer is currently listening.
        /// TODOC <_include file='doc\RecognizerState.uex' path='docs/doc[@for="RecognizerState.Listening"]/*' />
        Listening
    }

}


