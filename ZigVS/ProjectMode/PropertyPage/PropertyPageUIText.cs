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
    using System.Reflection;
    using System.Globalization;
    using System.Resources;
    using System.Threading;

    internal sealed class PropertyPageUIText
	{
        #region Constants
  
		internal const string Category_Tool = "Tool";
        internal const string ToolName = "Tool Name";
        internal const string ToolNameCaption = "Compiler Name (ex:zig.exe)";

        internal const string Category_Assembly = "Assembly";
        internal const string AssemblyName = "Name";
		internal const string AssemblyNameDescription = "Assembly Name";
        internal const string AssemblyVersion = "Version";
        internal const string AssemblyVersionDescription = "Assembly Version";
        internal const string OSType = " OS";
        internal const string OSTypeDescription = "OS Type";
        internal const string ConfigurationType = "Type";
		internal const string ConfigurationTypeDescription = "Assembly Type";
        internal const string OutputFile = "OutputFile";
        internal const string OutputFileDescription = "OutputFileDescription";

        internal const string Category_Build = "Build";
        internal const string UseBuildDotZig = "Use Build.zig";
        internal const string UseBuildDotZigDescription = "Use Build.zig";
        internal const string DefaultNamespace = "DefaultNamespace";
		internal const string DefaultNamespaceDescription = "DefaultNamespace";
        internal const string RootSourceName = "Root Source Name";
        internal const string RootSourceNameDescription = "Root Source File Name";
        internal const string IncludeDirs = "Header file directories";
        internal const string IncludeDirsDescription = "-I Option. c/cpp header file directories. semicolon ; separator";
        internal const string LibraryDirs = "Library file directories";
        internal const string LibraryDirsDescription = "-L Option. library file directories. semicolon ; separator";
        internal const string Libraries = "Library files";
        internal const string LibrariesDescription = "-l Option. library files. semicolon ; separator";
        internal const string BuildOption = "Build Option";
        internal const string BuildOptionCaption = "Build Option";

        internal const string Category_PreDebug = "1) PreDebug";
        internal const string PreDebugCommand = "Pre Debug Command";
        internal const string PreDebugCommandDescription = "Pre Debug Command";
        internal const string PreDebugCommandArguments = "Pre Debug Command Arguments";
        internal const string PreDebugCommandArgumentsDescription = "Pre Debug Command Arguments";

        internal const string Category_Debug = "2) Debug";
        internal const string WorkingDirectory = "Working Directory";
        internal const string WorkingDirectoryDescription = "Working Directory";
        internal const string StartupProgram = "StartupObject";
        internal const string StartupProgramDescription = "StartupObjectDescription";
        internal const string CommandLineArguments = "Command Line Arguments";
        internal const string CommandLineArgumentsDescription = "Command Line Arguments";
        internal const string DebugEngineType = "Debug Engine";
        internal const string DebugEngineTypeDescription = "Debug Engine";
        internal const string MIEngineLaunchOptions = "MIEngine Launch Option File";
        internal const string MIEngineLaunchOptionsDescription = "See https://github.com/Microsoft/MIEngine";    
        internal const string RemoteDebugMachine = "Remote Debug Machine";
        internal const string RemoteDebugMachineDescription = "Remote Debug Machine (IP Address)";

        internal const string Category_Directory = "Directory";
        internal const string IntDirName = "Intermediate Directory Name";
        internal const string IntDirNameCaption = "Intermediate Directory Name ";
        internal const string OutDirName = "Output Directory Name";
        internal const string OutDirNameCaption = "Output Directory Name";

        internal const string Category_Dependency = "Dependency";
        internal const string Modules = "Modules";
        internal const string ModulesDescription = "--mod Option. semicolon ; separator";
        internal const string Dependencies = "Dependencies";
        internal const string DependenciesDescription = "--deps option. semicolon ; separator";

        internal const string Category_Generation = "Generation";
        internal const string GenerateBuildDotZig = "Build.zig";
        internal const string GenerateBuildDotZigDescription = "Generate Build.zig";
        internal const string GenerateBuildDotZigDotZon = "Build.zig.zon";
        internal const string GenerateBuildDotZigDotZonDescription = "Generate Build.zig.zon";
/*
        internal const string NestedProjectFileAssemblyFilter = "NestedProjectFileAssemblyFilter";
        //internal const string MsgFailedToLoadTemplateFile = "Failed to add template file to project";
        internal const string ReloadPromptOnTargetFxChanged = "ReloadPromptOnTargetFxChanged";
        internal const string ReloadPromptOnTargetFxChangedCaption = "ReloadPromptOnTargetFxChangedCaption";

        internal const string ProjectFile = "ProjectFile";
        internal const string ProjectFileDescription = "ProjectFileDescription";
        internal const string ProjectFolder = "ProjectFolder";
        internal const string ProjectFolderDescription = "ProjectFolderDescription";
        internal const string TargetFrameworkMoniker = "TargetFrameworkMoniker";
        internal const string TargetFrameworkMonikerDescription = "TargetFrameworkMonikerDescription";
*/

        #endregion Constants

        #region Fields
        private static PropertyPageUIText? loader;
		private ResourceManager resourceManager;
		private static Object? internalSyncObjectInstance;
		#endregion Fields

		#region Constructors
		/// <summary>
		/// Internal explicitly defined default constructor.
		/// </summary>
		internal PropertyPageUIText()
		{
			resourceManager = new System.Resources.ResourceManager("ZigVS.Resource",
				Assembly.GetExecutingAssembly());
		}
		#endregion Constructors

		#region Properties
		/// <summary>
		/// Gets the internal sync. object.
		/// </summary>
		private static Object InternalSyncObject
		{
			get
			{
				if(internalSyncObjectInstance == null)
				{
					Object o = new Object();
					Interlocked.CompareExchange(ref internalSyncObjectInstance, o, null);
				}
				return internalSyncObjectInstance;
			}
		}
		/// <summary>
		/// Gets information about a specific culture.
		/// </summary>
		private static CultureInfo Culture
		{
			get { return CultureInfo.CurrentUICulture; }// null/*use ResourceManager default, CultureInfo.CurrentUICulture*/; }
		}

		/// <summary>
		/// Gets convenient access to culture-specific resources at runtime.
		/// </summary>
		public static ResourceManager ResourceManager
		{
			get
			{
				return GetLoader().resourceManager;
			}
		}
		#endregion Properties

		#region Public Implementation
		/// <summary>
		/// Provide access to resource string value.
		/// </summary>
		/// <param name="name">Received string name.</param>
		/// <param name="args">Arguments for the String.Format method.</param>
		/// <returns>Returns resources string value or null if error occurred.</returns>
		public static string? GetString(string name, params object[] args)
		{
			PropertyPageUIText resourcesInstance = GetLoader();
			if(resourcesInstance == null)
			{
				return null;
			}
			string res = resourcesInstance.resourceManager.GetString(name, PropertyPageUIText.Culture);

			if(args != null && args.Length > 0)
			{
				return String.Format(CultureInfo.CurrentCulture, res, args);
			}
			else
			{
				return res;
			}
		}
		/// <summary>
		/// Provide access to resource string value.
		/// </summary>
		/// <param name="name">Received string name.</param>
		/// <returns>Returns resources string value or null if error occurred.</returns>
		public static string GetString(string name)
		{
			string r_ResourceString = name;

			try
			{
                PropertyPageUIText resourcesInstance = GetLoader();
                if (resourcesInstance != null)
                {
//                    r_ResourceString = resourcesInstance.resourceManager.GetString(name, Resource.Culture);
                }
            }
			catch { }
			return r_ResourceString;
        }

		/// <summary>
		/// Provide access to resource object value.
		/// </summary>
		/// <param name="name">Received object name.</param>
		/// <returns>Returns resources object value or null if error occurred.</returns>
		public static object? GetObject(string name)
		{
			PropertyPageUIText resourcesInstance = GetLoader();

			if(resourcesInstance == null)
			{
				return null;
			}
			return resourcesInstance.resourceManager.GetObject(name, PropertyPageUIText.Culture);
		}
		#endregion Methods

		#region Private Implementation
		private static PropertyPageUIText GetLoader()
		{
			if(loader == null)
			{
				lock(InternalSyncObject)
				{
					if(loader == null)
					{
						loader = new PropertyPageUIText();
					}
				}
			}
			return loader;
		}
		#endregion
	}
}