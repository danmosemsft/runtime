//------------------------------------------------------------------
// <copyright file="IPromptEngine.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------

#if SPEECHSERVER || PROMPT_ENGINE

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Speech.Internal;
using System.Speech.Synthesis;

namespace System.Speech.Synthesis.TtsEngine
{
    /// <summary>
    /// TODOC
    /// </summary>
    [ComImport, Guid ("CD5CF526-EF11-4ED1-97B1-2569FD6F2320"), InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
#if !PROMPT_ENGINE
    internal 
#else
    public
#endif
interface IPromptEngine
    {
        /// <summary>
        /// TODOC
        /// </summary>
        /// <param name="backupVoice"></param>
        void SetBackupVoice (ITtsEngineSsml backupVoice);

        /// <summary>
        /// TODOC
        /// </summary>
        /// <param name="name"></param>
        /// <param name="alias"></param>
        void LoadDatabase ([MarshalAs (UnmanagedType.LPWStr)] string name, [MarshalAs (UnmanagedType.LPWStr)]  string alias);

        /// <summary>
        /// TODOC
        /// </summary>
        /// <param name="alias"></param>
        void UnloadDatabase ([MarshalAs (UnmanagedType.LPWStr)] string alias);
    };
}

#endif

