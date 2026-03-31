namespace ZigVS
{
#nullable enable
    using System;

    public enum ProjectDependencyStatus
    {
        Resolved,
        Missing,
        Dirty,
        Unresolved
    }

    public sealed class ProjectDependencySpec
    {
        public string Include { get; set; } = String.Empty;
        public string RepositoryUrl { get; set; } = String.Empty;
        public string Commit { get; set; } = String.Empty;
        public string ModuleName { get; set; } = String.Empty;
        public string RootSource { get; set; } = String.Empty;
        public string CheckoutDir { get; set; } = String.Empty;

        public ProjectDependencySpec Clone()
        {
            return new ProjectDependencySpec
            {
                Include = this.Include,
                RepositoryUrl = this.RepositoryUrl,
                Commit = this.Commit,
                ModuleName = this.ModuleName,
                RootSource = this.RootSource,
                CheckoutDir = this.CheckoutDir
            };
        }
    }

    public sealed class ProjectDependencyDetectionResult
    {
        public string ModuleName { get; set; } = String.Empty;
        public string RootSource { get; set; } = String.Empty;
    }

    public sealed class ProjectDependencyAddRequest
    {
        public string RepositoryUrl { get; set; } = String.Empty;
        public string? ReferenceName { get; set; }
        public string? Commit { get; set; }
        public string? SuggestedName { get; set; }
    }

    public sealed class ProjectDependencyStatusInfo
    {
        public ProjectDependencyStatus Status { get; set; }
        public string Description { get; set; } = String.Empty;
    }
}
