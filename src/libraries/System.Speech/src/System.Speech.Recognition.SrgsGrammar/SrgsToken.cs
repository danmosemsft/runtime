// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Speech.Internal;
using System.Speech.Internal.SrgsParser;
using System.Text;
using System.Xml;

namespace System.Speech.Recognition.SrgsGrammar
{
    /// <summary>Represents a word or short phrase that can be recognized.</summary>
    [Serializable]
    [DebuggerDisplay("{DebuggerDisplayString ()}")]
    public class SrgsToken : SrgsElement, IToken, IElement
    {
        private string _text = string.Empty;

        private string _pronunciation;

        private string _display;

        /// <summary>Gets or sets the written form of the word that should be spoken.</summary>
        /// <returns>The text contained within the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsToken" /> class instance.</returns>
        /// <exception cref="T:System.ArgumentNullException">An attempt is made to set <see cref="P:System.Speech.Recognition.SrgsGrammar.SrgsToken.Text" /> to <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">An attempt is made to assign an empty string to <see cref="P:System.Speech.Recognition.SrgsGrammar.SrgsToken.Text" />.</exception>
        /// <exception cref="T:System.ArgumentException">An attempt is made to assign a string that contains a quotation mark (") to <see cref="P:System.Speech.Recognition.SrgsGrammar.SrgsToken.Text" />.</exception>
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                Helpers.ThrowIfEmptyOrNull(value, nameof(value));
                string text = value.Trim(Helpers._achTrimChars);
                if (string.IsNullOrEmpty(text) || text.IndexOf('"') >= 0)
                {
                    throw new ArgumentException(SR.Get(SRID.InvalidTokenString), nameof(value));
                }
                _text = text;
            }
        }

        /// <summary>Gets or sets the string that defines the pronunciation for the token.</summary>
        /// <returns>Returns a string containing phones from the phonetic alphabet specified in <see cref="P:System.Speech.Recognition.SrgsGrammar.SrgsDocument.PhoneticAlphabet" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">An attempt is made to set <see cref="P:System.Speech.Recognition.SrgsGrammar.SrgsToken.Pronunciation" /> to <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">An attempt is made to assign an empty string to <see cref="P:System.Speech.Recognition.SrgsGrammar.SrgsToken.Pronunciation" />.</exception>
        public string Pronunciation
        {
            get
            {
                return _pronunciation;
            }
            set
            {
                Helpers.ThrowIfEmptyOrNull(value, nameof(value));
                _pronunciation = value;
            }
        }

        /// <summary>Gets or sets the display form of the text to be spoken.</summary>
        /// <returns>A representation of the token as it should be displayed.</returns>
        /// <exception cref="T:System.ArgumentNullException">An attempt is made to set <see cref="P:System.Speech.Recognition.SrgsGrammar.SrgsToken.Display" /> to <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">An attempt is made to assign an empty string to <see cref="P:System.Speech.Recognition.SrgsGrammar.SrgsToken.Display" />.</exception>
        public string Display
        {
            get
            {
                return _display;
            }
            set
            {
                Helpers.ThrowIfEmptyOrNull(value, nameof(value));
                _display = value;
            }
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsToken" /> class and specifies the text to be recognized.</summary>
        /// <param name="text">The text of the new <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsToken" /> class instance.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="text" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///   <paramref name="text" /> is empty.</exception>
        public SrgsToken(string text)
        {
            Helpers.ThrowIfEmptyOrNull(text, nameof(text));
            Text = text;
        }

        internal override void WriteSrgs(XmlWriter writer)
        {
            writer.WriteStartElement("token");
            if (_display != null)
            {
                writer.WriteAttributeString("sapi", "display", "http://schemas.microsoft.com/Speech/2002/06/SRGSExtensions", _display);
            }
            if (_pronunciation != null)
            {
                writer.WriteAttributeString("sapi", "pron", "http://schemas.microsoft.com/Speech/2002/06/SRGSExtensions", _pronunciation);
            }
            if (_text != null && _text.Length > 0)
            {
                writer.WriteString(_text);
            }
            writer.WriteEndElement();
        }

        internal override void Validate(SrgsGrammar grammar)
        {
            if (_pronunciation != null || _display != null)
            {
                grammar.HasPronunciation = true;
            }
            if (_pronunciation != null)
            {
                int num = 0;
                int num2 = 0;
                while (num < _pronunciation.Length)
                {
                    num2 = _pronunciation.IndexOf(';', num);
                    if (num2 == -1)
                    {
                        num2 = _pronunciation.Length;
                    }
                    string text = _pronunciation.Substring(num, num2 - num);
                    switch (grammar.PhoneticAlphabet)
                    {
                        case AlphabetType.Sapi:
                            PhonemeConverter.ConvertPronToId(text, grammar.Culture.LCID);
                            break;
                        case AlphabetType.Ups:
                            PhonemeConverter.UpsConverter.ConvertPronToId(text);
                            break;
                        case AlphabetType.Ipa:
                            PhonemeConverter.ValidateUpsIds(text.ToCharArray());
                            break;
                    }
                    num = num2 + 1;
                }
            }
            base.Validate(grammar);
        }

        internal override string DebuggerDisplayString()
        {
            StringBuilder stringBuilder = new StringBuilder("Token '");
            stringBuilder.Append(_text);
            stringBuilder.Append('\'');
            if (_pronunciation != null)
            {
                stringBuilder.Append(" Pronunciation '");
                stringBuilder.Append(_pronunciation);
                stringBuilder.Append('\'');
            }
            return stringBuilder.ToString();
        }
    }
}
