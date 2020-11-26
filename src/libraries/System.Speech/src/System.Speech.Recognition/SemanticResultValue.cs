// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Speech.Internal;
using System.Speech.Internal.GrammarBuilding;

namespace System.Speech.Recognition
{
    /// <summary>Represents a semantic value and optionally associates the value with a component of a speech recognition grammar.</summary>
    [DebuggerDisplay("{_tag.DebugSummary}")]
    public class SemanticResultValue
    {
        private TagElement _tag;

        internal TagElement Tag => _tag;

        /// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.SemanticResultValue" /> class and specifies a semantic value.</summary>
        /// <param name="value">The value managed by <see cref="T:System.Speech.Recognition.SemanticResultValue" />. Must be of type <see langword="bool" />, <see langword="int" />, <see langword="float" />, or <see langword="string" />.</param>
        public SemanticResultValue(object value)
        {
            Helpers.ThrowIfNull(value, nameof(value));
            _tag = new TagElement(value);
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.SemanticResultValue" /> class and associates a semantic value with a <see cref="T:System.String" /> object.</summary>
        /// <param name="phrase">A phrase to be used in recognition.</param>
        /// <param name="value">The value managed by <see cref="T:System.Speech.Recognition.SemanticResultValue" />. Must be of type <see langword="bool" />, <see langword="int" />, <see langword="float" />, or <see langword="string" />.</param>
        public SemanticResultValue(string phrase, object value)
        {
            Helpers.ThrowIfEmptyOrNull(phrase, nameof(phrase));
            Helpers.ThrowIfNull(value, nameof(value));
            _tag = new TagElement(new GrammarBuilderPhrase((string)phrase.Clone()), value);
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.SemanticResultValue" /> class and associates a semantic value with a <see cref="T:System.Speech.Recognition.GrammarBuilder" /> object.</summary>
        /// <param name="builder">A grammar component to be used in recognition.</param>
        /// <param name="value">The value managed by <see cref="T:System.Speech.Recognition.SemanticResultValue" />. Must be of type <see langword="bool" />, <see langword="int" />, <see langword="float" />, or <see langword="string" />.</param>
        public SemanticResultValue(GrammarBuilder builder, object value)
        {
            Helpers.ThrowIfNull(builder, nameof(builder));
            Helpers.ThrowIfNull(value, nameof(value));
            _tag = new TagElement(builder.Clone(), value);
        }

        /// <summary>Returns an instance of <see cref="T:System.Speech.Recognition.GrammarBuilder" /> constructed from the current <see cref="T:System.Speech.Recognition.SemanticResultValue" /> instance.</summary>
        /// <returns>Returns an instance of <see cref="T:System.Speech.Recognition.GrammarBuilder" /> constructed from the current <see cref="T:System.Speech.Recognition.SemanticResultValue" /> instance.</returns>
        public GrammarBuilder ToGrammarBuilder()
        {
            return new GrammarBuilder(this);
        }
    }
}
