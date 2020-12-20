// <copyright file="IGrammar.cs" company="Microsoft">
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
//      Interface definition for the IGrammar
// History:
//		6/1/2004	jeanfp		Created
//---------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace System.Speech.Internal.SrgsParser
{
    /// <summary>
    /// Interface definition for the IGrammar
    /// </summary>
    internal interface IGrammar : IElement
    {
        IRule CreateRule (string id, RulePublic publicRule, RuleDynamic dynamic, bool hasSCript);

        string Root { set; get; }
        System.Speech.Recognition.SrgsGrammar.SrgsTagFormat TagFormat { set; get; }
        Collection<string> GlobalTags { set; get; }
        GrammarType Mode { set; }
        CultureInfo Culture { set; }
        Uri XmlBase { set; }
        AlphabetType PhoneticAlphabet { set; }

#if !NO_STG
        string Language { set; get; }
        string Namespace { set; get; }
        bool Debug { set; }
        Collection<string> CodeBehind { get; set; }
        Collection<string> ImportNamespaces { get; set; }
        Collection<string> AssemblyReferences { get; set; }
#endif
    }

    internal enum GrammarType
    {
        VoiceGrammar, DtmfGrammar
    }
}
