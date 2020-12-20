// <copyright file="Variable.cs" company="Microsoft">
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
    [DebuggerDisplay ("{_value != null ? _value.ToString () : \"null\"}")]
#endif
    internal class Variable
    {
        //*******************************************************************
        //
        // Constructors
        //
        //*******************************************************************

        #region Constructors

        internal Variable (string name, int address, Data value, Variables properties, bool persist)
        {
            _name = name;
            _address = address;
            _value = value;
            _properties = properties;
            _persist = persist;
        }

        #endregion

        //*******************************************************************
        //
        // Internal Methods
        //
        //*******************************************************************

        #region Internal Methods

        internal Variable NewProperty (int address, string name, Data data)
        {
            return _properties.Add (address, name, data, _persist);
        }

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
                return _properties [name];
            }
        }

        internal string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        internal Data Data
        {
            get { return _value; }
            set { _value = value; }
        }
        internal int Address
        {
            get { return _address; }
            set { _address = value; }
        }

        internal Variables Properties
        {
            get { return _properties; }
        }

        internal bool Persist
        {
            get { return _persist; }
        }

        #endregion

        //*******************************************************************
        //
        // Private Fields
        //
        //*******************************************************************

        #region Private Fields

        private bool _persist;
        private Data _value;
        private Variables _properties;
        private int _address;
        private string _name;

        #endregion
    }
}
