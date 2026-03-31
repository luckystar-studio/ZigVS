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
#nullable enable
    using Microsoft.VisualStudio.Shell;
    using System;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using Task = System.Threading.Tasks.Task;

    internal sealed class ValidateEnvironmentCommand
    {
        private static readonly Regex s_versionRegex = new Regex(@"\d+\.\d+\.\d+(?:[-+][0-9A-Za-z.+-]+)?", RegexOptions.Compiled);

        static ValidateEnvironmentCommand? s_Instance = null;

        private ValidateEnvironmentCommand(OleMenuCommandService? i_OleMenuCommandService)
        {
            if (i_OleMenuCommandService != null)
            {
                i_OleMenuCommandService = i_OleMenuCommandService ?? throw new ArgumentNullException(nameof(i_OleMenuCommandService));

                var l_CommandID = new CommandID(CommandDefinition.s_CommandSetGuid, (int)CommandDefinition.CommandId.ValidateEnvironment);
                var l_MenuCommand = new MenuCommand(this.Execute, l_CommandID);
                i_OleMenuCommandService.AddCommand(l_MenuCommand);
            }
        }

        public static Task InitializeAsync()
        {
            var commandService = ZigVSPackage.GetInstance().GetService<IMenuCommandService, OleMenuCommandService>();
            s_Instance = new ValidateEnvironmentCommand(commandService);
            return Task.CompletedTask;
        }

        private void Execute(object i_senderObject, EventArgs i_EventArgs)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Common.OutputWindowPane.Show();
            WriteSection("Validate Zig Environment");
            WriteLine(ValidationLevel.Info, "Starting environment validation.");

            try
            {
                var l_GeneralOptions = ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    return await GeneralOptions.GetLiveInstanceAsync();
                });

                if (l_GeneralOptions == null)
                {
                    WriteLine(ValidationLevel.Error, "Could not load Tools > Options > ZigVS > General.");
                    return;
                }

                WriteSection("Context");
                WriteLine(ValidationLevel.Info, "Solution mode: " + Utilities.GetSolutionMode());

                string l_StartupPathString = "";
                if (Utilities.GetSolutionMode() == Utilities.SolutionMode.ProjectMode)
                {
                    l_StartupPathString = Utilities.GetCurrentProjectPath();
                }
                else if (Utilities.GetSolutionMode() == Utilities.SolutionMode.OpenFolderMode)
                {
                    l_StartupPathString = Utilities.GetOpenFolderPath();
                }

                if (!string.IsNullOrEmpty(l_StartupPathString))
                {
                    WriteLine(ValidationLevel.Info, "Active path: " + l_StartupPathString);
                }

                var l_GlobalZig = CreateToolReport("Global zig.exe", l_GeneralOptions.ToolPath, l_GeneralOptions.ToolPathExpanded, Parameter.c_compilerFileName);
                ToolSettingReport? l_ProjectZig = null;
                var l_ProjectOverride = TryGetStartupProjectToolOverride();
                if (l_ProjectOverride != null)
                {
                    l_ProjectZig = CreateToolReport("Project ToolPath", l_ProjectOverride.RawValue, l_ProjectOverride.ExpandedValue, Parameter.c_compilerFileName);
                    l_ProjectZig.SourcePath = l_ProjectOverride.ProjectFilePath;
                }

                var l_Zls = CreateToolReport("Global zls.exe", l_GeneralOptions.LanguageServerPath, l_GeneralOptions.LanguageServerPathExpanded, Parameter.c_languageServerFileName);

                WriteSection("Tool Paths");
                WriteToolReport(l_GlobalZig);
                if (l_ProjectZig != null)
                {
                    WriteToolReport(l_ProjectZig);
                }
                else
                {
                    WriteLine(ValidationLevel.Info, "Project ToolPath override: not detected.");
                }
                WriteToolReport(l_Zls);

                WriteSection("Versions");
                var l_GlobalZigVersion = GetVersionReport(l_GlobalZig, "version", "--version");
                WriteVersionReport(l_GlobalZigVersion);

                VersionReport? l_ProjectZigVersion = null;
                if (l_ProjectZig != null)
                {
                    l_ProjectZigVersion = GetVersionReport(l_ProjectZig, "version", "--version");
                    WriteVersionReport(l_ProjectZigVersion);
                }

                var l_ZlsVersion = GetVersionReport(l_Zls, "--version", "version");
                WriteVersionReport(l_ZlsVersion);

                WriteSection("Consistency");
                CompareToolchainVersions("Global zig.exe vs zls.exe", l_GlobalZigVersion, l_ZlsVersion);
                if (l_ProjectZigVersion != null)
                {
                    CompareToolchainVersions("Project ToolPath vs Global zig.exe", l_ProjectZigVersion, l_GlobalZigVersion);
                    CompareToolchainVersions("Project ToolPath vs zls.exe", l_ProjectZigVersion, l_ZlsVersion);
                }

                WriteSection("Recommended Settings");
                WriteRecommendations(l_GeneralOptions, l_GlobalZig, l_ProjectZig, l_Zls, l_GlobalZigVersion, l_ProjectZigVersion, l_ZlsVersion);

                WriteSection("Summary");
                bool l_HasError = l_GlobalZig.Level == ValidationLevel.Error ||
                                  l_Zls.Level == ValidationLevel.Error ||
                                  l_GlobalZigVersion.Level == ValidationLevel.Error ||
                                  l_ZlsVersion.Level == ValidationLevel.Error ||
                                  (l_ProjectZig != null && l_ProjectZig.Level == ValidationLevel.Error);
                if (l_HasError)
                {
                    WriteLine(ValidationLevel.Error, "Environment validation finished with blocking issues.");
                }
                else
                {
                    WriteLine(ValidationLevel.Ok, "Environment validation finished.");
                }
            }
            catch (Exception l_Exception)
            {
                WriteLine(ValidationLevel.Error, "Validation failed: " + l_Exception.Message);
            }
        }

        private static ToolSettingReport CreateToolReport(string i_LabelString, string? i_RawValueString, string? i_ExpandedValueString, string i_DefaultFileNameString)
        {
            var r_Report = new ToolSettingReport
            {
                Label = i_LabelString,
                RawValue = NormalizeConfiguredPath(i_RawValueString),
                ExpandedValue = NormalizeConfiguredPath(i_ExpandedValueString),
                DefaultFileName = i_DefaultFileNameString,
            };

            if (string.IsNullOrWhiteSpace(r_Report.RawValue))
            {
                r_Report.Level = ValidationLevel.Error;
                r_Report.Message = "Setting is empty.";
                return r_Report;
            }

            r_Report.UsesEnvironmentMacros = r_Report.RawValue.Contains("$(") || r_Report.RawValue.Contains("%");

            string? l_ResolvedPathString = ResolveConfiguredExecutable(r_Report.ExpandedValue);
            if (!string.IsNullOrEmpty(l_ResolvedPathString))
            {
                r_Report.ResolvedPath = l_ResolvedPathString;
                r_Report.Exists = true;
                r_Report.Level = ValidationLevel.Ok;
                if (IsPathSearchValue(r_Report.ExpandedValue))
                {
                    r_Report.Message = "Resolved from PATH.";
                }
                else
                {
                    r_Report.Message = "Resolved successfully.";
                }
            }
            else
            {
                r_Report.Level = ValidationLevel.Error;
                r_Report.Message = "Could not resolve the executable path.";
            }

            return r_Report;
        }

        private static string NormalizeConfiguredPath(string? i_ValueString)
        {
            return (i_ValueString ?? string.Empty).Trim().Trim('"');
        }

        private static string? ResolveConfiguredExecutable(string? i_ValueString)
        {
            string l_ValueString = NormalizeConfiguredPath(i_ValueString);
            if (string.IsNullOrWhiteSpace(l_ValueString))
            {
                return null;
            }

            try
            {
                if (IsPathSearchValue(l_ValueString))
                {
                    string? l_ResolvedString = Utilities.ResolvePath(l_ValueString);
                    if (!string.IsNullOrEmpty(l_ResolvedString) && File.Exists(l_ResolvedString))
                    {
                        return Path.GetFullPath(l_ResolvedString);
                    }

                    return null;
                }

                string l_FullPathString = Path.GetFullPath(l_ValueString);
                return File.Exists(l_FullPathString) ? l_FullPathString : null;
            }
            catch
            {
                return null;
            }
        }

        private static bool IsPathSearchValue(string i_ValueString)
        {
            return !string.IsNullOrWhiteSpace(i_ValueString) &&
                   !i_ValueString.Contains(Path.DirectorySeparatorChar.ToString()) &&
                   !i_ValueString.Contains(Path.AltDirectorySeparatorChar.ToString()) &&
                   !Path.IsPathRooted(i_ValueString);
        }

        private static ProjectToolOverride? TryGetStartupProjectToolOverride()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (Utilities.GetSolutionMode() != Utilities.SolutionMode.ProjectMode)
            {
                return null;
            }

            string l_ProjectDirectoryString = Utilities.GetCurrentProjectPath();
            if (string.IsNullOrEmpty(l_ProjectDirectoryString) || !Directory.Exists(l_ProjectDirectoryString))
            {
                return null;
            }

            var l_ProjectFileArray = Directory.GetFiles(l_ProjectDirectoryString, "*.zigproj", SearchOption.TopDirectoryOnly);
            if (l_ProjectFileArray.Length == 0)
            {
                return null;
            }

            string l_ProjectFileString = l_ProjectFileArray[0];
            try
            {
                var l_Document = XDocument.Load(l_ProjectFileString);
                foreach (var l_Element in l_Document.Descendants())
                {
                    if (l_Element.Name.LocalName == "ToolPath" && !string.IsNullOrWhiteSpace(l_Element.Value))
                    {
                        string l_RawString = l_Element.Value.Trim();
                        return new ProjectToolOverride
                        {
                            ProjectFilePath = l_ProjectFileString,
                            RawValue = l_RawString,
                            ExpandedValue = EnvExpander.Expand(l_RawString),
                        };
                    }
                }
            }
            catch
            {
            }

            return new ProjectToolOverride
            {
                ProjectFilePath = l_ProjectFileString,
                RawValue = string.Empty,
                ExpandedValue = string.Empty,
            };
        }

        private static VersionReport GetVersionReport(ToolSettingReport i_Report, string i_PrimaryArgumentsString, string i_FallbackArgumentsString)
        {
            var r_Report = new VersionReport
            {
                Label = i_Report.Label,
                Level = i_Report.Exists ? ValidationLevel.Warning : ValidationLevel.Error,
                Message = i_Report.Exists ? "Could not read version information." : "Skipped because the executable is unresolved.",
            };

            if (!i_Report.Exists || string.IsNullOrEmpty(i_Report.ResolvedPath))
            {
                return r_Report;
            }

            if (TryRunTool(i_Report.ResolvedPath, i_PrimaryArgumentsString, out string l_OutputString))
            {
                FillVersionReport(r_Report, l_OutputString);
                return r_Report;
            }

            if (!string.IsNullOrWhiteSpace(i_FallbackArgumentsString) &&
                TryRunTool(i_Report.ResolvedPath, i_FallbackArgumentsString, out l_OutputString))
            {
                FillVersionReport(r_Report, l_OutputString);
                return r_Report;
            }

            r_Report.Level = ValidationLevel.Warning;
            r_Report.Message = "Executable was found, but the version command did not succeed.";
            return r_Report;
        }

        private static void FillVersionReport(VersionReport i_Report, string i_OutputString)
        {
            i_Report.Output = i_OutputString;
            string l_TrimmedOutputString = i_OutputString.Trim();
            if (string.IsNullOrEmpty(l_TrimmedOutputString))
            {
                i_Report.Level = ValidationLevel.Warning;
                i_Report.Message = "Version command returned no output.";
                return;
            }

            i_Report.Level = ValidationLevel.Ok;
            i_Report.Message = l_TrimmedOutputString;

            var l_Match = s_versionRegex.Match(l_TrimmedOutputString);
            if (l_Match.Success)
            {
                i_Report.ParsedVersion = ParseVersionString(l_Match.Value);
            }
        }

        private static SemanticVersion? ParseVersionString(string i_VersionString)
        {
            string l_VersionString = i_VersionString.Trim();
            var l_Match = Regex.Match(l_VersionString, @"^(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)");
            if (!l_Match.Success)
            {
                return null;
            }

            return new SemanticVersion
            {
                Raw = l_VersionString,
                Major = int.Parse(l_Match.Groups["major"].Value, CultureInfo.InvariantCulture),
                Minor = int.Parse(l_Match.Groups["minor"].Value, CultureInfo.InvariantCulture),
                Patch = int.Parse(l_Match.Groups["patch"].Value, CultureInfo.InvariantCulture),
            };
        }

        private static bool TryRunTool(string i_FileNameString, string i_ArgumentsString, out string o_OutputString)
        {
            o_OutputString = string.Empty;

            try
            {
                var l_ProcessStartInfo = new ProcessStartInfo
                {
                    FileName = i_FileNameString,
                    Arguments = i_ArgumentsString,
                    WorkingDirectory = Path.GetDirectoryName(i_FileNameString),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                };

                using (var l_Process = new Process())
                {
                    l_Process.StartInfo = l_ProcessStartInfo;
                    if (!l_Process.Start())
                    {
                        return false;
                    }

                    string l_StandardOutputString = l_Process.StandardOutput.ReadToEnd();
                    string l_StandardErrorString = l_Process.StandardError.ReadToEnd();
                    l_Process.WaitForExit(3000);

                    o_OutputString = (l_StandardOutputString + Environment.NewLine + l_StandardErrorString).Trim();
                    return l_Process.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private static void CompareToolchainVersions(string i_LabelString, VersionReport? i_LeftReport, VersionReport? i_RightReport)
        {
            if (i_LeftReport == null || i_RightReport == null)
            {
                return;
            }

            if (i_LeftReport.ParsedVersion == null || i_RightReport.ParsedVersion == null)
            {
                WriteLine(ValidationLevel.Info, i_LabelString + ": comparison skipped because a version could not be parsed.");
                return;
            }

            if (i_LeftReport.ParsedVersion.Major == i_RightReport.ParsedVersion.Major &&
                i_LeftReport.ParsedVersion.Minor == i_RightReport.ParsedVersion.Minor)
            {
                WriteLine(ValidationLevel.Ok, i_LabelString + ": " +
                    i_LeftReport.ParsedVersion.Raw + " and " + i_RightReport.ParsedVersion.Raw + " are aligned.");
            }
            else
            {
                WriteLine(ValidationLevel.Warning, i_LabelString + ": " +
                    i_LeftReport.ParsedVersion.Raw + " and " + i_RightReport.ParsedVersion.Raw +
                    " do not share the same major.minor version.");
            }
        }

        private static void WriteToolReport(ToolSettingReport i_Report)
        {
            WriteLine(i_Report.Level, i_Report.Label + ": " + i_Report.Message);
            WriteIndented("Raw setting: " + DisplayOrMissing(i_Report.RawValue));
            WriteIndented("Expanded setting: " + DisplayOrMissing(i_Report.ExpandedValue));
            if (!string.IsNullOrEmpty(i_Report.SourcePath))
            {
                WriteIndented("Source: " + i_Report.SourcePath);
            }
            WriteIndented("Resolved path: " + DisplayOrMissing(i_Report.ResolvedPath));
        }

        private static void WriteVersionReport(VersionReport i_Report)
        {
            WriteLine(i_Report.Level, i_Report.Label + " version: " + i_Report.Message);
        }

        private static void WriteRecommendations(
            GeneralOptions i_GeneralOptions,
            ToolSettingReport i_GlobalZig,
            ToolSettingReport? i_ProjectZig,
            ToolSettingReport i_Zls,
            VersionReport i_GlobalZigVersion,
            VersionReport? i_ProjectZigVersion,
            VersionReport i_ZlsVersion)
        {
            if (i_GlobalZig.Exists && IsPathSearchValue(i_GlobalZig.ExpandedValue))
            {
                WriteLine(ValidationLevel.Ok, "Global zig.exe is being resolved from PATH. This matches the recommended setup.");
            }
            else if (i_GlobalZig.Exists)
            {
                WriteLine(ValidationLevel.Info, "Global zig.exe is pinned to a specific path. This is fine, but PATH or $(ZIG_HOME) is easier to share across machines.");
            }

            if (i_Zls.Exists && IsPathSearchValue(i_Zls.ExpandedValue))
            {
                WriteLine(ValidationLevel.Ok, "zls.exe is being resolved from PATH. This matches the recommended setup.");
            }
            else if (i_Zls.Exists)
            {
                WriteLine(ValidationLevel.Info, "zls.exe is pinned to a specific path. Consider PATH or a stable environment variable if the team shares settings.");
            }

            if (i_GeneralOptions.TDebugSwitch == ZigVS.Switch.on)
            {
                WriteLine(ValidationLevel.Warning, "Language Server Debug Mode is ON. Recommended default is OFF unless you are diagnosing ZLS communication.");
            }
            else
            {
                WriteLine(ValidationLevel.Ok, "Language Server Debug Mode is OFF.");
            }

            if (i_GlobalZigVersion.ParsedVersion != null && i_ZlsVersion.ParsedVersion != null)
            {
                if (i_GlobalZigVersion.ParsedVersion.Major != i_ZlsVersion.ParsedVersion.Major ||
                    i_GlobalZigVersion.ParsedVersion.Minor != i_ZlsVersion.ParsedVersion.Minor)
                {
                    WriteLine(ValidationLevel.Warning, "Recommended: keep global zig.exe and zls.exe on the same major.minor release line.");
                }
            }

            if (i_ProjectZig != null && i_ProjectZig.Exists)
            {
                if (i_ProjectZigVersion != null &&
                    i_GlobalZigVersion.ParsedVersion != null &&
                    i_ProjectZigVersion.ParsedVersion != null &&
                    (i_ProjectZigVersion.ParsedVersion.Major != i_GlobalZigVersion.ParsedVersion.Major ||
                     i_ProjectZigVersion.ParsedVersion.Minor != i_GlobalZigVersion.ParsedVersion.Minor))
                {
                    WriteLine(ValidationLevel.Warning, "Project ToolPath points to a different Zig version than Tools > Options. Only do this intentionally.");
                }
                else
                {
                    WriteLine(ValidationLevel.Info, "Project ToolPath override is present. Keep it aligned with the global toolchain unless the project truly requires a different Zig.");
                }
            }
            else
            {
                WriteLine(ValidationLevel.Info, "No project ToolPath override detected. The global zig.exe setting will be used for editor features.");
            }
        }

        private static string DisplayOrMissing(string? i_ValueString)
        {
            return string.IsNullOrWhiteSpace(i_ValueString) ? "<missing>" : i_ValueString;
        }

        private static void WriteSection(string i_TitleString)
        {
            Common.OutputWindowPane.OutputString(Environment.NewLine + i_TitleString + Environment.NewLine);
            Common.OutputWindowPane.OutputString(new string('-', i_TitleString.Length) + Environment.NewLine);
        }

        private static void WriteIndented(string i_MessageString)
        {
            Common.OutputWindowPane.OutputString("  " + i_MessageString + Environment.NewLine);
        }

        private static void WriteLine(ValidationLevel i_Level, string i_MessageString)
        {
            Common.OutputWindowPane.OutputString("[" + i_Level.ToString().ToUpperInvariant() + "] " + i_MessageString + Environment.NewLine);
        }

        private sealed class ToolSettingReport
        {
            public string Label { get; set; } = string.Empty;
            public string RawValue { get; set; } = string.Empty;
            public string ExpandedValue { get; set; } = string.Empty;
            public string? ResolvedPath { get; set; }
            public string? SourcePath { get; set; }
            public string DefaultFileName { get; set; } = string.Empty;
            public bool Exists { get; set; }
            public bool UsesEnvironmentMacros { get; set; }
            public ValidationLevel Level { get; set; } = ValidationLevel.Info;
            public string Message { get; set; } = string.Empty;
        }

        private sealed class VersionReport
        {
            public string Label { get; set; } = string.Empty;
            public string Output { get; set; } = string.Empty;
            public SemanticVersion? ParsedVersion { get; set; }
            public ValidationLevel Level { get; set; } = ValidationLevel.Info;
            public string Message { get; set; } = string.Empty;
        }

        private sealed class ProjectToolOverride
        {
            public string ProjectFilePath { get; set; } = string.Empty;
            public string RawValue { get; set; } = string.Empty;
            public string ExpandedValue { get; set; } = string.Empty;
        }

        private sealed class SemanticVersion
        {
            public string Raw { get; set; } = string.Empty;
            public int Major { get; set; }
            public int Minor { get; set; }
            public int Patch { get; set; }
        }

        private enum ValidationLevel
        {
            Ok,
            Info,
            Warning,
            Error
        }
    }
}
