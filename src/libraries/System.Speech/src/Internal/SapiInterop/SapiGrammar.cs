//------------------------------------------------------------------
// <copyright file="SapiGrammar.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
//  All the calls to SAPI interfaces are wraped into the class 'SapiRecognizer',
// 'SapiContext' and 'SapiGrammar'. 
//
// The SAPI call are executed in the context of a proxy that is either a 
// pass-through or forward the request to an MTA thread for SAPI 5.1
//
// History:
//		4/1/2006	jeanfp		
//------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Speech.Recognition;
using System.Speech.Internal.ObjectTokens;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Speech.Internal.SapiInterop
{
    internal class SapiGrammar : IDisposable
    {
        //*******************************************************************
        //
        // Constructors
        //
        //*******************************************************************

        #region Constructors

        internal SapiGrammar (ISpRecoGrammar sapiGrammar, SapiProxy thread)
        {
            _sapiGrammar = sapiGrammar;
            _sapiProxy = thread;
        }

        public void Dispose ()
        {
            if (!_disposed)
            {
                Marshal.ReleaseComObject (_sapiGrammar);
                GC.SuppressFinalize (this);
                _disposed = true;
            }
        }

        #endregion

        //*******************************************************************
        //
        // Internal Methods
        //
        //*******************************************************************

        #region Internal Methods

        internal void SetGrammarState (SPGRAMMARSTATE state)
        {
            _sapiProxy.Invoke2 (delegate { _sapiGrammar.SetGrammarState (state); });
        }

        internal void SetWordSequenceData (string text, SPTEXTSELECTIONINFO info)
        {
            SPTEXTSELECTIONINFO selectionInfo = info;
            _sapiProxy.Invoke2 (delegate { _sapiGrammar.SetWordSequenceData (text, (uint) text.Length, ref selectionInfo); });
        }

        internal void LoadCmdFromMemory (IntPtr grammar, SPLOADOPTIONS options)
        {
            _sapiProxy.Invoke2 (delegate { _sapiGrammar.LoadCmdFromMemory (grammar, options); });
        }

        internal void LoadDictation (string pszTopicName, SPLOADOPTIONS options)
        {
            _sapiProxy.Invoke2 (delegate { _sapiGrammar.LoadDictation (pszTopicName, options); });
        }

        internal SAPIErrorCodes SetDictationState (SPRULESTATE state)
        {
            return (SAPIErrorCodes) _sapiProxy.Invoke (delegate { return _sapiGrammar.SetDictationState (state); });
        }

        internal SAPIErrorCodes SetRuleState (string name, SPRULESTATE state)
        {
            return (SAPIErrorCodes) _sapiProxy.Invoke (delegate { return _sapiGrammar.SetRuleState (name, IntPtr.Zero, state); });
        }
       
        /*
         * The Set of methods are only available with SAPI 5.3. There is no need then to use the SAPI proxy to switch 
         * the call to an MTA thread.
         * 
         */
        internal void SetGrammarLoader (ISpGrammarResourceLoader resourceLoader)
        {
            SpRecoGrammar2.SetGrammarLoader (resourceLoader);
        }

        internal void LoadCmdFromMemory2 (IntPtr grammar, SPLOADOPTIONS options, string sharingUri, string baseUri)
        {
            SpRecoGrammar2.LoadCmdFromMemory2 (grammar, options, sharingUri, baseUri);
        }

        internal void SetRulePriority (string name, UInt32 id, Int32 priority)
        {
            SpRecoGrammar2.SetRulePriority (name, id, priority);
        }
        internal void SetRuleWeight (string name, UInt32 id, float weight)
        {
            SpRecoGrammar2.SetRuleWeight (name, id, weight);
        }
        internal void SetDictationWeight (float weight)
        {
            SpRecoGrammar2.SetDictationWeight (weight);
        }

        #endregion

        //*******************************************************************
        //
        // Internal Properties
        //
        //*******************************************************************

        #region Internal Properties

        internal ISpRecoGrammar2 SpRecoGrammar2
        {
            get
            {
                if (_sapiGrammar2 == null)
                {
                    _sapiGrammar2 = (ISpRecoGrammar2) _sapiGrammar;
                }
                return (ISpRecoGrammar2) _sapiGrammar2;
            }
        }

        #endregion

        //*******************************************************************
        //
        // Private Fields
        //
        //*******************************************************************

        #region Private Methods

        private ISpRecoGrammar2 _sapiGrammar2;
        private ISpRecoGrammar _sapiGrammar;
        private SapiProxy _sapiProxy;
        private bool _disposed;

        #endregion
    }
}
