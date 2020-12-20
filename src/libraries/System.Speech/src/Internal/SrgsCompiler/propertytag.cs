using System;
using System.Runtime.InteropServices;
using System.Speech.Internal.SrgsParser;

#pragma warning disable 1634, 1691 // Allows suppression of certain PreSharp messages.

namespace System.Speech.Internal.SrgsCompiler
{
    /// <summary>
    /// Summary description for Rule.
    /// </summary>
    internal sealed class PropertyTag : ParseElement, IPropertyTag
    {
        //*******************************************************************
        //
        // Constructors
        //
        //*******************************************************************

        #region Constructors

        internal PropertyTag (ParseElement parent, Backend backend)
            : base (parent._rule)
        {
        }

        #endregion

        //*******************************************************************
        //
        // Internal Methods
        //
        //*******************************************************************

        #region Internal Methods

#pragma warning disable 56507

        /// TODOC <_include file='doc\Tag.uex' path='docs/doc[@for="Tag.RepeatProbability"]/*' />
        // The probability that this item will be repeated.
        void IPropertyTag.NameValue (IElement parent, string name, object value)
        {
            //Return if the Tag content is empty
            string sValue = value as string;
            if (string.IsNullOrEmpty (name) && (value == null || (sValue != null && string.IsNullOrEmpty ((sValue).Trim ()))))
            {
                return;
            }

            // Build semantic properties to attach to epsilon transition.
            // <tag>Name=</tag>             pszValue = null     vValue = VT_EMPTY
            // <tag>Name="string"</tag>     pszValue = "string" vValue = VT_EMPTY
            // <tag>Name=true</tag>         pszValue = null     vValue = VT_BOOL
            // <tag>Name=123</tag>          pszValue = null     vValue = VT_I4
            // <tag>Name=3.14</tag>         pszValue = null     vValue = VT_R8            

            if (!string.IsNullOrEmpty (name))
            {
                // Set property name
                _propInfo._pszName = name;
            }
            else
            {
                // If no property, set the name to the anonymous property name
                _propInfo._pszName = "=";
            }

            // Set property value
            _propInfo._comValue = value;
            if (value == null)
            {
                _propInfo._comType = VarEnum.VT_EMPTY;
            }
            else if (sValue != null)
            {
                _propInfo._comType = VarEnum.VT_EMPTY;
            }
            else if (value is int)
            {
                _propInfo._comType = VarEnum.VT_I4;
            }
            else if (value is double)
            {
                _propInfo._comType = VarEnum.VT_R8;
            }
            else if (value is bool)
            {
                _propInfo._comType = VarEnum.VT_BOOL;
            }
            else
            {
                // should never get here
                System.Diagnostics.Debug.Assert (false);
            }
        }

        void IElement.PostParse (IElement parentElement)
        {
            ParseElementCollection parent = (ParseElementCollection) parentElement;
            _propInfo._ulId = (uint) parent._rule._iSerialize2;

            // Attach the semantic properties on the parent element.
            parent.AddSementicPropertyTag (_propInfo);
        }

#pragma warning restore 56507

        #endregion

        //*******************************************************************
        //
        // Private Fields
        //
        //*******************************************************************

        #region Private Fields

        private CfgGrammar.CfgProperty _propInfo = new CfgGrammar.CfgProperty ();

        #endregion

    }
}
