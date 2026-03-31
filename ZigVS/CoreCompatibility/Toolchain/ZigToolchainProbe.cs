namespace ZigVS.CoreCompatibility.Toolchain
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text.RegularExpressions;

    internal sealed class ZigToolchainProbe : IZigToolchainProbe
    {
        static readonly Regex s_VersionRegex = new Regex(@"\d+\.\d+\.\d+(?:[-+][0-9A-Za-z.+-]+)?", RegexOptions.Compiled);

        readonly Func<string, string?> m_PathResolver;
        readonly Func<string, string, ToolExecutionResult> m_ToolRunner;

        internal ZigToolchainProbe()
            : this(Utilities.ResolvePath, RunTool)
        {
        }

        internal ZigToolchainProbe(
            Func<string, string?> pathResolver,
            Func<string, string, ToolExecutionResult> toolRunner)
        {
            m_PathResolver = pathResolver ?? throw new ArgumentNullException(nameof(pathResolver));
            m_ToolRunner = toolRunner ?? throw new ArgumentNullException(nameof(toolRunner));
        }

        public ToolchainProbeResult Probe(ToolchainProbeRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            ToolchainProbeResult result = new ToolchainProbeResult
            {
                Label = request.Label ?? string.Empty,
                RawValue = NormalizeConfiguredPath(request.RawValue),
                ExpandedValue = NormalizeConfiguredPath(request.ExpandedValue)
            };

            if (string.IsNullOrWhiteSpace(result.RawValue))
            {
                result.PathStatus = ProbeStatus.Error;
                result.PathMessage = "Setting is empty.";
                result.Status = result.PathStatus;
                return result;
            }

            string? resolvedPath = ResolveConfiguredExecutable(result.ExpandedValue);
            if (string.IsNullOrWhiteSpace(resolvedPath))
            {
                result.PathStatus = ProbeStatus.Error;
                result.PathMessage = "Could not resolve the executable path.";
                result.Status = result.PathStatus;
                return result;
            }

            result.ResolvedPath = resolvedPath;
            result.ResolvedFromPath = IsPathSearchValue(result.ExpandedValue);
            result.Exists = true;
            result.PathStatus = ProbeStatus.Ok;
            result.PathMessage = result.ResolvedFromPath ? "Resolved from PATH." : "Resolved successfully.";
            result.Status = result.PathStatus;

            if (string.IsNullOrWhiteSpace(request.PrimaryVersionArguments) &&
                string.IsNullOrWhiteSpace(request.FallbackVersionArguments))
            {
                return result;
            }

            result.VersionProbeAttempted = true;

            ToolExecutionResult versionExecution = TryRunVersionCommand(result.ResolvedPath!, request.PrimaryVersionArguments);
            if (!versionExecution.Succeeded && !string.IsNullOrWhiteSpace(request.FallbackVersionArguments))
            {
                versionExecution = TryRunVersionCommand(result.ResolvedPath!, request.FallbackVersionArguments);
            }

            if (!versionExecution.Succeeded)
            {
                result.VersionStatus = ProbeStatus.Warning;
                result.VersionMessage = "Executable was found, but the version command did not succeed.";
                result.Status = Max(result.PathStatus, result.VersionStatus);
                return result;
            }

            FillVersionInformation(result, versionExecution.Output);
            result.Status = Max(result.PathStatus, result.VersionStatus);
            return result;
        }

        ToolExecutionResult TryRunVersionCommand(string fileName, string arguments)
        {
            if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(arguments))
            {
                return new ToolExecutionResult
                {
                    Succeeded = false
                };
            }

            return m_ToolRunner(fileName, arguments);
        }

        static void FillVersionInformation(ToolchainProbeResult result, string output)
        {
            result.VersionOutput = (output ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(result.VersionOutput))
            {
                result.VersionStatus = ProbeStatus.Warning;
                result.VersionMessage = "Version command returned no output.";
                return;
            }

            result.VersionStatus = ProbeStatus.Ok;
            result.VersionMessage = result.VersionOutput;

            Match match = s_VersionRegex.Match(result.VersionOutput);
            if (match.Success)
            {
                result.SemanticVersion = ParseVersionString(match.Value);
            }
        }

        string? ResolveConfiguredExecutable(string configuredValue)
        {
            string value = NormalizeConfiguredPath(configuredValue);
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            try
            {
                if (IsPathSearchValue(value))
                {
                    string? resolvedFromPath = m_PathResolver(value);
                    if (!string.IsNullOrWhiteSpace(resolvedFromPath) && File.Exists(resolvedFromPath))
                    {
                        return Path.GetFullPath(resolvedFromPath);
                    }

                    return null;
                }

                string fullPath = Path.GetFullPath(value);
                return File.Exists(fullPath) ? fullPath : null;
            }
            catch
            {
                return null;
            }
        }

        static string NormalizeConfiguredPath(string? value)
        {
            return (value ?? string.Empty).Trim().Trim('"');
        }

        static bool IsPathSearchValue(string value)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   !value.Contains(Path.DirectorySeparatorChar.ToString()) &&
                   !value.Contains(Path.AltDirectorySeparatorChar.ToString()) &&
                   !Path.IsPathRooted(value);
        }

        internal static SemanticVersion? ParseVersionString(string version)
        {
            string normalizedVersion = (version ?? string.Empty).Trim();
            Match match = Regex.Match(normalizedVersion, @"^(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)");
            if (!match.Success)
            {
                return null;
            }

            return new SemanticVersion
            {
                Raw = normalizedVersion,
                Major = int.Parse(match.Groups["major"].Value),
                Minor = int.Parse(match.Groups["minor"].Value),
                Patch = int.Parse(match.Groups["patch"].Value)
            };
        }

        static ProbeStatus Max(ProbeStatus left, ProbeStatus right)
        {
            return left >= right ? left : right;
        }

        static ToolExecutionResult RunTool(string fileName, string arguments)
        {
            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    WorkingDirectory = Path.GetDirectoryName(fileName) ?? Environment.CurrentDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (Process process = new Process())
                {
                    process.StartInfo = processStartInfo;
                    if (!process.Start())
                    {
                        return new ToolExecutionResult
                        {
                            Succeeded = false
                        };
                    }

                    if (!process.WaitForExit(3000))
                    {
                        try
                        {
                            process.Kill();
                        }
                        catch
                        {
                        }

                        return new ToolExecutionResult
                        {
                            Succeeded = false
                        };
                    }

                    string standardOutput = process.StandardOutput.ReadToEnd();
                    string standardError = process.StandardError.ReadToEnd();

                    return new ToolExecutionResult
                    {
                        Succeeded = process.ExitCode == 0,
                        Output = (standardOutput + Environment.NewLine + standardError).Trim()
                    };
                }
            }
            catch
            {
                return new ToolExecutionResult
                {
                    Succeeded = false
                };
            }
        }
    }
}
