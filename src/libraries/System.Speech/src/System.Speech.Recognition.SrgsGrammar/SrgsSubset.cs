// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Speech.Internal;
using System.Speech.Internal.SrgsParser;
using System.Xml;

namespace System.Speech.Recognition.SrgsGrammar
{
	/// <summary>Defines methods and properties that can be used to match a given string with a spoken phrase.</summary>
	[Serializable]
	[DebuggerDisplay("{DebuggerDisplayString ()}")]
	public class SrgsSubset : SrgsElement, ISubset, IElement
	{
		private SubsetMatchingMode _matchMode;

		private string _text;

		/// <summary>Gets or sets the matching mode for the subset.</summary>
		/// <returns>A member of the <see cref="T:System.Speech.Recognition.SubsetMatchingMode" /> enumeration.</returns>
		/// <exception cref="T:System.ArgumentException">An attempt is made to set <see cref="P:System.Speech.Recognition.SrgsGrammar.SrgsSubset.MatchingMode" /> to a value that is not a member of the <see cref="T:System.Speech.Recognition.SubsetMatchingMode" /> enumeration.</exception>
		public SubsetMatchingMode MatchingMode
		{
			get
			{
				return _matchMode;
			}
			set
			{
				if (value != SubsetMatchingMode.OrderedSubset && value != 0 && value != SubsetMatchingMode.OrderedSubsetContentRequired && value != SubsetMatchingMode.SubsequenceContentRequired)
				{
					throw new ArgumentException(SR.Get(SRID.InvalidSubsetAttribute), "value");
				}
				_matchMode = value;
			}
		}

		/// <summary>Gets or sets as string that contains the portion of a spoken phrase to be matched.</summary>
		/// <returns>A string that contains the portion of a spoken phrase to be matched.</returns>
		/// <exception cref="T:System.ArgumentNullException">An attempt is made to set <see cref="P:System.Speech.Recognition.SrgsGrammar.SrgsSubset.Text" /> to <see langword="null" /> or to an empty string.</exception>
		/// <exception cref="T:System.ArgumentException">An attempt is made to set <see cref="P:System.Speech.Recognition.SrgsGrammar.SrgsSubset.Text" /> using a string that contains only white space characters (' ', '\t', '\n', '\r').</exception>
		public string Text
		{
			get
			{
				return _text;
			}
			set
			{
				Helpers.ThrowIfEmptyOrNull(value, "value");
				value = value.Trim(Helpers._achTrimChars);
				Helpers.ThrowIfEmptyOrNull(value, "value");
				_text = value;
			}
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsSubset" /> class, specifying the portion of the phrase to be matched.</summary>
		/// <param name="text">The portion of the phrase to be matched.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="text" /> is <see langword="null" />.</exception>
		public SrgsSubset(string text)
			: this(text, SubsetMatchingMode.Subsequence)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsSubset" /> class, specifying the portion to be matched and the mode in which the text should be matched.</summary>
		/// <param name="text">The portion of the phrase to be matched.</param>
		/// <param name="matchingMode">The mode in which <paramref name="text" /> should be matched with the spoken phrase.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="text" /> is <see langword="null" />.</exception>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="text" /> is empty.
		/// <paramref name="text" /> contains only white space characters (that is, ' ', '\t', '\n', '\r').
		/// <paramref name="matchingMode" /> is set to a value in the <see cref="T:System.Speech.Recognition.SubsetMatchingMode" /> enumeration.</exception>
		public SrgsSubset(string text, SubsetMatchingMode matchingMode)
		{
			Helpers.ThrowIfEmptyOrNull(text, "text");
			if (matchingMode != SubsetMatchingMode.OrderedSubset && matchingMode != 0 && matchingMode != SubsetMatchingMode.OrderedSubsetContentRequired && matchingMode != SubsetMatchingMode.SubsequenceContentRequired)
			{
				throw new ArgumentException(SR.Get(SRID.InvalidSubsetAttribute), "matchingMode");
			}
			_matchMode = matchingMode;
			_text = text.Trim(Helpers._achTrimChars);
			Helpers.ThrowIfEmptyOrNull(_text, "text");
		}

		internal override void WriteSrgs(XmlWriter writer)
		{
			writer.WriteStartElement("sapi", "subset", "http://schemas.microsoft.com/Speech/2002/06/SRGSExtensions");
			if (_matchMode != 0)
			{
				string value = null;
				switch (_matchMode)
				{
				case SubsetMatchingMode.Subsequence:
					value = "subsequence";
					break;
				case SubsetMatchingMode.OrderedSubset:
					value = "ordered-subset";
					break;
				case SubsetMatchingMode.SubsequenceContentRequired:
					value = "subsequence-content-required";
					break;
				case SubsetMatchingMode.OrderedSubsetContentRequired:
					value = "ordered-subset-content-required";
					break;
				}
				writer.WriteAttributeString("sapi", "match", "http://schemas.microsoft.com/Speech/2002/06/SRGSExtensions", value);
			}
			if (_text != null && _text.Length > 0)
			{
				writer.WriteString(_text);
			}
			writer.WriteEndElement();
		}

		internal override void Validate(SrgsGrammar grammar)
		{
			grammar.HasSapiExtension = true;
			base.Validate(grammar);
		}

		internal override string DebuggerDisplayString()
		{
			return _text + " [" + _matchMode.ToString() + "]";
		}
	}
}
