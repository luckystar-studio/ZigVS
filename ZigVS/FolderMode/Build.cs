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
    using System.Collections.Generic;
    using System.IO;

#nullable enable
#pragma warning disable VSTHRD002
    public class Build
    {
        const char c_platformSplitter = '-';
        const char c_configurationSplitter = '|';
        const char c_spacer = ' ';

        public static List<System.String> ConfigurationList()
        {
            List<System.String> r_ConfigurationList = new List<System.String>();

            var l_FolderModelOption = FolderModeOptions.GetLiveInstanceAsync().Result;
            if (l_FolderModelOption != null)
            {
                try
                {
                    string[] l_ArchitectureArray = l_FolderModelOption.ArchitectureList.Split(c_spacer);
                    string[] l_OSArray = l_FolderModelOption.OSList.Split(c_spacer);
                    string[] l_OptimizeArray = l_FolderModelOption.OptimizeList.Split(c_spacer);

                    foreach (var l_Architecture in l_ArchitectureArray)
                    {
                        foreach (var l_OS in l_OSArray)
                        {
                            foreach (var l_Optimize in l_OptimizeArray)
                            {
                                var l_ConfigurationString = l_Architecture + c_platformSplitter + l_OS + c_configurationSplitter + l_Optimize;
                                r_ConfigurationList.Add(l_ConfigurationString);
                            }
                        }
                    }
                }
                catch { }
                if (r_ConfigurationList.Count == 0)
                {
                    r_ConfigurationList = new List<System.String>()
                    {
                        "x86_64-windows|Debug",
                        "x86_64-windows|ReleaseFast",
                        "aarch64-windows|Debug",
                        "aarch64-windows|ReleaseFast",
                        "x86_64-linux|Debug",
                        "x86_64-linux|ReleaseFast",
                        "aarch64-linux|Debug",
                        "aarch64-linux|ReleaseFast"
                    };
                }
            }
            return r_ConfigurationList;
        }

        public static string CreateBuildCommand(
            string i_workingDirectoryString, System.String i_ConfigurationString, string i_commandString = "")
        {
            string l_architectureString = Build.GetArchitecture(i_ConfigurationString);
            string l_modeString = Build.GetMode(i_ConfigurationString);

            return @"build " + i_commandString + " " +
                    @"--verbose " +
                    @"-Dtarget=" + l_architectureString + " " +
                    @"-Doptimize=" + l_modeString + " " +
                    @"--cache-dir " + GetIntermeditatePath(i_workingDirectoryString, i_ConfigurationString) + " " +
                    @"--prefix-exe-dir " + GetOutputPath(i_workingDirectoryString, i_ConfigurationString);
        }

        public static string GetIntermeditatePath(string i_workingDirectoryString, System.String? i_ConfigurationString)
        {
            string r_IntermeditatePath = "";
            var l_FolderModelOption = FolderModeOptions.GetLiveInstanceAsync().Result;
            if (l_FolderModelOption != null)
            {
                r_IntermeditatePath = Path.Combine(
                    i_workingDirectoryString,
                    l_FolderModelOption.IntermediateDirectoryName,
                    ConvertConfigurationToDirectoryName(i_ConfigurationString));
            }
            return r_IntermeditatePath;
        }

        public static string GetOutputPath(string i_workingDirectoryString, System.String? i_ConfigurationString)
        {
            string r_OutputPath = "";
            var l_FolderModelOption = FolderModeOptions.GetLiveInstanceAsync().Result;
            if (l_FolderModelOption != null)
            {
                r_OutputPath = Path.Combine(
                    i_workingDirectoryString,
                    l_FolderModelOption.OutputDirectoryName,
                    ConvertConfigurationToDirectoryName(i_ConfigurationString));
            }
            return r_OutputPath;
        }

        public static string ConvertConfigurationToDirectoryName(System.String? i_ConfigurationString)
        {
            string r_directoryNameString = "";
            if (i_ConfigurationString != null)
            {
                r_directoryNameString = i_ConfigurationString.Replace(c_configurationSplitter, c_platformSplitter);
            }
            return r_directoryNameString;

        }
        public static string GetArchitecture(System.String i_ConfigurationString)
        {
            string[] l_architectureString = i_ConfigurationString.Split(c_configurationSplitter);
            return l_architectureString[0];
        }

        public static string GetMode(System.String i_ConfigurationString)
        {
            string[] l_architectureString = i_ConfigurationString.Split(c_configurationSplitter);
            return l_architectureString[1];
        }
    }
#pragma warning restore VSTHRD002
}