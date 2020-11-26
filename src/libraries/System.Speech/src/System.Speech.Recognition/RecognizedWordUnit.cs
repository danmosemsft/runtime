// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;

namespace System.Speech.Recognition
{
    /// <summary>Provides the atomic unit of recognized speech.</summary>
    [Serializable]
    [DebuggerDisplay("Text: {Text}")]
    public class RecognizedWordUnit
    {
        internal TimeSpan _audioPosition;

        internal TimeSpan _audioDuration;

        private string _text;

        private string _lexicalForm;

        private float _confidence;

        private string _pronunciation;

        private DisplayAttributes _displayAttributes;

        /// <summary>Gets the normalized text for a recognized word.</summary>
        /// <returns>A string that contains the normalized text output for a given input word.</returns>
        public string Text => _text;

        /// <summary>Gets a value, assigned by the recognizer, that represents the likelihood that a recognized word matches a given input.</summary>
        /// <returns>A relative measure of the certainty of correct recognition for a word. The value is from 0.0 to 1.0, for low to high confidence, respectively.</returns>
        public float Confidence => _confidence;

        /// <summary>Gets the phonetic spelling of a recognized word.</summary>
        /// <returns>A string of characters from a supported phonetic alphabet, such as the International Phonetic Alphabet (IPA) or the Universal Phone Set (UPS).</returns>
        public string Pronunciation => _pronunciation;

        /// <summary>Gets the unnormalized text of a recognized word.</summary>
        /// <returns>Returns a <see cref="T:System.String" /> containing the text of a recognized word, without any normalization.</returns>
        public string LexicalForm => _lexicalForm;

        /// <summary>Gets formatting information used to create the text output from the current <see cref="T:System.Speech.Recognition.RecognizedWordUnit" /> instance.</summary>
        /// <returns>Specifies the use of white space to display of the contents of a <see cref="T:System.Speech.Recognition.RecognizedWordUnit" /> object.</returns>
        public DisplayAttributes DisplayAttributes => _displayAttributes;

        /// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.RecognizedWordUnit" /> class.</summary>
        /// <param name="text">The normalized text for a recognized word.
        ///  This value can be <see langword="null" />, "", or <see cref="F:System.String.Empty" />.</param>
        /// <param name="confidence">A <see langword="float" /> value from 0.0 through 1.0 indicating the certainty of word recognition.</param>
        /// <param name="pronunciation">The phonetic spelling of a recognized word.
        ///  This value can be <see langword="null" />, "", or <see cref="F:System.String.Empty" />.</param>
        /// <param name="lexicalForm">The unnormalized text for a recognized word.
        ///  This argument is required and may not be <see langword="null" />, "", or <see cref="F:System.String.Empty" />.</param>
        /// <param name="displayAttributes">Defines the use of white space to display recognized words.</param>
        /// <param name="audioPosition">The location of the recognized word in the audio input stream.
        ///  This value can be <see cref="F:System.TimeSpan.Zero" />.</param>
        /// <param name="audioDuration">The length of the audio input corresponding to the recognized word.
        ///  This value can be <see cref="F:System.TimeSpan.Zero" />.</param>
        public RecognizedWordUnit(string text, float confidence, string pronunciation, string lexicalForm, DisplayAttributes displayAttributes, TimeSpan audioPosition, TimeSpan audioDuration)
        {
            if (lexicalForm == null)
            {
                throw new ArgumentNullException("lexicalForm");
            }
            if (confidence < 0f || confidence > 1f)
            {
                throw new ArgumentOutOfRangeException(SR.Get(SRID.InvalidConfidence));
            }
            _text = ((text == null || text.Length == 0) ? null : text);
            _confidence = confidence;
            _pronunciation = ((pronunciation == null || pronunciation.Length == 0) ? null : pronunciation);
            _lexicalForm = lexicalForm;
            _displayAttributes = displayAttributes;
            _audioPosition = audioPosition;
            _audioDuration = audioDuration;
        }

        internal static byte DisplayAttributesToSapiAttributes(DisplayAttributes displayAttributes)
        {
            return (byte)((uint)displayAttributes >> 1);
        }

        internal static DisplayAttributes SapiAttributesToDisplayAttributes(byte sapiAttributes)
        {
            return (DisplayAttributes)(sapiAttributes << 1);
        }
    }
}
