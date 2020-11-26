// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace System.Speech.Synthesis.TtsEngine
{
	internal struct TextFragmentInterop
	{
		internal FragmentStateInterop _state;

		[MarshalAs(UnmanagedType.LPWStr)]
		internal string _textToSpeak;

		internal int _textOffset;

		internal int _textLength;

		internal static IntPtr FragmentToPtr(List<TextFragment> textFragments, Collection<IntPtr> memoryBlocks)
		{
			TextFragmentInterop textFragmentInterop = default(TextFragmentInterop);
			int count = textFragments.Count;
			int num = Marshal.SizeOf((object)textFragmentInterop);
			IntPtr intPtr = Marshal.AllocCoTaskMem(num * count);
			memoryBlocks.Add(intPtr);
			for (int i = 0; i < count; i++)
			{
				textFragmentInterop._state.FragmentStateToPtr(textFragments[i].State, memoryBlocks);
				textFragmentInterop._textToSpeak = textFragments[i].TextToSpeak;
				textFragmentInterop._textOffset = textFragments[i].TextOffset;
				textFragmentInterop._textLength = textFragments[i].TextLength;
				Marshal.StructureToPtr((object)textFragmentInterop, (IntPtr)((long)intPtr + i * num), fDeleteOld: false);
			}
			return intPtr;
		}
	}
}
