// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Speech.Internal.Synthesis;

namespace System.Speech.AudioFormat
{
    /// <summary>Represents information about an audio format.</summary>
    [Serializable]
    public class SpeechAudioFormatInfo
    {
        private int _averageBytesPerSecond;

        private short _bitsPerSample;

        private short _blockAlign;

        private EncodingFormat _encodingFormat;

        private short _channelCount;

        private int _samplesPerSecond;

        private byte[] _formatSpecificData;

        /// <summary>Gets the average bytes per second of the audio.</summary>
        /// <returns>The value for the average bytes per second.</returns>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public int AverageBytesPerSecond => _averageBytesPerSecond;

        /// <summary>Gets the bits per sample of the audio.</summary>
        /// <returns>The value for the bits per sample.</returns>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public int BitsPerSample => _bitsPerSample;

        /// <summary>Gets or sets the block alignment in bytes.</summary>
        /// <returns>The value for the block alignment.</returns>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public int BlockAlign => _blockAlign;

        /// <summary>Gets the encoding format of the audio.</summary>
        /// <returns>The encoding format of the audio.</returns>
        public EncodingFormat EncodingFormat => _encodingFormat;

        /// <summary>Gets the channel count of the audio.</summary>
        /// <returns>The value for the channel count.</returns>
        public int ChannelCount => _channelCount;

        /// <summary>Gets the samples per second of the audio format.</summary>
        /// <returns>The value for the samples per second.</returns>
        public int SamplesPerSecond => _samplesPerSecond;

        internal byte[] WaveFormat
        {
            get
            {
                WAVEFORMATEX wAVEFORMATEX = default(WAVEFORMATEX);
                wAVEFORMATEX.wFormatTag = (short)EncodingFormat;
                wAVEFORMATEX.nChannels = (short)ChannelCount;
                wAVEFORMATEX.nSamplesPerSec = SamplesPerSecond;
                wAVEFORMATEX.nAvgBytesPerSec = AverageBytesPerSecond;
                wAVEFORMATEX.nBlockAlign = (short)BlockAlign;
                wAVEFORMATEX.wBitsPerSample = (short)BitsPerSample;
                wAVEFORMATEX.cbSize = (short)FormatSpecificData().Length;
                byte[] array = wAVEFORMATEX.ToBytes();
                if (wAVEFORMATEX.cbSize > 0)
                {
                    byte[] array2 = new byte[array.Length + wAVEFORMATEX.cbSize];
                    Array.Copy(array, array2, array.Length);
                    Array.Copy(FormatSpecificData(), 0, array2, array.Length, wAVEFORMATEX.cbSize);
                    array = array2;
                }
                return array;
            }
        }

        private SpeechAudioFormatInfo(EncodingFormat encodingFormat, int samplesPerSecond, short bitsPerSample, short channelCount, byte[] formatSpecificData)
        {
            if (encodingFormat == (EncodingFormat)0)
            {
                throw new ArgumentException(SR.Get(SRID.CannotUseCustomFormat), nameof(encodingFormat));
            }
            if (samplesPerSecond <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(samplesPerSecond), SR.Get(SRID.MustBeGreaterThanZero));
            }
            if (bitsPerSample <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bitsPerSample), SR.Get(SRID.MustBeGreaterThanZero));
            }
            if (channelCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(channelCount), SR.Get(SRID.MustBeGreaterThanZero));
            }
            _encodingFormat = encodingFormat;
            _samplesPerSecond = samplesPerSecond;
            _bitsPerSample = bitsPerSample;
            _channelCount = channelCount;
            if (formatSpecificData == null)
            {
                _formatSpecificData = new byte[0];
            }
            else
            {
                _formatSpecificData = (byte[])formatSpecificData.Clone();
            }
            if ((uint)(encodingFormat - 6) <= 1u)
            {
                if (bitsPerSample != 8)
                {
                    throw new ArgumentOutOfRangeException(nameof(bitsPerSample));
                }
                if (formatSpecificData != null && formatSpecificData.Length != 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(formatSpecificData));
                }
            }
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Speech.AudioFormat.SpeechAudioFormatInfo" /> class and specifies the encoding format, samples per second, bits per sample, number of channels, average bytes per second, block alignment value, and an array containing format-specific data.</summary>
        /// <param name="encodingFormat">The encoding format.</param>
        /// <param name="samplesPerSecond">The value for the samples per second.</param>
        /// <param name="bitsPerSample">The value for the bits per sample.</param>
        /// <param name="channelCount">The value for the channel count.</param>
        /// <param name="averageBytesPerSecond">The value for the average bytes per second.</param>
        /// <param name="blockAlign">The value for the BlockAlign.</param>
        /// <param name="formatSpecificData">A byte array containing the format-specific data.</param>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public SpeechAudioFormatInfo(EncodingFormat encodingFormat, int samplesPerSecond, int bitsPerSample, int channelCount, int averageBytesPerSecond, int blockAlign, byte[] formatSpecificData)
            : this(encodingFormat, samplesPerSecond, (short)bitsPerSample, (short)channelCount, formatSpecificData)
        {
            if (averageBytesPerSecond <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(averageBytesPerSecond), SR.Get(SRID.MustBeGreaterThanZero));
            }
            if (blockAlign <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(blockAlign), SR.Get(SRID.MustBeGreaterThanZero));
            }
            _averageBytesPerSecond = averageBytesPerSecond;
            _blockAlign = (short)blockAlign;
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Speech.AudioFormat.SpeechAudioFormatInfo" /> class and specifies the samples per second, bits per sample, and the number of channels.</summary>
        /// <param name="samplesPerSecond">The value for the samples per second.</param>
        /// <param name="bitsPerSample">The value for the bits per sample.</param>
        /// <param name="channel">A member of the <see cref="T:System.Speech.AudioFormat.AudioChannel" /> enumeration (indicating <c>Mono</c> or <c>Stereo</c>).</param>
        public SpeechAudioFormatInfo(int samplesPerSecond, AudioBitsPerSample bitsPerSample, AudioChannel channel)
            : this(EncodingFormat.Pcm, samplesPerSecond, (short)bitsPerSample, (short)channel, null)
        {
            _blockAlign = (short)(_channelCount * (_bitsPerSample / 8));
            _averageBytesPerSecond = _samplesPerSecond * _blockAlign;
        }

        /// <summary>Returns the format-specific data of the audio format.</summary>
        /// <returns>A byte array containing the format-specific data.</returns>
        public byte[] FormatSpecificData()
        {
            return (byte[])_formatSpecificData.Clone();
        }

        /// <summary>Returns whether a given object is an instance of <see cref="T:System.Speech.AudioFormat.SpeechAudioFormatInfo" /> and equal to the current instance of <see cref="T:System.Speech.AudioFormat.SpeechAudioFormatInfo" />.</summary>
        /// <param name="obj">The object to be compared.</param>
        /// <returns>Returns <see langword="true" /> if the current instance of <see cref="T:System.Speech.AudioFormat.SpeechAudioFormatInfo" /> and that obtained from the <paramref name="obj" /> argument are equal, otherwise returns <see langword="false" />.</returns>
        public override bool Equals(object obj)
        {
            SpeechAudioFormatInfo speechAudioFormatInfo = obj as SpeechAudioFormatInfo;
            if (speechAudioFormatInfo == null)
            {
                return false;
            }
            if (!_averageBytesPerSecond.Equals(speechAudioFormatInfo._averageBytesPerSecond) || !_bitsPerSample.Equals(speechAudioFormatInfo._bitsPerSample) || !_blockAlign.Equals(speechAudioFormatInfo._blockAlign) || !_encodingFormat.Equals(speechAudioFormatInfo._encodingFormat) || !_channelCount.Equals(speechAudioFormatInfo._channelCount) || !_samplesPerSecond.Equals(speechAudioFormatInfo._samplesPerSecond))
            {
                return false;
            }
            if (_formatSpecificData.Length != speechAudioFormatInfo._formatSpecificData.Length)
            {
                return false;
            }
            for (int i = 0; i < _formatSpecificData.Length; i++)
            {
                if (_formatSpecificData[i] != speechAudioFormatInfo._formatSpecificData[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>Returns the hash code of the audio format.</summary>
        /// <returns>The value for the hash code.</returns>
        public override int GetHashCode()
        {
            return _averageBytesPerSecond.GetHashCode();
        }
    }
}
