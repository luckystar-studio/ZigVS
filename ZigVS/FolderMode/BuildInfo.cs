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
    using System;
    using System.Diagnostics;
    using System.IO;

#nullable enable
#pragma warning disable VSTHRD002
    public class BuildInfo
    {
        static FileSystemWatcher? s_FileSystemWatcher = null;
        static string s_BuildInfoString = ""; // ToDo atomic
        static string s_BuildDotZigPathString = "";

        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath == s_BuildDotZigPathString)
            {
                s_BuildInfoString = "";
            }
        }

        public static string GetExeName(string? i_buildFilePath, string? i_intDirectory)
        {
            try
            {
                const string c_BuildInfo = "BuildInfo";

                if (i_buildFilePath != null &&
                    (i_buildFilePath != s_BuildDotZigPathString ||
                    s_FileSystemWatcher == null))
                {
                    s_BuildDotZigPathString = i_buildFilePath;
                    s_BuildInfoString = "";

                    s_FileSystemWatcher = new FileSystemWatcher(Path.GetDirectoryName(i_buildFilePath));

                    s_FileSystemWatcher.NotifyFilter = NotifyFilters.CreationTime
                                                    | NotifyFilters.DirectoryName
                                                    | NotifyFilters.FileName
                                                    | NotifyFilters.LastAccess
                                                    | NotifyFilters.LastWrite
                                                    | NotifyFilters.Size;

                    s_FileSystemWatcher.Changed += OnChanged;
                    s_FileSystemWatcher.Created += OnChanged;
                    s_FileSystemWatcher.Deleted += OnChanged;
                    s_FileSystemWatcher.Renamed += OnChanged;
                }

                if (s_BuildInfoString == "")
                {
                    var l_FolderModelOption = FolderModeOptions.GetLiveInstanceAsync().Result;
                    if (File.Exists(i_buildFilePath) && (l_FolderModelOption != null))
                    {
                        string l_BuildFile = File.ReadAllText(i_buildFilePath) + l_FolderModelOption.BuildInfoCapturer;

                        string l_BuildInfoPathString = System.IO.Path.Combine(i_intDirectory, c_BuildInfo + Parameter.c_fileExtension);
                        var l_toolPathString = Utilities.GetToolPathFromEnvironmentValue();

                        if (File.Exists(l_BuildInfoPathString))
                        {
                            File.Delete(l_BuildInfoPathString);
                        }
                        File.WriteAllText(l_BuildInfoPathString, l_BuildFile);
                        {
                            var l_BuildProcess = new Process
                            {
                                StartInfo = new ProcessStartInfo
                                {
                                    FileName = Path.Combine(l_toolPathString, l_FolderModelOption.ToolPath),
                                    WorkingDirectory = i_intDirectory,
                                    Arguments = l_FolderModelOption.BuildCommand_buildexe + " " + l_BuildInfoPathString,
                                    UseShellExecute = false,
                                    RedirectStandardOutput = true,
                                    RedirectStandardError = true,
                                    CreateNoWindow = true
                                }
                            };
                            l_BuildProcess.Start();
                            var l_standerdOutString = l_BuildProcess.StandardOutput.ReadToEnd();
                            var l_StandardErrorString = l_BuildProcess.StandardError.ReadToEnd();

                            l_BuildProcess.WaitForExit();

                            var l_filePathString = Path.Combine(i_intDirectory, c_BuildInfo + Parameter.c_windowsExecutableFileExtension);
                            if (!File.Exists(l_filePathString))
                            {
                                Common.OutputWindowPane.OutputString("Error --- Could not get bold output file name.-----------------" + Environment.NewLine);
                                Common.OutputWindowPane.OutputString("Zig Tool chain version does not match with Output name capturer code." + Environment.NewLine);
                                Common.OutputWindowPane.OutputString("Use correct version of Zig Tool chain or Edit Output name capturer code from Option Menu." + Environment.NewLine);
                                Common.OutputWindowPane.OutputString(l_standerdOutString + Environment.NewLine);
                                Common.OutputWindowPane.OutputString(l_StandardErrorString + Environment.NewLine);
                                Common.OutputWindowPane.OutputString("--------------------------" + Environment.NewLine);
                            }
                            else
                            {
                                var l_Process = new Process
                                {
                                    StartInfo = new ProcessStartInfo
                                    {
                                        FileName = Path.Combine(i_intDirectory, c_BuildInfo + Parameter.c_windowsExecutableFileExtension),
                                        Arguments = "",
                                        WorkingDirectory = i_intDirectory,
                                        UseShellExecute = false,
                                        RedirectStandardOutput = true,
                                        RedirectStandardError = true,
                                        CreateNoWindow = true
                                    }
                                };
                                l_Process.Start();
                                s_BuildInfoString = l_Process.StandardOutput.ReadToEnd();
                                l_Process.WaitForExit();
                            }
                        }
                    }
                }
            }
            catch { }
            return s_BuildInfoString;
        }
    }
#pragma warning restore VSTHRD002

}
