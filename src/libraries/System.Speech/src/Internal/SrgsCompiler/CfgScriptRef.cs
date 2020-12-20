using System;
using System.Runtime.InteropServices;
using System.Speech.Internal.SrgsParser;

namespace System.Speech.Internal.SrgsCompiler
{
    /// <summary>
    /// Summary description for CfgScriptRef.
    /// </summary>
    [StructLayout (LayoutKind.Sequential)]
    internal struct CfgScriptRef
    {
        //*******************************************************************
        //
        // Internal Fields
        //
        //*******************************************************************

        #region Internal Fields

        // should be private but the order is absolutly key for marshalling
        internal int _idRule;

        internal int _idMethod;

        internal RuleMethodScript _method;

        #endregion
    }
}
