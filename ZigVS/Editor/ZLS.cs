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

using Microsoft.VisualStudio.Shell;
using System;
using System.Threading.Tasks;
using ZigVS.Options;
using ZigVS.CoreCompatibility;

namespace ZigVS
{
    public class ZLS
    {
        public class Settings
        {
            public bool inlay_hints_show_variable_type_hints { get; set; }
            public bool inlay_hints_show_struct_literal_field_type { get; set; }
            public bool inlay_hints_show_parameter_name { get; set; }
            public bool inlay_hints_show_builtin { get; set; }
            public bool inlay_hints_exclude_single_argument { get; set; }
            public bool inlay_hints_hide_redundant_param_names { get; set; }
            public bool inlay_hints_hide_redundant_param_names_last_token { get; set; }

            public string zig_exe_path { get; set; }

            public Settings()
            {
                zig_exe_path = string.Empty;
            }
        }

        public async static Task<Settings> GetSettingsAsync()
        {
            var r_Settings = new Settings();

             await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var textEditorOption = ThreadHelper.JoinableTaskFactory.Run(async () => {
                return await TextEditorAdvancedOptions.GetLiveInstanceAsync();
            });
            var generalOptions = ThreadHelper.JoinableTaskFactory.Run(async () => {
                return await GeneralOptions.GetLiveInstanceAsync();
            });

            r_Settings.inlay_hints_show_variable_type_hints = textEditorOption.inlay_hints_show_variable_type_hints == Switch.on;
            r_Settings.inlay_hints_show_struct_literal_field_type = textEditorOption.inlay_hints_show_struct_literal_field_type == Switch.on;
            r_Settings.inlay_hints_show_parameter_name = textEditorOption.inlay_hints_show_parameter_name == Switch.on;
            r_Settings.inlay_hints_show_builtin = textEditorOption.inlay_hints_show_builtin == Switch.on;
            r_Settings.inlay_hints_exclude_single_argument = textEditorOption.inlay_hints_exclude_single_argument == Switch.on;
            r_Settings.inlay_hints_hide_redundant_param_names = textEditorOption.inlay_hints_hide_redundant_param_names == Switch.on;
            r_Settings.inlay_hints_hide_redundant_param_names_last_token = textEditorOption.inlay_hints_hide_redundant_param_names_last_token == Switch.on;

            if (generalOptions != null)
            {
                ToolchainProbeResult zigProbe = CoreServices.ToolchainProbe.Probe(new ToolchainProbeRequest
                {
                    Label = "Global zig.exe",
                    RawValue = generalOptions.ToolPath,
                    ExpandedValue = generalOptions.ToolPathExpanded,
                    DefaultFileName = Parameter.c_compilerFileName
                });
                r_Settings.zig_exe_path = zigProbe.ResolvedPath ?? string.Empty;
            }

            return r_Settings;
        }
    }
}
