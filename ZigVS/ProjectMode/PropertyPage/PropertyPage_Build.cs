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
#nullable enable
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Project;
    using Microsoft.VisualStudio.Project.Automation;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using System;
    using System.Runtime.InteropServices;
    using EnvDTE;
    using EnvDTE80;

    [ComVisible(true)]
    [Guid("A1090467-4E21-4684-A9F2-C4D4248E7D54")]
    public class PropertyPage_Build : SettingsPage
    {
        private string? assemblyName = "";
        private string assemblyVersion = "";
        private string outputFile = "";

        private OSType osType = OSType.Windows;
        private OutputType configurationType = OutputType.Application;

        private string buildOption = "";

        private string useBuildDotZig = "";
        private string rootSourceName = "";

        private string includeDirs = "";
        private string libraryDirs = "";
        private string libraries = "";

        private string[] modules = { "" };
        private string[] dependencies = { "" };

        private string intDirName = "";
        private string outDirName = "";

        private string generateBuildDotZig = "";
        private string generateBuildDotZigDotZon = "";


        static ProjectNode GetCurrentProject()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            DTE2? dTE2 = AsyncPackage.GetGlobalService(typeof(SDTE)) as DTE2;
            UIHierarchy uih = dTE2!.ToolWindows.SolutionExplorer;
            Array selectedItems = (Array)uih.SelectedItems;

            UIHierarchyItem? firstItem = selectedItems.GetValue(0) as UIHierarchyItem;
            OAProject? pi = firstItem!.Object as OAProject;

            return pi!.Project;
        }

        public PropertyPage_Build() : base(GetCurrentProject())
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            this.Name = ZigVS.Resources.Resource.BuildCaption;
            this.assemblyName = (ProjectManager as ZigVSProjectNode)!.GetName();
        }

        public PropertyPage_Build(ProjectNode projectManager)
            : base(projectManager)
        {
            this.Name = ZigVS.Resources.Resource.BuildCaption;
        }

        [ResourcesCategoryAttribute(PropertyPageUIText.Category_Directory)]
        [LocDisplayName(PropertyPageUIText.IntDirName)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.IntDirNameCaption)]
        public string IntDirName
        {
            get { return this.intDirName!; }
            set { this.intDirName = value; this.IsDirty = true; }
        }

        [ResourcesCategoryAttribute(PropertyPageUIText.Category_Directory)]
        [LocDisplayName(PropertyPageUIText.OutDirName)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.OutDirNameCaption)]
        public string OutDirName
        {
            get { return this.outDirName!; }
            set { this.outDirName = value; this.IsDirty = true; }
        }

        [ResourcesCategoryAttribute(PropertyPageUIText.Category_Build)]
        [LocDisplayName(PropertyPageUIText.BuildOption)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.BuildOption)]
        public string BuildOption
        {
            get { return this.buildOption!; }
            set { this.buildOption = value; this.IsDirty = true; }
        }

        [ResourcesCategoryAttribute(PropertyPageUIText.Category_Build)]
        [LocDisplayName(PropertyPageUIText.UseBuildDotZig)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.UseBuildDotZigDescription)]
        public string UseBuildDotZig
        {
            get { return this.useBuildDotZig!; }
            set { this.useBuildDotZig = value; this.IsDirty = true; }
        }

        [ResourcesCategoryAttribute(PropertyPageUIText.Category_Build)]
        [LocDisplayName(PropertyPageUIText.IncludeDirs)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.IncludeDirsDescription)]
        public string IncludeDirs
        {
            get { return this.includeDirs!; }
            set { this.includeDirs = value; this.IsDirty = true; }
        }

        [ResourcesCategoryAttribute(PropertyPageUIText.Category_Build)]
        [LocDisplayName(PropertyPageUIText.LibraryDirs)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.LibraryDirsDescription)]
        public string LibraryDirs
        {
            get { return this.libraryDirs!; }
            set { this.libraryDirs = value; this.IsDirty = true; }
        }

        [ResourcesCategoryAttribute(PropertyPageUIText.Category_Build)]
        [LocDisplayName(PropertyPageUIText.Libraries)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.LibrariesDescription)]
        public string Libraries
        {
            get { return this.libraries!; }
            set { this.libraries = value; this.IsDirty = true; }
        }

        [ResourcesCategoryAttribute(PropertyPageUIText.Category_Build)]
        [LocDisplayName(PropertyPageUIText.RootSourceName)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.RootSourceNameDescription)]
        public string RootSourceName
        {
            get { return this.rootSourceName!; }
            set { this.rootSourceName = value; this.IsDirty = true; }
        }

        [ResourcesCategoryAttribute(PropertyPageUIText.Category_Assembly)]
        [LocDisplayName(PropertyPageUIText.OSType)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.OSTypeDescription)]
        public OSType OSType
        {
            get { return this.osType; }
            set { this.osType = value; this.IsDirty = true; }
        }

        [ResourcesCategoryAttribute(PropertyPageUIText.Category_Assembly)]
        [LocDisplayName(PropertyPageUIText.AssemblyName)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.AssemblyNameDescription)]
        public string AssemblyName
        {
            get { return this.assemblyName!; }
            set { this.assemblyName = value; this.IsDirty = true; }
        }

        [ResourcesCategoryAttribute(PropertyPageUIText.Category_Assembly)]
        [LocDisplayName(PropertyPageUIText.AssemblyVersion)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.AssemblyVersionDescription)]
        public string AssemblyVersion
        {
            get { return this.assemblyVersion!; }
            set { this.assemblyVersion = value; this.IsDirty = true; }
        }

        [ResourcesCategoryAttribute(PropertyPageUIText.Category_Assembly)]
        [LocDisplayName(PropertyPageUIText.ConfigurationType)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.ConfigurationTypeDescription)]
        public OutputType ConfigurationType
        {
            get { return this.configurationType; }
            set { this.configurationType = value; this.IsDirty = true; }
        }

        /// <summary>
        /// Gets the output file name depending on current OutputType.
        /// </summary>
        /// <remarks>IsDirty flag was switched to true.</remarks>
        [ResourcesCategoryAttribute(PropertyPageUIText.Category_Assembly)]
        [LocDisplayName(PropertyPageUIText.OutputFile)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.OutputFileDescription)]
        public string OutputFile
        {
            get { return this.outputFile; }
        }

        [ResourcesCategoryAttribute(PropertyPageUIText.Category_Dependency)]
        [LocDisplayName(PropertyPageUIText.Modules)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.ModulesDescription)]
        public string[] Modules
        {
            get { return this.modules!; }
            set { this.modules = value; this.IsDirty = true; }
        }

        [ResourcesCategoryAttribute(PropertyPageUIText.Category_Dependency)]
        [LocDisplayName(PropertyPageUIText.Dependencies)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.DependenciesDescription)]
        public string[] Dependencies
        {
            get { return this.dependencies!; }
            set { this.dependencies = value; this.IsDirty = true; }
        }


        [ResourcesCategoryAttribute(PropertyPageUIText.Category_Generation)]
        [LocDisplayName(PropertyPageUIText.GenerateBuildDotZig)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.GenerateBuildDotZigDescription)]
        public string GenerateBuildDotZig
        {
            get { return this.generateBuildDotZig!; }
            set { this.generateBuildDotZig = value; this.IsDirty = true; }
        }

        [ResourcesCategoryAttribute(PropertyPageUIText.Category_Generation)]
        [LocDisplayName(PropertyPageUIText.GenerateBuildDotZigDotZon)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.GenerateBuildDotZigDotZonDescription)]
        public string GenerateBuildDotZigDotZon
        {
            get { return this.generateBuildDotZigDotZon!; }
            set { this.generateBuildDotZigDotZon = value; this.IsDirty = true; }
        }

        /// <summary>
        /// Returns class FullName property value.
        /// </summary>
        public override string GetClassName()
        {
            return this.GetType().FullName;
        }

        /// <summary>
        /// Bind properties.
        /// </summary>
        protected override void BindProperties()
        {
            if (this.ProjectManager == null)
            {
                return;
            }

            this.assemblyName = this.ProjectManager.GetProjectProperty("AssemblyName", _PersistStorageType.PST_PROJECT_FILE, false);
            this.assemblyVersion = this.ProjectManager.GetProjectProperty("AssemblyVersion", _PersistStorageType.PST_PROJECT_FILE, false);
            string l_osType = this.ProjectManager.GetProjectProperty("OSType", _PersistStorageType.PST_PROJECT_FILE, false);
            string l_configurationType = this.ProjectManager.GetProjectProperty("ConfigurationType", _PersistStorageType.PST_PROJECT_FILE, false);
            this.outputFile = this.ProjectManager.GetProjectProperty("OutputName", _PersistStorageType.PST_PROJECT_FILE, true);

            this.buildOption = this.ProjectManager.GetProjectProperty("BuildOption", _PersistStorageType.PST_PROJECT_FILE, false);
            this.useBuildDotZig = this.ProjectManager.GetProjectProperty("UseBuildDotZig", _PersistStorageType.PST_PROJECT_FILE, false);
            this.rootSourceName = this.ProjectManager.GetProjectProperty("RootSourceName", _PersistStorageType.PST_PROJECT_FILE, false);
            this.includeDirs = this.ProjectManager.GetProjectProperty("IncludeDirs", _PersistStorageType.PST_PROJECT_FILE, false);
            this.libraryDirs = this.ProjectManager.GetProjectProperty("LibraryDirs", _PersistStorageType.PST_PROJECT_FILE, false);
            this.libraries = this.ProjectManager.GetProjectProperty("Libraries", _PersistStorageType.PST_PROJECT_FILE, false);
            var l_tempDependenciesArray = this.ProjectManager.GetProjectProperty("Dependencies", _PersistStorageType.PST_PROJECT_FILE, false);
            this.dependencies = l_tempDependenciesArray.Split(';');
            var l_tempModluesArray = this.ProjectManager.GetProjectProperty("Modules", _PersistStorageType.PST_PROJECT_FILE, false);
            this.modules = l_tempModluesArray.Split(';');
            this.intDirName = this.ProjectManager.GetProjectProperty("IntDirName", _PersistStorageType.PST_PROJECT_FILE, false);
            this.outDirName = this.ProjectManager.GetProjectProperty("outDirName", _PersistStorageType.PST_PROJECT_FILE, false);

            this.generateBuildDotZig = this.ProjectManager.GetProjectProperty("GenerateBuildDotZig", _PersistStorageType.PST_PROJECT_FILE, false);
            this.generateBuildDotZigDotZon = this.ProjectManager.GetProjectProperty("GenerateBuildDotZigDotZon", _PersistStorageType.PST_PROJECT_FILE, false);

            //       this.defaultNamespace = this.ProjectManager.GetProjectProperty("RootNamespace", _PersistStorageType.PST_PROJECT_FILE, false);

            //         this.rootSourceName = this.ProjectManager.GetProjectProperty("BuildCommand_pre", _PersistStorageType.PST_PROJECT_FILE, false);

            if (l_osType != null && l_osType.Length > 0)
            {
                try
                {
                    this.OSType = (OSType)Enum.Parse(typeof(OSType), l_osType);
                }
                catch (ArgumentException)
                {
                }
            }

            if (l_configurationType != null && l_configurationType.Length > 0)
            {
                try
                {
                    this.configurationType = (OutputType)Enum.Parse(typeof(OutputType), l_configurationType);
                }
                catch (ArgumentException)
                {
                }
            }

        }

        protected override int ApplyChanges()
        {
            if (this.ProjectManager == null)
            {
                return VSConstants.E_INVALIDARG;
            }

            ThreadHelper.ThrowIfNotOnUIThread();
            IVsPropertyPageFrame? l_IVsPropertyPageFrame = (IVsPropertyPageFrame)this.ProjectManager.Site.GetService((typeof(SVsPropertyPageFrame)));
            //		bool reloadRequired = this.ProjectManager.TargetFrameworkMoniker != this.targetFrameworkMoniker;

            this.ProjectManager.SetProjectProperty("BuildOption", _PersistStorageType.PST_PROJECT_FILE, this.buildOption);
            this.ProjectManager.SetProjectProperty("UseBuildDotZig", _PersistStorageType.PST_PROJECT_FILE, this.useBuildDotZig);
            this.ProjectManager.SetProjectProperty("RootSourceName", _PersistStorageType.PST_PROJECT_FILE, this.rootSourceName);

            this.ProjectManager.SetProjectProperty("IncludeDirs", _PersistStorageType.PST_PROJECT_FILE, this.includeDirs.TrimEnd(';'));
            this.ProjectManager.SetProjectProperty("LibraryDirs", _PersistStorageType.PST_PROJECT_FILE, this.libraryDirs.TrimEnd(';'));
            this.ProjectManager.SetProjectProperty("Libraries", _PersistStorageType.PST_PROJECT_FILE, this.libraries.TrimEnd(';'));
            var l_tmpDependenciesString = string.Join(";", this.dependencies);
            this.ProjectManager.SetProjectProperty("Dependencies", _PersistStorageType.PST_PROJECT_FILE, l_tmpDependenciesString);
            var l_tmpModulesString = string.Join(";", this.modules);
            this.ProjectManager.SetProjectProperty("Modules", _PersistStorageType.PST_PROJECT_FILE, l_tmpModulesString);

            this.ProjectManager.SetProjectProperty("IntDirName", _PersistStorageType.PST_PROJECT_FILE, this.intDirName);
            this.ProjectManager.SetProjectProperty("OutDirName", _PersistStorageType.PST_PROJECT_FILE, this.outDirName);


            this.ProjectManager.SetProjectProperty("AssemblyName", _PersistStorageType.PST_PROJECT_FILE, this.assemblyName);
            this.ProjectManager.SetProjectProperty("AssemblyVersion", _PersistStorageType.PST_PROJECT_FILE, this.assemblyVersion);
            this.ProjectManager.SetProjectProperty("OSType", _PersistStorageType.PST_PROJECT_FILE, this.osType.ToString());
            this.ProjectManager.SetProjectProperty("ConfigurationType", _PersistStorageType.PST_PROJECT_FILE, this.configurationType.ToString());
            //       this.ProjectManager.SetProjectProperty("RootNamespace", _PersistStorageType.PST_PROJECT_FILE, this.defaultNamespace);

            this.ProjectManager.SetProjectProperty("GenerateBuildDotZig", _PersistStorageType.PST_PROJECT_FILE, this.generateBuildDotZig);
            this.ProjectManager.SetProjectProperty("GenerateBuildDotZigDotZon", _PersistStorageType.PST_PROJECT_FILE, this.generateBuildDotZigDotZon);

            this.IsDirty = false;

            	if (true)
                {
                    // This prevents the property page from displaying bad data from the zombied (unloaded) project
                    l_IVsPropertyPageFrame?.HideFrame();
                    l_IVsPropertyPageFrame?.ShowFrame(this.GetType().GUID);
                }

            return VSConstants.S_OK;
        }
    }
}