using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Speech.AudioFormat;
using System.Speech.Internal;
using System.Speech.Internal.ObjectTokens;
using System.Speech.Internal.Synthesis;

namespace System.Speech.Synthesis
{
	/// <summary>Represents an installed speech synthesis engine.</summary>
	[Serializable]
	[DebuggerDisplay("{(_name != null ? \"'\" + _name + \"' \" : \"\") +  (_culture != null ? \" '\" + _culture.ToString () + \"' \" : \"\") + (_gender != VoiceGender.NotSet ? \" '\" + _gender.ToString () + \"' \" : \"\") + (_age != VoiceAge.NotSet ? \" '\" + _age.ToString () + \"' \" : \"\") + (_variant > 0 ? \" \" + _variant.ToString () : \"\")}")]
	public class VoiceInfo
	{
		private string _name;

		private CultureInfo _culture;

		private VoiceGender _gender;

		private VoiceAge _age;

		private int _variant = -1;

		[NonSerialized]
		private string _id;

		[NonSerialized]
		private string _registryKeyPath;

		[NonSerialized]
		private string _assemblyName;

		[NonSerialized]
		private string _clsid;

		[NonSerialized]
		private string _description;

		[NonSerialized]
		private ReadOnlyDictionary<string, string> _attributes;

		[NonSerialized]
		private ReadOnlyCollection<SpeechAudioFormatInfo> _audioFormats;

		/// <summary>Gets the gender of the voice.</summary>
		/// <returns>Returns the gender of the voice.</returns>
		public VoiceGender Gender => _gender;

		/// <summary>Gets the age of the voice.</summary>
		/// <returns>Returns the age of the voice.</returns>
		public VoiceAge Age => _age;

		/// <summary>Gets the name of the voice.</summary>
		/// <returns>Returns the name of the voice.</returns>
		public string Name => _name;

		/// <summary>Gets the culture of the voice.</summary>
		/// <returns>Returns a <see cref="T:System.Globalization.CultureInfo" /> object that provides information about a specific culture, such as the names of the culture, the writing system, the calendar used, and how to format dates and sort strings.</returns>
		public CultureInfo Culture => _culture;

		/// <summary>Gets the ID of the voice.</summary>
		/// <returns>Returns the identifier for the voice.</returns>
		public string Id => _id;

		/// <summary>Gets the description of the voice.</summary>
		/// <returns>Returns the description of the voice.</returns>
		public string Description
		{
			get
			{
				if (_description == null)
				{
					return string.Empty;
				}
				return _description;
			}
		}

		/// <summary>Gets the collection of audio formats that the voice supports.</summary>
		/// <returns>Returns a collection of the audio formats that the voice supports.</returns>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public ReadOnlyCollection<SpeechAudioFormatInfo> SupportedAudioFormats => _audioFormats;

		/// <summary>Gets additional information about the voice.</summary>
		/// <returns>Returns a collection of name/value pairs that describe and identify the voice.</returns>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public IDictionary<string, string> AdditionalInfo
		{
			get
			{
				if (_attributes == null)
				{
					_attributes = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(0));
				}
				return _attributes;
			}
		}

		internal int Variant => _variant;

		internal string AssemblyName => _assemblyName;

		internal string Clsid => _clsid;

		internal string RegistryKeyPath => _registryKeyPath;

		internal VoiceInfo(string name)
		{
			Helpers.ThrowIfEmptyOrNull(name, "name");
			_name = name;
		}

		internal VoiceInfo(CultureInfo culture)
		{
			Helpers.ThrowIfNull(culture, "culture");
			if (culture.Equals(CultureInfo.InvariantCulture))
			{
				throw new ArgumentException(SR.Get(SRID.InvariantCultureInfo), "culture");
			}
			_culture = culture;
		}

		internal VoiceInfo(ObjectToken token)
		{
			_registryKeyPath = token._sKeyId;
			_id = token.Name;
			_description = token.Description;
			_name = token.TokenName();
			SsmlParserHelpers.TryConvertAge(token.Age.ToLowerInvariant(), out _age);
			SsmlParserHelpers.TryConvertGender(token.Gender.ToLowerInvariant(), out _gender);
			if (token.Attributes.TryGetString("Language", out string value))
			{
				_culture = SapiAttributeParser.GetCultureInfoFromLanguageString(value);
			}
			if (token.TryGetString("Assembly", out string value2))
			{
				_assemblyName = value2;
			}
			if (token.TryGetString("CLSID", out string value3))
			{
				_clsid = value3;
			}
			if (token.Attributes != null)
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				string[] valueNames = token.Attributes.GetValueNames();
				foreach (string text in valueNames)
				{
					if (token.Attributes.TryGetString(text, out string value4))
					{
						dictionary.Add(text, value4);
					}
				}
				_attributes = new ReadOnlyDictionary<string, string>(dictionary);
			}
			if (token.Attributes != null && token.Attributes.TryGetString("AudioFormats", out string value5))
			{
				_audioFormats = new ReadOnlyCollection<SpeechAudioFormatInfo>(SapiAttributeParser.GetAudioFormatsFromString(value5));
			}
			else
			{
				_audioFormats = new ReadOnlyCollection<SpeechAudioFormatInfo>(new List<SpeechAudioFormatInfo>());
			}
		}

		internal VoiceInfo(VoiceGender gender)
		{
			_gender = gender;
		}

		internal VoiceInfo(VoiceGender gender, VoiceAge age)
		{
			_gender = gender;
			_age = age;
		}

		internal VoiceInfo(VoiceGender gender, VoiceAge age, int voiceAlternate)
		{
			if (voiceAlternate < 0)
			{
				throw new ArgumentOutOfRangeException("voiceAlternate", SR.Get(SRID.PromptBuilderInvalidVariant));
			}
			_gender = gender;
			_age = age;
			_variant = voiceAlternate + 1;
		}

		/// <summary>Compares the fields of the voice with the specified <see cref="T:System.Speech.Synthesis.VoiceInfo" /> object to determine whether they contain the same values.</summary>
		/// <param name="obj">The specified <see cref="T:System.Speech.Synthesis.VoiceInfo" /> object.</param>
		/// <returns>
		///   <see langword="true" /> if the fields of the two <see cref="T:System.Speech.Synthesis.VoiceInfo" /> objects are equal; otherwise, <see langword="false" />.</returns>
		public override bool Equals(object obj)
		{
			VoiceInfo voiceInfo = obj as VoiceInfo;
			if (voiceInfo != null && _name == voiceInfo._name && (_age == voiceInfo._age || _age == VoiceAge.NotSet || voiceInfo._age == VoiceAge.NotSet) && (_gender == voiceInfo._gender || _gender == VoiceGender.NotSet || voiceInfo._gender == VoiceGender.NotSet))
			{
				if (_culture != null && voiceInfo._culture != null)
				{
					return _culture.Equals(voiceInfo._culture);
				}
				return true;
			}
			return false;
		}

		/// <summary>Provides a hash code for a <c>VoiceInfo</c> object.</summary>
		/// <returns>A hash code for the current <see cref="T:System.Speech.Synthesis.VoiceInfo" /> object.</returns>
		public override int GetHashCode()
		{
			return _name.GetHashCode();
		}

		internal static bool ValidateGender(VoiceGender gender)
		{
			if (gender != VoiceGender.Female && gender != VoiceGender.Male && gender != VoiceGender.Neutral)
			{
				return gender == VoiceGender.NotSet;
			}
			return true;
		}

		internal static bool ValidateAge(VoiceAge age)
		{
			if (age != VoiceAge.Adult && age != VoiceAge.Child && age != 0 && age != VoiceAge.Senior)
			{
				return age == VoiceAge.Teen;
			}
			return true;
		}
	}
}
