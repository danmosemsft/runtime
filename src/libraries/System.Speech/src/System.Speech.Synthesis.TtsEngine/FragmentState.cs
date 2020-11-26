// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Speech.Internal;

namespace System.Speech.Synthesis.TtsEngine
{
    /// <summary>Provides detailed information about a <see langword="TextFragment" />.</summary>
    [ImmutableObject(true)]
    public struct FragmentState : IEquatable<FragmentState>
    {
        private TtsEngineAction _action;

        private int _langId;

        private int _emphasis;

        private int _duration;

        private SayAs _sayAs;

        private Prosody _prosody;

        private char[] _phoneme;

        /// <summary>Returns the requested speech synthesizer action.</summary>
        /// <returns>A member of <see cref="System.Speech.Synthesis.TtsEngine.TtsEngineAction" /> indicating the speech synthesis action requested by SSML input.</returns>
        public TtsEngineAction Action
        {
            get
            {
                return _action;
            }
            internal set
            {
                _action = value;
            }
        }

        /// <summary>Returns the language supported by the current <see cref="System.Speech.Synthesis.TtsEngine.FragmentState" />.</summary>
        /// <returns>Returns an <see langword="int" /> containing an identifier for the language used by the current <see cref="System.Speech.Synthesis.TtsEngine.FragmentState" />.</returns>
        public int LangId
        {
            get
            {
                return _langId;
            }
            internal set
            {
                _langId = value;
            }
        }

        /// <summary>Returns instructions on how to emphasize a <see cref="System.Speech.Synthesis.TtsEngine.TextFragment" />.</summary>
        /// <returns>Returns an <see langword="int" /> value indicating how to emphasize a <see cref="System.Speech.Synthesis.TtsEngine.TextFragment" />.</returns>
        public int Emphasis
        {
            get
            {
                return _emphasis;
            }
            internal set
            {
                _emphasis = value;
            }
        }

        /// <summary>Returns the desired time for rendering a <see cref="System.Speech.Synthesis.TtsEngine.TextFragment" /></summary>
        /// <returns>Returns an <see langword="int" /> containing a value in millisecond of the desired time for rendering a <see cref="System.Speech.Synthesis.TtsEngine.TextFragment" />.</returns>
        public int Duration
        {
            get
            {
                return _duration;
            }
            internal set
            {
                _duration = value;
            }
        }

        /// <summary>Returns information about the context for the generation of speech from text.</summary>
        /// <returns>Returns a value <see cref="System.Speech.Synthesis.TtsEngine.SayAs" /> instance if the SSML used by a speech synthesis engine contains detailed information about the context to be used to generate speech, otherwise <see langword="null" />.</returns>
        public SayAs SayAs
        {
            get
            {
                return _sayAs;
            }
            internal set
            {
                Helpers.ThrowIfNull(value, nameof(value));
                _sayAs = value;
            }
        }

        /// <summary>Returns detailed information about the pitch, speaking rate, and volume of speech output.</summary>
        /// <returns>Returns a valid instance of <see cref="System.Speech.Synthesis.TtsEngine.Prosody" /> containing the pitch, speaking rate, and volume settings, and changes to those setting, for speech output.</returns>
        public Prosody Prosody
        {
            get
            {
                return _prosody;
            }
            internal set
            {
                Helpers.ThrowIfNull(value, nameof(value));
                _prosody = value;
            }
        }

        /// <summary>Returns phonetic information for a <see cref="System.Speech.Synthesis.TtsEngine.TextFragment" /></summary>
        public char[] Phoneme
        {
            get
            {
                return _phoneme;
            }
            internal set
            {
                Helpers.ThrowIfNull(value, nameof(value));
                _phoneme = value;
            }
        }

        /// <summary>Constructs a new instance of <see langword="FragmentState" />.</summary>
        /// <param name="action">A member of the <see cref="System.Speech.Synthesis.TtsEngine.TtsEngineAction" /> enumeration that specifies a speech synthesis action.</param>
        /// <param name="langId">The id of the language being used. Corresponds to the XML <c>xml:lang</c> attribute.</param>
        /// <param name="emphasis">The emphasis to be applied to speech output or pauses.</param>
        /// <param name="duration">The time allotted to speak the text of the <see langword="TextFragment" />.</param>
        /// <param name="sayAs">A member of the <see cref="System.Speech.Synthesis.TtsEngine.SayAs" /> class, indicating the type of text of the <see langword="TextFragment" /> and the level of detail required for accurate rendering of the contained text.
        ///  Corresponds to the <c>&lt;say-as&gt;</c> XML tag in the SSML specification
        ///  The argument may be <see langword="null." /></param>
        /// <param name="prosody">A <see cref="System.Speech.Synthesis.TtsEngine.Prosody" /> object indicating characteristics of the speech output such as pitch, speaking rate and volume.
        ///  Corresponds to the <c>&lt;prosody&gt;</c> XML tag in the SSML specification</param>
        /// <param name="phonemes">An array of <see langword="char" /> objects providing the phonetic pronunciation for text contained in the <see cref="System.Speech.Synthesis.TtsEngine.TextFragment" />, using the International Phonetic Alphabet (IPA) specification.
        ///  Corresponds to the <c>&lt;phoneme&gt;</c> XML tag in the SSML specification.
        ///  This argument may be <see langword="null" />.</param>
        public FragmentState(TtsEngineAction action, int langId, int emphasis, int duration, SayAs sayAs, Prosody prosody, char[] phonemes)
        {
            _action = action;
            _langId = langId;
            _emphasis = emphasis;
            _duration = duration;
            _sayAs = sayAs;
            _prosody = prosody;
            _phoneme = phonemes;
        }

        /// <summary>Determines if two instances of <see cref="System.Speech.Synthesis.TtsEngine.FragmentState" /> describes the same <see cref="System.Speech.Synthesis.TtsEngine.TextFragment" /> state.</summary>
        /// <param name="state1">An instance of <see cref="System.Speech.Synthesis.TtsEngine.TextFragment" /> whose described state is compared against the instance of <see cref="System.Speech.Synthesis.TtsEngine.TextFragment" /> provided by the <paramref name="state2" /> argument.</param>
        /// <param name="state2">An instance of <see cref="System.Speech.Synthesis.TtsEngine.TextFragment" /> whose described state is compared against the instance of <see cref="System.Speech.Synthesis.TtsEngine.TextFragment" /> provided by the <paramref name="state1" /> argument.</param>
        /// <returns>Returns <see langword="true" /> if both instances of <see cref="System.Speech.Synthesis.TtsEngine.FragmentState" />, <paramref name="state1" /> and <paramref name="state2" />, describe the same <see cref="System.Speech.Synthesis.TtsEngine.TextFragment" /> state, otherwise <see langword="false" /> is returned.</returns>
        public static bool operator ==(FragmentState state1, FragmentState state2)
        {
            if (state1.Action == state2.Action && state1.LangId == state2.LangId && state1.Emphasis == state2.Emphasis && state1.Duration == state2.Duration && state1.SayAs == state2.SayAs && state1.Prosody == state2.Prosody)
            {
                return object.Equals(state1.Phoneme, state2.Phoneme);
            }
            return false;
        }

        /// <summary>Determines if two instances of <see cref="System.Speech.Synthesis.TtsEngine.FragmentState" /> describes the different <see cref="System.Speech.Synthesis.TtsEngine.TextFragment" /> state.</summary>
        /// <param name="state1">An instance of <see cref="System.Speech.Synthesis.TtsEngine.TextFragment" /> whose described state is compared against the instance of <see cref="System.Speech.Synthesis.TtsEngine.TextFragment" /> provided by the <paramref name="state2" /> argument.</param>
        /// <param name="state2">An instance of <see cref="System.Speech.Synthesis.TtsEngine.TextFragment" /> whose described state is compared against the instance of <see cref="System.Speech.Synthesis.TtsEngine.TextFragment" /> provided by the <paramref name="state1" /> argument.</param>
        /// <returns>Returns <see langword="true" /> if both instances of <see cref="System.Speech.Synthesis.TtsEngine.FragmentState" />, <paramref name="state1" /> and <paramref name="state2" />, do not describe the same <see cref="System.Speech.Synthesis.TtsEngine.TextFragment" /> state, otherwise <see langword="false" /> is returned.</returns>
        public static bool operator !=(FragmentState state1, FragmentState state2)
        {
            return !(state1 == state2);
        }

        /// <summary>Determines if a given instance of <see cref="System.Speech.Synthesis.TtsEngine.FragmentState" /> is equal to the current instance of <see cref="System.Speech.Synthesis.TtsEngine.FragmentState" />.</summary>
        /// <param name="other">An instance of <see cref="System.Speech.Synthesis.TtsEngine.TextFragment" /> that</param>
        /// <returns>Returns <see langword="true" />, if both the current instance of <see cref="System.Speech.Synthesis.TtsEngine.TextFragment" /> and that supplied through the <paramref name="other" /> argument describe the same <see cref="System.Speech.Synthesis.TtsEngine.TextFragment" /> state. Returns <see langword="false" /> if the current <see cref="System.Speech.Synthesis.TtsEngine.TextFragment" /> and the <paramref name="other" /> argument do not support the same <see cref="System.Speech.Synthesis.TtsEngine.TextFragment" /> state.</returns>
        public bool Equals(FragmentState other)
        {
            return this == other;
        }

        /// <summary>Determines if a given object is an instance <see cref="System.Speech.Synthesis.TtsEngine.FragmentState" /> equal to the current instance of <see cref="System.Speech.Synthesis.TtsEngine.FragmentState" />.</summary>
        /// <param name="obj">An object that can be cast to an instance of <see cref="System.Speech.Synthesis.TtsEngine.TextFragment" /></param>
        /// <returns>Returns <see langword="true" />, if the current instance of <see cref="System.Speech.Synthesis.TtsEngine.TextFragment" /> and that obtained from the <see cref="object" /> provided by the <paramref name="obj" /> argument describe the same <see cref="System.Speech.Synthesis.TtsEngine.TextFragment" /> state. Returns <see langword="false" /> if the current <see cref="System.Speech.Synthesis.TtsEngine.TextFragment" /> and the <paramref name="obj" /> argument do not support the same <see cref="System.Speech.Synthesis.TtsEngine.TextFragment" /> state.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is FragmentState))
            {
                return false;
            }
            return Equals((FragmentState)obj);
        }

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
