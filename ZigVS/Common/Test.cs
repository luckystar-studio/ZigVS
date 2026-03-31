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

namespace ZigVS.Common
{
    using ZigVS.CoreCompatibility;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.Shell.Interop;
    using System;
    using System.Text.RegularExpressions;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;

#nullable enable
    static class Test
    {
        public static void LaunchDebugger(string i_exePathString)
        {
            var l_ServiceProvider = 
                (System.IServiceProvider)ZigVSPackage.GetInstance();
            
            var l_VsDebugTargetInfo = new VsDebugTargetInfo
            {
                dlo = DEBUG_LAUNCH_OPERATION.DLO_CreateProcess,
                bstrCurDir = Directory.GetCurrentDirectory(),
                bstrExe = i_exePathString,
                bstrArg = "",
                bstrEnv = null,
                bstrOptions = "",
                bstrPortName = null,
                bstrMdmRegisteredName = null,
                bstrRemoteMachine = null,
                cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf<VsDebugTargetInfo>(),
                grfLaunch = (uint)(__VSDBGLAUNCHFLAGS.DBGLAUNCH_Silent | __VSDBGLAUNCHFLAGS.DBGLAUNCH_StopDebuggingOnEnd),
                fSendStdoutToOutputWindow = 0,  // 1 = route stdout and stderr to the debug-output window
                clsidCustom = Microsoft.VisualStudio.VSConstants.DebugEnginesGuids.NativeOnly_guid
            };
            IntPtr intPtr = Marshal.AllocCoTaskMem((int)l_VsDebugTargetInfo.cbSize);
            Marshal.StructureToPtr(l_VsDebugTargetInfo, intPtr, fDeleteOld: false);

            var l_service = l_ServiceProvider.GetService(typeof(IVsDebugger));
#pragma warning disable VSTHRD010
            var l_IVsDebugger = (IVsDebugger)l_service;
            if (l_IVsDebugger != null)
            {
                l_IVsDebugger.LaunchDebugTargets(1u, intPtr);
            }
#pragma warning restore VSTHRD010
        }

        public static List<string> BuildUnitTestAndFindExeFile(string i_zigExePathString, string i_sourceCodePathString, string i_testNameString)
        {
            List<string> r_createdFileList = new List<string>();
            if (string.IsNullOrWhiteSpace(i_zigExePathString))
            {
                return r_createdFileList;
            }

            var l_zigFolderString = System.Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            using (FileSystemWatcher watcher = new FileSystemWatcher(Path.Combine(l_zigFolderString, "AppData\\Local\\zig\\o")))
            {
                watcher.NotifyFilter = NotifyFilters.FileName;// | NotifyFilters.CreationTime;
                watcher.Filter = "*.exe";
                watcher.IncludeSubdirectories = true;
                watcher.Renamed += (sender, e) =>
                {
                    r_createdFileList.Add(e.FullPath);
                };
                watcher.Created += (sender, e) =>
                {
                    r_createdFileList.Add(e.FullPath);
                };
                watcher.EnableRaisingEvents = true;
                var l_argString = "test --test-no-exec --test-filter \"" + i_testNameString + "\" " + i_sourceCodePathString;
                RunProcess(i_zigExePathString, l_argString);
                watcher.EnableRaisingEvents = false;
            }
            return r_createdFileList;
        }

        public static ResultSet RunTestProcess(string i_exeString, string i_argumentsString)
        {
            ResultSet r_ResultSet = new ResultSet();
            try
            {
                if (string.IsNullOrWhiteSpace(i_exeString))
                {
                    r_ResultSet.m_ErrorMessage = "Failed to start the test. Please check PATH to Zig Tool-chain.";
                    return r_ResultSet;
                }

                if (!string.IsNullOrEmpty(i_argumentsString))
                {
                    var l_testProcess = new System.Diagnostics.Process();
                    l_testProcess.StartInfo.FileName = i_exeString;
                    l_testProcess.StartInfo.Arguments = i_argumentsString;
                    l_testProcess.StartInfo.UseShellExecute = false;
                    l_testProcess.StartInfo.RedirectStandardError = true;
                    l_testProcess.StartInfo.RedirectStandardOutput = true;
                    l_testProcess.Start();

                    string l_standardOutput = l_testProcess.StandardOutput.ReadToEnd();
                    string l_standardError = l_testProcess.StandardError.ReadToEnd();
                    l_testProcess.WaitForExit();

                    ZigTestParseResult l_ParseResult = CoreServices.TestResultParser.Parse(new ZigTestProcessOutput
                    {
                        StandardOutput = l_standardOutput,
                        StandardError = l_standardError,
                        ExitCode = l_testProcess.ExitCode
                    });

                    r_ResultSet = ConvertParseResult(l_ParseResult);
                }
            }
            catch (Exception l_Exception)
            {
                r_ResultSet.m_ErrorMessage = "Failed to start the test. Please check PATH to Zig Tool-chain." + Environment.NewLine + l_Exception.ToString();
            }

            return r_ResultSet;
        }

        static ResultSet ConvertParseResult(ZigTestParseResult parseResult)
        {
            ResultSet resultSet = new ResultSet
            {
                m_resultString = parseResult.Output,
                m_ErrorMessage = parseResult.DiagnosticMessage
            };

            switch (parseResult.Outcome)
            {
                case ZigTestOutcome.Passed:
                    resultSet.m_TestOutcome = TestOutcome.Passed;
                    break;
                case ZigTestOutcome.NotFound:
                    resultSet.m_TestOutcome = TestOutcome.NotFound;
                    break;
                case ZigTestOutcome.Skipped:
                    resultSet.m_TestOutcome = TestOutcome.Skipped;
                    break;
                default:
                    resultSet.m_TestOutcome = TestOutcome.Failed;
                    break;
            }

            return resultSet;
        }

        public static void RunProcess(string i_exeString, string i_argumentsString)
        {
            try
            {
                if (!string.IsNullOrEmpty(i_argumentsString))
                {
                    var l_testProcess = new System.Diagnostics.Process();
                    l_testProcess.StartInfo.FileName = i_exeString;
                    l_testProcess.StartInfo.Arguments = i_argumentsString;
                    l_testProcess.StartInfo.UseShellExecute = false;
                    l_testProcess.StartInfo.RedirectStandardError = true;
                    l_testProcess.StartInfo.RedirectStandardOutput = false;
                    l_testProcess.Start();
                    l_testProcess.StandardError.ReadToEnd();
                }
            }
            catch
            {
            }
        }

        public class ResultSet
        {
            public TestOutcome m_TestOutcome = TestOutcome.Failed;
            public string m_resultString = "";
            public string? m_ErrorMessage = null;
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenThread(int dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

        [DllImport("kernel32.dll")]
        private static extern uint SuspendThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        private static extern uint ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        private const int THREAD_SUSPEND_RESUME = 0x0002;

        private static void SuspendProcess(System.Diagnostics.Process i_Process)
        {
            foreach (System.Diagnostics.ProcessThread thread in i_Process.Threads)
            {
                IntPtr pOpenThread = OpenThread(THREAD_SUSPEND_RESUME, false, (uint)thread.Id);

                if (pOpenThread != IntPtr.Zero)
                {
                    SuspendThread(pOpenThread);
                    CloseHandle(pOpenThread);
                }
            }
        }

        private static void ResumeProcess(System.Diagnostics.Process i_Process)
        {
            foreach (System.Diagnostics.ProcessThread thread in i_Process.Threads)
            {
                IntPtr pOpenThread = OpenThread(THREAD_SUSPEND_RESUME, false, (uint)thread.Id);

                if (pOpenThread != IntPtr.Zero)
                {
                    ResumeThread(pOpenThread);
                    CloseHandle(pOpenThread);
                }
            }
        }
    }
}
