using System;

namespace System.Speech.Recognition
{

    /// TODOC <_include file='doc\AudioStatusChangedEventArgs.uex' path='docs/doc[@for="AudioStatusChangedEventArgs"]/*' />
    // EventArgs used in the AudioStateChangedEventArgs event.
    
    public class AudioStateChangedEventArgs : EventArgs
    {
        //*******************************************************************
        //
        // Constructors
        //
        //*******************************************************************

        #region Constructors

        internal AudioStateChangedEventArgs(AudioState audioState)
        {
            _audioState = audioState;
        }

        #endregion



        //*******************************************************************
        //
        // Public Properties
        //
        //*******************************************************************

        #region public Properties

        /// TODOC <_include file='doc\AudioStatusChangedEventArgs.uex' path='docs/doc[@for="AudioStatusChangedEventArgs.AudioStatus"]/*' />
        public AudioState AudioState
        {
            get { return _audioState; }
        }

        #endregion



        //*******************************************************************
        //
        // Private Fields
        //
        //*******************************************************************

        #region Private Fields

        private AudioState _audioState;

        #endregion
    }

}
