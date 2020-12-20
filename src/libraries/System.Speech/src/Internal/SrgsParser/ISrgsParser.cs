namespace System.Speech.Internal.SrgsParser
{
    internal interface ISrgsParser
    {
        void Parse ();
        IElementFactory ElementFactory { set; }
    }
}
