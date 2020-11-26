// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Speech.AudioFormat;
using System.Speech.Internal;
using System.Speech.Internal.SapiInterop;

namespace System.Speech.Recognition
{
    /// <summary>Provides access to the shared speech recognition service available on the Windows desktop.</summary>
    public class SpeechRecognizer : IDisposable
    {
        private bool _disposed;

        private RecognizerBase _recognizerBase;

        private SapiRecognizer _sapiRecognizer;

        private EventHandler<AudioSignalProblemOccurredEventArgs> _audioSignalProblemOccurredDelegate;

        private EventHandler<AudioLevelUpdatedEventArgs> _audioLevelUpdatedDelegate;

        private EventHandler<AudioStateChangedEventArgs> _audioStateChangedDelegate;

        private EventHandler<SpeechHypothesizedEventArgs> _speechHypothesizedDelegate;

        /// <summary>Gets the state of a <see cref="System.Speech.Recognition.SpeechRecognizer" /> object.</summary>
        /// <returns>The state of the <see langword="SpeechRecognizer" /> object.</returns>
        public RecognizerState State => RecoBase.State;

        /// <summary>Gets or sets a value that indicates whether this <see cref="System.Speech.Recognition.SpeechRecognizer" /> object is ready to process speech.</summary>
        /// <returns>
        ///   <see langword="true" /> if this <see cref="System.Speech.Recognition.SpeechRecognizer" /> object is performing speech recognition; otherwise, <see langword="false" />.</returns>
        public bool Enabled
        {
            get
            {
                return RecoBase.Enabled;
            }
            set
            {
                RecoBase.Enabled = value;
            }
        }

        /// <summary>Gets or sets a value that indicates whether the shared recognizer pauses recognition operations while an application is handling a <see cref="System.Speech.Recognition.SpeechRecognitionEngine.SpeechRecognized" /> event.</summary>
        /// <returns>
        ///   <see langword="true" /> if the shared recognizer waits to process input while any application is handling the <see cref="System.Speech.Recognition.SpeechRecognitionEngine.SpeechRecognized" /> event; otherwise, <see langword="false" />.</returns>
        public bool PauseRecognizerOnRecognition
        {
            get
            {
                return RecoBase.PauseRecognizerOnRecognition;
            }
            set
            {
                RecoBase.PauseRecognizerOnRecognition = value;
            }
        }

        /// <summary>Gets a collection of the <see cref="System.Speech.Recognition.Grammar" /> objects that are loaded in this <see cref="System.Speech.Recognition.SpeechRecognizer" /> instance.</summary>
        /// <returns>A collection of the <see cref="System.Speech.Recognition.Grammar" /> objects that the application loaded into the current instance of the shared recognizer.</returns>
        public ReadOnlyCollection<Grammar> Grammars => RecoBase.Grammars;

        /// <summary>Gets information about the shared speech recognizer.</summary>
        /// <returns>Information about the shared speech recognizer.</returns>
        public RecognizerInfo RecognizerInfo => RecoBase.RecognizerInfo;

        /// <summary>Gets the state of the audio being received by the speech recognizer.</summary>
        /// <returns>The state of the audio input to the speech recognizer.</returns>
        public AudioState AudioState => RecoBase.AudioState;

        /// <summary>Gets the level of the audio being received by the speech recognizer.</summary>
        /// <returns>The audio level of the input to the speech recognizer, from 0 through 100.</returns>
        public int AudioLevel => RecoBase.AudioLevel;

        /// <summary>Gets the current location in the audio stream being generated by the device that is providing input to the speech recognizer.</summary>
        /// <returns>The current location in the speech recognizer's audio input stream through which it has received input.</returns>
        public TimeSpan AudioPosition => RecoBase.AudioPosition;

        /// <summary>Gets the current location of the recognizer in the audio input that it is processing.</summary>
        /// <returns>The position of the recognizer in the audio input that it is processing.</returns>
        public TimeSpan RecognizerAudioPosition => RecoBase.RecognizerAudioPosition;

        /// <summary>Gets the format of the audio being received by the speech recognizer.</summary>
        /// <returns>The audio input format for the speech recognizer, or <see langword="null" /> if the input to the recognizer is not configured.</returns>
        public SpeechAudioFormatInfo AudioFormat => RecoBase.AudioFormat;

        /// <summary>Gets or sets the maximum number of alternate recognition results that the shared recognizer returns for each recognition operation.</summary>
        /// <returns>The maximum number of alternate results that the speech recognizer returns for each recognition operation.</returns>
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
                    try
                    {
                        _recognizerBase.Initialize(_sapiRecognizer, inproc: false);
                    }
                    catch (COMException e)
                    {
                        throw RecognizerBase.ExceptionFromSapiCreateRecognizerError(e);
                    }
                    PauseRecognizerOnRecognition = false;
                    _recognizerBase._haveInputSource = true;
                    if (AudioPosition != TimeSpan.Zero)
                    {
                        _recognizerBase.AudioState = AudioState.Silence;
                    }
                    _recognizerBase.StateChanged += StateChangedProxy;
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

        /// <summary>Occurs when the running state of the Windows Desktop Speech Technology recognition engine changes.</summary>
        public event EventHandler<StateChangedEventArgs> StateChanged;

        /// <summary>Occurs when the shared recognizer finalizes an asynchronous recognition operation for emulated input.</summary>
        public event EventHandler<EmulateRecognizeCompletedEventArgs> EmulateRecognizeCompleted;

        /// <summary>Occurs when the recognizer finishes the asynchronous loading of a speech recognition grammar.</summary>
        public event EventHandler<LoadGrammarCompletedEventArgs> LoadGrammarCompleted;

        /// <summary>Occurs when the recognizer detects input that it can identify as speech.</summary>
        public event EventHandler<SpeechDetectedEventArgs> SpeechDetected;

        /// <summary>Occurs when the recognizer receives input that matches one of its speech recognition grammars.</summary>
        public event EventHandler<SpeechRecognizedEventArgs> SpeechRecognized;

        /// <summary>Occurs when the recognizer receives input that does not match any of the speech recognition grammars it has loaded.</summary>
        public event EventHandler<SpeechRecognitionRejectedEventArgs> SpeechRecognitionRejected;

        /// <summary>Occurs when the recognizer pauses to synchronize recognition and other operations.</summary>
        public event EventHandler<RecognizerUpdateReachedEventArgs> RecognizerUpdateReached;

        /// <summary>Occurs when the recognizer has recognized a word or words that may be a component of multiple complete phrases in a grammar.</summary>
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

        /// <summary>Occurs when the recognizer encounters a problem in the audio signal.</summary>
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

        /// <summary>Occurs when the shared recognizer reports the level of its audio input.</summary>
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

        /// <summary>Occurs when the state changes in the audio being received by the recognizer.</summary>
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

        /// <summary>Initializes a new instance of the <see cref="System.Speech.Recognition.SpeechRecognizer" /> class.</summary>
        public SpeechRecognizer()
        {
            _sapiRecognizer = new SapiRecognizer(SapiRecognizer.RecognizerType.Shared);
        }

        /// <summary>Disposes the <see cref="System.Speech.Recognition.SpeechRecognizer" /> object.</summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Disposes the <see cref="System.Speech.Recognition.SpeechRecognizer" /> object and releases resources used during the session.</summary>
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

        /// <summary>Loads a speech recognition grammar.</summary>
        /// <param name="grammar">The speech recognition grammar to load.</param>
        public void LoadGrammar(Grammar grammar)
        {
            RecoBase.LoadGrammar(grammar);
        }

        /// <summary>Asynchronously loads a speech recognition grammar.</summary>
        /// <param name="grammar">The speech recognition grammar to load.</param>
        public void LoadGrammarAsync(Grammar grammar)
        {
            RecoBase.LoadGrammarAsync(grammar);
        }

        /// <summary>Unloads a specified speech recognition grammar from the shared recognizer.</summary>
        /// <param name="grammar">The grammar to unload.</param>
        public void UnloadGrammar(Grammar grammar)
        {
            RecoBase.UnloadGrammar(grammar);
        }

        /// <summary>Unloads all speech recognition grammars from the shared recognizer.</summary>
        public void UnloadAllGrammars()
        {
            RecoBase.UnloadAllGrammars();
        }

        /// <summary>Emulates input of a phrase to the shared speech recognizer, using text instead of audio for synchronous speech recognition.</summary>
        /// <param name="inputText">The input for the recognition operation.</param>
        /// <returns>The recognition result for the recognition operation, or <see langword="null" />, if the operation is not successful or Windows Speech Recognition is in the Sleeping state.</returns>
        public RecognitionResult EmulateRecognize(string inputText)
        {
            if (Enabled)
            {
                return RecoBase.EmulateRecognize(inputText);
            }
            throw new InvalidOperationException(SR.Get(SRID.RecognizerNotEnabled));
        }

        /// <summary>Emulates input of a phrase to the shared speech recognizer, using text instead of audio for synchronous speech recognition, and specifies how the recognizer handles Unicode comparison between the phrase and the loaded speech recognition grammars.</summary>
        /// <param name="inputText">The input phrase for the recognition operation.</param>
        /// <param name="compareOptions">A bitwise combination of the enumeration values that describe the type of comparison to use for the emulated recognition operation.</param>
        /// <returns>The recognition result for the recognition operation, or <see langword="null" />, if the operation is not successful or Windows Speech Recognition is in the Sleeping state.</returns>
        public RecognitionResult EmulateRecognize(string inputText, CompareOptions compareOptions)
        {
            if (Enabled)
            {
                return RecoBase.EmulateRecognize(inputText, compareOptions);
            }
            throw new InvalidOperationException(SR.Get(SRID.RecognizerNotEnabled));
        }

        /// <summary>Emulates input of specific words to the shared speech recognizer, using text instead of audio for synchronous speech recognition, and specifies how the recognizer handles Unicode comparison between the words and the loaded speech recognition grammars.</summary>
        /// <param name="wordUnits">An array of word units that contains the input for the recognition operation.</param>
        /// <param name="compareOptions">A bitwise combination of the enumeration values that describe the type of comparison to use for the emulated recognition operation.</param>
        /// <returns>The recognition result for the recognition operation, or <see langword="null" />, if the operation is not successful or Windows Speech Recognition is in the Sleeping state.</returns>
        public RecognitionResult EmulateRecognize(RecognizedWordUnit[] wordUnits, CompareOptions compareOptions)
        {
            if (Enabled)
            {
                return RecoBase.EmulateRecognize(wordUnits, compareOptions);
            }
            throw new InvalidOperationException(SR.Get(SRID.RecognizerNotEnabled));
        }

        /// <summary>Emulates input of a phrase to the shared speech recognizer, using text instead of audio for asynchronous speech recognition.</summary>
        /// <param name="inputText">The input for the recognition operation.</param>
        public void EmulateRecognizeAsync(string inputText)
        {
            if (Enabled)
            {
                RecoBase.EmulateRecognizeAsync(inputText);
                return;
            }
            throw new InvalidOperationException(SR.Get(SRID.RecognizerNotEnabled));
        }

        /// <summary>Emulates input of a phrase to the shared speech recognizer, using text instead of audio for asynchronous speech recognition, and specifies how the recognizer handles Unicode comparison between the phrase and the loaded speech recognition grammars.</summary>
        /// <param name="inputText">The input phrase for the recognition operation.</param>
        /// <param name="compareOptions">A bitwise combination of the enumeration values that describe the type of comparison to use for the emulated recognition operation.</param>
        public void EmulateRecognizeAsync(string inputText, CompareOptions compareOptions)
        {
            if (Enabled)
            {
                RecoBase.EmulateRecognizeAsync(inputText, compareOptions);
                return;
            }
            throw new InvalidOperationException(SR.Get(SRID.RecognizerNotEnabled));
        }

        /// <summary>Emulates input of specific words to the shared speech recognizer, using text instead of audio for asynchronous speech recognition, and specifies how the recognizer handles Unicode comparison between the words and the loaded speech recognition grammars.</summary>
        /// <param name="wordUnits">An array of word units that contains the input for the recognition operation.</param>
        /// <param name="compareOptions">A bitwise combination of the enumeration values that describe the type of comparison to use for the emulated recognition operation.</param>
        public void EmulateRecognizeAsync(RecognizedWordUnit[] wordUnits, CompareOptions compareOptions)
        {
            if (Enabled)
            {
                RecoBase.EmulateRecognizeAsync(wordUnits, compareOptions);
                return;
            }
            throw new InvalidOperationException(SR.Get(SRID.RecognizerNotEnabled));
        }

        /// <summary>Requests that the shared recognizer pause and update its state.</summary>
        public void RequestRecognizerUpdate()
        {
            RecoBase.RequestRecognizerUpdate();
        }

        /// <summary>Requests that the shared recognizer pause and update its state and provides a user token for the associated event.</summary>
        /// <param name="userToken">User-defined information that contains information for the operation.</param>
        public void RequestRecognizerUpdate(object userToken)
        {
            RecoBase.RequestRecognizerUpdate(userToken);
        }

        /// <summary>Requests that the shared recognizer pause and update its state and provides an offset and a user token for the associated event.</summary>
        /// <param name="userToken">User-defined information that contains information for the operation.</param>
        /// <param name="audioPositionAheadToRaiseUpdate">The offset from the current <see cref="System.Speech.Recognition.SpeechRecognizer.AudioPosition" /> to delay the request.</param>
        public void RequestRecognizerUpdate(object userToken, TimeSpan audioPositionAheadToRaiseUpdate)
        {
            RecoBase.RequestRecognizerUpdate(userToken, audioPositionAheadToRaiseUpdate);
        }

        private void StateChangedProxy(object sender, StateChangedEventArgs e)
        {
            this.StateChanged?.Invoke(this, e);
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
