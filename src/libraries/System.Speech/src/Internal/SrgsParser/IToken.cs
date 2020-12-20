namespace System.Speech.Internal.SrgsParser
{
    /// <summary>
    /// Interface definition for the IToken
    /// </summary>
    internal interface IToken : IElement
    {
        string Text { set; }
        string Display { set; }
        string Pronunciation { set; }
    }

    internal delegate IToken CreateTokenCallback (IElement parent, string content, string pronumciation, string display, float reqConfidence);
}
