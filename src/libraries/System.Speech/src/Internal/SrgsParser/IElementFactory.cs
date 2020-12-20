// <copyright file="IElementFactory.cs" company="Microsoft">
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
//      Interface definition for the IElementFactory
// History:
//		6/1/2004	jeanfp		Created
//---------------------------------------------------------------------------

using System;

namespace System.Speech.Internal.SrgsParser
{
    /// <summary>
    /// Interface definition for the IElementFactory
    /// </summary>
    internal interface IElementFactory
    {
        // Grammar
        void RemoveAllRules ();

        IElementText CreateText (IElement parent, string value);
        IToken CreateToken (IElement parent, string content, string pronumciation, string display, float reqConfidence);
        IPropertyTag CreatePropertyTag (IElement parent);
        ISemanticTag CreateSemanticTag (IElement parent);
        IItem CreateItem (IElement parent, IRule rule, int minRepeat, int maxRepeat, float repeatProbability, float weight);
        IRuleRef CreateRuleRef (IElement parent, Uri srgsUri);
        IRuleRef CreateRuleRef (IElement parent, Uri srgsUri, string semanticKey, string parameters);
        void InitSpecialRuleRef (IElement parent, IRuleRef special);
        IOneOf CreateOneOf (IElement parent, IRule rule);
        ISubset CreateSubset (IElement parent, string text, MatchMode matchMode);

        IGrammar Grammar { get; }

        IRuleRef Null { get; }
        IRuleRef Void { get; }
        IRuleRef Garbage { get; }

#if !NO_STG
        string AddScript (IGrammar grammar, string rule, string code, string filename, int line);
        void AddScript (IGrammar grammar, string script, string filename, int line);
        void AddScript (IGrammar grammar, string rule, string code);
#endif

        void AddItem (IOneOf oneOf, IItem value);
        void AddElement (IRule rule, IElement value);
        void AddElement (IItem item, IElement value);
    }
}
