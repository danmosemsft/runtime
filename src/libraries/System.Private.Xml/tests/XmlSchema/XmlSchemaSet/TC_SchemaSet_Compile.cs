// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Xunit;
using Xunit.Abstractions;
using System.IO;
using System.Xml.Schema;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace System.Xml.Tests
{
    //[TestCase(Name = "TC_SchemaSet_Compile", Desc = "", Priority = 0)]
    public class TC_SchemaSet_Compile : TC_SchemaSetBase
    {
        private ITestOutputHelper _output;

        public TC_SchemaSet_Compile(ITestOutputHelper output)
        {
            _output = output;
        }


        public bool bWarningCallback;
        public bool bErrorCallback;
        public int errorCount;
        public int warningCount;
        public bool WarningInnerExceptionSet = false;
        public bool ErrorInnerExceptionSet = false;

        //hook up validaton callback
        private void ValidationCallback(object sender, ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Warning)
            {
                _output.WriteLine("WARNING: ");
                bWarningCallback = true;
                warningCount++;
                WarningInnerExceptionSet = (args.Exception.InnerException != null);
                _output.WriteLine("\nInnerExceptionSet : " + WarningInnerExceptionSet + "\n");
            }
            else if (args.Severity == XmlSeverityType.Error)
            {
                _output.WriteLine("ERROR: ");
                bErrorCallback = true;
                errorCount++;
                ErrorInnerExceptionSet = (args.Exception.InnerException != null);
                _output.WriteLine("\nInnerExceptionSet : " + ErrorInnerExceptionSet + "\n");
            }

            _output.WriteLine(args.Message); // Print the error to the screen.
        }

        [Fact]
        //[Variation(Desc = "v1 - Compile on empty collection")]
        public void v1()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.Compile();
            return;
        }

        [Fact]
        //[Variation(Desc = "v2 - Compile after error in Add")]
        public void v2()
        {
            XmlSchemaSet sc = new XmlSchemaSet();

            try
            {
                sc.Add(null, Path.Combine(TestData._Root, "schema1.xdr"));
            }
            catch (XmlSchemaException)
            {
                sc.Compile();
                // GLOBALIZATION
                return;
            }
            Assert.True(false);
        }

        [Fact]
        //[Variation(Desc = "TFS_470021 Unexpected local particle qualified name when chameleon schema is added to set")]
        public void TFS_470021()
        {
            string cham = @"<?xml version='1.0' encoding='utf-8' ?>
<xs:schema id='a0'
                  elementFormDefault='qualified'
                  xmlns:xs='http://www.w3.org/2001/XMLSchema'>
  <xs:complexType name='ctseq1_a'>
    <xs:sequence>
      <xs:element name='foo'/>
    </xs:sequence>
    <xs:attribute name='abt0' type='xs:string'/>
  </xs:complexType>
  <xs:element name='gect1_a' type ='ctseq1_a'/>
</xs:schema>";
            string main = @"<?xml version='1.0' encoding='utf-8' ?>
<xs:schema id='m0'
                  targetNamespace='http://tempuri.org/chameleon1'
                  elementFormDefault='qualified'
                  xmlns='http://tempuri.org/chameleon1'
                  xmlns:mstns='http://tempuri.org/chameleon1'
                  xmlns:xs='http://www.w3.org/2001/XMLSchema'>
  <xs:include schemaLocation='cham.xsd' />

  <xs:element name='root'>
    <xs:complexType>
      <xs:sequence maxOccurs='unbounded'>
        <xs:any namespace='##any' processContents='lax'/>
      </xs:sequence>
    </xs:complexType>
  </xs:element>
</xs:schema>";

            using (var tempDirectory = new TempDirectory())
            {
                string chamPath = Path.Combine(tempDirectory.Path, "cham.xsd");
                string tempDirectoryPath = tempDirectory.Path[tempDirectory.Path.Length - 1] == Path.DirectorySeparatorChar ?
                    tempDirectory.Path :
                    tempDirectory.Path + Path.DirectorySeparatorChar;

                using (XmlWriter w = XmlWriter.Create(chamPath))
                {
                    using (XmlReader r = XmlReader.Create(new StringReader(cham)))
                        w.WriteNode(r, true);
                }
                XmlSchemaSet ss = new XmlSchemaSet();
                ss.ValidationEventHandler += new ValidationEventHandler(ValidationCallback);

                ss.Add(null, XmlReader.Create(new StringReader(cham)));
                // TempDirectory path must end with a DirectorySeratorChar, otherwise it will throw in the Xml validation.
                var settings = new XmlReaderSettings() { XmlResolver = new XmlUrlResolver() };
                ss.Add(null, XmlReader.Create(new StringReader(main), settings, tempDirectoryPath));
                ss.Compile();

                Assert.Equal(2, ss.Count);
                foreach (XmlSchemaElement e in ss.GlobalElements.Values)
                {
                    _output.WriteLine(e.QualifiedName.ToString());
                    XmlSchemaComplexType type = e.ElementSchemaType as XmlSchemaComplexType;
                    XmlSchemaSequence seq = type.ContentTypeParticle as XmlSchemaSequence;
                    foreach (XmlSchemaObject child in seq.Items)
                    {
                        if (child is XmlSchemaElement)
                            _output.WriteLine("\t" + (child as XmlSchemaElement).QualifiedName);
                    }
                }
                Assert.Equal(0, warningCount);
                Assert.Equal(0, errorCount);
            }
        }

        /// <summary>
        /// Test for issue #30218, resource Sch_MinLengthGtBaseMinLength
        /// </summary>
        [Fact]
        public void MinLengthGtBaseMinLength_Throws()
        {
            string schema = @"<?xml version='1.0' encoding='utf-8' ?>
<xs:schema elementFormDefault='qualified'
           xmlns:xs='http://www.w3.org/2001/XMLSchema'>
    <xs:simpleType name='foo'>
        <xs:restriction base='xs:string'>
            <xs:minLength value='5'/>
        </xs:restriction>
    </xs:simpleType>
    <xs:simpleType name='bar'>
        <xs:restriction base='foo'>
            <xs:minLength value='4'/>
        </xs:restriction>
    </xs:simpleType>
</xs:schema>
";

            XmlSchemaSet ss = new XmlSchemaSet();
            ss.Add(null, XmlReader.Create(new StringReader(schema)));

            Exception ex = Assert.Throws<XmlSchemaException>(() => ss.Compile());
            Assert.Contains("minLength", ex.Message);
        }


        /// <summary>
        /// Test for issue #30218, resource Sch_MaxLengthGtBaseMaxLength
        /// </summary>
        [Fact]
        public void MaxLengthGtBaseMaxLength_Throws()
        {
            string schema = @"<?xml version='1.0' encoding='utf-8' ?>
<xs:schema elementFormDefault='qualified'
           xmlns:xs='http://www.w3.org/2001/XMLSchema'>
    <xs:simpleType name='foo'>
        <xs:restriction base='xs:string'>
            <xs:maxLength value='5'/>
        </xs:restriction>
    </xs:simpleType>
    <xs:simpleType name='bar'>
        <xs:restriction base='foo'>
            <xs:maxLength value='6'/>
        </xs:restriction>
    </xs:simpleType>
</xs:schema>
";

            XmlSchemaSet ss = new XmlSchemaSet();
            ss.Add(null, XmlReader.Create(new StringReader(schema)));

            Exception ex = Assert.Throws<XmlSchemaException>(() => ss.Compile());
            Assert.Contains("maxLength", ex.Message);
        }

        #region "Testing presence of minLength or maxLength and Length"

        public static IEnumerable<object[]> MaxMinLengthBaseLength_ThrowsData
        {
            get
            {
                return new List<object[]>()
                {
                    new object[]
                    {  // minLength and length specified in same derivation step.
                        @"<?xml version='1.0' encoding='utf-8' ?>
<xs:schema elementFormDefault='qualified'
           xmlns:xs='http://www.w3.org/2001/XMLSchema'>
    <xs:simpleType name='foo'>
        <xs:restriction base='xs:string'>
            <xs:minLength value='5'/>
            <xs:length value='5' />
        </xs:restriction>
    </xs:simpleType>
</xs:schema>
"
                    },
                    new object[]
                    {  // maxLength and length specified in same derivation step.
                        @"<?xml version='1.0' encoding='utf-8' ?>
<xs:schema elementFormDefault='qualified'
           xmlns:xs='http://www.w3.org/2001/XMLSchema'>
    <xs:simpleType name='foo'>
        <xs:restriction base='xs:string'>
            <xs:maxLength value='5'/>
            <xs:length value='5' />
        </xs:restriction>
    </xs:simpleType>
</xs:schema>
"
                    },
                    new object[]
                    {  // base type has minLength; derived type has lesser length
                        @"<?xml version='1.0' encoding='utf-8' ?>
<xs:schema elementFormDefault='qualified'
           xmlns:xs='http://www.w3.org/2001/XMLSchema'>
    <xs:simpleType name='foo'>
        <xs:restriction base='xs:string'>
            <xs:minLength value='5'/>
        </xs:restriction>
    </xs:simpleType>
    <xs:simpleType name='bar'>
        <xs:restriction base='foo'>
            <xs:length value='4'/>
        </xs:restriction>
    </xs:simpleType>
</xs:schema>
"
                    },
                    new object[]
                    {  // base type has maxLength; derived type has greater length
                        @"<?xml version='1.0' encoding='utf-8' ?>
<xs:schema elementFormDefault='qualified'
           xmlns:xs='http://www.w3.org/2001/XMLSchema'>
    <xs:simpleType name='foo'>
        <xs:restriction base='xs:string'>
            <xs:maxLength value='5'/>
        </xs:restriction>
    </xs:simpleType>
    <xs:simpleType name='bar'>
        <xs:restriction base='foo'>
            <xs:length value='6'/>
        </xs:restriction>
    </xs:simpleType>
</xs:schema>
"
                    },
                    new object[]
                    {  // base type has length; derived type has lesser maxLength
                        @"<?xml version='1.0' encoding='utf-8' ?>
<xs:schema elementFormDefault='qualified'
           xmlns:xs='http://www.w3.org/2001/XMLSchema'>
    <xs:simpleType name='foo'>
        <xs:restriction base='xs:string'>
            <xs:length value='5'/>
        </xs:restriction>
    </xs:simpleType>
    <xs:simpleType name='bar'>
        <xs:restriction base='foo'>
            <xs:maxLength value='4'/>
        </xs:restriction>
    </xs:simpleType>
</xs:schema>
"
                    },
                    new object[]
                    {  // base type has length; derived type has greater minLength
                        @"<?xml version='1.0' encoding='utf-8' ?>
<xs:schema elementFormDefault='qualified'
           xmlns:xs='http://www.w3.org/2001/XMLSchema'>
    <xs:simpleType name='foo'>
        <xs:restriction base='xs:string'>
            <xs:length value='5'/>
        </xs:restriction>
    </xs:simpleType>
    <xs:simpleType name='bar'>
        <xs:restriction base='foo'>
            <xs:minLength value='6'/>
        </xs:restriction>
    </xs:simpleType>
</xs:schema>
"
                    },
                    new object[]
                    {  // base type has maxLength; derived type has greater length
                        @"<?xml version='1.0' encoding='utf-8' ?>
<xs:schema elementFormDefault='qualified'
           xmlns:xs='http://www.w3.org/2001/XMLSchema'>
    <xs:simpleType name='foo'>
        <xs:restriction base='xs:string'>
            <xs:maxLength value='5'/>
        </xs:restriction>
    </xs:simpleType>
    <xs:simpleType name='bar'>
        <xs:restriction base='foo'>
            <xs:length value='6'/>
        </xs:restriction>
    </xs:simpleType>
</xs:schema>
"
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(MaxMinLengthBaseLength_ThrowsData))]
        public void MaxMinLengthBaseLength_Throws(string schema)
        {
            XmlSchemaSet ss = new XmlSchemaSet();
            ss.Add(null, XmlReader.Create(new StringReader(schema)));

            Exception ex = Assert.Throws<XmlSchemaException>(() => ss.Compile());
            Assert.Contains("length", ex.Message);
            Assert.Contains("minLength", ex.Message);
            Assert.Contains("maxLength", ex.Message);

            // Issue 30218: invalid formatters
            Regex rx = new Regex(@"\{.*[a-zA-Z ]+.*\}");
            Assert.Empty(rx.Matches(ex.Message));
        }

        [Fact]
        public void MinLengthGtMaxLength_Throws()
        {
            string schema = @"<?xml version='1.0' encoding='utf-8' ?>
<xs:schema elementFormDefault='qualified'
           xmlns:xs='http://www.w3.org/2001/XMLSchema'>
    <xs:simpleType name='foo'>
        <xs:restriction base='xs:string'>
            <xs:maxLength value='5'/>
            <xs:minLength value='8'/>
        </xs:restriction>
    </xs:simpleType>
</xs:schema>
";
            XmlSchemaSet ss = new XmlSchemaSet();
            ss.Add(null, XmlReader.Create(new StringReader(schema)));

            Exception ex = Assert.Throws<XmlSchemaException>(() => ss.Compile());
            // The thrown error message has an upper case 'M' in both
            // minLength and maxLength.
            Assert.Contains("minlength", ex.Message.ToLower());
            Assert.Contains("maxlength", ex.Message.ToLower());

            // Issue 30218: invalid formatters
            Regex rx = new Regex(@"\{[0-9]*[a-zA-Z ]+[^\}]*\}");
            Assert.Empty(rx.Matches(ex.Message));
        }

        public static IEnumerable<object[]> MaxMinLengthBaseLength_TestData
        {
            get
            {
                return new List<object[]>()
                {
                    new object[]
                    {  // base type has length; derived type has equal maxLength
                        @"<?xml version='1.0' encoding='utf-8' ?>
<xs:schema elementFormDefault='qualified'
           xmlns:xs='http://www.w3.org/2001/XMLSchema'>
    <xs:simpleType name='foo'>
        <xs:restriction base='xs:string'>
            <xs:length value='5'/>
        </xs:restriction>
    </xs:simpleType>
    <xs:simpleType name='bar'>
        <xs:restriction base='foo'>
            <xs:maxLength value='5'/>
        </xs:restriction>
    </xs:simpleType>
</xs:schema>
"
                    },
                    new object[]
                    {  // base type has length; derived type has greater maxLength
                        @"<?xml version='1.0' encoding='utf-8' ?>
<xs:schema elementFormDefault='qualified'
           xmlns:xs='http://www.w3.org/2001/XMLSchema'>
    <xs:simpleType name='foo'>
        <xs:restriction base='xs:string'>
            <xs:length value='5'/>
        </xs:restriction>
    </xs:simpleType>
    <xs:simpleType name='bar'>
        <xs:restriction base='foo'>
            <xs:maxLength value='6'/>
        </xs:restriction>
    </xs:simpleType>
</xs:schema>
"
                    },
                    new object[]
                    {  // base type has length; derived type has equal minLength
                        @"<?xml version='1.0' encoding='utf-8' ?>
<xs:schema elementFormDefault='qualified'
           xmlns:xs='http://www.w3.org/2001/XMLSchema'>
    <xs:simpleType name='foo'>
        <xs:restriction base='xs:string'>
            <xs:length value='5'/>
        </xs:restriction>
    </xs:simpleType>
    <xs:simpleType name='bar'>
        <xs:restriction base='foo'>
            <xs:minLength value='5'/>
        </xs:restriction>
    </xs:simpleType>
</xs:schema>
"
                    },
                    new object[]
                    {  // base type has length; derived type has lesser minLength
                        @"<?xml version='1.0' encoding='utf-8' ?>
<xs:schema elementFormDefault='qualified'
           xmlns:xs='http://www.w3.org/2001/XMLSchema'>
    <xs:simpleType name='foo'>
        <xs:restriction base='xs:string'>
            <xs:length value='5'/>
        </xs:restriction>
    </xs:simpleType>
    <xs:simpleType name='bar'>
        <xs:restriction base='foo'>
            <xs:minLength value='4'/>
        </xs:restriction>
    </xs:simpleType>
</xs:schema>
"
                    },
                    new object[]
                    {  // base type has minLength; derived type has equal length
                        @"<?xml version='1.0' encoding='utf-8' ?>
<xs:schema elementFormDefault='qualified'
           xmlns:xs='http://www.w3.org/2001/XMLSchema'>
    <xs:simpleType name='foo'>
        <xs:restriction base='xs:string'>
            <xs:minLength value='5'/>
        </xs:restriction>
    </xs:simpleType>
    <xs:simpleType name='bar'>
        <xs:restriction base='foo'>
            <xs:length value='5'/>
        </xs:restriction>
    </xs:simpleType>
</xs:schema>
"
                    },
                    new object[]
                    {  // base type has minLength; derived type has greater length
                        @"<?xml version='1.0' encoding='utf-8' ?>
<xs:schema elementFormDefault='qualified'
           xmlns:xs='http://www.w3.org/2001/XMLSchema'>
    <xs:simpleType name='foo'>
        <xs:restriction base='xs:string'>
            <xs:minLength value='5'/>
        </xs:restriction>
    </xs:simpleType>
    <xs:simpleType name='bar'>
        <xs:restriction base='foo'>
            <xs:length value='6'/>
        </xs:restriction>
    </xs:simpleType>
</xs:schema>
"
                    },
                    new object[]
                    {  // base type has maxLength; derived type has equal length
                        @"<?xml version='1.0' encoding='utf-8' ?>
<xs:schema elementFormDefault='qualified'
           xmlns:xs='http://www.w3.org/2001/XMLSchema'>
    <xs:simpleType name='foo'>
        <xs:restriction base='xs:string'>
            <xs:maxLength value='5'/>
        </xs:restriction>
    </xs:simpleType>
    <xs:simpleType name='bar'>
        <xs:restriction base='foo'>
            <xs:length value='5'/>
        </xs:restriction>
    </xs:simpleType>
</xs:schema>
"
                    },
                    new object[]
                    {  // base type has maxLength; derived type has lesser length
                        @"<?xml version='1.0' encoding='utf-8' ?>
<xs:schema elementFormDefault='qualified'
           xmlns:xs='http://www.w3.org/2001/XMLSchema'>
    <xs:simpleType name='foo'>
        <xs:restriction base='xs:string'>
            <xs:maxLength value='5'/>
        </xs:restriction>
    </xs:simpleType>
    <xs:simpleType name='bar'>
        <xs:restriction base='foo'>
            <xs:length value='4'/>
        </xs:restriction>
    </xs:simpleType>
</xs:schema>
"
                    },
                    new object[]
                    {  // minLength is equal to maxLength
                        @"<?xml version='1.0' encoding='utf-8' ?>
<xs:schema elementFormDefault='qualified'
           xmlns:xs='http://www.w3.org/2001/XMLSchema'>
    <xs:simpleType name='foo'>
        <xs:restriction base='xs:string'>
            <xs:maxLength value='5'/>
            <xs:minLength value='5'/>
        </xs:restriction>
    </xs:simpleType>
</xs:schema>
"
                    },
                    new object[]
                    {  // minLength is less than maxLength
                        @"<?xml version='1.0' encoding='utf-8' ?>
<xs:schema elementFormDefault='qualified'
           xmlns:xs='http://www.w3.org/2001/XMLSchema'>
    <xs:simpleType name='foo'>
        <xs:restriction base='xs:string'>
            <xs:maxLength value='8'/>
            <xs:minLength value='5'/>
        </xs:restriction>
    </xs:simpleType>
</xs:schema>
"
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(MaxMinLengthBaseLength_TestData))]
        public void MaxMinLengthBaseLength_Test(string schema)
        {
            XmlSchemaSet ss = new XmlSchemaSet();
            ss.Add(null, XmlReader.Create(new StringReader(schema)));

            Exception exception;
            try
            {
                ss.Compile();
                exception = null;
            } catch (Exception ex)
            {
                exception = ex;
            }
            Assert.Null(exception);
        }
        #endregion

        /// <summary>
        /// Test for issue #30218, resource Sch_LengthGtBaseLength
        /// </summary>
        [Fact]
        public void LengthGtBaseLength_Throws()
        {
            string schema = @"<?xml version='1.0' encoding='utf-8' ?>
<xs:schema elementFormDefault='qualified'
           xmlns:xs='http://www.w3.org/2001/XMLSchema'>
    <xs:simpleType name='foo'>
        <xs:restriction base='xs:string'>
            <xs:length value='5'/>
        </xs:restriction>
    </xs:simpleType>
    <xs:simpleType name='bar'>
        <xs:restriction base='foo'>
            <xs:length value='6'/>
        </xs:restriction>
    </xs:simpleType>
</xs:schema>
";

            XmlSchemaSet ss = new XmlSchemaSet();
            ss.Add(null, XmlReader.Create(new StringReader(schema)));

            Exception ex = Assert.Throws<XmlSchemaException>(() => ss.Compile());
            Assert.Contains("length", ex.Message);

            // Issue 30218: invalid formatters
            Regex rx = new Regex(@"\{.*[a-zA-Z ]+.*\}");
            Assert.Empty(rx.Matches(ex.Message));
        }
    }
}
