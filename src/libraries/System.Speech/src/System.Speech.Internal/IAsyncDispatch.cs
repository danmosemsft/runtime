// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Internal
{
	internal interface IAsyncDispatch
	{
		void Post(object evt);

		void Post(object[] evt);

		void PostOperation(Delegate callback, params object[] parameters);
	}
}
