
using System;

namespace System.Speech.Synthesis
{
	/// <summary>
	/// TODOC
	/// </summary>
    public class StateChangedEventArgs : EventArgs
    {
		//*******************************************************************
		//
		// Constructors
		//
		//*******************************************************************

		#region Constructors

        /// <summary>
        /// TODOC
        /// </summary>
        /// <param name="state"></param>
        /// <param name="previousState"></param>
        internal StateChangedEventArgs (SynthesizerState state, SynthesizerState previousState)
        {
            _state = state;
            _previousState = previousState;
        }

		#endregion

		//*******************************************************************
		//
		// Public Properties
		//
        //*******************************************************************

        #region public Properties

        // Use Add* naming convention.

        /// <summary>
		/// TODOC
		/// </summary>
        public SynthesizerState State
        {
            get
            {
                return _state;
            }
        }

        /// <summary>
        /// TODOC
        /// </summary>
        public SynthesizerState PreviousState
        {
            get
            {
                return _previousState;
            }
        }

		#endregion

		//*******************************************************************
		//
		// Private Fields
		//
		//*******************************************************************

		#region Private Fields

        private SynthesizerState _state;

        private SynthesizerState _previousState;

        #endregion
    }
}

