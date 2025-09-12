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
    using System;
    using System.ComponentModel;

    internal class GeneralOptions : BaseOptionModel<GeneralOptions>
    {
        [Category("Build Tool")]
        [DisplayName("Tool Path (zig)")]
        [Description("Path to the build tool. Can use Visual Studio macros or environment variables (e.g. $(ZIG_HOME)\\zig.exe)")]
        [DefaultValue(Parameter.c_compilerFileName)]
        public string ToolPath { get; set; } = Parameter.c_compilerFileName;

        [Browsable(false)]
        public string ToolPathExpanded => EnvExpander.Expand(ToolPath);

        [Category("Language Server")]
        [DisplayName("Language Server File Name (zls)")]
        [Description("Language Server File Name.")]
        [DefaultValue(Parameter.c_languageServerFileName)]
        public string LanguageServerPath { get; set; } = Parameter.c_languageServerFileName;

        [Browsable(false)]
        public string LanguageServerPathExpanded => EnvExpander.Expand(LanguageServerPath);

        [Category("Language Server")]
        [DisplayName("Debug Mode")]
        [Description("Show communication between language server and Visual Studio")]
        [DefaultValue(Switch.off)]
        [TypeConverter(typeof(EnumConverter))] // This will make use of enums more resilient
        public Switch TDebugSwitch { get; set; } = Switch.off;

        [Category("Language Server")]
        [DisplayName("Arguments")]
        [Description("Arguments for ZLS when Debug Mode is off")]
        [DefaultValue("")]
        public string Arguments { get; set; } = "";

        [Category("Language Server")]
        [DisplayName("Debug Arguments")]
        [Description("Debug Arguments for ZLS")]
        [DefaultValue("--enable-stderr-logs --log-level debug")]
        public string DebugArguments { get; set; } = "--enable-stderr-logs --log-level debug";

        [Category("Package Installer")]
        [DisplayName("Home URL")]
        [Description("Home URL")]
        [DefaultValue(@"https://github.com/search?q=language%3Azig+path%3Abuild.zig+content%3AREADME.md+lib&type=repositories&s=stars&o=desc")]
        public string HomeUrl { get; set; } = @"https://github.com/search?q=language%3Azig+path%3Abuild.zig+content%3AREADME.md+lib&type=repositories&s=stars&o=desc";

        [Category("Package Installer")]
        [DisplayName("Git Command")]
        [Description("Git Command")]
        [DefaultValue(Parameter.c_gitToolFileName)]
        public string GitPath { get; set; } = Parameter.c_gitToolFileName;

        [Browsable (false)]
        public string GitPathExpanded => EnvExpander.Expand(GitPath);

        [Category("Package Installer")]
        [DisplayName("Git Package Option")]
        [Description("Git Package Option")]
        [DefaultValue("clone --recursive")]
        public string GitOption { get; set; } = "clone --recursive";

        [Category("Package Installer")]
        [DisplayName("Zig Package Option")]
        [Description("Zig Package Option")]
        [DefaultValue("fetch --save")]
        public string ZigFetchOption { get; set; } = "fetch --save";
    }

    public enum Switch
    {
        on,
        off
    }
}
