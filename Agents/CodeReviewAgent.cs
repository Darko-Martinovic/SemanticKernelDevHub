using Microsoft.SemanticKernel;
using System.ComponentModel;
using Octokit;
using SemanticKernelDevHub.Models;
using SemanticKernelDevHub.Plugins;

namespace SemanticKernelDevHub.Agents;

/// <summary>
/// Agent responsible for code review and analysis tasks
/// </summary>
public class CodeReviewAgent : IAgent
{
    private readonly Kernel _kernel;
    private readonly GitHubClient? _gitHubClient;
    private readonly string? _repoOwner;
    private readonly string? _repoName;
    private readonly GitHubPlugin? _gitHubPlugin;
    private readonly JiraIntegrationAgent? _jiraAgent;

    public string Name => "CodeReviewAgent";
    
    public string Description => "Analyzes code quality, suggests improvements, and performs automated code reviews for C#, VB.NET, T-SQL, JavaScript, React, and Java";

    public CodeReviewAgent(Kernel kernel, GitHubPlugin? gitHubPlugin = null, JiraIntegrationAgent? jiraAgent = null)
    {
        _kernel = kernel;
        _gitHubPlugin = gitHubPlugin;
        _jiraAgent = jiraAgent;
        
        // Initialize GitHub client if token is available
        var gitHubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        if (!string.IsNullOrEmpty(gitHubToken))
        {
            _gitHubClient = new GitHubClient(new ProductHeaderValue("SemanticKernelDevHub"))
            {
                Credentials = new Credentials(gitHubToken)
            };
            
            _repoOwner = Environment.GetEnvironmentVariable("GITHUB_REPO_OWNER");
            _repoName = Environment.GetEnvironmentVariable("GITHUB_REPO_NAME");
        }
    }

    public Task InitializeAsync()
    {
        Console.WriteLine($"✅ {Name} initialized successfully");
        return Task.CompletedTask;
    }

    public Task RegisterFunctionsAsync(Kernel kernel)
    {
        kernel.ImportPluginFromObject(this, "CodeReview");
        Console.WriteLine($"🔧 {Name} functions registered with kernel");
        return Task.CompletedTask;
    }

    public IEnumerable<string> GetFunctionNames()
    {
        return new[]
        {
            "AnalyzeCode",
            "SuggestImprovements",
            "CheckCodingStandards",
            "ReviewPullRequest"
        };
    }

    /// <summary>
    /// Analyzes the provided code and gives feedback
    /// </summary>
    /// <param name="code">The code to analyze</param>
    /// <param name="language">Programming language (C#, VB.NET, T-SQL, JavaScript, React, Java)</param>
    /// <returns>Code analysis results</returns>
    [KernelFunction("analyze_code")]
    [Description("Analyzes code quality and provides detailed feedback for C#, VB.NET, T-SQL, JavaScript, React, and Java")]
    public async Task<string> AnalyzeCode(
        [Description("The code to analyze")] string code,
        [Description("Programming language: C#, VB.NET, T-SQL, JavaScript, React, or Java")] string language = "C#")
    {
        // Validate supported language
        var supportedLanguages = new[] { "C#", "VB.NET", "T-SQL", "JavaScript", "React", "Java" };
        if (!supportedLanguages.Contains(language, StringComparer.OrdinalIgnoreCase))
        {
            return $"❌ Unsupported language: {language}. Supported languages are: {string.Join(", ", supportedLanguages)}";
        }

        var languageSpecificGuidance = GetLanguageSpecificGuidance(language);
        
        var prompt = $@"
You are an expert code reviewer specializing in {language}. Analyze the following {language} code and provide:

1. **Code Quality Assessment**: Rate the overall quality (1-10)
2. **Strengths**: What's done well
3. **Issues Found**: Any bugs, inefficiencies, or problems specific to {language}
4. **Suggestions**: Specific improvements with {language} examples
5. **Best Practices**: {language}-specific recommendations

{languageSpecificGuidance}

Code to analyze:
```{GetLanguageCodeBlock(language)}
{code}
```

Please provide a comprehensive but concise review focusing on {language} best practices.";

        var response = await _kernel.InvokePromptAsync(prompt);
        return response.ToString();
    }

    /// <summary>
    /// Suggests specific improvements for the provided code
    /// </summary>
    /// <param name="code">The code to improve</param>
    /// <param name="focus">Specific area to focus on (performance, readability, etc.)</param>
    /// <param name="language">Programming language (C#, VB.NET, T-SQL, JavaScript, React, Java)</param>
    /// <returns>Improvement suggestions</returns>
    [KernelFunction("suggest_improvements")]
    [Description("Provides specific code improvement suggestions for supported languages")]
    public async Task<string> SuggestImprovements(
        [Description("The code to improve")] string code,
        [Description("Focus area: performance, readability, maintainability, security")] string focus = "general",
        [Description("Programming language: C#, VB.NET, T-SQL, JavaScript, React, or Java")] string language = "C#")
    {
        // Validate supported language
        var supportedLanguages = new[] { "C#", "VB.NET", "T-SQL", "JavaScript", "React", "Java" };
        if (!supportedLanguages.Contains(language, StringComparer.OrdinalIgnoreCase))
        {
            return $"❌ Unsupported language: {language}. Supported languages are: {string.Join(", ", supportedLanguages)}";
        }

        var prompt = $@"
Focus on {focus} improvements for this {language} code. Provide:

1. **Priority Issues**: Most important improvements needed for {language}
2. **Refactored Code**: Show improved {language} version with explanations
3. **Performance Tips**: {language}-specific performance considerations
4. **Modern Patterns**: Current {language} best practices and patterns

Code:
```{GetLanguageCodeBlock(language)}
{code}
```

Focus area: {focus}
Language: {language}";

        var response = await _kernel.InvokePromptAsync(prompt);
        return response.ToString();
    }

    /// <summary>
    /// Checks code against common coding standards
    /// </summary>
    /// <param name="code">Code to check</param>
    /// <param name="standard">Coding standard to check against</param>
    /// <param name="language">Programming language (C#, VB.NET, T-SQL, JavaScript, React, Java)</param>
    /// <returns>Standards compliance report</returns>
    [KernelFunction("check_coding_standards")]
    [Description("Checks code compliance with coding standards for supported languages")]
    public async Task<string> CheckCodingStandards(
        [Description("The code to check")] string code,
        [Description("Coding standard to apply")] string standard = "Language Default",
        [Description("Programming language: C#, VB.NET, T-SQL, JavaScript, React, or Java")] string language = "C#")
    {
        // Validate supported language
        var supportedLanguages = new[] { "C#", "VB.NET", "T-SQL", "JavaScript", "React", "Java" };
        if (!supportedLanguages.Contains(language, StringComparer.OrdinalIgnoreCase))
        {
            return $"❌ Unsupported language: {language}. Supported languages are: {string.Join(", ", supportedLanguages)}";
        }

        // Get language-specific standard if default is requested
        if (standard == "Language Default")
        {
            standard = GetDefaultStandard(language);
        }

        var prompt = $@"
Review this {language} code against {standard} coding standards. Check for:

1. **Naming Conventions**: Variables, methods, classes following {language} standards
2. **Code Structure**: Formatting, indentation, organization for {language}
3. **Documentation**: Comments, documentation appropriate for {language}
4. **Language Patterns**: Proper use of {language} features and idioms
5. **Compliance Score**: Rate adherence to {standard} standards (1-10)

Code to review:
```{GetLanguageCodeBlock(language)}
{code}
```

Language: {language}
Standard: {standard}";

        var response = await _kernel.InvokePromptAsync(prompt);
        return response.ToString();
    }

    /// <summary>
    /// Reviews a pull request (placeholder for GitHub integration)
    /// </summary>
    /// <param name="pullRequestNumber">PR number to review</param>
    /// <returns>Pull request review summary</returns>
    [KernelFunction("review_pull_request")]
    [Description("Reviews a GitHub pull request")]
    public async Task<string> ReviewPullRequest(
        [Description("Pull request number to review")] int pullRequestNumber)
    {
        if (_gitHubClient == null || string.IsNullOrEmpty(_repoOwner) || string.IsNullOrEmpty(_repoName))
        {
            return "❌ GitHub integration not configured. Please set GITHUB_TOKEN, GITHUB_REPO_OWNER, and GITHUB_REPO_NAME in your .env file.";
        }

        try
        {
            // Get PR details from GitHub
            var pullRequest = await _gitHubClient.PullRequest.Get(_repoOwner, _repoName, pullRequestNumber);
            var files = await _gitHubClient.PullRequest.Files(_repoOwner, _repoName, pullRequestNumber);

            var summary = $@"
📋 **Pull Request Review Summary**

**PR #{pullRequestNumber}**: {pullRequest.Title}
**Author**: {pullRequest.User.Login}
**Status**: {pullRequest.State}
**Files Changed**: {files.Count}

**Description**: {pullRequest.Body ?? "No description provided"}

**Files to Review**:
{string.Join("\n", files.Take(5).Select(f => $"- {f.FileName} (+{f.Additions}/-{f.Deletions})"))}
{(files.Count > 5 ? $"... and {files.Count - 5} more files" : "")}

*Note: Detailed code analysis would require additional implementation to fetch and analyze file contents.*";

            return summary;
        }
        catch (Exception ex)
        {
            return $"❌ Error reviewing pull request #{pullRequestNumber}: {ex.Message}";
        }
    }

    /// <summary>
    /// Reviews a specific commit using GitHub integration
    /// </summary>
    /// <param name="commitSha">The commit SHA to review</param>
    /// <returns>Structured code review result</returns>
    [KernelFunction("review_commit")]
    [Description("Reviews a specific GitHub commit with detailed analysis")]
    public async Task<CodeReviewResult> ReviewCommit(
        [Description("The SHA of the commit to review")] string commitSha)
    {
        if (_gitHubPlugin == null)
        {
            throw new InvalidOperationException("GitHub plugin not available for commit review");
        }

        var startTime = DateTime.Now;
        var result = new CodeReviewResult
        {
            Metadata = new ReviewMetadata
            {
                ReviewType = CodeReviewType.Commit,
                Target = commitSha,
                ReviewDate = startTime
            }
        };

        try
        {
            // Get commit details
            var commitInfo = await _gitHubPlugin.GetCommitDetails(commitSha);
            
            // Filter to supported languages only
            var supportedFiles = commitInfo.FilesChanged
                .Where(f => f.IsSupportedForReview)
                .ToList();

            result.Metadata.TotalFilesAnalyzed = supportedFiles.Count;

            // Review each file
            foreach (var file in supportedFiles.Take(10)) // Limit to 10 files
            {
                var fileReview = await ReviewFile(file);
                result.FileReviews.Add(fileReview);
            }

            // Calculate overall score and generate summary
            result.OverallScore = result.FileReviews.Any() 
                ? (int)result.FileReviews.Average(f => f.Score)
                : 0;

            result.KeyIssues = result.FileReviews
                .SelectMany(f => f.Issues)
                .GroupBy(issue => issue)
                .Where(g => g.Count() > 1) // Issues that appear in multiple files
                .Select(g => $"{g.Key} (found in {g.Count()} files)")
                .ToList();

            result.Recommendations = GenerateRecommendations(result.FileReviews);

            result.Summary = await GenerateCommitSummary(commitInfo, result);

            result.Metadata.ReviewDuration = DateTime.Now - startTime;

            return result;
        }
        catch (Exception ex)
        {
            result.Summary = $"❌ Error reviewing commit {commitSha}: {ex.Message}";
            result.OverallScore = 0;
            result.Metadata.ReviewDuration = DateTime.Now - startTime;
            return result;
        }
    }

    /// <summary>
    /// Reviews the latest commit in the repository
    /// </summary>
    /// <returns>Code review result for the latest commit</returns>
    [KernelFunction("review_latest_commit")]
    [Description("Reviews the most recent commit in the repository")]
    public async Task<CodeReviewResult> ReviewLatestCommit()
    {
        if (_gitHubPlugin == null)
        {
            throw new InvalidOperationException("GitHub plugin not available");
        }

        var commits = await _gitHubPlugin.GetRecentCommits(1);
        if (!commits.Any())
        {
            return new CodeReviewResult
            {
                Summary = "No commits found in repository",
                OverallScore = 0
            };
        }

        return await ReviewCommit(commits.First().Sha);
    }

    /// <summary>
    /// Lists recent commits from the repository
    /// </summary>
    /// <param name="count">Number of commits to retrieve</param>
    /// <returns>List of recent commits</returns>
    [KernelFunction("list_recent_commits")]
    [Description("Lists recent commits from the GitHub repository")]
    public async Task<List<GitHubCommitInfo>> ListRecentCommits(
        [Description("Number of commits to retrieve (1-20)")] int count = 10)
    {
        if (_gitHubPlugin == null)
        {
            throw new InvalidOperationException("GitHub plugin not available");
        }

        count = Math.Max(1, Math.Min(count, 20)); // Limit between 1-20
        return await _gitHubPlugin.GetRecentCommits(count);
    }

    /// <summary>
    /// Reviews a file change from GitHub
    /// </summary>
    private async Task<FileReviewResult> ReviewFile(GitHubFileInfo file)
    {
        var fileReview = new FileReviewResult
        {
            FileName = file.FileName,
            Language = file.DetectedLanguage
        };

        try
        {
            if (string.IsNullOrEmpty(file.Patch))
            {
                fileReview.Review = "No changes to review";
                fileReview.Score = 10;
                return fileReview;
            }

            var codeToReview = file.GetCodeContent();
            if (string.IsNullOrWhiteSpace(codeToReview))
            {
                fileReview.Review = "No substantial code changes found";
                fileReview.Score = 10;
                return fileReview;
            }

            // Analyze the code using existing methods
            var analysis = await AnalyzeCode(codeToReview, file.DetectedLanguage);
            
            // Parse the analysis to extract score and details
            fileReview.Review = analysis;
            fileReview.Score = ExtractScoreFromAnalysis(analysis);
            fileReview.Issues = ExtractIssuesFromAnalysis(analysis);
            fileReview.Suggestions = ExtractSuggestionsFromAnalysis(analysis);

            return fileReview;
        }
        catch (Exception ex)
        {
            fileReview.Review = $"Error analyzing file: {ex.Message}";
            fileReview.Score = 0;
            return fileReview;
        }
    }

    /// <summary>
    /// Generates an overall summary for the commit review
    /// </summary>
    private async Task<string> GenerateCommitSummary(GitHubCommitInfo commitInfo, CodeReviewResult result)
    {
        var prompt = $@"
Create a comprehensive commit review summary based on the following information:

**Commit Information:**
- SHA: {commitInfo.Sha}
- Message: {commitInfo.Message}
- Author: {commitInfo.Author}
- Files Changed: {commitInfo.FilesChanged.Count}
- Total Additions: {commitInfo.TotalAdditions}
- Total Deletions: {commitInfo.TotalDeletions}

**Review Results:**
- Overall Score: {result.OverallScore}/10
- Files Reviewed: {result.FileReviews.Count}
- Key Issues: {string.Join(", ", result.KeyIssues)}

**File Reviews:**
{string.Join("\n", result.FileReviews.Select(f => $"- {f.FileName} ({f.Language}): {f.Score}/10"))}

Provide a concise summary that includes:
1. Overall assessment of the commit quality
2. Main strengths and concerns
3. Impact assessment
4. Key recommendations for improvement

Keep it professional and actionable.";

        var response = await _kernel.InvokePromptAsync(prompt);
        return response.ToString();
    }

    /// <summary>
    /// Generates recommendations based on file reviews
    /// </summary>
    private List<string> GenerateRecommendations(List<FileReviewResult> fileReviews)
    {
        var recommendations = new List<string>();

        var lowScoreFiles = fileReviews.Where(f => f.Score < 6).ToList();
        if (lowScoreFiles.Any())
        {
            recommendations.Add($"Review and improve {lowScoreFiles.Count} files with low quality scores");
        }

        var commonIssues = fileReviews
            .SelectMany(f => f.Issues)
            .GroupBy(issue => issue.Split(' ').Take(3).Aggregate("", (a, b) => a + " " + b).Trim())
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        foreach (var issue in commonIssues)
        {
            recommendations.Add($"Address common issue across multiple files: {issue}");
        }

        if (fileReviews.Any(f => f.Language == "C#" && f.Issues.Any(i => i.Contains("async"))))
        {
            recommendations.Add("Review async/await patterns in C# code");
        }

        if (fileReviews.Any(f => f.Language == "JavaScript" && f.Issues.Any(i => i.Contains("var"))))
        {
            recommendations.Add("Consider using let/const instead of var in JavaScript");
        }

        return recommendations.Any() ? recommendations : new List<string> { "No specific recommendations - code quality looks good!" };
    }

    /// <summary>
    /// Extracts quality score from analysis text
    /// </summary>
    private int ExtractScoreFromAnalysis(string analysis)
    {
        // Look for patterns like "Quality Assessment: X/10" or "Assessment: X"
        var patterns = new[]
        {
            @"Quality Assessment[:\s]*(\d+)/10",
            @"Assessment[:\s]*(\d+)/10",
            @"Quality[:\s]*(\d+)/10",
            @"Score[:\s]*(\d+)/10",
            @"Assessment[:\s]*(\d+)"
        };

        foreach (var pattern in patterns)
        {
            var match = System.Text.RegularExpressions.Regex.Match(analysis, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (match.Success && int.TryParse(match.Groups[1].Value, out var score))
            {
                return Math.Max(1, Math.Min(score, 10)); // Ensure between 1-10
            }
        }

        return 5; // Default neutral score if not found
    }

    /// <summary>
    /// Extracts issues from analysis text
    /// </summary>
    private List<string> ExtractIssuesFromAnalysis(string analysis)
    {
        var issues = new List<string>();
        
        // Look for sections with issues
        var issuePatterns = new[]
        {
            @"Issues Found[:\s]*\n?(.+?)(?=\n\*\*|\n\d+\.|\z)",
            @"Problems[:\s]*\n?(.+?)(?=\n\*\*|\n\d+\.|\z)",
            @"Concerns[:\s]*\n?(.+?)(?=\n\*\*|\n\d+\.|\z)"
        };

        foreach (var pattern in issuePatterns)
        {
            var match = System.Text.RegularExpressions.Regex.Match(analysis, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
            if (match.Success)
            {
                var issueText = match.Groups[1].Value.Trim();
                var lines = issueText.Split('\n')
                    .Where(line => !string.IsNullOrWhiteSpace(line) && line.Trim().StartsWith("-"))
                    .Select(line => line.Trim().TrimStart('-').Trim())
                    .ToList();
                issues.AddRange(lines);
            }
        }

        return issues.Any() ? issues : new List<string>();
    }

    /// <summary>
    /// Extracts suggestions from analysis text
    /// </summary>
    private List<string> ExtractSuggestionsFromAnalysis(string analysis)
    {
        var suggestions = new List<string>();
        
        // Look for sections with suggestions
        var suggestionPatterns = new[]
        {
            @"Suggestions[:\s]*\n?(.+?)(?=\n\*\*|\n\d+\.|\z)",
            @"Recommendations[:\s]*\n?(.+?)(?=\n\*\*|\n\d+\.|\z)",
            @"Improvements[:\s]*\n?(.+?)(?=\n\*\*|\n\d+\.|\z)"
        };

        foreach (var pattern in suggestionPatterns)
        {
            var match = System.Text.RegularExpressions.Regex.Match(analysis, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
            if (match.Success)
            {
                var suggestionText = match.Groups[1].Value.Trim();
                var lines = suggestionText.Split('\n')
                    .Where(line => !string.IsNullOrWhiteSpace(line) && line.Trim().StartsWith("-"))
                    .Select(line => line.Trim().TrimStart('-').Trim())
                    .ToList();
                suggestions.AddRange(lines);
            }
        }

        return suggestions.Any() ? suggestions : new List<string>();
    }

    // ...existing helper methods...
    
    /// <summary>
    /// Gets language-specific guidance for code review
    /// </summary>
    /// <param name="language">The programming language</param>
    /// <returns>Language-specific review guidance</returns>
    private static string GetLanguageSpecificGuidance(string language)
    {
        return language.ToUpper() switch
        {
            "C#" => @"
**C# Specific Focus Areas**:
- Proper use of async/await patterns
- LINQ usage and performance
- Memory management and IDisposable
- Nullable reference types
- Exception handling best practices
- XML documentation comments",

            "VB.NET" => @"
**VB.NET Specific Focus Areas**:
- Proper variable declarations (Option Strict)
- Error handling with Try/Catch
- Object lifecycle management
- .NET Framework/Core compatibility
- Performance considerations",

            "T-SQL" => @"
**T-SQL Specific Focus Areas**:
- Query performance and execution plans
- Proper indexing strategies
- SQL injection prevention
- Transaction management
- Set-based operations vs cursors
- Stored procedure best practices",

            "JAVASCRIPT" => @"
**JavaScript Specific Focus Areas**:
- ES6+ modern syntax usage
- Async/await vs Promises
- Variable scoping (let/const vs var)
- Error handling and validation
- Performance considerations
- Browser compatibility",

            "REACT" => @"
**React Specific Focus Areas**:
- Component lifecycle and hooks
- State management patterns
- Performance optimization (memo, useMemo, useCallback)
- Props validation and TypeScript usage
- Accessibility considerations
- Testing best practices",

            "JAVA" => @"
**Java Specific Focus Areas**:
- Object-oriented design principles
- Exception handling best practices
- Memory management and garbage collection
- Concurrency and thread safety
- Design patterns implementation
- Code organization and packages",

            _ => "**General Focus Areas**: Code structure, readability, and maintainability"
        };
    }

    /// <summary>
    /// Gets the appropriate code block language identifier
    /// </summary>
    /// <param name="language">The programming language</param>
    /// <returns>Code block identifier</returns>
    private static string GetLanguageCodeBlock(string language)
    {
        return language.ToUpper() switch
        {
            "C#" => "csharp",
            "VB.NET" => "vbnet",
            "T-SQL" => "sql",
            "JAVASCRIPT" => "javascript",
            "REACT" => "jsx",
            "JAVA" => "java",
            _ => "text"
        };
    }

    /// <summary>
    /// Gets the default coding standard for a language
    /// </summary>
    /// <param name="language">The programming language</param>
    /// <returns>Default coding standard for the language</returns>
    private static string GetDefaultStandard(string language)
    {
        return language.ToUpper() switch
        {
            "C#" => "Microsoft C# Coding Conventions",
            "VB.NET" => "Microsoft VB.NET Coding Conventions",
            "T-SQL" => "SQL Server Best Practices",
            "JAVASCRIPT" => "ESLint Recommended + Airbnb Style Guide",
            "REACT" => "React Best Practices + ESLint React",
            "JAVA" => "Oracle Java Code Conventions",
            _ => "General Coding Standards"
        };
    }
}
