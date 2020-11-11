using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Speech.AudioFormat;
using System.Speech.Internal;
using System.Speech.Internal.ObjectTokens;

namespace System.Speech.Recognition
{
	/// <summary>Represents information about a <see cref="T:System.Speech.Recognition.SpeechRecognizer" /> or <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> instance.</summary>
	public class RecognizerInfo : IDisposable
	{
		private ReadOnlyDictionary<string, string> _attributes;

		private string _id;

		private string _name;

		private string _description;

		private string _sapiObjectTokenId;

		private CultureInfo _culture;

		private ReadOnlyCollection<SpeechAudioFormatInfo> _supportedAudioFormats;

		private ObjectToken _objectToken;

		/// <summary>Gets the identifier of a <see cref="T:System.Speech.Recognition.SpeechRecognizer" /> or <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> instance.</summary>
		/// <returns>Returns the identifier for a specific <see cref="T:System.Speech.Recognition.SpeechRecognizer" /> or <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> instance.</returns>
		public string Id => _id;

		/// <summary>Gets the friendly name of a <see cref="T:System.Speech.Recognition.SpeechRecognizer" /> or <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> instance.</summary>
		/// <returns>Returns the friendly name for a specific <see cref="T:System.Speech.Recognition.SpeechRecognizer" /> or <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> instance.</returns>
		public string Name => _name;

		/// <summary>Gets the description of a <see cref="T:System.Speech.Recognition.SpeechRecognizer" /> or <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> instance.</summary>
		/// <returns>Returns a <see langword="string" /> that describes the configuration for a specific <see cref="T:System.Speech.Recognition.SpeechRecognizer" /> or <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> instance.</returns>
		public string Description => _description;

		/// <summary>Gets the culture supported by a <see cref="T:System.Speech.Recognition.SpeechRecognizer" /> or <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> instance.</summary>
		/// <returns>Returns information about the culture supported by a given <see cref="T:System.Speech.Recognition.SpeechRecognizer" /> or <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> instance.</returns>
		public CultureInfo Culture => _culture;

		/// <summary>Gets the audio formats supported by a <see cref="T:System.Speech.Recognition.SpeechRecognizer" /> or <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> instance.</summary>
		/// <returns>Returns a list of audio formats supported by a specific <see cref="T:System.Speech.Recognition.SpeechRecognizer" /> or <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> instance.</returns>
		public ReadOnlyCollection<SpeechAudioFormatInfo> SupportedAudioFormats => _supportedAudioFormats;

		/// <summary>Gets additional information about a <see cref="T:System.Speech.Recognition.SpeechRecognizer" /> or <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> instance.</summary>
		/// <returns>Returns an instance of <see cref="T:System.Collections.Generic.IDictionary`2" /> containing information about the configuration of a <see cref="T:System.Speech.Recognition.SpeechRecognizer" /> or <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> object.</returns>
		public IDictionary<string, string> AdditionalInfo => _attributes;

		private RecognizerInfo(ObjectToken token, CultureInfo culture)
		{
			_id = token.Name;
			_description = token.Description;
			_sapiObjectTokenId = token.Id;
			_name = token.TokenName();
			_culture = culture;
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string[] valueNames = token.Attributes.GetValueNames();
			foreach (string text in valueNames)
			{
				if (token.Attributes.TryGetString(text, out string value))
				{
					dictionary[text] = value;
				}
			}
			_attributes = new ReadOnlyDictionary<string, string>(dictionary);
			if (token.Attributes.TryGetString("AudioFormats", out string value2))
			{
				_supportedAudioFormats = new ReadOnlyCollection<SpeechAudioFormatInfo>(SapiAttributeParser.GetAudioFormatsFromString(value2));
			}
			else
			{
				_supportedAudioFormats = new ReadOnlyCollection<SpeechAudioFormatInfo>(new List<SpeechAudioFormatInfo>());
			}
			_objectToken = token;
		}

		internal static RecognizerInfo Create(ObjectToken token)
		{
			if (token.Attributes == null)
			{
				return null;
			}
			if (!token.Attributes.TryGetString("Language", out string value))
			{
				return null;
			}
			CultureInfo cultureInfoFromLanguageString = SapiAttributeParser.GetCultureInfoFromLanguageString(value);
			if (cultureInfoFromLanguageString != null)
			{
				return new RecognizerInfo(token, cultureInfoFromLanguageString);
			}
			return null;
		}

		internal ObjectToken GetObjectToken()
		{
			return _objectToken;
		}

		/// <summary>Disposes the <c>RecognizerInfo</c> object.</summary>
		public void Dispose()
		{
			_objectToken.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}
