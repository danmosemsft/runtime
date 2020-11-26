// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Speech.Internal;

namespace System.Speech.Recognition
{
    /// <summary>Represents the semantic organization of a recognized phrase.</summary>
    [Serializable]
    [DebuggerDisplay("'{_keyName}'= {Value}  -  Children = {_dictionary.Count}")]
    [DebuggerTypeProxy(typeof(SemanticValueDebugDisplay))]
    public sealed class SemanticValue : IDictionary<string, SemanticValue>, ICollection<KeyValuePair<string, SemanticValue>>, IEnumerable<KeyValuePair<string, SemanticValue>>, IEnumerable
    {
        internal class SemanticValueDebugDisplay
        {
            private object _name;

            private object _value;

            private float _confidence;

            private IDictionary<string, SemanticValue> _dictionary;

            public object Value => _value;

            public object Count => _dictionary.Count;

            public object KeyName => _name;

            public object Confidence => _confidence;

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public SemanticValue[] AKeys
            {
                get
                {
                    SemanticValue[] array = new SemanticValue[_dictionary.Count];
                    int num = 0;
                    foreach (KeyValuePair<string, SemanticValue> item in _dictionary)
                    {
                        array[num++] = item.Value;
                    }
                    return array;
                }
            }

            public SemanticValueDebugDisplay(SemanticValue value)
            {
                _value = value.Value;
                _dictionary = value._dictionary;
                _name = value.KeyName;
                _confidence = value.Confidence;
            }
        }

        internal Dictionary<string, SemanticValue> _dictionary;

        internal bool _valueFieldSet;

        private string _keyName;

        private float _confidence;

        private object _value;

        /// <summary>A read-only property that returns the information contained in the current <see cref="T:System.Speech.Recognition.SemanticValue" />.</summary>
        /// <returns>Returns an <see cref="T:System.Object" /> instance containing the information stored in the current <see cref="T:System.Speech.Recognition.SemanticValue" /> instance.</returns>
        public object Value
        {
            get
            {
                return _value;
            }
            internal set
            {
                _value = value;
            }
        }

        /// <summary>Returns a relative measure of the certainty as to the correctness of the semantic parsing that returned the current instance of <see cref="T:System.Speech.Recognition.SemanticValue" />.</summary>
        /// <returns>Returns a <see langword="float" /> that is a relative measure of the certainty of semantic parsing that returned the current instance of <see cref="T:System.Speech.Recognition.SemanticValue" />.</returns>
        public float Confidence => _confidence;

        /// <summary>Returns child <see cref="T:System.Speech.Recognition.SemanticValue" /> instances that  belong to the current <see cref="T:System.Speech.Recognition.SemanticValue" />.</summary>
        /// <param name="key">A key for a <see langword="KeyValuePair&lt;String, SemanticValue&gt;" /> contained in the current instance of <see cref="T:System.Speech.Recognition.SemanticValue" />.</param>
        /// <returns>Returns a child of the current <see cref="T:System.Speech.Recognition.SemanticValue" /> that can be indexed as part of a key value pair: <c>KeyValuePair&lt;String,</c><c>SemanticValue&gt;</c>.</returns>
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown if no child member of the current instance of <see cref="T:System.Speech.Recognition.SemanticValue" /> has the key matching the <paramref name="key" /> parameter.</exception>
        /// <exception cref="T:System.InvalidOperationException">Thrown if code attempts to change the <see cref="T:System.Speech.Recognition.SemanticValue" /> at a given index.</exception>
        public SemanticValue this[string key]
        {
            get
            {
                return _dictionary[key];
            }
            set
            {
                throw new InvalidOperationException(SR.Get(SRID.CollectionReadOnly));
            }
        }

        /// <summary>Returns the number of child <see cref="T:System.Speech.Recognition.SemanticValue" /> objects under the current <see cref="T:System.Speech.Recognition.SemanticValue" /> instance.</summary>
        /// <returns>The number of child <see cref="T:System.Speech.Recognition.SemanticValue" /> objects under the current <see cref="T:System.Speech.Recognition.SemanticValue" />.</returns>
        public int Count => _dictionary.Count;

        bool ICollection<KeyValuePair<string, SemanticValue>>.IsReadOnly => true;

        ICollection<string> IDictionary<string, SemanticValue>.Keys => _dictionary.Keys;

        ICollection<SemanticValue> IDictionary<string, SemanticValue>.Values => _dictionary.Values;

        internal string KeyName => _keyName;

        /// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.SemanticValue" /> class and specifies a semantic value, a key name, and a confidence level.</summary>
        /// <param name="keyName">A key that can be used to reference this <see cref="T:System.Speech.Recognition.SemanticValue" /> instance.</param>
        /// <param name="value">An object containing information to be stored in the <see cref="T:System.Speech.Recognition.SemanticValue" /> object.</param>
        /// <param name="confidence">A <see langword="float" /> containing an estimate of the certainty of semantic analysis.</param>
        public SemanticValue(string keyName, object value, float confidence)
        {
            Helpers.ThrowIfNull(keyName, "keyName");
            _dictionary = new Dictionary<string, SemanticValue>();
            _confidence = confidence;
            _keyName = keyName;
            _value = value;
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.SemanticValue" /> class and specifies a semantic value.</summary>
        /// <param name="value">The information to be stored in the <see cref="T:System.Speech.Recognition.SemanticValue" /> object.</param>
        public SemanticValue(object value)
            : this(string.Empty, value, -1f)
        {
        }

        /// <summary>Determines whether a specified object is an instance of <c>SemanticValue</c> and equal to the current instance of <c>SemanticValue</c>.</summary>
        /// <param name="obj">The object to evaluate.</param>
        /// <returns>
        ///   <see langword="true" /> if the specified Object is equal to the current Object; otherwise, <see langword="false" />.</returns>
        public override bool Equals(object obj)
        {
            SemanticValue semanticValue = obj as SemanticValue;
            if (semanticValue == null || semanticValue.Count != Count || (semanticValue.Value == null && Value != null) || (semanticValue.Value != null && !semanticValue.Value.Equals(Value)))
            {
                return false;
            }
            foreach (KeyValuePair<string, SemanticValue> item in _dictionary)
            {
                if (!semanticValue.ContainsKey(item.Key) || !semanticValue[item.Key].Equals(this[item.Key]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>Provides a hash code for a <c>SemanticValue</c> object.</summary>
        /// <returns>A hash code for the current <see cref="T:System.Speech.Recognition.SemanticValue" /> object.</returns>
        public override int GetHashCode()
        {
            return Count;
        }

        /// <summary>Indicates whether the current <see cref="T:System.Speech.Recognition.SemanticValue" /> instance collection contains a specific key and a specific instance of <see cref="T:System.Speech.Recognition.SemanticValue" /> expressed as a key/value pair.</summary>
        /// <param name="item">An instance of <see cref="T:System.Collections.Generic.KeyValuePair`2" /> instantiated for a given value of a key string and a <see cref="T:System.Speech.Recognition.SemanticValue" /> instance.</param>
        /// <returns>Returns a <see langword="bool" /> which is <see langword="true" /> if the current <see cref="T:System.Speech.Recognition.SemanticValue" /> contains an instance of <c>KeyValuePair&lt;String, SemanticValue&gt;</c> for a specified value of the key string and the <see cref="T:System.Speech.Recognition.SemanticValue" />. Otherwise, <see langword="false" /> is returned.</returns>
        public bool Contains(KeyValuePair<string, SemanticValue> item)
        {
            if (_dictionary.ContainsKey(item.Key))
            {
                return _dictionary.ContainsValue(item.Value);
            }
            return false;
        }

        /// <summary>Indicates whether the current <see cref="T:System.Speech.Recognition.SemanticValue" /> instance collection contains a child <see cref="T:System.Speech.Recognition.SemanticValue" /> instance with a given key string.</summary>
        /// <param name="key">
        ///   <see cref="T:System.String" /> containing the key string used to identify a child instance of <see cref="T:System.Speech.Recognition.SemanticValue" /> under the current <see cref="T:System.Speech.Recognition.SemanticValue" />.</param>
        /// <returns>Returns a <see langword="bool" />, <see langword="true" /> if a child instance <see cref="T:System.Speech.Recognition.SemanticValue" /> tagged with the string <paramref name="key" /> is found, <see langword="false" /> if not.</returns>
        public bool ContainsKey(string key)
        {
            return _dictionary.ContainsKey(key);
        }

        void ICollection<KeyValuePair<string, SemanticValue>>.Add(KeyValuePair<string, SemanticValue> key)
        {
            throw new NotSupportedException(SR.Get(SRID.CollectionReadOnly));
        }

        void IDictionary<string, SemanticValue>.Add(string key, SemanticValue value)
        {
            throw new NotSupportedException(SR.Get(SRID.CollectionReadOnly));
        }

        void ICollection<KeyValuePair<string, SemanticValue>>.Clear()
        {
            throw new NotSupportedException(SR.Get(SRID.CollectionReadOnly));
        }

        bool ICollection<KeyValuePair<string, SemanticValue>>.Remove(KeyValuePair<string, SemanticValue> key)
        {
            throw new NotSupportedException(SR.Get(SRID.CollectionReadOnly));
        }

        bool IDictionary<string, SemanticValue>.Remove(string key)
        {
            throw new NotSupportedException(SR.Get(SRID.CollectionReadOnly));
        }

        void ICollection<KeyValuePair<string, SemanticValue>>.CopyTo(KeyValuePair<string, SemanticValue>[] array, int index)
        {
            ((ICollection<KeyValuePair<string, SemanticValue>>)_dictionary).CopyTo(array, index);
        }

        IEnumerator<KeyValuePair<string, SemanticValue>> IEnumerable<KeyValuePair<string, SemanticValue>>.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>Returns an enumerator that iterates through a collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, SemanticValue>>)this).GetEnumerator();
        }

        bool IDictionary<string, SemanticValue>.TryGetValue(string key, out SemanticValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }
    }
}
