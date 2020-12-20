// <copyright file="ISubset.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// Description: 
//      The Srgs xml parser parse takes as input an xml reader and the IElementFactory
//      interface. With this virtualization scheme, the srgs parser do not create 
//      directly Srgs elements. Instead methods on a set of interfaces for each 
//      Srgs Element creates the element themselve. The underlying implementation
//      for each elements is different for the SrgsDocument class and the Srgs
//      Compiler.
//
//      Interface definition for the ISubset element
// History:
//		6/14/2005	jeanfp		Created
//---------------------------------------------------------------------------using System;


namespace System.Speech.Internal.SrgsParser
{
    /// <summary>
    /// Interface definition for the ISubset
    /// </summary>
    internal interface ISubset : IElement
    {
    }

    // Must be in the same order as the Srgs enum
    internal enum MatchMode
    {
        AllWords = 0,
        Subsequence = 1,
        OrderedSubset = 3,
        SubsequenceContentRequired = 5,
        OrderedSubsetContentRequired = 7 
    }
}
