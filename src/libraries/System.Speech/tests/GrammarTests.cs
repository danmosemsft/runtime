// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Speech.Recognition;
using System.Speech.Recognition.SrgsGrammar;
using System.Text;
using System.Xml;
using Xunit;

namespace SampleSynthesisTests
{
    public class GrammarTests : FileCleanupTestBase
    {
        [Fact]
        public void WriteStronglyTypedGrammarToXml()
        {
            SrgsDocument srgsDoc = CreateSrgsDocument();

            var builder = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(builder))
            {
                srgsDoc.WriteSrgs(writer);
            }

            Assert.Contains("someRule", builder.ToString());
        }

        [Fact]
        public void CompileStronglyTypedGrammarToCfg()
        {
            SrgsDocument srgsDoc = CreateSrgsDocument();

            using var ms = new MemoryStream();
            SrgsGrammarCompiler.Compile(srgsDoc, ms);

            Assert.True(ms.Position > 0);
        }

        [Fact]
        public void CompileStronglyTypedGrammarFromFileToCfg()
        {
            SrgsDocument srgsDoc = CreateSrgsDocument();

            string temp = GetTestFilePath();

            using (XmlWriter writer = XmlWriter.Create(temp))
            {
                srgsDoc.WriteSrgs(writer);
            }

            using var ms = new MemoryStream();

            SrgsGrammarCompiler.Compile(temp, ms);

            Assert.True(ms.Position > 0);
        }

        [Fact]
        public void CompileStronglyTypedGrammarToDllFromPath()
        {
            SrgsDocument srgsDoc = CreateSrgsDocument();

            string temp = GetTestFilePath();

            // Cannot compile to assemblies on .NET Core
            Assert.Throws<PlatformNotSupportedException>(() => SrgsGrammarCompiler.CompileClassLibrary(srgsDoc, temp, new string[0], keyFile:null));
        }

        [Fact]
        public void ParseGrammarXml()
        {
            string xml = @"<grammar version=""1.0"" xml:lang=""en-US"" root=""playCommands"" xmlns=""http://www.w3.org/2001/06/grammar"">
                             <rule id=""playCommands"">
                               <ruleref uri=""#playAction"" />
                               <item> the </item>
                               <ruleref uri=""#fileWords"" />
                             </rule>
                             <rule id=""playAction"">
                               <one-of>
                                 <item> play </item>
                                 <item> start </item>
                                 <item> begin </item>
                               </one-of>
                             </rule>
                             <rule id=""fileWords"">
                               <one-of>
                                 <item> song </item>
                                 <item> tune </item>
                                 <item> track </item>
                                 <item> item </item>
                               </one-of>
                             </rule>

                           </grammar>";
            var grammar = new Grammar(new MemoryStream(Encoding.Unicode.GetBytes(xml)));

            grammar.Name = "test";

        }

        [Fact]
        public void GrammarBuilder()
        {
            Choices colorChoice = new Choices(new string[] {"red", "green", "blue"});
            GrammarBuilder colorElement = new GrammarBuilder(colorChoice);

            GrammarBuilder makePhrase = new GrammarBuilder("Make background");
            makePhrase.Append(colorElement);
            GrammarBuilder setPhrase = new GrammarBuilder("Set background to");
            setPhrase.Append(colorElement);

            Choices bothChoices = new Choices(new GrammarBuilder[] {makePhrase, setPhrase});
            Grammar grammar = new Grammar((GrammarBuilder)bothChoices);
            grammar.Name = "backgroundColor";

            using (var rec = new SpeechRecognitionEngine())
            {
                rec.LoadGrammar(grammar);
            }
        }

        private SrgsDocument CreateSrgsDocument()
        {
            SrgsDocument srgsDoc = new SrgsDocument();
            SrgsRule rule = new SrgsRule("someRule");
            SrgsItem item = new SrgsItem("someItem");
            item.Add(new SrgsSemanticInterpretationTag("out = \"foo\";"));
            SrgsOneOf oneOf = new SrgsOneOf(item);
            rule.Add(oneOf);

            srgsDoc.Rules.Add(rule);
            srgsDoc.Root = rule;
            return srgsDoc;
        }
    }
}
