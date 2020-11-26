// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace System.Speech.Internal.SapiInterop
{
    [ComImport]
    [Guid("B9AC5783-FCD0-4b21-B119-B4F8DA8FD2C3")]
    internal interface ISpGrammarResourceLoader
    {
        [PreserveSig]
        int LoadResource(string bstrResourceUri, bool fAlwaysReload, out IStream pStream, ref string pbstrMIMEType, ref short pfModified, ref string pbstrRedirectUrl);

        string GetLocalCopy(Uri resourcePath, out string mimeType, out Uri redirectUrl);

        void ReleaseLocalCopy(string path);
    }
}
