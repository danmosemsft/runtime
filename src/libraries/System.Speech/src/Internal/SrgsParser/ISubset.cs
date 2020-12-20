namespace System.Speech.Internal.SrgsParser
{
    /// <summary>
    /// Interface definition for the ISubset
    /// </summary>
    internal interface ISubset : IElement
    {
    }

    // Must be in the same order as the Srgs enum
    internal enum MatchMode
    {
        AllWords = 0,
        Subsequence = 1,
        OrderedSubset = 3,
        SubsequenceContentRequired = 5,
        OrderedSubsetContentRequired = 7 
    }
}
