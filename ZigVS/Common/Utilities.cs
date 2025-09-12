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

namespace ZigVS
{
    using Microsoft.VisualStudio.ComponentModelHost;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.Workspace.VSIntegration.Contracts;
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

#nullable enable
    public class Utilities
    {
        public static TInterface GetRequiredService<TService, TInterface>() where TService : class where TInterface : class
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            return (TInterface)ServiceProvider.GlobalProvider.GetService(typeof(TService));
        }

        //public static void SetEnvironmentValue(string i_ZigToolChainPathString)
        //{
        //    //            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

        //    i_ZigToolChainPathString = i_ZigToolChainPathString.TrimEnd('\\') + '\\';

        //    Environment.SetEnvironmentVariable(Parameter.c_ToolPath_EnvironmentVariable_NameString, i_ZigToolChainPathString, EnvironmentVariableTarget.User);
        //    Common.OutputWindowPane.OutputString(
        //        "Environment Value '" +
        //        Parameter.c_ToolPath_EnvironmentVariable_NameString +
        //        "' = " + i_ZigToolChainPathString + Environment.NewLine);
        //}

        public static void SetPATHEnvironmentValue(string i_ZigToolChainPathString ,
            EnvironmentVariableTarget i_EnvironmentVariableTarget)
        {
            //            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            i_ZigToolChainPathString = i_ZigToolChainPathString.TrimEnd('\\') + '\\';

            var l_currentPathString = Environment.GetEnvironmentVariable(
                Parameter.c_PATH_EnvironmentVariable_NameString, i_EnvironmentVariableTarget);
            string l_newPathString = i_ZigToolChainPathString;
            if (string.IsNullOrEmpty(l_currentPathString))
            {
            }
            else if( l_currentPathString.Split(';').Contains(i_ZigToolChainPathString) == false)
            {
                l_newPathString = i_ZigToolChainPathString + ";" + l_currentPathString;
            }
            Environment.SetEnvironmentVariable(
                Parameter.c_PATH_EnvironmentVariable_NameString, l_newPathString, i_EnvironmentVariableTarget);

            Common.OutputWindowPane.OutputString(
                "Updating environment variable '" +
                Parameter.c_PATH_EnvironmentVariable_NameString +
                "' = " + l_newPathString + Environment.NewLine);
        }

        public enum SolutionMode
        {
            ProjectMode = 0,
            OpenFolderMode = 1,
            None = 2
        }

        public static SolutionMode GetSolutionMode()
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            var r_SolutionMode = SolutionMode.None;
            var l_DTE = (EnvDTE.DTE)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(EnvDTE.DTE));

            if (l_DTE != null && !string.IsNullOrEmpty(l_DTE.Solution.FullName))
            {
                if (System.IO.Path.GetExtension(l_DTE.Solution.FullName) == ".sln")
                {
                    r_SolutionMode = SolutionMode.ProjectMode;
                }
                else
                {
                    r_SolutionMode = SolutionMode.OpenFolderMode;
                }
            }

            return r_SolutionMode;
        }

        public static string GetCurrentProjectPath()
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            string r_Path = "";
            var l_DTE = (EnvDTE.DTE)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(EnvDTE.DTE));
            if (l_DTE != null)
            {
                var l_startupProjects = (Array)l_DTE.Solution.SolutionBuild.StartupProjects;
                var l_startUpProjectString = (String)l_startupProjects.GetValue(0);

                foreach (EnvDTE.Project l_Project in l_DTE.Solution.Projects)
                {
                    if (l_Project.Name == System.IO.Path.GetFileNameWithoutExtension(l_startUpProjectString))
                    {
                        r_Path = System.IO.Path.GetDirectoryName(l_Project.FullName);
                        break;
                    }
                }
            }

            return r_Path;
        }

        public static string GetOpenFolderPath()
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            string r_Path = "";

            var l_IVsFolderWorkspaceService = GetRequiredService<SComponentModel, IComponentModel>().GetService<IVsFolderWorkspaceService>();
            if (l_IVsFolderWorkspaceService != null)
            {
                var l_WorkSpace = l_IVsFolderWorkspaceService.CurrentWorkspace;
                if (l_WorkSpace != null)
                {
                    r_Path = l_WorkSpace.Location;
                }
            }

            return r_Path;
        }

        public static void ShowToolWindow(Type i_Type)
        {
            ToolWindowPane? l_ToolWindowPane = ZigVSPackage.GetInstance().FindToolWindow(i_Type, 0, true);
            if ((null != l_ToolWindowPane) && (null != l_ToolWindowPane.Frame))
            {
#pragma warning disable VSTHRD010
                ((IVsWindowFrame)l_ToolWindowPane.Frame).Show();
#pragma warning restore VSTHRD010   
            }
        }

        public static bool IsFullyQualifiedPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            // Check if rooted (starts with drive letter or UNC prefix)
            try {
                if (!Path.IsPathRooted(path))
                {
                    return false;
                }
            }
            catch (ArgumentNullException)
            {
                // if it had invalid characters it's not fully qualified
                return false;
            }

            // Ensure it's not just "C:" but "C:\something"
            string root = Path.GetPathRoot(path);
            if (string.IsNullOrEmpty(root))
            {
                return false;
            }

            // Fully qualified means more than just the root
            return path.Length > root.Length;
        }

        public static string? ResolvePath(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return null;

            // Case 1: Already fully qualified
            if (IsFullyQualifiedPath(fileName))
            {
                return Path.GetFullPath(fileName);
            }

            // Case 2: Contains directory separators → relative to current dir
            if (fileName.Contains(Path.DirectorySeparatorChar) || fileName.Contains(Path.AltDirectorySeparatorChar))
            {
                try
                {
                    string full = Path.GetFullPath(fileName);
                    return full;
                    //return File.Exists(full) ? full : null;
                }
                catch (ArgumentException)
                {
                    return null;
                }
            }

            // Case 3: Search PATH
            string? pathEnv = Environment.GetEnvironmentVariable(Parameter.c_PATH_EnvironmentVariable_NameString);
            if (pathEnv == null)
            {
                return null;
            }

            foreach (string dir in pathEnv.Split(new [] { Path.PathSeparator }, StringSplitOptions.RemoveEmptyEntries))
            {
                string candidate = Path.Combine(dir, fileName);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            return null;
        }
    }

    public static class EnvExpander
    {
        // Matches $(NAME) where NAME = letters, digits, underscore
        private static readonly Regex MsBuildVar = new Regex(@"\$\((?<name>[A-Za-z0-9_]+)\)",
            RegexOptions.Compiled);

        public static string Expand(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return input ?? string.Empty;

            // First, expand $(NAME) ourselves
            string replaced = MsBuildVar.Replace(input!, m =>
            {
                var name = m.Groups["name"].Value;
                var value = Environment.GetEnvironmentVariable(name);
                return value ?? String.Empty; // remove token if not defined
            });

            // Then, also allow %NAME% style via the OS
            replaced = Environment.ExpandEnvironmentVariables(replaced);

            return replaced;
        }
    }
}
