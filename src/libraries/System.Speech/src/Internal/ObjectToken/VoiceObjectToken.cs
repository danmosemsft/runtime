#if false
//---------------------------------------------------------------------------
//
// <copyright file="VoiceObjectToken.cs" company="Microsoft">
//    Copyright (C) Microsoft Corporation.  All rights reserved.
// </copyright>
//
//
// Description: 
//		Encapsulation for an Object Token of type voice
//
// History:
//		7/1/2004	jeanfp		
//---------------------------------------------------------------------------

using Microsoft.Win32;
using System;
using System.Diagnostics;

using RegistryEntry = System.Collections.Generic.KeyValuePair<string, object>;

namespace System.Speech.Internal.ObjectTokens
{
    /// <summary>
    /// Summary description for VoiceObjectToken.
    /// </summary>
#if VSCOMPILE
    [DebuggerDisplay ("{Name}")]
#endif
    internal class VoiceObjectToken : ObjectToken
    {
        //*******************************************************************
        //
        // Constructors
        //
        //*******************************************************************

        #region Constructors

        protected VoiceObjectToken (string keyId, RegistryKey hkey)
            : base (keyId, hkey)
        {
        }

        static internal VoiceObjectToken Create (string sCategoryId, string sTokenId)
        {
            string id;
            RegistryKey hkey = ObjectToken.CreateKey (sCategoryId, sTokenId, false, out id);
            if (hkey != null)
            {
                return new VoiceObjectToken (id, hkey);
            }
            return null;
        }

        #endregion


        //*******************************************************************
        //
        // Public Methods
        //
        //*******************************************************************

        #region Public Methods

        /// TODOC
        public override bool Equals (object obj)
        {
            VoiceObjectToken refObj = obj as VoiceObjectToken;
            if (refObj == null)
            {
                return false;
            }

            return Id == refObj.Id;
        }

        /// TODOC 
        public override int GetHashCode ()
        {
            return Id.GetHashCode ();
        }

        #endregion

        //*******************************************************************
        //
        // Internal Properties
        //
        //*******************************************************************

        #region Internal Properties

        /// <summary>
        /// Returns the Age from a voice token
        /// </summary>
        /// <value></value>
        internal string Age
        {
            get
            {
                string age;
                if (Attributes == null || !Attributes.TryGetString ("Age", out age))
                {
                    age = string.Empty;
                }
                return age;
            }
        }

        /// <summary>
        /// Returns the gender
        /// </summary>
        /// <value></value>
        internal string Gender
        {
            get
            {
                string gender;
                if (Attributes == null || !Attributes.TryGetString ("Gender", out gender))
                {
                    gender = string.Empty;
                }
                return gender;
            }
        }

#if SPEECHSERVER

        internal VoiceCategory VoiceCategory
        {
            set
            {
                _category = value;
            }
            get
            {
                return _category;
            }
        }
#endif

        #endregion

        //*******************************************************************
        //
        // Protected Methods
        //
        //*******************************************************************

        #region Protected Methods

        protected override void Dispose (bool disposing)
        {
            base.Dispose (disposing);
        }

        #endregion

        //*******************************************************************
        //
        // Private Fields
        //
        //*******************************************************************

        #region Private Fields

#if SPEECHSERVER

        private VoiceCategory _category = VoiceCategory.Default;

#endif

        #endregion

    }

    //*******************************************************************
    //
    // Private Types
    //
    //*******************************************************************

    #region Private Types

#if SPEECHSERVER

    internal enum VoiceCategory
    {
        Default,
        ScanSoft
    }

#endif

    #endregion

}
#endif