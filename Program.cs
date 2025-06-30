using DotNetEnv;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticKernelDevHub.Agents;

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

    // Initialize and register CodeReviewAgent
    Console.WriteLine("\n🤖 Initializing agents...");
    var codeReviewAgent = new CodeReviewAgent(kernel);
    await codeReviewAgent.InitializeAsync();
    await codeReviewAgent.RegisterFunctionsAsync(kernel);

    // Get registered functions
    var functions = kernel.Plugins.GetFunctionsMetadata();
    Console.WriteLine($"� Available functions: [{string.Join(", ", functions.Select(f => f.Name))}]");
    
    Console.WriteLine("\n🎉 Semantic Kernel with Agents Ready!");

    // Interactive menu
    await RunInteractiveMenu(kernel, codeReviewAgent);
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error initializing Semantic Kernel: {ex.Message}");
    Console.WriteLine($"📋 Details: {ex}");
}

static async Task RunInteractiveMenu(Kernel kernel, CodeReviewAgent codeReviewAgent)
{
    while (true)
    {
        Console.WriteLine("\n" + new string('=', 50));
        Console.WriteLine("🚀 Semantic Kernel DevHub - Agent Menu");
        Console.WriteLine(new string('=', 50));
        Console.WriteLine("Choose an option:");
        Console.WriteLine("1. Test Code Review Agent");
        Console.WriteLine("2. Analyze Sample Code");
        Console.WriteLine("3. Check Coding Standards");
        Console.WriteLine("4. Review GitHub Pull Request");
        Console.WriteLine("5. Exit");
        Console.Write("\nEnter your choice (1-5): ");

        var choice = Console.ReadLine();

        try
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
                    Console.WriteLine("\n👋 Thank you for using Semantic Kernel DevHub!");
                    return;
                default:
                    Console.WriteLine("\n❌ Invalid choice. Please enter 1-5.");
                    break;
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
