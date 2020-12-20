using System.Speech.Internal.SrgsParser;

namespace System.Speech.Internal.SrgsParser
{
    /// <summary>
    /// Interface definition for the IElementTag
    /// </summary>
    internal interface IPropertyTag : IElement
    {
        void NameValue (IElement parent, string name, object value);
    }
}
