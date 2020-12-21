// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;
using System.Speech.Recognition;
using System.Speech.Internal.SrgsParser;
using System.Text;

namespace System.Speech.Internal.GrammarBuilding
{
    /// <summary>
    ///
    /// </summary>
    [DebuggerDisplay("{DebugSummary}")]
    internal sealed class RuleRefElement : GrammarBuilderBase
    {
        #region Constructors

        /// <summary>
        ///
        /// </summary>
        internal RuleRefElement(RuleElement rule)
        {
            _rule = rule;
        }

        /// <summary>
        ///
        /// </summary>
        internal RuleRefElement(RuleElement rule, string semanticKey)
        {
            _rule = rule;
            _semanticKey = semanticKey;
        }

        #endregion

        #region Public Methods
        public override bool Equals(object obj)
        {
            RuleRefElement refObj = obj as RuleRefElement;
            if (refObj == null)
            {
                return false;
            }
            return _semanticKey == refObj._semanticKey && _rule.Equals(refObj._rule);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        ///
        /// </summary>
        internal void Add(GrammarBuilderBase item)
        {
            _rule.Add(item);
        }

        /// <summary>
        ///
        /// </summary>
        internal override GrammarBuilderBase Clone()
        {
            return new RuleRefElement(_rule, _semanticKey);
        }

        /// <summary>
        ///
        /// </summary>
        internal void CloneItems(RuleRefElement builders)
        {
            _rule.CloneItems(builders._rule);
        }

        /// <summary>
        ///
        /// </summary>
        internal override IElement CreateElement(IElementFactory elementFactory, IElement parent, IRule rule, IdentifierCollection ruleIds)
        {
            // Create the new rule and add the reference to the item
            return elementFactory.CreateRuleRef(parent, new Uri("#" + Rule.RuleName, UriKind.Relative), _semanticKey, null);
        }

        #endregion

        #region Internal Properties

        internal RuleElement Rule
        {
            get
            {
                return _rule;
            }
        }

        internal override string DebugSummary
        {
            get
            {
                return "#" + Rule.Name + (_semanticKey != null ? ":" + _semanticKey : "");
            }
        }

        #endregion

        #region Private Fields

        /// <summary>
        ///
        /// </summary>
        private readonly RuleElement _rule;
        private readonly string _semanticKey;

        #endregion
    }
}
