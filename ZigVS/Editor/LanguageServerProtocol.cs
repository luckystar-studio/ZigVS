namespace ZigVS
{
#nullable enable
    /*
    m_Capabilities = new Capabilities()
    {
        _vs_supportsVisualStudioExtensions = true,
                _vs_supportedSnippetVersion = 1,
                _vs_supportsIconExtensions = true,
                _vs_supportsDiagnosticRequests = true,
                workspace = new Workspace()
                {
                    applyEdit = true,
                    workspaceEdit = new Workspaceedit()
                    {
                        documentChanges = true,
                        resourceOperations = new object[] { }
                    },
                    didChangeConfiguration = new Didchangeconfiguration(),
                    didChangeWatchedFiles = new Didchangewatchedfiles(),
                    symbol = new Symbol(),
                    executeCommand = new Executecommand()
                    {
                        _vs_supportedCommands = new string[]
                        {
                            "_ms_setClipboard", "_ms_openUrl"
                        }
                    },
                    semanticTokens = new Semantictokens()
                    {
                        refreshSupport = true
                    },
                    diagnostics = new Diagnostics()
                    {
                        refreshSupport = true
                    }
                },
                textDocument = new Textdocument()
                {
                    _vs_onAutoInsert = new _Vs_Onautoinsert(),
                    synchronization = new Synchronization()
                    {
                        didSave = true
                    },
                    completion = new Completion()
                    {
                        _vs_completionList = new _Vs_Completionlist()
                        {
                            _vs_data = true,
                            _vs_commitCharacters = true
                        },
                        completionItem = new Completionitem(),
                        completionItemKind = new Completionitemkind()
                        {
                            valueSet = new int[]
                            {
                                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25//, 118115, 118116, 118117, 118118, 118119, 118120, 118121, 118122, 118123, 118124, 118125, 118126
                            }
                        },
                        completionList = new Completionlist()
                        {
                            itemDefaults = new string[]
                            {
                                "commitCharacters", "editRange", "insertTextFormat"
                            }
                        }
                    },
                    hover = new Hover()
                    {
                        contentFormat = new string[]
                        {
                            "plaintext"
                        }
                    },
                    signatureHelp = new Signaturehelp()
                    {
                        signatureInformation = new Signatureinformation()
                        {
                            documentationFormat = new string[]
                            {
                                "plaintext"
                            },
                            parameterInformation = new Parameterinformation()
                            {
                                labelOffsetSupport = true
                            }
                        },
                        contextSupport = true
                    },
                    definition = new Definition(),
                    typeDefinition = new Typedefinition(),
                    implementation = new Implementation(),
                    references = new References(),
                    documentHighlight = new Documenthighlight(),
                    documentSymbol = new Documentsymbol(),
                    codeAction = new Codeaction()
                    {
                        codeActionLiteralSupport = new Codeactionliteralsupport
                        {
                            _vs_codeActionGroup = new _Vs_Codeactiongroup()
                            {
                                _vs_valueSet = new string[]
                                {
                                        "quickfix", "refactor", "refactor.extract", "refactor.inline", "refactor.rewrite", "source", "source.organizeImports"
                                }
                            }
                        },
                        resolveSupport = new Resolvesupport()
                        {
                            properties = new string[]
                            {
                                "additionalTextEdits", "command", "commitCharacters", "description", "detail", "documentation", "insertText", "insertTextFormat", "label"
                            },
                        },
                        dataSupport = true
                    },
                    codeLens = new Codelens(),
                    documentLink = new Documentlink(),
                    formatting = new Formatting(),
                    rangeFormatting = new Rangeformatting(),
                    onTypeFormatting = new Ontypeformatting(),
                    rename = new Rename()
                    {
                        prepareSupport = true,
                        prepareSupportDefaultBehavior = 1
                    },
                    publishDiagnostics = new Publishdiagnostics()
                    {
                        tagSupport = new Tagsupport()
                        {
                            valueSet = new int[]
                            {
                                1, 2//, -1, -2
                            }
                        }
                    },
                    foldingRange = new Foldingrange()
                    {
                        _vs_refreshSupport = true,
                        foldingRange = new Foldingrange1()
                        {
                            collapsedText = true
                        }
                    },
                    linkedEditingRange = new Linkededitingrange(),
                    semanticTokens = new Semantictokens1()
                    {
                        requests = new Requests()
                        {
                            range = true,
                            full = new Full()
                            {
                                range = true
                            }
                        },
                        tokenTypes = new string[]
                        {
                            "namespace", "type", "class", "enum", "interface", "struct", "typeParameter", "parameter", "variable", "property", "enumMember", "event", "function", "method", "macro", "keyword", "modifier", "comment", "string", "number", "regexp", "operator", "cppMacro", "cppEnumerator", "cppGlobalVariable", "cppLocalVariable", "cppParameter", "cppType", "cppRefType", "cppValueType", "cppFunction", "cppMemberFunction", "cppMemberField", "cppStaticMemberFunction", "cppStaticMemberField", "cppProperty", "cppEvent", "cppClassTemplate", "cppGenericType", "cppFunctionTemplate", "cppNamespace", "cppLabel", "cppUserDefinedLiteralRaw", "cppUserDefinedLiteralNumber", "cppUserDefinedLiteralString", "cppOperator", "cppMemberOperator", "cppNewDelete"
                        },
                        tokenModifiers = new string[]
                        {
                            "declaration", "definition", "readonly", "static", "deprecated", "abstract", "async", "modification", "documentation", "defaultLibrary"
                        },
                        formats = new string[]
                        {
                            "relative"
                        }
                    }
                }
            };
*/
    public class LanguageServerProtocal_Iinitialize
    {
        public string traceparent { get; set; }
        public string jsonrpc { get; set; }
        public int id { get; set; }
        public string method { get; set; }
        public Params _params { get; set; }
    }

    public class Params
    {
        public int processId { get; set; }
        public string rootPath { get; set; }
        public string rootUri { get; set; }
        public Capabilities capabilities { get; set; }
    }

    public class Capabilities
    {
        public bool _vs_supportsVisualStudioExtensions { get; set; }
        public int _vs_supportedSnippetVersion { get; set; }
        public bool _vs_supportsIconExtensions { get; set; }
        public bool _vs_supportsDiagnosticRequests { get; set; }
        public Workspace workspace { get; set; }
        public Textdocument textDocument { get; set; }
    }

    public class Workspace
    {
        public bool applyEdit { get; set; }
        public Workspaceedit workspaceEdit { get; set; }
        public Didchangeconfiguration didChangeConfiguration { get; set; }
        public Didchangewatchedfiles didChangeWatchedFiles { get; set; }
        public Symbol symbol { get; set; }
        public Executecommand executeCommand { get; set; }
        public Semantictokens semanticTokens { get; set; }
        public Diagnostics diagnostics { get; set; }
    }

    public class Workspaceedit
    {
        public bool documentChanges { get; set; }
        public object[] resourceOperations { get; set; }
    }

    public class Didchangeconfiguration
    {
    }

    public class Didchangewatchedfiles
    {
    }

    public class Symbol
    {
    }

    public class Executecommand
    {
        public string[] _vs_supportedCommands { get; set; }
    }

    public class Semantictokens
    {
        public bool refreshSupport { get; set; }
    }

    public class Diagnostics
    {
        public bool refreshSupport { get; set; }
    }

    public class Textdocument
    {
        public _Vs_Onautoinsert _vs_onAutoInsert { get; set; }
        public Synchronization synchronization { get; set; }
        public Completion completion { get; set; }
        public Hover hover { get; set; }
        public Signaturehelp signatureHelp { get; set; }
        public Definition definition { get; set; }
        public Typedefinition typeDefinition { get; set; }
        public Implementation implementation { get; set; }
        public References references { get; set; }
        public Documenthighlight documentHighlight { get; set; }
        public Documentsymbol documentSymbol { get; set; }
        public Codeaction codeAction { get; set; }
        public Codelens codeLens { get; set; }
        public Documentlink documentLink { get; set; }
        public Formatting formatting { get; set; }
        public Rangeformatting rangeFormatting { get; set; }
        public Ontypeformatting onTypeFormatting { get; set; }
        public Rename rename { get; set; }
        public Publishdiagnostics publishDiagnostics { get; set; }
        public Foldingrange foldingRange { get; set; }
        public Linkededitingrange linkedEditingRange { get; set; }
        public Semantictokens1 semanticTokens { get; set; }
    }

    public class _Vs_Onautoinsert
    {
    }

    public class Synchronization
    {
        public bool didSave { get; set; }
    }

    public class Completion
    {
        public _Vs_Completionlist _vs_completionList { get; set; }
        public Completionitem completionItem { get; set; }
        public Completionitemkind completionItemKind { get; set; }
        public Completionlist completionList { get; set; }
    }

    public class _Vs_Completionlist
    {
        public bool _vs_data { get; set; }
        public bool _vs_commitCharacters { get; set; }
    }

    public class Completionitem
    {
    }

    public class Completionitemkind
    {
        public int[] valueSet { get; set; }
    }

    public class Completionlist
    {
        public string[] itemDefaults { get; set; }
    }

    public class Hover
    {
        public string[] contentFormat { get; set; }
    }

    public class Signaturehelp
    {
        public Signatureinformation signatureInformation { get; set; }
        public bool contextSupport { get; set; }
    }

    public class Signatureinformation
    {
        public string[] documentationFormat { get; set; }
        public Parameterinformation parameterInformation { get; set; }
    }

    public class Parameterinformation
    {
        public bool labelOffsetSupport { get; set; }
    }

    public class Definition
    {
    }

    public class Typedefinition
    {
    }

    public class Implementation
    {
    }

    public class References
    {
    }

    public class Documenthighlight
    {
    }

    public class Documentsymbol
    {
    }

    public class Codeaction
    {
        public Codeactionliteralsupport codeActionLiteralSupport { get; set; }
        public Resolvesupport resolveSupport { get; set; }
        public bool dataSupport { get; set; }
    }

    public class Codeactionliteralsupport
    {
        public _Vs_Codeactiongroup _vs_codeActionGroup { get; set; }
        public Codeactionkind codeActionKind { get; set; }
    }

    public class _Vs_Codeactiongroup
    {
        public string[] _vs_valueSet { get; set; }
    }

    public class Codeactionkind
    {
        public string[] valueSet { get; set; }
    }

    public class Resolvesupport
    {
        public string[] properties { get; set; }
    }

    public class Codelens
    {
    }

    public class Documentlink
    {
    }

    public class Formatting
    {
    }

    public class Rangeformatting
    {
    }

    public class Ontypeformatting
    {
    }

    public class Rename
    {
        public bool prepareSupport { get; set; }
        public int prepareSupportDefaultBehavior { get; set; }
    }

    public class Publishdiagnostics
    {
        public Tagsupport tagSupport { get; set; }
    }

    public class Tagsupport
    {
        public int[] valueSet { get; set; }
    }

    public class Foldingrange
    {
        public bool _vs_refreshSupport { get; set; }
        public Foldingrange1 foldingRange { get; set; }
    }

    public class Foldingrange1
    {
        public bool collapsedText { get; set; }
    }

    public class Linkededitingrange
    {
    }

    public class Semantictokens1
    {
        public Requests requests { get; set; }
        public string[] tokenTypes { get; set; }
        public string[] tokenModifiers { get; set; }
        public string[] formats { get; set; }
    }

    public class Requests
    {
        public bool range { get; set; }
        public Full full { get; set; }
    }

    public class Full
    {
        public bool range { get; set; }
    }
}