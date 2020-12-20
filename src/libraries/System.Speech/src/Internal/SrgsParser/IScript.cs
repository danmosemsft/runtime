namespace System.Speech.Internal.SrgsParser
{
    /// <summary>
    /// Interface definition for the IScript
    /// </summary>
    internal interface IScript : IElement
    {
        IScript Create (string rule, RuleMethodScript onInit);
    }

    internal enum RuleMethodScript
    {
        onInit = 1,
        onParse = 2,
        onRecognition = 3,
        onError
    }
}
