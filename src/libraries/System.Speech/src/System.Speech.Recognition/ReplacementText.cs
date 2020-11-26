// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace System.Speech.Recognition
{
	/// <summary>Contains information about a speech normalization procedure that has been performed on recognition results.</summary>
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public class ReplacementText
	{
		private DisplayAttributes _displayAttributes;

		private string _text;

		private int _wordIndex;

		private int _countOfWords;

		/// <summary>Gets information about the leading and trailing spaces for the text replaced by the speech normalization procedure.</summary>
		/// <returns>Returns a <see cref="T:System.Speech.Recognition.DisplayAttributes" /> object that specifies the use of white space to display text replaced by normalization.</returns>
		public DisplayAttributes DisplayAttributes => _displayAttributes;

		/// <summary>Gets the recognized text replaced by the speech normalization procedure.</summary>
		/// <returns>Returns the recognized text replaced by the speech normalization procedure.</returns>
		public string Text => _text;

		/// <summary>Gets the location of the first recognized word replaced by the speech normalization procedure.</summary>
		/// <returns>Returns the location of the first recognized word replaced by the speech normalization procedure.</returns>
		public int FirstWordIndex => _wordIndex;

		/// <summary>Gets the number of recognized words replaced by the speech normalization procedure.</summary>
		/// <returns>Returns the number of recognized words replaced by the speech normalization procedure.</returns>
		public int CountOfWords => _countOfWords;

		internal ReplacementText(DisplayAttributes displayAttributes, string text, int wordIndex, int countOfWords)
		{
			_displayAttributes = displayAttributes;
			_text = text;
			_wordIndex = wordIndex;
			_countOfWords = countOfWords;
		}
	}
}
