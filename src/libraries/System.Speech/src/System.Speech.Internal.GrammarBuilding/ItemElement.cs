// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Speech.Internal.SrgsParser;
using System.Speech.Recognition;

namespace System.Speech.Internal.GrammarBuilding
{
	internal sealed class ItemElement : BuilderElements
	{
		private readonly int _minRepeat = 1;

		private readonly int _maxRepeat = 1;

		internal ItemElement(GrammarBuilderBase builder)
			: this(builder, 1, 1)
		{
		}

		internal ItemElement(int minRepeat, int maxRepeat)
			: this((GrammarBuilderBase)null, minRepeat, maxRepeat)
		{
		}

		internal ItemElement(GrammarBuilderBase builder, int minRepeat, int maxRepeat)
		{
			if (builder != null)
			{
				Add(builder);
			}
			_minRepeat = minRepeat;
			_maxRepeat = maxRepeat;
		}

		internal ItemElement(List<GrammarBuilderBase> builders, int minRepeat, int maxRepeat)
		{
			foreach (GrammarBuilderBase builder in builders)
			{
				base.Items.Add(builder);
			}
			_minRepeat = minRepeat;
			_maxRepeat = maxRepeat;
		}

		internal ItemElement(GrammarBuilder builders)
		{
			foreach (GrammarBuilderBase item in builders.InternalBuilder.Items)
			{
				base.Items.Add(item);
			}
		}

		public override bool Equals(object obj)
		{
			ItemElement itemElement = obj as ItemElement;
			if (itemElement == null)
			{
				return false;
			}
			if (!base.Equals(obj))
			{
				return false;
			}
			if (_minRepeat == itemElement._minRepeat)
			{
				return _maxRepeat == itemElement._maxRepeat;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		internal override GrammarBuilderBase Clone()
		{
			ItemElement itemElement = new ItemElement(_minRepeat, _maxRepeat);
			itemElement.CloneItems(this);
			return itemElement;
		}

		internal override IElement CreateElement(IElementFactory elementFactory, IElement parent, IRule rule, IdentifierCollection ruleIds)
		{
			IItem item = elementFactory.CreateItem(parent, rule, _minRepeat, _maxRepeat, 0.5f, 1f);
			CreateChildrenElements(elementFactory, item, rule, ruleIds);
			return item;
		}
	}
}
