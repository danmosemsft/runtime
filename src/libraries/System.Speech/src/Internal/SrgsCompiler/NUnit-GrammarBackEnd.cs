//------------------------------------------------------------------
// <copyright file="NUnit-GrammarBackEnd.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// Description: 
//		Debug only helper routines
//
// History:
//		5/1/2004	jeanfp		Created from the Sapi Managed code
//------------------------------------------------------------------

#if VSCOMPILE

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Speech.Internal.SrgsParser;
using System.Text;

namespace System.Speech.Internal.SrgsCompiler
{
    /// <summary>
    /// Summary description for DebugHelper.
    /// </summary>
    internal sealed partial class Backend
    {
        internal static void DumpCfgInHex (Stream stream, TextWriter tw)
        {
            CfgGrammar.CfgHeader header;
            using (StreamMarshaler streamHelper = new StreamMarshaler (stream))
            {
                header = CfgGrammar.ConvertCfgHeader (streamHelper);
            }

            StringBlob words = header.pszWords;
            StringBlob symbols = header.pszSymbols;

            // Dumps the symbols
            string formatInfo = string.Empty;
            tw.Write ("Semantics: ");
            if ((header.GrammarOptions & GrammarOptions.STG) == GrammarOptions.STG)
            {
                tw.Write ("'Strongly typed'");
            }
            else if ((header.GrammarOptions & GrammarOptions.TagFormat) == GrammarOptions.KeyValuePairSrgs || (header.GrammarOptions & GrammarOptions.TagFormat) == GrammarOptions.KeyValuePairs)
            {
                tw.Write ("'Key/Value Pairs'");
            }
            else if ((header.GrammarOptions & GrammarOptions.MssV1) == GrammarOptions.MssV1)
            {
                tw.Write ("'Microsoft Speech Server v1.0'" );
            }
            else if ((header.GrammarOptions & GrammarOptions.W3cV1) == GrammarOptions.W3cV1)
            {
                tw.Write ("'W3C v1.0'");
            }

            tw.Write (" - Alphabet: ");
            tw.Write ((((header.GrammarOptions & GrammarOptions.IpaPhoneme) == GrammarOptions.IpaPhoneme) ? "'Ipa'" : "'Sapi'"));
            tw.Write (" - LangId: ");
            tw.Write (string.Format (CultureInfo.InvariantCulture, "'{0:x}", header.langId));
            tw.WriteLine (header.GrammarMode == GrammarType.VoiceGrammar ? " - Voice'\n" : " - DTMF'\n");
            tw.Write ("\nWord Blobs - Phonemes:");
            tw.WriteLine ("----------");
            for (int i = 1; i <= words.Count; i++)
            {
                tw.WriteLine (string.Format (CultureInfo.InvariantCulture, "  {0,3}: \"{1}\"", i, words [i]));
            }

            tw.WriteLine ("\nSymbols Blobs");
            tw.WriteLine ("---------------");
            for (int i = 1; i <= symbols.Count; i++)
            {
                tw.WriteLine (string.Format (CultureInfo.InvariantCulture, "  {0,3}: \"{1}\"", i, symbols [i]));
            }

            //
            // print the Semantic Tags
            //
            tw.WriteLine (string.Format (CultureInfo.InvariantCulture, "\nSemantic Tags [{0}]", header.tags.Length));
            tw.WriteLine ("-----------------");
            for (int i = 0; i < header.tags.Length; i++)
            {
                CfgSemanticTag cfgTag = header.tags [i];

                string value;
                string name = GetSemanticValue (cfgTag, symbols, out value);
                if (name == "SemanticKey")
                {
                    tw.WriteLine (string.Format (CultureInfo.InvariantCulture, "  {0,2}: Arc: {1,-16} '{2,-2}-{3,2}'", i, "[" + value + "]", cfgTag.ArcIndex, cfgTag.EndArcIndex));
                }
                else
                {
                    // Change the name to make it more explicit
                    if (name == "=")
                    {
                        name = "<value>";
                    }
                    tw.WriteLine (string.Format (CultureInfo.InvariantCulture, "  {0,2}: Arc: {1,-16} '{2,-2}-{3,2}' Value '{4}' ({5}) Id: '{6}'", i, name, cfgTag.ArcIndex, cfgTag.EndArcIndex, value, cfgTag.PropVariantType, cfgTag._propId));
                }
            }

            //
            // print the rules
            //
            int rootRule = (int) header.ulRootRuleIndex == -1 ? 0 : (int) header.ulRootRuleIndex;
            int rootArc = 0;
            tw.WriteLine (string.Format (CultureInfo.InvariantCulture, "\nRules [{0}]", header.rules.Length));
            tw.WriteLine ("-----");
            for (int i = 0; i < header.rules.Length; i++)
            {
                CfgRule cfgRule = header.rules [i];

                string rootPrefix = "  ";
                if (rootRule == i)
                {
                    rootArc = (int) cfgRule.FirstArcIndex;
                    rootPrefix = "->";
                }

                string ruleName = "'" + (cfgRule._nameOffset > 0 ? symbols.FromOffset (cfgRule._nameOffset) : "<NO NAME>") + "'";
                tw.Write (string.Format (CultureInfo.InvariantCulture, "{0}{1}: {2,-10} Id: {3,-2} flag: '{4:x}' First: '{5}'", rootPrefix, i, ruleName, cfgRule._id, cfgRule._flag, cfgRule.FirstArcIndex));

                tw.WriteLine ("");
            }
            //
            // Print the scriptRefs 
            //
            if (header.scripts.Length > 0)
            {
                tw.WriteLine (string.Format (CultureInfo.InvariantCulture, "\nScripts [{0}]", header.scripts.Length));
                tw.WriteLine ("-------\n");
                tw.WriteLine ("RULE                METHOD              TYPE");
                for (int i = 0; i < header.scripts.Length; i++)
                {
                    CfgScriptRef script = header.scripts [i];
                    tw.WriteLine (string.Format (CultureInfo.InvariantCulture, "  {0,-20}{1,-20}{2}", symbols [script._idRule], symbols [script._idMethod], script._method.ToString ()));
                }
            }
            //
            //  Initialize the arcs
            //
            tw.WriteLine (string.Format (CultureInfo.InvariantCulture, "\nArcs [{0}]", header.arcs.Length - 1));
            tw.WriteLine ("----");

            //  We repersist the static AND dynamic parts for now. This allows the grammar to be queried
            //  with the automation interfaces
            for (int k = 1; k < header.arcs.Length; k++)
            {
                CfgArc arc = header.arcs [k];
                string word = null;

                if (arc.RuleRef)
                {
                    word = string.Format (CultureInfo.InvariantCulture, "<{0}>", GetRuleRefName (header.rules [(int) arc.TransitionIndex], symbols));
                }
                else
                {
                    switch (arc.TransitionIndex)
                    {
                        case CfgGrammar.SPWILDCARDTRANSITION:
                            word = "*";
                            break;

                        case CfgGrammar.SPDICTATIONTRANSITION:
                            word = "Dictation";
                            break;

                        case CfgGrammar.SPTEXTBUFFERTRANSITION:
                            word = "TextBuffer";
                            break;

                        default:
                            word = arc.TransitionIndex == 0 ? "" : words [(int) arc.TransitionIndex];
                            // If the word contains a '\n' or '\n' replace the character by at escape character
                            if (word.IndexOf ('\n') >= 0 || word.IndexOf ('\r') >= 0 || word.IndexOf ('\t') >= 0)
                            {
                                word = word.Replace ('\n', '*').Replace ('\r', '*').Replace ('\t', '*');
                            }
                            break;
                    }
                }

                // pad the word with some underscore if it is the end of a word
                if (arc.LastArc)
                {
                    if (arc.TransitionIndex == 0 && !arc.RuleRef)
                    {
                        word = new string ('_', 25);
                    }
                    else
                    {
                        int lenUnderscore = word.Length >= 23 ? 2 : 25 - word.Length;
                        string underscore = new string ('_', lenUnderscore);
                        word += underscore;
                    }
                }

                string ruleName = string.Empty;
                foreach (CfgRule rule in header.rules)
                {
                    if (rule.FirstArcIndex == k)
                    {
                        ruleName = "<" + (rule._nameOffset > 0 ? symbols.FromOffset (rule._nameOffset) : "NO NAME") + ">";
                    }
                }
                string nextArc = arc.NextStartArcIndex != 0 ? string.Format (CultureInfo.InvariantCulture, "-> {0,-3}", arc.NextStartArcIndex) : "-> END";
                tw.Write (string.Format (CultureInfo.InvariantCulture, "{0}{1,9} {2,4} {3}    {4,-25}", k == rootArc ? "->" : "  ", ruleName, k + ":", nextArc, word));

                if (arc.MatchMode != 0)
                {
                    tw.Write (" " + ((MatchMode) arc.MatchMode).ToString ());
                }

                if (arc.HasSemanticTag)
                {
                    bool hasSemanticInfo = false;
                    tw.Write (" " + GetTag (arc, k, header.tags, header.rules, symbols, ref hasSemanticInfo));
                }

                tw.WriteLine ();
            }

            if (header.BasePath != null)
            {
                tw.WriteLine ("\n\nBase Path: " + header.BasePath);
            }

            tw.WriteLine ("\n\nLangId: " + header.langId);

            if (header.pszGlobalTags > 0)
            {
                tw.WriteLine (string.Format (CultureInfo.InvariantCulture, "\nGlobal tags: \"{0}\"", symbols.FromOffset (header.pszGlobalTags)));
            }

            //
            //  Now a formatted dump
            //


            for (int iRule = 0; iRule < header.rules.Length; iRule++)
            {
                CfgRule rule = header.rules [iRule];
                if (!rule.Import)
                {
                    if (rule._nameOffset > 0)
                    {
                        tw.WriteLine (string.Format (CultureInfo.InvariantCulture, "\nRule {0} - {1}\n", iRule, symbols.FromOffset (rule._nameOffset)));
                    }
                    else
                    {
                        tw.WriteLine (string.Format (CultureInfo.InvariantCulture, "\nRule {0} - <NO NAME> (ID={1})\n", iRule, rule._id));
                    }
                }
                else
                {
                    tw.WriteLine (string.Format (CultureInfo.InvariantCulture, "\nRule {0,2}\n", rule.FirstArcIndex));
                }

                CalcPath (tw, rule, header, words, symbols);
            }
        }

        internal static void CalcAllPaths (Stream stream, TextWriter tw)
        {
            CfgGrammar.CfgHeader header;
            using (StreamMarshaler streamHelper = new StreamMarshaler (stream))
            {
                header = CfgGrammar.ConvertCfgHeader (streamHelper);
            }
            StringBlob words = header.pszWords;
            StringBlob symbols = header.pszSymbols;

            for (int iRule = 0; iRule < header.rules.Length; iRule++)
            {
                CalcPath (tw, header.rules [iRule], header, words, symbols);
            }
        }

        private static void CalcPath (TextWriter tw, CfgRule rule, CfgGrammar.CfgHeader header, StringBlob words, StringBlob symbols)
        {
            int [] Path = new int [10000];
            byte [] aFlags = new byte [header.arcs.Length];
            List<StringBuilder> asbPath = new List<StringBuilder> ();

            RecurseDump (asbPath, header, words, symbols, (int) rule.FirstArcIndex, aFlags, Path, 0);

            string [] asPath = new string [asbPath.Count];

            for (int i = 0; i < asbPath.Count; i++)
            {
                asPath [i] = asbPath [i].ToString ();
            }

            Array.Sort (asPath);
            for (int i = 0; i < asPath.Length; i++)
            {
                tw.WriteLine (asPath [i]);
            }

        }

        private static void RecurseDump (List<StringBuilder> asb, CfgGrammar.CfgHeader header, StringBlob words, StringBlob symbols, int iArc, byte [] pFlags, int [] pPathIndexList, int ulPathLen)
        {
            if (iArc == 0)
            {
                return;
            }

            StringBuilder sb = new StringBuilder ();

            while (true)
            {
                if (header.arcs.Length <= iArc)
                {
                    sb.Append ("!!!! ERROR !!!!");
                    return;
                }
                CfgArc pArc = header.arcs [iArc];

                if ((pFlags [iArc] & 0x40) == 0)      // Don't allow 2 recursions
                {
                    if (((pFlags [iArc]) & 0x80) != 0)
                    {
                        pFlags [iArc] |= 0x40;
                        pPathIndexList [ulPathLen] = -(iArc);
                    }
                    else
                    {
                        pFlags [iArc] |= 0x80;
                        pPathIndexList [ulPathLen] = iArc;
                    }
                    if (pArc.NextStartArcIndex == 0)    // Terminal -- Print the stuff out!
                    {
                        sb.Append ("\"");
                        for (int i = 0; i <= ulPathLen; i++)
                        {
                            int ulIndex;
                            if (pPathIndexList [i] < 0)
                            {
                                ulIndex = -pPathIndexList [i];
                            }
                            else
                            {
                                ulIndex = pPathIndexList [i];
                            }
                            CfgArc currentArc = header.arcs [ulIndex];
                            sb.Append (GetWordForArc (currentArc, header.rules, words, symbols));
                        }
                        sb.Append ("\"");
                        //
                        //  Now the semantic info...
                        //
                        bool bHaveSemanticInfo = false;
                        for (int i = 0; i <= ulPathLen; i++)
                        {
                            int ulVal;
                            if (pPathIndexList [i] < 0)
                            {
                                ulVal = -pPathIndexList [i];
                            }
                            else
                            {
                                ulVal = pPathIndexList [i];
                            }
                            if (header.arcs [ulVal].HasSemanticTag)
                            {
                                sb.Append (GetTag (header.arcs [ulVal], ulVal, header.tags, header.rules, symbols, ref bHaveSemanticInfo));
                            }
                        }
                        asb.Add (sb);
                        if (asb.Count > 2000)
                        {
                            asb.Add (new StringBuilder ("Too many arcs"));
                            break;
                        }
                        sb = new StringBuilder ();
                    }
                    else
                    {
                        RecurseDump (asb, header, words, symbols, (int) pArc.NextStartArcIndex, pFlags, pPathIndexList, ulPathLen + 1);
                    }
                    if ((pFlags [iArc] & 0x40) != 0)
                    {
                        pFlags [iArc] &= unchecked ((byte) ~(0x40));
                    }
                    else
                    {
                        pFlags [iArc] &= unchecked ((byte) ~(0x80));
                    }
                }
                if (pArc.LastArc)
                {
                    break;
                }
                iArc++;
            }
        }

        private static string GetWordForArc (CfgArc currentArc, CfgRule [] rules, StringBlob words, StringBlob symbols)
        {
            StringBuilder sb = new StringBuilder ();
            if (currentArc.RuleRef)
            {
                sb.Append ("<");
                sb.Append (GetRuleRefName (rules [currentArc.TransitionIndex], symbols));
                sb.Append ("> ");
            }
            else
            {
                if (currentArc.TransitionIndex > 0)
                {
                    if (currentArc.TransitionIndex == CfgGrammar.SPTEXTBUFFERTRANSITION)
                    {
                        sb.Append ("[TEXTBUFFER] ");
                    }
                    else if (currentArc.TransitionIndex == CfgGrammar.SPWILDCARDTRANSITION)
                    {
                        sb.Append ("... ");
                    }
                    else if (currentArc.TransitionIndex == CfgGrammar.SPDICTATIONTRANSITION)
                    {
                        sb.Append ("* ");
                    }
                    else
                    {
                        string word = words [(int) currentArc.TransitionIndex];

                        // If the word contains a '\n' or '\n' replace the character by at escape character
                        if (word.IndexOf ('\n') >= 0 || word.IndexOf ('\r') >= 0 || word.IndexOf ('\t') >= 0)
                        {
                            word = word.Replace ('\n', '*').Replace ('\r', '*').Replace ('\t', '*');
                        }

                        sb.Append (string.Format (CultureInfo.InvariantCulture, "{0}{1}", word, (string.IsNullOrEmpty (word)) ? "" : " "));
                    }
                }
            }
            return sb.ToString ();
        }

        private static string GetTag (CfgArc arc, int ulVal, CfgSemanticTag [] tags, CfgRule [] rules, StringBlob symbols, ref bool bHaveSemanticInfo)
        {
            StringBuilder sb = new StringBuilder ();
            foreach (CfgSemanticTag tag in tags)
            {
                if (tag.ArcIndex == ulVal)
                {
                    // Prepend the semantic value with '--' if this is the first value found
                    if (!bHaveSemanticInfo)
                    {
                        sb.Append (" -- ");
                    }
                    string value;
                    string name = GetSemanticValue (tag, symbols, out value);

                    if (name == "SemanticKey")
                    {
                        System.Diagnostics.Debug.Assert (arc.RuleRef);
                        string ruleref = GetRuleRefName (rules [arc.TransitionIndex], symbols);
                        sb.Append (string.Format (CultureInfo.InvariantCulture, " [\"{0}\"]=<{1}>", value, ruleref));
                    }
                    else
                    {
                        sb.Append (string.Format (CultureInfo.InvariantCulture, " {0}{1}\"{2}\"", name, name != "=" ? "=" : "", value));
                    }
                    bHaveSemanticInfo = true;
                }
            }
            return sb.ToString ();
        }

        private static string GetRuleRefName (CfgRule rule, StringBlob symbols)
        {
            if (rule._nameOffset > 0)
            {
                return symbols.FromOffset (rule._nameOffset);
            }
            else
            {
                return string.Format (CultureInfo.InvariantCulture, "ID={0}", rule._id);
            }
        }

        private static string GetSemanticValue (CfgSemanticTag tag, StringBlob symbols, out string value)
        {
            switch (tag.PropVariantType)
            {
                case VarEnum.VT_EMPTY:
                    value = tag._valueOffset > 0 ? symbols.FromOffset (tag._valueOffset) : tag._valueOffset.ToString (CultureInfo.InvariantCulture);
                    break;

                case VarEnum.VT_I4:
                case VarEnum.VT_UI4:
                    value = tag._varInt.ToString ();
                    break;

                case VarEnum.VT_R8:
                    value = tag._varDouble.ToString ();
                    break;

                case VarEnum.VT_BOOL:
                    value = tag._varInt == 0 ? "false" : "true";
                    break;

                default:
                    value = "Unknown property type";
                    break;
            }

            return tag._nameOffset > 0 ? symbols.FromOffset (tag._nameOffset) : tag._nameOffset.ToString (CultureInfo.InvariantCulture); ;
        }
    }
}

#endif
