// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;

namespace System.Speech.Internal.Synthesis
{
    internal class AudioData : IDisposable
    {
        internal Uri _uri;

        internal string _mimeType;

        internal Stream _stream;

        private string _localFile;

        private ResourceLoader _resourceLoader;

        internal AudioData(Uri uri, ResourceLoader resourceLoader)
        {
            _uri = uri;
            _resourceLoader = resourceLoader;
            _stream = _resourceLoader.LoadFile(uri, out _mimeType, out Uri _, out _localFile);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        ~AudioData()
        {
            Dispose(disposing: false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_localFile != null)
                {
                    _resourceLoader.UnloadFile(_localFile);
                }
                if (_stream != null)
                {
                    _stream.Dispose();
                    _stream = null;
                    _localFile = null;
                    _uri = null;
                }
            }
        }
    }
}
