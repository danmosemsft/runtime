// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Speech.Synthesis.TtsEngine;

namespace System.Speech.Internal.Synthesis
{
    internal class TtsProxySapi : ITtsEngineProxy
    {
        private ITtsEngine _sapiEngine;

        private IntPtr _iSite;

        internal override AlphabetType EngineAlphabet => AlphabetType.Sapi;

        internal TtsProxySapi(ITtsEngine sapiEngine, IntPtr iSite, int lcid)
            : base(lcid)
        {
            _iSite = iSite;
            _sapiEngine = sapiEngine;
        }

        internal override IntPtr GetOutputFormat(IntPtr preferedFormat)
        {
            Guid pTargetFmtId = SAPIGuids.SPDFID_WaveFormatEx;
            Guid pOutputFormatId = default(Guid);
            IntPtr ppCoMemOutputWaveFormatEx = IntPtr.Zero;
            _sapiEngine.GetOutputFormat(ref pTargetFmtId, preferedFormat, out pOutputFormatId, out ppCoMemOutputWaveFormatEx);
            return ppCoMemOutputWaveFormatEx;
        }

        internal override void AddLexicon(Uri lexicon, string mediaType)
        {
        }

        internal override void RemoveLexicon(Uri lexicon)
        {
        }

        internal override void Speak(List<TextFragment> frags, byte[] wfx)
        {
            GCHandle gCHandle = GCHandle.Alloc(wfx, GCHandleType.Pinned);
            try
            {
                IntPtr pWaveFormatEx = gCHandle.AddrOfPinnedObject();
                GCHandle sapiFragLast = default(GCHandle);
                if (ConvertTextFrag.ToSapi(frags, ref sapiFragLast))
                {
                    Guid rguidFormatId = SAPIGuids.SPDFID_WaveFormatEx;
                    try
                    {
                        _sapiEngine.Speak(SPEAKFLAGS.SPF_DEFAULT, ref rguidFormatId, pWaveFormatEx, sapiFragLast.AddrOfPinnedObject(), _iSite);
                    }
                    finally
                    {
                        ConvertTextFrag.FreeTextSegment(ref sapiFragLast);
                    }
                }
            }
            finally
            {
                gCHandle.Free();
            }
        }

        internal override char[] ConvertPhonemes(char[] phones, AlphabetType alphabet)
        {
            if (alphabet == AlphabetType.Ipa)
            {
                return _alphabetConverter.IpaToSapi(phones);
            }
            return phones;
        }

        internal override void ReleaseInterface()
        {
            Marshal.ReleaseComObject(_sapiEngine);
        }
    }
}
