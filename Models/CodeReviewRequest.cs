using SemanticKernelDevHub.Models;

namespace SemanticKernelDevHub.Models;

/// <summary>
/// Represents a request for code review
/// </summary>
public class CodeReviewRequest
{
    /// <summary>    /// <summary>
    /// The commit or PR that was reviewed
    /// </summary>
    public string Target { get; set; } = string.Empty;

    /// <summary>
    /// The repository name being reviewed
    /// </summary>
    public string RepositoryName { get; set; } = string.Empty;

    /// <summary>
    /// Total number of files analyzed
    /// </summary>
    public int TotalFilesAnalyzed { get; set; }

    /// <summary>
    /// The type of review requested
    /// </summary>
    public CodeReviewType ReviewType { get; set; }

    /// <summary>
    /// The commit SHA to review (for commit reviews)
    /// </summary>
    public string? CommitSha { get; set; }

    /// <summary>
    /// The pull request number to review (for PR reviews)
    /// </summary>
    public int? PullRequestNumber { get; set; }

    /// <summary>
    /// Specific files to focus on (optional)
    /// </summary>
    public List<string> FilesToReview { get; set; } = new();

    /// <summary>
    /// Focus areas for the review
    /// </summary>
    public List<string> FocusAreas { get; set; } = new();

    /// <summary>
    /// Whether to include only supported languages
    /// </summary>
    public bool OnlySupportedLanguages { get; set; } = true;

    /// <summary>
    /// Maximum number of files to review
    /// </summary>
    public int MaxFilesToReview { get; set; } = 10;

    /// <summary>
    /// Whether to include detailed file analysis
    /// </summary>
    public bool IncludeDetailedAnalysis { get; set; } = true;
}

/// <summary>
/// Types of code reviews that can be performed
/// </summary>
public enum CodeReviewType
{
    /// <summary>
    /// Review a specific commit
    /// </summary>
    Commit,

    /// <summary>
    /// Review a pull request
    /// </summary>
    PullRequest,

    /// <summary>
    /// Review specific files
    /// </summary>
    Files,

    /// <summary>
    /// Review latest changes
    /// </summary>
    Latest
}

/// <summary>
/// Represents the result of a code review
/// </summary>
public class CodeReviewResult
{
    /// <summary>
    /// Overall review summary
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Overall quality score (1-10)
    /// </summary>
    public int OverallScore { get; set; }

    /// <summary>
    /// Individual file reviews
    /// </summary>
    public List<FileReviewResult> FileReviews { get; set; } = new();

    /// <summary>
    /// Key issues found across all files
    /// </summary>
    public List<string> KeyIssues { get; set; } = new();

    /// <summary>
    /// Recommended improvements
    /// </summary>
    public List<string> Recommendations { get; set; } = new();

    /// <summary>
    /// Files that were skipped and reasons
    /// </summary>
    public List<string> SkippedFiles { get; set; } = new();

    /// <summary>
    /// Review metadata
    /// </summary>
    public ReviewMetadata Metadata { get; set; } = new();

    /// <summary>
    /// Formats the result as a readable string with comprehensive details and clear explanations
    /// </summary>
    public override string ToString()
    {
        var result = $"📊 **Code Review Results**\n\n";

        // Add repository information if available
        if (!string.IsNullOrEmpty(Metadata.RepositoryName))
        {
            result += $"**📂 Repository**: {Metadata.RepositoryName}\n";
        }

        // Add branch information if available
        if (!string.IsNullOrEmpty(Metadata.BranchName))
        {
            result += $"**🌿 Branch**: {Metadata.BranchName}\n";
        }

        // Make the target more descriptive based on review type
        if (!string.IsNullOrEmpty(Metadata.Target))
        {
            var targetDescription = Metadata.ReviewType switch
            {
                CodeReviewType.Commit => $"**🎯 Commit**: {Metadata.Target[..Math.Min(8, Metadata.Target.Length)]}... ({Metadata.Target})",
                CodeReviewType.PullRequest => $"**🎯 Pull Request**: #{Metadata.Target}",
                CodeReviewType.Latest => $"**🎯 Latest Commit**: {Metadata.Target[..Math.Min(8, Metadata.Target.Length)]}... ({Metadata.Target})",
                CodeReviewType.Files => $"**🎯 Target Files**: {Metadata.Target}",
                _ => $"**🎯 Target**: {Metadata.Target}"
            };
            result += $"{targetDescription}\n";
        }

        result += $"**🏆 Overall Score**: {OverallScore}/10\n";
        result += $"**📁 Files Reviewed**: {FileReviews.Count}\n";
        result += $"**📅 Review Date**: {Metadata.ReviewDate:yyyy-MM-dd HH:mm}\n";

        if (Metadata.ReviewDuration.TotalSeconds > 0)
        {
            result += $"**⏱️ Review Duration**: {Metadata.ReviewDuration.TotalSeconds:F1} seconds\n";
        }

        result += "\n";

        // Add detailed file breakdown
        if (FileReviews.Any())
        {
            result += "**📄 Files Analyzed**:\n";
            foreach (var file in FileReviews.OrderByDescending(f => f.Score))
            {
                var scoreEmoji = file.Score switch
                {
                    >= 9 => "🟢",
                    >= 7 => "🟡",
                    >= 5 => "🟠",
                    _ => "🔴"
                };

                var issueCount = file.Issues.Count > 0 ? $", {file.Issues.Count} issues" : "";
                var suggestionCount = file.Suggestions.Count > 0 ? $", {file.Suggestions.Count} suggestions" : "";

                result += $"  {scoreEmoji} **{file.FileName}** ({file.Language}): {file.Score}/10{issueCount}{suggestionCount}\n";
            }
            result += "\n";
        }

        // Add skipped files if any
        if (SkippedFiles.Any())
        {
            result += "**⏭️ Skipped Files**:\n";
            result += string.Join("\n", SkippedFiles.Select(file => $"  - {file}")) + "\n\n";
        }

        // Add key issues with better formatting
        if (KeyIssues.Any())
        {
            result += "**🚨 Critical Issues Found**:\n";
            result += string.Join("\n", KeyIssues.Select(issue => $"  ⚠️ {issue}")) + "\n\n";
        }

        // Add recommendations with better formatting
        if (Recommendations.Any())
        {
            result += "**💡 Recommendations**:\n";
            result += string.Join("\n", Recommendations.Select(rec => $"  ✅ {rec}")) + "\n\n";
        }

        // Add performance summary
        if (FileReviews.Any())
        {
            var averageScore = FileReviews.Average(f => f.Score);
            var totalIssues = FileReviews.Sum(f => f.Issues.Count);
            var totalSuggestions = FileReviews.Sum(f => f.Suggestions.Count);

            result += "**📊 Summary Statistics**:\n";
            result += $"  • Average File Score: {averageScore:F1}/10\n";
            result += $"  • Total Issues Identified: {totalIssues}\n";
            result += $"  • Total Suggestions Provided: {totalSuggestions}\n";

            if (FileReviews.Count > 0)
            {
                var highQualityFiles = FileReviews.Count(f => f.Score >= 8);
                var lowQualityFiles = FileReviews.Count(f => f.Score < 6);
                result += $"  • High Quality Files (8-10): {highQualityFiles}\n";
                result += $"  • Files Needing Attention (<6): {lowQualityFiles}\n";
            }
            result += "\n";
        }

        // Add the detailed summary
        if (!string.IsNullOrEmpty(Summary))
        {
            result += "**📋 Detailed Analysis**:\n";
            result += Summary;
        }

        return result;
    }
}

/// <summary>
/// Review result for a specific file
/// </summary>
public class FileReviewResult
{
    /// <summary>
    /// The file that was reviewed
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// The detected programming language
    /// </summary>
    public string Language { get; set; } = string.Empty;

    /// <summary>
    /// Quality score for this file (1-10)
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// Review feedback for this file
    /// </summary>
    public string Review { get; set; } = string.Empty;

    /// <summary>
    /// Issues found in this file
    /// </summary>
    public List<string> Issues { get; set; } = new();

    /// <summary>
    /// Suggestions for this file
    /// </summary>
    public List<string> Suggestions { get; set; } = new();

    /// <summary>
    /// Gets a summary of this file review
    /// </summary>
    public string GetSummary()
    {
        var scoreEmoji = Score switch
        {
            >= 9 => "🟢 Excellent",
            >= 7 => "🟡 Good",
            >= 5 => "🟠 Fair",
            _ => "🔴 Needs Improvement"
        };

        return $"{scoreEmoji} | {FileName} ({Language}) | Score: {Score}/10 | Issues: {Issues.Count} | Suggestions: {Suggestions.Count}";
    }
}

/// <summary>
/// Metadata about the review process
/// </summary>
public class ReviewMetadata
{
    /// <summary>
    /// When the review was performed
    /// </summary>
    public DateTime ReviewDate { get; set; } = DateTime.Now;

    /// <summary>
    /// The type of review performed
    /// </summary>
    public CodeReviewType ReviewType { get; set; }

    /// <summary>
    /// The commit or PR that was reviewed
    /// </summary>
    public string Target { get; set; } = string.Empty;

    /// <summary>
    /// The repository name being reviewed
    /// </summary>
    public string RepositoryName { get; set; } = string.Empty;

    /// <summary>
    /// The branch name where the commit/PR exists
    /// </summary>
    public string BranchName { get; set; } = string.Empty;

    /// <summary>
    /// Total number of files analyzed
    /// </summary>
    public int TotalFilesAnalyzed { get; set; }

    /// <summary>
    /// Time taken for the review
    /// </summary>
    public TimeSpan ReviewDuration { get; set; }

    /// <summary>
    /// Version of the review agent
    /// </summary>
    public string AgentVersion { get; set; } = "1.0";
}
