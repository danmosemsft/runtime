//------------------------------------------------------------------
// <copyright file="SpeechUI.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------

#if !SPEECHSERVER

using System.Speech.Internal;

namespace System.Speech.Recognition
{
    /// TODOC <_include file='doc\SpeechUI.uex' path='docs/doc[@for="SpeechUI"]/*' />
    public class SpeechUI
    {
        internal SpeechUI()
        {
        }

        /// TODOC <_include file='doc\SpeechUI.uex' path='docs/doc[@for="SpeechUI.SendTextFeedback"]/*' />
        public static bool SendTextFeedback(RecognitionResult result, string feedback, bool isSuccessfulAction)
        {
            Helpers.ThrowIfNull (result,  "result");
            Helpers.ThrowIfEmptyOrNull (feedback, "feedback");

            return result.SetTextFeedback(feedback, isSuccessfulAction);
      }
    }
}
#endif
