// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Speech.Internal;
using System.Speech.Internal.GrammarBuilding;

namespace System.Speech.Recognition
{
    /// <summary>Represents a set of alternatives in the constraints of a speech recognition grammar.</summary>
    [DebuggerDisplay("{_oneOf.DebugSummary}")]
    public class Choices
    {
        private OneOfElement _oneOf = new OneOfElement();

        internal OneOfElement OneOf => _oneOf;

        /// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.Choices" /> class that contains an empty set of alternatives.</summary>
        public Choices()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.Choices" /> class from an array containing one or more <see cref="T:System.String" /> objects.</summary>
        /// <param name="phrases">An array containing the set of alternatives.</param>
        public Choices(params string[] phrases)
        {
            Helpers.ThrowIfNull(phrases, nameof(phrases));
            Add(phrases);
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.Choices" /> class from an array containing one or more <see cref="T:System.Speech.Recognition.GrammarBuilder" /> objects.</summary>
        /// <param name="alternateChoices">An array containing the set of alternatives.</param>
        public Choices(params GrammarBuilder[] alternateChoices)
        {
            Helpers.ThrowIfNull(alternateChoices, nameof(alternateChoices));
            Add(alternateChoices);
        }

        /// <summary>Adds an array containing one or more <see cref="T:System.String" /> objects to the set of alternatives.</summary>
        /// <param name="phrases">The strings to add to this <see cref="T:System.Speech.Recognition.Choices" /> object.</param>
        public void Add(params string[] phrases)
        {
            Helpers.ThrowIfNull(phrases, nameof(phrases));
            foreach (string text in phrases)
            {
                Helpers.ThrowIfEmptyOrNull(text, "phrase");
                _oneOf.Add(text);
            }
        }

        /// <summary>Adds an array containing one or more <see cref="T:System.Speech.Recognition.GrammarBuilder" /> objects to the set of alternatives.</summary>
        /// <param name="alternateChoices">The <see cref="T:System.Speech.Recognition.GrammarBuilder" /> objects to add to this <see cref="T:System.Speech.Recognition.Choices" /> object.</param>
        public void Add(params GrammarBuilder[] alternateChoices)
        {
            Helpers.ThrowIfNull(alternateChoices, nameof(alternateChoices));
            foreach (GrammarBuilder grammarBuilder in alternateChoices)
            {
                Helpers.ThrowIfNull(grammarBuilder, "alternateChoice");
                _oneOf.Items.Add(new ItemElement(grammarBuilder));
            }
        }

        /// <summary>Returns a <see cref="T:System.Speech.Recognition.GrammarBuilder" /> object from this <see cref="T:System.Speech.Recognition.Choices" /> object.</summary>
        /// <returns>A <see cref="T:System.Speech.Recognition.GrammarBuilder" /> that matches this <see cref="T:System.Speech.Recognition.Choices" /> object.</returns>
        public GrammarBuilder ToGrammarBuilder()
        {
            return new GrammarBuilder(this);
        }
    }
}
