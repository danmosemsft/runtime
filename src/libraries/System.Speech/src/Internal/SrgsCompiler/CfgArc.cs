//---------------------------------------------------------------------------
// <copyright file="CfgArc.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
//
//
// Description: 
//		SAPI respresentation for an Arc in a CFG file
//
// History:
//		5/1/2004	jeanfp		Created from the Sapi Managed code
//---------------------------------------------------------------------------
using System;
using System.Globalization;
using System.Speech.Internal.SrgsParser;

namespace System.Speech.Internal.SrgsCompiler
{
    /// <summary>
    /// Summary description for CfgArc.
    /// </summary>
    internal struct CfgArc
    {
        //*******************************************************************
        //
        // Constructors
        //
        //*******************************************************************

        #region Constructors

        internal CfgArc (CfgArc arc)
        {
            _flag1 = arc._flag1;
            _flag2 = arc._flag2;
        }

        #endregion

        //*******************************************************************
        //
        // Internal Properties
        //
        //*******************************************************************

        #region Internal Properties

        internal bool RuleRef
        {
            get
            {
                return ((_flag1 & 0x1) != 0);
            }
            set
            {
                if (value)
                {
                    _flag1 |= 0x1;
                }
                else
                {
                    _flag1 &= ~0x1U;
                }
            }
        }

        internal bool LastArc
        {
            get
            {
                return ((_flag1 & 0x2) != 0);
            }
            set
            {
                if (value)
                {
                    _flag1 |= 0x2;
                }
                else
                {
                    _flag1 &= ~0x2U;
                }
            }
        }

        internal bool HasSemanticTag
        {
            get
            {
                return ((_flag1 & 0x4) != 0);
            }
            set
            {
                if (value)
                {
                    _flag1 |= 0x4;
                }
                else
                {
                    _flag1 &= ~0x4U;
                }
            }
        }

        internal bool LowConfRequired
        {
            get
            {
                return ((_flag1 & 0x8) != 0);
            }
            set
            {
                if (value)
                {
                    _flag1 |= 0x8;
                }
                else
                {
                    _flag1 &= ~0x8U;
                }
            }
        }

        internal bool HighConfRequired
        {
            get
            {
                return ((_flag1 & 0x10) != 0);
            }
            set
            {
                if (value)
                {
                    _flag1 |= 0x10;
                }
                else
                {
                    _flag1 &= ~0x10U;
                }
            }
        }

        internal uint TransitionIndex
        {
            get
            {
                return (_flag1 >> 5) & 0x3FFFFF;
            }
            set
            {
                if (value > 0x3FFFFFU)
                {
                    XmlParser.ThrowSrgsException (SRID.TooManyArcs);
                }

                _flag1 &= ~(0x3FFFFFU << 5);
                _flag1 |= value << 5;
            }
        }

        internal uint MatchMode
        {
            set
            {
                _flag1 &= ~(0x38000000U);
                _flag1 |= value << 27;
            }
#if CFGDUMP || VSCOMPILE
            get
            {
                return (_flag1 >> 27) & 0x7;
            }
#endif
        }

        //		internal uint Weight
//		{
//			get
//			{
//				return _flag2 & 0xFF;
//			}
//			set
//			{
//				if (value > 0xFF)
//				{
//					throw new OverflowException (SR.Get (SRID.TooManyArcs));
//				}
//
//				_flag2 &= ~(uint) 0xFF;
//				_flag2 |= value;
//			}
//		}
//
        internal uint NextStartArcIndex
        {
            get
            {
                return (_flag2 >> 8) & 0x3FFFFF;
            }
            set
            {
                if (value > 0x3FFFFF)
                {
                    XmlParser.ThrowSrgsException (SRID.TooManyArcs);
                }

                _flag2 &= ~(0x3FFFFFU << 8);
                _flag2 |= value << 8;
            }
        }

#if	false
        internal string DumpFlags
        {
            get
            {
                return string.Format (CultureInfo.InvariantCulture, "flag1: {0:x} flag2: {1:x}", _flag1, _flag2);
            }
        }
#endif

        #endregion

        //*******************************************************************
        //
        // Private Fields
        //
        //*******************************************************************

        #region private Fields

        private uint _flag1;

        private uint _flag2;

        #endregion
    }
}
