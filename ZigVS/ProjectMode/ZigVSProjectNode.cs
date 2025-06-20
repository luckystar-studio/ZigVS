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
    using Microsoft.Build.Execution;
    using Microsoft.VisualStudio.Project;
    using Microsoft.VisualStudio.Project.Automation;
    using System;
    using System.IO;
    using System.Linq;
    using VSLangProj;

    public class ZigVSProjectNode : ProjectNode
    {
        private ZigVSPackage m_ZigVSPackage;
        private VSLangProj.VSProject? m_VSProject;

        public ZigVSProjectNode(ZigVSPackage i_ZigVSPackage) : base(i_ZigVSPackage)
        {
            m_ZigVSPackage = i_ZigVSPackage;
            AddCatIdMapping(typeof(FileNodeProperties), typeof(FileNodeProperties).GUID);
        }

        protected internal VSLangProj.VSProject VSProject
        {
            get
            {
                if (m_VSProject == null)
                {
                    m_VSProject = new OAVSProject(this);
                }

                return m_VSProject;
            }
        }

        public override Guid ProjectGuid
        {
            get { return Parameter.s_ProjectFactoryGuid; }
        }

        public override string ProjectType
        {
            get { return "ZigProjectType"; }
        }

        public string? GetName()
        {
            return null;
        }

        public override FileNode CreateFileNode(ProjectElement i_ProjectElement)
        {
            var l_ZigVSProjectFileNode = new ZigVSProjectFileNode(this, i_ProjectElement);

            l_ZigVSProjectFileNode.OleServiceProvider.AddService(typeof(EnvDTE.Project), CreateServices, false);
            l_ZigVSProjectFileNode.OleServiceProvider.AddService(typeof(EnvDTE.ProjectItem), l_ZigVSProjectFileNode.ServiceFactory, false);
            l_ZigVSProjectFileNode.OleServiceProvider.AddService(typeof(VSProject), CreateServices, false);

            return l_ZigVSProjectFileNode;
        }

        protected override bool PerformTargetFrameworkCheck()
        {
            return true;
        }

        /// <summary>
        /// Generate new Guid value and update it with GeneralPropertyPage GUID.
        /// </summary>
        /// <returns>Returns the property pages that are independent of configuration.</returns>
        protected override Guid[] GetConfigurationIndependentPropertyPages()
        {
            Guid[] r_GuildArray = new Guid[3];
            r_GuildArray[0] = typeof(PropertyPage_General).GUID;
            r_GuildArray[1] = typeof(PropertyPage_Build).GUID;
            r_GuildArray[2] = typeof(PropertyPage_Debug).GUID;
            return r_GuildArray;
        }

        /// <summary>
        /// Overriding to provide project general property page.
        /// </summary>
        /// <returns>Returns the GeneralPropertyPage GUID value.</returns>
        protected override Guid[] GetPriorityProjectDesignerPages()
        {
            Guid[] r_GuildArray = new Guid[1];
            r_GuildArray[0] = typeof(PropertyPage_General).GUID;
            r_GuildArray[1] = typeof(PropertyPage_Build).GUID;
            r_GuildArray[2] = typeof(PropertyPage_Debug).GUID;
            return r_GuildArray;
        }

        public override void AddFileFromTemplate(string i_sourceString, string i_targetString)
        {
            string l_nameSpaceString = FileTemplateProcessor.GetFileNamespace(i_targetString, this);
            string l_classNameString = Path.GetFileNameWithoutExtension(i_targetString);
            string l_projectNameString =
                CurrentConfig.Properties.Single<ProjectPropertyInstance>(
                        (prop) =>
                            {
                                if (String.Compare(prop.Name, "MSBuildProjectName") == 0)
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                ).EvaluatedValue;//  Properties["MSBuildProjectName"];

            FileTemplateProcessor.AddReplace("$nameSpace$", l_nameSpaceString);
            FileTemplateProcessor.AddReplace("$className$", l_classNameString);
            FileTemplateProcessor.AddReplace("$projectName$", l_projectNameString);

            FileTemplateProcessor.UntokenFile(i_sourceString, i_targetString);
            FileTemplateProcessor.Reset();
        }

        private object CreateServices(Type i_serviceType)
        {
            object? r_serviceObject = null;
            if (typeof(VSLangProj.VSProject) == i_serviceType)
            {
                r_serviceObject = VSProject;
            }
            else if (typeof(EnvDTE.Project) == i_serviceType)
            {
                r_serviceObject = GetAutomationObject();
            }
            return r_serviceObject!;
        }
    }
}