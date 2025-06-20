/********************************************************************************************
Copyright(c) 2023 LuckyStar Studio LLC
All rights reserved.

Microsoft Public License (Ms-PL)

This license governs use of the accompanying software. If you use the software, you
accept this license. If you do not accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the
same meaning here as under U.S. copyright law.
A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions
and limitations in section 3, each contributor grants you a non-exclusive, worldwide,
royalty-free copyright license to reproduce its contribution, prepare derivative works of
its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions
and limitations in section 3, each contributor grants you a non-exclusive, worldwide,
royalty-free license under its licensed patents to make, have made, use, sell, offer for
sale, import, and/or otherwise dispose of its contribution in the software or derivative
works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors'
name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are
infringed by the software, your patent license from such contributor to the software ends
automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent,
trademark, and attribution notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only
under this license by including a complete copy of this license with your distribution.
If you distribute any portion of the software in compiled or object code form, you may only
do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it.The contributors give
no express warranties, guarantees, or conditions. You may have additional consumer rights
under your local laws which this license cannot change. To the extent permitted under your
local laws, the contributors exclude the implied warranties of merchantability, fitness for
a particular purpose and non-infringement.

********************************************************************************************/

namespace ZigVS.Test
{
#nullable disable
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    [DefaultExecutorUri(Parameter.c_TestExecutorUriString)]
    [FileExtension(Parameter.c_fileExtension)]
    public class TestDiscoverer : ITestDiscoverer
    {
        public void DiscoverTests(
            IEnumerable<string> i_sourceStringIEnumrable,
            IDiscoveryContext i_IDiscoveryContext,
            IMessageLogger i_IMessageLogger,
            ITestCaseDiscoverySink i_ITestCaseDiscoverySink)
        {
            i_IMessageLogger.SendMessage(TestMessageLevel.Error, "DiscoverTests() is Running -----------    ");

            if (i_IDiscoveryContext != null)
            {
                i_IMessageLogger.SendMessage(TestMessageLevel.Error, "Setting: " + i_IDiscoveryContext.RunSettings.SettingsXml);
            }

            if (i_sourceStringIEnumrable == null || i_ITestCaseDiscoverySink == null)
            {
                i_IMessageLogger.SendMessage(TestMessageLevel.Error,
                    "i_sourceStringIEnumrable or i_ITestCaseDiscoverySink is null -----------    ");
            }
            else
            {
                foreach (var i_sourceString in i_sourceStringIEnumrable)
                {
                    i_IMessageLogger.SendMessage(TestMessageLevel.Error, "Source: " + i_sourceString);

                    try
                    {
                        var l_sourceCodeString = System.IO.File.ReadAllText(i_sourceString);
                        var l_patternString = "(test)(\\s)(\")([\\w+-. ]+)(\")";
                        var l_groupCollection = Regex.Matches(l_sourceCodeString, l_patternString, RegexOptions.IgnoreCase);
                        foreach (Match l_Match in l_groupCollection)
                        {
                            TestCase l_TestCase = new TestCase();
                            l_TestCase.Source = i_sourceString;
                            l_TestCase.CodeFilePath = i_sourceString;
                            l_TestCase.DisplayName = l_Match.Groups[4].Value;
                            l_TestCase.LineNumber = l_Match.Groups[1].Index;
                            l_TestCase.ExecutorUri = Parameter.c_TestExecutorUri;
                            l_TestCase.FullyQualifiedName = i_sourceString + " " + l_Match.Groups[4].Value;
                            i_ITestCaseDiscoverySink.SendTestCase(l_TestCase);
                        }
                    }
                    catch (Exception e)
                    {
                        i_IMessageLogger.SendMessage(TestMessageLevel.Error, "could not read " + i_sourceString + " " + e.Message);
                    }
                }
            }
            i_IMessageLogger.SendMessage(TestMessageLevel.Error, "DiscoverTests() is done -----------    ");
        }
    }
}