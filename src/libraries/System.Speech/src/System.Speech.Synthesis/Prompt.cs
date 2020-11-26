// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.IO;
using System.Speech.Internal;

namespace System.Speech.Synthesis
{
    /// <summary>Represents information about what can be rendered, either text or an audio file, by the <see cref="T:System.Speech.Synthesis.SpeechSynthesizer" />.</summary>
    [DebuggerDisplay("{_text}")]
    public class Prompt
    {
        internal string _text;

        internal Uri _audio;

        internal SynthesisMediaType _media;

        internal bool _syncSpeak;

        internal Exception _exception;

        private bool _completed;

        private object _synthesizer;

        private static ResourceLoader _resourceLoader = new ResourceLoader();

        /// <summary>Gets whether the <see cref="T:System.Speech.Synthesis.Prompt" /> has finished playing.</summary>
        /// <returns>Returns <see langword="true" /> if the prompt has completed; otherwise <see langword="false" />.</returns>
        public bool IsCompleted
        {
            get
            {
                return _completed;
            }
            internal set
            {
                _completed = value;
            }
        }

        internal object Synthesizer
        {
            set
            {
                if (value != null && (_synthesizer != null || _completed))
                {
                    throw new ArgumentException(SR.Get(SRID.SynthesizerPromptInUse), nameof(value));
                }
                _synthesizer = value;
            }
        }

        /// <summary>Creates a new instance of the <see cref="T:System.Speech.Synthesis.Prompt" /> class and specifies the text to be spoken.</summary>
        /// <param name="textToSpeak">The text to be spoken.</param>
        public Prompt(string textToSpeak)
            : this(textToSpeak, SynthesisTextFormat.Text)
        {
        }

        /// <summary>Creates a new instance of the <see cref="T:System.Speech.Synthesis.Prompt" /> class from a <see cref="T:System.Speech.Synthesis.PromptBuilder" /> object.</summary>
        /// <param name="promptBuilder">The content to be spoken.</param>
        public Prompt(PromptBuilder promptBuilder)
        {
            Helpers.ThrowIfNull(promptBuilder, nameof(promptBuilder));
            _text = promptBuilder.ToXml();
            _media = SynthesisMediaType.Ssml;
        }

        /// <summary>Creates a new instance of the <see cref="T:System.Speech.Synthesis.Prompt" /> class and specifies the text to be spoken and whether its format is plain text or markup language.</summary>
        /// <param name="textToSpeak">The text to be spoken.</param>
        /// <param name="media">A value that specifies the format of the text.</param>
        public Prompt(string textToSpeak, SynthesisTextFormat media)
        {
            Helpers.ThrowIfNull(textToSpeak, nameof(textToSpeak));
            if ((uint)(_media = (SynthesisMediaType)media) <= 1u)
            {
                _text = textToSpeak;
                return;
            }
            throw new ArgumentException(SR.Get(SRID.SynthesizerUnknownMediaType), nameof(media));
        }

        internal Prompt(Uri promptFile, SynthesisMediaType media)
        {
            Helpers.ThrowIfNull(promptFile, nameof(promptFile));
            switch (_media = media)
            {
                case SynthesisMediaType.Text:
                case SynthesisMediaType.Ssml:
                    {
                        string mimeType;
                        Uri baseUri;
                        string localPath;
                        using (Stream stream = _resourceLoader.LoadFile(promptFile, out mimeType, out baseUri, out localPath))
                        {
                            try
                            {
                                using (TextReader textReader = new StreamReader(stream))
                                {
                                    _text = textReader.ReadToEnd();
                                }
                            }
                            finally
                            {
                                _resourceLoader.UnloadFile(localPath);
                            }
                        }
                        break;
                    }
                case SynthesisMediaType.WaveAudio:
                    _text = promptFile.ToString();
                    _audio = promptFile;
                    break;
                default:
                    throw new ArgumentException(SR.Get(SRID.SynthesizerUnknownMediaType), nameof(media));
            }
        }
    }
}
