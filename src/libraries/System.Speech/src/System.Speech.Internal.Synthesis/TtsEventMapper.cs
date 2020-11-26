// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Internal.Synthesis
{
	internal abstract class TtsEventMapper : ITtsEventSink
	{
		private ITtsEventSink _sink;

		internal TtsEventMapper(ITtsEventSink sink)
		{
			_sink = sink;
		}

		protected virtual void SendToOutput(TTSEvent evt)
		{
			if (_sink != null)
			{
				_sink.AddEvent(evt);
			}
		}

		public virtual void AddEvent(TTSEvent evt)
		{
			SendToOutput(evt);
		}

		public virtual void FlushEvent()
		{
			if (_sink != null)
			{
				_sink.FlushEvent();
			}
		}
	}
}
