// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace System.Speech.Internal.SapiInterop
{
	[ComImport]
	[Guid("8FC6D974-C81E-4098-93C5-0147F61ED4D3")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface ISpRecognizer2
	{
		[PreserveSig]
		int EmulateRecognitionEx(ISpPhrase pPhrase, uint dwCompareFlags);

		void SetTrainingState(bool fDoingTraining, bool fAdaptFromTrainingData);

		void ResetAcousticModelAdaptation();
	}
}
