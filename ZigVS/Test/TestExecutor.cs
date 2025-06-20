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
#nullable enable
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using System.Collections.Generic;
    using System.IO;

    [ExtensionUri(Parameter.c_TestExecutorUriString)]
    public class TestExecutor : ITestExecutor
    {
        bool m_cancelledBool = false;

        public void Cancel()
        {
            m_cancelledBool = true;
        }

        public void RunTests(IEnumerable<TestCase>? i_TestCaseEnumerable, IRunContext? i_IRunContext, IFrameworkHandle? i_IFrameworkHandle)
        {
            if (i_IRunContext != null && i_TestCaseEnumerable != null && i_IFrameworkHandle != null)
            {
                var l_toolPathString = Utilities.GetToolPathFromEnvironmentValue();

                m_cancelledBool = false;
                foreach (var i_TestCase in i_TestCaseEnumerable)
                {
                    if (m_cancelledBool == true)
                    {
                        break;
                    }
                    i_IFrameworkHandle.RecordStart(i_TestCase);

                    var l_ResultSet = new Common.Test.ResultSet();
                    if (i_IRunContext.IsBeingDebugged)
                    {
                        var l_exeList = Common.Test.BuildUnitTestAndFindExeFile(i_TestCase.Source, i_TestCase.DisplayName);
                        if (l_exeList.Count == 0)
                        {
                            l_ResultSet.m_TestOutcome = TestOutcome.Skipped;
                            l_ResultSet.m_resultString = "Could not found unit test exe";
                            l_ResultSet.m_ErrorMessage = "";
                        }
                        else
                        {
                            var l_debuggerResult = i_IFrameworkHandle.LaunchProcessWithDebuggerAttached(l_exeList[0], Path.GetDirectoryName(l_exeList[0]), "", null);

                            l_ResultSet.m_TestOutcome = TestOutcome.Skipped;
                            l_ResultSet.m_resultString = "Source:" + i_TestCase.Source;
                            l_ResultSet.m_ErrorMessage = "Exe:" + l_exeList[0] + " returned " + l_debuggerResult.ToString();
                        }
                    }
                    else
                    {
                        l_ResultSet = Common.Test.RunTestProcess(
                        Path.Combine(l_toolPathString, Parameter.c_compilerFileName),
                        "test --test-filter \"" + i_TestCase.DisplayName + "\" " + i_TestCase.Source);
                    }

                    var l_TestResult = new TestResult(i_TestCase);
                    l_TestResult.Outcome = l_ResultSet.m_TestOutcome;
                    l_TestResult.ErrorMessage = l_ResultSet.m_resultString;
                    l_TestResult.ErrorStackTrace = l_ResultSet.m_ErrorMessage;

                    i_IFrameworkHandle.RecordResult(l_TestResult);
                    i_IFrameworkHandle.RecordEnd(i_TestCase, l_TestResult.Outcome);
                }
            }
        }

        public void RunTests(IEnumerable<string>? i_stringEnumerable, IRunContext? i_IRunContext, IFrameworkHandle? i_IFrameworkHandle)
        {
            if (i_stringEnumerable != null && i_IFrameworkHandle != null)
            {
                m_cancelledBool = false;
                foreach (var l_sourceString in i_stringEnumerable)
                {
                    if (m_cancelledBool == true)
                    {
                        break;
                    }

                    var l_TestCase = new TestCase(l_sourceString, Parameter.c_TestExecutorUri, l_sourceString);
                    l_TestCase.Source = l_sourceString;
                    l_TestCase.CodeFilePath = l_sourceString;
                    l_TestCase.DisplayName = l_sourceString;
                    l_TestCase.LineNumber = 1;
                    l_TestCase.ExecutorUri = Parameter.c_TestExecutorUri;
                    l_TestCase.FullyQualifiedName = l_sourceString;

                    i_IFrameworkHandle.RecordStart(l_TestCase);

                    var l_toolPathString = Utilities.GetToolPathFromEnvironmentValue();

                    var l_ResultSet = new Common.Test.ResultSet();
                    /*                   if (i_IRunContext.IsBeingDebugged)
                                       {
                                           List<string> l_createdFileList = new List<string>();

                                           var l_zigFolderString = System.Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                                           using (FileSystemWatcher watcher = new FileSystemWatcher(Path.Combine(l_zigFolderString, "AppData\\Local\\zig\\o")))
                                           {
                                               watcher.NotifyFilter = NotifyFilters.FileName;// | NotifyFilters.CreationTime;
                                               watcher.Filter = "*.exe";
                                               watcher.IncludeSubdirectories = true;
                                               watcher.Renamed += (sender, e) =>
                                               {
                                                   l_createdFileList.Add(e.FullPath);
                                               };
                                               watcher.Created += (sender, e) =>
                                               {
                                                   l_createdFileList.Add(e.FullPath);
                                               };
                                               watcher.EnableRaisingEvents = true;
                                               RunTestProcess(Path.Combine(l_toolPathString, "zig.exe"), "test --test-no-exec " + l_sourceString);
                                               watcher.EnableRaisingEvents = false;
                                           }
                                           l_ResultSet.m_resultString = "file:";
                                           foreach (string l_testBinFileNameString in l_createdFileList)
                                           {
                                               l_ResultSet.m_resultString += l_testBinFileNameString + "  " + i_IFrameworkHandle.ToString() + "  ";

                                               var l_VsDebugTargetInfo = new VsDebugTargetInfo
                                               {
                                                   dlo = DEBUG_LAUNCH_OPERATION.DLO_CreateProcess,
                                                   bstrCurDir = Path.GetDirectoryName(l_testBinFileNameString),
                                                   bstrExe = l_testBinFileNameString,
                                                   bstrArg = "",
                                                   bstrEnv = null,
                                                   bstrOptions = "",
                                                   bstrPortName = null,
                                                   bstrMdmRegisteredName = null,
                                                   bstrRemoteMachine = null,
                                                   cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf<VsDebugTargetInfo>(),
                                                   grfLaunch = (uint)(__VSDBGLAUNCHFLAGS.DBGLAUNCH_Silent | __VSDBGLAUNCHFLAGS.DBGLAUNCH_StopDebuggingOnEnd),
                                                   fSendStdoutToOutputWindow = 1,  // 1 = route stdout and stderr to the debug-output window
                                                   clsidCustom = DebugEnginesGuids.NativeOnly_guid
                                               };

                                               VsShellUtilities.LaunchDebugger(ZigVSPackage.GetInstance(), l_VsDebugTargetInfo);
                                               i_IFrameworkHandle.LaunchProcessWithDebuggerAttached(
                                                     l_testBinFileNameString, Path.GetDirectoryName(l_testBinFileNameString), "", null);
                                               l_ResultSet.m_TestOutcome = TestOutcome.Failed;
                                           }
                                       }
                                       else
                                       {
                                       }*/
                    l_ResultSet = Common.Test.RunTestProcess(Path.Combine(l_toolPathString, Parameter.c_compilerFileName), "test " + l_sourceString);

                    var l_TestResult = new TestResult(l_TestCase);
                    l_TestResult.Outcome = l_ResultSet.m_TestOutcome;
                    l_TestResult.ErrorMessage = l_ResultSet.m_resultString;
                    l_TestResult.ErrorStackTrace = l_ResultSet.m_ErrorMessage;

                    i_IFrameworkHandle.RecordResult(l_TestResult);
                    i_IFrameworkHandle.RecordEnd(l_TestResult.TestCase, l_TestResult.Outcome);
                }
            }
        }
    }
}