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
    using EnvDTE;
    using EnvDTE80;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using Task = System.Threading.Tasks.Task;

#nullable enable

    public class AddRemoveSeparatorCommandBase
    {
        protected static AddRemoveSeparatorCommandBase? s_Instance;

        public enum Type
        {
            Add,
            Remove
        }
        protected Type m_TypeEnum = Type.Add;
        protected string m_undoNameString = "Add Comment";
        protected string m_separatorString = "//";

        public AddRemoveSeparatorCommandBase(
            string i_undoNameString,
            Type i_Type,
            string i_separatorString,
            int i_commandIDInt)
        {
            var l_CommandService = ZigVSPackage.GetInstance().GetService<IMenuCommandService, OleMenuCommandService>();
            if (l_CommandService != null)
            {
                m_undoNameString = i_undoNameString;
                m_TypeEnum = i_Type;
                m_separatorString = i_separatorString;
                var l_CommandId = new CommandID(CommandDefinition.s_CommandSetGuid, i_commandIDInt);
                var l_MenuCommand = new MenuCommand(this.Execute, l_CommandId);
                l_CommandService.AddCommand(l_MenuCommand);
            }
        }

        public static Task InitializeAsync()
        {
            s_Instance = new AddRemoveSeparatorCommandBase("Add Comment", Type.Add, "//", (int)CommandDefinition.CommandId.AddComment);
            return Task.CompletedTask;
        }

        public void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            DTE2 l_DTE = Package.GetGlobalService(typeof(SDTE)) as DTE2;
            TextDocument l_TextDocument = l_DTE.ActiveDocument?.Object("TextDocument") as TextDocument;
            TextSelection? l_Selection = l_TextDocument.Selection;
            if (l_Selection == null)
            {
                return;
            }

            int l_StartLineInt;
            int l_EndLineInt;

            if (l_Selection.IsEmpty)
            {
                l_StartLineInt = l_Selection.ActivePoint.Line;
                l_EndLineInt = l_StartLineInt;
            }
            else
            {
                l_StartLineInt = l_Selection.TopPoint.Line;
                l_EndLineInt = l_Selection.BottomPoint.Line;

                if (l_Selection.BottomPoint.AtStartOfLine && l_EndLineInt > l_StartLineInt)
                {
                    l_EndLineInt -= 1;
                }
            }

            if (l_EndLineInt < l_StartLineInt)
            {
                return;
            }

            var l_LineInfos = new List<LineInfo>();

            for (int l_LineInt = l_StartLineInt; l_LineInt <= l_EndLineInt; l_LineInt++)
            {
                var l_LineStartPoint = l_TextDocument.StartPoint.CreateEditPoint();
                l_LineStartPoint.MoveToLineAndOffset(l_LineInt, 1);

                var l_LineEndPoint = l_LineStartPoint.CreateEditPoint();
                l_LineEndPoint.EndOfLine();

                string l_LineTextString = l_LineStartPoint.GetText(l_LineEndPoint);

                int l_IndentLengthInt = 0;
                while (l_IndentLengthInt < l_LineTextString.Length && char.IsWhiteSpace(l_LineTextString[l_IndentLengthInt]))
                {
                    l_IndentLengthInt++;
                }

                bool l_HasSeparatorBool = 
                    l_LineTextString.Length - l_IndentLengthInt >= m_separatorString.Length &&
                    l_LineTextString.Substring(l_IndentLengthInt, m_separatorString.Length) == m_separatorString /*&&
                    (l_LineTextString.Length - l_IndentLengthInt == m_separatorString.Length || l_LineTextString[l_IndentLengthInt + m_separatorString.Length] != '/')*/;

                l_LineInfos.Add(new LineInfo(l_LineInt, l_IndentLengthInt, l_HasSeparatorBool));
            }

            bool l_ShouldUnseparateBool = l_LineInfos.Count > 0 && l_LineInfos.TrueForAll(i_Info => i_Info.HasSeparator);

            bool l_UndoOpenedBool = false;

            try
            {
                if (!l_DTE.UndoContext.IsOpen)
                {
                    l_DTE.UndoContext.Open("Zig " + m_undoNameString);
                    l_UndoOpenedBool = true;
                }

                foreach (var l_Info in l_LineInfos)
                {
                    if (m_TypeEnum == Type.Remove && l_Info.HasSeparator)
                    {
                        RemoveSeparator(l_TextDocument, l_Info.LineNumber, l_Info.IndentLength);
                    }
                    else if (m_TypeEnum == Type.Add)
                    {
                        AddSeparator(l_TextDocument, l_Info.LineNumber, l_Info.IndentLength);
                    }
                }
            }
            finally
            {
                if (l_UndoOpenedBool && l_DTE.UndoContext.IsOpen)
                {
                    l_DTE.UndoContext.Close();
                }
            }
        }

        private void AddSeparator(TextDocument i_TextDocument, int i_LineNumberInt, int i_IndentLengthInt)
        {
            var l_InsertPoint = i_TextDocument.StartPoint.CreateEditPoint();
            l_InsertPoint.MoveToLineAndOffset(i_LineNumberInt, 1);
            l_InsertPoint.Insert(m_separatorString);
        }

        private void RemoveSeparator(TextDocument i_TextDocument, int i_LineNumberInt, int i_IndentLengthInt)
        {
            var l_DeletePoint = i_TextDocument.StartPoint.CreateEditPoint();
            l_DeletePoint.MoveToLineAndOffset(i_LineNumberInt, i_IndentLengthInt + 1);
            l_DeletePoint.Delete(m_separatorString.Length);
        }

        private readonly struct LineInfo
        {
            public LineInfo(int i_LineNumberInt, int i_IndentLengthInt, bool i_HasSeparatorBool)
            {
                LineNumber = i_LineNumberInt;
                IndentLength = i_IndentLengthInt;
                HasSeparator = i_HasSeparatorBool;
            }

            public int LineNumber { get; }

            public int IndentLength { get; }

            public bool HasSeparator { get; }
        }
    }
}