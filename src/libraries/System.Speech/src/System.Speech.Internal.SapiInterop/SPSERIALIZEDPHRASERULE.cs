// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace System.Speech.Internal.SapiInterop
{
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	internal class SPSERIALIZEDPHRASERULE
	{
		internal uint pszNameOffset;

		internal uint ulId;

		internal uint ulFirstElement;

		internal uint ulCountOfElements;

		internal uint NextSiblingOffset;

		internal uint FirstChildOffset;

		internal float SREngineConfidence;

		internal sbyte Confidence;
	}
}
