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
    [Guid("9741DC76-FDF3-4AF2-BCFF-482A2875F479")]
    public class PropertyPage_Debug : SettingsPage
    {
        private string workingDirectory = "";
 
        private string preDebugCommmand = "";
        private string preDebugCommmandArguments = "";

        private string startProgram = "";
        private string commandLineArguments = "";
        private DebugEngine debugEngine = DebugEngine.WindowsNative;
        private string miEngineLaunchOptions = "LaunchOptions.xml";
        private string remoteDebugMachine = "";

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

        public PropertyPage_Debug() : base(GetCurrentProject())
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            this.Name = ZigVS.Resources.Resource.DebugCaption;
        }

        public PropertyPage_Debug(ProjectNode projectManager)
            : base(projectManager)
        {
            this.Name = ZigVS.Resources.Resource.DebugCaption;
        }

        [ResourcesCategoryAttribute(PropertyPageUIText.Category_PreDebug)]
        [LocDisplayName(PropertyPageUIText.PreDebugCommand)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.PreDebugCommandDescription)]
        public string PreDebugCommand
        {
            get { return this.preDebugCommmand!; }
            set { this.preDebugCommmand = value; this.IsDirty = true; }
        }

        [ResourcesCategoryAttribute(PropertyPageUIText.Category_PreDebug)]
        [LocDisplayName(PropertyPageUIText.PreDebugCommandArguments)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.PreDebugCommandArgumentsDescription)]
        public string PreDebugCommandArguments
        {
            get { return this.preDebugCommmandArguments!; }
            set { this.preDebugCommmandArguments = value; this.IsDirty = true; }
        }

        [ResourcesCategoryAttribute(PropertyPageUIText.Category_Debug)]
        [LocDisplayName(PropertyPageUIText.WorkingDirectory)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.WorkingDirectoryDescription)]
        public string WorkingDirectory
        {
            get { return this.workingDirectory!; }
            set { this.workingDirectory = value; this.IsDirty = true; }
        }

        [ResourcesCategoryAttribute(PropertyPageUIText.Category_Debug)]
        [LocDisplayName(PropertyPageUIText.StartupProgram)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.StartupProgramDescription)]
        public string StartProgram
        {
            get { return this.startProgram; }
            set { this.startProgram = value; this.IsDirty = true; }
        }

        [ResourcesCategoryAttribute(PropertyPageUIText.Category_Debug)]
        [LocDisplayName(PropertyPageUIText.CommandLineArguments)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.CommandLineArgumentsDescription)]
        public string CommandLineArguments
        {
            get { return this.commandLineArguments!; }
            set { this.commandLineArguments = value; this.IsDirty = true; }
        }

        [ResourcesCategoryAttribute(PropertyPageUIText.Category_Debug)]
        [LocDisplayName(PropertyPageUIText.DebugEngineType)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.DebugEngineTypeDescription)]
        public DebugEngine DebugEngine
        {
            get { return this.debugEngine!; }
            set { this.debugEngine = value; this.IsDirty = true; }
        }

        [ResourcesCategoryAttribute(PropertyPageUIText.Category_Debug)]
        [LocDisplayName(PropertyPageUIText.MIEngineLaunchOptions)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.MIEngineLaunchOptionsDescription)]
        public string MIEngineLaunchOptions
        {
            get { return this.miEngineLaunchOptions!; }
            set { this.miEngineLaunchOptions = value; this.IsDirty = true; }
        }
        
        [ResourcesCategoryAttribute(PropertyPageUIText.Category_Debug)]
        [LocDisplayName(PropertyPageUIText.RemoteDebugMachine)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.RemoteDebugMachineDescription)]
        public string RemoteDebugMachine
        {
            get { return this.remoteDebugMachine!; }
            set { this.remoteDebugMachine = value; this.IsDirty = true; }
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

            this.preDebugCommmand = this.ProjectManager.GetProjectProperty("PreDebugCommand", _PersistStorageType.PST_PROJECT_FILE, false);
            this.preDebugCommmandArguments = this.ProjectManager.GetProjectProperty("PreDebugCommandArguments", _PersistStorageType.PST_PROJECT_FILE, false);

            this.workingDirectory = this.ProjectManager.GetProjectProperty("WorkingDirectory", _PersistStorageType.PST_PROJECT_FILE, false);
            this.startProgram = this.ProjectManager.GetProjectProperty("StartProgram", _PersistStorageType.PST_PROJECT_FILE, false);
            this.commandLineArguments = this.ProjectManager.GetProjectProperty("CmdArgs", _PersistStorageType.PST_PROJECT_FILE, false);
            string l_debugEngineString = this.ProjectManager.GetProjectProperty("DebugEngine", _PersistStorageType.PST_PROJECT_FILE, false);
            this.miEngineLaunchOptions = this.ProjectManager.GetProjectProperty("MIEngineLaunchOptions", _PersistStorageType.PST_PROJECT_FILE, false);
            this.remoteDebugMachine = this.ProjectManager.GetProjectProperty("RemoteDebugMachine", _PersistStorageType.PST_PROJECT_FILE, false);
 
            if (!string.IsNullOrEmpty(l_debugEngineString))
            {
                try
                {
                    this.DebugEngine = (DebugEngine)Enum.Parse(typeof(DebugEngine), l_debugEngineString);
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
 
            this.ProjectManager.SetProjectProperty("PreDebugCommand", _PersistStorageType.PST_PROJECT_FILE, this.preDebugCommmand);
            this.ProjectManager.SetProjectProperty("PreDebugCommandArguments", _PersistStorageType.PST_PROJECT_FILE, this.preDebugCommmandArguments);

            this.ProjectManager.SetProjectProperty("WorkingDirectory", _PersistStorageType.PST_PROJECT_FILE, this.workingDirectory.ToString());
            this.ProjectManager.SetProjectProperty("StartProgram", _PersistStorageType.PST_PROJECT_FILE, this.startProgram.ToString());
            this.ProjectManager.SetProjectProperty("CmdArgs", _PersistStorageType.PST_PROJECT_FILE, this.commandLineArguments);
            this.ProjectManager.SetProjectProperty("DebugEngine", _PersistStorageType.PST_PROJECT_FILE, this.debugEngine.ToString());
            this.ProjectManager.SetProjectProperty("MIEngineLaunchOptions", _PersistStorageType.PST_PROJECT_FILE, this.miEngineLaunchOptions.ToString());
            this.ProjectManager.SetProjectProperty("RemoteDebugMachine", _PersistStorageType.PST_PROJECT_FILE, this.remoteDebugMachine);

            this.IsDirty = false;

            if (IsDirty)
            {
                IVsPropertyPageFrame propertyPageFrame = (IVsPropertyPageFrame)this.ProjectManager.Site.GetService((typeof(SVsPropertyPageFrame)));
                    // This prevents the property page from displaying bad data from the zombied (unloaded) project
                propertyPageFrame?.HideFrame();
                propertyPageFrame?.ShowFrame(this.GetType().GUID);
            }

            return VSConstants.S_OK;
        }
    }
}