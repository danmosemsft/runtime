
using System;

namespace System.Speech.Synthesis
{
    /// <summary>
    /// TODOC - Summary description for VoiceChangeEventArgs.
    /// </summary>
    public class VoiceChangeEventArgs : PromptEventArgs
    {
        //*******************************************************************
        //
        // Constructors
        //
        //*******************************************************************

        #region Constructors

        /// <summary>
        /// TODOC
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="voice"></param>
        internal VoiceChangeEventArgs(Prompt prompt, VoiceInfo voice) : base (prompt)
        {
            _voice = voice;
        }

        #endregion

        //*******************************************************************
        //
        // Public Properties
        //
        //*******************************************************************

        #region public Properties

        /// <summary>
        /// TODOC
        /// </summary>
        /// <value></value>
        public VoiceInfo Voice
        {
            get
            {
                return _voice;
            }
        }

        #endregion

        //*******************************************************************
        //
        // Private Fields
        //
        //*******************************************************************

        #region Private Fields

        VoiceInfo _voice;

        #endregion
    }
}
