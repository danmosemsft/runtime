// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Synthesis.TtsEngine
{
	/// <summary>Provides information about text stream items to be skipped.</summary>
	public class SkipInfo
	{
		private int _type;

		private int _count;

		/// <summary>Gets or sets the type of object to skip.</summary>
		/// <returns>An <see langword="int" /> representing the type of the object.</returns>
		public int Type
		{
			get
			{
				return _type;
			}
			set
			{
				_type = value;
			}
		}

		/// <summary>Gets or sets the number of items to be skipped.</summary>
		/// <returns>An <see langword="int" /> containing the number of items.</returns>
		public int Count
		{
			get
			{
				return _count;
			}
			set
			{
				_count = value;
			}
		}

		internal SkipInfo(int type, int count)
		{
			_type = type;
			_count = count;
		}

		/// <summary>Creates a new instance of the <see langword="SkipInfo" /> object.</summary>
		public SkipInfo()
		{
		}
	}
}
