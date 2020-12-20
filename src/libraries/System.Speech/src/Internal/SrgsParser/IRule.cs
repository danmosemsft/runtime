// <copyright file="IRule.cs" company="Microsoft">
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
//      Interface definition for the IRule
// History:
//		6/1/2004	jeanfp		Created
//---------------------------------------------------------------------------using System;

namespace System.Speech.Internal.SrgsParser
{
    internal interface IRule : IElement
    {
#if !NO_STG
        string BaseClass { set; get; }

        void CreateScript (IGrammar grammar, string rule, string method, RuleMethodScript type);
#endif
    }

    //*******************************************************************
    //
    // Internal Enums
    //
    //*******************************************************************

    #region Internal Enums

    /// TODOC <_include file='doc\Rule.uex' path='docs/doc[@for="RuleScope"]/*' />
    // RuleScope specifies how a rule behaves with respect to being able to be
    // referenced by other rules, and whether or not the rule can be activated
    // or not.
    internal enum RuleDynamic
    {
        /// TODOC <_include file='doc\Rule.uex' path='docs/doc[@for="RuleScope.Public"]/*' />
        True,
        /// TODOC <_include file='doc\Rule.uex' path='docs/doc[@for="RuleScope.Private"]/*' />
        False,
        //TODOC
        NotSet
    };

    /// TODOC <_include file='doc\Rule.uex' path='docs/doc[@for="RuleScope"]/*' />
    // RuleScope specifies how a rule behaves with respect to being able to be
    // referenced by other rules, and whether or not the rule can be activated
    // or not.
    internal enum RulePublic
    {
        /// TODOC <_include file='doc\Rule.uex' path='docs/doc[@for="RuleScope.Public"]/*' />
        True,
        /// TODOC <_include file='doc\Rule.uex' path='docs/doc[@for="RuleScope.Private"]/*' />
        False,
        //TODOC
        NotSet
    };

    #endregion

}
