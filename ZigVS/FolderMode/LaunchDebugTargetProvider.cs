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

namespace ZigVS
{
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.Workspace;
    using Microsoft.VisualStudio.Workspace.Debug;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using static Microsoft.VisualStudio.VSConstants;

    [ExportLaunchDebugTarget(
        LaunchDebugTargetProviderOptions.IsRuntimeSupportContext,
        c_GuidString,
        new[] { Parameter.c_windowsExecutableFileExtension },
        ProviderPriority.Lowest)]
    public sealed class LaunchDebugTargetProvider : ILaunchDebugTargetProvider
    {
        const string c_GuidString = "{C5C91049-C77B-45D1-AA91-77F19F341D39}";

        public void LaunchDebugTarget(
            IWorkspace i_IWorkspace,
            IServiceProvider i_IServiceProvider,
            DebugLaunchActionContext i_DebugLaunchActionContext)
        {
#pragma warning disable VSTHRD002       
            _ = ExecutePreDebugCommandAsync(i_IWorkspace.Location).Result;
            _ = LaunchDebuggerAsync(i_IWorkspace, i_IServiceProvider, i_DebugLaunchActionContext).Result;
#pragma warning restore VSTHRD002
        }

        private static async Task<string> ExecutePreDebugCommandAsync(string i_WorkingDirectoryString)
        {
            try
            {
                var l_FolderModeOptions = await FolderModeOptions.GetLiveInstanceAsync();
                if (!string.IsNullOrEmpty(l_FolderModeOptions.PreDebugCommand))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = l_FolderModeOptions.PreDebugCommand,
                        Arguments = l_FolderModeOptions.PreDebugCommandArguments,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        WorkingDirectory = i_WorkingDirectoryString
                    };

                    try
                    {
                        using (Process process = new Process { StartInfo = startInfo })
                        {
                            process.Start();
                            process.WaitForExit();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"error: {ex.Message}");
                    }
                }
            }
            catch { }
            return "";
        }

        private static async Task<string> LaunchDebuggerAsync(
            IWorkspace i_IWorkspace,
            IServiceProvider i_IServiceProvider,
            DebugLaunchActionContext i_DebugLaunchActionContext)
        {
            try
            {
                var l_ProjectConfigurationService = i_IWorkspace.GetService<IProjectConfigurationService>();
                if (l_ProjectConfigurationService != null)
                {
#pragma warning disable CS8600, 8602
                    string l_absoluteTargetPath = (string)i_DebugLaunchActionContext.LaunchConfiguration["target"];
                    string l_locationPath = Path.GetDirectoryName(l_absoluteTargetPath);
                    string l_workspacePath = i_IWorkspace.Location;

                    string l_relativeTargetPath = l_absoluteTargetPath.Substring(l_workspacePath.Length + 1);
                    var l_Configuration = l_ProjectConfigurationService.GetActiveProjectBuildConfiguration(
                        new ProjectTargetFileContext(l_relativeTargetPath/*Build.c_buildFileName*/));
#pragma warning restore cs8602

                    string l_intPath = Build.GetIntermeditatePath(l_locationPath, l_Configuration);
                    string l_outPath = Build.GetOutputPath(l_locationPath, l_Configuration);

                    string l_ExeString = BuildInfo.GetExeName(l_absoluteTargetPath, l_intPath);
                    if (string.IsNullOrEmpty(l_ExeString))
                    {
                        l_ExeString = Path.GetFileName(l_locationPath.TrimEnd(Path.DirectorySeparatorChar));
                    }

                    var l_FolderModeOptions = await FolderModeOptions.GetLiveInstanceAsync();

                    string l_optionString = "";
                    if (l_FolderModeOptions.DebugEngine == DebugEngine.MIEngine)
                    {
                        var l_filePathString = Path.Combine(l_locationPath, l_FolderModeOptions.LaunchOptionFileName);
                        try
                        {
                            l_optionString = File.ReadAllText(l_filePathString);
                        }
                        catch
                        {
                            Common.OutputWindowPane.OutputString("Could not read the file:" + l_filePathString + Environment.NewLine);
                        }
                    }

                    var l_VsDebugTargetInfo = new VsDebugTargetInfo
                    {
                        dlo = DEBUG_LAUNCH_OPERATION.DLO_CreateProcess,
                        bstrCurDir = l_outPath,
                        bstrExe = Path.Combine(l_outPath, l_ExeString),
                        bstrArg = l_FolderModeOptions.Arguments,
                        bstrEnv = null,
                        /*       env.OverrideProcessEnvironment()
                               .PrependToPathInEnviroment(
                                   package.GetDepsPath(profile),
                                   package.GetTargetPath(profile),
                                   ToolChainServiceExtensions.GetLibPath(),
                                   ToolChainServiceExtensions.GetBinPath()).ToEnvironmentBlock(),*/
                        bstrOptions = l_optionString,
                        bstrPortName = null,
                        bstrMdmRegisteredName = null,
                        bstrRemoteMachine = null,
                        cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf<VsDebugTargetInfo>(),
                        grfLaunch = (uint)(__VSDBGLAUNCHFLAGS.DBGLAUNCH_Silent | __VSDBGLAUNCHFLAGS.DBGLAUNCH_StopDebuggingOnEnd),
                        fSendStdoutToOutputWindow = 1,  // 1 = route stdout and stderr to the debug-output window
                        clsidCustom = l_FolderModeOptions.DebugEngine == DebugEngine.WindowsNative ?
                                DebugEnginesGuids.NativeOnly_guid : Microsoft.VisualStudio.Project.ProjectConfig.GetMIEngineGuid()
                    };

                    VsShellUtilities.LaunchDebugger(i_IServiceProvider, l_VsDebugTargetInfo);
                }
            }
            catch { }
            return "";
        }

        public bool SupportsContext_back(IWorkspace i_IWorkspace, string i_targetFilePathString)
        {
            bool r_resultBool = false;

            r_resultBool = Path.GetFileName(i_targetFilePathString) == Parameter.c_buildFileName;

            /* Can not use Async in UI thread
                        var l_IIndexWorkspaceService = i_IWorkspace.GetIndexWorkspaceService();
                        var l_dataValuesTask = l_IIndexWorkspaceService.GetFileDataValuesAsync<PropertySettings>(i_targetFilePathString, DebugLaunchActionContext.ContextTypeGuid);
                        var l_FileDataResultEnumerator= l_dataValuesTask.GetAwaiter().GetResult();
                        foreach(var l_FileDataResult in l_FileDataResultEnumerator)
                        {
                            try
                            {
                                var l_PropertySettings = l_FileDataResult.Value as PropertySettings;
                                var l_value = l_PropertySettings.GetValue<string>(LaunchConfigurationConstants.ProjectTargetKey);
                                r_resultBool = l_value == ContentDefinition.c_languageName;
                            }
                            catch
                            {
                            }
                        }
            */
            return r_resultBool;
        }

        public bool SupportsContext(IWorkspace i_IWorkspace, string i_targetFilePathString)
        {
            bool r_resultBool = false;
            var l_IIndexWorkspaceService = i_IWorkspace.GetIndexWorkspaceService();
            if (l_IIndexWorkspaceService != null)
            {
                var l_dataValuesTask =
                    l_IIndexWorkspaceService.GetFileDataValuesAsync<PropertySettings>(i_targetFilePathString, DebugLaunchActionContext.ContextTypeGuid);
                var l_FileDataResultEnumerator = i_IWorkspace.JTF.Run(() =>
                { return l_IIndexWorkspaceService.GetFileDataValuesAsync<PropertySettings>(i_targetFilePathString, DebugLaunchActionContext.ContextTypeGuid); });
                foreach (var l_FileDataResult in l_FileDataResultEnumerator)
                {
                    try
                    {
                        var l_PropertySettings = l_FileDataResult.Value as PropertySettings;
                        if (l_PropertySettings != null)
                        {
                            var l_value = l_PropertySettings.GetValue<string>(LaunchConfigurationConstants.ProjectTargetKey);
                            r_resultBool = l_value == Parameter.c_languageName;
                        }
                    }
                    catch
                    {
                    }
                }
            }
            return r_resultBool;
        }
    }
}