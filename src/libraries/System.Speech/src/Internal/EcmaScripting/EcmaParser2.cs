using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Tools;

namespace EcmaScripting
{
    public partial class EcmaParser : Parser
    {
        internal void Init (VM vm)
        {
            _vm = vm;
        }

        internal int Execute ()
        {
            return _vm.Execute (0);
        }

        internal int PushConstant (Data data)
        {
            int address = _vm.NewConstant (data);
            _vm.GenCode (VM.Opcode.Push, address);
            return address;
        }

        internal int PushConstant (int i)
        {
            return PushConstant (new Data (VarType.Number, (double) i));
        }

        internal int PushValue (Variable var)
        {
            _vm.GenCode (VM.Opcode.Push, var.Address);
            return var.Address;
        }

        internal int JumpZero ()
        {
            return _vm.GenCode (VM.Opcode.Jz);
        }

        internal int CurrentAddress ()
        {
            return _vm.CurrentAddress ();
        }

        internal int Jmp ()
        {
            return _vm.GenCode (VM.Opcode.Jmp);
        }

        internal void Fixup (int address)
        {
            _vm.Fixup (address);
        }

        internal void FixupWhile (int addressJz, int addressCondition)
        {
            _vm.GenCode (VM.Opcode.Jmp, addressCondition);
            _vm.Fixup (addressJz);
        }

        internal void Pop ()
        {
            _vm.GenCode (VM.Opcode.Pop);
        }

        internal void EndFunction ()
        {
            _vm.GenCode (VM.Opcode.Ret);
        }
        /// <summary>
        /// New and Assign
        /// </summary>
        /// <param name="name"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        internal Data NewVar (string name, bool hasInit)
        {
            Data data = null;
            if (!hasInit)
            {
                // Add the initialisation code
                data = new Data (VarType.Null, null);
                PushConstant (data);
            }

            Variable newVar = _vm.NewVar (name, data, false);

            _vm.GenCode (VM.Opcode.Load, newVar.Address);
            return newVar.Data;
        }

        internal Variable GetVariable (string name)
        {
            return _vm [name];
        }

        internal Variable GetProperty (Variable var, string name)
        {
            Variable newVar = var [name];
            if (newVar == null)
            {
                newVar = _vm.NewProperty (var, name, new Data (VarType.Null, null));
            }
            return newVar;
        }

        internal void Assign (Variable variable, AssignmentType type)
        {
            if (type != AssignmentType.Equal)
            {
                _vm.GenCode (VM.Opcode.Push, variable.Address);
                switch (type)
                {
                    case AssignmentType.And:
                        _vm.GenCode (VM.Opcode.And);
                        break;

                    case AssignmentType.Div:
                        _vm.GenCode (VM.Opcode.Div);
                        break;

                    case AssignmentType.Ls:
                        _vm.GenCode (VM.Opcode.ShiftL);
                        break;

                    case AssignmentType.Minus:
                        _vm.GenCode (VM.Opcode.Sub);
                        break;

                    case AssignmentType.Mod:
                        _vm.GenCode (VM.Opcode.Mod);
                        break;

                    case AssignmentType.Mul:
                        _vm.GenCode (VM.Opcode.Mul);
                        break;

                    case AssignmentType.Or:
                        _vm.GenCode (VM.Opcode.Or);
                        break;

                    case AssignmentType.Plus:
                        _vm.GenCode (VM.Opcode.Add);
                        break;

                    case AssignmentType.Rs:
                        _vm.GenCode (VM.Opcode.ShiftR);
                        break;

                    case AssignmentType.Xor:
                        _vm.GenCode (VM.Opcode.Xor);
                        break;

                    default:
                        throw new NotImplementedException ();
                }
            }
            _vm.GenCode (VM.Opcode.Load, variable.Address);
        }

        internal void PostInc(int address)
        {
            if (address < 0)
            {
                throw new FormatException ();
            }
            _vm.GenCode (VM.Opcode.Inc, address);
        }

        internal void PostDec (int address)
        {
            if (address < 0)
            {
                throw new FormatException ();
            }
            _vm.GenCode (VM.Opcode.Dec, address);
        }

        internal void PreInc (int address)
        {
            if (address < 0)
            {
                throw new FormatException ();
            }

            // Update the stack value
            PushConstant (1);
            _vm.GenCode (VM.Opcode.Add);

            // Udpate the memory value
            _vm.GenCode (VM.Opcode.Inc, address);
        }

        internal void PreDec (int address)
        {
            if (address < 0)
            {
                throw new FormatException ();
            }

            // Update the stack value
            PushConstant (-1);
            _vm.GenCode (VM.Opcode.Add);

            // Udpate the memory value
            _vm.GenCode (VM.Opcode.Inc, address);
        }

        internal void Add ()
        {
            _vm.GenCode (VM.Opcode.Add);
        }

        internal void Substract ()
        {
            _vm.GenCode (VM.Opcode.Sub);
        }
        internal void Multiply ()
        {
            _vm.GenCode (VM.Opcode.Mul);
        }
        internal void Divide ()
        {
            _vm.GenCode (VM.Opcode.Div);
        }
        internal void Mod ()
        {
            _vm.GenCode (VM.Opcode.Mod);
        }
        internal void ShiftRight ()
        {
            _vm.GenCode (VM.Opcode.ShiftR);
        }
        internal void ShiftLeft ()
        {
            _vm.GenCode (VM.Opcode.ShiftL);
        }
        internal void Equal ()
        {
            _vm.GenCode (VM.Opcode.Equ);
        }
        internal void NotEqual ()
        {
            _vm.GenCode (VM.Opcode.Nequ);
        }
        internal void Greater ()
        {
            _vm.GenCode (VM.Opcode.Greater);
        }
        internal void Smaller ()
        {
            _vm.GenCode (VM.Opcode.Smaller);
        }
        internal void GreaterOrEqual ()
        {
            _vm.GenCode (VM.Opcode.GreaterEq);
        }
        internal void SmallerOrEqual ()
        {
            _vm.GenCode (VM.Opcode.SmallerEq);
        }
        internal void LogicalAnd ()
        {
            _vm.GenCode (VM.Opcode.LAnd);
        }
        internal void LogicalOr ()
        {
            _vm.GenCode (VM.Opcode.Lor);
        }
        internal void And ()
        {
            _vm.GenCode (VM.Opcode.And);
        }
        internal void Or ()
        {
            _vm.GenCode (VM.Opcode.Or);
        }
        internal void Xor ()
        {
            _vm.GenCode (VM.Opcode.Xor);
        }

#if DEBUG
        internal string Dump ()
        {
            return _vm.DisplayDisassembly ();
        }

        internal string DumpResults ()
        {
            return _vm.DisplayResults ();
        }
#endif
        private VM _vm;
        internal Stack<int> _addresses = new Stack<int> ();
    }
#if DEBUG
    [DebuggerDisplay ("{Display ()}")]
#endif
    public class Data
    {
        internal Data (VarType type, object data)
        {
            _type = type;
            _data = data;
        }

        internal Data Clone ()
        {
            return new Data (_type, _data);
        }

        internal void Sum (Data data)
        {
            if (data._type == _type)
            {
                if (_type == VarType.Number)
                {
                    _data = (double) _data + (double) data._data;
                    return;
                }
                if (_type == VarType.String)
                {
                    _data = (string) _data + (string) data._data;
                    return;
                }
            }
            throw new InvalidOperationException ();
        }

        internal void Minus (Data data)
        {
            if (data._type == _type && _type == VarType.Number)
            {
                _data = (double) _data - (double) data._data;
                return;
            }
            throw new InvalidOperationException ();
        }

        internal void Multiply (Data data)
        {
            if (data._type == _type && _type == VarType.Number)
            {
                _data = (double) _data * (double) data._data;
                return;
            }
            throw new InvalidOperationException ();
        }

        internal void Divide (Data data)
        {
            if (data._type == _type && _type == VarType.Number)
            {
                _data = (double) _data / (double) data._data;
                return;
            }
            throw new InvalidOperationException ();
        }

        internal void Modulo (Data data)
        {
            if (data._type == _type && _type == VarType.Number)
            {
                _data = (double) _data % (double) data._data;
                return;
            }
            throw new InvalidOperationException ();
        }

        internal void ShiftRight (Data data)
        {
            if (data._type == _type && _type == VarType.Number)
            {
                _data = (double) (((int) (double) _data) >> ((int) (double) data._data));
                return;
            }
            throw new InvalidOperationException ();
        }

        internal void ShiftLeft (Data data)
        {
            if (data._type == _type && _type == VarType.Number)
            {
                _data = (double) (((int) (double) _data) << ((int) (double) data._data));
                return;
            }
            throw new InvalidOperationException ();
        }

        internal void Equal (Data data, bool equality)
        {
            if (data._type == _type)
            {
                _data = _data.Equals (data._data);
                if (!equality)
                {
                    _data = !(bool) _data;
                }
                _type = VarType.Boolean;
            }
            else
            {
                throw new InvalidOperationException ();
            }
        }

        internal void Greater (Data data)
        {
            if (data._type == _type && _type == VarType.Number)
            {
                _data = (double) _data > (double) data._data;
                _type = VarType.Boolean;
            }
            else
            {
                throw new InvalidOperationException ();
            }
        }

        internal void Smaller (Data data)
        {
            if (data._type == _type && _type == VarType.Number)
            {
                _data = (double) _data < (double) data._data;
                _type = VarType.Boolean;
            }
            else
            {
                throw new InvalidOperationException ();
            }
        }

        internal void GreaterOrEqual (Data data)
        {
            if (data._type == _type && _type == VarType.Number)
            {
                _data = (double) _data >= (double) data._data;
                _type = VarType.Boolean;
            }
            else
            {
                throw new InvalidOperationException ();
            }
        }

        internal void SmallerOrEqual (Data data)
        {
            if (data._type == _type && _type == VarType.Number)
            {
                _data = (double) _data <= (double) data._data;
                _type = VarType.Boolean;
            }
            else
            {
                throw new InvalidOperationException ();
            }
        }

        internal void LogicalAnd (Data data)
        {
            if (data._type == _type && _type == VarType.Boolean)
            {
                _data = (bool) _data && (bool) data._data;
                return;
            }
            throw new InvalidOperationException ();
        }

        internal void LogicalOr (Data data)
        {
            if (data._type == _type && _type == VarType.Boolean)
            {
                _data = (bool) _data || (bool) data._data;
                return;
            }
            throw new InvalidOperationException ();
        }

        internal void And (Data data)
        {
            if (data._type == _type && _type == VarType.Number)
            {
                _data = (double) ((int) (double) _data & (int) (double) data._data);
                return;
            }
            throw new InvalidOperationException ();
        }

        internal void Or (Data data)
        {
            if (data._type == _type && _type == VarType.Number)
            {
                _data = (double) ((int) (double) _data | (int) (double) data._data);
                return;
            }
            throw new InvalidOperationException ();
        }

        internal void Xor (Data data)
        {
            if (data._type == _type && _type == VarType.Number)
            {
                _data = (double) ((int) (double) _data ^ (int) (double) data._data);
                return;
            }
            throw new InvalidOperationException ();
        }

        internal bool IsFalse ()
        {
            if (_type == VarType.Boolean)
            {
                return (!(bool) _data);
            }
            throw new InvalidOperationException ();
        }

#if DEBUG
        public override string ToString ()
        {
            return Display ();
        }

        internal string Display ()
        {
            return (_data != null ? "" + _data.ToString () : "") + " {" + _type.ToString () + "}";
        }
#endif
        internal object _data;
        internal VarType _type;
    }

    public enum VarType
    {
        Number,
        String,
        Boolean,
        Null,
        Undefined,
        Compound
    }

    public enum AssignmentType
    {
        Equal,
        Mul,
        Div,
        Mod,
        Plus,
        Minus,
        Ls,
        Rs,
        And,
        Xor,
        Or
    }
}
