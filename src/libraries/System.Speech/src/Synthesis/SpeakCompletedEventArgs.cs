//------------------------------------------------------------------
// <copyright file="SpeakCompletedEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------

using System;

namespace System.Speech.Synthesis
{
    /// <summary>
    /// TODOC - Summary description for SpeakProgressEventArgs.
    /// </summary>
    public class SpeakCompletedEventArgs : PromptEventArgs
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
        internal SpeakCompletedEventArgs (Prompt prompt) : base (prompt)
        {
        }

        #endregion
    }
}
