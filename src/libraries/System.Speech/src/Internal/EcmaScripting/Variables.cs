// <copyright file="Variables.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace EcmaScripting
{
#if DEBUG
    [DebuggerDisplay ("Count #{Count}")]
    [DebuggerTypeProxy (typeof (VariablesDebugDisplay))]
#endif
    internal class Variables : IEnumerable<Variable>
    {
        //*******************************************************************
        //
        // Internal Methods
        //
        //*******************************************************************

        #region Internal Methods

        internal Variable Add (int address, string name, Data data, bool outVar)
        {
            Variable newVariable = this [name];
            if (newVariable != null)
            {
                throw new FormatException ("Duplicated Entry");
            }
            newVariable = new Variable (name, address, data, new Variables (), outVar);
            _variables.Add (newVariable);
            return newVariable;
        }

        internal void RenameOutAndRemoveNonePersist (string name, List<Data> memory)
        {
            for (int i = _variables.Count - 1; i >= 0; i--)
            {
                Variable variable = _variables [i];
                if (variable.Persist)
                {
                    string propName = variable.Name;
                    if (propName.IndexOf ("out") == 0)
                    {
                        _variables [i].Name = name;
                    }
                    else
                    {
                        _variables [i].Name = name + "." + propName;
                    }
                    UpdateContent (_variables [i], memory);
                }
                else
                {
                    _variables.RemoveAt (i);
                }
            }
        }

        #region IEnumerable implementation

        IEnumerator IEnumerable.GetEnumerator ()
        {
            for (int i = 0; i < _variables.Count; i++)
            {
                yield return _variables [i];
            }
        }

        IEnumerator<Variable> IEnumerable<Variable>.GetEnumerator ()
        {
            for (int i = 0; i < _variables.Count; i++)
            {
                yield return _variables [i];
            }
        }

        #region Debug Helpers

#if DEBUG

        internal bool Find (int address, out string name, out Variable var)
        {
            foreach (Variable variable in _variables)
            {
                if (variable.Address == address)
                {
                    name = variable.Name;
                    var = variable;
                    return true;
                }
                if (variable.Properties.Find (address, out name, out var))
                {
                    name = variable.Name + "." + name;
                    return true;
                }
            }
            name = null;
            var = null;
            return false;
        }
#endif
        #endregion

        #endregion

        #endregion

        //*******************************************************************
        //
        // Internal Properties
        //
        //*******************************************************************

        #region Internal Properties

        internal Variable this [string name]
        {
            get
            {
                foreach (Variable variable in _variables)
                {
                    if (variable.Name == name)
                    {
                        return variable;
                    }
                }
                return null;
            }
        }

        #endregion

        //*******************************************************************
        //
        // Private Methods
        //
        //*******************************************************************

        #region Private Methods

        private void UpdateContent (Variable variable, List<Data> memory)
        {
            Data data = memory [variable.Address];
            variable.Data = data;

            foreach (Variable property in variable.Properties)
            {
                UpdateContent (property, memory);
            }
        }

        #region Debug Helpers

#if DEBUG

        internal int Count
        {
            get
            {
                return _variables.Count;
            }
        }

        internal string Display ()
        {
            StringBuilder sb = new StringBuilder ();
            foreach (Variable variable in _variables)
            {
                sb.Append (variable.Name + ": " + variable.Data.Display ());
            }
            return sb.ToString ();
        }

        // Used by the debbugger display attribute
        private class VariablesDebugDisplay
        {
            internal VariablesDebugDisplay (Variables item)
            {
                _variables = item;
            }

            [DebuggerBrowsable (DebuggerBrowsableState.RootHidden)]
            internal Variable [] AKeys
            {
                get
                {
                    Variable [] variables = new Variable [_variables.Count];
                    int i = 0;
                    foreach (Variable var in _variables._variables)
                    {
                        variables [i++] = var;
                    }
                    return variables;
                }
            }

            private Variables _variables;
        }
#endif
        #endregion

        #endregion

        //*******************************************************************
        //
        // Private Fields
        //
        //*******************************************************************

        #region Private Fields

        private List<Variable> _variables = new List<Variable> ();

        #endregion
    }
}
