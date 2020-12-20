

using System;

namespace System.Speech.Recognition
{

    /// TODOC <_include file='doc\RecognizerBase.uex' path='docs/doc[@for="SpeechDetectedEventArgs"]/*' />
    // EventArgs used in the SpeechDetected event.
    
    public class SpeechDetectedEventArgs : EventArgs
    {
        //*******************************************************************
        //
        // Constructors
        //
        //*******************************************************************

        #region Constructors

        internal SpeechDetectedEventArgs(TimeSpan audioPosition)
        {
            _audioPosition = audioPosition;
        }

        #endregion



        //*******************************************************************
        //
        // Public Properties
        //
        //*******************************************************************

        #region public Properties

        /// TODOC <_include file='doc\RecognizerBase.uex' path='docs/doc[@for="SpeechDetectedEventArgs.AudioPosition"]/*' />
        public TimeSpan AudioPosition
        {
            get { return _audioPosition; }
        }

        #endregion



        //*******************************************************************
        //
        // Private Fields
        //
        //*******************************************************************

        #region Private Fields

        private TimeSpan _audioPosition;

        #endregion

    }

}
