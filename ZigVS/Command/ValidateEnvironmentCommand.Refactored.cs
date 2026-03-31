/********************************************************************************************
Copyright(c) 2023 LuckyStar Studio LLC
All rights reserved.
********************************************************************************************/

namespace ZigVS.Command
{
#nullable enable
    using ZigVS.CoreCompatibility;
    using ZigVS.CoreCompatibility.Toolchain;
    using Microsoft.VisualStudio.Shell;
    using System;
    using System.ComponentModel.Design;
    using Task = System.Threading.Tasks.Task;

    internal sealed class ValidateEnvironmentCommand
    {
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

        void Execute(object i_senderObject, EventArgs i_EventArgs)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Common.OutputWindowPane.Show();
            WriteSection("Validate Zig Environment");
            WriteLine(ProbeStatus.Info, "Starting environment validation.");

            try
            {
                var l_GeneralOptions = ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    return await GeneralOptions.GetLiveInstanceAsync();
                });

                if (l_GeneralOptions == null)
                {
                    WriteLine(ProbeStatus.Error, "Could not load Tools > Options > ZigVS > General.");
                    return;
                }

                ExecuteValidation(l_GeneralOptions);
            }
            catch (Exception l_Exception)
            {
                WriteLine(ProbeStatus.Error, "Validation failed: " + l_Exception.Message);
            }
        }

        void ExecuteValidation(GeneralOptions i_GeneralOptions)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            WriteSection("Context");
            Utilities.SolutionMode l_SolutionMode = Utilities.GetSolutionMode();
            WriteLine(ProbeStatus.Info, "Solution mode: " + l_SolutionMode);

            string l_ActivePathString = string.Empty;
            if (l_SolutionMode == Utilities.SolutionMode.ProjectMode)
            {
                l_ActivePathString = Utilities.GetCurrentProjectPath();
            }
            else if (l_SolutionMode == Utilities.SolutionMode.OpenFolderMode)
            {
                l_ActivePathString = Utilities.GetOpenFolderPath();
            }

            if (!string.IsNullOrWhiteSpace(l_ActivePathString))
            {
                WriteLine(ProbeStatus.Info, "Active path: " + l_ActivePathString);
            }

            ToolchainProbeResult l_GlobalZig = ProbeTool(
                "Global zig.exe",
                i_GeneralOptions.ToolPath,
                i_GeneralOptions.ToolPathExpanded,
                Parameter.c_compilerFileName,
                "version",
                "--version");

            ProjectToolOverride? l_ProjectToolOverride = ProjectToolOverrideReader.TryReadStartupProjectOverride();
            ToolchainProbeResult? l_ProjectZig = null;
            if (l_ProjectToolOverride != null)
            {
                l_ProjectZig = ProbeTool(
                    "Project ToolPath",
                    l_ProjectToolOverride.RawValue,
                    l_ProjectToolOverride.ExpandedValue,
                    Parameter.c_compilerFileName,
                    "version",
                    "--version");
            }

            ToolchainProbeResult l_Zls = ProbeTool(
                "Global zls.exe",
                i_GeneralOptions.LanguageServerPath,
                i_GeneralOptions.LanguageServerPathExpanded,
                Parameter.c_languageServerFileName,
                "--version",
                "version");

            WriteSection("Tool Paths");
            WriteToolReport(l_GlobalZig);
            if (l_ProjectZig != null)
            {
                WriteToolReport(l_ProjectZig, l_ProjectToolOverride?.ProjectFilePath);
            }
            else
            {
                WriteLine(ProbeStatus.Info, "Project ToolPath override: not detected.");
            }
            WriteToolReport(l_Zls);

            WriteSection("Versions");
            WriteVersionReport(l_GlobalZig);
            if (l_ProjectZig != null)
            {
                WriteVersionReport(l_ProjectZig);
            }
            WriteVersionReport(l_Zls);

            WriteSection("Consistency");
            CompareToolchainVersions("Global zig.exe vs zls.exe", l_GlobalZig, l_Zls);
            if (l_ProjectZig != null)
            {
                CompareToolchainVersions("Project ToolPath vs Global zig.exe", l_ProjectZig, l_GlobalZig);
                CompareToolchainVersions("Project ToolPath vs zls.exe", l_ProjectZig, l_Zls);
            }

            WriteSection("Recommended Settings");
            WriteRecommendations(i_GeneralOptions, l_GlobalZig, l_ProjectZig, l_Zls);

            WriteSection("Summary");
            bool l_HasError =
                HasBlockingIssue(l_GlobalZig) ||
                HasBlockingIssue(l_Zls) ||
                (l_ProjectZig != null && HasBlockingIssue(l_ProjectZig));

            if (l_HasError)
            {
                WriteLine(ProbeStatus.Error, "Environment validation finished with blocking issues.");
            }
            else
            {
                WriteLine(ProbeStatus.Ok, "Environment validation finished.");
            }
        }

        static ToolchainProbeResult ProbeTool(
            string i_LabelString,
            string? i_RawValueString,
            string? i_ExpandedValueString,
            string i_DefaultFileNameString,
            string i_PrimaryVersionArgumentsString,
            string i_FallbackVersionArgumentsString)
        {
            return CoreServices.ToolchainProbe.Probe(new ToolchainProbeRequest
            {
                Label = i_LabelString,
                RawValue = i_RawValueString ?? string.Empty,
                ExpandedValue = i_ExpandedValueString ?? string.Empty,
                DefaultFileName = i_DefaultFileNameString,
                PrimaryVersionArguments = i_PrimaryVersionArgumentsString,
                FallbackVersionArguments = i_FallbackVersionArgumentsString
            });
        }

        static bool HasBlockingIssue(ToolchainProbeResult i_Result)
        {
            return i_Result.Status == ProbeStatus.Error;
        }

        static void CompareToolchainVersions(string i_LabelString, ToolchainProbeResult? i_LeftResult, ToolchainProbeResult? i_RightResult)
        {
            if (i_LeftResult == null || i_RightResult == null)
            {
                return;
            }

            if (i_LeftResult.SemanticVersion == null || i_RightResult.SemanticVersion == null)
            {
                WriteLine(ProbeStatus.Info, i_LabelString + ": comparison skipped because a version could not be parsed.");
                return;
            }

            if (i_LeftResult.SemanticVersion.Major == i_RightResult.SemanticVersion.Major &&
                i_LeftResult.SemanticVersion.Minor == i_RightResult.SemanticVersion.Minor)
            {
                WriteLine(
                    ProbeStatus.Ok,
                    i_LabelString + ": " + i_LeftResult.SemanticVersion.Raw + " and " + i_RightResult.SemanticVersion.Raw + " are aligned.");
                return;
            }

            WriteLine(
                ProbeStatus.Warning,
                i_LabelString + ": " + i_LeftResult.SemanticVersion.Raw + " and " + i_RightResult.SemanticVersion.Raw +
                " do not share the same major.minor version.");
        }

        static void WriteToolReport(ToolchainProbeResult i_Result, string? i_SourcePathString = null)
        {
            WriteLine(i_Result.PathStatus, i_Result.Label + ": " + i_Result.PathMessage);
            WriteIndented("Raw setting: " + DisplayOrMissing(i_Result.RawValue));
            WriteIndented("Expanded setting: " + DisplayOrMissing(i_Result.ExpandedValue));
            if (!string.IsNullOrWhiteSpace(i_SourcePathString))
            {
                WriteIndented("Source: " + i_SourcePathString);
            }
            WriteIndented("Resolved path: " + DisplayOrMissing(i_Result.ResolvedPath));
        }

        static void WriteVersionReport(ToolchainProbeResult i_Result)
        {
            if (!i_Result.Exists || string.IsNullOrWhiteSpace(i_Result.ResolvedPath))
            {
                WriteLine(ProbeStatus.Error, i_Result.Label + " version: Skipped because the executable is unresolved.");
                return;
            }

            if (!i_Result.VersionProbeAttempted)
            {
                WriteLine(ProbeStatus.Warning, i_Result.Label + " version: Version probe was not attempted.");
                return;
            }

            WriteLine(i_Result.VersionStatus, i_Result.Label + " version: " + DisplayOrMissing(i_Result.VersionMessage));
        }

        static void WriteRecommendations(
            GeneralOptions i_GeneralOptions,
            ToolchainProbeResult i_GlobalZig,
            ToolchainProbeResult? i_ProjectZig,
            ToolchainProbeResult i_Zls)
        {
            if (i_GlobalZig.Exists && i_GlobalZig.ResolvedFromPath)
            {
                WriteLine(ProbeStatus.Ok, "Global zig.exe is being resolved from PATH. This matches the recommended setup.");
            }
            else if (i_GlobalZig.Exists)
            {
                WriteLine(ProbeStatus.Info, "Global zig.exe is pinned to a specific path. This is fine, but PATH or $(ZIG_HOME) is easier to share across machines.");
            }

            if (i_Zls.Exists && i_Zls.ResolvedFromPath)
            {
                WriteLine(ProbeStatus.Ok, "zls.exe is being resolved from PATH. This matches the recommended setup.");
            }
            else if (i_Zls.Exists)
            {
                WriteLine(ProbeStatus.Info, "zls.exe is pinned to a specific path. Consider PATH or a stable environment variable if the team shares settings.");
            }

            if (i_GeneralOptions.TDebugSwitch == ZigVS.Switch.on)
            {
                WriteLine(ProbeStatus.Warning, "Language Server Debug Mode is ON. Recommended default is OFF unless you are diagnosing ZLS communication.");
            }
            else
            {
                WriteLine(ProbeStatus.Ok, "Language Server Debug Mode is OFF.");
            }

            if (i_GlobalZig.SemanticVersion != null && i_Zls.SemanticVersion != null)
            {
                if (i_GlobalZig.SemanticVersion.Major != i_Zls.SemanticVersion.Major ||
                    i_GlobalZig.SemanticVersion.Minor != i_Zls.SemanticVersion.Minor)
                {
                    WriteLine(ProbeStatus.Warning, "Recommended: keep global zig.exe and zls.exe on the same major.minor release line.");
                }
            }

            if (i_ProjectZig == null)
            {
                WriteLine(ProbeStatus.Info, "No project ToolPath override detected. The global zig.exe setting will be used for editor features.");
                return;
            }

            if (!i_ProjectZig.Exists)
            {
                WriteLine(ProbeStatus.Warning, "Project ToolPath override is present but could not be resolved. Align or clear it unless the project intentionally pins another Zig.");
                return;
            }

            if (i_ProjectZig.SemanticVersion != null &&
                i_GlobalZig.SemanticVersion != null &&
                (i_ProjectZig.SemanticVersion.Major != i_GlobalZig.SemanticVersion.Major ||
                 i_ProjectZig.SemanticVersion.Minor != i_GlobalZig.SemanticVersion.Minor))
            {
                WriteLine(ProbeStatus.Warning, "Project ToolPath points to a different Zig version than Tools > Options. Only do this intentionally.");
                return;
            }

            WriteLine(ProbeStatus.Info, "Project ToolPath override is present. Keep it aligned with the global toolchain unless the project truly requires a different Zig.");
        }

        static string DisplayOrMissing(string? i_ValueString)
        {
            return string.IsNullOrWhiteSpace(i_ValueString) ? "<missing>" : i_ValueString!;
        }

        static void WriteSection(string i_TitleString)
        {
            Common.OutputWindowPane.OutputString(Environment.NewLine + i_TitleString + Environment.NewLine);
            Common.OutputWindowPane.OutputString(new string('-', i_TitleString.Length) + Environment.NewLine);
        }

        static void WriteIndented(string i_MessageString)
        {
            Common.OutputWindowPane.OutputString("  " + i_MessageString + Environment.NewLine);
        }

        static void WriteLine(ProbeStatus i_Status, string i_MessageString)
        {
            Common.OutputWindowPane.OutputString("[" + i_Status.ToString().ToUpperInvariant() + "] " + i_MessageString + Environment.NewLine);
        }
    }
}
