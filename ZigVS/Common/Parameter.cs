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
    using System;

    public class Parameter
    {
        public const string c_PackageNameStrig = "ZigVS";
        //   public const string c_PackageGuidString = "3fd9286d-949f-4d6e-a11a-1493feb6f915";

        public const string c_languageName = "zig";
        public const string c_fileExtension = ".zig";
        public const string c_compilerFileName = "zig.exe";
        public const string c_buildFileName = "build.zig";

        public const string c_LanguageGuid = "024D2A59-EEE2-4F2A-85C0-0560D7996063";

        public const string c_ProjectFactoryGuidString = "98E5F03A-0B5F-41B0-A3FA-DF2B47B954C1";
        public static readonly Guid s_ProjectFactoryGuid = new Guid(c_ProjectFactoryGuidString);

     //   public const string c_CompanyName = "LuckyStar Studio LLC";
        public const string c_PATH_EvnironmentVariable_NameString = "PATH";
        public const string c_ToolPath_EvnironmentVariable_NameString = "ZIG_TOOL_PATH";

        public const string c_TestExecutorUriString = "executor://" + c_PackageNameStrig;
        public static Uri c_TestExecutorUri = new Uri(c_TestExecutorUriString);


        public const string c_windowsExecutableFileExtension = ".exe";
        public const string c_linuxExecutableFileExtension = ".bin";
    }
}
