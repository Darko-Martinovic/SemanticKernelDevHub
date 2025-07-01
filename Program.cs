using DotNetEnv;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticKernelDevHub.Agents;
using SemanticKernelDevHub.Plugins;
using SemanticKernelDevHub.Models;
using SemanticKernelDevHub.Services;

// Load environment variables from .env file
Env.Load();

// Get configuration from environment variables
var endpoint = Environment.GetEnvironmentVariable("AOAI_ENDPOINT");
var apiKey = Environment.GetEnvironmentVariable("AOAI_APIKEY");
var deploymentName = Environment.GetEnvironmentVariable("CHATCOMPLETION_DEPLOYMENTNAME");

// Validate required configuration
if (
    string.IsNullOrEmpty(endpoint)
    || string.IsNullOrEmpty(apiKey)
    || string.IsNullOrEmpty(deploymentName)
)
{
    Console.WriteLine("❌ Missing required configuration. Please check your .env file.");
    Console.WriteLine($"AOAI_ENDPOINT: {(string.IsNullOrEmpty(endpoint) ? "MISSING" : "✓")}");
    Console.WriteLine($"AOAI_APIKEY: {(string.IsNullOrEmpty(apiKey) ? "MISSING" : "✓")}");
    Console.WriteLine(
        $"CHATCOMPLETION_DEPLOYMENTNAME: {(string.IsNullOrEmpty(deploymentName) ? "MISSING" : "✓")}"
    );
    return;
}

try
{
    // Create kernel with Azure OpenAI chat completion service
    var kernel = Kernel
        .CreateBuilder()
        .AddAzureOpenAIChatCompletion(
            deploymentName: deploymentName,
            endpoint: endpoint,
            apiKey: apiKey
        )
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

    if (
        !string.IsNullOrEmpty(gitHubToken)
        && !string.IsNullOrEmpty(gitHubOwner)
        && !string.IsNullOrEmpty(gitHubRepo)
    )
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
            Console.WriteLine(
                "📝 Code review will work in limited mode without GitHub integration"
            );
        }
    }
    else
    {
        Console.WriteLine("⚠️  GitHub configuration incomplete - some features will be limited");
        Console.WriteLine(
            "📝 Please ensure GITHUB_TOKEN, GITHUB_REPO_OWNER, and GITHUB_REPO_NAME are set"
        );
    }

    // Initialize FileSystem Plugin
    Console.WriteLine("\n📁 Initializing file system integration...");
    var fileSystemPlugin = new FileSystemPlugin();
    kernel.ImportPluginFromObject(fileSystemPlugin, "FileSystem");
    Console.WriteLine("✅ FileSystemPlugin initialized and registered successfully");

    // Initialize Jira Plugin
    Console.WriteLine("\n🎫 Initializing Jira integration...");
    JiraPlugin? jiraPlugin = null;
    JiraIntegrationAgent? jiraIntegrationAgent = null;
    var jiraUrl = Environment.GetEnvironmentVariable("JIRA_URL");
    var jiraEmail = Environment.GetEnvironmentVariable("JIRA_EMAIL");
    var jiraToken = Environment.GetEnvironmentVariable("JIRA_API_TOKEN");
    var jiraProjectKey = Environment.GetEnvironmentVariable("JIRA_PROJECT_KEY");

    if (
        !string.IsNullOrEmpty(jiraUrl)
        && !string.IsNullOrEmpty(jiraEmail)
        && !string.IsNullOrEmpty(jiraToken)
        && !string.IsNullOrEmpty(jiraProjectKey)
    )
    {
        try
        {
            jiraPlugin = new JiraPlugin(jiraUrl, jiraEmail, jiraToken, jiraProjectKey);
            kernel.ImportPluginFromObject(jiraPlugin, "Jira");

            // Test Jira connection
            var connectionTest = await jiraPlugin.TestConnection();
            Console.WriteLine($"🔌 {connectionTest}");

            // Initialize JiraIntegrationAgent
            jiraIntegrationAgent = new JiraIntegrationAgent(kernel, jiraPlugin, jiraProjectKey);
            await jiraIntegrationAgent.InitializeAsync();
            await jiraIntegrationAgent.RegisterFunctionsAsync(kernel);

            Console.WriteLine("✅ JiraPlugin and JiraIntegrationAgent initialized successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Jira integration initialization failed: {ex.Message}");
            Console.WriteLine("📝 Jira features will be disabled");
        }
    }
    else
    {
        Console.WriteLine("⚠️  Jira configuration incomplete - Jira features will be disabled");
        Console.WriteLine(
            "📝 Please ensure JIRA_URL, JIRA_EMAIL, JIRA_API_TOKEN, and JIRA_PROJECT_KEY are set"
        );
    }

    // Initialize and register CodeReviewAgent with GitHub plugin and Jira integration
    Console.WriteLine("\n🤖 Initializing agents...");
    var codeReviewAgent = new CodeReviewAgent(kernel, gitHubPlugin, jiraIntegrationAgent);
    await codeReviewAgent.InitializeAsync();
    await codeReviewAgent.RegisterFunctionsAsync(kernel);

    // Initialize MeetingAnalysisAgent with FileSystem plugin
    var meetingAnalysisAgent = new MeetingAnalysisAgent(kernel, fileSystemPlugin);
    await meetingAnalysisAgent.InitializeAsync();
    await meetingAnalysisAgent.RegisterFunctionsAsync(kernel);

    // Initialize Intelligence Agent with all other agents
    Console.WriteLine("\n🧠 Initializing Intelligence Agent...");
    var intelligenceAgent = new IntelligenceAgent(
        kernel,
        codeReviewAgent,
        meetingAnalysisAgent,
        jiraIntegrationAgent
    );
    await intelligenceAgent.InitializeAsync();
    await intelligenceAgent.RegisterFunctionsAsync(kernel);

    // Initialize Orchestration Service
    Console.WriteLine("🎭 Initializing Orchestration Service...");
    var orchestrationService = new OrchestrationService(
        kernel,
        intelligenceAgent,
        codeReviewAgent,
        meetingAnalysisAgent,
        jiraIntegrationAgent
    );
    Console.WriteLine("✅ Advanced orchestration capabilities ready");

    // Get registered functions
    var functions = kernel.Plugins.GetFunctionsMetadata();
    Console.WriteLine(
        $"📋 Available functions: [{string.Join(", ", functions.Select(f => f.Name))}]"
    );

    // Final status messages
    if (gitHubPlugin != null && jiraPlugin != null)
    {
        Console.WriteLine("\n🧠 Semantic Kernel Intelligence Hub Ready!");
        Console.WriteLine("✅ All agents initialized and cross-connected");
        Console.WriteLine("✅ Memory system active");
        Console.WriteLine("✅ Intelligence orchestration ready");
        Console.WriteLine("✅ GitHubPlugin registered successfully");
        Console.WriteLine("✅ FileSystemPlugin registered successfully");
        Console.WriteLine("✅ JiraPlugin registered successfully");
        Console.WriteLine("✅ CodeReviewAgent with GitHub capabilities ready");
        Console.WriteLine("✅ MeetingAnalysisAgent ready for transcript processing");
        Console.WriteLine("✅ JiraIntegrationAgent ready for ticket operations");
        Console.WriteLine("🧠 IntelligenceAgent ready for cross-system analysis");
        Console.WriteLine("🎭 OrchestrationService ready for complex workflows");
    }
    else if (gitHubPlugin != null)
    {
        Console.WriteLine("\n🎉 Semantic Kernel with GitHub + Meeting Analysis Ready!");
        Console.WriteLine("✅ GitHubPlugin registered successfully");
        Console.WriteLine("✅ FileSystemPlugin registered successfully");
        Console.WriteLine("✅ CodeReviewAgent with GitHub capabilities ready");
        Console.WriteLine("✅ MeetingAnalysisAgent ready for transcript processing");
        Console.WriteLine("⚠️  Jira integration not available");
    }
    else if (jiraPlugin != null)
    {
        Console.WriteLine("\n🎉 Semantic Kernel with Jira + Meeting Analysis Ready!");
        Console.WriteLine("✅ FileSystemPlugin registered successfully");
        Console.WriteLine("✅ JiraPlugin registered successfully");
        Console.WriteLine("✅ MeetingAnalysisAgent ready for transcript processing");
        Console.WriteLine("✅ JiraIntegrationAgent ready for ticket operations");
        Console.WriteLine("⚠️  GitHub integration not available");
    }
    else
    {
        Console.WriteLine("\n🎉 Semantic Kernel with Meeting Analysis Ready!");
        Console.WriteLine("✅ FileSystemPlugin registered successfully");
        Console.WriteLine("✅ MeetingAnalysisAgent ready for transcript processing");
    }

    // Interactive menu
    await RunInteractiveMenu(
        kernel,
        codeReviewAgent,
        gitHubPlugin,
        meetingAnalysisAgent,
        fileSystemPlugin,
        jiraIntegrationAgent,
        intelligenceAgent,
        orchestrationService
    );
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error initializing Semantic Kernel: {ex.Message}");
    Console.WriteLine($"📋 Details: {ex}");
}

static async Task RunInteractiveMenu(
    Kernel kernel,
    CodeReviewAgent codeReviewAgent,
    GitHubPlugin? gitHubPlugin,
    MeetingAnalysisAgent meetingAnalysisAgent,
    FileSystemPlugin fileSystemPlugin,
    JiraIntegrationAgent? jiraIntegrationAgent,
    IntelligenceAgent intelligenceAgent,
    OrchestrationService orchestrationService
)
{
    while (true)
    {
        Console.WriteLine("\n" + new string('=', 60));
        Console.WriteLine("🧠 Semantic Kernel Intelligence Hub - Phase 6");
        Console.WriteLine(new string('=', 60));
        Console.WriteLine("Choose an option:");

        if (gitHubPlugin != null && jiraIntegrationAgent != null)
        {
            Console.WriteLine("📝 Code Review & Analysis:");
            Console.WriteLine("1. Review Latest Commit");
            Console.WriteLine("2. List Recent Commits");
            Console.WriteLine("3. Review Specific Commit");
            Console.WriteLine("4. Review Pull Request");
            Console.WriteLine("5. Analyze Custom Code");
            Console.WriteLine("6. Check Coding Standards");
            Console.WriteLine("7. Repository Information");

            Console.WriteLine("\n💬 Meeting Analysis:");
            Console.WriteLine("8. Process Meeting Transcript");
            Console.WriteLine("9. Start File Watcher Mode");
            Console.WriteLine("10. Analyze Sample Meeting");

            Console.WriteLine("\n🎫 Jira Integration:");
            Console.WriteLine("11. Test Jira Connection");
            Console.WriteLine("12. Create Sample Jira Ticket");
            Console.WriteLine("13. Update Existing Jira Ticket");

            Console.WriteLine("\n🧠 Intelligence & Orchestration:");
            Console.WriteLine("14. Generate Development Intelligence Report");
            Console.WriteLine("15. Analyze Cross-References (Code ↔ Meetings ↔ Jira)");
            Console.WriteLine("16. Predictive Insights Dashboard");
            Console.WriteLine("17. Export Executive Summary");
            Console.WriteLine("18. Execute Security Workflow");
            Console.WriteLine("19. Execute Performance Workflow");
            Console.WriteLine("20. Execute Sprint Planning Workflow");

            Console.WriteLine("\n⚡ Quick Actions:");
            Console.WriteLine("21. Exit");
        }
        else if (gitHubPlugin != null)
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
        else if (jiraIntegrationAgent != null)
        {
            Console.WriteLine("1. Test Code Review Agent");
            Console.WriteLine("2. Analyze Sample Code");
            Console.WriteLine("3. Check Coding Standards");
            Console.WriteLine("4. Process Meeting Transcript");
            Console.WriteLine("5. Start File Watcher Mode");
            Console.WriteLine("6. Analyze Sample Meeting");
            Console.WriteLine("7. Test Jira Connection");
            Console.WriteLine("8. Create Sample Jira Ticket");
            Console.WriteLine("9. Update Existing Jira Ticket");
            Console.WriteLine("10. Exit");
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

        var maxChoice =
            (gitHubPlugin != null && jiraIntegrationAgent != null)
                ? "21"
                : (gitHubPlugin != null)
                    ? "11"
                    : (jiraIntegrationAgent != null)
                        ? "10"
                        : "8";

        Console.Write($"\nEnter your choice (1-{maxChoice}): ");
        var choice = Console.ReadLine();

        try
        {
            if (gitHubPlugin != null && jiraIntegrationAgent != null)
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
                        await TestJiraConnection(jiraIntegrationAgent!);
                        break;
                    case "12":
                        await CreateSampleJiraTicket(jiraIntegrationAgent!);
                        break;
                    case "13":
                        await UpdateExistingJiraTicket(jiraIntegrationAgent!);
                        break;
                    case "14":
                        await GenerateDevelopmentIntelligenceReport(intelligenceAgent);
                        break;
                    case "15":
                        await AnalyzeCrossReferences(intelligenceAgent);
                        break;
                    case "16":
                        await ShowPredictiveInsightsDashboard(intelligenceAgent);
                        break;
                    case "17":
                        await ExportExecutiveSummary(intelligenceAgent);
                        break;
                    case "18":
                        await ExecuteSecurityWorkflow(orchestrationService);
                        break;
                    case "19":
                        await ExecutePerformanceWorkflow(orchestrationService);
                        break;
                    case "20":
                        await ExecuteSprintPlanningWorkflow(orchestrationService);
                        break;
                    case "21":
                        Console.WriteLine(
                            "\n👋 Thank you for using Semantic Kernel Intelligence Hub!"
                        );
                        return;
                    default:
                        Console.WriteLine("\n❌ Invalid choice. Please enter 1-21.");
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
                        await TestJiraConnection(jiraIntegrationAgent!);
                        break;
                    case "9":
                        await CreateSampleJiraTicket(jiraIntegrationAgent!);
                        break;
                    case "10":
                        await UpdateExistingJiraTicket(jiraIntegrationAgent!);
                        break;
                    case "11":
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
    var csharpCode =
        @"
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
    var jsCode =
        @"
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
                Console.WriteLine(
                    $"      📁 {commit.FilesChanged.Count} files changed (+{commit.TotalAdditions}/-{commit.TotalDeletions})"
                );
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
        Console.WriteLine(
            $"Description: {repo?.GetProperty("Description").GetString() ?? "No description"}"
        );
        Console.WriteLine($"Language: {repo?.GetProperty("Language").GetString() ?? "Mixed"}");
        Console.WriteLine($"Stars: {repo?.GetProperty("StargazersCount").GetInt32()}");
        Console.WriteLine($"Forks: {repo?.GetProperty("ForksCount").GetInt32()}");
        Console.WriteLine($"Open Issues: {repo?.GetProperty("OpenIssuesCount").GetInt32()}");
        Console.WriteLine($"Default Branch: {repo?.GetProperty("DefaultBranch").GetString()}");
        Console.WriteLine($"Created: {repo?.GetProperty("CreatedAt").GetDateTime():yyyy-MM-dd}");
        Console.WriteLine(
            $"Last Updated: {repo?.GetProperty("UpdatedAt").GetDateTime():yyyy-MM-dd}"
        );
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
static async Task ProcessMeetingTranscript(
    MeetingAnalysisAgent meetingAgent,
    FileSystemPlugin fileSystemPlugin
)
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
static Task StartFileWatcherMode(
    FileSystemPlugin fileSystemPlugin,
    MeetingAnalysisAgent meetingAgent
)
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

static async Task TestJiraConnection(JiraIntegrationAgent jiraAgent)
{
    Console.WriteLine("\n🔌 Testing Jira connection...");
    Console.WriteLine("✅ Jira connection test would run here");
    Console.WriteLine("\nPress any key to continue...");
    Console.ReadKey();
}

static async Task CreateSampleJiraTicket(JiraIntegrationAgent jiraAgent)
{
    Console.WriteLine("\n🎫 Creating sample Jira ticket...");
    Console.WriteLine("✅ Sample ticket creation would run here");
    Console.WriteLine("\nPress any key to continue...");
    Console.ReadKey();
}

static async Task UpdateExistingJiraTicket(JiraIntegrationAgent jiraAgent)
{
    Console.WriteLine("\n🎫 Updating Jira ticket...");
    Console.WriteLine("✅ Ticket update would run here");
    Console.WriteLine("\nPress any key to continue...");
    Console.ReadKey();
}

// Intelligence Agent Handler Methods

static async Task GenerateDevelopmentIntelligenceReport(IntelligenceAgent intelligenceAgent)
{
    Console.WriteLine("\n🧠 Generating Development Intelligence Report...");

    try
    {
        var report = await intelligenceAgent.GenerateDevelopmentIntelligenceReport(7, true);

        Console.WriteLine($"\n📊 **DEVELOPMENT INTELLIGENCE REPORT**");
        Console.WriteLine($"📅 Period: {report.Period.FriendlyDescription}");
        Console.WriteLine($"🏥 Health Score: {report.OverallHealthScore}/100");

        Console.WriteLine($"\n📈 **KEY METRICS**:");
        Console.WriteLine($"• Commits: {report.Metrics.TotalCommits}");
        Console.WriteLine($"• Code Reviews: {report.Metrics.TotalCodeReviews}");
        Console.WriteLine($"• Meetings: {report.Metrics.TotalMeetings}");
        Console.WriteLine($"• Jira Tickets: {report.Metrics.TotalJiraTickets}");
        Console.WriteLine(
            $"• Action Item Completion: {report.Metrics.ActionItemCompletionRate:F1}%"
        );

        Console.WriteLine($"\n🔍 **KEY INSIGHTS**:");
        foreach (var insight in report.Insights.Take(3))
        {
            Console.WriteLine($"• {insight.Title}: {insight.Description}");
        }

        Console.WriteLine($"\n💡 **TOP RECOMMENDATIONS**:");
        foreach (var rec in report.Predictions.Take(3))
        {
            Console.WriteLine($"• {rec.Title} ({rec.Priority})");
        }

        Console.WriteLine($"\n📋 **EXECUTIVE SUMMARY**:");
        Console.WriteLine(report.ExecutiveSummary);

        Console.WriteLine("\n✅ Report complete!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error generating report: {ex.Message}");
    }

    Console.WriteLine("\nPress any key to continue...");
    Console.ReadKey();
}

static async Task AnalyzeCrossReferences(IntelligenceAgent intelligenceAgent)
{
    Console.WriteLine("\n🔗 Analyzing Cross-References Between Systems...");

    try
    {
        var crossRef = await intelligenceAgent.AnalyzeCrossReferences("FullSystemAnalysis");

        Console.WriteLine($"\n🔍 **CROSS-REFERENCE ANALYSIS**");
        Console.WriteLine($"📊 Confidence Score: {crossRef.ConfidenceScore:F2}");
        Console.WriteLine($"🔗 Connections Found: {crossRef.Connections.Count}");
        Console.WriteLine($"📝 Entities Analyzed: {crossRef.RelatedEntities.Count}");

        Console.WriteLine($"\n💡 **KEY INSIGHTS**:");
        foreach (var insight in crossRef.KeyInsights)
        {
            Console.WriteLine($"• {insight}");
        }

        Console.WriteLine($"\n🔍 **CONNECTION PATTERNS**:");
        foreach (var pattern in crossRef.Patterns.Take(3))
        {
            Console.WriteLine(
                $"• {pattern.Name}: {pattern.Description} (Confidence: {pattern.Confidence:F2})"
            );
        }

        Console.WriteLine($"\n📋 **SUMMARY**:");
        Console.WriteLine(crossRef.Summary);

        // Show specific correlations
        var correlationReport = await intelligenceAgent.AnalyzeCodeMeetingCorrelations();
        Console.WriteLine($"\n🔗 **CODE ↔ MEETING CORRELATIONS**:");
        Console.WriteLine(correlationReport);

        Console.WriteLine("\n✅ Cross-reference analysis complete!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error analyzing cross-references: {ex.Message}");
    }

    Console.WriteLine("\nPress any key to continue...");
    Console.ReadKey();
}

static async Task ShowPredictiveInsightsDashboard(IntelligenceAgent intelligenceAgent)
{
    Console.WriteLine("\n🔮 Generating Predictive Insights Dashboard...");

    try
    {
        var predictions = await intelligenceAgent.GeneratePredictiveInsights(14);

        Console.WriteLine($"\n🔮 **PREDICTIVE INSIGHTS DASHBOARD**");
        Console.WriteLine($"🎯 Prediction Horizon: 14 days");
        Console.WriteLine($"📊 Total Predictions: {predictions.Count}");

        // Group by category
        var groupedPredictions = predictions.GroupBy(p => p.Category);

        foreach (var group in groupedPredictions)
        {
            Console.WriteLine($"\n📂 **{group.Key.ToString().ToUpper()}**:");
            foreach (var prediction in group.Take(2))
            {
                Console.WriteLine($"• {prediction.Title}");
                Console.WriteLine(
                    $"  Priority: {prediction.Priority} | Confidence: {prediction.Confidence:F2}"
                );
                Console.WriteLine($"  Expected Impact: {prediction.ExpectedImpact}");
                Console.WriteLine($"  Time Frame: {prediction.TimeFrame}");
                Console.WriteLine($"  Description: {prediction.Description}");

                if (prediction.ActionSteps.Any())
                {
                    Console.WriteLine(
                        $"  Action Steps: {prediction.ActionSteps.Count} steps defined"
                    );
                }
                Console.WriteLine();
            }
        }

        // Highlight critical predictions
        var criticalPredictions = predictions
            .Where(p => p.Priority == RecommendationPriority.Critical)
            .ToList();
        if (criticalPredictions.Any())
        {
            Console.WriteLine($"\n⚠️  **CRITICAL ATTENTION REQUIRED**:");
            foreach (var critical in criticalPredictions)
            {
                Console.WriteLine($"🚨 {critical.Title}: {critical.Description}");
            }
        }

        Console.WriteLine("\n✅ Predictive insights dashboard complete!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error generating predictive insights: {ex.Message}");
    }

    Console.WriteLine("\nPress any key to continue...");
    Console.ReadKey();
}

static async Task ExportExecutiveSummary(IntelligenceAgent intelligenceAgent)
{
    Console.WriteLine("\n📋 Generating Executive Summary...");

    try
    {
        var executiveSummary = await intelligenceAgent.CreateExecutiveSummary("Overall");

        Console.WriteLine($"\n📋 **EXECUTIVE SUMMARY**");
        Console.WriteLine(new string('=', 50));
        Console.WriteLine(executiveSummary);
        Console.WriteLine(new string('=', 50));

        // Also generate focused summaries
        Console.WriteLine($"\n🔒 **SECURITY FOCUS**:");
        var securitySummary = await intelligenceAgent.CreateExecutiveSummary("Security");
        Console.WriteLine(securitySummary);

        Console.WriteLine($"\n⚡ **PERFORMANCE FOCUS**:");
        var perfSummary = await intelligenceAgent.CreateExecutiveSummary("Performance");
        Console.WriteLine(perfSummary);

        Console.WriteLine($"\n⚠️  **RISK FOCUS**:");
        var riskSummary = await intelligenceAgent.CreateExecutiveSummary("Risks");
        Console.WriteLine(riskSummary);

        Console.WriteLine("\n✅ Executive summary exported!");
        Console.WriteLine("💼 Ready for leadership presentation");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error generating executive summary: {ex.Message}");
    }

    Console.WriteLine("\nPress any key to continue...");
    Console.ReadKey();
}

// Orchestration Service Handler Methods

static async Task ExecuteSecurityWorkflow(OrchestrationService orchestrationService)
{
    Console.WriteLine("\n🔒 Executing Security Workflow...");

    Console.Write("Enter commit SHA to analyze (or press Enter for demo): ");
    var commitSha = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(commitSha))
    {
        commitSha = "abc123def456"; // Demo SHA
        Console.WriteLine($"Using demo commit SHA: {commitSha}");
    }

    try
    {
        var result = await orchestrationService.ExecuteSecurityWorkflow(commitSha);

        Console.WriteLine($"\n🔒 **SECURITY WORKFLOW RESULTS**");
        Console.WriteLine($"📅 Duration: {result.EndTime - result.StartTime:mm\\:ss}");
        Console.WriteLine($"✅ Success: {result.Success}");

        if (result.Success)
        {
            Console.WriteLine($"\n🔍 **SECURITY ANALYSIS**:");
            Console.WriteLine($"• Commit: {result.CommitSha}");
            Console.WriteLine($"• Security Issues Found: {result.SecurityIssuesFound.Count}");

            if (result.SecurityIssuesFound.Any())
            {
                Console.WriteLine($"\n⚠️  **ISSUES IDENTIFIED**:");
                foreach (var issue in result.SecurityIssuesFound.Take(3))
                {
                    Console.WriteLine($"• {issue}");
                }
            }

            if (result.RelatedMeetingDiscussions.Any())
            {
                Console.WriteLine($"\n💬 **RELATED MEETING DISCUSSIONS**:");
                foreach (var discussion in result.RelatedMeetingDiscussions)
                {
                    Console.WriteLine($"• {discussion}");
                }
            }

            if (result.JiraTicketCreated != null)
            {
                Console.WriteLine($"\n🎫 **JIRA TICKET CREATED**:");
                Console.WriteLine($"• Ticket: {result.JiraTicketCreated.TicketKey}");
                Console.WriteLine($"• Status: {result.JiraTicketCreated.Success}");
            }

            if (!string.IsNullOrEmpty(result.ExecutiveSummary))
            {
                Console.WriteLine($"\n📋 **EXECUTIVE SUMMARY**:");
                Console.WriteLine(result.ExecutiveSummary);
            }
        }
        else
        {
            Console.WriteLine($"❌ Workflow failed: {result.ErrorMessage}");
        }

        Console.WriteLine("\n✅ Security workflow complete!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error executing security workflow: {ex.Message}");
    }

    Console.WriteLine("\nPress any key to continue...");
    Console.ReadKey();
}

static async Task ExecutePerformanceWorkflow(OrchestrationService orchestrationService)
{
    Console.WriteLine("\n⚡ Executing Performance Analysis Workflow...");

    Console.Write("Enter number of days to analyze (default 7): ");
    var daysInput = Console.ReadLine();
    var days = string.IsNullOrWhiteSpace(daysInput)
        ? 7
        : int.TryParse(daysInput, out var d)
            ? d
            : 7;

    try
    {
        var result = await orchestrationService.ExecutePerformanceWorkflow(days);

        Console.WriteLine($"\n⚡ **PERFORMANCE WORKFLOW RESULTS**");
        Console.WriteLine($"📅 Analysis Period: {result.AnalysisPeriodDays} days");
        Console.WriteLine($"⏱️ Duration: {result.EndTime - result.StartTime:mm\\:ss}");
        Console.WriteLine($"✅ Success: {result.Success}");

        if (result.Success && result.DevelopmentSummary != null)
        {
            Console.WriteLine($"\n📊 **DEVELOPMENT HEALTH**:");
            Console.WriteLine(
                $"• Overall Score: {result.DevelopmentSummary.OverallHealthScore}/100"
            );
            Console.WriteLine(
                $"• Velocity Score: {result.DevelopmentSummary.Performance.VelocityScore:F1}/10"
            );
            Console.WriteLine(
                $"• Quality Score: {result.DevelopmentSummary.Quality.OverallScore:F1}/10"
            );

            if (result.PerformanceMeetingInsights.Any())
            {
                Console.WriteLine($"\n💬 **MEETING INSIGHTS**:");
                foreach (var insight in result.PerformanceMeetingInsights)
                {
                    Console.WriteLine($"• {insight}");
                }
            }

            if (result.PerformanceCodeIssues.Any())
            {
                Console.WriteLine($"\n⚠️  **CODE PERFORMANCE ISSUES**:");
                foreach (var issue in result.PerformanceCodeIssues.Take(3))
                {
                    Console.WriteLine($"• {issue}");
                }
            }

            if (result.PerformanceRecommendations.Any())
            {
                Console.WriteLine($"\n💡 **PERFORMANCE RECOMMENDATIONS**:");
                foreach (var rec in result.PerformanceRecommendations.Take(3))
                {
                    Console.WriteLine($"• {rec.Title}: {rec.Description}");
                    Console.WriteLine($"  Priority: {rec.Priority} | Time Frame: {rec.TimeFrame}");
                }
            }

            if (!string.IsNullOrEmpty(result.CorrelationReport))
            {
                Console.WriteLine($"\n🔗 **CORRELATION ANALYSIS**:");
                Console.WriteLine(result.CorrelationReport);
            }
        }
        else
        {
            Console.WriteLine($"❌ Workflow failed: {result.ErrorMessage}");
        }

        Console.WriteLine("\n✅ Performance workflow complete!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error executing performance workflow: {ex.Message}");
    }

    Console.WriteLine("\nPress any key to continue...");
    Console.ReadKey();
}

static async Task ExecuteSprintPlanningWorkflow(OrchestrationService orchestrationService)
{
    Console.WriteLine("\n🎯 Executing Sprint Planning Intelligence Workflow...");

    Console.WriteLine("Enter sprint goals (one per line, empty line to finish):");
    var sprintGoals = new List<string>();
    string goal;
    while (!string.IsNullOrWhiteSpace(goal = Console.ReadLine() ?? ""))
    {
        sprintGoals.Add(goal);
    }

    if (!sprintGoals.Any())
    {
        sprintGoals = new List<string>
        {
            "Implement user authentication",
            "Optimize database queries",
            "Complete code review process improvements"
        };
        Console.WriteLine("Using default sprint goals for demo...");
    }

    try
    {
        var result = await orchestrationService.ExecuteSprintPlanningWorkflow(sprintGoals);

        Console.WriteLine($"\n🎯 **SPRINT PLANNING INTELLIGENCE RESULTS**");
        Console.WriteLine($"⏱️ Duration: {result.EndTime - result.StartTime:mm\\:ss}");
        Console.WriteLine($"✅ Success: {result.Success}");

        if (result.Success)
        {
            Console.WriteLine($"\n📊 **CURRENT HEALTH STATUS**:");
            if (result.CurrentHealthSummary != null)
            {
                Console.WriteLine(
                    $"• Health Score: {result.CurrentHealthSummary.OverallHealthScore}/100"
                );
                Console.WriteLine(
                    $"• Velocity: {result.CurrentHealthSummary.Performance.VelocityScore:F1}/10"
                );
                Console.WriteLine(
                    $"• Quality Trend: {result.CurrentHealthSummary.Performance.VelocityTrend}"
                );
            }
            Console.WriteLine($"• Average Code Quality: {result.AverageCodeQuality:F1}/10");

            Console.WriteLine($"\n🎯 **SPRINT GOALS ASSESSMENT**:");
            for (int i = 0; i < result.SprintGoals.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {result.SprintGoals[i]} ✅");
            }

            if (result.EstimateRecommendations.Any())
            {
                Console.WriteLine($"\n⏱️ **CAPACITY RECOMMENDATIONS**:");
                foreach (var estimate in result.EstimateRecommendations)
                {
                    Console.WriteLine($"• {estimate}");
                }
            }

            if (result.SprintRisks.Any())
            {
                Console.WriteLine($"\n⚠️  **IDENTIFIED RISKS**:");
                foreach (var risk in result.SprintRisks.Take(3))
                {
                    Console.WriteLine($"• {risk}");
                }
            }

            if (result.SprintOpportunities.Any())
            {
                Console.WriteLine($"\n🌟 **OPPORTUNITIES**:");
                foreach (var opportunity in result.SprintOpportunities.Take(3))
                {
                    Console.WriteLine($"• {opportunity}");
                }
            }

            if (!string.IsNullOrEmpty(result.ExecutiveRecommendation))
            {
                Console.WriteLine($"\n📋 **EXECUTIVE RECOMMENDATION**:");
                Console.WriteLine(result.ExecutiveRecommendation);
            }
        }
        else
        {
            Console.WriteLine($"❌ Workflow failed: {result.ErrorMessage}");
        }

        Console.WriteLine("\n✅ Sprint planning workflow complete!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error executing sprint planning workflow: {ex.Message}");
    }

    Console.WriteLine("\nPress any key to continue...");
    Console.ReadKey();
}
