// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Internal
{
	internal class AsyncWorkItem
	{
		private Delegate _dynamicCallback;

		private object[] _postData;

		internal AsyncWorkItem(Delegate dynamicCallback, params object[] postData)
		{
			_dynamicCallback = dynamicCallback;
			_postData = postData;
		}

		internal void Invoke()
		{
			if ((object)_dynamicCallback != null)
			{
				_dynamicCallback.DynamicInvoke(_postData);
			}
		}
	}
}
