// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Internal.SrgsParser
{
    internal interface IRule : IElement
    {
        string BaseClass { get; set; }

        void CreateScript(IGrammar grammar, string rule, string method, RuleMethodScript type);
    }

    #region Internal Enums
    // RuleScope specifies how a rule behaves with respect to being able to be
    // referenced by other rules, and whether or not the rule can be activated
    // or not.
    internal enum RuleDynamic
    {
        True,
        False,
        NotSet
    };
    // RuleScope specifies how a rule behaves with respect to being able to be
    // referenced by other rules, and whether or not the rule can be activated
    // or not.
    internal enum RulePublic
    {
        True,
        False,
        NotSet
    };

    #endregion
}
