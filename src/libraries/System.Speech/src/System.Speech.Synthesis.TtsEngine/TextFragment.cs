using System.Runtime.InteropServices;
using System.Speech.Internal;

namespace System.Speech.Synthesis.TtsEngine
{
	/// <summary>Contains text and speech attribute information for consumption by a speech synthsizer engine.</summary>
	[StructLayout(LayoutKind.Sequential)]
	public class TextFragment
	{
		private FragmentState _state;

		[MarshalAs(UnmanagedType.LPWStr)]
		private string _textToSpeak = string.Empty;

		private int _textOffset;

		private int _textLength;

		/// <summary>Gets or sets speech attribute information for a <see langword="TextFragment" />.</summary>
		/// <returns>A <see cref="T:System.Speech.Synthesis.TtsEngine.FragmentState" /> instance is returned, or used to set speech attribute information for a <see cref="T:System.Speech.Synthesis.TtsEngine.TextFragment" />.</returns>
		public FragmentState State
		{
			get
			{
				return _state;
			}
			set
			{
				_state = value;
			}
		}

		/// <summary>Sets or gets the speech text of the fragment.</summary>
		/// <returns>A <see langword="System.String" /> is returned or can be used to set the speech text to be used by a speech synthesis engine to generate audio output.</returns>
		public string TextToSpeak
		{
			get
			{
				return _textToSpeak;
			}
			set
			{
				Helpers.ThrowIfEmptyOrNull(value, "value");
				_textToSpeak = value;
			}
		}

		/// <summary>Gets or sets the starting location of the text in the fragment.</summary>
		/// <returns>An <see langword="int" /> is returned or can be used to set the start location, in character, of the part of text string associated with this fragment to be spoken.</returns>
		public int TextOffset
		{
			get
			{
				return _textOffset;
			}
			set
			{
				_textOffset = value;
			}
		}

		/// <summary>Gets or sets the length of the speech text in the fragment.</summary>
		/// <returns>An <see langword="int" /> is returned or can be used to set the length, in character, of the text string associated with this fragment to be spoken.</returns>
		public int TextLength
		{
			get
			{
				return _textLength;
			}
			set
			{
				_textLength = value;
			}
		}

		/// <summary>Constructs a new instance of <see langword="TextFragment" />.</summary>
		public TextFragment()
		{
		}

		internal TextFragment(FragmentState fragState)
			: this(fragState, null, null, 0, 0)
		{
		}

		internal TextFragment(FragmentState fragState, string textToSpeak)
			: this(fragState, textToSpeak, textToSpeak, 0, textToSpeak.Length)
		{
		}

		internal TextFragment(FragmentState fragState, string textToSpeak, string textFrag, int offset, int length)
		{
			if (fragState.Action == TtsEngineAction.Speak || fragState.Action == TtsEngineAction.Pronounce)
			{
				textFrag = textToSpeak;
			}
			if (!string.IsNullOrEmpty(textFrag))
			{
				TextToSpeak = textFrag;
			}
			State = fragState;
			TextOffset = offset;
			TextLength = length;
		}
	}
}
