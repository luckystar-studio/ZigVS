# ZigVS

Add support for using the [Zig programming language](https://ziglang.org/) to [Visual Studio](https://learn.microsoft.com/en-us/visualstudio/windows/?view=vs-2022)  

* Day-to-day Zig development tasks can be performed directly from the Visual Studio GUI.
* Debugging and profiling can be used immediately without any special setup.
* Zig code is easy to integrate into existing C++ and C# projects.

*Note: Please **update Visual Studio** before installing ZigVS. If your version of Visual Studio is older than the required version for ZigVS, a version error will appear during installation.* 
___
  
# Features

* Edit Zig code with [Visual Studio Editor](https://learn.microsoft.com/en-us/visualstudio/ide/index-writing-code?view=vs-2022)
    * Zig language syntax highlighting
    * [Zig Language Server (ZLS)](https://github.com/zigtools/zls) support (completions, tool tips, find references and more)
    * Snippets
    * Zig Document Formatting 
    * Auto-Insert Parentheses/Bracket/Braces
    * Copy Json Text and Paste as Zig Structs
    * Comment and uncomment zig style comments  
        line comments (//), doc comments (///), top-level doc comments (//!)
    * Multiline string marking (\\\\)
    * Generate a unit test skeleton for the function at the cursor position
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
    * Set breakpoints and watches from the GUI, inspect Locals / Autos / Call Stack, step through code, and use conditional breakpoints and the Immediate / Watch windows while debugging
* Run code tests using [Visual Studio Testing Tools](https://learn.microsoft.com/en-us/visualstudio/test/?view=vs-2022)
    * Run, debug & manage unit tests from [Test Explorer](https://learn.microsoft.com/en-us/visualstudio/test/run-unit-tests-with-test-explorer?view=vs-2022)
* Supports [Visual Studio Profiler Tools](https://learn.microsoft.com/en-us/visualstudio/profiling/?view=vs-2022)
  * CPU Usage, Memory Usage, Events, File I/O
* Zig Tool-chain Installer
* Validate Zig Environment command
  * Verify zig.exe, zls.exe, version compatibility and recommended settings in the Output window
* Zig Package Installer

___
# Table of Contents

* [How to Install ZigVS](#how-to-install-zigvs)
* [How to Set up Zig compiler and ZLS language server manually](#how-to-set-up-zig-compiler-and-zls-language-server-manually)
* [How to Set up Zig and ZLS with the Tool-chain installer inside Visual Studio](#how-to-set-up-zig-and-zls-with-the-tool-chain-installer-inside-visual-studio)
* [Validate Zig Environment](#validate-zig-environment)
* [Project Mode and Folder Mode](#project-mode-and-folder-mode)
* [How to open an existing Zig package](#how-to-open-an-existing-zig-package)
* [How to create a new zig package](#how-to-create-a-new-zig-package)
* [How to create a new Zig Project](#how-to-create-a-new-zig-project)
* [How to start debugging](#how-to-start-debugging)
* [Cross-Platform Build & Debugging](#cross-platform-build--debugging)
* [Profiler](#profiler)
* [Testing](#testing)
* [Syntax Highlighting](#syntax-highlighting)
* [Editor Setting](#editor-setting)
* [Inlay hints](#inlay-hints)
* [Formatting](#formatting)
* [Snippets](#snippets)
* [Copy Json Text and Paste as Zig Structs](#copy-json-text-and-paste-as-zig-structs)
* [Comment and uncomment](#comment-and-uncomment)
* [Package Installer](#package-installer)
* [Setting](#setting)
* [Help](#help)
* [Questions, Requests, etc.](#questions-requests-etc)
* [License](#license)
* [Extension Name](#extension-name)
* [Publisher](#publisher)
* [Version History](#version-history)

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

# Validate Zig Environment

If ZigVS can not build, debug or start ZLS, you can validate the current tool configuration from the menu.

1. Select [Extensions] → [ZigVS] → [Validate Zig Environment]
2. ZigVS will open the **Output window** and validate:
   * the configured zig.exe path
   * the configured zls.exe path
   * the active project's ToolPath override in Project Mode, if present
   * the detected zig and zls versions
   * whether zig and zls are on the same major.minor release line
   * recommended settings such as using PATH or environment variables and keeping Language Server Debug Mode off unless needed
3. Review the messages marked as [OK], [INFO], [WARNING] or [ERROR]

Use this command after changing [Tools] → [Options] → [ZigVS], after installing a new Zig or ZLS version, or when debugging environment-related problems.

___

# Project Mode and Folder Mode  

**Visual Studio has two modes: Project mode and Folder mode.**

* **Project mode** is used when developing in C++, C#, F#, and similar languages. You open project file types such as .cppproj, .csproj and .fsproj. The Solution Explorer tree shows only the items that have been added to the project by default. Builds are not performed directly by Visual Studio but by a build tool called MSBuild. Project files can be built without Visual Studio. A “project” corresponds to what Zig calls a package. Although you can edit project files in a text editor, you can also configure them via Visual Studio's Project Properties GUI. A solution file can contain multiple project files, allowing you to coordinate several projects to create a larger application.

 * **Folder mode** supports languages that are not built via project files. As in Visual Studio Code, you open a folder, and the file tree shows all files under that folder. Builds are performed by each language's own build system.

<br>

**ZigVS supports both Project mode and Folder mode.**

* In ZigVS's Project mode, you use a project file type called .zigproj. To start a project, create a .zigproj; to open the project, open the .zigproj. When you build the project (even from Visual Studio's GUI), MSBuild reads the .zigproj and builds using zig.exe as the compiler and linker.

  * Managed git dependencies can now be stored directly in the `.zigproj` as `ZigDependency` items. In Solution Explorer they appear under a virtual `Dependencies` node, each dependency is pinned to a commit, and `msbuild` restores the checkout with `git` before a normal project-mode build.
  * Project Properties now expose `Git Path`, and each dependency node exposes `Commit`, `Module Name`, and `Root Source` in the Properties window. Deleting a dependency node removes it from the `.zigproj`.
  * Managed dependencies are only passed automatically to the build when `UseBuildDotZig=false`. If you use a custom `build.zig`, ZigVS keeps the dependency metadata and checkout, but it does not rewrite your custom build script.

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
# How to start debugging

## Project mode (.zigproj)

1. Open the `.zigproj` project or solution.
2. Make sure the project builds successfully in the selected configuration.
3. Set breakpoints in your Zig source code if needed.
4. Start debugging with one of the following:
   * [Debug] → [Start Debugging]
   * the Start button on the toolbar
   * `F5`

## Folder mode (build.zig)

1. Open the folder that contains `build.zig`.
2. In Solution Explorer, right-click `build.zig` and choose **[Set as Startup Item]**.
3. Make sure the folder-mode build succeeds.
4. Start debugging with [Debug] → [Start Debugging], the Start button, or `F5`.

## Useful debugger actions

* `F9`: Toggle breakpoint on the current line.
* `F5`: Continue execution until the next breakpoint.
* `F10`: Step over the next line.
* `F11`: Step into the next function call.
* `Shift + F11`: Step out of the current function.
* Hover the mouse over variables to inspect values while stopped at a breakpoint.
* Use the **Locals**, **Autos**, **Watch**, and **Call Stack** windows from the [Debug] menu to inspect state.
* You can edit breakpoints, add conditions, and disable or enable them from the **Breakpoints** window.

## What you can do with the debugger

With the Visual Studio debugger you can:

* pause execution at breakpoints
* inspect local variables and function arguments
* evaluate expressions in Watch windows
* step line by line through Zig code
* inspect the current call stack
* switch stack frames and inspect values in parent calls
* combine debugging with the Diagnostics Tools window for CPU and memory investigation

If debugging does not start, first confirm that the project or folder can be built successfully and that the active Zig toolchain path is correct. The **Validate Zig Environment** command is useful when debugging setup problems.

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
# Copy Json Text and Paste as Zig Structs

1. Copy Json text to clipboard
![](ZigVS/Documents/Images/CopyJsonStringAndPastAsStruct1.png)

2. In Zig code editor, right-click and select [Editor] → [Zig] → [Paste Json as Zig Structs]

![](ZigVS/Documents/Images/CopyJsonStringAndPastAsStruct2.png)

3. The Json text in the clipboard will be converted to Zig Structs and pasted into the editor at the cursor position.

![](ZigVS/Documents/Images/CopyJsonStringAndPastAsStruct3.png)

<br><br>
___
# Comment and uncomment

1, [Edit] → [Zig] → 

2, The selected lines will be commented or uncommented in zig style comments.  
   line comments (//), doc comments (///), top-level doc comments (//!)

![](ZigVS/Documents/Images/Edit_Zig.png)

3, You can also add [shortcuts](https://learn.microsoft.com/en-us/visualstudio/ide/identifying-and-customizing-keyboard-shortcuts-in-visual-studio?view=vs-2022) to these commands.


___
# Package Installer

1, [Extensions] → [ZigVS] → [Zig Package Installer]  

![](ZigVS/Documents/Images/Extensions_ZigVS_PackageInstaller.png)

2, Browse to the repository and branch that you want to install in the WebView.  
3, Select a Installation Method from the drop down.  
   In Open Folder mode, zig fetch, git and unzip are available. In Project mode, `add package` adds a managed git dependency to the current `.zigproj`.  
4. Once everything is set up, the Install button will become active, so press the Install button  

![](ZigVS/Documents/Images/PackageInstaller.png)

## Managed git dependencies in Project mode

When the install method is **Project Dependencies (.zigproj)**, ZigVS stores the dependency directly in the project file as a `ZigDependency` item instead of editing `build.zig`.

Each managed dependency stores:

* `RepositoryUrl`
* `Commit`
* `ModuleName`
* `RootSource`
* `CheckoutDir`

### How it behaves

* The dependency appears under the virtual **Dependencies** node in Solution Explorer.
* Before a normal project-mode build, `msbuild` restores the checkout with `git fetch`, `git checkout --detach`, and `git submodule update --init --recursive`.
* The dependency node's **Properties** window lets you edit `Commit`, `Module Name`, and `Root Source`.
* Deleting the dependency node removes the `ZigDependency` item from the `.zigproj`. You can also choose whether to delete the checked out package folder.

### How ZigVS passes dependencies to zig.exe

If `UseBuildDotZig=false`, ZigVS automatically injects managed dependencies into the generated `zig build-exe` / `zig build-lib` command line.

* For older Zig command lines, ZigVS can use legacy `--deps` / `--mod`.
* For Zig 0.12 and later, ZigVS uses the modern `--dep` and `-Mroot=...` / `-Mname=...` form.
* Existing `.zigproj` files are migrated automatically to match the detected Zig toolchain version.
* After a successful build, the project-mode target now prints `Built output: ...` and also checks that the expected output file actually exists.

If `UseBuildDotZig=true`, ZigVS still stores the dependency metadata and restores the checkout, but it does **not** rewrite your custom `build.zig`. In that case you must wire the dependency into `build.zig` yourself.

### Important notes

* The import name in Zig source comes from **ModuleName**.  
  Example: if `ModuleName=clap`, then use `const clap = @import("clap");`.
* `RootSource` must point to the dependency's Zig module entry file, for example `src/main.zig` or `clap.zig`.
* `ModuleName` does not have to match the repository name. If you want `@import("json")`, set `ModuleName` to `json`.
* ZigVS can restore and pass the dependency correctly, but the dependency project itself must still be compatible with the Zig version you are using.

### How to choose a package for Project Dependencies (.zigproj)

Before adding a package, check the following:

* **ModuleName** becomes the Zig import name used in your source code.  
  If you want `@import("json")`, set `ModuleName` to `json`.
* **RootSource** must point to the package's real root module file.  
  Common values are `src/main.zig`, `src/root.zig`, or a top-level file such as `clap.zig`.
* The package itself must support the Zig version you are using.  
  Even if ZigVS restores and passes the dependency correctly, the build will still fail if the package is not compatible with your active Zig toolchain.
* Prefer packages whose README or release notes explicitly mention the Zig version they support.
* For a first smoke test, prefer a small pure-Zig package that can be verified with a simple `@import(...)` and one or two API calls.

### Quick smoke test checklist

1. Choose a package that explicitly supports your Zig version.
2. Set `ModuleName` to the exact import name you want to use in source code.
3. Set `RootSource` to the package's actual root module file.
4. Build once and confirm the Output window shows both the checkout steps and `Built output: ...`.

<br><br>

___
# Setting

[Tools]→[Options]→[Zig]

![](ZigVS/Documents/Images/Tool_Option_Zig_General.png)

![](ZigVS/Documents/Images/Tool_Option_Zig_FolderMode.png)



___
# Help

a. See [User Manual Page](https://luckystar-studio.github.io/ZigVS-Web/)

or 

b. [Extensions] → [ZigVS] → [User Manual Page]  
![](ZigVS/Documents/Images/Extensions_ZigVS_UserManualPage.png)

<br><br>
___
# Questions, Requests, etc.

* [ZigVS Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=LuckystarStudio.ZigVS)  
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
    Version 0.16.0.0 (2026/04/24):
        Improvements
            Added support for Zig 0.16.0 and ZLS 0.16.0
            Added unit test skeleton generation.
                It creates a unit test template for the function at the cursor position.
            Included the fixes from GitHub pull request #1.

        Compatibility:
            Visual Studio 2026 18.5.1
            Zig Tool chain: zig-windows-x86_64-0.16.0.zip
            ZLS Language Server: zls-windows-x86_64-0.16.0.zip

    Version 0.15.2.3 (2026/04/01):
        Improvements
            Added managed git dependencies for `.zigproj` Project mode.
                Dependencies now appear under a virtual `Dependencies` node in Solution Explorer and can be added from the Zig Package Installer with `Project Dependencies (.zigproj)`.
                Dependency Properties now expose `Commit`, `Module Name`, and `Root Source`.
                During project-mode builds, ZigVS restores managed dependencies with `git` and automatically maps them to the correct Zig CLI syntax for the detected toolchain.
                Existing project files are migrated between legacy `--deps` / `--mod` and modern `--dep` / `-Mroot` / `-Mname` forms as needed.
                Project-mode builds now print `Built output: ...` and verify that the expected output file was actually produced.

        Compatibility:
            Visual Studio 2026 18.4.2
            Zig Tool chain: zig-windows-x86_64-0.15.2.zip
            ZLS Language Server: zls-windows-x86_64-0.15.1.zip

    Version 0.15.2.2 (2025/12/31):
        Added support for ZLS 0.15.1
        Compatibility:
            Visual Studio 2026 18.1.1
            Zig Tool chain: zig-windows-x86_64-0.15.2.zip
            ZLS Language Server: zls-windows-x86_64-0.15.1.zip

    Version 0.15.2.1 (2025/10/29):
        Improvements
            Comment and uncomment zig style comments
line comments (//), doc comments (///), top-level doc comments (//!)
            Multiline string marking (\\)
        Compatibility:
            Visual Studio 2022 17.14.14
            Zig Tool chain: zig-windows-x86_64-0.15.2.zip
            ZLS Language Server: zls-windows-x86_64-0.15.0.zip

    Version 0.15.2.0 (2025/10/16):
        Added support for Zig 0.15.2, Zig 0.15.1 and ZLS 0.15.0
        Improvements
            Add Copy Json Text and Paste as Zig Structs feature.
        Compatibility:
            Visual Studio 2022 17.14.14
            Zig Tool chain: zig-windows-x86_64-0.15.2.zip
            ZLS Language Server: zls-windows-x86_64-0.15.0.zip
            (tested briefly in Visual Studio 2026 and appears to work ok)

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
