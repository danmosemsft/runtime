// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#region Using directives

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Speech.Internal.SrgsParser;
using System.Text;

#endregion

namespace System.Speech.Internal.SrgsCompiler
{
    internal class RuleRef : ParseElement, IRuleRef
    {
        //*******************************************************************
        //
        // Constructors
        //
        //*******************************************************************

        #region Constructors

        /// <summary>
        /// Special private constructor for Special Rulerefs
        /// </summary>
        /// <param name="type"></param>
        /// <param name="rule"></param>
        private RuleRef(SpecialRuleRefType type, Rule rule)
            : base(rule)
        {
            _type = type;
        }

        /// <summary>
        /// Add transition corresponding to Special or Uri.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="backend"></param>
        /// <param name="uri"></param>
        /// <param name="undefRules"></param>
        /// <param name="semanticKey"></param>
        /// <param name="initParameters"></param>
        internal RuleRef(ParseElementCollection parent, Backend backend, Uri uri, List<Rule> undefRules, string semanticKey, string initParameters)
            : base(parent._rule)
        {
            string id = uri.OriginalString;

            Rule ruleRef = null;
            int posPound = id.IndexOf('#');

            // Get the initial state for the RuleRef.
            if (posPound == 0)
            {
                // Internal RuleRef.  Get InitialState of RuleRef.
                // GetRuleRef() may temporarily create a Rule placeholder for later resolution.
                ruleRef = GetRuleRef(backend, id.Substring(1), undefRules);
            }
            else
            {
                // External RuleRef.  Build URL:GrammarUri#RuleName
                StringBuilder sbExternalRuleUri = new StringBuilder("URL:");

                // Add the parameters to initialize a rule
                if (!string.IsNullOrEmpty(initParameters))
                {
                    // look for the # and insert the parameters
                    sbExternalRuleUri.Append(posPound > 0 ? id.Substring(0, posPound) : id);
                    sbExternalRuleUri.Append('>');
                    sbExternalRuleUri.Append(initParameters);
                    if (posPound > 0)
                    {
                        sbExternalRuleUri.Append(id.Substring(posPound));
                    }
                }
                else
                {
                    sbExternalRuleUri.Append(id);
                }

                // Get InitialState of external RuleRef.
                string sExternalRuleUri = sbExternalRuleUri.ToString();
                ruleRef = backend.FindRule(sExternalRuleUri);
                if (ruleRef == null)
                {
                    ruleRef = backend.CreateRule(sExternalRuleUri, SPCFGRULEATTRIBUTES.SPRAF_Import);
                }
            }
            Arc rulerefArc = backend.RuleTransition(ruleRef, _rule, 1.0f);

            if (!string.IsNullOrEmpty(semanticKey))
            {
                CfgGrammar.CfgProperty propertyInfo = new CfgGrammar.CfgProperty();
                propertyInfo._pszName = "SemanticKey";
                propertyInfo._comValue = semanticKey;
                propertyInfo._comType = VarEnum.VT_EMPTY;
                backend.AddPropertyTag(rulerefArc, rulerefArc, propertyInfo);
            }
            parent.AddArc(rulerefArc);
        }

        #endregion

        //*******************************************************************
        //
        // Internal Method
        //
        //*******************************************************************

        #region Internal Method

        /// <summary>
        /// Returns the initial state of a special rule.
        /// For each type of special rule we make a rule with a numeric id and return a reference to it.
        /// </summary>
        /// <param name="backend"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        internal void InitSpecialRuleRef(Backend backend, ParseElementCollection parent)
        {
            Rule rule = null;

            // Create a transition corresponding to Special or Uri
            switch (_type)
            {
                case SpecialRuleRefType.Null:
                    parent.AddArc(backend.EpsilonTransition(1.0f));
                    break;

                case SpecialRuleRefType.Void:
                    rule = backend.FindRule(szSpecialVoid);
                    if (rule == null)
                    {
                        rule = backend.CreateRule(szSpecialVoid, 0);
                        // Rule with no transitions is a void rule.
                        ((IRule)rule).PostParse(parent);
                    }
                    parent.AddArc(backend.RuleTransition(rule, parent._rule, 1.0f));
                    break;

                case SpecialRuleRefType.Garbage:
                    // Garbage transition is optional whereas Wildcard is not.  So we need additional epsilon transition.
                    OneOf oneOf = new OneOf(parent._rule, backend);
                    // Add the garbage transition
                    oneOf.AddArc(backend.RuleTransition(CfgGrammar.SPRULETRANS_WILDCARD, parent._rule, 0.5f));
                    // Add a parallele epsilon path
                    oneOf.AddArc(backend.EpsilonTransition(0.5f));
                    ((IOneOf)oneOf).PostParse(parent);
                    break;

                default:
                    System.Diagnostics.Debug.Assert(false, "Unknown special ruleref type");
                    break;
            }
        }

        #endregion

        //*******************************************************************
        //
        // Private Methods
        //
        //*******************************************************************

        #region Private Methods

        /// <summary>
        /// Return the initial state of the rule with the specified name.
        /// If the rule is not defined yet, create a placeholder Rule.
        /// </summary>
        /// <param name="backend"></param>
        /// <param name="sRuleId">Rule name</param>
        /// <param name="undefRules"></param>
        /// <returns></returns>
        private static Rule GetRuleRef(Backend backend, string sRuleId, List<Rule> undefRules)
        {
            System.Diagnostics.Debug.Assert(!string.IsNullOrEmpty(sRuleId));

            // Get specified rule.
            Rule rule = backend.FindRule(sRuleId);

            if (rule == null)
            {
                // Rule doesn't exist.  Create a placeholder rule and add StateHandle to UndefinedRules.
                rule = backend.CreateRule(sRuleId, 0);
                undefRules.Insert(0, rule);
            }

            return rule;
        }

        #endregion

        //*******************************************************************
        //
        // Public Properties
        //
        //*******************************************************************

        #region internal Properties

        static internal IRuleRef Null
        {
            get
            {
                return new RuleRef(SpecialRuleRefType.Null, null);
            }
        }

        static internal IRuleRef Void
        {
            get
            {
                return new RuleRef(SpecialRuleRefType.Void, null);
            }
        }
        static internal IRuleRef Garbage
        {
            get
            {
                return new RuleRef(SpecialRuleRefType.Garbage, null);
            }
        }

        #endregion

        //*******************************************************************
        //
        // Private Fields
        //
        //*******************************************************************

        #region Private Fields

        //*******************************************************************
        //
        // Private Enums
        //
        //*******************************************************************

        #region Private Enums

        /// TODOC <_include file='doc\SpecialRuleRef.uex' path='docs/doc[@for="SpecialRuleRefType"]/*' />
        // Special rule references allow grammars based on CFGs to have powerful
        // additional features, such as transitions into dictation (both recognized
        // or not recognized) and word seqeuences from SAPI 5.0.
        private enum SpecialRuleRefType
        {
            /// TODOC <_include file='doc\SpecialRuleRef.uex' path='docs/doc[@for="SpecialRuleRefType.Null"]/*' />
            // Defines a rule that is automatically matched that is, matched without
            // the user speaking any word.
            Null,
            /// TODOC <_include file='doc\SpecialRuleRef.uex' path='docs/doc[@for="SpecialRuleRefType.Void"]/*' />
            // Defines a rule that can never be spoken. Inserting VOID into a sequence
            // automatically makes that sequence unspeakable.
            Void,
            /// TODOC <_include file='doc\SpecialRuleRef.uex' path='docs/doc[@for="SpecialRuleRefType.Garbage"]/*' />
            // Defines a rule that may match any speech up until the next rule match,
            // the next token or until the end of spoken input.
            // Designed for applications that would like to recognize some phrases
            // without failing due to irrelevant, or ignorable words.
            Garbage,
        }

        #endregion

        private SpecialRuleRefType _type;

        private const string szSpecialVoid = "VOID";

        #endregion
    }
}