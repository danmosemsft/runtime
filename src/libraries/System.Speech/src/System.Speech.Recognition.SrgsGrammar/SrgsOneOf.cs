using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Speech.Internal;
using System.Speech.Internal.SrgsParser;
using System.Text;
using System.Xml;

namespace System.Speech.Recognition.SrgsGrammar
{
	/// <summary>Represents a list of alternative words or phrases, any one of which may be used to match speech input.</summary>
	[Serializable]
	[DebuggerDisplay("{DebuggerDisplayString ()}")]
	[DebuggerTypeProxy(typeof(OneOfDebugDisplay))]
	public class SrgsOneOf : SrgsElement, IOneOf, IElement
	{
		internal class OneOfDebugDisplay
		{
			private Collection<SrgsItem> _items;

			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			public SrgsItem[] AKeys
			{
				get
				{
					SrgsItem[] array = new SrgsItem[_items.Count];
					for (int i = 0; i < _items.Count; i++)
					{
						array[i] = _items[i];
					}
					return array;
				}
			}

			public OneOfDebugDisplay(SrgsOneOf oneOf)
			{
				_items = oneOf._items;
			}
		}

		private SrgsItemList _items = new SrgsItemList();

		/// <summary>Gets the list of all the alternatives contained in the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsOneOf" /> element.</summary>
		/// <returns>Returns the list of alternatives.</returns>
		public Collection<SrgsItem> Items => _items;

		internal override SrgsElement[] Children
		{
			get
			{
				SrgsElement[] array = new SrgsElement[_items.Count];
				int num = 0;
				foreach (SrgsItem item in _items)
				{
					array[num++] = item;
				}
				return array;
			}
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsOneOf" /> class.</summary>
		public SrgsOneOf()
		{
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsOneOf" /> class from an array of <see cref="T:System.String" /> objects.</summary>
		/// <param name="items">The alternative items to add.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="items" /> is <see langword="null" />.  
		/// Any element in the <paramref name="items" /> array is <see langword="null" />.</exception>
		public SrgsOneOf(params string[] items)
			: this()
		{
			Helpers.ThrowIfNull(items, "items");
			int num = 0;
			while (true)
			{
				if (num < items.Length)
				{
					if (items[num] == null)
					{
						break;
					}
					_items.Add(new SrgsItem(items[num]));
					num++;
					continue;
				}
				return;
			}
			throw new ArgumentNullException("items", SR.Get(SRID.ParamsEntryNullIllegal));
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsOneOf" /> class from an array of <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsItem" /> objects.</summary>
		/// <param name="items">The alternative items to add.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="items" /> is <see langword="null" />.  
		/// Any element in the <paramref name="items" /> array is <see langword="null" />.</exception>
		public SrgsOneOf(params SrgsItem[] items)
			: this()
		{
			Helpers.ThrowIfNull(items, "items");
			int num = 0;
			while (true)
			{
				if (num < items.Length)
				{
					SrgsItem srgsItem = items[num];
					if (srgsItem == null)
					{
						break;
					}
					_items.Add(srgsItem);
					num++;
					continue;
				}
				return;
			}
			throw new ArgumentNullException("items", SR.Get(SRID.ParamsEntryNullIllegal));
		}

		/// <summary>Adds an <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsItem" /> containing a word or a phrase to the list of alternatives.</summary>
		/// <param name="item">The item to add to the list of alternatives.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="item" /> is <see langword="null" />.</exception>
		public void Add(SrgsItem item)
		{
			Helpers.ThrowIfNull(item, "item");
			Items.Add(item);
		}

		internal override void WriteSrgs(XmlWriter writer)
		{
			writer.WriteStartElement("one-of");
			foreach (SrgsItem item in _items)
			{
				item.WriteSrgs(writer);
			}
			writer.WriteEndElement();
		}

		internal override string DebuggerDisplayString()
		{
			StringBuilder stringBuilder = new StringBuilder("SrgsOneOf Count = ");
			stringBuilder.Append(_items.Count);
			return stringBuilder.ToString();
		}
	}
}
