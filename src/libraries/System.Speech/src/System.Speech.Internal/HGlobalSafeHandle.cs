// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace System.Speech.Internal
{
    internal sealed class HGlobalSafeHandle : SafeHandle
    {
        private int _bufferSize;

        public override bool IsInvalid => handle == IntPtr.Zero;

        internal HGlobalSafeHandle()
            : base(IntPtr.Zero, ownsHandle: true)
        {
        }

        ~HGlobalSafeHandle()
        {
            Dispose(disposing: false);
        }

        protected override void Dispose(bool disposing)
        {
            ReleaseHandle();
            base.Dispose(disposing);
        }

        internal IntPtr Buffer(int size)
        {
            if (size > _bufferSize)
            {
                if (_bufferSize == 0)
                {
                    SetHandle(Marshal.AllocHGlobal(size));
                }
                else
                {
                    SetHandle(Marshal.ReAllocHGlobal(handle, (IntPtr)size));
                }
                GC.AddMemoryPressure(size - _bufferSize);
                _bufferSize = size;
            }
            return handle;
        }

        protected override bool ReleaseHandle()
        {
            if (handle != IntPtr.Zero)
            {
                if (_bufferSize > 0)
                {
                    GC.RemoveMemoryPressure(_bufferSize);
                    _bufferSize = 0;
                }
                Marshal.FreeHGlobal(handle);
                handle = IntPtr.Zero;
                return true;
            }
            return false;
        }
    }
}
