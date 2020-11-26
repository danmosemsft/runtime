// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;

namespace System.Speech.Synthesis
{
	/// <summary>Represents a prompt created from a file.</summary>
	[DebuggerDisplay("{_text}")]
	public class FilePrompt : Prompt
	{
		/// <summary>Creates a new instance of the <see cref="T:System.Speech.Synthesis.FilePrompt" /> class, and specifies the path to the file and its media type.</summary>
		/// <param name="path">The path of the file containing the prompt content.</param>
		/// <param name="media">The media type of the file.</param>
		public FilePrompt(string path, SynthesisMediaType media)
			: this(new Uri(path, UriKind.Relative), media)
		{
		}

		/// <summary>Creates a new instance of the <see cref="T:System.Speech.Synthesis.FilePrompt" /> class, and specifies the location of the file and its media type.</summary>
		/// <param name="promptFile">The URI of the file containing the prompt content.</param>
		/// <param name="media">The media type of the file.</param>
		public FilePrompt(Uri promptFile, SynthesisMediaType media)
			: base(promptFile, media)
		{
		}
	}
}
