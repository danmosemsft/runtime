// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Speech.Internal;
using System.Speech.Internal.SrgsCompiler;
using System.Speech.Recognition.SrgsGrammar;
using System.Text;

namespace System.Speech.Recognition
{
    /// <summary>A runtime object that references a speech recognition grammar, which an application can use to define the constraints for speech recognition.</summary>
    [DebuggerDisplay("Grammar: {(_uri != null ? \"uri=\" + _uri.ToString () + \" \" : \"\") + \"rule=\" + _ruleName }")]
    public class Grammar
    {
        private struct NameValuePair
        {
            internal string _name;

            internal string _value;
        }

        internal GrammarOptions _semanticTag;

        internal AppDomain _appDomain;

        internal AppDomainGrammarProxy _proxy;

        internal ScriptRef[] _scripts;

        private byte[] _cfgData;

        private Stream _appStream;

        private bool _isSrgsDocument;

        private SrgsDocument _srgsDocument;

        private GrammarBuilder _grammarBuilder;

        private IRecognizerInternal _recognizer;

        private GrammarState _grammarState;

        private Exception _loadException;

        private Uri _uri;

        private Uri _baseUri;

        private string _ruleName;

        private string _resources;

        private object[] _parameters;

        private string _onInitParameters;

        private bool _enabled = true;

        private bool _isStg;

        private bool _sapi53Only;

        private uint _sapiGrammarId;

        private float _weight = 1f;

        private int _priority;

        private InternalGrammarData _internalData;

        private string _grammarName = string.Empty;

        private Collection<Grammar> _ruleRefs;

        private static ResourceLoader _resourceLoader = new ResourceLoader();

        /// <summary>Gets or sets a value that controls whether a <see cref="T:System.Speech.Recognition.Grammar" /> can be used by a speech recognizer to perform recognition.</summary>
        /// <returns>The  <see langword="Enabled" /> property returns <see langword="true" /> if a speech recognizer can perform recognition using the speech recognition grammar; otherwise the property returns <see langword="false" />. The default is <see langword="true" />.</returns>
        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                if (_grammarState != 0 && _enabled != value)
                {
                    _recognizer.SetGrammarState(this, value);
                }
                _enabled = value;
            }
        }

        /// <summary>Gets or sets the weight value of a <see cref="T:System.Speech.Recognition.Grammar" /> object.</summary>
        /// <returns>The <see langword="Weight" /> property returns a floating point value indicating the relative weight that a recognition engine  instance should assign to the grammar when processing speech input. The range is from 0.0 to 1.0 inclusive. The default is 1.0.</returns>
        public float Weight
        {
            get
            {
                return _weight;
            }
            set
            {
                if ((double)value < 0.0 || (double)value > 1.0)
                {
                    throw new ArgumentOutOfRangeException("value", SR.Get(SRID.GrammarInvalidWeight));
                }
                if (_grammarState != 0 && !_weight.Equals(value))
                {
                    _recognizer.SetGrammarWeight(this, value);
                }
                _weight = value;
            }
        }

        /// <summary>Gets or sets the priority value of a <see cref="T:System.Speech.Recognition.Grammar" /> object.</summary>
        /// <returns>The <see langword="Priority" /> property returns an integer value that represents the relative priority of a specific <see cref="T:System.Speech.Recognition.Grammar" />. The range is from -128 to 127 inclusive. The default is 0.</returns>
        public int Priority
        {
            get
            {
                return _priority;
            }
            set
            {
                if (value < -128 || value > 127)
                {
                    throw new ArgumentOutOfRangeException("value", SR.Get(SRID.GrammarInvalidPriority));
                }
                if (_grammarState != 0 && _priority != value)
                {
                    _recognizer.SetGrammarPriority(this, value);
                }
                _priority = value;
            }
        }

        /// <summary>Gets or sets the name of a <see cref="T:System.Speech.Recognition.Grammar" /> object.</summary>
        /// <returns>The <see langword="Name" /> property returns the name of the <see cref="T:System.Speech.Recognition.Grammar" /> object. The default is <see langword="null" />.</returns>
        public string Name
        {
            get
            {
                return _grammarName;
            }
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }
                _grammarName = value;
            }
        }

        /// <summary>Gets the name of the root rule or entry point of a <see cref="T:System.Speech.Recognition.Grammar" /> object.</summary>
        /// <returns>The <see langword="RuleName" /> property returns the identifier for the root rule of the referenced speech recognition grammar. The default is <see langword="null" />.</returns>
        public string RuleName => _ruleName;

        /// <summary>Gets whether a <see cref="T:System.Speech.Recognition.Grammar" /> has been loaded by a speech recognizer.</summary>
        /// <returns>The <see langword="Loaded" /> property returns <see langword="true" /> if the referenced speech recognition grammar is currently loaded in a speech recognizer; otherwise the property returns <see langword="false" />. The default is <see langword="false" />.</returns>
        public bool Loaded => _grammarState == GrammarState.Loaded;

        internal Uri Uri => _uri;

        internal IRecognizerInternal Recognizer
        {
            get
            {
                return _recognizer;
            }
            set
            {
                _recognizer = value;
            }
        }

        internal GrammarState State
        {
            get
            {
                return _grammarState;
            }
            set
            {
                switch (value)
                {
                    case GrammarState.Unloaded:
                        _loadException = null;
                        _recognizer = null;
                        if (_appDomain != null)
                        {
                            AppDomain.Unload(_appDomain);
                            _appDomain = null;
                        }
                        break;
                    default:
                        _ = 3;
                        break;
                    case GrammarState.Loaded:
                        break;
                }
                _grammarState = value;
            }
        }

        internal Exception LoadException
        {
            get
            {
                return _loadException;
            }
            set
            {
                _loadException = value;
            }
        }

        internal byte[] CfgData => _cfgData;

        internal Uri BaseUri => _baseUri;

        internal bool Sapi53Only => _sapi53Only;

        internal uint SapiGrammarId
        {
            get
            {
                return _sapiGrammarId;
            }
            set
            {
                _sapiGrammarId = value;
            }
        }

        /// <summary>Gets whether a grammar is strongly typed.</summary>
        /// <returns>The <see langword="IsStg" /> property returns <see langword="true" /> if the grammar is strongly-typed; otherwise the property returns <see langword="false" />.</returns>
        protected internal virtual bool IsStg => _isStg;

        internal bool IsSrgsDocument => _isSrgsDocument;

        internal InternalGrammarData InternalData
        {
            get
            {
                return _internalData;
            }
            set
            {
                _internalData = value;
            }
        }

        /// <summary>Gets or sets a value with the name of a binary resource that was used to load the current <see cref="T:System.Speech.Recognition.Grammar" />.</summary>
        /// <returns>The <see langword="ResourceName" /> property returns the name of the binary resource from which the strongly-typed grammar, used by <see cref="T:System.Speech.Recognition.Grammar" />, was loaded.</returns>
        protected string ResourceName
        {
            get
            {
                return _resources;
            }
            set
            {
                Helpers.ThrowIfEmptyOrNull(value, "value");
                _resources = value;
            }
        }

        /// <summary>Raised when a speech recognizer performs recognition using the <see cref="T:System.Speech.Recognition.Grammar" /> object.</summary>
        public event EventHandler<SpeechRecognizedEventArgs> SpeechRecognized;

        internal Grammar(Uri uri, string ruleName, object[] parameters)
        {
            Helpers.ThrowIfNull(uri, "uri");
            _uri = uri;
            InitialGrammarLoad(ruleName, parameters, isImportedGrammar: false);
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.Grammar" /> class from a file.</summary>
        /// <param name="path">The path of the file that describes a speech recognition grammar in a supported format.</param>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="path" /> contains the empty string (""), or the file describes a grammar that does not contain a root rule.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="path" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.FormatException">The file does not contain a valid description, or describes a grammar that contains a rule reference that cannot be resolved.</exception>
        public Grammar(string path)
            : this(path, (string)null, (object[])null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.Grammar" /> class from a file and specifies a root rule.</summary>
        /// <param name="path">The path of the file that describes a speech recognition grammar in a supported format.</param>
        /// <param name="ruleName">The identifier of the rule to use as the entry point of the speech recognition grammar, or <see langword="null" /> to use the default root rule of the grammar description.</param>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="ruleName" /> cannot be resolved or is not public, <paramref name="path" /> is the empty string (""), or <paramref name="ruleName" /> is <see langword="null" /> and the grammar description does not define a root rule.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="path" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.FormatException">The file does not contain a valid description or describes a grammar that contains a rule reference that cannot be resolved.</exception>
        public Grammar(string path, string ruleName)
            : this(path, ruleName, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.Grammar" /> class from a file that contains a grammar definition, and specifies the name of a rule to be the entry point to the grammar.</summary>
        /// <param name="path">The path to a file, including DLLs, that contains a grammar specification.</param>
        /// <param name="ruleName">The identifier of the rule to use as the entry point of the speech recognition grammar, or <see langword="null" /> to use the default root rule of the grammar description.</param>
        /// <param name="parameters">Parameters to be passed to the initialization handler specified by the <see cref="P:System.Speech.Recognition.SrgsGrammar.SrgsRule.OnInit" /> property for the entry point or the root rule of the <see cref="T:System.Speech.Recognition.Grammar" /> to be created. This parameter may be null.</param>
        /// <exception cref="T:System.ArgumentException">Any of the parameters contain an invalid value.
        ///
        /// The file specified by <paramref name="path" /> does not contain a valid grammar or the rule specified in <paramref name="ruleName" />.
        ///
        /// The contents of the array parameters do not match the arguments of any of the rule's initialization handlers.
        ///
        /// The grammar has a relative rule reference that cannot be resolved by the default base <see cref="T:System.Uri" /> rule for grammars.</exception>
        public Grammar(string path, string ruleName, object[] parameters)
        {
            try
            {
                _uri = new Uri(path, UriKind.Relative);
            }
            catch (UriFormatException innerException)
            {
                throw new ArgumentException(SR.Get(SRID.RecognizerGrammarNotFound), "path", innerException);
            }
            InitialGrammarLoad(ruleName, parameters, isImportedGrammar: false);
        }

        /// <summary>Initializes a new instance of a <see cref="T:System.Speech.Recognition.Grammar" /> class from an <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" /> object.</summary>
        /// <param name="srgsDocument">The constraints for the speech recognition grammar.</param>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="srgsDocument" /> does not contain a root rule.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="srgsDocument" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.FormatException">
        ///   <paramref name="srgsDocument" /> contains a rule reference that cannot be resolved.</exception>
        public Grammar(SrgsDocument srgsDocument)
            : this(srgsDocument, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of a <see cref="T:System.Speech.Recognition.Grammar" /> class from an <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" /> object and specifies a root rule.</summary>
        /// <param name="srgsDocument">The constraints for the speech recognition grammar.</param>
        /// <param name="ruleName">The identifier of the rule to use as the entry point of the speech recognition grammar, or <see langword="null" /> to use the default root rule of the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" />.</param>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="ruleName" /> cannot be resolved or is not public, or <paramref name="ruleName" /> is <see langword="null" /> and <paramref name="srgsDocument" /> does not contain a root rule.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="srgsDocument" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.FormatException">
        ///   <paramref name="srgsDocument" /> contains a rule reference that cannot be resolved.</exception>
        public Grammar(SrgsDocument srgsDocument, string ruleName)
            : this(srgsDocument, ruleName, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.Grammar" /> class from an instance of <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" />, and specifies the name of a rule to be the entry point to the grammar.</summary>
        /// <param name="srgsDocument">An instance of <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" /> that contains the constraints for the speech recognition grammar.</param>
        /// <param name="ruleName">The identifier of the rule to use as the entry point of the speech recognition grammar, or <see langword="null" /> to use the default root rule of the grammar description.</param>
        /// <param name="parameters">Parameters to be passed to the initialization handler specified by the <see cref="P:System.Speech.Recognition.SrgsGrammar.SrgsRule.OnInit" /> property for the entry point or the root rule of the <see cref="T:System.Speech.Recognition.Grammar" /> to be created. This parameter may be null.</param>
        /// <exception cref="T:System.ArgumentException">Any of the parameters contain an invalid value.
        ///
        /// The <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" /> specified by <paramref name="srgsDocument" /> does not contain the rule specified by <paramref name="ruleName" />.
        ///
        /// The contents of the array parameters do not match the arguments of any of the rule's initialization handlers.</exception>
        public Grammar(SrgsDocument srgsDocument, string ruleName, object[] parameters)
            : this(srgsDocument, ruleName, null, parameters)
        {
        }

        /// <summary>Initializes a new instance of a <see cref="T:System.Speech.Recognition.Grammar" /> class from an <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" /> object, specifies a root rule, and defines a base Uniform Resource Identifier (URI) to resolve relative rule references.</summary>
        /// <param name="srgsDocument">The constraints for the speech recognition grammar.</param>
        /// <param name="ruleName">The identifier of the rule to use as the entry point of the speech recognition grammar, or <see langword="null" /> to use the default root rule of the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" />.</param>
        /// <param name="baseUri">The base URI to use to resolve any relative rule reference in the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" />, or <see langword="null" />.</param>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="ruleName" /> cannot be resolved or is not public, or <paramref name="ruleName" /> is <see langword="null" /> and <paramref name="srgsDocument" /> does not contain a root rule.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="srgsDocument" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.FormatException">
        ///   <paramref name="srgsDocument" /> contains a rule reference that cannot be resolved.</exception>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public Grammar(SrgsDocument srgsDocument, string ruleName, Uri baseUri)
            : this(srgsDocument, ruleName, baseUri, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.Grammar" /> class from an instance of <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" />, and specifies the name of a rule to be the entry point to the grammar and a base URI to resolve relative references.</summary>
        /// <param name="srgsDocument">An instance of <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" /> that contains the constraints for the speech recognition grammar.</param>
        /// <param name="ruleName">The identifier of the rule to use as the entry point of the speech recognition grammar, or <see langword="null" /> to use the default root rule of the grammar description.</param>
        /// <param name="baseUri">The base URI to use to resolve any relative rule reference in the grammar description, or <see langword="null" />.</param>
        /// <param name="parameters">Parameters to be passed to the initialization handler specified by the <see cref="P:System.Speech.Recognition.SrgsGrammar.SrgsRule.OnInit" /> property for the entry point or the root rule of the <see cref="T:System.Speech.Recognition.Grammar" /> to be created.This parameter may be null.</param>
        /// <exception cref="T:System.ArgumentException">Any of the parameters contain an invalid value.
        ///
        /// The <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" /> specified by <paramref name="srgsDocument" /> does not contain the rule specified in <paramref name="ruleName" />.
        ///
        /// The contents of the array parameters do not match the arguments of any of the rule's initialization handlers.
        ///
        /// The grammar has a relative rule reference that cannot be resolved by the default base <see cref="T:System.Uri" /> rule for grammars or the URI supplied by <paramref name="baseUri" />.</exception>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public Grammar(SrgsDocument srgsDocument, string ruleName, Uri baseUri, object[] parameters)
        {
            Helpers.ThrowIfNull(srgsDocument, "srgsDocument");
            _srgsDocument = srgsDocument;
            _isSrgsDocument = (srgsDocument != null);
            _baseUri = baseUri;
            InitialGrammarLoad(ruleName, parameters, isImportedGrammar: false);
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.Grammar" /> class from a <see cref="T:System.IO.Stream" />.</summary>
        /// <param name="stream">A stream that describes a speech recognition grammar in a supported format.</param>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="stream" /> describes a grammar that does not contain a root rule.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="stream" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.FormatException">The stream does not contain a valid description of a grammar, or describes a grammar that contains a rule reference that cannot be resolved.</exception>
        public Grammar(Stream stream)
            : this(stream, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.Grammar" /> class from a <see cref="T:System.IO.Stream" /> and specifies a root rule.</summary>
        /// <param name="stream">A stream that describes a speech recognition grammar in a supported format.</param>
        /// <param name="ruleName">The identifier of the rule to use as the entry point of the speech recognition grammar, or <see langword="null" /> to use the default root rule of the grammar description.</param>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="ruleName" /> cannot be resolved or is not public, or <paramref name="ruleName" /> is <see langword="null" /> and the grammar description does not define a root rule.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="stream" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.FormatException">The stream does not contain a valid description or describes a grammar that contains a rule reference that cannot be resolved.</exception>
        public Grammar(Stream stream, string ruleName)
            : this(stream, ruleName, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.Grammar" /> class from a <see cref="T:System.IO.Stream" /> and specifies a root rule.</summary>
        /// <param name="stream">A <see cref="T:System.IO.Stream" /> connected to an input/output object (including files, VisualStudio Resources, and DLLs) that contains a grammar specification.</param>
        /// <param name="ruleName">The identifier of the rule to use as the entry point of the speech recognition grammar, or <see langword="null" /> to use the default root rule of the grammar description.</param>
        /// <param name="parameters">Parameters to be passed to the initialization handler specified by the <see cref="P:System.Speech.Recognition.SrgsGrammar.SrgsRule.OnInit" /> property for the entry point or the root rule of the <see cref="T:System.Speech.Recognition.Grammar" /> to be created. This parameter may be null.</param>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="stream" /> is connected to a grammar that:
        ///
        /// Does not contain the rule specified in <paramref name="ruleName" />
        ///
        /// Requires initialization parameters different from those specified in <paramref name="parameters" />
        ///
        /// Contains a relative rule reference that cannot be resolved by the default base <see cref="T:System.Uri" /> rule for grammars</exception>
        public Grammar(Stream stream, string ruleName, object[] parameters)
            : this(stream, ruleName, null, parameters)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.Grammar" /> class from a stream, specifies a root rule, and defines a base Uniform Resource Identifier (URI) to resolve relative rule references.</summary>
        /// <param name="stream">A stream that describes a speech recognition grammar in a supported format.</param>
        /// <param name="ruleName">The identifier of the rule to use as the entry point of the speech recognition grammar, or <see langword="null" /> to use the default root rule of the grammar description.</param>
        /// <param name="baseUri">The base URI to use to resolve any relative rule reference in the grammar description, or <see langword="null" />.</param>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="ruleName" /> cannot be resolved or is not public, or <paramref name="ruleName" /> is <see langword="null" /> and the grammar description does not define a root rule.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="stream" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.FormatException">The stream does not contain a valid description or describes a grammar that contains a rule reference that cannot be resolved.</exception>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public Grammar(Stream stream, string ruleName, Uri baseUri)
            : this(stream, ruleName, baseUri, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.Grammar" /> class a <see cref="T:System.IO.Stream" /> and specifies a root rule and a base URI to resolve relative references.</summary>
        /// <param name="stream">A <see cref="T:System.IO.Stream" /> connected to an input/output object (including files, VisualStudio Resources, and DLLs) that contains a grammar specification.</param>
        /// <param name="ruleName">The identifier of the rule to use as the entry point of the speech recognition grammar, or <see langword="null" /> to use the default root rule of the grammar description.</param>
        /// <param name="baseUri">The base URI to use to resolve any relative rule reference in the grammar description, or <see langword="null" />.</param>
        /// <param name="parameters">Parameters to be passed to the initialization handler specified by the <see cref="P:System.Speech.Recognition.SrgsGrammar.SrgsRule.OnInit" /> property for the entry point or the root rule of the <see cref="T:System.Speech.Recognition.Grammar" /> to be created. This parameter may be null.</param>
        /// <exception cref="T:System.ArgumentException">Any of the parameters contain an invalid value.
        ///
        /// The <paramref name="stream" /> is connected to a grammar that does not contain the rule specified by <paramref name="ruleName" />.
        ///
        /// The contents of the array parameters do not match the arguments of any of the rule's initialization handlers.
        ///
        /// The grammar contains a relative rule reference that cannot be resolved by the default base <see cref="T:System.Uri" /> rule for grammars or the URI supplied by <paramref name="baseUri" />.</exception>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public Grammar(Stream stream, string ruleName, Uri baseUri, object[] parameters)
        {
            Helpers.ThrowIfNull(stream, "stream");
            if (!stream.CanRead)
            {
                throw new ArgumentException(SR.Get(SRID.StreamMustBeReadable), "stream");
            }
            _appStream = stream;
            _baseUri = baseUri;
            InitialGrammarLoad(ruleName, parameters, isImportedGrammar: false);
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.Grammar" /> class from a <see cref="T:System.Speech.Recognition.GrammarBuilder" /> object.</summary>
        /// <param name="builder">An instance of <see cref="T:System.Speech.Recognition.GrammarBuilder" /> that contains the constraints for the speech recognition grammar.</param>
        public Grammar(GrammarBuilder builder)
        {
            Helpers.ThrowIfNull(builder, "builder");
            _grammarBuilder = builder;
            InitialGrammarLoad(null, null, isImportedGrammar: false);
        }

        private Grammar(string onInitParameters, Stream stream, string ruleName)
        {
            _appStream = stream;
            _onInitParameters = onInitParameters;
            InitialGrammarLoad(ruleName, null, isImportedGrammar: true);
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.Grammar" /> class</summary>
        protected Grammar()
        {
        }

        /// <summary>The <see langword="StgInit" /> method initializes a strongly-typed grammar.</summary>
        /// <param name="parameters">Parameters to be passed to initialize the strongly-typed grammar.This parameter may be null.</param>
        protected void StgInit(object[] parameters)
        {
            _parameters = parameters;
            LoadAndCompileCfgData(isImportedGrammar: false, stgInit: true);
        }

        /// <summary>The <see langword="LoadLocalizedGrammarFromType" /> method returns a localized instance of a <see cref="T:System.Speech.Recognition.Grammar" /> object derived from <see cref="T:System.Type" />.</summary>
        /// <param name="type">In an assembly, the <see cref="T:System.Type" /> of an object based on <see cref="T:System.Speech.Recognition.Grammar" />.</param>
        /// <param name="onInitParameters">Parameters to be passed to an initialization method of the localizedobject based on <see cref="T:System.Speech.Recognition.Grammar" />. This parameter may be null.</param>
        /// <returns>The <see langword="LoadLocalizedGrammarFromType" /> method returns a valid object based on <see cref="T:System.Speech.Recognition.Grammar" />, or <see langword="null" /> if there has been an error.</returns>
        public static Grammar LoadLocalizedGrammarFromType(Type type, params object[] onInitParameters)
        {
            Helpers.ThrowIfNull(type, "type");
            if (type == typeof(Grammar) || !type.IsSubclassOf(typeof(Grammar)))
            {
                throw new ArgumentException(SR.Get(SRID.StrongTypedGrammarNotAGrammar), "type");
            }
            Assembly assembly = Assembly.GetAssembly(type);
            Type[] types = assembly.GetTypes();
            foreach (Type type2 in types)
            {
                string s = null;
                if ((!(type2 == type) && !type2.IsSubclassOf(type)) || !(type2.GetField("__cultureId") != null))
                {
                    continue;
                }
                try
                {
                    s = (string)type2.InvokeMember("__cultureId", BindingFlags.GetField, null, null, null, null);
                }
                catch (Exception ex)
                {
                    if (!(ex is MissingFieldException))
                    {
                        throw;
                    }
                }
                if (Helpers.CompareInvariantCulture(new CultureInfo(int.Parse(s, CultureInfo.InvariantCulture)), CultureInfo.CurrentUICulture))
                {
                    try
                    {
                        return (Grammar)assembly.CreateInstance(type2.FullName, ignoreCase: false, BindingFlags.CreateInstance, null, onInitParameters, null, null);
                    }
                    catch (MissingMemberException)
                    {
                        throw new ArgumentException(SR.Get(SRID.RuleScriptInvalidParameters, type2.Name, type2.Name));
                    }
                }
            }
            return null;
        }

        internal static Grammar Create(string grammarName, string ruleName, string onInitParameter, out Uri redirectUri)
        {
            redirectUri = null;
            grammarName = grammarName.Trim();
            Uri result;
            bool flag = Uri.TryCreate(grammarName, UriKind.Absolute, out result);
            int num = grammarName.IndexOf(".dll", StringComparison.OrdinalIgnoreCase);
            if (!flag || (num > 0 && num == grammarName.Length - 4))
            {
                Assembly assembly;
                if (flag)
                {
                    if (!result.IsFile)
                    {
                        throw new InvalidOperationException();
                    }
                    assembly = Assembly.LoadFrom(result.LocalPath);
                }
                else
                {
                    assembly = Assembly.Load(grammarName);
                }
                return LoadGrammarFromAssembly(assembly, ruleName, onInitParameter);
            }
            try
            {
                string localPath;
                using (Stream stream = _resourceLoader.LoadFile(result, out localPath, out redirectUri))
                {
                    try
                    {
                        return new Grammar(onInitParameter, stream, ruleName);
                    }
                    finally
                    {
                        _resourceLoader.UnloadFile(localPath);
                    }
                }
            }
            catch
            {
                Assembly assembly2 = Assembly.LoadFrom(grammarName);
                return LoadGrammarFromAssembly(assembly2, ruleName, onInitParameter);
            }
        }

        internal void OnRecognitionInternal(SpeechRecognizedEventArgs eventArgs)
        {
            this.SpeechRecognized?.Invoke(this, eventArgs);
        }

        internal static bool IsDictationGrammar(Uri uri)
        {
            if (uri == null || !uri.IsAbsoluteUri || uri.Scheme != "grammar" || !string.IsNullOrEmpty(uri.Host) || !string.IsNullOrEmpty(uri.Authority) || !string.IsNullOrEmpty(uri.Query) || uri.PathAndQuery != "dictation")
            {
                return false;
            }
            return true;
        }

        internal bool IsDictation(Uri uri)
        {
            bool flag = IsDictationGrammar(uri);
            if (!flag && this is DictationGrammar)
            {
                throw new ArgumentException(SR.Get(SRID.DictationInvalidTopic), "uri");
            }
            return flag;
        }

        internal Grammar Find(long grammarId)
        {
            if (_ruleRefs != null)
            {
                foreach (Grammar ruleRef in _ruleRefs)
                {
                    if (grammarId == ruleRef._sapiGrammarId)
                    {
                        return ruleRef;
                    }
                    Grammar result;
                    if ((result = ruleRef.Find(grammarId)) != null)
                    {
                        return result;
                    }
                }
            }
            return null;
        }

        internal Grammar Find(string ruleName)
        {
            if (_ruleRefs != null)
            {
                foreach (Grammar ruleRef in _ruleRefs)
                {
                    if (ruleName == ruleRef.RuleName)
                    {
                        return ruleRef;
                    }
                    Grammar result;
                    if ((result = ruleRef.Find(ruleName)) != null)
                    {
                        return result;
                    }
                }
            }
            return null;
        }

        internal void AddRuleRef(Grammar ruleRef, uint grammarId)
        {
            if (_ruleRefs == null)
            {
                _ruleRefs = new Collection<Grammar>();
            }
            _ruleRefs.Add(ruleRef);
            _sapiGrammarId = grammarId;
        }

        internal MethodInfo MethodInfo(string method)
        {
            return GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        private void LoadAndCompileCfgData(bool isImportedGrammar, bool stgInit)
        {
            Stream stream = IsStg ? LoadCfgFromResource(stgInit) : LoadCfg(isImportedGrammar, stgInit);
            SrgsRule[] array = RunOnInit(IsStg);
            if (array != null)
            {
                MemoryStream memoryStream = CombineCfg(_ruleName, stream, array);
                stream.Close();
                stream = memoryStream;
            }
            _cfgData = Helpers.ReadStreamToByteArray(stream, (int)stream.Length);
            stream.Close();
            _srgsDocument = null;
            _appStream = null;
        }

        private MemoryStream LoadCfg(bool isImportedGrammar, bool stgInit)
        {
            Uri uri = Uri;
            MemoryStream memoryStream = new MemoryStream();
            if (uri != null)
            {
                string mimeType;
                string localPath;
                using (Stream stream = _resourceLoader.LoadFile(uri, out mimeType, out _baseUri, out localPath))
                {
                    stream.Position = 0L;
                    SrgsGrammarCompiler.CompileXmlOrCopyCfg(stream, memoryStream, uri);
                }
                _resourceLoader.UnloadFile(localPath);
            }
            else if (_srgsDocument != null)
            {
                SrgsGrammarCompiler.Compile(_srgsDocument, memoryStream);
                if (_baseUri == null && _srgsDocument.BaseUri != null)
                {
                    _baseUri = _srgsDocument.BaseUri;
                }
            }
            else if (_grammarBuilder != null)
            {
                _grammarBuilder.Compile(memoryStream);
            }
            else
            {
                SrgsGrammarCompiler.CompileXmlOrCopyCfg(_appStream, memoryStream, null);
            }
            memoryStream.Position = 0L;
            _ruleName = CheckRuleName(memoryStream, _ruleName, isImportedGrammar, stgInit, out _sapi53Only, out _semanticTag);
            CreateSandbox(memoryStream);
            memoryStream.Position = 0L;
            return memoryStream;
        }

        private static Grammar LoadGrammarFromAssembly(Assembly assembly, string ruleName, string onInitParameters)
        {
            Type typeFromHandle = typeof(Grammar);
            Type type = null;
            Type[] types = assembly.GetTypes();
            foreach (Type type2 in types)
            {
                if (!type2.IsSubclassOf(typeFromHandle))
                {
                    continue;
                }
                string s = null;
                if (type2.Name == ruleName)
                {
                    type = type2;
                }
                if ((!(type2 == type) && (!(type != null) || !type2.IsSubclassOf(type))) || !(type2.GetField("__cultureId") != null))
                {
                    continue;
                }
                try
                {
                    s = (string)type2.InvokeMember("__cultureId", BindingFlags.GetField, null, null, null, null);
                }
                catch (Exception ex)
                {
                    if (!(ex is MissingFieldException))
                    {
                        throw;
                    }
                }
                if (Helpers.CompareInvariantCulture(new CultureInfo(int.Parse(s, CultureInfo.InvariantCulture)), CultureInfo.CurrentUICulture))
                {
                    try
                    {
                        object[] args = MatchInitParameters(type2, onInitParameters, assembly.GetName().Name, ruleName);
                        return (Grammar)assembly.CreateInstance(type2.FullName, ignoreCase: false, BindingFlags.CreateInstance, null, args, null, null);
                    }
                    catch (MissingMemberException)
                    {
                        throw new ArgumentException(SR.Get(SRID.RuleScriptInvalidParameters, type2.Name, type2.Name));
                    }
                }
            }
            return null;
        }

        private static object[] MatchInitParameters(Type type, string onInitParameters, string grammar, string rule)
        {
            ConstructorInfo[] constructors = type.GetConstructors();
            NameValuePair[] array = ParseInitParams(onInitParameters);
            object[] array2 = new object[array.Length];
            bool flag = false;
            for (int i = 0; i < constructors.Length; i++)
            {
                if (flag)
                {
                    break;
                }
                ParameterInfo[] parameters = constructors[i].GetParameters();
                if (parameters.Length > array.Length)
                {
                    continue;
                }
                flag = true;
                for (int j = 0; j < array.Length && flag; j++)
                {
                    NameValuePair nameValuePair = array[j];
                    if (nameValuePair._name == null)
                    {
                        array2[j] = nameValuePair._value;
                        continue;
                    }
                    bool flag2 = false;
                    for (int k = 0; k < parameters.Length; k++)
                    {
                        if (parameters[k].Name == nameValuePair._name)
                        {
                            array2[k] = ParseValue(parameters[k].ParameterType, nameValuePair._value);
                            flag2 = true;
                            break;
                        }
                    }
                    if (!flag2)
                    {
                        flag = false;
                    }
                }
            }
            if (!flag)
            {
                throw new FormatException(SR.Get(SRID.CantFindAConstructor, grammar, rule, FormatConstructorParameters(constructors)));
            }
            return array2;
        }

        private static object ParseValue(Type type, string value)
        {
            if (type == typeof(string))
            {
                return value;
            }
            return type.InvokeMember("Parse", BindingFlags.InvokeMethod, null, null, new object[1]
            {
                value
            }, CultureInfo.InvariantCulture);
        }

        private static string FormatConstructorParameters(ConstructorInfo[] cis)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < cis.Length; i++)
            {
                stringBuilder.Append((i > 0) ? " or sapi:parms=\"" : "sapi:parms=\"");
                ParameterInfo[] parameters = cis[i].GetParameters();
                for (int j = 0; j < parameters.Length; j++)
                {
                    if (j > 0)
                    {
                        stringBuilder.Append(';');
                    }
                    ParameterInfo parameterInfo = parameters[j];
                    stringBuilder.Append(parameterInfo.Name);
                    stringBuilder.Append(':');
                    stringBuilder.Append(parameterInfo.ParameterType.Name);
                }
                stringBuilder.Append('\\');
            }
            return stringBuilder.ToString();
        }

        private static NameValuePair[] ParseInitParams(string initParameters)
        {
            if (string.IsNullOrEmpty(initParameters))
            {
                return new NameValuePair[0];
            }
            string[] array = initParameters.Split(new char[1]
            {
                ';'
            }, StringSplitOptions.None);
            NameValuePair[] array2 = new NameValuePair[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                string text = array[i];
                int num = text.IndexOf(':');
                if (num >= 0)
                {
                    array2[i]._name = text.Substring(0, num);
                    array2[i]._value = text.Substring(num + 1);
                }
                else
                {
                    array2[i]._value = text;
                }
            }
            return array2;
        }

        private void InitialGrammarLoad(string ruleName, object[] parameters, bool isImportedGrammar)
        {
            _ruleName = ruleName;
            _parameters = parameters;
            if (!IsDictation(_uri))
            {
                LoadAndCompileCfgData(isImportedGrammar, stgInit: false);
            }
        }

        private void CreateSandbox(MemoryStream stream)
        {
            stream.Position = 0L;
            if (CfgGrammar.LoadIL(stream, out byte[] assemblyContent, out byte[] assemblyDebugSymbols, out ScriptRef[] scripts))
            {
                Assembly executingAssembly = Assembly.GetExecutingAssembly();
                _appDomain = AppDomain.CreateDomain("sandbox");
                _proxy = (AppDomainGrammarProxy)_appDomain.CreateInstanceFromAndUnwrap(executingAssembly.GetName().CodeBase, "System.Speech.Internal.SrgsCompiler.AppDomainGrammarProxy");
                _proxy.Init(_ruleName, assemblyContent, assemblyDebugSymbols);
                _scripts = scripts;
            }
        }

        private Stream LoadCfgFromResource(bool stgInit)
        {
            Assembly assembly = Assembly.GetAssembly(GetType());
            Stream manifestResourceStream = assembly.GetManifestResourceStream(ResourceName);
            if (manifestResourceStream == null)
            {
                throw new FormatException(SR.Get(SRID.RecognizerInvalidBinaryGrammar));
            }
            try
            {
                ScriptRef[] array = CfgGrammar.LoadIL(manifestResourceStream);
                if (array == null)
                {
                    throw new ArgumentException(SR.Get(SRID.CannotLoadDotNetSemanticCode));
                }
                _scripts = array;
            }
            catch (Exception innerException)
            {
                throw new ArgumentException(SR.Get(SRID.CannotLoadDotNetSemanticCode), innerException);
            }
            manifestResourceStream.Position = 0L;
            _ruleName = CheckRuleName(manifestResourceStream, GetType().Name, isImportedGrammar: false, stgInit, out _sapi53Only, out _semanticTag);
            _isStg = true;
            return manifestResourceStream;
        }

        private static MemoryStream CombineCfg(string rule, Stream stream, SrgsRule[] extraRules)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                SrgsDocument srgsDocument = new SrgsDocument();
                srgsDocument.TagFormat = SrgsTagFormat.KeyValuePairs;
                foreach (SrgsRule srgsRule in extraRules)
                {
                    srgsDocument.Rules.Add(srgsRule);
                }
                SrgsGrammarCompiler.Compile(srgsDocument, memoryStream);
                using (StreamMarshaler streamHelper = new StreamMarshaler(stream))
                {
                    long position = stream.Position;
                    Backend org = new Backend(streamHelper);
                    stream.Position = position;
                    memoryStream.Position = 0L;
                    MemoryStream memoryStream2 = new MemoryStream();
                    using (StreamMarshaler streamHelper2 = new StreamMarshaler(memoryStream))
                    {
                        Backend extra = new Backend(streamHelper2);
                        Backend backend = Backend.CombineGrammar(rule, org, extra);
                        using (StreamMarshaler streamBuffer = new StreamMarshaler(memoryStream2))
                        {
                            backend.Commit(streamBuffer);
                            memoryStream2.Position = 0L;
                            return memoryStream2;
                        }
                    }
                }
            }
        }

        private SrgsRule[] RunOnInit(bool stg)
        {
            SrgsRule[] result = null;
            bool flag = false;
            string text = ScriptRef.OnInitMethod(_scripts, _ruleName);
            if (text != null)
            {
                if (_proxy != null)
                {
                    result = _proxy.OnInit(text, _parameters, _onInitParameters, out Exception exceptionThrown);
                    flag = true;
                    if (exceptionThrown != null)
                    {
                        throw exceptionThrown;
                    }
                }
                else
                {
                    Type[] array = new Type[_parameters.Length];
                    for (int i = 0; i < _parameters.Length; i++)
                    {
                        array[i] = _parameters[i].GetType();
                    }
                    MethodInfo method = GetType().GetMethod(text, array);
                    if (!(method != null))
                    {
                        throw new ArgumentException(SR.Get(SRID.RuleScriptInvalidParameters, _ruleName, _ruleName));
                    }
                    result = (SrgsRule[])method.Invoke(this, _parameters);
                    flag = true;
                }
            }
            if (!stg && !flag && _parameters != null)
            {
                throw new ArgumentException(SR.Get(SRID.RuleScriptInvalidParameters, _ruleName, _ruleName));
            }
            return result;
        }

        private static string CheckRuleName(Stream stream, string rulename, bool isImportedGrammar, bool stgInit, out bool sapi53Only, out GrammarOptions grammarOptions)
        {
            sapi53Only = false;
            long position = stream.Position;
            using (StreamMarshaler streamHelper = new StreamMarshaler(stream))
            {
                CfgGrammar.CfgSerializedHeader cfgSerializedHeader = null;
                CfgGrammar.CfgHeader cfgHeader = CfgGrammar.ConvertCfgHeader(streamHelper, includeAllGrammarData: false, loadSymbols: true, out cfgSerializedHeader);
                StringBlob pszSymbols = cfgHeader.pszSymbols;
                string text = (cfgHeader.ulRootRuleIndex != uint.MaxValue && cfgHeader.ulRootRuleIndex < cfgHeader.rules.Length) ? pszSymbols.FromOffset(cfgHeader.rules[cfgHeader.ulRootRuleIndex]._nameOffset) : null;
                sapi53Only = ((cfgHeader.GrammarOptions & (GrammarOptions.MssV1 | GrammarOptions.IpaPhoneme | GrammarOptions.W3cV1 | GrammarOptions.STG)) != 0);
                if (text == null && string.IsNullOrEmpty(rulename))
                {
                    throw new ArgumentException(SR.Get(SRID.SapiErrorNoRulesToActivate));
                }
                if (!string.IsNullOrEmpty(rulename))
                {
                    bool flag = false;
                    CfgRule[] rules = cfgHeader.rules;
                    for (int i = 0; i < rules.Length; i++)
                    {
                        CfgRule cfgRule = rules[i];
                        if (pszSymbols.FromOffset(cfgRule._nameOffset) == rulename)
                        {
                            flag = (cfgRule.Export || stgInit || (!isImportedGrammar && (cfgRule.TopLevel || rulename == text)));
                            break;
                        }
                    }
                    if (!flag)
                    {
                        throw new ArgumentException(SR.Get(SRID.RecognizerRuleNotFoundStream, rulename));
                    }
                }
                else
                {
                    rulename = text;
                }
                grammarOptions = (cfgHeader.GrammarOptions & GrammarOptions.TagFormat);
            }
            stream.Position = position;
            return rulename;
        }
    }
}
