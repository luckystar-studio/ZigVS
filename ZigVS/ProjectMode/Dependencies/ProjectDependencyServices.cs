namespace ZigVS
{
#nullable enable
    using Microsoft.Build.Construction;
    using Microsoft.Build.Evaluation;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using ZigVS.CoreCompatibility;

    public sealed class ProjectDependencyRepository
    {
        public IReadOnlyList<ProjectDependencySpec> Load(Project buildProject)
        {
            return buildProject.GetItems("ZigDependency")
                .Select(ToSpec)
                .OrderBy(spec => spec.Include, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public void Upsert(Project buildProject, ProjectDependencySpec spec)
        {
            Remove(buildProject, spec.Include, false);

            ProjectItem item = buildProject.AddItem("ZigDependency", spec.Include).Single();
            item.SetMetadataValue("RepositoryUrl", spec.RepositoryUrl.Trim());
            item.SetMetadataValue("Commit", spec.Commit.Trim());
            item.SetMetadataValue("ModuleName", spec.ModuleName.Trim());
            item.SetMetadataValue("RootSource", NormalizeRelativePath(spec.RootSource));
            item.SetMetadataValue("CheckoutDir", NormalizeRelativePath(spec.CheckoutDir));

            buildProject.Save();
        }

        public void Remove(Project buildProject, string include)
        {
            Remove(buildProject, include, true);
        }

        public ProjectDependencySpec? Find(Project buildProject, string include)
        {
            ProjectItem? item = buildProject.GetItems("ZigDependency")
                .FirstOrDefault(candidate => String.Equals(candidate.EvaluatedInclude, include, StringComparison.OrdinalIgnoreCase));

            return item == null ? null : ToSpec(item);
        }

        private static void Remove(Project buildProject, string include, bool saveProject)
        {
            List<ProjectItem> items = buildProject.GetItems("ZigDependency")
                .Where(item => String.Equals(item.EvaluatedInclude, include, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (items.Count == 0)
            {
                return;
            }

            buildProject.RemoveItems(items);
            if (saveProject)
            {
                buildProject.Save();
            }
        }

        private static ProjectDependencySpec ToSpec(ProjectItem item)
        {
            return new ProjectDependencySpec
            {
                Include = item.EvaluatedInclude,
                RepositoryUrl = item.GetMetadataValue("RepositoryUrl"),
                Commit = item.GetMetadataValue("Commit"),
                ModuleName = item.GetMetadataValue("ModuleName"),
                RootSource = NormalizeRelativePath(item.GetMetadataValue("RootSource")),
                CheckoutDir = NormalizeRelativePath(item.GetMetadataValue("CheckoutDir"))
            };
        }

        private static string NormalizeRelativePath(string path)
        {
            return path.Replace('\\', '/').Trim();
        }
    }

    public sealed class DependencyAutoDetector
    {
        private static readonly Regex BuildZigZonNameRegex = new Regex(@"\.name\s*=\s*""(?<name>[^""]+)""", RegexOptions.Compiled);
        private static readonly Regex RootSourceDotPathRegex = new Regex(@"root_source_file\s*=\s*\.\s*\{\s*\.path\s*=\s*""(?<path>[^""]+)""", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex RootSourceBPathRegex = new Regex(@"root_source_file\s*=\s*b\.path\(\s*""(?<path>[^""]+)""\s*\)", RegexOptions.Compiled | RegexOptions.Singleline);

        public ProjectDependencyDetectionResult Detect(string repositoryRootPath, string fallbackName)
        {
            return new ProjectDependencyDetectionResult
            {
                ModuleName = DetectModuleName(repositoryRootPath, fallbackName),
                RootSource = DetectRootSource(repositoryRootPath)
            };
        }

        public string DetectModuleName(string repositoryRootPath, string fallbackName)
        {
            string? zigProjAssemblyName = ReadFirstZigProjProperty(repositoryRootPath, "AssemblyName");
            if (!String.IsNullOrWhiteSpace(zigProjAssemblyName))
            {
                return SanitizeModuleName(zigProjAssemblyName);
            }

            string buildZigZonPath = Path.Combine(repositoryRootPath, "build.zig.zon");
            if (File.Exists(buildZigZonPath))
            {
                Match match = BuildZigZonNameRegex.Match(File.ReadAllText(buildZigZonPath));
                if (match.Success)
                {
                    return SanitizeModuleName(match.Groups["name"].Value);
                }
            }

            return SanitizeModuleName(fallbackName);
        }

        public string DetectRootSource(string repositoryRootPath)
        {
            string? zigProjRootSource = ReadFirstZigProjProperty(repositoryRootPath, "RootSourceName");
            if (!String.IsNullOrWhiteSpace(zigProjRootSource))
            {
                return NormalizeRelativePath(zigProjRootSource);
            }

            string buildZigPath = Path.Combine(repositoryRootPath, "build.zig");
            if (File.Exists(buildZigPath))
            {
                string buildZigText = File.ReadAllText(buildZigPath);
                Match match = RootSourceBPathRegex.Match(buildZigText);
                if (!match.Success)
                {
                    match = RootSourceDotPathRegex.Match(buildZigText);
                }

                if (match.Success)
                {
                    return NormalizeRelativePath(match.Groups["path"].Value);
                }
            }

            foreach (string candidate in new[] { "src/root.zig", "src/lib.zig", "src/main.zig", "main.zig" })
            {
                if (File.Exists(Path.Combine(repositoryRootPath, candidate.Replace('/', Path.DirectorySeparatorChar))))
                {
                    return candidate;
                }
            }

            return String.Empty;
        }

        private static string? ReadFirstZigProjProperty(string repositoryRootPath, string propertyName)
        {
            string[] zigProjectFiles = Directory.GetFiles(repositoryRootPath, "*.zigproj", SearchOption.TopDirectoryOnly);
            if (zigProjectFiles.Length == 0)
            {
                return null;
            }

            try
            {
                XDocument document = XDocument.Load(zigProjectFiles[0]);
                XNamespace ns = document.Root?.Name.Namespace ?? XNamespace.None;
                XElement? propertyElement = document.Descendants(ns + propertyName)
                    .FirstOrDefault(element => !String.IsNullOrWhiteSpace(element.Value));
                return propertyElement?.Value.Trim();
            }
            catch
            {
                return null;
            }
        }

        internal static string NormalizeRelativePath(string path)
        {
            return path.Replace('\\', '/').Trim();
        }

        internal static string SanitizeModuleName(string value)
        {
            if (String.IsNullOrWhiteSpace(value))
            {
                return "dependency";
            }

            string sanitized = Regex.Replace(value.Trim(), @"[^A-Za-z0-9_\-]", "_");
            if (Char.IsDigit(sanitized[0]))
            {
                sanitized = "_" + sanitized;
            }

            return sanitized;
        }
    }

    public sealed class DependencyGitClient
    {
        public string ResolveCommit(string gitPath, string repositoryUrl, string? referenceName)
        {
            if (LooksLikeCommit(referenceName))
            {
                return referenceName!;
            }

            foreach (string refSpec in GetRefCandidates(referenceName))
            {
                string output = RunGit(gitPath, String.Empty, $"ls-remote \"{repositoryUrl}\" {refSpec}", false).Trim();
                if (String.IsNullOrWhiteSpace(output))
                {
                    continue;
                }

                string commit = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(line => line.Split('\t').FirstOrDefault())
                    .FirstOrDefault(value => !String.IsNullOrWhiteSpace(value)) ?? String.Empty;
                if (!String.IsNullOrWhiteSpace(commit))
                {
                    return commit.Trim();
                }
            }

            throw new InvalidOperationException("Failed to resolve the selected repository reference to a commit.");
        }

        public void Clone(string gitPath, string repositoryUrl, string targetDirectoryPath)
        {
            string? parentDirectoryPath = Path.GetDirectoryName(targetDirectoryPath);
            if (String.IsNullOrWhiteSpace(parentDirectoryPath))
            {
                throw new InvalidOperationException("Unable to resolve the dependency checkout directory.");
            }

            Directory.CreateDirectory(parentDirectoryPath);
            RunGit(gitPath, parentDirectoryPath, $"clone --recursive \"{repositoryUrl}\" \"{targetDirectoryPath}\"");
        }

        public void EnsureCheckout(string gitPath, string repositoryUrl, string commit, string checkoutDirectoryPath)
        {
            if (!Directory.Exists(Path.Combine(checkoutDirectoryPath, ".git")))
            {
                Clone(gitPath, repositoryUrl, checkoutDirectoryPath);
            }

            RunGit(gitPath, checkoutDirectoryPath, "fetch --all --tags --prune");
            RunGit(gitPath, checkoutDirectoryPath, $"checkout --detach {commit}");
            RunGit(gitPath, checkoutDirectoryPath, "submodule update --init --recursive");
        }

        public bool IsWorkingTreeDirty(string gitPath, string checkoutDirectoryPath)
        {
            if (!Directory.Exists(Path.Combine(checkoutDirectoryPath, ".git")))
            {
                return false;
            }

            string output = RunGit(gitPath, checkoutDirectoryPath, "status --porcelain", false);
            return !String.IsNullOrWhiteSpace(output);
        }

        public string GetCurrentCommit(string gitPath, string checkoutDirectoryPath)
        {
            return RunGit(gitPath, checkoutDirectoryPath, "rev-parse HEAD", false).Trim();
        }

        private static IEnumerable<string> GetRefCandidates(string? referenceName)
        {
            if (String.IsNullOrWhiteSpace(referenceName))
            {
                return new[] { "HEAD" };
            }

            return new[]
            {
                $"refs/heads/{referenceName}",
                $"refs/tags/{referenceName}^{{}}",
                $"refs/tags/{referenceName}",
                referenceName
            };
        }

        private static bool LooksLikeCommit(string? referenceName)
        {
            return !String.IsNullOrWhiteSpace(referenceName) && Regex.IsMatch(referenceName!, "^[0-9a-fA-F]{7,40}$");
        }

        private static string RunGit(string gitPath, string workingDirectoryPath, string arguments, bool throwOnError = true)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = gitPath,
                Arguments = arguments,
                WorkingDirectory = String.IsNullOrWhiteSpace(workingDirectoryPath) ? Environment.CurrentDirectory : workingDirectoryPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process())
            {
                process.StartInfo = startInfo;
                process.Start();
                string stdout = process.StandardOutput.ReadToEnd();
                string stderr = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (throwOnError && process.ExitCode != 0)
                {
                    throw new InvalidOperationException(String.IsNullOrWhiteSpace(stderr)
                        ? $"git {arguments} failed with exit code {process.ExitCode}."
                        : stderr.Trim());
                }

                return String.IsNullOrWhiteSpace(stdout) ? stderr : stdout;
            }
        }
    }

    public sealed class ProjectDependencyService
    {
        internal enum DependencyBuildCliStyle
        {
            LegacyDepsAndMod = 0,
            ModernDepAndModule = 1
        }

        private const string LegacyDependenciesOptionValue = "--deps $(Dependencies.Replace(';', ' --deps ')) ";
        private const string LegacyManagedDependenciesOptionValue = "--deps @(ZigDependency->'%(ModuleName)', ' --deps ') ";
        private const string LegacyModulesOptionValue = "--mod $(Modules.Replace(';', ' --mod ')) ";
        private const string LegacyManagedModulesOptionValue = "--mod @(ZigDependency->'%(ModuleName)::%(CheckoutDir)/%(RootSource)', ' --mod ') ";
        private const string ModernDependenciesOptionValue = "--dep $(Dependencies.Replace(';', ' --dep ')) ";
        private const string ModernManagedDependenciesOptionValue = "--dep @(ZigDependency->'%(ModuleName)', ' --dep ') ";
        private const string ModernModulesOptionValue = "-M$(Modules.Replace(';', ' -M')) ";
        private const string ModernManagedModulesOptionValue = "-M@(ZigDependency->'%(ModuleName)=%(CheckoutDir)/%(RootSource)', ' -M') ";
        private const string AllDependenciesOptionValue = "$(DependenciesOption) $(ManagedDependenciesOption)";
        private const string AllModulesOptionValue = "$(ModulesOption) $(ManagedModulesOption)";
        private const string LegacyBuildCommandValue = "\"$(ToolPath)\" $(ZigBuildCommand) --name $(AssemblyName) -target $(ZigArchitecture)-$(ZigOS) -Doptimize=$(ZigOptimize) -femit-bin=$(OutDir)$(OutputName) --cache-dir $(IntDir) $(ZigLibType) $(IncludeDirsOption) $(LibraryDirsOption) $(LibrariesOption) $(AllDependenciesOption) $(AllModulesOption) $(BuildOption) $(RootSourceName) $(AllCFilesFlat.Replace(';', ' ')) $(NativeProjectReferencePaths.Replace(';', ' ')) $(AllRCFilesFlat.Replace(';', ' '))";
        private const string ModernBuildCommandValue = "\"$(ToolPath)\" $(ZigBuildCommand) --name $(AssemblyName) -target $(ZigArchitecture)-$(ZigOS) -Doptimize=$(ZigOptimize) -femit-bin=$(OutDir)$(OutputName) --cache-dir $(IntDir) $(ZigLibType) $(IncludeDirsOption) $(LibraryDirsOption) $(LibrariesOption) $(AllDependenciesOption) -Mroot=$(RootSourceName) $(AllModulesOption) $(BuildOption) $(AllCFilesFlat.Replace(';', ' ')) $(NativeProjectReferencePaths.Replace(';', ' ')) $(AllRCFilesFlat.Replace(';', ' '))";
        private const string BuildOutputExistsConditionValue = "Exists('$(OutDir)$(OutputName)')";
        private const string BuildOutputMissingConditionValue = "!Exists('$(OutDir)$(OutputName)')";
        private const string BuildOutputErrorTaskLabel = "ZigVSBuildOutputError";
        private const string BuildOutputMessageTaskLabel = "ZigVSBuildOutputMessage";
        private const string BuildOutputErrorTextValue = "Zig build did not produce the expected output file: $(OutDir)$(OutputName)";
        private const string BuildOutputMessageTextValue = "Built output: $(OutDir)$(OutputName)";

        private readonly ProjectDependencyRepository repository = new ProjectDependencyRepository();
        private readonly DependencyAutoDetector autoDetector = new DependencyAutoDetector();
        private readonly DependencyGitClient gitClient = new DependencyGitClient();

        public static ProjectDependencyService Instance { get; } = new ProjectDependencyService();

        public IReadOnlyList<ProjectDependencySpec> LoadDependencies(ZigVSProjectNode projectNode)
        {
            List<ProjectDependencySpec> loadedDependencies = repository.Load(projectNode.BuildProject).ToList();
            EnsureCompatibleBuildCli(projectNode, loadedDependencies);
            List<ProjectDependencySpec> repairedDependencies = new List<ProjectDependencySpec>(loadedDependencies.Count);

            foreach (ProjectDependencySpec dependency in loadedDependencies)
            {
                ProjectDependencySpec repairedDependency = RepairLegacyMetadata(projectNode.ProjectFolder, dependency);
                if (HasDependencyMetadataChanges(dependency, repairedDependency))
                {
                    repository.Upsert(projectNode.BuildProject, repairedDependency);
                }

                repairedDependencies.Add(repairedDependency);
            }

            return repairedDependencies;
        }

        internal bool ApplyBuildCliSyntax(Project buildProject, DependencyBuildCliStyle cliStyle)
        {
            if (buildProject == null)
            {
                throw new ArgumentNullException(nameof(buildProject));
            }

            bool changed = false;
            foreach (ProjectTargetElement target in buildProject.Xml.Targets)
            {
                if (!String.Equals(target.Name, "BuildWithOutBuildDotZig", StringComparison.Ordinal))
                {
                    continue;
                }

                foreach (ProjectPropertyGroupElement propertyGroup in target.Children.OfType<ProjectPropertyGroupElement>())
                {
                    foreach (ProjectPropertyElement property in propertyGroup.Properties)
                    {
                        string desiredValue = GetDesiredBuildCliValue(property, cliStyle);
                        if (desiredValue == property.Value)
                        {
                            continue;
                        }

                        property.Value = desiredValue;
                        changed = true;
                    }
                }

                if (EnsureBuildOutputVerificationTasks(target))
                {
                    changed = true;
                }
            }

            if (changed)
            {
                buildProject.ReevaluateIfNecessary();
            }

            return changed;
        }

        public void SaveDependency(ZigVSProjectNode projectNode, ProjectDependencySpec spec)
        {
            Normalize(spec);
            repository.Upsert(projectNode.BuildProject, spec);
            EnsureCompatibleBuildCli(projectNode);
        }

        public void RemoveDependency(ZigVSProjectNode projectNode, string include)
        {
            repository.Remove(projectNode.BuildProject, include);
            EnsureCompatibleBuildCli(projectNode);
        }

        public ProjectDependencySpec? FindDependency(ZigVSProjectNode projectNode, string include)
        {
            return repository.Find(projectNode.BuildProject, include);
        }

        public ProjectDependencySpec PrepareDependency(ProjectDependencyAddRequest request, string projectDirectoryPath, string gitPath, IEnumerable<ProjectDependencySpec> existingDependencies)
        {
            string repositoryUrl = request.RepositoryUrl.Trim();
            string commit = String.IsNullOrWhiteSpace(request.Commit)
                ? gitClient.ResolveCommit(gitPath, repositoryUrl, request.ReferenceName)
                : request.Commit!.Trim();

            string packageRootPath = Path.Combine(projectDirectoryPath, "package");
            string temporaryDirectoryPath = Path.Combine(packageRootPath, ".zigvs-temp-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(packageRootPath);

            try
            {
                gitClient.Clone(gitPath, repositoryUrl, temporaryDirectoryPath);
                gitClient.EnsureCheckout(gitPath, repositoryUrl, commit, temporaryDirectoryPath);

                string fallbackName = String.IsNullOrWhiteSpace(request.SuggestedName)
                    ? GetRepositoryName(repositoryUrl)
                    : request.SuggestedName!;
                ProjectDependencyDetectionResult detection = autoDetector.Detect(temporaryDirectoryPath, fallbackName);
                string include = GetUniqueInclude(existingDependencies, detection.ModuleName);
                string checkoutDir = $"package/{include}";
                string finalDirectoryPath = Path.Combine(projectDirectoryPath, checkoutDir.Replace('/', Path.DirectorySeparatorChar));

                if (Directory.Exists(finalDirectoryPath))
                {
                    throw new InvalidOperationException($"Dependency checkout directory already exists: {finalDirectoryPath}");
                }

                Directory.Move(temporaryDirectoryPath, finalDirectoryPath);

                ProjectDependencySpec spec = new ProjectDependencySpec
                {
                    Include = include,
                    RepositoryUrl = repositoryUrl,
                    Commit = commit,
                    ModuleName = detection.ModuleName,
                    RootSource = detection.RootSource,
                    CheckoutDir = checkoutDir
                };
                Normalize(spec);
                return spec;
            }
            catch
            {
                if (Directory.Exists(temporaryDirectoryPath))
                {
                    Common.File.DeleteDirectoryRobust(temporaryDirectoryPath);
                }

                throw;
            }
        }

        public ProjectDependencySpec RefreshDependency(string projectDirectoryPath, string gitPath, ProjectDependencySpec spec)
        {
            ProjectDependencySpec updated = spec.Clone();
            Normalize(updated);

            string checkoutDirectoryPath = GetAbsoluteCheckoutPath(projectDirectoryPath, updated);
            gitClient.EnsureCheckout(gitPath, updated.RepositoryUrl, updated.Commit, checkoutDirectoryPath);

            ProjectDependencyDetectionResult detection = autoDetector.Detect(checkoutDirectoryPath, updated.Include);
            if (String.IsNullOrWhiteSpace(updated.ModuleName))
            {
                updated.ModuleName = detection.ModuleName;
            }

            if (String.IsNullOrWhiteSpace(updated.RootSource))
            {
                updated.RootSource = detection.RootSource;
            }

            Normalize(updated);
            return updated;
        }

        public ProjectDependencyStatusInfo GetStatus(string projectDirectoryPath, string gitPath, ProjectDependencySpec spec)
        {
            string checkoutDirectoryPath = GetAbsoluteCheckoutPath(projectDirectoryPath, spec);
            if (!Directory.Exists(checkoutDirectoryPath))
            {
                return new ProjectDependencyStatusInfo { Status = ProjectDependencyStatus.Missing, Description = "Checkout folder is missing." };
            }

            if (!Directory.Exists(Path.Combine(checkoutDirectoryPath, ".git")))
            {
                return new ProjectDependencyStatusInfo { Status = ProjectDependencyStatus.Missing, Description = "Checkout folder is not a git repository." };
            }

            if (String.IsNullOrWhiteSpace(spec.ModuleName) || String.IsNullOrWhiteSpace(spec.RootSource))
            {
                return new ProjectDependencyStatusInfo { Status = ProjectDependencyStatus.Unresolved, Description = "ModuleName or RootSource is not set." };
            }

            string rootSourcePath = Path.Combine(checkoutDirectoryPath, spec.RootSource.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(rootSourcePath))
            {
                return new ProjectDependencyStatusInfo { Status = ProjectDependencyStatus.Unresolved, Description = $"Root source was not found: {spec.RootSource}" };
            }

            string currentCommit = gitClient.GetCurrentCommit(gitPath, checkoutDirectoryPath);
            if (!String.IsNullOrWhiteSpace(spec.Commit) && !String.Equals(currentCommit, spec.Commit, StringComparison.OrdinalIgnoreCase))
            {
                return new ProjectDependencyStatusInfo
                {
                    Status = ProjectDependencyStatus.Unresolved,
                    Description = $"Checkout is at {currentCommit}, expected {spec.Commit}."
                };
            }

            if (gitClient.IsWorkingTreeDirty(gitPath, checkoutDirectoryPath))
            {
                return new ProjectDependencyStatusInfo { Status = ProjectDependencyStatus.Dirty, Description = "Checkout has local modifications." };
            }

            return new ProjectDependencyStatusInfo { Status = ProjectDependencyStatus.Resolved, Description = "Dependency is resolved." };
        }

        public string GetAbsoluteCheckoutPath(string projectDirectoryPath, ProjectDependencySpec spec)
        {
            return Path.Combine(projectDirectoryPath, DependencyAutoDetector.NormalizeRelativePath(spec.CheckoutDir).Replace('/', Path.DirectorySeparatorChar));
        }

        internal ProjectDependencySpec RepairLegacyMetadata(string projectDirectoryPath, ProjectDependencySpec spec)
        {
            ProjectDependencySpec updated = spec.Clone();
            Normalize(updated);

            if (String.IsNullOrWhiteSpace(updated.CheckoutDir) && !String.IsNullOrWhiteSpace(updated.Include))
            {
                updated.CheckoutDir = $"package/{updated.Include}";
            }

            if (!String.IsNullOrWhiteSpace(updated.ModuleName) && !String.IsNullOrWhiteSpace(updated.RootSource))
            {
                Normalize(updated);
                return updated;
            }

            string checkoutDirectoryPath = GetAbsoluteCheckoutPath(projectDirectoryPath, updated);
            if (!Directory.Exists(checkoutDirectoryPath))
            {
                Normalize(updated);
                return updated;
            }

            ProjectDependencyDetectionResult detection = autoDetector.Detect(checkoutDirectoryPath, updated.Include);
            if (String.IsNullOrWhiteSpace(updated.ModuleName))
            {
                updated.ModuleName = detection.ModuleName;
            }

            if (String.IsNullOrWhiteSpace(updated.RootSource))
            {
                updated.RootSource = detection.RootSource;
            }

            Normalize(updated);
            return updated;
        }

        private static string GetRepositoryName(string repositoryUrl)
        {
            string trimmed = repositoryUrl.TrimEnd('/');
            string name = trimmed.Split('/').LastOrDefault() ?? "dependency";
            if (name.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
            {
                name = name.Substring(0, name.Length - 4);
            }

            return DependencyAutoDetector.SanitizeModuleName(name);
        }

        private static string GetUniqueInclude(IEnumerable<ProjectDependencySpec> existingDependencies, string moduleName)
        {
            string include = DependencyAutoDetector.SanitizeModuleName(moduleName);
            HashSet<string> existing = new HashSet<string>(existingDependencies.Select(spec => spec.Include), StringComparer.OrdinalIgnoreCase);
            if (!existing.Contains(include))
            {
                return include;
            }

            int suffix = 2;
            while (existing.Contains(include + "_" + suffix))
            {
                suffix++;
            }

            return include + "_" + suffix;
        }

        private static void Normalize(ProjectDependencySpec spec)
        {
            spec.Include = spec.Include.Trim();
            spec.RepositoryUrl = spec.RepositoryUrl.Trim();
            spec.Commit = spec.Commit.Trim();
            spec.ModuleName = String.IsNullOrWhiteSpace(spec.ModuleName)
                ? String.Empty
                : DependencyAutoDetector.SanitizeModuleName(spec.ModuleName);
            spec.RootSource = DependencyAutoDetector.NormalizeRelativePath(spec.RootSource);
            spec.CheckoutDir = DependencyAutoDetector.NormalizeRelativePath(spec.CheckoutDir);
        }

        private static bool HasDependencyMetadataChanges(ProjectDependencySpec original, ProjectDependencySpec updated)
        {
            return !String.Equals(original.Include, updated.Include, StringComparison.Ordinal) ||
                !String.Equals(original.RepositoryUrl, updated.RepositoryUrl, StringComparison.Ordinal) ||
                !String.Equals(original.Commit, updated.Commit, StringComparison.Ordinal) ||
                !String.Equals(original.ModuleName, updated.ModuleName, StringComparison.Ordinal) ||
                !String.Equals(original.RootSource, updated.RootSource, StringComparison.Ordinal) ||
                !String.Equals(original.CheckoutDir, updated.CheckoutDir, StringComparison.Ordinal);
        }

        private void EnsureCompatibleBuildCli(ZigVSProjectNode projectNode, IReadOnlyList<ProjectDependencySpec> loadedDependencies)
        {
            if (loadedDependencies.Count == 0)
            {
                return;
            }

            DependencyBuildCliStyle? cliStyle = ResolveBuildCliStyle(projectNode);
            if (!cliStyle.HasValue)
            {
                return;
            }

            if (!ApplyBuildCliSyntax(projectNode.BuildProject, cliStyle.Value))
            {
                return;
            }

            projectNode.BuildProject.Save();
        }

        private void EnsureCompatibleBuildCli(ZigVSProjectNode projectNode)
        {
            IReadOnlyList<ProjectDependencySpec> loadedDependencies = repository.Load(projectNode.BuildProject).ToList();
            EnsureCompatibleBuildCli(projectNode, loadedDependencies);
        }

        private static DependencyBuildCliStyle? ResolveBuildCliStyle(ZigVSProjectNode projectNode)
        {
            string rawToolPath = projectNode.GetProjectPropertyUnevaluated("ToolPath", Microsoft.VisualStudio.Shell.Interop._PersistStorageType.PST_PROJECT_FILE);
            if (String.IsNullOrWhiteSpace(rawToolPath))
            {
                rawToolPath = Parameter.c_compilerFileName;
            }

            string expandedToolPath = String.IsNullOrWhiteSpace(rawToolPath)
                ? String.Empty
                : EnvExpander.Expand(rawToolPath);

            ToolchainProbeResult probe = CoreServices.ToolchainProbe.Probe(new ToolchainProbeRequest
            {
                Label = "Project ToolPath",
                RawValue = rawToolPath,
                ExpandedValue = expandedToolPath,
                DefaultFileName = Parameter.c_compilerFileName,
                PrimaryVersionArguments = "version",
                FallbackVersionArguments = "--version"
            });

            if (probe.SemanticVersion == null)
            {
                return null;
            }

            if (probe.SemanticVersion.Major > 0 || probe.SemanticVersion.Minor >= 12)
            {
                return DependencyBuildCliStyle.ModernDepAndModule;
            }

            return DependencyBuildCliStyle.LegacyDepsAndMod;
        }

        private static string GetDesiredBuildCliValue(ProjectPropertyElement property, DependencyBuildCliStyle cliStyle)
        {
            bool hasCondition = !String.IsNullOrWhiteSpace(property.Condition);
            switch (property.Name)
            {
                case "DependenciesOption":
                    if (!hasCondition)
                    {
                        return String.Empty;
                    }

                    return cliStyle == DependencyBuildCliStyle.ModernDepAndModule
                        ? ModernDependenciesOptionValue
                        : LegacyDependenciesOptionValue;

                case "ManagedDependenciesOption":
                    if (!hasCondition)
                    {
                        return String.Empty;
                    }

                    return cliStyle == DependencyBuildCliStyle.ModernDepAndModule
                        ? ModernManagedDependenciesOptionValue
                        : LegacyManagedDependenciesOptionValue;

                case "ModulesOption":
                    if (!hasCondition)
                    {
                        return String.Empty;
                    }

                    return cliStyle == DependencyBuildCliStyle.ModernDepAndModule
                        ? ModernModulesOptionValue
                        : LegacyModulesOptionValue;

                case "ManagedModulesOption":
                    if (!hasCondition)
                    {
                        return String.Empty;
                    }

                    return cliStyle == DependencyBuildCliStyle.ModernDepAndModule
                        ? ModernManagedModulesOptionValue
                        : LegacyManagedModulesOptionValue;

                case "AllDependenciesOption":
                    return AllDependenciesOptionValue;

                case "AllModulesOption":
                    return AllModulesOptionValue;

                case "BuildCommand":
                    return cliStyle == DependencyBuildCliStyle.ModernDepAndModule
                        ? ModernBuildCommandValue
                        : LegacyBuildCommandValue;

                default:
                    return property.Value;
            }
        }

        private static bool EnsureBuildOutputVerificationTasks(ProjectTargetElement target)
        {
            bool changed = false;

            ProjectTaskElement? errorTask = FindBuildOutputTask(
                target,
                "Error",
                BuildOutputErrorTaskLabel,
                BuildOutputMissingConditionValue,
                "Text",
                BuildOutputErrorTextValue);
            if (errorTask == null)
            {
                errorTask = target.AddTask("Error");
                changed = true;
            }

            if (!String.IsNullOrEmpty(errorTask.Label))
            {
                errorTask.Label = string.Empty;
                changed = true;
            }

            if (!String.Equals(errorTask.Condition, BuildOutputMissingConditionValue, StringComparison.Ordinal))
            {
                errorTask.Condition = BuildOutputMissingConditionValue;
                changed = true;
            }

            if (!TryGetTaskParameter(errorTask, "Text", out string? errorText) ||
                !String.Equals(errorText, BuildOutputErrorTextValue, StringComparison.Ordinal))
            {
                errorTask.SetParameter("Text", BuildOutputErrorTextValue);
                changed = true;
            }

            ProjectTaskElement? messageTask = FindBuildOutputTask(
                target,
                "Message",
                BuildOutputMessageTaskLabel,
                BuildOutputExistsConditionValue,
                "Text",
                BuildOutputMessageTextValue);
            if (messageTask == null)
            {
                messageTask = target.AddTask("Message");
                changed = true;
            }

            if (!String.IsNullOrEmpty(messageTask.Label))
            {
                messageTask.Label = string.Empty;
                changed = true;
            }

            if (!String.Equals(messageTask.Condition, BuildOutputExistsConditionValue, StringComparison.Ordinal))
            {
                messageTask.Condition = BuildOutputExistsConditionValue;
                changed = true;
            }

            if (!TryGetTaskParameter(messageTask, "Text", out string? messageText) ||
                !String.Equals(messageText, BuildOutputMessageTextValue, StringComparison.Ordinal))
            {
                messageTask.SetParameter("Text", BuildOutputMessageTextValue);
                changed = true;
            }

            if (!TryGetTaskParameter(messageTask, "Importance", out string? importance) ||
                !String.Equals(importance, "high", StringComparison.Ordinal))
            {
                messageTask.SetParameter("Importance", "high");
                changed = true;
            }

            return changed;
        }

        private static ProjectTaskElement? FindBuildOutputTask(
            ProjectTargetElement target,
            string taskName,
            string legacyLabel,
            string expectedCondition,
            string parameterName,
            string expectedParameterValue)
        {
            foreach (ProjectTaskElement task in target.Tasks)
            {
                if (!String.Equals(task.Name, taskName, StringComparison.Ordinal))
                {
                    continue;
                }

                if (String.Equals(task.Label, legacyLabel, StringComparison.Ordinal))
                {
                    return task;
                }

                if (!String.Equals(task.Condition, expectedCondition, StringComparison.Ordinal))
                {
                    continue;
                }

                if (!TryGetTaskParameter(task, parameterName, out string? parameterValue))
                {
                    continue;
                }

                if (String.Equals(parameterValue, expectedParameterValue, StringComparison.Ordinal))
                {
                    return task;
                }
            }

            return null;
        }

        private static bool TryGetTaskParameter(ProjectTaskElement task, string parameterName, out string? value)
        {
            string parameterValue = task.GetParameter(parameterName);
            if (parameterValue != null)
            {
                value = parameterValue;
                return true;
            }

            value = null;
            return false;
        }
    }
}
