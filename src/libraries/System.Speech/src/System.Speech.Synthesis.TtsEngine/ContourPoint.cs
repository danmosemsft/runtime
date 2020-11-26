// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

namespace System.Speech.Synthesis.TtsEngine
{
    /// <summary>Represents changes in pitch for the speech content of a <see cref="System.Speech.Synthesis.TtsEngine.TextFragment" />.</summary>
    [ImmutableObject(true)]
    public struct ContourPoint : IEquatable<ContourPoint>
    {
        private float _start;

        private float _change;

        private ContourPointChangeType _changeType;

        /// <summary>Gets a <see cref="float" /> that specifies the point at which to apply the pitch change in a <see cref="System.Speech.Synthesis.TtsEngine.TextFragment" />. This is expressed as the elapsed percentage of the duration of the <see cref="System.Speech.Synthesis.TtsEngine.TextFragment" /> at that point.</summary>
        public float Start => _start;

        /// <summary>Gets the value that represents the amount to raise or lower the pitch at a point in a <see cref="System.Speech.Synthesis.TtsEngine.TextFragment" />.</summary>
        public float Change => _change;

        /// <summary>Gets a member of <see cref="System.Speech.Synthesis.TtsEngine.ContourPoint.ChangeType" /> that specifies the unit to use for the number specified in the change parameter of a <see cref="System.Speech.Synthesis.TtsEngine.ContourPoint" /> object.</summary>
        public ContourPointChangeType ChangeType => _changeType;

        /// <summary>Creates a new instance of the <see cref="System.Speech.Synthesis.TtsEngine.ContourPoint" /> class.</summary>
        /// <param name="start">A <see cref="float" /> that specifies the point at which to apply the pitch change in the <see cref="System.Speech.Synthesis.TtsEngine.TextFragment" />. This is expressed as the elapsed percentage of the duration of the <see cref="System.Speech.Synthesis.TtsEngine.TextFragment" /> at that point.</param>
        /// <param name="change">A <see cref="float" /> that specifies the amount to raise or lower the pitch.</param>
        /// <param name="changeType">A member of <see cref="System.Speech.Synthesis.TtsEngine.ContourPoint.ChangeType" /> that specifies the unit to use for the number specified in the <paramref name="change" /> parameter.</param>
        public ContourPoint(float start, float change, ContourPointChangeType changeType)
        {
            _start = start;
            _change = change;
            _changeType = changeType;
        }

        /// <summary>Determines if two instances of <see cref="System.Speech.Synthesis.TtsEngine.ContourPoint" /> are equal.</summary>
        /// <param name="point1">An instance of <see cref="System.Speech.Synthesis.TtsEngine.ContourPoint" /> to compare against the instance of <see cref="System.Speech.Synthesis.TtsEngine.ContourPoint" /> provided by the <paramref name="point2" /> argument.</param>
        /// <param name="point2">An instance of <see cref="System.Speech.Synthesis.TtsEngine.ContourPoint" /> to compare against the instance of <see cref="System.Speech.Synthesis.TtsEngine.ContourPoint" /> provided by the <paramref name="point1" /> argument.</param>
        /// <returns>Returns <see langword="true" /> if the <see cref="System.Speech.Synthesis.TtsEngine.ContourPoint" /> instances referenced by <paramref name="point1" /> and <paramref name="point2" /> are equal, otherwise returns <see langword="false" />.</returns>
        public static bool operator ==(ContourPoint point1, ContourPoint point2)
        {
            if (point1.Start.Equals(point2.Start) && point1.Change.Equals(point2.Change))
            {
                return point1.ChangeType.Equals(point2.ChangeType);
            }
            return false;
        }

        /// <summary>Determines if two instances of <see cref="System.Speech.Synthesis.TtsEngine.ContourPoint" /> are NOT equal.</summary>
        /// <param name="point1">An instance of <see cref="System.Speech.Synthesis.TtsEngine.ContourPoint" /> to compare against the instance of <see cref="System.Speech.Synthesis.TtsEngine.ContourPoint" /> provided by the <paramref name="point2" /> argument.</param>
        /// <param name="point2">An instance of <see cref="System.Speech.Synthesis.TtsEngine.ContourPoint" /> to compare against the instance of <see cref="System.Speech.Synthesis.TtsEngine.ContourPoint" /> provided by the <paramref name="point1" /> argument.</param>
        /// <returns>Returns <see langword="true" /> if the <see cref="System.Speech.Synthesis.TtsEngine.ContourPoint" /> instances referenced by <paramref name="point1" /> and <paramref name="point2" /> are NOT equal, otherwise returns <see langword="false" />.</returns>
        public static bool operator !=(ContourPoint point1, ContourPoint point2)
        {
            return !(point1 == point2);
        }

        /// <summary>Determines if a given instance of <see cref="System.Speech.Synthesis.TtsEngine.ContourPoint" /> is equal to the current instance of <see cref="System.Speech.Synthesis.TtsEngine.ContourPoint" />.</summary>
        /// <param name="other">An instance of <see cref="System.Speech.Synthesis.TtsEngine.ContourPoint" /> that will be compared to the current instance.</param>
        /// <returns>Returns <see langword="true" /> if both the current instance of <see cref="System.Speech.Synthesis.TtsEngine.ContourPoint" /> and that supplied through the <paramref name="other" /> argument are equal, otherwise returns <see langword="false" />.</returns>
        public bool Equals(ContourPoint other)
        {
            return this == other;
        }

        /// <summary>Determines if a given object is an instance of <see cref="System.Speech.Synthesis.TtsEngine.ContourPoint" /> and equal to the current instance of <see cref="System.Speech.Synthesis.TtsEngine.ContourPoint" />.</summary>
        /// <param name="obj">An object that can be cast to an instance of <see cref="System.Speech.Synthesis.TtsEngine.ContourPoint" />.</param>
        /// <returns>Returns <see langword="true" /> if the current instance of <see cref="System.Speech.Synthesis.TtsEngine.ContourPoint" /> and that obtained from the <paramref name="obj" /> argument are equal, otherwise returns <see langword="false" />.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is ContourPoint))
            {
                return false;
            }
            return Equals((ContourPoint)obj);
        }

        /// <summary>Returns a hash code for this instance.</summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
