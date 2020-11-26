// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace System.Speech.Synthesis.TtsEngine
{
	[TypeLibType(16)]
	internal struct SPVPITCH
	{
		public int MiddleAdj;

		public int RangeAdj;
	}
}
