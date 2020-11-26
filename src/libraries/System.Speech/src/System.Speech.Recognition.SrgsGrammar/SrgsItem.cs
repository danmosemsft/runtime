// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Speech.Internal;
using System.Speech.Internal.SrgsParser;
using System.Text;
using System.Xml;

namespace System.Speech.Recognition.SrgsGrammar
{
	/// <summary>Represents a grammar element that contains phrases or other entities that a user can speak to produce a successful recognition.</summary>
	[Serializable]
	[DebuggerDisplay("{DebuggerDisplayString ()}")]
	[DebuggerTypeProxy(typeof(SrgsItemDebugDisplay))]
	public class SrgsItem : SrgsElement, IItem, IElement
	{
		internal class SrgsItemDebugDisplay
		{
			private float _weight = 1f;

			private float _repeatProbability = 0.5f;

			private int _minRepeat = -1;

			private int _maxRepeat = -1;

			private SrgsElementList _elements;

			public object Weigth => _weight;

			public object MinRepeat => _minRepeat;

			public object MaxRepeat => _maxRepeat;

			public object RepeatProbability => _repeatProbability;

			public object Count => _elements.Count;

			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			public SrgsElement[] AKeys
			{
				get
				{
					SrgsElement[] array = new SrgsElement[_elements.Count];
					for (int i = 0; i < _elements.Count; i++)
					{
						array[i] = _elements[i];
					}
					return array;
				}
			}

			public SrgsItemDebugDisplay(SrgsItem item)
			{
				_weight = item._weight;
				_repeatProbability = item._repeatProbability;
				_minRepeat = item._minRepeat;
				_maxRepeat = item._maxRepeat;
				_elements = item._elements;
			}
		}

		private float _weight = 1f;

		private float _repeatProbability = 0.5f;

		private int _minRepeat = -1;

		private int _maxRepeat = -1;

		private SrgsElementList _elements;

		/// <summary>Gets the collection of objects contained by the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsItem" /> instance.</summary>
		/// <returns>The collection of objects contained by the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsItem" /> instance.</returns>
		public Collection<SrgsElement> Elements => _elements;

		/// <summary>Gets or sets the probability that a user will repeat the contents of this <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsItem" /> instance.</summary>
		/// <returns>The probability, as a floating point value, that the contents of this item will be repeatedly spoken.</returns>
		/// <exception cref="T:System.ArgumentOutOfRangeException">An attempt is made to set <see cref="P:System.Speech.Recognition.SrgsGrammar.SrgsItem.RepeatProbability" /> to a value that is negative or larger than 1.0.</exception>
		public float RepeatProbability
		{
			get
			{
				return _repeatProbability;
			}
			set
			{
				if (value < 0f || value > 1f)
				{
					throw new ArgumentOutOfRangeException("value", SR.Get(SRID.InvalidRepeatProbability, value));
				}
				_repeatProbability = value;
			}
		}

		/// <summary>Gets the minimum number of times that a user must speak the contents of the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsItem" />.</summary>
		/// <returns>The minimum number of times that a user can speak the contents of the item.</returns>
		public int MinRepeat
		{
			get
			{
				if (_minRepeat != -1)
				{
					return _minRepeat;
				}
				return 1;
			}
		}

		/// <summary>Gets the maximum number of times that a user can speak the contents of the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsItem" />.</summary>
		/// <returns>The maximum number of times that a user can speak the contents of the item.</returns>
		public int MaxRepeat
		{
			get
			{
				if (_maxRepeat != -1)
				{
					return _maxRepeat;
				}
				return 1;
			}
		}

		/// <summary>Gets or sets a multiplying factor that adjusts the likelihood that an <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsItem" /> in a <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsOneOf" /> object will be spoken.</summary>
		/// <returns>A floating point value that adjusts the likelihood of this item being spoken.</returns>
		/// <exception cref="T:System.ArgumentOutOfRangeException">An attempt is made to set <see cref="P:System.Speech.Recognition.SrgsGrammar.SrgsItem.Weight" /> to a negative value.</exception>
		public float Weight
		{
			get
			{
				return _weight;
			}
			set
			{
				if (value <= 0f)
				{
					throw new ArgumentOutOfRangeException("value", SR.Get(SRID.InvalidWeight, value));
				}
				_weight = value;
			}
		}

		internal override SrgsElement[] Children
		{
			get
			{
				SrgsElement[] array = new SrgsElement[_elements.Count];
				int num = 0;
				foreach (SrgsElement element in _elements)
				{
					array[num++] = element;
				}
				return array;
			}
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsItem" /> class.</summary>
		public SrgsItem()
		{
			_elements = new SrgsElementList();
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsItem" /> class and specifies its textual contents.</summary>
		/// <param name="text">The text associated with the item.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="text" /> is <see langword="null" />.</exception>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="text" /> is an empty string.</exception>
		public SrgsItem(string text)
			: this()
		{
			Helpers.ThrowIfEmptyOrNull(text, "text");
			_elements.Add(new SrgsText(text));
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsItem" /> class and specifies an array of <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsElement" /> objects to add to this instance.</summary>
		/// <param name="elements">The array of objects to add to the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsItem" /> instance.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="elements" /> is <see langword="null" />.</exception>
		/// <exception cref="T:System.ArgumentException">Any member of the <paramref name="elements" /> array is <see langword="null" />.</exception>
		public SrgsItem(params SrgsElement[] elements)
			: this()
		{
			Helpers.ThrowIfNull(elements, "elements");
			int num = 0;
			while (true)
			{
				if (num < elements.Length)
				{
					if (elements[num] == null)
					{
						break;
					}
					_elements.Add(elements[num]);
					num++;
					continue;
				}
				return;
			}
			throw new ArgumentNullException("elements", SR.Get(SRID.ParamsEntryNullIllegal));
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsItem" /> class and specifies the number of times that its contents must be spoken.</summary>
		/// <param name="repeatCount">The number of times that the item must be spoken.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="repeatCount" /> is negative or is larger than 255.</exception>
		public SrgsItem(int repeatCount)
			: this()
		{
			SetRepeat(repeatCount);
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsItem" /> class and specifies minimum and maximum repetition counts.</summary>
		/// <param name="min">The minimum number of times that the text in the item must be repeated.</param>
		/// <param name="max">The maximum number of times that the text in the item can be repeated.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="min" /> is negative or larger than 255.
		/// <paramref name="max" /> is negative or larger than 255.</exception>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="min" /> is larger than <paramref name="max" />.</exception>
		public SrgsItem(int min, int max)
			: this()
		{
			SetRepeat(min, max);
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsItem" /> class, specifies the text associated with the item, and specifies minimum and maximum repetition counts.</summary>
		/// <param name="min">The minimum number of times that the item must be repeated.</param>
		/// <param name="max">The maximum number of times that the item can be repeated.</param>
		/// <param name="text">The text associated with the item.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="min" /> is negative or larger than 255.
		/// <paramref name="max" /> is negative or larger than 255.</exception>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="min" /> is larger than <paramref name="max" />.</exception>
		public SrgsItem(int min, int max, string text)
			: this(text)
		{
			SetRepeat(min, max);
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsItem" /> class, specifies an array of <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsElement" /> objects to add to this instance, and sets minimum and maximum repetition counts.</summary>
		/// <param name="min">The minimum number of times that the contents of the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsItem" /> object must be repeated.</param>
		/// <param name="max">The maximum number of times that the contents of the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsItem" /> object can be repeated.</param>
		/// <param name="elements">The array of objects to add to the <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsItem" /> instance.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="elements" /> is <see langword="null" />.</exception>
		/// <exception cref="T:System.ArgumentException">Any member of the <paramref name="elements" /> array is <see langword="null" />.</exception>
		public SrgsItem(int min, int max, params SrgsElement[] elements)
			: this(elements)
		{
			SetRepeat(min, max);
		}

		/// <summary>Sets the number of times that the contents of an <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsItem" /> must be spoken.</summary>
		/// <param name="count">The number of times that the item must be spoken.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="count" /> is less than 0 or greater than 255.</exception>
		public void SetRepeat(int count)
		{
			if (count < 0 || count > 255)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			_minRepeat = (_maxRepeat = count);
		}

		/// <summary>Sets the minimum number of times and the maximum number of times that an item can be spoken.</summary>
		/// <param name="minRepeat">The minimum number of times that the item must be spoken.</param>
		/// <param name="maxRepeat">The maximum number of times that the item can be spoken.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="minRepeat" /> is less than zero or larger than 255.
		/// <paramref name="maxRepeat" /> is less than zero or larger than 255.</exception>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="minRepeat" /> is larger than <paramref name="maxRepeat" />.</exception>
		public void SetRepeat(int minRepeat, int maxRepeat)
		{
			if (minRepeat < 0 || minRepeat > 255)
			{
				throw new ArgumentOutOfRangeException("minRepeat", SR.Get(SRID.InvalidMinRepeat, minRepeat));
			}
			if (maxRepeat != int.MaxValue && (maxRepeat < 0 || maxRepeat > 255))
			{
				throw new ArgumentOutOfRangeException("maxRepeat", SR.Get(SRID.InvalidMinRepeat, maxRepeat));
			}
			if (minRepeat > maxRepeat)
			{
				throw new ArgumentException(SR.Get(SRID.MinGreaterThanMax));
			}
			_minRepeat = minRepeat;
			_maxRepeat = maxRepeat;
		}

		/// <summary>Adds an object to the collection of objects contained in this <see cref="T:System.Speech.Recognition.SrgsGrammar.SrgsItem" /> instance.</summary>
		/// <param name="element">The object to add.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="element" /> is <see langword="null" />.</exception>
		public void Add(SrgsElement element)
		{
			Helpers.ThrowIfNull(element, "element");
			Elements.Add(element);
		}

		internal override void WriteSrgs(XmlWriter writer)
		{
			writer.WriteStartElement("item");
			if (!_weight.Equals(1f))
			{
				writer.WriteAttributeString("weight", _weight.ToString("0.########", CultureInfo.InvariantCulture));
			}
			if (!_repeatProbability.Equals(0.5f))
			{
				writer.WriteAttributeString("repeat-prob", _repeatProbability.ToString("0.########", CultureInfo.InvariantCulture));
			}
			if (_minRepeat == _maxRepeat)
			{
				if (_minRepeat != -1)
				{
					writer.WriteAttributeString("repeat", string.Format(CultureInfo.InvariantCulture, "{0}", new object[1]
					{
						_minRepeat
					}));
				}
			}
			else if (_maxRepeat == int.MaxValue || _maxRepeat == -1)
			{
				writer.WriteAttributeString("repeat", string.Format(CultureInfo.InvariantCulture, "{0}-", new object[1]
				{
					_minRepeat
				}));
			}
			else
			{
				int num = (_minRepeat == -1) ? 1 : _minRepeat;
				writer.WriteAttributeString("repeat", string.Format(CultureInfo.InvariantCulture, "{0}-{1}", new object[2]
				{
					num,
					_maxRepeat
				}));
			}
			Type right = null;
			foreach (SrgsElement element in _elements)
			{
				Type type = element.GetType();
				if (type == typeof(SrgsText) && type == right)
				{
					writer.WriteString(" ");
				}
				right = type;
				element.WriteSrgs(writer);
			}
			writer.WriteEndElement();
		}

		internal override string DebuggerDisplayString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (_elements.Count > 7)
			{
				stringBuilder.Append("SrgsItem Count = ");
				stringBuilder.Append(_elements.Count.ToString(CultureInfo.InvariantCulture));
			}
			else
			{
				if (_minRepeat != _maxRepeat || _maxRepeat != -1)
				{
					stringBuilder.Append('[');
					if (_minRepeat == _maxRepeat)
					{
						stringBuilder.Append(_minRepeat.ToString(CultureInfo.InvariantCulture));
					}
					else if (_maxRepeat == int.MaxValue || _maxRepeat == -1)
					{
						stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "{0},-", new object[1]
						{
							_minRepeat
						}));
					}
					else
					{
						int num = (_minRepeat == -1) ? 1 : _minRepeat;
						stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "{0},{1}", new object[2]
						{
							num,
							_maxRepeat
						}));
					}
					stringBuilder.Append("] ");
				}
				bool flag = true;
				foreach (SrgsElement element in _elements)
				{
					if (!flag)
					{
						stringBuilder.Append(' ');
					}
					stringBuilder.Append('{');
					stringBuilder.Append(element.DebuggerDisplayString());
					stringBuilder.Append('}');
					flag = false;
				}
			}
			return stringBuilder.ToString();
		}
	}
}
