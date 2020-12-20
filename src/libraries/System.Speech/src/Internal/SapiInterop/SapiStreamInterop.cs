//------------------------------------------------------------------
// <copyright file="SapiStreamInterop.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

using STATSTG = System.Runtime.InteropServices.ComTypes.STATSTG;

namespace System.Speech.Internal.SapiInterop
{
    #region enum

    internal enum SPFILEMODE
    {
        SPFM_OPEN_READONLY = 0,
        SPFM_CREATE_ALWAYS = 3
    }

    #endregion Enum

    #region Interface

    [ComImport, Guid ("BED530BE-2606-4F4D-A1C0-54C5CDA5566F"), InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ISpStreamFormat : IStream
    {
        // ISequentialStream Methods
        new void Read ([MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 1), Out] Byte [] pv, int cb, IntPtr pcbRead);
        new void Write ([MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 1)] Byte [] pv, int cb, IntPtr pcbWritten);

        // IStream Methods
        new void Seek (Int64 dlibMove, int dwOrigin, IntPtr plibNewPosition);
        new void SetSize (Int64 libNewSize);
        new void CopyTo (IStream pstm, Int64 cb, IntPtr pcbRead, IntPtr pcbWritten);
        new void Commit (int grfCommitFlags);
        new void Revert ();
        new void LockRegion (Int64 libOffset, Int64 cb, int dwLockType);
        new void UnlockRegion (Int64 libOffset, Int64 cb, int dwLockType);
        new void Stat (out STATSTG pstatstg, int grfStatFlag);
        new void Clone (out IStream ppstm);

        // ISpStreamFormat Methods
        void GetFormat (out Guid pguidFormatId, out IntPtr ppCoMemWaveFormatEx);
    }

    [ComImport, Guid ("BED530BE-2606-4F4D-A1C0-54C5CDA5566F"), InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ISpStream : ISpStreamFormat
    {
        // ISequentialStream Methods
        new void Read ([MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 1), Out] Byte [] pv, int cb, IntPtr pcbRead);
        new void Write ([MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 1)] Byte [] pv, int cb, IntPtr pcbWritten);
        // IStream Methods
        new void Seek (Int64 dlibMove, int dwOrigin, IntPtr plibNewPosition);
        new void SetSize (Int64 libNewSize);
        new void CopyTo (IStream pstm, Int64 cb, IntPtr pcbRead, IntPtr pcbWritten);
        new void Commit (int grfCommitFlags);
        new void Revert ();
        new void LockRegion (Int64 libOffset, Int64 cb, int dwLockType);
        new void UnlockRegion (Int64 libOffset, Int64 cb, int dwLockType);
        new void Stat (out STATSTG pstatstg, int grfStatFlag);
        new void Clone (out IStream ppstm);
        // ISpStreamFormat Methods
        new void GetFormat (out Guid pguidFormatId, out IntPtr ppCoMemWaveFormatEx);

        // ISpStream Methods
        void SetBaseStream (IStream pStream, ref Guid rguidFormat, IntPtr pWaveFormatEx);
        void Slot14 (); // void GetBaseStream(IStream ** ppStream);
        void BindToFile (string pszFileName, SPFILEMODE eMode, ref Guid pFormatId, IntPtr pWaveFormatEx, UInt64 ullEventInterest);
        void Close ();
    }

    #endregion
}
