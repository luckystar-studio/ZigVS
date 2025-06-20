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
    using System.IO.Compression;
    using System.Net.Http;
    using System.Threading;

#nullable enable
#pragma warning disable VSTHRD002
    public class InstallerBase
    {
        protected volatile bool m_IsRunningBool = false;
        protected volatile bool m_IsCancelBool = false;
        protected Thread? m_Thread = null;

        public InstallerBase()
        {
        }

        public enum State
        {
            None,
            Installing,
            Cancelling
        }

        public State GetState()
        {
            State r_State = State.None;
            if (m_IsRunningBool && !m_IsCancelBool)
            {
                r_State = State.Installing;
            }
            else if (m_IsCancelBool)
            {
                r_State = State.Cancelling;
            }
            return r_State;
        }

        public void CancelInstallation()
        {
            if (m_IsRunningBool)
            {
                m_IsCancelBool = true;
            }
        }

        protected void IfCanceledThrowException()
        {
            if (m_IsCancelBool)
            {
                throw new Exception("Canceled");
            }
        }

        protected void SetStatusInstalling()
        {
            m_IsCancelBool = false;
            m_IsRunningBool = true;
        }

        protected void SetStatusFinished()
        {
            m_IsRunningBool = false;
            m_IsCancelBool = false;
        }

        protected void RunProcess(string i_DirectoryString, string i_commandString, string i_optionString)
        {
            //           Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                Common.OutputWindowPane.Show();
                Common.OutputWindowPane.OutputString("Starting package installation --------------------" + Environment.NewLine);

                if (!Directory.Exists(i_DirectoryString))
                {
                    Directory.CreateDirectory(i_DirectoryString);
                }

                Common.OutputWindowPane.OutputString(Environment.NewLine + i_DirectoryString + " > " + i_commandString + ' ' + i_optionString + Environment.NewLine);

                IfCanceledThrowException();
                ProcessStartInfo l_ProcessStartInfo = new ProcessStartInfo();
                l_ProcessStartInfo.WorkingDirectory = i_DirectoryString;
                l_ProcessStartInfo.FileName = i_commandString;
                l_ProcessStartInfo.Arguments = i_optionString;
                l_ProcessStartInfo.RedirectStandardInput = false;
                l_ProcessStartInfo.RedirectStandardOutput = true;
                l_ProcessStartInfo.RedirectStandardError = true;
                l_ProcessStartInfo.UseShellExecute = false;
                l_ProcessStartInfo.CreateNoWindow = true;

                Process l_Process = new Process();
                l_Process.StartInfo = l_ProcessStartInfo;
                l_Process.OutputDataReceived += (sender, args) => Common.OutputWindowPane.OutputString(args.Data);
                l_Process.ErrorDataReceived += (sender, args) => Common.OutputWindowPane.OutputString(args.Data);
                if (l_Process.Start())
                {
                    l_Process.BeginOutputReadLine();
                    l_Process.BeginErrorReadLine();
                    l_Process.WaitForExit();
                }
                Common.OutputWindowPane.OutputString(Environment.NewLine + "Installation is finished" + Environment.NewLine);
            }
            catch (Exception i_ex)
            {
                Common.OutputWindowPane.OutputString(i_ex.Message + Environment.NewLine);
            }
            SetStatusFinished();
        }

        protected MemoryStream Download(string i_UrlString)
        {
            MemoryStream r_MemoryStream = new MemoryStream();

            uint l_cookie = 0;
            Common.Statusbar.Progress(l_cookie, 1, "", 0, 0);

            var l_messageString = "Downloading " + i_UrlString + " to memory…";
            Common.OutputWindowPane.OutputString(l_messageString + Environment.NewLine);

            using (var l_HttpClient = new HttpClient())
            {
                var l_HttpResponceMessage = l_HttpClient.GetAsync(i_UrlString, HttpCompletionOption.ResponseHeadersRead).Result;

                var contentLength = l_HttpResponceMessage.Content.Headers.ContentLength;

                using (var contentStream = l_HttpResponceMessage.Content.ReadAsStreamAsync().Result)
                {
                    var l_bufferByte = new byte[8192];
                    int l_bytesReadInt = 0;
                    long l_totalBytesReadLong = 0;

                    long l_lastProgressLong = -1;
                    while ((l_bytesReadInt = contentStream.ReadAsync(l_bufferByte, 0, l_bufferByte.Length).Result) > 0)
                    {
                        r_MemoryStream.WriteAsync(l_bufferByte, 0, l_bytesReadInt).Wait();
                        l_totalBytesReadLong += l_bytesReadInt;

                        if (contentLength.HasValue)
                        {
                            Common.OutputWindowPane.OutputProgress(l_totalBytesReadLong, contentLength.Value, ref l_lastProgressLong);
                            Common.Statusbar.Progress(l_cookie, 1, l_messageString, (uint)l_totalBytesReadLong, (uint)contentLength.GetValueOrDefault());
                        }
                        IfCanceledThrowException();
                    }
                }
                Common.Statusbar.Progress(l_cookie, 1, "", 0, 0);
                Common.OutputWindowPane.OutputString("Download complete." + Environment.NewLine);
            }

            return r_MemoryStream;
        }

        protected String? ExtractZip(MemoryStream i_zipMemoryStream, string i_destinationPathString)
        {
            String? r_pathString = null;

            uint l_cookie = 0;
            Common.Statusbar.Progress(l_cookie, 1, "", 0, 0);

            var l_messageString = "Extracting files to " + i_destinationPathString + "…";

            Common.OutputWindowPane.OutputString(l_messageString + Environment.NewLine);

            using (var l_ZipArchive = new ZipArchive(i_zipMemoryStream, ZipArchiveMode.Read))
            {
                var l_totalEntriesLong = l_ZipArchive.Entries.Count;
                int l_entryIndexLong = 0;

                long l_lastProgressLong = -1;
                foreach (var l_ZigArchiveEntries in l_ZipArchive.Entries)
                {
                    l_entryIndexLong++;
                    var l_fullPathString = Path.Combine(i_destinationPathString, l_ZigArchiveEntries.FullName);

                    if (string.IsNullOrEmpty(l_ZigArchiveEntries.Name))
                    {
                        if (r_pathString == null)
                        {
                            if (Directory.Exists(l_fullPathString))
                            {
                                throw new Exception("Directory already exists: " + l_fullPathString + Environment.NewLine +
                                                    "Please select an empty path to install." + Environment.NewLine);
                            }
                            else
                            {
                                r_pathString = Path.GetDirectoryName(l_fullPathString);
                            }
                        }
                        else
                        {
                            Directory.CreateDirectory(l_fullPathString);
                        }
                    }
                    else
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(l_fullPathString));

                        if (File.Exists(l_fullPathString))
                        {
                            File.Delete(l_fullPathString);
                        }
                        using (var fileStream = new FileStream(l_fullPathString, FileMode.Create, FileAccess.Write))
                        {
                            l_ZigArchiveEntries.Open().CopyTo(fileStream);
                        }
                    }

                    Common.OutputWindowPane.OutputProgress(l_entryIndexLong, l_totalEntriesLong, ref l_lastProgressLong);
                    Common.Statusbar.Progress(l_cookie, 1, l_messageString, (uint)l_entryIndexLong, (uint)l_totalEntriesLong);

                    IfCanceledThrowException();
                }
                Common.Statusbar.Progress(l_cookie, 1, "", 0, 0);
                Common.OutputWindowPane.OutputString("Extracting complete." + Environment.NewLine);
            }
            return r_pathString;
        }
    }
#pragma warning restore VSTHRD002
}
