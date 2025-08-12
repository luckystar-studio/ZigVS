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
    using System.ComponentModel;

    internal class FolderModeOptions : BaseOptionModel<FolderModeOptions>
    {
        [Category("1) Build Tool")]
        [DisplayName("Tool Path (zig)")]
        [Description("Path to the build tool.")]
        [DefaultValue(Parameter.c_compilerFileName)]
        public string ToolPath { get; set; } = Parameter.c_compilerFileName;

        [Category("1) Build Tool")]
        [DisplayName("Build File name")]
        [Description("The description of the property")]
        [DefaultValue(Parameter.c_buildFileName)]
        public string BuildFileName { get; set; } = Parameter.c_buildFileName;

        [Category("2) Configuration")]
        [DisplayName("Architecture enumeration")]
        [Description("Architecture enumeration")]
        [DefaultValue("x86_64 aarch64")]
        public string ArchitectureList { get; set; } = "x86_64 aarch64 wasm32";

        [Category("2) Configuration")]
        [DisplayName("OS enumeration")]
        [Description("OS enumeration")]
        [DefaultValue("windows linux")]
        public string OSList { get; set; } = "windows linux wasi";

        [Category("2) Configuration")]
        [DisplayName("Optimize enumeration")]
        [Description("Optimize enumeration")]
        [DefaultValue("Debug ReleaseFast")]
        public string OptimizeList { get; set; } = "Debug ReleaseFast";

        [Category("3) Command")]
        [DisplayName("Build")]
        [Description("Build Command")]
        [DefaultValue("")]
        public string BuildCommand_build { get; set; } = "";

        [Category("3) Command")]
        [DisplayName("Single File Build")]
        [Description("Build Command for Single File.")]
        [DefaultValue("build-exe")]
        public string BuildCommand_buildexe { get; set; } = "build-exe";

        [Category("3) Command")]
        [DisplayName("Clean")]
        [Description("Build Command to Clean.")]
        [DefaultValue("uninstall")]
        public string BuildCommand_clean { get; set; } = "uninstall";

        [Category("4) Directory")]
        [DisplayName("Intermediate Directory Name")]
        [Description("Intermediate Directory Name.")]
        [DefaultValue("int")]
        public string IntermediateDirectoryName { get; set; } = ".zig-cache";

        [Category("4) Directory")]
        [DisplayName("Output Directory Name")]
        [Description("Output Directory Name.")]
        [DefaultValue("out")]
        public string OutputDirectoryName { get; set; } = "zig-out";

        [Category("4) Directory")]
        [DisplayName("Package Directory Name")]
        [Description("Package Directory Name.")]
        [DefaultValue("lib")]
        public string PackageDirectoryName { get; set; } = "lib";

        [Category("5) Pre Debug")]
        [DisplayName("Pre Debug Command")]
        [Description("This command will be executed before the debugger launches. It is useful for copying the output files to the target platform.")]
        [DefaultValue("")]
        public string PreDebugCommand { get; set; } = "";

        [Category("5) Pre Debug")]
        [DisplayName("Pre Debug Command Arguments")]
        [Description("Arguments for Pre Debug Command.")]
        [DefaultValue("")]
        public string PreDebugCommandArguments { get; set; } = "";

        [Category("6) Debug")]
        [DisplayName(" Debug Engine")]
        [Description("MIEngine supports WSL,Docker,SSH / Linux,Android,iOS,macOS.")]
        [DefaultValue(DebugEngine.WindowsNative)]
        [TypeConverter(typeof(EnumConverter))]
        public DebugEngine DebugEngine { get; set; } = DebugEngine.WindowsNative;

        [Category("6) Debug")]
        [DisplayName(" MIEngine launch Options file name")]
        [Description("MIEngine requires a json/xml file to set options")]
        [DefaultValue("LaunchOptions.xml")]
        public string LaunchOptionFileName { get; set; } = "LaunchOptions.xml";

        [Category("6) Debug")]
        [DisplayName("Command Arguments")]
        [Description("The command line arguments to path to the application")]
        [DefaultValue("")]
        public string Arguments { get; set; } = "";

        [Category("6) Debug")]
        [DisplayName("Output name capturer")]
        [Description("code to capture output file name from build.zig")]
        [DefaultValue(DefaultCode)]
        public string BuildInfoCapturer { get; set; } = DefaultCode;


        const string DefaultCode =
@"
pub fn main() !void
{
    const __std = @import(""std"");
    const __process = std.process;

    var single_threaded_arena = std.heap.ArenaAllocator.init(__std.heap.page_allocator);
    defer single_threaded_arena.deinit();

    var thread_safe_arena: __std.heap.ThreadSafeAllocator = .{
        .child_allocator = single_threaded_arena.allocator(),
    };
    const arena = thread_safe_arena.allocator();

    const build_root_directory: __std.Build.Cache.Directory = .{
        .path = ""\\"",
        .handle = try __std.fs.cwd().openDir(""\\"", .{}),
    };

    const local_cache_directory: __std.Build.Cache.Directory = .{
        .path = ""\\"",
        .handle = try __std.fs.cwd().makeOpenPath(""\\"", .{}),
    };

    const global_cache_directory: __std.Build.Cache.Directory = .{
        .path = ""\\"",
        .handle = try __std.fs.cwd().makeOpenPath(""\\"", .{}),
    };

    const zig_lib_directory: __std.Build.Cache.Directory = .{
        .path = ""\\"",
        .handle = try __std.fs.cwd().makeOpenPath(""\\"", .{}),
    };

    var graph: std.Build.Graph = .{
        .arena = arena,
        .cache = .{
            .gpa = arena,
            .manifest_dir = try local_cache_directory.handle.makeOpenPath(""h"", .{}),
        },
        .zig_exe = """", // ToDo
        .env_map = try __process.getEnvMap(arena),
        .global_cache_root = global_cache_directory,
        .zig_lib_directory = zig_lib_directory,
        .host = .{
            .query = .{},
            .result = try std.zig.system.resolveTargetQuery(.{}),
        },
    };

    graph.cache.addPrefix(.{ .path = null, .handle = __std.fs.cwd() });
    graph.cache.addPrefix(build_root_directory);
    graph.cache.addPrefix(local_cache_directory);
    graph.cache.addPrefix(global_cache_directory);

    const available_deps: []const struct { []const u8, []const u8 } = &.{};

    const builder = try std.Build.create(
        &graph,
        build_root_directory,
        local_cache_directory,
        available_deps,
    );
    build(builder);

    const install_step = builder.getInstallStep();

    var selected_basename: ?[]const u8 = null;

    var i: usize = 0;
    while (i < install_step.dependencies.items.len) : (i += 1) {
        const dep = install_step.dependencies.items[i];
        if (__std.Build.Step.cast(dep, __std.Build.Step.InstallArtifact)) |ia| {
            if (ia.artifact.kind == .exe or ia.artifact.kind == .@""test"") {
                selected_basename = __std.fs.path.basename(ia.dest_sub_path);
                break;
            }
        }
    }

    const stdout = __std.io.getStdOut().writer();
    if (selected_basename) |name| {
        try stdout.print(""{s}"", .{name});
        return;
    }

    return error.NoInstallableArtifactFound;
}

";
    }

    public enum DebugEngine
    {
        WindowsNative,
        MIEngine
    }
}
