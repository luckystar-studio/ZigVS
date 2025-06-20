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
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using System;
    using System.ComponentModel.Design;
    using System.Globalization;
    using Task = System.Threading.Tasks.Task;

#nullable enable

    internal sealed class RatingAndReviewCommand
    {
        static RatingAndReviewCommand? s_Instance = null;

        private RatingAndReviewCommand(OleMenuCommandService? i_OleMenuCommandService)
        {
            if (i_OleMenuCommandService != null)
            {
                i_OleMenuCommandService = i_OleMenuCommandService ?? throw new ArgumentNullException(nameof(i_OleMenuCommandService));

                var l_CommandID = new CommandID(CommandDefinition.s_CommandSetGuid, (int)CommandDefinition.CommandId.RatingAndReview);
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
            s_Instance = new RatingAndReviewCommand(commandService);
            return System.Threading.Tasks.Task.CompletedTask;
        }

        private void Execute(object i_senderObject, EventArgs i_EventArgs)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                string l_URLString = "https://marketplace.visualstudio.com/items?itemName=LuckystarStudio.ZigVS&ssr=false#review-details";

                if (string.IsNullOrEmpty(l_URLString))
                {
                    string l_messageString = string.Format(CultureInfo.CurrentCulture, "Could not find rating and review page: " + l_URLString, this.GetType().FullName);
                    string l_titleString = "Rating";

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
                    System.Diagnostics.Process.Start(l_URLString);
                }
            }
            catch { }
        }
    }
}
