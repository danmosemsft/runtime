

using System;

namespace System.Speech.Recognition
{

    /// TODOC <_include file='doc\RecognizerStateChangedEventArgs.uex' path='docs/doc[@for="RecognizerStateChangedEventArgs"]/*' />
    // EventArgs used in the SpeechRecognizer.StateChanged event.
    
    public class StateChangedEventArgs : EventArgs
    {
        //*******************************************************************
        //
        // Constructors
        //
        //*******************************************************************

        #region Constructors

        internal StateChangedEventArgs(RecognizerState recognizerState)
        {
            _recognizerState = recognizerState;
        }

        #endregion



        //*******************************************************************
        //
        // Public Properties
        //
        //*******************************************************************

        #region public Properties

        /// TODOC <_include file='doc\RecognizerStateChangedEventArgs.uex' path='docs/doc[@for="RecognizerStateChangedEventArgs.RecognizerState"]/*' />
        public RecognizerState RecognizerState
        {
            get { return _recognizerState; }
        }

        #endregion



        //*******************************************************************
        //
        // Private Fields
        //
        //*******************************************************************

        #region Private Fields

        private RecognizerState _recognizerState;

        #endregion

    }

}
