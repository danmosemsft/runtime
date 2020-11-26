// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Speech.Internal;

namespace System.Speech.Recognition
{
    /// <summary>Provides text and status information on recognition operations to be displayed in the Speech platform user interface.</summary>
    public class SpeechUI
    {
        internal SpeechUI()
        {
        }

        /// <summary>Sends status and descriptive text to the Speech platform user interface about the status of a recognition operation.</summary>
        /// <param name="result">A valid <see cref="T:System.Speech.Recognition.RecognitionResult" /> instance.</param>
        /// <param name="feedback">A <see cref="T:System.String" /> containing a comment about the recognition operation that produced the <see cref="T:System.Speech.Recognition.RecognitionResult" /><paramref name="result" />.</param>
        /// <param name="isSuccessfulAction">A <see langword="bool" /> indicating whether the application deemed the recognition operation a success.</param>
        /// <returns>
        ///   <see langword="true" /> if the information provided to the method (<paramref name="Feedback" />, and <paramref name="isSuccessfulAction" />) was successfully made available to the Speech platform user interface, and <see langword="false" /> if the operation failed.</returns>
        public static bool SendTextFeedback(RecognitionResult result, string feedback, bool isSuccessfulAction)
        {
            Helpers.ThrowIfNull(result, "result");
            Helpers.ThrowIfEmptyOrNull(feedback, "feedback");
            return result.SetTextFeedback(feedback, isSuccessfulAction);
        }
    }
}
