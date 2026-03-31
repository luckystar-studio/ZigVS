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
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.ComponentModelHost;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.Workspace.VSIntegration.Contracts;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;

#nullable enable
    public class Utilities
    {
        private static ZigVSProjectNode? s_LastActiveZigVSProjectNode;

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
                string l_solutionExtensionString = System.IO.Path.GetExtension(l_DTE.Solution.FullName);
                if (string.Equals(l_solutionExtensionString, ".sln", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(l_solutionExtensionString, ".slnf", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(l_solutionExtensionString, ".slnx", StringComparison.OrdinalIgnoreCase))
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
            if (l_DTE != null && l_DTE.Solution != null && l_DTE.Solution.SolutionBuild != null)
            {
                if (!(l_DTE.Solution.SolutionBuild.StartupProjects is Array l_startupProjects) || l_startupProjects.Length == 0)
                {
                    return r_Path;
                }

                var l_startUpProjectString = l_startupProjects.GetValue(0) as string;
                if (string.IsNullOrWhiteSpace(l_startUpProjectString))
                {
                    return r_Path;
                }

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

        public static Microsoft.VisualStudio.Project.HierarchyNode? GetSelectedHierarchyNode()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            IVsMonitorSelection monitorSelection = GetRequiredService<SVsShellMonitorSelection, IVsMonitorSelection>();
            IntPtr hierarchyPointer = IntPtr.Zero;
            IntPtr selectionContainerPointer = IntPtr.Zero;

            try
            {
                uint itemId;
                IVsMultiItemSelect? multiSelect;
                monitorSelection.GetCurrentSelection(out hierarchyPointer, out itemId, out multiSelect, out selectionContainerPointer);

                if (hierarchyPointer == IntPtr.Zero || itemId == VSConstants.VSITEMID_NIL || multiSelect != null)
                {
                    return null;
                }

                IVsHierarchy? hierarchy = Marshal.GetObjectForIUnknown(hierarchyPointer) as IVsHierarchy;
                if (hierarchy is Microsoft.VisualStudio.Project.ProjectNode projectNode)
                {
                    return projectNode.NodeFromItemId(itemId);
                }

                ZigVSProjectNode? l_zigProjectNode = TryGetZigVSProjectNode(hierarchy);
                if (l_zigProjectNode != null)
                {
                    return l_zigProjectNode.NodeFromItemId(itemId);
                }

                return null;
            }
            finally
            {
                if (hierarchyPointer != IntPtr.Zero)
                {
                    Marshal.Release(hierarchyPointer);
                }

                if (selectionContainerPointer != IntPtr.Zero)
                {
                    Marshal.Release(selectionContainerPointer);
                }
            }
        }

        public static ZigVSProjectNode? GetSelectedZigVSProjectNode()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var hierarchyNode = GetSelectedHierarchyNode();
            if (hierarchyNode is ZigVSProjectNode projectNode)
            {
                RememberActiveZigVSProjectNode(projectNode);
                return projectNode;
            }

            ZigVSProjectNode? l_parentProjectNode = hierarchyNode?.ProjectManager as ZigVSProjectNode;
            if (l_parentProjectNode != null)
            {
                RememberActiveZigVSProjectNode(l_parentProjectNode);
                return l_parentProjectNode;
            }

            ZigVSProjectNode? l_solutionExplorerProjectNode = TryGetSelectedZigVSProjectNodeFromSolutionExplorer();
            if (l_solutionExplorerProjectNode != null)
            {
                RememberActiveZigVSProjectNode(l_solutionExplorerProjectNode);
                return l_solutionExplorerProjectNode;
            }

            return null;
        }

        public static ZigVSProjectNode? GetActiveZigVSProjectNode()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ZigVSProjectNode? l_selectedProjectNode = GetSelectedZigVSProjectNode();
            if (l_selectedProjectNode != null)
            {
                return l_selectedProjectNode;
            }

            ZigVSProjectNode? l_startupProjectNode = GetStartupZigVSProjectNode();
            if (l_startupProjectNode != null)
            {
                RememberActiveZigVSProjectNode(l_startupProjectNode);
                return l_startupProjectNode;
            }

            if (s_LastActiveZigVSProjectNode != null && !s_LastActiveZigVSProjectNode.IsClosed)
            {
                return s_LastActiveZigVSProjectNode;
            }

            ZigVSProjectNode? l_singleLoadedProjectNode = GetSingleLoadedZigVSProjectNode();
            if (l_singleLoadedProjectNode != null)
            {
                RememberActiveZigVSProjectNode(l_singleLoadedProjectNode);
                return l_singleLoadedProjectNode;
            }

            return null;
        }

        public static ZigVSProjectNode? GetCachedActiveZigVSProjectNode()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (s_LastActiveZigVSProjectNode != null && !s_LastActiveZigVSProjectNode.IsClosed)
            {
                return s_LastActiveZigVSProjectNode;
            }

            ZigVSProjectNode? l_startupProjectNode = GetStartupZigVSProjectNode();
            if (l_startupProjectNode != null)
            {
                RememberActiveZigVSProjectNode(l_startupProjectNode);
                return l_startupProjectNode;
            }

            ZigVSProjectNode? l_singleLoadedProjectNode = GetSingleLoadedZigVSProjectNode();
            if (l_singleLoadedProjectNode != null)
            {
                RememberActiveZigVSProjectNode(l_singleLoadedProjectNode);
                return l_singleLoadedProjectNode;
            }

            return null;
        }

        private static ZigVSProjectNode? GetStartupZigVSProjectNode()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                var l_DTE = (EnvDTE.DTE)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(EnvDTE.DTE));
                if (l_DTE == null || l_DTE.Solution == null || l_DTE.Solution.SolutionBuild == null)
                {
                    return null;
                }

                if (!(l_DTE.Solution.SolutionBuild.StartupProjects is Array l_startupProjects) || l_startupProjects.Length == 0)
                {
                    return null;
                }

                string? l_startupProjectUniqueNameString = l_startupProjects.GetValue(0) as string;
                IVsSolution l_solution = GetRequiredService<SVsSolution, IVsSolution>();
                if (!string.IsNullOrWhiteSpace(l_startupProjectUniqueNameString) &&
                    ErrorHandler.Succeeded(l_solution.GetProjectOfUniqueName(l_startupProjectUniqueNameString, out IVsHierarchy l_startupHierarchy)) &&
                    TryGetZigVSProjectNode(l_startupHierarchy) is ZigVSProjectNode l_uniqueNameProjectNode)
                {
                    return l_uniqueNameProjectNode;
                }

                string l_currentProjectPathString = GetCurrentProjectPath();
                if (string.IsNullOrWhiteSpace(l_currentProjectPathString))
                {
                    return null;
                }

                if (ErrorHandler.Succeeded(l_solution.GetProjectEnum((uint)__VSENUMPROJFLAGS.EPF_ALLINSOLUTION, Guid.Empty, out IEnumHierarchies l_projectsIEnumHierarchies)))
                {
                    IVsHierarchy[] l_hierarchyArray = new IVsHierarchy[1];
                    uint l_fetchedUint = 0;

                    for (l_projectsIEnumHierarchies.Reset();
                        l_projectsIEnumHierarchies.Next(1, l_hierarchyArray, out l_fetchedUint) == VSConstants.S_OK && l_fetchedUint == 1;
                        /* nothing */)
                    {
                        if (TryGetZigVSProjectNode(l_hierarchyArray[0]) is ZigVSProjectNode l_projectNode &&
                            string.Equals(
                                Path.GetFullPath(l_projectNode.ProjectFolder).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                                Path.GetFullPath(l_currentProjectPathString).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                                StringComparison.OrdinalIgnoreCase))
                        {
                            return l_projectNode;
                        }
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        private static ZigVSProjectNode? GetSingleLoadedZigVSProjectNode()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            List<ZigVSProjectNode> l_loadedProjectNodes = GetLoadedZigVSProjectNodes()
                .Where(projectNode => !projectNode.IsClosed)
                .GroupBy(projectNode => Path.GetFullPath(projectNode.ProjectFolder).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();

            return l_loadedProjectNodes.Count == 1 ? l_loadedProjectNodes[0] : null;
        }

        private static IEnumerable<ZigVSProjectNode> GetLoadedZigVSProjectNodes()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            IVsSolution l_solution = GetRequiredService<SVsSolution, IVsSolution>();
            if (ErrorHandler.Failed(l_solution.GetProjectEnum((uint)__VSENUMPROJFLAGS.EPF_ALLINSOLUTION, Guid.Empty, out IEnumHierarchies l_projectsIEnumHierarchies)))
            {
                yield break;
            }

            IVsHierarchy[] l_hierarchyArray = new IVsHierarchy[1];
            uint l_fetchedUint = 0;
            for (l_projectsIEnumHierarchies.Reset();
                l_projectsIEnumHierarchies.Next(1, l_hierarchyArray, out l_fetchedUint) == VSConstants.S_OK && l_fetchedUint == 1;
                /* nothing */)
            {
                ZigVSProjectNode? l_projectNode = TryGetZigVSProjectNode(l_hierarchyArray[0]);
                if (l_projectNode != null)
                {
                    yield return l_projectNode;
                }
            }
        }

        private static ZigVSProjectNode? TryGetZigVSProjectNode(IVsHierarchy? i_hierarchy)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (i_hierarchy is ZigVSProjectNode l_projectNode)
            {
                return l_projectNode;
            }

            if (i_hierarchy is Microsoft.VisualStudio.Project.HierarchyNode l_hierarchyNode)
            {
                if (l_hierarchyNode is ZigVSProjectNode l_directProjectNode)
                {
                    return l_directProjectNode;
                }

                return l_hierarchyNode.ProjectManager as ZigVSProjectNode;
            }

            if (i_hierarchy != null &&
                TryGetZigVSProjectNodeFromExtObject(i_hierarchy) is ZigVSProjectNode l_extObjectProjectNode)
            {
                return l_extObjectProjectNode;
            }

            return null;
        }

        private static ZigVSProjectNode? TryGetSelectedZigVSProjectNodeFromSolutionExplorer()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                EnvDTE80.DTE2? l_dte2 = AsyncPackage.GetGlobalService(typeof(SDTE)) as EnvDTE80.DTE2;
                if (l_dte2 == null)
                {
                    return null;
                }

                if (!(l_dte2.ToolWindows.SolutionExplorer.SelectedItems is Array l_selectedItems) || l_selectedItems.Length == 0)
                {
                    return null;
                }

                EnvDTE.UIHierarchyItem? l_firstItem = l_selectedItems.GetValue(0) as EnvDTE.UIHierarchyItem;
                if (l_firstItem?.Object is Microsoft.VisualStudio.Project.Automation.OAProject l_oaProject)
                {
                    return l_oaProject.Project as ZigVSProjectNode;
                }

                if (l_firstItem?.Object is EnvDTE.ProjectItem l_projectItem &&
                    l_projectItem.ContainingProject is Microsoft.VisualStudio.Project.Automation.OAProject l_containingProject)
                {
                    return l_containingProject.Project as ZigVSProjectNode;
                }
            }
            catch
            {
            }

            return null;
        }

        private static ZigVSProjectNode? TryGetZigVSProjectNodeFromExtObject(IVsHierarchy i_hierarchy)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                if (ErrorHandler.Succeeded(i_hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out object l_extObject)) &&
                    l_extObject is Microsoft.VisualStudio.Project.Automation.OAProject l_oaProject)
                {
                    return l_oaProject.Project as ZigVSProjectNode;
                }
            }
            catch (InvalidCastException)
            {
            }
            catch (COMException)
            {
            }

            return null;
        }

        private static void RememberActiveZigVSProjectNode(ZigVSProjectNode? i_projectNode)
        {
            if (i_projectNode != null && !i_projectNode.IsClosed)
            {
                s_LastActiveZigVSProjectNode = i_projectNode;
            }
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
