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
namespace ZigVS.Options
{
    using Microsoft.VisualStudio.Shell;
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    [Guid("9BE877D0-D32C-4F32-A673-3FFF4129EDAD")]
    internal class TextEditorAdvancedOptions : BaseOptionModel<TextEditorAdvancedOptions>
    {
        [Category("Inlay hints")]
        [DisplayName("Show variable type")]
        [Description("Toggle Zig Language Server inlay hints settings")]
        [DefaultValue(Switch.on)]
        [TypeConverter(typeof(EnumConverter))]
        public Switch inlay_hints_show_variable_type_hints { get; set; } = Switch.on;

        [Category("Inlay hints")]
        [DisplayName("Show struct literal field type")]
        [Description("Toggle Zig Language Server inlay hints settings")]
        [DefaultValue(Switch.on)]
        [TypeConverter(typeof(EnumConverter))]
        public Switch inlay_hints_show_struct_literal_field_type { get; set; } = Switch.on;

        [Category("Inlay hints")]
        [DisplayName("Show parameter name")]
        [Description("Toggle Zig Language Server inlay hints settings")]
        [DefaultValue(Switch.on)]
        [TypeConverter(typeof(EnumConverter))]
        public Switch inlay_hints_show_parameter_name { get; set; } = Switch.on;

        [Category("Inlay hints")]
        [DisplayName("Show builtin")]
        [Description("Toggle Zig Language Server inlay hints settings")]
        [DefaultValue(Switch.on)]
        [TypeConverter(typeof(EnumConverter))]
        public Switch inlay_hints_show_builtin { get; set; } = Switch.on;

        [Category("Inlay hints")]
        [DisplayName("Exclude single argument")]
        [Description("Toggle Zig Language Server inlay hints settings")]
        [DefaultValue(Switch.on)]
        [TypeConverter(typeof(EnumConverter))]
        public Switch inlay_hints_exclude_single_argument { get; set; } = Switch.on;

        [Category("Inlay hints")]
        [DisplayName("Hide redundant parameter names")]
        [Description("Toggle Zig Language Server inlay hints settings")]
        [DefaultValue(Switch.on)]
        [TypeConverter(typeof(EnumConverter))]
        public Switch inlay_hints_hide_redundant_param_names { get; set; } = Switch.on;

        [Category("Inlay hints")]
        [DisplayName("Hide redundant parameter names last token")]
        [Description("Toggle Zig Language Server inlay hints settings")]
        [DefaultValue(Switch.on)]
        [TypeConverter(typeof(EnumConverter))]
        public Switch inlay_hints_hide_redundant_param_names_last_token { get; set; } = Switch.on;

        [Category("Auto-Insert")]
        [DisplayName("Parentheses")]
        [Description("Automatically inserts a closing parenthesis")]
        [DefaultValue(Switch.on)]
        [TypeConverter(typeof(EnumConverter))]
        public Switch AutoInsertParenthesesSwitch { get; set; } = Switch.on;

        [Category("Auto-Insert")]
        [DisplayName("Braces")]
        [Description("Automatically inserts a closing brace")]
        [DefaultValue(Switch.on)]
        [TypeConverter(typeof(EnumConverter))]
        public Switch AutoInsertBracesSwitch { get; set; } = Switch.on;

        [Category("Auto-Insert")]
        [DisplayName("Brackets")]
        [Description("Automatically inserts a closing bracket")]
        [DefaultValue(Switch.on)]
        [TypeConverter(typeof(EnumConverter))]
        public Switch AutoInsertBracketsSwitch { get; set; } = Switch.on;

/*        protected override void OnApply(Microsoft.VisualStudio.Shell.PageApplyEventArgs e)
        {
            base.OnApply(e);
            LanguageClient.UpdateConfiguration();
        }*/
    }
}
#endif