using SemanticKernelDevHub.Models;

// Test the enhanced code review output with branch information
var testResult = new CodeReviewResult
{
    OverallScore = 8,
    Summary = "Good quality code with minor improvements needed",
    Metadata = new ReviewMetadata
    {
        ReviewType = CodeReviewType.Commit,
        Target = "a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8s9t0",
        RepositoryName = "SemanticKernelDevHub",
        BranchName = "feature/add-branch-info",
        TotalFilesAnalyzed = 3,
        ReviewDate = DateTime.Now,
        ReviewDuration = TimeSpan.FromSeconds(5.2)
    },
    FileReviews = new List<FileReviewResult>
    {
        new FileReviewResult
        {
            FileName = "Models/CodeReviewRequest.cs",
            Language = "C#",
            Score = 9,
            Issues = new List<string> { "Minor formatting issue" },
            Suggestions = new List<string> { "Consider adding XML documentation" }
        },
        new FileReviewResult
        {
            FileName = "Plugins/GitHubPlugin.cs",
            Language = "C#",
            Score = 8,
            Issues = new List<string> { "Exception handling could be improved" },
            Suggestions = new List<string> { "Add specific error messages", "Consider retry logic" }
        }
    }
};

Console.WriteLine("=== Enhanced Code Review Output with Branch Information ===");
Console.WriteLine(testResult.ToString());
