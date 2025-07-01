using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using SemanticKernelDevHub.Models;
using SemanticKernelDevHub.Plugins;

namespace SemanticKernelDevHub.Agents;

/// <summary>
/// Agent responsible for Jira integration and ticket management
/// </summary>
public class JiraIntegrationAgent : IAgent
{
    private readonly Kernel _kernel;
    private readonly JiraPlugin _jiraPlugin;
    private readonly string _projectKey;

    public string Name => "JiraIntegrationAgent";

    public string Description => "Manages Jira ticket operations, auto-detects ticket references, and integrates with code review and meeting analysis workflows";

    public JiraIntegrationAgent(Kernel kernel, JiraPlugin jiraPlugin, string projectKey)
    {
        _kernel = kernel;
        _jiraPlugin = jiraPlugin;
        _projectKey = projectKey;
    }

    public Task InitializeAsync()
    {
        Console.WriteLine($"✅ {Name} initialized successfully");
        return Task.CompletedTask;
    }

    public Task RegisterFunctionsAsync(Kernel kernel)
    {
        kernel.ImportPluginFromObject(this, "JiraIntegration");
        Console.WriteLine($"🔧 {Name} functions registered with kernel");
        return Task.CompletedTask;
    }

    public IEnumerable<string> GetFunctionNames()
    {
        return new[]
        {
            "CreateTicketFromActionItem",
            "UpdateTicketWithCodeReview",
            "DetectTicketReferences",
            "CreateTicketsFromMeeting",
            "LinkCodeReviewToTickets"
        };
    }

    /// <summary>
    /// Creates a Jira ticket from a meeting action item
    /// </summary>
    /// <param name="actionItem">Action item to convert to ticket</param>
    /// <returns>Ticket creation result</returns>
    [KernelFunction("create_ticket_from_action_item")]
    [Description("Creates a Jira ticket from a meeting action item")]
    public async Task<JiraOperationResult> CreateTicketFromActionItem(
        [Description("Action item to convert to a Jira ticket")] ActionItem actionItem)
    {
        try
        {
            Console.WriteLine($"🎫 Creating Jira ticket from action item: {actionItem.Description}");

            var ticketRequest = TicketCreationRequest.FromActionItem(actionItem, _projectKey);

            // Validate the request
            var validationErrors = ticketRequest.Validate();
            if (validationErrors.Any())
            {
                return JiraOperationResult.Failure(
                    $"❌ Validation failed: {string.Join(", ", validationErrors)}",
                    "CREATE_TICKET"
                );
            }

            // Create the ticket using Jira plugin
            var result = await _jiraPlugin.CreateTicket(
                ticketRequest.Title,
                ticketRequest.Description,
                ticketRequest.Priority,
                ticketRequest.IssueType
            );

            if (result.Success)
            {
                Console.WriteLine($"✅ Created ticket {result.TicketKey} for action item");

                // Use Semantic Kernel to generate a follow-up comment
                var followUpComment = await GenerateActionItemFollowUp(actionItem, result.TicketKey!);
                if (!string.IsNullOrEmpty(followUpComment))
                {
                    await _jiraPlugin.AddComment(result.TicketKey!, followUpComment);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            return JiraOperationResult.Failure($"❌ Error creating ticket from action item: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates a Jira ticket with code review results
    /// </summary>
    /// <param name="ticketKey">Jira ticket key to update</param>
    /// <param name="reviewResult">Code review results</param>
    /// <returns>Operation result</returns>
    [KernelFunction("update_ticket_with_code_review")]
    [Description("Updates a Jira ticket with code review results and recommendations")]
    public async Task<JiraOperationResult> UpdateTicketWithCodeReview(
        [Description("Jira ticket key (e.g., OPS-123)")] string ticketKey,
        [Description("Code review results to add to the ticket")] CodeReviewResult reviewResult)
    {
        try
        {
            Console.WriteLine($"🎫 Updating ticket {ticketKey} with code review results");

            // Create a formatted comment using the JiraComment helper
            var comment = JiraComment.CreateCodeReviewComment(reviewResult);

            // Add the comment to the ticket
            var result = await _jiraPlugin.AddComment(ticketKey, comment.Body);

            if (result.Success)
            {
                Console.WriteLine($"✅ Updated ticket {ticketKey} with code review results");

                // If the review score is low, use SK to suggest priority update
                if (reviewResult.OverallScore <= 5)
                {
                    var priorityRecommendation = await GeneratePriorityRecommendation(reviewResult);
                    if (!string.IsNullOrEmpty(priorityRecommendation))
                    {
                        await _jiraPlugin.AddComment(ticketKey, $"🔥 **Priority Recommendation**: {priorityRecommendation}");
                    }
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            return JiraOperationResult.Failure($"❌ Error updating ticket with code review: {ex.Message}");
        }
    }

    /// <summary>
    /// Detects ticket references in text using pattern matching
    /// </summary>
    /// <param name="text">Text to scan for ticket references</param>
    /// <returns>List of detected ticket keys</returns>
    [KernelFunction("detect_ticket_references")]
    [Description("Detects Jira ticket references in text using pattern matching")]
    public List<string> DetectTicketReferences(
        [Description("Text to scan for ticket references (e.g., PR titles, commit messages)")] string text)
    {
        var ticketKeys = new List<string>();

        try
        {
            // Pattern to match ticket keys like OPS-123, PROJ-456, etc.
            var pattern = @"\b([A-Z]+)-(\d+)\b";
            var matches = Regex.Matches(text, pattern, RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                var ticketKey = match.Value.ToUpper();
                if (!ticketKeys.Contains(ticketKey))
                {
                    ticketKeys.Add(ticketKey);
                }
            }

            if (ticketKeys.Any())
            {
                Console.WriteLine($"🔍 Detected {ticketKeys.Count} ticket references: {string.Join(", ", ticketKeys)}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error detecting ticket references: {ex.Message}");
        }

        return ticketKeys;
    }

    /// <summary>
    /// Creates multiple Jira tickets from meeting analysis results
    /// </summary>
    /// <param name="meetingResult">Meeting analysis results</param>
    /// <returns>List of ticket creation results</returns>
    [KernelFunction("create_tickets_from_meeting")]
    [Description("Creates multiple Jira tickets from meeting analysis action items")]
    public async Task<List<JiraOperationResult>> CreateTicketsFromMeeting(
        [Description("Meeting analysis results containing action items")] MeetingAnalysisResult meetingResult)
    {
        var results = new List<JiraOperationResult>();

        try
        {
            Console.WriteLine($"🎫 Creating {meetingResult.ActionItems.Count} tickets from meeting analysis");

            foreach (var actionItem in meetingResult.ActionItems)
            {
                var result = await CreateTicketFromActionItem(actionItem);
                results.Add(result);

                // Brief delay between ticket creations to avoid rate limiting
                await Task.Delay(500);
            }

            var successCount = results.Count(r => r.Success);
            Console.WriteLine($"✅ Successfully created {successCount}/{results.Count} tickets from meeting");

            // Use SK to generate a meeting summary comment for successful tickets
            if (successCount > 0)
            {
                var summaryComment = await GenerateMeetingSummaryComment(meetingResult, results);
                foreach (var successfulResult in results.Where(r => r.Success))
                {
                    await _jiraPlugin.AddComment(successfulResult.TicketKey!, summaryComment);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error creating tickets from meeting: {ex.Message}");
            results.Add(JiraOperationResult.Failure($"Error creating tickets from meeting: {ex.Message}"));
        }

        return results;
    }

    /// <summary>
    /// Links code review results to related Jira tickets found in PR/commit text
    /// </summary>
    /// <param name="reviewResult">Code review results</param>
    /// <param name="prTitle">Pull request title or commit message</param>
    /// <returns>List of linking operation results</returns>
    [KernelFunction("link_code_review_to_tickets")]
    [Description("Links code review results to Jira tickets detected in PR titles or commit messages")]
    public async Task<List<JiraOperationResult>> LinkCodeReviewToTickets(
        [Description("Code review results to link")] CodeReviewResult reviewResult,
        [Description("PR title or commit message to scan for ticket references")] string prTitle)
    {
        var results = new List<JiraOperationResult>();

        try
        {
            // Detect ticket references in the PR title
            var ticketKeys = DetectTicketReferences(prTitle);

            if (!ticketKeys.Any())
            {
                Console.WriteLine("🔍 No ticket references found in PR title");
                return results;
            }

            Console.WriteLine($"🔗 Linking code review to {ticketKeys.Count} detected tickets");

            foreach (var ticketKey in ticketKeys)
            {
                // Check if ticket exists
                var ticket = await _jiraPlugin.GetTicket(ticketKey);
                if (ticket == null)
                {
                    results.Add(JiraOperationResult.Failure($"Ticket {ticketKey} not found", "LINK_REVIEW"));
                    continue;
                }

                // Update ticket with code review results
                var result = await UpdateTicketWithCodeReview(ticketKey, reviewResult);
                results.Add(result);

                // Brief delay between updates
                await Task.Delay(300);
            }

            var successCount = results.Count(r => r.Success);
            Console.WriteLine($"✅ Successfully linked code review to {successCount}/{results.Count} tickets");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error linking code review to tickets: {ex.Message}");
            results.Add(JiraOperationResult.Failure($"Error linking code review: {ex.Message}"));
        }

        return results;
    }

    /// <summary>
    /// Orchestrates the complete workflow of processing a PR with Jira integration
    /// </summary>
    /// <param name="prTitle">Pull request title</param>
    /// <param name="reviewResult">Code review results</param>
    /// <returns>Comprehensive workflow result</returns>
    [KernelFunction("orchestrate_pr_workflow")]
    [Description("Orchestrates the complete PR workflow with Jira integration including ticket detection and updates")]
    public async Task<string> OrchestratePRWorkflow(
        [Description("Pull request title")] string prTitle,
        [Description("Code review results")] CodeReviewResult reviewResult)
    {
        try
        {
            Console.WriteLine($"🔄 Orchestrating PR workflow for: {prTitle}");

            var workflowSteps = new List<string>();

            // Step 1: Detect ticket references
            var ticketKeys = DetectTicketReferences(prTitle);
            workflowSteps.Add($"🔍 Detected {ticketKeys.Count} ticket references: {string.Join(", ", ticketKeys)}");

            if (ticketKeys.Any())
            {
                // Step 2: Link code review to existing tickets
                var linkResults = await LinkCodeReviewToTickets(reviewResult, prTitle);
                var successfulLinks = linkResults.Count(r => r.Success);
                workflowSteps.Add($"🔗 Successfully linked code review to {successfulLinks}/{linkResults.Count} tickets");

                // Step 3: Generate SK-powered insights for high-priority issues
                if (reviewResult.OverallScore <= 6)
                {
                    var insights = await GenerateWorkflowInsights(reviewResult, ticketKeys);
                    workflowSteps.Add($"🧠 Generated AI insights: {insights}");
                }
            }
            else
            {
                // Step 4: Suggest creating a new ticket if review found issues
                if (reviewResult.OverallScore <= 7 && reviewResult.KeyIssues.Any())
                {
                    var suggestion = await GenerateTicketCreationSuggestion(reviewResult, prTitle);
                    workflowSteps.Add($"💡 Ticket creation suggestion: {suggestion}");
                }
            }

            // Step 5: Generate workflow summary using Semantic Kernel
            var summary = await GenerateWorkflowSummary(prTitle, reviewResult, workflowSteps);

            return $@"🎯 **PR Workflow Complete**

{summary}

**Workflow Steps**:
{string.Join("\n", workflowSteps.Select((step, index) => $"{index + 1}. {step}"))}

**Review Summary**:
- **Score**: {reviewResult.OverallScore}/10
- **Files Analyzed**: {reviewResult.FileReviews.Count}
- **Key Issues**: {reviewResult.KeyIssues.Count}
- **Recommendations**: {reviewResult.Recommendations.Count}

---
*Workflow orchestrated by Semantic Kernel DevHub*";
        }
        catch (Exception ex)
        {
            return $"❌ Error orchestrating PR workflow: {ex.Message}";
        }
    }

    /// <summary>
    /// Tests Jira connectivity and permissions
    /// </summary>
    /// <returns>Connection test result</returns>
    [KernelFunction("test_jira_connectivity")]
    [Description("Tests Jira connection and validates permissions")]
    public async Task<string> TestJiraConnectivity()
    {
        try
        {
            Console.WriteLine("🔌 Testing Jira connectivity...");
            return await _jiraPlugin.TestConnection();
        }
        catch (Exception ex)
        {
            return $"❌ Jira connectivity test failed: {ex.Message}";
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Generates a follow-up comment for action item tickets using SK
    /// </summary>
    private async Task<string> GenerateActionItemFollowUp(ActionItem actionItem, string ticketKey)
    {
        var prompt = $@"
Generate a helpful follow-up comment for a Jira ticket created from this meeting action item:

**Action Item**: {actionItem.Description}
**Assigned To**: {actionItem.AssignedTo ?? "Unassigned"}
**Priority**: {actionItem.Priority}
**Due Date**: {actionItem.DueDate?.ToString("yyyy-MM-dd") ?? "Not specified"}
**Context**: {actionItem.Notes ?? "No additional context"}

Create a brief, actionable comment that:
1. Suggests next steps
2. Identifies potential blockers
3. Recommends resources or stakeholders to involve
4. Provides timeline guidance

Keep it professional and under 150 words.";

        try
        {
            var response = await _kernel.InvokePromptAsync(prompt);
            return $"🤖 **AI-Generated Follow-up**:\n\n{response}";
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Generates priority recommendation based on code review score
    /// </summary>
    private async Task<string> GeneratePriorityRecommendation(CodeReviewResult reviewResult)
    {
        var prompt = $@"
Based on this code review result, recommend if the Jira ticket priority should be adjusted:

**Overall Score**: {reviewResult.OverallScore}/10
**Key Issues**: {string.Join(", ", reviewResult.KeyIssues)}
**Files Analyzed**: {reviewResult.FileReviews.Count}

Current assessment suggests this may need higher priority attention. 
Provide a brief recommendation (1-2 sentences) on whether to increase priority and why.";

        try
        {
            var response = await _kernel.InvokePromptAsync(prompt);
            return response.ToString();
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Generates a meeting summary comment for created tickets
    /// </summary>
    private async Task<string> GenerateMeetingSummaryComment(MeetingAnalysisResult meetingResult, List<JiraOperationResult> results)
    {
        var prompt = $@"
Generate a summary comment for Jira tickets created from a meeting analysis:

**Meeting**: {meetingResult.SourceTranscript.Title}
**Date**: {meetingResult.SourceTranscript.MeetingDate:yyyy-MM-dd}
**Participants**: {meetingResult.Participants.Count}
**Total Action Items**: {meetingResult.ActionItems.Count}
**Tickets Created**: {results.Count(r => r.Success)}

Create a brief summary linking this ticket to the broader meeting context and related tickets.
Mention the meeting outcomes and how this action item contributes to the overall goals.
Keep it under 100 words.";

        try
        {
            var response = await _kernel.InvokePromptAsync(prompt);
            return $"📋 **Meeting Context**:\n\n{response}";
        }
        catch
        {
            return "📋 This ticket was created from meeting action item analysis.";
        }
    }

    /// <summary>
    /// Generates workflow insights using Semantic Kernel
    /// </summary>
    private async Task<string> GenerateWorkflowInsights(CodeReviewResult reviewResult, List<string> ticketKeys)
    {
        var prompt = $@"
Analyze this code review result in the context of these Jira tickets and provide insights:

**Tickets**: {string.Join(", ", ticketKeys)}
**Review Score**: {reviewResult.OverallScore}/10
**Key Issues**: {string.Join(", ", reviewResult.KeyIssues.Take(3))}

Provide 1-2 key insights about:
1. How the code review findings relate to the ticket objectives
2. Whether the issues found suggest scope creep or additional work needed
3. Risk assessment for the current implementation

Keep it concise and actionable.";

        try
        {
            var response = await _kernel.InvokePromptAsync(prompt);
            return response.ToString();
        }
        catch
        {
            return "Analysis completed with standard workflow.";
        }
    }

    /// <summary>
    /// Generates ticket creation suggestion using SK
    /// </summary>
    private async Task<string> GenerateTicketCreationSuggestion(CodeReviewResult reviewResult, string prTitle)
    {
        var prompt = $@"
Based on this code review, should we create a new Jira ticket?

**PR Title**: {prTitle}
**Review Score**: {reviewResult.OverallScore}/10
**Key Issues**: {string.Join(", ", reviewResult.KeyIssues.Take(3))}

The PR doesn't reference existing tickets but has quality issues.
Suggest whether to create a follow-up ticket and what it should focus on.
Provide a brief recommendation with a suggested ticket title.";

        try
        {
            var response = await _kernel.InvokePromptAsync(prompt);
            return response.ToString();
        }
        catch
        {
            return "Consider creating a follow-up ticket for code quality improvements.";
        }
    }

    /// <summary>
    /// Generates comprehensive workflow summary using SK
    /// </summary>
    private async Task<string> GenerateWorkflowSummary(string prTitle, CodeReviewResult reviewResult, List<string> workflowSteps)
    {
        var prompt = $@"
Create a concise summary for this PR workflow execution:

**PR**: {prTitle}
**Review Score**: {reviewResult.OverallScore}/10
**Workflow Steps Completed**: {workflowSteps.Count}

Summarize the key outcomes and next actions in 2-3 sentences.
Focus on what was accomplished and any follow-up needed.";

        try
        {
            var response = await _kernel.InvokePromptAsync(prompt);
            return response.ToString();
        }
        catch
        {
            return $"Completed automated workflow for PR: {prTitle}. Code review analysis and Jira integration processed successfully.";
        }
    }

    #endregion
}
