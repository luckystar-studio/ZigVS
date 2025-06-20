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
    using Microsoft.VisualStudio.Shell;

    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
#nullable enable
#pragma warning disable CS8603

    // All menu item attribute
    [ProvideMenuResource("Menus.ctmenu", 1)]
    // [Tool] -> [Option] -> [Zig] page attributes
    [ProvideOptionPage(typeof(DialogPageProvider.General), Parameter.c_PackageNameStrig, "General", 0, 0, true)]
    [ProvideOptionPage(typeof(DialogPageProvider.ProjectMode), Parameter.c_PackageNameStrig, "Project Mode", 0, 0, true)]
    [ProvideOptionPage(typeof(DialogPageProvider.FolderMode), Parameter.c_PackageNameStrig, "Folder Mode", 0, 0, true)]
    // [Tool] -> [Option] -> [Text Editor] page attributes
    // [ProvideLanguageEditorOptionPage(typeof(DialogPageProvider.TextEditorAdvanced), Parameter.c_PackageNameStrig, null, "Advanced", "#110")]
    // [Extensions] -> [ZigVS]
    [ProvideToolWindow(typeof(ZigVS.ToolchainInstallerWindow))]
    [ProvideToolWindow(typeof(ZigVS.PackageInstallerWindow))]

    public sealed class ZigVSPackage : Microsoft.VisualStudio.Project.ProjectPackage
    {
        static Package? s_Package;
        public static Package GetInstance()
        {
            return s_Package;
        }

        public ZigVSPackage()
        {
            s_Package = this;
        }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            // Initialize Commands
            _ = Command.FormattingCommand.InitializeAsync();
            _ = Command.ToolchainInstallerCommand.InitializeAsync();
            _ = Command.PackageInstallerCommand.InitializeAsync();
            _ = Command.PackageCreatorCommand.InitializeAsync();
            _ = Command.HelpCommand.InitializeAsync();
            _ = Command.QAndACommand.InitializeAsync();
            _ = Command.RatingAndReviewCommand.InitializeAsync();
            //      _ = Command.DebugEngineSelectorCommand.InitializeAsync(this);

            // Initialize Project
            var l_ZigVSProjectFactory = new ZigVSProjectFactory(this);
            RegisterProjectFactory(l_ZigVSProjectFactory);
        }

        public override string ProductUserContext
        {
            get { return Parameter.c_PackageNameStrig; }
        }
    }
#pragma warning restore CS8603  
}