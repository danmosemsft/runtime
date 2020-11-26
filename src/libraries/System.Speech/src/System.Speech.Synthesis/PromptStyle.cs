// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Synthesis
{
    /// <summary>Defines a style for speaking prompts that consists of settings for emphasis, rate, and volume.</summary>
    [Serializable]
    public class PromptStyle
    {
        private PromptRate _rate;

        private PromptVolume _volume;

        private PromptEmphasis _emphasis;

        /// <summary>Gets or sets the setting for the speaking rate of the style.</summary>
        /// <returns>Returns the setting for the speaking rate of the style.</returns>
        public PromptRate Rate
        {
            get
            {
                return _rate;
            }
            set
            {
                _rate = value;
            }
        }

        /// <summary>Gets or sets the setting for the volume (loudness) of the style.</summary>
        /// <returns>Returns the setting for the volume (loudness) of the style.</returns>
        public PromptVolume Volume
        {
            get
            {
                return _volume;
            }
            set
            {
                _volume = value;
            }
        }

        /// <summary>Gets or sets the setting for the emphasis of the style.</summary>
        /// <returns>Returns the setting for the emphasis of the style.</returns>
        public PromptEmphasis Emphasis
        {
            get
            {
                return _emphasis;
            }
            set
            {
                _emphasis = value;
            }
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Speech.Synthesis.PromptStyle" /> class.</summary>
        public PromptStyle()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Speech.Synthesis.PromptStyle" /> class and specifies the setting for the speaking rate of the style.</summary>
        /// <param name="rate">The setting for the speaking rate of the style.</param>
        public PromptStyle(PromptRate rate)
        {
            Rate = rate;
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Speech.Synthesis.PromptStyle" /> class and specifies the setting for the speaking volume of the style.</summary>
        /// <param name="volume">The setting for the volume (loudness) of the style.</param>
        public PromptStyle(PromptVolume volume)
        {
            Volume = volume;
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Speech.Synthesis.PromptStyle" /> class and specifies the setting for the emphasis of the style.</summary>
        /// <param name="emphasis">The setting for the emphasis of the style.</param>
        public PromptStyle(PromptEmphasis emphasis)
        {
            Emphasis = emphasis;
        }
    }
}
