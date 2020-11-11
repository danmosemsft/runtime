using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Speech.AudioFormat;
using System.Speech.Internal;
using System.Speech.Internal.Synthesis;
using System.Speech.Synthesis.TtsEngine;

namespace System.Speech.Synthesis
{
	/// <summary>Provides access to the functionality of an installed speech synthesis engine.</summary>
	public sealed class SpeechSynthesizer : IDisposable
	{
		private VoiceSynthesis _voiceSynthesis;

		private bool _isDisposed;

		private bool paused;

		private Stream _outputStream;

		private bool _closeStreamOnExit;

		/// <summary>Gets the current speaking state of the <see cref="T:System.Speech.Synthesis.SpeechSynthesizer" /> object.</summary>
		/// <returns>Returns the current speaking state of the <see cref="T:System.Speech.Synthesis.SpeechSynthesizer" /> object.</returns>
		public SynthesizerState State => VoiceSynthesizer.State;

		/// <summary>Gets or sets the speaking rate of the <see cref="T:System.Speech.Synthesis.SpeechSynthesizer" /> object.</summary>
		/// <returns>Returns the speaking rate of the <see cref="T:System.Speech.Synthesis.SpeechSynthesizer" /> object, from -10 through 10.</returns>
		public int Rate
		{
			get
			{
				return VoiceSynthesizer.Rate;
			}
			set
			{
				if (value < -10 || value > 10)
				{
					throw new ArgumentOutOfRangeException("value", SR.Get(SRID.RateOutOfRange));
				}
				VoiceSynthesizer.Rate = value;
			}
		}

		/// <summary>Get or sets the output volume of the <see cref="T:System.Speech.Synthesis.SpeechSynthesizer" /> object.</summary>
		/// <returns>Returns the volume of the <see cref="T:System.Speech.Synthesis.SpeechSynthesizer" />, from 0 through 100.</returns>
		public int Volume
		{
			get
			{
				return VoiceSynthesizer.Volume;
			}
			set
			{
				if (value < 0 || value > 100)
				{
					throw new ArgumentOutOfRangeException("value", SR.Get(SRID.ResourceUsageOutOfRange));
				}
				VoiceSynthesizer.Volume = value;
			}
		}

		/// <summary>Gets information about the current voice of the <see cref="T:System.Speech.Synthesis.SpeechSynthesizer" /> object.</summary>
		/// <returns>Returns information about the current voice of the <see cref="T:System.Speech.Synthesis.SpeechSynthesizer" /> object.</returns>
		public VoiceInfo Voice => VoiceSynthesizer.CurrentVoice(switchContext: true).VoiceInfo;

		private VoiceSynthesis VoiceSynthesizer
		{
			get
			{
				if (_voiceSynthesis == null && _isDisposed)
				{
					throw new ObjectDisposedException("SpeechSynthesizer");
				}
				if (_voiceSynthesis == null)
				{
					WeakReference speechSynthesizer = new WeakReference(this);
					_voiceSynthesis = new VoiceSynthesis(speechSynthesizer);
				}
				return _voiceSynthesis;
			}
		}

		/// <summary>Raised when the <see cref="T:System.Speech.Synthesis.SpeechSynthesizer" /> begins the speaking of a prompt.</summary>
		public event EventHandler<SpeakStartedEventArgs> SpeakStarted
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				Helpers.ThrowIfNull(value, "value");
				VoiceSynthesis voiceSynthesizer = VoiceSynthesizer;
				voiceSynthesizer._speakStarted = (EventHandler<SpeakStartedEventArgs>)Delegate.Combine(voiceSynthesizer._speakStarted, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				Helpers.ThrowIfNull(value, "value");
				VoiceSynthesis voiceSynthesizer = VoiceSynthesizer;
				voiceSynthesizer._speakStarted = (EventHandler<SpeakStartedEventArgs>)Delegate.Remove(voiceSynthesizer._speakStarted, value);
			}
		}

		/// <summary>Raised when the <see cref="T:System.Speech.Synthesis.SpeechSynthesizer" /> completes the speaking of a prompt.</summary>
		public event EventHandler<SpeakCompletedEventArgs> SpeakCompleted
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				Helpers.ThrowIfNull(value, "value");
				VoiceSynthesis voiceSynthesizer = VoiceSynthesizer;
				voiceSynthesizer._speakCompleted = (EventHandler<SpeakCompletedEventArgs>)Delegate.Combine(voiceSynthesizer._speakCompleted, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				Helpers.ThrowIfNull(value, "value");
				VoiceSynthesis voiceSynthesizer = VoiceSynthesizer;
				voiceSynthesizer._speakCompleted = (EventHandler<SpeakCompletedEventArgs>)Delegate.Remove(voiceSynthesizer._speakCompleted, value);
			}
		}

		/// <summary>Raised after the <see cref="T:System.Speech.Synthesis.SpeechSynthesizer" /> speaks each individual word of a prompt.</summary>
		public event EventHandler<SpeakProgressEventArgs> SpeakProgress
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				Helpers.ThrowIfNull(value, "value");
				VoiceSynthesizer.AddEvent(TtsEventId.WordBoundary, ref VoiceSynthesizer._speakProgress, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				Helpers.ThrowIfNull(value, "value");
				VoiceSynthesizer.RemoveEvent(TtsEventId.WordBoundary, ref VoiceSynthesizer._speakProgress, value);
			}
		}

		/// <summary>Raised when the <see cref="T:System.Speech.Synthesis.SpeechSynthesizer" /> encounters a bookmark in a prompt.</summary>
		public event EventHandler<BookmarkReachedEventArgs> BookmarkReached
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				Helpers.ThrowIfNull(value, "value");
				VoiceSynthesizer.AddEvent(TtsEventId.Bookmark, ref VoiceSynthesizer._bookmarkReached, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				Helpers.ThrowIfNull(value, "value");
				VoiceSynthesizer.RemoveEvent(TtsEventId.Bookmark, ref VoiceSynthesizer._bookmarkReached, value);
			}
		}

		/// <summary>Raised when the voice of the <see cref="T:System.Speech.Synthesis.SpeechSynthesizer" /> changes.</summary>
		public event EventHandler<VoiceChangeEventArgs> VoiceChange
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				Helpers.ThrowIfNull(value, "value");
				VoiceSynthesizer.AddEvent(TtsEventId.VoiceChange, ref VoiceSynthesizer._voiceChange, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				Helpers.ThrowIfNull(value, "value");
				VoiceSynthesizer.RemoveEvent(TtsEventId.VoiceChange, ref VoiceSynthesizer._voiceChange, value);
			}
		}

		/// <summary>Raised when a phoneme is reached.</summary>
		public event EventHandler<PhonemeReachedEventArgs> PhonemeReached
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				Helpers.ThrowIfNull(value, "value");
				VoiceSynthesizer.AddEvent(TtsEventId.Phoneme, ref VoiceSynthesizer._phonemeReached, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				Helpers.ThrowIfNull(value, "value");
				VoiceSynthesizer.RemoveEvent(TtsEventId.Phoneme, ref VoiceSynthesizer._phonemeReached, value);
			}
		}

		/// <summary>Raised when a viseme is reached.</summary>
		public event EventHandler<VisemeReachedEventArgs> VisemeReached
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				Helpers.ThrowIfNull(value, "value");
				VoiceSynthesizer.AddEvent(TtsEventId.Viseme, ref VoiceSynthesizer._visemeReached, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				Helpers.ThrowIfNull(value, "value");
				VoiceSynthesizer.RemoveEvent(TtsEventId.Viseme, ref VoiceSynthesizer._visemeReached, value);
			}
		}

		/// <summary>Raised when the state of the <see cref="T:System.Speech.Synthesis.SpeechSynthesizer" /> changes.</summary>
		public event EventHandler<StateChangedEventArgs> StateChanged
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				Helpers.ThrowIfNull(value, "value");
				VoiceSynthesis voiceSynthesizer = VoiceSynthesizer;
				voiceSynthesizer._stateChanged = (EventHandler<StateChangedEventArgs>)Delegate.Combine(voiceSynthesizer._stateChanged, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				Helpers.ThrowIfNull(value, "value");
				VoiceSynthesis voiceSynthesizer = VoiceSynthesizer;
				voiceSynthesizer._stateChanged = (EventHandler<StateChangedEventArgs>)Delegate.Remove(voiceSynthesizer._stateChanged, value);
			}
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.Speech.Synthesis.SpeechSynthesizer" /> class.</summary>
		public SpeechSynthesizer()
		{
		}

		/// <summary>Acts as a safeguard to clean up resources in the event that the <see cref="M:System.Speech.Synthesis.SpeechSynthesizer.Dispose" /> method is not called.</summary>
		~SpeechSynthesizer()
		{
			Dispose(disposing: false);
		}

		/// <summary>Disposes the <see cref="T:System.Speech.Synthesis.SpeechSynthesizer" /> object and releases resources used during the session.</summary>
		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		/// <summary>Selects a specific voice by name.</summary>
		/// <param name="name">The name of the voice to select.</param>
		public void SelectVoice(string name)
		{
			Helpers.ThrowIfEmptyOrNull(name, "name");
			TTSVoice engine = VoiceSynthesizer.GetEngine(name, CultureInfo.CurrentUICulture, VoiceGender.NotSet, VoiceAge.NotSet, 1, switchContext: true);
			if (engine == null || name != engine.VoiceInfo.Name)
			{
				throw new ArgumentException(SR.Get(SRID.SynthesizerSetVoiceNoMatch));
			}
			VoiceSynthesizer.Voice = engine;
		}

		/// <summary>Selects a voice with a specific gender.</summary>
		/// <param name="gender">The gender of the voice to select.</param>
		public void SelectVoiceByHints(VoiceGender gender)
		{
			SelectVoiceByHints(gender, VoiceAge.NotSet, 1, CultureInfo.CurrentUICulture);
		}

		/// <summary>Selects a voice with a specific gender and age.</summary>
		/// <param name="gender">The gender of the voice to select.</param>
		/// <param name="age">The age of the voice to select.</param>
		public void SelectVoiceByHints(VoiceGender gender, VoiceAge age)
		{
			SelectVoiceByHints(gender, age, 1, CultureInfo.CurrentUICulture);
		}

		/// <summary>Selects a voice with a specific gender and age, based on the position in which the voices are ordered.</summary>
		/// <param name="gender">The gender of the voice to select.</param>
		/// <param name="age">The age of the voice to select.</param>
		/// <param name="voiceAlternate">The position of the voice to select.</param>
		public void SelectVoiceByHints(VoiceGender gender, VoiceAge age, int voiceAlternate)
		{
			SelectVoiceByHints(gender, age, voiceAlternate, CultureInfo.CurrentUICulture);
		}

		/// <summary>Selects a voice with a specific gender, age, and locale, based on the position in which the voices are ordered.</summary>
		/// <param name="gender">The gender of the voice to select.</param>
		/// <param name="age">The age of the voice to select.</param>
		/// <param name="voiceAlternate">The position of the voice to select.</param>
		/// <param name="culture">The locale of the voice to select.</param>
		public void SelectVoiceByHints(VoiceGender gender, VoiceAge age, int voiceAlternate, CultureInfo culture)
		{
			Helpers.ThrowIfNull(culture, "culture");
			if (voiceAlternate < 0)
			{
				throw new ArgumentOutOfRangeException("voiceAlternate", SR.Get(SRID.PromptBuilderInvalidVariant));
			}
			if (!VoiceInfo.ValidateGender(gender))
			{
				throw new ArgumentException(SR.Get(SRID.EnumInvalid, "VoiceGender"), "gender");
			}
			if (!VoiceInfo.ValidateAge(age))
			{
				throw new ArgumentException(SR.Get(SRID.EnumInvalid, "VoiceAge"), "age");
			}
			TTSVoice engine = VoiceSynthesizer.GetEngine(null, culture, gender, age, voiceAlternate, switchContext: true);
			if (engine == null)
			{
				throw new InvalidOperationException(SR.Get(SRID.SynthesizerSetVoiceNoMatch));
			}
			VoiceSynthesizer.Voice = engine;
		}

		/// <summary>Asynchronously speaks the contents of a string.</summary>
		/// <param name="textToSpeak">The text to speak.</param>
		/// <returns>Returns the object that contains the content to speak.</returns>
		public Prompt SpeakAsync(string textToSpeak)
		{
			Helpers.ThrowIfNull(textToSpeak, "textToSpeak");
			Prompt prompt = new Prompt(textToSpeak, SynthesisTextFormat.Text);
			SpeakAsync(prompt);
			return prompt;
		}

		/// <summary>Asynchronously speaks the contents of a <see cref="T:System.Speech.Synthesis.Prompt" /> object.</summary>
		/// <param name="prompt">The content to speak.</param>
		public void SpeakAsync(Prompt prompt)
		{
			Helpers.ThrowIfNull(prompt, "prompt");
			prompt.Synthesizer = this;
			VoiceSynthesizer.SpeakAsync(prompt);
		}

		/// <summary>Asynchronously speaks a <see cref="T:System.String" /> that contains SSML markup.</summary>
		/// <param name="textToSpeak">The SMML markup to speak.</param>
		public Prompt SpeakSsmlAsync(string textToSpeak)
		{
			Helpers.ThrowIfNull(textToSpeak, "textToSpeak");
			Prompt prompt = new Prompt(textToSpeak, SynthesisTextFormat.Ssml);
			SpeakAsync(prompt);
			return prompt;
		}

		/// <summary>Asynchronously speaks the contents of a <see cref="T:System.Speech.Synthesis.PromptBuilder" /> object.</summary>
		/// <param name="promptBuilder">The content to speak.</param>
		/// <returns>Returns the object that contains the content to speak.</returns>
		public Prompt SpeakAsync(PromptBuilder promptBuilder)
		{
			Helpers.ThrowIfNull(promptBuilder, "promptBuilder");
			Prompt prompt = new Prompt(promptBuilder);
			SpeakAsync(prompt);
			return prompt;
		}

		/// <summary>Synchronously speaks the contents of a string.</summary>
		/// <param name="textToSpeak">The text to speak.</param>
		public void Speak(string textToSpeak)
		{
			Speak(new Prompt(textToSpeak, SynthesisTextFormat.Text));
		}

		/// <summary>Synchronously speaks the contents of a <see cref="T:System.Speech.Synthesis.Prompt" /> object.</summary>
		/// <param name="prompt">The content to speak.</param>
		public void Speak(Prompt prompt)
		{
			Helpers.ThrowIfNull(prompt, "prompt");
			if (State == SynthesizerState.Paused)
			{
				throw new InvalidOperationException(SR.Get(SRID.SynthesizerSyncSpeakWhilePaused));
			}
			prompt.Synthesizer = this;
			prompt._syncSpeak = true;
			VoiceSynthesizer.Speak(prompt);
		}

		/// <summary>Synchronously speaks the contents of a <see cref="T:System.Speech.Synthesis.PromptBuilder" /> object.</summary>
		/// <param name="promptBuilder">The content to speak.</param>
		public void Speak(PromptBuilder promptBuilder)
		{
			Speak(new Prompt(promptBuilder));
		}

		/// <summary>Synchronously speaks a <see cref="T:System.String" /> that contains SSML markup.</summary>
		/// <param name="textToSpeak">The SSML string to speak.</param>
		public void SpeakSsml(string textToSpeak)
		{
			Speak(new Prompt(textToSpeak, SynthesisTextFormat.Ssml));
		}

		/// <summary>Pauses the <see cref="T:System.Speech.Synthesis.SpeechSynthesizer" /> object.</summary>
		public void Pause()
		{
			if (!paused)
			{
				VoiceSynthesizer.Pause();
				paused = true;
			}
		}

		/// <summary>Resumes the <see cref="T:System.Speech.Synthesis.SpeechSynthesizer" /> object after it has been paused.</summary>
		public void Resume()
		{
			if (paused)
			{
				VoiceSynthesizer.Resume();
				paused = false;
			}
		}

		/// <summary>Cancels the asynchronous synthesis operation for a queued prompt.</summary>
		/// <param name="prompt">The content for which to cancel a speak operation.</param>
		public void SpeakAsyncCancel(Prompt prompt)
		{
			Helpers.ThrowIfNull(prompt, "prompt");
			VoiceSynthesizer.Abort(prompt);
		}

		/// <summary>Cancels all queued, asynchronous, speech synthesis operations.</summary>
		public void SpeakAsyncCancelAll()
		{
			VoiceSynthesizer.Abort();
		}

		/// <summary>Configures the <see cref="T:System.Speech.Synthesis.SpeechSynthesizer" /> object to append output to a file that contains Waveform format audio.</summary>
		/// <param name="path">The path to the file.</param>
		public void SetOutputToWaveFile(string path)
		{
			Helpers.ThrowIfEmptyOrNull(path, "path");
			SetOutputToNull();
			SetOutputStream(new FileStream(path, FileMode.Create, FileAccess.Write), null, headerInfo: true, closeStreamOnExit: true);
		}

		/// <summary>Configures the <see cref="T:System.Speech.Synthesis.SpeechSynthesizer" /> object to append output to a Waveform audio format file in a specified format.</summary>
		/// <param name="path">The path to the file.</param>
		/// <param name="formatInfo">The audio format information.</param>
		public void SetOutputToWaveFile(string path, SpeechAudioFormatInfo formatInfo)
		{
			Helpers.ThrowIfEmptyOrNull(path, "path");
			Helpers.ThrowIfNull(formatInfo, "formatInfo");
			SetOutputToNull();
			SetOutputStream(new FileStream(path, FileMode.Create, FileAccess.Write), formatInfo, headerInfo: true, closeStreamOnExit: true);
		}

		/// <summary>Configures the <see cref="T:System.Speech.Synthesis.SpeechSynthesizer" /> object to append output to a stream that contains Waveform format audio.</summary>
		/// <param name="audioDestination">The stream to which to append synthesis output.</param>
		public void SetOutputToWaveStream(Stream audioDestination)
		{
			Helpers.ThrowIfNull(audioDestination, "audioDestination");
			SetOutputStream(audioDestination, null, headerInfo: true, closeStreamOnExit: false);
		}

		/// <summary>Configures the <see cref="T:System.Speech.Synthesis.SpeechSynthesizer" /> object to append output to an audio stream.</summary>
		/// <param name="audioDestination">The stream to which to append synthesis output.</param>
		/// <param name="formatInfo">The format to use for the synthesis output.</param>
		public void SetOutputToAudioStream(Stream audioDestination, SpeechAudioFormatInfo formatInfo)
		{
			Helpers.ThrowIfNull(audioDestination, "audioDestination");
			Helpers.ThrowIfNull(formatInfo, "formatInfo");
			SetOutputStream(audioDestination, formatInfo, headerInfo: false, closeStreamOnExit: false);
		}

		/// <summary>Configures the <see cref="T:System.Speech.Synthesis.SpeechSynthesizer" /> object to send output to the default audio device.</summary>
		public void SetOutputToDefaultAudioDevice()
		{
			SetOutputStream(null, null, headerInfo: true, closeStreamOnExit: false);
		}

		/// <summary>Configures the <see cref="T:System.Speech.Synthesis.SpeechSynthesizer" /> object to not send output from synthesis operations to a device, file, or stream.</summary>
		public void SetOutputToNull()
		{
			if (_outputStream != Stream.Null)
			{
				VoiceSynthesizer.SetOutput(Stream.Null, null, headerInfo: true);
			}
			if (_outputStream != null && _closeStreamOnExit)
			{
				_outputStream.Close();
			}
			_outputStream = Stream.Null;
		}

		/// <summary>Gets the prompt that the <see cref="T:System.Speech.Synthesis.SpeechSynthesizer" /> is speaking.</summary>
		/// <returns>Returns the prompt object that is currently being spoken.</returns>
		public Prompt GetCurrentlySpokenPrompt()
		{
			return VoiceSynthesizer.Prompt;
		}

		/// <summary>Returns all of the installed speech synthesis (text-to-speech) voices.</summary>
		/// <returns>Returns a read-only collection of the voices currently installed on the system.</returns>
		public ReadOnlyCollection<InstalledVoice> GetInstalledVoices()
		{
			return VoiceSynthesizer.GetInstalledVoices(null);
		}

		/// <summary>Returns all of the installed speech synthesis  (text-to-speech) voices that support a specific locale.</summary>
		/// <param name="culture">The locale that the voice must support.</param>
		/// <returns>Returns a read-only collection of the voices currently installed on the system that support the specified locale.</returns>
		public ReadOnlyCollection<InstalledVoice> GetInstalledVoices(CultureInfo culture)
		{
			Helpers.ThrowIfNull(culture, "culture");
			if (culture.Equals(CultureInfo.InvariantCulture))
			{
				throw new ArgumentException(SR.Get(SRID.InvariantCultureInfo), "culture");
			}
			return VoiceSynthesizer.GetInstalledVoices(culture);
		}

		/// <summary>Adds a lexicon to the <see cref="T:System.Speech.Synthesis.SpeechSynthesizer" /> object.</summary>
		/// <param name="uri">The location of the lexicon information.</param>
		/// <param name="mediaType">The media type of the lexicon. Media type values are not case sensitive.</param>
		public void AddLexicon(Uri uri, string mediaType)
		{
			Helpers.ThrowIfNull(uri, "uri");
			VoiceSynthesizer.AddLexicon(uri, mediaType);
		}

		/// <summary>Removes a lexicon from the <see cref="T:System.Speech.Synthesis.SpeechSynthesizer" /> object.</summary>
		/// <param name="uri">The location of the lexicon document.</param>
		public void RemoveLexicon(Uri uri)
		{
			Helpers.ThrowIfNull(uri, "uri");
			VoiceSynthesizer.RemoveLexicon(uri);
		}

		private void SetOutputStream(Stream stream, SpeechAudioFormatInfo formatInfo, bool headerInfo, bool closeStreamOnExit)
		{
			SetOutputToNull();
			_outputStream = stream;
			_closeStreamOnExit = closeStreamOnExit;
			VoiceSynthesizer.SetOutput(stream, formatInfo, headerInfo);
		}

		private void Dispose(bool disposing)
		{
			if (!_isDisposed && disposing && _voiceSynthesis != null)
			{
				_isDisposed = true;
				SpeakAsyncCancelAll();
				if (_outputStream != null)
				{
					if (_closeStreamOnExit)
					{
						_outputStream.Close();
					}
					else
					{
						_outputStream.Flush();
					}
					_outputStream = null;
				}
			}
			if (_voiceSynthesis != null)
			{
				_voiceSynthesis.Dispose();
				_voiceSynthesis = null;
			}
			_isDisposed = true;
		}
	}
}
