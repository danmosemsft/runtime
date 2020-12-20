//------------------------------------------------------------------
// <copyright file="VisemeEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------

#if !SPEECHSERVER

namespace System.Speech.Synthesis
{
    /// <summary>
    /// TODOC
    /// </summary>
    public class VisemeReachedEventArgs : PromptEventArgs
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
        /// <param name="speakPrompt"></param>
        /// <param name="currentViseme"></param>
        /// <param name="audioPosition"></param>
        /// <param name="duration"></param>
        /// <param name="emphasis"></param>
        /// <param name="nextViseme"></param>
        internal VisemeReachedEventArgs (Prompt speakPrompt, int currentViseme, TimeSpan audioPosition, TimeSpan duration, SynthesizerEmphasis emphasis, int nextViseme) : base(speakPrompt)
        {
            _currentViseme = currentViseme;
            _audioPosition = audioPosition;
            _duration = duration;
            _emphasis = emphasis;
            _nextViseme = nextViseme;
        }

        #endregion

        //*******************************************************************
        //
        // Public Properties
        //
        //*******************************************************************

        #region Public Properties

        /// <summary>
        /// TODOC
        /// </summary>
        /// <value></value>
        public int Viseme
        {
            get { return _currentViseme; }
        }

        /// <summary>
        /// TODOC
        /// </summary>
        /// <value></value>
        public TimeSpan AudioPosition
        {
            get { return _audioPosition; }
        }

        /// <summary>
        /// TODOC
        /// </summary>
        /// <value></value>
        public TimeSpan Duration
        {
            get { return _duration; }
        }

        /// <summary>
        /// TODOC
        /// </summary>
        /// <value></value>
        public SynthesizerEmphasis Emphasis
        { 
            get { return _emphasis; } 
        }

        /// <summary>
        /// TODOC
        /// </summary>
        /// <value></value>
        public int NextViseme
        {
            get { return _nextViseme; }
        }

        #endregion

        //*******************************************************************
        //
        // Private Fields
        //
        //*******************************************************************

        #region Private Fields

        // Current Viseme being synthesized
        private int _currentViseme;

        // Audio position of current phoneme
        private TimeSpan _audioPosition;

        // Duration of current Viseme
        private TimeSpan _duration;

        // Features of the current phoneme
        private SynthesizerEmphasis _emphasis;

        // Next Viseme to be synthesized
        private int _nextViseme;                     

        #endregion
    }
}

#endif