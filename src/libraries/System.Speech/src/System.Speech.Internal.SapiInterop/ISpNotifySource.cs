// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace System.Speech.Internal.SapiInterop
{
	[ComImport]
	[Guid("5EFF4AEF-8487-11D2-961C-00C04F8EE628")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface ISpNotifySource
	{
		void SetNotifySink(ISpNotifySink pNotifySink);

		void SetNotifyWindowMessage(uint hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

		void Slot3();

		void Slot4();

		void Slot5();

		[PreserveSig]
		int WaitForNotifyEvent(uint dwMilliseconds);

		void Slot7();
	}
}
