//------------------------------------------------------------------
// <copyright file="BackEnd.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// Description: 
//		CFG Grammar backend
//
// History:
//		5/1/2004	jeanfp		Created from the Sapi Managed code
//------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Speech.Internal.SrgsParser;

namespace System.Speech.Internal.SrgsCompiler
{
    // The State objects include pointers for 2 linked list.
    // StateElementList and StateElementList2 are 2 classes that wraps 
    // the 2 State list. 
    // Checks are made to ensure that the State pointers are never reused.

#if DEBUG
    [DebuggerDisplay ("Count = {Count}")]
    [DebuggerTypeProxy (typeof (StateElementListDebugDisplay))]
#endif
    internal class StateElementList : IEnumerable<State>
    {
        //*******************************************************************
        //
        // Internal Methods
        //
        //*******************************************************************

        #region Internal Methods

        internal void Add (State state)
        {
            state.Init ();
            if (_startState == null)
            {
                _curState = _startState = state;
            }
            else
            {
                _curState = _curState.Add (state);
            }
        }

        internal void Remove (State state)
        {
            if (state == _startState)
            {
                _startState = state.Next;
            }
            if (state == _curState)
            {
                _curState = state.Prev;
            }
            System.Diagnostics.Debug.Assert ((state.Next != null || state.Prev != null) || (_startState == null && _curState == null));

            state.Remove ();
        }

        IEnumerator IEnumerable.GetEnumerator ()
        {
            for (State item = _startState; item != null; item = item.Next)
            {
                yield return item;
            }
        }

        IEnumerator<State> IEnumerable<State>.GetEnumerator ()
        {
            for (State item = _startState; item != null; item = item.Next)
            {
                yield return item;
            }
        }

        /// <summary>
        /// Creates a new state handle in a given rule
        /// </summary>
        /// <param name="rule"></param>
        /// <returns></returns>
        internal State CreateNewState (Rule rule)
        {
            //CfgGrammar.TraceInformation ("BackEnd::CreateNewState");
            uint hNewState = CfgGrammar.NextHandle;

#if VIEW_STATS
            _cStates++;
#endif
            State newState = new State (rule, hNewState);
            Add (newState);
#if DEBUG
            rule._cStates++;
#endif
            return newState;
        }

        /// <summary>
        /// Deletet a state
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        internal void DeleteState (State state)
        {
#if VIEW_STATS
            _cStates--;
#endif
#if DEBUG
            state.Rule._cStates--;
#endif
            Remove (state);
        }

        /// <summary>
        /// Optimizes the grammar network by removing the epsilon states and merging
        /// duplicate transitions.  See GrammarOptimization.doc for details.
        /// </summary>
        internal void Optimize ()
        {
            foreach (State state in this)
            {
                NormalizeTransitionWeights (state);
            }

#if DEBUG
            // Remove redundant epsilon transitions.
            int cStates = Count;
            RemoveEpsilonStates ();
            if (Count != cStates)
            {
                System.Diagnostics.Trace.WriteLine ("Grammar compiler, additional Epsilons could have been removed :" + (cStates - Count).ToString ());
                //System.Diagnostics.Debug.Assert (_states.Count == cStates);
            }
            // Remove duplicate transitions.
            //DumpGrammarStatistics ("GrammarOptimization: EpsilonRemoval");
#endif
            MergeDuplicateTransitions ();


#if DEBUG
            // Remove redundant epsilon transitions again now that identical epsilon transitions have been removed.
            //DumpGrammarStatistics("GrammarOptimization: DuplicateRemoval");
            cStates = Count;
            RemoveEpsilonStates ();
            //System.Diagnostics.Debug.Assert (_states.Count == cStates);
            if (Count != cStates)
            {
                System.Diagnostics.Trace.WriteLine ("Grammar compiler, additional Epsilons could have been removed post merge transition :" + (cStates - Count).ToString ());
            }
#endif
        }

        /// <summary>
        /// Description:
        /// 	Change all transitions ending at SourceState to end at DestState, instead.
        /// 	Replace references to SourceState with references to DestState before deleting SourceState.
        /// 	- There may be additional duplicate input transitions at DestState after the move.
        /// 
        /// Assumptions:
        /// - SourceState == !null, RuleInitialState, !DestState,   ...
        /// - DestState   ==  null, RuleInitialState, !SourceState, ...
        /// - SourceState.OutputArc.IsEmpty
        /// - !(SourceState == RuleInitialState AND DestState == nul)
        /// 
        /// Algorithm:
        /// - For each input transition into SourceState
        ///   - Transition.EndState = DestState
        ///   - If DestState != null, DestState.InputArcs += Transition
        ///   - SourceState.InputArcs -= Transition
        /// - SourceState.InputArcs.Clear()
        /// - If SourceState == RuleInitialState, RuleInitialState = DestState
        /// - Delete SourceState
        /// </summary>
        /// <param name="srcState"></param>
        /// <param name="destState"></param>
        internal void MoveInputTransitionsAndDeleteState (State srcState, State destState)
        {
            System.Diagnostics.Debug.Assert (srcState != null);
            System.Diagnostics.Debug.Assert (srcState != destState);

            // For each input transition into SourceState, change EndState to DestState.
            List<Arc> arcs = srcState.InArcs.ToList ();
            foreach (Arc arc in arcs)
            {
                // Change EndState to DestState
                arc.End = destState;
            }

            //srcState.InArcs.RemoveRange (0,srcState.InArcs.Count);
            // Replace references to SourceState with references to DestState before deleting SourceState
            if (srcState.Rule._firstState == srcState) // Update RuleInitialState reference, if necessary
            {
                System.Diagnostics.Debug.Assert (destState != null);
                srcState.Rule._firstState = destState;
            }

            // Delete SourceState
            System.Diagnostics.Debug.Assert (srcState != null);
            //System.Diagnostics.Debug.Assert (srcState.InArcs.IsEmpty);
            System.Diagnostics.Debug.Assert (srcState.OutArcs.IsEmpty);
            DeleteState (srcState);  // Delete state from handle table
        }

        /// <summary>
        /// Description:
        /// 	Change all transitions starting at SourceState to start at DestState, instead.
        /// 	Deleting SourceState.
        /// 	- The weights on the transitions have been properly adjusted.
        /// 		The weights are not changed when moving transitions.
        /// 	- There may be additional duplicate input transitions at DestState after the move.
        /// 
        /// Assumptions:
        /// - SourceState == !null, !RuleInitialState, !DestState,   ...
        /// - DestState   == !null,  RuleInitialState, !SourceState, ...
        /// - SourceState.InputArc.IsEmpty
        /// 
        /// Algorithm:
        /// - For each output transition from SourceState
        ///   - Transition.StartState = DestState
        ///   - DestState.OutputArcs += Transition
        /// - Delete SourceState
        /// </summary>
        /// <param name="srcState"></param>
        /// <param name="destState"></param>
        internal void MoveOutputTransitionsAndDeleteState (State srcState, State destState)
        {
            System.Diagnostics.Debug.Assert (srcState != null);
            System.Diagnostics.Debug.Assert ((destState != null) && (destState != srcState));
            System.Diagnostics.Debug.Assert (srcState.InArcs.IsEmpty);

            // For each output transition from SourceState, change StartState to DestState.
            List<Arc> arcs = srcState.OutArcs.ToList ();
            foreach (Arc arc in arcs)
            {
                // Change StartState to DestState
                arc.Start = destState;
            }

            // Delete SourceState
            System.Diagnostics.Debug.Assert (srcState != null);
            System.Diagnostics.Debug.Assert (srcState.InArcs.IsEmpty);
            //System.Diagnostics.Debug.Assert (srcState.OutArcs.IsEmpty);
            DeleteState (srcState);  // Delete state from handle table
        }

        #endregion

        //*******************************************************************
        //
        // Internal Property
        //
        //*******************************************************************

        #region Internal Property

#if DEBUG
        internal State First
        {
            get
            {
                return _startState;
            }
        }

        internal int Count
        {
            get
            {
                int c = 0;
                for (State se = _startState; se != null; se = se.Next)
                {
                    c++;
                }
                return c;
            }
        }

#endif
        #endregion

        //*******************************************************************
        //
        // Private Methods
        //
        //*******************************************************************

        #region Private Methods

#if DEBUG
        /// <summary>
        ///   Description:
        ///		Removing epsilon states from the grammar network.  
        ///		See GrammarOptimization.doc for details.
        ///		- There may be additional duplicate transitions after removing epsilon transitions.
        ///
        ///   Algorithm:
        ///   - For each State in the graph,
        ///     - If the state has a single input epsilon transition and is not the rule initial state,
        /// 	- Move properties to the right, if necessary.
        /// 	- If EpsilonTransition does not have properties and is not referenced by other properties,
        ///   - Delete EpsilonTransition.
        ///   - Multiply weight of all transitions from State by EpsilonTransition.Weight.
        ///   - MoveOutputTransitionsAndDeleteState(State, EpsilonTransition.StartState)
        ///    - If the state has a single output epsilon transition,
        ///    - Move properties to the left, if necessary.
        ///   - If EpsilonTransition does not have properties and is not referenced by other properties,
        ///   - Delete EpsilonTransition.
        ///   - MoveInputTransitionsAndDeleteState(State, EpsilonTransition.EndState)
        /// 
        ///    Moving SemanticTag:
        ///    - InputEpsilonTransitions  can move its semantic tag ownerships/references to the right.
        ///    - OutputEpsilonTransitions can move its semantic tag ownerships/references to the left.
        /// </summary>
        private void RemoveEpsilonStates ()
        {
            // For each state in the grammar graph, remove excess input/output epsilon transitions.
            for (State state = First, nextState = null; state != null; state = nextState)
            {
                nextState = state.Next;
                if (state.InArcs.CountIsOne && state.InArcs.First.IsEpsilonTransition && (state != state.Rule._firstState))
                {
                    // State has a single input epsilon transition and is not the rule initial state.
                    Arc epsilonArc = state.InArcs.First;

                    // Attempt to move properties referencing EpsilonArc to the right.
                    // Optimization can only be applied when the epsilon arc is not referenced by any properties.
                    if (MoveSemanticTagRight (epsilonArc))
                    {
                        // Delete the input epsilon transition
                        State pEpsilonStartState = epsilonArc.Start;
                        float flEpsilonWeight = epsilonArc.Weight;

                        DeleteTransition (epsilonArc);

                        // Multiply weight of all transitions from state by EpsilonWeight.
                        foreach (Arc arc in state.OutArcs)
                        {
                            arc.Weight *= flEpsilonWeight;
                        }

                        // Move all output transitions from state to pEpsilonStartState and delete state if appropriate.
                        if (state != pEpsilonStartState)
                        {
                            MoveOutputTransitionsAndDeleteState (state, pEpsilonStartState);
                        }
                    }
                }
                // Optimize output epsilon transition, if possible
                else if ((state.OutArcs.CountIsOne) && state.OutArcs.First.IsEpsilonTransition && (state != state.Rule._firstState))
                {
                    // State has a single output epsilon transition
                    Arc epsilonArc = state.OutArcs.First;

                    // Attempt to move properties referencing EpsilonArc to the left.
                    // Optimization can only be applied when the epsilon arc is not referenced by any properties
                    // and when the arc does not connect RuleInitialState to null.
                    if (!((state == state.Rule._firstState) && (epsilonArc.End == null)) && MoveSemanticTagLeft (epsilonArc))
                    {
                        // Delete the output epsilon transition
                        State pEpsilonEndState = epsilonArc.End;

                        DeleteTransition (epsilonArc);

                        // Move all input transitions from state to pEpsilonEndState and delete state if appropriate.
                        if (state != pEpsilonEndState)
                        {
                            MoveInputTransitionsAndDeleteState (state, pEpsilonEndState);
                        }
                    }
                }
            }
        }
#endif
        /// <summary>
        /// Description:
        /// 	Remove duplicate transitions starting from the same state, or ending at the same state.
        /// 	See GrammarOptimization.doc for details.
        /// 
        /// Algorithm:
        /// - Add all states to ToDoList
        /// - For each state left in the ToDoList,
        ///   - Merge any duplicate output transitions.
        /// - Add all states to ToDoList in reverse order.
        /// - Remove duplicate transitions to null (special case since there is no state for FinalState)
        /// - For each state left in the ToDoList,
        ///   - Merge any duplicate input transitions.
        /// 
        /// Notes:
        /// - For best optimization, we need to move semantic properties referencing the transitions.
        /// </summary>
        private void MergeDuplicateTransitions ()
        {
            // Collection of states with potential transitions to merge
            Stack<State> mergeStates = new Stack<State> ();

            RecursiveMergeDuplicatedOutputTransition (mergeStates);

            // Merge duplicate transitions to null within each rule.
            // - Build collections of transitions to null
            ArcListIn finalStateArcs = new ArcListIn ();
            foreach (State state in this)
            {
                foreach (Arc arc in state.OutArcs)
                {
                    if (arc.End == null)
                    {
                        finalStateArcs.Add (arc);
                    }
                }
            }

            // Merge the duplicated transition to the end
            //MergeDuplicateInputTransitions (finalStateArcs, mergeStates);

            // For each state in the collection, merge any duplicate input transitions.
            while (mergeStates.Count > 0)
            {
                State state = mergeStates.Pop ();
                if (state.InArcs.ContainsMoreThanOneItem)
                {
                    MergeDuplicateInputTransitions (state.InArcs, mergeStates);
                }
            }

            RecursiveMergeDuplicatedInputTransition (mergeStates);
        }

        private void RecursiveMergeDuplicatedInputTransition (Stack<State> mergeStates)
        {
            // Build collection of states with potential duplicate input transitions to merge.
            foreach (State state in this)
            {
                if (state.InArcs.ContainsMoreThanOneItem)
                {
                    MergeDuplicateInputTransitions (state.InArcs, mergeStates);
                }
            }

            // For each state in the collection, merge any duplicate input transitions.
            while (mergeStates.Count > 0)
            {
                State state = mergeStates.Pop ();
                if (state.InArcs.ContainsMoreThanOneItem)
                {
                    MergeDuplicateInputTransitions (state.InArcs, mergeStates);
                }
            }
        }

        private void RecursiveMergeDuplicatedOutputTransition (Stack<State> mergeStates)
        {
            // Build collection of states with potential duplicate output transitions to merge.
            foreach (State state in this)
            {
                if (state.OutArcs.ContainsMoreThanOneItem)
                {
                    MergeDuplicateOutputTransitions (state.OutArcs, mergeStates);
                }
            }

            // For each state in the collection, merge any duplicate output transitions.
            while (mergeStates.Count > 0)
            {
                State state = mergeStates.Pop ();
                if (state.OutArcs.ContainsMoreThanOneItem)
                {
                    MergeDuplicateOutputTransitions (state.OutArcs, mergeStates);
                }
            }
        }

        /// <summary>
        /// Description:
        ///		Sort and iterate through the input arcs and remove duplicate input transitions.
        ///		See GrammarOptimization.doc for details.
        /// 
        /// Algorithm:
        ///   - MergeIdenticalTransitions(Arcs)
        ///   - Sort the input transitions from the state (by content and # output arcs from start state)
        ///   - For each set of transitions with identical content and StartState.OutputArcs.Count() == 1
        ///			- Move semantic properties to the left, if necessary.
        ///			- Label the first property-less transition as CommonArc
        ///			- For each successive property-less transition (DuplicateArc)
        ///			  - Delete DuplicateArc
        ///			  - MoveInputTransitionsAndDeleteState(DuplicateArc.StartState, CommonArc.StartState)
        ///			- Add CommonArc.StartState to ToDoList if not there already.
        /// 
        ///  Moving SemanticTag:
        ///  - Duplicate input transitions can move its semantic tag ownerships/references to the left.
        /// </summary>
        /// <param name="arcs">Collection of input transitions to collapse</param>
        /// <param name="mergeStates">Collection of states with potential transitions to merge</param>
        private void MergeDuplicateInputTransitions (ArcList arcs, Stack<State> mergeStates)
        {
            Collection<Arc> arcsToMerge = null;

            // Merge identical transitions in arcs 
            MergeIdenticalTransitions (arcs);

            // Reference Arc
            Arc refArc = null;
            bool refSet = false;

            // Build a list of possible arcs to Merge
            foreach (Arc arc in arcs)
            {
                // Skip transitions whose end state has other incoming transitions or if the end state has more than one incoming transition
                bool skipTransition = arc.Start == null || !arc.Start.OutArcs.CountIsOne;
                // Find next set of duplicate output transitions (potentially with properties).
                if (refArc != null && Arc.CompareContent (arc, refArc) == 0)
                {
                    if (!skipTransition)
                    {
                        // Lazy init as entering this loop is a rare event
                        if (arcsToMerge == null)
                        {
                            arcsToMerge = new Collection<Arc> ();
                        }
                        // Add the first element
                        if (!refSet)
                        {
                            arcsToMerge.Add (refArc);
                            refSet = true;
                        }
                        arcsToMerge.Add (arc);
                    }
                }
                else
                {
                    // New word, reset everything
                    refArc = skipTransition ? null : arc;
                    refSet = false;
                }
            }

            // Combine the arcs if possible
            if (arcsToMerge != null)
            {
                refArc = null;
                Arc commonArc = null;                   // Common property-less transition to merge into
                State commonStartState = null;
                bool fCommonStartStateChanged = false;      // Did CommonStartState change and need re-optimization?

                foreach (Arc arc in arcsToMerge)
                {
                    if (refArc == null || Arc.CompareContent (arc, refArc) != 0)
                    {
                        // Purge the last operations and reset all the local
                        refArc = arc;

                        // If CommonStartState changed, renormalize weights and add it to MergeStates for reoptimization.
                        if (fCommonStartStateChanged)
                        {
                            AddToMergeStateList (mergeStates, commonStartState);
                        }

                        // Reset the arcs
                        commonArc = null;
                        commonStartState = null;
                        fCommonStartStateChanged = false;
                    }

                    // For each property-less duplicate transition
                    Arc duplicatedArc = arc;
                    State duplicatedStartState = duplicatedArc.Start;

                    // Attempt to move properties referencing duplicate arc to the right.
                    // Optimization can only be applied when the duplicate arc is not referenced by any properties
                    // and the duplicate end state is not the RuleOutitalState.
                    if (MoveSemanticTagLeft (duplicatedArc))
                    {
                        // duplicatedArc != commonArc
                        if (commonArc != null)
                        {
                            if (!fCommonStartStateChanged)
                            {
                                // Processing first duplicate arc.
                                // Multiply the weights of transitions from CommonStartState by CommonArc.Weight.
                                foreach (Arc arcOut in commonStartState.OutArcs)
                                {
                                    arcOut.Weight *= commonArc.Weight;
                                }

                                fCommonStartStateChanged = true;  // Output transitions of CommonStartState changed.
                            }

                            // Multiply the weights of transitions from DuplicateStartState by DuplicateArc.Weight.
                            foreach (Arc arcOut in duplicatedStartState.OutArcs)
                            {
                                arcOut.Weight *= duplicatedArc.Weight;
                            }

                            duplicatedArc.Weight += commonArc.Weight;// Merge duplicate arc weight with common arc
                            Arc.CopyTags (commonArc, duplicatedArc, Direction.Left);
                            DeleteTransition (commonArc);    // Delete successive duplicate transitions

                            // Move outputs of duplicate state to common state; Delete duplicate state
                            MoveInputTransitionsAndDeleteState (commonStartState, duplicatedStartState);
                        }

                        // Label first property-less transition as CommonArc
                        commonArc = duplicatedArc;
                        commonStartState = duplicatedStartState;
                    }
                }
                // If CommonStartState changed, renormalize weights and add it to MergeStates for reoptimization.
                if (fCommonStartStateChanged)
                {
                    AddToMergeStateList (mergeStates, commonStartState);
                }
            }
        }

        /// <summary>
        /// Description:
        /// 	Sort and iterate through the output arcs and remove duplicate output transitions.
        /// 	See GrammarOptimization.doc for details.
        /// 
        /// Algorithm:
        ///   - MergeIdenticalTransitions(Arcs)
        ///   - Sort the output transitions from the state (by content and # input arcs from end state)
        ///   - For each set of transitions with identical content, EndState != null, and EndState.InputArcs.Count() == 1
        /// 	- Move semantic properties to the right, if necessary.
        /// 	- Label the first property-less transition as CommonArc
        /// 	- For each property-less transition (DuplicateArc) including CommonArc
        /// 	  - Multiply the weights of output transitions from DuplicateArc.EndState by DuplicateArc.Weight.
        /// 	  - If DuplicateArc != CommonArc
        /// 	    - CommonArc.Weight += DuplicateArc.Weight
        /// 	    - Delete DuplicateArc
        /// 	    - MoveOutputTransitionsAndDeleteState(DuplicateArc.EndState, CommonArc.EndState)
        /// 	- Normalize weights of output transitions from CommonArc.EndState.
        /// 	- Add CommonArc.EndtState to ToDoList if not there already.
        /// 
        /// Moving SemanticTag:
        /// - Duplicate output transitions can move its semantic tag ownerships/references to the right.
        /// </summary>
        /// <param name="arcs">Collection of output transitions to collapse</param>
        /// <param name="mergeStates">Collection of states with potential transitions to merge</param>
        private void MergeDuplicateOutputTransitions (ArcList arcs, Stack<State> mergeStates)
        {
            Collection<Arc> arcsToMerge = null;

            // Merge identical transitions in arcs 
            MergeIdenticalTransitions (arcs);

            // Reference Arc
            Arc refArc = null;
            bool refSet = false;

            // Build a list of possible arcs to Merge
            foreach (Arc arc in arcs)
            {
                // Skip transitions whose end state has other incoming transitions or if the end state has more than one incoming transition
                bool skipTransition = arc.End == null || !arc.End.InArcs.CountIsOne;
                // Find next set of duplicate output transitions (potentially with properties).
                if (refArc != null && Arc.CompareContent (arc, refArc) == 0)
                {
                    if (skipTransition)
                    {
                        // Lazy init as entering this loop is a rare event
                        if (arcsToMerge == null)
                        {
                            arcsToMerge = new Collection<Arc> ();
                        }
                        // Add the first element
                        if (!refSet)
                        {
                            arcsToMerge.Add (refArc);
                            refSet = true;
                        }
                        arcsToMerge.Add (arc);
                    }
                }
                else
                {
                    // New word, reset everything
                    refArc = skipTransition ? null : arc;
                    refSet = false;
                }
            }

            // Combine the arcs if possible
            if (arcsToMerge != null)
            {
                refArc = null;
                Arc commonArc = null;                   // Common property-less transition to merge into
                State commonEndState = null;
                bool fCommonEndStateChanged = false;      // Did CommonEndState change and need re-optimization?

                foreach (Arc arc in arcsToMerge)
                {
                    if (refArc == null || Arc.CompareContent (arc, refArc) != 0)
                    {
                        // Purge the last operations and reset all the local
                        refArc = arc;

                        // If CommonEndState changed, renormalize weights and add it to MergeStates for reoptimization.
                        if (fCommonEndStateChanged)
                        {
                            AddToMergeStateList (mergeStates, commonEndState);
                        }

                        // Reset the arcs
                        commonArc = null;
                        commonEndState = null;
                        fCommonEndStateChanged = false;
                    }

                    // For each property-less duplicate transition
                    Arc duplicatedArc = arc;
                    State duplicatedEndState = duplicatedArc.End;

                    // Attempt to move properties referencing duplicate arc to the right.
                    // Optimization can only be applied when the duplicate arc is not referenced by any properties
                    // and the duplicate end state is not the RuleInitalState.
                    if ((duplicatedEndState != duplicatedEndState.Rule._firstState) && MoveSemanticTagRight (duplicatedArc))
                    {
                        // duplicatedArc != commonArc
                        if (commonArc != null)
                        {
                            if (!fCommonEndStateChanged)
                            {
                                // Processing first duplicate arc.
                                // Multiply the weights of transitions from CommonEndState by CommonArc.Weight.
                                foreach (Arc arcOut in commonEndState.OutArcs)
                                {
                                    arcOut.Weight *= commonArc.Weight;
                                }

                                fCommonEndStateChanged = true;  // Output transitions of CommonEndState changed.
                            }

                            // Multiply the weights of transitions from DuplicateEndState by DuplicateArc.Weight.
                            foreach (Arc arcOut in duplicatedEndState.OutArcs)
                            {
                                arcOut.Weight *= duplicatedArc.Weight;
                            }

                            duplicatedArc.Weight += commonArc.Weight;// Merge duplicate arc weight with common arc
                            Arc.CopyTags (commonArc, duplicatedArc, Direction.Right);
                            DeleteTransition (commonArc);    // Delete successive duplicate transitions

                            // Move outputs of duplicate state to common state; Delete duplicate state
                            MoveOutputTransitionsAndDeleteState (commonEndState, duplicatedEndState);
                        }

                        // Label first property-less transition as CommonArc
                        commonArc = duplicatedArc;
                        commonEndState = duplicatedEndState;
                    }
                }
                // If CommonEndState changed, renormalize weights and add it to MergeStates for reoptimization.
                if (fCommonEndStateChanged)
                {
                    AddToMergeStateList (mergeStates, commonEndState);
                }
            }
        }

        private static void AddToMergeStateList (Stack<State> mergeStates, State commonEndState)
        {
            NormalizeTransitionWeights (commonEndState);
            if (!mergeStates.Contains (commonEndState))
            {
                mergeStates.Push (commonEndState);
            }
        }

        /// <summary>
        /// Move any semantic tag ownership and optionally references to a unique 
        /// previous arc, if possible.
        /// 
        /// MoveReferences = true:  Return if arc is propertyless after the move.
        /// MoveReferences = false: Return if arc does not own semantic tag after the move.
        ///                         The arc can still be referenced by other semantic tags.
        /// </summary>
        /// <param name="arc"></param>
        /// <returns></returns>
        internal static bool MoveSemanticTagLeft (Arc arc)
        {
            // ToDo: Temporarily force semantic tag references to always move with tag.  See 37583.
            //       This changes the range of words spanned by the tag, which is a bug for SAPI grammars.
            State startState = arc.Start;

            // Can only move ownership/references if there is an unique input and output arc from the start state.
            // Cannot concatenate semantic tags.  (SemanticInterpretation script can arguably be concatenated.)
            // Cannot move ownership across RuleRef (to maintain semantics of $$ in SemanticTag JScript).
            // Cannot move semantic tag to special transition.  (SREngine may return multiple result arcs for the transition.)
            Arc previousArc = startState.InArcs.First;
            if ((startState.InArcs.CountIsOne) && (startState.OutArcs.CountIsOne) && CanTagsBeMoved (previousArc, arc))
            {
                // Move semantic tag ownership to the previous arc.
                Arc.CopyTags (arc, previousArc, Direction.Left);

                // Semantic tag and optionally references have been moved successfully.
                return true;
            }

            return arc.IsPropertylessTransition;
        }

        /// <summary>
        /// Move any semantic tag ownership and optionally references to a unique 
        /// next arc, if possible.
        /// 
        /// MoveReferences = true:  Return if arc is propertyless after the move.
        /// MoveReferences = false: Return if arc does not own semantic tag after the move.  
        ///                         The arc can still be referenced by other semantic tags.
        /// 
        /// Force semantic tag references to always move with tag.  See 37583.
        ///      This changes the range of words spanned by the tag, which is a bug for SAPI grammars.
        /// </summary>
        /// <param name="arc"></param>
        /// <returns></returns>
        internal static bool MoveSemanticTagRight (Arc arc)
        {
            System.Diagnostics.Debug.Assert (arc.End != null);

            State endState = arc.End;

            // Can only move ownership/references if there is an unique input and output arc from the end state.
            // Cannot concatenate semantic tags.  (SemanticInterpretation script can arguably be concatenated.)
            // Cannot move ownership across RuleRef (to maintain semantics of $$ in SemanticTag JScript).
            // Cannot move semantic tag to special transition.  (SREngine may return multiple result arcs for the transition.)
            Arc pNextArc = endState.OutArcs.First;
            if ((endState.InArcs.CountIsOne) && (endState.OutArcs.CountIsOne) && CanTagsBeMoved (arc, pNextArc))
            {
                // Move semantic tag ownership to the next arc.
                Arc.CopyTags (arc, pNextArc, Direction.Right);

                // Semantic tag and optionally references have been moved successfully.
                return true;
            }

            return arc.IsPropertylessTransition;
        }

        /// <summary>
        /// Check if tags can be moved from a source arc to a destination
        ///     - Semantic interpretation. Tags cannot be moved if they would end up over a rule ref.
        ///     - Sapi properties. Tag can be put anywhere. 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        internal static bool CanTagsBeMoved (Arc start, Arc end)
        {
            return (start.RuleRef == null) && (end.RuleRef == null) && (end.SpecialTransitionIndex == 0);
        }

        /// <summary>
        /// Description:
        ///		Detach and delete the specified transition from the graph.
        ///		Relocate or delete referencing semantic tags before deleting the transition.
        /// 
        /// Special Case:
        ///		Arc.EndState == null
        ///		Arc.Optional == true
        /// </summary>
        /// <param name="arc"></param>
        private static void DeleteTransition (Arc arc)
        {
            // Arc cannot own SemanticTag
            System.Diagnostics.Debug.Assert (arc.SemanticTagCount == 0);

            // Arc cannot be referenced by SemanticTags
            System.Diagnostics.Debug.Assert (arc.IsPropertylessTransition);

            // Detach arc from start and end state
            arc.AttachStates (null, null);
        }

        /// <summary>
        /// Description:
        ///    Merge identical transitions with identical content, StartState, and EndState.
        /// 
        /// Algorithm:
        /// - LastArc = Arcs[0]
        /// - For each Arc in Arcs[1-], 
        ///   - If Arc is identical to LastArc,
        ///   - LastArc.Weight += Arc.Weight
        ///   - Delete Arc
        ///   - Else LastArc = Arc
        /// 
        /// Moving SemanticTag:
        /// - Identical transitions have identical semantic tags.  Currently impossible to have identical 
        /// non-null tags.
        /// - MoveSemanticTagReferences(DuplicateArc, CommonArc)
        /// </summary>
        /// <param name="arcs"></param>
        private static void MergeIdenticalTransitions (ArcList arcs)
        {
            // // Need at least two transitions to merge.
            if (arcs.ContainsMoreThanOneItem)                                // Need at least two transitions to merge.
            {
                Arc refArc = null;
                Collection<Arc> arcsToDelete = null;
                foreach (Arc arc in arcs)
                {
                    if (refArc != null && Arc.CompareIdenticalTransitions (refArc, arc))
                    {
                        // Identical transition
                        arc.Weight += refArc.Weight;
                        refArc.ClearTags ();
                        if (arcsToDelete == null)
                        {
                            // delay the creation of the collection as this operation in unfrequent.
                            arcsToDelete = new Collection<Arc> ();
                        }
                        arcsToDelete.Add (refArc);
                    }
                    refArc = arc;
                }
                if (arcsToDelete != null)
                {
                    foreach (Arc arc in arcsToDelete)
                    {
                        // arc will become an orphan
                        DeleteTransition (arc);
                    }
                }
            }
        }

        /// <summary>
        /// Normalize the weights of output transitions from this state.
        /// See GrammarOptimization.doc for details.
        /// </summary>
        /// <param name="state"></param>
        private static void NormalizeTransitionWeights (State state)
        {
            float flSumWeights = 0.0f;

            // Compute the sum of the weights.
            foreach (Arc arc in state.OutArcs)
            {
                flSumWeights += arc.Weight;
            }

            // If Sum != 0 or 1, normalize transition weights by 1/Sum.
            if (!flSumWeights.Equals (0.0f) && !flSumWeights.Equals (1.0f))
            {
                float flNormalizationFactor = 1.0f / flSumWeights;

                foreach (Arc arc in state.OutArcs)
                {
                    arc.Weight *= flNormalizationFactor;
                }
            }
        }

        #endregion

        //*******************************************************************
        //
        // Private Types
        //
        //*******************************************************************

        #region Private Types

#if DEBUG
        // Used by the debbugger display attribute
        internal class StateElementListDebugDisplay
        {
            public StateElementListDebugDisplay (StateElementList states)
            {
                _states = states;
            }

            [DebuggerBrowsable (DebuggerBrowsableState.RootHidden)]
            public State [] AKeys
            {
                get
                {
                    State [] states = new State [_states.Count];
                    int i = 0;
                    foreach (State state in _states)
                    {
                        states [i++] = state;
                    }
                    return states;
                }
            }

            private StateElementList _states;
        }
#endif

        #endregion

        //*******************************************************************
        //
        // Private Fields
        //
        //*******************************************************************

        #region Private Fields

        private State _startState;
        private State _curState;

#if     VIEW_STATS
        static internal int _cStates = 0;
        static internal int _cArcs = 0;
#endif

        #endregion

    }
}
