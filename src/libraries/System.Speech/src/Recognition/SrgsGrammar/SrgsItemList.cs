//---------------------------------------------------------------------------
//
// <copyright file="SrgsItemList.cs" company="Microsoft">
//    Copyright (C) Microsoft Corporation.  All rights reserved.
// </copyright>
//
//
// Description: 
//      Srgs Item only Element List. Derived from SrgsItemList
//
// History:
//		5/1/2004	jeanfp		Created from the Kurosawa Code
//---------------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.Speech.Internal;

namespace System.Speech.Recognition.SrgsGrammar
{
    /// <summary>
    /// Summary description for SrgsItemList.
    /// </summary>
    [Serializable]
    internal class SrgsItemList : Collection<SrgsItem>
    {
        //*******************************************************************
        //
        // Interfaces Implementations
        //
        //*******************************************************************

        #region Interfaces Implementations

        protected override void InsertItem (int index, SrgsItem item)
        {
            Helpers.ThrowIfNull (item, "item");

            base.InsertItem (index, item);
        }

        #endregion
    }
}
