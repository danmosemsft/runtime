// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using System.Speech.AudioFormat;
using System.Speech.Internal;

namespace System.Speech.Recognition
{
    /// <summary>Represents audio input that is associated with a <see cref="T:System.Speech.Recognition.RecognitionResult" />.</summary>
    [Serializable]
    public class RecognizedAudio
    {
        private DateTime _startTime;

        private TimeSpan _audioPosition;

        private TimeSpan _audioDuration;

        private SpeechAudioFormatInfo _audioFormat;

        private byte[] _rawAudioData;

        /// <summary>Gets the format of the audio processed by a recognition engine.</summary>
        /// <returns>The format of the audio processed by the speech recognizer.</returns>
        public SpeechAudioFormatInfo Format => _audioFormat;

        /// <summary>Gets the system time at the start of the recognition operation.</summary>
        /// <returns>The system time at the start of the recognition operation.</returns>
        public DateTime StartTime => _startTime;

        /// <summary>Gets the location in the input audio stream for the start of the recognized audio.</summary>
        /// <returns>The location in the input audio stream for the start of the recognized audio.</returns>
        public TimeSpan AudioPosition => _audioPosition;

        /// <summary>Gets the duration of the input audio stream for the recognized audio.</summary>
        /// <returns>The duration within the input audio stream for the recognized audio.</returns>
        public TimeSpan Duration => _audioDuration;

        internal RecognizedAudio(byte[] rawAudioData, SpeechAudioFormatInfo audioFormat, DateTime startTime, TimeSpan audioPosition, TimeSpan audioDuration)
        {
            _audioFormat = audioFormat;
            _startTime = startTime;
            _audioPosition = audioPosition;
            _audioDuration = audioDuration;
            _rawAudioData = rawAudioData;
        }

        /// <summary>Writes audio to a stream in Wave format.</summary>
        /// <param name="outputStream">The stream that will receive the audio data.</param>
        public void WriteToWaveStream(Stream outputStream)
        {
            Helpers.ThrowIfNull(outputStream, "outputStream");
            using (StreamMarshaler sm = new StreamMarshaler(outputStream))
            {
                WriteWaveHeader(sm);
            }
            outputStream.Write(_rawAudioData, 0, _rawAudioData.Length);
            outputStream.Flush();
        }

        /// <summary>Writes the entire audio to a stream as raw data.</summary>
        /// <param name="outputStream">The stream that will receive the audio data.</param>
        public void WriteToAudioStream(Stream outputStream)
        {
            Helpers.ThrowIfNull(outputStream, "outputStream");
            outputStream.Write(_rawAudioData, 0, _rawAudioData.Length);
            outputStream.Flush();
        }

        /// <summary>Selects and returns a section of the current recognized audio as binary data.</summary>
        /// <param name="audioPosition">The starting point of the audio data to be returned.</param>
        /// <param name="duration">The length of the segment to be returned.</param>
        /// <returns>Returns a subsection of the recognized audio, as defined by <paramref name="audioPosition" /> and <paramref name="duration" />.</returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///   <paramref name="audioPosition" /> and <paramref name="duration" /> define a segment of audio outside the range of the current segment.</exception>
        /// <exception cref="T:System.InvalidOperationException">The current recognized audio contains no data.</exception>
        public RecognizedAudio GetRange(TimeSpan audioPosition, TimeSpan duration)
        {
            if (audioPosition.Ticks < 0)
            {
                throw new ArgumentOutOfRangeException("audioPosition", SR.Get(SRID.NegativeTimesNotSupported));
            }
            if (duration.Ticks < 0)
            {
                throw new ArgumentOutOfRangeException("duration", SR.Get(SRID.NegativeTimesNotSupported));
            }
            if (audioPosition > _audioDuration)
            {
                throw new ArgumentOutOfRangeException("audioPosition");
            }
            if (duration > audioPosition + _audioDuration)
            {
                throw new ArgumentOutOfRangeException("duration");
            }
            int num = (int)(_audioFormat.BitsPerSample * _audioFormat.SamplesPerSecond * audioPosition.Ticks / 80000000);
            int num2 = (int)(_audioFormat.BitsPerSample * _audioFormat.SamplesPerSecond * duration.Ticks / 80000000);
            if (num + num2 > _rawAudioData.Length)
            {
                num2 = _rawAudioData.Length - num;
            }
            byte[] array = new byte[num2];
            Array.Copy(_rawAudioData, num, array, 0, num2);
            return new RecognizedAudio(array, _audioFormat, _startTime + audioPosition, audioPosition, duration);
        }

        private void WriteWaveHeader(StreamMarshaler sm)
        {
            char[] array = new char[4]
            {
                'R',
                'I',
                'F',
                'F'
            };
            byte[] array2 = _audioFormat.FormatSpecificData();
            sm.WriteArray(array, array.Length);
            sm.WriteStream((uint)(_rawAudioData.Length + 38 + array2.Length));
            char[] array3 = new char[4]
            {
                'W',
                'A',
                'V',
                'E'
            };
            sm.WriteArray(array3, array3.Length);
            char[] array4 = new char[4]
            {
                'f',
                'm',
                't',
                ' '
            };
            sm.WriteArray(array4, array4.Length);
            sm.WriteStream(18 + array2.Length);
            sm.WriteStream((ushort)_audioFormat.EncodingFormat);
            sm.WriteStream((ushort)_audioFormat.ChannelCount);
            sm.WriteStream(_audioFormat.SamplesPerSecond);
            sm.WriteStream(_audioFormat.AverageBytesPerSecond);
            sm.WriteStream((ushort)_audioFormat.BlockAlign);
            sm.WriteStream((ushort)_audioFormat.BitsPerSample);
            sm.WriteStream((ushort)array2.Length);
            if (array2.Length != 0)
            {
                sm.WriteStream(array2);
            }
            char[] array5 = new char[4]
            {
                'd',
                'a',
                't',
                'a'
            };
            sm.WriteArray(array5, array5.Length);
            sm.WriteStream(_rawAudioData.Length);
        }
    }
}
