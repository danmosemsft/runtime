// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

﻿using System.IO;
using System.Speech.Recognition;
using System.Speech.Recognition.SrgsGrammar;
using System.Text;
using System.Xml;
using Xunit;

namespace SampleSynthesisTests
{
    public static class GrammarTests
    {
        [Fact]
        public static void CompileGrammar()
        {
            SrgsDocument srgsDoc = new SrgsDocument();
            SrgsRule rule = new SrgsRule("someRule");
            SrgsItem item = new SrgsItem("someItem");
            item.Add(new SrgsSemanticInterpretationTag("out = \"foo\";"));
            SrgsOneOf oneOf = new SrgsOneOf(item);
            rule.Add(oneOf);

            srgsDoc.Rules.Add(rule);
            srgsDoc.Root = rule;

            // Write the completed grammar to an XML-format SRGS grammar file.
            var builder = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(builder))
            {
                srgsDoc.WriteSrgs(writer);
            }

            Assert.Contains("someRule", builder.ToString());
        }

        [Fact]
        public static void ParseGrammar()
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
    }
}
