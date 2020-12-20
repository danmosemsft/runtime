//---------------------------------------------------------------------------
// <copyright file="HGlobalSafeHandle.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
//
// Description: 
//		Stream Helper.
//		Allocates a global memory buffer to do marshaling between a 
//		binary and a structured data. The global memory size increases and 
//		never shrinks. 
//		REVIEW: jeanfp use GCHandle rather HGlobal
//
// History:
//		5/1/2004	jeanfp		
//---------------------------------------------------------------------------
using System;
using System.Runtime.InteropServices;

namespace System.Speech.Internal
{
    /// <summary>
    /// Encapsulate SafeHandle for Win32 Memory Handles
    /// </summary>
    internal sealed class HGlobalSafeHandle : SafeHandle
    {
        //*******************************************************************
        //
        // Constructors
        //
        //*******************************************************************

        #region Constructors

        internal HGlobalSafeHandle () : base (IntPtr.Zero, true)
        {
        }

        // This destructor will run only if the Dispose method 
        // does not get called.
        ~HGlobalSafeHandle ()
        {
            Dispose (false);
        }

        protected override void Dispose (bool disposing)
        {
            ReleaseHandle ();
            base.Dispose (disposing);
        }

        #endregion

        //*******************************************************************
        //
        // Internal Methods
        //
        //*******************************************************************

        #region internal Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        internal IntPtr Buffer (int size)
        {
            if (size > _bufferSize)
            {
                if (_bufferSize == 0)
                {
                    SetHandle (Marshal.AllocHGlobal (size));
                }
                else
                {
                    SetHandle (Marshal.ReAllocHGlobal (handle, (IntPtr) size));
                }

                GC.AddMemoryPressure (size - _bufferSize);
                _bufferSize = size;
            }

            return handle;
        }

        /// <summary>
        /// True if the no memory is allocated
        /// </summary>
        /// <value></value>
        public override bool IsInvalid
        {
            get
            {
                return handle == IntPtr.Zero;
            }
        }

        #endregion

        //*******************************************************************
        //
        // Protected Methods
        //
        //*******************************************************************

        #region Protected Methods

        /// <summary>
        /// Releases the Win32 Memory handle
        /// </summary>
        /// <returns></returns>
        protected override bool ReleaseHandle ()
        {
            if (handle != IntPtr.Zero)
            {
                // Reset the extra information given to the GC
                if (_bufferSize > 0)
                {
                    GC.RemoveMemoryPressure (_bufferSize);
                    _bufferSize = 0;
                }

                Marshal.FreeHGlobal (handle);
                handle = IntPtr.Zero;
                return true;
            }

            return false;
        }


        #endregion

        //*******************************************************************
        //
        // Private Fields
        //
        //*******************************************************************

        #region Private Fields

        private int _bufferSize;

        #endregion
    }
}
