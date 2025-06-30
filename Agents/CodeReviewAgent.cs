using Microsoft.SemanticKernel;
using System.ComponentModel;
using Octokit;

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

    public string Name => "CodeReviewAgent";
    
    public string Description => "Analyzes code quality, suggests improvements, and performs automated code reviews for C#, VB.NET, T-SQL, JavaScript, React, and Java";

    public CodeReviewAgent(Kernel kernel)
    {
        _kernel = kernel;
        
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

    public async Task InitializeAsync()
    {
        // Perform any initialization logic here
        await Task.CompletedTask;
        Console.WriteLine($"✅ {Name} initialized successfully");
    }

    public async Task RegisterFunctionsAsync(Kernel kernel)
    {
        // Register this class's methods as kernel functions
        kernel.ImportPluginFromObject(this, "CodeReview");
        await Task.CompletedTask;
        Console.WriteLine($"🔧 {Name} functions registered with kernel");
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
