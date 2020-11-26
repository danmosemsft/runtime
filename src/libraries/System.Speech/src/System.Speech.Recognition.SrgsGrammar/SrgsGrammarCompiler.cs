// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using System.Speech.Internal;
using System.Speech.Internal.SrgsCompiler;
using System.Xml;

namespace System.Speech.Recognition.SrgsGrammar
{
    /// <summary>Compiles <see cref="System.Speech.Recognition.SrgsGrammar.SrgsDocument" /> and XML-format grammar files into binary grammar files with the .cfg extension and sends the output to a stream.</summary>
    public static class SrgsGrammarCompiler
    {
        /// <summary>Compiles an XML-format grammar file into a binary grammar file with the .cfg extension and sends the output to a stream.</summary>
        /// <param name="inputPath">The path of the file to compile.</param>
        /// <param name="outputStream">The stream that receives the results of compilation.</param>
        /// <exception cref="System.ArgumentNullException">
        ///   <paramref name="inputPath" /> is <see langword="null" />.
        /// <paramref name="outputStream" /> is <see langword="null" />.</exception>
        /// <exception cref="System.ArgumentException">
        ///   <paramref name="inputPath" /> is an empty string.</exception>
        public static void Compile(string inputPath, Stream outputStream)
        {
            Helpers.ThrowIfEmptyOrNull(inputPath, nameof(inputPath));
            Helpers.ThrowIfNull(outputStream, nameof(outputStream));
            using (XmlTextReader xmlTextReader = new XmlTextReader(new Uri(inputPath, UriKind.RelativeOrAbsolute).ToString()))
            {
                SrgsCompiler.CompileStream(new XmlReader[1]
                {
                    xmlTextReader
                }, null, outputStream, fOutputCfg: true, null, null, null);
            }
        }

        /// <summary>Compiles an <see cref="System.Speech.Recognition.SrgsGrammar.SrgsDocument" /> object into a binary grammar file with the .cfg extension and sends the output to a stream.</summary>
        /// <param name="srgsGrammar">The grammar to compile.</param>
        /// <param name="outputStream">The stream that receives the results of compilation.</param>
        /// <exception cref="System.ArgumentNullException">
        ///   <paramref name="srgsGrammar" /> is <see langword="null" />.
        /// <paramref name="outputStream" /> is <see langword="null" />.</exception>
        public static void Compile(SrgsDocument srgsGrammar, Stream outputStream)
        {
            Helpers.ThrowIfNull(srgsGrammar, nameof(srgsGrammar));
            Helpers.ThrowIfNull(outputStream, nameof(outputStream));
            SrgsCompiler.CompileStream(srgsGrammar, null, outputStream, fOutputCfg: true, null, null);
        }

        /// <summary>Compiles data for an XML-format grammar file referenced by an <see cref="System.Xml.XmlReader" /> into a binary grammar file with the .cfg extension and sends the output to a stream.</summary>
        /// <param name="reader">The <see cref="System.Xml.XmlReader" /> that reads the grammar. The grammar can reside in a physical file or in memory.</param>
        /// <param name="outputStream">The stream that will receive the results of compilation.</param>
        /// <exception cref="System.ArgumentNullException">
        ///   <paramref name="reader" /> is <see langword="null" />.
        /// <paramref name="outputStream" /> is <see langword="null" />.</exception>
        public static void Compile(XmlReader reader, Stream outputStream)
        {
            Helpers.ThrowIfNull(reader, nameof(reader));
            Helpers.ThrowIfNull(outputStream, nameof(outputStream));
            SrgsCompiler.CompileStream(new XmlReader[1]
            {
                reader
            }, null, outputStream, fOutputCfg: true, null, null, null);
        }

        /// <summary>Compiles multiple SRGS grammars into a DLL.</summary>
        /// <param name="inputPaths">A list of the grammars to compile.</param>
        /// <param name="outputPath">The path of the output DLL.</param>
        /// <param name="referencedAssemblies">A list of the assemblies referenced from the input grammars.</param>
        /// <param name="keyFile">The name of the file that contains a pair of keys, thereby enabling the output DLL to be signed.</param>
        /// <exception cref="System.ArgumentNullException">
        ///   <paramref name="inputPaths" /> is <see langword="null" />.
        /// <paramref name="outputPath" /> is <see langword="null" />.</exception>
        /// <exception cref="System.ArgumentException">
        ///   <paramref name="outputPath" /> is an empty string.
        /// Any element of the <paramref name="inputPaths" /> array is <see langword="null" />.</exception>
        public static void CompileClassLibrary(string[] inputPaths, string outputPath, string[] referencedAssemblies, string keyFile)
        {
            Helpers.ThrowIfNull(inputPaths, nameof(inputPaths));
            Helpers.ThrowIfEmptyOrNull(outputPath, nameof(outputPath));
            XmlTextReader[] array = new XmlTextReader[inputPaths.Length];
            try
            {
                for (int i = 0; i < inputPaths.Length; i++)
                {
                    if (inputPaths[i] == null)
                    {
                        throw new ArgumentException(SR.Get(SRID.ArrayOfNullIllegal), nameof(inputPaths));
                    }
                    array[i] = new XmlTextReader(new Uri(inputPaths[i], UriKind.RelativeOrAbsolute).ToString());
                }
                SrgsCompiler.CompileStream(array, outputPath, null, fOutputCfg: false, null, referencedAssemblies, keyFile);
            }
            finally
            {
                for (int j = 0; j < array.Length; j++)
                {
                    ((IDisposable)array[j])?.Dispose();
                }
            }
        }

        /// <summary>Compiles an SRGS document into a DLL.</summary>
        /// <param name="srgsGrammar">The <see cref="System.Speech.Recognition.SrgsGrammar.SrgsDocument" /> that contains the grammar to compile.</param>
        /// <param name="outputPath">The path of the output DLL.</param>
        /// <param name="referencedAssemblies">A list of the assemblies referenced from the input grammars.</param>
        /// <param name="keyFile">The name of the file that contains a pair of keys, thereby enabling the output DLL to be signed.</param>
        /// <exception cref="System.ArgumentNullException">
        ///   <paramref name="srgsGrammar" /> is <see langword="null" />.
        /// <paramref name="outputPath" /> is <see langword="null" />.</exception>
        /// <exception cref="System.ArgumentException">
        ///   <paramref name="outputPath" /> is an empty string.</exception>
        public static void CompileClassLibrary(SrgsDocument srgsGrammar, string outputPath, string[] referencedAssemblies, string keyFile)
        {
            Helpers.ThrowIfNull(srgsGrammar, nameof(srgsGrammar));
            Helpers.ThrowIfEmptyOrNull(outputPath, nameof(outputPath));
            SrgsCompiler.CompileStream(srgsGrammar, outputPath, null, fOutputCfg: false, referencedAssemblies, keyFile);
        }

        /// <summary>Compiles an SRGS grammar into a DLL.</summary>
        /// <param name="reader">The <see cref="System.Xml.XmlReader" /> that reads the grammar.</param>
        /// <param name="outputPath">The path of the output DLL.</param>
        /// <param name="referencedAssemblies">A list of the assemblies referenced from the input grammars.</param>
        /// <param name="keyFile">The name of the file that contains a pair of keys, thereby enabling the output DLL to be signed.</param>
        /// <exception cref="System.ArgumentNullException">
        ///   <paramref name="reader" /> is <see langword="null" />.
        /// <paramref name="outputPath" /> is <see langword="null" />.</exception>
        /// <exception cref="System.ArgumentException">
        ///   <paramref name="outputPath" /> is an empty string.</exception>
        public static void CompileClassLibrary(XmlReader reader, string outputPath, string[] referencedAssemblies, string keyFile)
        {
            Helpers.ThrowIfNull(reader, nameof(reader));
            Helpers.ThrowIfEmptyOrNull(outputPath, nameof(outputPath));
            SrgsCompiler.CompileStream(new XmlReader[1]
            {
                reader
            }, outputPath, null, fOutputCfg: false, null, referencedAssemblies, keyFile);
        }

        private static bool CheckIfCfg(Stream stream, out int cfgLength)
        {
            long position = stream.Position;
            bool result = CfgGrammar.CfgSerializedHeader.IsCfg(stream, out cfgLength);
            stream.Position = position;
            return result;
        }

        internal static void CompileXmlOrCopyCfg(Stream inputStream, Stream outputStream, Uri orginalUri)
        {
            SeekableReadStream seekableReadStream = new SeekableReadStream(inputStream);
            int cfgLength;
            bool flag = CheckIfCfg(seekableReadStream, out cfgLength);
            seekableReadStream.CacheDataForSeeking = false;
            if (flag)
            {
                Helpers.CopyStream(seekableReadStream, outputStream, cfgLength);
                return;
            }
            SrgsCompiler.CompileStream(new XmlReader[1]
            {
                new XmlTextReader(seekableReadStream)
            }, null, outputStream, fOutputCfg: true, orginalUri, null, null);
        }
    }
}
