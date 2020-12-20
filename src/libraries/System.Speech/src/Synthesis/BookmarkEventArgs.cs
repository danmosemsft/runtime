//------------------------------------------------------------------
// <copyright file="BookmarkEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------

using System;

namespace System.Speech.Synthesis
{
    /// <summary>
    /// TODOC - Summary description for BookmarkEventArgs.
    /// </summary>
    public class BookmarkReachedEventArgs : PromptEventArgs
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
        /// <param name="bookmark"></param>
        /// <param name="audioPosition"></param>
        internal BookmarkReachedEventArgs (Prompt prompt, string bookmark, TimeSpan audioPosition)
            : base (prompt)
        {
            _bookmark = bookmark;
            _audioPosition = audioPosition;
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
        public string Bookmark
        {
            get
            {
                return _bookmark;
            }
        }

        /// <summary>
        /// TODOC
        /// </summary>
        /// <value></value>
        public TimeSpan AudioPosition
        {
            get
            {
                return _audioPosition;
            }
        }


        #endregion

        //*******************************************************************
        //
        // Private Fields
        //
        //*******************************************************************

        #region Private Fields

        string _bookmark;

        // Audio and stream position
        private TimeSpan _audioPosition;

        #endregion
    }
}
