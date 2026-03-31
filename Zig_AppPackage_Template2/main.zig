// new zig project $projectName$, $className$.zig

const std = @import("std");
const print = std.debug.print;

const c_header = @cImport({
    @cInclude("c_header.h");
});

const cpp_header = @cImport({
    @cInclude("cpp_header.h");
});

pub fn main() !void
{
    print("hello, world\n",.{});

    const a  = c_header.c_add(1,1);
    print("1 + 1 = {}\n", .{a});
  
    const b  = cpp_header.cpp_add(2,2);
    print("2 + 2 = {}\n", .{b});
}

