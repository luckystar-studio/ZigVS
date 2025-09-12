# ZigVS

Add support for using the [Zig programming language](https://ziglang.org/) to [Visual Studio](https://learn.microsoft.com/en-us/visualstudio/windows/?view=vs-2022)  

*Note: Please **update Visual Studio** before installing ZigVS. If your version of Visual Studio is older than the required version for ZigVS, a version error will appear during installation.* 
___
  
# Features

* Edit Zig code with [Visual Studio Editor](https://learn.microsoft.com/en-us/visualstudio/ide/index-writing-code?view=vs-2022)
    * Zig language syntax highlighting
    * [Zig Language Server (ZLS)](https://github.com/zigtools/zls) support (completions, tool tips, find references and more)
    * Snippets
    * Zig Document Formatting 
    * Auto-Insert Parentheses/Bracket/Braces
* Folder mode
    * Create a new Zig package from the Visual Studio GUI
    * Build from Visual Studio using build.zig
    * Zig Build System provides [cross-platform build](https://ziglang.org/documentation/0.15.1/#Targets).
    * Application templates
* Project mode
    * Build from Visual Studio using project files (.zigproj)
    * Supports [cross-platform build](https://ziglang.org/documentation/0.15.1/#Targets) (Windows, Linux, Android, iOS, macOS, WASI / x86, x64, arm, arm64, wasm32)
    * Application, library and file templates
    * build.zig and build.zig.zon generation from project file
* Debug Zig code with [Visual Studio Debugger](https://learn.microsoft.com/en-us/visualstudio/debugger/?view=vs-2022)  
    * Support the Windows Native Debug Engine and [MIEngine](https://github.com/microsoft/MIEngine) for cross-platform debugging (Linux, Android, MacOS, iOS) 
* Run code tests using [Visual Studio Testing Tools](https://learn.microsoft.com/en-us/visualstudio/test/?view=vs-2022)
    * Run, debug & manage unit tests from [Test Explorer](https://learn.microsoft.com/en-us/visualstudio/test/run-unit-tests-with-test-explorer?view=vs-2022)
* Supports [Visual Studio Profiler Tools](https://learn.microsoft.com/en-us/visualstudio/profiling/?view=vs-2022)
  * CPU Usage, Memory Usage, Events, File I/O
* Zig Tool-chain Installer
* Zig Package Installer

___
# How to Install ZigVS
1. **Update Visual Studio**  
*If your version of Visual Studio is older than the required version for ZigVS, a version error will appear during installation.*

2. Two ways to get the extension
   a.  Install ZigVS within Visual Studio from **[Extension] → [Manage Extensions…] → [Browse]**
   ![ExtensionManager](ZigVS/Documents/Images/ExtensionManager.png)  
   **OR**
   b. Install ZigVS from [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=LuckystarStudio.ZigVS)
![MarketPlace](ZigVS/Documents/Images/ZigVS_marketplace.png)

___
# How to Set up Zig compiler and ZLS language server manually

1. Install Zig Compiler:  
Download [Zig](https://ziglang.org) and extract it to your desired location.  
ZigVS ver 0.15.1.0 has been tested with [Zig ver 0.15.1](https://ziglang.org/download/0.14.0/zig-windows-x86_64-0.15.1.zip).

2. Install ZLS Language Server:  
Download [Zig Language Server (zls)](https://github.com/zigtools/zls) and extract it to your desired location.

3. Do either:  
    a. Add the directories with the Zig compiler and ZLS binaries to the system **PATH** environment variable.  
**OR**
    b. In Visual Studio enter the paths to zig.exe and zls.exe in the [Tools] → [Options] -> [ZigVS] properties editor (and in the Project's Properties dialog box if using Project Mode).  

![Tool Option Zig General Built Tool](ZigVS/Documents/Images/Tool_Option_Zig_General_BuiltTool_AbsolutePath.png)

![Project Propety General](ZigVS/Documents/Images/ProjectPropety_General_AbsolutePath.png)  

The path can either be an absolute path or it may contain Visual Studio macros or user defined environment variables. For example, if using an environment variable such as ZIG_HOME, then in [Tools] → [Options] or the Project Properties, set the path to zig.exe using that environment variable as $(ZIG_HOME)\zig.exe.

![Tool Option Zig General Built Tool](ZigVS/Documents/Images/Tool_Option_Zig_General_BuiltTool_ZIG_HOME.png)

![Project Propety General](ZigVS/Documents/Images/ProjectPropety_General_ZIG_HOME.png)

___
# How to Set up Zig and ZLS with the Tool-chain installer inside Visual Studio

1. [Extensions] → [ZigVS] → [Zig Tool chain Installer]  

2. Tool Chain Installer window will open  
<br><br>
![Extensions_ZigVS_ToolChainInstaller](ZigVS/Documents/Images/ToolChainInstaller.png)  
3. Select Zig Tool chain Version
4. Select ZLS Version
5. Select CPU Architecture
6. Choose Installation Directory  
   If you specify a folder without write permissions, the Install button will remain disabled.  Select a different folder or adjust folder or user permissions.  

7. Make a selection to either modify the PATH variable to include the install directory, to set the ZIG_HOME variable to the install directory or to not modify any environment variables.  
  
   If you select 'Do not set', make sure to update the Tool Path in [Tools] → [Options] and in the Project Properties.

![Tool Option Zig General Built Tool](ZigVS/Documents/Images/Tool_Option_Zig_General_BuiltTool_AbsolutePath.png)

![Project Propety General](ZigVS/Documents/Images/ProjectPropety_General_AbsolutePath.png)

<br><br>
If you want to use an environment variable instead of modifying 'PATH', select "Set 'ZIG_HOME'" and add $(ZIG_HOME) in the [tool path] propery in [Tools] → [Options] and in the Project Properties

![Tool Option Zig General Built Tool](ZigVS/Documents/Images/Tool_Option_Zig_General_BuiltTool_ZIG_HOME.png)

![Project Propety General](ZigVS/Documents/Images/ProjectPropety_General_ZIG_HOME.png)
<br><br>
  
8. Once everything is set up, the Install button will become active, so press the Install button  

9. The progress and results of the installation will be displayed in the **[output window]**  
<br>

![Extensions_ZigVS_ToolChainInstaller](ZigVS/Documents/Images/ToolChainInstaller_output.png)

**Note**: If you see "Access to the path '....' is denied", the following scenarios might be considered  
    * File in use: Please close the application that has the file open  
    * Directory without write permission: Change user or folder permissions. (Not recommended: Run Visual Studio in Administrator mode to elevate permissions.)
<br><br>
___

# Project Mode and Folder Mode  

**Visual Studio has two modes: Project mode and Folder mode.**

* **Project mode** is used when developing in C++, C#, F#, and similar languages. You open project file types such as .cppproj, .csproj and .fsproj. The Solution Explorer tree shows only the items that have been added to the project by default. Builds are not performed directly by Visual Studio but by a build tool called MSBuild. Project files can be built without Visual Studio. A “project” corresponds to what Zig calls a package. Although you can edit project files in a text editor, you can also configure them via Visual Studio's Project Properties GUI. A solution file can contain multiple project files, allowing you to coordinate several projects to create a larger application.

 * **Folder mode** supports languages that are not built via project files. As in Visual Studio Code, you open a folder, and the file tree shows all files under that folder. Builds are performed by each language's own build system.

<br>

**ZigVS supports both Project mode and Folder mode.**

* In ZigVS's Project mode, you use a project file type called .zigproj. To start a project, create a .zigproj; to open the project, open the .zigproj. When you build the project (even from Visual Studio's GUI), MSBuild reads the .zigproj and builds using zig.exe as the compiler and linker.

  * The benefits of Zig's Project mode are:
    a) It makes integration with other projects easy. If you already have C++ or C# projects, using a Zig project file makes it easier to incorporate Zig code into existing codebases. For example, most commercially released games today are made with Visual Studio and this approach is useful when you want to use some Zig code within such existing projects.
    b) You can change build settings via the Visual Studio GUI.
    c) Developers who have worked on Microsoft's platform only need to learn the Zig language itself; they can continue to create new Zig projects, debug, profile performance, and run tests via the GUI as before—without having to learn Zig's command-line tools or how to write build.zig.

  * You can also generate a build.zig from a .zigproj. Because a project file is a structured data file with a fixed schema, conversion is possible. By contrast, build.zig is a program rather than a schema-defined data file, so understanding its contents requires compilation.

* In ZigVS's Folder mode a build.zig file is used. As in Visual Studio Code, open a folder that contains build.zig and set build.zig as the build file. To change the build, edit build.zig. You can still create new packages, build, debug, perform performance profiling, and run tests via the GUI just like with other projects. 

<br><br>
___
# How to open an existing Zig package

1. Select [File] → [Open] → [Folder]  
2. When the dialog appears, choose the folder that contains build.zig.  
3. In Solution Explorer, folders and files will be displayed.    
4. Simply opening a folder does not allow you to build or debug.


![](ZigVS/Documents/Images/Set_as_Startup_Item_1.png)  


5. Select build.zig with the mouse, **right-click** it, and choose **[Set as Startup Item]** from the context menu.

![Set As Startup Item_2](ZigVS/Documents/Images/Set_as_Startup_Item_2.png)


6. The Build menu will appear, the Debug button will be enabled, and you will be able to perform builds and debugging.

![Set As Startup Item_3](ZigVS/Documents/Images/Set_as_Startup_Item_3.png)

<br><br>
___
# How to create a new zig package

1. Select [File] → [New] → [Zig Package (zig.exe init)]

![](ZigVS/Documents/Images/New_ZigPackage.png)

2. The Package Creator dialog will appear.
3. Set new package Directory  
4. Set the package name
5. Choose whether to open folder mode immediately after the package is created.
6. The Create button will become enabled, so press the button.  

![](ZigVS/Documents/Images/PackageCreator.png)  

7. The result will be displayed in the Output window.

8. Simply opening a folder does not allow you to build or debug.


![](ZigVS/Documents/Images/Set_as_Startup_Item_1.png)  


9. Select build.zig with the mouse, **right-click** it, and choose **[Set as Startup Item]** from the context menu.

![Set As Startup Item_2](ZigVS/Documents/Images/Set_as_Startup_Item_2.png)


10. The Build menu will appear, the Debug button will be enabled, and you will be able to perform builds and debugging.

![Set As Startup Item_3](ZigVS/Documents/Images/Set_as_Startup_Item_3.png)

<br><br>
___

# How to create a new Zig Project

1. Select [File] → [New] → [Project]

2. The New Project wizard will appear.

3. Select Zig in the language filter.  

4. Select a Zig template and use the Wizard to create the project.  

![](ZigVS/Documents/Images/New_ZigProject.png)

5. The [Build] menu, [Debug] menu, [Start] button, and [Configuration] selection pull-down are enabled and you can start working.

![](ZigVS/Documents/Images/ProjectModeBuildDebug.png)

6. **To change settings**, open the project Property Pages.

![Project Propery Build](ZigVS/Documents/Images/ProjectPropery_Build.png)

___
# Cross-Platform Build & Debugging

[See User Manual Page](https://luckystar-studio.github.io/ZigVS-Web/)
___
# Profiler

1. [Debug] > [Start Debugging] (or Start on the toolbar, or F5).

   When the app finishes loading, the Summary view of the Diagnostics Tools appears. If you need to open the window, click Debug > Windows > Show Diagnostic Tools.

2. When you choose Record CPU Profile, Visual Studio will begin recording your functions and how much time they take to execute. You can only view this collected data when your application is halted at a breakpoint.

![](ZigVS/Documents/Images/Profiler.png)

3. Read [Visual Studio Profiler Tools](https://learn.microsoft.com/en-us/visualstudio/profiling/?view=vs-2022)

<br><br>
___
# Testing
To perform tests, select [Test] → [Test Explorer] to open the Test Explorer window. If .zig source code includes unit tests, their filenames will be listed.

![](ZigVS/Documents/Images/TestExplorer.png)

<br><br>
___
# Syntax Highlighting

Example of changing Literals and Operators  

![](ZigVS/Documents/Images/TextHighLight.png)

You can change the colors and fonts of the Syntax Highlighting. It is possible to create a more color-coded and visually appealing screen than the default settings of Visual Studio. The following Display Items can be modified.  

* Plain Text
* Comment
* Keyword
* Literal
* Operator
* String
* Type
* Inlay Hints - Parameters
* Inlay Hints - Types


[Tool]→[Environment]→[Fonts and Colors]→[Display items]  

![](ZigVS/Documents/Images/Tool_Option_Environment_FontsAndColors.png)


___
# Editor Setting

[Tools]→[Options]→[Text Editor]→[Zig]

![](ZigVS/Documents/Images/Tool_Option_TextEditor_Zig.png)
![Tool Option Text Editor Zig General](ZigVS/Documents/Images/Tool_Option_TextEditor_Zig_General.png)

![Tool Option Text Editor Zig. Scroll Bar](ZigVS/Documents/Images/Tool_Option_TextEditor_Zig._ScrollBar.png)

![Tool Option Text Editor Zig Tabs](ZigVS/Documents/Images/Tool_Option_TextEditor_Zig_Tabs.png)

![Tool Option Text Editor Zig Advanced](ZigVS/Documents/Images/Tool_Option_TextEditor_Zig_Advanced.png)

<br><br>
___
# Inlay hints

* There is a switch for changing the display method under [Tools] → [Options] → [Text Editor] → [Inlay Hints].  


![Tool Option Text Editor Inlay Hints](ZigVS/Documents/Images/Tool_Option_TextEditor_Inlay_Hints.png)

<br><br>
___
# Formatting
Right-click in the solution explorer and select 'zig fmt'  

![](ZigVS/Documents/Images/zig_fmt.png)  

<br><br>
___
# Snippets

* Open Snippet manager  
[Tool] → [Code Snippets Manager] → [Zig]

![](ZigVS/Documents/Images/CodeSnippetsManager.png)

* Insert Snippets  
Right-click in the code editor → [Snippets] → [Insert Snippet]

![](ZigVS/Documents/Images/SnippetsContextMenu.png)

Note: [Code snippets: what they are and how to add one](https://learn.microsoft.com/en-us/visualstudio/ide/code-snippets?view=vs-2022)

<br><br>

___
# Package Installer

1, [Extensions] → [ZigVS] → [Zig Package Installer]  

![](ZigVS/Documents/Images/Extensions_ZigVS_PackageInstaller.png)

2, Browse to the repository and branch that you want to install in the WebView.  
3, Select a Installation Method from the drop down.  
   Currently, zig fetch, git and unzip are working correctly.  
4. Once everything is set up, the Install button will become active, so press the Install button  

![](ZigVS/Documents/Images/PackageInstaller.png)

<br><br>
___
# Help

a. See [User Manual Page](https://luckystar-studio.github.io/ZigVS-Web/)

or 

b. [Extensions] → [ZigVS] → [User Manual Page]  
![](ZigVS/Documents/Images/Extensions_ZigVS_UserManualPage.png)

<br><br>
___
# Questions, Requests, etc.

* [ZigVS Visual Studio Macketplace](https://marketplace.visualstudio.com/items?itemName=LuckystarStudio.ZigVS)  
  * [Q&A Page](https://marketplace.visualstudio.com/items?itemName=LuckystarStudio.ZigVS&ssr=false#qna)  
  * [Rating & Review Page](https://marketplace.visualstudio.com/items?itemName=LuckystarStudio.ZigVS&ssr=false#review-details)

* [ZigVS Github](https://github.com/luckystar-studio/ZigVS)  
  * [Issues Page](https://github.com/luckystar-studio/ZigVS/issues)

___
# License
Refer to the LICENSE.txt file for licensing information.

___
# Extension Name
ZigVS

___
# Publisher
LuckyStar Studio LLC  

___
# Version History
```
    Version 0.15.1.0 (2025/09/10):
        Added support for Zig 0.15.1 and ZLS 0.15.0
            It is no longer mandatory to add the directory containing Zig.exe to the PATH environment variable. (See the documentation for details.)
        ZLS
            ZLS settings under [Tools] → [Options] now take effect immediately.
            Due to issues in both ZLS and Visual Studio, Inlay Hints sometimes did not work; we've implemented fixes/workarounds to address this as much as possible.
            The Inlay Hints toggle is now Alt + F1, the same as C#.
            Inlay Hints settings are now more granular and configurable.
        Fixes
            Fixed cases where the Toolchain Installer and Package Creator were hard to see under certain themes.
            Fixed an issue where Debug settings on the project property page were not applied.
            Fixed an issue where the project property page did not resize with the window.
            Fixed an issue where the Toolchain Installer created unnecessary duplicate entries in the user's PATH.
        Improvements
            You can now use environment variables and Visual Studio macros in [Tools] → [Options] and in project properties.
            Added [Tools] → [Options] → [Text Editor] → [Zig] → [Advanced] to hold editor option settings.

        Compatibility:
            Visual Studio 2022 17.14.14
            Zig Tool chain: zig-windows-x86_64-0.15.1.zip
            ZLS Language Server: zls-windows-x86_64-0.15.0.zip
            (tested briefly in Visual Studio 2026 and appears to work ok)

   Version 0.14.1.1 (2025/06/16):
        Fixed a bug where incomplete support for the Zig 0.14 spec changes meant the output file name wasn't correctly retrieved from Build.zig, preventing debugging from starting.
        Added 14 code snippets.
        You can now open the ZigVS GitHub page from the menu.

        Compatibility:
        Visual Studio 2022 17.14.3
        Zig Tool chain: zig-windows-x86_64-0.14.1.zip
        ZLS Language Server: zls-windows-x86_64-0.14.0.zip

    Version 0.14.1.0 (2025/06/16):
        Supported Zig 0.14.1.
        Modifications to the file and folder layout to prepare the ZigVS source code for public release. https://github.com/luckystar-studio/ZigVS

        Compatibility:
        Visual Studio 2022 17.14.3
        Zig Tool chain: zig-windows-x86_64-0.14.1.zip
        ZLS Language Server: zls-windows-x86_64-0.14.0.zip

    Version 0.14.0.1 (2025/03/07):
        Supported Zig 0.14.0.
        The compatibility issue between Visual Studio 2022 and ZLS has been resolved, so there is no longer a need to use a special version of ZLS.

        Compatibility:
        Visual Studio 2022 17.13.2
        Zig Tool chain: zig-windows-x86_64-0.14.0.zip
        ZLS Language Server: zls-windows-x86_64-0.14.0.zip

    Version 0.13.0.13 (2025/02/22):
        We modified the behavior so that if the Output window is not displayed when running the tool-chain or package installer, it will be shown automatically. This ensures that progress can be monitored.
        Automatically insert the corresponding closing character when pressing (, {, or [ under certain conditions.

        Compatibility:
        Visual Studio 2022 17.13.1
        Zig Tool chain: zig-windows-x86_64-0.13.0.zip
        ZLS Language Server: 0.13.0.VisualStudioCompatible

    Version 0.13.0.12 (2024/08/17):
        The user manual for ZigVS, which was included in the Visual Studio extension package,
        has been removed from the package to reduce its size by hosting it on GitHub.
        Updated the packages that ZigVS depends on.
        Corrected some spelling errors.
        Fixed minor bugs.

        Compatibility:
        Visual Studio 2022 17.11.0
        Zig Tool chain: zig-windows-x86_64-0.13.0.zip
        ZLS Language Server: 0.13.0.VisualStudioCompatible

    Version 0.13.0.11 (2024/08/15):
        In the previous version of ZigVS, unit tests could only be managed and executed on 
        a file-by-file basis from the Test Explorer. Now, they can be managed and executed individually.
        Additionally, it is now possible to debug unit tests from the Test Explorer.

        Compatibility:
        Visual Studio 2022 17.10.5
        Zig Tool chain: zig-windows-x86_64-0.13.0.zip
        ZLS Language Server: 0.13.0.VisualStudioCompatible

    Version 0.13.0.10 (2024/08/11):
        You can now toggle inlay hints on and off using Alt + F1.
        We fixed the issue where typing '_' would prematurely end the completion.

        Compatibility:
        Visual Studio 2022 17.10.5
        Zig Tool chain: zig-windows-x86_64-0.13.0.zip
        ZLS Language Server: 0.13.0.VisualStudioCompatible

    Version 0.13.0.9 (2024/07/08):
        In Folder mode
            The debugger and tests now function correctly even when you open a folder higher than the one containing build.zig.
            We have accelerated the startup of the debugger from the second time onward.

        Compatibility:
        Visual Studio 2022 17.10.3
        Zig Tool chain: zig-windows-x86_64-0.13.0.zip
        ZLS Language Server: 0.13.0.VisualStudioCompatible
        
    Version 0.13.0.7 (2024/07/07):
        Due to a bug in the extension installer, an exception occurred when creating a new project unless the user manually installed Desktop Development with C++. We have fixed the installer to ensure that the necessary components are installed automatically.
        We have improved the error messages to make the situation clearer when zig.exe or zls.exe cannot be found.
        We have made the behavior of the key to confirm auto-complete similar to C#.

        Compatibility:
        Visual Studio 2022 17.10.3
        Zig Tool chain: zig-windows-x86_64-0.13.0.zip
        ZLS Language Server: 0.13.0.VisualStudioCompatible
    
    Version 0.13.0.3 (2024/06/18):
        We have fixed the issue where you had to type the key twice for auto complete.

        Compatibility:
        Visual Studio 2022 17.10.1
        Zig Tool chain: zig-windows-x86_64-0.13.0.zip
        ZLS Language Server: 0.13.0.VisualStudioCompatible
        
    Version 0.13.0.2 (2024/06/17):
        You can now create a new Zig package by going to [File] → [New] → [Zig Package].
        The project template now includes build.zig, so the project can be built from both build.zig and .zigproj files.

        Compatibility:
        Visual Studio 2022 17.10.1
        Zig Tool chain: zig-windows-x86_64-0.13.0.zip
        ZLS Language Server: 0.13.0.VisualStudioCompatible
        
    Version 0.13.0.1 (2024/06/11):
        We have released the compatible ZIGVS version due to the release of ZIG and ZLS version 0.13.0.
        We have added support for building WebAssembly (WASM).
        We have modified the message for cases where tests are skipped due to compilation errors.
        We have improved the icons and logos.

        Compatibility:
        Visual Studio 2022 17.10.1
        Zig Tool chain: zig-windows-x86_64-0.13.0.zip
        ZLS Language Server: 0.13.0.VisualStudioCompatible
        
    Version 0.12.0.4 (2024/05/26):
        1, The project file generated for new projects now supports incremental builds. This prevents unnecessary zig builds.  
        2, The UI for setting modules in the project properties has been changed to allow multi-line editing instead of a single line.  
        3, The 'zig fetch' command now runs correctly for package installation through the package installer. Previously, the command was given a GitHub zip file, but zig couldn't handle the zip properly. Switching to .tar.gz resolved the issue.

        Compatibility:
        Visual Studio 2022 17.10.0
        Zig Tool chain: zig-windows-x86_64-0.12.0.zip
        ZLS Language Server: 0.12.0.VisualStudioCompatible

     Version 0.12.0.3 (2024/05/23):
        We have added an explanation on how to further color-code using Syntax Highlighting.
        To fix the issue with ZLS not functioning correctly, the recommended setup method for the Zig Tool chain has been changed from using the ZIG_TOOL_PATH environment variable to using the PATH environment variable. Consequently, the installer has also been modified to set the PATH.

        Compatibility:
        Visual Studio 2022 17.10.0
        Zig Tool chain: zig-windows-x86_64-0.12.0.zip
        ZLS Language Server: 0.12.0.VisualStudioCompatible

     Version 0.12.0.2 (2024/05/13):
        In ZigVS unit tests, we use the 'zig.exe test' command,
        but the result output format of the test command was changed in Zig 0.12.0.
        We have fixed cases where ZigVS could not correctly retrieve the test results.
        Additionally, if the output of 'zig.exe test' changes in the future and ZigVS can no longer read it,
        an error message will now be displayed to indicate this.

        Compatibility:
        Visual Studio 2022 17.9.6
        Zig Tool chain: zig-windows-x86_64-0.12.0.zip
        ZLS Language Server: 0.12.0.VisualStudioCompatible

     Version 0.12.0.1 (2024/05/07):
        We have released the compatible ZIGVS version due to the release of ZIG and ZLS version 0.12.0.
        Replace zig logo

        Compatibility:
        Visual Studio 2022 17.9.6
        Zig Tool chain: zig-windows-x86_64-0.12.0.zip
        ZLS Language Server: 0.12.0.VisualStudioCompatible
        
    Version 0.1.8 (2024/03/23):
        cross-platform build and debug
        fix a bug ZigVS disables CMake Project Debugger

        Compatibility:
        Visual Studio 2022 17.9.3
        Zig Tool chain: zig-windows-x86_64-0.12.0-dev.1849+bb0f7d55e.zip
        ZLS Language Server: 0.12.0.VisualStudioCompatible
        
    Version 0.1.6 (2024/02/24):
        Add Package Installer

        Compatibility:
        Visual Studio 2022 17.9
        Zig Tool chain: zig-windows-x86_64-0.12.0-dev.1849+bb0f7d55e.zip
        ZLS Language Server: 0.12.0.VisualStudioCompatible

    Version 0.1.5 (2024/01/28):
        Support Visual Studio Profiling Tools
        Minor Bug fix

        Compatibility:
        Visual Studio 2022 17.8.5
        Zig Tool chain: zig-windows-x86_64-0.12.0-dev.1849+bb0f7d55e.zip
        ZLS Language Server: 0.12.0.VisualStudioCompatible

    Version 0.1.4 (2024/01/21):
        Add Snippets Manager

    Version 0.1.3.1 (2024/01/14):
        ToolChainInstaller
            Check if install path already exists
            Extract empty directories during install

    Version 0.1.3 (2024/01/14):
        Project Property
            Header files directory field handles both relative and absolute paths
            Add library list and library directory fields
            Implemented modules and dependencies field
        Fix an issue where the User Manual does not appear in some cases
        Add Zig Tool chain Installer Window

    Version 0.1.2 (2024/01/07):
        Add file icon
        Support document formatting

    Version 0.1.1 (2023/12/30):
        Fixed not being able to specify the output destination in project mode
        Add editor settings

    Version 0.1 (2023/12/26):  
        Initial release

        Compatibility:
        Zig Tool chain: zig-windows-x86_64-0.12.0-dev.1849+bb0f7d55e.zip
        ZLS Language Server: 0.12.0.VisualStudioCompatible
```
