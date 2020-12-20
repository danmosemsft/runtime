using System;
using System.Runtime.Serialization;

namespace System.Speech.GrammarBuilding
{
	/// <summary>
	/// Exception class for the Srgs Grammar Compiler
	/// </summary>
	[Serializable]
	
    public class SrgsGrammarException : SystemException
	{
        /// <summary>
        /// TODOC
        /// </summary>
        public SrgsGrammarException ()
        {
        }

        /// <summary>
        /// TODOC
		/// </summary>
		public SrgsGrammarException(string message) : base (message)
		{
		}

		/// <summary>
		/// TODOC
		/// </summary>
		public SrgsGrammarException(string message, Exception innerException) : base (message, innerException)
		{
		}

        /// <summary>
        /// TODOC
        /// </summary>
        protected SrgsGrammarException (SerializationInfo info, StreamingContext context) : base (info, context)
        {
        }
    }
}
