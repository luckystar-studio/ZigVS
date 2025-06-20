___
# Version History
```
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
        In Open Folder mode
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