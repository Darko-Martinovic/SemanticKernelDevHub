using System.ComponentModel.DataAnnotations;

namespace SemanticKernelDevHub.Models;

/// <summary>
/// Represents an action item extracted from a meeting transcript
/// </summary>
public class ActionItem
{
    /// <summary>
    /// Unique identifier for the action item
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Description of the action item or task
    /// </summary>
    [Required]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Person or team assigned to complete this action
    /// </summary>
    public string AssignedTo { get; set; } = string.Empty;

    /// <summary>
    /// Priority level of the action item
    /// </summary>
    public ActionItemPriority Priority { get; set; } = ActionItemPriority.Medium;

    /// <summary>
    /// Due date for completion
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Current status of the action item
    /// </summary>
    public ActionItemStatus Status { get; set; } = ActionItemStatus.Open;

    /// <summary>
    /// Category or type of action item
    /// </summary>
    public string Category { get; set; } = "General";

    /// <summary>
    /// Estimated effort or complexity
    /// </summary>
    public string EstimatedEffort { get; set; } = string.Empty;

    /// <summary>
    /// Dependencies or prerequisites
    /// </summary>
    public List<string> Dependencies { get; set; } = new();

    /// <summary>
    /// Additional notes or context
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Meeting transcript ID this action item came from
    /// </summary>
    public string SourceTranscriptId { get; set; } = string.Empty;

    /// <summary>
    /// When the action item was identified
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// When the action item was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Relevant section or quote from the meeting
    /// </summary>
    public string SourceQuote { get; set; } = string.Empty;

    /// <summary>
    /// Tags for categorization and filtering
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Gets a formatted display string for the action item
    /// </summary>
    /// <returns>Formatted action item description</returns>
    public string GetDisplayString()
    {
        var priorityEmoji = Priority switch
        {
            ActionItemPriority.Urgent => "🔴",
            ActionItemPriority.High => "🟠",
            ActionItemPriority.Medium => "🟡",
            ActionItemPriority.Low => "🟢",
            _ => "⚪"
        };

        var assignee = !string.IsNullOrEmpty(AssignedTo) ? $" (Assigned: {AssignedTo})" : "";
        var dueDate = DueDate.HasValue ? $" [Due: {DueDate.Value:yyyy-MM-dd}]" : "";

        return $"{priorityEmoji} {Priority.ToString().ToUpper()}: {Description}{assignee}{dueDate}";
    }

    /// <summary>
    /// Determines priority based on keywords in the description
    /// </summary>
    /// <param name="description">Action item description</param>
    /// <returns>Suggested priority level</returns>
    public static ActionItemPriority DeterminePriority(string description)
    {
        var lowerDesc = description.ToLowerInvariant();

        var urgentKeywords = new[] { "urgent", "asap", "immediately", "critical", "emergency", "blocker" };
        var highKeywords = new[] { "important", "priority", "soon", "deadline", "required" };
        var lowKeywords = new[] { "research", "investigate", "consider", "explore", "future", "nice to have" };

        if (urgentKeywords.Any(keyword => lowerDesc.Contains(keyword)))
            return ActionItemPriority.Urgent;

        if (highKeywords.Any(keyword => lowerDesc.Contains(keyword)))
            return ActionItemPriority.High;

        if (lowKeywords.Any(keyword => lowerDesc.Contains(keyword)))
            return ActionItemPriority.Low;

        return ActionItemPriority.Medium;
    }

    /// <summary>
    /// Extracts potential assignee from description text
    /// </summary>
    /// <param name="description">Action item description</param>
    /// <param name="participants">List of meeting participants</param>
    /// <returns>Extracted assignee name or empty string</returns>
    public static string ExtractAssignee(string description, List<MeetingParticipant> participants)
    {
        var participantNames = participants.Select(p => p.Name.ToLowerInvariant()).ToList();
        var lowerDesc = description.ToLowerInvariant();

        // Look for assignment patterns
        var assignmentPatterns = new[]
        {
            @"assigned?\s+to\s+([a-zA-Z]+)",
            @"([a-zA-Z]+)\s+will\s+",
            @"([a-zA-Z]+)\s+should\s+",
            @"([a-zA-Z]+)\s+to\s+handle",
            @"([a-zA-Z]+)'s?\s+responsibility"
        };

        foreach (var pattern in assignmentPatterns)
        {
            var match = System.Text.RegularExpressions.Regex.Match(lowerDesc, pattern);
            if (match.Success)
            {
                var potentialName = match.Groups[1].Value;
                if (participantNames.Contains(potentialName))
                {
                    return participants.First(p => p.Name.ToLowerInvariant() == potentialName).Name;
                }
            }
        }

        return string.Empty;
    }
}

/// <summary>
/// Priority levels for action items
/// </summary>
public enum ActionItemPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Urgent = 4
}

/// <summary>
/// Status of an action item
/// </summary>
public enum ActionItemStatus
{
    Open,
    InProgress,
    Completed,
    Cancelled,
    OnHold
}
