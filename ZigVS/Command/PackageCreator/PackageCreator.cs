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
    using System.Windows;

    public class PackageCreator : InstallerBase
    {
        public void Start(string i_directoryString, string i_packageNameString, bool i_openFlagBool)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            if (m_IsRunningBool)
            {
                Common.OutputWindowPane.OutputString("Creation task is still running" + Environment.NewLine);
            }
            else
            {
                SetStatusInstalling();
                CreatePackage(i_directoryString, i_packageNameString, i_openFlagBool);
                /*   m_Thread = new Thread(new ThreadStart(() => CreatePackage(
                        i_directoryString, i_packageNameString, i_openFlagBool)));
                    m_Thread.Start();*/

                if (i_openFlagBool)
                {
                    EnvDTE.DTE l_DTE = (EnvDTE.DTE)((IServiceProvider)ZigVSPackage.GetInstance()).GetService(typeof(EnvDTE.DTE));
                    l_DTE?.ExecuteCommand("File.OpenFolder", Path.Combine(i_directoryString, i_packageNameString));
                }
            }
        }

        void CreatePackage(string i_directoryString, string i_packageNameString, bool i_openFlagBool)
        {
            try
            {
                Common.OutputWindowPane.Show();
                Common.OutputWindowPane.OutputString("Starting package creation --------------------" + Environment.NewLine);

                string l_pathString = Path.Combine(i_directoryString, i_packageNameString);
                if (Directory.Exists(l_pathString))
                {
                    CancelInstallation();
                    Common.OutputWindowPane.OutputString("This directory already exists. " + l_pathString + Environment.NewLine);
                }
                else
                {
                    Common.OutputWindowPane.OutputString("Creating directory: " + l_pathString + Environment.NewLine);
                    Directory.CreateDirectory(l_pathString);

                    if (!Directory.Exists(l_pathString))
                    {
                        Common.OutputWindowPane.OutputString("Could not find directory: " + l_pathString + Environment.NewLine);
                    }
                    else
                    {
                        IfCanceledThrowException();

                        ProcessStartInfo l_ProcessStartInfo = new ProcessStartInfo();
                        l_ProcessStartInfo.WorkingDirectory = l_pathString;
                        l_ProcessStartInfo.FileName = Parameter.c_compilerFileName;
                        l_ProcessStartInfo.Arguments = "init";
                        l_ProcessStartInfo.RedirectStandardInput = false;
                        l_ProcessStartInfo.RedirectStandardOutput = true;
                        l_ProcessStartInfo.RedirectStandardError = true;
                        l_ProcessStartInfo.UseShellExecute = false;
                        l_ProcessStartInfo.CreateNoWindow = true;

                        var l_Process = new System.Diagnostics.Process();
                        l_Process.StartInfo = l_ProcessStartInfo;
                        l_Process.OutputDataReceived += (sender, args) => Common.OutputWindowPane.OutputString(args.Data);
                        l_Process.ErrorDataReceived += (sender, args) => Common.OutputWindowPane.OutputString(args.Data);
                        if (l_Process.Start())
                        {
                            l_Process.BeginOutputReadLine();
                            l_Process.BeginErrorReadLine();
                            l_Process.WaitForExit();
                        }
                    }
                }
            }
            catch (Exception i_ex)
            {
                Common.OutputWindowPane.OutputString(i_ex.Message + Environment.NewLine);
            }

            SetStatusFinished();

#pragma warning disable VSTHRD001
            Application.Current.Dispatcher.Invoke(() =>
            {
                PackageCreatorWindowControl.GetInstance()?.Reset();
            });
#pragma warning restore VSTHRD001
        }
    }
}
