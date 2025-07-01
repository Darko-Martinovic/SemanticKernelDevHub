using Microsoft.SemanticKernel;
using System.ComponentModel;
using Octokit;
using SemanticKernelDevHub.Models;

namespace SemanticKernelDevHub.Plugins;

/// <summary>
/// GitHub integration plugin for Semantic Kernel
/// </summary>
public class GitHubPlugin
{
    private readonly GitHubClient _gitHubClient;
    private readonly string _repoOwner;
    private readonly string _repoName;

    public GitHubPlugin(string token, string repoOwner, string repoName)
    {
        _gitHubClient = new GitHubClient(new ProductHeaderValue("SemanticKernelDevHub"))
        {
            Credentials = new Credentials(token)
        };
        _repoOwner = repoOwner;
        _repoName = repoName;
    }

    /// <summary>
    /// Gets the latest commits from the repository
    /// </summary>
    /// <param name="count">Number of commits to retrieve</param>
    /// <returns>List of commit information</returns>
    [KernelFunction("get_recent_commits")]
    [Description("Retrieves recent commits from the GitHub repository")]
    public async Task<List<GitHubCommitInfo>> GetRecentCommits(
        [Description("Number of commits to retrieve (default: 10, max: 50)")] int count = 10
    )
    {
        try
        {
            // Limit to reasonable range
            count = Math.Max(1, Math.Min(count, 50));

            var request = new ApiOptions { PageCount = 1, PageSize = count };

            var commits = await _gitHubClient.Repository.Commit.GetAll(
                _repoOwner,
                _repoName,
                request
            );

            var commitInfos = new List<GitHubCommitInfo>();

            foreach (var commit in commits)
            {
                var commitInfo = new GitHubCommitInfo
                {
                    Sha = commit.Sha,
                    Message = commit.Commit.Message,
                    Author = commit.Commit.Author.Name,
                    Date = commit.Commit.Author.Date,
                    Url = commit.HtmlUrl
                };

                // Try to get branch information for each commit
                try
                {
                    commitInfo.BranchName = await GetCommitBranch(commit.Sha);
                }
                catch
                {
                    // If branch detection fails, leave it empty
                    commitInfo.BranchName = string.Empty;
                }

                commitInfos.Add(commitInfo);
            }

            return commitInfos;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to retrieve commits: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets detailed information about a specific commit
    /// </summary>
    /// <param name="commitSha">The SHA of the commit</param>
    /// <returns>Detailed commit information with file changes</returns>
    [KernelFunction("get_commit_details")]
    [Description("Gets detailed information about a specific commit including file changes")]
    public async Task<GitHubCommitInfo> GetCommitDetails(
        [Description("The SHA (hash) of the commit to analyze")] string commitSha
    )
    {
        try
        {
            var commit = await _gitHubClient.Repository.Commit.Get(
                _repoOwner,
                _repoName,
                commitSha
            );

            var commitInfo = new GitHubCommitInfo
            {
                Sha = commit.Sha,
                Message = commit.Commit.Message,
                Author = commit.Commit.Author.Name,
                Date = commit.Commit.Author.Date,
                Url = commit.HtmlUrl,
                FilesChanged =
                    commit.Files
                        ?.Select(
                            file =>
                                new GitHubFileInfo
                                {
                                    FileName = file.Filename,
                                    Status = file.Status,
                                    Additions = file.Additions,
                                    Deletions = file.Deletions,
                                    Changes = file.Changes,
                                    Patch = file.Patch
                                }
                        )
                        .ToList() ?? new List<GitHubFileInfo>()
            };

            // Try to get branch information
            try
            {
                commitInfo.BranchName = await GetCommitBranch(commitSha);
            }
            catch
            {
                // If branch detection fails, leave it empty
                commitInfo.BranchName = string.Empty;
            }

            return commitInfo;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to get commit details for {commitSha}: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Gets the branch(es) that contain a specific commit
    /// </summary>
    /// <param name="commitSha">The SHA of the commit</param>
    /// <returns>Branch name (or default branch if not found)</returns>
    [KernelFunction("get_commit_branch")]
    [Description("Gets the branch that contains a specific commit")]
    public async Task<string> GetCommitBranch(
        [Description("The SHA (hash) of the commit")] string commitSha
    )
    {
        try
        {
            // Try to find branches that contain this commit
            var branches = await _gitHubClient.Repository.Branch.GetAll(_repoOwner, _repoName);

            foreach (var branch in branches)
            {
                try
                {
                    // Check if this commit exists in this branch's history
                    var branchCommits = await _gitHubClient.Repository.Commit.GetAll(
                        _repoOwner,
                        _repoName,
                        new CommitRequest { Sha = branch.Name }
                    );

                    if (branchCommits.Any(c => c.Sha.StartsWith(commitSha)))
                    {
                        return branch.Name;
                    }
                }
                catch
                {
                    // Continue checking other branches if one fails
                    continue;
                }
            }

            // If not found in any specific branch, try to get the default branch
            var repo = await _gitHubClient.Repository.Get(_repoOwner, _repoName);
            return repo.DefaultBranch;
        }
        catch (Exception)
        {
            // If all else fails, return "main" as a reasonable default
            return "main";
        }
    }

    /// <summary>
    /// Gets pull request information
    /// </summary>
    /// <param name="pullRequestNumber">The PR number</param>
    /// <returns>Pull request information</returns>
    [KernelFunction("get_pull_request")]
    [Description("Gets detailed information about a pull request")]
    public async Task<string> GetPullRequestInfo(
        [Description("The pull request number")] int pullRequestNumber
    )
    {
        try
        {
            var pullRequest = await _gitHubClient.PullRequest.Get(
                _repoOwner,
                _repoName,
                pullRequestNumber
            );
            var files = await _gitHubClient.PullRequest.Files(
                _repoOwner,
                _repoName,
                pullRequestNumber
            );

            var filesList = files
                .Select(
                    f =>
                        new GitHubFileInfo
                        {
                            FileName = f.FileName,
                            Status = f.Status,
                            Additions = f.Additions,
                            Deletions = f.Deletions,
                            Changes = f.Changes,
                            Patch = f.Patch
                        }
                )
                .ToList();

            return System.Text.Json.JsonSerializer.Serialize(
                new
                {
                    Number = pullRequest.Number,
                    Title = pullRequest.Title,
                    Description = pullRequest.Body,
                    Author = pullRequest.User.Login,
                    State = pullRequest.State.ToString(),
                    CreatedAt = pullRequest.CreatedAt,
                    UpdatedAt = pullRequest.UpdatedAt,
                    Files = filesList
                }
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to get pull request {pullRequestNumber}: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Gets the content of a specific file from the repository
    /// </summary>
    /// <param name="filePath">Path to the file in the repository</param>
    /// <param name="reference">Branch or commit reference (optional)</param>
    /// <returns>File content as string</returns>
    [KernelFunction("get_file_content")]
    [Description("Retrieves the content of a file from the GitHub repository")]
    public async Task<string> GetFileContent(
        [Description("Path to the file in the repository")] string filePath,
        [Description("Branch or commit reference (default: main branch)")] string reference = "main"
    )
    {
        try
        {
            var fileContents = await _gitHubClient.Repository.Content.GetAllContents(
                _repoOwner,
                _repoName,
                filePath
            );

            if (fileContents.Count == 0)
            {
                return $"File not found: {filePath}";
            }

            var file = fileContents[0];
            return file.Type == ContentType.File ? file.Content : $"Path is not a file: {filePath}";
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to get file content for {filePath}: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Lists all files changed in a commit
    /// </summary>
    /// <param name="commitSha">The commit SHA</param>
    /// <returns>List of changed files with their status</returns>
    [KernelFunction("list_commit_files")]
    [Description("Lists all files that were changed in a specific commit")]
    public async Task<List<GitHubFileInfo>> ListCommitFiles(
        [Description("The SHA of the commit")] string commitSha
    )
    {
        try
        {
            var commit = await _gitHubClient.Repository.Commit.Get(
                _repoOwner,
                _repoName,
                commitSha
            );

            return commit.Files
                    ?.Select(
                        file =>
                            new GitHubFileInfo
                            {
                                FileName = file.Filename,
                                Status = file.Status,
                                Additions = file.Additions,
                                Deletions = file.Deletions,
                                Changes = file.Changes,
                                Patch = file.Patch
                            }
                    )
                    .ToList() ?? new List<GitHubFileInfo>();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to list files for commit {commitSha}: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Gets repository information
    /// </summary>
    /// <returns>Repository details</returns>
    [KernelFunction("get_repository_info")]
    [Description("Gets basic information about the GitHub repository")]
    public async Task<string> GetRepositoryInfo()
    {
        try
        {
            var repo = await _gitHubClient.Repository.Get(_repoOwner, _repoName);

            return System.Text.Json.JsonSerializer.Serialize(
                new
                {
                    Name = repo.Name,
                    FullName = repo.FullName,
                    Description = repo.Description,
                    Language = repo.Language,
                    StargazersCount = repo.StargazersCount,
                    ForksCount = repo.ForksCount,
                    OpenIssuesCount = repo.OpenIssuesCount,
                    DefaultBranch = repo.DefaultBranch,
                    CreatedAt = repo.CreatedAt,
                    UpdatedAt = repo.UpdatedAt,
                    Url = repo.HtmlUrl
                }
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get repository info: {ex.Message}", ex);
        }
    }
}
