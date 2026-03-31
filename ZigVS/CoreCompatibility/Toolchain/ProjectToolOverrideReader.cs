namespace ZigVS.CoreCompatibility.Toolchain
{
    using Microsoft.VisualStudio.Shell;
    using System;
    using System.IO;
    using System.Xml.Linq;

    internal static class ProjectToolOverrideReader
    {
        internal static ProjectToolOverride? TryReadStartupProjectOverride()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (Utilities.GetSolutionMode() != Utilities.SolutionMode.ProjectMode)
            {
                return null;
            }

            string projectDirectoryPath = Utilities.GetCurrentProjectPath();
            if (string.IsNullOrWhiteSpace(projectDirectoryPath) || !Directory.Exists(projectDirectoryPath))
            {
                return null;
            }

            return TryReadFromDirectory(projectDirectoryPath, true);
        }

        internal static ProjectToolOverride? TryReadFromSourcePath(string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                return null;
            }

            string? directoryPath = Directory.Exists(sourcePath) ? sourcePath : Path.GetDirectoryName(sourcePath);
            while (true)
            {
                if (string.IsNullOrWhiteSpace(directoryPath))
                {
                    return null;
                }

                string currentDirectoryPath = directoryPath;
                ProjectToolOverride? toolOverride = TryReadFromDirectory(currentDirectoryPath, false);
                if (toolOverride != null)
                {
                    return toolOverride;
                }

                directoryPath = GetParentDirectory(currentDirectoryPath);
            }
        }

        internal static ProjectToolOverride? TryReadProjectToolOverride(string projectFilePath, bool includeEmptyOverride = false)
        {
            if (string.IsNullOrWhiteSpace(projectFilePath) || !File.Exists(projectFilePath))
            {
                return null;
            }

            try
            {
                XDocument document = XDocument.Load(projectFilePath);
                foreach (XElement element in document.Descendants())
                {
                    if (string.Equals(element.Name.LocalName, "ToolPath", StringComparison.OrdinalIgnoreCase))
                    {
                        string rawValue = (element.Value ?? string.Empty).Trim();
                        if (!string.IsNullOrWhiteSpace(rawValue))
                        {
                            return new ProjectToolOverride
                            {
                                ProjectFilePath = projectFilePath,
                                RawValue = rawValue,
                                ExpandedValue = EnvExpander.Expand(rawValue)
                            };
                        }
                    }
                }

                if (includeEmptyOverride)
                {
                    return new ProjectToolOverride
                    {
                        ProjectFilePath = projectFilePath,
                        RawValue = string.Empty,
                        ExpandedValue = string.Empty
                    };
                }
            }
            catch
            {
            }

            return null;
        }

        static ProjectToolOverride? TryReadFromDirectory(string directoryPath, bool includeEmptyOverride)
        {
            if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
            {
                return null;
            }

            string[] projectFiles = Directory.GetFiles(directoryPath, "*.zigproj", SearchOption.TopDirectoryOnly);
            Array.Sort(projectFiles, StringComparer.OrdinalIgnoreCase);

            foreach (string projectFilePath in projectFiles)
            {
                ProjectToolOverride? toolOverride = TryReadProjectToolOverride(projectFilePath, includeEmptyOverride);
                if (toolOverride != null)
                {
                    return toolOverride;
                }
            }

            return null;
        }

        static string? GetParentDirectory(string directoryPath)
        {
            string trimmedDirectoryPath = directoryPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (string.IsNullOrWhiteSpace(trimmedDirectoryPath))
            {
                return null;
            }

            string? parentDirectoryPath = Path.GetDirectoryName(trimmedDirectoryPath);
            if (string.Equals(parentDirectoryPath, directoryPath, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return parentDirectoryPath;
        }
    }
}
