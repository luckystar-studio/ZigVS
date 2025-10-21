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

        private bool useBuildDotZig = false;
        private string rootSourceName = "";

        private string includeDirs = "";
        private string libraryDirs = "";
        private string libraries = "";

        private string[] modules = { "" };
        private string[] dependencies = { "" };

        private string intDirName = "";
        private string outDirName = "";

        private bool generateBuildDotZig = false;
        private bool generateBuildDotZigDotZon = false;


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

        [ResourcesCategoryAttribute(PropertyPageUIText.Category_Directory)]
        [LocDisplayName(PropertyPageUIText.IntDirName)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.IntDirNameCaption)]
        public string IntDirName
        {
            get { return this.intDirName!; }
            set { if (String.Compare(this.intDirName, value) != 0) this.IsDirty = true; this.intDirName = value; }
        }

        [ResourcesCategoryAttribute(PropertyPageUIText.Category_Directory)]
        [LocDisplayName(PropertyPageUIText.OutDirName)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.OutDirNameCaption)]
        public string OutDirName
        {
            get { return this.outDirName!; }
            set { if (String.Compare(this.outDirName, value) != 0) this.IsDirty = true; this.outDirName = value; }
        }

        [ResourcesCategoryAttribute(PropertyPageUIText.Category_Build)]
        [LocDisplayName(PropertyPageUIText.BuildOption)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.BuildOption)]
        public string BuildOption
        {
            get { return this.buildOption!; }
            set { if (String.Compare(this.buildOption, value) != 0) this.IsDirty = true; this.buildOption = value; }
        }

        [ResourcesCategoryAttribute(PropertyPageUIText.Category_Build)]
        [LocDisplayName(PropertyPageUIText.UseBuildDotZig)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.UseBuildDotZigDescription)]
        public bool UseBuildDotZig
        {
            get { return this.useBuildDotZig!; }
            set { if(this.useBuildDotZig != value) this.IsDirty = true; this.useBuildDotZig = value; }
        }

        [ResourcesCategoryAttribute(PropertyPageUIText.Category_Build)]
        [LocDisplayName(PropertyPageUIText.IncludeDirs)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.IncludeDirsDescription)]
        public string IncludeDirs
        {
            get { return this.includeDirs!; }
            set { if (String.Compare(this.includeDirs, value) != 0) this.IsDirty = true; this.includeDirs = value; }
        }

        [ResourcesCategoryAttribute(PropertyPageUIText.Category_Build)]
        [LocDisplayName(PropertyPageUIText.LibraryDirs)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.LibraryDirsDescription)]
        public string LibraryDirs
        {
            get { return this.libraryDirs!; }
            set { if (String.Compare(this.libraryDirs, value) != 0) this.IsDirty = true; this.libraryDirs = value; }
        }

        [ResourcesCategoryAttribute(PropertyPageUIText.Category_Build)]
        [LocDisplayName(PropertyPageUIText.Libraries)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.LibrariesDescription)]
        public string Libraries
        {
            get { return this.libraries!; }
            set { if (String.Compare(this.libraries, value) != 0) this.IsDirty = true; this.libraries = value; }
        }

        [ResourcesCategoryAttribute(PropertyPageUIText.Category_Build)]
        [LocDisplayName(PropertyPageUIText.RootSourceName)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.RootSourceNameDescription)]
        public string RootSourceName
        {
            get { return this.rootSourceName!; }
            set { if (String.Compare(this.rootSourceName, value) != 0) this.IsDirty = true; this.rootSourceName = value; }
        }

        [ResourcesCategoryAttribute(PropertyPageUIText.Category_Assembly)]
        [LocDisplayName(PropertyPageUIText.OSType)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.OSTypeDescription)]
        public OSType OSType
        {
            get { return this.osType; }
            set { if (this.osType != value) this.IsDirty = true; this.osType = value; }
        }

        [ResourcesCategoryAttribute(PropertyPageUIText.Category_Assembly)]
        [LocDisplayName(PropertyPageUIText.AssemblyName)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.AssemblyNameDescription)]
        public string AssemblyName
        {
            get { return this.assemblyName!; }
            set { if (String.Compare(this.assemblyName, value) != 0) this.IsDirty = true; this.assemblyName = value;  }
        }

        [ResourcesCategoryAttribute(PropertyPageUIText.Category_Assembly)]
        [LocDisplayName(PropertyPageUIText.AssemblyVersion)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.AssemblyVersionDescription)]
        public string AssemblyVersion
        {
            get { return this.assemblyVersion!; }
            set { if (String.Compare(this.assemblyVersion, value) != 0) this.IsDirty = true; this.assemblyVersion = value; }
        }

        [ResourcesCategoryAttribute(PropertyPageUIText.Category_Assembly)]
        [LocDisplayName(PropertyPageUIText.ConfigurationType)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.ConfigurationTypeDescription)]
        public OutputType ConfigurationType
        {
            get { return this.configurationType; }
            set { if (this.configurationType != value) this.IsDirty = true; this.configurationType = value; }
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
            set { if (string.Compare(string.Join(";", this.modules), string.Join(";", value)) != 0) this.IsDirty = true; this.modules = value; }
        }

        [ResourcesCategoryAttribute(PropertyPageUIText.Category_Dependency)]
        [LocDisplayName(PropertyPageUIText.Dependencies)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.DependenciesDescription)]
        public string[] Dependencies
        {
            get { return this.dependencies!; }
            set { if (string.Compare(string.Join(";", this.dependencies), string.Join(";", value)) != 0) this.IsDirty = true; this.dependencies = value; }
        }

        [ResourcesCategoryAttribute(PropertyPageUIText.Category_Generation)]
        [LocDisplayName(PropertyPageUIText.GenerateBuildDotZig)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.GenerateBuildDotZigDescription)]
        public bool GenerateBuildDotZig
        {
            get { return this.generateBuildDotZig!; }
            set { if (this.generateBuildDotZig != value) this.IsDirty = true; this.generateBuildDotZig = value; }
        }

        [ResourcesCategoryAttribute(PropertyPageUIText.Category_Generation)]
        [LocDisplayName(PropertyPageUIText.GenerateBuildDotZigDotZon)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.GenerateBuildDotZigDotZonDescription)]
        public bool GenerateBuildDotZigDotZon
        {
            get { return this.generateBuildDotZigDotZon!; }
            set { if (this.generateBuildDotZigDotZon != value) this.IsDirty = true; this.generateBuildDotZigDotZon = value; }
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

            this.assemblyName = this.ProjectManager.GetProjectPropertyUnevaluated("AssemblyName", _PersistStorageType.PST_PROJECT_FILE);
            this.assemblyVersion = this.ProjectManager.GetProjectPropertyUnevaluated("AssemblyVersion", _PersistStorageType.PST_PROJECT_FILE);
            string l_osType = this.ProjectManager.GetProjectPropertyUnevaluated("OSType", _PersistStorageType.PST_PROJECT_FILE);
            string l_configurationType = this.ProjectManager.GetProjectPropertyUnevaluated("ConfigurationType", _PersistStorageType.PST_PROJECT_FILE);
            this.outputFile = this.ProjectManager.GetProjectProperty("OutputName", _PersistStorageType.PST_PROJECT_FILE, true);

            this.buildOption = this.ProjectManager.GetProjectPropertyUnevaluated("BuildOption", _PersistStorageType.PST_PROJECT_FILE);
            string l_useBuildDotZig = this.ProjectManager.GetProjectPropertyUnevaluated("UseBuildDotZig", _PersistStorageType.PST_PROJECT_FILE);
            this.rootSourceName = this.ProjectManager.GetProjectPropertyUnevaluated("RootSourceName", _PersistStorageType.PST_PROJECT_FILE);
            this.includeDirs = this.ProjectManager.GetProjectPropertyUnevaluated("IncludeDirs", _PersistStorageType.PST_PROJECT_FILE);
            this.libraryDirs = this.ProjectManager.GetProjectPropertyUnevaluated("LibraryDirs", _PersistStorageType.PST_PROJECT_FILE);
            this.libraries = this.ProjectManager.GetProjectPropertyUnevaluated("Libraries", _PersistStorageType.PST_PROJECT_FILE);
            var l_tempDependenciesArray = this.ProjectManager.GetProjectPropertyUnevaluated("Dependencies", _PersistStorageType.PST_PROJECT_FILE);
            this.dependencies = l_tempDependenciesArray.Split(';');
            var l_tempModluesArray = this.ProjectManager.GetProjectPropertyUnevaluated("Modules", _PersistStorageType.PST_PROJECT_FILE);
            this.modules = l_tempModluesArray.Split(';');
            this.intDirName = this.ProjectManager.GetProjectPropertyUnevaluated("IntDirName", _PersistStorageType.PST_PROJECT_FILE);
            this.outDirName = this.ProjectManager.GetProjectPropertyUnevaluated("OutDirName", _PersistStorageType.PST_PROJECT_FILE);

            string l_generateBuildDotZig = this.ProjectManager.GetProjectPropertyUnevaluated("GenerateBuildDotZig", _PersistStorageType.PST_PROJECT_FILE);
            string l_generateBuildDotZigDotZon = this.ProjectManager.GetProjectPropertyUnevaluated("GenerateBuildDotZigDotZon", _PersistStorageType.PST_PROJECT_FILE);

            if (!string.IsNullOrEmpty(l_osType))
            {
                try
                {
                    this.OSType = (OSType)Enum.Parse(typeof(OSType), l_osType);
                }
                catch (ArgumentException)
                {
                }
            }

            if (!string.IsNullOrEmpty(l_configurationType))
            {
                try
                {
                    this.configurationType = (OutputType)Enum.Parse(typeof(OutputType), l_configurationType);
                }
                catch (ArgumentException)
                {
                }
            }

            if( !string.IsNullOrEmpty(l_useBuildDotZig))
            {
                try
                {
                    this.useBuildDotZig = bool.Parse(l_useBuildDotZig);
                }
                catch (ArgumentException)
                {
                }
            }

            if ( !string.IsNullOrEmpty(l_generateBuildDotZig))
            {
                try
                {
                    this.generateBuildDotZig = bool.Parse(l_generateBuildDotZig);
                }
                catch (ArgumentException)
                {
                }
            }

            if(!string.IsNullOrEmpty(l_generateBuildDotZigDotZon))
            {
                try
                {
                    this.generateBuildDotZigDotZon = bool.Parse(l_generateBuildDotZigDotZon);
                }
                catch (ArgumentException)
                {
                }
            }

            IsDirty = false;
        }

        protected override int ApplyChanges()
        {
            if (this.ProjectManager == null)
            {
                return VSConstants.E_INVALIDARG;
            }

            ThreadHelper.ThrowIfNotOnUIThread();
            IVsPropertyPageFrame? l_IVsPropertyPageFrame = (IVsPropertyPageFrame)this.ProjectManager.Site.GetService((typeof(SVsPropertyPageFrame)));

            this.ProjectManager.SetProjectProperty("BuildOption", _PersistStorageType.PST_PROJECT_FILE, this.buildOption);
            this.ProjectManager.SetProjectProperty("UseBuildDotZig", _PersistStorageType.PST_PROJECT_FILE, this.useBuildDotZig.ToString());
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

            this.ProjectManager.SetProjectProperty("GenerateBuildDotZig", _PersistStorageType.PST_PROJECT_FILE, this.generateBuildDotZig.ToString());
            this.ProjectManager.SetProjectProperty("GenerateBuildDotZigDotZon", _PersistStorageType.PST_PROJECT_FILE, this.generateBuildDotZigDotZon.ToString());

            IsDirty = false;

            return VSConstants.S_OK;
        }
    }
}