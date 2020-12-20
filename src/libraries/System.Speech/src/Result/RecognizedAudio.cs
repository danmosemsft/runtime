//------------------------------------------------------------------
// <copyright file="RecognizedAudio.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------


using System;
using System.IO;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Speech.AudioFormat;
using System.Speech.Internal;

#pragma warning disable 1634, 1691 // Allows suppression of certain PreSharp messages.

namespace System.Speech.Recognition
{

    /// TODOC <_include file='doc\RecognitionResult.uex' path='docs/doc[@for="RecognizedAudio"]/*' />

    [Serializable]
    public class RecognizedAudio
    {
        internal RecognizedAudio(byte[] rawAudioData, SpeechAudioFormatInfo audioFormat, DateTime startTime, TimeSpan audioPosition, TimeSpan audioDuration)
        {
            _audioFormat = audioFormat;
            _startTime = startTime;
            _audioPosition = audioPosition;
            _audioDuration = audioDuration;
            _rawAudioData = rawAudioData;
        }

#if !SPEECHSERVER
        /// TODOC <_include file='doc\RecognitionResult.uex' path='docs/doc[@for="RecognizedAudio.Format"]/*' />
        public SpeechAudioFormatInfo Format
        {
            get { return _audioFormat; }
        }
#endif

        // Chronological "wall-clock" time the user started speaking the result at. This is useful for latency calculations etc.
        /// TODOC <_include file='doc\RecognitionResult.uex' path='docs/doc[@for="RecognizedAudio.StartTime"]/*' />
        public DateTime StartTime
        {
            get { return _startTime; }
        }

        // Position in the audio stream this audio starts at.
        // Note: the stream starts at zero when the engine first starts processing audio.
        /// TODOC <_include file='doc\RecognitionResult.uex' path='docs/doc[@for="RecognizedAudio.AudioPosition"]/*' />
        public TimeSpan AudioPosition
        {
            get { return _audioPosition; }
        }

        // Length of this audio fragment
        /// TODOC <_include file='doc\RecognitionResult.uex' path='docs/doc[@for="RecognizedAudio.Duration"]/*' />
        public TimeSpan Duration
        {
            get { return _audioDuration; }
        }

        // Different ways to store the audio, either as a binary data stream or as a wave file.
        /// TODOC <_include file='doc\RecognitionResult.uex' path='docs/doc[@for="RecognizedAudio.WriteAudio1"]/*' />
        public void WriteToWaveStream (Stream outputStream)
        {
            Helpers.ThrowIfNull (outputStream, "outputStream");
            
            using (StreamMarshaler sm = new StreamMarshaler (outputStream))
            {
                WriteWaveHeader (sm);
            }

            // now write the raw data
            outputStream.Write (_rawAudioData, 0, _rawAudioData.Length);

            outputStream.Flush ();
        }

        // Different ways to store the audio, either as a binary data stream or as a wave file.
        /// TODOC <_include file='doc\RecognitionResult.uex' path='docs/doc[@for="RecognizedAudio.WriteAudio1"]/*' />
        public void WriteToAudioStream (Stream outputStream)
        {
            Helpers.ThrowIfNull (outputStream, "outputStream");

            // now write the raw data
            outputStream.Write (_rawAudioData, 0, _rawAudioData.Length);

            outputStream.Flush ();
        }

        // Get another audio object from this one representing a range of audio.
        /// TODOC <_include file='doc\RecognitionResult.uex' path='docs/doc[@for="RecognizedAudio.GetRange"]/*' />
        public RecognizedAudio GetRange (TimeSpan audioPosition, TimeSpan duration)
        {
            if (audioPosition.Ticks < 0)
            {
                throw new ArgumentOutOfRangeException ("audioPosition", SR.Get (SRID.NegativeTimesNotSupported));
            }
            if (duration.Ticks < 0)
            {
                throw new ArgumentOutOfRangeException ("duration", SR.Get (SRID.NegativeTimesNotSupported));
            }
            if (audioPosition > _audioDuration)
            {
                throw new ArgumentOutOfRangeException ("audioPosition");
            }

            if (duration > audioPosition + _audioDuration)
            {
                throw new ArgumentOutOfRangeException ("duration");
            }

            // Get the position and length in bytes offset and btyes length.
            int startPosition = (int) ((_audioFormat.BitsPerSample * _audioFormat.SamplesPerSecond * audioPosition.Ticks) / (TimeSpan.TicksPerSecond * 8));
            int length = (int) ((_audioFormat.BitsPerSample * _audioFormat.SamplesPerSecond * duration.Ticks) / (TimeSpan.TicksPerSecond * 8));
            if (startPosition + length > _rawAudioData.Length)
            {
                length = _rawAudioData.Length - startPosition;
            }

            // Extract the data from the original stream
            byte [] audioBytes = new byte [length];
            Array.Copy (_rawAudioData, startPosition, audioBytes, 0, length);
            return new RecognizedAudio (audioBytes, _audioFormat, _startTime + audioPosition, audioPosition, duration);
        }

        //*******************************************************************
        //
        // Private Fields
        //
        //*******************************************************************

        #region Private Methods

        private void WriteWaveHeader (StreamMarshaler sm)
        {
            char [] riff = new char [4] { 'R', 'I', 'F', 'F' };
            byte [] formatSpecificData = _audioFormat.FormatSpecificData ();
            sm.WriteArray<char> (riff, riff.Length);

            sm.WriteStream ((uint) (_rawAudioData.Length + 38 + formatSpecificData.Length)); // Must be four bytes

            char [] wave = new char [4] { 'W', 'A', 'V', 'E' };
            sm.WriteArray (wave, wave.Length);

            char [] fmt = new char [4] { 'f', 'm', 't', ' ' };
            sm.WriteArray (fmt, fmt.Length);

            sm.WriteStream (18 + formatSpecificData.Length);

            sm.WriteStream ((UInt16) _audioFormat.EncodingFormat);
            sm.WriteStream ((UInt16) _audioFormat.ChannelCount);
            sm.WriteStream (_audioFormat.SamplesPerSecond);
            sm.WriteStream (_audioFormat.AverageBytesPerSecond);
            sm.WriteStream ((UInt16) _audioFormat.BlockAlign);
            sm.WriteStream ((UInt16) _audioFormat.BitsPerSample);
            sm.WriteStream ((UInt16) formatSpecificData.Length);

            // write codec specific data
            if (formatSpecificData.Length > 0)
            {
                sm.WriteStream (formatSpecificData);
            }

            char [] data = new char [4] { 'd', 'a', 't', 'a' };
            sm.WriteArray (data, data.Length);
            sm.WriteStream (_rawAudioData.Length);
        }

#if SPEECHSERVER

#pragma warning disable 56518 // The Binary reader cannot be disposed or it would close the underlying stream
        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope")]
        internal RecognizedAudio RemoveMetaData()
        {
            // Check format:
            if ((int)_audioFormat.EncodingFormat == 0x8000 - (int)EncodingFormat.Pcm ||
                (int)_audioFormat.EncodingFormat == 0x8000 - (int)EncodingFormat.ULaw ||
                (int)_audioFormat.EncodingFormat == 0x8000 - (int)EncodingFormat.ALaw)
            {
                // We have metadata so remove it:

                // Read metadata info from the format:
                byte[] formatSpecificData = _audioFormat.FormatSpecificData();
                if (formatSpecificData.Length < 4)
                {
                    throw new FormatException(SR.Get(SRID.ExtraDataNotPresent));
                }

                ushort metaDataSize;
                using (MemoryStream metaDataStream = new MemoryStream(formatSpecificData))
                {
                    BinaryReader br = new BinaryReader(metaDataStream);
                    br.ReadUInt16(); // Is metadata present - not used here.
                    metaDataSize = br.ReadUInt16();
                }
                
                int bytesPerSample = _audioFormat.BitsPerSample / 8;
                if (bytesPerSample < 1 || bytesPerSample > 2)
                {
                    throw new FormatException(SR.Get(SRID.BitsPerSampleInvalid));
                }

                // Calculate size of data part of block. As well as the metadata there are 6 bytes of overhead {signature, number of samples and reserved}.
                int sampleBlockSize = (int)_audioFormat.BlockAlign - metaDataSize - 6;
                if (sampleBlockSize < 0)
                {
                    throw new FormatException(SR.Get(SRID.DataBlockSizeInvalid));
                }

                // Check we have a whole number of blocks:
                if (_rawAudioData.Length % _audioFormat.BlockAlign != 0)
                {
                    throw new FormatException(SR.Get(SRID.NotWholeNumberBlocks));
                }

                // Actually copy the audio data to a new array:
                byte[] newAudioData = ProcessMetaDataBlocks(metaDataSize, bytesPerSample, sampleBlockSize);

                // Make new format:
                EncodingFormat newEncodingFormat = (EncodingFormat)(-((int)_audioFormat.EncodingFormat - 0x8000));
                int newAverageBytesPerSecond = (int)(((long)_audioFormat.AverageBytesPerSecond * sampleBlockSize) / _audioFormat.BlockAlign);
                short newBlockAlign = (short)(bytesPerSample * _audioFormat.ChannelCount);
                SpeechAudioFormatInfo newAudioFormat = new SpeechAudioFormatInfo(newEncodingFormat, _audioFormat.SamplesPerSecond, _audioFormat.BitsPerSample, _audioFormat.ChannelCount, newAverageBytesPerSecond, newBlockAlign, null);

                // Make new RecognizedAudio {copying start, duration etc}:
                return new RecognizedAudio(newAudioData, newAudioFormat, _startTime, _audioPosition, _audioDuration);
            }
            else
            {
                // No metadata - just return.
                return this;
            }
        }

        // Copy 
        private byte[] ProcessMetaDataBlocks(ushort metaDataSize, int bytesPerSample, int sampleBlockSize)
        {
            // Make new array: {this may be slightly too big if there are some empty packets - we resize later}
            int numberOfBlocks = _rawAudioData.Length / _audioFormat.BlockAlign;
            byte[] newAudioData = new byte[numberOfBlocks * sampleBlockSize];

            // Copy new bytes: {removing metadata and zero parts of data}
            int numberAudioBytes = 0;
            using (MemoryStream dataStream = new MemoryStream(_rawAudioData))
            {
                BinaryReader br = new BinaryReader(dataStream);
                for (int i = 0; i < numberOfBlocks; i++)
                {
                    ushort signature = br.ReadUInt16();
                    ushort sampleCount = br.ReadUInt16();
                    br.ReadBytes(metaDataSize); // Meta data itself
                    br.ReadUInt16(); // Reserved value - not used here.
                    int bytesToCopy = sampleCount * bytesPerSample;

                    if (signature != (ushort)_audioFormat.EncodingFormat)
                    {
                        throw new FormatException(SR.Get(SRID.BlockSignatureInvalid));
                    }
                    if (bytesToCopy > sampleBlockSize)
                    {
                        throw new FormatException(SR.Get(SRID.NumberOfSamplesInvalid));
                    }

                    // Copy the required number of samles across:
                    int curPosition = (int)dataStream.Position;
                    int endPosition = curPosition + bytesToCopy;
                    for (; curPosition < endPosition; curPosition++, numberAudioBytes++)
                    {
                        newAudioData[numberAudioBytes] = _rawAudioData[curPosition];
                    }

                    dataStream.Position += sampleBlockSize;
                }
                Debug.Assert(dataStream.Position == _rawAudioData.Length);
            }

            // Resize if necessary:
            Debug.Assert(numberAudioBytes <= newAudioData.Length);
            if (numberAudioBytes < newAudioData.Length)
            {
                // Array.Resize(ref newAudioData, numberAudioBytes);
                byte [] tempAudioData = new byte[numberAudioBytes];
                Array.Copy(tempAudioData, newAudioData, numberAudioBytes);
                newAudioData = tempAudioData;
            }

            return newAudioData;
        }

#pragma warning restore 56518 // The Binary reader cannot be disposed or it would close the underlying stream

#endif

		#endregion


		//*******************************************************************
        //
        // Private Fields
        //
        //*******************************************************************

        #region Private Fields

        DateTime _startTime;
        TimeSpan _audioPosition;
        TimeSpan _audioDuration;
        SpeechAudioFormatInfo _audioFormat;
        byte [] _rawAudioData;

        #endregion


    }

}

