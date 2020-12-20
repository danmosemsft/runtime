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

#if SPEECHSERVER || PROMPT_ENGINE
       abstract internal void BackupVoice (IPromptEngine pe);
#endif

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

#if SPEECHSERVER || PROMPT_ENGINE

        override internal void BackupVoice (IPromptEngine pe)
        {
            pe.SetBackupVoice ((ITtsEngineSsml) new ProxyComSsml (_ssmlEngine, _site));
        }

        /// <summary>
        /// Maps the ITtsEngineSsml interface to the sapi ITtsEngine interface
        /// </summary>
        class ProxyComSsml : ITtsEngineSsml
        {
            internal ProxyComSsml (TtsEngineSsml sapiEngine, ITtsEngineSite site)
            {
                _ssmlEngine = sapiEngine;
                _site = site;
            }

        #region ITtsEngineSsml implementation

            void ITtsEngineSsml.GetOutputFormat (SpeakOutputFormat speakOutputFormat, IntPtr targetWaveFormat, out IntPtr waveHeader)
            {
                waveHeader = _ssmlEngine.GetOutputFormat (speakOutputFormat, targetWaveFormat);
            }

            /// <summary>
            /// Add a lexicon to the engine collection of lexicons
            /// </summary>
            /// <param name="location">A path or a Uri</param>
            /// <param name="mediaType">media type</param>
            /// <param name="site">Engine site (ITtsEngineSite)</param>
            void ITtsEngineSsml.AddLexicon (string location, string mediaType, IntPtr site)
            {
                _ssmlEngine.AddLexicon (new Uri (location, UriKind.RelativeOrAbsolute), mediaType, _site);
            }

            /// <summary>
            /// Removes a lexicon to the engine collection of lexicons
            /// </summary>
            /// <param name="location">A path or a Uri</param>
            /// <param name="site">Engine site (ITtsEngineSite)</param>
            void ITtsEngineSsml.RemoveLexicon (string location, IntPtr site)
            {
                _ssmlEngine.RemoveLexicon (new Uri (location, UriKind.RelativeOrAbsolute), _site);
            }

            /// <summary>
            /// Renders the specified text fragments array in the 
            /// specified output format.
            /// </summary>
            /// <param name="fragments">Text fragment with SSML 
            /// attributes information</param>
            /// <param name="count">Number of elements in fragment</param>
            /// <param name="waveFormat">Wave format header</param>
            /// <param name="site">Engine site (ITtsEngineSite)</param>
            void ITtsEngineSsml.Speak (IntPtr fragments, int count, IntPtr waveFormat, IntPtr site)
            {
                TextFragment [] textFragments = Converter.PtrToFragments (fragments, count);
               _ssmlEngine.Speak (textFragments, waveFormat, _site);
            }

            private TtsEngineSsml _ssmlEngine;
            private ITtsEngineSite _site;

        #endregion
        }
#endif

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

#if SPEECHSERVER || PROMPT_ENGINE

        override internal void BackupVoice (IPromptEngine pe)
        {
            pe.SetBackupVoice (_comEngine);
        }

#endif

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

#if SPEECHSERVER || PROMPT_ENGINE

        override internal void BackupVoice (IPromptEngine pe)
        {
            ITtsEngineSsml proxy = (ITtsEngineSsml) new ProxyComSapi (_sapiEngine);
            pe.SetBackupVoice (proxy);
        }

#endif

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

#if SPEECHSERVER || PROMPT_ENGINE

        /// <summary>
        /// Maps the ITtsEngineSsml interface to the sapi ITtsEngine interface
        /// </summary>
        class ProxyComSapi : ITtsEngineSsml
        {
            internal ProxyComSapi (ITtsEngine sapiEngine)
            {
                _sapiEngine = sapiEngine;
            }

        #region ITtsEngineSsml implementation

            void ITtsEngineSsml.GetOutputFormat (SpeakOutputFormat speakOutputFormat, IntPtr targetWaveFormat, out IntPtr waveHeader)
            {
                // Initialize TTS Engine
                Guid formatId = SAPIGuids.SPDFID_WaveFormatEx;
                Guid guidNull = new Guid ();
                IntPtr coMem = IntPtr.Zero;
                _sapiEngine.GetOutputFormat (ref formatId, targetWaveFormat, out guidNull, out coMem);
                waveHeader = coMem;
            }

            /// <summary>
            /// Add a lexicon to the engine collection of lexicons
            /// </summary>
            /// <param name="location">A path or a Uri</param>
            /// <param name="mediaType">media type</param>
            /// <param name="site">Engine site (ITtsEngineSite)</param>
            void ITtsEngineSsml.AddLexicon (string location, string mediaType, IntPtr site)
            {
            }

            /// <summary>
            /// Removes a lexicon to the engine collection of lexicons
            /// </summary>
            /// <param name="location">A path or a Uri</param>
            /// <param name="site">Engine site (ITtsEngineSite)</param>
            void ITtsEngineSsml.RemoveLexicon (string location, IntPtr site)
            {
            }

            /// <summary>
            /// Renders the specified text fragments array in the 
            /// specified output format.
            /// </summary>
            /// <param name="fragments">Text fragment with SSML 
            /// attributes information</param>
            /// <param name="count">Number of elements in fragment</param>
            /// <param name="waveFormat">Wave format header</param>
            /// <param name="site">Engine site (ITtsEngineSite)</param>
            void ITtsEngineSsml.Speak (IntPtr fragments, int count, IntPtr waveFormat, IntPtr site)
            {
                GCHandle spvTextFragment = new GCHandle ();
                TextFragment [] textFragments = Converter.PtrToFragments (fragments, count);
                ConvertTextFrag.ToSapi (new List<TextFragment> (textFragments), ref spvTextFragment);

                Guid formatId = SAPIGuids.SPDFID_WaveFormatEx;
                try
                {
                    _sapiEngine.Speak (0, ref formatId, waveFormat, spvTextFragment.AddrOfPinnedObject (), site);
                }
                finally
                {
                    ConvertTextFrag.FreeTextSegment (ref spvTextFragment);
                }
            }

        #endregion

            private ITtsEngine _sapiEngine;
        }


#endif
    }

#if SPEECHSERVER || PROMPT_ENGINE

    /// <summary>
    /// 
    /// </summary>
    internal class TextEngine : ITtsEngineProxy
    {
        //*******************************************************************
        //
        // Constructors
        //
        //*******************************************************************

        #region Constructors

        internal TextEngine (ITtsEngineSite iSite, int lcid)
            : base (lcid)
        {
            _iSite = iSite;
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
            return IntPtr.Zero;
        }

        override internal void AddLexicon (Uri lexicon, string mediaType)
        {
            // Ignore
        }

        override internal void RemoveLexicon (Uri lexicon)
        {
            // Ignore
        }

        override internal void Speak (List<TextFragment> frags, byte [] wfx)
        {
            TextEngineSsml.WriteTextFragments (_iSite, frags);

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
        }

        override internal void BackupVoice (IPromptEngine pe)
        {
            pe.SetBackupVoice (new TextEngineSsml (_iSite));
        }

        #endregion

        //*******************************************************************
        //
        // Private Fields
        //
        //*******************************************************************

        #region private Fields

        private ITtsEngineSite _iSite;

        #endregion

        //*******************************************************************
        //
        // Private Types
        //
        //*******************************************************************

        #region private Types

        class TextEngineSsml : ITtsEngineSsml
        {
            internal TextEngineSsml (ITtsEngineSite iSite)
            {
                _iSite = iSite;
            }

            /// <summary>
            /// Queries the engine about a specific output format. 
            /// The engine should examine the requested output format, 
            /// and return the closest format that it supports.
            /// </summary>
            public void GetOutputFormat (SpeakOutputFormat speakOutputFormat, IntPtr targetWaveFormat, out IntPtr waveHeader)
            {
                WAVEFORMATEX waveFormatEx = WAVEFORMATEX.Default;
                byte [] abHeader = waveFormatEx.ToBytes ();
                waveHeader = Marshal.AllocCoTaskMem (abHeader.Length);
                Marshal.Copy (abHeader, 0, waveHeader, abHeader.Length);
            }

            /// <summary>
            /// Add a lexicon to the engine collection of lexicons
            /// </summary>
            public void AddLexicon (string location, string mediaType, IntPtr site)
            {
            }

            /// <summary>
            /// Removes a lexicon to the engine collection of lexicons
            /// </summary>
            public void RemoveLexicon (string location, IntPtr site)
            {
            }

            /// <summary>
            /// Renders the specified text fragments array in the 
            /// specified output format.
            /// </summary>
            public void Speak (IntPtr fragments, int count, IntPtr waveHeader, IntPtr site)
            {
                WriteTextFragments (_iSite, new List<TextFragment> (Converter.PtrToFragments (fragments, count)));
            }

            public static void WriteTextFragments (ITtsEngineSite iSite, List<TextFragment> frags)
            {
                foreach (TextFragment frag in frags)
                {
                    if (frag.State.Action == TtsEngineAction.Speak)
                    {
                        string s = "0:TTS:" + frag.TextToSpeak + "\0";
                        GCHandle gc = GCHandle.Alloc (s, GCHandleType.Pinned);
                        try
                        {
                            iSite.Write (gc.AddrOfPinnedObject (), (frag.TextLength + 7) * 2);
                        }
                        finally
                        {
                            gc.Free ();
                        }
                    }
                }
            }

            private ITtsEngineSite _iSite;
        }

        #endregion
    }
#endif
    
#if SPEECHSERVER  || PROMPT_ENGINE

    class Converter
    {
        internal static TextFragment [] PtrToFragments (IntPtr fragments, int count)
        {
            TextFragment [] textFragments = new TextFragment [count];
            TextFragmentInterop fragInterop = new TextFragmentInterop ();
            int sizeOfFrag = Marshal.SizeOf (fragInterop);
            for (int i = 0; i < count; i++)
            {
                Marshal.PtrToStructure ((IntPtr) ((ulong) fragments + (ulong) (i * sizeOfFrag)), fragInterop);
                textFragments [i] = new TextFragment ();
                if (fragInterop._textToSpeak != null && fragInterop._textToSpeak.Length > 0)
                    textFragments [i].TextToSpeak = fragInterop._textToSpeak;
                textFragments [i].TextOffset = fragInterop._textOffset;
                textFragments [i].TextLength = fragInterop._textLength;
                textFragments [i].State = fragInterop._state.PtrToFragmentState ();
            }

            return textFragments;
        }
        /// <summary>
        /// TODOC
        /// </summary>
    
        [StructLayout (LayoutKind.Sequential)]
        internal class TextFragmentInterop
        {
            internal FragmentStateInterop _state;
            [MarshalAs (UnmanagedType.LPWStr)]
            internal string _textToSpeak;
            internal int _textOffset;
            internal int _textLength;
        }

        /// <summary>
        /// TODOC
        /// </summary>
        [StructLayout (LayoutKind.Sequential)]
        internal class FragmentStateInterop
        {
            internal TtsEngineAction _action;
            internal int _langId;
            internal int _emphasis;
            internal int _duration;
            internal IntPtr _sayAs;
            internal IntPtr _prosody;
            internal IntPtr _phoneme;

            FragmentStateInterop () { _action = TtsEngineAction.BeginPromptEngineOutput; _langId = _emphasis = _duration = 0; _sayAs = _phoneme = _prosody = IntPtr.Zero; }

            internal FragmentState PtrToFragmentState ()
            {
                FragmentState fragmentState = new FragmentState ();
                fragmentState.Action = _action;
                fragmentState.LangId = _langId;
                fragmentState.Emphasis = _emphasis;
                fragmentState.Duration = _duration;


                if (_sayAs != IntPtr.Zero)
                {
                    fragmentState.SayAs = new SayAs ();
                    Marshal.PtrToStructure (_sayAs, fragmentState.SayAs);
                }
                if (_phoneme != IntPtr.Zero)
                {
                    StringBuilder phones = new StringBuilder ();
                    int i = 0;
                    char phone = ' ';
                    do
                    {
                        Marshal.WriteInt16 (new IntPtr ((long) _phoneme + i * 2), phone);
                        phones.Append  (unchecked ((char) phone));
                    }
                    while (phones [i++] != 0);
                    fragmentState.Phoneme = phones.ToString().ToCharArray ();
                }

                if (_prosody != IntPtr.Zero)
                {
                    ProsodyInterop prosodyInterop = new ProsodyInterop ();
                    Marshal.PtrToStructure (_prosody, prosodyInterop);
                    fragmentState.Prosody = prosodyInterop.PtrToProsody ();
                }
                return fragmentState;
            }
        }

        /// <summary>
        /// TODOC
        /// </summary>
        [StructLayout (LayoutKind.Sequential)]
        internal class ProsodyInterop
        {
            internal ProsodyNumber _pitch;
            internal ProsodyNumber _range;
            internal ProsodyNumber _rate; // can be casted to a Prosody Rate
            internal int _duration;
            internal ProsodyNumber _volume;
            internal IntPtr _contourPoints;

            internal Prosody PtrToProsody ()
            {
                Prosody prosody = new Prosody ();
                prosody.Pitch = _pitch;
                prosody.Range = _range;
                prosody.Rate = _rate;
                prosody.Duration = _duration;
                prosody.Volume = _volume;

                // TODO: this if block doesn't work (bug 65562)
                // note when it's fixed: the ContourPoint struct isn't exact map of native version anymore (IsHz changed)
                //
                if (prosody.GetContourPoints () != null)
                {
                    List<ContourPoint> points = new List<ContourPoint> ();
                    int i = 0;
                    ContourPoint point = new ContourPoint ();
                    int sizeOfPoint = Marshal.SizeOf (points [0]);
                    do
                    {
                        Marshal.PtrToStructure (new IntPtr ((long) _contourPoints + i * sizeOfPoint), point);
                        points.Add (point);
                    }
                    while (point.Start != 100f);
                    prosody.SetContourPoints (points.ToArray ());
                }

                return prosody;
            }
        }
    }
#endif
}
