// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Diagnostics;
using System.Speech.Internal;
using System.Speech.Internal.SrgsParser;
using System.Text;
using System.Xml;

namespace System.Speech.Recognition.SrgsGrammar
{
	/// <summary>Represents the grammar element that specifies a reference to a rule.</summary>
	[Serializable]
	[ImmutableObject(true)]
	[DebuggerDisplay("{DebuggerDisplayString()}")]
	public class SrgsRuleRef : SrgsElement, IRuleRef, IElement
	{
		private enum SpecialRuleRefType
		{
			Null,
			Void,
			Garbage
		}

		/// <summary>Defines a rule that is automatically matched in the absence of any audio input.</summary>
		public static readonly SrgsRuleRef Null = new SrgsRuleRef(SpecialRuleRefType.Null);

		/// <summary>Defines a rule that can never be spoken. Inserting VOID into a sequence automatically makes that sequence unspeakable.</summary>
		public static readonly SrgsRuleRef Void = new SrgsRuleRef(SpecialRuleRefType.Void);

		/// <summary>Defines a rule that can match any speech up to the next rule match, the next token, or until the end of spoken input.</summary>
		public static readonly SrgsRuleRef Garbage = new SrgsRuleRef(SpecialRuleRefType.Garbage);

		/// <summary>Defines a rule that can match spoken input as defined by the dictation topic associated with this grammar.</summary>
		public static readonly SrgsRuleRef Dictation = new SrgsRuleRef(new Uri("grammar:dictation"));

		/// <summary>Indicates that speech input can contain spelled-out letters of a word, and that spelled-out letters can be recognized as a word.</summary>
		public static readonly SrgsRuleRef MnemonicSpelling = new SrgsRuleRef(new Uri("grammar:dictation#spelling"));

		private Uri _uri;

		private SpecialRuleRefType _type;

		private string _semanticKey;

		private string _params;

		/// <summary>Gets the URI for the rule that this <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsRuleRef" /> element references.</summary>
		/// <returns>The location of the rule to reference.</returns>
		public Uri Uri => _uri;

		/// <summary>Gets an alias string for the semantic dictionary.</summary>
		/// <returns>An alias string for the semantic dictionary.</returns>
		public string SemanticKey => _semanticKey;

		/// <summary>Gets the initialization parameters for a <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsRuleRef" /> element.</summary>
		/// <returns>The initialization parameters for a <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsRuleRef" /> element.</returns>
		public string Params => _params;

		/// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsRuleRef" /> class and specifies the location of the external grammar file to reference.</summary>
		/// <param name="uri">The location of a grammar file outside the containing grammar.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="uri" /> is <see langword="null" />.</exception>
		public SrgsRuleRef(Uri uri)
		{
			UriInit(uri, null, null, null);
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsRuleRef" /> class, specifying the location of the external grammar file and the identifier of the rule to reference.</summary>
		/// <param name="uri">The location of a grammar file outside the containing grammar.</param>
		/// <param name="rule">The identifier of the rule to reference.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="uri" /> is <see langword="null" />.  
		/// <paramref name="rule" /> is <see langword="null" />.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="rule" /> is empty.</exception>
		public SrgsRuleRef(Uri uri, string rule)
		{
			Helpers.ThrowIfEmptyOrNull(rule, "rule");
			UriInit(uri, rule, null, null);
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsRuleRef" /> class, specifying the location of the external grammar file, the identifier of the rule, and the string alias of the semantic dictionary.</summary>
		/// <param name="uri">The location of a grammar file outside the containing grammar.</param>
		/// <param name="rule">The identifier of the rule to reference.</param>
		/// <param name="semanticKey">An alias string for the semantic dictionary.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="uri" /> is <see langword="null" />.  
		/// <paramref name="semanticKey" /> is <see langword="null" />.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="semanticKey" /> is empty.</exception>
		public SrgsRuleRef(Uri uri, string rule, string semanticKey)
		{
			Helpers.ThrowIfEmptyOrNull(semanticKey, "semanticKey");
			UriInit(uri, rule, semanticKey, null);
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsRuleRef" /> class, specifying the location of the external grammar file, the identifier of the rule, the string alias of the semantic dictionary, and initialization parameters.</summary>
		/// <param name="uri">The location of a grammar file outside the containing grammar.</param>
		/// <param name="rule">The identifier of the rule to reference.</param>
		/// <param name="semanticKey">The semantic key.</param>
		/// <param name="parameters">The initialization parameters for a <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsRuleRef" /> object.</param>
		public SrgsRuleRef(Uri uri, string rule, string semanticKey, string parameters)
		{
			Helpers.ThrowIfEmptyOrNull(parameters, "parameters");
			UriInit(uri, rule, semanticKey, parameters);
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsRuleRef" /> class and specifies the rule to reference.</summary>
		/// <param name="rule">The object to reference.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="rule" /> is <see langword="null" />.</exception>
		public SrgsRuleRef(SrgsRule rule)
		{
			Helpers.ThrowIfNull(rule, "rule");
			_uri = new Uri("#" + rule.Id, UriKind.Relative);
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsRuleRef" /> class, specifying the rule to reference and a string that contains a semantic key.</summary>
		/// <param name="rule">The object to reference.</param>
		/// <param name="semanticKey">The semantic key.</param>
		public SrgsRuleRef(SrgsRule rule, string semanticKey)
			: this(rule)
		{
			Helpers.ThrowIfEmptyOrNull(semanticKey, "semanticKey");
			_semanticKey = semanticKey;
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsRuleRef" /> class, specifying the rule to reference, the string alias of the semantic dictionary, and initialization parameters.</summary>
		/// <param name="rule">The object to reference.</param>
		/// <param name="semanticKey">The semantic key.</param>
		/// <param name="parameters">The initialization parameters for a <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsRuleRef" /> object.</param>
		public SrgsRuleRef(SrgsRule rule, string semanticKey, string parameters)
			: this(rule)
		{
			Helpers.ThrowIfEmptyOrNull(parameters, "parameters");
			_semanticKey = semanticKey;
			_params = parameters;
		}

		private SrgsRuleRef(SpecialRuleRefType type)
		{
			_type = type;
		}

		internal SrgsRuleRef(string semanticKey, string parameters, Uri uri)
		{
			_uri = uri;
			_semanticKey = semanticKey;
			_params = parameters;
		}

		internal override void WriteSrgs(XmlWriter writer)
		{
			writer.WriteStartElement("ruleref");
			if (_uri != null)
			{
				writer.WriteAttributeString("uri", _uri.ToString());
			}
			else
			{
				string value;
				switch (_type)
				{
				case SpecialRuleRefType.Null:
					value = "NULL";
					break;
				case SpecialRuleRefType.Void:
					value = "VOID";
					break;
				case SpecialRuleRefType.Garbage:
					value = "GARBAGE";
					break;
				default:
					XmlParser.ThrowSrgsException(SRID.InvalidSpecialRuleRef);
					value = null;
					break;
				}
				writer.WriteAttributeString("special", value);
			}
			if (_semanticKey != null)
			{
				writer.WriteAttributeString("sapi", "semantic-key", "http://schemas.microsoft.com/Speech/2002/06/SRGSExtensions", _semanticKey);
			}
			if (_params != null)
			{
				writer.WriteAttributeString("sapi", "params", "http://schemas.microsoft.com/Speech/2002/06/SRGSExtensions", _params);
			}
			writer.WriteEndElement();
		}

		internal override void Validate(SrgsGrammar grammar)
		{
			bool flag = _params != null || _semanticKey != null;
			grammar._fContainsCode |= flag;
			grammar.HasSapiExtension |= flag;
			if (_uri != null)
			{
				string text = _uri.ToString();
				if (text[0] == '#')
				{
					bool flag2 = false;
					if (text.IndexOf("#grammar:dictation", StringComparison.Ordinal) == 0 || text.IndexOf("#grammar:dictation#spelling", StringComparison.Ordinal) == 0)
					{
						flag2 = true;
					}
					else
					{
						text = text.Substring(1);
						foreach (SrgsRule rule in grammar.Rules)
						{
							if (rule.Id == text)
							{
								flag2 = true;
								break;
							}
						}
					}
					if (!flag2)
					{
						XmlParser.ThrowSrgsException(SRID.UndefRuleRef, text);
					}
				}
			}
			base.Validate(grammar);
		}

		internal override string DebuggerDisplayString()
		{
			StringBuilder stringBuilder = new StringBuilder("SrgsRuleRef");
			if (_uri != null)
			{
				stringBuilder.Append(" uri='");
				stringBuilder.Append(_uri.ToString());
				stringBuilder.Append("'");
			}
			else
			{
				stringBuilder.Append(" special='");
				stringBuilder.Append(_type.ToString());
				stringBuilder.Append("'");
			}
			return stringBuilder.ToString();
		}

		private void UriInit(Uri uri, string rule, string semanticKey, string initParameters)
		{
			Helpers.ThrowIfNull(uri, "uri");
			if (string.IsNullOrEmpty(rule))
			{
				_uri = uri;
			}
			else
			{
				_uri = new Uri(uri.ToString() + "#" + rule, UriKind.RelativeOrAbsolute);
			}
			_semanticKey = semanticKey;
			_params = initParameters;
		}
	}
}
