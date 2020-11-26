// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Globalization;
using System.Speech.Internal;
using System.Speech.Internal.SrgsParser;
using System.Text;
using System.Xml;

namespace System.Speech.Recognition.SrgsGrammar
{
    /// <summary>Represents an element for associating a semantic value with a phrase in a grammar.</summary>
    [Serializable]
    [DebuggerDisplay("{DebuggerDisplayString ()}")]
    public class SrgsNameValueTag : SrgsElement, IPropertyTag, IElement
    {
        private string _name;

        private object _value;

        /// <summary>Gets or sets the name of the <see cref="System.Speech.Recognition.SrgsGrammar.SrgsNameValueTag" /> instance.</summary>
        /// <returns>A string that contains the name of the <see cref="System.Speech.Recognition.SrgsGrammar.SrgsNameValueTag" /> instance.</returns>
        /// <exception cref="System.ArgumentNullException">An attempt is made to set <see cref="System.Speech.Recognition.SrgsGrammar.SrgsNameValueTag.Name" /> to <see langword="null" />.</exception>
        /// <exception cref="System.ArgumentException">An attempt is made to set <see cref="System.Speech.Recognition.SrgsGrammar.SrgsNameValueTag.Name" /> to an empty string.</exception>
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = GetTrimmedName(value, "value");
            }
        }

        /// <summary>Gets or sets the value contained in the <see cref="System.Speech.Recognition.SrgsGrammar.SrgsNameValueTag" /> instance.</summary>
        /// <returns>The value contained in the <see cref="System.Speech.Recognition.SrgsGrammar.SrgsNameValueTag" /> instance.</returns>
        /// <exception cref="System.ArgumentNullException">An attempt is made to set <see cref="System.Speech.Recognition.SrgsGrammar.SrgsNameValueTag.Value" /> to <see langword="null" />.</exception>
        /// <exception cref="System.ArgumentException">An attempt is made to set <see cref="System.Speech.Recognition.SrgsGrammar.SrgsNameValueTag.Value" /> to an invalid type.</exception>
        public object Value
        {
            get
            {
                return _value;
            }
            set
            {
                Helpers.ThrowIfNull(value, nameof(value));
                if (value is string || value is bool || value is int || value is double)
                {
                    _value = value;
                    return;
                }
                throw new ArgumentException(SR.Get(SRID.InvalidValueType), nameof(value));
            }
        }

        /// <summary>Initializes a new instance of the <see cref="System.Speech.Recognition.SrgsGrammar.SrgsNameValueTag" /> class.</summary>
        public SrgsNameValueTag()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="System.Speech.Recognition.SrgsGrammar.SrgsNameValueTag" /> class, specifying a value for the instance.</summary>
        /// <param name="value">The value used to set the <see cref="System.Speech.Recognition.SrgsGrammar.SrgsNameValueTag.Value" /> property.</param>
        /// <exception cref="System.ArgumentNullException">
        ///   <paramref name="value" /> is <see langword="null" />.</exception>
        public SrgsNameValueTag(object value)
        {
            Helpers.ThrowIfNull(value, nameof(value));
            Value = value;
        }

        /// <summary>Initializes a new instance of the <see cref="System.Speech.Recognition.SrgsGrammar.SrgsNameValueTag" /> class, specifying a name and a value for the instance.</summary>
        /// <param name="name">The string used to set the <see cref="System.Speech.Recognition.SrgsGrammar.SrgsNameValueTag.Name" /> property on the <see cref="System.Speech.Recognition.SrgsGrammar.SrgsNameValueTag" /> object.</param>
        /// <param name="value">The object used to set the <see cref="System.Speech.Recognition.SrgsGrammar.SrgsNameValueTag.Value" /> property on the <see cref="System.Speech.Recognition.SrgsGrammar.SrgsNameValueTag" /> object.</param>
        /// <exception cref="System.ArgumentNullException">
        ///   <paramref name="value" /> is <see langword="null" />.
        /// <paramref name="name" /> is <see langword="null" />.</exception>
        /// <exception cref="System.ArgumentException">
        ///   <paramref name="name" /> is an empty string.</exception>
        public SrgsNameValueTag(string name, object value)
            : this(value)
        {
            _name = GetTrimmedName(name, "name");
        }

        internal override void WriteSrgs(XmlWriter writer)
        {
            bool flag = Value != null;
            bool flag2 = !string.IsNullOrEmpty(_name);
            writer.WriteStartElement("tag");
            StringBuilder stringBuilder = new StringBuilder();
            if (flag2)
            {
                stringBuilder.Append(_name);
                stringBuilder.Append('=');
            }
            if (flag)
            {
                if (Value is string)
                {
                    stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "\"{0}\"", new object[1]
                    {
                        Value.ToString()
                    });
                }
                else
                {
                    stringBuilder.Append(Value.ToString());
                }
            }
            writer.WriteString(stringBuilder.ToString());
            writer.WriteEndElement();
        }

        internal override void Validate(SrgsGrammar grammar)
        {
            switch (grammar.TagFormat)
            {
                case SrgsTagFormat.KeyValuePairs:
                    break;
                case SrgsTagFormat.Default:
                    grammar.TagFormat |= SrgsTagFormat.KeyValuePairs;
                    break;
                default:
                    XmlParser.ThrowSrgsException(SRID.SapiPropertiesAndSemantics);
                    break;
            }
        }

        void IPropertyTag.NameValue(IElement parent, string name, object value)
        {
            _name = name;
            _value = value;
        }

        internal override string DebuggerDisplayString()
        {
            StringBuilder stringBuilder = new StringBuilder("SrgsNameValue ");
            if (_name != null)
            {
                stringBuilder.Append(_name);
                stringBuilder.Append(" (");
            }
            if (_value != null)
            {
                if (_value is string)
                {
                    stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "\"{0}\"", new object[1]
                    {
                        _value.ToString()
                    });
                }
                else
                {
                    stringBuilder.Append(_value.ToString());
                }
            }
            else
            {
                stringBuilder.Append("null");
            }
            if (_name != null)
            {
                stringBuilder.Append(')');
            }
            return stringBuilder.ToString();
        }

        private static string GetTrimmedName(string name, string parameterName)
        {
            Helpers.ThrowIfEmptyOrNull(name, parameterName);
            name = name.Trim(Helpers._achTrimChars);
            Helpers.ThrowIfEmptyOrNull(name, parameterName);
            return name;
        }
    }
}
