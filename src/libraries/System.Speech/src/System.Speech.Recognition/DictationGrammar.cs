// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Recognition
{
	/// <summary>Represents a speech recognition grammar used for free text dictation.</summary>
	public class DictationGrammar : Grammar
	{
		private static Uri _defaultDictationUri = new Uri("grammar:dictation");

		/// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.DictationGrammar" /> class for the default dictation grammar provided by Windows Desktop Speech Technology.</summary>
		public DictationGrammar()
			: base(_defaultDictationUri, null, null)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.DictationGrammar" /> class with a specific dictation grammar.</summary>
		/// <param name="topic">An XML-compliant Universal Resource Identifier (URI) that specifies the dictation grammar, either <c>grammar:dictation</c> or <c>grammar:dictation#spelling</c>.</param>
		public DictationGrammar(string topic)
			: base(new Uri(topic, UriKind.RelativeOrAbsolute), null, null)
		{
		}

		/// <summary>Adds a context to a dictation grammar that has been loaded by a <see cref="T:System.Speech.Recognition.SpeechRecognizer" /> or a <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> object.</summary>
		/// <param name="precedingText">Text that indicates the start of a dictation context.</param>
		/// <param name="subsequentText">Text that indicates the end of a dictation context.</param>
		public void SetDictationContext(string precedingText, string subsequentText)
		{
			if (base.State != GrammarState.Loaded)
			{
				throw new InvalidOperationException(SR.Get(SRID.GrammarNotLoaded));
			}
			base.Recognizer.SetDictationContext(this, precedingText, subsequentText);
		}
	}
}
