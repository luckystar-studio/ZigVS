namespace Microsoft.VisualStudio.Project
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using OLECMDEXECOPT = Microsoft.VisualStudio.OLE.Interop.OLECMDEXECOPT;
    using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;
    using VsCommands = Microsoft.VisualStudio.VSConstants.VSStd97CmdID;
    using VsCommands2K = Microsoft.VisualStudio.VSConstants.VSStd2KCmdID;
    using vsCommandStatus = EnvDTE.vsCommandStatus;

    internal static class CommonUtils
    {

        private static readonly char[] DirectorySeparators = new[] {
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar
        };

        public static bool HasEndSeparator(string path)
        {
            return !string.IsNullOrEmpty(path) && DirectorySeparators.Contains(path[path.Length - 1]);
        }

        public static string TrimEndSeparator(string path)
        {
            if (HasEndSeparator(path))
            {
                if (path.Length > 2 && path[path.Length - 2] == ':')
                {
                    // The slash at the end of a drive specifier is not actually
                    // a separator.
                    return path;
                }
                else if (path.Length > 3 && path[path.Length - 2] == path[path.Length - 1] && path[path.Length - 3] == ':')
                {
                    // The double slash at the end of a schema is not actually a
                    // separator.
                    return path;
                }
                return path.Remove(path.Length - 1);
            }
            else
            {
                return path;
            }
        }
        public static string GetRelativeFilePath(string fromDirectory, string toFile)
        {
            var dirFullPath = Path.GetFullPath(TrimEndSeparator(fromDirectory));
            var fileFullPath = Path.GetFullPath(toFile);

            // If the root paths doesn't match return the file full path.
            if (!string.Equals(Path.GetPathRoot(dirFullPath), Path.GetPathRoot(fileFullPath), StringComparison.OrdinalIgnoreCase))
            {
                return fileFullPath;
            }

            var splitDirectory = dirFullPath.Split(DirectorySeparators);
            var splitFile = fileFullPath.Split(DirectorySeparators);

            var relativePath = new List<string>();
            var dirIndex = 0;

            var minLegth = Math.Min(splitDirectory.Length, splitFile.Length);

            while (dirIndex < minLegth
                && string.Equals(splitDirectory[dirIndex], splitFile[dirIndex], StringComparison.OrdinalIgnoreCase))
            {
                dirIndex++;
            }

            for (var i = splitDirectory.Length; i > dirIndex; i--)
            {
                relativePath.Add("..");
            }

            for (var i = dirIndex; i < splitFile.Length; i++)
            {
                relativePath.Add(splitFile[i]);
            }

            return string.Join(Path.DirectorySeparatorChar.ToString(), relativePath);
        }

        public static string EnsureEndSeparator(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }
            else if (!HasEndSeparator(path))
            {
                return path + Path.DirectorySeparatorChar;
            }
            else
            {
                return path;
            }
        }

        public static string GetRelativeDirectoryPath(string fromDirectory, string toDirectory)
        {
            var relativePath = GetRelativeFilePath(fromDirectory, toDirectory);
            return EnsureEndSeparator(relativePath);
        }

        public static string GetAbsoluteDirectoryPath(string root, string relativePath)
        {
            var absolutePath = GetAbsoluteFilePath(root, relativePath);
            return EnsureEndSeparator(absolutePath);
        }

        public static bool HasStartSeparator(string path)
        {
            return !string.IsNullOrEmpty(path) && DirectorySeparators.Contains(path[0]);
        }

        /// <summary>
        /// Returns a normalized file path created by joining relativePath to root.
        /// The result is not guaranteed to end with a backslash.
        /// </summary>
        /// <exception cref="ArgumentException">root is not an absolute path, or
        /// either path is invalid.</exception>
        public static string GetAbsoluteFilePath(string root, string relativePath)
        {
            var absolutePath = HasStartSeparator(relativePath)
                ? Path.GetFullPath(relativePath)
                : Path.Combine(root, relativePath);

            var split = absolutePath.Split(DirectorySeparators);
            var segments = new LinkedList<string>();

            for (var i = split.Length - 1; i >= 0; i--)
            {
                var segment = split[i];

                if (segment == "..")
                {
                    i--;
                }
                else if (segment != ".")
                {
                    segments.AddFirst(segment);
                }
            }

            // Cannot use Path.Combine because the result will not return an absolute path.
            // This happens because the root has missing the trailing slash.
            return string.Join(Path.DirectorySeparatorChar.ToString(), segments);
        }
    }
}