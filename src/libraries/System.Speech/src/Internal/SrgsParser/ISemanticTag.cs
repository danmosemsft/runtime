using System.Speech.Internal.SrgsParser;

namespace System.Speech.Internal.SrgsParser
{
    /// <summary>
    /// Interface definition for the IElementTag
    /// </summary>
    internal interface ISemanticTag : IElement
    {
        void Content (IElement parent, string value, int line);
    }
}
