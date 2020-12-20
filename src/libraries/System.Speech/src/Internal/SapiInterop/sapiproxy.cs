using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace System.Speech.Internal.SapiInterop
{
    internal abstract class SapiProxy : IDisposable
    {
        //*******************************************************************
        //
        // Constructors
        //
        //*******************************************************************

        #region Constructors

        public virtual void Dispose ()
        {
            GC.SuppressFinalize (this);
        }

        #endregion

        //*******************************************************************
        //
        // Internal Methods
        //
        //*******************************************************************

        #region Internal Methods

        internal abstract object Invoke (ObjectDelegate pfn);
        internal abstract void Invoke2 (VoidDelegate pfn);

        #endregion

        //*******************************************************************
        //
        // Internal Properties
        //
        //*******************************************************************

        #region Internal Properties

        internal ISpRecognizer Recognizer
        {
            get
            {
                return _recognizer;
            }
        }

        internal ISpRecognizer2 Recognizer2
        {
            get
            {
                if (_recognizer2 == null)
                {
                    _recognizer2 = (ISpRecognizer2) _recognizer;
                }
                return _recognizer2;
            }
        }

        internal ISpeechRecognizer SapiSpeechRecognizer
        {
            get
            {
                if (_speechRecognizer == null)
                {
                    _speechRecognizer = (ISpeechRecognizer) _recognizer;
                }
                return _speechRecognizer;
            }
        }

        #endregion

        //*******************************************************************
        //
        // Protected Fields
        //
        //*******************************************************************

        #region Protected Fields

        protected ISpeechRecognizer _speechRecognizer;
        protected ISpRecognizer2 _recognizer2;
        protected ISpRecognizer _recognizer;

        #endregion

        //*******************************************************************
        //
        // Internal Types
        //
        //*******************************************************************

        #region Protected Fields

        internal class PassThrough : SapiProxy, IDisposable
        {
            //*******************************************************************
            //
            // Constructors
            //
            //*******************************************************************

            #region Constructors

            internal PassThrough (ISpRecognizer recognizer)
            {
                _recognizer = recognizer;
            }

            ~PassThrough ()
            {
                Dispose (false);
            }
            public override void Dispose ()
            {
                try
                {
                    Dispose (true);
                }
                finally
                {
                    base.Dispose ();
                }
            }

            #endregion

            //*******************************************************************
            //
            // Internal Methods
            //
            //*******************************************************************

            #region Internal Methods

            override internal object Invoke (ObjectDelegate pfn)
            {
                return pfn.Invoke ();
            }

            override internal void Invoke2 (VoidDelegate pfn)
            {
                pfn.Invoke ();
            }

            #endregion

            //*******************************************************************
            //
            // Private Methods
            //
            //*******************************************************************

            #region Private Methods

            private void Dispose (bool disposing)
            {
                _recognizer2 = null;
                _speechRecognizer = null;
                Marshal.ReleaseComObject (_recognizer);
            }

            #endregion
        }


#pragma warning disable 1634, 1691 // Allows suppression of certain PreSharp messages.
#pragma warning disable 56500 // Remove all the catch all statements warnings used by the interop layer

        internal class MTAThread : SapiProxy, IDisposable
        {
            //*******************************************************************
            //
            // Constructors
            //
            //*******************************************************************

            #region Constructors

            internal MTAThread (SapiRecognizer.RecognizerType type)
            {
                _mta = new Thread (new ThreadStart (SapiMTAThread));
                if (!_mta.TrySetApartmentState (ApartmentState.MTA))
                {
                    throw new InvalidOperationException ();
                }
                _mta.IsBackground = true;
                _mta.Start ();

                if (type == SapiRecognizer.RecognizerType.InProc)
                {
                    Invoke2 (delegate { _recognizer = (ISpRecognizer) new SpInprocRecognizer (); });
                }
                else
                {
                    Invoke2 (delegate { _recognizer = (ISpRecognizer) new SpSharedRecognizer (); });
                }
            }

            ~MTAThread ()
            {
                Dispose (false);
            }

            public override void Dispose ()
            {
                try
                {
                    Dispose (true);
                }
                finally
                {
                    base.Dispose ();
                }
            }

            #endregion

            //*******************************************************************
            //
            // Internal Methods
            //
            //*******************************************************************

            #region Internal Methods

            override internal object Invoke (ObjectDelegate pfn)
            {
                lock (this)
                {
                    _doit = pfn;
                    _process.Set ();
                    _done.WaitOne ();
                    if (_exception == null)
                    {
                        return _result;
                    }
                    else
                    {
                        throw _exception;
                    }
                }
            }

            override internal void Invoke2 (VoidDelegate pfn)
            {
                lock (this)
                {
                    _doit2 = pfn;
                    _process.Set ();
                    _done.WaitOne ();
                    if (_exception != null)
                    {
                        throw _exception;
                    }
                }
            }

            #endregion

            //*******************************************************************
            //
            // Private Methods
            //
            //*******************************************************************

            #region Private Methods

            private void Dispose (bool disposing)
            {
                lock (this)
                {
                    _recognizer2 = null;
                    _speechRecognizer = null;
                    Invoke2 (delegate { Marshal.ReleaseComObject (_recognizer); });
                    ((IDisposable) _process).Dispose ();
                    ((IDisposable) _done).Dispose ();
                    _mta.Abort ();
                }
                base.Dispose ();
            }

            private void SapiMTAThread ()
            {
                while (true)
                {
                    try
                    {
                        _process.WaitOne ();
                        _exception = null;
                        if (_doit != null)
                        {
                            _result = _doit.Invoke ();
                            _doit = null;
                        }
                        else
                        {
                            _doit2.Invoke ();
                            _doit2 = null;
                        }
                    }
                    catch (Exception e)
                    {
                        _exception = e;
                    }
                    _done.Set ();
                }
            }

            #endregion

            //*******************************************************************
            //
            // Private Fields
            //
            //*******************************************************************

            #region Private Fields

            private Thread _mta;
            private AutoResetEvent _process = new AutoResetEvent (false);
            private AutoResetEvent _done = new AutoResetEvent (false);
            private ObjectDelegate _doit;
            private VoidDelegate _doit2;
            private object _result;
            private Exception _exception;

            #endregion
        }

        internal delegate object ObjectDelegate ();
        internal delegate void VoidDelegate ();
    }

        #endregion
}
