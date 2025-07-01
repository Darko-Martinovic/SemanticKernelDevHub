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

    public string Description =>
        "Analyzes code quality, suggests improvements, and performs automated code reviews for C#, VB.NET, T-SQL, JavaScript, React, and Java";

    public CodeReviewAgent(
        Kernel kernel,
        GitHubPlugin? gitHubPlugin = null,
        JiraIntegrationAgent? jiraAgent = null
    )
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
    /// /// <param name="code">The code to analyze</param>
    /// <param name="language">Programming language (C#, VB.NET, T-SQL, JavaScript, React, Java)</param>
    /// <returns>Code analysis results</returns>
    [KernelFunction("analyze_code")]
    [Description(
        "Analyzes code quality and provides detailed feedback for C#, VB.NET, T-SQL, JavaScript, React, and Java"
    )]
    public async Task<string> AnalyzeCode(
        [Description("The code to analyze")] string code,
        [Description("Programming language: C#, VB.NET, T-SQL, JavaScript, React, or Java")]
            string language = "C#"
    )
    {
        // Validate supported language
        var supportedLanguages = new[] { "C#", "VB.NET", "T-SQL", "JavaScript", "React", "Java" };
        if (!supportedLanguages.Contains(language, StringComparer.OrdinalIgnoreCase))
        {
            return $"❌ Unsupported language: {language}. Supported languages are: {string.Join(", ", supportedLanguages)}";
        }

        var languageSpecificGuidance = GetLanguageSpecificGuidance(language);

        var prompt =
            $@"
You are a senior code reviewer and architect specializing in {language} with over 10 years of experience. Provide an extremely detailed and thorough analysis of the following {language} code. Your review should be comprehensive, educational, and actionable.

## Review Structure (Provide detailed analysis for each section):

### 1. **Code Quality Assessment**
- Rate the overall quality (1-10) with detailed justification
- Assess readability, maintainability, and complexity
- Evaluate adherence to SOLID principles and design patterns
- Comment on code organization and structure

### 2. **Detailed Strengths Analysis**
- Identify specific well-implemented patterns or practices
- Highlight good use of {language} features and idioms
- Recognize proper error handling, logging, or documentation
- Praise appropriate design decisions and architectural choices

### 3. **Critical Issues & Bug Analysis**
- Identify potential bugs, race conditions, or logic errors
- Spot memory leaks, resource management issues, or performance bottlenecks
- Highlight security vulnerabilities or injection risks
- Point out exception handling problems or edge cases
- Flag any anti-patterns or code smells

### 4. **Specific Improvement Suggestions**
- Provide concrete, actionable recommendations with before/after examples
- Suggest refactoring opportunities with clear benefits
- Recommend modern {language} features or library usage
- Propose better algorithms or data structures where applicable
- Include specific code snippets showing improvements

### 5. **Best Practices & Standards Compliance**
- Evaluate naming conventions and coding standards adherence
- Assess documentation quality and completeness
- Check for proper unit testing considerations
- Review accessibility, performance, and scalability aspects

### 6. **Educational Insights**
- Explain WHY certain changes are recommended
- Provide context about {language} ecosystem best practices
- Share relevant design patterns or architectural concepts
- Include links to documentation or resources when applicable

{languageSpecificGuidance}

## Code to analyze:
```{GetLanguageCodeBlock(language)}
{code}
```

## Instructions:
- Be thorough and educational in your analysis
- Provide specific examples and explanations
- Focus on both immediate fixes and long-term improvements
- Consider maintainability, scalability, and team collaboration
- Include severity levels for issues (Critical, High, Medium, Low)
- Make your feedback actionable with clear next steps";

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
        [Description("Focus area: performance, readability, maintainability, security")]
            string focus = "general",
        [Description("Programming language: C#, VB.NET, T-SQL, JavaScript, React, or Java")]
            string language = "C#"
    )
    {
        // Validate supported language
        var supportedLanguages = new[] { "C#", "VB.NET", "T-SQL", "JavaScript", "React", "Java" };
        if (!supportedLanguages.Contains(language, StringComparer.OrdinalIgnoreCase))
        {
            return $"❌ Unsupported language: {language}. Supported languages are: {string.Join(", ", supportedLanguages)}";
        }

        var prompt =
            $@"
You are a senior software architect and {language} expert conducting a detailed code improvement analysis. Your goal is to provide comprehensive, actionable guidance that will significantly enhance the code quality.

## Comprehensive Improvement Analysis

### 1. **Priority Issues Analysis** (Focus: {focus})
- Identify and rank critical issues that need immediate attention
- Explain the impact of each issue on {focus}
- Provide risk assessment for leaving issues unaddressed
- Suggest implementation timeline for fixes

### 2. **Detailed Refactoring Plan**
Show improved {language} versions with comprehensive explanations:
- **Before/After Code Examples**: Provide complete refactored code snippets
- **Change Rationale**: Explain why each change improves {focus}
- **Implementation Steps**: Break down complex refactoring into manageable steps
- **Testing Considerations**: Suggest how to verify improvements work correctly

### 3. **Performance & Optimization Deep Dive**
Provide {language}-specific performance insights:
- **Bottleneck Analysis**: Identify performance-critical sections
- **Memory Usage**: Analyze memory efficiency and garbage collection impact
- **Algorithmic Complexity**: Evaluate time/space complexity improvements
- **Profiling Recommendations**: Suggest tools and metrics to measure improvements

### 4. **Modern {language} Patterns & Features**
Recommend current best practices:
- **Latest Language Features**: Show how newer {language} features can improve the code
- **Design Patterns**: Suggest appropriate patterns for better architecture
- **Library Recommendations**: Propose well-established libraries that could help
- **Framework Integration**: Consider how code fits into modern {language} frameworks

### 5. **Long-term Maintainability Strategy**
Focus on sustainable code improvements:
- **Code Organization**: Suggest better file/module structure
- **Documentation Strategy**: Recommend documentation improvements
- **Testing Strategy**: Propose comprehensive testing approaches
- **Team Collaboration**: Consider how changes affect team productivity

### 6. **Risk Assessment & Migration Plan**
- **Breaking Changes**: Identify potential breaking changes and mitigation strategies
- **Backward Compatibility**: Suggest approaches to maintain compatibility
- **Rollback Plans**: Recommend how to safely revert changes if needed
- **Monitoring**: Propose metrics to track improvement success

Code to improve:
```{GetLanguageCodeBlock(language)}
{code}
```

**Focus Area**: {focus}
**Programming Language**: {language}

## Instructions:
- Provide detailed, executable examples
- Explain the business impact of improvements
- Consider both technical debt reduction and feature enhancement
- Include estimated effort levels for each suggestion
- Make recommendations scalable for team adoption";

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
        [Description("Programming language: C#, VB.NET, T-SQL, JavaScript, React, or Java")]
            string language = "C#"
    )
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

        var prompt =
            $@"
You are a senior code quality engineer and {language} standards expert conducting a comprehensive coding standards audit. Your analysis should be thorough, educational, and provide clear guidance for improvement.

## Comprehensive Standards Compliance Review

### 1. **Detailed Naming Conventions Analysis**
Evaluate and provide specific feedback on:
- **Class Names**: Check PascalCase, descriptive naming, avoiding abbreviations
- **Method Names**: Verify verb-noun patterns, clarity, and consistency
- **Variable Names**: Assess camelCase, meaningful names, avoid single letters
- **Constants**: Review UPPER_CASE or PascalCase depending on {language} conventions
- **Interfaces**: Check proper prefixing and naming patterns
- **Namespaces/Packages**: Evaluate organization and hierarchy

### 2. **Code Structure & Organization Assessment**
Provide detailed analysis of:
- **File Organization**: Assess logical grouping and separation of concerns
- **Indentation & Formatting**: Check consistency with {standard} guidelines
- **Line Length**: Evaluate readability and screen-width considerations
- **Whitespace Usage**: Review spacing around operators, braces, and blocks
- **Method Length**: Assess Single Responsibility Principle adherence
- **Class Size**: Evaluate complexity and potential for refactoring

### 3. **Documentation Quality Review**
Comprehensive evaluation of:
- **Code Comments**: Assess quality, necessity, and clarity of inline comments
- **API Documentation**: Review method/class documentation completeness
- **README/Documentation**: Evaluate supporting documentation quality
- **Code Self-Documentation**: Assess how well code explains itself
- **Comment-to-Code Ratio**: Analyze appropriate documentation levels

### 4. **Language-Specific Pattern Compliance**
Detailed assessment of {language} best practices:
- **Design Patterns**: Evaluate proper implementation of common patterns
- **Language Idioms**: Check for proper use of {language}-specific features
- **Error Handling**: Assess exception handling patterns and conventions
- **Resource Management**: Review memory/resource cleanup practices
- **Async Patterns**: Evaluate asynchronous programming implementations
- **Type Safety**: Assess type usage and null safety practices

### 5. **Maintainability & Readability Metrics**
Provide comprehensive analysis of:
- **Cyclomatic Complexity**: Assess method and class complexity levels
- **Code Duplication**: Identify repeated patterns and suggest consolidation
- **Dependency Management**: Evaluate coupling and cohesion
- **Testability**: Assess how easily the code can be unit tested
- **Extensibility**: Review how well code supports future modifications

### 6. **Security & Performance Standards**
Evaluate compliance with security and performance guidelines:
- **Input Validation**: Check for proper sanitization and validation
- **SQL Injection Prevention**: Assess database interaction security
- **Performance Patterns**: Review efficient algorithm and data structure usage
- **Memory Management**: Evaluate resource usage and cleanup
- **Logging Standards**: Assess appropriate logging levels and practices

### 7. **Team Collaboration Standards**
Review aspects affecting team productivity:
- **Code Consistency**: Evaluate consistency across the codebase
- **Version Control**: Assess commit message quality and change organization
- **Code Review Readiness**: Evaluate how easily code can be reviewed
- **Onboarding Friendliness**: Assess how easily new team members can understand code

### 8. **Detailed Compliance Scoring & Action Plan**
Provide comprehensive scoring breakdown:
- **Overall Compliance Score**: Rate adherence to {standard} (1-10) with detailed justification
- **Category Scores**: Break down scores by major areas (naming, structure, documentation, etc.)
- **Priority Fixes**: List issues in order of importance with estimated effort
- **Quick Wins**: Identify easy improvements that provide immediate value
- **Long-term Improvements**: Suggest strategic changes for better standards compliance

Code to review:
```{GetLanguageCodeBlock(language)}
{code}
```

**Programming Language**: {language}
**Coding Standard**: {standard}

## Instructions:
- Provide specific examples of violations with corrections
- Explain WHY each standard exists and its benefits
- Include before/after code snippets for major suggestions
- Consider both individual developer and team-wide impact
- Prioritize suggestions based on impact and effort required
- Make recommendations actionable with clear next steps";

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
        [Description("Pull request number to review")] int pullRequestNumber
    )
    {
        if (
            _gitHubClient == null
            || string.IsNullOrEmpty(_repoOwner)
            || string.IsNullOrEmpty(_repoName)
        )
        {
            return "❌ GitHub integration not configured. Please set GITHUB_TOKEN, GITHUB_REPO_OWNER, and GITHUB_REPO_NAME in your .env file.";
        }

        try
        {
            // Get PR details from GitHub
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

            var summary =
                $@"
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
        [Description("The SHA of the commit to review")] string commitSha
    )
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
                ReviewDate = startTime,
                RepositoryName = _repoName ?? "Unknown Repository"
            }
        };

        try
        {
            // Get commit details
            var commitInfo = await _gitHubPlugin.GetCommitDetails(commitSha);

            // Get branch information for the commit
            try
            {
                var branchName = await _gitHubPlugin.GetCommitBranch(commitSha);
                result.Metadata.BranchName = branchName;
                commitInfo.BranchName = branchName;
            }
            catch
            {
                // If branch detection fails, continue without it
                result.Metadata.BranchName = "unknown";
            }

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
                OverallScore = 0,
                Metadata = new ReviewMetadata
                {
                    ReviewType = CodeReviewType.Latest,
                    RepositoryName = _repoName ?? "Unknown Repository",
                    BranchName = "main", // Default to main branch when no commits found
                    ReviewDate = DateTime.Now
                }
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
        [Description("Number of commits to retrieve (1-20)")] int count = 10
    )
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
    /// Generates a comprehensive, detailed summary for commit review that provides thorough analysis
    /// and actionable insights. This method creates in-depth commit reviews that help developers
    /// understand the impact, quality, and improvement opportunities of their changes.
    /// </summary>
    /// <param name="commitInfo">Detailed information about the commit being reviewed</param>
    /// <param name="result">The complete code review result containing all analysis data</param>
    /// <returns>A comprehensive, educational commit summary with detailed insights and recommendations</returns>
    private async Task<string> GenerateCommitSummary(
        GitHubCommitInfo commitInfo,
        CodeReviewResult result
    )
    {
        var prompt =
            $@"
You are a senior engineering manager and code review expert creating a comprehensive commit review summary. Your analysis should be thorough, educational, and provide clear strategic guidance for the development team.

## Commit Analysis Data:

### **Commit Metadata:**
- **SHA**: {commitInfo.Sha}
- **Commit Message**: ""{commitInfo.Message}""
- **Author**: {commitInfo.Author}
- **Files Modified**: {commitInfo.FilesChanged.Count} files
- **Lines Added**: {commitInfo.TotalAdditions}
- **Lines Deleted**: {commitInfo.TotalDeletions}
- **Net Change**: {commitInfo.TotalAdditions - commitInfo.TotalDeletions} lines

### **Code Review Results:**
- **Overall Quality Score**: {result.OverallScore}/10
- **Files Successfully Reviewed**: {result.FileReviews.Count}
- **Average File Score**: {(result.FileReviews.Any() ? result.FileReviews.Average(f => f.Score).ToString("F1") : "N/A")}
- **Critical Issues Identified**: {result.KeyIssues.Count}

### **Individual File Analysis:**
{string.Join("\n", result.FileReviews.Select(f => $"- **{f.FileName}** ({f.Language}): Score {f.Score}/10, Issues: {f.Issues.Count}, Suggestions: {f.Suggestions.Count}"))}

### **Cross-File Issues:**
{(result.KeyIssues.Any() ? string.Join("\n", result.KeyIssues.Select(issue => $"- {issue}")) : "No recurring issues found across multiple files")}

## Required Summary Structure:

### **1. Executive Summary**
Provide a high-level assessment of the commit's overall impact and quality:
- **Business Impact**: How does this commit affect the product/system?
- **Technical Quality**: Overall assessment of code quality and implementation
- **Risk Assessment**: Potential risks introduced by these changes
- **Strategic Alignment**: How well changes align with project goals

### **2. Detailed Technical Analysis**
Provide comprehensive technical evaluation:
- **Architecture & Design**: Evaluate design decisions and architectural impact
- **Code Quality Metrics**: Analyze quality scores and their implications
- **Performance Implications**: Assess potential performance impacts
- **Maintainability Impact**: How changes affect long-term maintainability
- **Security Considerations**: Any security implications of the changes

### **3. Strengths & Positive Highlights**
Identify and elaborate on what was done well:
- **Best Practices Followed**: Specific examples of good implementation
- **Code Quality Improvements**: Areas where quality was enhanced
- **Innovation & Efficiency**: Creative or efficient solutions implemented
- **Documentation & Testing**: Quality of supporting documentation

### **4. Areas for Improvement**
Provide detailed, actionable improvement guidance:
- **Critical Issues**: Issues that need immediate attention with specific remediation steps
- **Code Quality Concerns**: Areas where quality could be enhanced with examples
- **Performance Optimizations**: Specific opportunities for performance improvements
- **Maintainability Enhancements**: Changes that would improve long-term maintainability

### **5. Strategic Recommendations**
Provide forward-looking guidance:
- **Immediate Actions**: What should be done right now before merging/deploying
- **Short-term Improvements**: Changes to consider in upcoming iterations
- **Long-term Considerations**: Strategic improvements for future development
- **Team Learning Opportunities**: Lessons that can benefit the entire team

### **6. Quality Gates & Deployment Readiness**
Assess readiness for next steps:
- **Merge Readiness**: Is this commit ready for merging? What conditions must be met?
- **Testing Requirements**: What testing should be performed before deployment?
- **Monitoring Needs**: What should be monitored after deployment?
- **Rollback Planning**: Contingency plans if issues arise

## Instructions:
- Be specific and actionable in all recommendations
- Provide educational context explaining WHY improvements matter
- Consider both immediate and long-term implications
- Balance constructive criticism with recognition of good work
- Make the summary valuable for both the author and reviewing team members
- Include estimated effort levels for suggested improvements
- Consider the broader codebase and team context";

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
            recommendations.Add(
                $"Review and improve {lowScoreFiles.Count} files with low quality scores"
            );
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

        if (
            fileReviews.Any(f => f.Language == "JavaScript" && f.Issues.Any(i => i.Contains("var")))
        )
        {
            recommendations.Add("Consider using let/const instead of var in JavaScript");
        }

        return recommendations.Any()
            ? recommendations
            : new List<string> { "No specific recommendations - code quality looks good!" };
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
            var match = System.Text.RegularExpressions.Regex.Match(
                analysis,
                pattern,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );
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
            var match = System.Text.RegularExpressions.Regex.Match(
                analysis,
                pattern,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
                    | System.Text.RegularExpressions.RegexOptions.Singleline
            );
            if (match.Success)
            {
                var issueText = match.Groups[1].Value.Trim();
                var lines = issueText
                    .Split('\n')
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
            var match = System.Text.RegularExpressions.Regex.Match(
                analysis,
                pattern,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
                    | System.Text.RegularExpressions.RegexOptions.Singleline
            );
            if (match.Success)
            {
                var suggestionText = match.Groups[1].Value.Trim();
                var lines = suggestionText
                    .Split('\n')
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
    /// Provides comprehensive, detailed language-specific guidance for thorough code review.
    /// This method returns extensive review criteria that help generate in-depth, educational
    /// code analysis comments focusing on language-specific best practices, patterns, and potential issues.
    /// </summary>
    /// <param name="language">The programming language being reviewed</param>
    /// <returns>Detailed, comprehensive language-specific review guidance with specific focus areas and examples</returns>
    private static string GetLanguageSpecificGuidance(string language)
    {
        return language.ToUpper() switch
        {
            "C#"
                => @"
**C# Comprehensive Review Focus Areas**:

**1. Async/Await Patterns & Threading:**
- Check for proper async/await usage vs .Result/.Wait() blocking calls
- Verify ConfigureAwait(false) usage in library code
- Look for potential deadlocks in mixed sync/async code
- Evaluate Task.Run usage and thread pool efficiency
- Assess CancellationToken propagation and timeout handling

**2. Memory Management & Performance:**
- Review IDisposable implementation and using statement usage
- Check for memory leaks in event subscriptions and weak references
- Evaluate LINQ performance vs traditional loops for large datasets
- Assess string concatenation efficiency (StringBuilder vs string interpolation)
- Review struct vs class usage for performance-critical scenarios

**3. Modern C# Features & Patterns:**
- Evaluate nullable reference types usage and null safety
- Check for proper record types vs class usage
- Assess pattern matching and switch expressions utilization
- Review init-only properties and immutability patterns
- Evaluate local functions vs lambda expressions appropriateness

**4. Error Handling & Resilience:**
- Review exception handling granularity and custom exception types
- Check for proper exception filtering and rethrowing patterns
- Assess logging patterns and structured logging usage
- Evaluate fault tolerance and retry mechanisms
- Review input validation and defensive programming practices

**5. Architecture & Design Patterns:**
- Assess SOLID principles adherence (especially DI and SRP)
- Review dependency injection patterns and container usage
- Evaluate factory patterns and object creation strategies
- Check for proper separation of concerns and layering
- Assess unit testability and mock-friendly design",

            "VB.NET"
                => @"
**VB.NET Comprehensive Review Focus Areas**:

**1. Language-Specific Best Practices:**
- Verify Option Strict On usage for type safety
- Check Option Explicit On for variable declaration requirements
- Review Option Infer usage and type inference patterns
- Assess proper variable scope declarations (Dim, Private, Public)
- Evaluate error handling with structured Try/Catch/Finally blocks

**2. .NET Framework/Core Compatibility:**
- Review compatibility patterns across .NET versions
- Check for deprecated methods and suggest modern alternatives
- Assess namespace usage and import optimization
- Evaluate assembly loading and reflection patterns
- Review COM interop and legacy code integration

**3. Object-Oriented Programming:**
- Check inheritance hierarchies and virtual/override patterns
- Review interface implementation and polymorphism usage
- Assess constructor chaining and initialization patterns
- Evaluate property vs field usage and encapsulation
- Review shared (static) member usage and thread safety

**4. Performance & Optimization:**
- Assess collection usage and iteration patterns
- Review string manipulation and culture-aware operations
- Check database connection management and data access patterns
- Evaluate resource disposal and cleanup patterns
- Review multithreading and synchronization mechanisms

**5. Code Organization & Maintainability:**
- Check module organization and namespace structure
- Review XML documentation completeness and accuracy
- Assess naming conventions following VB.NET standards
- Evaluate code reusability and DRY principle adherence
- Review debugging and logging implementation patterns",

            "T-SQL"
                => @"
**T-SQL Comprehensive Review Focus Areas**:

**1. Query Performance & Optimization:**
- Analyze execution plans and index usage patterns
- Check for SARGable predicates and query optimization opportunities
- Review JOIN strategies and their performance implications
- Assess subquery vs CTE vs window function usage
- Evaluate parameter sniffing issues and query plan stability

**2. Security & Best Practices:**
- Check for SQL injection vulnerabilities and parameterization
- Review privilege escalation and principle of least privilege
- Assess dynamic SQL construction and validation
- Evaluate sensitive data handling and encryption usage
- Review audit trail and logging implementation

**3. Data Integrity & Consistency:**
- Check constraint usage (CHECK, FOREIGN KEY, UNIQUE)
- Review transaction boundaries and ACID properties
- Assess error handling and rollback strategies
- Evaluate data validation and business rule enforcement
- Review concurrency control and locking strategies

**4. Modern T-SQL Features & Patterns:**
- Assess CTE usage vs temporary tables for complex queries
- Review window functions and analytical capabilities
- Check for proper data type usage and storage efficiency
- Evaluate JSON/XML handling and modern data formats
- Review temporal tables and change tracking features

**5. Database Design & Architecture:**
- Check normalization levels and denormalization strategies
- Review indexing strategies and maintenance plans
- Assess stored procedure design and parameter handling
- Evaluate view usage and performance implications
- Review database schema organization and naming conventions

**6. Maintenance & Operations:**
- Check backup and recovery strategy implementation
- Review performance monitoring and alerting mechanisms
- Assess maintenance plan effectiveness
- Evaluate capacity planning and growth patterns
- Review documentation and change management processes",

            "JAVASCRIPT"
                => @"
**JavaScript Comprehensive Review Focus Areas**:

**1. Modern JavaScript & ES6+ Features:**
- Check for proper let/const usage vs var declarations
- Review arrow function usage and this binding implications
- Assess destructuring patterns and object/array manipulation
- Evaluate template literal usage vs string concatenation
- Review async/await vs Promise patterns and error handling

**2. Performance & Optimization:**
- Assess DOM manipulation efficiency and virtual DOM patterns
- Review event handling and memory leak prevention
- Check for proper debouncing/throttling in event handlers
- Evaluate bundle size optimization and code splitting
- Review lazy loading and performance monitoring patterns

**3. Error Handling & Debugging:**
- Check comprehensive error boundaries and try/catch usage
- Review logging patterns and debugging information
- Assess input validation and sanitization practices
- Evaluate error reporting and monitoring integration
- Review testing strategies and coverage patterns

**4. Security & Best Practices:**
- Check for XSS prevention and input sanitization
- Review CSRF protection and secure communication
- Assess dependency vulnerabilities and security updates
- Evaluate authentication and authorization patterns
- Review data validation and secure storage practices

**5. Code Organization & Architecture:**
- Assess module patterns and import/export organization
- Review function composition and pure function usage
- Check for proper separation of concerns
- Evaluate design patterns implementation (Observer, Factory, etc.)
- Review code reusability and maintainability patterns

**6. Browser Compatibility & Standards:**
- Check cross-browser compatibility considerations
- Review polyfill usage and feature detection
- Assess accessibility (a11y) compliance and ARIA usage
- Evaluate responsive design implementation
- Review progressive enhancement strategies",

            "REACT"
                => @"
**React Comprehensive Review Focus Areas**:

**1. Component Design & Architecture:**
- Review component composition vs inheritance patterns
- Check for proper prop drilling avoidance and context usage
- Assess component size and single responsibility adherence
- Evaluate higher-order components vs custom hooks
- Review render prop patterns and component reusability

**2. Performance & Optimization:**
- Check React.memo, useMemo, and useCallback usage
- Review virtual DOM optimization and reconciliation efficiency
- Assess bundle splitting and lazy loading implementation
- Evaluate state management performance implications
- Review profiling and performance monitoring practices

**3. Hooks & State Management:**
- Assess custom hooks design and reusability
- Review useState vs useReducer appropriateness
- Check useEffect dependencies and cleanup patterns
- Evaluate context usage vs external state management
- Review state normalization and data structure efficiency

**4. Type Safety & Development Experience:**
- Check TypeScript integration and type definitions
- Review PropTypes usage and runtime type checking
- Assess component interface design and documentation
- Evaluate development tools integration (DevTools, linting)
- Review testing patterns and component testability

**5. Accessibility & User Experience:**
- Check ARIA attributes and semantic HTML usage
- Review keyboard navigation and focus management
- Assess screen reader compatibility and alt text
- Evaluate color contrast and visual accessibility
- Review responsive design and mobile optimization

**6. Modern React Patterns & Best Practices:**
- Assess functional vs class component usage
- Review error boundary implementation and error handling
- Check Suspense and concurrent features usage
- Evaluate code splitting and dynamic imports
- Review server-side rendering and hydration patterns

**7. Testing & Quality Assurance:**
- Check unit testing coverage and quality
- Review integration testing strategies
- Assess snapshot testing appropriateness
- Evaluate end-to-end testing implementation
- Review component testing isolation and mocking",

            "JAVA"
                => @"
**Java Comprehensive Review Focus Areas**:

**1. Object-Oriented Design & SOLID Principles:**
- Review inheritance hierarchies and composition over inheritance
- Check interface segregation and dependency inversion
- Assess encapsulation and information hiding practices
- Evaluate polymorphism usage and method overriding patterns
- Review design pattern implementation (Factory, Strategy, Observer)

**2. Memory Management & Performance:**
- Check for memory leaks and object lifecycle management
- Review garbage collection implications and optimization
- Assess collection framework usage and algorithm efficiency
- Evaluate autoboxing/unboxing performance implications
- Review string handling and StringBuilder usage patterns

**3. Concurrency & Thread Safety:**
- Assess synchronization mechanisms and deadlock prevention
- Review thread pool usage and executor framework patterns
- Check for race conditions and atomic operation usage
- Evaluate concurrent collection usage appropriateness
- Review CompletableFuture and modern async patterns

**4. Exception Handling & Resilience:**
- Check checked vs unchecked exception usage
- Review exception hierarchy design and custom exceptions
- Assess try-with-resources and resource management
- Evaluate error recovery and fallback mechanisms
- Review logging patterns and structured error reporting

**5. Modern Java Features & APIs:**
- Assess lambda expressions and functional programming patterns
- Review stream API usage and performance implications
- Check for proper Optional usage and null safety
- Evaluate module system (JPMS) adoption and benefits
- Review record classes and modern data modeling

**6. Security & Best Practices:**
- Check input validation and sanitization practices
- Review secure coding practices and vulnerability prevention
- Assess cryptographic usage and key management
- Evaluate authentication and authorization patterns
- Review dependency security and update practices

**7. Testing & Code Quality:**
- Check unit testing coverage and quality (JUnit, TestNG)
- Review mock usage and test isolation patterns
- Assess integration testing strategies
- Evaluate code quality metrics and static analysis
- Review continuous integration and deployment practices",

            _
                => @"
**General Comprehensive Review Focus Areas**:

**1. Code Structure & Organization:**
- Review file organization and logical grouping
- Check naming conventions and consistency
- Assess code readability and maintainability
- Evaluate documentation quality and completeness
- Review architectural patterns and design decisions

**2. Performance & Efficiency:**
- Check algorithm efficiency and time complexity
- Review memory usage and resource management
- Assess I/O operations and optimization opportunities
- Evaluate caching strategies and data structures
- Review scalability considerations and bottlenecks

**3. Security & Error Handling:**
- Check input validation and sanitization
- Review error handling comprehensiveness
- Assess security vulnerability prevention
- Evaluate logging and monitoring practices
- Review data protection and privacy considerations

**4. Maintainability & Testing:**
- Check code modularity and reusability
- Review testing coverage and quality
- Assess debugging and troubleshooting ease
- Evaluate dependency management
- Review version control and change tracking

**5. Best Practices & Standards:**
- Check coding standard compliance
- Review industry best practice adherence
- Assess team collaboration patterns
- Evaluate documentation and knowledge sharing
- Review continuous improvement opportunities"
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
