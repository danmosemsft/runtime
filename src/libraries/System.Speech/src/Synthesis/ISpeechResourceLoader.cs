//------------------------------------------------------------------
// <copyright file="ISpeechResourceLoader.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------

#if SPEECHSERVER || PROMPT_ENGINE

using System;

namespace System.Speech.Synthesis
{
    /// <summary>
    /// Resource Loader interface definition
    /// </summary>
    internal interface ISpeechResourceLoader
    {
        /// <summary>
        /// Converts the resourcePath to a location in the file cache and returns a reference into the 
        /// cache
        /// </summary>
        /// <param name="resourcePath"></param>
        /// <param name="mimeType"></param>
        /// <param name="redirectUrl"></param>
        string GetLocalCopy (Uri resourcePath, out string mimeType, out Uri redirectUrl);

        /// <summary>
        /// Mark an entry in the file cache as unused.
        /// </summary>
        /// <param name="path"></param>
        void ReleaseLocalCopy (string path);
    }
}

#endif
