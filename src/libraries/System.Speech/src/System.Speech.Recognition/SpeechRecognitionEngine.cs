// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Speech.AudioFormat;
using System.Speech.Internal;
using System.Speech.Internal.ObjectTokens;
using System.Speech.Internal.SapiInterop;

namespace System.Speech.Recognition
{
    /// <summary>Provides the means to access and manage an in-process speech recognition engine.</summary>
    public class SpeechRecognitionEngine : IDisposable
    {
        private bool _disposed;

        private RecognizerBase _recognizerBase;

        private SapiRecognizer _sapiRecognizer;

        private EventHandler<AudioSignalProblemOccurredEventArgs> _audioSignalProblemOccurredDelegate;

        private EventHandler<AudioLevelUpdatedEventArgs> _audioLevelUpdatedDelegate;

        private EventHandler<AudioStateChangedEventArgs> _audioStateChangedDelegate;

        private EventHandler<SpeechHypothesizedEventArgs> _speechHypothesizedDelegate;

        /// <summary>Gets or sets the time interval during which a <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> accepts input containing only silence before finalizing recognition.</summary>
        /// <returns>The duration of the interval of silence.</returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">This property is set to less than 0 seconds.</exception>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public TimeSpan InitialSilenceTimeout
        {
            get
            {
                return RecoBase.InitialSilenceTimeout;
            }
            set
            {
                RecoBase.InitialSilenceTimeout = value;
            }
        }

        /// <summary>Gets or sets the time interval during which a <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> accepts input containing only background noise, before finalizing recognition.</summary>
        /// <returns>The duration of the time interval.</returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">This property is set to less than 0 seconds.</exception>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public TimeSpan BabbleTimeout
        {
            get
            {
                return RecoBase.BabbleTimeout;
            }
            set
            {
                RecoBase.BabbleTimeout = value;
            }
        }

        /// <summary>Gets or sets the interval of silence that the <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> will accept at the end of unambiguous input before finalizing a recognition operation.</summary>
        /// <returns>The duration of the interval of silence.</returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">This property is set to less than 0 seconds or greater than 10 seconds.</exception>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public TimeSpan EndSilenceTimeout
        {
            get
            {
                return TimeSpan.FromMilliseconds(RecoBase.QueryRecognizerSettingAsInt("ResponseSpeed"));
            }
            set
            {
                if (value.TotalMilliseconds < 0.0 || value.TotalMilliseconds > 10000.0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), SR.Get(SRID.EndSilenceOutOfRange));
                }
                RecoBase.UpdateRecognizerSetting("ResponseSpeed", (int)value.TotalMilliseconds);
            }
        }

        /// <summary>Gets or sets the interval of silence that the <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> will accept at the end of ambiguous input before finalizing a recognition operation.</summary>
        /// <returns>The duration of the interval of silence.</returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">This property is set to less than 0 seconds or greater than 10 seconds.</exception>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public TimeSpan EndSilenceTimeoutAmbiguous
        {
            get
            {
                return TimeSpan.FromMilliseconds(RecoBase.QueryRecognizerSettingAsInt("ComplexResponseSpeed"));
            }
            set
            {
                if (value.TotalMilliseconds < 0.0 || value.TotalMilliseconds > 10000.0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), SR.Get(SRID.EndSilenceOutOfRange));
                }
                RecoBase.UpdateRecognizerSetting("ComplexResponseSpeed", (int)value.TotalMilliseconds);
            }
        }

        /// <summary>Gets a collection of the <see cref="T:System.Speech.Recognition.Grammar" /> objects that are loaded in this <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> instance.</summary>
        /// <returns>The collection of <see cref="T:System.Speech.Recognition.Grammar" /> objects.</returns>
        public ReadOnlyCollection<Grammar> Grammars => RecoBase.Grammars;

        /// <summary>Gets information about the current instance of <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" />.</summary>
        /// <returns>Information about the current speech recognizer.</returns>
        public RecognizerInfo RecognizerInfo => RecoBase.RecognizerInfo;

        /// <summary>Gets the state of the audio being received by the <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" />.</summary>
        /// <returns>The state of the audio input to the speech recognizer.</returns>
        public AudioState AudioState => RecoBase.AudioState;

        /// <summary>Gets the level of the audio being received by the <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" />.</summary>
        /// <returns>The audio level of the input to the speech recognizer, from 0 through 100.</returns>
        public int AudioLevel => RecoBase.AudioLevel;

        /// <summary>Gets the current location of the <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> in the audio input that it is processing.</summary>
        /// <returns>The position of the recognizer in the audio input that it is processing.</returns>
        public TimeSpan RecognizerAudioPosition => RecoBase.RecognizerAudioPosition;

        /// <summary>Gets the current location in the audio stream being generated by the device that is providing input to the <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" />.</summary>
        /// <returns>The current location in the audio stream being generated by the input device.</returns>
        public TimeSpan AudioPosition => RecoBase.AudioPosition;

        /// <summary>Gets the format of the audio being received by the <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" />.</summary>
        /// <returns>The format of audio at the input to the <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> instance, or <see langword="null" /> if the input is not configured or set to the null input.</returns>
        public SpeechAudioFormatInfo AudioFormat => RecoBase.AudioFormat;

        /// <summary>Gets or sets the maximum number of alternate recognition results that the <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> returns for each recognition operation.</summary>
        /// <returns>The number of alternate results to return.</returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///   <see cref="P:System.Speech.Recognition.SpeechRecognitionEngine.MaxAlternates" /> is set to a value less than 0.</exception>
        public int MaxAlternates
        {
            get
            {
                return RecoBase.MaxAlternates;
            }
            set
            {
                RecoBase.MaxAlternates = value;
            }
        }

        private RecognizerBase RecoBase
        {
            get
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException("SpeechRecognitionEngine");
                }
                if (_recognizerBase == null)
                {
                    _recognizerBase = new RecognizerBase();
                    _recognizerBase.Initialize(_sapiRecognizer, inproc: true);
                    _recognizerBase.RecognizeCompleted += RecognizeCompletedProxy;
                    _recognizerBase.EmulateRecognizeCompleted += EmulateRecognizeCompletedProxy;
                    _recognizerBase.LoadGrammarCompleted += LoadGrammarCompletedProxy;
                    _recognizerBase.SpeechDetected += SpeechDetectedProxy;
                    _recognizerBase.SpeechRecognized += SpeechRecognizedProxy;
                    _recognizerBase.SpeechRecognitionRejected += SpeechRecognitionRejectedProxy;
                    _recognizerBase.RecognizerUpdateReached += RecognizerUpdateReachedProxy;
                }
                return _recognizerBase;
            }
        }

        /// <summary>Raised when the <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> finalizes an asynchronous recognition operation.</summary>
        public event EventHandler<RecognizeCompletedEventArgs> RecognizeCompleted;

        /// <summary>Raised when the <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> finalizes an asynchronous recognition operation of emulated input.</summary>
        public event EventHandler<EmulateRecognizeCompletedEventArgs> EmulateRecognizeCompleted;

        /// <summary>Raised when the <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> finishes the asynchronous loading of a <see cref="T:System.Speech.Recognition.Grammar" /> object.</summary>
        public event EventHandler<LoadGrammarCompletedEventArgs> LoadGrammarCompleted;

        /// <summary>Raised when the <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> detects input that it can identify as speech.</summary>
        public event EventHandler<SpeechDetectedEventArgs> SpeechDetected;

        /// <summary>Raised when the <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> receives input that matches any of its loaded and enabled <see cref="T:System.Speech.Recognition.Grammar" /> objects.</summary>
        public event EventHandler<SpeechRecognizedEventArgs> SpeechRecognized;

        /// <summary>Raised when the <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> receives input that does not match any of its loaded and enabled <see cref="T:System.Speech.Recognition.Grammar" /> objects.</summary>
        public event EventHandler<SpeechRecognitionRejectedEventArgs> SpeechRecognitionRejected;

        /// <summary>Raised when a running <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> pauses to accept modifications.</summary>
        public event EventHandler<RecognizerUpdateReachedEventArgs> RecognizerUpdateReached;

        /// <summary>Raised when the <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> has recognized a word or words that may be a component of multiple complete phrases in a grammar.</summary>
        public event EventHandler<SpeechHypothesizedEventArgs> SpeechHypothesized
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            add
            {
                Helpers.ThrowIfNull(value, nameof(value));
                if (_speechHypothesizedDelegate == null)
                {
                    RecoBase.SpeechHypothesized += SpeechHypothesizedProxy;
                }
                _speechHypothesizedDelegate = (EventHandler<SpeechHypothesizedEventArgs>)Delegate.Combine(_speechHypothesizedDelegate, value);
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            remove
            {
                Helpers.ThrowIfNull(value, nameof(value));
                _speechHypothesizedDelegate = (EventHandler<SpeechHypothesizedEventArgs>)Delegate.Remove(_speechHypothesizedDelegate, value);
                if (_speechHypothesizedDelegate == null)
                {
                    RecoBase.SpeechHypothesized -= SpeechHypothesizedProxy;
                }
            }
        }

        /// <summary>Raised when the <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> detects a problem in the audio signal.</summary>
        public event EventHandler<AudioSignalProblemOccurredEventArgs> AudioSignalProblemOccurred
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            add
            {
                Helpers.ThrowIfNull(value, nameof(value));
                if (_audioSignalProblemOccurredDelegate == null)
                {
                    RecoBase.AudioSignalProblemOccurred += AudioSignalProblemOccurredProxy;
                }
                _audioSignalProblemOccurredDelegate = (EventHandler<AudioSignalProblemOccurredEventArgs>)Delegate.Combine(_audioSignalProblemOccurredDelegate, value);
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            remove
            {
                Helpers.ThrowIfNull(value, nameof(value));
                _audioSignalProblemOccurredDelegate = (EventHandler<AudioSignalProblemOccurredEventArgs>)Delegate.Remove(_audioSignalProblemOccurredDelegate, value);
                if (_audioSignalProblemOccurredDelegate == null)
                {
                    RecoBase.AudioSignalProblemOccurred -= AudioSignalProblemOccurredProxy;
                }
            }
        }

        /// <summary>Raised when the <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> reports the level of its audio input.</summary>
        public event EventHandler<AudioLevelUpdatedEventArgs> AudioLevelUpdated
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            add
            {
                Helpers.ThrowIfNull(value, nameof(value));
                if (_audioLevelUpdatedDelegate == null)
                {
                    RecoBase.AudioLevelUpdated += AudioLevelUpdatedProxy;
                }
                _audioLevelUpdatedDelegate = (EventHandler<AudioLevelUpdatedEventArgs>)Delegate.Combine(_audioLevelUpdatedDelegate, value);
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            remove
            {
                Helpers.ThrowIfNull(value, nameof(value));
                _audioLevelUpdatedDelegate = (EventHandler<AudioLevelUpdatedEventArgs>)Delegate.Remove(_audioLevelUpdatedDelegate, value);
                if (_audioLevelUpdatedDelegate == null)
                {
                    RecoBase.AudioLevelUpdated -= AudioLevelUpdatedProxy;
                }
            }
        }

        /// <summary>Raised when the state changes in the audio being received by the <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" />.</summary>
        public event EventHandler<AudioStateChangedEventArgs> AudioStateChanged
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            add
            {
                Helpers.ThrowIfNull(value, nameof(value));
                if (_audioStateChangedDelegate == null)
                {
                    RecoBase.AudioStateChanged += AudioStateChangedProxy;
                }
                _audioStateChangedDelegate = (EventHandler<AudioStateChangedEventArgs>)Delegate.Combine(_audioStateChangedDelegate, value);
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            remove
            {
                Helpers.ThrowIfNull(value, nameof(value));
                _audioStateChangedDelegate = (EventHandler<AudioStateChangedEventArgs>)Delegate.Remove(_audioStateChangedDelegate, value);
                if (_audioStateChangedDelegate == null)
                {
                    RecoBase.AudioStateChanged -= AudioStateChangedProxy;
                }
            }
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> class using the default speech recognizer for the system.</summary>
        public SpeechRecognitionEngine()
        {
            Initialize(null);
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> class using the default speech recognizer for a specified locale.</summary>
        /// <param name="culture">The locale that the speech recognizer must support.</param>
        /// <exception cref="T:System.ArgumentException">None of the installed speech recognizers support the specified locale, or <paramref name="culture" /> is the invariant culture.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="Culture" /> is <see langword="null" />.</exception>
        public SpeechRecognitionEngine(CultureInfo culture)
        {
            Helpers.ThrowIfNull(culture, nameof(culture));
            if (culture.Equals(CultureInfo.InvariantCulture))
            {
                throw new ArgumentException(SR.Get(SRID.InvariantCultureInfo), nameof(culture));
            }
            foreach (RecognizerInfo item in InstalledRecognizers())
            {
                if (culture.Equals(item.Culture))
                {
                    Initialize(item);
                    return;
                }
            }
            foreach (RecognizerInfo item2 in InstalledRecognizers())
            {
                if (Helpers.CompareInvariantCulture(item2.Culture, culture))
                {
                    Initialize(item2);
                    return;
                }
            }
            throw new ArgumentException(SR.Get(SRID.RecognizerNotFound), nameof(culture));
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> class with a string parameter that specifies the name of the recognizer to use.</summary>
        /// <param name="recognizerId">The token name of the speech recognizer to use.</param>
        /// <exception cref="T:System.ArgumentException">No speech recognizer with that token name is installed, or <paramref name="recognizerId" /> is the empty string ("").</exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="recognizerId" /> is <see langword="null" />.</exception>
        public SpeechRecognitionEngine(string recognizerId)
        {
            Helpers.ThrowIfEmptyOrNull(recognizerId, nameof(recognizerId));
            foreach (RecognizerInfo item in InstalledRecognizers())
            {
                if (recognizerId.Equals(item.Id, StringComparison.OrdinalIgnoreCase))
                {
                    Initialize(item);
                    return;
                }
            }
            throw new ArgumentException(SR.Get(SRID.RecognizerNotFound), nameof(recognizerId));
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> using the information in a <see cref="T:System.Speech.Recognition.RecognizerInfo" /> object to specify the recognizer to use.</summary>
        /// <param name="recognizerInfo">The information for the specific speech recognizer.</param>
        public SpeechRecognitionEngine(RecognizerInfo recognizerInfo)
        {
            Helpers.ThrowIfNull(recognizerInfo, nameof(recognizerInfo));
            Initialize(recognizerInfo);
        }

        /// <summary>Disposes the <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> object.</summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Disposes the <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> object and releases resources used during the session.</summary>
        /// <param name="disposing">
        ///   <see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                if (_recognizerBase != null)
                {
                    _recognizerBase.Dispose();
                    _recognizerBase = null;
                }
                if (_sapiRecognizer != null)
                {
                    _sapiRecognizer.Dispose();
                    _sapiRecognizer = null;
                }
                _disposed = true;
            }
        }

        /// <summary>Returns information for all of the installed speech recognizers on the current system.</summary>
        /// <returns>A read-only collection of the <see cref="T:System.Speech.Recognition.RecognizerInfo" /> objects that describe the installed recognizers.</returns>
        public static ReadOnlyCollection<RecognizerInfo> InstalledRecognizers()
        {
            List<RecognizerInfo> list = new List<RecognizerInfo>();
            using (ObjectTokenCategory objectTokenCategory = ObjectTokenCategory.Create("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Speech\\Recognizers"))
            {
                if (objectTokenCategory != null)
                {
                    foreach (ObjectToken item in (IEnumerable<ObjectToken>)objectTokenCategory)
                    {
                        RecognizerInfo recognizerInfo = RecognizerInfo.Create(item);
                        if (recognizerInfo != null)
                        {
                            list.Add(recognizerInfo);
                        }
                    }
                }
            }
            return new ReadOnlyCollection<RecognizerInfo>(list);
        }

        /// <summary>Configures the <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> object to receive input from a Waveform audio format (.wav) file.</summary>
        /// <param name="path">The path of the file to use as input.</param>
        public void SetInputToWaveFile(string path)
        {
            Helpers.ThrowIfEmptyOrNull(path, nameof(path));
            RecoBase.SetInput(path);
        }

        /// <summary>Configures the <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> object to receive input from a stream that contains Waveform audio format (.wav) data.</summary>
        /// <param name="audioSource">The stream containing the audio data.</param>
        public void SetInputToWaveStream(Stream audioSource)
        {
            RecoBase.SetInput(audioSource, null);
        }

        /// <summary>Configures the <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> object to receive input from an audio stream.</summary>
        /// <param name="audioSource">The audio input stream.</param>
        /// <param name="audioFormat">The format of the audio input.</param>
        public void SetInputToAudioStream(Stream audioSource, SpeechAudioFormatInfo audioFormat)
        {
            Helpers.ThrowIfNull(audioSource, nameof(audioSource));
            Helpers.ThrowIfNull(audioFormat, nameof(audioFormat));
            RecoBase.SetInput(audioSource, audioFormat);
        }

        /// <summary>Disables the input to the speech recognizer.</summary>
        public void SetInputToNull()
        {
            RecoBase.SetInput(null, null);
        }

        /// <summary>Configures the <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> object to receive input from the default audio device.</summary>
        public void SetInputToDefaultAudioDevice()
        {
            RecoBase.SetInputToDefaultAudioDevice();
        }

        /// <summary>Performs a synchronous speech recognition operation.</summary>
        /// <returns>The recognition result for the input, or <see langword="null" /> if the operation is not successful or the recognizer is not enabled.</returns>
        public RecognitionResult Recognize()
        {
            return RecoBase.Recognize(RecoBase.InitialSilenceTimeout);
        }

        /// <summary>Performs a synchronous speech recognition operation with a specified initial silence timeout period.</summary>
        /// <param name="initialSilenceTimeout">The interval of time a speech recognizer accepts input containing only silence before finalizing recognition.</param>
        /// <returns>The recognition result for the input, or <see langword="null" /> if the operation is not successful or the recognizer is not enabled.</returns>
        public RecognitionResult Recognize(TimeSpan initialSilenceTimeout)
        {
            if (Grammars.Count == 0)
            {
                throw new InvalidOperationException(SR.Get(SRID.RecognizerHasNoGrammar));
            }
            return RecoBase.Recognize(initialSilenceTimeout);
        }

        /// <summary>Performs a single, asynchronous speech recognition operation.</summary>
        public void RecognizeAsync()
        {
            RecognizeAsync(RecognizeMode.Single);
        }

        /// <summary>Performs one or more asynchronous speech recognition operations.</summary>
        /// <param name="mode">Indicates whether to perform one or multiple recognition operations.</param>
        public void RecognizeAsync(RecognizeMode mode)
        {
            if (Grammars.Count == 0)
            {
                throw new InvalidOperationException(SR.Get(SRID.RecognizerHasNoGrammar));
            }
            RecoBase.RecognizeAsync(mode);
        }

        /// <summary>Terminates asynchronous recognition without waiting for the current recognition operation to complete.</summary>
        public void RecognizeAsyncCancel()
        {
            RecoBase.RecognizeAsyncCancel();
        }

        /// <summary>Stops asynchronous recognition after the current recognition operation completes.</summary>
        public void RecognizeAsyncStop()
        {
            RecoBase.RecognizeAsyncStop();
        }

        /// <summary>Returns the values of settings for the recognizer.</summary>
        /// <param name="settingName">The name of the setting to return.</param>
        /// <returns>The value of the setting.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="settingName" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="settingName" /> is the empty string ("").</exception>
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The recognizer does not have a setting by that name.</exception>
        public object QueryRecognizerSetting(string settingName)
        {
            return RecoBase.QueryRecognizerSetting(settingName);
        }

        /// <summary>Updates the specified speech recognition engine setting with the specified string value.</summary>
        /// <param name="settingName">The name of the setting to update.</param>
        /// <param name="updatedValue">The new value for the setting.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="settingName" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="settingName" /> is the empty string ("").</exception>
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The recognizer does not have a setting by that name.</exception>
        public void UpdateRecognizerSetting(string settingName, string updatedValue)
        {
            RecoBase.UpdateRecognizerSetting(settingName, updatedValue);
        }

        /// <summary>Updates the specified setting for the <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> with the specified integer value.</summary>
        /// <param name="settingName">The name of the setting to update.</param>
        /// <param name="updatedValue">The new value for the setting.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="settingName" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="settingName" /> is the empty string ("").</exception>
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The recognizer does not have a setting by that name.</exception>
        public void UpdateRecognizerSetting(string settingName, int updatedValue)
        {
            RecoBase.UpdateRecognizerSetting(settingName, updatedValue);
        }

        /// <summary>Synchronously loads a <see cref="T:System.Speech.Recognition.Grammar" /> object.</summary>
        /// <param name="grammar">The grammar object to load.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="Grammar" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        ///   <paramref name="Grammar" /> is not in a valid state.</exception>
        public void LoadGrammar(Grammar grammar)
        {
            RecoBase.LoadGrammar(grammar);
        }

        /// <summary>Asynchronously loads a speech recognition grammar.</summary>
        /// <param name="grammar">The speech recognition grammar to load.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="Grammar" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        ///   <paramref name="Grammar" /> is not in a valid state.</exception>
        /// <exception cref="T:System.OperationCanceledException">The asynchronous operation was canceled.</exception>
        public void LoadGrammarAsync(Grammar grammar)
        {
            RecoBase.LoadGrammarAsync(grammar);
        }

        /// <summary>Unloads a specified <see cref="T:System.Speech.Recognition.Grammar" /> object from the <see cref="T:System.Speech.Recognition.SpeechRecognitionEngine" /> instance.</summary>
        /// <param name="grammar">The grammar object to unload.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="Grammar" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.InvalidOperationException">The grammar is not loaded in this recognizer, or this recognizer is currently loading the grammar asynchronously.</exception>
        public void UnloadGrammar(Grammar grammar)
        {
            RecoBase.UnloadGrammar(grammar);
        }

        /// <summary>Unloads all <see cref="T:System.Speech.Recognition.Grammar" /> objects from the recognizer.</summary>
        public void UnloadAllGrammars()
        {
            RecoBase.UnloadAllGrammars();
        }

        /// <summary>Emulates input of a phrase to the speech recognizer, using text in place of audio for synchronous speech recognition.</summary>
        /// <param name="inputText">The input for the recognition operation.</param>
        /// <returns>The result for the recognition operation, or <see langword="null" /> if the operation is not successful or the recognizer is not enabled.</returns>
        /// <exception cref="T:System.InvalidOperationException">The recognizer has no speech recognition grammars loaded.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="inputText" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="inputText" /> is the empty string ("").</exception>
        public RecognitionResult EmulateRecognize(string inputText)
        {
            return EmulateRecognize(inputText, CompareOptions.IgnoreCase | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth);
        }

        /// <summary>Emulates input of a phrase to the speech recognizer, using text in place of audio for synchronous speech recognition, and specifies how the recognizer handles Unicode comparison between the phrase and the loaded speech recognition grammars.</summary>
        /// <param name="inputText">The input phrase for the recognition operation.</param>
        /// <param name="compareOptions">A bitwise combination of the enumeration values that describe the type of comparison to use for the emulated recognition operation.</param>
        /// <returns>The result for the recognition operation, or <see langword="null" /> if the operation is not successful or the recognizer is not enabled.</returns>
        /// <exception cref="T:System.InvalidOperationException">The recognizer has no speech recognition grammars loaded.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="inputText" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="inputText" /> is the empty string ("").</exception>
        /// <exception cref="T:System.NotSupportedException">
        ///   <paramref name="compareOptions" /> contains the <see cref="F:System.Globalization.CompareOptions.IgnoreNonSpace" />, <see cref="F:System.Globalization.CompareOptions.IgnoreSymbols" />, or <see cref="F:System.Globalization.CompareOptions.StringSort" /> flag.</exception>
        public RecognitionResult EmulateRecognize(string inputText, CompareOptions compareOptions)
        {
            if (Grammars.Count == 0)
            {
                throw new InvalidOperationException(SR.Get(SRID.RecognizerHasNoGrammar));
            }
            return RecoBase.EmulateRecognize(inputText, compareOptions);
        }

        /// <summary>Emulates input of specific words to the speech recognizer, using text in place of audio for synchronous speech recognition, and specifies how the recognizer handles Unicode comparison between the words and the loaded speech recognition grammars.</summary>
        /// <param name="wordUnits">An array of word units that contains the input for the recognition operation.</param>
        /// <param name="compareOptions">A bitwise combination of the enumeration values that describe the type of comparison to use for the emulated recognition operation.</param>
        /// <returns>The result for the recognition operation, or <see langword="null" /> if the operation is not successful or the recognizer is not enabled.</returns>
        /// <exception cref="T:System.InvalidOperationException">The recognizer has no speech recognition grammars loaded.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="wordUnits" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="wordUnits" /> contains one or more <see langword="null" /> elements.</exception>
        /// <exception cref="T:System.NotSupportedException">
        ///   <paramref name="compareOptions" /> contains the <see cref="F:System.Globalization.CompareOptions.IgnoreNonSpace" />, <see cref="F:System.Globalization.CompareOptions.IgnoreSymbols" />, or <see cref="F:System.Globalization.CompareOptions.StringSort" /> flag.</exception>
        public RecognitionResult EmulateRecognize(RecognizedWordUnit[] wordUnits, CompareOptions compareOptions)
        {
            if (Grammars.Count == 0)
            {
                throw new InvalidOperationException(SR.Get(SRID.RecognizerHasNoGrammar));
            }
            return RecoBase.EmulateRecognize(wordUnits, compareOptions);
        }

        /// <summary>Emulates input of a phrase to the speech recognizer, using text in place of audio for asynchronous speech recognition.</summary>
        /// <param name="inputText">The input for the recognition operation.</param>
        /// <exception cref="T:System.InvalidOperationException">The recognizer has no speech recognition grammars loaded, or the recognizer has an asynchronous recognition operation that is not yet complete.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="inputText" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="inputText" /> is the empty string ("").</exception>
        public void EmulateRecognizeAsync(string inputText)
        {
            EmulateRecognizeAsync(inputText, CompareOptions.IgnoreCase | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth);
        }

        /// <summary>Emulates input of a phrase to the speech recognizer, using text in place of audio for asynchronous speech recognition, and specifies how the recognizer handles Unicode comparison between the phrase and the loaded speech recognition grammars.</summary>
        /// <param name="inputText">The input phrase for the recognition operation.</param>
        /// <param name="compareOptions">A bitwise combination of the enumeration values that describe the type of comparison to use for the emulated recognition operation.</param>
        /// <exception cref="T:System.InvalidOperationException">The recognizer has no speech recognition grammars loaded, or the recognizer has an asynchronous recognition operation that is not yet complete.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="inputText" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="inputText" /> is the empty string ("").</exception>
        /// <exception cref="T:System.NotSupportedException">
        ///   <paramref name="compareOptions" /> contains the <see cref="F:System.Globalization.CompareOptions.IgnoreNonSpace" />, <see cref="F:System.Globalization.CompareOptions.IgnoreSymbols" />, or <see cref="F:System.Globalization.CompareOptions.StringSort" /> flag.</exception>
        public void EmulateRecognizeAsync(string inputText, CompareOptions compareOptions)
        {
            if (Grammars.Count == 0)
            {
                throw new InvalidOperationException(SR.Get(SRID.RecognizerHasNoGrammar));
            }
            RecoBase.EmulateRecognizeAsync(inputText, compareOptions);
        }

        /// <summary>Emulates input of specific words to the speech recognizer, using an array of <see cref="T:System.Speech.Recognition.RecognizedWordUnit" /> objects in place of audio for asynchronous speech recognition, and specifies how the recognizer handles Unicode comparison between the words and the loaded speech recognition grammars.</summary>
        /// <param name="wordUnits">An array of word units that contains the input for the recognition operation.</param>
        /// <param name="compareOptions">A bitwise combination of the enumeration values that describe the type of comparison to use for the emulated recognition operation.</param>
        /// <exception cref="T:System.InvalidOperationException">The recognizer has no speech recognition grammars loaded, or the recognizer has an asynchronous recognition operation that is not yet complete.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="wordUnits" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="wordUnits" /> contains one or more <see langword="null" /> elements.</exception>
        /// <exception cref="T:System.NotSupportedException">
        ///   <paramref name="compareOptions" /> contains the <see cref="F:System.Globalization.CompareOptions.IgnoreNonSpace" />, <see cref="F:System.Globalization.CompareOptions.IgnoreSymbols" />, or <see cref="F:System.Globalization.CompareOptions.StringSort" /> flag.</exception>
        public void EmulateRecognizeAsync(RecognizedWordUnit[] wordUnits, CompareOptions compareOptions)
        {
            if (Grammars.Count == 0)
            {
                throw new InvalidOperationException(SR.Get(SRID.RecognizerHasNoGrammar));
            }
            RecoBase.EmulateRecognizeAsync(wordUnits, compareOptions);
        }

        /// <summary>Requests that the recognizer pauses to update its state.</summary>
        public void RequestRecognizerUpdate()
        {
            RecoBase.RequestRecognizerUpdate();
        }

        /// <summary>Requests that the recognizer pauses to update its state and provides a user token for the associated event.</summary>
        /// <param name="userToken">User-defined information that contains information for the operation.</param>
        public void RequestRecognizerUpdate(object userToken)
        {
            RecoBase.RequestRecognizerUpdate(userToken);
        }

        /// <summary>Requests that the recognizer pauses to update its state and provides an offset and a user token for the associated event.</summary>
        /// <param name="userToken">User-defined information that contains information for the operation.</param>
        /// <param name="audioPositionAheadToRaiseUpdate">The offset from the current <see cref="P:System.Speech.Recognition.SpeechRecognitionEngine.AudioPosition" /> to delay the request.</param>
        public void RequestRecognizerUpdate(object userToken, TimeSpan audioPositionAheadToRaiseUpdate)
        {
            RecoBase.RequestRecognizerUpdate(userToken, audioPositionAheadToRaiseUpdate);
        }

        private void Initialize(RecognizerInfo recognizerInfo)
        {
            try
            {
                _sapiRecognizer = new SapiRecognizer(SapiRecognizer.RecognizerType.InProc);
            }
            catch (COMException e)
            {
                throw RecognizerBase.ExceptionFromSapiCreateRecognizerError(e);
            }
            if (recognizerInfo != null)
            {
                ObjectToken objectToken = recognizerInfo.GetObjectToken();
                if (objectToken == null)
                {
                    throw new ArgumentException(SR.Get(SRID.NullParamIllegal), nameof(recognizerInfo));
                }
                try
                {
                    _sapiRecognizer.SetRecognizer(objectToken.SAPIToken);
                }
                catch (COMException e2)
                {
                    throw new ArgumentException(SR.Get(SRID.RecognizerNotFound), RecognizerBase.ExceptionFromSapiCreateRecognizerError(e2));
                }
            }
            _sapiRecognizer.SetRecoState(SPRECOSTATE.SPRST_INACTIVE);
        }

        private void RecognizeCompletedProxy(object sender, RecognizeCompletedEventArgs e)
        {
            this.RecognizeCompleted?.Invoke(this, e);
        }

        private void EmulateRecognizeCompletedProxy(object sender, EmulateRecognizeCompletedEventArgs e)
        {
            this.EmulateRecognizeCompleted?.Invoke(this, e);
        }

        private void LoadGrammarCompletedProxy(object sender, LoadGrammarCompletedEventArgs e)
        {
            this.LoadGrammarCompleted?.Invoke(this, e);
        }

        private void SpeechDetectedProxy(object sender, SpeechDetectedEventArgs e)
        {
            this.SpeechDetected?.Invoke(this, e);
        }

        private void SpeechRecognizedProxy(object sender, SpeechRecognizedEventArgs e)
        {
            this.SpeechRecognized?.Invoke(this, e);
        }

        private void SpeechRecognitionRejectedProxy(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            this.SpeechRecognitionRejected?.Invoke(this, e);
        }

        private void RecognizerUpdateReachedProxy(object sender, RecognizerUpdateReachedEventArgs e)
        {
            this.RecognizerUpdateReached?.Invoke(this, e);
        }

        private void SpeechHypothesizedProxy(object sender, SpeechHypothesizedEventArgs e)
        {
            _speechHypothesizedDelegate?.Invoke(this, e);
        }

        private void AudioSignalProblemOccurredProxy(object sender, AudioSignalProblemOccurredEventArgs e)
        {
            _audioSignalProblemOccurredDelegate?.Invoke(this, e);
        }

        private void AudioLevelUpdatedProxy(object sender, AudioLevelUpdatedEventArgs e)
        {
            _audioLevelUpdatedDelegate?.Invoke(this, e);
        }

        private void AudioStateChangedProxy(object sender, AudioStateChangedEventArgs e)
        {
            _audioStateChangedDelegate?.Invoke(this, e);
        }
    }
}
