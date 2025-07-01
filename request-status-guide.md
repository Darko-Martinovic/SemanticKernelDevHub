# How to Check Request Status in Semantic Kernel DevHub

Based on the codebase analysis, here are the various ways to check the status of requests you submitted a few minutes ago:

## 🎯 Quick Status Check Methods

### 1. **Jira Ticket Status** (Most Likely)
If your request was a Jira ticket creation or update:

```bash
# Run the application
cd /workspace
dotnet run
```

Then select from the menu:
- **Option 11**: Test Jira Connection
- **Option 12**: Create Sample Jira Ticket (to see recent activity)
- **Option 13**: Update Existing Jira Ticket

### 2. **GitHub Integration Status**
If your request was related to code review or GitHub operations:

```bash
# Check recent commits
git log --oneline -10

# Look for recent activity
git status
```

From the application menu:
- **Option 1**: Review Latest Commit
- **Option 2**: List Recent Commits
- **Option 4**: Review Pull Request

### 3. **File Processing Status**
If your request was for meeting transcript or file analysis:

```bash
# Check incoming files directory
ls -la Data/Incoming/
```

From the application menu:
- **Option 8**: Process Meeting Transcript
- **Option 9**: Start File Watcher Mode

## 🔍 Detailed Status Checking

### Application-Level Status
The application provides real-time feedback through:
- ✅ Success indicators
- ❌ Error messages
- 📊 Progress reports
- 🔍 Processing status

### Integration-Specific Status Checks

#### **Jira Plugin Status**
```csharp
// Available functions in JiraPlugin:
- test_jira_connection()      // Test connectivity
- search_jira_tickets()       // Find your tickets
- get_jira_ticket()          // Get specific ticket details
- create_jira_ticket()       // Create new ticket
- update_jira_ticket()       // Update existing ticket
- add_jira_comment()         // Add comments
```

#### **GitHub Plugin Status**
```csharp
// Available functions in GitHubPlugin:
- Review latest commits
- List recent commits
- Review specific commit
- Review pull requests
- Repository information
```

#### **File System Plugin Status**
```csharp
// File processing status:
- ListIncomingFiles()        // Check pending files
- File watcher mode          // Real-time monitoring
- Processing results         // Analysis outcomes
```

## 🚀 Recommended Steps to Check Your Request

### Step 1: Determine Request Type
Based on what you submitted a few minutes ago:
- **Code review** → Check GitHub integration
- **Ticket creation** → Check Jira integration  
- **File analysis** → Check file system status

### Step 2: Run the Application
```bash
cd /workspace

# Install .NET SDK if needed (currently missing)
# The application requires .NET to run

# Once .NET is available:
dotnet run
```

### Step 3: Use Appropriate Menu Option
Select the relevant option from the interactive menu based on your request type.

### Step 4: Search for Your Request
Use the search functions to find your specific request:
- Jira: Search by ticket key or keywords
- GitHub: List recent commits/PRs
- Files: Check incoming directory

## 📋 Current System Status

Based on the workspace analysis:
- **Application**: Not currently running
- **Recent Activity**: Jira integration was recently completed (latest commit)
- **Available Files**: Sample code file in Data/Incoming/
- **.NET SDK**: Not installed (required to run the application)

## 🛠️ Troubleshooting

### If Application Won't Start:
1. Install .NET SDK
2. Check environment variables (.env file)
3. Verify integration credentials

### If Request Status Unclear:
1. Check console output for error messages
2. Look for integration-specific logs
3. Use test connection functions
4. Search by relevant keywords

### Integration-Specific Issues:
- **Jira**: Check JIRA_URL, JIRA_EMAIL, JIRA_API_TOKEN, JIRA_PROJECT_KEY
- **GitHub**: Check GITHUB_TOKEN, GITHUB_REPO_OWNER, GITHUB_REPO_NAME
- **Azure OpenAI**: Check AOAI_ENDPOINT, AOAI_APIKEY, CHATCOMPLETION_DEPLOYMENTNAME

## 📞 Getting Help

If you can't find your request status:
1. Provide more details about the type of request submitted
2. Specify which integration was used (Jira, GitHub, File processing)
3. Share any error messages or identifiers from your original request

The system is designed to provide immediate feedback, so if a request was submitted successfully, there should be visible confirmation with specific identifiers (ticket numbers, commit hashes, etc.).