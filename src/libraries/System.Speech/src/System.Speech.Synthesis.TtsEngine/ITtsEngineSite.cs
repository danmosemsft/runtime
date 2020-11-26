// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;

namespace System.Speech.Synthesis.TtsEngine
{
    /// <summary>Provides methods for writing audio data and events.</summary>
    public interface ITtsEngineSite
    {
        /// <summary>Determines the events the engine should raise.</summary>
        /// <returns>An <see langword="int" /> containing the sum of one or more members of the <see langword="TtsEventId" /> enumeration.</returns>
        int EventInterest
        {
            get;
        }

        /// <summary>Determines the action or actions the engine should perform.</summary>
        /// <returns>An <see langword="int" /> containing the sum of one or more members of the <see langword="TtsEngineAction" /> enumeration.</returns>
        int Actions
        {
            get;
        }

        /// <summary>Gets the speaking rate of the engine.</summary>
        /// <returns>An <see langword="int" /> containing the speaking rate.</returns>
        int Rate
        {
            get;
        }

        /// <summary>Gets the speaking volume of the engine.</summary>
        /// <returns>An <see langword="int" /> containing the speaking volume.</returns>
        int Volume
        {
            get;
        }

        /// <summary>Adds one or more events to the <see langword="EventInterest" /> property.</summary>
        /// <param name="events">An array of <see langword="SpeechEventInfo" /> objects.</param>
        /// <param name="count">The size of the array.</param>
        void AddEvents(SpeechEventInfo[] events, int count);

        /// <summary>Outputs audio data.</summary>
        /// <param name="data">The location of the output audio data.</param>
        /// <param name="count">The number of items in the output audio stream.</param>
        int Write(IntPtr data, int count);

        /// <summary>Returns the number and type of items to be skipped.</summary>
        SkipInfo GetSkipInfo();

        /// <summary>Returns the number of items skipped.</summary>
        /// <param name="skipped">The number of items skipped.</param>
        void CompleteSkip(int skipped);

        /// <summary>Loads the resource at the specified URI.</summary>
        /// <param name="uri">The URI of the resource.</param>
        /// <param name="mediaType">The media type of the resource.</param>
        Stream LoadResource(Uri uri, string mediaType);
    }
}
