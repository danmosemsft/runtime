// <copyright file="ISrgsParser.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// Description: 
//      The SrgsCompiler takes as input an ISrgsParser. Two classes
//      implements it, the XML parser or the SrgsDocument. 
//
// History:
//		2/1/2005	jeanfp		Created
//---------------------------------------------------------------------------

namespace System.Speech.Internal.SrgsParser
{
    internal interface ISrgsParser
    {
        void Parse ();
        IElementFactory ElementFactory { set; }
    }
}
