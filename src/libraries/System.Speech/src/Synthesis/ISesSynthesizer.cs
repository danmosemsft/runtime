//------------------------------------------------------------------
// <copyright file="ISesSynthesizer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------

#if SPEECHSERVER || PROMPT_ENGINE

using System;
using System.Collections.Generic;
using System.Speech.Synthesis;

namespace System.Speech.Synthesis
{
    /// <summary>
    /// Private SES interface to specify prompt data base and to hook the file cache.
    /// </summary>
    internal interface ISesSynthesizer
    {
        /// <summary>
        /// Load a prompt database giving a name and alias
        /// </summary>
        /// <param name="localName"></param>
        /// <param name="alias"></param>
        void LoadDatabase (string localName, string alias);

        /// <summary>
        /// Unload a prompt database
        /// </summary>
        /// <param name="alias"></param>
        void UnloadDatabase (string alias);

        /// <summary>
        /// Set the resource loader
        /// </summary>
        /// <param name="resourceLoader"></param>
        void SetResourceLoader (ISpeechResourceLoader resourceLoader);
    }
}

#endif

