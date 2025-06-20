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

namespace ZigVS.Test
{
#nullable disable
#pragma warning disable CS0414,CS0067,CS1998

    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.ComponentModelHost;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.TestWindow.Extensibility;
    using Microsoft.VisualStudio.Workspace;
    using Microsoft.VisualStudio.Workspace.VSIntegration.Contracts;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    // Read https://devblogs.microsoft.com/devops/writing-a-visual-studio-2012-unit-test-adapter/#comments
    // Read https://learn.microsoft.com/ja-jp/archive/blogs/bhuvaneshwari/authoring-a-new-visual-studio-unit-test-adapter
    // Read https://learn.microsoft.com/ja-jp/archive/blogs/aseemb/how-to-make-your-extension-visible-to-the-test-explorer-in-visual-studio-11
    // Read https://matthewmanela.com/blog/anatomy-of-the-chutzpah-test-adapter-for-vs-2012-rc/

    [Export(typeof(ITestContainerDiscoverer))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TestContainerDiscoverer : ITestContainerDiscoverer, IVsSolutionEvents
    {
        static TestContainerDiscoverer m_instance = null;

        ILogger m_Logger;
        IServiceProvider m_IServiceProvider = null;

        // Project Mode
        IVsSolution m_IVsSolution = null;
        private uint cookie1 = VSConstants.VSCOOKIE_NIL;
        static ConcurrentDictionary<string, TestContainer> m_projectModeTestContainersDictionary = null;

        // Folder Mode
        IVsFolderWorkspaceService m_IVsFolderWorkspaceService = null;
        IWorkspace m_IWorkspace = null;
        public event EventHandler TestContainersUpdated;
        static ConcurrentDictionary<string, string> m_folderModeSourceFileDictionary = null;


        [ImportingConstructor]
        public TestContainerDiscoverer(
            [Import(typeof(SVsServiceProvider))] IServiceProvider i_IServiceProvider,
            ILogger i_ILogger)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            m_instance = this;

            m_Logger = i_ILogger;
            if (m_Logger != null)
            {
                m_Logger.Log(MessageLevel.Error, "TestContainerDiscoverer() is called.");
            }

            m_IServiceProvider = i_IServiceProvider;

            // Project Mode event catch
            m_IVsSolution = (IVsSolution)m_IServiceProvider.GetService(typeof(SVsSolution));
            if (m_IVsSolution != null)
            {
                m_IVsSolution.AdviseSolutionEvents(this, out this.cookie1);
            }

            // Holder Mode event catch
            m_IVsFolderWorkspaceService = Utilities.GetRequiredService<SComponentModel, IComponentModel>().GetService<IVsFolderWorkspaceService>();
            if (m_IVsFolderWorkspaceService != null)
            {
                m_IVsFolderWorkspaceService.OnActiveWorkspaceChanged += OnActiveWorkspaceChangedAsync;
            }
        }

        public Uri ExecutorUri
        {
            get { return Parameter.c_TestExecutorUri; }
        }

        public IEnumerable<ITestContainer> TestContainers
        {
            get
            {
                return UpdateContainer().Values;
            }
        }

        private ConcurrentDictionary<string, TestContainer> UpdateContainer()
        {
            var r_sourceDictionary = new ConcurrentDictionary<string, TestContainer>();
            try
            {
                if (m_projectModeTestContainersDictionary != null)
                {
                    // Project Mode source code enumeration
                    r_sourceDictionary = m_projectModeTestContainersDictionary;
                }
                else
                {
                    // Project Mode source code enumeration
                    var l_IVsHierarchyEnum = SolusionItems.GetProjects(m_IVsSolution);
                    if (l_IVsHierarchyEnum.Count<IVsHierarchy>() != 0)
                    {
                        foreach (var l_IVsHierarchy in l_IVsHierarchyEnum)
                        {
                            var l_stringIEnumerable = SolusionItems.GetProjectItems(l_IVsHierarchy);
                            foreach (var l_string in l_stringIEnumerable)
                            {
                                var l_fileNameString = Path.GetFileName(l_string);
                                if (!string.IsNullOrEmpty(l_fileNameString))
                                {
                                    var l_fileExtensionString = Path.GetExtension(l_fileNameString);
                                    if (l_fileExtensionString.ToLower() == Parameter.c_fileExtension.ToLower())
                                    {
                                        if (m_projectModeTestContainersDictionary == null)
                                        {
                                            m_projectModeTestContainersDictionary = new ConcurrentDictionary<string, TestContainer>();
                                        }

                                        var l_TestContainer = new TestContainer(this, l_string);
                                        m_projectModeTestContainersDictionary.TryAdd(l_string, l_TestContainer);
                                    }
                                }
                            }
                        }
                        if (m_projectModeTestContainersDictionary != null)
                        {
                            r_sourceDictionary = m_projectModeTestContainersDictionary;
                        }
                    }
                }

                if ((m_projectModeTestContainersDictionary == null) &&
                    (m_folderModeSourceFileDictionary != null) &&
                    (m_folderModeSourceFileDictionary.Keys.Count != 0))
                {
                    var l_sourceDictionary = new ConcurrentDictionary<string, TestContainer>();

                    // folder mode
                    foreach (var l_sourceString in m_folderModeSourceFileDictionary.Keys)
                    {
                        var l_TestContainer = new TestContainer(this, l_sourceString);
                        l_sourceDictionary.TryAdd(l_sourceString, l_TestContainer);
                    }
                    r_sourceDictionary = l_sourceDictionary;
                }
            }
            catch
            {
            }
            return r_sourceDictionary;
        }

        // Open Folder Event
        private async Task OnActiveWorkspaceChangedAsync(object i_senderObject, EventArgs i_EventArgs)
        {
            m_folderModeSourceFileDictionary = null;
        }

        static public void AddSource(string i_sourcePathString)
        {
            if (!string.IsNullOrEmpty(i_sourcePathString))
            {
                var l_fileName = Path.GetFileName(i_sourcePathString);
                if (l_fileName != "builtin.zig" && l_fileName != "dependencies.zig")
                {
                    if (m_folderModeSourceFileDictionary == null)
                    {
                        m_folderModeSourceFileDictionary = new ConcurrentDictionary<string, string>();
                    }
                    m_folderModeSourceFileDictionary.TryAdd(i_sourcePathString, i_sourcePathString);
                }
            }
        }

        // Solution Events
        int IVsSolutionEvents.OnAfterCloseSolution(object pUnkReserved)
        {
            m_projectModeTestContainersDictionary = null;
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            m_projectModeTestContainersDictionary = null;
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            m_projectModeTestContainersDictionary = null;
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            m_projectModeTestContainersDictionary = null;
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            m_projectModeTestContainersDictionary = null;
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeCloseSolution(object pUnkReserved)
        {
            m_projectModeTestContainersDictionary = null;
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            m_projectModeTestContainersDictionary = null;
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            m_projectModeTestContainersDictionary = null;
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            m_projectModeTestContainersDictionary = null;
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            m_projectModeTestContainersDictionary = null;
            return VSConstants.S_OK;
        }
    }
}