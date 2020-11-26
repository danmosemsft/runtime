// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Speech.Internal;
using System.Speech.Internal.GrammarBuilding;
using System.Speech.Internal.SrgsCompiler;
using System.Speech.Internal.SrgsParser;
using System.Speech.Recognition.SrgsGrammar;
using System.Text;

namespace System.Speech.Recognition
{
    /// <summary>Provides a mechanism for programmatically building the constraints for a speech recognition grammar.</summary>
    [DebuggerDisplay("{DebugSummary}")]
    public class GrammarBuilder
    {
        private class InternalGrammarBuilder : BuilderElements
        {
            internal override GrammarBuilderBase Clone()
            {
                InternalGrammarBuilder internalGrammarBuilder = new InternalGrammarBuilder();
                foreach (GrammarBuilderBase item in base.Items)
                {
                    internalGrammarBuilder.Items.Add(item.Clone());
                }
                return internalGrammarBuilder;
            }

            internal override IElement CreateElement(IElementFactory elementFactory, IElement parent, IRule rule, IdentifierCollection ruleIds)
            {
                Collection<RuleElement> collection = new Collection<RuleElement>();
                CalcCount(null);
                Optimize(collection);
                foreach (RuleElement item in collection)
                {
                    base.Items.Add(item);
                }
                string text = ruleIds.CreateNewIdentifier("root");
                elementFactory.Grammar.Root = text;
                elementFactory.Grammar.TagFormat = SrgsTagFormat.KeyValuePairs;
                IRule rule2 = elementFactory.Grammar.CreateRule(text, RulePublic.False, RuleDynamic.NotSet, hasSCript: false);
                foreach (GrammarBuilderBase item2 in base.Items)
                {
                    if (item2 is RuleElement)
                    {
                        item2.CreateElement(elementFactory, rule2, rule2, ruleIds);
                    }
                }
                foreach (GrammarBuilderBase item3 in base.Items)
                {
                    if (!(item3 is RuleElement))
                    {
                        IElement element = item3.CreateElement(elementFactory, rule2, rule2, ruleIds);
                        if (element != null)
                        {
                            element.PostParse(rule2);
                            elementFactory.AddElement(rule2, element);
                        }
                    }
                }
                rule2.PostParse(elementFactory.Grammar);
                elementFactory.Grammar.PostParse(null);
                return null;
            }
        }

        private InternalGrammarBuilder _grammarBuilder;

        private CultureInfo _culture = CultureInfo.CurrentUICulture;

        /// <summary>Gets a string that shows the contents and structure of the grammar contained by the <see cref="T:System.Speech.Recognition.GrammarBuilder" />.</summary>
        /// <returns>The current content and structure of the <see cref="T:System.Speech.Recognition.GrammarBuilder" />.</returns>
        public string DebugShowPhrases => DebugSummary;

        /// <summary>Gets or sets the culture of the speech recognition grammar.</summary>
        /// <returns>The culture of the <see cref="T:System.Speech.Recognition.GrammarBuilder" />. The default is the executing thread's <see cref="P:System.Threading.Thread.CurrentUICulture" /> property.</returns>
        public CultureInfo Culture
        {
            get
            {
                return _culture;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                _culture = value;
            }
        }

        internal virtual string DebugSummary
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (GrammarBuilderBase item in InternalBuilder.Items)
                {
                    if (stringBuilder.Length > 0)
                    {
                        stringBuilder.Append(' ');
                    }
                    stringBuilder.Append(item.DebugSummary);
                }
                return stringBuilder.ToString();
            }
        }

        internal BuilderElements InternalBuilder => _grammarBuilder;

        /// <summary>Initializes a new, empty instance of the <see cref="T:System.Speech.Recognition.GrammarBuilder" /> class.</summary>
        public GrammarBuilder()
        {
            _grammarBuilder = new InternalGrammarBuilder();
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.GrammarBuilder" /> class from a sequence of words.</summary>
        /// <param name="phrase">The sequence of words.</param>
        public GrammarBuilder(string phrase)
            : this()
        {
            Append(phrase);
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.GrammarBuilder" /> class for a subset of a sequence of words.</summary>
        /// <param name="phrase">The sequence of words.</param>
        /// <param name="subsetMatchingCriteria">The matching mode the speech recognition grammar uses to recognize the phrase.</param>
        public GrammarBuilder(string phrase, SubsetMatchingMode subsetMatchingCriteria)
            : this()
        {
            Append(phrase, subsetMatchingCriteria);
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.GrammarBuilder" /> class from the sequence of words in a <see cref="T:System.String" /> and specifies how many times the <see cref="T:System.String" /> can be repeated.</summary>
        /// <param name="phrase">The repeated sequence of words.</param>
        /// <param name="minRepeat">The minimum number of times that input matching the phrase must occur to constitute a match.</param>
        /// <param name="maxRepeat">The maximum number of times that input matching the phrase can occur to constitute a match.</param>
        public GrammarBuilder(string phrase, int minRepeat, int maxRepeat)
            : this()
        {
            Append(phrase, minRepeat, maxRepeat);
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.GrammarBuilder" /> class from a repeated element.</summary>
        /// <param name="builder">The repeated element.</param>
        /// <param name="minRepeat">The minimum number of times that input matching the element defined by <paramref name="builder" /> must occur to constitute a match.</param>
        /// <param name="maxRepeat">The maximum number of times that input matching the element defined by <paramref name="builder" /> can occur to constitute a match.</param>
        public GrammarBuilder(GrammarBuilder builder, int minRepeat, int maxRepeat)
            : this()
        {
            Append(builder, minRepeat, maxRepeat);
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.GrammarBuilder" /> class from a set of alternatives.</summary>
        /// <param name="alternateChoices">The set of alternatives.</param>
        public GrammarBuilder(Choices alternateChoices)
            : this()
        {
            Append(alternateChoices);
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.GrammarBuilder" /> class from a semantic key.</summary>
        /// <param name="key">The semantic key.</param>
        public GrammarBuilder(SemanticResultKey key)
            : this()
        {
            Append(key);
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.GrammarBuilder" /> class from a semantic value.</summary>
        /// <param name="value">The semantic value or name/value pair.</param>
        public GrammarBuilder(SemanticResultValue value)
            : this()
        {
            Append(value);
        }

        /// <summary>Appends a phrase to the current sequence of grammar elements.</summary>
        /// <param name="phrase">The sequence of words to append.</param>
        public void Append(string phrase)
        {
            Helpers.ThrowIfEmptyOrNull(phrase, nameof(phrase));
            AddItem(new GrammarBuilderPhrase(phrase));
        }

        /// <summary>Appends an element for a subset of a phrase to the current sequence of grammar elements.</summary>
        /// <param name="phrase">The sequence of words to append.</param>
        /// <param name="subsetMatchingCriteria">The matching mode the grammar uses to recognize the phrase.</param>
        public void Append(string phrase, SubsetMatchingMode subsetMatchingCriteria)
        {
            Helpers.ThrowIfEmptyOrNull(phrase, nameof(phrase));
            ValidateSubsetMatchingCriteriaArgument(subsetMatchingCriteria, nameof(subsetMatchingCriteria));
            AddItem(new GrammarBuilderPhrase(phrase, subsetMatchingCriteria));
        }

        /// <summary>Appends a repeated phrase to the current sequence of grammar elements.</summary>
        /// <param name="phrase">The repeated sequence of words to append.</param>
        /// <param name="minRepeat">The minimum number of times that input matching <paramref name="phrase" /> must occur to constitute a match.</param>
        /// <param name="maxRepeat">The maximum number of times that input matching <paramref name="phrase" /> can occur to constitute a match.</param>
        public void Append(string phrase, int minRepeat, int maxRepeat)
        {
            Helpers.ThrowIfEmptyOrNull(phrase, nameof(phrase));
            ValidateRepeatArguments(minRepeat, maxRepeat, "minRepeat", "maxRepeat");
            GrammarBuilderPhrase grammarBuilderPhrase = new GrammarBuilderPhrase(phrase);
            if (minRepeat != 1 || maxRepeat != 1)
            {
                AddItem(new ItemElement(grammarBuilderPhrase, minRepeat, maxRepeat));
            }
            else
            {
                AddItem(grammarBuilderPhrase);
            }
        }

        /// <summary>Appends a grammar element to the current sequence of grammar elements.</summary>
        /// <param name="builder">The grammar element to append.</param>
        public void Append(GrammarBuilder builder)
        {
            Helpers.ThrowIfNull(builder, nameof(builder));
            Helpers.ThrowIfNull(builder.InternalBuilder, "builder.InternalBuilder");
            Helpers.ThrowIfNull(builder.InternalBuilder.Items, "builder.InternalBuilder.Items");
            foreach (GrammarBuilderBase item in builder.InternalBuilder.Items)
            {
                if (item == null)
                {
                    throw new ArgumentException(SR.Get(SRID.ArrayOfNullIllegal), nameof(builder));
                }
            }
            List<GrammarBuilderBase> list = (builder == this) ? builder.Clone().InternalBuilder.Items : builder.InternalBuilder.Items;
            foreach (GrammarBuilderBase item2 in list)
            {
                AddItem(item2);
            }
        }

        /// <summary>Appends a set of alternatives to the current sequence of grammar elements.</summary>
        /// <param name="alternateChoices">The set of alternatives to append.</param>
        public void Append(Choices alternateChoices)
        {
            Helpers.ThrowIfNull(alternateChoices, nameof(alternateChoices));
            AddItem(alternateChoices.OneOf);
        }

        /// <summary>Appends a semantic key to the current sequence of grammar elements.</summary>
        /// <param name="key">The semantic key to append.</param>
        public void Append(SemanticResultKey key)
        {
            Helpers.ThrowIfNull(key, "builder");
            AddItem(key.SemanticKeyElement);
        }

        /// <summary>Appends a semantic value to the current sequence of grammar elements.</summary>
        /// <param name="value">The semantic value to append.</param>
        public void Append(SemanticResultValue value)
        {
            Helpers.ThrowIfNull(value, "builder");
            AddItem(value.Tag);
        }

        /// <summary>Appends a repeated grammar element to the current sequence of grammar elements.</summary>
        /// <param name="builder">The repeated grammar element to append.</param>
        /// <param name="minRepeat">The minimum number of times that input matching the element defined by <paramref name="builder" /> must occur to constitute a match.</param>
        /// <param name="maxRepeat">The maximum number of times that input matching the element defined by <paramref name="builder" /> can occur to constitute a match.</param>
        public void Append(GrammarBuilder builder, int minRepeat, int maxRepeat)
        {
            Helpers.ThrowIfNull(builder, nameof(builder));
            ValidateRepeatArguments(minRepeat, maxRepeat, "minRepeat", "maxRepeat");
            Helpers.ThrowIfNull(builder.InternalBuilder, "builder.InternalBuilder");
            if (minRepeat != 1 || maxRepeat != 1)
            {
                AddItem(new ItemElement(builder.InternalBuilder.Items, minRepeat, maxRepeat));
            }
            else
            {
                Append(builder);
            }
        }

        /// <summary>Appends the default dictation grammar to the current sequence of grammar elements.</summary>
        public void AppendDictation()
        {
            AddItem(new GrammarBuilderDictation());
        }

        /// <summary>Appends the specified dictation grammar to the current sequence of grammar elements.</summary>
        /// <param name="category">The category of the dictation grammar to append.</param>
        public void AppendDictation(string category)
        {
            Helpers.ThrowIfEmptyOrNull(category, nameof(category));
            AddItem(new GrammarBuilderDictation(category));
        }

        /// <summary>Appends a recognition grammar element that matches any input to the current sequence of grammar elements.</summary>
        public void AppendWildcard()
        {
            AddItem(new GrammarBuilderWildcard());
        }

        /// <summary>Appends a grammar definition file to the current sequence of grammar elements.</summary>
        /// <param name="path">The path or Universal Resource Identifier (URI) of the file that describes a speech recognition grammar in a supported format.</param>
        public void AppendRuleReference(string path)
        {
            Helpers.ThrowIfEmptyOrNull(path, nameof(path));
            Uri uri;
            try
            {
                uri = new Uri(path, UriKind.RelativeOrAbsolute);
            }
            catch (UriFormatException ex)
            {
                throw new ArgumentException(ex.Message, path, ex);
            }
            AddItem(new GrammarBuilderRuleRef(uri, null));
        }

        /// <summary>Appends the specified rule of a grammar definition file to the current sequence of grammar elements.</summary>
        /// <param name="path">The file path or Universal Resource Identifier (URI) of the file that describes a speech recognition grammar in a supported format.</param>
        /// <param name="rule">The identifier of the rule to append, or <see langword="null" /> to append the default root rule of the grammar file.</param>
        public void AppendRuleReference(string path, string rule)
        {
            Helpers.ThrowIfEmptyOrNull(path, nameof(path));
            Helpers.ThrowIfEmptyOrNull(rule, nameof(rule));
            Uri uri;
            try
            {
                uri = new Uri(path, UriKind.RelativeOrAbsolute);
            }
            catch (UriFormatException ex)
            {
                throw new ArgumentException(ex.Message, path, ex);
            }
            AddItem(new GrammarBuilderRuleRef(uri, rule));
        }

        /// <summary>Creates a new <see cref="T:System.Speech.Recognition.GrammarBuilder" /> that contains a phrase followed by a <see cref="T:System.Speech.Recognition.GrammarBuilder" />.</summary>
        /// <param name="phrase">The first grammar element, which represents a sequence of words.</param>
        /// <param name="builder">The second grammar element.</param>
        /// <returns>Returns a <see cref="T:System.Speech.Recognition.GrammarBuilder" /> for the sequence of the <paramref name="phrase" /> parameter followed by the <paramref name="builder" /> parameter.</returns>
        public static GrammarBuilder operator +(string phrase, GrammarBuilder builder)
        {
            return Add(phrase, builder);
        }

        /// <summary>Creates a new <see cref="T:System.Speech.Recognition.GrammarBuilder" /> that contains a phrase followed by a <see cref="T:System.Speech.Recognition.GrammarBuilder" /> object.</summary>
        /// <param name="phrase">The first grammar element, which represents a sequence of words.</param>
        /// <param name="builder">The second grammar element.</param>
        /// <returns>A <see cref="T:System.Speech.Recognition.GrammarBuilder" /> for the sequence of the <paramref name="phrase" /> element followed by the <paramref name="builder" /> element.</returns>
        public static GrammarBuilder Add(string phrase, GrammarBuilder builder)
        {
            Helpers.ThrowIfNull(builder, nameof(builder));
            GrammarBuilder grammarBuilder = new GrammarBuilder(phrase);
            grammarBuilder.Append(builder);
            return grammarBuilder;
        }

        /// <summary>Creates a new <see cref="T:System.Speech.Recognition.GrammarBuilder" /> that contains a <see cref="T:System.Speech.Recognition.GrammarBuilder" /> followed by a phrase.</summary>
        /// <param name="builder">The first grammar element.</param>
        /// <param name="phrase">The second grammar element, which represents a sequence of words.</param>
        /// <returns>Returns a <see cref="T:System.Speech.Recognition.GrammarBuilder" /> for the sequence of the <paramref name="builder" /> parameter followed by the <paramref name="phrase" /> parameter.</returns>
        public static GrammarBuilder operator +(GrammarBuilder builder, string phrase)
        {
            return Add(builder, phrase);
        }

        /// <summary>Creates a new <see cref="T:System.Speech.Recognition.GrammarBuilder" /> that contains a <see cref="T:System.Speech.Recognition.GrammarBuilder" /> object followed by a phrase.</summary>
        /// <param name="builder">The first grammar element.</param>
        /// <param name="phrase">The second grammar element, which represents a sequence of words.</param>
        /// <returns>A <see cref="T:System.Speech.Recognition.GrammarBuilder" /> for the sequence of the <paramref name="builder" /> element followed by the <paramref name="phrase" /> element.</returns>
        public static GrammarBuilder Add(GrammarBuilder builder, string phrase)
        {
            Helpers.ThrowIfNull(builder, nameof(builder));
            GrammarBuilder grammarBuilder = builder.Clone();
            grammarBuilder.Append(phrase);
            return grammarBuilder;
        }

        /// <summary>Creates a new <see cref="T:System.Speech.Recognition.GrammarBuilder" /> that contains a <see cref="T:System.Speech.Recognition.Choices" /> object followed by a <see cref="T:System.Speech.Recognition.GrammarBuilder" /> object.</summary>
        /// <param name="choices">The first grammar element, which represents a set of alternatives.</param>
        /// <param name="builder">The second grammar element.</param>
        /// <returns>Returns a <see cref="T:System.Speech.Recognition.GrammarBuilder" /> for the sequence of the <paramref name="choices" /> parameter followed by the <paramref name="builder" /> parameter.</returns>
        public static GrammarBuilder operator +(Choices choices, GrammarBuilder builder)
        {
            return Add(choices, builder);
        }

        /// <summary>Creates a new <see cref="T:System.Speech.Recognition.GrammarBuilder" /> that contains a <see cref="T:System.Speech.Recognition.Choices" /> object followed by a <see cref="T:System.Speech.Recognition.GrammarBuilder" /> object.</summary>
        /// <param name="choices">The first grammar element, which represents a set of alternatives.</param>
        /// <param name="builder">The second grammar element.</param>
        /// <returns>A <see cref="T:System.Speech.Recognition.GrammarBuilder" /> for the sequence of the <paramref name="choices" /> element followed by the <paramref name="builder" /> element.</returns>
        public static GrammarBuilder Add(Choices choices, GrammarBuilder builder)
        {
            Helpers.ThrowIfNull(choices, nameof(choices));
            Helpers.ThrowIfNull(builder, nameof(builder));
            GrammarBuilder grammarBuilder = new GrammarBuilder(choices);
            grammarBuilder.Append(builder);
            return grammarBuilder;
        }

        /// <summary>Creates a new <see cref="T:System.Speech.Recognition.GrammarBuilder" /> that contains a <see cref="T:System.Speech.Recognition.GrammarBuilder" /> followed by a <see cref="T:System.Speech.Recognition.Choices" />.</summary>
        /// <param name="builder">The first grammar element.</param>
        /// <param name="choices">The second grammar element, which represents a set of alternative elements.</param>
        /// <returns>Returns a <see cref="T:System.Speech.Recognition.GrammarBuilder" /> for the sequence of the <paramref name="builder" /> parameter followed by the <paramref name="choices" /> parameter.</returns>
        public static GrammarBuilder operator +(GrammarBuilder builder, Choices choices)
        {
            return Add(builder, choices);
        }

        /// <summary>Creates a new <see cref="T:System.Speech.Recognition.GrammarBuilder" /> that contains a <see cref="T:System.Speech.Recognition.GrammarBuilder" /> object followed by a <see cref="T:System.Speech.Recognition.Choices" /> object.</summary>
        /// <param name="builder">The first grammar element.</param>
        /// <param name="choices">The second grammar element, which represents a set of alternatives.</param>
        /// <returns>A <see cref="T:System.Speech.Recognition.GrammarBuilder" /> for the sequence of the <paramref name="builder" /> element followed by the <paramref name="choices" /> element.</returns>
        public static GrammarBuilder Add(GrammarBuilder builder, Choices choices)
        {
            Helpers.ThrowIfNull(builder, nameof(builder));
            Helpers.ThrowIfNull(choices, nameof(choices));
            GrammarBuilder grammarBuilder = builder.Clone();
            grammarBuilder.Append(choices);
            return grammarBuilder;
        }

        /// <summary>Creates a new <see cref="T:System.Speech.Recognition.GrammarBuilder" /> that contains a sequence of two <see cref="T:System.Speech.Recognition.GrammarBuilder" /> objects.</summary>
        /// <param name="builder1">The first grammar element.</param>
        /// <param name="builder2">The second grammar element.</param>
        /// <returns>Returns a <see cref="T:System.Speech.Recognition.GrammarBuilder" /> for the sequence of the <paramref name="builder1" /> parameter followed by the <paramref name="builder2" /> parameter.</returns>
        public static GrammarBuilder operator +(GrammarBuilder builder1, GrammarBuilder builder2)
        {
            return Add(builder1, builder2);
        }

        /// <summary>Creates a new <see cref="T:System.Speech.Recognition.GrammarBuilder" /> that contains a sequence of two <see cref="T:System.Speech.Recognition.GrammarBuilder" /> objects.</summary>
        /// <param name="builder1">The first grammar element.</param>
        /// <param name="builder2">The second grammar element.</param>
        /// <returns>A <see cref="T:System.Speech.Recognition.GrammarBuilder" /> for the sequence of the <paramref name="builder1" /> element followed by the <paramref name="builder2" /> element.</returns>
        public static GrammarBuilder Add(GrammarBuilder builder1, GrammarBuilder builder2)
        {
            Helpers.ThrowIfNull(builder1, nameof(builder1));
            Helpers.ThrowIfNull(builder2, nameof(builder2));
            GrammarBuilder grammarBuilder = builder1.Clone();
            grammarBuilder.Append(builder2);
            return grammarBuilder;
        }

        /// <summary>Converts a string to a <see cref="T:System.Speech.Recognition.GrammarBuilder" /> object.</summary>
        /// <param name="phrase">The string to convert.</param>
        /// <returns>The converted string.</returns>
        public static implicit operator GrammarBuilder(string phrase)
        {
            return new GrammarBuilder(phrase);
        }

        /// <summary>Converts a <see cref="T:System.Speech.Recognition.Choices" /> object to a <see cref="T:System.Speech.Recognition.GrammarBuilder" /> object.</summary>
        /// <param name="choices">The set of alternatives to convert.</param>
        /// <returns>The converted <see cref="T:System.Speech.Recognition.Choices" /> object.</returns>
        public static implicit operator GrammarBuilder(Choices choices)
        {
            return new GrammarBuilder(choices);
        }

        /// <summary>Converts a <see cref="T:System.Speech.Recognition.SemanticResultKey" /> object to a <see cref="T:System.Speech.Recognition.GrammarBuilder" /> object.</summary>
        /// <param name="semanticKey">The semantic key to convert.</param>
        /// <returns>The converted <see cref="T:System.Speech.Recognition.SemanticResultKey" /> object.</returns>
        public static implicit operator GrammarBuilder(SemanticResultKey semanticKey)
        {
            return new GrammarBuilder(semanticKey);
        }

        /// <summary>Converts a <see cref="T:System.Speech.Recognition.SemanticResultValue" /> object to a <see cref="T:System.Speech.Recognition.GrammarBuilder" /> object.</summary>
        /// <param name="semanticValue">The <see cref="T:System.Speech.Recognition.SemanticResultValue" /> object to convert.</param>
        /// <returns>The converted <see cref="T:System.Speech.Recognition.SemanticResultValue" /> object.</returns>
        public static implicit operator GrammarBuilder(SemanticResultValue semanticValue)
        {
            return new GrammarBuilder(semanticValue);
        }

        internal static void ValidateRepeatArguments(int minRepeat, int maxRepeat, string minParamName, string maxParamName)
        {
            if (minRepeat < 0)
            {
                throw new ArgumentOutOfRangeException(minParamName, SR.Get(SRID.InvalidMinRepeat, minRepeat));
            }
            if (minRepeat > maxRepeat)
            {
                throw new ArgumentException(SR.Get(SRID.MinGreaterThanMax), maxParamName);
            }
        }

        internal static void ValidateSubsetMatchingCriteriaArgument(SubsetMatchingMode subsetMatchingCriteria, string paramName)
        {
            if ((uint)subsetMatchingCriteria > 3u)
            {
                throw new ArgumentException(SR.Get(SRID.EnumInvalid, paramName), paramName);
            }
        }

        internal void CreateGrammar(IElementFactory elementFactory)
        {
            IdentifierCollection ruleIds = new IdentifierCollection();
            elementFactory.Grammar.Culture = Culture;
            _grammarBuilder.CreateElement(elementFactory, null, null, ruleIds);
        }

        internal void Compile(Stream stream)
        {
            Backend backend = new Backend();
            CustomGrammar cg = new CustomGrammar();
            SrgsElementCompilerFactory elementFactory = new SrgsElementCompilerFactory(backend, cg);
            CreateGrammar(elementFactory);
            backend.Optimize();
            using (StreamMarshaler streamBuffer = new StreamMarshaler(stream))
            {
                backend.Commit(streamBuffer);
            }
            stream.Position = 0L;
        }

        internal GrammarBuilder Clone()
        {
            GrammarBuilder grammarBuilder = new GrammarBuilder();
            grammarBuilder._grammarBuilder = (InternalGrammarBuilder)_grammarBuilder.Clone();
            return grammarBuilder;
        }

        private void AddItem(GrammarBuilderBase item)
        {
            InternalBuilder.Items.Add(item.Clone());
        }
    }
}
