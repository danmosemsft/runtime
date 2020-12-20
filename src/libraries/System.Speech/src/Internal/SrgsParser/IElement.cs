namespace System.Speech.Internal.SrgsParser
{
    /// <summary>
    /// Interface definition for the IElement
    /// </summary>
    interface IElement
    {
        void PostParse (IElement parent);
    }
}
