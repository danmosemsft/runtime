// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Speech.Internal;
using System.Speech.Internal.Synthesis;

namespace System.Speech.Synthesis
{
	/// <summary>Contains information about a speech synthesis voice installed in Windows.</summary>
	[DebuggerDisplay("{VoiceInfo.Name} [{Enabled ? \"Enabled\" : \"Disabled\"}]")]
	public class InstalledVoice
	{
		private VoiceInfo _voice;

		private bool _enabled;

		private VoiceSynthesis _voiceSynthesizer;

		/// <summary>Gets information about a voice, such as culture, name, gender, and age.</summary>
		/// <returns>The information about an installed voice.</returns>
		public VoiceInfo VoiceInfo => _voice;

		/// <summary>Gets or sets whether a voice can be used to generate speech.</summary>
		/// <returns>Returns a <see langword="bool" /> that represents the enabled state of the voice.</returns>
		public bool Enabled
		{
			get
			{
				return _enabled;
			}
			set
			{
				SetEnabledFlag(value, switchContext: true);
			}
		}

		internal InstalledVoice(VoiceSynthesis voiceSynthesizer, VoiceInfo voice)
		{
			_voiceSynthesizer = voiceSynthesizer;
			_voice = voice;
			_enabled = true;
		}

		/// <summary>Determines if a given object is an instance of <see cref="T:System.Speech.Synthesis.InstalledVoice" /> and equal to the current instance of <see cref="T:System.Speech.Synthesis.InstalledVoice" />.</summary>
		/// <param name="obj">An object that can be cast to an instance of <see cref="T:System.Speech.Synthesis.InstalledVoice" />.</param>
		/// <returns>Returns <see langword="true" /> if the current instance of <see cref="T:System.Speech.Synthesis.InstalledVoice" /> and that obtained from the <paramref name="obj" /> argument are equal, otherwise returns <see langword="false" />.</returns>
		public override bool Equals(object obj)
		{
			InstalledVoice installedVoice = obj as InstalledVoice;
			if (installedVoice == null)
			{
				return false;
			}
			if (VoiceInfo.Name == installedVoice.VoiceInfo.Name && VoiceInfo.Age == installedVoice.VoiceInfo.Age && VoiceInfo.Gender == installedVoice.VoiceInfo.Gender)
			{
				return VoiceInfo.Culture.Equals(installedVoice.VoiceInfo.Culture);
			}
			return false;
		}

		/// <summary>Provides a hash code for an <c>InstalledVoice</c> object.</summary>
		/// <returns>A hash code for the current <see cref="T:System.Speech.Synthesis.InstalledVoice" /> object.</returns>
		public override int GetHashCode()
		{
			return VoiceInfo.Name.GetHashCode();
		}

		internal static InstalledVoice Find(List<InstalledVoice> list, VoiceInfo voiceId)
		{
			foreach (InstalledVoice item in list)
			{
				if (item.Enabled && item.VoiceInfo.Equals(voiceId))
				{
					return item;
				}
			}
			return null;
		}

		internal static InstalledVoice FirstEnabled(List<InstalledVoice> list, CultureInfo culture)
		{
			InstalledVoice installedVoice = null;
			foreach (InstalledVoice item in list)
			{
				if (item.Enabled)
				{
					if (Helpers.CompareInvariantCulture(item.VoiceInfo.Culture, culture))
					{
						return item;
					}
					if (installedVoice == null)
					{
						installedVoice = item;
					}
				}
			}
			return installedVoice;
		}

		internal void SetEnabledFlag(bool value, bool switchContext)
		{
			try
			{
				if (_enabled == value)
				{
					return;
				}
				_enabled = value;
				if (!_enabled)
				{
					if (_voice.Equals(_voiceSynthesizer.CurrentVoice(switchContext).VoiceInfo))
					{
						_voiceSynthesizer.Voice = null;
					}
				}
				else
				{
					_voiceSynthesizer.Voice = null;
				}
			}
			catch (InvalidOperationException)
			{
				_voiceSynthesizer.Voice = null;
			}
		}
	}
}
