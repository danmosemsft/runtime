// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Recognition.SrgsGrammar
{
    /// <summary>Enumerates values for the scope of a <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsRule" /> object.</summary>
    public enum SrgsRuleScope
    {
        /// <summary>The rule can be the target of a rule reference from an external grammar, which can use the rule to perform recognition. A public rule can always be activated for recognition.</summary>
        Public,
        /// <summary>The rule cannot be the target of a rule reference from an external grammar unless it is the root rule of its containing grammar.</summary>
        Private
    }
}
