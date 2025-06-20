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
#nullable enable
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Language.Intellisense;
    using Microsoft.VisualStudio.OLE.Interop;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Editor;
    using Microsoft.VisualStudio.Text.Operations;
    using Microsoft.VisualStudio.TextManager.Interop;
    using MSXML;
    using System;
    using System.Runtime.InteropServices;


    // See https://learn.microsoft.com/en-us/visualstudio/extensibility/walkthrough-implementing-code-snippets?view=vs-2022&tabs=csharp#add-the-insert-snippet-command-to-the-shortcut-menu
    internal class CompletionCommandHandler : IOleCommandTarget, IVsExpansionClient
    {

        IVsTextView m_IVsTextView;
        ITextView m_ITextView;
        TextViewCreationListner m_providerTextViewCreationListner;

        IOleCommandTarget m_nextIOleCommandTarget;

        ICompletionSession? m_ICompletionSession;

        IVsExpansionManager m_IVsExpansionManager;
        IVsExpansionSession? m_IVsExpansionSession;

        internal CompletionCommandHandler(IVsTextView i_IVsTextView, ITextView i_ITextView, TextViewCreationListner i_providerTextViewCreationListner)
        {
            m_ITextView = i_ITextView;
            m_IVsTextView = i_IVsTextView;
            m_providerTextViewCreationListner = i_providerTextViewCreationListner;

            //get the text manager from the service i_providerTextViewCreationListner
            IVsTextManager2 l_IVsTextManager2 = (IVsTextManager2)m_providerTextViewCreationListner.ServiceProvider.GetService(typeof(SVsTextManager));
            if (l_IVsTextManager2 == null)
            {
                throw new InvalidOperationException("Unable to get IVsTextManager2 service.");
            }
            else
            {
                l_IVsTextManager2.GetExpansionManager(out m_IVsExpansionManager);
                m_IVsExpansionSession = null;

                //add the command to the command chain
                i_IVsTextView.AddCommandFilter(this, out m_nextIOleCommandTarget);
            }
        }

        // This method adds the Code Snippets shortcut menu.
        public int QueryStatus(ref Guid i_pguidCmdGroupGuid, uint i_cCmdsUint, OLECMD[] i_prgCmdsOLECMDArray, IntPtr i_CmdTextIntPtr)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            if (!VsShellUtilities.IsInAutomationFunction(m_providerTextViewCreationListner.ServiceProvider))
            {
                if (i_pguidCmdGroupGuid == VSConstants.VSStd2K && i_cCmdsUint > 0)
                {
                    // make the Insert Snippet command appear on the context menu 
                    if ((uint)i_prgCmdsOLECMDArray[0].cmdID == (uint)VSConstants.VSStd2KCmdID.INSERTSNIPPET)
                    {
                        i_prgCmdsOLECMDArray[0].cmdf = (int)Constants.MSOCMDF_ENABLED | (int)Constants.MSOCMDF_SUPPORTED;
                        return VSConstants.S_OK;
                    }
                }
            }

            return m_nextIOleCommandTarget.QueryStatus(ref i_pguidCmdGroupGuid, i_cCmdsUint, i_prgCmdsOLECMDArray, i_CmdTextIntPtr);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            if (VsShellUtilities.IsInAutomationFunction(m_providerTextViewCreationListner.ServiceProvider))
            {
                return m_nextIOleCommandTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            }

            //make a copy of this so we can look at it after forwarding some commands
            uint commandID = nCmdID;
            char typedChar = char.MinValue;
            //make sure the input is a char before getting it

            if (pguidCmdGroup == new Guid("{875694cf-4e47-4e92-a15e-c6f296281c12}") && commandID == 0x5000)
            {
                LanguageClient.ToggleInlyHints();

                var l_ITextView = m_ITextView as IWpfTextView;
                if (l_ITextView != null)
                {
                    var l_dummyTextEdit = l_ITextView.TextBuffer.CreateEdit();
                    l_dummyTextEdit.Insert(0, " ");
                    l_dummyTextEdit.Apply();
                    var l_dummy2TextEdit = l_ITextView.TextBuffer.CreateEdit();
                    l_dummy2TextEdit.Delete(0, 1);
                    l_dummy2TextEdit.Apply();

                    return VSConstants.S_OK;
                }
            }

            //code previously written for Exec
            if (pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (uint)VSConstants.VSStd2KCmdID.TYPECHAR)
            {
                typedChar = (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);
            }

            //the snippet picker code starts here
            if (nCmdID == (uint)VSConstants.VSStd2KCmdID.INSERTSNIPPET)
            {
                IVsTextManager2 l_IVsTextManager2 = (IVsTextManager2)m_providerTextViewCreationListner.ServiceProvider.GetService(typeof(SVsTextManager));
                if (l_IVsTextManager2 != null)
                {
                    l_IVsTextManager2.GetExpansionManager(out m_IVsExpansionManager);

                    m_IVsExpansionManager.InvokeInsertionUI(
                        m_IVsTextView,
                        this,      //the expansion client
                        new Guid(Parameter.c_LanguageGuid),
                        null,       //use all snippet types
                        0,          //number of types (0 for all)
                        0,          //ignored if iCountTypes == 0
                        null,       //use all snippet kinds
                        0,          //use all snippet kinds
                        0,          //ignored if iCountTypes == 0
                        "Snippets", //the text to show in the prompt
                        string.Empty);  //only the ENTER key causes insert 
                }

                return VSConstants.S_OK;
            }

            //the expansion insertion is handled in OnItemChosen
            //if the expansion session is still active, handle tab/back tab/return/cancel
            if (m_IVsExpansionSession != null)
            {
                if (nCmdID == (uint)VSConstants.VSStd2KCmdID.BACKTAB)
                {
                    m_IVsExpansionSession.GoToPreviousExpansionField();
                    return VSConstants.S_OK;
                }
                else if (nCmdID == (uint)VSConstants.VSStd2KCmdID.TAB)
                {
                    m_IVsExpansionSession.GoToNextExpansionField(0); //false to support cycling through all the fields
                    return VSConstants.S_OK;
                }
                else if (nCmdID == (uint)VSConstants.VSStd2KCmdID.RETURN || nCmdID == (uint)VSConstants.VSStd2KCmdID.CANCEL)
                {
                    if (m_IVsExpansionSession.EndCurrentExpansion(0) == VSConstants.S_OK)
                    {
                        m_IVsExpansionSession = null;
                        return VSConstants.S_OK;
                    }
                }
            }

            //neither an expansion session nor a completion session is open, but we got a tab, so check whether the last word typed is a snippet shortcut 
            if (m_ICompletionSession == null && m_IVsExpansionSession == null && nCmdID == (uint)VSConstants.VSStd2KCmdID.TAB)
            {
                //get the word that was just added 
                CaretPosition pos = m_ITextView.Caret.Position;
                TextExtent word = m_providerTextViewCreationListner.NavigatorService.GetTextStructureNavigator(m_ITextView.TextBuffer).GetExtentOfWord(pos.BufferPosition - 1); //use the position 1 space back
                string textString = word.Span.GetText(); //the word that was just added
                //if it is a code snippet, insert it, otherwise carry on
                if (InsertAnyExpansion(textString, null, null))
                    return VSConstants.S_OK;
            }
            //check for a selection
            if (m_ICompletionSession != null && !m_ICompletionSession.IsDismissed)
            {
                //check for a commit character
                if (nCmdID == (uint)VSConstants.VSStd2KCmdID.RETURN ||
                    nCmdID == (uint)VSConstants.VSStd2KCmdID.TAB ||
                    char.IsWhiteSpace(typedChar) ||
                    (char.IsPunctuation(typedChar) && typedChar != '_')
                )
                {
                    //if the selection is fully selected, commit the current session
                    if (m_ICompletionSession.SelectedCompletionSet.SelectionStatus.IsSelected)
                    {
                        m_ICompletionSession.Commit();
                    }
                    else
                    {
                        //if there is no selection, dismiss the session
                        m_ICompletionSession.Dismiss();
                    }
                }
                if (nCmdID == (uint)VSConstants.VSStd2KCmdID.RETURN
                   || nCmdID == (uint)VSConstants.VSStd2KCmdID.TAB)
                {
                    return VSConstants.S_OK;
                }
            }

            //pass along the command so the char is added to the buffer
            int r_resultInt = m_nextIOleCommandTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

            var l_GeneralOptions = ThreadHelper.JoinableTaskFactory.Run(() => GeneralOptions.GetLiveInstanceAsync());
            if (l_GeneralOptions != null)
            {
                BracketAssist(typedChar, '(', ')', l_GeneralOptions.AutoInsertParenthesesSwitch == Switch.on);
                BracketAssist(typedChar, '{', '}', l_GeneralOptions.AutoInsertBracesSwitch == Switch.on);
                BracketAssist(typedChar, '[', ']', l_GeneralOptions.AutoInsertBrackets == Switch.on);
            }

            if (!typedChar.Equals(char.MinValue) && char.IsLetterOrDigit(typedChar))
            {
                if (m_ICompletionSession == null || m_ICompletionSession.IsDismissed) // If there is no active session, bring up completion
                {
                    TriggerCompletion();
                }
                else    //the completion session is already active, so just filter
                {
                    m_ICompletionSession.Filter();
                }
                r_resultInt = VSConstants.S_OK;
            }
            else if (commandID == (uint)VSConstants.VSStd2KCmdID.BACKSPACE   //redo the filter if there is a deletion
                || commandID == (uint)VSConstants.VSStd2KCmdID.DELETE)
            {
                if (m_ICompletionSession != null && !m_ICompletionSession.IsDismissed)
                    m_ICompletionSession.Filter();
                r_resultInt = VSConstants.S_OK;
            }
            return r_resultInt;
        }

        private void BracketAssist(char i_inputChar, char i_openBracketChar, char i_closeBracketChar, bool i_switchBool)
        {
            if (i_inputChar == i_openBracketChar && i_switchBool)
            {
                var l_caretPosition = m_ITextView.Caret.Position.BufferPosition;
                var l_ITextSnapshotLine = l_caretPosition.GetContainingLine();
                var l_prevITextSnapshotLine = l_ITextSnapshotLine.Snapshot.GetLineFromLineNumber(l_ITextSnapshotLine.LineNumber - 1);

                var l_lineString = l_ITextSnapshotLine.GetText();
                var l_preLineString = l_prevITextSnapshotLine.GetText();

                // Ignore if `{` is inside a comment
                var l_cursorXInt = l_caretPosition.Position - l_ITextSnapshotLine.Start.Position;
                var l_commentXInt = l_lineString.IndexOf("//");
                if (l_commentXInt < 0 || l_cursorXInt <= l_commentXInt)
                {
                    if (l_commentXInt < 0)
                    {
                        l_commentXInt = l_lineString.Length;
                    }
                    bool l_foundBraceBool = false;
                    for (var l_index = l_cursorXInt; l_index < l_commentXInt; l_index++)
                    {
                        if (l_lineString[l_index] == i_closeBracketChar)
                        {
                            l_foundBraceBool = true;
                            break;
                        }
                    }

                    if (!l_foundBraceBool)
                    {
                        var l_XDouble = m_ITextView.Caret.Left;

                        using (ITextEdit edit = m_ITextView.TextBuffer.CreateEdit())
                        {
                            edit.Insert(l_caretPosition.Position, i_closeBracketChar.ToString());
                            edit.Apply();
                        }
                        m_ITextView.Caret.MoveTo(new SnapshotPoint(m_ITextView.TextSnapshot, l_caretPosition.Position));
                    }
                }
            }
        }

        private bool TriggerCompletion()
        {
            //the caret must be in a non-projection location 
            SnapshotPoint? caretPoint =
            m_ITextView.Caret.Position.Point.GetPoint(
                textBuffer => (!textBuffer.ContentType.IsOfType("projection")), PositionAffinity.Predecessor);
            if (!caretPoint.HasValue)
            {
                return false;
            }

            m_ICompletionSession = m_providerTextViewCreationListner.CompletionBroker.CreateCompletionSession
            (m_ITextView,
                caretPoint.Value.Snapshot.CreateTrackingPoint(caretPoint.Value.Position, PointTrackingMode.Positive),
                true);

            //subscribe to the Dismissed event on the session 
            m_ICompletionSession.Dismissed += this.OnSessionDismissed;
            m_ICompletionSession.Start();

            return true;
        }

        private void OnSessionDismissed(object sender, EventArgs e)
        {
            if (m_ICompletionSession != null)
            {
                m_ICompletionSession.Dismissed -= this.OnSessionDismissed;
                m_ICompletionSession = null;
            }
        }

        public int EndExpansion()
        {
            m_IVsExpansionSession = null;
            return VSConstants.S_OK;
        }

        public int FormatSpan(IVsTextLines pBuffer, TextSpan[] ts)
        {
            return VSConstants.S_OK;
        }

        public int GetExpansionFunction(IXMLDOMNode xmlFunctionNode, string bstrFieldName, out IVsExpansionFunction? pFunc)
        {
            pFunc = null;
            return VSConstants.S_OK;
        }

        public int IsValidKind(IVsTextLines pBuffer, TextSpan[] ts, string bstrKind, out int pfIsValidKind)
        {
            pfIsValidKind = 1;
            return VSConstants.S_OK;
        }

        public int IsValidType(IVsTextLines pBuffer, TextSpan[] ts, string[] rgTypes, int iCountTypes, out int pfIsValidType)
        {
            pfIsValidType = 1;
            return VSConstants.S_OK;
        }

        public int OnAfterInsertion(IVsExpansionSession pSession)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeInsertion(IVsExpansionSession pSession)
        {
            return VSConstants.S_OK;
        }

        public int PositionCaretForEditing(IVsTextLines pBuffer, TextSpan[] ts)
        {
            return VSConstants.S_OK;
        }

        public int OnItemChosen(string pszTitle, string pszPath)
        {
            InsertAnyExpansion(null, pszTitle, pszPath);
            return VSConstants.S_OK;
        }

        private bool InsertAnyExpansion(string? shortcut, string? title, string? path)
        {
            //first get the location of the caret, and set up a TextSpan
            int endColumn, startLine;
            //get the column number from  the IVsTextView, not the ITextView
            m_IVsTextView.GetCaretPos(out startLine, out endColumn);

            TextSpan addSpan = new TextSpan();
            addSpan.iStartIndex = endColumn;
            addSpan.iEndIndex = endColumn;
            addSpan.iStartLine = startLine;
            addSpan.iEndLine = startLine;

            if (shortcut != null) //get the expansion from the shortcut
            {
                //reset the TextSpan to the width of the shortcut, 
                //because we're going to replace the shortcut with the expansion
                addSpan.iStartIndex = addSpan.iEndIndex - shortcut.Length;

                m_IVsExpansionManager.GetExpansionByShortcut(
                    this,
                    new Guid(Parameter.c_LanguageGuid),
                    shortcut,
                    m_IVsTextView,
                    new TextSpan[] { addSpan },
                    0,
                    out path,
                    out title);

            }
            if (title != null && path != null)
            {
                IVsTextLines textLines;
                m_IVsTextView.GetBuffer(out textLines);
                IVsExpansion bufferExpansion = (IVsExpansion)textLines;

                if (bufferExpansion != null)
                {
                    int hr = bufferExpansion.InsertNamedExpansion(
                        title,
                        path,
                        addSpan,
                        this,
                        new Guid(Parameter.c_LanguageGuid),
                        0,
                       out m_IVsExpansionSession);
                    if (VSConstants.S_OK == hr)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}