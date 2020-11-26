// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

namespace System.Speech.Synthesis.TtsEngine
{
    /// <summary>Used to specify the type of event, and its arguments (if any)  to be generated as part of the rendering of text to speech by a custom synthetic speech engine.</summary>
    [ImmutableObject(true)]
    public struct SpeechEventInfo : IEquatable<SpeechEventInfo>
    {
        private short _eventId;

        private short _parameterType;

        private int _param1;

        private IntPtr _param2;

        /// <summary>Gets and set the Speech platform event which an instance of <see cref="System.Speech.Synthesis.TtsEngine.SpeechEventInfo" /> is used to request.</summary>
        /// <returns>Returns a member of <see cref="System.Speech.Synthesis.TtsEngine.TtsEventId" /> as a <see langword="short" />, indicating the event type the <see cref="System.Speech.Synthesis.TtsEngine.SpeechEventInfo" /> object is to generate.</returns>
        public short EventId
        {
            get
            {
                return _eventId;
            }
            internal set
            {
                _eventId = value;
            }
        }

        /// <summary>Returns the data type of the object pointed to by the <see langword="IntPtr" /> returned by the <see cref="System.Speech.Synthesis.TtsEngine.SpeechEventInfo.Param2" /> parameter on the current <see cref="System.Speech.Synthesis.TtsEngine.SpeechEventInfo" /> object.</summary>
        /// <returns>A <see langword="short" /> value corresponding to a member of the <see cref="System.Speech.Synthesis.TtsEngine.EventParameterType" /> enumeration and indicating the data type of the object pointed to by the <see langword="IntPtr" /> returned by the <see cref="System.Speech.Synthesis.TtsEngine.SpeechEventInfo.Param2" /> parameter and used as the second argument for the constructor of the current <see cref="System.Speech.Synthesis.TtsEngine.SpeechEventInfo" /> object.</returns>
        public short ParameterType
        {
            get
            {
                return _parameterType;
            }
            internal set
            {
                _parameterType = value;
            }
        }

        /// <summary>Gets and set the <see langword="integer" /> value (<paramref name="param1" /> in the constructor) to be passed to the Speech platform to generate an event the current instance of <see cref="System.Speech.Synthesis.TtsEngine.SpeechEventInfo" /> is used to request.</summary>
        /// <returns>Returns the <see langword="integer" /> to be passed to Speech platform when the event specified by the current instance of <see cref="System.Speech.Synthesis.TtsEngine.SpeechEventInfo" /> is generated.</returns>
        public int Param1
        {
            get
            {
                return _param1;
            }
            internal set
            {
                _param1 = value;
            }
        }

        /// <summary>Gets and set the <see langword="System.IntPtr" /> instance (<paramref name="param2" /> in the constructor) referencing the object to be passed to the Speech platform to generate an event the current instance of <see cref="System.Speech.Synthesis.TtsEngine.SpeechEventInfo" /> is used to request.</summary>
        /// <returns>Returns the <see langword="System.IntPtr" /> referencing the object to be passed to Speech platform when the event specified by the current instance of <see cref="System.Speech.Synthesis.TtsEngine.SpeechEventInfo" /> is generated.</returns>
        public IntPtr Param2
        {
            get
            {
                return _param2;
            }
            internal set
            {
                _param2 = value;
            }
        }

        /// <summary>Constucts an appropriate <see cref="System.Speech.Synthesis.TtsEngine.SpeechEventInfo" />.</summary>
        /// <param name="eventId">An instance of <see cref="System.Speech.Synthesis.TtsEngine.TtsEventId" /> indicating the sort of Speech platform event the <see cref="System.Speech.Synthesis.TtsEngine.SpeechEventInfo" /> object is to handle.</param>
        /// <param name="parameterType">An instance of <see cref="System.Speech.Synthesis.TtsEngine.EventParameterType" /> indicating how the <see langword="System.IntPtr" /> reference of <paramref name="param2" /> is to be interpreted, and, by implication, the use of <paramref name="param1" />.</param>
        /// <param name="param1">An integer value to be passed to the Speech platform when the event requested by the instance of <see cref="System.Speech.Synthesis.TtsEngine.SpeechEventInfo" /> to be constructed is generated.
        ///  The exact meaning of this integer is implicitly determined by the value of <paramref name="parameterType" />.</param>
        /// <param name="param2">A <see langword="System.IntPtr" /> instance referencing an object. to be passed to the Speech platform when the event requested by the instance of <see cref="System.Speech.Synthesis.TtsEngine.SpeechEventInfo" /> to be constructed is generated.
        ///  The type which must be referenced is explicitly defined by the value <paramref name="parameterType" />. The value <see langword="System.IntPtr.Zero" />.</param>
        public SpeechEventInfo(short eventId, short parameterType, int param1, IntPtr param2)
        {
            _eventId = eventId;
            _parameterType = parameterType;
            _param1 = param1;
            _param2 = param2;
        }

        /// <summary>Determines whether two instances of <c>SpeechEventInfo</c> are equal.</summary>
        /// <param name="event1">The <see cref="System.Speech.Synthesis.TtsEngine.SpeechEventInfo" /> object to compare to <paramref name="event2" />.</param>
        /// <param name="event2">The <see cref="System.Speech.Synthesis.TtsEngine.SpeechEventInfo" /> object to compare to <paramref name="event1" />.</param>
        /// <returns>
        ///   <see langword="true" /> if <paramref name="event1" /> is the same as <paramref name="event2" />; otherwise, <see langword="false" />.</returns>
        public static bool operator ==(SpeechEventInfo event1, SpeechEventInfo event2)
        {
            if (event1.EventId == event2.EventId && event1.ParameterType == event2.ParameterType && event1.Param1 == event2.Param1)
            {
                return event1.Param2 == event2.Param2;
            }
            return false;
        }

        /// <summary>Determines whether two instances of <c>SpeechEventInfo</c> are not equal.</summary>
        /// <param name="event1">The <see cref="System.Speech.Synthesis.TtsEngine.SpeechEventInfo" /> object to compare to <paramref name="event2" />.</param>
        /// <param name="event2">The <see cref="System.Speech.Synthesis.TtsEngine.SpeechEventInfo" /> object to compare to <paramref name="event1" />.</param>
        /// <returns>
        ///   <see langword="true" /> if <paramref name="event1" /> is different from <paramref name="event2" />; otherwise, <see langword="false" />.</returns>
        public static bool operator !=(SpeechEventInfo event1, SpeechEventInfo event2)
        {
            return !(event1 == event2);
        }

        /// <summary>Determines whether a specified <c>SpeechEventInfo</c> object is equal to the current instance of <c>SpeechEventInfo</c>.</summary>
        /// <param name="other">The <see cref="System.Speech.Synthesis.TtsEngine.SpeechEventInfo" /> object to evaluate.</param>
        /// <returns>
        ///   <see langword="true" /> if <paramref name="other" /> is equal to the current <see cref="System.Speech.Synthesis.TtsEngine.SpeechEventInfo" /> object; otherwise, <see langword="false" />.</returns>
        public bool Equals(SpeechEventInfo other)
        {
            return this == other;
        }

        /// <summary>Determines whether a specified object is an instance of <c>SpeechEventInfo</c> and equal to the current instance of <c>SpeechEventInfo</c>.</summary>
        /// <param name="obj">The object to evaluate.</param>
        /// <returns>
        ///   <see langword="true" /> if <paramref name="obj" /> is equal to the current <see cref="System.Speech.Synthesis.TtsEngine.SpeechEventInfo" /> object; otherwise, <see langword="false" />.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is SpeechEventInfo))
            {
                return false;
            }
            return Equals((SpeechEventInfo)obj);
        }

        /// <summary>Provides a hash code for a <c>SpeechEventInfo</c> object.</summary>
        /// <returns>A hash code for a <see cref="System.Speech.Synthesis.TtsEngine.SpeechEventInfo" /> object.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
