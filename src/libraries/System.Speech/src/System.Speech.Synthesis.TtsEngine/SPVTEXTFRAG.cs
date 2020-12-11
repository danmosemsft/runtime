// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace System.Speech.Synthesis.TtsEngine
{
	[StructLayout(LayoutKind.Sequential)]
	internal class SPVTEXTFRAG
	{
		public IntPtr pNext;

		public SPVSTATE State;

		public IntPtr pTextStart;

		public int ulTextLen;

		public int ulTextSrcOffset;

		public GCHandle gcText;

		public GCHandle gcNext;

		public GCHandle gcPhoneme;

		public GCHandle gcSayAsCategory;
	}
}
