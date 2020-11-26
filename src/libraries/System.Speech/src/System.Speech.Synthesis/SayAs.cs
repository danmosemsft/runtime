// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Synthesis
{
    /// <summary>Enumerates the content types for the speaking of elements such as times, dates, and currency.</summary>
    public enum SayAs
    {
        /// <summary>Spell the word or phrase. For example, say "clock" as "C L O C K".</summary>
        SpellOut,
        /// <summary>Speak a number as an ordinal number. For example, speak "3rd" as "third".</summary>
        NumberOrdinal,
        /// <summary>Speak a number as a cardinal number. For example, speak "3" as "three".</summary>
        NumberCardinal,
        /// <summary>Speak a number sequence as a date. For example, speak "05/19/2004" or "19.5.2004" as "may nineteenth two thousand four".</summary>
        Date,
        /// <summary>Speak a number sequence as a date including the day, month, and year. For example, speak "12/05/2004" as "May twelfth two thousand four".</summary>
        DayMonthYear,
        /// <summary>Speak a number sequence as a date including the day, month, and year. For example, speak "12/05/2004" as "December fifth two thousand four".</summary>
        MonthDayYear,
        /// <summary>Speak a number sequence as a date including the day, month, and year. For example, speak "2004/05/12" as "May twelfth two thousand four".</summary>
        YearMonthDay,
        /// <summary>Speak a number sequence as a year and month. For example, speak "2004/05" as "May two thousand four".</summary>
        YearMonth,
        /// <summary>Speak a number sequence as a month and year. For example, speak "05/2004" as "May two thousand four".</summary>
        MonthYear,
        /// <summary>Speak a number sequence as a month and day. For example, speak "05/12" as "may twelfth", and speak "12/5" as "December 5th".</summary>
        MonthDay,
        /// <summary>Speak a number sequence as a day and month. For example, speak "12/05" as "May twelfth", and speak "05/12" as "December 5th".</summary>
        DayMonth,
        /// <summary>Speak a number as a year. For example, speak "1998" as "nineteen ninety-eight".</summary>
        Year,
        /// <summary>Speak a word as a month. For example, speak "June" as "June".</summary>
        Month,
        /// <summary>Speak a number as the day in a date. For example, speak "3rd" as "third".</summary>
        Day,
        /// <summary>Speak a number sequence as a time. For example, speak "9:45" as "nine forty-five", and speak "9:45am" as "nine forty-five A M".</summary>
        Time,
        /// <summary>Speak a number sequence as a time using the 24-hour clock. For example, speak "18:00" as "eighteen hundred hours".</summary>
        Time24,
        /// <summary>Speak a number sequence as a time using the 12-hour clock. For example, speak "03:25" as "three twenty-five".</summary>
        Time12,
        /// <summary>Speak a number sequence as a U.S. telephone number. For example, speak "(306) 555-1212" as "Area code three zero six five five five one two one two".</summary>
        Telephone,
        /// <summary>Speak the word or phrase as text. For example, speak "timeline" as "timeline".</summary>
        Text
    }
}
