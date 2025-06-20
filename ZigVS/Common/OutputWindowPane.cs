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
    using EnvDTE;
    using EnvDTE80;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using System;

#nullable enable
#pragma warning disable VSTHRD010
    public class OutputWindowPane
    {
        static IVsOutputWindowPane? s_IVsOutputWindowPane = null;

        public OutputWindowPane()
        {
            CheckIVsOutputWindowPane();
        }

        private static void CheckIVsOutputWindowPane()
        {
            if (s_IVsOutputWindowPane == null)
            {
                s_IVsOutputWindowPane = Package.GetGlobalService(typeof(SVsGeneralOutputWindowPane)) as IVsOutputWindowPane;
                s_IVsOutputWindowPane?.Activate();
            }
        }

        public static void Show()
        {
            var l_DTE2 = (DTE2)Package.GetGlobalService(typeof(DTE));
            var l_Window = l_DTE2.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
            l_Window.Visible = true;

            CheckIVsOutputWindowPane();

            /* another way to show the output window, but it is not needed as the above works fine
            var l_IVsUIShell = (IVsUIShell)Package.GetGlobalService(typeof(SVsUIShell));
             var l_outputWindowGuid = VSConstants.StandardToolWindows.Output;
             l_IVsUIShell.FindToolWindow((uint)__VSFINDTOOLWIN.FTW_fForceCreate, ref l_outputWindowGuid, out var l_IVsWindowFrame);
             l_IVsWindowFrame?.Show();

            // Show the general pane  
            var l_IVsOutputWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            if (l_IVsOutputWindow!=null && m_IVsOutputWindowPane == null)
            {
                System.Guid l_generalPaneGuid = VSConstants.GUID_OutWindowGeneralPane;
                l_IVsOutputWindow.GetPane(ref l_generalPaneGuid, out IVsOutputWindowPane l_generalIVsOutputWindowPane);

                if (l_generalIVsOutputWindowPane != null)
                {
                    l_generalIVsOutputWindowPane.Activate();
                }
                else
                {
                }   
            }*/

            Clear();
        }

        public static void Clear()
        {
            CheckIVsOutputWindowPane();
            if (s_IVsOutputWindowPane != null)
            {
                s_IVsOutputWindowPane.Clear();
                OutputString(Environment.NewLine);
            }
        }

        public static void OutputString(string i_messageString)
        {
            CheckIVsOutputWindowPane();
            if (s_IVsOutputWindowPane != null)
            {
                s_IVsOutputWindowPane.OutputStringThreadSafe(i_messageString);
            }
        }

        public static void OutputProgress(long i_currentInt, long i_totalInt, ref long i_lastInt)
        {
            var l_progressInt = (int)(10 * i_currentInt / i_totalInt);
            if (i_lastInt != l_progressInt)
            {
                Common.OutputWindowPane.OutputString("[" + new string('■', (int)l_progressInt) + new string(' ', (int)(10 - l_progressInt)) + ']' + Environment.NewLine);
                i_lastInt = l_progressInt;
            }
        }
    }
#pragma warning restore VSTHRD010
}
