// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;
using System.Speech.Internal;

namespace System.Speech.Synthesis.TtsEngine
{
    /// <summary>Contains information about the content type (such as currency, date, or address) or language construct that determine how text should be spoken.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public class SayAs
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        private string _interpretAs;

        [MarshalAs(UnmanagedType.LPWStr)]
        private string _format;

        [MarshalAs(UnmanagedType.LPWStr)]
        private string _detail;

        /// <summary>Gets or sets the value of the <c>interpret-as</c> attribute for a <c>say-as</c> element in the SSML markup language of a prompt.</summary>
        public string InterpretAs
        {
            get
            {
                return _interpretAs;
            }
            set
            {
                Helpers.ThrowIfEmptyOrNull(value, "value");
                _interpretAs = value;
            }
        }

        /// <summary>Gets or sets the value of the <c>format</c> attribute for a <c>say-as</c> element in the SSML markup language of a prompt.</summary>
        public string Format
        {
            get
            {
                return _format;
            }
            set
            {
                Helpers.ThrowIfEmptyOrNull(value, "value");
                _format = value;
            }
        }

        /// <summary>Gets or sets the value of the <c>detail</c> attribute for a <c>say-as</c> element in the SSML markup language of a prompt.</summary>
        public string Detail
        {
            get
            {
                return _detail;
            }
            set
            {
                Helpers.ThrowIfEmptyOrNull(value, "value");
                _detail = value;
            }
        }

        /// <summary>Creates a new instance of the <c>SayAs</c> class.</summary>
        public SayAs()
        {
        }
    }
}
