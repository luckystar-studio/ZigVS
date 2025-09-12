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

#if true
namespace ZigVS
{
    using EnvDTE;
    using Microsoft.VisualStudio.LanguageServer.Client;
    using Microsoft.VisualStudio.Project;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.TextManager.Interop;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    public class LanguageClientMiddleLayer : ILanguageClientMiddleLayer
    {
        public readonly static LanguageClientMiddleLayer Instance = new LanguageClientMiddleLayer();

        private LanguageClientMiddleLayer()
        {
        }

        public bool CanHandle(string methodName)
        {
            return true;
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

// helper funcs from ChatGPT
    // start ≤ (l,c) < end   (lexicographic; end is exclusive)
    private static bool InRangeExclusive(int sL, int sC, int eL, int eC, int l, int c)
    {
        if (l < sL) return false;
        if (l == sL && c < sC) return false;
        if (l > eL) return false;
        if (l == eL && c >= eC) return false;
        return true;
    }

    private static int SafeInt(JToken t)
    {
        if (t == null) return -1;
        if (t.Type == JTokenType.Integer) return (int)t;
        int x; return int.TryParse(t.ToString(), out x) ? x : -1;
    }

    // Clone + coerce to a "boring" shape VS reliably digests
    private static JArray NormalizeInlayHints(JArray src)
    {
        var outArr = new JArray();

        foreach (var tok in src)
        {
            if (!(tok is JObject o)) continue;

            // ensure position exists & ints are ints
            if (!(o["position"] is JObject pos)) continue;
            pos["line"] = CoerceInt(pos["line"]);
            pos["character"] = CoerceInt(pos["character"]);

            // label: keep string or array of parts with string "value"
            var label = o["label"];
            if (label == null) continue;

            if (label.Type == JTokenType.Array)
            {
                var parts = (JArray)label;
                for (int i = 0; i < parts.Count; i++)
                {
                    if (parts[i] is JObject p && p["value"] != null)
                        p["value"] = p["value"].ToString();
                }
            }
            else if (label.Type != JTokenType.String)
            {
                o["label"] = label.ToString();
            }

            // kind → int (if present)
            if (o["kind"] != null) o["kind"] = CoerceInt(o["kind"]);

            // padding defaults (optional)
            if (o["paddingLeft"] == null)  o["paddingLeft"]  = false;
            if (o["paddingRight"] == null) o["paddingRight"] = false;

            outArr.Add(o);
        }
        return outArr;

        static JValue CoerceInt(JToken t)
        {
            if (t == null) return new JValue(0);
            if (t.Type == JTokenType.Integer) return (JValue)t;
            int x; return new JValue(int.TryParse(t.ToString(), out x) ? x : 0);
        }
    }
// helper funcs end

    // ---- feature switches ----
    private const bool ENABLE_NORMALIZE   = true;  // set false to return server's array as-is
    private const bool ENABLE_RANGE_FILTER = true; // set false to skip start ≤ pos < end check

    public async Task<JToken> HandleRequestAsync(string methodName, JToken i_methodParamJToken, Func<JToken, Task<JToken>> sendRequest)
        {
            // capture request range (for cheap safety filter)
            int sL = -1, sC = -1, eL = -1, eC = -1;
            if (string.Equals(methodName, "textDocument/inlayHint", StringComparison.Ordinal))
            {
                try
                {
                    var r = i_methodParamJToken?["range"];
                    var s = r?["start"];
                    var e = r?["end"];
                    sL = SafeInt(s?["line"]);       sC = SafeInt(s?["character"]);
                    eL = SafeInt(e?["line"]);       eC = SafeInt(e?["character"]);
                    var uri = i_methodParamJToken?["textDocument"]?["uri"]?.ToString();
                    Debug.WriteLine($"INLAYHINT REQ uri={uri} range=({sL},{sC})-({eL},{eC})");
                }
                catch { }
            }

            // forward unchanged
            JToken result;
            try
            {
                result = await sendRequest(i_methodParamJToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HandleRequestAsync forward ERROR for {methodName}: {ex}");
                throw;
            }

            if (!string.Equals(methodName, "textDocument/inlayHint", StringComparison.Ordinal))
                return result;

            try
            {
                if (result == null || result.Type == JTokenType.Null) return result;
                if (result.Type != JTokenType.Array) { Debug.WriteLine($"INLAYHINT RESP not array: {result.Type}"); return result; }

                var arr = (JArray)result;
                Debug.WriteLine($"INLAYHINT RESP count={arr.Count}");

                // --- (A) cheap safety: range filter (end exclusive) ---
                JArray postFilter = arr;
                if (ENABLE_RANGE_FILTER && sL >= 0 && eL >= 0)
                {
                    var filtered = new JArray();
                    for (int i = 0; i < arr.Count; i++)
                    {
                        if (!(arr[i] is JObject o)) continue;
                        var pos = o["position"] as JObject; if (pos == null) continue;
                        int l = SafeInt(pos["line"]);
                        int c = SafeInt(pos["character"]);
                        if (InRangeExclusive(sL, sC, eL, eC, l, c))
                            filtered.Add(o);
                    }
                    if (filtered.Count != arr.Count)
                        Debug.WriteLine($"INLAYHINT RESP filtered by range: {filtered.Count}/{arr.Count}");
                    postFilter = filtered;
                }

                // --- (B) normalization (clone + coerce token types) ---
                if (ENABLE_NORMALIZE)
                {
                    var normalized = NormalizeInlayHints(postFilter);
                    return normalized;
                }

                // if normalization disabled, return filtered (or original) as-is
                return postFilter;
            }
            catch
            {
                // never disrupt the pipeline
                return result;
            }
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