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

namespace ZigVS.Command
{
#nullable enable
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using Task = System.Threading.Tasks.Task;

    internal sealed class FormattingCommand
    {
        static FormattingCommand? s_Instance;

        private FormattingCommand(OleMenuCommandService? i_OleMenuCommandService)
        {
            if (i_OleMenuCommandService != null)
            {
                i_OleMenuCommandService = i_OleMenuCommandService ?? throw new ArgumentNullException(nameof(i_OleMenuCommandService));

                var l_CommandID = new CommandID(CommandDefinition.s_CommandSetGuid, (int)CommandDefinition.CommandId.Formatting);
                var l_MenuCommand = new MenuCommand(this.Execute, l_CommandID);
                i_OleMenuCommandService.AddCommand(l_MenuCommand);
            }
        }

        public static Task InitializeAsync()
        {
            // Switch to the main thread - the call to AddCommand in TestCommand's constructor requires
            // the UI thread.
            //    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            var commandService = ZigVSPackage.GetInstance().GetService<IMenuCommandService, OleMenuCommandService>();
            s_Instance = new FormattingCommand(commandService);
            return System.Threading.Tasks.Task.CompletedTask;
        }

        private List<VSITEMSELECTION> GetSelectedFiles()
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            var r_VSITEMSELECTION_List = new List<VSITEMSELECTION>();
            try
            {
                var l_SVsShellMonitorSelection = (IVsMonitorSelection)Package.GetGlobalService(typeof(SVsShellMonitorSelection));

                l_SVsShellMonitorSelection.GetCurrentSelection(out IntPtr l_hierarchyIntPtr, out uint l_itemIDUint, out IVsMultiItemSelect l_IVsMultiItemSelect, out IntPtr i_selectedContainerIntPtr);

                if (l_itemIDUint != VSConstants.VSITEMID_NIL && l_hierarchyIntPtr != IntPtr.Zero)
                {
                    var l_IVsHierarchy = Marshal.GetObjectForIUnknown(l_hierarchyIntPtr) as IVsHierarchy;

                    if (l_itemIDUint != VSConstants.VSITEMID_SELECTION)
                    {
                        r_VSITEMSELECTION_List.Add(new VSITEMSELECTION()
                        {
                            itemid = l_itemIDUint,
                            pHier = l_IVsHierarchy
                        }
                        );
                        //                        IVsHierarchy hierarchy = Marshal.GetObjectForIUnknown(hierarchyPtr) as IVsHierarchy;
                        //                       ((IVsProject)hierarchy).GetMkDocument(itemId, out string itemFullPath);
                    }
                    else
                    {

                    }
                }
            }
            catch { }

            return r_VSITEMSELECTION_List;
        }
#pragma warning disable VSTHRD100, VSTHRD010
        private async void Execute(object i_senderObject, EventArgs i_EventArgs)
        {
            //        ThreadHelper.ThrowIfNotOnUIThread();
            var l_selectedItem = GetSelectedFiles().First();
            l_selectedItem.pHier.GetCanonicalName(l_selectedItem.itemid, out var l_pathString);

            if (l_pathString == null)
            {
                string l_messageString = string.Format(CultureInfo.CurrentCulture, "Could not format ", this.GetType().FullName);
                string l_titleString = "Formatting";

                // Show a message box to prove we were here
                VsShellUtilities.ShowMessageBox(
                    ZigVSPackage.GetInstance(),
                    l_messageString,
                    l_titleString,
                    OLEMSGICON.OLEMSGICON_INFO,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
            else
            {
                var l_FolderModelOption = await FolderModeOptions.GetLiveInstanceAsync();
                var l_toolPathString = Utilities.GetToolPathFromEnvironmentValue();

                if (File.Exists(l_pathString) && (l_FolderModelOption != null))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = Path.Combine(l_toolPathString, l_FolderModelOption.ToolPath),
                        Arguments = "fmt " + l_pathString,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
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
        }
#pragma warning restore VSTHRD100
    }
}
