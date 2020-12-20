//------------------------------------------------------------------
// <copyright file="TTSEvent.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
//  Contains either a reference to an audio audioStream or a list of 
//  bookmark fragments.
//
// History:
//		2/1/2005	jeanfp		Created from the Sapi Managed code
//------------------------------------------------------------------

using System;
using System.Speech.Internal.ObjectTokens;
using System.Speech.Synthesis;
using System.Speech.Synthesis.TtsEngine;

namespace System.Speech.Internal.Synthesis
{
    /// <summary>
    /// 
    /// </summary>
    internal class TTSEvent 
    {
        //*******************************************************************
        //
        // Constructors
        //
        //*******************************************************************

        #region Constructors

        internal TTSEvent (TtsEventId id, Prompt prompt, Exception exception, VoiceInfo voice)
        {
            _evtId = id;
            _prompt = prompt;
            _exception = exception;
            _voice = voice;
        }

        internal TTSEvent (TtsEventId id, Prompt prompt, Exception exception, VoiceInfo voice, TimeSpan audioPosition, long streamPosition, string bookmark, uint wParam, IntPtr lParam)
            : this (id, prompt, exception, voice)
        {
            _audioPosition = audioPosition;
            _bookmark = bookmark;
            _wParam = wParam;
            _lParam = lParam;
#if SPEECHSERVER
            _streamPosition = streamPosition;
#endif
        }

#if !SPEECHSERVER
        private TTSEvent()
        {
        }

        static internal TTSEvent CreatePhonemeEvent(string phoneme, string nextPhoneme, 
                                                    TimeSpan duration, SynthesizerEmphasis emphasis,
                                                    Prompt prompt, TimeSpan audioPosition)
        {
            TTSEvent ttsEvent = new TTSEvent();
            ttsEvent._evtId = TtsEventId.Phoneme;
            ttsEvent._audioPosition = audioPosition;
            ttsEvent._prompt = prompt;
            ttsEvent._phoneme = phoneme;
            ttsEvent._nextPhoneme = nextPhoneme;
            ttsEvent._phonemeDuration = duration;
            ttsEvent._phonemeEmphasis = emphasis;

            return ttsEvent;
        }
#endif

        #endregion

        //*******************************************************************
        //
        // Internal Properties
        //
        //*******************************************************************

        #region Internal Properties

        internal TtsEventId Id
        {
            get
            {
                return _evtId;
            }
        }

        internal Exception Exception
        {
            get
            {
                return _exception;
            }
        }

        internal Prompt Prompt
        {
            get
            {
                return _prompt;
            }
        }

        internal VoiceInfo Voice
        {
            get
            {
                return _voice;
            }
        }

        internal TimeSpan AudioPosition
        {
            get
            {
                return _audioPosition;
            }
        }

        internal string Bookmark
        {
            get
            {
                return _bookmark;
            }
        }

        internal IntPtr LParam
        {
            get
            {
                return _lParam;
            }
        }

        internal uint WParam
        {
            get
            {
                return _wParam;
            }
        }

#if !SPEECHSERVER
        internal SynthesizerEmphasis PhonemeEmphasis
        {
            get
            {
                return _phonemeEmphasis;
            }
        }


        internal string Phoneme
        {
            get
            {
                return _phoneme;
            }
        }

        internal string NextPhoneme
        {
            get
            {
                return _nextPhoneme;
            }
            set
            {
                _nextPhoneme = value;
            }
        }

        internal TimeSpan PhonemeDuration
        {
            get
            {
                return _phonemeDuration;
            }
        }

#else
        internal long StreamPosition
        {
            get
            {
                return _streamPosition;
            }
        }
#endif

        #endregion

        //*******************************************************************
        //
        // Private Fields
        //
        //*******************************************************************

        #region private Fields

        private TtsEventId _evtId;
        private Exception _exception;
        private VoiceInfo _voice;
        private TimeSpan _audioPosition;
        private string _bookmark;
        private uint _wParam;
        private IntPtr _lParam;
        private Prompt _prompt;


#if !SPEECHSERVER
        //
        // Data for phoneme event
        //
        private string _phoneme;
        private string _nextPhoneme;
        private TimeSpan _phonemeDuration;
        private SynthesizerEmphasis _phonemeEmphasis;
#else
        private long _streamPosition;
#endif
        #endregion

    }
}
