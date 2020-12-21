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
    internal sealed class GrammarBuilderRuleRef : GrammarBuilderBase
    {
        #region Constructors

        /// <summary>
        ///
        /// </summary>
        internal GrammarBuilderRuleRef(Uri uri, string rule)
        {
            _uri = uri.OriginalString + ((rule != null) ? "#" + rule : "");
        }

        /// <summary>
        ///
        /// </summary>
        private GrammarBuilderRuleRef(string sgrsUri)
        {
            _uri = sgrsUri;
        }

        #endregion

        #region Public Methods
        public override bool Equals(object obj)
        {
            GrammarBuilderRuleRef refObj = obj as GrammarBuilderRuleRef;
            if (refObj == null)
            {
                return false;
            }
            return _uri == refObj._uri;
        }
        public override int GetHashCode()
        {
            return _uri.GetHashCode();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        ///
        /// </summary>
        internal override GrammarBuilderBase Clone()
        {
            return new GrammarBuilderRuleRef(_uri);
        }

        /// <summary>
        ///
        /// </summary>
        internal override IElement CreateElement(IElementFactory elementFactory, IElement parent, IRule rule, IdentifierCollection ruleIds)
        {
            Uri ruleUri = new(_uri, UriKind.RelativeOrAbsolute);
            return elementFactory.CreateRuleRef(parent, ruleUri, null, null);
        }

        #endregion

        #region Internal Properties

        internal override string DebugSummary
        {
            get
            {
                return "#" + _uri;
            }
        }

        #endregion

        #region Private Fields

        /// <summary>
        ///
        /// </summary>
        private readonly string _uri;

        #endregion
    }
}
