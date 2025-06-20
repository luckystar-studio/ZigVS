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
    using Newtonsoft.Json.Linq;
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;

#pragma warning disable VSTHRD002
    public class ToolchainInstaller : InstallerBase
    {
        public void Start(string i_ZigToolChainString, string i_ZLSString, string i_CPUString, string i_DirectoryString, bool i_EnvBool)
        {
            if (m_IsRunningBool)
            {
                Common.OutputWindowPane.OutputString("Install task is still running" + Environment.NewLine);
            }
            else
            {
                SetStatusInstalling();
                m_Thread = new Thread(new ThreadStart(() => InstallToolChain(
                    i_ZigToolChainString, i_ZLSString, i_CPUString, i_DirectoryString, i_EnvBool)));
                m_Thread.Start();
            }
        }

        void InstallToolChain(string i_ZigVersionString, string i_ZLSVersionString, string i_CPUString, string i_DirectoryString, bool i_EnvBool)
        {
            try
            {
                Common.OutputWindowPane.Show();
                Common.OutputWindowPane.OutputString("Starting toolchain installation --------------------" + Environment.NewLine);

                var l_ZigToolchainUrlString = FindZigUrl(i_ZigVersionString, i_CPUString);
                var l_ZLSUrlString = FindZLSUrl(i_ZLSVersionString, i_CPUString);

                IfCanceledThrowException();
                var l_toolChainZipMemoryStream = Download(l_ZigToolchainUrlString);
                if (l_toolChainZipMemoryStream != null)
                {
                    IfCanceledThrowException();
                    // case the path does not include '/'
                    var l_DirectoryString = i_DirectoryString.TrimEnd('\\') + '\\';
                    var l_toolChainPathString = ExtractZip(l_toolChainZipMemoryStream, l_DirectoryString) ?? "";
                    if (!string.IsNullOrEmpty(l_toolChainPathString))
                    {
                        IfCanceledThrowException();
                        var l_ZLSZipMemoryStream = Download(l_ZLSUrlString);
                        if (l_ZLSZipMemoryStream != null)
                        {
                            IfCanceledThrowException();
                            var l_ZLSPathString = ExtractZip(l_ZLSZipMemoryStream, l_toolChainPathString);

                            if (i_EnvBool)
                            {
                                Utilities.SetPATHEnvironmentValue(l_toolChainPathString);
                            }

                            Common.OutputWindowPane.OutputString("Installation Complete --------------------" + Environment.NewLine);
                            Common.OutputWindowPane.OutputString("You might need to restart Visual Studio." + Environment.NewLine);
                        }
                    }
                }
            }
            catch (Exception i_ex)
            {
                Common.OutputWindowPane.OutputString(i_ex.Message + Environment.NewLine);
            }

            SetStatusFinished();

            ToolchainInstallerWindowControl.GetInstance()?.Reset();
        }

        string FindZigUrl(string i_versionNameString, string i_CPUString)
        {
            string r_ZigToolchainUrlString = "https://ziglang.org/builds/zig-windows-" + i_CPUString + "-0.14.1.zip";
            try
            {
                var l_vesionJsonMemoryStream = Download("https://ziglang.org/download/index.json");
                if (l_vesionJsonMemoryStream != null)
                {
                    l_vesionJsonMemoryStream.Position = 0;
                    var l_reader = new StreamReader(l_vesionJsonMemoryStream, Encoding.UTF8);
                    var l_jsonString = l_reader.ReadToEndAsync().Result;
                    var l_JObject = JObject.Parse(l_jsonString);
                    var l_JToken = l_JObject.SelectToken("$['" + i_versionNameString + "']['" + i_CPUString + "-windows']['tarball']");
                    if (l_JToken != null)
                    {
                        var l_tmpString = l_JToken.ToObject<String>();
                        if (l_tmpString != null)
                        {
                            r_ZigToolchainUrlString = l_tmpString;
                        }
                    }
                }
            }
            catch { }

            return r_ZigToolchainUrlString;
        }

        string FindZLSUrl(string i_versionNameString, string i_CPUString)
        {
            return "https://github.com/zigtools/zls/releases/download/" + i_versionNameString + "/zls-" + i_CPUString + "-windows.zip";
        }
    }
#pragma warning restore VSTHRD002
}
