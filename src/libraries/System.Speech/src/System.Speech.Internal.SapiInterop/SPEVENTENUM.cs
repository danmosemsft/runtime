// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Internal.SapiInterop
{
    internal enum SPEVENTENUM : ushort
    {
        SPEI_UNDEFINED = 0,
        SPEI_START_INPUT_STREAM = 1,
        SPEI_END_INPUT_STREAM = 2,
        SPEI_VOICE_CHANGE = 3,
        SPEI_TTS_BOOKMARK = 4,
        SPEI_WORD_BOUNDARY = 5,
        SPEI_PHONEME = 6,
        SPEI_SENTENCE_BOUNDARY = 7,
        SPEI_VISEME = 8,
        SPEI_TTS_AUDIO_LEVEL = 9,
        SPEI_TTS_PRIVATE = 0xF,
        SPEI_MIN_TTS = 1,
        SPEI_MAX_TTS = 0xF,
        SPEI_END_SR_STREAM = 34,
        SPEI_SOUND_START = 35,
        SPEI_SOUND_END = 36,
        SPEI_PHRASE_START = 37,
        SPEI_RECOGNITION = 38,
        SPEI_HYPOTHESIS = 39,
        SPEI_SR_BOOKMARK = 40,
        SPEI_PROPERTY_NUM_CHANGE = 41,
        SPEI_PROPERTY_STRING_CHANGE = 42,
        SPEI_FALSE_RECOGNITION = 43,
        SPEI_INTERFERENCE = 44,
        SPEI_REQUEST_UI = 45,
        SPEI_RECO_STATE_CHANGE = 46,
        SPEI_ADAPTATION = 47,
        SPEI_START_SR_STREAM = 48,
        SPEI_RECO_OTHER_CONTEXT = 49,
        SPEI_SR_AUDIO_LEVEL = 50,
        SPEI_SR_RETAINEDAUDIO = 51,
        SPEI_SR_PRIVATE = 52,
        SPEI_ACTIVE_CATEGORY_CHANGED = 53,
        SPEI_TEXTFEEDBACK = 54,
        SPEI_RECOGNITION_ALL = 55,
        SPEI_BARGE_IN = 56,
        SPEI_RESERVED1 = 30,
        SPEI_RESERVED2 = 33,
        SPEI_RESERVED3 = 0x3F
    }
}
