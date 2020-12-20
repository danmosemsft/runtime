//------------------------------------------------------------------
// <copyright file="MSSGrammarCompiler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------

#if SPEECHSERVER

using System;
using System.IO;
using System.Globalization;
using System.Xml;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Speech.Internal;
using System.Speech.Internal.SrgsCompiler;

namespace System.Speech.Recognition.SrgsGrammar
{
    /// <summary>
    /// Extended version of SrgsGrammarCompiler that compiles Xml Srgs data into a CFG.
    /// Also detects an existing CFG and returns it unchanged.
    /// Also returns various properties about the grammar.
    /// </summary>
    
    public static class MssGrammarCompiler // This is public for now, but really should be internal.
    {

        //*******************************************************************
        //
        // Public Methods
        //
        //*******************************************************************

#region Public Methods

        /// <summary>
        /// Compiles a grammar.
        /// <param name="inputStream">Stream containing grammar data. Must be readable, not necessarily seekable.
        /// Seek position must point to start of grammar initially, upon return points to end of data.
        /// </param>
        /// <param name="outputStream">Stream where compiled grammar is output. Must be writeable and seekable.
        /// Seek position must point to start of data initially, upon return points to end of data.
        /// </param>
        /// <param name="culture">Language of grammar or null if no xml:lang supplied.</param>
        /// <param name="mode">Voice of Dtmf grammar.</param>
        /// <param name="referencedGrammars">List of rule references to external grammars.
        /// List will have the rule names {fragments} removed and will contain no duplicate entries.
        /// Relative rule references will be merged with the xml:base if present, otherwise will be returned as relative Uris.
        /// </param>
        /// </summary>
        static public void Compile( // This is public for now, but really should be internal.
            Stream inputStream,
            Stream outputStream,
            out CultureInfo culture,
            out SrgsGrammarMode mode,
            out Collection<Uri> referencedGrammars)
        {
            // Parameter validate:
            ValidateArguments(inputStream, outputStream);

            long initialOutputPosition = outputStream.Position;

            SrgsGrammarCompiler.CompileXmlOrCopyCfg(inputStream, outputStream, null);

            // Rewind output stream:
            long finalOutputPosition = outputStream.Position;
            outputStream.Position = initialOutputPosition;

            // Get the required data from the compiled cfg:
            ExtractHeaderInfo(outputStream, out culture, out mode, out referencedGrammars);

            // Reset position to end:
            outputStream.Position = finalOutputPosition;

        }

#endregion 


        //*******************************************************************
        //
        // Private Methods
        //
        //*******************************************************************

#region Private Methods

        // Check the inputStream and outputStream have the correct features
        static private void ValidateArguments(Stream inputStream, Stream outputStream)
        {
            if (inputStream == null)
            {
                throw new ArgumentNullException("inputStream");
            }
            if (outputStream == null)
            {
                throw new ArgumentNullException("outputStream");
            }
            if (!inputStream.CanRead)
            {
                throw new ArgumentException(SR.Get(SRID.StreamMustBeReadable), "inputStream");
            }
            if (!outputStream.CanWrite)
            {
                throw new ArgumentException(SR.Get(SRID.StreamMustBeWriteable), "outputStream");
            }
            if (!outputStream.CanSeek)
            {
                throw new ArgumentException(SR.Get(SRID.StreamMustBeSeekable), "outputStream");
            }
        }

        // Pulls the required data out of a stream containing a cfg.
        // Stream must point to start of cfg on entry and is reset to same point on exit.
        static private void ExtractHeaderInfo(Stream stream, out CultureInfo culture, out SrgsGrammarMode mode, out Collection<Uri> referencedGrammars)
        {
            long initialPosition = stream.Position;

            CfgGrammar.CfgHeader header;
            using (StreamMarshaler streamHelper = new StreamMarshaler(stream)) // Use StreamMarshaler which helps deserialize certain data types
            {
                CfgGrammar.CfgSerializedHeader serializedHeader = null;
                header = CfgGrammar.ConvertCfgHeader(streamHelper, false, false, out serializedHeader);

                // Get CultureInfo:
                if (header.langId != 0)
                {
                    // LCID are not valid with custom Culture info
                    culture = header.langId == 0x540A ? new CultureInfo ("es-us") : culture = new CultureInfo (header.langId); // No need to clone - header is going away
                }
                else
                {
                    culture = null;
                }

                // Get Mode:
                mode = (SrgsGrammarMode)header.GrammarMode;

                // Get referenced grammars:
                referencedGrammars = ExtractRuleReferences(ref header, streamHelper, serializedHeader, initialPosition);
            }

            stream.Position = initialPosition;
        }

        private static Collection<Uri> ExtractRuleReferences(ref CfgGrammar.CfgHeader header, StreamMarshaler streamHelper, CfgGrammar.CfgSerializedHeader serializedHeader, long initialPosition)
        {
            // Get base Uri:
            Uri baseUri = null;
            if (!string.IsNullOrEmpty(header.BasePath))
            {
                baseUri = new Uri(header.BasePath);
            }

            // Get referenced grammars:
            Collection<Uri> referencedGrammars = new Collection<Uri>();
            foreach (CfgRule rule in header.rules)
            {
                if (rule.Import)
                {
                    // External rule refs are stored as imported rules with the uri stored as the rule name.
                    // Get the rule name field:
                    streamHelper.Stream.Position = initialPosition + serializedHeader.pszSymbols
                        + (rule._nameOffset * Helpers._sizeOfChar);
                    string uriString = streamHelper.ReadNullTerminatedString();

                    if (uriString.StartsWith("URL:", StringComparison.OrdinalIgnoreCase))
                    {
                        // Strip off leading "URL:"
                        uriString = uriString.Substring(4);

                        // Remove fragment:
                        // {Would use UriBuilder but it doesn't seem to support relative Uris}.
                        int fragmentPosition = uriString.LastIndexOf("#", StringComparison.Ordinal);
                        if (fragmentPosition != -1)
                        {
                            uriString = uriString.Remove(fragmentPosition);
                        }

                        // Merge with baseUri:
                        Uri uri;
                        if (baseUri != null)
                        {
                            uri = new Uri(baseUri, uriString);
                        }
                        else
                        {
                            uri = new Uri(uriString, UriKind.RelativeOrAbsolute);
                        }

                        // If not already in collection then add:
                        if (!referencedGrammars.Contains(uri))
                        {
                            referencedGrammars.Add(uri);
                        }
                    }
                }
            }
            return referencedGrammars;
        }
    
#endregion

    }
}

#endif
