namespace ZigVS.CoreCompatibility
{
    using System.Collections.Generic;

    internal interface IBuildArtifactResolver
    {
        BuildArtifactResolutionResult Resolve(BuildArtifactResolutionRequest request);
    }

    internal interface ITestResultParser
    {
        ZigTestParseResult Parse(ZigTestProcessOutput output);
    }

    internal interface IZigToolchainProbe
    {
        ToolchainProbeResult Probe(ToolchainProbeRequest request);
    }

    internal sealed class BuildArtifactResolutionRequest
    {
        public string BuildFilePath { get; set; } = string.Empty;
        public string IntermediateDirectory { get; set; } = string.Empty;
        public string OutputDirectory { get; set; } = string.Empty;
        public string WorkingDirectory { get; set; } = string.Empty;
        public string Configuration { get; set; } = string.Empty;
        public string FallbackExecutableBaseName { get; set; } = string.Empty;
        public string ExecutableExtension { get; set; } = string.Empty;
        public string ResolvedZigExePath { get; set; } = string.Empty;
    }

    internal sealed class BuildArtifactResolutionResult
    {
        public bool Success { get; set; }
        public string ArtifactFileName { get; set; } = string.Empty;
        public string ArtifactPath { get; set; } = string.Empty;
        public ArtifactNameSource NameSource { get; set; } = ArtifactNameSource.Unknown;
        public IReadOnlyList<string> AttemptedPaths { get; set; } = new string[0];
        public string Message { get; set; } = string.Empty;
    }

    internal enum ArtifactNameSource
    {
        Unknown = 0,
        BuildInfoCapturer = 1,
        FolderNameFallback = 2
    }

    internal sealed class ToolchainProbeRequest
    {
        public string Label { get; set; } = string.Empty;
        public string RawValue { get; set; } = string.Empty;
        public string ExpandedValue { get; set; } = string.Empty;
        public string DefaultFileName { get; set; } = string.Empty;
        public string PrimaryVersionArguments { get; set; } = string.Empty;
        public string FallbackVersionArguments { get; set; } = string.Empty;
    }

    internal sealed class ToolchainProbeResult
    {
        public string Label { get; set; } = string.Empty;
        public string RawValue { get; set; } = string.Empty;
        public string ExpandedValue { get; set; } = string.Empty;
        public string? ResolvedPath { get; set; }
        public bool ResolvedFromPath { get; set; }
        public bool Exists { get; set; }
        public bool VersionProbeAttempted { get; set; }
        public string VersionOutput { get; set; } = string.Empty;
        public SemanticVersion? SemanticVersion { get; set; }
        public ProbeStatus PathStatus { get; set; } = ProbeStatus.Info;
        public string PathMessage { get; set; } = string.Empty;
        public ProbeStatus VersionStatus { get; set; } = ProbeStatus.Info;
        public string VersionMessage { get; set; } = string.Empty;
        public ProbeStatus Status { get; set; } = ProbeStatus.Info;
    }

    internal sealed class ToolExecutionResult
    {
        public bool Succeeded { get; set; }
        public string Output { get; set; } = string.Empty;
    }

    internal enum ProbeStatus
    {
        Ok = 0,
        Info = 1,
        Warning = 2,
        Error = 3
    }

    internal sealed class SemanticVersion
    {
        public string Raw { get; set; } = string.Empty;
        public int Major { get; set; }
        public int Minor { get; set; }
        public int Patch { get; set; }
    }

    internal sealed class ZigTestProcessOutput
    {
        public string StandardOutput { get; set; } = string.Empty;
        public string StandardError { get; set; } = string.Empty;
        public int ExitCode { get; set; }
    }

    internal sealed class ZigTestParseResult
    {
        public ZigTestOutcome Outcome { get; set; } = ZigTestOutcome.Failed;
        public string Output { get; set; } = string.Empty;
        public string? DiagnosticMessage { get; set; }
    }

    internal enum ZigTestOutcome
    {
        Passed = 0,
        Failed = 1,
        Skipped = 2,
        NotFound = 3
    }

    internal sealed class ProjectToolOverride
    {
        public string ProjectFilePath { get; set; } = string.Empty;
        public string RawValue { get; set; } = string.Empty;
        public string ExpandedValue { get; set; } = string.Empty;
    }
}
