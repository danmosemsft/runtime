//---------------------------------------------------------------------------
//
// <copyright file="ObjectTokenCategory.cs" company="Microsoft">
//    Copyright (C) Microsoft Corporation.  All rights reserved.
// </copyright>
//
//
// Description: 
//		Object Token Category
//
// History:
//		7/1/2004	jeanfp		
//---------------------------------------------------------------------------

using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;

#if SERVERTESTDLL
using Microsoft.Speech.Internal.SapiInterop;
#else // SERVERTESTDLL
using System.Speech.Internal.SapiInterop;
#endif // SERVERTESTDLL


namespace System.Speech.Internal.ObjectTokens
{
    /// <summary>
    /// Summary description for ObjectTokenCategory.
    /// </summary>
    internal class ObjectTokenCategory : RegistryDataKey, IEnumerable<ObjectToken>
    {
        //*******************************************************************
        //
        // Constructors
        //
        //*******************************************************************

        #region Constructors

        protected ObjectTokenCategory(string keyId, RegistryDataKey key)
            : base(keyId, key)
        {
        }

        static internal ObjectTokenCategory Create (string sCategoryId)
        {
            RegistryDataKey key = RegistryDataKey.Open(sCategoryId, true);
            return new ObjectTokenCategory(sCategoryId, key);
        }

        #endregion

        //*******************************************************************
        //
        // Internal Methods
        //
        //*******************************************************************

        #region internal Methods

        internal ObjectToken OpenToken (string keyName)
        {
            // Check if the token is for a voice
            string tokenName = keyName;
            if (!string.IsNullOrEmpty (tokenName) && tokenName.IndexOf ("HKEY_", StringComparison.Ordinal) != 0)
            {
                tokenName = string.Format (CultureInfo.InvariantCulture, @"{0}\Tokens\{1}", Id, tokenName);
            }

            return ObjectToken.Open(null, tokenName, false);
        }

#if false
        internal ObjectToken CreateToken (string keyName)
        {
            return new ObjectToken (Id, @"Token\" + keyName, true);
        }

        internal void DeleteToken (string keyName)
        {
            DeleteKey (@"Token\" + keyName);
        }
#endif
        internal IList<ObjectToken> FindMatchingTokens(string requiredAttributes, string optionalAttributes)
        {
            IList<ObjectToken> objectTokenList = new List<ObjectToken>();
            ISpObjectTokenCategory category = null;
            IEnumSpObjectTokens enumTokens = null;

            try
            {
                // Note - enumerated tokens should not be torn down/disposed by us (see SpInitTokenList in spuihelp.h)
                category = (ISpObjectTokenCategory)new SpObjectTokenCategory();
                category.SetId(_sKeyId, false);
                category.EnumTokens(requiredAttributes, optionalAttributes, out enumTokens);

                uint tokenCount;
                enumTokens.GetCount(out tokenCount);
                for (uint index = 0; index < tokenCount; ++index)
                {
                    ISpObjectToken spObjectToken = null;

                    enumTokens.Item(index, out spObjectToken);
                    ObjectToken objectToken = ObjectToken.Open(spObjectToken);
                    objectTokenList.Add(objectToken);
                }
            }
            finally
            {
                if (enumTokens != null)
                {
                    Marshal.ReleaseComObject(enumTokens);
                }
                if (category != null)
                {
                    Marshal.ReleaseComObject(category);
                }
            }

            return objectTokenList;
        }

        #region IEnumerable implementation

        IEnumerator<ObjectToken> IEnumerable<ObjectToken>.GetEnumerator()
        {
            IList<ObjectToken> objectTokenList = FindMatchingTokens(null, null);

            foreach (ObjectToken objectToken in objectTokenList)
            {
                yield return objectToken;
            }
        }

        IEnumerator IEnumerable.GetEnumerator ()
        {
            return ((IEnumerable<ObjectToken>) this).GetEnumerator ();
        }

        #endregion

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
    }
}
