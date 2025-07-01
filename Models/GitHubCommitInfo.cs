namespace SemanticKernelDevHub.Models;

/// <summary>
/// Represents GitHub commit information
/// </summary>
public class GitHubCommitInfo
{
    /// <summary>
    /// The commit SHA (unique identifier)
    /// </summary>
    public string Sha { get; set; } = string.Empty;

    /// <summary>
    /// The commit message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The author of the commit
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// The date the commit was made
    /// </summary>
    public DateTimeOffset Date { get; set; }

    /// <summary>
    /// URL to view the commit on GitHub
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// The branch name where this commit exists (may be multiple branches)
    /// </summary>
    public string BranchName { get; set; } = string.Empty;

    /// <summary>
    /// List of files changed in this commit
    /// </summary>
    public List<GitHubFileInfo> FilesChanged { get; set; } = new();

    /// <summary>
    /// Short version of the SHA for display
    /// </summary>
    public string ShortSha => Sha.Length > 8 ? Sha[..8] : Sha;

    /// <summary>
    /// Total number of additions across all files
    /// </summary>
    public int TotalAdditions => FilesChanged.Sum(f => f.Additions);

    /// <summary>
    /// Total number of deletions across all files
    /// </summary>
    public int TotalDeletions => FilesChanged.Sum(f => f.Deletions);

    /// <summary>
    /// Total number of changes across all files
    /// </summary>
    public int TotalChanges => FilesChanged.Sum(f => f.Changes);

    /// <summary>
    /// Returns a formatted string representation of the commit
    /// </summary>
    public override string ToString()
    {
        var firstLine = Message.Split('\n')[0];
        var branchInfo = !string.IsNullOrEmpty(BranchName) ? $" [{BranchName}]" : "";
        return $"{ShortSha} - {firstLine} ({Author}){branchInfo}";
    }
}
