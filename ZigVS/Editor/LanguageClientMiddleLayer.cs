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

#if false
namespace ZigVS
{
    using EnvDTE;
    using Microsoft.VisualStudio.LanguageServer.Client;
    using Microsoft.VisualStudio.Project;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.TextManager.Interop;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Threading.Tasks;

    public class LanguageClientMiddleLayer : ILanguageClientMiddleLayer
    {
        public readonly static LanguageClientMiddleLayer Instance = new LanguageClientMiddleLayer();

        private LanguageClientMiddleLayer()
        {
        }

        public bool CanHandle(string methodName)
        {
            return false;
        }

        public bool CanHandle2(string methodName)
        {
            var r_handleBool = true;
            switch (methodName)
            {
                case "textDocument/completion":
                    //                       r_handleBool = true;
                    break;
                case "initialize":
                    break;
                case "textDocument/documentHighlight":
                    break;
                case "textDocument/codeAction":
                    break;
                case "textDocument/didChange":
                    break;
                case "textDocument/foldingRange":
                    break;
                case "textDocument/publishDiagnostics":
                    break;
                case "textDocument/didOpen":
                    break;
                case "textDocument/semanticTokens/full":
                    break;
                case "textDocument/semanticTokens/range":
                    break;
                case "textDocument/documentSymbol":
                    break;
                case "textDocument/hover":
                    break;
                case "textDocument/didClose":
                    break;
                case "textDocument/signatureHelp":
                    break;
                default:
                    break;
            }
            return r_handleBool;
        }

        public async Task HandleNotificationAsync(string methodName, JToken methodParam, Func<JToken, Task> sendNotification)
        {
             await sendNotification(methodParam);
        }

        public async Task<JToken> HandleRequestAsync(string methodName, JToken i_methodParamJToken, Func<JToken, Task<JToken>> sendRequest)
        {
            JToken? r_JToken = await sendRequest(i_methodParamJToken);
            return r_JToken;
            // find triggerKind
            /*    foreach (var l_childJToken in i_methodParamJToken.Children())
                {
                    if (l_childJToken.Type == JTokenType.Property &&
                        ((JProperty)l_childJToken).Name == "context")
                    {
                        var l_JArray = ((JProperty)l_childJToken).Value;
                        foreach (var l_childJToken2 in l_JArray.Children())
                        {
                            if (l_childJToken2.Type == JTokenType.Property &&
                                ((JProperty)l_childJToken2).Name == "triggerKind")
                            {
                                var l_triggerKindInt = ((JProperty)l_childJToken2).Value;
                                ((JProperty)l_childJToken2).Value = 1;
                            }
                        }

                        int a2 = 0;
                    }
                }*/
/*
            if (r_JToken == null)
            {
                InsertCharacterAtCursor('.');
            }*/
          /*  foreach (var l_childJToken in r_JToken.Children())
            {
                if (l_childJToken.Type == JTokenType.Property &&
                    ((JProperty)l_childJToken).Name == "items" )
                {
                    var l_JArray = ((JArray)((JProperty)l_childJToken).Value);
                    int a = 0;
                }
            }*/


             /*   
                try
                {
                    if (l_JToken != null)
                    {
                                         string l_shortcutString = "";
                                          var l_uriString = methodParam.SelectToken("textDocument.uri").ToString();
                                          var l_lineUint = Int32.Parse(methodParam.SelectToken("position.line").ToString());
                                          var l_characterUint = Int32.Parse(methodParam.SelectToken("position.character").ToString());

                                          await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                                          m_DTE = (EnvDTE.DTE)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(EnvDTE.DTE));

                                      //    var documents = m_DTE.Documents.Cast<EnvDTE.Document>();
                                      //    documents.SingleOrDefault(d => d.FullName == l_uriString);
                                          var l_Document = m_DTE.ActiveDocument; 

                                          if (l_Document != null )
                                          {
                                              var l_TextDocument = (EnvDTE.TextDocument)l_Document.Object(nameof(EnvDTE.TextDocument));

                                              var l_EditPoint = l_TextDocument.StartPoint.CreateEditPoint();
                                              l_EditPoint.MoveToLineAndOffset(l_lineUint, l_characterUint);
                                              l_shortcutString = l_EditPoint.GetText(1);
                                          }
                        
                        foreach (var l_childJToken in l_JToken.Children())
                        {
                            if (l_childJToken.Type == JTokenType.Property &&
                                ((JProperty)l_childJToken).Name == "items" &&
                                m_IntellisenseItemList != null)
                            {
                                foreach (var l_IntellisenseItem in m_IntellisenseItemList)
                                {
                                    if (true)
                                    {
                                        var l_JObject = JObject.FromObject(l_IntellisenseItem);
                                        var l_JArray = ((JArray)((JProperty)l_childJToken).Value);
                                        l_JArray.Add(l_JObject);
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
                catch { }
                return l_JToken;*/

        }
        /*
        private void InsertCharacterAtCursor(char i_char)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var l_IVsTextManager = (IVsTextManager)((IServiceProvider)ZigVSPackage.GetInstance()).GetService(typeof(SVsTextManager));
            l_IVsTextManager.GetActiveView(1, null, out IVsTextView l_IVsTextView);

            l_IVsTextView.GetCaretPos(out int l_lineInt, out int l_columnInt);
            l_IVsTextView.GetBuffer(out IVsTextLines l_IVsTextLines);

            l_IVsTextLines.CreateEditPoint(l_lineInt, l_columnInt, out object editPoint);
            ((EnvDTE.EditPoint)editPoint).Insert(i_char.ToString());
        }*/
    }
}
#endif