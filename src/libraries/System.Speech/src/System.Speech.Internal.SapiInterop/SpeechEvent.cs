using System.Runtime.InteropServices;
using System.Speech.AudioFormat;

namespace System.Speech.Internal.SapiInterop
{
	internal class SpeechEvent : IDisposable
	{
		private SPEVENTENUM _eventId;

		private SPEVENTLPARAMTYPE _paramType;

		private ulong _audioStreamOffset;

		private ulong _wParam;

		private ulong _lParam;

		private TimeSpan _audioPosition;

		private int _sizeMemoryPressure;

		internal SPEVENTENUM EventId => _eventId;

		internal ulong AudioStreamOffset => _audioStreamOffset;

		internal ulong WParam => _wParam;

		internal ulong LParam => _lParam;

		internal TimeSpan AudioPosition => _audioPosition;

		private SpeechEvent(SPEVENTENUM eEventId, SPEVENTLPARAMTYPE elParamType, ulong ullAudioStreamOffset, IntPtr wParam, IntPtr lParam)
		{
			_eventId = eEventId;
			_paramType = elParamType;
			_audioStreamOffset = ullAudioStreamOffset;
			_wParam = (ulong)wParam.ToInt64();
			_lParam = (ulong)(long)lParam;
			if (_paramType == SPEVENTLPARAMTYPE.SPET_LPARAM_IS_POINTER || _paramType == SPEVENTLPARAMTYPE.SPET_LPARAM_IS_STRING)
			{
				GC.AddMemoryPressure(_sizeMemoryPressure = Marshal.SizeOf((object)_lParam));
			}
		}

		private SpeechEvent(SPEVENT sapiEvent, SpeechAudioFormatInfo audioFormat)
			: this(sapiEvent.eEventId, sapiEvent.elParamType, sapiEvent.ullAudioStreamOffset, sapiEvent.wParam, sapiEvent.lParam)
		{
			if (audioFormat == null || audioFormat.EncodingFormat == (EncodingFormat)0)
			{
				_audioPosition = TimeSpan.Zero;
			}
			else
			{
				_audioPosition = ((audioFormat.AverageBytesPerSecond > 0) ? new TimeSpan((long)(sapiEvent.ullAudioStreamOffset * 10000000 / (ulong)audioFormat.AverageBytesPerSecond)) : TimeSpan.Zero);
			}
		}

		private SpeechEvent(SPEVENTEX sapiEventEx)
			: this(sapiEventEx.eEventId, sapiEventEx.elParamType, sapiEventEx.ullAudioStreamOffset, sapiEventEx.wParam, sapiEventEx.lParam)
		{
			_audioPosition = new TimeSpan((long)sapiEventEx.ullAudioTimeOffset);
		}

		~SpeechEvent()
		{
			Dispose();
		}

		public void Dispose()
		{
			if (_lParam != 0L)
			{
				if (_paramType == SPEVENTLPARAMTYPE.SPET_LPARAM_IS_TOKEN || _paramType == SPEVENTLPARAMTYPE.SPET_LPARAM_IS_OBJECT)
				{
					Marshal.Release((IntPtr)(long)_lParam);
				}
				else if (_paramType == SPEVENTLPARAMTYPE.SPET_LPARAM_IS_POINTER || _paramType == SPEVENTLPARAMTYPE.SPET_LPARAM_IS_STRING)
				{
					Marshal.FreeCoTaskMem((IntPtr)(long)_lParam);
				}
				if (_sizeMemoryPressure > 0)
				{
					GC.RemoveMemoryPressure(_sizeMemoryPressure);
					_sizeMemoryPressure = 0;
				}
				_lParam = 0uL;
			}
			GC.SuppressFinalize(this);
		}

		internal static SpeechEvent TryCreateSpeechEvent(ISpEventSource sapiEventSource, bool additionalSapiFeatures, SpeechAudioFormatInfo audioFormat)
		{
			SpeechEvent result = null;
			uint pulFetched;
			if (additionalSapiFeatures)
			{
				((ISpEventSource2)sapiEventSource).GetEventsEx(1u, out SPEVENTEX pEventArray, out pulFetched);
				if (pulFetched == 1)
				{
					result = new SpeechEvent(pEventArray);
				}
			}
			else
			{
				sapiEventSource.GetEvents(1u, out SPEVENT pEventArray2, out pulFetched);
				if (pulFetched == 1)
				{
					result = new SpeechEvent(pEventArray2, audioFormat);
				}
			}
			return result;
		}
	}
}
