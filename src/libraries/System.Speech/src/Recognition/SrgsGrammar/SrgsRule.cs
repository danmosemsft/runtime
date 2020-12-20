//---------------------------------------------------------------------------
//
// <copyright file="SrgsRule.cs" company="Microsoft">
//    Copyright (C) Microsoft Corporation.  All rights reserved.
// </copyright>
//
//
// Description: 
//
// History:
//		5/1/2004	jeanfp		Created from the Kurosawa Code
//---------------------------------------------------------------------------

using System;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Speech.Internal;
using System.Speech.Internal.SrgsParser;

#pragma warning disable 1634, 1691 // Allows suppression of certain PreSharp messages.

namespace System.Speech.Recognition.SrgsGrammar
{
    /// TODOC <_include file='doc\Rule.uex' path='docs/doc[@for="Rule"]/*' />
    [Serializable]
    [DebuggerDisplay ("Rule={_id.ToString()} Scope={_scope.ToString()}")]
    [DebuggerTypeProxy (typeof (SrgsRuleDebugDisplay))]
    public class SrgsRule : IRule
    {
        //*******************************************************************
        //
        // Constructors
        //
        //*******************************************************************

        #region Constructors

        /// TODOC <_include file='doc\Rule.uex' path='docs/doc[@for="Rule.Rule1"]/*' />
        private SrgsRule ()
        {
            _elements = new SrgsElementList ();
        }

        /// TODOC <_include file='doc\Rule.uex' path='docs/doc[@for="Rule.Rule2"]/*' />
        public SrgsRule (string id)
            : this ()
        {
            XmlParser.ValidateRuleId (id);
            Id = id;
        }

        /// TODOC <_include file='doc\Rule.uex' path='docs/doc[@for="Rule.Rule2"]/*' />
        public SrgsRule (string id, params SrgsElement [] elements)
            : this ()
        {
            Helpers.ThrowIfNull (elements, "elements");

            XmlParser.ValidateRuleId (id);
            Id = id;

            for (int iElement = 0; iElement < elements.Length; iElement++)
            {
                if (elements [iElement] == null)
                {
                    throw new ArgumentNullException ("elements", SR.Get (SRID.ParamsEntryNullIllegal));
                }
                ((Collection<SrgsElement>) _elements).Add (elements [iElement]);
            }
        }

        #endregion

        //*******************************************************************
        //
        // Public Methods
        //
        //*******************************************************************

        #region public Method

        /// <summary>
        /// TODOC
        /// </summary>
        /// <param name="element"></param>
        public void Add (SrgsElement element)
        {
            Helpers.ThrowIfNull (element, "element");

            Elements.Add (element);
        }

        #endregion

        //*******************************************************************
        //
        // Public Properties
        //
        //*******************************************************************

        #region public Properties

        /// TODOC <_include file='doc\Rule.uex' path='docs/doc[@for="Rule.Elements"]/*' />
        public Collection<SrgsElement> Elements
        {
            get
            {
                return _elements;
            }
        }

        /// TODOC <_include file='doc\Rule.uex' path='docs/doc[@for="Rule.Id"]/*' />
        public string Id
        {
            get
            {
                return _id;
            }
            set
            {
                XmlParser.ValidateRuleId (value);
                _id = value;
            }
        }

        /// TODOC <_include file='doc\Rule.uex' path='docs/doc[@for="Rule.Scope"]/*' />
        public SrgsRuleScope Scope
        {
            get
            {
                return _scope;
            }
            set
            {
                _scope = value;
                _isScopeSet = true;
            }
        }

#if !NO_STG

        /// |summary|
        /// classname
        /// |/summary|
        public string BaseClass
        {
            set
            {
                // base value can be null
#pragma warning disable 56526
                _baseclass = value;
#pragma warning restore 56526
            }
            get
            {
                return _baseclass;
            }
        }

        /// |summary|
        /// OnInit
        /// |/summary|
        public string Script
        {
            set
            {
                Helpers.ThrowIfEmptyOrNull (value, "value");
                _script = value;
            }
            get
            {
                return _script;
            }
        }

        /// |summary|
        /// OnInit
        /// |/summary|
        public string OnInit
        {
            set
            {
                ValidateIdentifier (value);
                _onInit = value;
            }
            get
            {
                return _onInit;
            }
        }

        /// |summary|
        /// OnParse
        /// |/summary|
        public string OnParse
        {
            set
            {
                ValidateIdentifier (value);
                _onParse = value;
            }
            get
            {
                return _onParse;
            }
        }

        /// |summary|
        /// OnError
        /// |/summary|
        public string OnError
        {
            set
            {
                ValidateIdentifier (value);
                _onError = value;
            }
            get
            {
                return _onError;
            }
        }

        /// |summary|
        /// OnRecognition
        /// |/summary|
        public string OnRecognition
        {
            set
            {
                ValidateIdentifier (value);
                _onRecognition = value;
            }
            get
            {
                return _onRecognition;
            }
        }

#endif

        #endregion

        //*******************************************************************
        //
        // Internal Methods
        //
        //*******************************************************************

        #region Internal Methods

        internal void WriteSrgs (XmlWriter writer)
        {
            // Empty rule are not allowed
            if (Elements.Count == 0)
            {
                XmlParser.ThrowSrgsException (SRID.InvalidEmptyRule, "rule", _id);
            }

            // Write <rule id="MyRule" scope="public">
            writer.WriteStartElement ("rule");
            writer.WriteAttributeString ("id", _id);
            if (_isScopeSet)
            {
                switch (_scope)
                {
                    case SrgsRuleScope.Private:
                        writer.WriteAttributeString ("scope", "private");
                        break;

                    case SrgsRuleScope.Public:
                        writer.WriteAttributeString ("scope", "public");
                        break;
                }
            }
#if !NO_STG
            // Write the 'baseclass' attribute
            if (_baseclass != null)
            {
                writer.WriteAttributeString ("sapi", "baseclass", XmlParser.sapiNamespace, _baseclass);
            }
#endif
            // Write <rule id="MyRule" sapi:dynamic="true">
            if (_dynamic != RuleDynamic.NotSet)
            {
                writer.WriteAttributeString ("sapi", "dynamic", XmlParser.sapiNamespace, _dynamic == RuleDynamic.True ? "true" : "false");
            }

#if !NO_STG
            // Write the 'onInit' code snippet
            if (OnInit != null)
            {
                writer.WriteAttributeString ("sapi", "onInit", XmlParser.sapiNamespace, OnInit);
            }

            // Write <rule onParse="symbol">
            if (OnParse != null)
            {
                writer.WriteAttributeString ("sapi", "onParse", XmlParser.sapiNamespace, OnParse);
            }

            // Write <rule onError="symbol">
            if (OnError != null)
            {
                writer.WriteAttributeString ("sapi", "onError", XmlParser.sapiNamespace, OnError);
            }

            // Write <rule onRecognition="symbol">
            if (OnRecognition != null)
            {
                writer.WriteAttributeString ("sapi", "onRecognition", XmlParser.sapiNamespace, OnRecognition);
            }
#endif
            // Write <rule> body and footer.
            Type previousElementType = null;

            foreach (SrgsElement element in _elements)
            {
                // Insert space between consecutive SrgsText elements.
                Type elementType = element.GetType ();

                if ((elementType == typeof (SrgsText)) && (elementType == previousElementType))
                {
                    writer.WriteString (" ");
                }

                previousElementType = elementType;
                element.WriteSrgs (writer);
            }

            writer.WriteEndElement ();

#if !NO_STG
            // Write the <script> elements for the OnParse, OnError and OnRecognition code.
            // At the bottom of the code
            if (HasCode)
            {
                WriteScriptElement (writer, _script);
            }
#endif
        }

        // Validate the SRGS element.
        /// <summary>
        /// Validate each element and recurse through all the children srgs
        /// elements if any.
        /// </summary>
        internal void Validate (SrgsGrammar grammar)
        {
#if !NO_STG
            bool fScript = HasCode || _onInit != null || _onParse != null || _onError != null || _onRecognition != null || _baseclass != null;
            grammar._fContainsCode |= fScript;
            grammar.HasSapiExtension |= fScript;

            if (_dynamic != RuleDynamic.NotSet)
            {
                grammar.HasSapiExtension = true;
            }

            if (OnInit != null && Scope != SrgsRuleScope.Public)
            {
                XmlParser.ThrowSrgsException (SRID.OnInitOnPublicRule, "OnInit", Id);
            }

            if (OnRecognition != null && Scope != SrgsRuleScope.Public)
            {
                XmlParser.ThrowSrgsException (SRID.OnInitOnPublicRule, "OnRecognition", Id);
            }
#endif
            // Validate all the children
            foreach (SrgsElement element in _elements)
            {
                element.Validate (grammar);
            }
        }

        void IElement.PostParse (IElement grammar)
        {
            ((SrgsGrammar) grammar).Rules.Add (this);
        }

#if !NO_STG

        void IRule.CreateScript (IGrammar grammar, string rule, string method, RuleMethodScript type)
        {
            switch (type)
            {
                case RuleMethodScript.onInit:
                    _onInit = method;
                    break;

                case RuleMethodScript.onParse:
                    _onParse = method;
                    break;

                case RuleMethodScript.onRecognition:
                    _onRecognition = method;
                    break;

                case RuleMethodScript.onError:
                    _onError = method;
                    break;

                default:
                    // unknow method!!!
                    System.Diagnostics.Debug.Assert (false);
                    break;
            }
        }

#endif

        #endregion

        //*******************************************************************
        //
        // Internal Properties
        //
        //*******************************************************************

        #region Internal Properties

        /// TODOC <_include file='doc\Rule.uex' path='docs/doc[@for="Rule.Scope"]/*' />
        internal RuleDynamic Dynamic
        {
            set
            {
                _dynamic = value;
            }
            get
            {
                return _dynamic;
            }
        }

#if !NO_STG

        internal bool HasCode
        {
            get
            {
                return _script.Length > 0;
            }
        }

#endif

        #endregion

        //*******************************************************************
        //
        // Private Methods
        //
        //*******************************************************************

        #region Private Methods
#if !SPEECHSERVER

        private void WriteScriptElement(XmlWriter writer, string sCode)
        {
            writer.WriteStartElement("sapi", "script", XmlParser.sapiNamespace);
            writer.WriteAttributeString("sapi", "rule", XmlParser.sapiNamespace, _id);
            writer.WriteCData(sCode);
            writer.WriteEndElement();
        }

#pragma warning disable 56507 // check for null or empty strings

        void ValidateIdentifier(string s)
        {
            if (s == _id)
            {
                XmlParser.ThrowSrgsException(SRID.ConstructorNotAllowed, _id);
            }

            if (s != null && (s.IndexOfAny(invalidChars) >= 0 || s.Length == 0))
            {
                XmlParser.ThrowSrgsException(SRID.InvalidMethodName);
            }
        }

#pragma warning restore 56507

#endif

        #endregion

        //*******************************************************************
        //
        // Private Fields
        //
        //*******************************************************************

        #region Private Fields

        private SrgsElementList _elements;

        private string _id;

        private SrgsRuleScope _scope = SrgsRuleScope.Private;

        private RuleDynamic _dynamic = RuleDynamic.NotSet;

        private bool _isScopeSet;

#if !NO_STG

        // class name for the code behind
        private string _baseclass;

        // .Net Language for this grammar
        private string _script = string.Empty;

        private string _onInit;

        private string _onParse;

        private string _onError;

        private string _onRecognition;
#endif
        static readonly char [] invalidChars = new char [] { '?', '*', '+', '|', '(', ')', '^', '$', '/', ';', '.', '=', '<', '>', '[', ']', '{', '}', '\\', ' ', '\t', '\r', '\n' };

        #endregion

        //*******************************************************************
        //
        // Private Types
        //
        //*******************************************************************

        #region Private Types

        // Used by the debbugger display attribute
        internal class SrgsRuleDebugDisplay
        {
            public SrgsRuleDebugDisplay (SrgsRule rule)
            {
                _rule = rule;
            }

            public object Id
            {
                get
                {
                    return _rule.Id;
                }
            }

            public object Scope
            {
                get
                {
                    return _rule.Scope;
                }
            }

#if !NO_STG

            public object BaseClass
            {
                get
                {
                    return _rule.BaseClass;
                }
            }

            public object Script
            {
                get
                {
                    return _rule.Script;
                }
            }

            public object OnInit
            {
                get
                {
                    return _rule.OnInit;
                }
            }

            public object OnParse
            {
                get
                {
                    return _rule.OnParse;
                }
            }

            public object OnError
            {
                get
                {
                    return _rule.OnError;
                }
            }

            public object OnRecognition
            {
                get
                {
                    return _rule.OnRecognition;
                }
            }

#endif

            public object Count
            {
                get
                {
                    return _rule._elements.Count;
                }
            }

            [DebuggerBrowsable (DebuggerBrowsableState.RootHidden)]
            public SrgsElement [] AKeys
            {
                get
                {
                    SrgsElement [] elements = new SrgsElement [_rule._elements.Count];
                    for (int i = 0; i < _rule._elements.Count; i++)
                    {
                        elements [i] = _rule._elements [i];
                    }
                    return elements;
                }
            }

            private SrgsRule _rule;
        }

        #endregion
    }

    //*******************************************************************
    //
    // Public Enums
    //
    //*******************************************************************

    #region Public Enums

    /// TODOC <_include file='doc\Rule.uex' path='docs/doc[@for="SrgsRuleScope"]/*' />
    // SrgsRuleScope specifies how a rule behaves with respect to being able to be
    // referenced by other rules, and whether or not the rule can be activated
    // or not.
    public enum SrgsRuleScope
    {
        /// TODOC <_include file='doc\Rule.uex' path='docs/doc[@for="SrgsRuleScope.Public"]/*' />
        // Public rules can be both activated as well as referenced by rules in other grammars
        Public,
        /// TODOC <_include file='doc\Rule.uex' path='docs/doc[@for="SrgsRuleScope.Private"]/*' />
        // Private rules can not be activated, but they can be referenced by rules in the same grammar
        Private
    };

    #endregion
}

