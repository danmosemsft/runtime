// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Internal.Synthesis
{
    internal class LexiconEntry
    {
        internal Uri _uri;

        internal string _mediaType;

        internal LexiconEntry(Uri uri, string mediaType)
        {
            _uri = uri;
            _mediaType = mediaType;
        }

        public override bool Equals(object obj)
        {
            LexiconEntry lexiconEntry = obj as LexiconEntry;
            if (lexiconEntry != null)
            {
                return _uri.Equals(lexiconEntry._uri);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return _uri.GetHashCode();
        }
    }
}
