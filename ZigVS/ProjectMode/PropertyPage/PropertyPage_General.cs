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
    [Guid("15F42713-60F2-43A3-9733-35F30F21F22E")]
    public class PropertyPage_General : SettingsPage
    {
        private string toolName="";
 
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

        public PropertyPage_General() : base(GetCurrentProject())
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            this.Name = ZigVS.Resources.Resource.GeneralCaption;
         }

        public PropertyPage_General(ProjectNode projectManager)
            : base(projectManager)
        {
            this.Name = ZigVS.Resources.Resource.GeneralCaption;
        }
 
        [ResourcesCategoryAttribute(PropertyPageUIText.Category_Tool)]
        [LocDisplayName(PropertyPageUIText.ToolName)]
        [ResourcesDescriptionAttribute(PropertyPageUIText.ToolNameCaption)]
        public string ToolName
        {
            get { return this.toolName!; }
            set { this.toolName = value; this.IsDirty = true; }
        }

        public override string GetClassName()
        {
            return this.GetType().FullName;
        }

        protected override void BindProperties()
        {
            if(this.ProjectManager == null)
            {
                return;
            }

            this.toolName = this.ProjectManager.GetProjectProperty("ToolName", _PersistStorageType.PST_PROJECT_FILE, false);

     /*       try
            {
				this.targetFrameworkMoniker = this.ProjectManager.TargetFrameworkMoniker;
            }
            catch (ArgumentException)
            {
            }*/
        }

        protected override int ApplyChanges()
        {
            if (this.ProjectManager == null)
            {
                return VSConstants.E_INVALIDARG;
            }

            ThreadHelper.ThrowIfNotOnUIThread();
			IVsPropertyPageFrame propertyPageFrame = (IVsPropertyPageFrame)this.ProjectManager.Site.GetService((typeof(SVsPropertyPageFrame)));
            //		bool reloadRequired = this.ProjectManager.TargetFrameworkMoniker != this.targetFrameworkMoniker;

            this.ProjectManager.SetProjectProperty("ToolName", _PersistStorageType.PST_PROJECT_FILE, this.toolName);

            /*	if (reloadRequired)
                {
                    if (MessageBox.Show(Resource.GetString(Resource.ReloadPromptOnTargetFxChanged), Resource.GetString(Resource.ReloadPromptOnTargetFxChangedCaption), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        this.ProjectManager.TargetFrameworkMoniker = this.targetFrameworkMoniker;
                    }
                }
    */
            this.IsDirty = false;

		/*	if (reloadRequired)
			{
				// This prevents the property page from displaying bad data from the zombied (unloaded) project
				propertyPageFrame.HideFrame();
				propertyPageFrame.ShowFrame(this.GetType().GUID);
			}*/

            return VSConstants.S_OK;
        }
    }
}