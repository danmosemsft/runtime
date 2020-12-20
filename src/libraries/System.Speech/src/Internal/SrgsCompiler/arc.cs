//---------------------------------------------------------------------------
// <copyright file="Arc.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
//
//
// Description: 
//		Custom list to handle efficently the removal of elements 
//
// History:
//		5/1/2004	jeanfp		
//---------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Speech.Internal.SrgsParser;
using System.Text;

namespace System.Speech.Internal.SrgsCompiler
{
    /// <summary>
    /// Summary description for Arc.
    /// </summary>
#if DEBUG
    [DebuggerDisplay ("{ToString ()}")]
#endif
    internal class Arc : IComparer<Arc>, IComparable<Arc>
    {
        //*******************************************************************
        //
        // Constructors
        //
        //*******************************************************************

        #region Constructors

        internal Arc ()
        {
#if VIEW_STATS
            GrammarBackEnd._cArcs++;
#endif
        }

        internal Arc (Arc arc)
            : this ()
        {
            _start = arc._start;
            _end = arc._end;
            _iWord = arc._iWord;
            _flWeight = arc._flWeight;
            _confidence = arc._confidence;
            _ruleRef = arc._ruleRef;
            _specialTransitionIndex = arc._specialTransitionIndex;
            _iSerialize = arc._iSerialize;
            _matchMode = arc._matchMode;
#if DEBUG
            _fCheckingForExitPath = arc._fCheckingForExitPath;
            _be = arc._be;
#endif
        }

        internal Arc (Arc arc, State start, State end)
            : this (arc)
        {
            _start = start;
            _end = end;
        }

        internal Arc (Arc arc, State start, State end, int wordId)
            : this (arc, start, end)
        {
            _iWord = wordId;
        }

        internal Arc (string sWord, Rule ruleRef, StringBlob words, float flWeight, int confidence, Rule specialRule, MatchMode matchMode, ref bool fNeedWeightTable)
            : this (sWord, ruleRef, words, flWeight, confidence, specialRule, _serializeToken++, matchMode, ref fNeedWeightTable)
        {
        }

        private Arc (string sWord, Rule ruleRef, StringBlob words, float flWeight, int confidence, Rule specialRule, uint iSerialize, MatchMode matchMode, ref bool fNeedWeightTable)
            : this (0, flWeight, confidence, 0, matchMode, ref fNeedWeightTable)
        {
            //CfgGrammar.TraceInformation ("Arc::Arc");
            _ruleRef = ruleRef;
            _iSerialize = iSerialize;

            if (ruleRef == null)
            {
                if (specialRule != null)
                {
                    _specialTransitionIndex = (specialRule == CfgGrammar.SPRULETRANS_WILDCARD) ? CfgGrammar.SPWILDCARDTRANSITION : (specialRule == CfgGrammar.SPRULETRANS_DICTATION) ? CfgGrammar.SPDICTATIONTRANSITION : CfgGrammar.SPTEXTBUFFERTRANSITION;
                }
                else
                {
                    words.Add (sWord, out _iWord);
                }
            }
        }

        internal Arc (int iWord, float flWeight, int confidence, int ulSpecialTransitionIndex, MatchMode matchMode, ref bool fNeedWeightTable)
            : this ()
        {
            //CfgGrammar.TraceInformation ("Arc::Arc");
            _confidence = confidence;
            _iWord = iWord;
            _flWeight = flWeight;
            _matchMode = matchMode;
            _iSerialize = _serializeToken++;

            if (!flWeight.Equals (CfgGrammar.DEFAULT_WEIGHT))
            {
                fNeedWeightTable |= true;
            }

            _specialTransitionIndex = ulSpecialTransitionIndex;
        }

        #endregion

        //*******************************************************************
        //
        // Internal Methods
        //
        //*******************************************************************

        #region internal Methods

        #region IComparable<Arc> Interface implementation

        public int CompareTo (Arc obj1)
        {
            return Compare (this, obj1);
        }

        int IComparer<Arc>.Compare (Arc obj1, Arc obj2)
        {
            return Compare (obj1, obj2);
        }

        private int Compare (Arc obj1, Arc obj2)
        {
            if (obj1 == obj2)
                return 0;

            if (obj1 == null)
                return -1;

            if (obj2 == null)
                return 1;

            Arc arc1 = (Arc) obj1;
            Arc arc2 = (Arc) obj2;
            int diff = arc1.SortRank () - arc2.SortRank ();
            diff = diff != 0 ? diff : (int) arc1._iSerialize - (int) arc2._iSerialize;

            System.Diagnostics.Debug.Assert (diff != 0);
            return diff;
        }

        internal static int CompareContent (Arc arc1, Arc arc2)
        {
            // Compare arcs based on IndexOfWord, IsRuleRef, SpecialTransitionIndex, Optional, and RequiredConfidence.
            // SemanticTag, StartState, EndState, Weight, and SerializeIndex are not factors.
            if (arc1._iWord != arc2._iWord)
                return arc1._iWord - arc2._iWord;
            else
            {
                if (arc1._ruleRef != null || arc2._ruleRef != null || ((arc1._specialTransitionIndex - arc2._specialTransitionIndex) + (arc1._confidence - arc2._confidence) != 0))
                {
                    int diff = 0;
                    if (arc1._ruleRef != null || arc2._ruleRef != null)
                    {
                        if (arc1._ruleRef != null && arc2._ruleRef == null)
                        {
                            diff = -1;
                        }
                        else if (arc1._ruleRef == null && arc2._ruleRef != null)
                        {
                            diff = 1;
                        }
                        else
                        {
                            diff = String.Compare(arc1._ruleRef.Name, arc2._ruleRef.Name, StringComparison.CurrentCulture);
                        }
                    }

                    if (diff != 0)
                        return diff;
                    else if (arc1._specialTransitionIndex != arc2._specialTransitionIndex)
                        return arc1._specialTransitionIndex - arc2._specialTransitionIndex;
                    else if (arc1._confidence != arc2._confidence)
                        return arc1._confidence - arc2._confidence;
                }
                // An identical match
                return 0;
            }
        }

        internal static int CompareContentForKey (Arc arc1, Arc arc2)
        {
            int diff = CompareContent (arc1, arc2);
            if (diff == 0)
            {
                return (int) arc1._iSerialize - (int) arc2._iSerialize;
            }
            return diff;
        }

        #endregion

        internal float Serialize (StreamMarshaler streamBuffer, bool isLast, uint arcIndex)
        {
            //CfgGrammar.TraceInformation ("Arc::SerializeArcData");

            CfgArc A = new CfgArc ();

            A.LastArc = isLast;
            A.HasSemanticTag = SemanticTagCount > 0;
            A.NextStartArcIndex = (uint) (_end != null ? _end.SerializeId : 0);
            if (_ruleRef != null)
            {
                A.RuleRef = true;
                A.TransitionIndex = (uint) _ruleRef._iSerialize; //_pFirstState.SerializeId;
            }
            else
            {
                A.RuleRef = false;
                if (_specialTransitionIndex != 0)
                {
                    A.TransitionIndex = (uint) _specialTransitionIndex;
                }
                else
                {
                    A.TransitionIndex = (uint) _iWord;
                }
            }

            A.LowConfRequired = (_confidence < 0);
            A.HighConfRequired = (_confidence > 0);
            A.MatchMode = (uint) _matchMode;

            // For new arcs SerializeId is INFINITE so we set it correctly here.
            // For existing states we preserve the index from loading,
            //  unless new states have been added in, in which case the arc index,
            //  and hence the transition id have changed. There is a workaround in ReloadCmd
            //  to invalidate rules in this case.
            _iSerialize = arcIndex;

            streamBuffer.WriteStream (A);
            return _flWeight;
        }

        internal static float SerializeExtraEpsilonWithTag (StreamMarshaler streamBuffer, Arc arc, bool isLast, uint arcIndex)
        {
            CfgArc A = new CfgArc ();

            A.LastArc = isLast;
            A.HasSemanticTag = true;
            A.NextStartArcIndex = arcIndex;
            A.TransitionIndex = 0;

            A.LowConfRequired = false;
            A.HighConfRequired = false;
            A.MatchMode = (uint) arc._matchMode;

            streamBuffer.WriteStream (A);
            return arc._flWeight;
        }

        internal void SetArcIndexForTag (int iArc, uint iArcOffset, bool tagsCannotSpanOverMultipleArcs)
        {
            _startTags [iArc]._cfgTag.StartArcIndex = iArcOffset;
            _startTags [iArc]._cfgTag.ArcIndex = iArcOffset;
            if (tagsCannotSpanOverMultipleArcs)
            {
                _startTags [iArc]._cfgTag.EndArcIndex = iArcOffset;
            }
        }

        internal void SetEndArcIndexForTags ()
        {
            //CfgGrammar.TraceInformation ("Arc::SerializeSemanticData");
            if (_endTags != null)
            {
                foreach (Tag tag in _endTags)
                {
                    tag._cfgTag.EndArcIndex = (uint) _iSerialize;
                }
            }
        }

        /// <summary>
        /// Compare the contents and number of output arcs from the start state.
        /// The comparison is done by Arc content, number of arcs at then and the id for the last arc
        /// </summary>
        /// <param name="arc1"></param>
        /// <param name="arc2"></param>
        /// <returns></returns>
        internal static int CompareForDuplicateInputTransitions (Arc arc1, Arc arc2)
        {
            int iContentCompare = Arc.CompareContent (arc1, arc2);

            if (iContentCompare != 0)
            {
                return iContentCompare;
            }

            // Compare by arc Id
            return (int) (arc1._start != null ? arc1._start.Id : 0) - (int) (arc2._start != null ? arc2._start.Id : 0);
        }

        /// <summary>
        /// Compare the contents and number of input arcs to the end state.
        /// The comparison is done by Arc content, number of arcs at then and the id for the last arc
        /// </summary>
        /// <param name="arc1"></param>
        /// <param name="arc2"></param>
        /// <returns></returns>
        internal static int CompareForDuplicateOutputTransitions (Arc arc1, Arc arc2)
        {
            // Compare content and number of other input transitions to the end state.
            int iContentCompare = Arc.CompareContent (arc1, arc2);

            if (iContentCompare != 0)
            {
                return iContentCompare;
            }

            // Compare by arc Id
            return (int) (arc1._end != null ? arc1._end.Id : 0) - (int) (arc2._end != null ? arc2._end.Id : 0);
        }

        /// <summary>
        /// Compare the contents and start/end states of two arcs.
        /// </summary>
        /// <param name="arc1"></param>
        /// <param name="arc2"></param>
        /// <returns></returns>
        internal static int CompareIdenticalTransitions (Arc arc1, Arc arc2)
        {
            // Same start arc
            int diff = (int) (arc1._start != null ? arc1._start.Id : 0) - (int) (arc2._start != null ? arc2._start.Id : 0);
            if (diff == 0)
            {
                // Same end arc
                if ((diff = (int) (arc1._end != null ? arc1._end.Id : 0) - (int) (arc2._end != null ? arc2._end.Id : 0)) == 0)
                {
                    // Same tag
                    diff = arc1.SameTags (arc2) ? 0 : 1;
                }
            }
            return diff;
        }

        internal void AddStartTag (Tag tag)
        {
            if (_startTags == null)
            {
                _startTags = new Collection<Tag> ();
            }
            _startTags.Add (tag);
        }

        internal void AddEndTag (Tag tag)
        {
            if (_endTags == null)
            {
                _endTags = new Collection<Tag> ();
            }
            _endTags.Add (tag);
        }

        internal void ClearTags ()
        {
            _startTags = null;
            _endTags = null;
        }

        static internal void CopyTags (Arc src, Arc dest, Direction move)
        {
            // Copy the start tags if any
            if (src._startTags != null)
            {
                // if dest has not tags just move the collection
                if (dest._startTags == null)
                {
                    dest._startTags = src._startTags;
                }
                else
                {
                    if (move == Direction.Right)
                    {
                        for (int i = 0; i < src._startTags.Count; i++)
                        {
                            dest._startTags.Insert (i, src._startTags [i]);
                        }
                    }
                    else
                    {
                        // if dest has tags add the ones from the source to the existing ones
                        foreach (Tag tag in src._startTags)
                        {
                            dest._startTags.Add (tag);
                        }
                    }
                }
            }

            // Copy the end tags if any
            if (src._endTags != null)
            {
                // if dest has not tags just move the collection
                if (dest._endTags == null)
                {
                    dest._endTags = src._endTags;
                }
                else
                {
                    if (move == Direction.Right)
                    {
                        for (int i = 0; i < src._endTags.Count; i++)
                        {
                            dest._endTags.Insert (i, src._endTags [i]);
                        }
                    }
                    else
                    {
                        // if dest has tags add the ones from the source to the existing ones
                        foreach (Tag tag in src._endTags)
                        {
                            dest._endTags.Add (tag);
                        }
                    }
                }
            }

            // No tags src associated with the 'src' anymore
            src._startTags = src._endTags = null;
        }

        internal void CloneTags (Arc arc, List<Tag> _tags, Dictionary<Tag, Tag> endArcs, Backend be)
        {
            if (arc._startTags != null)
            {
                if (_startTags == null)
                {
                    _startTags = new Collection<Tag> ();
                }
                foreach (Tag tag in arc._startTags)
                {
                    Tag newTag = new Tag (tag);
                    _tags.Add (newTag);
                    _startTags.Add (newTag);
                    endArcs.Add (tag, newTag);
#if DEBUG
                    newTag._be = be;
#endif
                    if (be != null)
                    {
                        int idTagName;
                        newTag._cfgTag._nameOffset = be.Symbols.Add (tag._be.Symbols.FromOffset (tag._cfgTag._nameOffset), out idTagName);

                        if (tag._cfgTag._valueOffset != 0 && tag._cfgTag.PropVariantType == System.Runtime.InteropServices.VarEnum.VT_EMPTY)
                        {
                            newTag._cfgTag._valueOffset = be.Symbols.Add (tag._be.Symbols.FromOffset (tag._cfgTag._valueOffset), out idTagName);
                        }
                    }
                }
            }

            if (arc._endTags != null)
            {
                if (_endTags == null)
                {
                    _endTags = new Collection<Tag> ();
                }
                foreach (Tag tag in arc._endTags)
                {
                    Tag newTag = endArcs [tag];
                    _endTags.Add (newTag);
                    endArcs.Remove (tag);
                }
            }
        }

        internal bool SameTags (Arc arc)
        {
            // no tags ok
            bool same = _startTags == null && arc._startTags == null;

            // Compare each tag if not null
            if (!same && _startTags != null && arc._startTags != null && _startTags.Count == arc._startTags.Count)
            {
                same = true;
                for (int i = 0; i < _startTags.Count; i++)
                {
                    same &= _startTags [i] == arc._startTags [i];
                }
            }

            // Compare end tags if the start tags are equal
            if (same)
            {
                same = _endTags == null && arc._endTags == null;

                // Compare each tag if not null
                if (!same && _endTags != null && arc._endTags != null && _endTags.Count == arc._endTags.Count)
                {
                    same = true;
                    for (int i = 0; i < _endTags.Count; i++)
                    {
                        same &= _endTags [i] == arc._endTags [i];
                    }
                }
            }
            return same;
        }

        internal void ConnectStates ()
        {
            if (_end != null)
            {
                _end.InArcs.Add (this);
            }

            if (_start != null)
            {
                _start.OutArcs.Add (this);
            }
        }

        /// <summary>
        /// Is the arc an epsilon transition?
        /// </summary>
        /// <returns></returns>
        internal bool IsEpsilonTransition
        {
            get
            {
                return (_ruleRef == null) &&               // Not a ruleref 
                    (_specialTransitionIndex == 0) &&      // Not a special transition (wildcard, dictation, ...)
                    (_iWord == 0);                    // Not a word
            }
        }

        /// <summary>
        /// Is this arc an arc without attached properties?
        /// </summary>
        /// <returns>Is this arc an arc without attached properties?</returns>
        internal bool IsPropertylessTransition
        {
            get
            {
                // Does not own semantic property & No tag references
                return _startTags == null && _endTags == null;
            }
        }


#if DEBUG

        override public string ToString ()
        {
            return (_start != null ? "#" + _start.Id.ToString (CultureInfo.InvariantCulture) : "") + " <- " + DebuggerDisplayTags () + " -> " + (_end != null ? "#" + _end.Id.ToString (CultureInfo.InvariantCulture) : "");
        }

        internal string DebuggerDisplayTags ()
        {
            StringBuilder sb = new StringBuilder ();
            if (_iWord == 0 && (_ruleRef != null || _specialTransitionIndex != 0))
            {
                sb.Append ("<");
                if (_ruleRef != null)
                {
                    sb.Append (_ruleRef.Name);
                }
                else
                {
                    switch (_specialTransitionIndex)
                    {
                        case CfgGrammar.SPWILDCARDTRANSITION:
                            sb.Append ("GARBAGE");
                            break;

                        case CfgGrammar.SPTEXTBUFFERTRANSITION:
                            sb.Append ("TEXTBUFFER");
                            break;

                        case CfgGrammar.SPDICTATIONTRANSITION:
                            sb.Append ("DICTATION");
                            break;
                    }
                }
                sb.Append (">");
            }
            else
            {
                sb.Append ("'");
                sb.Append (_iWord == 0 ? new string (new char [] { (char) 0x3b5 }) : _be != null ? _be.Words [_iWord] : _iWord.ToString (CultureInfo.InvariantCulture));
                sb.Append ("'");
            }

            if (_startTags != null || _endTags != null)
            {
                // Check if the tags are the same
                bool same = _startTags != null && _endTags != null && _endTags.Count == _startTags.Count;

                // Compare each tag if not null
                for (int i = 0; same && i < _endTags.Count; i++)
                {
                    same &= _startTags [i] == _endTags [i];
                }

                sb.Append (" (");
                if (_startTags != null)
                {
                    bool first = true;
                    foreach (Tag tag in _startTags)
                    {
                        if (!first)
                        {
                            sb.Append ("|");
                        }
                        sb.Append (GetSemanticTag (tag));
                        first = false;
                    }
                }
                else
                {
                    sb.Append ("-");
                }
                if (!same)
                {
                    sb.Append (",");
                    if (_endTags != null)
                    {
                        bool first = true;
                        foreach (Tag tag in _endTags)
                        {
                            if (!first)
                            {
                                sb.Append ("|");
                            }
                            sb.Append (GetSemanticTag (tag));
                            first = false;
                        }
                    }
                    else
                    {
                        sb.Append ("-");
                    }
                }
                sb.Append (")");
            }
            return sb.ToString ();
        }

#endif

        #endregion

        //*******************************************************************
        //
        // Internal Properties
        //
        //*******************************************************************

        #region internal Properties

        internal int SemanticTagCount
        {
            get
            {
                return _startTags == null ? 0 : _startTags.Count;
            }
        }

        internal State Start
        {
            set
            {
                if (value != _start)
                {
                    if (_start != null)
                    {
                        _start.OutArcs.Remove (this);
                    }
                    _start = value;
                    if (_start != null)
                    {
                        _start.OutArcs.Add (this);
                    }
                }
            }
            get
            {
                return _start;
            }
        }

        internal State End
        {
            set
            {
                // If no change, then do nothing
                if (value != _end)
                {
                    if (_end != null)
                    {
                        _end.InArcs.Remove (this);
                    }
                    _end = value;
                    if (_end != null)
                    {
                        _end.InArcs.Add (this);
                    }
                }
            }
            get
            {
                return _end;
            }
        }

        internal int WordId
        {
            get
            {
                return _iWord;
            }
        }

        internal Rule RuleRef
        {
            set
            {
                if ((_start != null && !_start.OutArcs.IsEmpty) || (_end != null && !_end.InArcs.IsEmpty))
                {
                    throw new InvalidOperationException ();
                }
                _ruleRef = value;
            }
            get
            {
                return _ruleRef;
            }
        }

        internal float Weight
        {
            set
            {
                _flWeight = value;
            }
            get
            {
                return _flWeight;
            }
        }

        internal int SpecialTransitionIndex
        {
            get
            {
                return _specialTransitionIndex;
            }
        }

#if DEBUG
        internal bool CheckingForExitPath
        {
            set
            {
                _fCheckingForExitPath = value;
            }
            get
            {
                return _fCheckingForExitPath;
            }
        }

        internal Backend Backend
        {
            set
            {
                _be = value;
            }
        }
#endif
        #endregion

        //*******************************************************************
        //
        // Private Methods
        //
        //*******************************************************************

        #region private Methods

#if DEBUG
        private string GetSemanticTag (Tag tag)
        {
            StringBuilder sb = new StringBuilder ();
            string value;
            string tagName = GetSemanticValue (tag._cfgTag, _be.Symbols, out value);
            if (tagName != "SemanticKey")
            {
                if (tagName != "=")
                {
                    sb.Append (tagName);
                    sb.Append ("=");
                }
                sb.Append (value);
            }
            else
            {
                sb.Append ("[");
                sb.Append (value);
                sb.Append ("]");
            }
            return sb.ToString ();
        }

        private static string GetSemanticValue (CfgSemanticTag tag, StringBlob symbols, out string value)
        {
            switch (tag.PropVariantType)
            {
                case VarEnum.VT_EMPTY:
                    value = tag._valueOffset > 0 ? symbols.FromOffset (tag._valueOffset) : tag._valueOffset.ToString (CultureInfo.InvariantCulture);
                    break;

                case VarEnum.VT_I4:
                case VarEnum.VT_UI4:
                    value = tag._varInt.ToString (CultureInfo.InvariantCulture);
                    break;

                case VarEnum.VT_R8:
                    value = tag._varDouble.ToString (CultureInfo.InvariantCulture);
                    break;

                case VarEnum.VT_BOOL:
                    value = tag._varInt == 0 ? "false" : "true";
                    break;

                default:
                    value = "Unknown property type";
                    break;
            }

            return tag._nameOffset > 0 ? symbols.FromOffset (tag._nameOffset) : tag._nameOffset.ToString (CultureInfo.InvariantCulture); ;
        }
#endif

        // Sort arcs in a state based on type, and then on index. 
        // Arcs loaded froma file have their index preserved where possible. New dynamic states have index == INFINITE,
        private int SortRank ()
        {
            int ret = 0;

            if (_ruleRef != null)
                ret = 0x1000000 + _ruleRef._cfgRule._nameOffset;      // It's a rule - Place 2nd in list

            if (_iWord != 0)
                ret += 0x2000000 + _iWord;// It's a word - Place last in list

            if (_specialTransitionIndex != 0)
                ret += 0x3000000; // It's a special transition (dictation, text buffer, or wildcard)

            return ret;                // It's an epsilon -- We're first
        }

        #endregion

        //*******************************************************************
        //
        // Private Fields
        //
        //*******************************************************************

        #region Private Fields

        // Transition start state
        private State _start;

        // Transition end state (or NULL for final state)
        private State _end;

        // Either word index or pRule but not both
        private int _iWord;

        // Rule ref
        private Rule _ruleRef;

        // If != 0 then transition to dictation, text buffer, or wildcard
        private int _specialTransitionIndex;

        private float _flWeight;

        // current matching mode
        private MatchMode _matchMode;

        private int _confidence;

        // Index of arc in table when serialized. Recreated when we reload grammar.

        private uint _iSerialize;

        // If non-null then has semantic tag associated with this
        private Collection<Tag> _startTags;
        private Collection<Tag> _endTags;

        private static uint _serializeToken = 1;

#if DEBUG
        // This is where the TransitionId comes from in engine interfaces.
        private bool _fCheckingForExitPath;
        private Backend _be;
#endif

        #endregion
    }

    //*******************************************************************
    //
    // Internal Types
    //
    //*******************************************************************

    #region private Methods

    internal enum Direction
    {
        Right,
        Left
    }
    #endregion

}
