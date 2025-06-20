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
    using System.ComponentModel;

    internal class GeneralOptions : BaseOptionModel<GeneralOptions>
    {
        [Category("1) Language Server")]
        [DisplayName("Language Server File Name (zls)")]
        [Description("Language Server File Name.")]
        [DefaultValue("zls.exe")]
        public string LanguageServerPath { get; set; } = "zls.exe";

        [Category("1) Language Server")]
        [DisplayName("Debug Mode")]
        [Description("Show communication between Language server and client.")]
        [DefaultValue(Switch.off)]
        [TypeConverter(typeof(EnumConverter))] // This will make use of enums more resilient
        public Switch TDebugSwitch { get; set; } = Switch.off;

        [Category("1) Language Server")]
        [DisplayName("")]
        [Description("")]
        [DefaultValue("")]
        public string Arguments { get; set; } = "";

        [Category("1) Language Server")]
        [DisplayName("Debug Arguments")]
        [Description("Debug Arguments")]
        [DefaultValue("--enable-debug-log --enable-message-tracing")]
        public string DebugArguments { get; set; } = "--enable-debug-log --enable-message-tracing";

        [Category("2) Package Installer")]
        [DisplayName(" Home URL")]
        [Description("Home URL")]
        [DefaultValue(@"https://github.com/search?q=language%3Azig+path%3Abuild.zig+content%3AREADME.md+lib&type=repositories&s=stars&o=desc")]
        public string HomeUrl { get; set; } = @"https://github.com/search?q=language%3Azig+path%3Abuild.zig+content%3AREADME.md+lib&type=repositories&s=stars&o=desc";

        [Category("2) Package Installer")]
        [DisplayName("Git Command")]
        [Description("Git Command")]
        [DefaultValue("git.exe")]
        public string GitPath { get; set; } = "git.exe";

        [Category("2) Package Installer")]
        [DisplayName("Git Package Option")]
        [Description("Git Package Option")]
        [DefaultValue("clone --recursive")]
        public string GitOption { get; set; } = "clone --recursive";

        [Category("2) Package Installer")]
        [DisplayName("Zig Package Option")]
        [Description("Zig Package Option")]
        [DefaultValue("fetch --save")]
        public string ZigFetchOption { get; set; } = "fetch --save";

        [Category("3) Editor")]
        [DisplayName("Auto-Insert Parentheses")]
        [Description("Automatically inserts a closing parenthesis")]
        [DefaultValue(Switch.on)]
        [TypeConverter(typeof(EnumConverter))]
        public Switch AutoInsertParenthesesSwitch { get; set; } = Switch.on;

        [Category("3) Editor")]
        [DisplayName("Auto-Insert Braces")]
        [Description("Automatically inserts a closing brace ")]
        [DefaultValue(Switch.on)]
        [TypeConverter(typeof(EnumConverter))]
        public Switch AutoInsertBracesSwitch { get; set; } = Switch.on;

        [Category("3) Editor")]
        [DisplayName("Auto-Insert Brackets")]
        [Description("Automatically inserts a closing Bracket")]
        [DefaultValue(Switch.on)]
        [TypeConverter(typeof(EnumConverter))]
        public Switch AutoInsertBrackets { get; set; } = Switch.on;
    }

    public enum Switch
    {
        on,
        off
    }
}
