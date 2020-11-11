using System.Collections.ObjectModel;
using System.Globalization;
using System.Speech.Internal;
using System.Speech.Internal.SrgsCompiler;
using System.Speech.Internal.SrgsParser;
using System.Xml;

namespace System.Speech.Recognition.SrgsGrammar
{
	/// <summary>Defines a design-time object that is used to build strongly-typed runtime grammars that conform to the Speech Recognition Grammar Specification (SRGS) Version 1.0.</summary>
	[Serializable]
	public class SrgsDocument
	{
		private SrgsGrammar _grammar;

		private Uri _baseUri;

		/// <summary>Gets or sets the base URI of the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" /> class.</summary>
		/// <returns>The current base URI of <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" />.</returns>
		public Uri XmlBase
		{
			get
			{
				return _grammar.XmlBase;
			}
			set
			{
				_grammar.XmlBase = value;
			}
		}

		/// <summary>Gets or sets the culture information for the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" /> instance.</summary>
		/// <returns>A <see cref="T:System.Globalization.CultureInfo" /> object that contains the current culture information for <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" />.</returns>
		/// <exception cref="T:System.ArgumentNullException">The value being assigned to <see cref="P:System.Speech.Recognition.SrgsGrammar.SrgsDocument.Culture" /> is <see langword="null" />.</exception>
		/// <exception cref="T:System.ArgumentException">The value being assigned to <see cref="P:System.Speech.Recognition.SrgsGrammar.SrgsDocument.Culture" /> is <see cref="P:System.Globalization.CultureInfo.InvariantCulture" />.</exception>
		public CultureInfo Culture
		{
			get
			{
				return _grammar.Culture;
			}
			set
			{
				Helpers.ThrowIfNull(value, "value");
				if (value.Equals(CultureInfo.InvariantCulture))
				{
					throw new ArgumentException(SR.Get(SRID.InvariantCultureInfo), "value");
				}
				_grammar.Culture = value;
			}
		}

		/// <summary>Gets or sets the root rule of the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" /> class.</summary>
		/// <returns>Returns the rule that is designated as the root rule of the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" />.</returns>
		public SrgsRule Root
		{
			get
			{
				return _grammar.Root;
			}
			set
			{
				_grammar.Root = value;
			}
		}

		/// <summary>Gets or sets the mode for the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" /> class.</summary>
		/// <returns>The recognition mode of the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" />.</returns>
		public SrgsGrammarMode Mode
		{
			get
			{
				if (_grammar.Mode != 0)
				{
					return SrgsGrammarMode.Dtmf;
				}
				return SrgsGrammarMode.Voice;
			}
			set
			{
				_grammar.Mode = ((value != 0) ? GrammarType.DtmfGrammar : GrammarType.VoiceGrammar);
			}
		}

		/// <summary>Gets or sets the phonetic alphabet of the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" /> class.</summary>
		/// <returns>Returns the phonetic alphabet that must be used to specify custom pronunciations in the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsToken" /> object.</returns>
		public SrgsPhoneticAlphabet PhoneticAlphabet
		{
			get
			{
				return (SrgsPhoneticAlphabet)_grammar.PhoneticAlphabet;
			}
			set
			{
				_grammar.PhoneticAlphabet = (AlphabetType)value;
				_grammar.HasPhoneticAlphabetBeenSet = true;
			}
		}

		/// <summary>Gets the collection of rules that are currently defined for the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" /> class.</summary>
		/// <returns>Returns the rules defined for the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" /> object.</returns>
		public SrgsRulesCollection Rules => _grammar.Rules;

		/// <summary>Gets or sets the programming language used for inline code in the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" /> class.</summary>
		/// <returns>The <see cref="P:System.Speech.Recognition.SrgsGrammar.SrgsDocument.Language" /> property returns the programming language to which <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" /> is currently set.</returns>
		public string Language
		{
			get
			{
				return _grammar.Language;
			}
			set
			{
				_grammar.Language = value;
			}
		}

		/// <summary>Gets or sets the namespace of the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" /> class.</summary>
		/// <returns>The <see cref="P:System.Speech.Recognition.SrgsGrammar.SrgsDocument.Namespace" /> property returns the namespace for the current <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" />.</returns>
		public string Namespace
		{
			get
			{
				return _grammar.Namespace;
			}
			set
			{
				_grammar.Namespace = value;
			}
		}

		/// <summary>Gets the code-behind information for the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" /> instance.</summary>
		/// <returns>The <see cref="P:System.Speech.Recognition.SrgsGrammar.SrgsDocument.CodeBehind" /> property returns a string collection that contains a list of the code-behind documents.</returns>
		public Collection<string> CodeBehind => _grammar.CodeBehind;

		/// <summary>Gets or sets whether line numbers should be added to inline scripts.</summary>
		/// <returns>The <see cref="P:System.Speech.Recognition.SrgsGrammar.SrgsDocument.Debug" /> property returns <see langword="true" /> if line numbers should be added for debugging purposes; otherwise the property returns <see langword="false" />.</returns>
		public bool Debug
		{
			get
			{
				return _grammar.Debug;
			}
			set
			{
				_grammar.Debug = value;
			}
		}

		/// <summary>Gets or sets the .NET scripting language for the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" /> class.</summary>
		/// <returns>The <see langword="Script" /> property returns the current .NET scripting language for the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" /> class.</returns>
		/// <exception cref="T:System.ArgumentNullException">An attempt is made to set the <see cref="P:System.Speech.Recognition.SrgsGrammar.SrgsDocument.Script" /> property to null.</exception>
		/// <exception cref="T:System.ArgumentException">An attempt is made to set the <see cref="P:System.Speech.Recognition.SrgsGrammar.SrgsDocument.Script" /> property to an empty string.</exception>
		public string Script
		{
			get
			{
				return _grammar.Script;
			}
			set
			{
				Helpers.ThrowIfEmptyOrNull(value, "value");
				_grammar.Script = value;
			}
		}

		/// <summary>Gets the related namespaces for the current <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" /> instance.</summary>
		/// <returns>The <see cref="P:System.Speech.Recognition.SrgsGrammar.SrgsDocument.ImportNamespaces" /> property returns a string collection that contains a list of the related namespaces in the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" /> instance.</returns>
		public Collection<string> ImportNamespaces => _grammar.ImportNamespaces;

		/// <summary>Gets the assembly reference information for the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" /> instance.</summary>
		/// <returns>The <see cref="P:System.Speech.Recognition.SrgsGrammar.SrgsDocument.AssemblyReferences" /> property returns a string collection containing a list of the assembly references.</returns>
		public Collection<string> AssemblyReferences => _grammar.AssemblyReferences;

		internal SrgsTagFormat TagFormat
		{
			set
			{
				_grammar.TagFormat = value;
			}
		}

		internal Uri BaseUri => _baseUri;

		internal SrgsGrammar Grammar => _grammar;

		/// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" /> class.</summary>
		public SrgsDocument()
		{
			_grammar = new SrgsGrammar();
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" /> class specifying the location of the XML document that is used to fill in the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" /> instance.</summary>
		/// <param name="path">The location of the SRGS XML file.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="path" /> is <see langword="null" />.</exception>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="path" /> is an empty string.</exception>
		public SrgsDocument(string path)
		{
			Helpers.ThrowIfEmptyOrNull(path, "path");
			using (XmlTextReader srgsGrammar = new XmlTextReader(path))
			{
				Load(srgsGrammar);
			}
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" /> class from an instance of <see cref="T:System.Xml.XmlReader" /> that references an XML-format grammar file.</summary>
		/// <param name="srgsGrammar">The <see cref="T:System.Xml.XmlReader" /> object that was created with the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" /> XML instance.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="srgsGrammar" /> is <see langword="null" />.</exception>
		public SrgsDocument(XmlReader srgsGrammar)
		{
			Helpers.ThrowIfNull(srgsGrammar, "srgsGrammar");
			Load(srgsGrammar);
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" /> class from a <see cref="T:System.Speech.Recognition.GrammarBuilder" /> object.</summary>
		/// <param name="builder">The <see cref="T:System.Speech.Recognition.GrammarBuilder" /> object used to create the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" /> instance.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="builder" /> is <see langword="null" />.</exception>
		public SrgsDocument(GrammarBuilder builder)
		{
			Helpers.ThrowIfNull(builder, "builder");
			_grammar = new SrgsGrammar();
			_grammar.Culture = builder.Culture;
			IElementFactory elementFactory = new SrgsElementFactory(_grammar);
			builder.CreateGrammar(elementFactory);
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" /> class and specifies an <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsRule" /> object to be the root rule of the grammar.</summary>
		/// <param name="grammarRootRule">The <see langword="root rule" /> in the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" /> object.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="grammarRootRule" /> is <see langword="null" />.</exception>
		public SrgsDocument(SrgsRule grammarRootRule)
			: this()
		{
			Helpers.ThrowIfNull(grammarRootRule, "grammarRootRule");
			Root = grammarRootRule;
			Rules.Add(grammarRootRule);
		}

		/// <summary>Writes the contents of the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" /> object to an XML-format grammar file that conforms to the Speech Recognition Grammar Specification (SRGS) Version 1.0.</summary>
		/// <param name="srgsGrammar">The <see cref="T:System.Xml.XmlWriter" /> that is used to write the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsDocument" /> instance.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="srgsGrammar" /> is <see langword="null" />.</exception>
		public void WriteSrgs(XmlWriter srgsGrammar)
		{
			Helpers.ThrowIfNull(srgsGrammar, "srgsGrammar");
			_grammar.Validate();
			_grammar.WriteSrgs(srgsGrammar);
		}

		internal void Load(XmlReader srgsGrammar)
		{
			_grammar = new SrgsGrammar();
			_grammar.PhoneticAlphabet = AlphabetType.Sapi;
			XmlParser xmlParser = new XmlParser(srgsGrammar, null);
			xmlParser.ElementFactory = new SrgsElementFactory(_grammar);
			xmlParser.Parse();
			if (!string.IsNullOrEmpty(srgsGrammar.BaseURI))
			{
				_baseUri = new Uri(srgsGrammar.BaseURI);
			}
		}

		internal static GrammarOptions TagFormat2GrammarOptions(SrgsTagFormat value)
		{
			GrammarOptions result = GrammarOptions.KeyValuePairs;
			switch (value)
			{
			case SrgsTagFormat.KeyValuePairs:
				result = GrammarOptions.KeyValuePairSrgs;
				break;
			case SrgsTagFormat.MssV1:
				result = GrammarOptions.MssV1;
				break;
			case SrgsTagFormat.W3cV1:
				result = GrammarOptions.W3cV1;
				break;
			}
			return result;
		}

		internal static SrgsTagFormat GrammarOptions2TagFormat(GrammarOptions value)
		{
			SrgsTagFormat result = SrgsTagFormat.Default;
			switch (value & GrammarOptions.TagFormat)
			{
			case GrammarOptions.MssV1:
				result = SrgsTagFormat.MssV1;
				break;
			case GrammarOptions.W3cV1:
				result = SrgsTagFormat.W3cV1;
				break;
			case GrammarOptions.KeyValuePairs:
			case GrammarOptions.KeyValuePairSrgs:
				result = SrgsTagFormat.KeyValuePairs;
				break;
			}
			return result;
		}
	}
}
