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
    using Microsoft.VisualStudio.Workspace;
    using Microsoft.VisualStudio.Workspace.Build;
    using Microsoft.VisualStudio.Workspace.Debug;
    using Microsoft.VisualStudio.Workspace.Indexing;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using ZigVS.Test;

    // Read https://learn.microsoft.com/en-us/visualstudio/extensibility/workspace-indexing?view=vs-2022

    public class FileScanner : IFileScanner
    {
        public FileScanner() { }

        public async Task<T> ScanContentAsync<T>(string i_filePathString, CancellationToken i_CancellationToken)
            where T : class
        {
            if (typeof(T) == FileScannerTypeConstants.FileDataValuesType)
            {
                // Insert file name into TestContainerDiscoverer
                TestContainerDiscoverer.AddSource(i_filePathString);

                var l_FileDataValueList = GetFileDataValues(i_filePathString);
                return await Task.FromResult((T)(IReadOnlyCollection<FileDataValue>)l_FileDataValueList);
            }
            else if (typeof(T) == FileScannerTypeConstants.FileReferenceInfoType)
            {
                var l_FileDataValueList = GetFileReferenceInfos(i_filePathString);
                return await Task.FromResult((T)(IReadOnlyCollection<FileReferenceInfo>)l_FileDataValueList);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private List<FileDataValue> GetFileDataValues(string i_filePathString)
        {
            var r_FileDataValueList = new List<FileDataValue>();

            if (Path.GetFileName(i_filePathString) == Parameter.c_buildFileName)
            {
                string l_directoryName = Path.GetFileName(Path.GetDirectoryName(i_filePathString).TrimEnd(Path.DirectorySeparatorChar));

                var l_PropertySettings = new PropertySettings
                {
                    [LaunchConfigurationConstants.NameKey] = "[" + l_directoryName + "]" + Parameter.c_buildFileName,   // this is used at launch button UI text
                    [LaunchConfigurationConstants.DebugTypeKey] = LaunchConfigurationConstants.NativeOptionKey,
                    [LaunchConfigurationConstants.ProjectKey] = i_filePathString,
                    [LaunchConfigurationConstants.ProjectTargetKey] = Parameter.c_languageName,
                    [LaunchConfigurationConstants.ProgramKey] = "e",
                };

                r_FileDataValueList.Add(
                    new FileDataValue(
                        type: DebugLaunchActionContext.ContextTypeGuid,
                        name: DebugLaunchActionContext.IsDefaultStartupProjectEntry,
                        value: l_PropertySettings,
                        target: null,
                        context: null));

                foreach (var l_configuration in Build.ConfigurationList())
                {
                    r_FileDataValueList.Add(new FileDataValue(
                                            type: BuildConfigurationContext.ContextTypeGuid,
                                            name: BuildConfigurationContext.DataValueName,
                                            value: null,
                                            target: l_configuration,
                                            context: l_configuration));
                    r_FileDataValueList.Add(new FileDataValue(
                                            type: BuildConfigurationContext.ContextTypeGuid,
                                            name: BuildConfigurationContext.DataValueName,
                                            value: null,
                                            target: null,
                                            context: l_configuration));
                }
            }
            return r_FileDataValueList;
        }

        private static List<FileReferenceInfo> GetFileReferenceInfos(string i_FilePathString)
        {
            return new List<FileReferenceInfo>();
        }
    }
}