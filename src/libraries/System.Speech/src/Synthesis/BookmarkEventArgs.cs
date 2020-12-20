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

#if SPEECHSERVER
        /// <summary>
        /// TODOC
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="bookmark"></param>
        /// <param name="audioPosition"></param>
        /// <param name="streamPosition"></param>
        internal BookmarkReachedEventArgs (Prompt prompt, string bookmark, TimeSpan audioPosition, long streamPosition)
            : base (prompt)
        {
            _bookmark = bookmark;
            _audioPosition = audioPosition;
            _streamPosition = streamPosition;
        }

#else
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

#endif

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

#if SPEECHSERVER

        /// <summary>
        /// TODOC
        /// </summary>
        /// <value></value>
        internal long StreamPosition
        {
            get
            {
                return _streamPosition;
            }
        }

#endif

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
#if SPEECHSERVER
        private long _streamPosition;
#endif

        #endregion
    }
}
