// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Speech.Internal;
using System.Speech.Internal.SrgsParser;
using System.Text;
using System.Xml;

namespace System.Speech.Recognition.SrgsGrammar
{
    /// <summary>Represents a tag that contains ECMAScript that is run when the rule is matched.</summary>
    [Serializable]
    [DebuggerDisplay("{DebuggerDisplayString ()}")]
    public class SrgsSemanticInterpretationTag : SrgsElement, ISemanticTag, IElement
    {
        private string _script = string.Empty;

        /// <summary>Gets or sets the ECMAScript for the tag.</summary>
        /// <returns>A string that contains the semantic interpretation script for the tag.</returns>
        /// <exception cref="System.ArgumentNullException">An attempt is made to set Script to <see langword="null" />.</exception>
        public string Script
        {
            get
            {
                return _script;
            }
            set
            {
                Helpers.ThrowIfNull(value, nameof(value));
                _script = value;
            }
        }

        /// <summary>Creates an instance of the <see cref="System.Speech.Recognition.SrgsGrammar.SrgsSemanticInterpretationTag" /> class.</summary>
        public SrgsSemanticInterpretationTag()
        {
        }

        /// <summary>Creates an instance of the <see cref="System.Speech.Recognition.SrgsGrammar.SrgsSemanticInterpretationTag" /> class, specifying the script contents of the tag.</summary>
        /// <param name="script">A string that contains the ECMAScript for the tag.</param>
        /// <exception cref="System.ArgumentNullException">
        ///   <paramref name="script" /> is <see langword="null" />.</exception>
        public SrgsSemanticInterpretationTag(string script)
        {
            Helpers.ThrowIfNull(script, nameof(script));
            _script = script;
        }

        internal override void Validate(SrgsGrammar grammar)
        {
            if (grammar.TagFormat == SrgsTagFormat.Default)
            {
                grammar.TagFormat |= SrgsTagFormat.W3cV1;
            }
            else if (grammar.TagFormat == SrgsTagFormat.KeyValuePairs)
            {
                XmlParser.ThrowSrgsException(SRID.SapiPropertiesAndSemantics);
            }
        }

        internal override void WriteSrgs(XmlWriter writer)
        {
            string text = Script.Trim(Helpers._achTrimChars);
            writer.WriteStartElement("tag");
            if (!string.IsNullOrEmpty(text))
            {
                writer.WriteString(text);
            }
            writer.WriteEndElement();
        }

        internal override string DebuggerDisplayString()
        {
            StringBuilder stringBuilder = new StringBuilder("SrgsSemanticInterpretationTag '");
            stringBuilder.Append(_script);
            stringBuilder.Append('\'');
            return stringBuilder.ToString();
        }

        void ISemanticTag.Content(IElement parent, string value, int line)
        {
            Script = value;
        }
    }
}
