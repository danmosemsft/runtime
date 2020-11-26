// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Speech.Internal;
using System.Speech.Internal.GrammarBuilding;

namespace System.Speech.Recognition
{
	/// <summary>Associates a key string with <see cref="T:System.Speech.Recognition.SemanticResultValue" /> values to define <see cref="T:System.Speech.Recognition.SemanticValue" /> objects.</summary>
	[DebuggerDisplay("{_semanticKey.DebugSummary}")]
	public class SemanticResultKey
	{
		private readonly SemanticKeyElement _semanticKey;

		internal SemanticKeyElement SemanticKeyElement => _semanticKey;

		private SemanticResultKey(string semanticResultKey)
		{
			Helpers.ThrowIfEmptyOrNull(semanticResultKey, "semanticResultKey");
			_semanticKey = new SemanticKeyElement(semanticResultKey);
		}

		/// <summary>Assigns a semantic key to one or more <see cref="T:System.String" /> instances used to create a speech recognition grammar.</summary>
		/// <param name="semanticResultKey">The tag to be used access the <see cref="T:System.Speech.Recognition.SemanticValue" /> instance associated with the <see cref="T:System.String" /> objects specified by the <paramref name="phrases" /> argument.</param>
		/// <param name="phrases">One or more <see cref="T:System.String" /> objects, whose concatenated text will be associated with a <see cref="T:System.Speech.Recognition.SemanticValue" /> object accessible with the tag defined in <paramref name="semanticResultKey" />.</param>
		public SemanticResultKey(string semanticResultKey, params string[] phrases)
			: this(semanticResultKey)
		{
			Helpers.ThrowIfEmptyOrNull(semanticResultKey, "semanticResultKey");
			Helpers.ThrowIfNull(phrases, "phrases");
			foreach (string text in phrases)
			{
				_semanticKey.Add((string)text.Clone());
			}
		}

		/// <summary>Assigns a semantic key to one or more <see cref="T:System.Speech.Recognition.GrammarBuilder" /> objects used to create a speech recognition grammar.</summary>
		/// <param name="semanticResultKey">The tag to be used as a semantic key to access the <see cref="T:System.Speech.Recognition.SemanticValue" /> instance associated with the <see cref="T:System.Speech.Recognition.GrammarBuilder" /> objects specified by the <paramref name="builders" /> argument.</param>
		/// <param name="builders">An array of grammar components that will be associated with a <see cref="T:System.Speech.Recognition.SemanticValue" /> object accessible with the tag defined in <paramref name="semanticResultKey" />.</param>
		public SemanticResultKey(string semanticResultKey, params GrammarBuilder[] builders)
			: this(semanticResultKey)
		{
			Helpers.ThrowIfEmptyOrNull(semanticResultKey, "semanticResultKey");
			Helpers.ThrowIfNull(builders, "phrases");
			foreach (GrammarBuilder grammarBuilder in builders)
			{
				_semanticKey.Add(grammarBuilder.Clone());
			}
		}

		/// <summary>Returns an instance of <see cref="T:System.Speech.Recognition.GrammarBuilder" /> constructed from the current <see cref="T:System.Speech.Recognition.SemanticResultKey" /> instance.</summary>
		public GrammarBuilder ToGrammarBuilder()
		{
			return new GrammarBuilder(this);
		}
	}
}
