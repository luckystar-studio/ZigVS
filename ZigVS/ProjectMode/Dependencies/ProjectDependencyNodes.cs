namespace ZigVS
{
#nullable enable
    using EnvDTE;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Project;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public sealed class DependenciesRootNode : HierarchyNode
    {
        public const string c_virtualNodeName = "__zigvs_dependencies__";
        private readonly ZigVSProjectNode projectNode;

        public DependenciesRootNode(ZigVSProjectNode projectNode)
            : base(projectNode)
        {
            this.projectNode = projectNode;
        }

        public override string Caption => "Dependencies";

        public override string VirtualNodeName => c_virtualNodeName;

        public override string Url => Path.Combine(this.projectNode.ProjectFolder, "package");

        public override Guid ItemTypeGuid => VSConstants.GUID_ItemType_VirtualFolder;

        public override int MenuCommandId => VsMenus.IDM_VS_CTXT_REFERENCEROOT;

        public override int SortPriority => 200;

        public void RebuildChildren(IEnumerable<DependencyNode> dependencyNodes)
        {
            List<HierarchyNode> children = new List<HierarchyNode>();
            for (HierarchyNode? child = this.FirstChild; child != null; child = child.NextSibling)
            {
                children.Add(child);
            }

            foreach (HierarchyNode child in children)
            {
                this.RemoveChild(child);
            }

            foreach (DependencyNode dependencyNode in dependencyNodes)
            {
                this.AddChild(dependencyNode);
            }

            this.Redraw(UIHierarchyElements.Icon | UIHierarchyElements.Caption);
            this.projectNode.Redraw(UIHierarchyElements.Icon | UIHierarchyElements.Caption);
        }
    }

    [ComVisible(true)]
    public sealed class DependencyNode : HierarchyNode
    {
        private readonly ZigVSProjectNode projectNode;
        private readonly ProjectDependencySpec spec;
        private readonly ProjectDependencyStatusInfo statusInfo;

        public DependencyNode(ZigVSProjectNode projectNode, ProjectDependencySpec spec, ProjectDependencyStatusInfo statusInfo)
            : base(projectNode)
        {
            this.projectNode = projectNode;
            this.spec = spec;
            this.statusInfo = statusInfo;
        }

        public ProjectDependencySpec Spec => this.spec.Clone();

        public override string Caption => this.statusInfo.Status == ProjectDependencyStatus.Resolved
            ? this.spec.Include
            : $"{this.spec.Include} ({this.statusInfo.Status.ToString().ToLowerInvariant()})";

        public override string VirtualNodeName => "__zigvs_dependency__:" + this.spec.Include;

        public override string Url => ProjectDependencyService.Instance.GetAbsoluteCheckoutPath(this.projectNode.ProjectFolder, this.spec);

        public override Guid ItemTypeGuid => VSConstants.GUID_ItemType_VirtualFolder;

        public override int MenuCommandId => VsMenus.IDM_VS_CTXT_REFERENCE;

        public override int SortPriority => 210;

        public override VsStateIcon StateIconIndex => VsStateIcon.STATEICON_NOSTATEICON;

        protected override NodeProperties CreatePropertiesObject()
        {
            return new DependencyNodeProperties(this);
        }

        protected override int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText, ref vsCommandStatus result)
        {
            if (cmdGroup == VSConstants.GUID_VSStandardCommandSet97 && cmd == (uint)VSConstants.VSStd97CmdID.Delete)
            {
                result = vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
                return VSConstants.S_OK;
            }

            return base.QueryStatusOnNode(cmdGroup, cmd, pCmdText, ref result);
        }

        protected override int ExecCommandOnNode(Guid cmdGroup, uint cmd, Microsoft.VisualStudio.OLE.Interop.OLECMDEXECOPT nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (cmdGroup == VSConstants.GUID_VSStandardCommandSet97 && cmd == (uint)VSConstants.VSStd97CmdID.Delete)
            {
                RemoveFromProject();
                return VSConstants.S_OK;
            }

            return base.ExecCommandOnNode(cmdGroup, cmd, nCmdexecopt, pvaIn, pvaOut);
        }

        public void Save(ProjectDependencySpec updatedSpec)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ProjectDependencyService.Instance.SaveDependency(this.projectNode, updatedSpec);
            this.projectNode.RefreshDependencyNodes();
        }

        public string GetStatusDescription()
        {
            return this.statusInfo.Description;
        }

        private void RemoveFromProject()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            int removeAnswer = VsShellUtilities.ShowMessageBox(
                this.projectNode.Site,
                $"Remove '{this.spec.Include}' from this Zig project?",
                "Remove Zig Dependency",
                OLEMSGICON.OLEMSGICON_QUERY,
                OLEMSGBUTTON.OLEMSGBUTTON_YESNO,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND);
            if (removeAnswer != (int)VSConstants.MessageBoxResult.IDYES)
            {
                return;
            }

            bool deleteCheckout = false;
            string checkoutDirectoryPath = ProjectDependencyService.Instance.GetAbsoluteCheckoutPath(this.projectNode.ProjectFolder, this.spec);
            if (Directory.Exists(checkoutDirectoryPath))
            {
                int deleteAnswer = VsShellUtilities.ShowMessageBox(
                    this.projectNode.Site,
                    this.statusInfo.Status == ProjectDependencyStatus.Dirty
                        ? "The checkout has local changes. Delete the checkout folder too?"
                        : "Delete the checked out package folder too?",
                    "Delete Dependency Checkout",
                    OLEMSGICON.OLEMSGICON_QUERY,
                    OLEMSGBUTTON.OLEMSGBUTTON_YESNO,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND);
                deleteCheckout = deleteAnswer == (int)VSConstants.MessageBoxResult.IDYES;
            }

            ProjectDependencyService.Instance.RemoveDependency(this.projectNode, this.spec.Include);
            if (deleteCheckout && Directory.Exists(checkoutDirectoryPath))
            {
                try
                {
                    Common.File.DeleteDirectoryRobust(checkoutDirectoryPath);
                }
                catch (Exception ex)
                {
                    VsShellUtilities.ShowMessageBox(
                        this.projectNode.Site,
                        $"The dependency was removed from the project, but the checkout folder could not be deleted.{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                        "Delete Dependency Checkout",
                        OLEMSGICON.OLEMSGICON_WARNING,
                        OLEMSGBUTTON.OLEMSGBUTTON_OK,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                }
            }

            this.projectNode.RefreshDependencyNodes();
        }
    }

    [ComVisible(true)]
    public sealed class DependencyNodeProperties : NodeProperties
    {
        private readonly DependencyNode node;

        public DependencyNodeProperties(DependencyNode node)
            : base(node)
        {
            this.node = node;
        }

        [Category("Dependency")]
        [DisplayName("Name")]
        [ReadOnly(true)]
        public string Include => this.node.Spec.Include;

        [Category("Dependency")]
        [DisplayName("Repository URL")]
        [Description("Git repository URL. Build or refresh will restore from this repository.")]
        public string RepositoryUrl
        {
            get => this.node.Spec.RepositoryUrl;
            set
            {
                ProjectDependencySpec updated = this.node.Spec;
                updated.RepositoryUrl = value ?? String.Empty;
                this.node.Save(updated);
            }
        }

        [Category("Dependency")]
        [DisplayName("Commit")]
        [Description("Pinned git commit. Build or refresh will restore this revision.")]
        public string Commit
        {
            get => this.node.Spec.Commit;
            set
            {
                ProjectDependencySpec updated = this.node.Spec;
                updated.Commit = value ?? String.Empty;
                this.node.Save(updated);
            }
        }

        [Category("Dependency")]
        [DisplayName("Module Name")]
        [Description("Module name passed to zig --deps / --mod.")]
        public string ModuleName
        {
            get => this.node.Spec.ModuleName;
            set
            {
                ProjectDependencySpec updated = this.node.Spec;
                updated.ModuleName = value ?? String.Empty;
                this.node.Save(updated);
            }
        }

        [Category("Dependency")]
        [DisplayName("Root Source")]
        [Description("Relative path inside the dependency checkout.")]
        public string RootSource
        {
            get => this.node.Spec.RootSource;
            set
            {
                ProjectDependencySpec updated = this.node.Spec;
                updated.RootSource = value ?? String.Empty;
                this.node.Save(updated);
            }
        }

        [Category("Dependency")]
        [DisplayName("Checkout Dir")]
        [ReadOnly(true)]
        public string CheckoutDir => this.node.Spec.CheckoutDir;

        [Category("Dependency")]
        [DisplayName("Status")]
        [ReadOnly(true)]
        public string Status => this.node.GetStatusDescription();

        public override string GetClassName()
        {
            return this.GetType().FullName;
        }
    }
}
