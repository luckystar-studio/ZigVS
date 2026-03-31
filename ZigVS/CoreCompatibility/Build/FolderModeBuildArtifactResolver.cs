namespace ZigVS.CoreCompatibility.Build
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    internal sealed class FolderModeBuildArtifactResolver : IBuildArtifactResolver
    {
        readonly Func<BuildArtifactResolutionRequest, string> m_BuildInfoNameResolver;

        internal FolderModeBuildArtifactResolver()
            : this(GetArtifactNameFromBuildInfo)
        {
        }

        internal FolderModeBuildArtifactResolver(Func<BuildArtifactResolutionRequest, string> buildInfoNameResolver)
        {
            m_BuildInfoNameResolver = buildInfoNameResolver ?? throw new ArgumentNullException(nameof(buildInfoNameResolver));
        }

        public BuildArtifactResolutionResult Resolve(BuildArtifactResolutionRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            List<string> attemptedPaths = new List<string>();

            string artifactFileName = (m_BuildInfoNameResolver(request) ?? string.Empty).Trim();
            ArtifactNameSource nameSource = ArtifactNameSource.BuildInfoCapturer;

            if (string.IsNullOrWhiteSpace(artifactFileName))
            {
                artifactFileName = (request.FallbackExecutableBaseName ?? string.Empty).Trim();
                nameSource = ArtifactNameSource.FolderNameFallback;
            }

            if (string.IsNullOrWhiteSpace(artifactFileName))
            {
                return new BuildArtifactResolutionResult
                {
                    Success = false,
                    NameSource = ArtifactNameSource.Unknown,
                    AttemptedPaths = attemptedPaths,
                    Message = "Could not determine the executable name."
                };
            }

            foreach (string candidatePath in GetCandidatePaths(request.OutputDirectory, artifactFileName, request.ExecutableExtension))
            {
                attemptedPaths.Add(candidatePath);
                if (File.Exists(candidatePath))
                {
                    return new BuildArtifactResolutionResult
                    {
                        Success = true,
                        ArtifactFileName = Path.GetFileName(candidatePath),
                        ArtifactPath = candidatePath,
                        NameSource = nameSource,
                        AttemptedPaths = attemptedPaths,
                        Message = nameSource == ArtifactNameSource.BuildInfoCapturer
                            ? "Resolved using BuildInfoCapturer."
                            : "BuildInfoCapturer returned no name. Resolved using folder name fallback."
                    };
                }
            }

            return new BuildArtifactResolutionResult
            {
                Success = false,
                ArtifactFileName = artifactFileName,
                NameSource = nameSource,
                AttemptedPaths = attemptedPaths,
                Message = nameSource == ArtifactNameSource.BuildInfoCapturer
                    ? "BuildInfoCapturer returned a name, but the executable was not found in the output directory."
                    : "BuildInfoCapturer returned no name, and the folder name fallback was not found in the output directory."
            };
        }

        static IEnumerable<string> GetCandidatePaths(string outputDirectory, string artifactFileName, string executableExtension)
        {
            HashSet<string> seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrWhiteSpace(outputDirectory) && !string.IsNullOrWhiteSpace(artifactFileName))
            {
                AddCandidate(Path.Combine(outputDirectory, artifactFileName), seenPaths);

                if (!string.IsNullOrWhiteSpace(executableExtension) &&
                    !artifactFileName.EndsWith(executableExtension, StringComparison.OrdinalIgnoreCase))
                {
                    AddCandidate(Path.Combine(outputDirectory, artifactFileName + executableExtension), seenPaths);
                }
            }

            foreach (string candidatePath in seenPaths)
            {
                yield return candidatePath;
            }
        }

        static void AddCandidate(string candidatePath, HashSet<string> seenPaths)
        {
            if (!string.IsNullOrWhiteSpace(candidatePath))
            {
                seenPaths.Add(candidatePath);
            }
        }

        static string GetArtifactNameFromBuildInfo(BuildArtifactResolutionRequest request)
        {
            return BuildInfo.GetExeName(
                request.BuildFilePath,
                request.IntermediateDirectory,
                request.ResolvedZigExePath);
        }
    }
}
