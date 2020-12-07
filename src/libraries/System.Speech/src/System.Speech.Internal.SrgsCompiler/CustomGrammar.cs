// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Speech.Internal.SrgsParser;
using System.Text;

namespace System.Speech.Internal.SrgsCompiler
{
    internal class CustomGrammar
    {
        internal class CfgResource
        {
            internal string name;

            internal byte[] data;
        }

        internal string _language = "C#";

        internal string _namespace;

        internal List<Rule> _rules = new List<Rule>();

        internal Collection<string> _codebehind = new Collection<string>();

        internal bool _fDebugScript;

        internal Collection<string> _assemblyReferences = new Collection<string>();

        internal Collection<string> _importNamespaces = new Collection<string>();

        internal string _keyFile;

        internal Collection<ScriptRef> _scriptRefs = new Collection<ScriptRef>();

        internal List<string> _types = new List<string>();

        internal StringBuilder _script = new StringBuilder();
        internal bool HasScript
        {
            get
            {
                bool flag = _script.Length > 0 || _codebehind.Count > 0;
                if (!flag)
                {
                    foreach (Rule rule in _rules)
                    {
                        if (rule.Script.Length > 0)
                        {
                            return true;
                        }
                    }
                    return flag;
                }
                return flag;
            }
        }

        internal CustomGrammar()
        {
        }

        internal void Combine(CustomGrammar cg, string innerCode)
        {
            if (_rules.Count == 0)
            {
                _language = cg._language;
            }
            else if (_language != cg._language)
            {
                XmlParser.ThrowSrgsException(SRID.IncompatibleLanguageProperties);
            }
            if (_namespace == null)
            {
                _namespace = cg._namespace;
            }
            else if (_namespace != cg._namespace)
            {
                XmlParser.ThrowSrgsException(SRID.IncompatibleNamespaceProperties);
            }
            _fDebugScript |= cg._fDebugScript;
            foreach (string item in cg._codebehind)
            {
                if (!_codebehind.Contains(item))
                {
                    _codebehind.Add(item);
                }
            }
            foreach (string assemblyReference in cg._assemblyReferences)
            {
                if (!_assemblyReferences.Contains(assemblyReference))
                {
                    _assemblyReferences.Add(assemblyReference);
                }
            }
            foreach (string importNamespace in cg._importNamespaces)
            {
                if (!_importNamespaces.Contains(importNamespace))
                {
                    _importNamespaces.Add(importNamespace);
                }
            }
            _keyFile = cg._keyFile;
            _types.AddRange(cg._types);
            foreach (Rule rule in cg._rules)
            {
                if (_types.Contains(rule.Name))
                {
                    XmlParser.ThrowSrgsException(SRID.RuleDefinedMultipleTimes2, rule.Name);
                }
            }
            _script.Append(innerCode);
        }



    }
}
