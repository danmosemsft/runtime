// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#define CODE_ANALYSIS

using System.Speech.Recognition;
using System.Speech.Internal.SrgsParser;
using System.Diagnostics;

namespace System.Speech.Internal.GrammarBuilding
{
    /// <summary>
    ///
    /// </summary>
    internal sealed class GrammarBuilderDictation : GrammarBuilderBase
    {
        #region Constructors

        /// <summary>
        ///
        /// </summary>
        internal GrammarBuilderDictation()
            : this(null)
        {
        }

        /// <summary>
        ///
        /// </summary>
        internal GrammarBuilderDictation(string category)
        {
            _category = category;
        }

        #endregion

        #region Public Methods
        public override bool Equals(object obj)
        {
            GrammarBuilderDictation refObj = obj as GrammarBuilderDictation;
            if (refObj == null)
            {
                return false;
            }
            return _category == refObj._category;
        }
        public override int GetHashCode()
        {
            return _category == null ? 0 : _category.GetHashCode();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        ///
        /// </summary>
        internal override GrammarBuilderBase Clone()
        {
            return new GrammarBuilderDictation(_category);
        }

        /// <summary>
        ///
        /// </summary>
        internal override IElement CreateElement(IElementFactory elementFactory, IElement parent, IRule rule, IdentifierCollection ruleIds)
        {
            // Return the IRuleRef to the dictation grammar
            return CreateRuleRefToDictation(elementFactory, parent);
        }

        #endregion

        #region Internal Properties

        internal override string DebugSummary
        {
            get
            {
                string category = _category != null ? ":" + _category : string.Empty;
                return "dictation" + category;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///
        /// </summary>
        private IRuleRef CreateRuleRefToDictation(IElementFactory elementFactory, IElement parent)
        {
            Uri ruleUri;
            if (!string.IsNullOrEmpty(_category) && _category == "spelling")
            {
                ruleUri = new Uri("grammar:dictation#spelling", UriKind.RelativeOrAbsolute);
            }
            else
            {
                ruleUri = new Uri("grammar:dictation", UriKind.RelativeOrAbsolute);
            }

            return elementFactory.CreateRuleRef(parent, ruleUri, null, null);
        }

        #endregion

        #region Private Fields

        /// <summary>
        ///
        /// </summary>
        private readonly string _category;

        #endregion
    }
}
