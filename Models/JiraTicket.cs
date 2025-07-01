namespace SemanticKernelDevHub.Models;

/// <summary>
/// Represents a Jira ticket/issue
/// </summary>
public class JiraTicket
{
    /// <summary>
    /// Jira ticket key (e.g., OPS-123)
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Ticket title/summary
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the ticket
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Priority level (High, Medium, Low)
    /// </summary>
    public string Priority { get; set; } = "Medium";

    /// <summary>
    /// Current status (To Do, In Progress, Done, etc.)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Issue type (Task, Bug, Story, etc.)
    /// </summary>
    public string IssueType { get; set; } = "Task";

    /// <summary>
    /// Assigned user display name
    /// </summary>
    public string? Assignee { get; set; }

    /// <summary>
    /// Reporter user display name
    /// </summary>
    public string? Reporter { get; set; }

    /// <summary>
    /// Direct URL to the ticket in Jira
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Date when the ticket was created
    /// </summary>
    public DateTime? CreatedDate { get; set; }

    /// <summary>
    /// Date when the ticket was last updated
    /// </summary>
    public DateTime? UpdatedDate { get; set; }

    /// <summary>
    /// Collection of comments on this ticket
    /// </summary>
    public List<JiraComment> Comments { get; set; } = new();

    /// <summary>
    /// Gets a formatted summary of the ticket
    /// </summary>
    /// <returns>Formatted ticket summary</returns>
    public string GetFormattedSummary()
    {
        var summary = $@"
🎫 **Jira Ticket: {Key}**
📋 **Title**: {Title}
🔥 **Priority**: {Priority}
📊 **Status**: {Status}
🏷️ **Type**: {IssueType}
👤 **Assignee**: {Assignee ?? "Unassigned"}
👤 **Reporter**: {Reporter ?? "Unknown"}
📅 **Created**: {CreatedDate?.ToString("yyyy-MM-dd HH:mm") ?? "Unknown"}
📅 **Updated**: {UpdatedDate?.ToString("yyyy-MM-dd HH:mm") ?? "Unknown"}
🔗 **URL**: {Url}

📝 **Description**:
{(string.IsNullOrEmpty(Description) ? "No description provided" : Description)}";

        if (Comments.Any())
        {
            summary += $"\n\n💬 **Comments** ({Comments.Count}):";
            foreach (var comment in Comments.Take(3)) // Show first 3 comments
            {
                summary += $"\n- {comment.Author}: {comment.Body.Substring(0, Math.Min(100, comment.Body.Length))}...";
            }
            if (Comments.Count > 3)
            {
                summary += $"\n... and {Comments.Count - 3} more comments";
            }
        }

        return summary;
    }

    /// <summary>
    /// Checks if this ticket matches a given pattern (e.g., ticket key pattern)
    /// </summary>
    /// <param name="pattern">Pattern to match against</param>
    /// <returns>True if ticket matches the pattern</returns>
    public bool MatchesPattern(string pattern)
    {
        return Key.Contains(pattern, StringComparison.OrdinalIgnoreCase) ||
               Title.Contains(pattern, StringComparison.OrdinalIgnoreCase) ||
               Description.Contains(pattern, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets ticket priority as a numeric value for sorting
    /// </summary>
    /// <returns>Priority as integer (1=High, 2=Medium, 3=Low)</returns>
    public int GetPriorityValue()
    {
        return Priority.ToUpper() switch
        {
            "HIGH" or "HIGHEST" => 1,
            "MEDIUM" => 2,
            "LOW" or "LOWEST" => 3,
            _ => 2
        };
    }

    /// <summary>
    /// Checks if the ticket is in an active state
    /// </summary>
    /// <returns>True if ticket is active (not done/closed)</returns>
    public bool IsActive()
    {
        var inactiveStatuses = new[] { "DONE", "CLOSED", "RESOLVED", "CANCELLED" };
        return !inactiveStatuses.Contains(Status.ToUpper());
    }

    /// <summary>
    /// Gets a short display text for the ticket
    /// </summary>
    /// <returns>Short ticket representation</returns>
    public string GetShortDisplay()
    {
        var truncatedTitle = Title.Length > 50 ? Title.Substring(0, 47) + "..." : Title;
        return $"{Key}: {truncatedTitle} [{Priority}] ({Status})";
    }

    /// <summary>
    /// Creates a ticket from action item
    /// </summary>
    /// <param name="actionItem">Action item to convert</param>
    /// <param name="projectKey">Jira project key</param>
    /// <returns>Jira ticket representation</returns>
    public static JiraTicket FromActionItem(ActionItem actionItem, string projectKey)
    {
        var priority = actionItem.Priority switch
        {
            ActionItemPriority.High => "High",
            ActionItemPriority.Medium => "Medium",
            ActionItemPriority.Low => "Low",
            _ => "Medium"
        };

        return new JiraTicket
        {
            Title = actionItem.Description,
            Description = $@"Action Item from Meeting Analysis

**Original Task**: {actionItem.Description}
**Assigned To**: {actionItem.AssignedTo ?? "Unassigned"}
**Due Date**: {actionItem.DueDate?.ToString("yyyy-MM-dd") ?? "Not specified"}
**Context**: {actionItem.Notes ?? "No additional context"}
**Notes**: {actionItem.Notes ?? "No additional notes"}

This ticket was automatically created from a meeting action item analysis.",
            Priority = priority,
            IssueType = "Task",
            Status = "To Do"
        };
    }

    public override string ToString() => GetShortDisplay();
}
