namespace ZigVS.EditAndContinue
{
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.TextManager.Interop;
    using System;
    using System.Collections.Generic;
    using System.IO;

    // See https://github.com/microsoft/microsoft-pdb
    // See https://llvm.org/docs/PDB/index.html
    // See https://llvm.org/docs/PDB/DbiStream.html#dbi-type-server-map-substream
    // See https://learn.microsoft.com/en-us/cpp/build/reference/z7-zi-zi-debug-information-format?view=msvc-170

    // To use Visual Studio’s Edit & Continue feature,
    // the .pdb file must contain ENC (Edit and Continue) information.
    // However, the use of ENC information is not publicly documented.
    // Unless a .pdb file containing ENC data is passed to the debugger,
    // the processing will not be handed off to the Visual Studio extension.
#if false
    public sealed class EncRebuildableProjectCfg : IVsENCRebuildableProjectCfg2
    {
        // アクティブ文ID → 現在のTextSpan
        private readonly Dictionary<uint, TextSpan> _activeSpans = new Dictionary<uint, TextSpan>();
        // 例外スパン（編集禁止領域）
        private readonly List<ENC_EXCEPTION_SPAN> _exceptionSpans = new List<ENC_EXCEPTION_SPAN>();
        // 直近のビルド状態
        private ENC_BUILD_STATE _encState = ENC_BUILD_STATE.ENC_NOT_MODIFIED;

        // ランタイム／成果物への参照（あなたのプロジェクトシステム側のオブジェクト）
        private readonly string _currentPePath;
        private Guid _currentMvid;

        public EncRebuildableProjectCfg(string pePath, Guid mvid)
        {
            _currentPePath = pePath;
            _currentMvid = mvid;
        }

        // A. Debug lifecycle
        public int StartDebuggingPE() { _encState = ENC_BUILD_STATE.ENC_NOT_MODIFIED; return VSConstants.S_OK; }
        public int StopDebuggingPE() { _activeSpans.Clear(); _exceptionSpans.Clear(); return VSConstants.S_OK; }

        // B. Break/active statements
        public int EnterBreakStateOnPE(uint cActive, ENC_ACTIVE_STATEMENT[] actives, uint reason)
        {
            _activeSpans.Clear();
            for (int i = 0; i < cActive && i < actives.Length; i++)
            {
                // actives[i].id / actives[i].tsPosition / actives[i].posType 等を保存
                _activeSpans[actives[i].id] = actives[i].tsPosition;
            }
            return VSConstants.S_OK;
        }
        public int ExitBreakStateOnPE() => VSConstants.S_OK;

        // C. Build & state
        public int BuildForEnc(object pUpdatePE)
        {
            // 差分のあるソースだけを再コンパイルし、実行中プロセスに適用可能な更新を生成。
            // ここは言語／ツールチェーン依存。成功時は ENC_APPLY_READY、失敗時はエラー種別を保持。
            bool ok = RunIncrementalEncBuild(out bool hasCompileErrors, out bool hasRudeEdits);
            _encState = ok ? ENC_BUILD_STATE.ENC_APPLY_READY
                           : hasRudeEdits ? ENC_BUILD_STATE.ENC_NONCONTINUABLE_ERRORS
                                          : ENC_BUILD_STATE.ENC_COMPILE_ERRORS;
            return VSConstants.S_OK;
        }
        public int GetENCBuildState(ENC_BUILD_STATE[] p) { p[0] = _encState; return VSConstants.S_OK; }
        public int EncApplySucceeded(int fStop)
        {
            // 適用成功後の後始末。必要ならMVIDやPEパスを新ビルド成果物のものに更新。
            _encState = ENC_BUILD_STATE.ENC_NOT_MODIFIED;
            return VSConstants.S_OK;
        }

        // D. Source mapping after edits
        public int GetCurrentActiveStatementPosition(uint id, TextSpan[] span)
        {
            if (_activeSpans.TryGetValue(id, out var s)) { span[0] = s; return VSConstants.S_OK; }
            return VSConstants.E_FAIL;
        }
        public int GetExceptionSpanCount(uint[] pc) { pc[0] = (uint)_exceptionSpans.Count; return VSConstants.S_OK; }
        public int GetExceptionSpans(uint celt, ENC_EXCEPTION_SPAN[] rg, uint[] fetched)
        {
            uint n = Math.Min((uint)_exceptionSpans.Count, celt);
            for (int i = 0; i < n; i++) rg[i] = _exceptionSpans[i];
//            if (fetched is { Length: > 0 }) fetched[0] = n;  // ToDo
            return VSConstants.S_OK;
        }
        public int GetCurrentExceptionSpanPosition(uint id, TextSpan[] span)
        {
            // id に対応する例外スパンの “現在位置” を返す（追跡している場合）
            return VSConstants.E_NOTIMPL;
        }

        // E. Module identity & timestamp
        public int GetPEidentity(Guid[] mvid, string[] peName)
        {
            mvid[0] = _currentMvid; // 管理対象ならメタデータのMVID、ネイティブならPDBシグネチャなど安定GUID
            peName[0] = Path.GetFileName(_currentPePath);
            return VSConstants.S_OK;
        }

        public int GetPEBuildTimeStamp(Microsoft.VisualStudio.OLE.Interop.FILETIME[] pTimeStamp)
        {
            return VSConstants.S_OK;
        }

        // 補助（あなたのビルド／リンク／パッチに置き換え）
        private bool RunIncrementalEncBuild(out bool hasCompileErrors, out bool hasRudeEdits)
        {
            hasCompileErrors = false; hasRudeEdits = false;
            // …差分検出→再コンパイル→パッチ生成 …
            return true;
        }

        public int GetExceptionSpanCount(out uint pc)
        {
            pc = (uint)_exceptionSpans.Count;
            return VSConstants.S_OK;
        }

        public int GetExceptionSpans(uint celt, ENC_EXCEPTION_SPAN[] rgelt, ref uint pceltFetched)
        {
            uint n = Math.Min((uint)_exceptionSpans.Count, celt);
            for (int i = 0; i < n; i++) rgelt[i] = _exceptionSpans[i];
            pceltFetched = n;
            return VSConstants.S_OK;
        }
    }
#endif
}