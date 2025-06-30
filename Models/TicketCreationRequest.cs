namespace SemanticKernelDevHub.Models;

/// <summary>
/// Data structure for creating new Jira tickets
/// </summary>
public class TicketCreationRequest
{
    /// <summary>
    /// Project key where the ticket will be created
    /// </summary>
    public string ProjectKey { get; set; } = string.Empty;

    /// <summary>
    /// Ticket title/summary
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the ticket
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Issue type (Task, Bug, Story, Epic)
    /// </summary>
    public string IssueType { get; set; } = "Task";

    /// <summary>
    /// Priority level (High, Medium, Low)
    /// </summary>
    public string Priority { get; set; } = "Medium";

    /// <summary>
    /// Assignee username or email
    /// </summary>
    public string? Assignee { get; set; }

    /// <summary>
    /// Reporter username or email
    /// </summary>
    public string? Reporter { get; set; }

    /// <summary>
    /// Labels to apply to the ticket
    /// </summary>
    public List<string> Labels { get; set; } = new();

    /// <summary>
    /// Components associated with the ticket
    /// </summary>
    public List<string> Components { get; set; } = new();

    /// <summary>
    /// Due date for the ticket
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Story points estimate
    /// </summary>
    public int? StoryPoints { get; set; }

    /// <summary>
    /// Parent ticket key (for subtasks)
    /// </summary>
    public string? ParentKey { get; set; }

    /// <summary>
    /// Custom fields for the ticket
    /// </summary>
    public Dictionary<string, object> CustomFields { get; set; } = new();

    /// <summary>
    /// Source of the ticket creation (e.g., "Meeting Analysis", "Code Review")
    /// </summary>
    public string Source { get; set; } = "Manual";

    /// <summary>
    /// Creates a ticket request from an action item
    /// </summary>
    /// <param name="actionItem">Action item to convert</param>
    /// <param name="projectKey">Target project key</param>
    /// <returns>Ticket creation request</returns>
    public static TicketCreationRequest FromActionItem(ActionItem actionItem, string projectKey)
    {
        var priority = actionItem.Priority switch
        {
            ActionItemPriority.High => "High",
            ActionItemPriority.Medium => "Medium",
            ActionItemPriority.Low => "Low",
            _ => "Medium"
        };

        var description = $@"**Action Item from Meeting Analysis**

**Original Task**: {actionItem.Task}

**Context**: {actionItem.Context ?? "No additional context provided"}

**Notes**: {actionItem.Notes ?? "No additional notes"}

**Meeting Details**:
- **Due Date**: {actionItem.DueDate?.ToString("yyyy-MM-dd") ?? "Not specified"}
- **Generated**: {DateTime.Now:yyyy-MM-dd HH:mm}

---
*This ticket was automatically created from meeting action item analysis by Semantic Kernel DevHub*";

        return new TicketCreationRequest
        {
            ProjectKey = projectKey,
            Title = actionItem.Task,
            Description = description,
            IssueType = "Task",
            Priority = priority,
            Assignee = actionItem.AssignedTo,
            DueDate = actionItem.DueDate,
            Labels = new List<string> { "meeting-action-item", "auto-generated" },
            Source = "Meeting Analysis"
        };
    }

    /// <summary>
    /// Creates a ticket request from code review results
    /// </summary>
    /// <param name="reviewResult">Code review result</param>
    /// <param name="projectKey">Target project key</param>
    /// <param name="commitSha">Commit SHA being reviewed</param>
    /// <returns>Ticket creation request</returns>
    public static TicketCreationRequest FromCodeReview(CodeReviewResult reviewResult, string projectKey, string? commitSha = null)
    {
        var priority = reviewResult.OverallScore switch
        {
            <= 3 => "High",
            <= 6 => "Medium",
            _ => "Low"
        };

        var issueType = reviewResult.OverallScore <= 5 ? "Bug" : "Task";

        var title = $"Code Review Issues - {(string.IsNullOrEmpty(commitSha) ? "Multiple Files" : $"Commit {commitSha.Substring(0, 8)}")}";

        var description = $@"**Automated Code Review Results**

**Overall Score**: {reviewResult.OverallScore}/10
**Review Type**: {reviewResult.Metadata.ReviewType}
**Target**: {reviewResult.Metadata.Target}
**Review Date**: {reviewResult.Metadata.ReviewDate:yyyy-MM-dd HH:mm}
**Files Analyzed**: {reviewResult.Metadata.TotalFilesAnalyzed}

**Summary**:
{reviewResult.Summary}

**Key Issues Found**:
{(reviewResult.KeyIssues.Any() ? string.Join("\n", reviewResult.KeyIssues.Select(i => $"• {i}")) : "No significant issues found")}

**Recommendations**:
{(reviewResult.Recommendations.Any() ? string.Join("\n", reviewResult.Recommendations.Select(r => $"• {r}")) : "No specific recommendations")}

**File-Specific Issues**:
{string.Join("\n", reviewResult.FileReviews.Where(f => f.Score < 7).Select(f => $@"
**{f.FileName}** (Score: {f.Score}/10)
{(f.Issues.Any() ? string.Join("\n", f.Issues.Select(i => $"  - {i}")) : "  - No specific issues")}
"))}

---
*This ticket was automatically generated from code review analysis by Semantic Kernel DevHub*";

        return new TicketCreationRequest
        {
            ProjectKey = projectKey,
            Title = title,
            Description = description,
            IssueType = issueType,
            Priority = priority,
            Labels = new List<string> { "code-review", "auto-generated", "quality-improvement" },
            Source = "Code Review"
        };
    }

    /// <summary>
    /// Creates a ticket request for a bug report
    /// </summary>
    /// <param name="title">Bug title</param>
    /// <param name="description">Bug description</param>
    /// <param name="projectKey">Target project key</param>
    /// <param name="priority">Bug priority</param>
    /// <returns>Ticket creation request</returns>
    public static TicketCreationRequest CreateBugReport(string title, string description, string projectKey, string priority = "Medium")
    {
        return new TicketCreationRequest
        {
            ProjectKey = projectKey,
            Title = title,
            Description = description,
            IssueType = "Bug",
            Priority = priority,
            Labels = new List<string> { "bug", "needs-investigation" },
            Source = "Bug Report"
        };
    }

    /// <summary>
    /// Creates a ticket request for a feature request
    /// </summary>
    /// <param name="title">Feature title</param>
    /// <param name="description">Feature description</param>
    /// <param name="projectKey">Target project key</param>
    /// <param name="priority">Feature priority</param>
    /// <returns>Ticket creation request</returns>
    public static TicketCreationRequest CreateFeatureRequest(string title, string description, string projectKey, string priority = "Medium")
    {
        return new TicketCreationRequest
        {
            ProjectKey = projectKey,
            Title = title,
            Description = description,
            IssueType = "Story",
            Priority = priority,
            Labels = new List<string> { "feature-request", "enhancement" },
            Source = "Feature Request"
        };
    }

    /// <summary>
    /// Validates the ticket creation request
    /// </summary>
    /// <returns>List of validation errors</returns>
    public List<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(ProjectKey))
            errors.Add("Project key is required");

        if (string.IsNullOrWhiteSpace(Title))
            errors.Add("Title is required");

        if (string.IsNullOrWhiteSpace(Description))
            errors.Add("Description is required");

        if (Title.Length > 255)
            errors.Add("Title must be 255 characters or less");

        var validIssueTypes = new[] { "Task", "Bug", "Story", "Epic", "Subtask" };
        if (!validIssueTypes.Contains(IssueType, StringComparer.OrdinalIgnoreCase))
            errors.Add($"Issue type must be one of: {string.Join(", ", validIssueTypes)}");

        var validPriorities = new[] { "Highest", "High", "Medium", "Low", "Lowest" };
        if (!validPriorities.Contains(Priority, StringComparer.OrdinalIgnoreCase))
            errors.Add($"Priority must be one of: {string.Join(", ", validPriorities)}");

        return errors;
    }

    /// <summary>
    /// Gets a summary of the ticket request
    /// </summary>
    /// <returns>Formatted summary</returns>
    public string GetSummary()
    {
        return $@"**Ticket Creation Request Summary**

**Project**: {ProjectKey}
**Title**: {Title}
**Type**: {IssueType}
**Priority**: {Priority}
**Source**: {Source}
{(string.IsNullOrEmpty(Assignee) ? "" : $"**Assignee**: {Assignee}")}
{(DueDate.HasValue ? $"**Due Date**: {DueDate:yyyy-MM-dd}" : "")}
{(Labels.Any() ? $"**Labels**: {string.Join(", ", Labels)}" : "")}

**Description Preview**: {(Description.Length > 100 ? Description.Substring(0, 97) + "..." : Description)}";
    }

    public override string ToString() => $"{ProjectKey}: {Title} [{IssueType}/{Priority}]";
}
