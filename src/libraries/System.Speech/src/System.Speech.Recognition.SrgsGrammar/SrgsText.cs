// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Speech.Internal;
using System.Speech.Internal.SrgsParser;
using System.Xml;

namespace System.Speech.Recognition.SrgsGrammar
{
    /// <summary>Represents the textual content of grammar elements defined by the World Wide Web Consortium (W3C) Speech Recognition Grammar Specification (SRGS) Version 1.0.</summary>
    [Serializable]
    [DebuggerDisplay("{DebuggerDisplayString ()}")]
    public class SrgsText : SrgsElement, IElementText, IElement
    {
        private string _text = string.Empty;

        /// <summary>Gets or sets the text contained within the <see cref="System.Speech.Recognition.SrgsGrammar.SrgsText" /> class instance.</summary>
        /// <returns>The text contained within the <see cref="System.Speech.Recognition.SrgsGrammar.SrgsText" /> instance.</returns>
        /// <exception cref="System.ArgumentNullException">An attempt is made to set <see cref="System.Speech.Recognition.SrgsGrammar.SrgsText.Text" /> to <see langword="null" />.</exception>
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                Helpers.ThrowIfNull(value, nameof(value));
                XmlParser.ParseText(null, value, null, null, -1f, null);
                _text = value;
            }
        }

        /// <summary>Initializes a new instance of the <see cref="System.Speech.Recognition.SrgsGrammar.SrgsText" /> class.</summary>
        public SrgsText()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="System.Speech.Recognition.SrgsGrammar.SrgsText" /> class, specifying the text of the instance.</summary>
        /// <param name="text">The value used to set the <see cref="System.Speech.Recognition.SrgsGrammar.SrgsText.Text" /> property on the <see cref="System.Speech.Recognition.SrgsGrammar.SrgsText" /> instance.</param>
        /// <exception cref="System.ArgumentNullException">
        ///   <paramref name="text" /> is <see langword="null" />.</exception>
        public SrgsText(string text)
        {
            Helpers.ThrowIfNull(text, nameof(text));
            Text = text;
        }

        internal override void WriteSrgs(XmlWriter writer)
        {
            if (_text != null && _text.Length > 0)
            {
                writer.WriteString(_text);
            }
        }

        internal override string DebuggerDisplayString()
        {
            return "'" + _text + "'";
        }
    }
}
