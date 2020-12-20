using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Speech.AudioFormat;
using System.Speech.Internal.SapiInterop;
using System.Speech.Internal.Synthesis;

#pragma warning disable 1634, 1691 // Allows suppression of certain PreSharp messages.

using STATSTG = System.Runtime.InteropServices.ComTypes.STATSTG;

namespace System.Speech.Internal.SapiInterop
{
    internal class SpStreamWrapper : IStream, IDisposable
    {
        //*******************************************************************
        //
        // Constructors
        //
        //*******************************************************************

        #region Constructors

        internal SpStreamWrapper (Stream stream)
        {
            _stream = stream;
            _endOfStreamPosition = stream.Length;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose ()
        {
            _stream.Dispose ();
            GC.SuppressFinalize (this);
        }

        #endregion

        //*******************************************************************
        //
        // Public Methods
        //
        //*******************************************************************

        #region public Methods

        #region ISpStreamFormat interface implementation

        public void Read (byte [] pv, int cb, IntPtr pcbRead)
        {
            if (_endOfStreamPosition >= 0 && _stream.Position + cb > _endOfStreamPosition)
            {
                cb = (int) (_endOfStreamPosition - _stream.Position);
            }

            int read = 0;
            try
            {
                read = _stream.Read (pv, 0, cb);
            }
            catch (EndOfStreamException)
            {
                read = 0;
            }

            if (pcbRead != IntPtr.Zero)
            {
                Marshal.WriteIntPtr (pcbRead, new IntPtr (read));
            }
        }

        public void Write (byte [] pv, int cb, IntPtr pcbWritten)
        {
            throw new NotSupportedException ();
        }

        public void Seek (long offset, int seekOrigin, IntPtr plibNewPosition)
        {
            _stream.Seek (offset, (SeekOrigin) seekOrigin);

            if (plibNewPosition != IntPtr.Zero)
            {
                Marshal.WriteIntPtr (plibNewPosition, new IntPtr (_stream.Position));
            }
        }
        public void SetSize (long libNewSize)
        {
            throw new NotSupportedException ();
        }
        public void CopyTo (IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten)
        {
            throw new NotSupportedException ();
        }
        public void Commit (int grfCommitFlags)
        {
            _stream.Flush ();
        }
        public void Revert ()
        {
            throw new NotSupportedException ();
        }
        public void LockRegion (long libOffset, long cb, int dwLockType)
        {
            throw new NotSupportedException ();
        }
        public void UnlockRegion (long libOffset, long cb, int dwLockType)
        {
            throw new NotSupportedException ();
        }
        public void Stat (out STATSTG pstatstg, int grfStatFlag)
        {
            pstatstg = new STATSTG ();
            pstatstg.cbSize = _stream.Length;
        }

        public void Clone (out IStream ppstm)
        {
            throw new NotSupportedException ();
        }

        #endregion

        #endregion

        //*******************************************************************
        //
        // Private Fields
        //
        //*******************************************************************

        #region Private Fields

        private Stream _stream;
        protected long _endOfStreamPosition = -1;

        #endregion

    }
}
