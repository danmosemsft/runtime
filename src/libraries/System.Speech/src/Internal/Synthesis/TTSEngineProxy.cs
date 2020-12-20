//------------------------------------------------------------------
// <copyright file="TTSEngineProxy.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
//  Contains either a reference to an audio audioStream or a list of 
//  text fragments.
//
// History:
//		2/1/2005	jeanfp		Created from the Sapi Managed code
//------------------------------------------------------------------

using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Speech.Internal.ObjectTokens;
using System.Speech.Synthesis.TtsEngine;
using System.Text;
using System.Threading;

namespace System.Speech.Internal.Synthesis
{
    abstract class ITtsEngineProxy
    {
        internal ITtsEngineProxy (int lcid)
        {
            _alphabetConverter = new AlphabetConverter (lcid);
        }

        abstract internal IntPtr GetOutputFormat (IntPtr targetFormat);
        abstract internal void AddLexicon (Uri lexicon, string mediaType);
        abstract internal void RemoveLexicon (Uri lexicon);
        abstract internal void Speak (List<TextFragment> frags, byte [] wfx);
        abstract internal void ReleaseInterface ();
        abstract internal char [] ConvertPhonemes (char [] phones, AlphabetType alphabet);
        abstract internal AlphabetType EngineAlphabet { get; }
        internal AlphabetConverter AlphabetConverter { get { return _alphabetConverter; } }


        protected AlphabetConverter _alphabetConverter;
    }

    internal class TtsProxySsml : ITtsEngineProxy
    {
        //*******************************************************************
        //
        // Constructors
        //
        //*******************************************************************

        #region Constructors

        internal TtsProxySsml (TtsEngineSsml ssmlEngine, ITtsEngineSite site, int lcid)
            : base (lcid)
        {
            _ssmlEngine = ssmlEngine;
            _site = site;
        }

        #endregion

        //*******************************************************************
        //
        // Internal Methods
        //
        //*******************************************************************

        #region Internal Methods

        override internal IntPtr GetOutputFormat (IntPtr targetFormat)
        {
            return _ssmlEngine.GetOutputFormat (SpeakOutputFormat.WaveFormat, targetFormat);
        }

        override internal void AddLexicon (Uri lexicon, string mediaType)
        {
            _ssmlEngine.AddLexicon (lexicon, mediaType, _site);
        }

        override internal void RemoveLexicon (Uri lexicon)
        {
            _ssmlEngine.RemoveLexicon (lexicon, _site);
        }

        override internal void Speak (List<TextFragment> frags, byte [] wfx)
        {
            GCHandle gc = GCHandle.Alloc (wfx, GCHandleType.Pinned);
            try
            {
                IntPtr waveFormat = gc.AddrOfPinnedObject ();
                _ssmlEngine.Speak (frags.ToArray (), waveFormat, _site);
            }
            finally
            {
                gc.Free ();
            }
        }

        override internal char [] ConvertPhonemes (char [] phones, AlphabetType alphabet)
        {
            if (alphabet == AlphabetType.Ipa)
            {
                return phones;
            }
            else
            {
                return _alphabetConverter.SapiToIpa (phones);
            }
        }

        override internal AlphabetType EngineAlphabet
        {
            get
            {
                return AlphabetType.Ipa;
            }
        }

        /// <summary>
        /// Release the COM interface for COM object
        /// </summary>
        override internal void ReleaseInterface ()
        {
        }


        #endregion

        //*******************************************************************
        //
        // Private Fields
        //
        //*******************************************************************

        #region private Fields

        private TtsEngineSsml _ssmlEngine;
        private ITtsEngineSite _site;

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    internal class TtsProxyCom : ITtsEngineProxy
    {
        //*******************************************************************
        //
        // Constructors
        //
        //*******************************************************************

        #region Constructors

        internal TtsProxyCom (ITtsEngineSsml comEngine, IntPtr iSite, int lcid)
            : base (lcid)
        {
            _iSite = iSite;
            _comEngine = comEngine;
        }

        #endregion

        //*******************************************************************
        //
        // Internal Methods
        //
        //*******************************************************************

        #region Internal Methods

        override internal IntPtr GetOutputFormat (IntPtr targetFormat)
        {
            IntPtr waveFormatEx;
            _comEngine.GetOutputFormat (targetFormat != IntPtr.Zero ? SpeakOutputFormat.WaveFormat : SpeakOutputFormat.Text, targetFormat, out waveFormatEx);
            return waveFormatEx;
        }

        override internal void AddLexicon (Uri lexicon, string mediaType)
        {
            _comEngine.AddLexicon (lexicon.ToString (), mediaType, _iSite);
        }

        override internal void RemoveLexicon (Uri lexicon)
        {
            _comEngine.RemoveLexicon (lexicon.ToString (), _iSite);
        }
        override internal void Speak (List<TextFragment> frags, byte [] wfx)
        {
            GCHandle gc = GCHandle.Alloc (wfx, GCHandleType.Pinned);
            try
            {
                IntPtr waveFormat = gc.AddrOfPinnedObject ();
                // Marshal all the Text fragments data into unmanaged memory
                Collection<IntPtr> memoryBlocksAllocated = new Collection<IntPtr> ();

                // Keep the list of all the memory blocks allocated by Marshal.CoTaskMemAlloc
                IntPtr fragmentInterop = TextFragmentInterop.FragmentToPtr (frags, memoryBlocksAllocated);
                try
                {
                    _comEngine.Speak (fragmentInterop, frags.Count, waveFormat, _iSite);
                }
                finally
                {
                    // Release all the allocated memory
                    foreach (IntPtr ptr in memoryBlocksAllocated)
                    {
                        Marshal.FreeCoTaskMem (ptr);
                    }
                }
            }
            finally
            {
                gc.Free ();
            }
        }

        /// <summary>
        /// Release the COM interface for COM object
        /// </summary>
        override internal void ReleaseInterface ()
        {
            Marshal.ReleaseComObject (_comEngine);
        }

        override internal AlphabetType EngineAlphabet
        {
            get
            {
                return AlphabetType.Ipa;
            }
        }

        override internal char [] ConvertPhonemes (char [] phones, AlphabetType alphabet)
        {
            if (alphabet == AlphabetType.Ipa)
            {
                return phones;
            }
            else
            {
                return _alphabetConverter.SapiToIpa (phones);
            }
        }


        #endregion

        //*******************************************************************
        //
        // Private Fields
        //
        //*******************************************************************

        #region private Fields

        private ITtsEngineSsml _comEngine;

        // This variable is stored here but never created or deleted
        private IntPtr _iSite;

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    internal class TtsProxySapi : ITtsEngineProxy
    {
        //*******************************************************************
        //
        // Constructors
        //
        //*******************************************************************

        #region Constructors

        internal TtsProxySapi (ITtsEngine sapiEngine, IntPtr iSite, int lcid)
            : base (lcid)
        {
            _iSite = iSite;
            _sapiEngine = sapiEngine;
        }

        #endregion

        //*******************************************************************
        //
        // Internal Methods
        //
        //*******************************************************************

        #region Internal Methods

        override internal IntPtr GetOutputFormat (IntPtr preferedFormat)
        {
            // Initialize TTS Engine
            Guid formatId = SAPIGuids.SPDFID_WaveFormatEx;
            Guid guidNull = new Guid ();
            IntPtr coMem = IntPtr.Zero;

            _sapiEngine.GetOutputFormat (ref formatId, preferedFormat, out guidNull, out coMem);
            return coMem;
        }

        override internal void AddLexicon (Uri lexicon, string mediaType)
        {
            // SAPI: Ignore
        }

        override internal void RemoveLexicon (Uri lexicon)
        {
            // SAPI: Ignore
        }

        override internal void Speak (List<TextFragment> frags, byte [] wfx)
        {
            GCHandle gc = GCHandle.Alloc (wfx, GCHandleType.Pinned);
            try
            {
                IntPtr waveFormat = gc.AddrOfPinnedObject ();
                GCHandle spvTextFragment = new GCHandle ();

                if (ConvertTextFrag.ToSapi (frags, ref spvTextFragment))
                {
                    Guid formatId = SAPIGuids.SPDFID_WaveFormatEx;
                    try
                    {
                        _sapiEngine.Speak (0, ref formatId, waveFormat, spvTextFragment.AddrOfPinnedObject (), _iSite);

                    }
                    finally
                    {
                        ConvertTextFrag.FreeTextSegment (ref spvTextFragment);
                    }
                }
            }
            finally
            {
                gc.Free ();
            }
        }

        override internal AlphabetType EngineAlphabet
        {
            get
            {
                return AlphabetType.Sapi;
            }
        }

        override internal char [] ConvertPhonemes (char [] phones, AlphabetType alphabet)
        {
            if (alphabet == AlphabetType.Ipa)
            {
                return _alphabetConverter.IpaToSapi (phones);
            }
            else
            {
                return phones;
            }
        }

        /// <summary>
        /// Release the COM interface for COM object
        /// </summary>
        override internal void ReleaseInterface ()
        {
            Marshal.ReleaseComObject (_sapiEngine);
        }


        #endregion

        //*******************************************************************
        //
        // Private Fields
        //
        //*******************************************************************

        #region private Fields

        private ITtsEngine _sapiEngine;

        // This variable is stored here but never created or deleted
        private IntPtr _iSite;

        #endregion

    }

    
}
