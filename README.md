# Semantic Kernel DevHub - GitHub Code Review Agent

A comprehensive multi-phase Semantic Kernel application with deep GitHub integration for intelligent code review across multiple programming languages.

## 🚀 Features

### Phase 1: Core Foundation

- ✅ Semantic Kernel integration with Azure OpenAI
- ✅ Environment configuration via `.env` file
- ✅ Modular project structure with proper separation of concerns

### Phase 2: Agent Architecture

- ✅ `IAgent` interface for extensible agent system
- ✅ `CodeReviewAgent` with Semantic Kernel function registration
- ✅ Multi-language code analysis support (C#, VB.NET, T-SQL, JavaScript, React, Java)

### Phase 3: GitHub Integration

- ✅ **GitHubPlugin** with comprehensive GitHub API integration via Octokit
- ✅ **Interactive Menu System** with 8 different code review options
- ✅ **Real-time GitHub Data Access** (commits, PRs, files, repository info)

## 🎯 Supported Languages

The system provides intelligent code review for:

- **C#** - .NET applications, syntax validation, best practices
- **VB.NET** - Visual Basic .NET code analysis
- **T-SQL** - Database queries, stored procedures, optimization
- **JavaScript** - Modern JS, ES6+, Node.js applications
- **React** - Component analysis, hooks, performance patterns
- **Java** - Enterprise applications, Spring framework

## 🛠️ Architecture

```
SemanticKernelDevHub/
├── Program.cs                    # Main application entry point
├── Agents/
│   ├── IAgent.cs                # Agent interface
│   └── CodeReviewAgent.cs       # Main code review agent
├── Plugins/
│   └── GitHubPlugin.cs          # GitHub API integration
├── Models/
│   ├── GitHubCommitInfo.cs      # Commit data models
│   ├── GitHubFileInfo.cs        # File information models
│   └── CodeReviewRequest.cs     # Review request/result models
└── Data/
    ├── Incoming/                # Input data staging
    ├── Archive/                 # Processed data archive
    └── Templates/               # Code review templates
```

## 🎮 Interactive Menu Options

1. **Review Latest Commit** - Analyze the most recent repository commit
2. **List Recent Commits** - Display recent commit history with details
3. **Review Specific Commit** - Deep analysis of a particular commit by SHA
4. **Review Pull Request** - Comprehensive PR review with recommendations
5. **Analyze Custom Code** - Review any code snippet directly
6. **Check Coding Standards** - Validate against language-specific best practices
7. **Repository Information** - Display repository metadata and statistics
8. **Exit** - Graceful application shutdown

## 🔧 Configuration

Create a `.env` file in the root directory:

```env
# Azure OpenAI Configuration
AOAI_ENDPOINT=https://your-instance.openai.azure.com/
AOAI_APIKEY=your-api-key-here
CHATCOMPLETION_DEPLOYMENTNAME=gpt-35-turbo

# GitHub Configuration
GITHUB_TOKEN=your-github-token
GITHUB_REPO_OWNER=your-username
GITHUB_REPO_NAME=your-repository
```

## 🚀 Usage

```bash
# Build the project
dotnet build

# Run the application
dotnet run
```

## 📋 Registered Semantic Kernel Functions

The application registers 13 functions with the Semantic Kernel:

**GitHub Integration Functions:**

- `get_recent_commits` - Retrieve recent repository commits
- `get_commit_details` - Get detailed information about a specific commit
- `get_pull_request` - Fetch pull request information
- `get_file_content` - Retrieve file contents from repository
- `list_commit_files` - List files changed in a commit
- `get_repository_info` - Get repository metadata

**Code Analysis Functions:**

- `analyze_code` - Perform intelligent code analysis
- `suggest_improvements` - Generate improvement recommendations
- `check_coding_standards` - Validate coding standards compliance
- `review_pull_request` - Comprehensive pull request review
- `review_commit` - Detailed commit analysis
- `review_latest_commit` - Quick review of the latest commit
- `list_recent_commits` - Display formatted commit history

## 🎉 Key Achievements

### ✅ **Complete Integration Stack**

- Semantic Kernel + Azure OpenAI + GitHub API
- Real-time data retrieval and AI-powered analysis
- Interactive console application with full menu system

### ✅ **Production-Ready Architecture**

- Clean separation of concerns with agents, plugins, and models
- Proper error handling and validation
- Environment-based configuration management

### ✅ **Multi-Language Support**

- Language-specific prompts and analysis patterns
- Extensible framework for adding new languages
- Tailored recommendations per technology stack

### ✅ **Live GitHub Integration**

- Real repository data access via Octokit
- Commit history analysis with proper formatting
- Repository metadata and statistics

## 🧪 Verified Functionality

**✅ Repository Information Retrieval:**

```
Name: AzureOpenAIConsole
Description: A modular C# console application demonstrating Azure OpenAI...
Language: C#, Stars: 0, Forks: 0, Issues: 1
Created: 2025-06-28, Updated: 2025-06-29
```

**✅ Recent Commits Display:**

```
fd7608cc - Making the app more generic, by introducing a service...
👤 Darko Martinovic | 📅 2025-06-29 06:50

7fdc2078 - Code Reorganization/Cleaning up the using statements...
👤 Darko Martinovic | 📅 2025-06-29 06:35
```

## 🎯 Next Steps

The foundation is complete and fully functional. Potential enhancements:

1. **Advanced Analysis** - Add complexity metrics, security vulnerability detection
2. **Team Collaboration** - Multi-repository support, team review workflows
3. **CI/CD Integration** - GitHub Actions integration, automated PR reviews
4. **Reporting** - Generate detailed reports, export analysis results
5. **Web Interface** - Build a web-based UI for the review system

## 🏆 Success Metrics

- ✅ **13 Semantic Kernel functions** registered and operational
- ✅ **Multi-language code review** capabilities implemented
- ✅ **Real-time GitHub integration** via Octokit API
- ✅ **Interactive menu system** with 8 operational options
- ✅ **Production-ready architecture** with proper error handling
- ✅ **Live data verification** - actual repository data retrieved and displayed

The Semantic Kernel DevHub represents a complete, production-ready solution for AI-powered code review with deep GitHub integration.
