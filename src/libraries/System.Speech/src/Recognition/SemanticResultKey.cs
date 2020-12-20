
using System.Collections.Generic;
using System.Diagnostics;
using System.Speech.Internal.GrammarBuilding;
using System.Speech.Internal;

namespace System.Speech.Recognition
{
    /// <summary>
    /// 
    /// </summary>

    [DebuggerDisplay ("{_semanticKey.DebugSummary}")]
    public class SemanticResultKey
    {
        //*******************************************************************
        //
        // Constructors
        //
        //*******************************************************************

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="semanticResultKey"></param>
        private SemanticResultKey (string semanticResultKey)
            : base ()
        {
            Helpers.ThrowIfEmptyOrNull (semanticResultKey, "semanticResultKey");

            _semanticKey = new SemanticKeyElement (semanticResultKey);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="semanticResultKey"></param>
        /// <param name="phrases"></param>
        public SemanticResultKey (string semanticResultKey, params string [] phrases)
            : this (semanticResultKey)
        {
            Helpers.ThrowIfEmptyOrNull (semanticResultKey, "semanticResultKey");
            Helpers.ThrowIfNull (phrases, "phrases");

            // Build a grammar builder with all the phrases
            foreach (string phrase in phrases)
            {
                _semanticKey.Add ((string) phrase.Clone ());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="semanticResultKey"></param>
        /// <param name="builders"></param>
        public SemanticResultKey (string semanticResultKey, params GrammarBuilder [] builders)
            : this (semanticResultKey)
        {
            Helpers.ThrowIfEmptyOrNull (semanticResultKey, "semanticResultKey");
            Helpers.ThrowIfNull (builders, "phrases");

            // Build a grammar builder with all the grammar builders
            foreach (GrammarBuilder builder in builders)
            {
                _semanticKey.Add (builder.Clone ());
            }
        }

        #endregion

        //*******************************************************************
        //
        // Public Methods
        //
        //*******************************************************************

        #region Public Methods

        /// <summary>
        /// TODOC
        /// </summary>
        /// <returns></returns>
        public GrammarBuilder ToGrammarBuilder ()
        {
                return new GrammarBuilder (this);
        }

        #endregion

        //*******************************************************************
        //
        // Internal Properties
        //
        //*******************************************************************

        #region Internal Properties

        internal SemanticKeyElement SemanticKeyElement
        {
            get
            {
                return _semanticKey;
            }
        }

        #endregion

        //*******************************************************************
        //
        // Private Fields
        //
        //*******************************************************************

        #region Private Fields

        private readonly SemanticKeyElement _semanticKey;

        #endregion

    }
}
