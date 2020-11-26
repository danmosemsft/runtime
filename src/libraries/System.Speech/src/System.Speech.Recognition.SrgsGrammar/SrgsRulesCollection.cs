// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;
using System.Speech.Internal;

namespace System.Speech.Recognition.SrgsGrammar
{
	/// <summary>Represents a collection of <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsRule" /> objects.</summary>
	[Serializable]
	public sealed class SrgsRulesCollection : KeyedCollection<string, SrgsRule>
	{
		/// <summary>Adds the contents of an array of <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsRule" /> objects to the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsRulesCollection" /> object.</summary>
		/// <param name="rules">The array of rule objects to add to the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsRulesCollection" /> object.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="rules" /> is <see langword="null" />.</exception>
		/// <exception cref="T:System.ArgumentException">Any <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsRule" /> object in the <paramref name="rules" /> array is <see langword="null" />.</exception>
		public void Add(params SrgsRule[] rules)
		{
			Helpers.ThrowIfNull(rules, "rules");
			int num = 0;
			while (true)
			{
				if (num < rules.Length)
				{
					if (rules[num] == null)
					{
						break;
					}
					base.Add(rules[num]);
					num++;
					continue;
				}
				return;
			}
			throw new ArgumentNullException("rules", SR.Get(SRID.ParamsEntryNullIllegal));
		}

		protected override string GetKeyForItem(SrgsRule rule)
		{
			if (rule == null)
			{
				throw new ArgumentNullException("rule");
			}
			return rule.Id;
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsRulesCollection" /> class.</summary>
		public SrgsRulesCollection()
		{
		}
	}
}
