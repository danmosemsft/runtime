//---------------------------------------------------------------------------
//
// <copyright file="SrgsElementList.cs" company="Microsoft">
//    Copyright (C) Microsoft Corporation.  All rights reserved.
// </copyright>
//
//
// Description: 
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
    /// Summary description for SrgsElementList.
    /// </summary>
    [Serializable]
    internal class SrgsElementList : Collection<SrgsElement>
    {
        //*******************************************************************
        //
        // Interfaces Implementations
        //
        //*******************************************************************

        #region Interfaces Implementations

        protected override void InsertItem (int index, SrgsElement element)
        {
            Helpers.ThrowIfNull (element, "element");

            base.InsertItem (index, element);
        }

        #endregion
    }
}
