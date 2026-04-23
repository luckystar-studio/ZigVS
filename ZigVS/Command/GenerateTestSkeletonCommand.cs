namespace ZigVS.Command
{
    using EnvDTE;
    using EnvDTE80;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using System;
    using System.ComponentModel.Design;
    using System.IO;
    using Task = System.Threading.Tasks.Task;
    using ZigVS.CoreCompatibility.Testing;

#nullable enable

    internal sealed class GenerateTestSkeletonCommand
    {
        private static GenerateTestSkeletonCommand? s_Instance;
        private readonly ZigTestSkeletonGenerator m_Generator = new ZigTestSkeletonGenerator();

        private GenerateTestSkeletonCommand(OleMenuCommandService? i_OleMenuCommandService)
        {
            if (i_OleMenuCommandService != null)
            {
                CommandID l_commandId = new CommandID(
                    CommandDefinition.s_CommandSetGuid,
                    (int)CommandDefinition.CommandId.GenerateTestSkeleton);
                OleMenuCommand l_menuCommand = new OleMenuCommand(this.Execute, l_commandId);
                l_menuCommand.BeforeQueryStatus += this.BeforeQueryStatus;
                i_OleMenuCommandService.AddCommand(l_menuCommand);
            }
        }

        public static Task InitializeAsync()
        {
            OleMenuCommandService? l_commandService = ZigVSPackage.GetInstance().GetService<IMenuCommandService, OleMenuCommandService>();
            s_Instance = new GenerateTestSkeletonCommand(l_commandService);
            return Task.CompletedTask;
        }

        private void BeforeQueryStatus(object i_senderObject, EventArgs i_eventArgs)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!(i_senderObject is OleMenuCommand l_menuCommand))
            {
                return;
            }

            bool l_isAvailableBool = TryGetActiveZigDocumentContext(
                out DTE2? l_dte,
                out TextDocument? l_textDocument,
                out TextSelection? l_selection,
                out string? l_sourceTextString,
                out int l_caretLineInt,
                out string _);

            l_menuCommand.Supported = true;
            l_menuCommand.Visible = true;
            l_menuCommand.Enabled = l_isAvailableBool &&
                                    l_dte != null &&
                                    l_textDocument != null &&
                                    l_selection != null &&
                                    l_sourceTextString != null &&
                                    m_Generator.TryGenerate(l_sourceTextString, l_caretLineInt, out _);
        }

        private void Execute(object i_senderObject, EventArgs i_eventArgs)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!TryGetActiveZigDocumentContext(
                out DTE2? l_dte,
                out TextDocument? l_textDocument,
                out TextSelection? l_selection,
                out string? l_sourceTextString,
                out int l_caretLineInt,
                out string l_errorMessageString))
            {
                ShowMessage(l_errorMessageString);
                return;
            }

            if (!m_Generator.TryGenerate(l_sourceTextString ?? string.Empty, l_caretLineInt, out GenerateTestSkeletonResult? l_result) ||
                l_result == null ||
                !l_result.Success)
            {
                ShowMessage(l_result?.ErrorMessage ?? "The caret is not inside a Zig function.");
                return;
            }

            bool l_undoOpenedBool = false;

            try
            {
                if (l_dte != null && !l_dte.UndoContext.IsOpen)
                {
                    l_dte.UndoContext.Open("Zig Generate Test Skeleton");
                    l_undoOpenedBool = true;
                }

                EditPoint l_editPoint = l_textDocument!.StartPoint.CreateEditPoint();
                l_editPoint.MoveToAbsoluteOffset(l_result.ReplaceStartOffset + 1);
                if (l_result.ReplaceLength > 0)
                {
                    l_editPoint.Delete(l_result.ReplaceLength);
                }

                l_editPoint.Insert(l_result.GeneratedText);
                l_selection!.MoveToAbsoluteOffset(l_result.CaretOffset + 1, false);
            }
            finally
            {
                if (l_undoOpenedBool && l_dte != null && l_dte.UndoContext.IsOpen)
                {
                    l_dte.UndoContext.Close();
                }
            }
        }

        private static bool TryGetActiveZigDocumentContext(
            out DTE2? o_Dte,
            out TextDocument? o_TextDocument,
            out TextSelection? o_Selection,
            out string? o_SourceTextString,
            out int o_CaretLineInt,
            out string o_ErrorMessageString)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            o_Dte = Package.GetGlobalService(typeof(SDTE)) as DTE2;
            o_TextDocument = null;
            o_Selection = null;
            o_SourceTextString = null;
            o_CaretLineInt = 0;
            o_ErrorMessageString = "An active Zig document is required.";

            if (o_Dte?.ActiveDocument == null)
            {
                return false;
            }

            if (!IsZigDocument(o_Dte.ActiveDocument))
            {
                o_ErrorMessageString = "This command is only available for Zig source files.";
                return false;
            }

            try
            {
                o_TextDocument = o_Dte.ActiveDocument.Object("TextDocument") as TextDocument;
            }
            catch
            {
                o_TextDocument = null;
            }

            if (o_TextDocument == null)
            {
                o_ErrorMessageString = "The active Zig document is not editable.";
                return false;
            }

            o_Selection = o_TextDocument.Selection;
            if (o_Selection == null)
            {
                o_ErrorMessageString = "The active Zig document does not have a text selection.";
                return false;
            }

            o_CaretLineInt = o_Selection.ActivePoint.Line;
            EditPoint l_startPoint = o_TextDocument.StartPoint.CreateEditPoint();
            o_SourceTextString = l_startPoint.GetText(o_TextDocument.EndPoint);
            return true;
        }

        private static bool IsZigDocument(Document i_document)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string l_candidatePathString = i_document.FullName;
            if (string.IsNullOrWhiteSpace(l_candidatePathString))
            {
                l_candidatePathString = i_document.Name;
            }

            return string.Equals(Path.GetExtension(l_candidatePathString), Parameter.c_fileExtension, StringComparison.OrdinalIgnoreCase);
        }

        private static void ShowMessage(string i_messageString)
        {
            VsShellUtilities.ShowMessageBox(
                ZigVSPackage.GetInstance(),
                i_messageString,
                "Generate Test Skeleton",
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}
