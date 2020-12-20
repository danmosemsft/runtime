//------------------------------------------------------------------
// <copyright file="AudioException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace System.Speech.Internal.Synthesis
{
    /// <summary>
    /// TODOC
    /// </summary>
    [Serializable]
    internal class AudioException : Exception
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
        internal AudioException ()
        {
        }

        /// <summary>
        /// TODOC
        /// </summary>
        internal AudioException (MMSYSERR errorCode) : base (String.Format (System.Globalization.CultureInfo.InvariantCulture, "{0} - Error Code: 0x{1:x}", SR.Get (SRID.AudioDeviceError), (int)errorCode))
        {
        }

        /// <summary>
        /// TODOC
        /// </summary>
        protected AudioException (SerializationInfo info, StreamingContext context) : base (info, context)
        {
        }

        #endregion
    }
}
