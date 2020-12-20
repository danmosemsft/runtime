//------------------------------------------------------------------
// <copyright file="VM.cs" company="Microsoft">
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
    internal class VM
    {
        //*******************************************************************
        //
        // Constructors
        //
        //*******************************************************************

        #region Constructors

        /// <summary>
        /// The VM may be initialize with a list of existing variables
        /// </summary>
        /// <param name="variables"></param>
        internal VM (Variables variables)
        {
            _variables = variables;

            // Allocate the space for all the variables
            Allocate (variables);
        }

        #endregion

        //*******************************************************************
        //
        // Internal Methods
        //
        //*******************************************************************

        #region Internal Methods

        internal int NewConstant (Data data)
        {
            int address = _memory.Count;
            _memory.Add (data);
            return address;
        }

        internal Variable NewVar (string name, Data data, bool outVar)
        {
            int address = _memory.Count;
            _memory.Add (data);
            return _variables.Add (address, name, data, outVar);
        }

        internal Variable NewProperty (Variable var, string name, Data data)
        {
            int address = _memory.Count;
            _memory.Add (data);
            return var.NewProperty (address, name, data);
        }

        internal int GenCode (Opcode opcode)
        {
            return GenCode (opcode, -1);
        }

        internal int GenCode (Opcode opcode, int operand)
        {
            _instructions.Add (new Instruction (opcode, operand));
            return _instructions.Count - 1;
        }

        internal int CurrentAddress ()
        {
            return _instructions.Count;
        }

        internal void Fixup (int address)
        {
            int curAddress = _instructions.Count;
            _instructions [address] = new Instruction (_instructions [address]._opcode, curAddress);
        }

        internal int Execute (int ip)
        {
            bool go = true;

            while (go)
            {
                Instruction instruction = _instructions [ip++];
                Opcode opcode = instruction._opcode;
                int address = instruction._operand;
                Data data;
                switch (opcode)
                {
                    case Opcode.Push:
                        data = _memory [address];
                        _stack.Push (data.Clone ());
                        break;

                    case Opcode.Pop:
                        _stack.Pop ();
                        break;

                    case Opcode.Jz:
                        if ((_stack.Pop ().IsFalse ()))
                        {
                            ip = address;
                        }
                        break;

                    case Opcode.Jmp:
                        ip = address;
                        break;

                    case Opcode.Load:
                        _memory [address] = _stack.Peek ();
                        break;

                    case Opcode.Inc:
                        _memory [address].Sum (_one);
                        break;

                    case Opcode.Dec:
                        _memory [address].Sum (_minusOne);
                        break;

                    case Opcode.Add:
                    case Opcode.Sub:
                    case Opcode.Mul:
                    case Opcode.Div:
                    case Opcode.Mod:
                    case Opcode.ShiftL:
                    case Opcode.ShiftR:
                    case Opcode.Equ:
                    case Opcode.Nequ:
                    case Opcode.Greater:
                    case Opcode.GreaterEq:
                    case Opcode.Smaller:
                    case Opcode.SmallerEq:
                    case Opcode.LAnd:
                    case Opcode.Lor:
                    case Opcode.And:
                    case Opcode.Or:
                    case Opcode.Xor:
                        Eval (opcode);
                        break;

                    case Opcode.Ret:
                        go = false;
                        break;

                    default:
                        throw new NotImplementedException ();
                }
            }
            System.Diagnostics.Debug.Assert (_stack.Count == 0);
            return 0;
        }

        internal Variable this [string name]
        {
            get
            {
                Variable variable = _variables [name];
                if (variable == null)
                {
                    throw new FormatException ("Unknown variable: " + name);
                }
                return variable;
            }
        }

        #region Debug Helpers

#if DEBUG

        internal string DisplayMemory (bool all)
        {
            StringBuilder sb = new StringBuilder ();

            for (int i = 0; i < _memory.Count; i++)
            {
                Data data = _memory [i];
                Variable variable;
                string name;
                bool isVariable = _variables.Find (i, out name, out variable);
                if (isVariable || all)
                {
                    if (all)
                    {
                        sb.Append (string.Format (CultureInfo.InvariantCulture, "{0,-4}    ", i + ":"));
                    }

                    sb.Append (string.Format (CultureInfo.InvariantCulture, "{0,-8}", isVariable ? name : ""));

                    if (data != null)
                    {
                        sb.Append (string.Format (CultureInfo.InvariantCulture, "{0} {1}{2}{3}", data._data != null ? data._data.ToString () : "", '{', data._type.ToString (), '}'));
                    }
                    sb.Append ("\n");
                }
            }

            return sb.ToString ();
        }

        internal string DisplayDisassembly ()
        {
            // Start with the memory allocation
            StringBuilder sb = new StringBuilder (DisplayMemory (true));
            sb.Append ("\n");

            // Let the VM generate the code
            for (int i = 0; i < _instructions.Count; i++)
            {
                Instruction instruction = _instructions [i];
                Opcode opcode = instruction._opcode;
                int operand = instruction._operand;
                string name;
                Variable var;

                sb.Append (string.Format (CultureInfo.InvariantCulture, "{0,-4}    {1,-6}", i + ":", opcode));
                switch (opcode)
                {
                    case Opcode.Load:
                        sb.Append (operand);
                        _variables.Find (operand, out name, out var);
                        sb.Append ("  " + name);
                        break;

                    case Opcode.Push:
                        sb.Append (operand);
                        _variables.Find (operand, out name, out var);
                        if (name != null)
                        {
                            sb.Append ("  " + name);
                        }
                        sb.Append ("  " + _memory [operand]);
                        break;

                    case Opcode.Jz:
                    case Opcode.Jmp:
                        sb.Append (operand);
                        break;
                }
                sb.Append ("\n");
            }
            return sb.ToString ();
        }

        internal string DisplayResults ()
        {
            return DisplayMemory (false);
        }

#endif
        #endregion

        #endregion

        //*******************************************************************
        //
        // Private Methods
        //
        //*******************************************************************

        #region Private Methods

        /// <summary>
        /// Copy into the VM a list of variables
        /// </summary>
        /// <param name="variables"></param>
        private void Allocate (Variables variables)
        {
            foreach (Variable variable in variables)
            {
                int address = _memory.Count;
                _memory.Add (variable.Data);
                variable.Address = address;

                Allocate (variable.Properties);
            }
        }

        private void Eval (Opcode code)
        {
            Data op2 = _stack.Pop ();
            Data op1 = _stack.Pop ();
            switch (code)
            {
                case Opcode.Add:
                    op1.Sum (op2);
                    break;

                case Opcode.Sub:
                    op1.Minus (op2);
                    break;

                case Opcode.Mul:
                    op1.Multiply (op2);
                    break;

                case Opcode.Div:
                    op1.Divide (op2);
                    break;

                case Opcode.Mod:
                    op1.Modulo (op2);
                    break;

                case Opcode.ShiftR:
                    op1.ShiftRight (op2);
                    break;

                case Opcode.ShiftL:
                    op1.ShiftLeft (op2);
                    break;

                case Opcode.Equ:
                    op1.Equal (op2, true);
                    break;

                case Opcode.Nequ:
                    op1.Equal (op2, false);
                    break;

                case Opcode.Greater:
                    op1.Greater (op2);
                    break;

                case Opcode.GreaterEq:
                    op1.GreaterOrEqual (op2);
                    break;

                case Opcode.Smaller:
                    op1.Smaller (op2);
                    break;

                case Opcode.SmallerEq:
                    op1.SmallerOrEqual (op2);
                    break;

                case Opcode.LAnd:
                    op1.LogicalAnd (op2);
                    break;

                case Opcode.Lor:
                    op1.LogicalOr (op2);
                    break;

                case Opcode.And:
                    op1.And (op2);
                    break;

                case Opcode.Or:
                    op1.Or (op2);
                    break;

                case Opcode.Xor:
                    op1.Xor (op2);
                    break;

                default:
                    throw new NotImplementedException ();
            }

            _stack.Push (op1);
        }

        #endregion

        //*******************************************************************
        //
        // Internal Properties
        //
        //*******************************************************************

        #region Internal Properties

        internal List<Data> Memory
        {
            get
            {
                return _memory;
            }
        }

        #endregion

        //*******************************************************************
        //
        // Internal Types
        //
        //*******************************************************************

        #region Internal Types

        internal enum Opcode
        {
            Load,
            Push,
            Pop,
            Inc,
            Dec,
            Add,
            Sub,
            Mul,
            Div,
            Mod,
            ShiftR,
            ShiftL,
            Equ,
            Nequ,
            Greater,
            Smaller,
            GreaterEq,
            SmallerEq,
            LAnd,
            Lor,
            And,
            Or,
            Xor,
            Jmp,
            Jz,
            Ret
        }

        #endregion

        //*******************************************************************
        //
        // Private Methods
        //
        //*******************************************************************

        #region Private Methods

#if DEBUG
        [DebuggerDisplay ("{_opcode.ToString ()} {_operand.ToString ()}")]
#endif
        struct Instruction
        {
            internal Instruction (Opcode opcode, int operand)
            {
                _opcode = opcode;
                _operand = operand;
            }

            internal Opcode _opcode;
            internal int _operand;
        }

        #endregion

        //*******************************************************************
        //
        // Private Fields
        //
        //*******************************************************************

        #region Private Fields

        private Variables _variables;
        private List<Instruction> _instructions = new List<Instruction> (100);
        private List<Data> _memory = new List<Data> (100);
        private Stack<Data> _stack = new Stack<Data> (100);
        private readonly Data _one = new Data (VarType.Number, 1.0);
        private readonly Data _minusOne = new Data (VarType.Number, -1.0);

        #endregion
    }
}
