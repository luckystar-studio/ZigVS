namespace ZigVS.CoreCompatibility
{
    using ZigVS.CoreCompatibility.Build;
    using ZigVS.CoreCompatibility.Testing;
    using ZigVS.CoreCompatibility.Toolchain;

    internal static class CoreServices
    {
        static readonly IBuildArtifactResolver s_BuildArtifactResolver = new FolderModeBuildArtifactResolver();
        static readonly ITestResultParser s_TestResultParser = new ZigTestResultParser();
        static readonly IZigToolchainProbe s_ToolchainProbe = new ZigToolchainProbe();

        internal static IBuildArtifactResolver BuildArtifactResolver
        {
            get { return s_BuildArtifactResolver; }
        }

        internal static ITestResultParser TestResultParser
        {
            get { return s_TestResultParser; }
        }

        internal static IZigToolchainProbe ToolchainProbe
        {
            get { return s_ToolchainProbe; }
        }
    }
}
