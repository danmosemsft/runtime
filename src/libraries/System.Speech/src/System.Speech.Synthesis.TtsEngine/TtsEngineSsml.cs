// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Synthesis.TtsEngine
{
    /// <summary>Abstract base class to be implemented by all text to speech synthesis engines.</summary>
    public abstract class TtsEngineSsml
    {
        /// <summary>Constructs a new instance of <see cref="T:System.Speech.Synthesis.TtsEngine.TtsEngineSsml" /> based on an appropriate Voice Token registry key.</summary>
        /// <param name="registryKey">Full name of the registry key for the Voice Token associated with the <see cref="T:System.Speech.Synthesis.TtsEngine.TtsEngineSsml" /> implementation. engine.</param>
        protected TtsEngineSsml(string registryKey)
        {
        }

        /// <summary>Returns the best matching audio output supported by a given synthesize engine response to a request to the synthesizer engine for the support of a particular output format.</summary>
        /// <param name="speakOutputFormat">Valid member of the <see cref="T:System.Speech.Synthesis.TtsEngine.SpeakOutputFormat" /> enumeration indicating the type of requested audio output format.</param>
        /// <param name="targetWaveFormat">A pointer to a <see langword="struct" /> containing detail setting for the audio format type requested by the <paramref name="speakOutputFormat" /> argument.</param>
        /// <returns>Returns a valid <see langword="IntPtr" /> instance referring to a <see langword="struct" /> containing detailed information about the output format.</returns>
        public abstract IntPtr GetOutputFormat(SpeakOutputFormat speakOutputFormat, IntPtr targetWaveFormat);

        /// <summary>Adds a lexicon to the <see langword="Synthesizer" /><see langword="Voice" /> implemented by the current <see cref="T:System.Speech.Synthesis.TtsEngine.TtsEngineSsml" /> instance.</summary>
        /// <param name="uri">A valid instance of <see langword="System.Uri" /> indicating the location of the lexicon information.</param>
        /// <param name="mediaType">A string containing the media type of the lexicon. Media types are case insensitive.</param>
        /// <param name="site">A reference to an <see cref="T:System.Speech.Synthesis.TtsEngine.ITtsEngineSite" /> interface used to interact with the platform infrastructure.</param>
        public abstract void AddLexicon(Uri uri, string mediaType, ITtsEngineSite site);

        /// <summary>Removes a lexicon currently loaded by the <see langword="Synthesizer" /><see langword="Voice" /> implemented by the current <see cref="T:System.Speech.Synthesis.TtsEngine.TtsEngineSsml" /> instance.</summary>
        /// <param name="uri">A valid instance of <see langword="System.Uri" /> indicating the location of the lexicon information.</param>
        /// <param name="site">A reference to an <see cref="T:System.Speech.Synthesis.TtsEngine.ITtsEngineSite" /> interface passed in by the platform infrastructure to allow access to the infrastructure resources.</param>
        public abstract void RemoveLexicon(Uri uri, ITtsEngineSite site);

        /// <summary>Renders specified <see cref="T:System.Speech.Synthesis.TtsEngine.TextFragment" /> array in the specified output format.</summary>
        /// <param name="fragment">An array of <see cref="T:System.Speech.Synthesis.TtsEngine.TextFragment" /> instances containing the text to be rendered into speech.</param>
        /// <param name="waveHeader">An <see langword="IntPtr" /> pointing to a structure containing audio output format.</param>
        /// <param name="site">A reference to an <see cref="T:System.Speech.Synthesis.TtsEngine.ITtsEngineSite" /> interface passed in by the platform infrastructure to allow access to the infrastructure resources.</param>
        public abstract void Speak(TextFragment[] fragment, IntPtr waveHeader, ITtsEngineSite site);
    }
}
