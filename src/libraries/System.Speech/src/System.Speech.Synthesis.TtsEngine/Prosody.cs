using System.Runtime.InteropServices;
using System.Speech.Internal;

namespace System.Speech.Synthesis.TtsEngine
{
	/// <summary>Represents a collection of settings for voice properties such as <see langword="Pitch" />, <see langword="Rate" /> and <see langword="Volume" />.</summary>
	[StructLayout(LayoutKind.Sequential)]
	public class Prosody
	{
		internal ProsodyNumber _pitch;

		internal ProsodyNumber _range;

		internal ProsodyNumber _rate;

		internal int _duration;

		internal ProsodyNumber _volume;

		internal ContourPoint[] _contourPoints;

		/// <summary>Gets or sets the baseline pitch of the <see langword="TextFragment" />.</summary>
		public ProsodyNumber Pitch
		{
			get
			{
				return _pitch;
			}
			set
			{
				_pitch = value;
			}
		}

		/// <summary>Gets or sets the pitch range of the <see langword="TextFragment" />.</summary>
		public ProsodyNumber Range
		{
			get
			{
				return _range;
			}
			set
			{
				_range = value;
			}
		}

		/// <summary>Gets or sets the speaking rate of the <see langword="TextFragment" />.</summary>
		public ProsodyNumber Rate
		{
			get
			{
				return _rate;
			}
			set
			{
				_rate = value;
			}
		}

		/// <summary>Gets or sets the duration of the <see langword="TextFragment" /> in milliseconds.</summary>
		/// <returns>A value in milliseconds for the desired time to speak the text.</returns>
		public int Duration
		{
			get
			{
				return _duration;
			}
			set
			{
				_duration = value;
			}
		}

		/// <summary>Gets or sets the speaking volume (loudness) of the <see langword="TextFragment" />.</summary>
		public ProsodyNumber Volume
		{
			get
			{
				return _volume;
			}
			set
			{
				_volume = value;
			}
		}

		/// <summary>Returns an array containing the <see langword="ContourPoints" /> of the <see langword="TextFragment" />.</summary>
		public ContourPoint[] GetContourPoints()
		{
			return _contourPoints;
		}

		/// <summary>Sets the <see langword="ContourPoints" /> of the <see langword="TextFragment" />.</summary>
		/// <param name="points">A byte array of <see langword="ContourPoint" /> objects.</param>
		public void SetContourPoints(ContourPoint[] points)
		{
			Helpers.ThrowIfNull(points, "points");
			_contourPoints = (ContourPoint[])points.Clone();
		}

		/// <summary>Constructs a new instance of the <see langword="Prosody" /> class.</summary>
		public Prosody()
		{
			Pitch = new ProsodyNumber(0);
			Range = new ProsodyNumber(0);
			Rate = new ProsodyNumber(0);
			Volume = new ProsodyNumber(-1);
		}

		internal Prosody Clone()
		{
			Prosody prosody = new Prosody();
			prosody._pitch = _pitch;
			prosody._range = _range;
			prosody._rate = _rate;
			prosody._duration = _duration;
			prosody._volume = _volume;
			return prosody;
		}
	}
}
