//---------------------------------------------------------------------------
//
// <copyright file="SrgsRulesCollection.cs" company="Microsoft">
//    Copyright (C) Microsoft Corporation.  All rights reserved.
// </copyright>
//
//
// Description: 
//
// History:
//		5/1/2004	jeanfp		
//---------------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.Speech.Internal;

namespace System.Speech.Recognition.SrgsGrammar
{
    /// <summary>
    /// Summary description for Rules.
    /// </summary>
    [Serializable]
    
    public sealed class SrgsRulesCollection : KeyedCollection<string, SrgsRule>
    {
        /// <summary>
        /// TODOC
        /// </summary>
        /// <param name="rules"></param>
        public void Add (params SrgsRule [] rules)
        {
            Helpers.ThrowIfNull (rules, "rules");

            for (int iRule = 0; iRule < rules.Length; iRule++)
            {
                if (rules [iRule] == null)
                {
                    throw new ArgumentNullException ("rules", SR.Get (SRID.ParamsEntryNullIllegal));
                }
                base.Add (rules [iRule]);
            }
        }

        /// <summary>
        /// TODOC
        /// </summary>
        /// <param name="rule"></param>
        /// <returns></returns>
        protected override string GetKeyForItem (SrgsRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException ("rule");
            }
            return rule.Id;
        }
    }
}
