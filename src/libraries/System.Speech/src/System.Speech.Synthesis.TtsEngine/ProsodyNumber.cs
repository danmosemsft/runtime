// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

namespace System.Speech.Synthesis.TtsEngine
{
	/// <summary>Specifies prosody attributes and their values.</summary>
	[ImmutableObject(true)]
	public struct ProsodyNumber : IEquatable<ProsodyNumber>
	{
		/// <summary>Holds a value that represents a setting for a prosody attribute.</summary>
		public const int AbsoluteNumber = int.MaxValue;

		private int _ssmlAttributeId;

		private bool _isPercent;

		private float _number;

		private ProsodyUnit _unit;

		/// <summary>Gets the identifier for an SSML prosody attribute.</summary>
		/// <returns>The identifier for an SSML prosody attribute.</returns>
		public int SsmlAttributeId
		{
			get
			{
				return _ssmlAttributeId;
			}
			internal set
			{
				_ssmlAttributeId = value;
			}
		}

		/// <summary>Gets whether the <c>Number</c> property represents a percent value.</summary>
		/// <returns>
		///   <see langword="true" /> if the <see cref="P:System.Speech.Synthesis.TtsEngine.ProsodyNumber.Number" /> represents a percent value, otherwise, <see langword="false" />.</returns>
		public bool IsNumberPercent
		{
			get
			{
				return _isPercent;
			}
			internal set
			{
				_isPercent = value;
			}
		}

		/// <summary>Gets a numeric value for an SSML prosody attribute.</summary>
		/// <returns>The numerical value for an SSML prosody attribute.</returns>
		public float Number
		{
			get
			{
				return _number;
			}
			internal set
			{
				_number = value;
			}
		}

		/// <summary>Gets the unit in which the amount of change is specified.</summary>
		/// <returns>The unit in which the amount of change is specified, for example Hz (Hertz) or semitone.</returns>
		public ProsodyUnit Unit
		{
			get
			{
				return _unit;
			}
			internal set
			{
				_unit = value;
			}
		}

		/// <summary>Creates a new instance of the <c>ProsodyNumber</c> class and specifies the identifier for a prosody attribute.</summary>
		/// <param name="ssmlAttributeId">The identifier for a prosody attribute.</param>
		public ProsodyNumber(int ssmlAttributeId)
		{
			_ssmlAttributeId = ssmlAttributeId;
			_number = 1f;
			_isPercent = true;
			_unit = ProsodyUnit.Default;
		}

		/// <summary>Creates a new instance of the <c>ProsodyNumber</c> class and specifies a value for a prosody attribute.</summary>
		/// <param name="number">A value for a prosody attribute.</param>
		public ProsodyNumber(float number)
		{
			_ssmlAttributeId = int.MaxValue;
			_number = number;
			_isPercent = false;
			_unit = ProsodyUnit.Default;
		}

		/// <summary>Determines whether two instances of <c>ProsodyNumber</c> are equal.</summary>
		/// <param name="prosodyNumber1">The <see cref="T:System.Speech.Synthesis.TtsEngine.ProsodyNumber" /> object to compare to <paramref name="prosodyNumber2" />.</param>
		/// <param name="prosodyNumber2">The  <see cref="T:System.Speech.Synthesis.TtsEngine.ProsodyNumber" /> object to compare to <paramref name="prosodyNumber1" />.</param>
		/// <returns>
		///   <see langword="true" /> if <paramref name="prosodyNumber1" /> is the same as <paramref name="prosodyNumber2" />;  otherwise, <see langword="false" />.</returns>
		public static bool operator ==(ProsodyNumber prosodyNumber1, ProsodyNumber prosodyNumber2)
		{
			if (prosodyNumber1._ssmlAttributeId == prosodyNumber2._ssmlAttributeId && prosodyNumber1.Number.Equals(prosodyNumber2.Number) && prosodyNumber1.IsNumberPercent == prosodyNumber2.IsNumberPercent)
			{
				return prosodyNumber1.Unit == prosodyNumber2.Unit;
			}
			return false;
		}

		/// <summary>Determines whether two instances of <c>ProsodyNumber</c> are not equal.</summary>
		/// <param name="prosodyNumber1">The <see cref="T:System.Speech.Synthesis.TtsEngine.ProsodyNumber" /> object to compare to <paramref name="prosodyNumber2" />.</param>
		/// <param name="prosodyNumber2">The <see cref="T:System.Speech.Synthesis.TtsEngine.ProsodyNumber" /> object to compare to <paramref name="prosodyNumber1" />.</param>
		/// <returns>
		///   <see langword="true" /> if <paramref name="prosodyNumber1" /> is different from <paramref name="prosodyNumber2" />; otherwise, <see langword="false" />.</returns>
		public static bool operator !=(ProsodyNumber prosodyNumber1, ProsodyNumber prosodyNumber2)
		{
			return !(prosodyNumber1 == prosodyNumber2);
		}

		/// <summary>Determines whether a specified <c>ProsodyNumber</c> object is equal to the current instance of <c>ProsodyNumber</c>.</summary>
		/// <param name="other">The <see cref="T:System.Speech.Synthesis.TtsEngine.ProsodyNumber" /> object to evaluate.</param>
		/// <returns>
		///   <see langword="true" /> if <paramref name="other" /> is equal to the current <see cref="T:System.Speech.Synthesis.TtsEngine.ProsodyNumber" /> object; otherwise, <see langword="false" />.</returns>
		public bool Equals(ProsodyNumber other)
		{
			return this == other;
		}

		/// <summary>Determines whether a specified object is an instance of <c>ProsodyNumber</c> and equal to the current instance of <c>ProsodyNumber</c>.</summary>
		/// <param name="obj">The <see cref="T:System.Object" /> to evaluate.</param>
		/// <returns>
		///   <see langword="true" /> if <paramref name="obj" /> is equal to the current <see cref="T:System.Speech.Synthesis.TtsEngine.ProsodyNumber" /> object; otherwise, <see langword="false" />.</returns>
		public override bool Equals(object obj)
		{
			if (!(obj is ProsodyNumber))
			{
				return false;
			}
			return Equals((ProsodyNumber)obj);
		}

		/// <summary>Provides a hash code for a <c>ProsodyNumber</c> object.</summary>
		/// <returns>A hash code for a <see cref="T:System.Speech.Synthesis.TtsEngine.ProsodyNumber" /> object.</returns>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
