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
    using Microsoft.VisualStudio.Workspace.Extensions.Build;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    // Read https://learn.microsoft.com/en-us/visualstudio/extensibility/workspace-file-contexts?view=vs-2022
    // Read https://learn.microsoft.com/en-us/visualstudio/extensibility/workspace-build?view=vs-2022

    public sealed class FileContextProvider : IFileContextProvider, IFileContextProvider<string>
    {
        public FileContextProvider() { }

        public Task<IReadOnlyCollection<FileContext>> GetContextsForFileAsync(string i_filePath, string i_context, CancellationToken i_CancellationToken)
        {
            return GetContextsForFileAsync(i_filePath, i_CancellationToken);
        }

        public async Task<IReadOnlyCollection<FileContext>> GetContextsForFileAsync(string i_filePath, CancellationToken cancellationToken)
        {
            var l_fileContextList = new List<FileContext>();

            var l_FolderModelOption = await FolderModeOptions.GetLiveInstanceAsync();
            if (l_FolderModelOption != null)
            {
                if (Path.GetFileName(i_filePath).Equals(Parameter.c_buildFileName))
                {
                    var l_toolPathString = Utilities.GetToolPathFromEnvironmentValue();

                    foreach (var l_configuration in Build.ConfigurationList())
                    {
                        {   // build command
                            string l_commandOptionString = Build.CreateBuildCommand(
                                Path.GetDirectoryName(i_filePath),
                                l_configuration,
                                l_FolderModelOption.BuildCommand_build);
                            var l_LaunchCommand = new LaunchCommand(
                                Path.Combine(l_toolPathString, l_FolderModelOption.ToolPath),
                                l_commandOptionString,
                                LaunchCommandOption.None,
                                workingDirectory: Path.GetDirectoryName(i_filePath),
                                projectFullPath: i_filePath);

                            var l_BuildActionContext = new BuildActionContext(new LaunchCommand[] { l_LaunchCommand }, l_configuration);

                            l_fileContextList.Add(
                                new FileContext(
                                    FileContextProviderFactory.s_ProviderTypeGuid,
                                    BuildContextTypes.BuildContextTypeGuid,
                                    l_BuildActionContext,
                                    new string[] { i_filePath }
                                )
                            );
                        }
                        {   // build clean command
                            string l_commandOptionString = Build.CreateBuildCommand(
                                Path.GetDirectoryName(i_filePath),
                                l_configuration,
                                l_FolderModelOption.BuildCommand_clean);
                            var l_LaunchCommand = new LaunchCommand(
                                Path.Combine(l_toolPathString, l_FolderModelOption.ToolPath),
                                l_commandOptionString,
                                LaunchCommandOption.None,
                                workingDirectory: Path.GetDirectoryName(i_filePath),
                                projectFullPath: i_filePath);

                            var l_BuildActionContext = new BuildActionContext(new LaunchCommand[] { l_LaunchCommand }, l_configuration);

                            l_fileContextList.Add(
                                new FileContext(
                                    FileContextProviderFactory.s_ProviderTypeGuid,
                                    BuildContextTypes.CleanContextTypeGuid,
                                    l_BuildActionContext,
                                    new string[] { i_filePath }
                                )
                            );
                        }
                    }
                    return await Task.FromResult(l_fileContextList.ToArray());
                }

            }
            return FileContext.EmptyFileContexts;
        }
    }
}