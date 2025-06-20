// new zig code $projectName$, $className$.zig
const std = @import("std");

export fn add(a: i32, b: i32) i32 {
    return a + b;
}

test "basic add functionality" {
    try std.testing.expect(add(3, 7) == 10);
}
