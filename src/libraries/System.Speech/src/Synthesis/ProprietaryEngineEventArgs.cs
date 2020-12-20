//------------------------------------------------------------------
// <copyright file="ProprietaryEngineEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------

#if SPEECHSERVER

using System;

namespace System.Speech.Synthesis
{
    /// <summary>
    /// Events Args for Proprietary synthesisis events
    /// </summary>
    public class ProprietaryEngineEventArgs : PromptEventArgs
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
        /// <param name="id"></param>
        /// <param name="data"></param>
        internal ProprietaryEngineEventArgs (Prompt prompt, int id, IntPtr data) : base (prompt) 
        {
            _id = id;
            _data = data;
        }

        #endregion

        //*******************************************************************
        //
        // Public Properties
        //
        //*******************************************************************

        #region public Properties

        /// <summary>
        /// Event Id
        /// </summary>
        public int EventId
        {
            get
            {
                return _id;
            }
        }

        /// <summary>
        /// Synthesizer event specific data
        /// </summary>
        public IntPtr ProprietaryData
        {
            get
            {
                return _data;
            }
        }

        #endregion

        //*******************************************************************
        //
        // Private Fields
        //
        //*******************************************************************

        #region Private Fields

        private int _id;
        private IntPtr _data;

        #endregion
    }
}
#endif