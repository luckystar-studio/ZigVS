namespace ZigVS.Command
{
    using Microsoft.VisualStudio.Shell;
    using System;
    using System.ComponentModel.Design;

#nullable enable
    internal sealed class AddProjectDependencyCommand
    {
        static AddProjectDependencyCommand? s_Instance = null;

        public static async System.Threading.Tasks.Task InitializeAsync()
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                OleMenuCommandService l_OleMenuCommandService = ZigVSPackage.GetInstance().GetService<IMenuCommandService, OleMenuCommandService>();
                s_Instance = new AddProjectDependencyCommand(l_OleMenuCommandService);
            });
        }

        private AddProjectDependencyCommand(OleMenuCommandService i_OleMenuCommandService)
        {
            if (i_OleMenuCommandService != null)
            {
                var l_CommandId = new CommandID(CommandDefinition.s_CommandSetGuid, (int)CommandDefinition.CommandId.AddProjectDependency);
                var l_MenuItem = new OleMenuCommand(this.Execute, l_CommandId);
                l_MenuItem.BeforeQueryStatus += this.BeforeQueryStatus;
                i_OleMenuCommandService.AddCommand(l_MenuItem);
            }
        }

        private void BeforeQueryStatus(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (sender is OleMenuCommand l_MenuCommand)
            {
                var l_SelectedNode = Utilities.GetSelectedHierarchyNode();
                bool l_IsVisible =
                    l_SelectedNode is ZigVSProjectNode ||
                    l_SelectedNode is DependenciesRootNode;

                l_MenuCommand.Supported = l_IsVisible;
                l_MenuCommand.Visible = l_IsVisible;
                l_MenuCommand.Enabled = l_IsVisible && Utilities.GetActiveZigVSProjectNode() != null;
            }
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            _ = Utilities.GetSelectedZigVSProjectNode();
            PackageInstallerWindowControl.RequestManagedProjectDependencyMode();
            Utilities.ShowToolWindow(typeof(PackageInstallerWindow));
            PackageInstallerWindowControl.GetInstance()?.Reset();
            PackageInstallerWindowControl.GetInstance()?.SelectManagedProjectDependencyMethod();
        }
    }
}
