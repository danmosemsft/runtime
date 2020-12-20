// <copyright file="IOneOf.cs" company="Microsoft">
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
//      Interface definition for the IElement
// History:
//		6/1/2004	jeanfp		Created
//---------------------------------------------------------------------------using System;


namespace System.Speech.Internal.SrgsParser
{
    /// <summary>
    /// Interface definition for the IOneOf
    /// </summary>
    internal interface IOneOf : IElement
    {
    }
}
