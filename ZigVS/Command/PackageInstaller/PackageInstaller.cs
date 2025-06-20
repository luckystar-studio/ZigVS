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
    using System.Threading;

    public class PackageInstaller : InstallerBase
    {
        public void StartCommand(string i_DirectoryString, string i_commandString, string i_optionString)
        {
            if (m_IsRunningBool)
            {
                Common.OutputWindowPane.OutputString("Install task is still running" + Environment.NewLine);
            }
            else
            {
                SetStatusInstalling();

                m_Thread = new Thread(new ThreadStart(() => RunProcess2(i_DirectoryString, i_commandString, i_optionString)));
                m_Thread.Start();
            }
        }
        protected void RunProcess2(string i_DirectoryString, string i_commandString, string i_optionString)
        {
            RunProcess(i_DirectoryString, i_commandString, i_optionString);
            PackageInstallerWindowControl.GetInstance()?.Reset();
        }

        public void StartUnzip(string i_DirectoryString, string i_UrlString)
        {
            if (m_IsRunningBool)
            {
                Common.OutputWindowPane.OutputString("Install task is still running" + Environment.NewLine);
            }
            else
            {
                SetStatusInstalling();
                m_Thread = new Thread(new ThreadStart(() => DownloadAndUnzip(i_DirectoryString, i_UrlString)));
                m_Thread.Start();
            }
        }

        void DownloadAndUnzip(string i_DirectoryString, string i_UrlString)
        {
            try
            {
                Common.OutputWindowPane.Show();
                Common.OutputWindowPane.OutputString("Starting package installation --------------------" + Environment.NewLine);

                IfCanceledThrowException();
                var l_zipMemoryStream = Download(i_UrlString);
                if (l_zipMemoryStream != null)
                {
                    IfCanceledThrowException();
                    // case the path does not include '/'
                    i_DirectoryString = i_DirectoryString.TrimEnd('\\') + '\\';
                    var l_toolChainPathString = ExtractZip(l_zipMemoryStream, i_DirectoryString);
                    if (!string.IsNullOrEmpty(l_toolChainPathString))
                    {
                        IfCanceledThrowException();
                        Common.OutputWindowPane.OutputString("Installation finished --------------------" + Environment.NewLine);
                    }
                }
            }
            catch (Exception i_ex)
            {
                Common.OutputWindowPane.OutputString(i_ex.Message + Environment.NewLine);
            }

            SetStatusFinished();

            PackageInstallerWindowControl.GetInstance()?.Reset();
        }
    }
}