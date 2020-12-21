// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Speech.Recognition;
using System.Speech.Internal.SrgsParser;
using System.Text;

namespace System.Speech.Internal.GrammarBuilding
{
    /// <summary>
    ///
    /// </summary>
    internal sealed class RuleElement : BuilderElements
    {
        #region Constructors

        /// <summary>
        ///
        /// </summary>
        internal RuleElement(string name)
        {
            _name = name;
        }

        /// <summary>
        ///
        /// </summary>
        internal RuleElement(GrammarBuilderBase builder, string name)
            : this(name)
        {
            Add(builder);
        }

        #endregion

        #region Public Methods
        public override bool Equals(object obj)
        {
            RuleElement refObj = obj as RuleElement;
            if (refObj == null)
            {
                return false;
            }
            if (!base.Equals(obj))
            {
                return false;
            }
            return _name == refObj._name;
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
        internal override GrammarBuilderBase Clone()
        {
            RuleElement rule = new(_name);
            rule.CloneItems(this);
            return rule;
        }

        /// <summary>
        ///
        /// </summary>
        internal override IElement CreateElement(IElementFactory elementFactory, IElement parent, IRule rule, IdentifierCollection ruleIds)
        {
            if (_rule == null)
            {
                IGrammar grammar = elementFactory.Grammar;

                // Create the rule
                _ruleName = ruleIds.CreateNewIdentifier(Name);

                _rule = grammar.CreateRule(_ruleName, RulePublic.False, RuleDynamic.NotSet, false);

                // Create the children elements
                CreateChildrenElements(elementFactory, _rule, ruleIds);

                _rule.PostParse(grammar);
            }
            return _rule;
        }

        /// <summary>
        ///
        /// </summary>
        internal override int CalcCount(BuilderElements parent)
        {
            // clear any existing value
            _rule = null;
            return base.CalcCount(parent);
        }

        #endregion

        #region Internal Properties

        internal override string DebugSummary
        {
            get
            {
                return _name + "=" + base.DebugSummary;
            }
        }

        internal string Name
        {
            get
            {
                return _name;
            }
        }

        internal string RuleName
        {
            get
            {
                return _ruleName;
            }
        }

        #endregion

        #region Private Fields

        /// <summary>
        ///
        /// </summary>
        private readonly string _name;
        private string _ruleName;
        private IRule _rule;

        #endregion
    }
}
