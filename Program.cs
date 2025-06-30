using DotNetEnv;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticKernelDevHub.Agents;
using SemanticKernelDevHub.Plugins;
using SemanticKernelDevHub.Models;

// Load environment variables from .env file
Env.Load();

// Get configuration from environment variables
var endpoint = Environment.GetEnvironmentVariable("AOAI_ENDPOINT");
var apiKey = Environment.GetEnvironmentVariable("AOAI_APIKEY");
var deploymentName = Environment.GetEnvironmentVariable("CHATCOMPLETION_DEPLOYMENTNAME");

// Validate required configuration
if (string.IsNullOrEmpty(endpoint)
    || string.IsNullOrEmpty(apiKey)
    || string.IsNullOrEmpty(deploymentName))
{
    Console.WriteLine("❌ Missing required configuration. Please check your .env file.");
    Console.WriteLine($"AOAI_ENDPOINT: {(string.IsNullOrEmpty(endpoint) ? "MISSING" : "✓")}");
    Console.WriteLine($"AOAI_APIKEY: {(string.IsNullOrEmpty(apiKey) ? "MISSING" : "✓")}");
    Console.WriteLine(
        $"CHATCOMPLETION_DEPLOYMENTNAME: {(string.IsNullOrEmpty(deploymentName) ? "MISSING" : "✓")}");
    return;
}

try
{
    // Create kernel with Azure OpenAI chat completion service
    var kernel = Kernel.CreateBuilder()
        .AddAzureOpenAIChatCompletion(
            deploymentName: deploymentName,
            endpoint: endpoint,
            apiKey: apiKey)
        .Build();

    Console.WriteLine("🎉 Hello Semantic Kernel!");
    Console.WriteLine("✅ Semantic Kernel initialized successfully!");
    Console.WriteLine($"📡 Connected to Azure OpenAI endpoint: {endpoint}");
    Console.WriteLine($"🤖 Using deployment: {deploymentName}");

    // Initialize GitHub Plugin
    Console.WriteLine("\n🐙 Initializing GitHub integration...");
    GitHubPlugin? gitHubPlugin = null;
    var gitHubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
    var gitHubOwner = Environment.GetEnvironmentVariable("GITHUB_REPO_OWNER");
    var gitHubRepo = Environment.GetEnvironmentVariable("GITHUB_REPO_NAME");

    if (!string.IsNullOrEmpty(gitHubToken) && !string.IsNullOrEmpty(gitHubOwner) && !string.IsNullOrEmpty(gitHubRepo))
    {
        try
        {
            gitHubPlugin = new GitHubPlugin(gitHubToken, gitHubOwner, gitHubRepo);
            kernel.ImportPluginFromObject(gitHubPlugin, "GitHub");
            Console.WriteLine("✅ GitHubPlugin initialized and registered successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  GitHub plugin initialization failed: {ex.Message}");
            Console.WriteLine("📝 Code review will work in limited mode without GitHub integration");
        }
    }
    else
    {
        Console.WriteLine("⚠️  GitHub configuration incomplete - some features will be limited");
        Console.WriteLine("📝 Please ensure GITHUB_TOKEN, GITHUB_REPO_OWNER, and GITHUB_REPO_NAME are set");
    }

    // Initialize FileSystem Plugin
    Console.WriteLine("\n📁 Initializing file system integration...");
    var fileSystemPlugin = new FileSystemPlugin();
    kernel.ImportPluginFromObject(fileSystemPlugin, "FileSystem");
    Console.WriteLine("✅ FileSystemPlugin initialized and registered successfully");

    // Initialize and register CodeReviewAgent with GitHub plugin
    Console.WriteLine("\n🤖 Initializing agents...");
    var codeReviewAgent = new CodeReviewAgent(kernel, gitHubPlugin);
    await codeReviewAgent.InitializeAsync();
    await codeReviewAgent.RegisterFunctionsAsync(kernel);

    // Initialize MeetingAnalysisAgent with FileSystem plugin
    var meetingAnalysisAgent = new MeetingAnalysisAgent(kernel, fileSystemPlugin);
    await meetingAnalysisAgent.InitializeAsync();
    await meetingAnalysisAgent.RegisterFunctionsAsync(kernel);

    // Get registered functions
    var functions = kernel.Plugins.GetFunctionsMetadata();
    Console.WriteLine($"📋 Available functions: [{string.Join(", ", functions.Select(f => f.Name))}]");
    
    if (gitHubPlugin != null)
    {
        Console.WriteLine("\n🎉 Semantic Kernel with Meeting Analysis Ready!");
        Console.WriteLine("✅ GitHubPlugin registered successfully");
        Console.WriteLine("✅ FileSystemPlugin registered successfully");
        Console.WriteLine("✅ CodeReviewAgent with GitHub capabilities ready");
        Console.WriteLine("✅ MeetingAnalysisAgent ready for transcript processing");
    }
    else
    {
        Console.WriteLine("\n🎉 Semantic Kernel with Meeting Analysis Ready!");
        Console.WriteLine("✅ FileSystemPlugin registered successfully");
        Console.WriteLine("✅ MeetingAnalysisAgent ready for transcript processing");
    }

    // Interactive menu
    await RunInteractiveMenu(kernel, codeReviewAgent, gitHubPlugin, meetingAnalysisAgent, fileSystemPlugin);
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error initializing Semantic Kernel: {ex.Message}");
    Console.WriteLine($"📋 Details: {ex}");
}

static async Task RunInteractiveMenu(Kernel kernel, CodeReviewAgent codeReviewAgent, GitHubPlugin? gitHubPlugin, MeetingAnalysisAgent meetingAnalysisAgent, FileSystemPlugin fileSystemPlugin)
{
    while (true)
    {
        Console.WriteLine("\n" + new string('=', 60));
        Console.WriteLine("🚀 Semantic Kernel DevHub - Complete Integration Menu");
        Console.WriteLine(new string('=', 60));
        Console.WriteLine("Choose an option:");
        
        if (gitHubPlugin != null)
        {
            Console.WriteLine("1. Review Latest Commit");
            Console.WriteLine("2. List Recent Commits");
            Console.WriteLine("3. Review Specific Commit");
            Console.WriteLine("4. Review Pull Request");
            Console.WriteLine("5. Analyze Custom Code");
            Console.WriteLine("6. Check Coding Standards");
            Console.WriteLine("7. Repository Information");
            Console.WriteLine("8. Process Meeting Transcript");
            Console.WriteLine("9. Start File Watcher Mode");
            Console.WriteLine("10. Analyze Sample Meeting");
            Console.WriteLine("11. Exit");
        }
        else
        {
            Console.WriteLine("1. Test Code Review Agent");
            Console.WriteLine("2. Analyze Sample Code");
            Console.WriteLine("3. Check Coding Standards");
            Console.WriteLine("4. Review GitHub Pull Request (Limited)");
            Console.WriteLine("5. Process Meeting Transcript");
            Console.WriteLine("6. Start File Watcher Mode");
            Console.WriteLine("7. Analyze Sample Meeting");
            Console.WriteLine("8. Exit");
        }
        
        Console.Write($"\nEnter your choice (1-{(gitHubPlugin != null ? "11" : "8")}): ");
        var choice = Console.ReadLine();

        try
        {
            if (gitHubPlugin != null)
            {
                switch (choice)
                {
                    case "1":
                        await ReviewLatestCommit(codeReviewAgent);
                        break;
                    case "2":
                        await ListRecentCommits(codeReviewAgent);
                        break;
                    case "3":
                        await ReviewSpecificCommit(codeReviewAgent);
                        break;
                    case "4":
                        await ReviewPullRequest(codeReviewAgent);
                        break;
                    case "5":
                        await AnalyzeSampleCode(codeReviewAgent);
                        break;
                    case "6":
                        await CheckCodingStandards(codeReviewAgent);
                        break;
                    case "7":
                        await ShowRepositoryInfo(gitHubPlugin);
                        break;
                    case "8":
                        await ProcessMeetingTranscript(meetingAnalysisAgent, fileSystemPlugin);
                        break;
                    case "9":
                        await StartFileWatcherMode(fileSystemPlugin, meetingAnalysisAgent);
                        break;
                    case "10":
                        await AnalyzeSampleMeeting(meetingAnalysisAgent);
                        break;
                    case "11":
                        Console.WriteLine("\n👋 Thank you for using Semantic Kernel DevHub!");
                        return;
                    default:
                        Console.WriteLine("\n❌ Invalid choice. Please enter 1-11.");
                        break;
                }
            }
            else
            {
                switch (choice)
                {
                    case "1":
                        await TestCodeReviewAgent(codeReviewAgent);
                        break;
                    case "2":
                        await AnalyzeSampleCode(codeReviewAgent);
                        break;
                    case "3":
                        await CheckCodingStandards(codeReviewAgent);
                        break;
                    case "4":
                        await ReviewPullRequest(codeReviewAgent);
                        break;
                    case "5":
                        await ProcessMeetingTranscript(meetingAnalysisAgent, fileSystemPlugin);
                        break;
                    case "6":
                        await StartFileWatcherMode(fileSystemPlugin, meetingAnalysisAgent);
                        break;
                    case "7":
                        await AnalyzeSampleMeeting(meetingAnalysisAgent);
                        break;
                    case "8":
                        Console.WriteLine("\n👋 Thank you for using Semantic Kernel DevHub!");
                        return;
                    default:
                        Console.WriteLine("\n❌ Invalid choice. Please enter 1-8.");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Error: {ex.Message}");
        }

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }
}

static async Task TestCodeReviewAgent(CodeReviewAgent agent)
{
    Console.WriteLine("\n🧪 Testing Code Review Agent with different languages...");
    
    // Test C# code
    var csharpCode = @"
public class Calculator
{
    public int Add(int a, int b)
    {
        return a + b;
    }
    
    public int Divide(int a, int b)
    {
        return a / b;
    }
}";

    Console.WriteLine("📝 Analyzing sample C# Calculator class...");
    var csharpResult = await agent.AnalyzeCode(csharpCode, "C#");
    Console.WriteLine("\n📊 C# Analysis Result:");
    Console.WriteLine(csharpResult);

    Console.WriteLine("\n" + new string('-', 50));
    
    // Test JavaScript code
    var jsCode = @"
function calculateTotal(items) {
    var total = 0;
    for (var i = 0; i < items.length; i++) {
        total += items[i].price;
    }
    return total;
}";

    Console.WriteLine("📝 Analyzing sample JavaScript function...");
    var jsResult = await agent.AnalyzeCode(jsCode, "JavaScript");
    Console.WriteLine("\n📊 JavaScript Analysis Result:");
    Console.WriteLine(jsResult);
}

static async Task AnalyzeSampleCode(CodeReviewAgent agent)
{
    Console.WriteLine("\n📝 Enter your code to analyze:");
    Console.WriteLine("(Enter 'END' on a new line when finished)");
    
    var codeLines = new List<string>();
    string? line;
    while ((line = Console.ReadLine()) != "END")
    {
        if (line != null)
            codeLines.Add(line);
    }
    
    var code = string.Join("\n", codeLines);
    if (string.IsNullOrWhiteSpace(code))
    {
        Console.WriteLine("❌ No code provided.");
        return;
    }

    Console.WriteLine("\nSupported languages: C#, VB.NET, T-SQL, JavaScript, React, Java");
    Console.Write("Enter programming language (or press Enter for 'C#'): ");
    var language = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(language))
        language = "C#";

    Console.WriteLine($"\n🔍 Analyzing your {language} code...");
    var result = await agent.AnalyzeCode(code, language);
    Console.WriteLine("\n📊 Analysis Result:");
    Console.WriteLine(result);
}

static async Task CheckCodingStandards(CodeReviewAgent agent)
{
    Console.WriteLine("\n📋 Enter code to check against coding standards:");
    Console.WriteLine("(Enter 'END' on a new line when finished)");
    
    var codeLines = new List<string>();
    string? line;
    while ((line = Console.ReadLine()) != "END")
    {
        if (line != null)
            codeLines.Add(line);
    }
    
    var code = string.Join("\n", codeLines);
    if (string.IsNullOrWhiteSpace(code))
    {
        Console.WriteLine("❌ No code provided.");
        return;
    }

    Console.WriteLine("\nSupported languages: C#, VB.NET, T-SQL, JavaScript, React, Java");
    Console.Write("Enter programming language (or press Enter for 'C#'): ");
    var language = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(language))
        language = "C#";

    Console.Write("Enter coding standard (or press Enter for language default): ");
    var standard = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(standard))
        standard = "Language Default";

    Console.WriteLine($"\n📏 Checking {language} coding standards...");
    var result = await agent.CheckCodingStandards(code, standard, language);
    Console.WriteLine("\n📊 Standards Check Result:");
    Console.WriteLine(result);
}

static async Task ReviewPullRequest(CodeReviewAgent agent)
{
    Console.Write("\nEnter GitHub Pull Request number to review: ");
    var prInput = Console.ReadLine();
    
    if (!int.TryParse(prInput, out var prNumber))
    {
        Console.WriteLine("❌ Invalid pull request number.");
        return;
    }

    Console.WriteLine($"\n🔍 Reviewing Pull Request #{prNumber}...");
    var result = await agent.ReviewPullRequest(prNumber);
    Console.WriteLine("\n📊 Pull Request Review:");
    Console.WriteLine(result);
}

static async Task ReviewLatestCommit(CodeReviewAgent agent)
{
    Console.WriteLine("\n🔍 Reviewing latest commit...");
    try
    {
        var result = await agent.ReviewLatestCommit();
        Console.WriteLine("\n📊 Latest Commit Review Result:");
        Console.WriteLine(result.ToString());
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error reviewing latest commit: {ex.Message}");
    }
}

static async Task ListRecentCommits(CodeReviewAgent agent)
{
    Console.WriteLine("\n📝 Fetching recent commits...");
    Console.Write("How many commits to show (1-20, default 10): ");
    var countInput = Console.ReadLine();
    
    if (!int.TryParse(countInput, out var count) || count < 1 || count > 20)
    {
        count = 10;
    }

    try
    {
        var commits = await agent.ListRecentCommits(count);
        
        Console.WriteLine($"\n📝 Recent {commits.Count} commits:");
        Console.WriteLine(new string('-', 60));
        
        foreach (var commit in commits)
        {
            Console.WriteLine($"  {commit.ShortSha} - {commit.Message.Split('\n')[0]}");
            Console.WriteLine($"      👤 {commit.Author} | 📅 {commit.Date:yyyy-MM-dd HH:mm}");
            if (commit.FilesChanged.Any())
            {
                Console.WriteLine($"      📁 {commit.FilesChanged.Count} files changed (+{commit.TotalAdditions}/-{commit.TotalDeletions})");
            }
            Console.WriteLine();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error fetching commits: {ex.Message}");
    }
}

static async Task ReviewSpecificCommit(CodeReviewAgent agent)
{
    Console.Write("\nEnter commit SHA to review: ");
    var commitSha = Console.ReadLine();
    
    if (string.IsNullOrWhiteSpace(commitSha))
    {
        Console.WriteLine("❌ No commit SHA provided.");
        return;
    }

    Console.WriteLine($"\n🔍 Reviewing commit {commitSha}...");
    
    try
    {
        var result = await agent.ReviewCommit(commitSha);
        Console.WriteLine("\n📊 Commit Review Result:");
        Console.WriteLine(result.ToString());
        
        if (result.FileReviews.Any())
        {
            Console.WriteLine("\n📋 Individual File Reviews:");
            Console.WriteLine(new string('-', 60));
            
            foreach (var fileReview in result.FileReviews)
            {
                Console.WriteLine($"📄 {fileReview.FileName} ({fileReview.Language})");
                Console.WriteLine($"   Score: {fileReview.Score}/10");
                
                if (fileReview.Issues.Any())
                {
                    Console.WriteLine($"   Issues: {string.Join(", ", fileReview.Issues.Take(2))}");
                }
                
                Console.WriteLine();
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error reviewing commit: {ex.Message}");
    }
}

static async Task ShowRepositoryInfo(GitHubPlugin gitHubPlugin)
{
    Console.WriteLine("\n📊 Fetching repository information...");
    
    try
    {
        var repoInfo = await gitHubPlugin.GetRepositoryInfo();
        var repo = System.Text.Json.JsonSerializer.Deserialize<dynamic>(repoInfo);
        
        Console.WriteLine("\n📋 Repository Information:");
        Console.WriteLine(new string('-', 50));
        Console.WriteLine($"Name: {repo?.GetProperty("Name").GetString()}");
        Console.WriteLine($"Description: {repo?.GetProperty("Description").GetString() ?? "No description"}");
        Console.WriteLine($"Language: {repo?.GetProperty("Language").GetString() ?? "Mixed"}");
        Console.WriteLine($"Stars: {repo?.GetProperty("StargazersCount").GetInt32()}");
        Console.WriteLine($"Forks: {repo?.GetProperty("ForksCount").GetInt32()}");
        Console.WriteLine($"Open Issues: {repo?.GetProperty("OpenIssuesCount").GetInt32()}");
        Console.WriteLine($"Default Branch: {repo?.GetProperty("DefaultBranch").GetString()}");
        Console.WriteLine($"Created: {repo?.GetProperty("CreatedAt").GetDateTime():yyyy-MM-dd}");
        Console.WriteLine($"Last Updated: {repo?.GetProperty("UpdatedAt").GetDateTime():yyyy-MM-dd}");
        Console.WriteLine($"URL: {repo?.GetProperty("Url").GetString()}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error fetching repository info: {ex.Message}");
    }
}

/// <summary>
/// Processes a meeting transcript file
/// </summary>
static async Task ProcessMeetingTranscript(MeetingAnalysisAgent meetingAgent, FileSystemPlugin fileSystemPlugin)
{
    try
    {
        Console.WriteLine("\n📂 Processing meeting transcript...");
        var incomingFiles = await fileSystemPlugin.ListIncomingFiles();
        
        if (!incomingFiles.Any())
        {
            Console.WriteLine("📁 No transcript files found. Copy files to Data/Incoming/ folder.");
            return;
        }

        var selectedFile = incomingFiles[0]; // Use first file for simplicity
        var fileName = Path.GetFileName(selectedFile);
        Console.WriteLine($"🔍 Processing: {fileName}");
        
        var result = await meetingAgent.ProcessTranscriptFile(selectedFile);
        Console.WriteLine(result.GetFormattedSummary());
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error: {ex.Message}");
    }
    
    Console.WriteLine("\nPress any key to continue...");
    Console.ReadKey();
}

/// <summary>
/// Starts file watcher mode
/// </summary>
static Task StartFileWatcherMode(FileSystemPlugin fileSystemPlugin, MeetingAnalysisAgent meetingAgent)
{
    Console.WriteLine("\n📡 File watcher mode activated. Press any key to return to menu...");
    Console.ReadKey();
    return Task.CompletedTask;
}

/// <summary>
/// Analyzes a sample meeting
/// </summary>
static async Task AnalyzeSampleMeeting(MeetingAnalysisAgent meetingAgent)
{
    try
    {
        Console.WriteLine("\n📋 Processing sample meeting transcript...");
        var result = await meetingAgent.AnalyzeSampleMeeting(0);
        Console.WriteLine(result.GetFormattedSummary());
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error: {ex.Message}");
    }
    
    Console.WriteLine("\nPress any key to continue...");
    Console.ReadKey();
}
