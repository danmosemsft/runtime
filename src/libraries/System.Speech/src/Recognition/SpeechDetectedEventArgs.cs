// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace System.Speech.Recognition
{
    /// TODOC <_include file='doc\RecognizerBase.uex' path='docs/doc[@for="SpeechDetectedEventArgs"]/*' />
    // EventArgs used in the SpeechDetected event.

    public class SpeechDetectedEventArgs : EventArgs
    {
        #region Constructors

        internal SpeechDetectedEventArgs(TimeSpan audioPosition)
        {
            _audioPosition = audioPosition;
        }

        #endregion




        #region public Properties

        /// TODOC <_include file='doc\RecognizerBase.uex' path='docs/doc[@for="SpeechDetectedEventArgs.AudioPosition"]/*' />
        public TimeSpan AudioPosition
        {
            get { return _audioPosition; }
        }

        #endregion




        #region Private Fields

        private TimeSpan _audioPosition;

        #endregion
    }
}
